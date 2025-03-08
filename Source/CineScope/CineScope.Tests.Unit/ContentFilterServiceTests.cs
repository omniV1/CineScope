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
    public class ContentFilterServiceTests
    {
        [Fact]
        public async Task ValidateContent_WithCleanText_ShouldReturnTrue()
        {
            // Arrange
            var cleanText = "This is a family-friendly review of a great movie.";

            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            // Mock banned words collection
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

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service to test
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await contentFilterService.ValidateContentAsync(cleanText);

            // Assert
            Assert.True(result.IsApproved);
            Assert.Empty(result.ViolationWords);
        }

        [Fact]
        public async Task ValidateContent_WithOffensiveText_ShouldReturnFalse()
        {
            // Arrange
            var offensiveText = "This movie was offensive1 and had some offensive2 content.";

            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            // Mock banned words collection
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

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service to test
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await contentFilterService.ValidateContentAsync(offensiveText);

            // Assert
            Assert.False(result.IsApproved);
            Assert.Equal(2, result.ViolationWords.Count);
            Assert.Contains("offensive1", result.ViolationWords);
            Assert.Contains("offensive2", result.ViolationWords);
        }

        [Fact]
        public async Task ValidateContent_WithInactiveBannedWord_ShouldReturnTrue()
        {
            // Renamed this test method to avoid the duplicate name error
            // Arrange
            var text = "This text contains a word that used to be banned.";

            // Set up a disabled banned word - removed AddedAt and UpdatedAt properties
            var bannedWords = new List<BannedWord>
            {
                new BannedWord
                {
                    Word = "banned",
                    Severity = 5,
                    Category = "Test",
                    IsActive = false
                }
            };

            // Create an empty cursor for active words query
            var emptyMockCursor = new Mock<IAsyncCursor<BannedWord>>();
            emptyMockCursor.Setup(c => c.Current).Returns(new List<BannedWord>());
            emptyMockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Mock banned words collection
            var mockCollection = new Mock<IMongoCollection<BannedWord>>();

            // Setup to return empty list when filtering for active words
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyMockCursor.Object);

            // Mock database service
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock settings
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service to test
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act
            var result = await contentFilterService.ValidateContentAsync(text);

            // Assert
            Assert.True(result.IsApproved);
            Assert.Empty(result.ViolationWords);
        }
    }
}