using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace PatientRegistrationWPF1
{
    public partial class OpenFileWindow : Window
    {
        private string connectionString =
            @"Server=HP\SQLEXPRESS;Database=Globis_ICLDC_AD;Trusted_Connection=True;TrustServerCertificate=True;";

        public OpenFileWindow(int fileNo, string patientName)
        {
            InitializeComponent();

            txtFileNo.Text = fileNo.ToString();
            txtPatientName.Text = patientName;

            txtTime.Text = DateTime.Now.ToString("HH:mm");
            dpStartDate.SelectedDate = DateTime.Today;
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Check File No.
            if (string.IsNullOrWhiteSpace(txtFileNo.Text))
            {
                MessageBox.Show(
                    "Please enter File No.",
                    "Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Check File No. is a number
            if (!int.TryParse(txtFileNo.Text, out int fileNo))
            {
                MessageBox.Show(
                    "Please enter a valid File No.",
                    "Invalid File No.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Visit No.
            int visitNo = 1;

            if (!string.IsNullOrWhiteSpace(txtVisitNo.Text))
            {
                if (!int.TryParse(txtVisitNo.Text, out visitNo))
                {
                    MessageBox.Show(
                        "Please enter a valid Visit No.",
                        "Invalid Visit No.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
            }

            // Start date
            DateTime visitDate = dpStartDate.SelectedDate ?? DateTime.Now;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check whether this File No. already exists
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Openfile
                        WHERE FileNo = @FileNo
                        AND VisitNo = @VisitNo";

                    using (SqlCommand checkCommand =
                           new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@FileNo", fileNo);

                        checkCommand.Parameters.AddWithValue(
                            "@VisitNo", visitNo);

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "This patient visit already exists.\n\n" +
                                "File No.: " + fileNo +
                                "\nVisit No.: " + visitNo,
                                "Already Exists",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            return;
                        }
                    }

                    // Insert into Openfile
                    string insertQuery = @"
                        INSERT INTO Openfile
                        (
                            FileNo,
                            VisitNo,
                            VDate
                        )
                        VALUES
                        (
                            @FileNo,
                            @VisitNo,
                            @VDate
                        )";

                    using (SqlCommand command =
                           new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@FileNo", fileNo);

                        command.Parameters.AddWithValue(
                            "@VisitNo", visitNo);

                        command.Parameters.AddWithValue(
                            "@VDate", visitDate);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Open File saved successfully.\n\n" +
                    "File No.: " + fileNo +
                    "\nVisit No.: " + visitNo,
                    "Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while saving Open File:\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}