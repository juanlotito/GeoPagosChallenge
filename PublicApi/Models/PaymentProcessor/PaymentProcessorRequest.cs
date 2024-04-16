namespace PublicApi.Models.PaymentProcessor
{
    public class PaymentProcessorRequest
    {
        public int PaymentRequestId { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
    }
}
