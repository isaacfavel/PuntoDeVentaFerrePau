using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace PuntoDeVentaFerrePau
{
    public partial class FormReimprimirTicket : Form
    {
        private TextBox txtFolio = new TextBox();
        private Button btnBuscar = new Button();
        private Button btnImprimir = new Button();

        // ¡Este es el control mágico que mostrará el PDF adentro del programa!
        private WebBrowser visorPdf = new WebBrowser();
        private string rutaTicketActual = "";

        public FormReimprimirTicket()
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
            // Hacemos la ventana mucho más grande (como un ticket largo)
            this.Size = new Size(600, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F7 - Reimpresión de Tickets";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true;

            Label lblTitulo = new Label { Text = "BUSCAR TICKET POR FOLIO", ForeColor = azulMarino, Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // Caja para escribir el número
            txtFolio.Location = new Point(20, 50);
            txtFolio.Width = 250;
            txtFolio.Font = new Font("Arial", 14);
            txtFolio.BackColor = Color.White;
            txtFolio.ForeColor = textoOscuro;
            txtFolio.BorderStyle = BorderStyle.FixedSingle;
            txtFolio.KeyDown += TxtFolio_KeyDown;
            this.Controls.Add(txtFolio);

            // Botón de buscar a un lado (Azul Marino)
            btnBuscar.Text = "BUSCAR";
            btnBuscar.BackColor = azulMarino;
            btnBuscar.ForeColor = Color.White;
            btnBuscar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnBuscar.FlatStyle = FlatStyle.Flat;
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Size = new Size(120, 30);
            btnBuscar.Location = new Point(280, 50);
            btnBuscar.Click += BtnBuscar_Click;
            this.Controls.Add(btnBuscar);

            // EL VISOR DEL TICKET (Abarca el centro de la pantalla)
            visorPdf.Location = new Point(20, 100);
            visorPdf.Size = new Size(540, 520);
            this.Controls.Add(visorPdf);

            // Botón grandote de IMPRIMIR hasta abajo (Naranja)
            btnImprimir.Text = "IMPRIMIR TICKET";
            btnImprimir.BackColor = naranjaFerre;
            btnImprimir.ForeColor = Color.White;
            btnImprimir.Font = new Font("Arial", 14, FontStyle.Bold);
            btnImprimir.FlatStyle = FlatStyle.Flat;
            btnImprimir.FlatAppearance.BorderSize = 0;
            btnImprimir.Size = new Size(540, 50);
            btnImprimir.Location = new Point(20, 640);
            btnImprimir.Enabled = false; // Se bloquea hasta que encuentres un ticket válido
            btnImprimir.Click += BtnImprimir_Click;
            this.Controls.Add(btnImprimir);

            // Salir con Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void TxtFolio_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuscarYMostrarTicket();
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            BuscarYMostrarTicket();
        }

        private void BuscarYMostrarTicket()
        {
            string folio = txtFolio.Text.Trim();
            if (string.IsNullOrEmpty(folio)) return;

            string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaCarpeta = Path.Combine(rutaDocumentos, "Tickets_FerrePau");
            string rutaArchivo = Path.Combine(rutaCarpeta, $"Ticket_{folio}.pdf");

            if (File.Exists(rutaArchivo))
            {
                rutaTicketActual = rutaArchivo;

                // Mágia: Le decimos al visor que dibuje el PDF en nuestra ventana
                visorPdf.Navigate(rutaArchivo);

                btnImprimir.Enabled = true; // Ya podemos imprimir
                btnImprimir.Focus();
            }
            else
            {
                MessageBox.Show($"No se encontró el Ticket con folio {folio}.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnImprimir.Enabled = false;
                visorPdf.Navigate("about:blank"); // Limpiamos la pantalla si no hay nada
                txtFolio.SelectAll();
                txtFolio.Focus();
            }
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rutaTicketActual) && File.Exists(rutaTicketActual))
            {
                try
                {
                    // Esto le dice a Windows que mande el archivo silenciosamente a tu impresora predeterminada
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaTicketActual,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    System.Diagnostics.Process.Start(psi);

                    MessageBox.Show("El ticket se está enviando a la impresora.", "Imprimiendo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Revisa la conexión de tu impresora. Error: {ex.Message}", "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}