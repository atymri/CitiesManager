using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CitiesManager.Core.DTOs;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace CitiesManager.Core.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public AuthenticationResponse CreateJwtToken(User user)
        {
            var issuer = _configuration["jwt:Issuer"]; // who makes the token?
            var audience = _configuration["jwt:Audience"]; // who uses the token?
            var expirationDate= DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["jwt:EXPIRATION_MINUTES"])); // when token expires?
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"])); // secret key used for encryption (has to be env variables but we are in development )

            var claims = new Claim[]
            {
                // the subject (sub), issued at (iat), jti (jwt unique id) are recommended claims and not optional.

                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()), // subject (user id)
                new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64), // issued at (date and time of the token generated)
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (unique identifier for the token)
                new (ClaimTypes.NameIdentifier, user.Email),
                new (ClaimTypes.Email, user.Email)// unique name identifier of the user (Email) (optional)
            };

            var signInCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenGenerator = new JwtSecurityToken(issuer: issuer, 
                audience: audience,
                claims: claims, 
                expires: expirationDate, 
                signingCredentials: signInCreds);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenGenerator);

            return new AuthenticationResponse()
            {
                Email = user.Email, 
                ExpireDate = expirationDate, 
                Token = token,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpireDate = 
                    DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_MINUTES"]))
            };
        }

        public ClaimsPrincipal? GetPrincipalFromAccessToken(string? token)
        {
            var tokenValidator = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false // important, its false because its being called with an expired token.
            };

            var principal=
                new JwtSecurityTokenHandler().ValidateToken(token, tokenValidator, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                throw new SecurityTokenException("Invalid token");
        
            return principal;

        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
