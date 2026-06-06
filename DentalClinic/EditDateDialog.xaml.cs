using System;
using System.Windows;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class EditDateDialog : Window
    {
        public DateTime SelectedDate { get; private set; }

        public EditDateDialog(string title, DateTime? currentDate = null)
        {
            InitializeComponent();

            TitleText.Text = title;

            if (currentDate.HasValue)
            {
                DatePicker.SelectedDate = currentDate.Value;
            }
            else
            {
                DatePicker.SelectedDate = DateTime.Today;
            }

            DatePicker.DisplayDateStart = new DateTime(1925, 1, 1);
            DatePicker.DisplayDateEnd = DateTime.Today;

            Loaded += (s, e) =>
            {
                if (Owner != null)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedDate = DatePicker.SelectedDate.Value;
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