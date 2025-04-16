using Xunit;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;

namespace CineScope.UnitTests
{
    public class AuthTests
    {
        [Fact]
        public void LoginRequest_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var loginRequest = new LoginRequest();

            // Assert
            Assert.Equal(string.Empty, loginRequest.Username);
            Assert.Equal(string.Empty, loginRequest.Password);
            Assert.False(loginRequest.RememberMe);
        }

        [Fact]
        public void LoginRequest_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "testpass",
                RememberMe = true
            };

            // Assert
            Assert.Equal("testuser", loginRequest.Username);
            Assert.Equal("testpass", loginRequest.Password);
            Assert.True(loginRequest.RememberMe);
        }

        [Theory]
        [InlineData("", "password", false, "Username")]
        [InlineData("username", "", false, "Password")]
        public void LoginRequest_WhenValidated_RequiredFieldsEnforced(string username, string password, bool rememberMe, string expectedError)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password,
                RememberMe = rememberMe
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(loginRequest, new ValidationContext(loginRequest), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains(expectedError));
        }

        [Fact]
        public void RegisterRequest_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var registerRequest = new RegisterRequest();

            // Assert
            Assert.Equal(string.Empty, registerRequest.Username);
            Assert.Equal(string.Empty, registerRequest.Email);
            Assert.Equal(string.Empty, registerRequest.Password);
            Assert.Equal(string.Empty, registerRequest.ConfirmPassword);
        }

        [Fact]
        public void RegisterRequest_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Assert
            Assert.Equal("testuser", registerRequest.Username);
            Assert.Equal("test@example.com", registerRequest.Email);
            Assert.Equal("password123", registerRequest.Password);
            Assert.Equal("password123", registerRequest.ConfirmPassword);
        }

        [Theory]
        [InlineData("ab", "test@example.com", "password123", "password123", "Username")] // Username too short
        [InlineData("", "test@example.com", "password123", "password123", "Username")] // Username required
        [InlineData("user", "", "password123", "password123", "Email")] // Email required
        [InlineData("user", "invalid-email", "password123", "password123", "Email")] // Invalid email
        [InlineData("user", "test@example.com", "", "password123", "Password")] // Password required
        [InlineData("user", "test@example.com", "pass", "pass", "Password")] // Password too short
        [InlineData("user", "test@example.com", "password123", "different", "ConfirmPassword")] // Passwords don't match
        public void RegisterRequest_WhenValidated_ValidationRulesEnforced(string username, string email, string password, string confirmPassword, string expectedError)
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerRequest, new ValidationContext(registerRequest), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains(expectedError));
        }

        [Fact]
        public void RegisterRequest_WhenValidData_PassesValidation()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "validuser",
                Email = "valid@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerRequest, new ValidationContext(registerRequest), validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void AuthResponse_WhenCreated_HasCorrectDefaults()
        {
            // Arrange & Act
            var authResponse = new AuthResponse();

            // Assert
            Assert.False(authResponse.Success);
            Assert.Equal(string.Empty, authResponse.Message);
            Assert.Equal(string.Empty, authResponse.Token);
            Assert.NotNull(authResponse.User);
            Assert.Equal(string.Empty, authResponse.User.Id);
            Assert.Equal(string.Empty, authResponse.User.Username);
        }

        [Fact]
        public void AuthResponse_WhenPropertiesSet_StoresCorrectValues()
        {
            // Arrange
            var user = new UserDto
            {
                Id = "123",
                Username = "testuser",
                Email = "test@example.com"
            };

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = "jwt.token.here",
                User = user
            };

            // Assert
            Assert.True(authResponse.Success);
            Assert.Equal("Login successful", authResponse.Message);
            Assert.Equal("jwt.token.here", authResponse.Token);
            Assert.Equal("123", authResponse.User.Id);
            Assert.Equal("testuser", authResponse.User.Username);
            Assert.Equal("test@example.com", authResponse.User.Email);
        }

        [Theory]
        [InlineData(true, "Success", "token123", "user1", "Successful login")]
        [InlineData(false, "Invalid credentials", "", "", "Failed login")]
        public void AuthResponse_DifferentScenarios_HandledCorrectly(bool success, string message, string token, string userId, string testCase)
        {
            // Arrange
            var user = new UserDto { Id = userId };
            var authResponse = new AuthResponse
            {
                Success = success,
                Message = message,
                Token = token,
                User = user
            };

            // Assert
            Assert.Equal(success, authResponse.Success);
            Assert.Equal(message, authResponse.Message);
            Assert.Equal(token, authResponse.Token);
            Assert.Equal(userId, authResponse.User.Id);
        }
    }
} 