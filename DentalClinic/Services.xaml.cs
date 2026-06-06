using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class Services : Page
    {
        private DatabaseService _databaseService;

        public Services()
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
                    ServicesWrapPanel.Children.Clear();

                    if (servicesData.Rows.Count > 0)
                    {
                        foreach (DataRow service in servicesData.Rows)
                        {
                            var serviceCard = CreateServiceCard(service);
                            ServicesWrapPanel.Children.Add(serviceCard);
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

        private Border CreateServiceCard(DataRow service)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ServiceCardStyle")
            };

            int serviceId = Convert.ToInt32(service["id"]);
            string serviceName = service["name"].ToString();
            string description = service["description"]?.ToString() ?? "Подробное описание услуги";
            decimal price = Convert.ToDecimal(service["price"]);

            string icon = GetServiceIcon(serviceName);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var topPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var nameText = new TextBlock
            {
                Text = serviceName,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            topPanel.Children.Add(iconText);
            topPanel.Children.Add(nameText);
            Grid.SetRow(topPanel, 0);

            var descriptionText = new TextBlock
            {
                Text = GetShortDescription(description, serviceName),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(descriptionText, 1);

            var bottomPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(225, 232, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 6, 12, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var priceText = new TextBlock
            {
                Text = $"{price:N0} руб.",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132))
            };

            bottomPanel.Child = priceText;
            Grid.SetRow(bottomPanel, 2);

            mainGrid.Children.Add(topPanel);
            mainGrid.Children.Add(descriptionText);
            mainGrid.Children.Add(bottomPanel);

            border.Child = mainGrid;

            border.MouseLeftButtonDown += (s, e) => ServiceCard_Click(serviceId, serviceName);

            return border;
        }

        private void ServiceCard_Click(int serviceId, string serviceName)
        {
            NavigationService?.Navigate(new Appointment(serviceId));
        }

        private string GetServiceIcon(string serviceName)
        {
            string lowerName = serviceName.ToLower();

            if (lowerName.Contains("консультация") || lowerName.Contains("диагностика") || lowerName.Contains("осмотр"))
                return "🔍";
            else if (lowerName.Contains("лечение") || lowerName.Contains("кариес") || lowerName.Contains("пульпит") || lowerName.Contains("терапия"))
                return "💊";
            else if (lowerName.Contains("гигиена") || lowerName.Contains("чистка") || lowerName.Contains("профилактика"))
                return "✨";
            else if (lowerName.Contains("отбеливание") || lowerName.Contains("эстетик"))
                return "⭐";
            else if (lowerName.Contains("протезирование") || lowerName.Contains("коронк") || lowerName.Contains("мост"))
                return "🦷";
            else if (lowerName.Contains("имплантация") || lowerName.Contains("имплант"))
                return "⚡";
            else if (lowerName.Contains("прикус") || lowerName.Contains("брекет") || lowerName.Contains("ортодонт") || lowerName.Contains("элайнер"))
                return "🦴";
            else if (lowerName.Contains("удаление") || lowerName.Contains("хирург"))
                return "❌";
            else if (lowerName.Contains("детск") || lowerName.Contains("ребенок"))
                return "👶";
            else
                return "🎯";
        }

        private string GetShortDescription(string fullDescription, string serviceName)
        {

            if (!string.IsNullOrEmpty(fullDescription) && fullDescription != "Подробное описание услуги")
            {
                if (fullDescription.Length > 60)
                    return fullDescription.Substring(0, 60) + "...";
                else
                    return fullDescription;
            }

            string lowerName = serviceName.ToLower();

            if (lowerName.Contains("консультация"))
                return "Профессиональный осмотр и консультация стоматолога с составлением плана лечения";
            else if (lowerName.Contains("диагностика"))
                return "Точная диагностика с использованием современного оборудования и составление плана лечения";
            else if (lowerName.Contains("лечение кариеса"))
                return "Безболезненное лечение кариеса с использованием современных пломбировочных материалов";
            else if (lowerName.Contains("пульпит"))
                return "Лечение корневых каналов с использованием микроскопа и современных методик";
            else if (lowerName.Contains("гигиена"))
                return "Профессиональная чистка зубов с удалением налета и зубного камня";
            else if (lowerName.Contains("отбеливание"))
                return "Безопасное отбеливание эмали с гарантированным результатом и минимальной чувствительностью";
            else if (lowerName.Contains("протезирование"))
                return "Восстановление утраченных зубов с использованием современных материалов и технологий";
            else if (lowerName.Contains("имплантация"))
                return "Современные методы имплантации с пожизненной гарантией и быстрым восстановлением";
            else if (lowerName.Contains("прикус") || lowerName.Contains("ортодонт"))
                return "Исправление положения зубов и прикуса с использованием брекет-систем и элайнеров";
            else if (lowerName.Contains("удаление"))
                return "Безболезненное удаление зубов любой сложности с современной анестезией";
            else
                return "Профессиональная стоматологическая услуга с использованием современных технологий и материалов";
        }

        private void ShowNoServicesMessage()
        {
            var noServicesText = new TextBlock
            {
                Text = "Услуги временно недоступны",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            ServicesWrapPanel.Children.Add(noServicesText);
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

        private void AppointmentButton_Click(object sender, RoutedEventArgs e)
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
    }
}