using System;

namespace HUCMS.Models.HUCMS.Commons
{
    public class Application
    {
        public Guid? application_code { get; set; }
        public string? application_number { get; set; }
        public string? value { get; set; }
        public Guid? tasks_task_code { get; set; }
        public string? pro_name { get; set; }
        public int? is_completed { get; set; }
        public string? status { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? start_date { get; set; }
        public string? date_created_et { get; set; }
        public string? UserId { get; set; }
        public string? serviceName { get; set; }
        public string? description_local { get; set; }
        public string? service_description_en { get; set; }
        public string? description_en { get; set; }
        public string? username { get; set; }
        public Guid? services_service_code { get; set; }
        public bool? is_synched { get; set; }
        public DateTime? date_synched { get; set; }
        public Guid? organization_code { get; set; }
        public Guid? application_detail_id { get; set; }
        public Guid? meta_data_forms_form_code { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? diagnosis_code { get; set; }
        public string? document { get; set; }
    }

}
