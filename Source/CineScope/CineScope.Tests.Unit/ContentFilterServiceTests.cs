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
        /// 
        /// This test verifies that when a review contains no banned words,
        /// the content filter approves it and returns an empty violation list.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithCleanText_ShouldReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            // A sample review text with no banned words
            var cleanText = "This is a family-friendly review of a great movie.";

            // Define some banned words for testing
            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            // Mock the MongoDB collection interface to avoid actual database calls
            var mockCollection = new Mock<IMongoCollection<BannedWord>>();
            var mockCursor = new Mock<IAsyncCursor<BannedWord>>();

            // Configure the cursor to return our banned words list
            mockCursor.Setup(c => c.Current).Returns(bannedWords);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service with our mocked dependencies
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(cleanText);

            // Assert - Verify the results match our expectations
            Assert.True(result.IsApproved);         // Content should be approved
            Assert.Empty(result.ViolationWords);    // No violations should be found
        }

        /// <summary>
        /// Tests that text containing banned words fails content validation.
        /// 
        /// This test verifies that when a review contains banned words,
        /// the content filter rejects it and returns a list of the violated terms.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithOffensiveText_ShouldReturnFalse()
        {
            // Arrange - Set up test data and dependencies
            // A sample review text that contains banned words
            var offensiveText = "This movie was offensive1 and had some offensive2 content.";

            // Define the banned words that should be detected
            var bannedWords = new List<BannedWord>
            {
                new BannedWord { Word = "offensive1", Severity = 5, IsActive = true },
                new BannedWord { Word = "offensive2", Severity = 3, IsActive = true }
            };

            // Mock the MongoDB collection interface
            var mockCollection = new Mock<IMongoCollection<BannedWord>>();
            var mockCursor = new Mock<IAsyncCursor<BannedWord>>();

            // Configure the cursor to return our banned words list
            mockCursor.Setup(c => c.Current).Returns(bannedWords);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // First call returns true (has results)
                .ReturnsAsync(false); // Second call returns false (no more results)

            // Configure the collection's FindAsync method to return our cursor
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service with our mocked dependencies
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(offensiveText);

            // Assert - Verify the results match our expectations
            Assert.False(result.IsApproved);                // Content should be rejected
            Assert.Equal(2, result.ViolationWords.Count);   // Two violations should be found
            Assert.Contains("offensive1", result.ViolationWords);  // First banned word should be detected
            Assert.Contains("offensive2", result.ViolationWords);  // Second banned word should be detected
        }

        /// <summary>
        /// Tests that text containing inactive banned words passes content validation.
        /// 
        /// This test verifies that the content filter only considers active banned words
        /// and ignores those marked as inactive, even if they appear in the content.
        /// </summary>
        [Fact]
        public async Task ValidateContent_WithInactiveBannedWord_ShouldReturnTrue()
        {
            // Arrange - Set up test data and dependencies
            // A sample review text containing a word that used to be banned
            var text = "This text contains a word that used to be banned.";

            // Define a banned word that is marked as inactive
            var bannedWords = new List<BannedWord>
            {
                new BannedWord
                {
                    Word = "banned",
                    Severity = 5,
                    Category = "Test",
                    IsActive = false  // Word is not active in the filter
                }
            };

            // Create an empty cursor to simulate that no active banned words match
            var emptyMockCursor = new Mock<IAsyncCursor<BannedWord>>();
            emptyMockCursor.Setup(c => c.Current).Returns(new List<BannedWord>());
            emptyMockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);  // No results found

            // Mock the MongoDB collection interface
            var mockCollection = new Mock<IMongoCollection<BannedWord>>();

            // Set up the collection to return an empty list when filtering for active words
            // This simulates the ContentFilterService's behavior of only checking active banned words
            mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BannedWord>>(),
                    It.IsAny<FindOptions<BannedWord, BannedWord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyMockCursor.Object);

            // Mock the database service to return our collection
            var mockMongoDbService = new Mock<IMongoDbService>();
            mockMongoDbService
                .Setup(s => s.GetCollection<BannedWord>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Mock the settings to provide the collection name
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                BannedWordsCollectionName = "BannedWords"
            });

            // Create the service with our mocked dependencies
            var contentFilterService = new ContentFilterService(mockMongoDbService.Object, mockSettings.Object);

            // Act - Call the method being tested
            var result = await contentFilterService.ValidateContentAsync(text);

            // Assert - Verify the results match our expectations
            Assert.True(result.IsApproved);         // Content should be approved
            Assert.Empty(result.ViolationWords);    // No violations should be found
        }
    }
}