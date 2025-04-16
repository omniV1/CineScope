using CineScope.Server.Models;
using CineScope.Shared.DTOs;
using System.Linq;

namespace CineScope.Server.Extensions
{
    public static class MappingExtensions
    {
        public static BannedWordDto ToDto(this BannedWord model)
        {
            return new BannedWordDto
            {
                Id = model.Id,
                Word = model.Word,
                Severity = model.Severity,
                Category = model.Category,
                IsActive = model.IsActive
            };
        }

        public static BannedWord ToModel(this BannedWordDto dto)
        {
            return new BannedWord
            {
                Id = dto.Id,
                Word = dto.Word,
                Severity = dto.Severity,
                Category = dto.Category,
                IsActive = dto.IsActive
            };
        }

        public static UserAdminDto ToAdminDto(this User model, int reviewCount = 0)
        {
            return new UserAdminDto
            {
                Id = model.Id,
                Username = model.Username,
                Email = model.Email,
                ProfilePictureUrl = model.ProfilePictureUrl,
                Roles = model.Roles,
                JoinDate = model.CreatedAt,
                ReviewCount = reviewCount,
                LastLogin = model.LastLogin,
                Status = model.IsLocked ? "Suspended" : "Active"
            };
        }

        public static ReviewModerationDto ToModerationDto(this Review model, string username = "", string movieTitle = "")
        {
            return new ReviewModerationDto
            {
                Id = model.Id,
                UserId = model.UserId,
                Username = username,
                MovieId = model.MovieId,
                MovieTitle = movieTitle,
                Rating = model.Rating,
                Text = model.Text,
                CreatedAt = model.CreatedAt,
                FlaggedWords = model.FlaggedWords?.ToList() ?? new List<string>(),
                ModerationStatus = model.IsApproved ? "Approved" : "Pending"
            };
        }
    }
}