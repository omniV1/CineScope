using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Shared.DTOs
{
    public class ReviewModerationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string MovieId { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> FlaggedWords { get; set; } = new();
        public string ModerationStatus { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
    }

}
