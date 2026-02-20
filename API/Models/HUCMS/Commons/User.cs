namespace HUCMS.Models.HUCMS.Commons
{
    public class User
    {
        public Guid? UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public Guid? depCode { get; set; } // Assigned Department
    }
}
