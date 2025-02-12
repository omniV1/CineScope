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
Our current focus is specifically on documenting the login system and homepage requirements. The site architecture documentation needs significant expansion.

### Current Progress
- We have an initial sitemap available here: [Current Sitemap](https://github.com/omniV1/CineScope/blob/main/Documents/sitemap/readme.md)
- The current sitemap provides a basic structure but requires enhancement
- *While visual improvements would be beneficial, they are not the primary focus at this stage*

### Site Documentation Tools
- [ x ] Establish documentation in Figma (primary tool)
  - *Note: Initial exploration of Figma has shown promise, but additional learning time will be needed*
- [ x ] Develop wireframe documentation standards
- [ x ] Create shared access protocols for team collaboration

## Functional Requirements Documentation
- [X] Transfer all functional requirements into Jira as structured tasks
- [x] Review and refactor existing functional requirements
- [x] Update non-functional requirements as needed
  - *This may require significant restructuring but is manageable within current timeline*

### Documentation Sharing
- [x] Wireframe documentation access will be provided to team
- [x] Team members will have full editing capabilities

## Landing page Wireframe creation
- [] Create wireframe for the public state of the landing page (will need a login section and only allow read only access so no writing reviews). 
- [] Create wireframe for the private member access of the landing page allowing users to view thier profile, views, and create reviews for movies. 
   - In the the landing page we must be able to filter reviews by categories (Recently added, Top Rated, Featured movies)
   - We must also be able to search for movies in a search bar.
- [] Create the movie details page allowing users to select a movie and enter a page that displays the movie, the reviews, highlights the user review if applicable, and allows the Member to create a review.
    - We must be able to create a review when logged in.
    - If we are logged out only allow the user to view the reviews and movie details but NOT create reviews. 


## Documentation Timeline Considerations
1. Functional requirements documentation in Jira takes priority
2. Architecture diagrams will be developed alongside requirements documentation
3. Site documentation will focus exclusively on login and homepage initially
4. Wireframe access and collaboration protocols will be established as requirements are finalized

---
**Remember:** Our goal is to create comprehensive functional requirement documentation. All diagrams and documentation should focus on clearly communicating requirements rather than technical implementation details.
