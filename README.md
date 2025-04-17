# CineScope Movie Review Platform

![CineScope Logo](Documents/Images/Landing_Page_Wireframe.png)

## For Movie Lovers, By Movie Lovers

CineScope is a modern, user-friendly movie review platform that enables movie enthusiasts to share their opinions and discover new films. Built using C# ASP.NET Core Blazor with MongoDB as the database system, the platform delivers a responsive web interface where users can browse movies by various categories, write and manage reviews, and interact with other users' content.

## Team Members

| Name | Role | Department |
|------|------|------------|
| Carter Wright | Scrum Master | Development |
| Rian Smart | Product Owner | Management |
| Owen Lindsey | Developer | Development |
| Andrew Mack | Developer | Development |

## Features

- **User Authentication**: Secure login/registration system with account protection
- **Movie Browsing**: Browse films by category, rating, and release date
- **Review Management**: Create, read, update, and delete your movie reviews
- **Content Filtering**: Automatic moderation of user-submitted content
- **Responsive Design**: Seamless experience across desktop and mobile devices
- **Admin Dashboard**: Content moderation and user management tools

## Repository Structure

```
CineScope/
├── Documents/                  # Documentation files
│   ├── Help/                   # Guides and training materials
│   ├── Images/                 # Wireframes and diagrams
│   ├── Requirements/           # Functional and non-functional requirements
│   │   ├── FunctionalRequirements.md    # Detailed functional requirements
│   │   ├── NonFunctionalRequirements.md # Performance and quality requirements
│   │   └── User-Stories.md     # User stories and priorities
│   ├── Technical-Reports/      # Technical design and documentation
│   │   ├── Functional-Requirements.md   # Full requirements specification
│   │   ├── Technical-Design.md # System architecture and implementation details
│   │   └── Version-description.md # Project status and milestones
│   └── Training/               # Training modules
│       ├── IT-TrainingModule.md # IT administrator guide
│       └── User-TrainingModule.md # End-user documentation
├── Source/                     # Source code
│   └── CineScope/              # Main application
│       ├── Client/             # Blazor WebAssembly client application
│       ├── Server/             # ASP.NET Core server application
│       └── Shared/             # Shared code between Client and Server
└── README.md                   # This file
```

## Key Documentation

- **System Architecture**: See [Technical Design Document](Documents/Technical-Reports/Technical-Design.md)
- **Project Requirements**: See [Functional Requirements](Documents/Technical-Reports/Functional-Requirements.md)
- **User Stories**: See [User Stories](Documents/Requirements/User-Stories.md)
- **User Interface**: UI wireframes can be found in the [Images folder](Documents/Images/)
- **Development Guide**: See [MongoDB Integration Guide](Documents/Help/Milestone3_DevHelp.md)
- **Admin Documentation**: See [Admin Implementation Guide](Documents/Help/Admin-Implementation.md)

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (local) or [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) (cloud)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/omniV1/CineScope.git
   cd CineScope
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Configure MongoDB connection in `appsettings.json`

4. Run the application:
   ```bash
   cd Source/CineScope/Server
   dotnet run
   ```

## Development Workflow

Our team follows an Agile development process:

1. User stories and requirements are managed in [Jira](https://cinescopedev.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog)
2. Source code is stored in this GitHub repository
3. Development follows the specifications in our [Technical Design Document](Documents/Technical-Reports/Technical-Design.md)
4. Testing procedures are outlined in [Test Procedures](Documents/Training/Test-procedures-gcu.md)

## Deployment and Maintenance

For information on deployment and maintenance procedures, please refer to:

- [Maintenance Plan](Documents/Technical-Reports/Maintenance/MaintenancePlan.md)
- [Admin Implementation Guide](Documents/Help/Admin-implementation-support.md)
- [IT Training Module](Documents/Training/IT-TrainingModule.md)

## License

This project is licensed under the MIT License.
