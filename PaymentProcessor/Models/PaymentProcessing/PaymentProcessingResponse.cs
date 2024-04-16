namespace PaymentProcessor.Models.PaymentProcessing
{
    public class PaymentProcessingResponse
    {
        public int PaymentRequestId { get; set; }
        public bool IsApproved { get; set; }
        public string Message { get; set; }
    }
}
