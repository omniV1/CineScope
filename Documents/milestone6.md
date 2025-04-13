# Software Maintenance Plan
for CineScope Movie Review Platform

**Team Members:** Carter Wright, Rian Smart, Owen Lindsey, Andrew Mack


**Grand Canyon University: CST-326**

**Date:** 4/13/2025

---

## Table of Contents

1.  [Maintenance Plan Overview](#maintenance-plan-overview)
2.  [Software Version and Delivery](#software-version-and-delivery)
3.  [Network and Other related IT Infrastructure](#network-and-other-related-it-infrastructure)
4.  [Frequency of Software updates](#frequency-of-software-updates)
5.  [Prioritization of categories of updates](#prioritization-of-categories-of-updates)
6.  [Methods of Bug Reporting and New Feature Suggestions](#methods-of-bug-reporting-and-new-feature-suggestions)
7.  [Software Version Control for Bug Fixes and Product Enhancements](#software-version-control-for-bug-fixes-and-product-enhancements)
8.  [Training Plan Overview](#training-plan-overview)
9.  [Software Tools Installation](#software-tools-installation)
10. [Software Updates](#software-updates)
11. [Reporting Bugs](#reporting-bugs)
12. [References](#references)

---

## Maintenance Plan Overview

This document outlines the plan for maintaining the CineScope Movie Review Platform software after its initial delivery. It covers versioning, delivery, infrastructure requirements, update frequency and prioritization, bug reporting, version control practices, and training for future maintainers and users. The goal is to ensure the long-term stability, usability, and relevance of the CineScope application based on the project's requirements [SDMP-REF-4] and technical design [SDMP-REF-1]. Comprehensive training materials for both IT administrators [SDMP-REF-6] and end-users [SDMP-REF-5] are provided separately.

## Software Version and Delivery

The software for CineScope will be identified using version numbers, potentially aligned with sprint completion or major feature releases tracked in Jira using SCRUM IDs. The project follows a feature-driven development approach, where work is organized around user stories and associated tasks defined in Jira (e.g., SCRUM-19, SCRUM-27).

The source code is managed in a GitHub repository (`https://github.com/omniV1/CineScope`). A branching strategy is employed to manage development and releases:
*   **`main` branch**: Represents the stable, production-ready code. Merges to `main` trigger deployments.
*   **Feature branches**: Created for developing new features or enhancements (e.g., `feature/SCRUM-XX`, `dev/feature-name`). These branches are typically based on the `main` branch.
*   **Bugfix branches**: Created to address bugs (e.g., `bugfix/SCRUM-YY`). Also branched from `main` or a release branch if applicable.

When a feature or bug fix is complete and tested according to the established procedures [SDMP-REF-3], the corresponding branch is merged into the `main` branch via a Pull Request, following a code review process.

CineScope is deployed to **Microsoft Azure**, likely using Azure App Service or a similar service. A CI/CD pipeline, configured using **GitHub Actions**, automatically builds, tests, and deploys the application to Azure whenever changes are merged into the `main` branch. Detailed deployment steps and Azure configuration are covered in the IT Administrator Guide [SDMP-REF-6].

## Network and Other related IT Infrastructure

To maintain and update the CineScope application, developers or maintainers will require specific tools installed on their development machines. The required infrastructure includes development tools, database access, version control, and potentially Azure management tools. A detailed list of required software and installation steps is provided in the "Software Tools Installation" section below and further elaborated in the IT Administrator Guide [SDMP-REF-6]. The general requirements include:

1.  **Integrated Development Environment (IDE)**: Visual Studio or VS Code.
2.  **.NET SDK**: Correct version for the project.
3.  **Database**: MongoDB (local or cloud access).
4.  **Database Management Tool**: MongoDB Compass or similar.
5.  **Version Control Client**: Git or GitHub Desktop.
6.  **Web Browser**: Modern browser for testing.
7.  **(Optional) Azure CLI / Azure Tools**: For Azure interaction.

These tools allow developers to access the source code, modify it, manage the database, run the application locally, push changes, and interact with the Azure deployment environment.

## Frequency of Software updates

Software updates for CineScope will be implemented for several reasons:
*   **New Features**: Adding new functionality based on user stories in the backlog [SDMP-REF-4].
*   **Bug Fixes**: Addressing defects discovered during testing [SDMP-REF-3] or reported from production.
*   **Enhancements**: Improving existing features or usability.
*   **Dependency Updates**: Updating libraries and frameworks for security or performance improvements.
*   **Policy Changes**: Implementing changes required by external regulations or internal policies.

The frequency of updates will generally align with the **sprint cycle** (e.g., 2-week sprints). Updates are scheduled based on the priorities set during sprint planning, managed via Jira. Minor bug fixes, especially critical ones found in production, may be deployed more frequently outside the regular sprint cycle via hotfix branches/releases.

Major updates involving significant new features will be carefully planned and tested [SDMP-REF-3] before release. Software updates will be managed to balance the need for new functionality and fixes with the cost and risk associated with frequent changes. The deployment process itself is detailed in the IT Administrator Guide [SDMP-REF-6].

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
    *   **External (Production)**: Bugs encountered by end-users or identified through production monitoring (detailed in [SDMP-REF-6]) should be reported to the Product Owner or a designated support channel. End users can refer to the User Guide [SDMP-REF-5] for basic troubleshooting. These reports are then verified and logged as high-priority defects in Jira.
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
    5.  The PR undergoes code review, testing [SDMP-REF-3], and potentially QA validation.
    6.  Upon approval, the branch is merged into `main`, and the feature is deployed to **Azure**.

This process ensures that the `main` branch always contains stable, reviewed code and provides traceability between code changes and Jira tasks/stories.

## Training Plan Overview

To ensure effective maintenance and utilization of the CineScope software, specific training resources are available for different roles:

1.  **IT Administrators / Maintainers**:
    *   **Primary Resource**: The **CineScope Complete IT Administrator Guide** [SDMP-REF-6] provides comprehensive details on initial setup, Azure deployment, MongoDB configuration, monitoring, maintenance, backup/recovery, security, performance optimization, content management, system updates, troubleshooting, and specific team instructions.
    *   **Key Areas**: Understanding the technology stack [SDMP-REF-1], system architecture [SDMP-REF-1], development environment setup (see below and [SDMP-REF-6]), codebase structure [SDMP-REF-2], development workflow (Git, Jira, CI/CD), testing procedures [SDMP-REF-3], debugging, database management [SDMP-REF-2], and Azure platform specifics [SDMP-REF-6].
    *   **Methodology**: Review of documentation ([SDMP-REF-1], [SDMP-REF-2], [SDMP-REF-3], [SDMP-REF-4], [SDMP-REF-6], this plan), pair programming, hands-on exercises.

2.  **End Users**:
    *   **Primary Resource**: The **CineScope User Guide** [SDMP-REF-5] details how to use the platform's features, including registration, discovering movies, writing reviews, managing profiles, understanding ratings, and basic troubleshooting.
    *   **Key Areas**: Navigation, searching/filtering, review creation/management, profile customization.
    *   **Methodology**: Self-guided review of the User Guide [SDMP-REF-5].

3.  **Developers**:
    *   Developers involved in maintenance should be familiar with both the IT Administrator Guide [SDMP-REF-6] (for understanding the environment) and the core development documents ([SDMP-REF-1], [SDMP-REF-2], [SDMP-REF-3], [SDMP-REF-4]).

This multi-faceted approach ensures that all personnel interacting with the system have the necessary knowledge for their roles.

## Software Tools Installation

New maintainers (developers or IT administrators) need to set up their environment. The following provides a summary; detailed steps are available in the IT Administrator Guide [SDMP-REF-6].

1.  **Install Visual Studio**: From [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/). Include "ASP.NET and web development" and potentially "Azure development" workloads. Ensure the correct .NET SDK version is included. (Alternative: .NET SDK + VS Code).
2.  **Install MongoDB**: Local Community Server from [https://www.mongodb.com/try/download/community](https://www.mongodb.com/try/download/community) or obtain connection details for the cloud instance [SDMP-REF-2].
3.  **Install MongoDB Compass**: From [https://www.mongodb.com/try/download/compass](https://www.mongodb.com/try/download/compass). Connect to local or cloud instance.
4.  **Install Git / GitHub Desktop**: Git CLI from [https://git-scm.com/downloads](https://git-scm.com/downloads) or GitHub Desktop from [https://desktop.github.com/](https://desktop.github.com/).
5.  **Install Azure CLI (Optional)**: From [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).
6.  **Clone the Repository**: Use Git CLI, GitHub Desktop, or Visual Studio to clone `https://github.com/omniV1/CineScope.git`.
7.  **Configure Project Settings**: Open the solution, configure `appsettings.Development.json` with the correct MongoDB connection string [SDMP-REF-2], using user secrets or Azure Key Vault for sensitive data. Build the solution.

Refer to Section 2 ("Initial Setup") of the IT Administrator Guide [SDMP-REF-6] for exhaustive, step-by-step instructions.

## Software Updates

The process for applying code changes (bug fixes, enhancements) involves development, testing, review, and deployment.

1.  **Task Assignment**: Obtain a Jira task (e.g., `SCRUM-XXX`).
2.  **Branching**: Update local `main`, create a feature/bugfix branch (`feature/SCRUM-XXX-desc` or `bugfix/SCRUM-YYY-desc`).
3.  **Implementation & Local Testing**: Code the changes, write/update tests [SDMP-REF-3], test locally.
4.  **Committing & Pushing**: Commit changes with clear messages, push the branch to GitHub.
5.  **Pull Request & Code Review**: Create a PR targeting `main`, assign reviewers. Address feedback.
6.  **Merging**: Once approved and CI checks pass, merge the PR into `main`.
7.  **Deployment**: The merge triggers the GitHub Actions CI/CD pipeline to deploy to **Azure**. Monitor the deployment.

Detailed deployment pipeline configuration and release management procedures are available in the IT Administrator Guide [SDMP-REF-6], Section 9 ("System Updates").

## Reporting Bugs

The process for reporting and handling bugs:

*   **Testing Phase Bugs**:
    1.  Bugs found during development (manual tests [SDMP-REF-3], automated tests, reviews) are logged in **Jira**.
    2.  Reports include steps to reproduce, expected vs. actual behavior, environment details, screenshots.
    3.  Bugs are linked to stories/epics, assigned, prioritized, and addressed in sprints.

*   **Production Bugs**:
    1.  Bugs reported by end-users (who can consult the User Guide [SDMP-REF-5] for basic troubleshooting) or detected via monitoring tools (detailed in [SDMP-REF-6]) are high priority.
    2.  Reports are channeled to the Product Owner/support contact, verified, and logged as high-priority defects in **Jira**.
    3.  Investigation is immediate. Fixes may be deployed via a hotfix process or scheduled for the next sprint start.

Clear, detailed reports are crucial. Refer to Section 10 ("Troubleshooting") in the IT Administrator Guide [SDMP-REF-6] for common issues and support procedures.

---

## References

*   **[SDMP-REF-1]**: Technical Design Document. Located at: `Documents/milestone3-Technical-Design.md`. Provides details on system architecture, components, database schema, and technical implementation choices.
*   **[SDMP-REF-2]**: MongoDB Integration Guide. Located at: `Help/Milestone3_DevHelp.md`. Provides detailed instructions for setting up, configuring, and using MongoDB within the Blazor application.
*   **[SDMP-REF-3]**: Test Procedures Document. Located at: `Training/Test-procedures-gcu.md`. Outlines the procedures for verifying application functionality, including setup, execution steps, pass/fail criteria, and non-functional test considerations.
*   **[SDMP-REF-4]**: Software Functional Requirements Document. Located at: `Documents/milestone2-Functional-Requirements-document.md`. Details the functional specifications, user interface designs, and functional/non-functional requirements mapped to use cases and SCRUM IDs.
*   **[SDMP-REF-5]**: CineScope User Guide. Located at: `Training/User-TrainingModule.md`. Provides guidance for end-users on navigating and utilizing the platform's features.
*   **[SDMP-REF-6]**: CineScope Complete IT Administrator Guide. Located at: `Training/IT-TrainingModule.md`. Offers comprehensive instructions for IT staff on deployment, configuration, monitoring, maintenance, security, and troubleshooting.
