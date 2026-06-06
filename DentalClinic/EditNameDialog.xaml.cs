using System;
using System.Windows;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class EditNameDialog : Window
    {
        public EditNameDialog(string currentFullName)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(currentFullName))
            {
                var nameParts = currentFullName.Split(' ');
                if (nameParts.Length >= 1) LastNameTextBox.Text = nameParts[0];
                if (nameParts.Length >= 2) FirstNameTextBox.Text = nameParts[1];
                if (nameParts.Length >= 3) MiddleNameTextBox.Text = nameParts[2];
            }

            Loaded += (s, e) =>
            {
                if (Owner != null)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                LastNameTextBox.Focus();
            };
        }

        public (string lastName, string firstName, string middleName) GetNameParts()
        {
            return (LastNameTextBox.Text.Trim(), FirstNameTextBox.Text.Trim(), MiddleNameTextBox.Text.Trim());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) || string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Фамилия и имя обязательны для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DialogResult = false;
            }
        }
    }
}