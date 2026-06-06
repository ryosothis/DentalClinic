using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class Login : Page
    {
        private DatabaseService _databaseService;

        public Login()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        private void HomePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new MainWindow());
        }

        private void AboutPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new About());
        }

        private void ServicesPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void PricePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Price());
        }

        private void ProfilePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Profile());
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailTextBox.Text;
                string password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Заполните email и пароль");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    ShowError("Введите корректный email");
                    return;
                }

                User user = await _databaseService.AuthorizeUserAsync(email, password);

                if (user != null)
                {
                    AuthManager.Login(user);

                    ShowSuccess($"Вход выполнен успешно");
                    NavigationService?.Navigate(new MainWindow());
                }
                else
                {
                    ShowError("Неверные email или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void RegisterLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Register());
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}