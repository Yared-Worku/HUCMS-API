namespace HUCMS.Models.HUCMS
{
    public class GetLab
    {
        public string? lab_test { get; set; }
        public string? lab_result { get; set; }
        public Guid? lab_Code { get; set; }
        public Guid? detail_code { get; set; }
    }
}
