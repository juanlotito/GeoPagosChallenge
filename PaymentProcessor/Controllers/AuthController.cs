using Microsoft.AspNetCore.Mvc;
using PaymentProcessor.Models.Auth;
using PaymentProcessor.Services.Interface;

namespace PaymentProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserCredentials credentials)
        {
            if (_authService.IsValidUser(credentials))
            {
                var token = _authService.GenerateJwtToken(credentials.Username);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }

    }
}
