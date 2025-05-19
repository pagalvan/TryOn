using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BLL;
using Entities;

namespace GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Permitir arrastrar la ventana
            this.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana de login en modo inicio de sesión
            LoginWindow loginWindow = new LoginWindow();
            this.Hide();
            loginWindow.ShowDialog();
            this.Show();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana de login en modo registro
            LoginWindow loginWindow = new LoginWindow();
            this.Hide();
            loginWindow.ShowDialog();
            this.Show();
        }

        private void VerColeccion_Click(object sender, RoutedEventArgs e)
        {
            // Aquí implementaremos la navegación al catálogo en modo invitado
            MessageBox.Show("Navegando al catálogo en modo invitado. Esta funcionalidad se implementará próximamente.",
                "Ver Colección", MessageBoxButton.OK, MessageBoxImage.Information);

            // CatalogoWindow catalogoWindow = new CatalogoWindow(modoInvitado: true);
            // this.Hide();
            // catalogoWindow.ShowDialog();
            // this.Show();
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