using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic
{
    public partial class AdminAppointments : Page
    {
        private readonly DatabaseService _databaseService;

        public AdminAppointments()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                DataTable appointmentsData = await _databaseService.GetAllAppointmentsAdminAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    AppointmentsDataGrid.Items.Clear();

                    if (appointmentsData.Rows.Count > 0)
                    {
                        foreach (DataRow row in appointmentsData.Rows)
                        {
                            var appointment = new AppointmentAdminViewModel
                            {
                                Id = Convert.ToInt32(row["id"]),
                                PatientName = $"{row["patient_last_name"]} {row["patient_first_name"]}".Trim(),
                                DoctorName = $"{row["doctor_last_name"]} {row["doctor_first_name"]}".Trim(),
                                ServiceName = row["service_name"]?.ToString() ?? "Не указана",
                                AppointmentDate = Convert.ToDateTime(row["appointment_date"]).ToString("dd.MM.yyyy HH:mm")
                            };
                            AppointmentsDataGrid.Items.Add(appointment);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Записи не найдены", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки записей: {ex.Message}", "Ошибка",
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAppointmentsAsync();
        }

        private async void DeleteAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int appointmentId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _databaseService.DeleteAppointmentAsync(appointmentId);
                        if (success)
                        {
                            MessageBox.Show("Запись успешно удалена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadAppointmentsAsync();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении записи", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}