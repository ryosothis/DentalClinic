using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class AdminUserEdit : Page
    {
        private readonly DatabaseService _databaseService;
        private int _editingUserId = -1;
        private bool _isEditMode = false;

        public AdminUserEdit()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            BirthDatePicker.DisplayDateEnd = DateTime.Today;
        }

        public AdminUserEdit(int userId) : this()
        {
            _editingUserId = userId;
            _isEditMode = true;
            TitleText.Text = "Редактирование пользователя";
            PasswordPanel.Visibility = Visibility.Collapsed;
            SaveButton.Content = "Обновить";

            Loaded += async (s, e) => await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                DataTable userData = await _databaseService.GetUserByIdAsync(_editingUserId);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (userData.Rows.Count > 0)
                    {
                        DataRow user = userData.Rows[0];

                        FirstNameTextBox.Text = user["first_name"].ToString();
                        LastNameTextBox.Text = user["last_name"].ToString();
                        MiddleNameTextBox.Text = user["middle_name"]?.ToString() ?? "";
                        EmailTextBox.Text = user["email"].ToString();
                        PhoneTextBox.Text = user["phone_number"]?.ToString() ?? "";

                        if (user["birth_date"] != DBNull.Value)
                        {
                            BirthDatePicker.SelectedDate = Convert.ToDateTime(user["birth_date"]);
                        }

                        int roleId = Convert.ToInt32(user["role_id"]);
                        foreach (ComboBoxItem item in RoleComboBox.Items)
                        {
                            if (item.Tag != null && Convert.ToInt32(item.Tag) == roleId)
                            {
                                RoleComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.GoBack();
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
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

        private void AdminPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new AdminPanel());
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MessageBox.Show("Заполните обязательные поля (Имя, Фамилия, Email)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_isEditMode && PasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!IsValidEmail(EmailTextBox.Text))
                {
                    MessageBox.Show("Введите корректный email", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (RoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите роль пользователя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                string email = EmailTextBox.Text;
                string phone = PhoneTextBox.Text;
                DateTime? birthDate = BirthDatePicker.SelectedDate;
                int roleId = Convert.ToInt32((RoleComboBox.SelectedItem as ComboBoxItem).Tag);

                if (_isEditMode)
                {
                    bool success = await _databaseService.UpdateUserAsync(
                        _editingUserId, firstName, lastName, middleName, email, phone,
                        birthDate ?? DateTime.MinValue, roleId);

                    if (success)
                    {
                        MessageBox.Show("Пользователь успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminUsers());
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении пользователя", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    string password = PasswordBox.Password;

                    if (await _databaseService.CheckEmailExistsAsync(email))
                    {
                        MessageBox.Show("Пользователь с таким email уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int userId = await _databaseService.CreateUserAdminAsync(
                        email, password, roleId, firstName, middleName, lastName, phone,
                        birthDate ?? DateTime.MinValue);

                    if (userId > 0)
                    {
                        MessageBox.Show("Пользователь успешно создан", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminUsers());
                    }
                    else if (userId == -1)
                    {
                        MessageBox.Show("Пользователь с таким email уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании пользователя", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminUsers());
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
    }
}