using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic
{
    public partial class Appointment : Page
    {
        private DatabaseService _databaseService;
        private int _selectedServiceId = -1;
        private int _selectedDoctorId = -1;
        private decimal _selectedServicePrice = 0;
        private string _selectedServiceName = "";
        private string _selectedDoctorName = "";

        public Appointment()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            AppointmentDatePicker.DisplayDateStart = DateTime.Today;

            Loaded += async (s, e) => await LoadDataAsync();
        }

        public Appointment(int serviceId) : this()
        {
            _selectedServiceId = serviceId;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                ShowLoadingIndicator(true);

                var servicesTask = _databaseService.GetServicesAsync();
                var doctorsTask = _databaseService.GetDoctorsAsync();

                await Task.WhenAll(servicesTask, doctorsTask);

                await Dispatcher.InvokeAsync(() =>
                {
                    LoadServices(servicesTask.Result);
                    LoadDoctors(doctorsTask.Result);

                    if (_selectedServiceId > 0)
                    {
                        SelectServiceById(_selectedServiceId);
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void LoadServices(DataTable servicesData)
        {
            ServicesStackPanel.Children.Clear();

            if (servicesData.Rows.Count > 0)
            {
                foreach (DataRow service in servicesData.Rows)
                {
                    var serviceCard = CreateServiceCard(service);
                    ServicesStackPanel.Children.Add(serviceCard);
                }
            }
            else
            {
                ServicesStackPanel.Children.Add(new TextBlock
                {
                    Text = "Услуги временно недоступны",
                    Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }
        }

        private void LoadDoctors(DataTable doctorsData)
        {
            DoctorsStackPanel.Children.Clear();

            if (doctorsData.Rows.Count > 0)
            {
                foreach (DataRow doctor in doctorsData.Rows)
                {
                    var doctorCard = CreateDoctorCard(doctor);
                    DoctorsStackPanel.Children.Add(doctorCard);
                }
            }
            else
            {
                DoctorsStackPanel.Children.Add(new TextBlock
                {
                    Text = "Врачи временно недоступны",
                    Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }
        }

        private Border CreateServiceCard(DataRow service)
        {
            int serviceId = Convert.ToInt32(service["id"]);
            string serviceName = service["name"].ToString();
            string description = service["description"]?.ToString() ?? "Подробное описание услуги";
            decimal price = Convert.ToDecimal(service["price"]);

            var border = new Border
            {
                Style = (Style)FindResource("SelectionCardStyle"),
                Tag = serviceId
            };

            var stackPanel = new StackPanel();

            var firstLineGrid = new Grid();
            firstLineGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            firstLineGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = serviceName,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);

            var priceText = new TextBlock
            {
                Text = $"{price:N0} ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(70, 126, 234)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(priceText, 1);

            firstLineGrid.Children.Add(nameText);
            firstLineGrid.Children.Add(priceText);

            var descriptionText = new TextBlock
            {
                Text = GetShortDescription(description),
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            };

            stackPanel.Children.Add(firstLineGrid);
            stackPanel.Children.Add(descriptionText);
            border.Child = stackPanel;

            border.MouseLeftButtonDown += (s, e) => SelectService(serviceId, serviceName, price, border);

            return border;
        }

        private Border CreateDoctorCard(DataRow doctor)
        {
            int doctorId = Convert.ToInt32(doctor["id"]);
            string firstName = doctor["first_name"].ToString();
            string lastName = doctor["last_name"].ToString();
            string middleName = doctor["middle_name"]?.ToString() ?? "";
            string specialization = doctor["specialization"]?.ToString() ?? "Стоматолог";
            int experienceYears = Convert.ToInt32(doctor["experience_years"]);

            string fullName = $"{lastName} {firstName} {middleName}".Trim();
            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();

            var border = new Border
            {
                Style = (Style)FindResource("SelectionCardStyle"),
                Tag = doctorId
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarBorder = new Border
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(225, 232, 255)),
                CornerRadius = new CornerRadius(25),
                BorderBrush = new SolidColorBrush(Color.FromRgb(201, 222, 255)),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 12, 0)
            };

            var initialsText = new TextBlock
            {
                Text = initials,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            avatarBorder.Child = initialsText;
            Grid.SetColumn(avatarBorder, 0);

            var infoPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = fullName,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var specializationText = new TextBlock
            {
                Text = specialization,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 12
            };

            var experienceText = new TextBlock
            {
                Text = $"Опыт работы: {experienceYears} лет",
                Foreground = new SolidColorBrush(Color.FromRgb(70, 126, 234)),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 4, 0, 0)
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(specializationText);
            infoPanel.Children.Add(experienceText);
            Grid.SetColumn(infoPanel, 1);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(infoPanel);
            border.Child = grid;

            border.MouseLeftButtonDown += (s, e) => SelectDoctor(doctorId, fullName, border);

            return border;
        }

        private void SelectService(int serviceId, string serviceName, decimal price, Border selectedBorder)
        {
            foreach (var child in ServicesStackPanel.Children)
            {
                if (child is Border border)
                {
                    border.Style = (Style)FindResource("SelectionCardStyle");
                }
            }

            selectedBorder.Style = (Style)FindResource("SelectedCardStyle");

            _selectedServiceId = serviceId;
            _selectedServiceName = serviceName;
            _selectedServicePrice = price;

            UpdateAppointmentInfo();
        }

        private void SelectServiceById(int serviceId)
        {
            foreach (var child in ServicesStackPanel.Children)
            {
                if (child is Border border && border.Tag is int id && id == serviceId)
                {
                    string serviceName = "";
                    decimal price = 0;

                    if (border.Child is Grid grid && grid.Children.Count >= 2)
                    {
                        if (grid.Children[0] is StackPanel leftPanel && leftPanel.Children.Count > 0)
                        {
                            if (leftPanel.Children[0] is TextBlock nameText)
                                serviceName = nameText.Text;
                        }
                        if (grid.Children[1] is TextBlock priceText)
                        {
                            string priceStr = priceText.Text.Replace(" ₽", "").Replace(" ", "");
                            decimal.TryParse(priceStr, out price);
                        }
                    }

                    SelectService(serviceId, serviceName, price, border);
                    break;
                }
            }
        }

        private void SelectDoctor(int doctorId, string doctorName, Border selectedBorder)
        {
            foreach (var child in DoctorsStackPanel.Children)
            {
                if (child is Border border)
                {
                    border.Style = (Style)FindResource("SelectionCardStyle");
                }
            }

            selectedBorder.Style = (Style)FindResource("SelectedCardStyle");

            _selectedDoctorId = doctorId;
            _selectedDoctorName = doctorName;

            UpdateAppointmentInfo();
        }

        private void UpdateAppointmentInfo()
        {
            SelectedServiceText.Text = _selectedServiceId > 0 ? _selectedServiceName : "Не выбрана";
            SelectedDoctorText.Text = _selectedDoctorId > 0 ? _selectedDoctorName : "Не выбран";
            SelectedPriceText.Text = _selectedServiceId > 0 ? $"{_selectedServicePrice:N0} ₽" : "0 ₽";
        }

        private string GetShortDescription(string fullDescription)
        {
            if (string.IsNullOrEmpty(fullDescription) || fullDescription == "Подробное описание услуги")
                return "Профессиональная стоматологическая услуга";

            if (fullDescription.Length > 60)
                return fullDescription.Substring(0, 60) + "...";

            return fullDescription;
        }

        private void ShowLoadingIndicator(bool show)
        {
            if (show)
            {
                ProgressBar.Visibility = Visibility.Visible;
                MainContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
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
            }
        }

        private async void ConfirmAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedServiceId <= 0)
                {
                    MessageBox.Show("Пожалуйста, выберите услугу", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedDoctorId <= 0)
                {
                    MessageBox.Show("Пожалуйста, выберите врача", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите дату приема", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (TimeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите время приема", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value;
                string selectedTime = ((ComboBoxItem)TimeComboBox.SelectedItem).Content.ToString();

                DateTime appointmentDateTime = selectedDate.Add(TimeSpan.Parse(selectedTime));

                if (appointmentDateTime < DateTime.Now)
                {
                    MessageBox.Show("Нельзя записаться на прошедшую дату", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isTimeAvailable = await _databaseService.IsAppointmentTimeAvailableAsync(
                    _selectedDoctorId, appointmentDateTime);

                if (!isTimeAvailable)
                {
                    MessageBox.Show($"Врач {_selectedDoctorName} уже занят на выбранное время ({appointmentDateTime:HH:mm}).\nПожалуйста, выберите другое время.",
                        "Время занято", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? userId = AuthManager.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    MessageBox.Show("Ошибка авторизации. Пожалуйста, войдите снова.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.Navigate(new Login());
                    return;
                }


                bool success = await _databaseService.CreateAppointmentAsync(
                    userId.Value, _selectedDoctorId, _selectedServiceId, appointmentDateTime);

                if (success)
                {
                    MessageBox.Show($"Запись на прием успешно создана!\n\n" +
                                  $"Услуга: {_selectedServiceName}\n" +
                                  $"Врач: {_selectedDoctorName}\n" +
                                  $"Дата: {appointmentDateTime:dd.MM.yyyy}\n" +
                                  $"Время: {appointmentDateTime:HH:mm}",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService?.Navigate(new MainWindow());
                }
                else
                {
                    MessageBox.Show("Ошибка при создании записи. Пожалуйста, попробуйте позже.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании записи: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}