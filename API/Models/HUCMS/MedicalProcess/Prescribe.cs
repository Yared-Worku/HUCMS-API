namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class Prescribe
    {
        public string? application_number { get; set; }
        public Guid? todocode { get; set; }
        public Guid? tasks_task_code { get; set; }
        public Guid? UserId { get; set; }
        public int? looked { get; set; }
        public Guid? jump_From { get; set; }
        public Guid? services_service_code { get; set; }
        public Guid? organization_code { get; set; }
        public Guid? ProcessDetailCode { get; set; }
        public Guid? diagnosisCode { get; set; }
        public string? RX { get; set; }
    }
}
