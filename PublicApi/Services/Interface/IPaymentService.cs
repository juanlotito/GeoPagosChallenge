using PublicApi.Models.Payment;

namespace PublicApi.Services.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponse> AuthorizePayment(PaymentRequest request, string token);
        Task CheckAndReverseIfNotConfirmed(int paymentId);
        Task<bool> DoesRequestRequireConfirmation(int paymentTypeId);
        Task<PaymentConfirmationResult> ConfirmPayment(int paymentRequestId);
        Task<bool> ReversePayment(int paymentRequestId);
        Task<PaymentRequest> GetPaymentStatus(int paymentRequestId);
        Task<IEnumerable<ApprovedPayment>> GetAuthorizedPayments();
    }
}
