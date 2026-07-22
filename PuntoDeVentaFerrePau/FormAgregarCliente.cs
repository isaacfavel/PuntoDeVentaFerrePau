using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormAgregarCliente : Form
    {
        // --- CONEXIÓN A SUPABASE ---
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private TextBox txtNombre = new TextBox();
        private TextBox txtTelefono = new TextBox();
        private TextBox txtDireccion = new TextBox(); // <--- NUEVO CAMPO
        private Button btnGuardar = new Button();

        public FormAgregarCliente()
        {
            InitializeComponent();
            ConfigurarPantalla();
        }

        private void ConfigurarPantalla()
        {
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.BackColor = fondoApp;
            // Ventana un poco más alta para que quepa la dirección
            this.Size = new Size(450, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Registrar Nuevo Cliente para Crédito";
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };

            Label lblTitulo = new Label { Text = "NUEVO CLIENTE", Font = new Font("Arial", 20, FontStyle.Bold), ForeColor = azulMarino, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // --- NOMBRE ---
            Label lblNombre = new Label { Text = "Nombre Completo:", Font = new Font("Arial", 12), ForeColor = azulMarino, Location = new Point(20, 80), AutoSize = true };
            this.Controls.Add(lblNombre);

            txtNombre.Location = new Point(20, 110);
            txtNombre.Size = new Size(390, 30);
            txtNombre.Font = new Font("Arial", 14);
            txtNombre.ForeColor = textoOscuro;
            txtNombre.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtNombre);

            // --- TELÉFONO ---
            Label lblTelefono = new Label { Text = "Teléfono:", Font = new Font("Arial", 12), ForeColor = azulMarino, Location = new Point(20, 160), AutoSize = true };
            this.Controls.Add(lblTelefono);

            txtTelefono.Location = new Point(20, 190);
            txtTelefono.Size = new Size(390, 30);
            txtTelefono.Font = new Font("Arial", 14);
            txtTelefono.ForeColor = textoOscuro;
            txtTelefono.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtTelefono);

            // --- DIRECCIÓN ---
            Label lblDireccion = new Label { Text = "Dirección:", Font = new Font("Arial", 12), ForeColor = azulMarino, Location = new Point(20, 240), AutoSize = true };
            this.Controls.Add(lblDireccion);

            txtDireccion.Location = new Point(20, 270);
            txtDireccion.Size = new Size(390, 30);
            txtDireccion.Font = new Font("Arial", 14);
            txtDireccion.ForeColor = textoOscuro;
            txtDireccion.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtDireccion);

            // --- BOTÓN GUARDAR ---
            btnGuardar.Text = "GUARDAR CLIENTE";
            btnGuardar.BackColor = naranjaFerre;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnGuardar.Size = new Size(390, 45);
            btnGuardar.Location = new Point(20, 325); // Movido más abajo
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.Click += async (s, e) => await GuardarClienteBD();
            this.Controls.Add(btnGuardar);
        }

        private async Task GuardarClienteBD()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGuardar.Enabled = false;
                btnGuardar.Text = "GUARDANDO...";

                string url = $"{SupabaseUrl}/rest/v1/cliente";
                JObject nuevoCliente = new JObject();
                nuevoCliente["nombre"] = txtNombre.Text.Trim();
                nuevoCliente["telefono"] = txtTelefono.Text.Trim();
                nuevoCliente["direccion"] = txtDireccion.Text.Trim(); // Guardamos la dirección
                nuevoCliente["saldo"] = 0; // Inicia con deuda en 0

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
                request.Content = new StringContent(nuevoCliente.ToString(), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("¡Cliente registrado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al guardar en Supabase.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                btnGuardar.Enabled = true;
                btnGuardar.Text = "GUARDAR CLIENTE";
            }
        }
    }
}