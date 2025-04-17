
## Slide 1: Business Need

**CineScope: A Modern Movie Review Platform**

**Business Problem:**
- Movie enthusiasts lack a dedicated platform to discover, rate, and review films
- Existing platforms often mix content types or have overwhelming interfaces
- Users need a streamlined experience for sharing opinions and finding new films

**Solution:**
- CineScope delivers a focused movie review platform with intuitive navigation
- Clean interface shows featured films, top-rated content, and user reviews
- Content moderation ensures quality discussions

**Target Users:**
- Movie enthusiasts seeking to track their viewing history
- Film critics wanting to share detailed reviews
- Casual viewers looking for quality recommendations

*[Include screenshot of the landing page showing featured movies interface]*

## Slide 2: User Stories & System Architecture

**Key User Stories:**
- "As a user, I want to see featured movies on the landing page so I can discover new films" (SCRUM-19, 6 points)
- "As a user, I want to login and logout to manage my unique profile" (SCRUM-27, 4 points)
- "As a user, I want to manage movie reviews to share my opinions" (SCRUM-34, 4 points)
- "As a user, I want to filter reviews by rating or date" (SCRUM-45, 7 points)

**System Architecture:**
- N-Layer Architecture with clear separation of concerns:
  - **Presentation Layer:** Blazor WebAssembly components
  - **Business Logic Layer:** Services implementing core functionality
  - **Data Access Layer:** MongoDB repositories
  - **Database Layer:** MongoDB Atlas cloud database

*[Include N-layer architecture diagram from Documents/Images/Updated_N-Layer.png]*

## Slide 3: Functional & Non-Functional Requirements

**Functional Requirements:**
- **Landing Page (FR-1.1 - FR-1.11):**
  - Display featured, recently viewed, top-rated movies
  - Navigate to movie details when clicked
- **Authentication (FR-4.1 - FR-4.7):**
  - Login with credentials, password reset, account security
  - Lock account after three failed attempts
- **Review System (FR-3.1 - FR-3.23):**
  - Create, read, update, delete reviews
  - Input validation and content filtering
- **Content Filtering (FR-5.1 - FR-5.4):**
  - Maintain and check against banned word list
  - Flag inappropriate content

**Non-Functional Requirements:**
- Loading time < 3 seconds (NFR-1.1)
- Support 100 concurrent users (NFR-1.2)
- Fully responsive design (NFR-1.3)
- Process authentication in < 2 seconds (NFR-2.1)

## Slide 4: Technical Requirements

**Conceptual Design:**
- Component-based architecture with modular UI elements
- JWT-based authentication for stateless security
- Content filtering system with severity levels
- Client-side caching for improved performance

**Logical Design:**
- MongoDB Schema:
  - Users: credentials, roles, preferences
  - Movies: details, genres, ratings
  - Reviews: text, ratings, flags
  - BannedWords: term, severity, category
- Repository pattern for data access
- Service layer implementing business logic

**Physical Design:**
- Deployment to Azure App Service
- MongoDB Atlas for database hosting
- GitHub Actions CI/CD pipeline
- Azure CDN for static content delivery

*[Include MongoDB schema diagram and deployment architecture diagram]*

## Slide 5: Testing

**Testing Methodology:**
- Unit testing of individual components
- Integration testing of component interactions
- UI testing of critical user journeys
- Performance testing under load conditions

**Acceptance Test Procedures:**
1. **User Authentication (FR-4.1)**
   - Test valid credentials, invalid attempts, account lockout
   - Pass: User authenticated and redirected to home page
   - Fail: Authentication fails or security measures not triggered

2. **Movie Browsing (FR-2.1)**
   - Test category selection, filtering, sorting
   - Pass: Movies filtered correctly by selected criteria
   - Fail: Incorrect filtering or missing results

3. **Review Creation (FR-3.1)**
   - Test rating selection, text input, submission
   - Pass: Review saved and displayed on movie page
   - Fail: Review not saved or displayed incorrectly

*[Include test coverage metrics chart]*

## Slide 6: Quality Assessment

**Quality Metrics Analysis:**

1. **Sprint Burndown Performance:**
   - Tracking close to ideal burndown line
   - Faster progress mid-sprint (March 5-11)
   - All planned work completed by sprint end

2. **User Satisfaction Ratings:**
   - Authentication: 8.5/10 (highest rated)
   - UI Components: 7.0/10 (opportunity for improvement)
   - Core Features: 7.6/10
   - Overall: 7.5/10

3. **Velocity by Feature Area:**
   - Core Features: Peak in Week 2 (7 story points)
   - UI Components: Strong in Week 1, decreased after
   - Backend Services: Improved over time, peak in Week 3

4. **Task Complexity vs. Completion Time:**
   - Most tasks accurately estimated
   - Some 3-point stories took longer than expected
   - Authentication features required most development time (5.5 days)

*[Include sprint burndown chart and velocity chart]*

## Slide 7: Demo

**Live Application Demonstration:**

1. **User Authentication:**
   - Registration process
   - Login functionality
   - Account security features

2. **Movie Browsing:**
   - Featured movies section
   - Category-based filtering
   - Search functionality

3. **Review System:**
   - Creating a new review
   - Content filtering in action
   - Editing and managing reviews

4. **Admin Functionality:**
   - Content moderation
   - User management
   - Banned word configuration

*[Include QR code to live demo or prepare local instance]*

**Future Roadmap:**
- Enhanced recommendation system
- Social features for sharing
- Mobile application development
