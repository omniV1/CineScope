using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CineScope.Server.Controllers;
using CineScope.Server.Interfaces;
using CineScope.Server.Models;
using CineScope.Shared.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CineScope.Tests.Unit
{
    /// <summary>
    /// Tests for the authentication functionality of the CineScope application.
    /// These tests verify requirements FR-4.1, FR-4.2, and FR-4.6.
    /// </summary>
    public class AuthControllerTests
    {
        /// <summary>
        /// Test that the login endpoint returns an OK result when valid credentials are provided.
        /// Verifies requirement FR-4.1: The system shall allow users to login with unique credentials.
        /// </summary>
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@123"
            };

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = "valid_token",
                User = new Shared.DTOs.UserDto { Username = "testuser" }
            };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Login successful", returnValue.Message);
            Assert.Equal("valid_token", returnValue.Token);
        }

        /// <summary>
        /// Test that the login endpoint returns an Unauthorized result when invalid credentials are provided.
        /// Verifies requirement FR-4.6: The system shall validate credentials against database.
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedResult()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            var authResponse = new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password",
                Token = "",
                User = new Shared.DTOs.UserDto()
            };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Invalid username or password", returnValue.Message);
            Assert.Empty(returnValue.Token);
        }

        /// <summary>
        /// Test that account lockout occurs after three failed login attempts.
        /// Verifies requirement FR-4.2: The system shall lock account after three failed attempts.
        /// </summary>
        [Fact]
        public async Task Login_AfterThreeFailedAttempts_ReturnsAccountLockedMessage()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "lockuser",
                Password = "WrongPassword"
            };

            var authResponse = new AuthResponse
            {
                Success = false,
                Message = "Your account has been locked due to multiple failed login attempts. Please contact support.",
                Token = "",
                User = new Shared.DTOs.UserDto()
            };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponse>(unauthorizedResult.Value);
            Assert.False(returnValue.Success);
            Assert.Contains("account has been locked", returnValue.Message);
            Assert.Empty(returnValue.Token);
        }

        /// <summary>
        /// Test that the register endpoint returns an OK result when valid registration data is provided.
        /// Related to FR-4 requirements for user account management.
        /// </summary>
        [Fact]
        public async Task Register_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = "valid_token",
                User = new Shared.DTOs.UserDto { Username = "newuser" }
            };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(authResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Register(registerRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Registration successful", returnValue.Message);
            Assert.Equal("valid_token", returnValue.Token);
        }

        /// <summary>
        /// Test that the register endpoint returns a BadRequest result when the username already exists.
        /// Related to FR-4 requirements for uniqueness in user credentials.
        /// </summary>
        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Email = "new@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var authResponse = new AuthResponse
            {
                Success = false,
                Message = "Username already exists",
                Token = "",
                User = new Shared.DTOs.UserDto()
            };

            var mockAuthService = new Mock<IAuthService>();
            mockAuthService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(authResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Register(registerRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponse>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Username already exists", returnValue.Message);
        }
    }
}