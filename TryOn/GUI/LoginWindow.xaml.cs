using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BLL;
using Entities;

namespace GUI
{
    public partial class LoginWindow : Window
    {
        private readonly ClienteService _clienteService;
        private readonly AdministradorService _administradorService;
        private bool _isSignIn;

        public LoginWindow(bool isSignIn = true)
        {
            InitializeComponent();
            _clienteService = new ClienteService();
            _administradorService = new AdministradorService();
            _isSignIn = isSignIn;

            // Configurar la ventana según el modo (Sign In o Sign Up)
            ConfigurarVentana();

            // Permitir arrastrar la ventana
            this.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void ConfigurarVentana()
        {
            if (_isSignIn)
            {
                LoginPanel.Visibility = Visibility.Visible;
                RegisterPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                RegisterPanel.Visibility = Visibility.Visible;
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            _isSignIn = true;
            ConfigurarVentana();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            _isSignIn = false;
            ConfigurarVentana();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("La funcionalidad de recuperación de contraseña se implementará próximamente.",
                "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //private void TipoUsuario_Checked(object sender, RoutedEventArgs e)
        //{
        //    // Ajustar la interfaz según el tipo de usuario seleccionado
        //    if (AdminRadioButton.IsChecked == true && !_isSignIn)
        //    {
        //        MessageBox.Show("El registro de administradores solo puede ser realizado por otro administrador.",
        //            "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        //        ClienteRadioButton.IsChecked = true;
        //    }
        //}

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Validar campos
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Por favor, complete todos los campos.",
                        "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Intentar iniciar sesión como cliente primero
                Cliente cliente = _clienteService.IniciarSesion(email, password);
                if (cliente != null)
                {
                    MessageBox.Show($"Bienvenido, {cliente.Nombre}!",
                        "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Aquí implementaremos la navegación al catálogo como cliente
                    // CatalogoWindow catalogoWindow = new CatalogoWindow(cliente);
                    // catalogoWindow.Show();
                    this.Close();
                    return;
                }

                // Si no es cliente, intentar como administrador
                Administrador admin = _administradorService.IniciarSesion(email, password);
                if (admin != null)
                {
                    MessageBox.Show($"Bienvenido, Administrador {admin.Nombre}!",
                        "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Aquí implementaremos la navegación al panel de administración
                    // AdminPanelWindow adminPanel = new AdminPanelWindow(admin);
                    // adminPanel.Show();
                    this.Close();
                    return;
                }

                // Si llegamos aquí, las credenciales son incorrectas
                MessageBox.Show("Correo electrónico o contraseña incorrectos.",
                    "Error de inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = NombreTextBox.Text.Trim();
                string apellido = ApellidoTextBox.Text.Trim();
                string email = RegisterEmailTextBox.Text.Trim();
                string password = RegisterPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                // Validar campos
                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Por favor, complete todos los campos.",
                        "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Las contraseñas no coinciden.",
                        "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Cliente nuevoCliente = new Cliente
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    Password = password,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                string resultado = _clienteService.Guardar(nuevoCliente);
                if (resultado.StartsWith("Cliente guardado"))
                {
                    MessageBox.Show("Registro exitoso. Ahora puede iniciar sesión.",
                        "Registro completado", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cambiar a la vista de inicio de sesión
                    _isSignIn = true;
                    ConfigurarVentana();

                    // Pre-llenar el campo de correo electrónico
                    EmailTextBox.Text = email;
                }
                else
                {
                    MessageBox.Show(resultado,
                        "Error de registro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrarse: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}