using System;

namespace CineScope.Shared.DTOs
{
    public class ReviewDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MovieId { get; set; }
        public double Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; }
    }
}