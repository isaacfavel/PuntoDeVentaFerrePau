using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormBuscarProducto : Form
    {
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        public string IdProductoSeleccionado { get; private set; } = "";

        // Variable para controlar la búsqueda en vivo sin saturar la nube
        private int contadorBusqueda = 0;

        private TextBox txtBuscador = new TextBox();
        private DataGridView dgvResultados = new DataGridView();
        private Label lblInstrucciones = new Label();

        public FormBuscarProducto()
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

            this.Icon = Properties.Resources.iconoLogo; 
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F3 - Buscar Producto por Nombre";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.KeyPreview = true;

            // Instrucciones
            lblInstrucciones.Text = "ESCRIBE EL NOMBRE DEL PRODUCTO:";
            lblInstrucciones.ForeColor = azulMarino;
            lblInstrucciones.Font = new Font("Arial", 10, FontStyle.Bold);
            lblInstrucciones.Location = new Point(20, 20);
            lblInstrucciones.AutoSize = true;
            this.Controls.Add(lblInstrucciones);

            // Caja de búsqueda
            txtBuscador.Location = new Point(20, 45);
            txtBuscador.Width = 695;
            txtBuscador.Font = new Font("Arial", 16);
            txtBuscador.BackColor = Color.White;
            txtBuscador.ForeColor = textoOscuro;
            txtBuscador.BorderStyle = BorderStyle.FixedSingle;
            txtBuscador.TextChanged += TxtBuscador_TextChanged;
            txtBuscador.KeyDown += TxtBuscador_KeyDown;
            this.Controls.Add(txtBuscador);

            // --- DISEÑO DE LA TABLA ---
            dgvResultados.Location = new Point(20, 100);
            dgvResultados.Size = new Size(695, 450);
            dgvResultados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResultados.BackgroundColor = Color.White;
            dgvResultados.BorderStyle = BorderStyle.None;
            dgvResultados.RowHeadersVisible = false;
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.ReadOnly = true;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Desactivamos el ajuste automático para respetar los anchos personalizados
            dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvResultados.EnableHeadersVisualStyles = false;

            // Colores tabla
            dgvResultados.ColumnHeadersDefaultCellStyle.BackColor = azulMarino;
            dgvResultados.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvResultados.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvResultados.ColumnHeadersDefaultCellStyle.SelectionBackColor = azulMarino;

            dgvResultados.DefaultCellStyle.BackColor = Color.White;
            dgvResultados.DefaultCellStyle.ForeColor = textoOscuro;
            dgvResultados.DefaultCellStyle.Font = new Font("Arial", 12);
            dgvResultados.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 210);
            dgvResultados.DefaultCellStyle.SelectionForeColor = azulMarino;
            dgvResultados.RowTemplate.Height = 40;

            // Columnas configuradas
            dgvResultados.Columns.Add("id", "CÓDIGO");
            dgvResultados.Columns.Add("desc", "DESCRIPCIÓN");
            dgvResultados.Columns.Add("precio", "PRECIO");
            dgvResultados.Columns.Add("stock", "STOCK");

            // --- ANCHOS EXACTOS PARA LEER EL TEXTO LARGO ---
            dgvResultados.Columns[0].Visible = false; // Código oculto
            dgvResultados.Columns[1].Width = 485;     // Descripción súper amplia
            dgvResultados.Columns[2].Width = 110;     // Precio ajustado a los números
            dgvResultados.Columns[3].Width = 80;      // Stock muy compacto

            dgvResultados.KeyDown += DgvResultados_KeyDown;
            dgvResultados.CellDoubleClick += DgvResultados_CellDoubleClick;
            // --- NUEVO: Detectar cuando escribes estando en la tabla ---
            dgvResultados.KeyPress += DgvResultados_KeyPress;
            this.Controls.Add(dgvResultados);

            this.KeyDown += FormBuscarProducto_KeyDown;
            this.ActiveControl = txtBuscador;
        }

        // --- BÚSQUEDA EN VIVO ---
        private async void TxtBuscador_TextChanged(object sender, EventArgs e)
        {
            string texto = txtBuscador.Text.Trim();

            if (string.IsNullOrEmpty(texto))
            {
                dgvResultados.Rows.Clear();
                return;
            }

            contadorBusqueda++;
            int busquedaActual = contadorBusqueda;

            await Task.Delay(300);

            if (busquedaActual != contadorBusqueda) return;

            await BuscarEnSupabase(texto);
        }

        private void TxtBuscador_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (dgvResultados.Rows.Count > 0) dgvResultados.Focus();
            }
        }

        private async Task BuscarEnSupabase(string texto)
        {
            try
            {
                dgvResultados.Rows.Clear();

                string url = $"{SupabaseUrl}/rest/v1/producto?descripcion=ilike.*{texto}*&select=id,descripcion,precio,cantidad";

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
                        dgvResultados.Rows.Add(
                            prod["id"].ToString(),
                            prod["descripcion"].ToString(),
                            $"$ {Convert.ToDouble(prod["precio"]):F2}",
                            prod["cantidad"].ToString()
                        );
                    }
                }
            }
            catch (Exception)
            {
                // Silencioso para no interrumpir al usuario
            }
        }

        private void DgvResultados_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SeleccionarFila();
            }
        }

        private void DgvResultados_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            SeleccionarFila();
        }

        private void SeleccionarFila()
        {
            if (dgvResultados.SelectedRows.Count > 0)
            {
                // Captura el ID desde la columna 0, que es la que está oculta en este código
                IdProductoSeleccionado = dgvResultados.SelectedRows[0].Cells[0].Value.ToString();
                this.Close();
            }
        }

        private void FormBuscarProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }
        private void DgvResultados_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Si la tecla es una letra, número, puntuación, espacio o retroceso (borrar)
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == (char)Keys.Back)
            {
                txtBuscador.Focus(); // Mandamos el cursor de vuelta a la caja

                if (e.KeyChar == (char)Keys.Back)
                {
                    // Si presionó borrar, quitamos la última letra si es que hay texto
                    if (txtBuscador.Text.Length > 0)
                    {
                        txtBuscador.Text = txtBuscador.Text.Substring(0, txtBuscador.Text.Length - 1);
                    }
                }
                else
                {
                    // Si es una letra normal, la agregamos al texto actual
                    txtBuscador.Text += e.KeyChar;
                }

                // Ponemos el cursor al final de la palabra para que sigas escribiendo fluido
                txtBuscador.SelectionStart = txtBuscador.Text.Length;

                // Le decimos a la tabla que ya nos encargamos de esa tecla para que no haga ruidos raros
                e.Handled = true;
            }
        }
    }
}