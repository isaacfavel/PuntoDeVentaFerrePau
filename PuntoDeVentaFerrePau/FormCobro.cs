using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVentaFerrePau
{
    public partial class FormCobro : Form
    {
        private double totalCobrar;
        public bool PagoConfirmado { get; private set; } = false;
        public double CambioEntregar { get; private set; } = 0.0;

        // Esta propiedad le dirá a Form1 si el cajero quiere ticket o no
        public bool ImprimirTicket { get; private set; } = false;

        public FormCobro(double totalVenta)
        {
            InitializeComponent();
            totalCobrar = totalVenta;
            AplicarTemaYPosiciones();
        }

        private void AplicarTemaYPosiciones()
        {
            // --- PALETA DE COLORES "LIGHT" ---
            Color fondoApp = Color.FromArgb(244, 246, 249);
            Color azulMarino = Color.FromArgb(32, 54, 97);
            Color naranjaFerre = Color.FromArgb(244, 114, 22);
            Color textoOscuro = Color.FromArgb(40, 40, 40);

            this.BackColor = fondoApp;
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Cobrar Venta";
            this.Icon = Properties.Resources.iconoLogo;
            // Total
            Label lblTotal = new Label { Text = $"TOTAL: $ {totalCobrar:F2}", Font = new Font("Arial", 24, FontStyle.Bold), ForeColor = azulMarino, Size = new Size(360, 50), Location = new Point(10, 20), TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblTotal);

            // Efectivo
            Label lblEfectivo = new Label { Text = "Efectivo Recibido:", Font = new Font("Arial", 12), ForeColor = azulMarino, Location = new Point(20, 90), AutoSize = true };
            this.Controls.Add(lblEfectivo);

            TextBox txtEfectivo = new TextBox { Name = "txtEfectivo", Font = new Font("Arial", 22, FontStyle.Bold), Size = new Size(340, 40), Location = new Point(20, 120), TextAlign = HorizontalAlignment.Right };
            txtEfectivo.TextChanged += TxtEfectivo_TextChanged;
            txtEfectivo.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            this.Controls.Add(txtEfectivo);

            // Cambio
            Label lblCambio = new Label { Name = "lblCambio", Text = "Cambio: $ 0.00", Font = new Font("Arial", 18, FontStyle.Bold), ForeColor = Color.Green, Size = new Size(340, 40), Location = new Point(20, 180), TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(lblCambio);

            // --- BOTONES DE COBRO ---
            Button btnConTicket = new Button { Text = "COBRAR CON TICKET", BackColor = naranjaFerre, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Size = new Size(340, 50), Location = new Point(20, 240), FlatStyle = FlatStyle.Flat };
            btnConTicket.FlatAppearance.BorderSize = 0;
            btnConTicket.Click += (s, e) => { ImprimirTicket = true; ProcesarPago(txtEfectivo); };
            this.Controls.Add(btnConTicket);

            Button btnSinTicket = new Button { Text = "COBRAR SIN TICKET", BackColor = azulMarino, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Size = new Size(340, 50), Location = new Point(20, 300), FlatStyle = FlatStyle.Flat };
            btnSinTicket.FlatAppearance.BorderSize = 0;
            btnSinTicket.Click += (s, e) => { ImprimirTicket = false; ProcesarPago(txtEfectivo); };
            this.Controls.Add(btnSinTicket);
        }

        private void TxtEfectivo_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            Label lblCambio = (Label)this.Controls["lblCambio"];
            if (double.TryParse(txt.Text, out double efectivo))
            {
                CambioEntregar = efectivo - totalCobrar;
                lblCambio.Text = CambioEntregar >= 0 ? $"Cambio: $ {CambioEntregar:F2}" : "Falta dinero";
                lblCambio.ForeColor = CambioEntregar >= 0 ? Color.Green : Color.Red;
            }
        }

        private void ProcesarPago(TextBox txt)
        {
            if (double.TryParse(txt.Text, out double efectivo) && efectivo >= totalCobrar)
            {
                PagoConfirmado = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Efectivo insuficiente o inválido.");
            }
        }
    }
}