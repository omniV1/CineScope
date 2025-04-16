# CineScope 🎬

CineScope is a modern, user-friendly movie review platform that enables movie enthusiasts to share opinions and discover new films. Built using Blazor C# ASP.NET Core (MVC) with MongoDB as the database system, the platform delivers a responsive web interface for browsing movies, writing reviews, and interacting with other users' content.

## 🚀 Project Progress

- ✅ **Milestone 1**: Initial project setup and repository configuration
- ✅ **Milestone 2**: Functional Requirements Document completed
- ✅ **Milestone 3**: Technical Design Document completed
- ✅ **Milestone 4**: Test Procedures Document and Version Description Document completed
- 🏗️ **Sprint 1 in progress** (Feb 24 - Mar 9, 2025): Implementing Featured Movies, User Authentication, and Review Management

## ✨ Features

- **Landing Page Implementation**: Featured movies, recently viewed, top-rated, and genre-based sections
- **Authentication System**: Secure user registration and login with account lockout protection
- **Review Management**: Create, read, update, and delete movie reviews with ratings and text feedback
- **Content Filtering**: Automated screening of user-generated content to maintain community standards
- **Movie Browsing**: Advanced filtering and sorting options for movie discovery
- **Responsive Design**: Mobile and desktop friendly interface

## 🛠️ Technology Stack

- **Frontend**: Blazor Web App, MudBlazor UI components
- **Backend**: C# ASP.NET Core MVC
- **Database**: MongoDB
- **Authentication**: JWT-based authentication with ASP.NET Core Identity
- **Testing**: xUnit, Moq, bUnit for component testing
- **Version Control**: Git with GitHub

## 📋 Project Architecture

The application implements an N-layer architecture:
- **Presentation Layer**: Handles user interface rendering and user input processing
- **Business Logic Layer**: Implements core application functionality and business rules
- **Data Access Layer**: Manages database interactions and data persistence
- **Database Layer**: MongoDB database system for data storage

## 👥 Team Members

- **Carter Wright**: Scrum Master (Development)
- **Rian Smart**: Product Owner (Management)
- **Owen Lindsey**: Developer (Development)
- **Andrew Mack**: Developer (Development)

## 📂 Repository Structure

```
CineScope/
├── Documents/                  # Project documentation
│   ├── milestone2-Functional-Requirements-document.md
│   ├── milestone3-Technical-Desgin.md
│   ├── milestone4-test-procedures-gcu.md
│   ├── milestone4-version-description.md
│   └── Images/                 # Wireframes and diagrams
├── Source/                     # Source code
│   └── CineScope/
│       ├── Client/             # Blazor WebAssembly client
│       ├── Server/             # ASP.NET Core server
│       ├── Shared/             # Shared models and DTOs
│       ├── CineScope.Tests.Unit/
│       └── CineScope.Tests.Integration/
└── README.md
```

## 🔧 Configuration Setup

### Development Environment Setup

1. Copy the template configuration files:
```bash
cp Server/appsettings.template.json Server/appsettings.json
cp Client/wwwroot/appsettings.template.json Client/wwwroot/appsettings.json
```

2. Update the configuration files with your actual values:
   - Server/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key
     - `JwtSettings:Secret`: A secure random string for JWT signing
     - `MongoDbSettings:ConnectionString`: Your MongoDB connection string
     - `RecaptchaSettings:SecretKey`: Your reCAPTCHA secret key

   - Client/wwwroot/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key

### Production Environment

For production deployment, use environment variables or a secure configuration management system to provide the following values:

- `ANTHROPIC_API_KEY`
- `JWT_SECRET`
- `MONGODB_CONNECTION_STRING`
- `RECAPTCHA_SECRET_KEY`

### Security Notes

- Never commit sensitive configuration values to version control
- Use different API keys and secrets for development and production
- Regularly rotate secrets and API keys
- Monitor for any accidental commits of sensitive data

## 🧪 Testing

Comprehensive test procedures have been developed for:
- User Authentication
- Movie Browsing and Filtering
- Review Creation and Management
- Content Filtering
- User Profile Management

## 🚀 Getting Started

### Prerequisites
- .NET Core SDK 8.0 or later
- MongoDB 6.0 or later
- Visual Studio 2022 or preferred IDE
- Git

### Installation & Setup
1. Clone the repository:
```bash
git clone https://github.com/omniV1/CineScope.git
cd CineScope
```

2. Configure MongoDB:
   - Update the connection string in `Source/CineScope/Server/appsettings.json`

3. Install dependencies and run:
```bash
dotnet restore
dotnet run --project Source/CineScope/Server/CineScope.Server.csproj
```

## 🤝 Contributing

1. Clone the repository:
```bash
git clone https://github.com/omniV1/CineScope.git
cd CineScope
```

2. Set up your git identity:
```bash
git config user.name "Your Name"
git config user.email "your.email@example.com"
```

3. Create your working branch:
```bash
git checkout -b Name-Dev
Example:
git checkout -b John-Dev
```

4. Push your branch:
```bash
git add .
git commit -m "your commit message"
git push origin Name-Dev
```

Create a Pull Request on GitHub when you're ready for review.

## 🔍 Sprint Planning

**Sprint 1 (February 24 - March 9, 2025):**
- SCRUM-19: As a user, I want to see featured movies...
- SCRUM-27: As a user, I want to be able to login an...
- SCRUM-34: As a user, I want to manage movie reviews...

**Sprint 2 (March 9 - March 16, 2025):**
- SCRUM-45: "As a user, I want to be able to filter reviews..."

**Final Project Delivery**: March 30, 2025

---

*Last updated: March 10, 2025*
