using PaymentProcessor.Models.PaymentProcessing;

namespace PaymentProcessor.Services.Interface
{
    public interface IPaymentProcessorService
    {
        public PaymentProcessingResponse ProcessPayment(PaymentProcessingRequest request);
    }
}
