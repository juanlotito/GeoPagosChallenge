using PaymentProcessor.Models.Auth;

namespace PaymentProcessor.Services.Interface
{
    public interface IAuthService
    {
        public string GenerateJwtToken(string username);
        public bool IsValidUser(UserCredentials credentials);
    }
}
