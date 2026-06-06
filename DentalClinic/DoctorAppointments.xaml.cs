using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class DoctorAppointments : Page
    {
        private DatabaseService _databaseService;
        private int _doctorUserId;
        private int _currentDoctorId;

        public DoctorAppointments()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _doctorUserId = AuthManager.CurrentUserId ?? -1;

            Loaded += async (s, e) => await LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                if (!AuthManager.IsDoctor())
                {
                    ShowErrorMessage("Доступ только для врачей");
                    NavigationService?.GoBack();
                    return;
                }

                ShowLoadingIndicator(true);

                _currentDoctorId = await _databaseService.GetDoctorIdByUserIdAsync(_doctorUserId);

                if (_currentDoctorId <= 0)
                {
                    ShowErrorMessage("Профиль врача не найден");
                    return;
                }

                DataTable appointments = await _databaseService.GetDoctorAppointmentsForTodayAsync(_doctorUserId);

                await Dispatcher.InvokeAsync(() =>
                {
                    DateText.Text = $"На {DateTime.Today:dd.MM.yyyy}";
                    AppointmentsStackPanel.Children.Clear();

                    if (appointments.Rows.Count > 0)
                    {
                        NoAppointmentsText.Visibility = Visibility.Collapsed;

                        foreach (DataRow appointment in appointments.Rows)
                        {
                            var appointmentCard = CreateAppointmentCard(appointment);
                            AppointmentsStackPanel.Children.Add(appointmentCard);
                        }
                    }
                    else
                    {
                        NoAppointmentsText.Visibility = Visibility.Visible;
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки записей: {ex.Message}");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private Border CreateAppointmentCard(DataRow appointment)
        {
            var border = new Border
            {
                Style = (Style)FindResource("AppointmentCardStyle")
            };

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();

            var timeText = new TextBlock
            {
                Text = $"⏰ {((DateTime)appointment["appointment_date"]):HH:mm}",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var patientName = $"{appointment["last_name"]} {appointment["first_name"]} {appointment["middle_name"]}".Trim();
            var patientText = new TextBlock
            {
                Text = $"👤 {patientName}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var serviceText = new TextBlock
            {
                Text = $"🦷 {appointment["service_name"]}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var phone = appointment["phone_number"]?.ToString() ?? "Не указан";
            var contactText = new TextBlock
            {
                Text = $"📞 {phone}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                Margin = new Thickness(0, 0, 0, 10)
            };

            leftPanel.Children.Add(timeText);
            leftPanel.Children.Add(patientText);
            leftPanel.Children.Add(serviceText);
            leftPanel.Children.Add(contactText);

            Grid.SetColumn(leftPanel, 0);

            var rightPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var diagnosisButton = new Button
            {
                Content = "Поставить диагноз",
                Style = (Style)FindResource("DiagnosisButtonStyle"),
                ToolTip = "Добавить диагноз и лечение в медицинскую карту"
            };

            if (appointment["user_id"] != null && appointment["user_id"] != DBNull.Value)
            {
                diagnosisButton.Tag = Convert.ToInt32(appointment["user_id"]);
                diagnosisButton.Click += DiagnosisButton_Click;
            }
            else
            {
                diagnosisButton.IsEnabled = false;
                diagnosisButton.ToolTip = "Ошибка: ID пациента не найден";
                diagnosisButton.Content = "Ошибка данных";
            }

            rightPanel.Children.Add(diagnosisButton);

            Grid.SetColumn(rightPanel, 1);

            mainGrid.Children.Add(leftPanel);
            mainGrid.Children.Add(rightPanel);
            border.Child = mainGrid;

            return border;
        }

        private async void DiagnosisButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
                int patientId = (int)button.Tag;
                DateTime visitDate = DateTime.Now;

                var dialog = new DiagnosisDialog();
                if (dialog.ShowDialog() == true)
                {
                    string diagnosis = dialog.Diagnosis;
                    string treatment = dialog.Treatment;

                    int recordId = await _databaseService.CreateMedicalRecordAsync(
                        patientId, _currentDoctorId, visitDate, diagnosis, treatment);

                    if (recordId > 0)
                    {
                        MessageBox.Show("Диагноз успешно сохранен в медицинскую карту", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при сохранении диагноза", "Ошибка",
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

        private void ShowLoadingIndicator(bool show)
        {
            ProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Обработчики навигации
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
    }
}