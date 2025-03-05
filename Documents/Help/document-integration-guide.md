# How to Update the Version Description Document with Jira Test Tickets

This straightforward guide explains exactly how to update your Version Description Document with the test tickets you create in Jira.

## Direct Links to Documents

- [Version Description Document](https://github.com/omniV1/CineScope/blob/main/Documents/milestone4-version-description.md) - Update this document with test ticket information
- [Test Procedures Document](https://github.com/omniV1/CineScope/blob/main/Documents/milestone4-test-procedures-gcu.md) - Reference this document in your updates
- [Test Tickets Guide](https://github.com/omniV1/CineScope/blob/main/Documents/Help/test-tickets.md) - Use this for creating the test tickets in Jira

## Step-by-Step Instructions

### Step 1: Create the Test Tickets in Jira

First, create all 10 test tickets in Jira as child tasks of the relevant user stories:
- SCRUM-19: Featured Movies (3 test tickets)
- SCRUM-27: User Authentication (3 test tickets)
- SCRUM-34: Review Management (4 test tickets)

Take note of the ticket IDs assigned by Jira (e.g., SCRUM-101, SCRUM-102, etc.).

### Step 2: Add a Test Tickets Table to the Version Description Document

In your Version Description Document, find Section 6 (Milestone Deliverables) and add this new subsection:

```markdown
### Test Tickets

The following test tickets have been created in Jira to verify the functionality of Sprint 1 user stories:

| Test Ticket ID | Test Description | Parent User Story | Story Points | Assignee |
|----------------|------------------|-------------------|--------------|----------|
| SCRUM-101 | Test Featured Movies Display | SCRUM-19 | 2 | Owen Lindsey |
| SCRUM-102 | Test Movie Navigation System | SCRUM-19 | 2 | Owen Lindsey |
| SCRUM-103 | Test Error Handling for Movie Display | SCRUM-19 | 2 | Owen Lindsey |
| SCRUM-104 | Test Successful Login Flow | SCRUM-27 | 2 | Owen Lindsey |
| SCRUM-105 | Test Failed Login and Error Handling | SCRUM-27 | 3 | Owen Lindsey |
| SCRUM-106 | Test Account Recovery | SCRUM-27 | 3 | Owen Lindsey |
| SCRUM-107 | Test Review Creation | SCRUM-34 | 2 | Owen Lindsey |
| SCRUM-108 | Test Review Editing and Deletion | SCRUM-34 | 2 | Owen Lindsey |
| SCRUM-109 | Test Content Filtering in Reviews | SCRUM-34 | 3 | Owen Lindsey |
| SCRUM-110 | Test User Profile Review Management | SCRUM-34 | 2 | Owen Lindsey |
```

⚠️ **Important**: Replace SCRUM-101, SCRUM-102, etc. with your actual Jira ticket IDs.

### Step 3: Update the User Story Table with Test Ticket References

In Section 3 (Project Status), update the user story table to show which test tickets are associated with each user story:

```markdown
| SCRUM ID | User Story | Story Points | Assigned To | Status | Test Tickets |
|----------|------------|--------------|-------------|--------|--------------|
| SCRUM-19 | As a user, I want to see featured movies... | 6 | Carter Wright | To Do | SCRUM-101, SCRUM-102, SCRUM-103 |
| SCRUM-27 | As a user, I want to be able to login an... | 4 | Owen Lindsey | To Do | SCRUM-104, SCRUM-105, SCRUM-106 |
| SCRUM-34 | As a user, I want to manage movie reviews... | 4 | Andrew Mack | To Do | SCRUM-107, SCRUM-108, SCRUM-109, SCRUM-110 |
```

### Step 4: Update Next Steps Section

In Section 7 (Next Steps), update the implementation plan to include test execution:

```markdown
| User Story ID | Implementation Tasks | Testing Tasks | Assigned To |
|---------------|----------------------|---------------|-------------|
| SCRUM-19 | Implement featured movies UI | Execute test tickets SCRUM-101, 102, 103 | Carter/Owen |
| SCRUM-27 | Develop authentication system | Execute test tickets SCRUM-104, 105, 106 | Owen |
| SCRUM-34 | Create review management | Execute test tickets SCRUM-107, 108, 109, 110 | Andrew/Owen |
```

### Step 5: Reference the Test Procedures Document

Add a clear reference to the Test Procedures document at the end of Section 6:

```markdown
All test tickets are based on the detailed procedures defined in the [Test Procedures Document](https://github.com/omniV1/CineScope/blob/main/Documents/milestone4-test-procedures-gcu.md), which provides comprehensive setup instructions, test steps, and pass/fail criteria.
```

## Before You Commit

✅ Double-check that you've:
- Used the correct Jira ticket IDs
- Included all 10 test tickets
- Properly linked each test ticket to its parent user story
- Added the test tickets table to the Milestone Deliverables section
- Updated the user story table with test ticket references
- Updated the Next Steps section with test execution tasks
- Added a reference to the Test Procedures document
