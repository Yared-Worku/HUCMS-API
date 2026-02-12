namespace HU_api.Entities.HU
{
    public class Customer
    {
        public Guid? Customer_ID { get; set; }
        public string? Applicant_First_Name_AM { get; set; }
        public string? Applicant_First_Name_EN { get; set; }
        public string? Applicant_Middle_Name_AM { get; set; }
        public string? Applicant_Middle_Name_En { get; set; }
        public string? Applicant_Last_Name_AM { get; set; }
        public string? Applicant_Last_Name_EN { get; set; }
        public string? TIN { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Email { get; set; }
        public string? Mobile_No { get; set; }
        public string? Photo { get; set; }
        public string? SDP_ID { get; set; }
        public string? ID_NO { get; set; }
        public string? depname { get; set; }
        public string? username { get; set; }

        public string? Created_By { get; set; }
        public string? Updated_By { get; set; }
        //public Guid? Deleted_By { get; set; }
        //public bool? Is_Deleted { get; set; }
        public DateTime? Created_Date { get; set; }
        public DateTime? Updated_Date { get; set; }
        //public DateTime? Deleted_Date { get; set; }
        public string? Signiture { get; set; }
 
    }
}
