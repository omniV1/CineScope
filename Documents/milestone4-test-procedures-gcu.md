# SELF-SCAN FUNCTIONAL SPEC

Version Description Document for CineScope Movie Review Platform

Owen Lindsey, Andrew Mack, Carter Wright, Rian Smart

Grand Canyon University: CST-326

March 3, 2025


# Test Procedures Document

## AUTHORS

| Name | Role | Department |
|------|------|------------|
| Carter Wright | Scrum Master | Development |
| Rian Smart | Product Owner | Management |
| Owen Lindsey | Developer | Development |
| Andrew Mack | Developer | Development |

## DOCUMENT HISTORY

| Date | Version | Document Revision Description | Document Author |
|------|---------|------------------------------|-----------------|
| 03/03/25 | 1.0 | Initial creation of Test Procedures Document | Team CineScope |

## APPROVALS

| Approval Date | Approved Version | Approver Role | Approver |
|--------------|------------------|---------------|----------|
| | 1.0 | | |

## I. Introduction

### Purpose of the Document

This document outlines the test procedures for verifying the functionality of the CineScope movie review platform. It provides detailed procedures for testing key functional behaviors of the application, ensuring that all components meet the specified requirements and deliver the expected user experience.

### Scope

This document covers the testing procedures for five core functional behaviors of the CineScope application, with detailed test cases for each functionality. The procedures include setup instructions, execution steps, and clear pass/fail criteria to ensure comprehensive testing coverage.

## II. Functional Requirements for Testing

The following table outlines the key functional requirements being tested:

| Use Case ID | FR ID | Functional Requirement | SCRUM ID |
|-------------|-------|----------------------| -------- |
| UC-4 | FR-4.1 | The system shall allow users to login with unique credentials | SCRUM-28 |
| UC-2 | FR-2.1 | The system shall provide filters for movie browsing and discovery | SCRUM-46 |
| UC-3 | FR-3.1 | The system shall provide a "Create Review" functionality | SCRUM-35 |
| UC-5 | FR-5.2 | The system shall check review text against banned content list | SCRUM-41 |
| UC-4 | FR-4.7 | The system shall ensure account security through monitoring | SCRUM-33 |

## III. Test Procedures

### User Authentication (Login) - FR-4.1

1. [Setup](#setup)
   1. Ensure the CineScope application is running in a test environment
   2. Verify MongoDB database is accessible with test user accounts:
      - testuser / Test@123 / testuser@cinescope.test
      - lockeduser / Test@123 / lockeduser@cinescope.test
   3. Clear browser cookies and cache
   4. Navigate to the CineScope landing page
   5. Click the "LOGIN" button to access the login page

2. [Testing](#testing)
   
   | Step | Action | Expected Result |
   |------|--------|----------------|
   | 1 | Enter username: "testuser" | Field accepts input |
   | 2 | Enter password: "Test@123" | Field accepts masked input |
   | 3 | Click "Login" button | System processes request |
   | 4 | Observe system response | User authenticated and redirected |
   | 5 | Enter username: "testuser" | Field accepts input |
   | 6 | Enter password: "wrongpassword" | Field accepts masked input |
   | 7 | Click "Login" button | System displays error message |
   | 8 | Repeat steps 5-7 twice more | Account lockout on third attempt |

3. [Pass/Fail](#passfail)
   1. If valid credentials are entered:
      1. User is authenticated and redirected to the home page, pass
      2. User-specific navigation options are displayed, pass
      
      ![Login Wireframe](https://github.com/omniV1/CineScope/blob/Owen-Dev/Documents/Images/Login.png)

   2. If not, fail

   3. If invalid password is entered three times:
      1. System displays error message "Invalid username or password", pass
      2. After three failed attempts, system displays account lockout message, pass
      
      ![Registration Successful Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Register.png)

   4. If not, fail

   | FR ID | Criteria Description | Result | Notes |
   |-------|---------------------|--------|-------|
   | FR-4.1 | User successfully authenticated with credentials | □ Pass □ Fail | |
   | FR-4.6 | System validates credentials against database | □ Pass □ Fail | |
   | FR-4.2 | System locks account after three failed attempts | □ Pass □ Fail | |
   | FR-4.5 | System provides appropriate error feedback | □ Pass □ Fail | |

### Movie Browsing - FR-2.1

1. [Setup](#setup)
   1. Navigate to the CineScope website
   2. Log in with valid credentials
   3. Go to the main movie browsing page
   
![Landing page](https://github.com/omniV1/CineScope/blob/Michael-Dev/Documents/Images/Landing-Page-logged-in.png)

1. [Testing](#testing)

   | Step | Action | Expected Result |
   |------|--------|----------------|
   | 1 | Click on "Categories" dropdown | Menu expands with genre options |
   | 2 | Select a specific genre (e.g., "Action") | Movies filtered by selected genre |
   | 3 | Click on "Filters" button | Filter panel opens |
   | 4 | Set rating filter to "4+ stars" | Filter applied |
   | 5 | Click "Apply" button | Results updated with both filters |

2. [Pass/Fail](#passfail)
   1. If category selection filters movies correctly:
      1. Only movies from the selected category are displayed, pass
      2. Category heading is updated to reflect selection, pass
      
      ![Movies Browsing Wireframe](https://github.com/omniV1/CineScope/blob/Michael-Dev/Documents/Images/Genre-Filter.png)

   2. If not, fail

   3. If rating filter works correctly:
      1. Only movies with 4+ star ratings are displayed, pass
      2. Filter indicator shows active filters, pass
      
      ![Search Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Search-Movie.png)

   4. If not, fail

   | FR ID | Criteria Description | Result | Notes |
   |-------|---------------------|--------|-------|
   | FR-2.1 | System provides filter functionality | □ Pass □ Fail | |
   | FR-2.2 | System allows filter category selection | □ Pass □ Fail | |
   | FR-2.3 | System updates results based on filters | □ Pass □ Fail | |

### Review Creation - FR-3.1

1. [Setup](#setup)
   1. Navigate to the CineScope website
   2. Log in with valid credentials
   3. Browse to a specific movie details page
   4. Click "Write a Review" button

2. [Testing](#testing)

   | Step | Action | Expected Result |
   |------|--------|----------------|
   | 1 | Select a 4-star rating | Stars highlighted up to 4th star |
   | 2 | Enter valid review text | Field accepts text input |
   | 3 | Click "Submit Review" button | Review processed and saved |
   | 4 | Enter review with banned word | Field accepts text input |
   | 5 | Click "Submit Review" button | System flags inappropriate content |

3. [Pass/Fail](#passfail)
   1. If valid review is submitted:
      1. Review is saved and displayed on the movie page, pass
      2. User receives confirmation message, pass
      
      ![Review System Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/Create-review.png)

   2. If not, fail

   3. If review with banned content is submitted:
      1. System displays appropriate warning message, pass
      2. Review is not published, pass
      
      ![My Reviews Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/edit-profile-my-reviews.png)

   4. If not, fail

   | FR ID | Criteria Description | Result | Notes |
   |-------|---------------------|--------|-------|
   | FR-3.1 | System provides review creation page | □ Pass □ Fail | |
   | FR-3.2 | System allows rating and text input | □ Pass □ Fail | |
   | FR-3.4 | System applies content filter to review | □ Pass □ Fail | |
   | FR-3.6 | System confirms successful review creation | □ Pass □ Fail | |

### Content Filtering - FR-5.2

1. [Setup](#setup)
   1. Navigate to the CineScope website
   2. Log in with administrator credentials
   3. Access the content management section
   4. Go to "Banned Words" management page

2. [Testing](#testing)

   | Step | Action | Expected Result |
   |------|--------|----------------|
   | 1 | Add a new banned word | Interface accepts new word input |
   | 2 | Set severity level | Severity options selectable |
   | 3 | Save changes | Word added to banned list |
   | 4 | Test word in review creation | Review with banned word flagged |

3. [Pass/Fail](#passfail)
   1. If content filtering works correctly:
      1. New banned word is added to database, pass
      2. Review containing banned word is flagged, pass
      3. Appropriate warning message is displayed to user, pass
      
      

   2. If not, fail

   | FR ID | Criteria Description | Result | Notes |
   |-------|---------------------|--------|-------|
   | FR-5.1 | System maintains banned word list | □ Pass □ Fail | |
   | FR-5.2 | System checks text against banned list | □ Pass □ Fail | |
   | FR-5.3 | System flags reviews with inappropriate content | □ Pass □ Fail | |

### User Profile Management - FR-4.7

1. [Setup](#setup)
   1. Navigate to the CineScope website
   2. Log in with valid credentials
   3. Click on user profile icon
   4. Select "My Profile" option

2. [Testing](#testing)

   | Step | Action | Expected Result |
   |------|--------|----------------|
   | 1 | Click "Edit Profile" button | Edit form displayed with current values |
   | 2 | Update username field | Field accepts new input |
   | 3 | Click "Save Changes" button | Changes processed and saved |
   | 4 | Log out and attempt login with new credentials | Authentication with new credentials |

3. [Pass/Fail](#passfail)
   1. If profile update is successful:
      1. System displays confirmation message, pass
      2. Updated information is visible in profile, pass
      3. User can log in with new credentials, pass
      
      ![Edit Profile Wireframe](https://github.com/omniV1/CineScope/blob/main/Documents/Images/edit-profile-my-reviews.png)

   2. If not, fail

   | FR ID | Criteria Description | Result | Notes |
   |-------|---------------------|--------|-------|
   | FR-4.7 | System ensures account security | □ Pass □ Fail | |
   | FR-4.1 | System allows login with updated credentials | □ Pass □ Fail | |

## IV. Non-Functional Requirements Testing

The following matrix defines non-functional requirements (NFRs) that should be evaluated during testing:

| NFR ID | Non-Functional Requirement | Measurement Method | Acceptance Criteria | SCRUM ID |
|--------|----------------------------|-------------------|---------------------|----------|
| NFR-1.2 | The system shall support 100 concurrent users | Load testing | 100 simultaneous logins | SCRUM-21 |
| NFR-1.3 | The system shall be fully responsive on mobile and desktop | Device testing | Correct rendering on all devices | SCRUM-21 |
| NFR-2.1 | The system shall process authentication within 2 seconds | Stopwatch measurement | Complete within 2 seconds | SCRUM-36 |
| NFR-2.3 | The system shall maintain 300ms maximum latency for queries | Performance monitoring | <300ms query response | SCRUM-36 |
| NFR-5.2 | System shall maintain 99.9% authentication accuracy | Error rate tracking | <0.1% authentication errors | SCRUM-41 |

## V. Test Execution Sign-off

| Role | Name | Signature | Date |
|------|------|----------|------|
| Test Executor | | | |
| Test Reviewer | | | |
| QA Manager | | | |
