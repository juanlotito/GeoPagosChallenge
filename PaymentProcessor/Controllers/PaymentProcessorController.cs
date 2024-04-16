using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentProcessor.Models.PaymentProcessing;
using PaymentProcessor.Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentProcessorController : ControllerBase
    {
        private readonly IPaymentProcessorService _paymentProcessorService;

        public PaymentProcessorController(IPaymentProcessorService paymentProcessorService)
        {
            _paymentProcessorService = paymentProcessorService;
        }

        [HttpPost("process")]
        [Authorize]
        public ActionResult<PaymentProcessingResponse> ProcessPayment([FromBody, Required] PaymentProcessingRequest request)
        {
            try 
            {
                var response = _paymentProcessorService.ProcessPayment(request);
                return Ok(response);
            } 
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
