using PublicApi.Models.PaymentProcessor;

namespace PublicApi.Services.Interface
{
    public interface IExternalPaymentProcessor
    {
        public Task<PaymentProcessorResponse> ProcessPaymentAsync(PaymentProcessorRequest request, string token);
    }
}
