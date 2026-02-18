using System;

namespace HUCMS.Models.HUCMS.MedicalCertificate
{
    public class MedicalCertificateReport
    {
        // Application Identity
        public string? Application_No { get; set; }
        public string? Service_Name { get; set; }
        public string? Cust_Phone_No { get; set; }

        // Applicant Personal Details
        public string? Applicant_First_Name_EN { get; set; }
        public string? Applicant_Middle_Name_En { get; set; }
        public string? Applicant_Last_Name_EN { get; set; }
        public string? ID_NO { get; set; }
        public int? Age { get; set; }
        public string? Depname { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Cust_TIN_No { get; set; }

        // Medical Findings
        public string? RX { get; set; }
        public string? Patient_Condition { get; set; }
        public string? Health_Profetional_Recomendation { get; set; }
        public string? Detail_Diagnosis { get; set; }

        // Date & Personnel
        public string? Date_Of_Issuance { get; set; }
        public string? Fname_Doctor { get; set; }
        public string? Lname_Doctor { get; set; }
        public string? Attended_Date { get; set; }

        // Base64 Image String
        public string? Cust_Photo { get; set; }
    }
}