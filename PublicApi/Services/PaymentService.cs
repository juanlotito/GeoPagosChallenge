using PublicApi.Models.Enum;
using PublicApi.Models.Payment;
using PublicApi.Models.PaymentProcessor;
using PublicApi.Repositories.Interface;
using PublicApi.Services.Interface;

namespace PublicApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IExternalPaymentProcessor _paymentProcessor;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;


        public PaymentService(IPaymentRepository paymentRepository, IExternalPaymentProcessor paymentProcessor, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _paymentRepository = paymentRepository;
            _paymentProcessor = paymentProcessor;
        }

        public async Task<PaymentRequest> GetPaymentStatus(int paymentRequestId)
        {
            return await _paymentRepository.GetPaymentRequestByIdAsync(paymentRequestId);
        }

        public async Task<IEnumerable<ApprovedPayment>> GetAuthorizedPayments()
        {
            return await _paymentRepository.GetAllAuthorizedPaymentsAsync();
        }

        public async Task<PaymentResponse> AuthorizePayment(PaymentRequest request, string token)
        {
            int insertedId = await _paymentRepository.AddPaymentRequest(request);

            var processorRequest = new PaymentProcessorRequest
            {
                Amount = request.Amount,
                CustomerId = request.CustomerId,
                PaymentRequestId = insertedId
            };

            _backgroundTaskQueue.Enqueue(async (cancellationToken) =>
            {
                try
                {
                    var processorResponse = await _paymentProcessor.ProcessPaymentAsync(processorRequest, token);
                    await _paymentRepository.UpdatePaymentStatus(insertedId, (int)PaymentStatus.Pending);
                }
                catch (Exception ex)
                {
                    await _paymentRepository.UpdatePaymentStatus(insertedId, (int)PaymentStatus.Denied);
                }
            });

            return new PaymentResponse
            {
                Success = true,
                PaymentRequestId = insertedId
            };
        }

        public async Task CheckAndReverseIfNotConfirmed(int paymentId)
        {
            var paymentRequest = await _paymentRepository.GetPaymentRequestByIdAsync(paymentId);

            if (paymentRequest == null)
            {
                Console.WriteLine($"No payment request found with ID {paymentId}");
                return;
            }

            if (!paymentRequest.IsConfirmed && paymentRequest.RequiresConfirmation)
            {
                if (paymentRequest.StatusId == 3)
                {
                    paymentRequest.StatusId = 4;

                    await _paymentRepository.UpdatePaymentRequest(paymentId, paymentRequest.StatusId, paymentRequest.IsConfirmed);

                    Console.WriteLine($"Payment request ID {paymentId} has been reversed due to lack of confirmation.");
                }
            }
        }

        public async Task<bool> DoesRequestRequireConfirmation(int clientId)
        {
            var client = await this._paymentRepository.GetClientById(clientId);

            return client.CustomerTypeDescription == "Segundo";
        }

        public async Task<PaymentConfirmationResult> ConfirmPayment(int paymentRequestId)
        {
            var paymentRequest = await _paymentRepository.GetPaymentRequestByIdAsync(paymentRequestId);

            if (paymentRequest == null)
            {
                return new PaymentConfirmationResult { Success = false, Message = "Payment request not found." };
            }

            if (paymentRequest.IsConfirmed)
            {
                return new PaymentConfirmationResult { Success = false, Message = "Payment request is already confirmed." };
            }

            if ((DateTime.UtcNow - paymentRequest.RequestDate).TotalMinutes > 5 && paymentRequest.RequiresConfirmation)
            {
                await this._paymentRepository.ReversePayment(paymentRequestId); 
                return new PaymentConfirmationResult { Success = false, Message = "Payment request confirmation time has expired and has been reversed." };
            }

            paymentRequest.IsConfirmed = true;
            paymentRequest.StatusId = 1; 

            await _paymentRepository.UpdatePaymentRequest(paymentRequestId, paymentRequest.StatusId, paymentRequest.IsConfirmed);
            await _paymentRepository.AddApprovedPayment(paymentRequestId);

            return new PaymentConfirmationResult { Success = true };
        }

        public async Task<bool> ReversePayment(int paymentRequestId)
        {
            try
            {
                await _paymentRepository.ReversePayment(paymentRequestId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
