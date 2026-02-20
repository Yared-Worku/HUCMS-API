namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class LabRequest
    {
        public Guid? UserId { get; set; }
        public Guid? diagnosisCode { get; set; }
        public string? application_number { get; set; }
        public Guid? organization_code { get; set; }
        public string? lab_test { get; set; }
        public Guid? tasks_task_code { get; set; }
        public Guid? detail_code { get; set; }
        public Guid? lab_Code { get; set; }
    }
}
