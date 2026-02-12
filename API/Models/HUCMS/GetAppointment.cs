namespace HUCMS.Models.HUCMS
{
    public class GetAppointment
    {
        public DateOnly? appointment_date { get; set; }
        public Guid? appointment_Code { get; set; }
        public String? appointment_reason { get; set; }
    }
}
