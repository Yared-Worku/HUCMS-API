namespace HU_api.Entities.HU
{
    public class Tree
    {
          public string? topic_code { get; set; }
        public string? service_code { get; set; }
        public string? Registration_code { get; set; }
        public string? description_en { get; set; }
        public string? description_local { get; set; }
        public string? description_am { get; set; }
        public string? description_or { get; set; }
        public string? description_tg { get; set; }
        public bool is_active { get; set; }
        public bool is_login_required { get; set; }
        public bool is_duplicate_allowed { get; set; }
        public string? RequirementsTOApply_en { get; set; }
        public string? RequirementsTOApply_Local { get; set; }
        public string? RequirementsTOApply_am { get; set; }
        public string? RequirementsTOApply_or { get; set; }
        public string? RequirementsTOApply_tg { get; set; }
        public string? task_code { get; set; }
        public string? RoleId { get; set; }
        public string Departments { get; set; }
        public string organization_code { get; set; }

        //public string? department_en { get; set; }
        public string? meta_data_forms_form_code { get; set; }
    }
}
