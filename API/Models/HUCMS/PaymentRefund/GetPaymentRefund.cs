namespace HUCMS.Models.HUCMS.PaymentRefund
{
    public class GetPaymentRefund
    {
        public string? amount_inWord { get; set; }
        public Guid? pr_Code { get; set; }
        public float? amount_inDigit { get; set; }
    }
}
