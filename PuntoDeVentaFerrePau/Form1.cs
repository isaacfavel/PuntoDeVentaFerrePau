using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing.Printing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace PuntoDeVentaFerrePau
{
    public partial class Form1 : Form
    {
        // --- CONEXIÓN A SUPABASE ---
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private double totalVentaAcumulado = 0.0;

        // --- NUEVAS VARIABLES PARA EL TICKET ---
        private string ultimoFolio = "";
        private double ultimoPago = 0.0;
        private double ultimoCambio = 0.0;

        // --- VARIABLES PARA EL TICKET DE CORTE DE CAJA ---
        private string corteFecha = "";
        private int corteTickets = 0;
        private double corteTotal = 0.0;

        // --- BOTÓN PARA REIMPRIMIR ÚLTIMO TICKET ---
        private Button btnReimprimirUltimo = new Button();

        // Guardamos el nombre del cajero que inició sesión
        private string nombreCajeroActual = "";

        public Form1(string cajero)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.nombreCajeroActual = cajero; // Lo recibimos desde el Login
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AplicarTemaFerrePau();
            ConfigurarTabla();
        }

        private void AplicarTemaFerrePau()
        {
            this.Icon = Properties.Resources.iconoLogo;

            // --- MAGIA PARA EL FOCUS GLOBAL ---
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown_Global;

            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color blancoCard = Color.White;
            Color textoGris = Color.FromArgb(120, 120, 120);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.BackColor = fondoApp;

            lblTitulo.Text = "FERRE PAU";
            lblTitulo.Font = new System.Drawing.Font("Arial", 40, FontStyle.Bold);
            lblTitulo.ForeColor = azulMarino;
            lblTitulo.Location = new Point(20, 20);

            lblCajero.Text = "CAJERO: " + nombreCajeroActual.ToUpper();
            lblCajero.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            lblCajero.ForeColor = textoGris;
            lblCajero.Location = new Point(this.ClientSize.Width - 300, 20);
            lblCajero.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // =========================================================================
            // --- MENÚ SUPERIOR DE BOTONES ---
            // =========================================================================
            FlowLayoutPanel panelMenu = new FlowLayoutPanel();
            panelMenu.Location = new Point(20, 85);
            panelMenu.Size = new Size(this.ClientSize.Width - 40, 50);
            panelMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(panelMenu);

            void AgregarBotonMenu(string texto, Keys tecla)
            {
                Button btn = new Button();
                btn.Text = texto;
                btn.BackColor = azulMarino;
                btn.ForeColor = Color.White;
                btn.Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;

                btn.AutoSize = true;
                btn.MinimumSize = new Size(110, 40);
                btn.Height = 40;
                btn.Margin = new Padding(0, 0, 8, 0);
                btn.Cursor = Cursors.Hand;
                btn.TabStop = false;

                // --- MATAR EL BORDE FEO DE WINDOWS EN EL F1 ---
                btn.NotifyDefault(false);

                btn.Click += (s, ev) =>
                {
                    txtCodigo_KeyDown(txtCodigo, new KeyEventArgs(tecla));
                    txtCodigo.Focus();
                };
                panelMenu.Controls.Add(btn);
            }

            //AgregarBotonMenu("F1 - AYUDA", Keys.F1);
            AgregarBotonMenu("F2 - COMÚN", Keys.F2);
            AgregarBotonMenu("F3 - BUSCAR", Keys.F3);
            AgregarBotonMenu("F4 - ALTA", Keys.F4);
            AgregarBotonMenu("F5 - MODIFICAR", Keys.F5);
            AgregarBotonMenu("F6 - INV. BAJO", Keys.F6);
            AgregarBotonMenu("F7 - TICKETS", Keys.F7);
            AgregarBotonMenu("F8 - HISTORIAL", Keys.F8);
            AgregarBotonMenu("F9 - BAJAS", Keys.F9);
            AgregarBotonMenu("F10 - CORTE", Keys.F10);
            AgregarBotonMenu("F11 - PDF", Keys.F11);

            Label lblInstruccionBuscar = new Label();
            lblInstruccionBuscar.Text = "CÓDIGO DE PRODUCTO (ESCANEADO O F3)";
            lblInstruccionBuscar.Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold);
            lblInstruccionBuscar.ForeColor = textoGris;
            lblInstruccionBuscar.Location = new Point(20, 145);
            lblInstruccionBuscar.AutoSize = true;
            this.Controls.Add(lblInstruccionBuscar);

            txtCodigo.BackColor = blancoCard;
            txtCodigo.ForeColor = textoOscuro;
            txtCodigo.Font = new System.Drawing.Font("Arial", 18);
            txtCodigo.BorderStyle = BorderStyle.FixedSingle;
            txtCodigo.Width = 500;
            txtCodigo.Location = new Point(20, 165);

            btnCobrar.Text = "F12 - COBRAR";
            btnCobrar.BackColor = naranjaFerre;
            btnCobrar.ForeColor = Color.White;
            btnCobrar.Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
            btnCobrar.FlatStyle = FlatStyle.Flat;
            btnCobrar.FlatAppearance.BorderSize = 0;
            btnCobrar.Size = new Size(200, 60);
            btnCobrar.Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 80);
            btnCobrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCobrar.TabStop = false;
            btnCobrar.Cursor = Cursors.Hand;
            btnCobrar.NotifyDefault(false);

            btnCobrar.Click += (s, e) =>
            {
                txtCodigo_KeyDown(txtCodigo, new KeyEventArgs(Keys.F12));
                txtCodigo.Focus();
            };

            lblTotal.Text = "$ 0.00";
            lblTotal.Font = new System.Drawing.Font("Arial", 48, FontStyle.Bold);
            lblTotal.ForeColor = azulMarino;
            lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(btnCobrar.Left - 350, btnCobrar.Top - 10);

            btnReimprimirUltimo.Text = "REIMPRIMIR ÚLTIMO TICKET";
            btnReimprimirUltimo.BackColor = azulMarino;
            btnReimprimirUltimo.ForeColor = Color.White;
            btnReimprimirUltimo.Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            btnReimprimirUltimo.FlatStyle = FlatStyle.Flat;
            btnReimprimirUltimo.FlatAppearance.BorderSize = 0;
            btnReimprimirUltimo.Size = new Size(250, 40);
            btnReimprimirUltimo.Location = new Point(20, this.ClientSize.Height - 60);
            btnReimprimirUltimo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReimprimirUltimo.TabStop = false;
            btnReimprimirUltimo.Cursor = Cursors.Hand;
            btnReimprimirUltimo.NotifyDefault(false);
            btnReimprimirUltimo.Click += BtnReimprimirUltimo_Click;

            if (!this.Controls.Contains(btnReimprimirUltimo))
            {
                this.Controls.Add(btnReimprimirUltimo);
            }

            // --- DISEÑO DE LA TABLA ---
            dgvCarrito.BackgroundColor = fondoApp;
            dgvCarrito.BorderStyle = BorderStyle.None;
            dgvCarrito.EnableHeadersVisualStyles = false;

            dgvCarrito.ColumnHeadersDefaultCellStyle.BackColor = azulMarino;
            dgvCarrito.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCarrito.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            dgvCarrito.ColumnHeadersDefaultCellStyle.SelectionBackColor = azulMarino;

            dgvCarrito.DefaultCellStyle.BackColor = blancoCard;
            dgvCarrito.DefaultCellStyle.ForeColor = textoOscuro;
            dgvCarrito.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold);

            dgvCarrito.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgvCarrito.RowHeadersVisible = false;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCarrito.Location = new Point(20, 225);
            dgvCarrito.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 325);
            dgvCarrito.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgvCarrito.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 210);
            dgvCarrito.DefaultCellStyle.SelectionForeColor = azulMarino;

            // =========================================================================
            // --- SOLUCIÓN AL FOCUS DE LA TABLA ---
            // Si la tabla gana el foco (por un clic), mandamos el cursor de regreso al buscador
            // =========================================================================
            dgvCarrito.GotFocus += (s, e) =>
            {
                txtCodigo.Focus();
            };
        }

        // =====================================================================================
        // ESTO SOLUCIONA EL PROBLEMA DEL FOCUS: ATRAPA LAS TECLAS F DESDE CUALQUIER LADO
        // =====================================================================================
        private void Form1_KeyDown_Global(object sender, KeyEventArgs e)
        {
            // Si el usuario presiona una tecla entre F1 y F12, y el focus NO está en la caja de texto
            if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12 && !txtCodigo.Focused)
            {
                // Pasamos la tecla obligatoriamente al evento del buscador
                txtCodigo_KeyDown(txtCodigo, e);
                // Forzamos el focus de regreso
                txtCodigo.Focus();
            }
        }

        private void ConfigurarTabla()
        {
            dgvCarrito.Columns.Clear();
            dgvCarrito.Columns.Add("Codigo", "CÓDIGO");
            dgvCarrito.Columns.Add("Descripcion", "DESCRIPCIÓN");
            dgvCarrito.Columns.Add("Precio", "PRECIO");
            dgvCarrito.Columns.Add("Cantidad", "CANTIDAD");
            dgvCarrito.Columns.Add("Total", "TOTAL");
            dgvCarrito.Columns.Add("Stock", "EXISTENCIA");

            dgvCarrito.ReadOnly = true;
            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCarrito.MultiSelect = false;
            dgvCarrito.AllowUserToDeleteRows = false;

            dgvCarrito.Columns["Codigo"].FillWeight = 150;
            dgvCarrito.Columns["Descripcion"].FillWeight = 450;
            dgvCarrito.Columns["Precio"].FillWeight = 100;
            dgvCarrito.Columns["Cantidad"].FillWeight = 90;
            dgvCarrito.Columns["Total"].FillWeight = 120;
            dgvCarrito.Columns["Stock"].FillWeight = 90;

            dgvCarrito.RowTemplate.Height = 40;
            dgvCarrito.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn columna in dgvCarrito.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private async void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string codigo = txtCodigo.Text.Trim();
                if (!string.IsNullOrEmpty(codigo))
                {
                    txtCodigo.Enabled = false;
                    await BuscarYAgregarProductoApi(codigo);
                    txtCodigo.Enabled = true;
                    txtCodigo.Text = "";
                    txtCodigo.Focus();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (string.IsNullOrEmpty(txtCodigo.Text.Trim())) BorrarFilaSeleccionada();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                MoverSeleccionArriba();
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                MoverSeleccionAbajo();
            }
            else if ((e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) && string.IsNullOrEmpty(txtCodigo.Text.Trim()))
            {
                e.SuppressKeyPress = true;
                CambiarCantidadSeleccionada(1);
            }
            else if ((e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) && string.IsNullOrEmpty(txtCodigo.Text.Trim()))
            {
                e.SuppressKeyPress = true;
                CambiarCantidadSeleccionada(-1);
            }
            // --- F12: COBRAR VENTA ---
            else if (e.KeyCode == Keys.F12)
            {
                e.Handled = true;

                string textoMonto = lblTotal.Text.Replace("$", "").Trim();

                if (double.TryParse(textoMonto, out double totalExacto))
                {
                    if (totalExacto <= 0)
                    {
                        MessageBox.Show("Agrega al menos un producto para cobrar.", "Carrito Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    FormCobro ventanaCobro = new FormCobro(totalExacto);
                    ventanaCobro.ShowDialog();

                    if (ventanaCobro.PagoConfirmado)
                    {
                        string idVentaBD = await RegistrarVentaSupabase(totalExacto);
                        if (idVentaBD == "ERROR") return;

                        ultimoFolio = idVentaBD;
                        await ActualizarInventarioSupabase();

                        MessageBox.Show($"Cambio a entregar: $ {ventanaCobro.CambioEntregar:F2}", "Venta Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ImprimirYGuardarTicket(ultimoFolio, ventanaCobro.ImprimirTicket);

                        dgvCarrito.Rows.Clear();
                        ActualizarTotalVenta();
                        txtCodigo.Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                FormAyuda ventanaAyuda = new FormAyuda();
                ventanaAyuda.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                FormArticuloComun ventanaComun = new FormArticuloComun();
                ventanaComun.ShowDialog();

                if (ventanaComun.FueConfirmado)
                {
                    string codigoGenerico = "COMUN-" + DateTime.Now.Ticks.ToString().Substring(10);
                    double totalImporte = ventanaComun.PrecioArticulo * ventanaComun.CantidadArticulo;

                    int index = dgvCarrito.Rows.Add(
                        codigoGenerico,
                        ventanaComun.NombreArticulo,
                        $"$ {ventanaComun.PrecioArticulo:F2}",
                        ventanaComun.CantidadArticulo,
                        $"$ {totalImporte:F2}"
                    );

                    dgvCarrito.ClearSelection();
                    dgvCarrito.Rows[index].Selected = true;
                    dgvCarrito.FirstDisplayedScrollingRowIndex = index;

                    ActualizarTotalVenta();
                }
            }
            else if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                FormBuscarProducto ventanaBuscar = new FormBuscarProducto();
                ventanaBuscar.ShowDialog();

                if (!string.IsNullOrEmpty(ventanaBuscar.IdProductoSeleccionado))
                {
                    txtCodigo.Enabled = false;
                    await BuscarYAgregarProductoApi(ventanaBuscar.IdProductoSeleccionado);
                    txtCodigo.Enabled = true;
                    txtCodigo.Focus();
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                e.Handled = true;
                FormAgregarProducto ventanaAgregar = new FormAgregarProducto();
                ventanaAgregar.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                FormModificarProducto ventanaModificar = new FormModificarProducto();
                ventanaModificar.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F6)
            {
                e.Handled = true;
                FormInventarioBajo ventanaInventario = new FormInventarioBajo();
                ventanaInventario.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F7)
            {
                e.Handled = true;
                FormReimprimirTicket ventanaReimprimir = new FormReimprimirTicket();
                ventanaReimprimir.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                e.Handled = true;
                FormHistorialVentas ventanaHistorial = new FormHistorialVentas();
                ventanaHistorial.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F9)
            {
                e.Handled = true;
                FormDarDeBajaProducto ventanaBaja = new FormDarDeBajaProducto();
                ventanaBaja.ShowDialog();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F10)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                await RealizarCorteDeCaja();
                txtCodigo.Focus();
            }
            else if (e.KeyCode == Keys.F11)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                FormAbrirPdf ventanaPdf = new FormAbrirPdf();
                ventanaPdf.ShowDialog();
                txtCodigo.Focus();
            }
        }

        private void MoverSeleccionArriba()
        {
            if (dgvCarrito.Rows.Count > 0 && dgvCarrito.SelectedRows.Count > 0)
            {
                int indexActual = dgvCarrito.SelectedRows[0].Index;
                if (indexActual > 0)
                {
                    dgvCarrito.ClearSelection();
                    dgvCarrito.Rows[indexActual - 1].Selected = true;
                    dgvCarrito.FirstDisplayedScrollingRowIndex = indexActual - 1;
                }
            }
        }

        private void MoverSeleccionAbajo()
        {
            if (dgvCarrito.Rows.Count > 0 && dgvCarrito.SelectedRows.Count > 0)
            {
                int indexActual = dgvCarrito.SelectedRows[0].Index;
                if (indexActual < dgvCarrito.Rows.Count - 1)
                {
                    dgvCarrito.ClearSelection();
                    dgvCarrito.Rows[indexActual + 1].Selected = true;
                    dgvCarrito.FirstDisplayedScrollingRowIndex = indexActual + 1;
                }
            }
        }

        private async Task BuscarYAgregarProductoApi(string idProducto)
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
                        string desc = producto["descripcion"].ToString();
                        double precio = Convert.ToDouble(producto["precio"]);
                        int stock = Convert.ToInt32(producto["cantidad"]);

                        if (stock <= 0)
                        {
                            MessageBox.Show("¡Producto agotado!", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        bool productoEncontrado = false;
                        foreach (DataGridViewRow fila in dgvCarrito.Rows)
                        {
                            if (fila.Cells[0].Value != null && fila.Cells[0].Value.ToString() == idProducto)
                            {
                                int cantActual = Convert.ToInt32(fila.Cells[3].Value);
                                if (cantActual + 1 > stock)
                                {
                                    MessageBox.Show("¡Stock insuficiente!", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                fila.Cells[3].Value = cantActual + 1;
                                fila.Cells[4].Value = $"$ {(cantActual + 1) * precio:F2}";

                                dgvCarrito.ClearSelection();
                                fila.Selected = true;
                                productoEncontrado = true;
                                break;
                            }
                        }

                        if (!productoEncontrado)
                        {
                            int index = dgvCarrito.Rows.Add(idProducto, desc, $"$ {precio:F2}", 1, $"$ {precio:F2}", stock);
                            dgvCarrito.ClearSelection();
                            dgvCarrito.Rows[index].Selected = true;
                        }

                        ActualizarTotalVenta();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void dgvCarrito_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                BorrarFilaSeleccionada();
            }
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
            {
                CambiarCantidadSeleccionada(1);
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                CambiarCantidadSeleccionada(-1);
            }
        }

        private void BorrarFilaSeleccionada()
        {
            if (dgvCarrito.SelectedRows.Count > 0)
            {
                int indexBorrar = dgvCarrito.SelectedRows[0].Index;
                DataGridViewRow fila = dgvCarrito.SelectedRows[0];

                dgvCarrito.Rows.Remove(fila);
                ActualizarTotalVenta();
                if (dgvCarrito.Rows.Count > 0)
                {
                    int nuevoIndex = (indexBorrar > 0) ? indexBorrar - 1 : 0;
                    dgvCarrito.ClearSelection();
                    dgvCarrito.Rows[nuevoIndex].Selected = true;
                }

                txtCodigo.Focus();
            }
        }

        private void CambiarCantidadSeleccionada(int ajuste)
        {
            if (dgvCarrito.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = dgvCarrito.SelectedRows[0];

                int cantidadActual = Convert.ToInt32(fila.Cells[3].Value);
                int stock = Convert.ToInt32(fila.Cells[5].Value);

                string precioTexto = fila.Cells[2].Value.ToString().Replace("$", "").Trim();
                double precioUnitario = Convert.ToDouble(precioTexto);

                int nuevaCantidad = cantidadActual + ajuste;

                if (nuevaCantidad > stock)
                {
                    MessageBox.Show($"¡Stock insuficiente! Solo hay {stock} piezas disponibles.", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (nuevaCantidad <= 0) return;

                fila.Cells[3].Value = nuevaCantidad;

                double nuevoTotalFila = nuevaCantidad * precioUnitario;
                fila.Cells[4].Value = $"$ {nuevoTotalFila:F2}";

                ActualizarTotalVenta();
            }
        }

        private async Task ActualizarInventarioSupabase()
        {
            try
            {
                foreach (DataGridViewRow fila in dgvCarrito.Rows)
                {
                    string idProducto = fila.Cells[0].Value.ToString();
                    int cantidadVendida = Convert.ToInt32(fila.Cells[3].Value);
                    int stockActual = Convert.ToInt32(fila.Cells[5].Value);

                    int nuevoStock = stockActual - cantidadVendida;

                    string url = $"{SupabaseUrl}/rest/v1/producto?id=eq.{idProducto}";

                    JObject actualizacion = new JObject();
                    actualizacion["cantidad"] = nuevoStock;

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Headers.Add("apikey", SupabaseKey);
                    request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                    request.Content = new StringContent(actualizacion.ToString(), System.Text.Encoding.UTF8, "application/json");

                    await clienteHttp.SendAsync(request);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un error al actualizar el inventario: {ex.Message}", "Error de Nube", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> RegistrarVentaSupabase(double totalPagado)
        {
            try
            {
                string url = $"{SupabaseUrl}/rest/v1/venta";
                DateTime ahora = DateTime.Now;

                JObject nuevaVenta = new JObject();
                nuevaVenta["fecha"] = ahora.ToString("dd/MM/yyyy");
                nuevaVenta["hora"] = ahora.ToString("HH:mm:ss");
                nuevaVenta["total"] = totalPagado;

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");
                request.Headers.Add("Prefer", "return=representation");

                request.Content = new StringContent(nuevaVenta.ToString(), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string detalleError = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Supabase rechazó la venta.\n\nError exacto: {detalleError}", "Falla en la Nube", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "ERROR";
                }
                else
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray arregloVenta = JArray.Parse(jsonRespuesta);
                    if (arregloVenta.Count > 0)
                    {
                        JObject ventaGenerada = (JObject)arregloVenta[0];
                        return ventaGenerada["id"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error interno al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return "ERROR";
        }

        private void ActualizarTotalVenta()
        {
            double totalVenta = 0;

            foreach (DataGridViewRow fila in dgvCarrito.Rows)
            {
                if (fila.Cells[4].Value != null)
                {
                    string valorCelda = fila.Cells[4].Value.ToString();
                    valorCelda = valorCelda.Replace("$", "").Replace(" ", "").Replace(",", "").Trim();

                    if (double.TryParse(valorCelda, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double subtotal))
                    {
                        totalVenta += subtotal;
                    }
                }
            }
            lblTotal.Text = $"$ {totalVenta:F2}";
        }

        // =========================================================================================
        // --- 1. MÉTODO MAESTRO PARA IMPRIMIR DIRECTO AL PAPEL Y GUARDAR PDF CON iTextSharp ---
        // =========================================================================================
        private void ImprimirYGuardarTicket(string idVenta, bool imprimirFisico)
        {
            if (imprimirFisico)
            {
                try
                {
                    PrintDocument pdFisico = new PrintDocument();
                    pdFisico.DefaultPageSettings.PaperSize = new PaperSize("TicketTermico", 220, 800);
                    pdFisico.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                    pdFisico.PrintPage += new PrintPageEventHandler(GenerarDiseñoTicketImpresora);
                    pdFisico.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Verifica tu miniprinter física. Error: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            try
            {
                string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string rutaCarpeta = Path.Combine(rutaDocumentos, "Tickets_FerrePau");
                if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);
                string nombreArchivo = Path.Combine(rutaCarpeta, $"Ticket_{idVenta}.pdf");

                iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(180, 1000);
                Document doc = new Document(pageSize, 10, 10, 10, 10);
                PdfWriter.GetInstance(doc, new FileStream(nombreArchivo, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font tituloGrande = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 16, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font tituloNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font negrita = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font firma;
                try
                {
                    BaseFont bfCursiva = BaseFont.CreateFont(@"C:\Windows\Fonts\MTCORSVA.TTF", BaseFont.WINANSI, BaseFont.EMBEDDED);
                    firma = new iTextSharp.text.Font(bfCursiva, 11);
                }
                catch
                {
                    firma = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, iTextSharp.text.Font.ITALIC);
                }

                Paragraph numCompra = new Paragraph($"Folio: {idVenta}                    {DateTime.Now.ToString("dd/MM/yyyy")}", texto);
                numCompra.Alignment = Element.ALIGN_LEFT;
                doc.Add(numCompra);
                doc.Add(Chunk.NEWLINE);

                Paragraph encabezado = new Paragraph();
                encabezado.Alignment = Element.ALIGN_CENTER;
                encabezado.Add(new Chunk("FERRE-PAU\n", tituloGrande));
                encabezado.Add(new Chunk("Calle Vicente Guerrero\nHERIBERTO ADAME MEZA\nRFC: AAMH8807069RA\nTEL: 871-144-0669", tituloNormal));
                doc.Add(encabezado);
                doc.Add(Chunk.NEWLINE);

                PdfPCell CrearCelda(string txt, iTextSharp.text.Font font, int alineacion)
                {
                    PdfPCell celda = new PdfPCell(new Phrase(txt, font));
                    celda.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    celda.HorizontalAlignment = alineacion;
                    return celda;
                }

                PdfPTable tabla = new PdfPTable(3);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 15f, 75f, 35f });

                tabla.AddCell(CrearCelda("C", negrita, Element.ALIGN_LEFT));
                tabla.AddCell(CrearCelda("Descripción", negrita, Element.ALIGN_LEFT));
                tabla.AddCell(CrearCelda("T", negrita, Element.ALIGN_RIGHT));

                foreach (DataGridViewRow fila in dgvCarrito.Rows)
                {
                    string cantidad = fila.Cells[3].Value.ToString();
                    string descripcion = fila.Cells[1].Value.ToString();
                    string totalFila = fila.Cells[4].Value.ToString().Replace("$", "").Trim();

                    tabla.AddCell(CrearCelda(cantidad, texto, Element.ALIGN_LEFT));
                    tabla.AddCell(CrearCelda(descripcion, texto, Element.ALIGN_LEFT));
                    tabla.AddCell(CrearCelda(totalFila, texto, Element.ALIGN_RIGHT));
                }

                doc.Add(tabla);
                doc.Add(Chunk.NEWLINE);

                Paragraph totalTxt = new Paragraph($"Total: {lblTotal.Text}", negrita);
                totalTxt.Alignment = Element.ALIGN_RIGHT;
                doc.Add(totalTxt);
                doc.Add(Chunk.NEWLINE);

                Paragraph gracias = new Paragraph("¡Gracias por su compra!", texto);
                gracias.Alignment = Element.ALIGN_CENTER;
                doc.Add(gracias);

                Paragraph creador = new Paragraph("power for Isaac Romero :)", firma);
                creador.Alignment = Element.ALIGN_CENTER;
                doc.Add(creador);

                doc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando PDF de respaldo: " + ex.Message, "Error iTextSharp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =========================================================================================
        // --- 2. DISEÑO PARA LA IMPRESORA TÉRMICA DE 58mm (Ajuste Perfecto) ---
        // =========================================================================================
        private void GenerarDiseñoTicketImpresora(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            System.Drawing.Font fontTitulo = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            System.Drawing.Font fontNormal = new System.Drawing.Font("Arial", 8);
            System.Drawing.Font fontNegrita = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
            System.Drawing.Font fontCursiva = new System.Drawing.Font("Arial", 8, FontStyle.Italic);

            int y = 10;
            int x = 5;
            int ancho = 175;

            StringFormat formatoCentro = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat formatoDerecha = new StringFormat { Alignment = StringAlignment.Far };

            g.DrawString($"Folio: {ultimoFolio}", fontNormal, Brushes.Black, new PointF(x, y));
            g.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 20), formatoDerecha);
            y += 25;

            g.DrawString("FERRE-PAU", fontTitulo, Brushes.Black, new RectangleF(x, y, ancho, 25), formatoCentro);
            y += 20;
            g.DrawString("Calle Vicente Guerrero", fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
            y += 15;
            g.DrawString("HERIBERTO ADAME MEZA", fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
            y += 15;
            g.DrawString("RFC: AAMH8807069RA", fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
            y += 15;
            g.DrawString("TEL: 871-144-0669", fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
            y += 30;

            g.DrawString("C", fontNegrita, Brushes.Black, new PointF(x, y));
            g.DrawString("Descripción", fontNegrita, Brushes.Black, new PointF(x + 20, y));
            g.DrawString("T", fontNegrita, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoDerecha);
            y += 15;

            foreach (DataGridViewRow fila in dgvCarrito.Rows)
            {
                string cant = fila.Cells[3].Value.ToString();
                string desc = fila.Cells[1].Value.ToString();
                string totalFila = fila.Cells[4].Value.ToString().Replace("$", "").Trim();

                if (desc.Length > 14)
                {
                    g.DrawString(cant, fontNormal, Brushes.Black, new PointF(x, y));
                    g.DrawString(desc.Substring(0, 14), fontNormal, Brushes.Black, new PointF(x + 20, y));
                    g.DrawString(totalFila, fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoDerecha);
                    y += 15;
                    g.DrawString(desc.Substring(14), fontNormal, Brushes.Black, new PointF(x + 20, y));
                }
                else
                {
                    g.DrawString(cant, fontNormal, Brushes.Black, new PointF(x, y));
                    g.DrawString(desc, fontNormal, Brushes.Black, new PointF(x + 20, y));
                    g.DrawString(totalFila, fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoDerecha);
                }
                y += 18;
            }

            y += 10;
            string textoTotal = lblTotal.Text;
            g.DrawString($"Total: {textoTotal}", fontNegrita, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoDerecha);
            y += 25;

            g.DrawString("¡Gracias por su compra!", fontNormal, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
            y += 15;
            g.DrawString("power for Isaac Romero :)", fontCursiva, Brushes.Black, new RectangleF(x, y, ancho, 15), formatoCentro);
        }

        // --- MÉTODOS PARA EL CORTE DE CAJA (F10) ---
        private async Task RealizarCorteDeCaja()
        {
            try
            {
                string fechaHoy = DateTime.Now.ToString("dd/MM/yyyy");

                string url = $"{SupabaseUrl}/rest/v1/venta?fecha=eq.{Uri.EscapeDataString(fechaHoy)}&select=total";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray ventasDeHoy = JArray.Parse(jsonRespuesta);

                    double totalCaja = 0;
                    int cantidadVentas = ventasDeHoy.Count;

                    foreach (JObject venta in ventasDeHoy)
                    {
                        totalCaja += Convert.ToDouble(venta["total"]);
                    }

                    DialogResult resultado = MessageBox.Show(
                        $"--- RESUMEN DEL DÍA ---\n\n" +
                        $"Fecha: {fechaHoy}\n" +
                        $"Tickets Emitidos: {cantidadVentas}\n" +
                        $"Total en Caja: $ {totalCaja:F2}\n\n" +
                        $"¿Deseas imprimir y guardar el Ticket de Corte de Caja?",
                        "F10 - Corte de Caja",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (resultado == DialogResult.Yes)
                    {
                        ImprimirTicketCorte(fechaHoy, cantidadVentas, totalCaja);
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo obtener la información de Supabase.", "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al hacer el corte: {ex.Message}");
            }
        }

        private void ImprimirTicketCorte(string fecha, int tickets, double total)
        {
            corteFecha = fecha;
            corteTickets = tickets;
            corteTotal = total;

            PrintDocument pdCorte = new PrintDocument();

            pdCorte.DefaultPageSettings.PaperSize = new PaperSize("TicketTermico", 300, 600);
            pdCorte.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            pdCorte.PrintPage += new PrintPageEventHandler(GenerarDiseñoCorte);

            string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaCarpeta = Path.Combine(rutaDocumentos, "Tickets_FerrePau");
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            string fechaArchivo = fecha.Replace("/", "-");
            string nombreArchivo = Path.Combine(rutaCarpeta, $"CorteCaja_{fechaArchivo}.pdf");

            try
            {
                pdCorte.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pdCorte.PrinterSettings.PrintToFile = true;
                pdCorte.PrinterSettings.PrintFileName = nombreArchivo;
                pdCorte.Print();

                MessageBox.Show("El Corte de Caja se ha guardado exitosamente en tus Documentos.", "Corte Finalizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el corte: {ex.Message}");
            }
        }

        private void GenerarDiseñoCorte(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            System.Drawing.Font fontTitulo = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            System.Drawing.Font fontNormal = new System.Drawing.Font("Arial", 10);
            System.Drawing.Font fontNegrita = new System.Drawing.Font("Arial", 10, FontStyle.Bold);

            int y = 20;
            int x = 10;

            g.DrawString("FERRE PAU", fontTitulo, Brushes.Black, new PointF(x + 50, y));
            y += 30;
            g.DrawString("CORTE DE CAJA", fontNegrita, Brushes.Black, new PointF(x + 40, y));
            y += 20;
            g.DrawString("------------------------------------------------", fontNormal, Brushes.Black, new PointF(x, y));
            y += 20;
            g.DrawString("Fecha del corte: " + corteFecha, fontNormal, Brushes.Black, new PointF(x, y));
            y += 20;
            g.DrawString("Hora de impresión: " + DateTime.Now.ToString("HH:mm:ss"), fontNormal, Brushes.Black, new PointF(x, y));
            y += 20;
            g.DrawString("Cajero: " + lblCajero.Text.Replace("CAJERO: ", "").Trim(), fontNormal, Brushes.Black, new PointF(x, y));
            y += 20;
            g.DrawString("------------------------------------------------", fontNormal, Brushes.Black, new PointF(x, y));
            y += 30;

            g.DrawString("Total de Tickets: " + corteTickets, fontNormal, Brushes.Black, new PointF(x, y));
            y += 40;
            g.DrawString("TOTAL EN CAJA:", fontNegrita, Brushes.Black, new PointF(x, y));
            y += 25;
            g.DrawString("$ " + corteTotal.ToString("F2"), new System.Drawing.Font("Arial", 18, FontStyle.Bold), Brushes.Black, new PointF(x + 40, y));
            y += 50;

            g.DrawString("------------------------------------------------", fontNormal, Brushes.Black, new PointF(x, y));
            y += 20;
            g.DrawString("Firma del Cajero", fontNormal, Brushes.Black, new PointF(x + 45, y));
        }

        private void BtnReimprimirUltimo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ultimoFolio))
            {
                MessageBox.Show("Aún no se ha realizado ninguna venta en esta sesión.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCodigo.Focus();
                return;
            }

            string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaCarpeta = Path.Combine(rutaDocumentos, "Tickets_FerrePau");
            string rutaArchivo = Path.Combine(rutaCarpeta, $"Ticket_{ultimoFolio}.pdf");

            if (File.Exists(rutaArchivo))
            {
                try
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaArchivo,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);

                    MessageBox.Show("El último ticket se está enviando a la impresora.", "Imprimiendo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Revisa la conexión de tu impresora. Error: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"No se encontró el archivo del último ticket (Folio: {ultimoFolio}).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            txtCodigo.Focus();
        }
    }
}