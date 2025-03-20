using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Shared.DTOs
{
    public class ModerationActionDto
    {
        public string ActionType { get; set; } = string.Empty; // "Approve", "Reject", "Modify"
        public string Reason { get; set; } = string.Empty;
        public string ModifiedContent { get; set; } = string.Empty;
    }
}
