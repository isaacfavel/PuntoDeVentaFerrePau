using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormInventarioBajo : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private DataGridView dgvFaltantes = new DataGridView();

        public FormInventarioBajo()
        {
            InitializeComponent();
            ConfigurarPantalla();

            // Al abrir la ventana, mandamos llamar la búsqueda automáticamente
            _ = CargarInventarioBajo();
        }

        private void ConfigurarPantalla()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.Icon = Properties.Resources.iconoLogo; 
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F6 - Reporte de Inventario Bajo";
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label lblTitulo = new Label
            {
                Text = "PRODUCTOS POR AGOTARSE (5 PIEZAS O MENOS)",
                ForeColor = azulMarino,
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitulo);

            // --- DISEÑO DE LA TABLA ESTILO FERRE-PAU ---
            dgvFaltantes.Location = new Point(20, 60);
            dgvFaltantes.Size = new Size(695, 480);
            dgvFaltantes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvFaltantes.BackgroundColor = Color.White;
            dgvFaltantes.BorderStyle = BorderStyle.None;
            dgvFaltantes.RowHeadersVisible = false;
            dgvFaltantes.AllowUserToAddRows = false;
            dgvFaltantes.ReadOnly = true;
            dgvFaltantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Desactivar autoajuste para usar nuestros anchos
            dgvFaltantes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvFaltantes.EnableHeadersVisualStyles = false;

            // Colores de la tabla
            dgvFaltantes.ColumnHeadersDefaultCellStyle.BackColor = azulMarino;
            dgvFaltantes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvFaltantes.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvFaltantes.ColumnHeadersDefaultCellStyle.SelectionBackColor = azulMarino;

            dgvFaltantes.DefaultCellStyle.BackColor = Color.White;
            dgvFaltantes.DefaultCellStyle.ForeColor = textoOscuro;
            dgvFaltantes.DefaultCellStyle.Font = new Font("Arial", 12);
            dgvFaltantes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 210);
            dgvFaltantes.DefaultCellStyle.SelectionForeColor = azulMarino;
            dgvFaltantes.RowTemplate.Height = 40;

            // Columnas
            dgvFaltantes.Columns.Add("id", "CÓDIGO");
            dgvFaltantes.Columns.Add("desc", "DESCRIPCIÓN");
            dgvFaltantes.Columns.Add("stock", "STOCK ACTUAL");

            // --- ANCHOS EXACTOS AJUSTADOS (695px total aprox) ---
            dgvFaltantes.Columns[0].Visible = false; // Código oculto
            dgvFaltantes.Columns[1].Width = 525;     // Descripción súper amplia
            dgvFaltantes.Columns[2].Width = 150;     // Stock compacto y centrado

            // Centrar el texto del stock para que se vea mejor
            dgvFaltantes.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn col in dgvFaltantes.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            this.Controls.Add(dgvFaltantes);

            // Salir con Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private async Task CargarInventarioBajo()
        {
            try
            {
                // cantidad=lte.5 (Menor o igual a 5)
                // order=cantidad.asc (Ordena de menor a mayor para ver los que tienen 0 primero)
                string url = $"{SupabaseUrl}/rest/v1/producto?cantidad=lte.5&select=id,descripcion,cantidad&order=cantidad.asc";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray productos = JArray.Parse(jsonRespuesta);

                    foreach (JObject prod in productos)
                    {
                        dgvFaltantes.Rows.Add(
                            prod["id"].ToString(),
                            prod["descripcion"].ToString(),
                            prod["cantidad"].ToString()
                        );
                    }

                    if (productos.Count == 0)
                    {
                        MessageBox.Show("¡Excelente! No tienes ningún producto con stock bajo.", "Inventario Sano", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Hubo un error al consultar tu inventario en la nube.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}