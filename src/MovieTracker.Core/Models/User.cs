using Microsoft.AspNetCore.Identity; // email, password hash, username
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieTracker.Core.Models
{
    public class User : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
