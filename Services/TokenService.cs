using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes TokenService with app configuration.
        /// </summary>
        /// <param name="config">App configuration injected via DI.</param>
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string clientId)
        {
            // Create symmetric security key using the secret from appsettings
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            // Use HMAC SHA256 algorithm for signing the token
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Define claims to embed inside the token payload
            // ClaimTypes.Name is used in BooksController to get the ClientId
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Build the JWT token with issuer, audience, claims, expiry and signing key
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                                        double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            // Serialize token object to a JWT string and return
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}