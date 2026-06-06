using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;

namespace DentalClinic
{
    public partial class About : Page
    {
        private DatabaseService _databaseService;

        public About()
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
                    DoctorsWrapPanel.Children.Clear();

                    if (doctorsData.Rows.Count > 0)
                    {
                        foreach (DataRow doctor in doctorsData.Rows)
                        {
                            var doctorCard = CreateDoctorCard(doctor);
                            DoctorsWrapPanel.Children.Add(doctorCard);
                        }
                    }
                    else
                    {
                        ShowNoDoctorsMessage();
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

        private Border CreateDoctorCard(DataRow doctor)
        {
            var border = new Border
            {
                Style = (Style)FindResource("DoctorCardStyle")
            };

            var stackPanel = new StackPanel();

            string firstName = doctor["first_name"].ToString();
            string lastName = doctor["last_name"].ToString();
            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();

            var avatarBorder = new Border
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(225, 232, 255)),
                CornerRadius = new CornerRadius(25),
                BorderBrush = new SolidColorBrush(Color.FromRgb(201, 222, 255)),
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var initialsText = new TextBlock
            {
                Text = initials,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            avatarBorder.Child = initialsText;

            string middleName = doctor["middle_name"]?.ToString() ?? "";
            string fullName = $"{lastName} {firstName} {middleName}".Trim();

            var nameText = new TextBlock
            {
                Text = fullName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 68, 132)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            var specializationText = new TextBlock
            {
                Text = doctor["specialization"]?.ToString() ?? "Стоматолог",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            int experienceYears = Convert.ToInt32(doctor["experience_years"]);
            var experienceText = new TextBlock
            {
                Text = $"{experienceYears} лет",
                Foreground = new SolidColorBrush(Color.FromRgb(70, 126, 234)),
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };

            stackPanel.Children.Add(avatarBorder);
            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(specializationText);
            stackPanel.Children.Add(experienceText);

            border.Child = stackPanel;
            return border;
        }

        private void ShowNoDoctorsMessage()
        {
            var noDoctorsText = new TextBlock
            {
                Text = "Информация о врачах временно недоступна",
                Foreground = new SolidColorBrush(Color.FromRgb(130, 140, 160)),
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            DoctorsWrapPanel.Children.Add(noDoctorsText);
        }

        private void HomePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new MainWindow());
        }

        private void AboutPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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