namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class SetAppointment
    {
        public Guid? UserId { get; set; }
        public Guid? diagnosisCode { get; set; }
        public DateOnly? appointment_date { get; set; }
        public string? appointment_reason { get; set; }
        public Guid? appointment_Code { get; set; }
    }
}
