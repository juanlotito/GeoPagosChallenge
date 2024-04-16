using Microsoft.AspNetCore.Mvc;
using PublicApi.Models;
using PublicApi.Models.Authorization;
using PublicApi.Models.Enum;
using PublicApi.Models.Payment;
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
        public async Task<IActionResult> GetPaymentStatus(int paymentRequestId)
        {
            try
            {
                var paymentStatus = await _paymentService.GetPaymentStatus(paymentRequestId);

                return paymentStatus != null
                    ? Ok(new Response(true, "Search done correctly", paymentStatus))
                    : NotFound(new Response(false, "Payment request not found.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpGet("/payments/authorized")]
        public async Task<IActionResult> GetAuthorizedPayments()
        {
            try
            {
                var authorizedPayments = await _paymentService.GetAuthorizedPayments();

                return authorizedPayments.Count() > 0
                    ? Ok(new Response(true, "Search done correctly", authorizedPayments))
                    : NotFound(new Response(false, "No payments found", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }
        #endregion

        #region POST
        [HttpPost("/payments/authorize")]
        public async Task<IActionResult> AuthorizePayment([FromBody] AuthorizationRequest request, [FromHeader] string token)
        {
            try 
            {
                var response = await this._paymentService.AuthorizePayment(new PaymentRequest
                {
                    CustomerId = request.CustomerId,
                    Amount = request.Amount,
                    PaymentTypesId = request.Type,
                    IsConfirmed = false,
                    StatusId = (int)PaymentStatus.Pending,
                    RequiresConfirmation = await this._paymentService.DoesRequestRequireConfirmation(request.CustomerId)
                }, token);

                if (request.RequiresConfirmation && response.Approved)
                {
                    _backgroundTaskQueue.Enqueue(async token =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), token);
                        await this._paymentService.CheckAndReverseIfNotConfirmed(response.PaymentRequestId);
                    });
                }

                return Ok(new Response(response.Success, "Request is being processed.", response));
            }
            catch(Exception ex) 
            {
                return BadRequest(new Response(false, $"Error in payment authorization: {ex.Message}", null));
            }
            
        }

        [HttpPost("/payments/confirm/{paymentRequestId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentRequestId) 
        {
            try 
            {
                var result = await _paymentService.ConfirmPayment(paymentRequestId);

                return result.Success
                    ? Ok(new Response(true, result.Message, null))
                    : BadRequest(new Response(false, result.Message, null));
            } 
            catch(Exception ex) 
            {
                return StatusCode(500, new Response(false, ex.Message, null));
            }
        }

        [HttpPost("/payments/reverse/{paymentRequestId}")]
        public async Task<IActionResult> ReversePayment(int paymentRequestId)
        {
            try
            {
                return await _paymentService.ReversePayment(paymentRequestId)
                    ? Ok(new Response(true, "Payment has been successfully reversed.", null))
                    : BadRequest(new Response(false, "Failed to reverse payment.", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response(false, ex.Message, null));
            }
        }
        #endregion
    }
}
