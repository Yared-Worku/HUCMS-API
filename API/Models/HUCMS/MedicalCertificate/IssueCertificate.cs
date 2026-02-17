namespace HUCMS.Models.HUCMS.MedicalCertificate
{
    public class IssueCertificate
    {
        public string? application_number { get; set; }
        public Guid? UserId { get; set; }
        public string? health_profetional_recomendation { get; set; }
        public string? patient_condition { get; set; }
        public Guid? processDetailCode { get; set; }
        public Guid? todocode { get; set; }
    }
}
