using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace DentalClinic
{
    public partial class AdminAddAppointment : Page
    {
        private readonly DatabaseService _databaseService;

        public AdminAddAppointment()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadFormDataAsync();
        }

        private async Task LoadFormDataAsync()
        {
            try
            {
                var patientsTask = _databaseService.GetPatientsAsync();
                var doctorsTask = _databaseService.GetDoctorsAsync();
                var servicesTask = _databaseService.GetServicesAsync();

                await Task.WhenAll(patientsTask, doctorsTask, servicesTask);

                await Dispatcher.InvokeAsync(() =>
                {
                    PatientComboBox.Items.Clear();
                    var patientsData = patientsTask.Result;
                    foreach (DataRow row in patientsData.Rows)
                    {
                        var patient = new
                        {
                            Id = Convert.ToInt32(row["id"]),
                            FullName = $"{row["last_name"]} {row["first_name"]} {row["middle_name"]}".Trim()
                        };
                        PatientComboBox.Items.Add(patient);
                    }

                    DoctorComboBox.Items.Clear();
                    var doctorsData = doctorsTask.Result;
                    foreach (DataRow row in doctorsData.Rows)
                    {
                        var doctor = new
                        {
                            Id = Convert.ToInt32(row["id"]),
                            FullName = $"{row["last_name"]} {row["first_name"]} {row["middle_name"]}".Trim()
                        };
                        DoctorComboBox.Items.Add(doctor);
                    }

                    ServiceComboBox.Items.Clear();
                    var servicesData = servicesTask.Result;
                    foreach (DataRow row in servicesData.Rows)
                    {
                        var service = new
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Name = row["name"].ToString()
                        };
                        ServiceComboBox.Items.Add(service);
                    }

                    AppointmentDatePicker.SelectedDate = DateTime.Today;
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

        private async void CreateAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PatientComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пациента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DoctorComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите врача", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ServiceComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите услугу", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TimeTextBox.Text) || !IsValidTime(TimeTextBox.Text))
                {
                    MessageBox.Show("Введите корректное время в формате HH:mm", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int patientId = GetSelectedId(PatientComboBox.SelectedItem);
                int doctorId = GetSelectedId(DoctorComboBox.SelectedItem);
                int serviceId = GetSelectedId(ServiceComboBox.SelectedItem);

                DateTime appointmentDate = AppointmentDatePicker.SelectedDate.Value;
                TimeSpan time = TimeSpan.Parse(TimeTextBox.Text);
                appointmentDate = appointmentDate.Date + time;

                if (appointmentDate < DateTime.Now)
                {
                    MessageBox.Show("Нельзя создавать записи в прошлом", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int appointmentId = await _databaseService.CreateAppointmentAdminAsync(
                    patientId, doctorId, serviceId, appointmentDate);

                if (appointmentId > 0)
                {
                    MessageBox.Show("Запись успешно создана", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.Navigate(new AdminAppointments());
                }
                else
                {
                    MessageBox.Show("Ошибка при создании записи", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetSelectedId(object selectedItem)
        {
            if (selectedItem == null) return -1;

            var type = selectedItem.GetType();
            var idProperty = type.GetProperty("Id");
            if (idProperty != null)
            {
                return (int)idProperty.GetValue(selectedItem);
            }
            return -1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminAppointments());
        }

        private bool IsValidTime(string time)
        {
            return TimeSpan.TryParse(time, out _);
        }
    }
}