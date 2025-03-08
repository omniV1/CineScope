using System;

namespace CineScope.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for review information.
    /// Used to transfer review data between the server and client.
    /// </summary>
    public class ReviewDto
    {
        /// <summary>
        /// Unique identifier for the review.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the user who created this review.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the movie being reviewed.
        /// </summary>
        public string MovieId { get; set; } = string.Empty;

        /// <summary>
        /// Numerical rating given by the user, typically on a scale (e.g., 1-5 stars).
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// The user's written review text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp indicating when the review was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The username of the review author.
        /// Included for display purposes.
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}