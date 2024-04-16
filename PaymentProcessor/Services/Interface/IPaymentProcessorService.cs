using PaymentProcessor.Models;

namespace PaymentProcessor.Services.Interface
{
    public interface IPaymentProcessorService
    {
        public PaymentProcessingResponse ProcessPayment(PaymentProcessingRequest request);
    }
}
