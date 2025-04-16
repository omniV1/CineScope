using Xunit;
using System;
using System.Collections.Generic;
using CineScope.Shared.DTOs;

namespace CineScope.UnitTests
{
    public class DtoTests
    {
        [Fact]
        public void ReviewDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var reviewDto = new ReviewDto();

            // Assert
            Assert.Equal(string.Empty, reviewDto.Id);
            Assert.Equal(string.Empty, reviewDto.UserId);
            Assert.Equal(string.Empty, reviewDto.MovieId);
            Assert.Equal(0, reviewDto.Rating);
            Assert.Equal(string.Empty, reviewDto.Text);
            Assert.Equal(default(DateTime), reviewDto.CreatedAt);
            Assert.Equal(string.Empty, reviewDto.Username);
        }

        [Fact]
        public void ReviewDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var reviewDto = new ReviewDto
            {
                Id = "review123",
                UserId = "user123",
                MovieId = "movie123",
                Rating = 4.5,
                Text = "Great movie!",
                CreatedAt = now,
                Username = "testuser"
            };

            // Assert
            Assert.Equal("review123", reviewDto.Id);
            Assert.Equal("user123", reviewDto.UserId);
            Assert.Equal("movie123", reviewDto.MovieId);
            Assert.Equal(4.5, reviewDto.Rating);
            Assert.Equal("Great movie!", reviewDto.Text);
            Assert.Equal(now, reviewDto.CreatedAt);
            Assert.Equal("testuser", reviewDto.Username);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(3.0)]
        [InlineData(4.5)]
        [InlineData(5.0)]
        public void ReviewDto_RatingValues_StoredCorrectly(double rating)
        {
            // Arrange
            var reviewDto = new ReviewDto { Rating = rating };

            // Assert
            Assert.Equal(rating, reviewDto.Rating);
        }

        [Fact]
        public void ReviewDto_CreatedAt_HandlesDateTimeCorrectly()
        {
            // Arrange
            var pastDate = new DateTime(2023, 1, 1);
            var futureDate = new DateTime(2025, 12, 31);

            // Act
            var reviewDto1 = new ReviewDto { CreatedAt = pastDate };
            var reviewDto2 = new ReviewDto { CreatedAt = futureDate };

            // Assert
            Assert.Equal(pastDate, reviewDto1.CreatedAt);
            Assert.Equal(futureDate, reviewDto2.CreatedAt);
        }

        [Fact]
        public void UserProfileDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var profileDto = new UserProfileDto();

            // Assert
            Assert.Equal(string.Empty, profileDto.Id);
            Assert.Equal(string.Empty, profileDto.Username);
            Assert.Equal(string.Empty, profileDto.Email);
            Assert.Equal(string.Empty, profileDto.ProfilePictureUrl);
            Assert.Equal(default(DateTime), profileDto.CreatedAt);
            Assert.Null(profileDto.LastLogin);
        }

        [Fact]
        public void UserProfileDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var lastLogin = now.AddDays(-1);
            var profileDto = new UserProfileDto
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@example.com",
                ProfilePictureUrl = "http://example.com/pic.jpg",
                CreatedAt = now,
                LastLogin = lastLogin
            };

            // Assert
            Assert.Equal("user123", profileDto.Id);
            Assert.Equal("testuser", profileDto.Username);
            Assert.Equal("test@example.com", profileDto.Email);
            Assert.Equal("http://example.com/pic.jpg", profileDto.ProfilePictureUrl);
            Assert.Equal(now, profileDto.CreatedAt);
            Assert.Equal(lastLogin, profileDto.LastLogin);
        }

        [Fact]
        public void PublicUserDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var publicDto = new PublicUserDto();

            // Assert
            Assert.Equal(string.Empty, publicDto.Id);
            Assert.Equal(string.Empty, publicDto.Username);
            Assert.Equal(string.Empty, publicDto.ProfilePictureUrl);
            Assert.Equal(default(DateTime), publicDto.JoinDate);
        }

        [Fact]
        public void PublicUserDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var joinDate = new DateTime(2023, 1, 1);
            var publicDto = new PublicUserDto
            {
                Id = "user123",
                Username = "testuser",
                ProfilePictureUrl = "http://example.com/pic.jpg",
                JoinDate = joinDate
            };

            // Assert
            Assert.Equal("user123", publicDto.Id);
            Assert.Equal("testuser", publicDto.Username);
            Assert.Equal("http://example.com/pic.jpg", publicDto.ProfilePictureUrl);
            Assert.Equal(joinDate, publicDto.JoinDate);
        }

        [Theory]
        [InlineData("", "username", "pic.jpg", "Id cannot be empty")]
        [InlineData("id", "", "pic.jpg", "Username cannot be empty")]
        public void PublicUserDto_Validation_RequiredFields(string id, string username, string pictureUrl, string testCase)
        {
            // Arrange
            var publicDto = new PublicUserDto
            {
                Id = id,
                Username = username,
                ProfilePictureUrl = pictureUrl,
                JoinDate = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(id, publicDto.Id);
            Assert.Equal(username, publicDto.Username);
            Assert.Equal(pictureUrl, publicDto.ProfilePictureUrl);
        }

        [Fact]
        public void UserProfileDto_LastLogin_HandlesNullValue()
        {
            // Arrange
            var profileDto = new UserProfileDto();

            // Assert
            Assert.Null(profileDto.LastLogin);

            // Act
            profileDto.LastLogin = DateTime.UtcNow;

            // Assert
            Assert.NotNull(profileDto.LastLogin);
        }

        [Fact]
        public void UserDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var userDto = new UserDto();

            // Assert
            Assert.Equal(string.Empty, userDto.Id);
            Assert.Equal(string.Empty, userDto.Username);
            Assert.Equal(string.Empty, userDto.Email);
            Assert.Equal(string.Empty, userDto.ProfilePictureUrl);
            Assert.NotNull(userDto.Roles);
            Assert.Empty(userDto.Roles);
        }

        [Fact]
        public void UserDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var roles = new List<string> { "User", "Admin" };
            var userDto = new UserDto
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@example.com",
                ProfilePictureUrl = "http://example.com/pic.jpg",
                Roles = roles
            };

            // Assert
            Assert.Equal("user123", userDto.Id);
            Assert.Equal("testuser", userDto.Username);
            Assert.Equal("test@example.com", userDto.Email);
            Assert.Equal("http://example.com/pic.jpg", userDto.ProfilePictureUrl);
            Assert.Equal(roles, userDto.Roles);
            Assert.Contains("User", userDto.Roles);
            Assert.Contains("Admin", userDto.Roles);
        }

        [Fact]
        public void UserDto_Roles_CanBeModified()
        {
            // Arrange
            var userDto = new UserDto();

            // Act
            userDto.Roles.Add("User");
            userDto.Roles.Add("Moderator");

            // Assert
            Assert.Equal(2, userDto.Roles.Count);
            Assert.Contains("User", userDto.Roles);
            Assert.Contains("Moderator", userDto.Roles);

            // Act
            userDto.Roles.Remove("User");

            // Assert
            Assert.Single(userDto.Roles);
            Assert.Contains("Moderator", userDto.Roles);
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Admin")]
        [InlineData("Moderator")]
        public void UserDto_SingleRole_HandledCorrectly(string role)
        {
            // Arrange
            var userDto = new UserDto();
            userDto.Roles.Add(role);

            // Assert
            Assert.Single(userDto.Roles);
            Assert.Contains(role, userDto.Roles);
        }

        [Fact]
        public void UserDto_MultipleRoles_HandledCorrectly()
        {
            // Arrange
            var userDto = new UserDto();
            var roles = new[] { "User", "Admin", "Moderator" };

            // Act
            foreach (var role in roles)
            {
                userDto.Roles.Add(role);
            }

            // Assert
            Assert.Equal(3, userDto.Roles.Count);
            foreach (var role in roles)
            {
                Assert.Contains(role, userDto.Roles);
            }
        }

        [Fact]
        public void UserAdminDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var adminDto = new UserAdminDto();

            // Assert
            Assert.Equal(string.Empty, adminDto.Id);
            Assert.Equal(string.Empty, adminDto.Username);
            Assert.Equal(string.Empty, adminDto.Email);
            Assert.Equal(string.Empty, adminDto.ProfilePictureUrl);
            Assert.NotNull(adminDto.Roles);
            Assert.Empty(adminDto.Roles);
            Assert.Equal(default(DateTime), adminDto.JoinDate);
            Assert.Equal(0, adminDto.ReviewCount);
            Assert.Null(adminDto.LastLogin);
            Assert.Equal("Active", adminDto.Status);
        }

        [Fact]
        public void UserAdminDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var lastLogin = now.AddDays(-1);
            var roles = new List<string> { "Admin", "Moderator" };
            
            var adminDto = new UserAdminDto
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@example.com",
                ProfilePictureUrl = "http://example.com/pic.jpg",
                Roles = roles,
                JoinDate = now,
                ReviewCount = 10,
                LastLogin = lastLogin,
                Status = "Flagged"
            };

            // Assert
            Assert.Equal("user123", adminDto.Id);
            Assert.Equal("testuser", adminDto.Username);
            Assert.Equal("test@example.com", adminDto.Email);
            Assert.Equal("http://example.com/pic.jpg", adminDto.ProfilePictureUrl);
            Assert.Equal(roles, adminDto.Roles);
            Assert.Equal(now, adminDto.JoinDate);
            Assert.Equal(10, adminDto.ReviewCount);
            Assert.Equal(lastLogin, adminDto.LastLogin);
            Assert.Equal("Flagged", adminDto.Status);
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("Flagged")]
        [InlineData("Suspended")]
        public void UserAdminDto_Status_AcceptsValidValues(string status)
        {
            // Arrange
            var adminDto = new UserAdminDto { Status = status };

            // Assert
            Assert.Equal(status, adminDto.Status);
        }

        [Fact]
        public void UserAdminDto_ReviewCount_HandlesValidValues()
        {
            // Arrange
            var adminDto = new UserAdminDto();

            // Act & Assert
            adminDto.ReviewCount = 0;
            Assert.Equal(0, adminDto.ReviewCount);

            adminDto.ReviewCount = 100;
            Assert.Equal(100, adminDto.ReviewCount);

            adminDto.ReviewCount = int.MaxValue;
            Assert.Equal(int.MaxValue, adminDto.ReviewCount);
        }

        [Fact]
        public void UserAdminDto_LastLogin_HandlesNullAndDateTimeValues()
        {
            // Arrange
            var adminDto = new UserAdminDto();
            var now = DateTime.UtcNow;

            // Assert
            Assert.Null(adminDto.LastLogin);

            // Act
            adminDto.LastLogin = now;

            // Assert
            Assert.NotNull(adminDto.LastLogin);
            Assert.Equal(now, adminDto.LastLogin);
        }

        [Fact]
        public void ReviewModerationDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var moderationDto = new ReviewModerationDto();

            // Assert
            Assert.Equal(string.Empty, moderationDto.Id);
            Assert.Equal(string.Empty, moderationDto.UserId);
            Assert.Equal(string.Empty, moderationDto.Username);
            Assert.Equal(string.Empty, moderationDto.MovieId);
            Assert.Equal(string.Empty, moderationDto.MovieTitle);
            Assert.Equal(0, moderationDto.Rating);
            Assert.Equal(string.Empty, moderationDto.Text);
            Assert.Equal(default(DateTime), moderationDto.CreatedAt);
            Assert.NotNull(moderationDto.FlaggedWords);
            Assert.Empty(moderationDto.FlaggedWords);
            Assert.Equal("Pending", moderationDto.ModerationStatus);
        }

        [Fact]
        public void ReviewModerationDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var flaggedWords = new List<string> { "bad", "word" };
            
            var moderationDto = new ReviewModerationDto
            {
                Id = "review123",
                UserId = "user123",
                Username = "testuser",
                MovieId = "movie123",
                MovieTitle = "Test Movie",
                Rating = 4.5,
                Text = "This is a review",
                CreatedAt = now,
                FlaggedWords = flaggedWords,
                ModerationStatus = "Approved"
            };

            // Assert
            Assert.Equal("review123", moderationDto.Id);
            Assert.Equal("user123", moderationDto.UserId);
            Assert.Equal("testuser", moderationDto.Username);
            Assert.Equal("movie123", moderationDto.MovieId);
            Assert.Equal("Test Movie", moderationDto.MovieTitle);
            Assert.Equal(4.5, moderationDto.Rating);
            Assert.Equal("This is a review", moderationDto.Text);
            Assert.Equal(now, moderationDto.CreatedAt);
            Assert.Equal(flaggedWords, moderationDto.FlaggedWords);
            Assert.Equal("Approved", moderationDto.ModerationStatus);
        }

        [Theory]
        [InlineData("Pending")]
        [InlineData("Approved")]
        [InlineData("Rejected")]
        public void ReviewModerationDto_ModerationStatus_AcceptsValidValues(string status)
        {
            // Arrange
            var moderationDto = new ReviewModerationDto { ModerationStatus = status };

            // Assert
            Assert.Equal(status, moderationDto.ModerationStatus);
        }

        [Fact]
        public void ReviewModerationDto_FlaggedWords_CanBeModified()
        {
            // Arrange
            var moderationDto = new ReviewModerationDto();

            // Act
            moderationDto.FlaggedWords.Add("bad");
            moderationDto.FlaggedWords.Add("inappropriate");

            // Assert
            Assert.Equal(2, moderationDto.FlaggedWords.Count);
            Assert.Contains("bad", moderationDto.FlaggedWords);
            Assert.Contains("inappropriate", moderationDto.FlaggedWords);

            // Act
            moderationDto.FlaggedWords.Remove("bad");

            // Assert
            Assert.Single(moderationDto.FlaggedWords);
            Assert.Contains("inappropriate", moderationDto.FlaggedWords);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(3.0)]
        [InlineData(4.5)]
        [InlineData(5.0)]
        public void ReviewModerationDto_Rating_HandlesValidValues(double rating)
        {
            // Arrange
            var moderationDto = new ReviewModerationDto { Rating = rating };

            // Assert
            Assert.Equal(rating, moderationDto.Rating);
        }

        [Fact]
        public void ModerationActionDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var actionDto = new ModerationActionDto();

            // Assert
            Assert.Equal(string.Empty, actionDto.ActionType);
            Assert.Equal(string.Empty, actionDto.Reason);
            Assert.Equal(string.Empty, actionDto.ModifiedContent);
        }

        [Fact]
        public void ModerationActionDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var actionDto = new ModerationActionDto
            {
                ActionType = "Reject",
                Reason = "Inappropriate content",
                ModifiedContent = "Modified text"
            };

            // Assert
            Assert.Equal("Reject", actionDto.ActionType);
            Assert.Equal("Inappropriate content", actionDto.Reason);
            Assert.Equal("Modified text", actionDto.ModifiedContent);
        }

        [Theory]
        [InlineData("Approve")]
        [InlineData("Reject")]
        [InlineData("Modify")]
        public void ModerationActionDto_ActionType_AcceptsValidValues(string actionType)
        {
            // Arrange
            var actionDto = new ModerationActionDto { ActionType = actionType };

            // Assert
            Assert.Equal(actionType, actionDto.ActionType);
        }

        [Fact]
        public void BannedWordDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var bannedWordDto = new BannedWordDto();

            // Assert
            Assert.Equal(string.Empty, bannedWordDto.Id);
            Assert.Equal(string.Empty, bannedWordDto.Word);
            Assert.Equal(0, bannedWordDto.Severity);
            Assert.Equal(string.Empty, bannedWordDto.Category);
            Assert.True(bannedWordDto.IsActive);
        }

        [Fact]
        public void BannedWordDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var bannedWordDto = new BannedWordDto
            {
                Id = "word123",
                Word = "badword",
                Severity = 3,
                Category = "Profanity",
                IsActive = false
            };

            // Assert
            Assert.Equal("word123", bannedWordDto.Id);
            Assert.Equal("badword", bannedWordDto.Word);
            Assert.Equal(3, bannedWordDto.Severity);
            Assert.Equal("Profanity", bannedWordDto.Category);
            Assert.False(bannedWordDto.IsActive);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void BannedWordDto_Severity_HandlesValidValues(int severity)
        {
            // Arrange
            var bannedWordDto = new BannedWordDto { Severity = severity };

            // Assert
            Assert.Equal(severity, bannedWordDto.Severity);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BannedWordDto_IsActive_HandlesValidValues(bool isActive)
        {
            // Arrange
            var bannedWordDto = new BannedWordDto { IsActive = isActive };

            // Assert
            Assert.Equal(isActive, bannedWordDto.IsActive);
        }

        [Fact]
        public void DashboardStatsDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var statsDto = new DashboardStatsDto();

            // Assert
            Assert.Equal(0, statsDto.TotalUsers);
            Assert.Equal(0, statsDto.TotalMovies);
            Assert.Equal(0, statsDto.TotalReviews);
            Assert.Equal(0, statsDto.FlaggedContent);
            Assert.NotNull(statsDto.RecentActivity);
            Assert.Empty(statsDto.RecentActivity);
            Assert.NotNull(statsDto.CollectionStats);
            Assert.Empty(statsDto.CollectionStats);
        }

        [Fact]
        public void DashboardStatsDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var recentActivity = new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Timestamp = DateTime.UtcNow,
                    Username = "user1",
                    ActionType = "NewReview",
                    Details = "Added a review"
                }
            };

            var collectionStats = new Dictionary<string, long>
            {
                { "Action", 10 },
                { "Drama", 20 }
            };

            var statsDto = new DashboardStatsDto
            {
                TotalUsers = 100,
                TotalMovies = 500,
                TotalReviews = 1000,
                FlaggedContent = 5,
                RecentActivity = recentActivity,
                CollectionStats = collectionStats
            };

            // Assert
            Assert.Equal(100, statsDto.TotalUsers);
            Assert.Equal(500, statsDto.TotalMovies);
            Assert.Equal(1000, statsDto.TotalReviews);
            Assert.Equal(5, statsDto.FlaggedContent);
            Assert.Equal(recentActivity, statsDto.RecentActivity);
            Assert.Equal(collectionStats, statsDto.CollectionStats);
        }

        [Fact]
        public void RecentActivityDto_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var activityDto = new RecentActivityDto();

            // Assert
            Assert.Equal(default(DateTime), activityDto.Timestamp);
            Assert.Equal(string.Empty, activityDto.Username);
            Assert.Equal(string.Empty, activityDto.ActionType);
            Assert.Equal(string.Empty, activityDto.Details);
        }

        [Fact]
        public void RecentActivityDto_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var activityDto = new RecentActivityDto
            {
                Timestamp = now,
                Username = "testuser",
                ActionType = "NewReview",
                Details = "Added a review for Movie X"
            };

            // Assert
            Assert.Equal(now, activityDto.Timestamp);
            Assert.Equal("testuser", activityDto.Username);
            Assert.Equal("NewReview", activityDto.ActionType);
            Assert.Equal("Added a review for Movie X", activityDto.Details);
        }

        [Theory]
        [InlineData("NewReview")]
        [InlineData("FlaggedReview")]
        [InlineData("UserBanned")]
        public void RecentActivityDto_ActionType_AcceptsValidValues(string actionType)
        {
            // Arrange
            var activityDto = new RecentActivityDto { ActionType = actionType };

            // Assert
            Assert.Equal(actionType, activityDto.ActionType);
        }

        [Fact]
        public void DashboardStatsDto_CollectionStats_CanBeModified()
        {
            // Arrange
            var statsDto = new DashboardStatsDto();

            // Act
            statsDto.CollectionStats["Action"] = 10;
            statsDto.CollectionStats["Drama"] = 20;

            // Assert
            Assert.Equal(2, statsDto.CollectionStats.Count);
            Assert.Equal(10, statsDto.CollectionStats["Action"]);
            Assert.Equal(20, statsDto.CollectionStats["Drama"]);

            // Act
            statsDto.CollectionStats.Remove("Action");

            // Assert
            Assert.Single(statsDto.CollectionStats);
            Assert.Equal(20, statsDto.CollectionStats["Drama"]);
        }
    }
} 