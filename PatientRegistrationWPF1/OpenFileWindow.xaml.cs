using Microsoft.Data.SqlClient;
using System.Windows;
using System.Data;


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
            string connectionString =
                @"Server=HP\SQLEXPRESS;Database=Globis_ICLDC_AD;Trusted_Connection=True;TrustServerCertificate=True;";

            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Please enter MRN or Patient Name.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT
                    FileNo AS MRN,
                    PName AS PatientName,
                    MobNo AS Mobile,
                    EmID AS EmiratesID,
                    Create_Date AS CreatedDate
                FROM PatDtls
                WHERE
                    CAST(FileNo AS VARCHAR(50)) LIKE @Search
                    OR PName LIKE @Search
                ORDER BY FileNo DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Search",
                            "%" + searchText + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            System.Data.DataTable table =
                                new System.Data.DataTable();

                            table.Load(reader);

                            dgPatients.ItemsSource = table.DefaultView;

                            if (table.Rows.Count == 0)
                            {
                                MessageBox.Show("No patient found.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while searching: " + ex.Message,
                    "Search Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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