namespace PublicApi.Models.Payment
{
    public class PaymentRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int StatusId { get; set; }
        public int PaymentTypesId { get; set; }
        public bool IsConfirmed { get; set; }
        public bool RequiresConfirmation { get; set; } = false; 
        public string? Details { get; set; } = string.Empty;
    }
}
