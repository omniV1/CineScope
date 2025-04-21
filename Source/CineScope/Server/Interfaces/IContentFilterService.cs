using CineScope.Server.Models;
using System.Threading.Tasks;

namespace CineScope.Server.Interfaces
{
    /// <summary>
    /// Interface for content filtering operations.
    /// </summary>
    public interface IContentFilterService
    {
        /// <summary>
        /// Validates content against the list of banned words and patterns.
        /// </summary>
        /// <param name="content">The text content to validate</param>
        /// <returns>A result object indicating if the content is approved and any violation details</returns>
        Task<ContentFilterResult> ValidateContentAsync(string content);

        /// <summary>
        /// Manually refreshes the banned words cache.
        /// </summary>
        Task RefreshCacheAsync();
    }
} 