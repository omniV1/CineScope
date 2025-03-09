using System.Threading.Tasks;
using CineScope.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Server.Controllers
{
    /// <summary>
    /// API controller for content filtering operations.
    /// Provides endpoints for validating user-generated content.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContentFilterController : ControllerBase
    {
        /// <summary>
        /// Reference to the content filter service for validation logic.
        /// </summary>
        private readonly ContentFilterService _contentFilterService;

        /// <summary>
        /// Initializes a new instance of the ContentFilterController.
        /// </summary>
        /// <param name="contentFilterService">Injected content filter service</param>
        public ContentFilterController(ContentFilterService contentFilterService)
        {
            _contentFilterService = contentFilterService;
        }

        /// <summary>
        /// POST: api/ContentFilter/validate
        /// Validates provided content against the list of banned words.
        /// </summary>
        /// <param name="content">The content to validate</param>
        /// <returns>Validation result indicating if the content is approved and any violations</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateContent([FromBody] string content)
        {
            // Check for null or empty content
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest(new { IsApproved = false, Message = "Content cannot be empty" });
            }

            // Validate the content
            var result = await _contentFilterService.ValidateContentAsync(content);

            // Return the validation result
            return Ok(result);
        }
    }
}