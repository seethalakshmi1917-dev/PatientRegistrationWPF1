namespace PatientRegistrationWPF1
{
    public class InsuranceInfo
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Package { get; set; } = "";

        public int PackageId { get; set; }
        public string MemberNo { get; set; } = "";

        public string CardNo { get; set; } = "";
        public string ExpiryDate { get; set; } = "";

        public string DeductibleAmt { get; set; } = "";
        public string CoInsurancePercent { get; set; } = "";

        public string CoPayAmount { get; set; } = "";
        public int VisitNo { get; set; }
    }
}