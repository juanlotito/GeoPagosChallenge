using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Models.Authorization;
using PublicApi.Models.Payment;
using PublicApi.Repositories.Interface;
using PublicApi.Services.Interface;

namespace PublicApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        public PaymentController(IPaymentService paymentService, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _paymentService = paymentService;
        }

        #region GET
        [HttpGet("/payments/status/{paymentRequestId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(int paymentRequestId)
        {
            try
            {
                var paymentStatus = await _paymentService.GetPaymentStatus(paymentRequestId);

                if (paymentStatus != null)
                    return Ok(paymentStatus);
                else
                    return NotFound("Payment request not found.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/payments/authorized")]
        [Authorize]
        public async Task<IActionResult> GetAuthorizedPayments()
        {
            try
            {
                var authorizedPayments = await _paymentService.GetAuthorizedPayments();
                return Ok(authorizedPayments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region POST
        [HttpPost("/payments/authorize")]
        public async Task<IActionResult> AuthorizePayment([FromBody] AuthorizationRequest request, [FromHeader] string token)
        {
            var paymentRequest = new PaymentRequest
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                PaymentTypesId = request.Type,
                IsConfirmed = false,
                StatusId = 3,
                RequiresConfirmation = await this._paymentService.DoesRequestRequireConfirmation(request.CustomerId)
            };

            var response = await this._paymentService.AuthorizePayment(paymentRequest, token);

            if (request.RequiresConfirmation && response.Approved)
            {
                _backgroundTaskQueue.Enqueue(async token =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                    await this._paymentService.CheckAndReverseIfNotConfirmed(response.PaymentRequestId);
                });
            }

            return Ok(response);
        }

        [HttpPost("/payments/confirm/{paymentRequestId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentRequestId) 
        {
            var result = await _paymentService.ConfirmPayment(paymentRequestId);

            if (result.Success)
            {
                return Ok(new { Message = "Payment confirmed successfully." });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }

        [HttpPost("/payments/reverse/{paymentRequestId}")]
        public async Task<IActionResult> ReversePayment(int paymentRequestId)
        {
            try
            {
                var result = await _paymentService.ReversePayment(paymentRequestId);

                if (result)
                {
                    return Ok(new { Message = "Payment has been successfully reversed." });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to reverse payment." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        #endregion
    }
}
