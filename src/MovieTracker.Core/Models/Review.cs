using System;
using System.Collections.Generic;
using System.Text;

namespace MovieTracker.Core.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int MovieId { get; set; }

        public int Rating { get; set; }

        public string? Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
