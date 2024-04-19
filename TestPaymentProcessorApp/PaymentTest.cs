using Microsoft.AspNetCore.Mvc;
using Moq;
using Npgsql;
using PublicApi.Controllers;
using PublicApi.Models;
using PublicApi.Models.Authorization;
using PublicApi.Models.Payment;
using PublicApi.Services.Interface;
using PublicApi.Utils.RabbitMQ.Interface;
using System.Data;

namespace TestPaymentProcessorApp
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IRabbitMQService> _mockRabbitMQService;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockRabbitMQService = new Mock<IRabbitMQService>();
            _controller = new PaymentController(_mockPaymentService.Object, _mockRabbitMQService.Object);
        }

        #region Test for GETs
        [Fact]
        public async Task GetPaymentStatus_ReturnsNotFound_WhenPaymentRequestIsNotFound()
        {
            // Arrange
            var paymentRequestId = 999;
            _mockPaymentService.Setup(s => s.GetPaymentStatus(paymentRequestId))
                               .ReturnsAsync((PaymentRequest)null);

            // Act
            var result = await _controller.GetPaymentStatus(paymentRequestId);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<Response>(actionResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Payment request not found.", response.Message);
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsOk_WhenPaymentRequestIsFound()
        {
            // Arrange
            var paymentRequestId = 1;
            var mockPaymentRequest = new PaymentRequest {CustomerId = 1, Amount = 10, RequestDate = DateTime.Now, StatusId = 3, PaymentTypesId = 1, IsConfirmed = false, RequiresConfirmation = true  };

            _mockPaymentService.Setup(s => s.GetPaymentStatus(paymentRequestId))
                               .ReturnsAsync(mockPaymentRequest);

            // Act
            var result = await _controller.GetPaymentStatus(paymentRequestId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response>(actionResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Search done correctly", response.Message);
            Assert.Equal(mockPaymentRequest, response.Data);
        }
        #endregion

        #region Test for Authorize
        [Fact]
        public async Task AuthorizePayment_ReturnsOk_WhenPaymentIsProcessed()
        {
            // Arrange
            var request = new AuthorizationRequest
            {
                CustomerId = 1,
                Amount = 100.00m,
                Type = 1,
                RequiresConfirmation = false
            };
            var response = new PaymentResponse { Success = true, Approved = true, PaymentRequestId = 123 };

            _mockPaymentService.Setup(s => s.AuthorizePayment(It.IsAny<PaymentRequest>(), It.IsAny<string>()))
                               .ReturnsAsync(response);
            // No necesitas setup para RabbitMQ si RequiresConfirmation es false

            // Act
            var result = await _controller.AuthorizePayment(request, "token");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnResponse = Assert.IsType<Response>(okResult.Value);
            Assert.True(returnResponse.Success);
        }

        //[Fact]
        //public async Task AuthorizePayment_SendsMessageAndClosesConnection_WhenRequiresConfirmation()
        //{
        //    // Arrange
        //    var request = new AuthorizationRequest
        //    {
        //        CustomerId = 1,
        //        Amount = 100.00m,
        //        Type = 1,
        //        RequiresConfirmation = true
        //    };
        //    var response = new PaymentResponse { Success = true, Approved = true, PaymentRequestId = 123 };

        //    _mockPaymentService.Setup(s => s.AuthorizePayment(It.IsAny<PaymentRequest>(), It.IsAny<string>()))
        //                       .ReturnsAsync(response);

        //    _mockRabbitMQService.Setup(r => r.SendMessage(It.IsAny<string>(), It.IsAny<byte[]>()));
        //    _mockRabbitMQService.Setup(r => r.CloseConnection());

        //    // Act
        //    var result = await _controller.AuthorizePayment(request, "token");

        //    // Assert
        //    _mockRabbitMQService.Verify(r => r.SendMessage("payment_confirmation", It.IsAny<byte[]>()), Times.Once);
        //    _mockRabbitMQService.Verify(r => r.CloseConnection(), Times.Once);
        //}
        #endregion

        #region Test for ConfirmPayment
        [Fact]
        public async Task ConfirmPayment_ReturnsBadRequest_WhenConfirmationFails()
        {
            // Arrange
            var paymentRequestId = 1;
            var result = new PaymentConfirmationResult { Success = false, Message = "Confirmation failed." };
            _mockPaymentService.Setup(s => s.ConfirmPayment(paymentRequestId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.ConfirmPayment(paymentRequestId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            var returnResponse = Assert.IsType<Response>(badRequestResult.Value);
            Assert.False(returnResponse.Success);
            Assert.Equal("Confirmation failed.", returnResponse.Message);
        }
        #endregion

        #region Test for ReversePayment
        [Fact]
        public async Task ReversePayment_ReturnsOk_WhenReversalIsSuccessful()
        {
            // Arrange
            var paymentRequestId = 1;
            _mockPaymentService.Setup(s => s.ReversePayment(paymentRequestId))
                               .ReturnsAsync(true);

            // Act
            var response = await _controller.ReversePayment(paymentRequestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnResponse = Assert.IsType<Response>(okResult.Value);
            Assert.True(returnResponse.Success);
            Assert.Equal("Payment has been successfully reversed.", returnResponse.Message);
        }
        #endregion

        //Al no haber podido testear esto en pruebas integrales, deje un ping a la base de datos  
        [Fact]
        public async Task DatabaseIsAccessible()
        {
            var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=GeoPagos;Username=postgres;Password=1234;");
            try
            {
                await connection.OpenAsync();
                Assert.True(connection.State == ConnectionState.Open);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

    }
}