namespace HU_api.Entities.HU
{
    public class AddTask
    {
        public string? task_code { get; set; }
        public string? registration_code { get; set; }
        public string? description_en { get; set; }
        public string? description_local { get; set; }
        public int? estimated_time_hours { get; set; }
        public int? level { get; set; }
        public bool notification_required { get; set; }
        public bool special_rules { get; set; }
        public bool is_active { get; set; }
        public string? services_service_code { get; set; }
        public string? created_by { get; set; }
        public string? workflow_code { get; set; }
        //public string? application_detail_id { get; set; }
        public string? task_types_task_type_code { get; set; }
        public string? roleId { get; set; }
        public string? meta_data_forms_form_code { get; set; }
        public string? status_en { get; set; }
        public string? status_local { get; set; }
        public bool physical_presence { get; set; }
        public string? Name_en { get; set; }
        public string? form_name { get; set; }
        public string? description_en_tasktype { get; set; }
        public string? description_en_services { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? RoleName { get; set; }

    }
}
