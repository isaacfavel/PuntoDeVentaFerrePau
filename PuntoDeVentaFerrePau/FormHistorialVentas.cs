using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormHistorialVentas : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private DateTimePicker dtpFecha = new DateTimePicker();
        private Button btnBuscar = new Button();
        private DataGridView dgvVentas = new DataGridView();
        private Label lblGranTotal = new Label();

        public FormHistorialVentas()
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

            this.Icon = new Icon(@"C:\Users\chino\source\repos\PuntoDeVentaFerrePau\PuntoDeVentaFerrePau\logo1 (1).ico");
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F8 - Historial de Ventas";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true;

            Label lblTitulo = new Label { Text = "HISTORIAL DE VENTAS POR FECHA", ForeColor = azulMarino, Font = new Font("Arial", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // Calendario para elegir el día
            Label lblInstruccion = new Label { Text = "SELECCIONA LA FECHA:", ForeColor = azulMarino, Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(20, 70), AutoSize = true };
            this.Controls.Add(lblInstruccion);

            dtpFecha.Location = new Point(230, 65);
            dtpFecha.Size = new Size(150, 30);
            dtpFecha.Font = new Font("Arial", 14);
            dtpFecha.Format = DateTimePickerFormat.Short; // Muestra solo el día/mes/año
            this.Controls.Add(dtpFecha);

            // Botón de búsqueda (Azul Marino)
            btnBuscar.Text = "BUSCAR VENTAS";
            btnBuscar.BackColor = azulMarino;
            btnBuscar.ForeColor = Color.White;
            btnBuscar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnBuscar.FlatStyle = FlatStyle.Flat;
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Size = new Size(180, 32);
            btnBuscar.Location = new Point(400, 64);
            btnBuscar.Click += BtnBuscar_Click;
            this.Controls.Add(btnBuscar);

            // --- DISEÑO DE LA TABLA ESTILO FERRE-PAU ---
            dgvVentas.Location = new Point(20, 120);
            dgvVentas.Size = new Size(695, 350);
            dgvVentas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgvVentas.BackgroundColor = Color.White;
            dgvVentas.BorderStyle = BorderStyle.None;
            dgvVentas.RowHeadersVisible = false;
            dgvVentas.AllowUserToAddRows = false;
            dgvVentas.ReadOnly = true;
            dgvVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Ajuste manual de columnas
            dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvVentas.EnableHeadersVisualStyles = false;

            // Colores de la tabla
            dgvVentas.ColumnHeadersDefaultCellStyle.BackColor = azulMarino;
            dgvVentas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVentas.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            dgvVentas.ColumnHeadersDefaultCellStyle.SelectionBackColor = azulMarino;

            dgvVentas.DefaultCellStyle.BackColor = Color.White;
            dgvVentas.DefaultCellStyle.ForeColor = textoOscuro;
            dgvVentas.DefaultCellStyle.Font = new Font("Arial", 14);
            dgvVentas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 210);
            dgvVentas.DefaultCellStyle.SelectionForeColor = azulMarino;
            dgvVentas.RowTemplate.Height = 40;

            // Agregamos las columnas
            dgvVentas.Columns.Add("folio", "FOLIO DEL TICKET");
            dgvVentas.Columns.Add("total", "TOTAL DE LA VENTA");

            // --- ANCHOS EXACTOS ---
            dgvVentas.Columns[0].Width = 400; // El folio es largo, le damos espacio
            dgvVentas.Columns[1].Width = 275; // Total
            dgvVentas.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; // Alineamos el dinero a la derecha

            foreach (DataGridViewColumn col in dgvVentas.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable; // Bloquea el color azul por defecto
            }

            this.Controls.Add(dgvVentas);

            // Etiqueta para la suma total de ese día (Naranja)
            lblGranTotal.Text = "TOTAL DEL DÍA: $ 0.00";
            lblGranTotal.ForeColor = naranjaFerre;
            lblGranTotal.Font = new Font("Arial", 22, FontStyle.Bold);
            lblGranTotal.Location = new Point(20, 490);
            lblGranTotal.AutoSize = true;
            this.Controls.Add(lblGranTotal);

            // Salir con Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private async void BtnBuscar_Click(object sender, EventArgs e)
        {
            await BuscarVentasPorFecha();
        }

        private async Task BuscarVentasPorFecha()
        {
            try
            {
                dgvVentas.Rows.Clear();
                btnBuscar.Text = "BUSCANDO...";
                btnBuscar.Enabled = false;

                // Sacamos la fecha seleccionada en el formato exacto que guardas (ej. 17/07/2026)
                string fechaBusqueda = dtpFecha.Value.ToString("dd/MM/yyyy");

                string url = $"{SupabaseUrl}/rest/v1/venta?fecha=eq.{Uri.EscapeDataString(fechaBusqueda)}&select=id,total";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray ventas = JArray.Parse(jsonRespuesta);

                    double sumaDia = 0.0;

                    foreach (JObject venta in ventas)
                    {
                        double totalVenta = Convert.ToDouble(venta["total"]);
                        sumaDia += totalVenta;

                        dgvVentas.Rows.Add(
                            venta["id"].ToString(),
                            $"$ {totalVenta:F2}"
                        );
                    }

                    lblGranTotal.Text = $"TOTAL DEL DÍA: $ {sumaDia:F2}";

                    if (ventas.Count == 0)
                    {
                        MessageBox.Show($"No se encontraron ventas para la fecha: {fechaBusqueda}", "Sin ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Hubo un error al consultar tu base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBuscar.Text = "BUSCAR VENTAS";
                btnBuscar.Enabled = true;
            }
        }
    }
}