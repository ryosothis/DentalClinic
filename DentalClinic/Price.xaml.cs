using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class Price : Page
    {
        private DatabaseService _databaseService;

        public Price()
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
                ShowLoadingIndicator(true);

                DataTable servicesData = await _databaseService.GetServicesAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    ServicesGrid.Children.Clear();
                    ServicesGrid.RowDefinitions.Clear();

                    ServicesGrid.ColumnDefinitions.Clear();
                    ServicesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    ServicesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var serviceHeader = new TextBlock
                    {
                        Text = "Услуга",
                        FontWeight = FontWeights.Bold,
                        FontSize = 17,
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    Grid.SetRow(serviceHeader, 0);
                    Grid.SetColumn(serviceHeader, 0);
                    ServicesGrid.Children.Add(serviceHeader);

                    var priceHeader = new TextBlock
                    {
                        Text = "Стоимость",
                        FontWeight = FontWeights.Bold,
                        FontSize = 17,
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    Grid.SetRow(priceHeader, 0);
                    Grid.SetColumn(priceHeader, 1);
                    ServicesGrid.Children.Add(priceHeader);

                    if (servicesData.Rows.Count > 0)
                    {
                        for (int i = 0; i < servicesData.Rows.Count; i++)
                        {
                            ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                            DataRow service = servicesData.Rows[i];
                            string serviceName = service["name"].ToString();
                            decimal price = Convert.ToDecimal(service["price"]);

                            // Название услуги
                            var nameText = new TextBlock
                            {
                                Text = serviceName,
                                FontSize = 15,
                                Margin = new Thickness(0, 0, 0, 8),
                                TextWrapping = TextWrapping.Wrap,
                                VerticalAlignment = VerticalAlignment.Top
                            };
                            Grid.SetRow(nameText, i + 1);
                            Grid.SetColumn(nameText, 0);
                            ServicesGrid.Children.Add(nameText);

                            // Цена
                            var priceText = new TextBlock
                            {
                                Text = $"{price:N0} руб.",
                                FontSize = 15,
                                Margin = new Thickness(20, 0, 0, 8),
                                VerticalAlignment = VerticalAlignment.Top
                            };
                            Grid.SetRow(priceText, i + 1);
                            Grid.SetColumn(priceText, 1);
                            ServicesGrid.Children.Add(priceText);
                        }
                    }
                    else
                    {
                        ShowNoServicesMessage();
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
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void ShowNoServicesMessage()
        {
            ServicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var noServicesText = new TextBlock
            {
                Text = "Информация об услугах временно недоступна",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(noServicesText, 1);
            Grid.SetColumn(noServicesText, 0);
            Grid.SetColumnSpan(noServicesText, 2);
            ServicesGrid.Children.Add(noServicesText);
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

        private void BookAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthManager.IsAuthenticated || AuthManager.GetCurrentUserId().HasValue)
            {
                NavigationService?.Navigate(new Services());
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в систему для записи на прием");
                NavigationService?.Navigate(new Login());
            }
        }
    }
}