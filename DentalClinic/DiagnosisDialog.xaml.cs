using System.Windows;
using System.Windows.Input;

namespace DentalClinic
{
    public partial class DiagnosisDialog : Window
    {
        public DiagnosisDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => DiagnosisTextBox.Focus();
            Loaded += (s, e) =>
            {
                if (Owner != null)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            };
        }

        public string Diagnosis => DiagnosisTextBox.Text.Trim();
        public string Treatment => TreatmentTextBox.Text.Trim();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Diagnosis))
            {
                MessageBox.Show("Введите диагноз", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DiagnosisTextBox.Focus();
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