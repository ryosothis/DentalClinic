using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;

namespace DentalClinic
{
    public partial class Profile : Page
    {
        private DatabaseService _databaseService;
        private int _currentUserId;
        private bool _isEditingMode = false;

        public Profile(int userId)
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _currentUserId = userId;

            Loaded += async (s, e) => await LoadDataAsync();
        }

        public Profile() : this(GetCurrentUserIdFromAuth())
        {
        }

        private static int GetCurrentUserIdFromAuth()
        {
            if (AuthManager.CurrentUser != null)
            {
                return AuthManager.CurrentUser.Id;
            }

            var userIdFromApp = AuthManager.GetCurrentUserId();
            if (userIdFromApp.HasValue)
            {
                return userIdFromApp.Value;
            }

            MessageBox.Show("Пожалуйста, войдите в систему для просмотра профиля");
            return -1;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (_currentUserId <= 0)
                {
                    NavigationService?.Navigate(new MainWindow());
                    return;
                }

                ShowLoadingIndicator(true);

                bool userExists = await _databaseService.UserExistsAsync(_currentUserId);

                if (!userExists)
                {
                    ShowErrorMessage("Пользователь не найден. Пожалуйста, войдите снова.");
                    AuthManager.Logout();
                    await Task.Delay(2000);
                    NavigationService?.Navigate(new MainWindow());
                    return;
                }

                await Task.WhenAll(LoadUserDataAsync(), LoadMedicalHistoryAsync());

                ShowRoleSpecificButtons();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void ShowRoleSpecificButtons()
        {
            Dispatcher.Invoke(() =>
            {
                RoleButtonsPanel.Visibility = Visibility.Visible;

                if (AuthManager.IsAdmin())
                {
                    AdminPanelButton.Visibility = Visibility.Visible;
                }

                if (AuthManager.IsDoctor())
                {
                    DoctorAppointmentsButton.Visibility = Visibility.Visible;
                }
            });
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                DataTable userData = await _databaseService.GetUserProfileAsync(_currentUserId);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (userData.Rows.Count > 0)
                    {
                        DataRow user = userData.Rows[0];

                        string firstName = user["first_name"].ToString();
                        string lastName = user["last_name"].ToString();
                        string middleName = user["middle_name"]?.ToString() ?? "";

                        string fullName = $"{lastName} {firstName} {middleName}".Trim();
                        FullNameTextBox.Text = fullName;

                        if (user["birth_date"] != DBNull.Value)
                        {
                            DateTime birthDate = (DateTime)user["birth_date"];
                            BirthDateTextBox.Text = birthDate.ToString("dd.MM.yyyy");
                        }
                        else
                        {
                            BirthDateTextBox.Text = "Не указана";
                        }

                        EmailTextBox.Text = user["email"].ToString();
                        PhoneTextBox.Text = user["phone_number"]?.ToString() ?? "Не указан";

                        string roleInfo = AuthManager.GetRoleName();
                        RoleTextBlock.Text = $"Роль: {roleInfo}";
                    }
                    else
                    {
                        ShowErrorMessage("Данные пользователя не найдены");
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ShowErrorMessage($"Ошибка загрузки профиля: {ex.Message}");
                });
            }
        }

        private async Task LoadMedicalHistoryAsync()
        {
            try
            {
                DataTable medicalHistory = await _databaseService.GetUserMedicalHistoryAsync(_currentUserId);

                await Dispatcher.InvokeAsync(() =>
                {
                    MedicalHistoryStackPanel.Children.Clear();

                    if (medicalHistory.Rows.Count > 0)
                    {
                        foreach (DataRow record in medicalHistory.Rows)
                        {
                            var medicalCard = CreateMedicalCard(record);
                            MedicalHistoryStackPanel.Children.Add(medicalCard);
                        }
                    }
                    else
                    {
                        ShowNoRecordsMessage();
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ShowErrorMessage($"Ошибка загрузки истории: {ex.Message}");
                });
            }
        }

        private async void EditNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new EditNameDialog(FullNameTextBox.Text);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    var (lastName, firstName, middleName) = dialog.GetNameParts();

                    bool success = await _databaseService.UpdateUserNameAsync(_currentUserId, firstName, lastName, middleName);

                    if (success)
                    {
                        ShowSuccessMessage("ФИО успешно обновлено");
                        FullNameTextBox.Text = $"{lastName} {firstName} {middleName}".Trim();

                        if (AuthManager.CurrentUser != null)
                        {
                            AuthManager.CurrentUser.FirstName = firstName;
                            AuthManager.CurrentUser.LastName = lastName;
                            AuthManager.CurrentUser.MiddleName = middleName;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Ошибка при обновлении ФИО");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}");
            }
        }

        private async void EditBirthDateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? currentDate = null;
                if (DateTime.TryParseExact(BirthDateTextBox.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    currentDate = parsedDate;
                }

                var dialog = new EditDateDialog("Дата рождения", currentDate);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    DateTime newDate = dialog.SelectedDate;

                    bool success = await _databaseService.UpdateUserBirthDateAsync(_currentUserId, newDate);

                    if (success)
                    {
                        ShowSuccessMessage("Дата рождения успешно обновлена");
                        BirthDateTextBox.Text = newDate.ToString("dd.MM.yyyy");

                        if (AuthManager.CurrentUser != null)
                        {
                            AuthManager.CurrentUser.BirthDate = newDate;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Ошибка при обновлении даты рождения");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}");
            }
        }

        private async void EditEmailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new EditTextDialog("Email", EmailTextBox.Text, "Введите новый email");
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    string newEmail = dialog.Text.Trim();

                    if (string.IsNullOrWhiteSpace(newEmail))
                    {
                        ShowErrorMessage("Email не может быть пустым");
                        return;
                    }

                    if (!IsValidEmail(newEmail))
                    {
                        ShowErrorMessage("Введите корректный email");
                        return;
                    }

                    int result = await _databaseService.UpdateUserEmailAsync(_currentUserId, newEmail);

                    switch (result)
                    {
                        case 1:
                            ShowSuccessMessage("Email успешно обновлен");
                            EmailTextBox.Text = newEmail;

                            if (AuthManager.CurrentUser != null)
                            {
                                AuthManager.CurrentUser.Email = newEmail;
                            }
                            AuthManager.CurrentUserEmail = newEmail;
                            break;
                        case -1:
                            ShowErrorMessage("Этот email уже используется другим пользователем");
                            break;
                        case 0:
                            ShowErrorMessage("Пользователь не найден");
                            break;
                        default:
                            ShowErrorMessage("Ошибка при обновлении email");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}");
            }
        }

        private async void EditPhoneButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new EditTextDialog("Телефон", PhoneTextBox.Text, "Введите новый номер телефона");
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    string newPhone = dialog.Text.Trim();

                    bool success = await _databaseService.UpdateUserPhoneAsync(_currentUserId, newPhone);

                    if (success)
                    {
                        ShowSuccessMessage("Номер телефона успешно обновлен");
                        PhoneTextBox.Text = newPhone;

                        if (AuthManager.CurrentUser != null)
                        {
                            AuthManager.CurrentUser.PhoneNumber = newPhone;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Ошибка при обновлении номера телефона");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthManager.Logout();
                NavigationService?.Navigate(new MainWindow());
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

        private Border CreateMedicalCard(DataRow record)
        {
            var border = new Border
            {
                Style = (Style)FindResource("MedicalCardStyle")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();

            var recordTypeText = new TextBlock
            {
                Text = "Медицинская запись",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            };

            var diagnosisText = new TextBlock
            {
                Text = record["diagnosis"]?.ToString() ?? "Консультация",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                Margin = new Thickness(0, 0, 0, 4),
                TextWrapping = TextWrapping.Wrap
            };

            var doctorName = record["doctor_name"]?.ToString() ?? "Врач не указан";
            var doctorText = new TextBlock
            {
                Text = $"Врач: {doctorName}",
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var treatment = record["treatment"]?.ToString();
            if (!string.IsNullOrEmpty(treatment))
            {
                var treatmentText = new TextBlock
                {
                    Text = $"Лечение: {treatment}",
                    Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                leftPanel.Children.Add(treatmentText);
            }

            leftPanel.Children.Add(recordTypeText);
            leftPanel.Children.Add(diagnosisText);
            leftPanel.Children.Add(doctorText);

            Grid.SetColumn(leftPanel, 0);

            var rightPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var dateLabelText = new TextBlock
            {
                Text = "Дата приёма",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            };

            DateTime visitDate = (DateTime)record["visit_date"];
            var dateText = new TextBlock
            {
                Text = visitDate.ToString("dd.MM.yyyy HH:mm"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132))
            };

            rightPanel.Children.Add(dateLabelText);
            rightPanel.Children.Add(dateText);

            Grid.SetColumn(rightPanel, 1);

            grid.Children.Add(leftPanel);
            grid.Children.Add(rightPanel);
            border.Child = grid;

            return border;
        }

        private void ShowNoRecordsMessage()
        {
            var noRecordsText = new TextBlock
            {
                Text = "Медицинских записей не найдено",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            MedicalHistoryStackPanel.Children.Add(noRecordsText);
        }

        private void ShowLoadingIndicator(bool show)
        {
            if (show)
            {
                ProgressBar.Visibility = Visibility.Visible;
                MainContent.Visibility = Visibility.Collapsed;
                RoleButtonsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPanel());
        }

        private void DoctorAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DoctorAppointments());
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
            }
        }
    }
}