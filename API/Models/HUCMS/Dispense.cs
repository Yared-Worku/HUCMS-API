namespace HUCMS.Models.HUCMS
{
    public class Dispense
    {
        public Guid? todocode { get; set; }
        public Guid? UserId { get; set; }
        public Guid? detail_code { get; set; }
        public int? quantity { get; set; }
        public string? remark { get; set; }
        public string? application_number { get; set; }
    }
}
