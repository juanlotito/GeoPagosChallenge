using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using PaymentProcessor.Models.Auth;
using PaymentProcessor.Services.Interface;

namespace PaymentProcessor.Services
{
    public class AuthService : IAuthService
    {
        public string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4c5nNI0zNIgBonQ4c5nNI0zNIgBonQ12"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
    
            var token = new JwtSecurityToken(
                issuer: "PublicApiIssuer",
                audience: "PaymentProcessorAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);
    
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    
        public bool IsValidUser(UserCredentials credentials)
        {
            // Como es un challenge, no creí necesario implementar una DB para almacenar y validar usuarios/contraseñas
            return credentials.Username == "admin" && credentials.Password == "admin";
        }

    }
}
