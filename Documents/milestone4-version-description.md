# Version Description Document

Version Description Document for CineScope Movie Review Platform

Owen Lindsey, Andrew Mack, Carter Wright, Rian Smart

Grand Canyon University: CST-326

March 3, 2025

---

A Version Description Document is used to identify the contents of a delivery. For this class, a scaled version of this is used to provide artifacts of your management process and to develop a SELF-SCAN FUNCTIONAL SPEC.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Overview](#2-system-overview)
3. [Project Status](#3-project-status)
4. [Sprint Metrics](#4-sprint-metrics)
5. [Risk Management Strategy](#5-risk-management-strategy)
6. [Milestone Deliverables](#6-milestone-deliverables)
7. [Next Steps](#7-next-steps)

## 1. Introduction

### Purpose of the Document

This Version Description Document summarizes the current delivery and project status for the CineScope movie review platform development. It serves as an artifact to identify the work planned by the CLC group for Sprint 1 and provides insights into project metrics, risks, and future planning.

### Scope of Delivery

This document covers the planning and preparation for Sprint 1 (February 24, 2025 - March 9, 2025), which focuses on the implementation of:

1. Featured Movies Display (SCRUM-19)
2. User Authentication (SCRUM-27)
3. Review Management (SCRUM-34)

Additionally, it includes all test procedures that have been developed to verify the functionality of these features and other core components of the CineScope platform.

## 2. System Overview

CineScope is a comprehensive movie review platform built using Blazor C# ASP.NET Core (MVC) with MongoDB as the database system. The application implements an N-layer architecture that promotes separation of concerns:

1. **Presentation Layer**: Handles user interface rendering and user input processing
2. **Business Logic Layer**: Implements core application functionality and business rules
3. **Data Access Layer**: Manages database interactions and data persistence
4. **Database Layer**: MongoDB database system for data storage

The system allows users to browse movies, read and write reviews, and interact with a community of movie enthusiasts. Core features include:

- User authentication and profile management
- Movie browsing with advanced filtering options
- Review creation and management
- Content moderation system
- Responsive design for mobile and desktop devices

## 3. Project Status

### Current Status Summary

| SCRUM ID | User Story | Story Points | Assigned To | Status |
|----------|------------|--------------|-------------|--------|
| SCRUM-19 | As a user, I want to see featured movies... | 6 | Carter Wright (CW) | To Do |
| SCRUM-27 | As a user, I want to be able to login an... | 4 | Owen Lindsey (O) | To Do |
| SCRUM-34 | As a user, I want to manage movie reviews... | 4 | Andrew Mack (AM) | To Do |
| **Total Sprint 1** | **3 User Stories** | **14** | **Team** | **To Do** |

### Current Velocity

The team is currently in Sprint 1 (February 24 - March 9, 2025) with a planned velocity of 14 story points across 3 user stories. Test procedures will be added as child tasks to each user story but have not yet been created in Jira.

### Burndown Chart Summary

Sprint 1 has just begun, and the team is in the early stages of implementation. The burndown chart shows that work is yet to begin on the planned user stories. Daily standups will be used to track progress and identify any blockers early.

## 4. Sprint Metrics

### Sprint 1 User Stories

| User Story ID | Description | Story Points | Status | Assigned To |
|---------------|-------------|--------------|--------|-------------|
| SCRUM-19 | As a user, I want to see featured movies... | 6 | To Do | Carter Wright (CW) |
| SCRUM-27 | As a user, I want to be able to login an... | 4 | To Do | Owen Lindsey (O) |
| SCRUM-34 | As a user, I want to manage movie reviews... | 4 | To Do | Andrew Mack (AM) |

### Planned Test Tasks (To Be Added)

Each user story will have associated test tasks that will be created in Jira. Based on our test procedures document, the following test areas will be covered:

| Test Area | Associated User Story | Estimated Points | Planned Test Types |
|-----------|------------------------|-------------------|-------------------|
| Featured Movies Display | SCRUM-19 | 3 | UI, Integration |
| User Authentication | SCRUM-27 | 3 | Unit, Integration, Security |
| Review Management | SCRUM-34 | 3 | Unit, Integration, UI |

### Git Activity Summary (Projected)

| Team Member | Role | Responsibilities |
|-------------|------|------------------|
| Owen Lindsey | Developer | Authentication system implementation, test procedure development |
| Andrew Mack | Developer | Review management, database integration |
| Carter Wright | Scrum Master | Featured movies UI, sprint coordination |
| Rian Smart | Product Owner | Requirements refinement, acceptance criteria verification |

### Test Coverage Goals

| Component | Target Coverage % | Unit Tests | Integration Tests | UI Tests |
|-----------|-----------------|------------|-------------------|----------|
| Featured Movies | 80% | 5 | 2 | 3 |
| User Authentication | 85% | 8 | 3 | 2 |
| Review Management | 80% | 6 | 2 | 2 |

## 5. Risk Management Strategy

### Sprint Capacity Management

To mitigate the risk of taking on an excessive number of tasks beyond scheduled capacity, we have implemented the following strategy for our first and upcoming sprints:

1. **Clear User Story Scope**: We have limited Sprint 1 to three well-defined user stories (SCRUM-19, SCRUM-27, SCRUM-34) with a total of 14 story points, which represents a manageable workload for our four-person team over a one-week sprint.

2. **Task Decomposition**: Each user story will be decomposed into specific implementation and testing tasks. No single user story exceeds 6 story points, making estimation more accurate and preventing hidden complexity.

3. **Test Procedure Development**: We have created comprehensive test procedures for each functional area, which will be added as formal tasks in Jira. This ensures that testing is allocated proper time and resources rather than being rushed at the end of the sprint.

4. **Daily Progress Tracking**: We will monitor progress through daily standups, tracking completion of tasks against our burndown chart to identify delays early.

5. **Buffer Allocation**: We have deliberately kept our total sprint commitment (14 story points) below our theoretical maximum capacity, providing a buffer for unexpected complexities or technical debt.

### Supporting Data

| Metric | Current Sprint (Sprint 1) | Next Sprint (Projected) |
|--------|---------------------------|-------------------------|
| Team Size | 4 developers | 4 developers |
| Sprint Duration | 2 weeks (Feb 24 - Mar 9) | 1 week (Mar 9 - Mar 16) |
| Planned Story Points | 14 | 14-16 (to be determined) |
| Story Points per Team Member | 3.5 | 3.5-4 |
| User Stories | 3 | 3-4 (to be determined) |
| Test Tasks | Planned (not yet in Jira) | Will be included in initial planning |

This capacity management approach is based on industry standards that suggest an individual can effectively manage 3-5 story points per week, depending on complexity. Our current allocation aligns with this guideline while providing room for the additional test tasks that will be created.

## 6. Milestone Deliverables

The following artifacts have been completed and delivered as part of this milestone:

1. **Documentation**:
   - Software Requirements Document (Completed)
   - Technical Design Document (Completed)
   - Test Procedures Document (Completed)
   - This Version Description Document (Completed)

2. **Project Setup**:
   - Jira Project Configuration (Completed)
   - User Stories Creation (Completed for Sprint 1)
   - Sprint Planning (Completed for Sprint 1)
   - GitHub Repository Setup (Completed)

3. **Design Artifacts**:
   - User Interface Wireframes (Completed)
   - Database Schema Design (Completed)
   - Architecture Diagrams (Completed)

4. **Test Planning**:
   - Test Procedures for Featured Movies (Completed)
   - Test Procedures for User Authentication (Completed)
   - Test Procedures for Review Management (Completed)
   - Test Procedures for Content Filtering (Completed)
   - Test Procedures for User Profile Management (Completed)

Note: Code implementation will begin during Sprint 1 execution based on the defined user stories (SCRUM-19, SCRUM-27, SCRUM-34).

## 7. Next Steps

### Sprint 1 Implementation Tasks

| User Story ID | Implementation Tasks | Testing Tasks | Assigned To |
|---------------|----------------------|---------------|-------------|
| SCRUM-19 | Implement featured movies UI components | Execute test procedures for movie browsing | Carter Wright |
| SCRUM-27 | Develop authentication system | Execute test procedures for user authentication | Owen Lindsey |
| SCRUM-34 | Create review management functionality | Execute test procedures for review system | Andrew Mack |

### Upcoming Sprints

Based on our Jira backlog, the following work is planned for upcoming sprints:

**Sprint 2 (March 9 - March 16, 2025)**
- SCRUM-45: "As a user, I want to be able to filter reviews..." (7 story points)

**Sprint 3 (March 16 - March 23, 2025)**
- SCRUM-39: "As a user, I want to view movies by genre..." (5 story points)

### Projected Timeline

| Milestone | Target Completion | Key Deliverables |
|-----------|-------------------|------------------|
| Sprint 1 Completion | March 9, 2025 | Featured Movies, Authentication, Review Management |
| Sprint 2 Completion | March 16, 2025 | Review Filtering |
| Sprint 3 Completion | March 23, 2025 | Genre-Based Movie Browsing |
| Final Project Delivery | March 30, 2025 | Complete CineScope Platform |

All test procedures have been developed in advance of implementation to ensure proper test coverage and quality assurance throughout the development process.
