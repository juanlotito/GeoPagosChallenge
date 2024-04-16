using PaymentProcessor.Models.PaymentProcessing;
using PaymentProcessor.Services.Interface;
using System;

namespace PaymentProcessor.Services
{
    public class PaymentProcessorService : IPaymentProcessorService
    {
        public PaymentProcessingResponse ProcessPayment(PaymentProcessingRequest request)
        {
            bool isAmountInteger = request.Amount == Math.Floor(request.Amount);

            if (isAmountInteger)
            {
                return new PaymentProcessingResponse
                {
                    PaymentRequestId = request.PaymentRequestId,
                    IsApproved = true,
                    Message = "Payment is approved."
                };
            }
            else
            {
                return new PaymentProcessingResponse
                {
                    PaymentRequestId = request.PaymentRequestId,
                    IsApproved = false,
                    Message = "Payment is denied. Amount must be an integer."
                };
            }
        }
    }
}
