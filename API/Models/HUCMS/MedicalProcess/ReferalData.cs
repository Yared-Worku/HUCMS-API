namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class ReferalData
    {
        public Guid? UserId { get; set; }
        public Guid? refCode { get; set; }
        public Guid? diagnosisCode { get; set; }
        public Guid? processDetailCode { get; set; }
        public string? application_number { get; set; }
        public Guid? todocode { get; set; }
        public string? vitalSign { get; set; }
        public string? physicalExamination { get; set; }
        public string? referalReason { get; set; }
    }
}
