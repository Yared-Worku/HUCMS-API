namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class LabResult
    {
        public Guid? UserId { get; set; }
        public string? application_number { get; set; }
        public string? lab_result { get; set; }
        public Guid? todocode { get; set; }
    }
}
