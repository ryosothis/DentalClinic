using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace DentalClinic
{
    public partial class MainWindow : Page
    {
        private DatabaseService _databaseService;
        private CancellationTokenSource _searchCancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.Trim();

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(500);

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                {
                    HideSearchResults();
                    return;
                }

                DataTable servicesData = await _databaseService.GetServicesAsync();

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                var filteredServices = servicesData.AsEnumerable()
                    .Where(row => row.Field<string>("name").ToLower().Contains(searchText.ToLower()) ||
                                 (row.Field<string>("description")?.ToLower().Contains(searchText.ToLower()) ?? false))
                    .Take(10)
                    .Select(row => new SearchResultItem
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Name = row["name"].ToString(),
                        Description = row.Field<string>("description") ?? "Описание отсутствует",
                        Price = Convert.ToDecimal(row["price"])
                    })
                    .ToList();

                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                if (filteredServices.Any())
                {
                    ShowSearchResults(filteredServices);
                }
                else
                {
                    HideSearchResults();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                HideSearchResults();
                Debug.WriteLine($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ShowSearchResults(List<SearchResultItem> services)
        {
            SearchResultsListBox.ItemsSource = services;
            SearchResultsBorder.Visibility = Visibility.Visible;
            SearchResultsBorder.MaxHeight = 300;
        }

        private void HideSearchResults()
        {
            SearchResultsBorder.Visibility = Visibility.Collapsed;
            SearchResultsListBox.ItemsSource = null;
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is SearchResultItem selectedService)
            {
                NavigationService?.Navigate(new ServiceInfo(selectedService.Id));
                HideSearchResults();
                SearchTextBox.Text = "";
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!SearchResultsBorder.IsMouseOver && !SearchTextBox.IsMouseOver)
            {
                HideSearchResults();
            }
        }

        private void HomeButton_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void AboutButton_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new About());
        }

        private void ServicesButton_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void PricesButton_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Price());
        }

        private void ProfileButton_Click(object sender, MouseButtonEventArgs e)
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

        private void BracesCard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void DiagnosticsCard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void TreatmentCard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void BracesDetails_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void DiagnosticsDetails_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void TreatmentDetails_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void ProstheticsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void ImplantationButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void WhiteningButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void ChildrenButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Services());
        }

        private void MakeAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                NavigationService?.Navigate(new Appointment());
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в систему для записи на прием");
                NavigationService?.Navigate(new Login());
            }
        }

        private void AddressButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Наш адрес:\nг. Москва, ул. Стоматологическая, д. 123\nТелефон: +7 (495) 123-45-67",
                "Наш адрес", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MapsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string address = "г. Москва, ул. Стоматологическая, д. 123";
                string url = $"https://yandex.ru/maps/?text={Uri.EscapeDataString(address)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть Яндекс Карты: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VkButton_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://vk.com/fearless");
        }

        private void WhatsAppButton_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://wa.me/89539096254");
        }

        private void TelegramButton_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://t.me/flexstylist");
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}