# Software Functional Requirements Document

for CineScope Movie Review Platform

Owen, Andrew Mack, Carter Wright, Rian Smart

Grand Canyon University: CST-326

February 23, 2025

Document Version 2.0

## AUTHORS

| Name | Role | Department |
|------|------|------------|
| Carter Wright | Scrum Master | Development |
| Rian Smart | Product Owner | Management |
| Owen | Developer | Development |
| Andrew Mack | Developer | Development |

## DOCUMENT HISTORY

| Date | Version | Document Revision Description | Document Author |
|------|---------|------------------------------|-----------------|
| 02/11/25 | 1.0 | Initial creation of Software Requirements Document | Team CineScope |

## APPROVALS

| Approval Date | Approved Version | Approver Role | Approver |
|--------------|------------------|---------------|----------|
| | 1.0 | | |

# Table of Contents

# Table of Contents

1. [Introduction](#1-introduction) ........................................................................... 
   
   1.1. [Purpose of the Document](#11-purpose-of-the-document) .........................
   
   1.2. [Project Scope](#12-project-scope) .......................................................
   
   1.3. [Related Documents and Resources](#13-related-documents-and-resources) ..............
   
   1.4. [Terms/Acronyms and Definitions](#14-termsacronyms-and-definitions) .........
   
   1.5. [Risks and Assumptions](#15-risks-and-assumptions) ..................................
   
3. [System/Solutions Overview](#2-systemsolutions-overview) ............................
   
   2.1. [System Architecture and Communication](#21-system-architecture-and-communication) ...................
   
   2.2. [Site Navigation Structure](#22-site-navigation-structure) .............................
   
   2.3. [User Interface Design](#23-user-interface-design) .....................................
   
4. [Functional Specifications](#3-functional-specifications) ................................ 

   3.1. [Landing Page Implementation](#31-landing-page-implementation) .........................
   
   3.2. [Authentication System](#32-authentication-system) ........................................
   
   3.3. [Review Management](#33-review-management) .........................................
   
   3.4. [Content Filtering](#34-content-filtering) ................................................
    
   3.5. [Functional Requirements](#35-functional-requirements) ...............................
    
   3.6. [Non-Functional Requirements](#36-non-functional-requirements) .............................
   
5. [Implementation Plan](#4-implementation-plan) .............................................. 
   
6. [Quality Assurance](#5-quality-assurance) ......................................................  

 

# 1. Introduction

## 1.1 Purpose of the Document

This functional specification document provides comprehensive details about the implementation and behavior of the CineScope movie review platform. The document serves as a bridge between high-level user stories and detailed technical specifications, ensuring clear communication between stakeholders and development team members. By outlining functional requirements, technical constraints, and implementation approaches, this document facilitates the successful development of a robust movie review system.

## 1.2 Project Scope

CineScope addresses the need for a modern, user-friendly movie review platform that enables movie enthusiasts to share their opinions and discover new films. Built using C# ASP.NET Core Web App (MVC) with MongoDB as the database system, the platform delivers a responsive web interface where users can browse movies by various categories, write and manage reviews, and interact with other users' content.

The system implements comprehensive user authentication, ensuring secure access to personalized features while maintaining data privacy. Content moderation systems protect community standards through automated filtering and user feedback mechanisms. The platform's architecture emphasizes scalability and performance, supporting concurrent users while maintaining responsive interaction.

## 1.3 Related Documents and Resources

The requirements outlined in this document connect to a comprehensive set of project resources and documentation. Our development process utilizes several integrated platforms to manage different aspects of the project lifecycle.

Version Control and Source Code:
The project's source code is maintained in our GitHub repository at https://github.com/omniV1/CineScope. This repository contains the complete codebase, documentation, and development history.

Project Management:
Task tracking and sprint planning are managed through our Jira board at https://cinescopedev.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog. This platform coordinates our development efforts and tracks progress across all project components.

Technical Documentation:
Detailed technical documentation and project guidelines are maintained in our Confluence space at https://cinescopedev.atlassian.net/wiki/spaces/CineScope/overview. This wiki serves as our central knowledge base and provides additional context for implementation details.

Supporting Documentation:
- Functional Requirements Specifications (FR-1 through FR-5)
- Non-Functional Requirements Specifications (NFR-1 through NFR-5)
- Use Case Diagrams (UC-1 through UC-5)
- Sprint Planning Materials
- User Stories and Acceptance Criteria
- Technical Architecture Documentation

## 1.4 Terms/Acronyms and Definitions

| Term/Acronym | Definition | Description |
|--------------|------------|-------------|
| MVC | Model View Controller | Architectural pattern separating data, logic, and presentation |
| CRUD | Create Read Update Delete | Basic database operations for content management |
| FR | Functional Requirement | Specific behavior or function the system must perform |
| NFR | Non-Functional Requirement | Quality attributes and performance standards |
| UC | Use Case | Description of user interactions with the system |
| API | Application Programming Interface | Interface for component communication |
| UI | User Interface | Visual elements and interactions |
| DB | Database | Structured data storage system |

## 1.5 Risks and Assumptions

The development of CineScope carries several important considerations for implementation. The system must carefully manage performance under high concurrent user loads, particularly during review submission and content filtering operations. Security remains paramount, requiring robust protection of user authentication and content management systems. Data integrity must be maintained during concurrent operations, while system availability requires careful monitoring during peak usage periods.

The project operates under several foundational assumptions. Users are expected to access the platform through modern web browsers supporting current web standards. The chosen MongoDB infrastructure should provide sufficient performance characteristics for anticipated data volumes. Network conditions must support acceptable response times for regular operation. The development team brings necessary expertise in C# and MongoDB development to implement required functionality.

# 2. System/Solutions Overview

## 2.1 System Architecture and Communication

The CineScope platform implements a comprehensive communication structure to support effective team collaboration and development workflows. Figure 1 illustrates the primary communication channels and their purposes.

![Communication Pipeline](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Communication.png)

Figure 1: Team Communication Pathways

The development architecture follows an N-layer design pattern that promotes separation of concerns and maintainability. Figure 2 shows the architectural layers and their relationships.

![N-Layer Architecture](https://github.com/omniV1/CineScope/blob/main/Documents/Images/N-layer.png)
Figure 2: CineScope N-Layer Architecture

## 2.2 Site Navigation Structure

The platform implements a hierarchical navigation structure that organizes content and functionality logically for users. Figure 3 presents the complete site navigation map.

```mermaid
graph TD
    Landing[Landing Page] --> Movies[Movies by Category]
    Landing --> AuthSection[Authentication]
    Landing --> Search[Search]
    
    AuthSection --> Login[Login]
    AuthSection --> Register[Register]
    Login --> UserProfile[User Profile]
    Register --> UserProfile
    
    Movies --> Featured[Featured Movies]
    Movies --> Recent[Recently Added]
    Movies --> TopRated[Top Rated]
    
    UserProfile --> MyReviews[My Reviews]
    MyReviews --> CreateReview[Create Review]
    MyReviews --> EditReview[Edit Review]
    MyReviews --> DeleteReview[Delete Review]
    
    Movies --> MoviePage[Movie Details Page]
    MoviePage --> Reviews[Reviews Section]
    Reviews --> FilterReviews[Filter Reviews]
    Reviews --> SortReviews[Sort Reviews]
```
Figure 3: CineScope Site Navigation Map

### 2.3 User Interface Design

![Authentication Wireframes](https://github.com/omniV1/CineScope/blob/main/Documents/Images/WireFrames.png)


The following section presents detailed wireframe specifications for key system interfaces, organized by major functional areas and user interaction flows.

#### Landing Page Interface

The landing page serves as the primary entry point to CineScope, establishing the platform's theatrical aesthetic and brand identity. A dramatic red curtain motif frames the central "CINESCOPE" logo, immediately conveying the cinematic focus of the platform. The tagline "FOR MOVIE LOVERS, BY MOVIE LOVERS" reinforces the community-driven nature of the service. Below this, the platform's core value proposition appears in three commanding words: "EXPLORE. CONNECT. DISCOVER." Two prominent action buttons, "LOGIN" and "REGISTER", guide users toward immediate engagement with the platform.

#### Authentication System

**Login Interface**
The login interface maintains the established branding while streamlining the authentication process. Users encounter a clean, focused layout with clearly labeled input fields for username and password. Convenience features include "Forgot Username?" and "Forgot Password?" recovery options positioned near their relevant fields. Social media authentication alternatives appear as distinct buttons for Facebook, Google, and Apple ID integration. New users are guided to the registration process through a prominent link, while help center access remains readily available.

**Registration Interface**
The registration process implements a structured data collection form that prioritizes clarity and completeness. Users provide essential information through clearly labeled fields for name, email address, mobile number, and password with confirmation. The interface includes a mandatory terms and conditions acceptance checkbox. A prominent "Register" button appears once all required fields are complete. Help center access is maintained through a visible link, ensuring support throughout the registration process.

**Registration Success**
Upon successful registration, users receive immediate visual confirmation through a green checkmark indicator. The interface displays a countdown to automatic redirection while providing a manual redirect option. The help center link remains accessible during this transition phase.

#### Profile Management

**Profile Overview**
The profile management interface organizes user-specific functions into logical sections. Personal details and review collections are accessible through clear navigation elements. The interface includes profile image management and a straightforward logout option. This centralized hub allows users to manage all aspects of their platform presence.

**Profile Editing**
The profile editing interface presents user information in an editable format with clear interaction points. Each field – name, surname, username, password, email address, and mobile number – features an accompanying edit icon. The layout maintains consistency with the platform's overall design language while clearly indicating editable elements.

#### Review System

**Review Creation Interface**
The review creation process centers around the specific movie being reviewed, displaying the title and release year prominently. A five-star rating system allows for quick scoring, while a dedicated text area accepts detailed written feedback. The interface includes a persistent navigation bar for accessing key platform features during review composition.

**Review Management**
The "My Reviews" section presents a user's review history in a scrollable format. Each entry displays the movie title, year, star rating, and a preview of the written review. The interface allows for quick assessment of past contributions while maintaining easy access to full review content.

#### Movie Details

The movie details interface provides comprehensive film information through several distinct sections:

Primary Information
A prominent display of the movie poster accompanies basic details including title, release year, and language. The overall rating appears prominently near the top of the page.

Description Section
The movie's plot summary and key details appear in a clearly formatted text block, providing immediate context for viewers.

Cast Information
A horizontal scroll of cast member thumbnails allows quick recognition of key performers, each with associated character information.

Review Collection
User reviews appear in a dedicated section, displaying individual ratings and written feedback in a consistent format.

Similar Content
Related movie suggestions appear as thumbnail previews, encouraging further platform exploration.

#### Navigation Elements

The platform implements persistent navigation through a bottom-mounted bar containing four key elements:

Movie Catalog Access
Direct access to the full movie collection, organized by categories including top rated, new releases, and featured selections.

Search Functionality
A powerful search interface allows users to quickly locate specific content within the platform.

Bookmarking System
Users can maintain collections of favorite or saved content for future reference.

Profile Access
Quick access to user-specific functions and settings remains consistently available.

All interfaces maintain visual consistency through the platform's signature red and gray color scheme, ensuring a cohesive user experience across all functional areas. Typography and spacing follow established patterns that prioritize readability and clear information hierarchy.

# 3. Functional Specifications

## 3.1 Landing Page Implementation

The landing page serves as the primary entry point, presenting users with an intuitive interface for movie discovery. The implementation supports several distinct movie sections, including featured content, recently viewed films, and genre-based collections. Each section implements efficient content loading and navigation while maintaining responsive performance under load.

The page structure emphasizes modularity, enabling independent updates to specific sections without impacting overall functionality. Performance optimization includes appropriate caching strategies and lazy loading techniques to maintain quick response times. Error handling provides graceful degradation and clear user feedback when issues arise.

## 3.2 Authentication System

User authentication implements comprehensive security measures while maintaining usability. The login interface provides clear feedback and intuitive operation while enforcing robust security protocols. Password management includes secure recovery mechanisms and appropriate account protection measures such as temporary lockouts after failed attempts.

The system maintains secure session management with appropriate timeout handling and state synchronization. Authentication status remains clearly visible through consistent UI indicators. Integration with other system components ensures appropriate access control while maintaining security boundaries.

## 3.3 Review Management

The review system enables rich interaction with movie content through a comprehensive set of features. Users can create detailed reviews including ratings and textual feedback, with content filtering ensuring appropriate community standards. The interface supports both basic and advanced filtering options, allowing efficient content discovery and organization.

The implementation handles concurrent operations appropriately, maintaining data consistency while supporting multiple simultaneous users. Performance optimization ensures quick response times for common operations while maintaining system stability under load. Audit logging tracks significant operations while protecting user privacy.

## 3.4 Content Filtering

Content filtering maintains community standards through automated screening of user submissions. The system maintains an updated list of restricted content with automatic propagation and failover protection. Processing occurs within strict performance boundaries, completing analysis quickly while maintaining high accuracy rates.

The filtering engine implements sophisticated pattern matching while supporting efficient operation at scale. Error handling provides clear feedback when content requires modification. The system maintains detailed logging for administrative review while protecting user privacy.

## 3.5 Functional Requirements

The system's functional requirements encompass several key areas of functionality, each mapped to specific use cases and implementation tasks. These requirements define the core behaviors necessary for the CineScope platform to meet user needs effectively.


### Landing Page Requirements

| Use Case ID | Functional Req. ID | Functional Requirement | SCRUM ID |
|-------------|-------------------|---------------------|-----------|
| UC-1 | FR-1.1 | The System shall display movies on the Landing page | SCRUM-21 |
| UC-1 | FR-1.2 | The System shall display section for Features Movies | SCRUM-22 |
| UC-1 | FR-1.3 | The System shall display section for Recently Viewed Movies | SCRUM-23 |
| UC-1 | FR-1.4 | The System shall display section for Top-Rated Movies | SCRUM-24 |
| UC-1 | FR-1.5 | The System shall display section for Rom-Com Movies | SCRUM-24 |
| UC-1 | FR-1.6 | The System shall display section for Thriller/Horror Movies | SCRUM-24 |
| UC-1 | FR-1.7 | The System shall display section for Action Movies | SCRUM-24 |
| UC-1 | FR-1.8 | The System shall display section for Sci-FI Movies | SCRUM-24 |
| UC-1 | FR-1.9 | The System shall display section for Recently Added Movies | |
| UC-1 | FR-1.10 | The System shall redirect the user to a movie's page if clicked | SCRUM-25 |
| UC-1 | FR-1.11 | The System shall display an error message and log the user out if an error occurs | SCRUM-26 |

### Review Management Requirements

| Use Case ID | Functional Req. ID | Functional Requirement | SCRUM ID |
|-------------|-------------------|---------------------|-----------|
| UC-2 | FR-2.1 | The system shall provide a "Filters" button on the movie reviews page | SCRUM-46 |
| UC-2 | FR-2.2 | The system shall allow users to select a filter category | SCRUM-46 |
| UC-2 | FR-2.3 | The system shall update the displayed reviews based on the selected filter | SCRUM-48 |
| UC-2 | FR-2.4 | The system shall provide a "Sort" button on the movie reviews page | SCRUM-47 |
| UC-2 | FR-2.5 | The system shall allow users to select a sorting option | SCRUM-47 |
| UC-2 | FR-2.6 | The system shall update the displayed reviews based on the selected sorting order | SCRUM-49 |
| UC-2 | FR-2.7 | The system shall display a message if no reviews match the selected filter | SCRUM-50 |
| UC-2 | FR-2.8 | The system shall display an error message if an error occurs while filtering | SCRUM-50 |

### User Review Operations

| Use Case ID | Functional Req. ID | Functional Requirement | SCRUM ID |
|-------------|-------------------|---------------------|-----------|
| UC-3 | FR-3.1 | The system shall provide a "Create Review" page | SCRUM-35 |
| UC-3 | FR-3.2 | The system shall allow users to input movie title, rating, and review text | SCRUM-35 |
| UC-3 | FR-3.3 | The system shall validate the review input for required fields | SCRUM-35 |
| UC-3 | FR-3.4 | The system shall apply content filter to review text | SCRUM-35 |
| UC-3 | FR-3.5 | The system shall save review to database if it passes filter | SCRUM-35 |
| UC-3 | FR-3.6 | The system shall display confirmation message upon successful creation | SCRUM-35 |
| UC-3 | FR-3.7 | The system shall display error message if review fails filter | SCRUM-35 |

### Authentication Requirements

| Use Case ID | Functional Req. ID | Functional Requirement | SCRUM ID |
|-------------|-------------------|---------------------|-----------|
| UC-4 | FR-4.1 | The system shall allow users to login with unique credentials | SCRUM-28 |
| UC-4 | FR-4.2 | The system shall lock account after three failed attempts | SCRUM-29 |
| UC-4 | FR-4.3 | The system shall provide password reset functionality | SCRUM-30 |
| UC-4 | FR-4.4 | The system shall allow users to log out | SCRUM-31 |
| UC-4 | FR-4.5 | The system shall handle all authentication errors appropriately | SCRUM-32 |
| UC-4 | FR-4.6 | The system shall validate credentials against database | SCRUM-29 |
| UC-4 | FR-4.7 | The system shall ensure account security through monitoring | SCRUM-33 |

### Content Filtering Requirements

| Use Case ID | Functional Req. ID | Functional Requirement | SCRUM ID |
|-------------|-------------------|---------------------|-----------|
| UC-5 | FR-5.1 | The system shall maintain a list of banned words and phrases | SCRUM-40 |
| UC-5 | FR-5.2 | The system shall check review text against banned list | SCRUM-41 |
| UC-5 | FR-5.3 | The system shall flag reviews containing inappropriate content | SCRUM-41 |
| UC-5 | FR-5.4 | The system shall allow users to view the banned word list | SCRUM-44 |

### Performance Requirements

| ID | Non-Functional Requirement | SCRUM ID |
|----|---------------------------|-----------|
| NFR-1.1 | The system shall load the landing page within 3 seconds | SCRUM-21 |
| NFR-1.2 | The system shall support 100 concurrent users | SCRUM-21 |
| NFR-1.3 | The system shall be fully responsive on mobile and desktop | SCRUM-21 |
| NFR-1.4 | The system shall use modular design for easy updates | SCRUM-21 |
| NFR-1.6 | The system shall support the latest two versions of major browsers | SCRUM-21 |

### Review System Requirements

| ID | Non-Functional Requirement | SCRUM ID |
|----|---------------------------|-----------|
| NFR-2.1 | The system shall update filtered reviews within 2 seconds | SCRUM-36 |
| NFR-2.2 | The system shall support 100 concurrent users filtering reviews | SCRUM-36 |
| NFR-2.3 | The system shall maintain 300ms maximum latency for queries | SCRUM-36 |
| NFR-2.4 | The system shall limit users to 100 filter requests per hour | SCRUM-36 |
| NFR-2.5 | The system shall maintain 90-day audit logs | SCRUM-36 |

### Content Management Requirements

| ID | Non-Functional Requirement | SCRUM ID |
|----|---------------------------|-----------|
| NFR-5.1 | Content filter shall process text within 200ms up to 5000 characters | SCRUM-41 |
| NFR-5.2 | System shall maintain 99.9% content filter accuracy | SCRUM-41 |
| NFR-5.3 | Banned word list shall update within 30 seconds across instances | SCRUM-40 |
| NFR-5.4 | System shall log filter rejections within 100ms | SCRUM-43 |
| NFR-5.5 | System shall maintain backup with 5-second failover | SCRUM-40 |


# 4. Implementation Plan

The development schedule organizes work into focused sprints, with clear dependencies and delivery milestones. Implementation is structured around core functional areas with specific Jira tasks tracking progress.

## Landing Page Development (SCRUM-19 through SCRUM-26)
The landing page implementation establishes the foundation for user interaction. Development encompasses our base layout creation, featured movies section, recently viewed movies, genre-based sections, and movie navigation systems. Error handling and recovery mechanisms ensure system reliability across all landing page components.

## Authentication System (SCRUM-27 through SCRUM-33)
User authentication development provides comprehensive security and access control. This includes login interface creation, core authentication logic, password recovery system, and logout functionality. The system incorporates robust error handling and state management to maintain secure user sessions.

## Review Management System (SCRUM-34 through SCRUM-38, SCRUM-45 through SCRUM-50)
The review system implementation delivers both core review functionality and advanced filtering capabilities. The first phase implements basic review operations including form creation, display functionality, update capabilities, and deletion mechanisms. The second phase enhances user interaction through comprehensive filtering and sorting systems, including interface development, logic implementation, and error handling. All components integrate with content filtering to maintain community standards.

## Content Filtering System (SCRUM-39 through SCRUM-44)
Content filtering development ensures appropriate content standards through comprehensive word management, processing engines, API implementation, logging systems, and user feedback mechanisms. This system provides the foundation for maintaining community guidelines across all user-generated content.

# 5. Quality Assurance

Testing encompasses functional verification, performance validation, and security assessment. Each component undergoes comprehensive testing to ensure compliance with both functional and non-functional requirements. Integration testing verifies proper component interaction, while load testing confirms performance under expected usage patterns.
