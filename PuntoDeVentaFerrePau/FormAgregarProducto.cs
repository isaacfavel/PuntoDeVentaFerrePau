using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormAgregarProducto : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private TextBox txtCodigo = new TextBox();
        private TextBox txtDescripcion = new TextBox();
        private TextBox txtPrecio = new TextBox();
        private TextBox txtStock = new TextBox();
        private Button btnGuardar = new Button();

        public FormAgregarProducto()
        {
            InitializeComponent();
            ConfigurarPantalla();
        }

        private void ConfigurarPantalla()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.Icon = Properties.Resources.iconoLogo; this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F4 - Alta de Productos";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitulo = new Label { Text = "NUEVO PRODUCTO", ForeColor = azulMarino, Font = new Font("Arial", 20, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            int y = 70;
            CrearCampo("CÓDIGO DE BARRAS:", txtCodigo, y, azulMarino);
            y += 90;
            CrearCampo("DESCRIPCIÓN:", txtDescripcion, y, azulMarino);
            y += 90;
            CrearCampo("PRECIO DE VENTA ($):", txtPrecio, y, azulMarino);
            y += 90;
            CrearCampo("CANTIDAD INICIAL (STOCK):", txtStock, y, azulMarino);

            btnGuardar.Text = "GUARDAR EN NUBE";
            btnGuardar.BackColor = naranjaFerre;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Font = new Font("Arial", 14, FontStyle.Bold);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Size = new Size(440, 50);
            btnGuardar.Location = new Point(20, 430);
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void CrearCampo(string textoLabel, TextBox cajaTexto, int y, Color colorLabel)
        {
            Label lbl = new Label { Text = textoLabel, ForeColor = colorLabel, Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lbl);

            cajaTexto.Location = new Point(20, y + 25);
            cajaTexto.Width = 440;
            cajaTexto.Font = new Font("Arial", 14);
            cajaTexto.BackColor = Color.White;
            cajaTexto.ForeColor = Color.Black;
            cajaTexto.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(cajaTexto);
        }

        private async void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) || string.IsNullOrWhiteSpace(txtStock.Text))
            {
                MessageBox.Show("Por favor, llena todos los campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGuardar.Enabled = false;
            btnGuardar.Text = "GUARDANDO...";

            try
            {
                string url = $"{SupabaseUrl}/rest/v1/producto";
                JObject nuevoProducto = new JObject();
                nuevoProducto["id"] = txtCodigo.Text.Trim();
                nuevoProducto["descripcion"] = txtDescripcion.Text.Trim().ToUpper();
                nuevoProducto["precio"] = Convert.ToDouble(txtPrecio.Text.Trim());
                nuevoProducto["cantidad"] = Convert.ToInt32(txtStock.Text.Trim());

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
                request.Content = new StringContent(nuevoProducto.ToString(), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("¡Producto guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    // --- AQUÍ ESTÁ LA MAGIA PARA DETECTAR EL DUPLICADO ---
                    if (error.Contains("23505") || error.Contains("duplicate key"))
                    {
                        DialogResult respuesta = MessageBox.Show(
                            "Este código de producto ya está registrado en tu inventario.\n\n¿Quieres abrir la ventana para actualizar su precio o stock?",
                            "Producto ya existente",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (respuesta == DialogResult.Yes)
                        {
                            string codigoDuplicado = txtCodigo.Text.Trim();

                            // Primero cerramos la ventana actual (Alta)
                            this.Close();

                            // Luego abrimos la ventana de Modificar (F5) pasándole el código que acabamos de escribir
                            FormModificarProducto ventanaModificar = new FormModificarProducto(codigoDuplicado);
                            ventanaModificar.ShowDialog();
                        }
                    }
                    else
                    {
                        // Si es un error distinto al de duplicado, mostramos el mensaje original
                        MessageBox.Show($"Hubo un problema al guardar.\nDetalle: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Revisa que el precio y stock sean números correctos.\nError: {ex.Message}", "Error de Formato", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // En caso de que no se cierre la ventana, reactivamos el botón
                btnGuardar.Enabled = true;
                btnGuardar.Text = "GUARDAR EN NUBE";
            }
        }
    }
}