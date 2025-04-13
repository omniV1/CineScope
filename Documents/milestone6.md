# Software Maintenance Plan
for CineScope Movie Review Platform

**Team Members:** Carter Wright, Rian Smart, Owen Lindsey, Andrew Mack
**(Adjust as needed for the maintenance team)**

**Grand Canyon University: CST-326**

**Date:** 4/13/2025

---

## Table of Contents

- [Software Maintenance Plan](#software-maintenance-plan)
  - [Table of Contents](#table-of-contents)
  - [Maintenance Plan Overview](#maintenance-plan-overview)
  - [Software Version and Delivery](#software-version-and-delivery)
  - [Network and Other related IT Infrastructure](#network-and-other-related-it-infrastructure)
  - [Frequency of Software updates](#frequency-of-software-updates)
  - [Prioritization of categories of updates](#prioritization-of-categories-of-updates)
  - [Methods of Bug Reporting and New Feature Suggestions](#methods-of-bug-reporting-and-new-feature-suggestions)
  - [Software Version Control for Bug Fixes and Product Enhancements](#software-version-control-for-bug-fixes-and-product-enhancements)
  - [Training Plan Overview](#training-plan-overview)
  - [Software Tools Installation](#software-tools-installation)
  - [Software Updates](#software-updates)
  - [Reporting Bugs](#reporting-bugs)
  - [References](#references)

---

## Maintenance Plan Overview

This document outlines the plan for maintaining the CineScope Movie Review Platform software after its initial delivery. It covers versioning, delivery, infrastructure requirements, update frequency and prioritization, bug reporting, version control practices, and training for future maintainers. The goal is to ensure the long-term stability, usability, and relevance of the CineScope application based on the project's requirements [SDMP-REF-4] and technical design [SDMP-REF-1].

## Software Version and Delivery

The software for CineScope will be identified using version numbers, potentially aligned with sprint completion or major feature releases tracked in Jira using SCRUM IDs. The project follows a feature-driven development approach, where work is organized around user stories and associated tasks defined in Jira (e.g., SCRUM-19, SCRUM-27).

The source code is managed in a GitHub repository (`https://github.com/omniV1/CineScope`). A branching strategy is employed to manage development and releases:
*   **`main` branch**: Represents the stable, production-ready code. Merges to `main` trigger deployments.
*   **Feature branches**: Created for developing new features or enhancements (e.g., `feature/SCRUM-XX`, `dev/feature-name`). These branches are typically based on the `main` branch.
*   **Bugfix branches**: Created to address bugs (e.g., `bugfix/SCRUM-YY`). Also branched from `main` or a release branch if applicable.

When a feature or bug fix is complete and tested according to the established procedures [SDMP-REF-3], the corresponding branch is merged into the `main` branch via a Pull Request, following a code review process.

CineScope is deployed to **Microsoft Azure**, likely using Azure App Service or a similar service. A CI/CD pipeline, configured using **GitHub Actions**, automatically builds, tests, and deploys the application to Azure whenever changes are merged into the `main` branch.

## Network and Other related IT Infrastructure

To maintain and update the CineScope application, developers or maintainers will require the following tools installed on their development machines:

1.  **Integrated Development Environment (IDE)**: **Visual Studio** (latest recommended version) with ASP.NET and web development workload installed. Alternatively, **Visual Studio Code** with the C# extension and .NET SDK can be used.
2.  **.NET SDK**: The version corresponding to the project's target framework (e.g., .NET 6.0 or later, check project files).
3.  **Database**: **MongoDB**. This can be a local installation or connection details for a cloud-hosted instance (like MongoDB Atlas, used by the project [SDMP-REF-2]).
4.  **Database Management Tool**: **MongoDB Compass** or a similar GUI tool for interacting with the MongoDB database, inspecting data, and running queries.
5.  **Version Control Client**: **Git** command-line tool or a GUI client like **GitHub Desktop** to interact with the GitHub repository.
6.  **Web Browser**: A modern web browser like Google Chrome, Firefox, or Edge for testing the web application.
7.  **(Optional) Azure CLI / Azure Tools for Visual Studio**: For interacting with Azure resources if direct management is needed.

These tools allow developers to access the source code from GitHub, make modifications using the IDE, manage and interact with the MongoDB database, run and debug the application locally, and push changes back to the repository. The GitHub Actions pipeline then handles the deployment to **Azure** based on updates to the `main` branch.

## Frequency of Software updates

Software updates for CineScope will be implemented for several reasons:
*   **New Features**: Adding new functionality based on user stories in the backlog [SDMP-REF-4].
*   **Bug Fixes**: Addressing defects discovered during testing [SDMP-REF-3] or reported from production.
*   **Enhancements**: Improving existing features or usability.
*   **Dependency Updates**: Updating libraries and frameworks for security or performance improvements.
*   **Policy Changes**: Implementing changes required by external regulations or internal policies.

The frequency of updates will generally align with the **sprint cycle** (e.g., 2-week sprints). Updates are scheduled based on the priorities set during sprint planning, managed via Jira.

Minor bug fixes, especially critical ones found in production, may be deployed more frequently outside the regular sprint cycle via hotfix branches/releases.

Major updates involving significant new features will be carefully planned and tested before release. Software updates will be managed to balance the need for new functionality and fixes with the cost and risk associated with frequent changes. All significant changes undergo testing (unit, integration, acceptance) before being merged to `main` and deployed [SDMP-REF-3].

## Prioritization of categories of updates

Updates for CineScope will be categorized and prioritized as follows:

1.  **Bug Fixes**:
    *   **Priority**: Highest priority, especially for bugs affecting core functionality or found in production.
    *   **Order**: Bugs are prioritized based on severity and user impact. Critical production bugs > Major bugs > Minor bugs. Input from the Product Owner (Rian Smart) and development team determines the exact order.
    *   **Scheduling**: Addressed immediately or scheduled for the current/next sprint based on severity.

2.  **New Features / Enhancements**:
    *   **Priority**: Prioritized below critical bug fixes.
    *   **Order**: Determined during sprint planning meetings. Prioritization is based on business value, client/Product Owner (Rian Smart) requirements, and strategic goals outlined in project documentation [SDMP-REF-4]. If the client has no preference, the team prioritizes based on dependencies and perceived development efficiency.
    *   **Scheduling**: Added to sprint backlogs based on priority and team capacity.

## Methods of Bug Reporting and New Feature Suggestions

Mechanisms for identifying necessary maintenance work:

*   **Bug Reporting**:
    *   **Internal (Testing)**: Bugs found during development sprints through manual testing (following ATPs [SDMP-REF-3]), automated tests (unit, integration), or code reviews are logged directly into **Jira** as defects, often linked to the relevant user story or task.
    *   **External (Production)**: Bugs encountered by end-users or identified through production monitoring should be reported to the Product Owner or a designated support channel. These reports are then verified and logged as high-priority defects in Jira. The application might also implement automated error reporting mechanisms that log issues for the development team.
*   **New Feature Suggestions**:
    *   Suggestions for new features or significant enhancements primarily come from the **Product Owner (Rian Smart)** based on stakeholder feedback, market analysis, or strategic planning.
    *   Suggestions are captured in the **Jira backlog** as new user stories or epics.
    *   These suggestions are discussed, refined, and prioritized during backlog grooming and sprint planning sessions.

## Software Version Control for Bug Fixes and Product Enhancements

CineScope utilizes Git with a branching strategy hosted on GitHub for version control:

*   **Bug Fixes**:
    1.  A bug is identified and logged as a task/issue in Jira (e.g., `SCRUM-XXX`).
    2.  A developer creates a new branch from the `main` branch (or appropriate release branch), named descriptively (e.g., `bugfix/SCRUM-XXX-login-error`).
    3.  The developer implements the fix on this branch, commits the changes, and runs relevant tests [SDMP-REF-3].
    4.  A Pull Request (PR) is created on GitHub to merge the fix branch into `main`.
    5.  The PR is reviewed by at least one other team member.
    6.  Upon approval, the branch is merged into `main`, triggering the CI/CD pipeline for deployment to **Azure**.

*   **Product Enhancements (New Features)**:
    1.  A new feature is defined as a User Story in Jira (e.g., `SCRUM-YYY`).
    2.  During a sprint, a developer assigned to the story creates a feature branch from `main` (e.g., `feature/SCRUM-YYY-movie-filtering`).
    3.  The developer implements the feature on this branch, potentially involving multiple commits and pushes. Unit and integration tests are written alongside the code.
    4.  Once the feature is complete according to the acceptance criteria [SDMP-REF-4], a Pull Request is created to merge the feature branch into `main`.
    5.  The PR undergoes code review, testing, and potentially QA validation according to procedures [SDMP-REF-3].
    6.  Upon approval, the branch is merged into `main`, and the feature is deployed to **Azure**.

This process ensures that the `main` branch always contains stable, reviewed code and provides traceability between code changes and Jira tasks/stories.

## Training Plan Overview

To ensure that new team members or developers unfamiliar with CineScope can effectively maintain the software, a training plan covering the following areas is recommended:

1.  **Project Overview**: Understanding the purpose, scope, and target audience of CineScope, as detailed in the requirements [SDMP-REF-4].
2.  **Technology Stack**: Familiarity with C#, ASP.NET Core, Blazor Server, MongoDB, xUnit, Moq, HTML, CSS (Bootstrap), and potentially JavaScript/jQuery if used. Refer to the technology stack outlined in the technical design [SDMP-REF-1].
3.  **Architecture**: Understanding the N-layer architecture (Presentation, Business Logic, Data Access) and the project's specific implementation, detailed in the technical design [SDMP-REF-1].
4.  **Development Environment Setup**: Installing and configuring the necessary tools (IDE, SDK, Database, Git) as listed below.
5.  **Codebase Familiarization**: Navigating the project structure, understanding key components, models, services, and repositories. Reviewing the MongoDB Integration Guide [SDMP-REF-2] is crucial for data access understanding.
6.  **Development Workflow**: Understanding the Agile/Scrum process, using Jira for task management, following the Git branching strategy, code review process, and CI/CD pipeline with GitHub Actions and **Azure DevOps/Azure Pipelines** (if used alongside/instead of GitHub Actions) or deployment scripts targeting **Azure**.
7.  **Testing**: Understanding the testing strategy, running existing unit and integration tests, and writing new tests. Familiarity with the Acceptance Test Procedures [SDMP-REF-3].
8.  **Debugging**: Techniques for debugging the Blazor application and backend services.
9.  **Database Management**: Basic interaction with MongoDB using MongoDB Compass or similar tools.
10. **Azure Platform**: Basic understanding of the Azure services used for hosting (e.g., Azure App Service) and deployment process.

This training can be achieved through documentation review (specifically [SDMP-REF-1], [SDMP-REF-2], [SDMP-REF-3], [SDMP-REF-4], and this Maintenance Plan), pair programming with existing team members, and hands-on exercises.

## Software Tools Installation

New maintainers need to set up their development environment. Follow these steps:

1.  **Install Visual Studio**:
    *   Navigate to [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/)
    *   Download the Community, Professional, or Enterprise edition (Community is free).
    *   Run the installer and select the "ASP.NET and web development" workload. Consider adding the "Azure development" workload as well. Ensure the relevant .NET SDK version is included.
    *   Complete the installation.
    *(Alternatively, install .NET SDK from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) and Visual Studio Code from [https://code.visualstudio.com/](https://code.visualstudio.com/))*

2.  **Install MongoDB**:
    *   **Option A (Local Install)**:
        *   Navigate to [https://www.mongodb.com/try/download/community](https://www.mongodb.com/try/download/community)
        *   Download the MongoDB Community Server for your OS.
        *   Follow the installation instructions for your OS. This typically involves setting up the data directory and starting the MongoDB service.
    *   **Option B (Cloud Atlas)**: Obtain connection string and credentials for the project's shared MongoDB Atlas instance [SDMP-REF-2]. No local installation is needed, but network access is required.

3.  **Install MongoDB Compass (Optional but Recommended)**:
    *   Navigate to [https://www.mongodb.com/try/download/compass](https://www.mongodb.com/try/download/compass)
    *   Download and install the GUI tool for MongoDB.
    *   Connect to your local MongoDB instance (usually `mongodb://localhost:27017`) or the cloud instance using the connection string.

4.  **Install Git / GitHub Desktop**:
    *   **Git CLI**: Navigate to [https://git-scm.com/downloads](https://git-scm.com/downloads) and install Git for your OS.
    *   **GitHub Desktop**: Navigate to [https://desktop.github.com/](https://desktop.github.com/) download and install. Connect your GitHub account.

5.  **Install Azure CLI (Optional but Recommended)**:
    *   Navigate to [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
    *   Follow instructions to install the Azure CLI for your OS.
    *   Log in using `az login`.

6.  **Clone the Repository**:
    *   Using Git CLI: `git clone https://github.com/omniV1/CineScope.git`
    *   Using GitHub Desktop: Click "File" -> "Clone repository", find `omniV1/CineScope`, choose a local path, and clone.
    *   Using Visual Studio: Open Visual Studio, select "Clone a repository", enter the repository URL (`https://github.com/omniV1/CineScope.git`), choose a local path, and clone.

7.  **Configure Project Settings**:
    *   Open the CineScope solution (`.sln` file) in Visual Studio.
    *   Locate the `appsettings.json` or `appsettings.Development.json` file.
    *   Ensure the `MongoDBSettings` section has the correct `ConnectionString` for your local or cloud database instance and the correct `DatabaseName`, as specified in the MongoDB guide [SDMP-REF-2]. **Do not commit sensitive credentials directly to the repository.** Use user secrets, Azure Key Vault, or Azure App Configuration for production/shared credentials.
    *   Build the solution (Build -> Build Solution) to restore NuGet packages.

## Software Updates

Follow this process when making code changes for bug fixes or enhancements:

1.  **Get Assigned Task**: Ensure you have a corresponding task/issue assigned in Jira (e.g., `SCRUM-XXX`).
2.  **Update Local Repository**: Ensure your local `main` branch is up-to-date:
    ```bash
    git checkout main
    git pull origin main
    ```
3.  **Create New Branch**: Create a branch from `main` named according to the convention:
    ```bash
    # For features:
    git checkout -b feature/SCRUM-XXX-brief-description
    # For bug fixes:
    git checkout -b bugfix/SCRUM-YYY-brief-description
    ```
4.  **Implement Changes**: Make the necessary code changes in Visual Studio or your preferred IDE. Write or update unit/integration tests as needed, following project standards [SDMP-REF-3].
5.  **Test Locally**: Run the application locally, perform manual testing, and run automated tests (using the Test Explorer in Visual Studio or `dotnet test` CLI command) to verify the changes [SDMP-REF-3].
6.  **Commit Changes**: Commit your changes with clear, descriptive messages:
    ```bash
    git add .
    git commit -m "SCRUM-XXX: Implement feature/fix description"
    ```
7.  **Push Branch**: Push your branch to the GitHub repository:
    ```bash
    git push origin feature/SCRUM-XXX-brief-description
    ```
8.  **Create Pull Request**: Go to the CineScope GitHub repository in your web browser. GitHub should prompt you to create a Pull Request for your recently pushed branch. Create the PR, targeting the `main` branch.
    *   Fill in the PR description, linking the Jira task (e.g., "Closes SCRUM-XXX").
    *   Assign reviewers (other team members).
9.  **Code Review**: Team members review the code, provide feedback, and request changes if necessary. Address any feedback by pushing additional commits to your branch.
10. **Merge**: Once the PR is approved and any required checks (like CI build/tests) pass, merge the Pull Request into the `main` branch (usually done by the reviewer or the author via the GitHub interface). Delete the feature/bugfix branch after merging.
11. **Deployment**: The merge to `main` will automatically trigger the GitHub Actions CI/CD pipeline, which will deploy the changes to **Azure**. Monitor the deployment process via GitHub Actions logs and the Azure portal.

## Reporting Bugs

The process for reporting and handling bugs depends on where they are found:

*   **Testing Phase Bugs**:
    1.  Bugs identified during development sprints (manual testing [SDMP-REF-3], automated tests, peer reviews) should be documented.
    2.  Create a new bug/defect issue in **Jira**.
    3.  Provide detailed information:
        *   Steps to reproduce the bug.
        *   Expected behavior vs. actual behavior.
        *   Screenshots or error messages, if applicable.
        *   Environment details (browser, OS, etc.).
    4.  Link the bug to the relevant User Story or Epic if possible.
    5.  Assign the bug to the appropriate developer or the Scrum Master/Product Owner for triage.
    6.  The bug will be prioritized and potentially added to the current or next sprint backlog.

*   **Production Bugs**:
    1.  Bugs reported by end-users or detected via monitoring tools (e.g., Azure Application Insights) are considered high priority.
    2.  Reports should be channeled to the Product Owner or a designated support contact.
    3.  The report is verified by the team to confirm it's a genuine bug.
    4.  A high-priority bug/defect issue is created in **Jira** with detailed information, similar to testing phase bugs.
    5.  The bug is assigned immediately to a developer or the team for investigation.
    6.  Based on severity, a fix might be implemented immediately via a hotfix process (branching from `main`, fixing, reviewing, merging, deploying quickly to **Azure**) or scheduled for the very beginning of the next sprint.

Clear and detailed bug reports are essential for efficient resolution.

---

## References

*   **[SDMP-REF-1]**: Technical Design Document. Located at: `Documents/milestone3-Technical-Design.md`. Provides details on system architecture, components, database schema, and technical implementation choices.
*   **[SDMP-REF-2]**: MongoDB Integration Guide. Located at: `Help/Milestone3_DevHelp.md`. Provides detailed instructions for setting up, configuring, and using MongoDB within the Blazor application, including models, repositories, services, and Blazor-specific integration points.
*   **[SDMP-REF-3]**: Test Procedures Document. Located at: `Documents/milestone4-test-procedures-gcu.md`. Outlines the procedures for verifying application functionality, including setup, execution steps, pass/fail criteria, and non-functional test considerations.
*   **[SDMP-REF-4]**: Software Functional Requirements Document. Located at: `Documents/milestone2-Functional-Requirements-document.md`. Details the functional specifications, user interface designs, and functional/non-functional requirements mapped to use cases and SCRUM IDs.