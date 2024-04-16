namespace PublicApi.Models.PaymentProcessor
{
    public class PaymentProcessorResponse
    {
        public int PaymentRequestId { get; set; }
        public bool IsApproved { get; set; }
        public string Message { get; set; }
    }
}
