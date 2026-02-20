namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class GetAppointment
    {
        public DateOnly? appointment_date { get; set; }
        public Guid? appointment_Code { get; set; }
        public string? appointment_reason { get; set; }
    }
}
