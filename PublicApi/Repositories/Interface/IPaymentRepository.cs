using PublicApi.Models.Customer;
using PublicApi.Models.Payment;

namespace PublicApi.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentRequest> GetPaymentRequestByIdAsync(int id);
        Task<bool> GetIsConfirmed(int paymentId);
        Task<int> AddPaymentRequest(PaymentRequest paymentRequest);
        Task UpdatePaymentStatus(int paymentRequestId, int statusId, string? details);
        Task ConfirmPayment(int paymentRequestId);
        Task<Customer> GetClientById(int clientId);
        Task UpdatePaymentRequest(int paymentRequestId, int newStatusId, bool isConfirmed);
        Task ReversePayment(int paymentRequestId);
        Task<IEnumerable<ApprovedPayment>> GetAllAuthorizedPaymentsAsync();
        Task AddApprovedPayment(int paymentId);
    }
}
