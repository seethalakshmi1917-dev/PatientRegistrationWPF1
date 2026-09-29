
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;

namespace PatientRegistrationWPF1
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<InsuranceInfo> insuranceList = new ObservableCollection<InsuranceInfo>();
        public MainWindow()
        {
            InitializeComponent();

            dgInsurance.ItemsSource = insuranceList;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string connectionString =
                @"Server=HP\SQLEXPRESS;Database=Globis_ICLDC_AD;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Get next FileNo
                    string fileNoQuery =
                        "SELECT ISNULL(MAX(FileNo), 0) + 1 FROM PatDtls";

                    int fileNo;

                    using (SqlCommand fileNoCommand =
                           new SqlCommand(fileNoQuery, connection))
                    {
                        fileNo = Convert.ToInt32(fileNoCommand.ExecuteScalar());
                    }

                    // 2. Save Patient
                    string patientQuery = @"
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

                    using (SqlCommand command =
                           new SqlCommand(patientQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileNo", fileNo);
                        command.Parameters.AddWithValue("@PName", txtPatientName.Text);
                        command.Parameters.AddWithValue("@MobNo", txtMobile.Text);
                        command.Parameters.AddWithValue("@EmID", txtEmiratesID.Text);

                        command.ExecuteNonQuery();
                    }

                    // 3. Save Insurance
                    foreach (InsuranceInfo insurance in insuranceList)
                    {
                        int insuranceId = 0;

                        // Find Insurance ID from AccDef
                        string insuranceQuery = @"
                    SELECT IdDef
                    FROM AccDef
                    WHERE AcName = @AcName";

                        using (SqlCommand insuranceCommand =
                               new SqlCommand(insuranceQuery, connection))
                        {
                            insuranceCommand.Parameters.AddWithValue(
                                "@AcName", insurance.Name);

                            object result = insuranceCommand.ExecuteScalar();

                            if (result != null)
                                insuranceId = Convert.ToInt32(result);
                        }

                        // Skip if insurance was not found
                        if (insuranceId == 0)
                            continue;

                        // 4. Get next Invoice Number
                        string invoiceNoQuery =
                            "SELECT ISNULL(MAX(InvNo), 0) + 1 FROM CInvsD";

                        int invoiceNo;

                        using (SqlCommand invoiceCommand =
                               new SqlCommand(invoiceNoQuery, connection))
                        {
                            invoiceNo =
                                Convert.ToInt32(invoiceCommand.ExecuteScalar());
                        }

                        // 5. Get next VisitNo for this patient
                        string visitNoQuery = @"
                    SELECT ISNULL(MAX(VisitNo), 0) + 1
                    FROM Openfile
                    WHERE FileNo = @FileNo";

                        int visitNo;

                        using (SqlCommand visitCommand =
                               new SqlCommand(visitNoQuery, connection))
                        {
                            visitCommand.Parameters.AddWithValue("@FileNo", fileNo);

                            visitNo =
                                Convert.ToInt32(visitCommand.ExecuteScalar());
                        }

                        // 6. Save insurance in Openfile
                        string openFileQuery = @"
                    INSERT INTO Openfile
                    (
                        FileNo,
                        VisitNo,
                        VDate,
                        InsureAc,
                        InvNo
                    )
                    VALUES
                    (
                        @FileNo,
                        @VisitNo,
                        GETDATE(),
                        @InsureAc,
                        @InvNo
                    )";

                        using (SqlCommand openFileCommand =
                               new SqlCommand(openFileQuery, connection))
                        {
                            openFileCommand.Parameters.AddWithValue("@FileNo", fileNo);
                            openFileCommand.Parameters.AddWithValue("@VisitNo", visitNo);
                            openFileCommand.Parameters.AddWithValue("@InsureAc", insuranceId);
                            openFileCommand.Parameters.AddWithValue("@InvNo", invoiceNo);

                            openFileCommand.ExecuteNonQuery();
                        }

                        // 7. Save Insurance details in CInvsD
                        string invoiceQuery = @"
    INSERT INTO CInvsD
    (
        InvNo,
        FileNo,
        VisitNo,
        IdDef,
        MemberNo,
        CardNo,
        InsPackage,
        Date
    )
    VALUES
    (
        @InvNo,
        @FileNo,
        @VisitNo,
        @IdDef,
        @MemberNo,
        @CardNo,
        @InsPackage,
        CAST(GETDATE() AS DATE)
    )";

                        using (SqlCommand invoiceCommand =
                               new SqlCommand(invoiceQuery, connection))
                        {
                            invoiceCommand.Parameters.AddWithValue("@InvNo", invoiceNo);
                            invoiceCommand.Parameters.AddWithValue("@FileNo", fileNo);
                            invoiceCommand.Parameters.AddWithValue("@VisitNo", visitNo);
                            invoiceCommand.Parameters.AddWithValue("@IdDef", insuranceId);
                            invoiceCommand.Parameters.AddWithValue("@MemberNo", insurance.MemberNo ?? "");
                            invoiceCommand.Parameters.AddWithValue("@CardNo", insurance.CardNo ?? "");
                            invoiceCommand.Parameters.AddWithValue("@InsPackage", insurance.PackageId);

                            invoiceCommand.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show(
                        "Patient and insurance saved successfully.\nFile No: " + fileNo,
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            dpDOB.SelectedDate = null;
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

            txtCardNo.Clear();
            dpExpiry.SelectedDate = DateTime.Now;
            insuranceList.Clear();
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
        // ✅ ADD THESE TWO METHODS HERE
        private void BtnAddInsurance_Click(object sender, RoutedEventArgs e)
        {
            string type = "INS";
            if (cmbInsuranceType.SelectedItem is ComboBoxItem selectedType)
                type = selectedType.Content.ToString() == "Insurance" ? "INS" : "COR";

            string name = cmbInsuranceName.SelectedItem is ComboBoxItem selectedName ? selectedName.Content.ToString() ?? "" : "";
            string package = cmbPackages.SelectedItem is ComboBoxItem selectedPkg ? selectedPkg.Content.ToString() ?? "" : "";
            string expiryDate = dpExpiry.SelectedDate?.ToString("dd/MM/yyyy") ?? "";

            var insurance = new InsuranceInfo
            {
                Type = type,
                Name = name,
                Package = package,
                CardNo = txtCardNo.Text,
                ExpiryDate = expiryDate,
                DeductibleAmt = "0.00",
                CoInsurancePercent = "0.00"
            };

            insuranceList.Add(insurance);
            txtCardNo.Clear();
            dpExpiry.SelectedDate = DateTime.Now;
        }

        private void BtnDeleteInsurance_Click(object sender, RoutedEventArgs e)
        {
            if (dgInsurance.SelectedItem is InsuranceInfo selected)
            {
                if (MessageBox.Show("Delete this record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    insuranceList.Remove(selected);
                }
            }
            else
            {
                MessageBox.Show("Please select a row first.", "Info");
            }
        }

        // Your existing method starts here:
       
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

