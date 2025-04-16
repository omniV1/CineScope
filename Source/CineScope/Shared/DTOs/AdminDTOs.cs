using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Shared.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalReviews { get; set; }
        public int FlaggedContent { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public Dictionary<string, long> CollectionStats { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "NewReview", "FlaggedReview", etc.
        public string Details { get; set; } = string.Empty;
    }

}
