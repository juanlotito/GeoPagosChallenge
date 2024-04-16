using Microsoft.AspNetCore.Mvc;
using Moq;
using PublicApi.Controllers;
using PublicApi.Models;
using PublicApi.Models.Payment;
using PublicApi.Services.Interface;

namespace TestPaymentProcessorApp
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IBackgroundTaskQueue> _mockBackgroundTaskQueue;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockBackgroundTaskQueue = new Mock<IBackgroundTaskQueue>();
            _controller = new PaymentController(_mockPaymentService.Object, _mockBackgroundTaskQueue.Object);
        }

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
    }

}