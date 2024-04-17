using Newtonsoft.Json;
using PublicApi.Models.Enum;
using PublicApi.Models.PaymentProcessor;
using PublicApi.Repositories.Interface;
using PublicApi.Services.Interface;
using System.Text;

namespace PublicApi.Services
{
    public class ExternalPaymentProcessor : IExternalPaymentProcessor
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentProcessorConfig _paymentProcessorConfig;

        public ExternalPaymentProcessor(IPaymentRepository paymentRepository, PaymentProcessorConfig paymentProcessorConfig)
        {
            _paymentRepository = paymentRepository;
            _paymentProcessorConfig = paymentProcessorConfig;
        }

        public async Task<PaymentProcessorResponse> ProcessPaymentAsync(PaymentProcessorRequest request, string token)
        {
            var processorRequest = new PaymentProcessorRequest
            {
                PaymentRequestId = request.PaymentRequestId,
                Amount = request.Amount,
                CustomerId = request.CustomerId
            };

            var content = new StringContent(JsonConvert.SerializeObject(processorRequest), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var client = new HttpClient())
            {
                //JWT
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(_paymentProcessorConfig.Uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    await _paymentRepository.UpdatePaymentStatus(request.PaymentRequestId, (int)PaymentStatus.Denied);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var processedResult = JsonConvert.DeserializeObject<PaymentProcessorResponse>(responseString);

                await _paymentRepository.ConfirmPayment(request.PaymentRequestId);

                return processedResult;
            }
        }

    }
}
