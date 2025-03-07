using System;
using System.Collections.Generic;

namespace CineScope.Shared.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}