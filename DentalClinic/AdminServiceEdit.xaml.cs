using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DentalClinic
{
    public partial class AdminServiceEdit : Page
    {
        private DatabaseService _databaseService;
        private int _editingServiceId = -1;
        private bool _isEditMode = false;

        public AdminServiceEdit()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        public AdminServiceEdit(int serviceId) : this()
        {
            _editingServiceId = serviceId;
            _isEditMode = true;
            TitleText.Text = "Редактирование услуги";
            SaveButton.Content = "Обновить";

            Loaded += async (s, e) => await LoadServiceDataAsync();
        }

        private async Task LoadServiceDataAsync()
        {
            try
            {
                DataTable serviceData = await _databaseService.GetServiceByIdAsync(_editingServiceId);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (serviceData.Rows.Count > 0)
                    {
                        DataRow service = serviceData.Rows[0];

                        NameTextBox.Text = service["name"].ToString();
                        DescriptionTextBox.Text = service["description"]?.ToString() ?? "";
                        PriceTextBox.Text = Convert.ToDecimal(service["price"]).ToString("F0");
                    }
                    else
                    {
                        MessageBox.Show("Услуга не найдена", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.GoBack();
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных услуги: {ex.Message}", "Ошибка",
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Введите название услуги", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(PriceTextBox.Text) || !decimal.TryParse(PriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (price <= 0)
                {
                    MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string name = NameTextBox.Text;
                string description = DescriptionTextBox.Text;

                if (_isEditMode)
                {
                    bool success = await _databaseService.UpdateServiceAsync(
                        _editingServiceId, name, description, price);

                    if (success)
                    {
                        MessageBox.Show("Услуга успешно обновлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminServices());
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении услуги", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    int serviceId = await _databaseService.CreateServiceAsync(name, description, price);

                    if (serviceId > 0)
                    {
                        MessageBox.Show("Услуга успешно создана", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminServices());
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании услуги", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminServices());
        }

        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}