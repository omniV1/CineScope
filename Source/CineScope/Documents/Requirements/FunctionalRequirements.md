# CineScope Functional Requirements

## UC-1: Feature Movies on Landing Page

### Purpose
To provide users with an organized and accessible movie browsing experience on the landing page.

### Requirements

| FR ID | Description |
|-------|-------------|
| FR-1.1 | The System shall display movies on the Landing page |
| FR-1.2 | The System shall display section for Featured Movies |
| FR-1.3 | The System shall display section for Recently Viewed Movies |
| FR-1.4 | The System shall display section for Top-Rated Movies |
| FR-1.5 | The System shall display section for Rom-Com Movies |
| FR-1.6 | The System shall display section for Thriller/Horror Movies |
| FR-1.7 | The System shall display section for Action Movies |
| FR-1.8 | The System shall display section for Sci-Fi Movies |
| FR-1.9 | The System shall display section for Recently Added Movies |
| FR-1.10 | The System shall redirect the user to a movie's page if clicked |
| FR-1.11 | The System shall display an error message and log the user out if an error occurs on the landing page |

## UC-2: Filtering Reviews

### Purpose
To enable users to effectively sort and filter movie reviews.

### Requirements

| FR ID | Description |
|-------|-------------|
| FR-2.1 | The system shall provide a "Filters" button on the movie reviews page |
| FR-2.2 | The system shall allow users to select a filter category (Rating, Date posted, etc) |
| FR-2.3 | The system shall update the displayed reviews based on the selected filter |
| FR-2.4 | The system shall provide a "Sort" button on the movie reviews page |
| FR-2.5 | The system shall allow users to select a sorting option (A-Z, Low-High) |
| FR-2.6 | The system shall update the displayed reviews based on the selected sorting order |
| FR-2.7 | The system shall display a message if no reviews match the selected filter or sorting option |
| FR-2.8 | The system shall display an error message if an error occurs while filtering or sorting reviews |

## UC-3: CRUD Operations

### Purpose
To manage the creation, reading, updating, and deletion of movie reviews.

### Requirements

| FR ID | Description |
|-------|-------------|
| FR-3.1 | The system shall provide a "Create Review" page for users to enter movie reviews |
| FR-3.2 | The system shall allow users to input a movie title, rating, and review text |
| FR-3.3 | The system shall validate the review input for required fields |
| FR-3.4 | The system shall apply a content filter to the review text before submission |
| FR-3.5 | The system shall save the review to the database if it passes the content filter |
| FR-3.6 | The system shall display a confirmation message upon successful review creation |
| FR-3.7 | The system shall display an error message if the review fails the content filter |
| FR-3.8 | The system shall provide a "Reviews" page for users to view reviews from other users |
| FR-3.9 | The system shall retrieve and display a list of reviews that passed the content filter |
| FR-3.10 | The system shall allow users to filter reviews by rating or date |
| FR-3.11 | The system shall display a message if no reviews are available |
| FR-3.12 | The system shall provide a "My Reviews" page for users to view their existing reviews |
| FR-3.13 | The system shall allow users to select a review to update |
| FR-3.14 | The system shall allow users to modify the rating and/or review text |
| FR-3.15 | The system shall apply a content filter to the updated review text before submission |
| FR-3.16 | The system shall update the review in the database if it passes the content filter |
| FR-3.17 | The system shall display a confirmation message upon successful review update |
| FR-3.18 | The system shall display an error message if the updated review fails the content filter |
| FR-3.19 | The system shall allow users to select a review to delete from the "My Reviews" page |
| FR-3.20 | The system shall prompt users for confirmation before deleting a review |
| FR-3.21 | The system shall remove the review from the database upon confirmation |
| FR-3.22 | The system shall display a confirmation message upon successful review deletion |
| FR-3.23 | The system shall display an error message if there is a system error during review deletion |

## UC-4: Login/Logout Operations

### Purpose
To manage user authentication and session handling.

### Requirements

| FR ID | Description |
|-------|-------------|
| FR-4.1 | The system shall allow users to login with their unique credentials and validate user credentials against the database |
| FR-4.2 | The system shall lock the account for a specified duration (ex: 15 minutes) after three failed login attempts and display a message informing the user |
| FR-4.3 | The system shall allow users to reset their password and regain access to their account by sending a recovery email to their registered email address |
| FR-4.4 | The system shall allow users to log out of their account and return to the main menu |
| FR-4.5 | The system shall handle errors such as username or password not found, account locked or disabled, database connection errors, and network or server errors |
| FR-4.6 | The system shall validate user credentials against the database to ensure the user's account exists and the credentials are correct |
| FR-4.7 | The system shall ensure the user's account is secure by preventing multiple login attempts and locking the account temporarily after three failed attempts |

## UC-5: Content Filter

### Purpose
To maintain appropriate content standards within the review system.

### Requirements

| FR ID | Description |
|-------|-------------|
| FR-5.1 | The system shall maintain a list of banned words and phrases for the content filter |
| FR-5.2 | The system shall check review text against the banned list during submission and updates |
| FR-5.3 | The system shall flag reviews containing inappropriate content and provide feedback to the user |
| FR-5.4 | The system shall allow users to request and view the list of banned words |