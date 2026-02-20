namespace HUCMS.Models.HUCMS.Commons
{
    public class AddTopic
    {
        public string? Topic_code { get; set; }
        public string? description_or { get; set; }
        public string? description_en { get; set; }
        public string? description_local { get; set; }
        public string? description_am { get; set; }
        public string? description_tg { get; set; }
        public bool is_synched { get; set; }
        public bool is_published { get; set; }
        public bool is_active { get; set; }
        public string? date_created { get; set; }
        public string? created_by { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Parent_Description { get; set; }
        public string? Topic_link { get; set; }
        public string? Topic_help_Link { get; set; }
        public string? Parent_Topic_Id { get; set; }

    }
}
