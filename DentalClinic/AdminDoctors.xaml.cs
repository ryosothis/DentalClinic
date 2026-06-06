using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic
{
    public partial class AdminDoctors : Page
    {
        private readonly DatabaseService _databaseService;
        private List<DoctorViewModel> _allDoctors;

        public AdminDoctors()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadDoctorsAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            try
            {
                DataTable doctorsData = await _databaseService.GetDoctorsAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    DoctorsDataGrid.Items.Clear();
                    _allDoctors = new List<DoctorViewModel>();

                    if (doctorsData.Rows.Count > 0)
                    {
                        foreach (DataRow row in doctorsData.Rows)
                        {
                            var doctor = new DoctorViewModel
                            {
                                Id = Convert.ToInt32(row["id"]),
                                FirstName = row["first_name"].ToString(),
                                LastName = row["last_name"].ToString(),
                                MiddleName = row["middle_name"]?.ToString() ?? "",
                                Specialization = row["specialization"]?.ToString() ?? "Не указана",
                                ExperienceYears = row["experience_years"] != DBNull.Value ?
                                                Convert.ToInt32(row["experience_years"]) : 0,
                                Education = row["education"]?.ToString() ?? "Не указано"
                            };

                            doctor.FullName = $"{doctor.LastName} {doctor.FirstName} {doctor.MiddleName}".Trim();

                            if (doctor.ExperienceYears == 0)
                            {
                                doctor.ExperienceText = "Без опыта";
                            }
                            else if (doctor.ExperienceYears == 1)
                            {
                                doctor.ExperienceText = "1 год";
                            }
                            else if (doctor.ExperienceYears >= 2 && doctor.ExperienceYears <= 4)
                            {
                                doctor.ExperienceText = $"{doctor.ExperienceYears} года";
                            }
                            else
                            {
                                doctor.ExperienceText = $"{doctor.ExperienceYears} лет";
                            }

                            _allDoctors.Add(doctor);
                            DoctorsDataGrid.Items.Add(doctor);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Врачи не найдены", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки врачей: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void FilterDoctors()
        {
            if (_allDoctors == null) return;

            var searchText = SearchTextBox.Text.ToLower();

            DoctorsDataGrid.Items.Clear();

            foreach (var doctor in _allDoctors)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   doctor.FullName.ToLower().Contains(searchText) ||
                                   doctor.Specialization.ToLower().Contains(searchText);

                if (matchesSearch)
                {
                    DoctorsDataGrid.Items.Add(doctor);
                }
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
            await LoadDoctorsAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterDoctors();
        }

        private void AddDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminDoctorEdit());
        }

        private void EditDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int doctorId)
            {
                NavigationService?.Navigate(new AdminDoctorEdit(doctorId));
            }
        }

        private async void DeleteDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int doctorId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого врача?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _databaseService.DeleteDoctorAsync(doctorId);
                        if (success)
                        {
                            MessageBox.Show("Врач успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadDoctorsAsync();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении врача", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении врача: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}