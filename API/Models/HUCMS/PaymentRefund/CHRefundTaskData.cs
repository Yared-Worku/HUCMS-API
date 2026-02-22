namespace HUCMS.Models.HUCMS.PaymentRefund
{
    public class CHRefundTaskData
    {
        public string? application_number { get; set; }
        public string? amount_inWord { get; set; }
        public Guid? tasks_task_code { get; set; }
        public Guid? UserId { get; set; }
        public Guid? todocode { get; set; }
        public Guid? services_service_code { get; set; }
        public float? amount_inDigit { get; set; }
        public Guid? process_detail_code { get; set; }
    }
}
