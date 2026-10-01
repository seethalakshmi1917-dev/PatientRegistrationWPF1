
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

            if (string.IsNullOrWhiteSpace(txtPatientName.Text))
            {
                MessageBox.Show("Please enter Patient Name.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int fileNo = 0;
                            bool isExistingPatient = false;

                           
                            if (!string.IsNullOrWhiteSpace(txtMRN.Text))
                            {
                                if (int.TryParse(txtMRN.Text, out int enteredMRN))
                                {
                                    string checkQuery = @"
                                SELECT COUNT(*)
                                FROM PatDtls
                                WHERE FileNo = @FileNo";

                                    using (SqlCommand checkCommand =
                                           new SqlCommand(checkQuery, connection, transaction))
                                    {
                                        checkCommand.Parameters.AddWithValue("@FileNo", enteredMRN);

                                        int count = Convert.ToInt32(
                                            checkCommand.ExecuteScalar());

                                        if (count > 0)
                                        {
                                            fileNo = enteredMRN;
                                            isExistingPatient = true;
                                        }
                                    }
                                }
                            }

                            
                            // 2. NEW PATIENT
                          
                            if (!isExistingPatient)
                            {
                                string fileNoQuery = @"
                            SELECT ISNULL(MAX(FileNo), 0) + 1
                            FROM PatDtls";

                                using (SqlCommand fileNoCommand =
                                       new SqlCommand(fileNoQuery, connection, transaction))
                                {
                                    fileNo = Convert.ToInt32(
                                        fileNoCommand.ExecuteScalar());
                                }

                                string insertPatientQuery = @"
    INSERT INTO PatDtls
    (
        FileNo,
        PName,
        Man_FileNo,
        EmID,
        TelOff,
        MobNo,
        TelHome,
        Email,
        RName,
        RPhone,
        DtBirth,
        HomeAddress,
        MapCordinates,
        Create_Date
    )
    VALUES
    (
        @FileNo,
        @PName,
        @Man_FileNo,
        @EmID,
        @TelOff,
        @MobNo,
        @TelHome,
        @Email,
        @RName,
        @RPhone,
        @DtBirth,
        @HomeAddress,
        @MapCordinates,
        GETDATE()
    )";

                                using (SqlCommand command =
                                       new SqlCommand(insertPatientQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@FileNo", fileNo);
                                    command.Parameters.AddWithValue("@PName", txtPatientName.Text);
                                    command.Parameters.AddWithValue("@Man_FileNo", txtOtherMRN.Text);
                                    command.Parameters.AddWithValue("@EmID", txtEmiratesID.Text);

                                    command.Parameters.AddWithValue("@TelOff", txtOfficePhone.Text);
                                    command.Parameters.AddWithValue("@MobNo", txtMobile.Text);
                                    command.Parameters.AddWithValue("@TelHome", txtHomePhone.Text);
                                    command.Parameters.AddWithValue("@Email", txtEmail.Text);

                                    command.Parameters.AddWithValue("@RName", txtRelative.Text);
                                    command.Parameters.AddWithValue("@RPhone", txtRelativePhone.Text);

                                    command.Parameters.AddWithValue(
                                        "@DtBirth",
                                        dpDOB.SelectedDate.HasValue
                                            ? dpDOB.SelectedDate.Value
                                            : DBNull.Value);

                                    command.Parameters.AddWithValue(
                                        "@HomeAddress",
                                        txtHomeCareAddress.Text);

                                    command.Parameters.AddWithValue(
                                        "@MapCordinates",
                                        txtGoogleMapCoordinates.Text);
                                    command.ExecuteNonQuery();
                                }

                                
                                txtMRN.Text = fileNo.ToString();
                            }
                            else
                            {

                                // 3. EXISTING PATIENT - UPDATE


                                string updatePatientQuery = @"
    UPDATE PatDtls
    SET
        PName = @PName,
        Man_FileNo = @Man_FileNo,
        EmID = @EmID,
        TelOff = @TelOff,
        MobNo = @MobNo,
        TelHome = @TelHome,
        Email = @Email,
        RName = @RName,
        RPhone = @RPhone,
        DtBirth = @DtBirth,
        HomeAddress = @HomeAddress,
        MapCordinates = @MapCordinates
    WHERE FileNo = @FileNo";

                                using (SqlCommand updateCommand =
                                       new SqlCommand(updatePatientQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@FileNo", fileNo);
                                    updateCommand.Parameters.AddWithValue("@PName", txtPatientName.Text);
                                    updateCommand.Parameters.AddWithValue("@Man_FileNo", txtOtherMRN.Text);
                                    updateCommand.Parameters.AddWithValue("@EmID", txtEmiratesID.Text);

                                    updateCommand.Parameters.AddWithValue("@TelOff", txtOfficePhone.Text);
                                    updateCommand.Parameters.AddWithValue("@MobNo", txtMobile.Text);
                                    updateCommand.Parameters.AddWithValue("@TelHome", txtHomePhone.Text);
                                    updateCommand.Parameters.AddWithValue("@Email", txtEmail.Text);

                                    updateCommand.Parameters.AddWithValue("@RName", txtRelative.Text);
                                    updateCommand.Parameters.AddWithValue("@RPhone", txtRelativePhone.Text);

                                    updateCommand.Parameters.AddWithValue(
                                        "@DtBirth",
                                        dpDOB.SelectedDate.HasValue
                                            ? dpDOB.SelectedDate.Value
                                            : DBNull.Value);

                                    updateCommand.Parameters.AddWithValue(
                                        "@HomeAddress",
                                        txtHomeCareAddress.Text);

                                    updateCommand.Parameters.AddWithValue(
                                        "@MapCordinates",
                                        txtGoogleMapCoordinates.Text);


                                    updateCommand.ExecuteNonQuery();
                                }

                       
                                // 4. Remove old insurance records
                                
                                string deleteInvoiceQuery = @"
                            DELETE FROM CInvsD
                            WHERE FileNo = @FileNo";

                                using (SqlCommand deleteInvoiceCommand =
                                       new SqlCommand(deleteInvoiceQuery, connection, transaction))
                                {
                                    deleteInvoiceCommand.Parameters.AddWithValue("@FileNo", fileNo);
                                    deleteInvoiceCommand.ExecuteNonQuery();
                                }

                                string deleteOpenFileQuery = @"
                            DELETE FROM Openfile
                            WHERE FileNo = @FileNo";

                                using (SqlCommand deleteOpenFileCommand =
                                       new SqlCommand(deleteOpenFileQuery, connection, transaction))
                                {
                                    deleteOpenFileCommand.Parameters.AddWithValue("@FileNo", fileNo);
                                    deleteOpenFileCommand.ExecuteNonQuery();
                                }
                            }

                            
                            // 5. Save current insurance information
                          

                            foreach (InsuranceInfo insurance in insuranceList)
                            {
                                int insuranceId = 0;

                                string insuranceQuery = @"
                            SELECT IdDef
                            FROM AccDef
                            WHERE AcName = @AcName";

                                using (SqlCommand insuranceCommand =
                                       new SqlCommand(insuranceQuery, connection, transaction))
                                {
                                    insuranceCommand.Parameters.AddWithValue(
                                        "@AcName", insurance.Name);

                                    object result = insuranceCommand.ExecuteScalar();

                                    if (result != null)
                                        insuranceId = Convert.ToInt32(result);
                                }

                                if (insuranceId == 0)
                                    continue;

                              
                                // 6. Get next Invoice Number
                               

                                string invoiceNoQuery = @"
                            SELECT ISNULL(MAX(InvNo), 0) + 1
                            FROM CInvsD";

                                int invoiceNo;

                                using (SqlCommand invoiceCommand =
                                       new SqlCommand(invoiceNoQuery, connection, transaction))
                                {
                                    invoiceNo = Convert.ToInt32(
                                        invoiceCommand.ExecuteScalar());
                                }

                                // 7. Get Visit Number
                              

                                string visitNoQuery = @"
                            SELECT ISNULL(MAX(VisitNo), 0) + 1
                            FROM Openfile
                            WHERE FileNo = @FileNo";

                                int visitNo;

                                using (SqlCommand visitCommand =
                                       new SqlCommand(visitNoQuery, connection, transaction))
                                {
                                    visitCommand.Parameters.AddWithValue("@FileNo", fileNo);

                                    visitNo = Convert.ToInt32(
                                        visitCommand.ExecuteScalar());
                                }

                               
                                // 8. Insert Openfile
                                

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
                                       new SqlCommand(openFileQuery, connection, transaction))
                                {
                                    openFileCommand.Parameters.AddWithValue("@FileNo", fileNo);
                                    openFileCommand.Parameters.AddWithValue("@VisitNo", visitNo);
                                    openFileCommand.Parameters.AddWithValue("@InsureAc", insuranceId);
                                    openFileCommand.Parameters.AddWithValue("@InvNo", invoiceNo);

                                    openFileCommand.ExecuteNonQuery();
                                }

                                // 9. Insert CInvsD
                              
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
                                       new SqlCommand(invoiceQuery, connection, transaction))
                                {
                                    invoiceCommand.Parameters.AddWithValue("@InvNo", invoiceNo);
                                    invoiceCommand.Parameters.AddWithValue("@FileNo", fileNo);
                                    invoiceCommand.Parameters.AddWithValue("@VisitNo", visitNo);
                                    invoiceCommand.Parameters.AddWithValue("@IdDef", insuranceId);
                                    invoiceCommand.Parameters.AddWithValue(
                                        "@MemberNo",
                                        insurance.MemberNo ?? "");
                                    invoiceCommand.Parameters.AddWithValue(
                                        "@CardNo",
                                        insurance.CardNo ?? "");
                                    invoiceCommand.Parameters.AddWithValue(
                                        "@InsPackage",
                                        insurance.PackageId);

                                    invoiceCommand.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            if (isExistingPatient)
                            {
                                MessageBox.Show(
                                    "Patient details updated successfully.\nMRN: " + fileNo,
                                    "Updated",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Patient and insurance saved successfully.\nMRN: " + fileNo,
                                    "Saved",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
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
            OpenFileWindow openFilePopup = new OpenFileWindow();
            openFilePopup.Owner = this;
            openFilePopup.ShowDialog();
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

        private void BtnAddInsurance_Click(object sender, RoutedEventArgs e)
        {
            string type = "INS";

            if (cmbInsuranceType.SelectedItem is ComboBoxItem selectedType)
                type = selectedType.Content.ToString() == "Insurance" ? "INS" : "COR";

            string name = cmbInsuranceName.SelectedItem is ComboBoxItem selectedName
                ? selectedName.Content.ToString() ?? ""
                : "";

            string package = cmbPackages.SelectedItem is ComboBoxItem selectedPkg
                ? selectedPkg.Content.ToString() ?? ""
                : "";

            string payer = cmbPayer.SelectedItem is ComboBoxItem selectedPayer
                ? selectedPayer.Content.ToString() ?? ""
                : "";

            string expiryDate = dpExpiry.SelectedDate?.ToString("dd/MM/yyyy") ?? "";

            var insurance = new InsuranceInfo
            {
                Type = type,
                Name = name,
                Package = package,
                Payer = payer,
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

