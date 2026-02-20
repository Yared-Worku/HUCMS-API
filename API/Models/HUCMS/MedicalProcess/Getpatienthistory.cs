namespace HUCMS.Models.HUCMS.MedicalProcess
{
    public class Getpatienthistory
    {
        public string? detail_diagnosis { get; set; }
        public string? Doctor_FirstName { get; set; }
        public string? created_date { get; set; }
        public string? Doctor_LastName { get; set; }
        public string? LabTechnician_FirstName { get; set; }
        public string? LabTechnician_LastName { get; set; }
        public string? RX { get; set; }
        public string? appointment_reason { get; set; }
        public string? appointment_date { get; set; }
        public string? reason_for_referal { get; set; }
        public string? lab_test { get; set; }
        public string? lab_result { get; set; }
    }
}
