using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PuntoDeVentaFerrePau
{
    public partial class FormLogin : Form
    {
        // --- CONEXIÓN A SUPABASE ---
        private static readonly string SupabaseUrl = "https://pttcycyawmylaxdhlnit.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0dGN5Y3lhd215bGF4ZGhsbml0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM5ODkxOTUsImV4cCI6MjA5OTU2NTE5NX0.ja9Hs1bebABoMlnDdUROhR0g5AACjCEnupwgAnWA2fE";
        private static readonly HttpClient clienteHttp = new HttpClient();

        private Panel tarjetaLogin = new Panel();
        private TextBox txtUsuario = new TextBox();
        private TextBox txtPassword = new TextBox();
        private Button btnIngresar = new Button();

        // Colores
        private Color fondoApp = Color.FromArgb(244, 246, 249);
        private Color azulMarino = Color.FromArgb(32, 54, 97);
        private Color naranjaFerre = Color.FromArgb(244, 114, 22);
        private Color grisPlaceholder = Color.FromArgb(150, 150, 150);
        private Color textoOscuro = Color.FromArgb(40, 40, 40);

        public FormLogin()
        {
            InitializeComponent();
            ConfigurarPantalla();
        }

        private void ConfigurarPantalla()
        {
            this.Icon = new Icon(@"C:\Users\chino\source\repos\PuntoDeVentaFerrePau\PuntoDeVentaFerrePau\logo1 (1).ico");
            this.Size = new Size(380, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = fondoApp;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Ferre-Pau - Iniciar Sesión";

            // --- LOGO REAL CON PICTUREBOX ---
            PictureBox picLogo = new PictureBox();
            picLogo.Size = new Size(160, 160);
            picLogo.Location = new Point(100, 40);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom; // Ajusta la imagen sin deformarla

            // Tu ruta exacta del logo
            try
            {
                picLogo.Image = Image.FromFile(@"C:\Users\chino\source\repos\PuntoDeVentaFerrePau\PuntoDeVentaFerrePau\logo1.png");
            }
            catch
            {
                // Si llegaras a mover la imagen de lugar por error, se pondrá gris para no trabar el programa
                picLogo.BackColor = Color.LightGray;
            }

            // Mantenemos el recorte circular perfecto para que se vea elegante
            System.Drawing.Drawing2D.GraphicsPath rutaCircular = new System.Drawing.Drawing2D.GraphicsPath();
            rutaCircular.AddEllipse(0, 0, picLogo.Width, picLogo.Height);
            picLogo.Region = new Region(rutaCircular);

            this.Controls.Add(picLogo);

            // Textos
            Label lblBienvenido = new Label { Text = "¡BIENVENIDO!", Font = new Font("Arial", 22, FontStyle.Bold | FontStyle.Italic), ForeColor = azulMarino, AutoSize = true, Location = new Point(75, 220) };
            this.Controls.Add(lblBienvenido);
            Label lblSubtitulo = new Label { Text = "Inicia sesión para continuar", Font = new Font("Arial", 10, FontStyle.Italic), ForeColor = grisPlaceholder, AutoSize = true, Location = new Point(100, 260) };
            this.Controls.Add(lblSubtitulo);

            // Tarjeta Blanca
            tarjetaLogin.BackColor = Color.White;
            tarjetaLogin.Size = new Size(300, 200);
            tarjetaLogin.Location = new Point(32, 300);
            this.Controls.Add(tarjetaLogin);

            // Caja de Usuario
            txtUsuario.Text = "Usuario";
            txtUsuario.ForeColor = grisPlaceholder;
            txtUsuario.Font = new Font("Arial", 14);
            txtUsuario.BorderStyle = BorderStyle.FixedSingle;
            txtUsuario.Size = new Size(260, 30);
            txtUsuario.Location = new Point(20, 20);
            txtUsuario.Enter += (s, e) => { if (txtUsuario.Text == "Usuario") { txtUsuario.Text = ""; txtUsuario.ForeColor = textoOscuro; } };
            txtUsuario.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtUsuario.Text)) { txtUsuario.Text = "Usuario"; txtUsuario.ForeColor = grisPlaceholder; } };
            tarjetaLogin.Controls.Add(txtUsuario);

            // Caja de Contraseña
            txtPassword.Text = "Contraseña";
            txtPassword.ForeColor = grisPlaceholder;
            txtPassword.Font = new Font("Arial", 14);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Size = new Size(260, 30);
            txtPassword.Location = new Point(20, 70);
            txtPassword.Enter += (s, e) => { if (txtPassword.Text == "Contraseña") { txtPassword.Text = ""; txtPassword.ForeColor = textoOscuro; txtPassword.UseSystemPasswordChar = true; } };
            txtPassword.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtPassword.Text)) { txtPassword.Text = "Contraseña"; txtPassword.ForeColor = grisPlaceholder; txtPassword.UseSystemPasswordChar = false; } };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnIngresar_Click(s, e); } };
            tarjetaLogin.Controls.Add(txtPassword);

            // Botón Ingresar
            btnIngresar.Text = "INGRESAR";
            btnIngresar.BackColor = naranjaFerre;
            btnIngresar.ForeColor = Color.White;
            btnIngresar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Size = new Size(260, 45);
            btnIngresar.Location = new Point(20, 130);
            btnIngresar.Click += BtnIngresar_Click;
            tarjetaLogin.Controls.Add(btnIngresar);

            this.ActiveControl = txtUsuario;
        }

        private async void BtnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (usuario == "Usuario" || string.IsNullOrEmpty(usuario) || password == "Contraseña" || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor ingresa tu usuario y contraseña.", "Campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnIngresar.Text = "VALIDANDO...";
            btnIngresar.Enabled = false;

            try
            {
                // Hacemos la consulta a la tabla 'usuario', buscando el 'nombreusuario' y 'pasword' (con una sola S) exactos
                string url = $"{SupabaseUrl}/rest/v1/usuario?nombreusuario=eq.{Uri.EscapeDataString(usuario)}&pasword=eq.{Uri.EscapeDataString(password)}&select=nombrecompleto";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", SupabaseKey);
                request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

                HttpResponseMessage response = await clienteHttp.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    JArray arrUsuarios = JArray.Parse(jsonRespuesta);

                    // Si el arreglo trae información, significa que el usuario y clave existen y son correctos
                    if (arrUsuarios.Count > 0)
                    {
                        JObject usr = (JObject)arrUsuarios[0];
                        string nombreCompleto = usr["nombrecompleto"].ToString();

                        // Mandamos el nombre del empleado que se logueó a la caja registradora
                        Form1 ventanaCaja = new Form1(nombreCompleto);
                        ventanaCaja.FormClosed += (s, args) => this.Close();
                        this.Hide();
                        ventanaCaja.Show();
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos. Verifica tus datos.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPassword.Text = "";
                        txtPassword.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Error al contactar con la base de datos.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error en la red: {ex.Message}");
            }
            finally
            {
                btnIngresar.Text = "INGRESAR";
                btnIngresar.Enabled = true;
            }
        }
    }
}