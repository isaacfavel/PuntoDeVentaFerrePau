using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormModificarProducto : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private TextBox txtCodigo = new TextBox();
        private Label lblDescripcion = new Label();
        private TextBox txtPrecio = new TextBox();
        private TextBox txtStockActual = new TextBox();
        private Button btnActualizar = new Button();

        // Modificamos el constructor para que pueda recibir un código de manera opcional
        public FormModificarProducto(string codigoPreCargado = "")
        {
            InitializeComponent();
            ConfigurarPantalla();

            // Si le mandamos un código desde otra ventana, lo busca solito
            if (!string.IsNullOrEmpty(codigoPreCargado))
            {
                txtCodigo.Text = codigoPreCargado;
                _ = BuscarProductoParaEditar(codigoPreCargado);
            }
        }

        private void ConfigurarPantalla()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.Icon = new Icon(@"C:\Users\chino\source\repos\PuntoDeVentaFerrePau\PuntoDeVentaFerrePau\logo1 (1).ico");
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp; // Fondo claro
            this.Text = "F5 - Modificar Precio o Stock";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitulo = new Label { Text = "ACTUALIZAR PRODUCTO", ForeColor = azulMarino, Font = new Font("Arial", 18, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            int y = 70;
            CrearCampo("1. ESCANEA EL CÓDIGO (O PRESIONA F3):", txtCodigo, y, azulMarino, textoOscuro);
            txtCodigo.KeyDown += TxtCodigo_KeyDown;

            y += 85;
            lblDescripcion.Text = "Esperando producto...";
            lblDescripcion.ForeColor = Color.DimGray; // Gris oscuro para que resalte en el fondo claro
            lblDescripcion.Font = new Font("Arial", 12, FontStyle.Italic);
            lblDescripcion.Location = new Point(20, y);
            lblDescripcion.AutoSize = true;
            this.Controls.Add(lblDescripcion);

            y += 50;
            CrearCampo("2. NUEVO PRECIO DE VENTA ($):", txtPrecio, y, azulMarino, textoOscuro);
            txtPrecio.Enabled = false; // Se bloquean hasta que busquemos un producto

            y += 85;
            CrearCampo("3. STOCK TOTAL (Modifica la cantidad final):", txtStockActual, y, azulMarino, textoOscuro);
            txtStockActual.Enabled = false;

            btnActualizar.Text = "ACTUALIZAR EN NUBE";
            btnActualizar.BackColor = naranjaFerre;
            btnActualizar.ForeColor = Color.White;
            btnActualizar.Font = new Font("Arial", 14, FontStyle.Bold);
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Size = new Size(440, 50);
            btnActualizar.Location = new Point(20, 430);
            btnActualizar.Enabled = false;
            btnActualizar.Click += BtnActualizar_Click;
            this.Controls.Add(btnActualizar);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void CrearCampo(string textoLabel, TextBox cajaTexto, int y, Color colorLabel, Color colorTexto)
        {
            Label lbl = new Label { Text = textoLabel, ForeColor = colorLabel, Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lbl);

            cajaTexto.Location = new Point(20, y + 25);
            cajaTexto.Width = 440;
            cajaTexto.Font = new Font("Arial", 14);
            cajaTexto.BackColor = Color.White;
            cajaTexto.ForeColor = colorTexto;
            cajaTexto.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(cajaTexto);
        }

        private async void TxtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            // Si escanean el código físico y dan Enter
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string idProducto = txtCodigo.Text.Trim();
                if (!string.IsNullOrEmpty(idProducto))
                {
                    await BuscarProductoParaEditar(idProducto);
                }
            }
            // Si presionan F3 para buscar por nombre
            else if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                FormBuscarProducto ventanaBuscar = new FormBuscarProducto();
                ventanaBuscar.ShowDialog();

                if (!string.IsNullOrEmpty(ventanaBuscar.IdProductoSeleccionado))
                {
                    txtCodigo.Text = ventanaBuscar.IdProductoSeleccionado;
                    await BuscarProductoParaEditar(ventanaBuscar.IdProductoSeleccionado);
                }
            }
        }

        private async Task BuscarProductoParaEditar(string idProducto)
        {
            try
            {
                string url = $"{SupabaseUrl}/rest/v1/producto?id=eq.{idProducto}&select=descripcion,precio,cantidad";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray arregloProductos = JArray.Parse(jsonRespuesta);

                    if (arregloProductos.Count > 0)
                    {
                        JObject producto = (JObject)arregloProductos[0];

                        // Mostramos los datos actuales en pantalla y cambiamos el color para que se vea activo
                        lblDescripcion.Text = producto["descripcion"].ToString();
                        lblDescripcion.ForeColor = Color.FromArgb(32, 54, 97); // Azul marino

                        txtPrecio.Text = producto["precio"].ToString();
                        txtStockActual.Text = producto["cantidad"].ToString();

                        // Desbloqueamos los controles
                        txtPrecio.Enabled = true;
                        txtStockActual.Enabled = true;
                        btnActualizar.Enabled = true;

                        txtPrecio.Focus(); // Mandamos el cursor al precio
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el producto con ese código.", "No existe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}");
            }
        }

        private async void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPrecio.Text) || string.IsNullOrWhiteSpace(txtStockActual.Text))
            {
                MessageBox.Show("Los campos de precio y stock no pueden estar vacíos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnActualizar.Enabled = false;
            btnActualizar.Text = "ACTUALIZANDO...";

            try
            {
                string idProducto = txtCodigo.Text.Trim();
                string url = $"{SupabaseUrl}/rest/v1/producto?id=eq.{idProducto}";

                JObject actualizacion = new JObject();
                actualizacion["precio"] = Convert.ToDouble(txtPrecio.Text.Trim());
                actualizacion["cantidad"] = Convert.ToInt32(txtStockActual.Text.Trim());

                // Usamos PATCH para actualizar datos existentes
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
                request.Content = new StringContent(actualizacion.ToString(), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("¡Producto actualizado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al intentar actualizar en Supabase.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Revisa que hayas ingresado números válidos.\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnActualizar.Enabled = true;
                btnActualizar.Text = "ACTUALIZAR EN NUBE";
            }
        }
    }
}