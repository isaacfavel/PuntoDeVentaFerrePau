using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVentaFerrePau
{
    public partial class FormAyuda : Form
    {
        public FormAyuda()
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
            this.Size = new Size(600, 700); // Un poco más alta para que quepa el F9
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = fondoApp;
            this.Text = "F1 - Ayuda y Atajos de Teclado";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true;

            Label lblTitulo = new Label
            {
                Text = "ATAJOS DE TECLADO (FERRE-PAU)",
                ForeColor = azulMarino, // Título en azul marino
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitulo);

            int y = 70;

            // Función adaptada a los colores claros
            void AgregarAtajo(string tecla, string descripcion)
            {
                Label lblTecla = new Label { Text = tecla, ForeColor = naranjaFerre, Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(30, y), AutoSize = true };
                Label lblDesc = new Label { Text = "- " + descripcion, ForeColor = textoOscuro, Font = new Font("Arial", 12), Location = new Point(110, y), AutoSize = true, MaximumSize = new Size(450, 0) };

                this.Controls.Add(lblTecla);
                this.Controls.Add(lblDesc);
                y += 40; // Espacio entre cada línea
            }

            // Agregamos todos los atajos, incluyendo el nuevo F9
            AgregarAtajo("F1", "Muestra esta pantalla de ayuda.");
            AgregarAtajo("F3", "Busca productos por nombre (si no tienen código).");
            AgregarAtajo("F4", "Da de alta un producto nuevo en el inventario.");
            AgregarAtajo("F5", "Actualiza el precio o el stock de un producto.");
            AgregarAtajo("F6", "Muestra reporte de inventario bajo (5 piezas o menos).");
            AgregarAtajo("F7", "Busca y abre un ticket anterior para reimprimirlo.");
            AgregarAtajo("F8", "Muestra el historial de ventas por fecha.");
            AgregarAtajo("F9", "Da de baja (elimina) un producto del inventario."); // <-- NUEVO ATAJO
            AgregarAtajo("F10", "Realiza el Corte de Caja del día y genera ticket.");
            AgregarAtajo("F12", "Abre la ventana para COBRAR la venta actual.");

            y += 10; // Un poco más de espacio para separar las teclas especiales

            AgregarAtajo("+ / -", "Suma o resta 1 a la cantidad del producto seleccionado.");
            AgregarAtajo("SUPR", "Elimina el producto seleccionado de la venta actual.");
            AgregarAtajo("↑ / ↓", "Mueve la selección hacia arriba o abajo en el carrito.");

            // Instrucción para salir
            Label lblCerrar = new Label
            {
                Text = "Presiona ESCAPE (ESC) para cerrar esta ventana.",
                ForeColor = Color.DimGray,
                Font = new Font("Arial", 10, FontStyle.Italic),
                Location = new Point(20, y + 30),
                AutoSize = true
            };
            this.Controls.Add(lblCerrar);

            // Cerramos la ventana si presionan Escape
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }
    }
}