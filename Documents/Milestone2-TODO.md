# CineScope Project Documentation Requirements

## Architecture Documentation Requirements
We need to create detailed architecture documentation that will serve as the foundation for our functional requirements. This involves creating two distinct diagrams:

### Technical Architecture Diagram
- [X] Create a comprehensive diagram of our technology stack that includes:
  - C# Blazor web application architecture and components
  - Tailwind CSS implementation and styling structure
  - MongoDB database architecture and relationships
  - Technology logos for visual clarity
  - Connection flows between different technology components

*Note: This diagram should be visually appealing while maintaining technical accuracy. The inclusion of official logos and proper color schemes will help with presentation clarity.*

### Team Communication Architecture
- [x] Document our communication infrastructure including:
  - Discord channel structure and communication protocols
  - Confluence documentation organization and standards
  - Jira workflow and project management methodology
  - Integration points between communication tools
  - Team roles and access level documentation

## Site Architecture Documentation
Our current focus is specifically on documenting the login system, homepage requirements, and movie browsing functionality. The site architecture documentation needs significant expansion.

### Current Progress
- We have an initial sitemap available here: [Current Sitemap](https://github.com/omniV1/CineScope/blob/main/Documents/sitemap/readme.md)
- The current sitemap provides a basic structure but requires enhancement
- *While visual improvements would be beneficial, they are not the primary focus at this stage*

### Site Documentation Tools
- [x] Establish documentation in Figma (primary tool)
  - *Note: Initial exploration of Figma has shown promise, but additional learning time will be needed*
- [x] Develop wireframe documentation standards
- [x] Create shared access protocols for team collaboration

## Functional Requirements Documentation
- [X] Transfer all functional requirements into Jira as structured tasks
- [x] Review and refactor existing functional requirements
- [x] Update non-functional requirements as needed
  - *This may require significant restructuring but is manageable within current timeline*

### Documentation Sharing
- [x] Wireframe documentation access will be provided to team
- [x] Team members will have full editing capabilities

## Movie Browsing and Details Implementation
### Landing Page Requirements
**Andrew will accomplish this task**
- [ ] Create unified landing page wireframe with authentication states:
  - Common Features:
    - Movie grid/list view with basic information
    - Filtering system for movies:
      - Recently added
      - Top rated
      - Featured movies
      - Genre categories
    - Search functionality with search bar
    - Movie thumbnails, titles, and basic information
  - Non-authenticated State:
    - Login/Register prompt in header
    - Read-only access to movie listings
  - Authenticated State:
    - Profile section in header
    - Quick access to user's review history
    - Link to detailed profile page showing user's activity and reviews

### Movie Details Page Requirements
- [ ] Create wireframe for movie details page with two states:
  - Public (Non-logged in) View:
    - Display comprehensive movie information (release date, description, genre)
    - Show average user score
    - Display existing user reviews in read-only mode
    - Clear indication of login requirement for leaving reviews
  - Member (Logged in) View:
    - All public view features
    - Interface for creating new reviews
    - Display user's existing review if applicable
    - Ability to edit/delete user's own review
    - Highlight the logged-in user's review distinctly

## Documentation Timeline Considerations
1. Functional requirements documentation in Jira takes priority
2. Architecture diagrams will be developed alongside requirements documentation
3. Site documentation will focus exclusively on login, homepage, and movie browsing initially
4. Wireframe access and collaboration protocols will be established as requirements are finalized

---
**Remember:** Our goal is to create comprehensive functional requirement documentation. All diagrams and documentation should focus on clearly communicating requirements rather than technical implementation details.
