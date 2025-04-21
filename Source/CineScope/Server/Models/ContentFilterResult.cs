using System.Collections.Generic;

namespace CineScope.Server.Models
{
    /// <summary>
    /// Represents the result of a content filter validation.
    /// Contains approval status and details of any violations.
    /// Enhanced with severity scoring for graduated responses.
    /// </summary>
    public class ContentFilterResult
    {
        /// <summary>
        /// Indicates whether the content is approved (true) or rejected (false).
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// List of banned words found in the content.
        /// Empty if no violations are found.
        /// </summary>
        public List<string> ViolationWords { get; set; } = new();

        /// <summary>
        /// Cumulative severity score of all violations.
        /// Can be used for graduated responses (warning vs. rejection).
        /// </summary>
        public int SeverityScore { get; set; } = 0;

        /// <summary>
        /// Gets a user-friendly message explaining the reason for rejection.
        /// </summary>
        /// <returns>A message suitable for displaying to users</returns>
        public string GetUserFriendlyMessage()
        {
            if (IsApproved)
            {
                return "Content approved";
            }

            // Customize messages based on severity
            if (SeverityScore >= 10)
            {
                return "Your content contains highly inappropriate language and has been rejected.";
            }
            else if (SeverityScore >= 5)
            {
                return "Your content contains inappropriate language. Please revise before submitting.";
            }
            else
            {
                return "Your content may contain inappropriate language. Please review before submitting.";
            }
        }
    }
} 