using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DentalClinic
{
    public partial class AdminDoctorEdit : Page
    {
        private readonly DatabaseService _databaseService;
        private int _editingDoctorId = -1;
        private bool _isEditMode = false;

        public AdminDoctorEdit()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);
        }

        public AdminDoctorEdit(int doctorId) : this()
        {
            _editingDoctorId = doctorId;
            _isEditMode = true;
            TitleText.Text = "Редактирование врача";
            SaveButton.Content = "Обновить";

            Loaded += async (s, e) => await LoadDoctorDataAsync();
        }

        private async Task LoadDoctorDataAsync()
        {
            try
            {
                DataTable doctorData = await _databaseService.GetDoctorByIdAsync(_editingDoctorId);

                await Dispatcher.InvokeAsync(() =>
                {
                    if (doctorData.Rows.Count > 0)
                    {
                        DataRow doctor = doctorData.Rows[0];

                        FirstNameTextBox.Text = doctor["first_name"].ToString();
                        LastNameTextBox.Text = doctor["last_name"].ToString();
                        MiddleNameTextBox.Text = doctor["middle_name"]?.ToString() ?? "";
                        SpecializationTextBox.Text = doctor["specialization"].ToString();
                        ExperienceTextBox.Text = doctor["experience_years"].ToString();
                        EducationTextBox.Text = doctor["education"]?.ToString() ?? "";
                    }
                    else
                    {
                        MessageBox.Show("Врач не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.GoBack();
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных врача: {ex.Message}", "Ошибка",
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
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(SpecializationTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ExperienceTextBox.Text))
                {
                    MessageBox.Show("Заполните обязательные поля", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(ExperienceTextBox.Text, out int experience) || experience < 0)
                {
                    MessageBox.Show("Введите корректный опыт работы", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                string specialization = SpecializationTextBox.Text;
                string education = EducationTextBox.Text;

                if (_isEditMode)
                {
                    bool success = await _databaseService.UpdateDoctorAsync(
                        _editingDoctorId, firstName, lastName, middleName,
                        specialization, experience, education);

                    if (success)
                    {
                        MessageBox.Show("Врач успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminDoctors());
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении врача", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    int doctorId = await _databaseService.CreateDoctorAsync(
                        firstName, lastName, middleName,
                        specialization, experience, education);

                    if (doctorId > 0)
                    {
                        MessageBox.Show("Врач успешно создан", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new AdminDoctors());
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании врача", "Ошибка",
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
            NavigationService?.Navigate(new AdminDoctors());
        }

        private void ExperienceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}