using System.ComponentModel.DataAnnotations;

namespace PaymentProcessor.Models.PaymentProcessing
{
    public class PaymentProcessingRequest
    {
        [Required, Range(1, 500, ErrorMessage = "The PaymentRequestId must be more than 0")] public int PaymentRequestId { get; set; }
        [Required, Range(1, double.MaxValue, ErrorMessage = "The Amount must be more than 0")] public decimal Amount { get; set; }
        [Required, Range(1, 500, ErrorMessage = "The CustomerId must be more than 0")] public int CustomerId { get; set; }
    }
}
