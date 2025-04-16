# Guide to Creating Test Tickets in Jira for CineScope Sprint 1

## Introduction

This document contains detailed test tickets for Sprint 1 of the CineScope project. The test tickets are organized by user story and follow a consistent format designed to provide comprehensive testing coverage for each functional area.

## How to Create These Test Tickets in Jira

### Step 1: Navigate to the Right Location
1. Log in to Jira at [cinescopedev.atlassian.net](https://cinescopedev.atlassian.net)
2. Navigate to the CineScope project
3. Go to the Backlog view

### Step 2: Create Child Tasks for Each User Story
For each test ticket, you'll create a subtask (child task) under the main user story:

1. Find the parent user story (SCRUM-19, SCRUM-27, or SCRUM-34)
2. Click on the "..." menu next to the user story
3. Select "Create subtask"

### Step 3: Fill in the Ticket Information
For each test ticket, complete the following fields:

- **Summary**: Use the title from the test ticket (e.g., "Test Featured Movies Display")
- **Description**: Copy the entire description section from the test ticket document
- **Assignee**: Assign to yourself (as the QA resource)
- **Story Points**: Use the estimated effort points from the "Additional Information" section
- **Priority**: Set according to the priority listed in the "Additional Information" section
- **Labels**: Add "testing", "QA", and any other relevant labels

### Step 4: Formatting the Description
To ensure proper formatting in Jira:

1. When pasting the description, use Jira's markdown or rich text editor
2. For tables, use Jira's table formatter (or paste as markdown)
3. For images, you may need to:
   - Upload them as attachments to the Jira ticket
   - Update the image references in the description to point to the attachments

### Step 5: Linking the Tickets
Make sure each test ticket is properly linked:

1. In the "Linked Issues" section of the ticket
2. Click "Link issue"
3. Select "tests" as the link type
4. Link to the appropriate implementation subtask (e.g., link "Test Featured Movies Display" to SCRUM-22 "Featured movies")

### Step 6: Setting the Workflow Status
Set each ticket to the appropriate status:

1. Initially set to "To Do"
2. They will move through "In Progress" to "Done" as testing is completed

## Important Notes
- All test tickets should be created as subtasks of their parent user story, not as subtasks of implementation tasks
- Keep the same hierarchical level as the implementation subtasks
- Make sure each test is linked to the specific implementation task(s) it tests
- Maintain the standard format (Objective, Test Procedures, Pass/Fail Criteria) for all test tickets

## Test Tickets for Sprint 1

## SCRUM-19: Featured Movies Test Tickets

### Test Ticket 1: Test Featured Movies Display

**Title:** SCRUM-XX: Test Featured Movies Display

**Description:**

#### Objective
Verify that the featured movies section displays correctly on the landing page and that all UI components function as expected.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify test data includes at least 5 featured movies
3. Clear browser cache and cookies
4. Navigate to the CineScope landing page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Observe featured movies carousel | Carousel displays with featured movies |
| 2 | Verify movie information | Each movie shows title, year, and rating |
| 3 | Test carousel navigation controls | Left/right controls navigate through movies |
| 4 | Test responsive behavior | Verify display adapts to different screen sizes |
| 5 | Click on a featured movie | User is redirected to movie details page |
| 6 | Test with empty featured list | System displays appropriate message |

##### 3. Pass/Fail Criteria
1. If featured movies display correctly:
   1. All featured movies are visible in the carousel, pass
   2. Movie titles, years, and ratings are correctly displayed, pass
   3. Carousel navigation controls work as expected, pass
   
   ![Movies Browsing Wireframe](https://github.com/omniV1/CineScope/blob/Michael-Dev/Documents/Images/Movies_Browsing_Wireframe.png)

2. If responsive design works correctly:
   1. Layout adjusts appropriately for desktop, tablet, and mobile, pass
   2. All UI elements remain accessible on smaller screens, pass

3. If navigation functions correctly:
   1. Clicking a movie redirects to the correct movie details page, pass
   2. Appropriate error handling occurs for any issues, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| UI Display | All featured movies visible with correct information | □ Pass □ Fail | |
| Navigation | Carousel controls function correctly | □ Pass □ Fail | |
| Responsiveness | Displays correctly on all screen sizes | □ Pass □ Fail | |
| Error Handling | Appropriate messages for edge cases | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-19, SCRUM-22
- Estimated effort: 2 story points
- Priority: High

---

### Test Ticket 2: Test Movie Navigation System

**Title:** SCRUM-XX: Test Movie Navigation System

**Description:**

#### Objective
Verify that the navigation between different movie sections and individual movie details functions correctly and intuitively.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify test data includes multiple movie categories
3. Clear browser cache and cookies
4. Navigate to the CineScope landing page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Click on a movie category tab | Category-specific movies display |
| 2 | Navigate between different categories | Each category loads correct content |
| 3 | Test "See All" functionality | Complete category list view displays |
| 4 | Test breadcrumb navigation | Breadcrumbs show correct path |
| 5 | Use back navigation | System returns to previous view |
| 6 | Test navigation from search results | System navigates to correct movie |

##### 3. Pass/Fail Criteria
1. If category navigation works correctly:
   1. Clicking category tabs displays appropriate content, pass
   2. "See All" link shows complete category listing, pass
   
   ![Search Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Search_Wireframe.png)

2. If movie detail navigation works correctly:
   1. Clicking a movie redirects to the correct movie details page, pass
   2. Breadcrumb navigation displays current location, pass
   3. Back navigation returns to previous view, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Category Navigation | Tabs correctly filter content | □ Pass □ Fail | |
| Detail Navigation | Clicking movies opens correct detail page | □ Pass □ Fail | |
| History Navigation | Back button works correctly | □ Pass □ Fail | |
| Breadcrumbs | Display correct navigation path | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-19, SCRUM-25
- Estimated effort: 2 story points
- Priority: High

---

### Test Ticket 3: Test Error Handling and Edge Cases

**Title:** SCRUM-XX: Test Error Handling and Edge Cases for Movie Display

**Description:**

#### Objective
Verify that the system properly handles error conditions and edge cases in the movie display system.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Prepare test scenarios for various error conditions
3. Clear browser cache and cookies
4. Navigate to the CineScope landing page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Test with no movies available | System displays appropriate message |
| 2 | Test with image loading failures | System shows placeholder images |
| 3 | Test with partial data (missing fields) | System handles missing data gracefully |
| 4 | Simulate network interruption | System shows appropriate error message |
| 5 | Test with extremely long movie titles | UI handles text overflow appropriately |
| 6 | Test with different languages/character sets | Non-English characters display correctly |

##### 3. Pass/Fail Criteria
1. If empty state handling works correctly:
   1. "No movies available" message displays when appropriate, pass
   2. UI remains usable and doesn't break, pass

2. If error handling works correctly:
   1. Network errors show appropriate message, pass
   2. Image loading failures show placeholder, pass
   3. Missing data fields are handled gracefully, pass

3. If edge cases are handled correctly:
   1. Long text is truncated or wrapped appropriately, pass
   2. International character sets render correctly, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Empty States | No-data messages display correctly | □ Pass □ Fail | |
| Network Errors | Error messages are user-friendly | □ Pass □ Fail | |
| Data Integrity | System handles incomplete data | □ Pass □ Fail | |
| Text Handling | Long text and special characters handled | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-19, SCRUM-26
- Estimated effort: 2 story points
- Priority: Medium

---

## SCRUM-27: User Authentication Test Tickets

### Test Ticket 4: Test Successful Login Flow

**Title:** SCRUM-XX: Test Successful Login Flow

**Description:**

#### Objective
Verify that users can successfully log in with valid credentials and that the system properly authenticates and redirects users.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify test user accounts exist in the database:
   - Regular user: testuser / Test@123
   - Admin user: adminuser / Admin@123
3. Clear browser cache and cookies
4. Navigate to the CineScope login page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Enter valid username "testuser" | Field accepts input |
| 2 | Enter valid password "Test@123" | Field masks input appropriately |
| 3 | Click "Login" button | System processes credentials |
| 4 | Observe redirect after login | User redirected to home page |
| 5 | Check for user-specific elements | User profile and options visible |
| 6 | Test with admin credentials | Admin redirected to admin dashboard |
| 7 | Test "Remember Me" functionality | Session persists across browser restarts |

##### 3. Pass/Fail Criteria
1. If login process works correctly:
   1. User successfully authenticated with valid credentials, pass
   2. User redirected to appropriate page based on role, pass
   3. User-specific UI elements displayed, pass
   
   ![Login Wireframe](https://github.com/omniV1/CineScope/blob/Owen-Dev/Documents/Images/Login_Wireframe.png)

2. If session management works correctly:
   1. "Remember Me" functionality maintains session, pass
   2. Session timeout works as expected, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Authentication | Valid credentials authenticate successfully | □ Pass □ Fail | |
| Redirection | User redirected to appropriate page | □ Pass □ Fail | |
| UI Updates | User-specific UI elements appear | □ Pass □ Fail | |
| Session Management | Sessions persist appropriately | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-27
- Estimated effort: 2 story points
- Priority: Critical

---

### Test Ticket 5: Test Failed Login and Error Handling

**Title:** SCRUM-XX: Test Failed Login and Error Handling

**Description:**

#### Objective
Verify that the system properly handles failed login attempts, displays appropriate error messages, and implements security features like account lockout.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify test user accounts exist in the database
3. Clear browser cache and cookies
4. Navigate to the CineScope login page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Enter valid username with invalid password | System rejects login |
| 2 | Enter non-existent username | System rejects login |
| 3 | Leave username/password fields empty | Validation prevents submission |
| 4 | Test SQL injection patterns in fields | Input sanitized, login rejected |
| 5 | Attempt login multiple times with wrong password | Account locks after defined attempts |
| 6 | Test XSS vulnerability in login fields | Input sanitized, no script execution |

##### 3. Pass/Fail Criteria
1. If error handling works correctly:
   1. Appropriate error messages display for invalid credentials, pass
   2. Messages don't reveal whether username or password was incorrect, pass
   
   ![Registration Successful Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Registration_Successful_Wireframe.png)

2. If security features work correctly:
   1. Account locks after defined number of failed attempts, pass
   2. System resists common attack patterns, pass
   3. User notified of account lockout, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Error Messages | Clear but secure error messages | □ Pass □ Fail | |
| Input Validation | Required fields validated | □ Pass □ Fail | |
| Account Lockout | Locks after multiple failures | □ Pass □ Fail | |
| Security | Resists injection and XSS attempts | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-27
- Estimated effort: 3 story points
- Priority: Critical

---

### Test Ticket 6: Test Account Recovery

**Title:** SCRUM-XX: Test Account Recovery

**Description:**

#### Objective
Verify that the account recovery process functions correctly, allowing users to reset passwords and regain access to locked accounts.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify test user accounts exist in the database
3. Configure email testing environment
4. Navigate to the CineScope login page

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Click "Forgot Password" link | Recovery form displays |
| 2 | Enter valid email address | System sends recovery email |
| 3 | Check email for recovery link | Email received with secure link |
| 4 | Follow recovery link | Password reset form displays |
| 5 | Enter and confirm new password | System accepts new password |
| 6 | Test login with new password | Login successful with new password |
| 7 | Test old password | Old password no longer works |

##### 3. Pass/Fail Criteria
1. If password recovery works correctly:
   1. Recovery email sent to valid email address, pass
   2. Recovery link works and displays reset form, pass
   3. New password can be set, pass
   
   ![Registration Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Registration_Wireframe.png)

2. If security features work correctly:
   1. Old password no longer works after reset, pass
   2. Recovery links expire after use or timeout, pass
   3. Brute force protections on recovery form, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Recovery Email | Sent promptly with secure link | □ Pass □ Fail | |
| Reset Process | Form works and accepts new password | □ Pass □ Fail | |
| Security | Old passwords invalidated | □ Pass □ Fail | |
| Expiration | Links expire appropriately | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-27
- Estimated effort: 3 story points
- Priority: High

---

## SCRUM-34: Review Management Test Tickets

### Test Ticket 7: Test Review Creation

**Title:** SCRUM-XX: Test Review Creation

**Description:**

#### Objective
Verify that authenticated users can successfully create movie reviews with ratings and text content.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Log in with a test user account
3. Navigate to a movie details page
4. Click the "Write a Review" button

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Select a star rating (1-5) | Rating selection highlighted |
| 2 | Enter review text | Text area accepts input |
| 3 | Submit review | System processes and saves review |
| 4 | Verify review appears on movie page | New review displayed with username |
| 5 | Test with minimum required fields | System enforces required fields |
| 6 | Test character limits | System enforces maximum length |

##### 3. Pass/Fail Criteria
1. If review creation works correctly:
   1. User can select star rating, pass
   2. User can enter and submit review text, pass
   3. Review appears on the movie page after submission, pass
   
   ![Review System Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Review_System_Wireframe.png)

2. If validation works correctly:
   1. System enforces required fields, pass
   2. System enforces character limits, pass
   3. User receives confirmation of successful submission, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Rating Selection | Star rating can be selected | □ Pass □ Fail | |
| Text Input | Review text can be entered | □ Pass □ Fail | |
| Submission | Review successfully saves | □ Pass □ Fail | |
| Validation | Required fields enforced | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-34
- Estimated effort: 2 story points
- Priority: High

---

### Test Ticket 8: Test Review Editing and Deletion

**Title:** SCRUM-XX: Test Review Editing and Deletion

**Description:**

#### Objective
Verify that users can edit and delete their own reviews, and that appropriate permissions are enforced.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Log in with a test user account that has existing reviews
3. Navigate to the user's review history or a movie with their review

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Locate an existing review | Edit and delete options visible |
| 2 | Click edit button | Edit form displays with current content |
| 3 | Modify rating and text | Form updates with new content |
| 4 | Submit changes | System updates the review |
| 5 | Click delete button | Confirmation dialog appears |
| 6 | Confirm deletion | Review is removed from system |
| 7 | Test editing another user's review | Edit option not available |

##### 3. Pass/Fail Criteria
1. If review editing works correctly:
   1. User can edit their own reviews, pass
   2. Updated content displays after submission, pass
   3. User cannot edit others' reviews, pass
   
   ![My Reviews Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/My_Reviews_Wireframe.png)

2. If review deletion works correctly:
   1. User can delete their own reviews, pass
   2. Deleted reviews no longer appear, pass
   3. Confirmation required before deletion, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Edit Functionality | Can modify existing reviews | □ Pass □ Fail | |
| Delete Functionality | Can remove user's reviews | □ Pass □ Fail | |
| Permissions | Can't modify others' reviews | □ Pass □ Fail | |
| UI Updates | Changes reflect immediately | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-34
- Estimated effort: 2 story points
- Priority: Medium

---

### Test Ticket 9: Test Content Filtering in Reviews

**Title:** SCRUM-XX: Test Content Filtering in Reviews

**Description:**

#### Objective
Verify that the content filtering system properly screens review text for inappropriate content before publication.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Verify banned word list is populated in the database
3. Log in with a test user account
4. Navigate to a movie details page
5. Click the "Write a Review" button

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Submit review with clean content | Review passes filtering |
| 2 | Submit review with banned words | Review flagged or rejected |
| 3 | Submit review with borderline content | System behavior matches policy |
| 4 | Test filter with special characters | Evasion attempts detected |
| 5 | Test with different languages | International banned content detected |
| 6 | Test admin override for flagged content | Admin can approve/reject |

##### 3. Pass/Fail Criteria
1. If content filtering works correctly:
   1. Clean content passes without issue, pass
   2. Banned content is flagged or rejected, pass
   3. User receives appropriate notification, pass
   
   ![Movie Details Wireframes](https://github.com/omniV1/CineScope/blob/Michael-Dev/Documents/Images/Movie_Details_Wireframes.png)

2. If security features work correctly:
   1. Filter detects evasion attempts, pass
   2. International banned content is detected, pass
   3. Admin override functions work, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Clean Content | Passes without issue | □ Pass □ Fail | |
| Banned Content | Properly flagged or rejected | □ Pass □ Fail | |
| Evasion Detection | Catches obfuscation attempts | □ Pass □ Fail | |
| Admin Controls | Override functions work | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-34
- Estimated effort: 3 story points
- Priority: High

---

### Test Ticket 10: Test User Profile Review Management

**Title:** SCRUM-XX: Test User Profile Review Management

**Description:**

#### Objective
Verify that users can view, manage, and navigate their review history from their user profile.

#### Test Procedures

##### 1. Setup
1. Ensure the CineScope application is running in a test environment
2. Log in with a test user account that has multiple reviews
3. Navigate to the user profile section

##### 2. Execute
| Step | Action | Expected Result |
|------|--------|----------------|
| 1 | Access "My Reviews" section | Review history displays |
| 2 | Test sorting options | Reviews sort as specified |
| 3 | Test filtering options | Reviews filter as specified |
| 4 | Click on a review | Navigates to associated movie |
| 5 | Test pagination | Pagination controls work |
| 6 | Test bulk actions if available | Can perform actions on multiple reviews |

##### 3. Pass/Fail Criteria
1. If review management works correctly:
   1. All user reviews are listed, pass
   2. Sorting and filtering functions work, pass
   3. Navigation to movie pages works, pass
   
   ![Edit Profile Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Edit_Profile_Wireframe.png)

2. If UI/UX works correctly:
   1. Review list is readable and well-formatted, pass
   2. Pagination works for many reviews, pass
   3. Actions are intuitive and accessible, pass

| Test Area | Criteria | Result | Notes |
|-----------|----------|--------|-------|
| Review Display | Complete history shown | □ Pass □ Fail | |
| Sorting/Filtering | Controls work correctly | □ Pass □ Fail | |
| Navigation | Links to movie pages work | □ Pass □ Fail | |
| Usability | Interface is intuitive | □ Pass □ Fail | |

#### Additional Information
- Related to: SCRUM-34
- Estimated effort: 2 story points
- Priority: Medium