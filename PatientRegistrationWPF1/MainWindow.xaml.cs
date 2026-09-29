
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace PatientRegistrationWPF1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string connectionString =
                @"Server=HP\SQLEXPRESS;Database=Globis_ICLDC_AD;Trusted_Connection=True;TrustServerCertificate=True;";

            string query = @"
        INSERT INTO PatDtls
        (
            FileNo,
            PName,
            MobNo,
            EmID,
            Create_Date
        )
        VALUES
        (
            @FileNo,
            @PName,
            @MobNo,
            @EmID,
            GETDATE()
        )";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the next FileNo
                    string fileNoQuery = "SELECT ISNULL(MAX(FileNo), 0) + 1 FROM PatDtls";

                    int fileNo;

                    using (SqlCommand fileNoCommand =
                           new SqlCommand(fileNoQuery, connection))
                    {
                        fileNo = Convert.ToInt32(fileNoCommand.ExecuteScalar());
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileNo", fileNo);
                        command.Parameters.AddWithValue("@PName", txtPatientName.Text);
                        command.Parameters.AddWithValue("@MobNo", txtMobile.Text);
                        command.Parameters.AddWithValue("@EmID", txtEmiratesID.Text);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "Patient saved successfully.\nFile No: " + fileNo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtMRN.Clear();
            txtPatientName.Clear();
            txtOtherMRN.Clear();
            txtEmiratesID.Clear();
            txtAge.Clear();
            txtHomePhone.Clear();
            txtOfficePhone.Clear();
            txtMobile.Clear();
            txtEmail.Clear();
            txtRelative.Clear();
            txtRelativePhone.Clear();

            cmbGender.SelectedIndex = -1;
            cmbTC.SelectedIndex = -1;
            cmbNationality.SelectedIndex = -1;
            cmbProfession.SelectedIndex = -1;
            cmbPlace.SelectedIndex = -1;
            cmbVisaStatus.SelectedIndex = -1;
            cmbPatientStatus.SelectedIndex = -1;

            dpDOB.SelectedDate = null;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnAttachDocuments_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DpDOB_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (dpDOB.SelectedDate.HasValue)
            {
                DateTime dob = dpDOB.SelectedDate.Value;

                int age = DateTime.Today.Year - dob.Year;

                if (dob.Date > DateTime.Today.AddYears(-age))
                    age--;

                txtAge.Text = age.ToString();
            }
        }
    }
}

