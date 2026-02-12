namespace HUCMS.Models.HUCMS
{
    public class Prescribe
    {
        public Guid? UserId { get; set; }
        public Guid? diagnosisCode { get; set; }
        public string? RX { get; set; }
    }
}
