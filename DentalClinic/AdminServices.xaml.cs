using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic
{
    public partial class AdminServices : Page
    {
        private DatabaseService _databaseService;
        private List<ServiceViewModel> _allServices;

        public AdminServices()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadServicesAsync();
        }

        private async Task LoadServicesAsync()
        {
            try
            {
                DataTable servicesData = await _databaseService.GetServicesAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    ServicesDataGrid.Items.Clear();
                    _allServices = new List<ServiceViewModel>();

                    if (servicesData.Rows.Count > 0)
                    {
                        foreach (DataRow row in servicesData.Rows)
                        {
                            var service = new ServiceViewModel
                            {
                                Id = Convert.ToInt32(row["id"]),
                                Name = row["name"].ToString(),
                                Description = row["description"]?.ToString() ?? "Описание отсутствует",
                                Price = $"{Convert.ToDecimal(row["price"]):N0} руб."
                            };

                            _allServices.Add(service);
                            ServicesDataGrid.Items.Add(service);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void FilterServices()
        {
            if (_allServices == null) return;

            var searchText = SearchTextBox.Text.ToLower();

            ServicesDataGrid.Items.Clear();

            foreach (var service in _allServices)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   service.Name.ToLower().Contains(searchText) ||
                                   service.Description.ToLower().Contains(searchText);

                if (matchesSearch)
                {
                    ServicesDataGrid.Items.Add(service);
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
            await LoadServicesAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterServices();
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminServiceEdit());
        }

        private void EditServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int serviceId)
            {
                NavigationService?.Navigate(new AdminServiceEdit(serviceId));
            }
        }

        private async void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int serviceId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _databaseService.DeleteServiceAsync(serviceId);
                        if (success)
                        {
                            MessageBox.Show("Услуга успешно удалена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadServicesAsync();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении услуги", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
