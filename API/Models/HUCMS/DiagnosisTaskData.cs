namespace HUCMS.Models.HUCMS
{
    public class DiagnosisTaskData
    {
        public string? application_number { get; set; }
        public string? diagnosis { get; set; }
        public Guid? tasks_task_code { get; set; }
        public Guid? UserId { get; set; }
        public Guid? organization_code { get; set; }
        public Guid? services_service_code { get; set; }
        public Guid? diagnosis_Code { get; set; }
        public Guid? process_detail_code { get; set; }
    }
}
