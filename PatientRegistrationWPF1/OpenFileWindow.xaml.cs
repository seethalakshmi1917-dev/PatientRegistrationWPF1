using System.Windows;

namespace PatientRegistrationWPF1
{
    public partial class OpenFileWindow : Window
    {
        public OpenFileWindow()
        {
            InitializeComponent();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}