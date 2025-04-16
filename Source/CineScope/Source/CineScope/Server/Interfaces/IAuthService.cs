using System.Threading.Tasks;
using CineScope.Shared.Auth;

namespace CineScope.Server.Interfaces
{
    /// <summary>
    /// Interface defining authentication operations.
    /// Enables dependency injection and unit testing of authentication functionality.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user based on login credentials.
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>Authentication result with token if successful</returns>
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);

        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="registerRequest">The registration information</param>
        /// <returns>Registration result with token if successful</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
    }
}