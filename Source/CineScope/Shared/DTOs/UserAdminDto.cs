using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Shared.DTOs
{
    public class UserAdminDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime JoinDate { get; set; }
        public int ReviewCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Status { get; set; } = "Active"; // "Active", "Flagged", "Suspended"
    }
}
