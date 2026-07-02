using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Handles authentication and JWT token generation.
    /// Does not require authorization (public endpoint).
    /// 
    /// Route: {version}/api/Auth
    /// Example: v1/api/Auth/token
    /// </summary>
    [ApiController]
    [Route("{version}/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes AuthController with required dependencies.
        /// </summary>
        /// <param name="config">App configuration for reading ClientCredentials.</param>
        /// <param name="tokenService">Service used to generate JWT tokens.</param>
        /// <param name="logger">Logger for recording authentication events.</param>
        public AuthController(
            IConfiguration config,
            TokenService tokenService,
            ILogger<AuthController> logger)
        {
            _config = config;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a JWT Bearer token using Basic Authentication.
        /// 
        /// How to call this endpoint:
        /// 1. Combine ClientId and ClientSecret with colon separator
        ///    => "libraryClient:librarySecret123"
        /// 2. Encode the combined string to Base64
        ///    => "bGlicmFyeUNsaWVudDpsaWJyYXJ5U2VjcmV0MTIz"
        /// 3. Set the Authorization header in Postman:
        ///    Key   : Authorization
        ///    Value : Basic bGlicmFyeUNsaWVudDpsaWJyYXJ5U2VjcmV0MTIz
        /// 4. Send GET request to v1/api/Auth/token
        /// 
        /// On success, use the returned token as:
        ///    Authorization: Bearer {token}
        /// for all protected Book endpoints.
        /// </summary>
        /// <returns>
        /// JWT token string with token type and expiry duration on success.
        /// 401 Unauthorized if credentials are missing or invalid.
        /// 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            // Read the Authorization header from the incoming request
            var authHeader = Request.Headers["Authorization"].ToString();

            // Validate that header exists and follows "Basic " prefix format
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
            {
                _logger.LogWarning(
                    "Token request failed: Missing or invalid Authorization header.");

                return Unauthorized(new
                {
                    success = false,
                    message = "Authorization header is missing or invalid."
                });
            }

            try
            {
                // Strip the "Basic " prefix to get only the Base64 encoded string
                var base64Credentials = authHeader.Substring("Basic ".Length).Trim();

                // Decode Base64 string back to "ClientId:ClientSecret" format
                var decodedCredentials = Encoding.UTF8.GetString(
                    Convert.FromBase64String(base64Credentials));

                var parts = decodedCredentials.Split(':', 2);

                if (parts.Length != 2)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid credentials format. Expected ClientId:ClientSecret"
                    });
                }

                var clientId = parts[0];
                var clientSecret = parts[1];

                // Read expected credentials from appsettings.json
                var validClientId = _config["ClientCredentials:ClientId"];
                var validClientSecret = _config["ClientCredentials:ClientSecret"];

                // Validate submitted credentials against appsettings values
                if (clientId != validClientId || clientSecret != validClientSecret)
                {
                    _logger.LogWarning(
                        "Token request failed: Invalid ClientId or ClientSecret " +
                        "for ClientId: {ClientId}", clientId);

                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid ClientId or ClientSecret."
                    });
                }

                // Credentials are valid, generate JWT token
                var token = _tokenService.GenerateToken(clientId);

                _logger.LogInformation(
                    "Token generated successfully for ClientId: {ClientId}", clientId);

                return Ok(new
                {
                    success = true,
                    token,
                    tokenType = "Bearer",
                    expiresIn = $"{_config["Jwt:ExpiryMinutes"]} minutes"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating token.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating the token."
                });
            }
        }
    }
}