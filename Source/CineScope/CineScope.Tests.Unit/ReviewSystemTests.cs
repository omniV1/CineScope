using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using CineScope.Client.Components.Reviews;
using CineScope.Client.Services.Auth;
using CineScope.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Tests for the review creation and management functionality.
    /// These tests verify requirements FR-3.1, FR-3.2, FR-3.4, and FR-3.6.
    /// </summary>
    public class ReviewSystemTests : TestContext
    {
        private readonly UserDto _mockUser = new UserDto
        {
            Id = "user123",
            Username = "testuser",
            Email = "testuser@example.com",
            Roles = new List<string> { "User" }
        };

        // Track snackbar calls without using Moq's Verify
        private List<(string Message, Severity Severity)> _snackbarCalls = new List<(string, Severity)>();
        private ISnackbar _snackbar;

        /// <summary>
        /// Sets up the test context with required services.
        /// </summary>
        public ReviewSystemTests()
        {
            // Register MudBlazor services required by our components
            Services.AddMudServices();

            // Set up JSInterop for MudBlazor components
            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudPopover.initialize", _ => true);
            JSInterop.SetupVoid("mudElementRef.saveMeasurements", _ => true);
            JSInterop.SetupVoid("mudPopover.connect", _ => true);

            // Register mock HttpClient
            Services.AddSingleton<HttpClient>(GetMockHttpClient());

            // Mock auth service
            var mockAuthService = new Mock<AuthService>(null, null);
            mockAuthService
                .Setup(s => s.GetCurrentUser())
                .ReturnsAsync(_mockUser);
            Services.AddSingleton(mockAuthService.Object);

            // THE SOLUTION: Use a smart approach that avoids all constructor and interface issues
            // ---------------------------------------------------------------------------------
            
            // First create a mock with a minimal setup - no need to mock every method, just intercept calls
            var mockSnackbar = new Mock<ISnackbar>();
            
            // Register our callback to capture calls for later verification
            // The key is using the proper overload of Add that doesn't have optional parameters in the expression
            mockSnackbar
                .Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Callback<string, Severity, Action<SnackbarOptions>, string>((message, severity, _, __) => 
                {
                    _snackbarCalls.Add((message, severity));
                });
                
            // Handle the required return value for the Add method - AVOIDING actual Snackbar creation
            // Instead of using .Returns(new Snackbar()) which fails due to constructor issues,
            // use .Returns(null) since we don't actually use the return value in our tests
            mockSnackbar
                .Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns((Snackbar)null);
                
            // Set required properties for the mock
            mockSnackbar.SetupGet(x => x.Configuration).Returns(new SnackbarConfiguration());
            mockSnackbar.SetupGet(x => x.ShownSnackbars).Returns(new List<Snackbar>());
            
            // Save the mock interface for use in tests
            _snackbar = mockSnackbar.Object;
            
            // Register the mock with the service provider
            Services.AddSingleton(_snackbar);
        }

        /// <summary>
        /// Tests that the CreateReview component allows users to input a rating and review text.
        /// Verifies FR-3.1: The system shall provide a "Create Review" page.
        /// Verifies FR-3.2: The system shall allow users to input movie title, rating, and review text.
        /// </summary>
        [Fact]
        public void CreateReview_ShouldAllowUserInputOfRatingAndText()
        {
            // Act - Render the CreateReview component with parameters
            var cut = RenderComponent<CreateReview>(parameters =>
            {
                parameters.Add(p => p.MovieId, "movie123");
                parameters.Add(p => p.CurrentUserId, "user123");
            });

            // Assert - Verify the review form elements exist
            Assert.Contains("Your Rating", cut.Markup);
            Assert.Contains("<div class=\"mud-rating", cut.Markup);
            Assert.Contains("<textarea", cut.Markup);
            Assert.Contains("Submit Review", cut.Markup);
        }

        /// <summary>
        /// Tests that the review submission is properly validated before being sent to the server.
        /// Verifies FR-3.3: The system shall validate the review input for required fields.
        /// </summary>
        [Fact]
        public void CreateReview_ShouldValidateRequiredFieldsBeforeSubmission()
        {
            // Act - Render the CreateReview component with parameters
            var cut = RenderComponent<CreateReview>(parameters =>
            {
                parameters.Add(p => p.MovieId, "movie123");
                parameters.Add(p => p.CurrentUserId, "user123");
            });

            // Find and click the submit button without entering any data
            var submitButton = cut.Find("button[variant='Filled']");
            submitButton.Click();

            // Assert - Verify the validation message is shown
            Assert.Contains("Review text is required", cut.Markup);

            // Verify the submit button is disabled due to validation failure
            Assert.Contains("disabled", submitButton.OuterHtml);
        }

        /// <summary>
        /// Tests that the review content is filtered for inappropriate content.
        /// Verifies FR-3.4: The system shall apply content filter to review text.
        /// </summary>
        [Fact]
        public async Task CreateReview_ShouldFilterInappropriateContent()
        {
            // Create a handler for the review submission event
            var reviewSubmitted = false;
            void SubmittedHandler(ReviewDto r)
            {
                reviewSubmitted = true;
            }

            // Create the event callback using the method delegate
            var submittedCallback = EventCallback.Factory.Create<ReviewDto>(this, (Action<ReviewDto>)SubmittedHandler);

            // Act - Render the CreateReview component with parameters
            var cut = RenderComponent<CreateReview>(parameters =>
            {
                parameters.Add(p => p.MovieId, "movie123");
                parameters.Add(p => p.CurrentUserId, "user123");
                parameters.Add(p => p.OnReviewSubmitted, submittedCallback);
            });

            // Set the rating
            var instance = cut.Instance;
            var field = typeof(CreateReview).GetField("ratingValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(instance, 4);

            // Set the review text with inappropriate content
            var textField = cut.Find("textarea");
            textField.Change("This is a review with the banned word offensive1 in it.");

            // Find and click the submit button
            var submitButton = cut.Find("button[variant='Filled']");
            submitButton.Click();

            // Allow the async operation to complete
            await Task.Delay(100);

            // Assert - Verify the content warning is shown
            Assert.Contains("Content Warning", cut.Markup);
            Assert.Contains("offensive1", cut.Markup);
        }

        /// <summary>
        /// Tests that successful review submission displays a confirmation.
        /// Verifies FR-3.6: The system shall display confirmation message upon successful creation.
        /// </summary>
        [Fact]
        public async Task CreateReview_ShouldDisplayConfirmationOnSuccessfulSubmission()
        {
            // Arrange - Set up tracking variable
            var reviewSubmitted = false;

            // Clear any previous snackbar calls
            _snackbarCalls.Clear();

            // Create a handler for the review submission event
            void SubmittedHandler(ReviewDto r)
            {
                reviewSubmitted = true;
            }

            // Create the event callback using the method delegate
            var submittedCallback = EventCallback.Factory.Create<ReviewDto>(this, (Action<ReviewDto>)SubmittedHandler);

            // Act - Render the CreateReview component
            var cut = RenderComponent<CreateReview>(parameters =>
            {
                parameters.Add(p => p.MovieId, "movie123");
                parameters.Add(p => p.CurrentUserId, "user123");
                parameters.Add(p => p.OnReviewSubmitted, submittedCallback);
            });

            // Set the rating
            var instance = cut.Instance;
            var field = typeof(CreateReview).GetField("ratingValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(instance, 4);

            // Set the review text with clean content
            var textField = cut.Find("textarea");
            textField.Change("This is a clean review with no inappropriate content.");

            // Find and click the submit button
            var submitButton = cut.Find("button[variant='Filled']");
            submitButton.Click();

            // Allow the async operation to complete
            await Task.Delay(100);

            // Assert - Verify the callback was invoked indicating successful submission
            Assert.True(reviewSubmitted);

            // Use our captured snackbar calls to verify the message
            Assert.NotEmpty(_snackbarCalls);
            var lastMessage = _snackbarCalls.Last();
            Assert.Contains("submitted successfully", lastMessage.Message);
            Assert.Equal(Severity.Success, lastMessage.Severity);
        }

        /// <summary>
        /// Tests that the system correctly displays a user's reviews.
        /// Related to review management functionality.
        /// </summary>
        [Fact]
        public void ReviewList_ShouldDisplayUserReviews()
        {
            // Arrange - Create sample review data
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = "rev1",
                    MovieId = "movie1",
                    UserId = "user123",
                    Username = "testuser",
                    Rating = 4,
                    Text = "Good movie",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new ReviewDto
                {
                    Id = "rev2",
                    MovieId = "movie2",
                    UserId = "user123",
                    Username = "testuser",
                    Rating = 5,
                    Text = "Excellent movie",
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };

            // Act - Render the ReviewList component
            var cut = RenderComponent<ReviewList>(parameters =>
            {
                parameters.Add(p => p.Reviews, reviews);
                parameters.Add(p => p.Title, "My Reviews");
                parameters.Add(p => p.ShowActions, true);
                parameters.Add(p => p.CurrentUserId, "user123");
            });

            // Assert - Verify the reviews are displayed
            Assert.Contains("Good movie", cut.Markup);
            Assert.Contains("Excellent movie", cut.Markup);
            Assert.Contains("testuser", cut.Markup);

            // Verify the rating components are displayed
            Assert.Contains("mud-rating", cut.Markup);

            // Verify the action buttons are displayed (Edit and Delete)
            Assert.Contains("Edit", cut.Markup);
            Assert.Contains("Delete", cut.Markup);
        }

        /// <summary>
        /// Tests that the filter functionality works correctly for reviews.
        /// </summary>
        [Fact]
        public void ReviewList_ShouldFilterReviewsByRating()
        {
            // Arrange - Create sample review data with different ratings
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = "rev1",
                    MovieId = "movie1",
                    UserId = "user123",
                    Username = "testuser",
                    Rating = 2, // Low rating
                    Text = "Not very good",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new ReviewDto
                {
                    Id = "rev2",
                    MovieId = "movie1",
                    UserId = "user456",
                    Username = "anotheruser",
                    Rating = 5, // High rating
                    Text = "Excellent movie",
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };

            // Act - Render the ReviewList component
            var cut = RenderComponent<ReviewList>(parameters =>
            {
                parameters.Add(p => p.Reviews, reviews);
                parameters.Add(p => p.Title, "All Reviews");
                parameters.Add(p => p.ShowActions, false);
            });

            // Assert - Verify both reviews are initially displayed
            Assert.Contains("Not very good", cut.Markup);
            Assert.Contains("Excellent movie", cut.Markup);

            // Find and click the filter menu
            var filterButton = cut.Find("button[aria-label='Filter']");
            filterButton.Click();

            // Find and click "Positive (4-5 ★)" filter option
            var filterOptions = cut.FindAll(".mud-list-item");
            var positiveFilter = filterOptions.FirstOrDefault(e => e.TextContent.Contains("Positive"));
            positiveFilter.Click();

            // Verify only high rating reviews are shown
            Assert.DoesNotContain("Not very good", cut.Markup); // Low rating should be filtered out
            Assert.Contains("Excellent movie", cut.Markup); // High rating should remain
        }

        /// <summary>
        /// Tests that the sorting functionality works correctly for reviews.
        /// </summary>
        [Fact]
        public void ReviewList_ShouldSortReviewsBySelectedOrder()
        {
            // Arrange - Create sample review data with different dates
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = "rev1",
                    MovieId = "movie1",
                    UserId = "user123",
                    Username = "testuser",
                    Rating = 4,
                    Text = "Newer review",
                    CreatedAt = DateTime.Now.AddDays(-1) // Newer
                },
                new ReviewDto
                {
                    Id = "rev2",
                    MovieId = "movie1",
                    UserId = "user456",
                    Username = "anotheruser",
                    Rating = 5,
                    Text = "Older review",
                    CreatedAt = DateTime.Now.AddDays(-30) // Older
                }
            };

            // Act - Render the ReviewList component
            var cut = RenderComponent<ReviewList>(parameters =>
            {
                parameters.Add(p => p.Reviews, reviews);
                parameters.Add(p => p.Title, "All Reviews");
                parameters.Add(p => p.ShowActions, false);
            });

            // By default, newest first sorting should be applied
            // The newer review should appear before the older one
            var reviewTexts = cut.FindAll(".mud-typography-body1")
                .Where(el => el.TextContent == "Newer review" || el.TextContent == "Older review")
                .Select(el => el.TextContent)
                .ToList();

            Assert.Equal(2, reviewTexts.Count);
            Assert.Equal("Newer review", reviewTexts[0]);
            Assert.Equal("Older review", reviewTexts[1]);

            // Find and click the sort menu
            var sortButton = cut.Find("button[aria-label='Sort']");
            sortButton.Click();

            // Find and click "Oldest First" sort option
            var sortOptions = cut.FindAll(".mud-list-item");
            var oldestFirstOption = sortOptions.FirstOrDefault(e => e.TextContent.Contains("Oldest First"));
            oldestFirstOption.Click();

            // Get the updated order of reviews
            reviewTexts = cut.FindAll(".mud-typography-body1")
                .Where(el => el.TextContent == "Newer review" || el.TextContent == "Older review")
                .Select(el => el.TextContent)
                .ToList();

            // Verify the order has changed
            Assert.Equal(2, reviewTexts.Count);
            Assert.Equal("Older review", reviewTexts[0]);
            Assert.Equal("Newer review", reviewTexts[1]);
        }

        /// <summary>
        /// Tests that a user can delete their own reviews.
        /// </summary>
        [Fact]
        public async Task ReviewList_ShouldAllowDeletingOwnReviews()
        {
            // Arrange - Create sample review data
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = "rev1",
                    MovieId = "movie1",
                    UserId = "user123", // Matching the current user ID
                    Username = "testuser",
                    Rating = 4,
                    Text = "A review to be deleted",
                    CreatedAt = DateTime.Now.AddDays(-5)
                }
            };

            // Track if delete was called
            bool deleteWasCalled = false;
            string deletedReviewId = null;

            // Create a handler for the delete event
            void DeleteHandler(string id)
            {
                deleteWasCalled = true;
                deletedReviewId = id;
            }

            // Create the event callback using the method delegate
            var deleteCallback = EventCallback.Factory.Create<string>(this, (Action<string>)DeleteHandler);

            // Register mock HttpClient for the delete operation
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://localhost/") };
            Services.AddSingleton(httpClient);

            // Act - Render the ReviewList component
            var cut = RenderComponent<ReviewList>(parameters =>
            {
                parameters.Add(p => p.Reviews, reviews);
                parameters.Add(p => p.Title, "My Reviews");
                parameters.Add(p => p.ShowActions, true);
                parameters.Add(p => p.CurrentUserId, "user123");
                parameters.Add(p => p.OnReviewDeleted, deleteCallback);
            });

            // Find and click the delete button
            var deleteButton = cut.Find("button.mud-button-text-error");
            deleteButton.Click();

            // Allow the async operation to complete
            await Task.Delay(100);

            // Assert - Verify the delete callback was called with the correct ID
            Assert.True(deleteWasCalled);
            Assert.Equal("rev1", deletedReviewId);
        }

        /// <summary>
        /// Creates a mock HttpClient for testing review-related components.
        /// </summary>
        private HttpClient GetMockHttpClient()
        {
            // Configure mock handler
            var handler = new Mock<HttpMessageHandler>();

            // Configure handler for content filter validation
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("api/ContentFilter/validate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        IsApproved = false,
                        ViolationWords = new List<string> { "offensive1" }
                    }), Encoding.UTF8, "application/json")
                });

            // Configure handler for review submission
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString().Contains("api/Review")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(JsonSerializer.Serialize(new ReviewDto
                    {
                        Id = "newrev",
                        MovieId = "movie123",
                        UserId = "user123",
                        Username = "testuser",
                        Rating = 4,
                        Text = "This is a clean review with no inappropriate content.",
                        CreatedAt = DateTime.Now
                    }), Encoding.UTF8, "application/json")
                });

            // Configure handler for loading clean content (not triggering content filter)
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("api/ContentFilter/validate") &&
                        req.Content != null &&
                        req.Content.ReadAsStringAsync().Result.Contains("clean")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        IsApproved = true,
                        ViolationWords = new List<string>()
                    }), Encoding.UTF8, "application/json")
                });

            return new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost/") };
        }
    }
}