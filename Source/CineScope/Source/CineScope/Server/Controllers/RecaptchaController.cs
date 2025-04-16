using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace CineScope.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecaptchaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public RecaptchaController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] RecaptchaVerificationRequest request)
        {
            try
            {
                var secretKey = _configuration["RecaptchaSettings:SecretKey"];
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", request.Token)
                });

                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(jsonResponse);

                if (verificationResult.Success)
                {
                    return Ok(new { success = true });
                }

                return BadRequest(new { success = false, message = "reCAPTCHA verification failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class RecaptchaVerificationRequest
    {
        public string Token { get; set; }
        public string Action { get; set; }
    }

    public class RecaptchaVerificationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
} 