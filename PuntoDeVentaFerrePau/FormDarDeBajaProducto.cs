using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormDarDeBajaProducto : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private TextBox txtCodigo = new TextBox();
        private Label lblDescripcion = new Label();
        private Button btnEliminar = new Button();

        public FormDarDeBajaProducto()
        {
            InitializeComponent();
            ConfigurarPantalla();
        }

        private void ConfigurarPantalla()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color rojoPeligro = Color.FromArgb(220, 53, 69); // Rojo para la baja
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.Icon = Properties.Resources.iconoLogo; 
            this.Size = new Size(500, 400); // Un poco más pequeña porque tiene menos campos
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F9 - Dar de Baja Producto";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitulo = new Label { Text = "ELIMINAR PRODUCTO", ForeColor = rojoPeligro, Font = new Font("Arial", 18, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            int y = 70;
            Label lblInstruccion = new Label { Text = "1. ESCANEA EL CÓDIGO (O PRESIONA F3):", ForeColor = azulMarino, Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lblInstruccion);

            txtCodigo.Location = new Point(20, y + 25);
            txtCodigo.Width = 440;
            txtCodigo.Font = new Font("Arial", 14);
            txtCodigo.BackColor = Color.White;
            txtCodigo.ForeColor = textoOscuro;
            txtCodigo.BorderStyle = BorderStyle.FixedSingle;
            txtCodigo.KeyDown += TxtCodigo_KeyDown;
            this.Controls.Add(txtCodigo);

            y += 85;
            lblDescripcion.Text = "Esperando producto...";
            lblDescripcion.ForeColor = Color.DimGray;
            lblDescripcion.Font = new Font("Arial", 12, FontStyle.Italic);
            lblDescripcion.Location = new Point(20, y);
            lblDescripcion.AutoSize = true;
            this.Controls.Add(lblDescripcion);

            btnEliminar.Text = "ELIMINAR DE LA NUBE";
            btnEliminar.BackColor = rojoPeligro;
            btnEliminar.ForeColor = Color.White;
            btnEliminar.Font = new Font("Arial", 14, FontStyle.Bold);
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Size = new Size(440, 50);
            btnEliminar.Location = new Point(20, 280);
            btnEliminar.Enabled = false; // Se activa hasta que busquemos algo
            btnEliminar.Click += BtnEliminar_Click;
            this.Controls.Add(btnEliminar);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private async void TxtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string idProducto = txtCodigo.Text.Trim();
                if (!string.IsNullOrEmpty(idProducto))
                {
                    await BuscarProductoParaEliminar(idProducto);
                }
            }
            else if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                FormBuscarProducto ventanaBuscar = new FormBuscarProducto();
                ventanaBuscar.ShowDialog();

                if (!string.IsNullOrEmpty(ventanaBuscar.IdProductoSeleccionado))
                {
                    txtCodigo.Text = ventanaBuscar.IdProductoSeleccionado;
                    await BuscarProductoParaEliminar(ventanaBuscar.IdProductoSeleccionado);
                }
            }
        }

        private async Task BuscarProductoParaEliminar(string idProducto)
        {
            try
            {
                string url = $"{SupabaseUrl}/rest/v1/producto?id=eq.{idProducto}&select=descripcion";
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
                        lblDescripcion.Text = producto["descripcion"].ToString();
                        lblDescripcion.ForeColor = Color.FromArgb(220, 53, 69); // Lo pintamos de rojo para alertar

                        btnEliminar.Enabled = true; // Habilitamos el botón de borrar
                        btnEliminar.Focus(); // Mandamos el cursor al botón directamente
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el producto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}");
            }
        }

        private async void BtnEliminar_Click(object sender, EventArgs e)
        {
            // Doble confirmación de seguridad
            DialogResult confirmacion = MessageBox.Show(
                $"¿ESTÁS SEGURO DE ELIMINAR ESTE PRODUCTO?\n\n{lblDescripcion.Text}\n\nEsta acción no se puede deshacer.",
                "Cuidado - Eliminar Producto",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2 // El botón "NO" por defecto para evitar dedazos
            );

            if (confirmacion == DialogResult.Yes)
            {
                btnEliminar.Enabled = false;
                btnEliminar.Text = "ELIMINANDO...";

                try
                {
                    string idProducto = txtCodigo.Text.Trim();
                    string url = $"{SupabaseUrl}/rest/v1/producto?id=eq.{idProducto}";

                    // Usamos HttpMethod.Delete para borrarlo permanentemente
                    var request = new HttpRequestMessage(HttpMethod.Delete, url);
                    request.Headers.Add("apikey", SupabaseKey);
                    request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                    HttpResponseMessage response = await clienteHttp.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar de Supabase.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnEliminar.Enabled = true;
                    btnEliminar.Text = "ELIMINAR DE LA NUBE";
                }
            }
        }
    }
}