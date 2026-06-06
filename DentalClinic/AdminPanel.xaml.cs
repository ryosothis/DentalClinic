using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml;

namespace DentalClinic
{
    public partial class AdminPanel : Page
    {
        private DatabaseService _databaseService;
        private List<AppointmentViewModel> _allAppointments;

        public AdminPanel()
        {
            InitializeComponent();

            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadAdminDataAsync();
        }

        private async Task LoadAdminDataAsync()
        {
            try
            {
                var statsTask = LoadStatisticsAsync();
                var appointmentsTask = LoadAppointmentsAsync();

                await Task.WhenAll(statsTask, appointmentsTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var usersCount = await _databaseService.GetUsersCountAsync();
                var appointmentsToday = await _databaseService.GetAppointmentsTodayCountAsync();
                var servicesCount = await _databaseService.GetServicesCountAsync();
                var doctorsCount = await _databaseService.GetDoctorsCountAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    PatientsCountText.Text = usersCount.ToString();
                    AppointmentsTodayText.Text = appointmentsToday.ToString();
                    ServicesCountText.Text = servicesCount.ToString();
                    DoctorsCountText.Text = doctorsCount.ToString();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                DataTable appointmentsData = await _databaseService.GetAllAppointmentsAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    AppointmentsDataGrid.Items.Clear();
                    _allAppointments = new List<AppointmentViewModel>();

                    if (appointmentsData.Rows.Count > 0)
                    {
                        foreach (DataRow row in appointmentsData.Rows)
                        {
                            var appointment = new AppointmentViewModel
                            {
                                Id = Convert.ToInt32(row["id"]),
                                PatientName = $"{row["patient_last_name"]} {row["patient_first_name"]}",
                                DoctorName = $"{row["doctor_last_name"]} {row["doctor_first_name"]}",
                                ServiceName = row["service_name"].ToString(),
                                AppointmentDate = Convert.ToDateTime(row["appointment_date"]).ToString("dd.MM.yyyy HH:mm")
                            };
                            _allAppointments.Add(appointment);
                            AppointmentsDataGrid.Items.Add(appointment);
                        }
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
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminUsers());
        }

        private void ManageServicesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminServices());
        }

        private void ManageDoctorsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminDoctors());
        }

        private void ManageAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminAppointments());
        }

        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminAddAppointment());
        }

        private async void RefreshAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAppointmentsAsync();
            await LoadStatisticsAsync();
            MessageBox.Show("Данные обновлены", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
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
                            await LoadStatisticsAsync();
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

        private async void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allAppointments == null || _allAppointments.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Записи_стоматология_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await ExportDataToExcelAsync(_allAppointments, saveFileDialog.FileName);
                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportDataToExcelAsync(List<AppointmentViewModel> appointments, string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Записи на прием");

                        worksheet.Cells[1, 1].Value = "Отчет о записях на прием";
                        worksheet.Cells[1, 1].Style.Font.Size = 16;
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1, 1, 5].Merge = true;

                        worksheet.Cells[3, 1].Value = "Дата формирования:";
                        worksheet.Cells[3, 2].Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        worksheet.Cells[3, 1].Style.Font.Bold = true;
                        worksheet.Cells[3, 2].Style.Font.Bold = true;

                        var headerRow = 5;
                        worksheet.Cells[headerRow, 1].Value = "ID";
                        worksheet.Cells[headerRow, 2].Value = "Пациент";
                        worksheet.Cells[headerRow, 3].Value = "Врач";
                        worksheet.Cells[headerRow, 4].Value = "Услуга";
                        worksheet.Cells[headerRow, 5].Value = "Дата и время";

                        var headerStyle = worksheet.Cells[headerRow, 1, headerRow, 5].Style;
                        headerStyle.Font.Bold = true;

                        for (int i = 0; i < appointments.Count; i++)
                        {
                            var row = headerRow + 1 + i;
                            worksheet.Cells[row, 1].Value = appointments[i].Id;
                            worksheet.Cells[row, 2].Value = appointments[i].PatientName;
                            worksheet.Cells[row, 3].Value = appointments[i].DoctorName;
                            worksheet.Cells[row, 4].Value = appointments[i].ServiceName;
                            worksheet.Cells[row, 5].Value = appointments[i].AppointmentDate;
                        }

                        var totalRow = headerRow + 1 + appointments.Count;
                        worksheet.Cells[totalRow, 1].Value = "Итого записей:";
                        worksheet.Cells[totalRow, 2].Value = appointments.Count;
                        worksheet.Cells[totalRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[totalRow, 2].Style.Font.Bold = true;

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        package.SaveAs(new FileInfo(filePath));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка экспорта в Excel: {ex.Message}");
                }
            });
        }
    }
}