# Software Maintenance Plan
for CineScope Movie Review Platform

**Team Members:** Carter Wright, Rian Smart, Owen Lindsey, Andrew Mack
**(Adjust as needed for the maintenance team)**

**Grand Canyon University: CST-326**

**Date:** 04/13/2025

---

## Table of Contents

1.  [Maintenance Plan Overview](#maintenance-plan-overview)
2.  [Software Version and Delivery](#software-version-and-delivery)
3.  [Network and Other related IT Infrastructure](#network-and-other-related-it-infrastructure)
4.  [Frequency of Software updates](#frequency-of-software-updates)
5.  [Prioritization of categories of updates](#prioritization-of-categories-of-updates)
6.  [Methods of Bug Reporting and New Feature Suggestions](#methods-of-bug-reporting-and-new-feature-suggestions)
7.  [Software Version Control Workflow](#software-version-control-workflow)
8.  [Training Plan Overview](#training-plan-overview)
9.  [Software Tools Installation](#software-tools-installation)
10. [Software Updates Workflow](#software-updates-workflow)
11. [Bug Reporting Process](#bug-reporting-process)
12. [References](#references)

---

## Maintenance Plan Overview

Maintaining software effectively requires a clear strategy encompassing various post-delivery activities. This document outlines the comprehensive plan for the ongoing maintenance of the CineScope Movie Review Platform. It details the procedures and guidelines for versioning, deployment, infrastructure management, handling updates, prioritizing tasks, reporting issues, version control practices, and training resources. The primary objective is to ensure the platform remains stable, usable, and relevant, aligning with its initial requirements [SDMP-REF-4] and technical design [SDMP-REF-1]. Separate, detailed training materials support both IT administrators [SDMP-REF-6] and end-users [SDMP-REF-5].

## Software Version and Delivery

A consistent approach to software versioning and delivery is essential for managing releases and deployments effectively. CineScope software versions typically align with the completion of development sprints or the release of major features, tracked using SCRUM IDs within Jira. The project adheres to a feature-driven development methodology, guided by user stories [SDMP-REF-4]. The source code resides in the GitHub repository `https://github.com/omniV1/CineScope`, managed using a defined branching strategy.

**Branching Strategy Summary:**

| Branch Pattern | Purpose | Base Branch | Merge Target |
|---|---|---|---|
| `main` | Stable, production-ready code | - | - |
| `feature/SCRUM-XX` or `dev/feature-name` | New feature development | `main` | `main` |
| `bugfix/SCRUM-YY` or `hotfix/name` | Bug fixing | `main` / Release | `main` |

Changes are integrated into the `main` branch only after thorough review and approval via GitHub Pull Requests, ensuring code quality.

**Deployment Overview:**

The application is delivered to end-users via deployment to the Microsoft Azure cloud platform. This process is automated to ensure consistency and reliability.

| Component | Detail |
|---|---|
| Platform | Microsoft Azure (App Service or similar) |
| CI/CD | GitHub Actions pipeline |
| Trigger | Merge to `main` branch |
| Process | Build -> Test [SDMP-REF-3] -> Deploy to Azure |
| Details | See IT Administrator Guide [SDMP-REF-6] for configuration specifics |

## Network and Other related IT Infrastructure

Maintaining and updating CineScope requires specific hardware and software infrastructure, primarily for the personnel involved in these tasks. This section outlines the necessary tools and environment components needed for effective development, administration, and maintenance activities.

**Required Tools for Maintainers:**

The following table summarizes the core tools required. Detailed installation and configuration steps are available in the IT Administrator Guide [SDMP-REF-6].

| Tool Category | Specific Tool | Purpose | Installation Source / Reference |
|---|---|---|---|
| IDE | Visual Studio or VS Code | Code development, debugging | [Visual Studio](https://visualstudio.microsoft.com/downloads/) / [VS Code](https://code.visualstudio.com/) |
| SDK | .NET SDK (project version) | Build and run application | [Microsoft .NET](https://dotnet.microsoft.com/download) |
| Database | MongoDB | Data storage | [MongoDB Community](https://www.mongodb.com/try/download/community) / Cloud Atlas [SDMP-REF-2] |
| DB Management | MongoDB Compass | Database interaction | [MongoDB Compass](https://www.mongodb.com/try/download/compass) |
| Version Control | Git / GitHub Desktop | Source code management | [Git](https://git-scm.com/downloads) / [GitHub Desktop](https://desktop.github.com/) |
| Browser | Modern Browser | Web application testing | Standard installation |
| Cloud CLI (Optional) | Azure CLI | Azure resource management | [Azure CLI Docs](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) |

Refer to Section 9 ([Software Tools Installation](#software-tools-installation)) for a checklist and the IT Administrator Guide [SDMP-REF-6] for comprehensive setup instructions.

## Frequency of Software updates

The CineScope platform will evolve over time through regular software updates. This section describes the types of updates anticipated and the typical frequency at which they will be deployed. Updates generally align with the established sprint cycle (e.g., 2 weeks), although urgent fixes might necessitate out-of-band deployments.

**Types of Software Updates:**

| Update Type | Description | Typical Trigger |
|---|---|---|
| New Features | Adding functionality per user stories [SDMP-REF-4]. | Sprint Planning |
| Bug Fixes | Addressing defects found in testing [SDMP-REF-3] or production. | Bug Reports / Jira |
| Enhancements | Improving existing features/usability. | Backlog / User Feedback |
| Dependency Updates | Updating libraries for security/performance. | Audits / Releases |
| Policy Changes | Implementing required regulatory or internal changes. | Policy Updates |

All significant changes are subject to the testing procedures outlined in [SDMP-REF-3] before being released. The deployment methodology itself is covered in the IT Administrator Guide [SDMP-REF-6].

## Prioritization of categories of updates

With multiple potential updates (bug fixes, new features, etc.), a clear prioritization strategy is needed to allocate resources effectively. This section defines how different categories of updates are prioritized for implementation. The core principle is to address critical issues affecting users first, followed by planned enhancements and features.

**Update Prioritization Matrix:**

| Category | Priority Level | Scheduling | Determination Basis |
|---|---|---|---|
| Bug Fixes (Production - Critical) | Highest | Immediate (Hotfix) | Severity, User Impact, Product Owner Input |
| Bug Fixes (Production - Major/Minor) | High | Current / Next Sprint | Severity, User Impact, Product Owner Input |
| Bug Fixes (Testing Phase) | Medium | Next Sprint / Backlog | Severity, Dependency, Team Capacity |
| New Features / Enhancements | Medium / Low | Sprint Backlog | Business Value [SDMP-REF-4], Product Owner Input, Team Capacity |

## Methods of Bug Reporting and New Feature Suggestions

Effective maintenance relies on clear communication channels for identifying necessary work. This section details how bugs should be reported by testers and end-users, and how suggestions for new features or enhancements are collected and processed.

*   **Bug Reporting:**
    *   **Internal Testing:** Defects discovered during development activities (manual testing [SDMP-REF-3], automated tests, code reviews) must be logged promptly in **Jira**. These reports require detailed reproduction steps, comparison of expected versus actual results, environment specifics, and supporting evidence like logs or screenshots.
    *   **Production:** Issues encountered by end-users (potentially after consulting the User Guide [SDMP-REF-5]) or identified via system monitoring [SDMP-REF-6] should be directed to the Product Owner or a designated support channel. Once verified, these are logged as high-priority defects in Jira with thorough details.

*   **New Feature Suggestions:**
    *   Ideas for enhancing the platform typically come from the **Product Owner (Rian Smart)**, informed by stakeholder input, market trends, or strategic objectives.
    *   These suggestions are formally captured within the **Jira backlog** as user stories or epics.
    *   They are subsequently discussed, refined, estimated, and prioritized during backlog grooming and sprint planning ceremonies.

## Software Version Control Workflow

A standardized version control workflow using Git and GitHub is crucial for collaborative development, code quality, and release management. This section outlines the step-by-step process for handling both bug fixes and the development of new features within the CineScope codebase.

*   **Bug Fixes Workflow:**
    1.  **Log:** The bug is identified and formally logged in Jira (e.g., `SCRUM-XXX`).
    2.  **Branch:** A developer creates a specific branch for the fix, usually from `main`, following a naming convention (e.g., `bugfix/SCRUM-XXX-login-error`).
    3.  **Implement:** The code fix is developed on this branch, including any necessary updates to tests [SDMP-REF-3].
    4.  **Review:** A Pull Request (PR) is created on GitHub to merge the fix into `main`. Code review by peers is mandatory.
    5.  **Merge:** After addressing feedback and gaining approval, the PR is merged.
    6.  **Deploy:** Merging triggers the automated CI/CD pipeline, deploying the fix to Azure.

*   **Product Enhancements Workflow:**
    1.  **Define:** The feature is clearly defined as a User Story or Epic in Jira (e.g., `SCRUM-YYY`).
    2.  **Branch:** A developer creates a feature branch from `main` (e.g., `feature/SCRUM-YYY-movie-filtering`).
    3.  **Implement:** The feature is built on this branch, including corresponding tests [SDMP-REF-3].
    4.  **Review:** A PR targeting `main` is created. It undergoes code review and potentially QA validation against acceptance criteria [SDMP-REF-4].
    5.  **Merge:** Upon approval, the PR is merged into `main`.
    6.  **Deploy:** The merge initiates the CI/CD process for deployment to Azure.

## Training Plan Overview

Ensuring that both maintainers and end-users are adequately trained is vital for the success and longevity of the CineScope platform. This section outlines the available training resources tailored to different roles, facilitating effective system management and utilization.

**Training Resources Matrix:**

| Target Audience | Primary Resource | Key Areas Covered | Methodology |
|---|---|---|---|
| IT Administrators / Maintainers | IT Administrator Guide [SDMP-REF-6] | Deployment, Config, Monitoring, Maintenance, Backup, Security, Performance, Troubleshooting, Azure Specifics | Documentation Review, Pair Programming, Hands-on Exercises |
| End Users | User Guide [SDMP-REF-5] | Navigation, Search, Filtering, Review Creation/Management, Profile Customization, Basic Troubleshooting | Self-Guided Document Review |
| Developers (Maintenance Role) | IT Admin Guide [SDMP-REF-6], Technical Docs ([SDMP-REF-1], [SDMP-REF-2]), Test Procs [SDMP-REF-3], Requirements [SDMP-REF-4] | Environment Understanding, Architecture, Data Access, Testing, Requirements | Documentation Review, Pair Programming |

These resources provide the necessary knowledge base for different personnel interacting with the system.

## Software Tools Installation

Setting up the correct development and administration environment is the first step for any maintainer. This section provides a high-level checklist of the required software tools. For detailed, step-by-step installation and configuration instructions, maintainers must refer to Section 2 ("Initial Setup") of the IT Administrator Guide [SDMP-REF-6].

**Installation Checklist:**

*   [ ] **IDE:** Visual Studio or VS Code with ".NET" and "Azure" workloads.
*   [ ] **.NET SDK:** Project-specific version.
*   [ ] **MongoDB:** Local server or Cloud Atlas connection details [SDMP-REF-2].
*   [ ] **MongoDB Compass:** Database GUI tool.
*   [ ] **Git / GitHub Desktop:** Version control clients.
*   [ ] **Azure CLI (Optional):** Command-line interface for Azure.
*   [ ] **Repository Clone:** Local copy of `https://github.com/omniV1/CineScope.git`.
*   [ ] **Project Configuration:** Set up `appsettings.Development.json` (using secrets/Key Vault), build solution.

## Software Updates Workflow

Applying updates to the CineScope platform follows a defined workflow to ensure changes are implemented safely and effectively. This section outlines the standard sequence of steps from task assignment through to deployment and monitoring. More granular details on pipeline configuration and release management are in the IT Administrator Guide [SDMP-REF-6].

**Standard Update Workflow:**

1.  **Task Assignment:** A developer receives an assigned task or bug fix from Jira.
2.  **Sync Environment:** The developer updates their local `main` branch with the latest changes from the remote repository (`git pull`).
3.  **Branching:** A new Git branch is created specifically for the task, following project naming conventions.
4.  **Implementation:** The necessary code modifications are made, including writing or updating unit and integration tests [SDMP-REF-3].
5.  **Local Testing:** The developer runs the application and automated tests locally to confirm the changes work as expected and don't introduce regressions.
6.  **Version Control:** Changes are committed with descriptive messages and pushed to the remote repository on the specific branch.
7.  **Code Review:** A Pull Request is created on GitHub, targeting the `main` branch. Peers review the code for quality, correctness, and adherence to standards.
8.  **Merge:** After addressing review feedback and receiving approval(s), the Pull Request is merged into the `main` branch.
9.  **Automated Deployment:** The merge automatically triggers the configured GitHub Actions CI/CD pipeline. This pipeline builds the application, runs automated tests, and deploys the new version to the Azure environment.
10. **Monitor:** The deployment process is monitored via the CI/CD tool interface. Post-deployment, application health and logs are checked using Azure monitoring tools [SDMP-REF-6] to ensure the update was successful.

## Bug Reporting Process

A clear and efficient bug reporting process is essential for identifying and resolving issues promptly. This section outlines the procedures for reporting bugs found during different phases of the software lifecycle.

*   **Testing Phase Bugs:** These are bugs discovered before a release, during development or testing activities.
    1.  **Identification:** Issues identified through manual testing [SDMP-REF-3], automated test execution, or code reviews.
    2.  **Logging:** A new defect issue must be created in **Jira**.
    3.  **Details:** The Jira issue must include comprehensive information: precise steps to reproduce the bug, the expected behavior versus the observed actual behavior, details about the testing environment (browser, OS, etc.), and any relevant logs or screenshots.
    4.  **Management:** Defects are linked to relevant user stories or epics, prioritized by the team, and assigned to developers for resolution within the sprint structure.

*   **Production Bugs:** These are issues reported after the software has been released to end-users.
    1.  **Reporting/Detection:** Bugs may be reported by end-users (who should first consult the User Guide [SDMP-REF-5] for simple solutions) via designated support channels, or detected automatically by monitoring systems configured as per the IT Admin Guide [SDMP-REF-6].
    2.  **Verification & Logging:** All production reports are verified by the support or development team. Confirmed bugs are logged as high-priority defects in **Jira** with detailed information.
    3.  **Investigation:** Production bugs are assigned immediately for investigation to determine the root cause and impact.
    4.  **Resolution:** Based on severity, a fix is implemented either through an urgent hotfix deployment process or scheduled for the very beginning of the next available sprint.

Refer to Section 10 ("Troubleshooting") in the IT Administrator Guide [SDMP-REF-6] for more detailed troubleshooting steps and guidance.

---

## References

*   **[SDMP-REF-1]**: Technical Design Document. (`Documents/milestone3-Technical-Design.md`) - System architecture, components, database schema.
[Technical Design](https://github.com/omniV1/CineScope/blob/main/Documents/Technical-Reports/Technical-Design.md) 
*   **[SDMP-REF-2]**: MongoDB Integration Guide. (`Help/Milestone3_DevHelp.md`) - MongoDB setup, configuration, and usage.
[Dev help](https://github.com/omniV1/CineScope/blob/main/Documents/Help/Milestone3_DevHelp.md)
*   **[SDMP-REF-3]**: Test Procedures Document. (`Training/Test-procedures-gcu.md`) - Functional verification steps, pass/fail criteria.
*   [Test proceedures](https://github.com/omniV1/CineScope/blob/main/Documents/Training/Test-procedures-gcu.md)
*   **[SDMP-REF-4]**: Software Functional Requirements Document. (`Documents/milestone2-Functional-Requirements-document.md`) - Functional specifications, UI designs, requirements mapping.
*   [Functional Specifcations](https://github.com/omniV1/CineScope/blob/main/Documents/Technical-Reports/Functional-Requirements.md)
*   **[SDMP-REF-5]**: CineScope User Guide. (`Training/User-TrainingModule.md`) - End-user guidance on platform features.
*   [User Guide and FAQ's](https://github.com/omniV1/CineScope/blob/main/Documents/Training/User-TrainingModule.md)
*   **[SDMP-REF-6]**: CineScope Complete IT Administrator Guide. (`Training/IT-TrainingModule.md`) - Comprehensive IT instructions for deployment, monitoring, maintenance, etc.
[IT guide](https://github.com/omniV1/CineScope/blob/main/Documents/Training/IT-TrainingModule.md#2-initial-setup) 
