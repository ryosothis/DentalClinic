using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class Register : Page
    {
        private DatabaseService _databaseService;

        public Register()
        {
            InitializeComponent();

            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            BirthDatePicker.DisplayDateEnd = DateTime.Today;

            RegisterButton.Click += RegisterButton_Click;
            LoginLink.MouseDown += LoginLink_MouseDown;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                string phone = PhoneTextBox.Text;
                string email = EmailTextBox.Text;
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                DateTime? birthDate = BirthDatePicker.SelectedDate;
                bool agreement = AgreementCheckBox.IsChecked ?? false;

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Заполните все обязательные поля (Имя, Фамилия, Email, Пароль)");
                    return;
                }

                if (!agreement)
                {
                    ShowError("Необходимо согласие с условиями использования");
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowError("Пароли не совпадают");
                    return;
                }

                if (password.Length < 6)
                {
                    ShowError("Пароль должен содержать минимум 6 символов");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    ShowError("Введите корректный email");
                    return;
                }

                if (!birthDate.HasValue)
                {
                    ShowError("Укажите дату рождения");
                    return;
                }

                if (birthDate.Value > DateTime.Today)
                {
                    ShowError("Дата рождения не может быть в будущем");
                    return;
                }

                if (await _databaseService.CheckEmailExistsAsync(email))
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                int? userId = await _databaseService.RegisterUserAsync(
                    email, password, firstName, middleName, lastName, phone, birthDate.Value);

                if (userId.HasValue)
                {
                    ShowSuccess("Регистрация прошла успешно!");
                    NavigationService?.Navigate(new Login());
                }
                else
                {
                    ShowError("Ошибка при регистрации пользователя");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void LoginLink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Login());
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
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                NavigationService?.Navigate(new Profile());
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в систему для просмотра профиля");
                NavigationService?.Navigate(new Login());
            }
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