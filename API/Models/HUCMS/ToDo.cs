namespace BPM.Entities.HU
{
    public class ToDo
    {
        public string? application_number { get; set; }
        public Guid? todocode { get; set; }
        public Guid? tasks_task_code { get; set; }
        public string? UserId { get; set; }
        public int? looked { get; set; }
        public Guid? jump_From { get; set; }
        public Guid? services_service_code { get; set; }
        public Guid? organization_code { get; set; }
    }
}
