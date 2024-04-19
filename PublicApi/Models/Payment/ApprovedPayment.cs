namespace PublicApi.Models.Payment
{
    public class ApprovedPayment
    {
        public int PaymentId { get; set; }
        public DateTime PaymentApprovalDate { get; set; }
        public double PaymentAmount { get; set; }
        public int PaymentCustomerId { get; set; }
    }
}
