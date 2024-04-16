namespace PublicApi.Models.Payment
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public int PaymentRequestId { get; set; }
        public bool Approved { get; set; }
    }
}
