using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DentalClinic
{
    public partial class AdminUsers : Page
    {
        private readonly DatabaseService _databaseService;
        private List<UserViewModel> _allUsers;

        public AdminUsers()
        {
            InitializeComponent();
            string connectionString = "Host=245e1-rw.db.pub.dbaas.postgrespro.ru;Port=5432;Database=dbdiploma;Username=opyonkov_vv;Password=&08358M4MU#";
            _databaseService = new DatabaseService(connectionString);

            Loaded += async (s, e) => await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                DataTable usersData = await _databaseService.GetAllUsersAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    UsersDataGrid.Items.Clear();
                    _allUsers = new List<UserViewModel>();

                    if (usersData.Rows.Count > 0)
                    {
                        foreach (DataRow row in usersData.Rows)
                        {
                            var user = new UserViewModel
                            {
                                Id = Convert.ToInt32(row["id"]),
                                FirstName = row["first_name"].ToString(),
                                LastName = row["last_name"].ToString(),
                                MiddleName = row["middle_name"]?.ToString() ?? "",
                                Email = row["email"]?.ToString() ?? "",
                                PhoneNumber = row["phone_number"]?.ToString() ?? "Не указан",
                                BirthDate = row["birth_date"] != DBNull.Value ?
                                          Convert.ToDateTime(row["birth_date"]).ToString("dd.MM.yyyy") : "Не указана",
                                RoleName = GetRoleName(Convert.ToInt32(row["role_id"]))
                            };

                            user.FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
                            _allUsers.Add(user);
                            UsersDataGrid.Items.Add(user);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователи не найдены", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Администратор",
                2 => "Пользователь",
                3 => "Врач",
                _ => "Неизвестно"
            };
        }

        private void FilterUsers()
        {
            if (_allUsers == null) return;

            var searchText = SearchTextBox.Text.ToLower();
            var selectedRole = (RoleFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            UsersDataGrid.Items.Clear();

            foreach (var user in _allUsers)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   user.FullName.ToLower().Contains(searchText) ||
                                   user.Email.ToLower().Contains(searchText) ||
                                   user.PhoneNumber.ToLower().Contains(searchText);

                bool matchesRole = selectedRole == "Все роли" ||
                                 user.RoleName == selectedRole;

                if (matchesSearch && matchesRole)
                {
                    UsersDataGrid.Items.Add(user);
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
            await LoadUsersAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void RoleFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterUsers();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminUserEdit());
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                NavigationService?.Navigate(new AdminUserEdit(userId));
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _databaseService.DeleteUserAsync(userId);
                        if (success)
                        {
                            MessageBox.Show("Пользователь успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadUsersAsync();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении пользователя", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}