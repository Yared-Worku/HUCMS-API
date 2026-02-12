namespace HU_api.Entities.HU
{
    public class Department
    {
        public string? depCode { get; set; }
        public string? depName { get; set; }
        public string? orgCode { get; set; }
        public Guid? created_by { get; set; }
        public DateTime? created_date { get; set; }
        public bool? is_active { get; set; }
        // public bool? is_published { get; set; } // optional
    }
}
