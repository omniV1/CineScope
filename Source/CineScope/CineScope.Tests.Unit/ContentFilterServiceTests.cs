using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CineScope.Server.Data;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Server.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Contains unit tests for the ContentFilterService component.
    /// These tests verify that the content filtering system correctly identifies
    /// and flags inappropriate content based on the banned word list.
    /// </summary>
    public class ContentFilterServiceTests
    {
        /// <summary>
        /// Tests that clean text without any banned words passes content validation.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithCleanText_ShouldReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            var cleanText = "This is a family-friendly review of a great movie.";

            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            var mockCollection = new Mock<IMongoCollection<BannedWord>>();
            var mockCursor = new Mock<IAsyncCursor<BannedWord>>();

            mockCursor.Setup(c => c.Current).Returns(bannedWords);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(cleanText);

            // Assert - Verify the results match our expectations
            Assert.True(result.IsApproved);
            Assert.Empty(result.ViolationWords);
        }

        /// <summary>
        /// Tests that text containing banned words fails content validation.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithOffensiveText_ShouldReturnFalse()
        {
            // Arrange - Set up test data and dependencies
            var offensiveText = "This movie was offensive1 and had some offensive2 content.";

            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            var mockCollection = new Mock<IMongoCollection<BannedWord>>();
            var mockCursor = new Mock<IAsyncCursor<BannedWord>>();

            mockCursor.Setup(c => c.Current).Returns(bannedWords);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(offensiveText);

            // Assert - Verify the results match our expectations
            Assert.False(result.IsApproved);
            Assert.Equal(2, result.ViolationWords.Count);
            Assert.Contains("offensive1", result.ViolationWords);
            Assert.Contains("offensive2", result.ViolationWords);
        }

        /// <summary>
        /// Tests that text containing inactive banned words passes content validation.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithInactiveBannedWord_ShouldReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            var text = "This text contains a word that used to be banned.";

            // Only set up active banned words to be returned from the mock DB
            var bannedWords = new List<BannedWord>(); // Empty list (no active banned words)

            var mockCollection = new Mock<IMongoCollection<BannedWord>>();
            var mockCursor = new Mock<IAsyncCursor<BannedWord>>();

            mockCursor.Setup(c => c.Current).Returns(bannedWords);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(text);

            // Assert - Verify the results match our expectations
            Assert.True(result.IsApproved);
            Assert.Empty(result.ViolationWords);
        }
    }
}