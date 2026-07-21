using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVentaFerrePau
{
    public partial class FormArticuloComun : Form
    {
        private TextBox txtNombre = new TextBox();
        private TextBox txtPrecio = new TextBox();
        private TextBox txtCantidad = new TextBox();
        private Button btnAgregar = new Button();

        // Variables públicas para que la ventana principal de ventas pueda leer lo que escribiste
        public string NombreArticulo { get; private set; } = "";
        public double PrecioArticulo { get; private set; } = 0;
        public double CantidadArticulo { get; private set; } = 0;
        public bool FueConfirmado { get; private set; } = false;

        public FormArticuloComun()
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
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F2 - Agregar Artículo Común";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true;

            Label lblTitulo = new Label { Text = "ARTÍCULO COMÚN (SIN CÓDIGO)", ForeColor = azulMarino, Font = new Font("Arial", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            int y = 70;

            // 1. Nombre
            CrearCampo("1. DESCRIPCIÓN DEL ARTÍCULO:", txtNombre, y, azulMarino, textoOscuro);
            // Al dar Enter, salta al precio
            txtNombre.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtPrecio.Focus(); } };

            y += 85;
            // 2. Precio
            CrearCampo("2. PRECIO UNITARIO ($):", txtPrecio, y, azulMarino, textoOscuro);
            txtPrecio.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtCantidad.Focus(); } };

            y += 85;
            // 3. Cantidad (Permite decimales para granel)
            CrearCampo("3. CANTIDAD (Ej. 1, 1.5, 0.25):", txtCantidad, y, azulMarino, textoOscuro);
            txtCantidad.Text = "1"; // Valor por defecto
            // Al dar Enter en la cantidad, presiona el botón agregar automáticamente
            txtCantidad.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; btnAgregar.PerformClick(); } };

            y += 85;
            // Botón Agregar
            btnAgregar.Text = "AGREGAR A LA VENTA";
            btnAgregar.BackColor = naranjaFerre;
            btnAgregar.ForeColor = Color.White;
            btnAgregar.Font = new Font("Arial", 14, FontStyle.Bold);
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Size = new Size(440, 50);
            btnAgregar.Location = new Point(20, y);
            btnAgregar.Click += BtnAgregar_Click;
            this.Controls.Add(btnAgregar);

            // Salir con Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void CrearCampo(string textoLabel, TextBox cajaTexto, int y, Color colorLabel, Color colorTexto)
        {
            Label lbl = new Label { Text = textoLabel, ForeColor = colorLabel, Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lbl);

            cajaTexto.Location = new Point(20, y + 25);
            cajaTexto.Width = 440;
            cajaTexto.Font = new Font("Arial", 14);
            cajaTexto.BackColor = Color.White;
            cajaTexto.ForeColor = colorTexto;
            cajaTexto.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(cajaTexto);
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre o descripción del artículo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!double.TryParse(txtPrecio.Text.Trim(), out double precio) || precio <= 0)
            {
                MessageBox.Show("Ingresa un precio válido.", "Error de Precio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return;
            }

            if (!double.TryParse(txtCantidad.Text.Trim(), out double cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida.", "Error de Cantidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return;
            }

            // Guardamos los datos en las variables públicas y avisamos que todo salió bien
            NombreArticulo = txtNombre.Text.Trim().ToUpper();
            PrecioArticulo = precio;
            CantidadArticulo = cantidad;
            FueConfirmado = true;

            this.Close();
        }
    }
}