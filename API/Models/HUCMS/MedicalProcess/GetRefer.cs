namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class GetRefer
    {
        public Guid? ref_Code { get; set; }
        public string? vitalSign { get; set; }
        public string? physicalExamination { get; set; }
        public string? referalReason { get; set; }
    }
}
