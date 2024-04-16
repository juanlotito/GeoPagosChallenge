using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;


namespace TestPaymentProcessorApp
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var response = await _client.GetAsync($"/payments/status/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsOk_ForValidId()
        {
            // Arrange
            var validId = 1;

            // Act
            var response = await _client.GetAsync($"/payments/status/{validId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
