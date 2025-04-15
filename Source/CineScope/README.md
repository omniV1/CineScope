# CineScope - Movie Review and Discussion Platform

## What is CineScope?
CineScope is a web application that lets users:
- Browse and search for movies
- Write and read movie reviews
- Discuss movies with other users
- Get movie recommendations
- Ask questions about movies using AI-powered chat

## Project Structure
The project is divided into several main parts:

### 1. Client (Frontend)
- Located in the `/Client` folder
- This is what users see in their web browser
- Built with Blazor WebAssembly (a framework for building web apps)
- Contains all the user interface elements like:
  - Movie listings
  - Review forms
  - User profiles
  - Navigation menus

### 2. Server (Backend)
- Located in the `/Server` folder
- This is the "brain" of the application that:
  - Stores and manages user data
  - Handles user authentication (login/register)
  - Processes movie reviews
  - Communicates with the movie database
  - Manages content moderation

### 3. Shared
- Located in the `/Shared` folder
- Contains code that's used by both the Client and Server
- Includes things like:
  - Data models (how information is structured)
  - Common functions
  - Shared interfaces

### 4. MCPServer
- Located in the `/CineScope.MCPServer` folder
- Handles AI-powered movie chat features
- Processes natural language questions about movies

## Configuration Setup

To run the application, you'll need to set up your configuration files:

1. Copy `Server/appsettings.template.json` to create your own `appsettings.Development.json`
2. Fill in the following configuration values in your `appsettings.Development.json`:
   - AnthropicSettings.ApiKey: Your Anthropic API key
   - MongoDbSettings.ConnectionString: Your MongoDB connection string
   - RecaptchaSettings.SecretKey: Your reCAPTCHA secret key

**Important**: Never commit `appsettings.Development.json` or any files containing sensitive information to the repository.

### Development Environment Setup

1. Copy the template configuration files:
   ```bash
   cp Server/appsettings.template.json Server/appsettings.json
   cp Client/wwwroot/appsettings.template.json Client/wwwroot/appsettings.json
   ```

2. Update the configuration files with your actual values:
   - Server/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key (for AI features)
     - `JwtSettings:Secret`: A secure random string (for user authentication)
     - `MongoDbSettings:ConnectionString`: Your MongoDB connection string (for database)
     - `RecaptchaSettings:SecretKey`: Your reCAPTCHA secret key (for spam prevention)

   - Client/wwwroot/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key (for AI chat)

### Production Environment

For production deployment, use environment variables or a secure configuration management system to provide the following values:

- `ANTHROPIC_API_KEY`: For AI-powered features
- `JWT_SECRET`: For secure user authentication
- `MONGODB_CONNECTION_STRING`: For database access
- `RECAPTCHA_SECRET_KEY`: For spam protection

### Security Notes

- Never commit sensitive configuration values to version control
- Use different API keys and secrets for development and production
- Regularly rotate secrets and API keys
- Monitor for any accidental commits of sensitive data

## Technologies Used
- **Frontend**: Blazor WebAssembly, MudBlazor (UI components)
- **Backend**: ASP.NET Core, MongoDB
- **AI Features**: Anthropic Claude API
- **Authentication**: JWT (JSON Web Tokens)
- **Database**: MongoDB
- **UI Framework**: MudBlazor (Material Design components)

## Getting Started
1. Install the .NET SDK
2. Set up a MongoDB database
3. Copy and configure the appsettings files
4. Run the application:
   ```bash
   dotnet run
   ```

## Need Help?
- Check the comments in the code - they're written to be beginner-friendly!
- Each folder has its own README explaining its purpose
- Code is organized into logical folders based on functionality 