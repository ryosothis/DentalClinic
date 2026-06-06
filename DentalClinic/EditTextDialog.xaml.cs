using System;
using System.Windows;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class EditTextDialog : Window
    {
        public string Text => InputTextBox.Text.Trim();

        public EditTextDialog(string title, string currentText, string placeholder = "")
        {
            InitializeComponent();

            TitleText.Text = title;
            InputTextBox.Text = currentText;

            Loaded += (s, e) =>
            {
                if (Owner != null)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Поле не может быть пустым", "Ошибка",
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