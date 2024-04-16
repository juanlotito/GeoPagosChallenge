namespace PublicApi.Models.Payment
{
    public class ApprovedPayment
    {
        public int PaymentRequestId { get; set; }
        public DateTime ApprovalDate { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
    }
}
