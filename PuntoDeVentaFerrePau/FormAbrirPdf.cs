using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PuntoDeVentaFerrePau
{
    public partial class FormAbrirPdf : Form
    {
        // --- CONEXIÓN A SUPABASE ---
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        // --- CONTROLES DE LA PANTALLA ---
        private DataGridView dgvVentas = new DataGridView();
        private WebBrowser visorPdf = new WebBrowser();
        private Label lblEstadoPdf = new Label();
        private Button btnImprimir = new Button();
        private string rutaTicketActual = "";

        public FormAbrirPdf()
        {
            InitializeComponent();
            ConfigurarPantalla();
            _ = CargarVentasRecientes(); // Cargamos las ventas al abrir la ventana
        }

        private void ConfigurarPantalla()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color blancoCard = Color.White;
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.Icon = Properties.Resources.iconoLogo;
            this.BackColor = fondoApp;
            // Ventana ancha para que quepa la tabla y el visor juntos
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "F11 - Historial y Visor de Tickets";
            this.KeyPreview = true;

            // Título
            Label lblTitulo = new Label { Text = "HISTORIAL DE VENTAS RECIENTES", ForeColor = azulMarino, Font = new Font("Arial", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            Label lblInstruccion = new Label { Text = "Selecciona una venta de la lista para ver su ticket.", ForeColor = Color.Gray, Font = new Font("Arial", 10), Location = new Point(20, 50), AutoSize = true };
            this.Controls.Add(lblInstruccion);

            // --- CONFIGURACIÓN DE LA TABLA (MITAD IZQUIERDA) ---
            dgvVentas.Location = new Point(20, 80);
            dgvVentas.Size = new Size(550, 460);
            dgvVentas.BackgroundColor = fondoApp;
            dgvVentas.BorderStyle = BorderStyle.None;
            dgvVentas.EnableHeadersVisualStyles = false;
            dgvVentas.ColumnHeadersDefaultCellStyle.BackColor = azulMarino;
            dgvVentas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVentas.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvVentas.DefaultCellStyle.BackColor = blancoCard;
            dgvVentas.DefaultCellStyle.ForeColor = textoOscuro;
            dgvVentas.DefaultCellStyle.Font = new Font("Arial", 10);
            dgvVentas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgvVentas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 210);
            dgvVentas.DefaultCellStyle.SelectionForeColor = azulMarino;
            dgvVentas.RowHeadersVisible = false;
            dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVentas.ReadOnly = true;
            dgvVentas.AllowUserToAddRows = false;
            dgvVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVentas.MultiSelect = false;

            dgvVentas.Columns.Add("id", "ID VENTA");
            dgvVentas.Columns.Add("fecha", "FECHA");
            dgvVentas.Columns.Add("hora", "HORA");
            dgvVentas.Columns.Add("total", "TOTAL");

            dgvVentas.Columns["id"].FillWeight = 80;
            dgvVentas.Columns["fecha"].FillWeight = 100;
            dgvVentas.Columns["hora"].FillWeight = 100;
            dgvVentas.Columns["total"].FillWeight = 100;

            // Evento cuando le das clic a un renglón
            dgvVentas.SelectionChanged += DgvVentas_SelectionChanged;
            this.Controls.Add(dgvVentas);

            // --- CONFIGURACIÓN DEL VISOR PDF (MITAD DERECHA) ---
            lblEstadoPdf.Text = "Vista previa del ticket...";
            lblEstadoPdf.ForeColor = azulMarino;
            lblEstadoPdf.Font = new Font("Arial", 12, FontStyle.Bold);
            lblEstadoPdf.Location = new Point(600, 50);
            lblEstadoPdf.AutoSize = true;
            this.Controls.Add(lblEstadoPdf);

            visorPdf.Location = new Point(600, 80);
            visorPdf.Size = new Size(450, 400); // Un espacio pequeño como pediste
            this.Controls.Add(visorPdf);

            // Botón Naranja para Imprimir el ticket seleccionado directamente
            btnImprimir.Text = "IMPRIMIR ESTE TICKET";
            btnImprimir.BackColor = naranjaFerre;
            btnImprimir.ForeColor = Color.White;
            btnImprimir.Font = new Font("Arial", 12, FontStyle.Bold);
            btnImprimir.FlatStyle = FlatStyle.Flat;
            btnImprimir.FlatAppearance.BorderSize = 0;
            btnImprimir.Size = new Size(450, 50);
            btnImprimir.Location = new Point(600, 490);
            btnImprimir.Enabled = false; // Se bloquea hasta que encuentres uno válido
            btnImprimir.Click += BtnImprimir_Click;
            this.Controls.Add(btnImprimir);

            // Salir con la tecla Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private async Task CargarVentasRecientes()
        {
            try
            {
                // Pedimos las últimas 100 ventas ordenadas por id de mayor a menor (los más recientes primero)
                string url = $"{SupabaseUrl}/rest/v1/venta?select=id,fecha,hora,total&order=id.desc&limit=100";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray ventas = JArray.Parse(jsonRespuesta);

                    dgvVentas.Rows.Clear();

                    foreach (JObject venta in ventas)
                    {
                        string id = venta["id"].ToString();
                        string fecha = venta["fecha"].ToString();
                        string hora = venta["hora"].ToString();
                        double total = Convert.ToDouble(venta["total"]);

                        dgvVentas.Rows.Add(id, fecha, hora, $"$ {total:F2}");
                    }

                    dgvVentas.ClearSelection();
                }
                else
                {
                    MessageBox.Show("No se pudieron cargar las ventas desde Supabase.", "Error de Nube", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al cargar la tabla: {ex.Message}");
            }
        }

        private void DgvVentas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count > 0)
            {
                // Agarramos el ID del renglón que seleccionaste
                string folioSeleccionado = dgvVentas.SelectedRows[0].Cells["id"].Value.ToString();

                string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string rutaArchivo = Path.Combine(rutaDocumentos, "Tickets_FerrePau", $"Ticket_{folioSeleccionado}.pdf");

                if (File.Exists(rutaArchivo))
                {
                    rutaTicketActual = rutaArchivo;
                    lblEstadoPdf.Text = $"Mostrando Ticket Folio: {folioSeleccionado}";
                    lblEstadoPdf.ForeColor = Color.Green;

                    visorPdf.Navigate(rutaArchivo); // Cargamos el PDF
                    btnImprimir.Enabled = true;
                }
                else
                {
                    rutaTicketActual = "";
                    lblEstadoPdf.Text = $"Ticket Folio {folioSeleccionado} no guardado en esta PC.";
                    lblEstadoPdf.ForeColor = Color.Red;

                    visorPdf.Navigate("about:blank"); // Ponemos el visor en blanco
                    btnImprimir.Enabled = false;
                }
            }
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rutaTicketActual) && File.Exists(rutaTicketActual))
            {
                try
                {
                    // Lo mandamos a la miniprinter de forma silenciosa
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaTicketActual,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);

                    MessageBox.Show("El ticket se está enviando a la impresora térmica.", "Imprimiendo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Revisa la conexión de tu impresora. Error: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}