using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineScope.Shared.DTOs
{
    public class BannedWordDto
    {
        public string Id { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public int Severity { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}