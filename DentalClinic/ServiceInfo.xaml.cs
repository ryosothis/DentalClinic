using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class ServiceInfo : Page
    {
        private DatabaseService _databaseService;
        private int _serviceId;

        public ServiceInfo(int serviceId)
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
            _serviceId = serviceId;

            Loaded += async (s, e) => await LoadServiceInfoAsync();
        }

        public ServiceInfo() : this(1)
        {
        }

        private async Task LoadServiceInfoAsync()
        {
            try
            {
                ShowLoadingIndicator(true);

                DataTable serviceData = await _databaseService.GetServiceByIdAsync(_serviceId);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (serviceData.Rows.Count > 0)
                    {
                        DataRow service = serviceData.Rows[0];

                        string serviceName = service["name"].ToString();
                        string description = service["description"]?.ToString() ?? "Описание временно недоступно";
                        decimal price = Convert.ToDecimal(service["price"]);

                        ServiceNameText.Text = serviceName;
                        ServiceDescriptionText.Text = description;
                        ServicePriceText.Text = $"{price:N0} руб.";

                        AddAdvantages(serviceName);
                    }
                    else
                    {
                        ShowErrorMessage("Услуга не найдена");
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ShowErrorMessage($"Ошибка загрузки информации об услуге: {ex.Message}");
                });
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void AddAdvantages(string serviceName)
        {
            AdvantagesStackPanel.Children.Clear();

            // Генерируем преимущества в зависимости от типа услуги
            var advantages = GetAdvantagesForService(serviceName);

            foreach (string advantage in advantages)
            {
                var advantagePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 12),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var iconBorder = new Border
                {
                    Width = 24,
                    Height = 24,
                    Background = new SolidColorBrush(Color.FromRgb(225, 232, 255)),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0, 0, 12, 0)
                };

                var iconText = new TextBlock
                {
                    Text = "✓",
                    Foreground = new SolidColorBrush(Color.FromRgb(70, 126, 234)),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                iconBorder.Child = iconText;

                var advantageText = new TextBlock
                {
                    Text = advantage,
                    Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                advantagePanel.Children.Add(iconBorder);
                advantagePanel.Children.Add(advantageText);

                AdvantagesStackPanel.Children.Add(advantagePanel);
            }
        }

        private string[] GetAdvantagesForService(string serviceName)
        {
            // Возвращаем преимущества в зависимости от типа услуги
            if (serviceName.ToLower().Contains("консультация"))
            {
                return new string[]
                {
                    "Профессиональная диагностика",
                    "Индивидуальный план лечения",
                    "Ответы на все вопросы",
                    "Рекомендации по уходу"
                };
            }
            else if (serviceName.ToLower().Contains("лечение") || serviceName.ToLower().Contains("кариес") || serviceName.ToLower().Contains("пульпит"))
            {
                return new string[]
                {
                    "Безболезненное лечение",
                    "Современные материалы",
                    "Пожизненная гарантия на работу",
                    "Сохраняем здоровые ткани"
                };
            }
            else if (serviceName.ToLower().Contains("гигиена") || serviceName.ToLower().Contains("чистка"))
            {
                return new string[]
                {
                    "Удаление зубного камня",
                    "Отбеливание на 1-2 тона",
                    "Фторирование эмали",
                    "Профилактика кариеса"
                };
            }
            else if (serviceName.ToLower().Contains("удаление"))
            {
                return new string[]
                {
                    "Безболезненная процедура",
                    "Быстрое восстановление",
                    "Минимальная травматичность",
                    "Профессиональный уход после операции"
                };
            }
            else
            {
                return new string[]
                {
                    "Высокое качество материалов",
                    "Опытные специалисты",
                    "Современное оборудование",
                    "Индивидуальный подход"
                };
            }
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
                NavigationService?.Navigate(new Appointment(_serviceId));
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в систему для записи на прием");
                NavigationService?.Navigate(new Login());
            }
        }
    }
}