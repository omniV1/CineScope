# Technical Report: CineScope Agile-Based Process

Owen Lindsey

Grand Canyon University CST-326

March 9, 2025 

Version 1.0 ***DRAFT***
## Project Overview

CineScope is a movie review platform that allows users to browse movies, read and write reviews, and interact with other movie enthusiasts. We built it using C# ASP.NET Core Web App (MVC) with MongoDB as the database system. The platform has a responsive web interface that works on both mobile and desktop devices.

### Quality Assessment

Our CineScope project has several strengths:

1. **N-layer Architecture**: We used a layered approach (presentation, business logic, data access, and database) to keep the code organized and make future updates easier. This is shown in our technical design document with clear diagrams of how these layers work together.

2. **User-Friendly Design**: Our wireframes focus on making the site easy to use with clear navigation. Features like movie filtering, review management, and content moderation help create a good user experience.

3. **Good Documentation**: We created thorough documentation including functional requirements, technical specifications, and test procedures. This helped our team stay on the same page throughout development.

4. **Security Features**: We included important security measures like proper user authentication, content filtering for inappropriate words, and account protection (like locking accounts after failed login attempts).

5. **Testing Plan**: Our test procedures document has step-by-step instructions for checking key features, making sure the application works as expected.

### Client Feedback Analysis

For our academic exercise, we considered what potential client feedback might include for a platform like CineScope. In a real-world scenario, stakeholders might highlight these areas for improvement:

1. **Performance**: Database query optimization and better caching would be important for improving load times under high user traffic.

2. **Mobile Experience**: While our design is responsive, additional refinements to the mobile interface would enhance usability on smaller screens.

3. **Analytics**: A production version would benefit from features that track user interaction patterns and review trends.

4. **Social Media Integration**: Expanding functionality to allow sharing content on social platforms would increase visibility.

For future iterations in a real-world context, priorities might include:

1. Implementing comprehensive caching for frequently accessed data
2. Conducting targeted user testing on mobile interfaces
3. Developing an analytics implementation plan
4. Designing social sharing capabilities

## Agile-based Project Management

Our team used Scrum to manage the CineScope project. This Agile approach helped us adjust to changing requirements while keeping our development organized.

### Process Evaluation

#### What Worked Well

1. **Smart Sprint Planning**: For Sprint 1, we picked just three user stories (14 story points total). This kept our workload manageable and helped us make steady progress without getting overwhelmed.

2. **Good Documentation**: Before writing code, we created detailed documents for Software Requirements, Technical Design, and Test Procedures. This helped everyone understand what we needed to build.

3. **Clear Team Roles**: Each person had a specific job (Scrum Master, Product Owner, Developers) with clear responsibilities, which made our workflow more efficient.

4. **Good Communication**: As our Technical Design Document shows, we used Discord for daily chats, Jira for tracking tasks, and GitHub for managing code. This helped us stay connected and organized.

5. **Planning Tests Early**: We created test procedures before starting development, which helped us catch problems early instead of at the end.

#### What Could Be Better

1. **Breaking Down Tasks**: While we identified user stories, we didn't always break them into small enough tasks. Our Version Description Document shows test tasks were planned but "not yet in Jira," which left gaps in our tracking.

2. **Sprint Planning Data**: Since we're a new team, we didn't have past data to help plan sprints accurately. In future projects, we should track our velocity to make better estimates.

3. **Managing Risks**: We mostly focused on not taking on too much work, but didn't fully address other risks like technical challenges or external dependencies.

4. **Involving Stakeholders**: We could have shown our progress to stakeholders more often during development instead of waiting until the end.

### Recommendations for Future Projects

1. **Better Task Breakdown**: Break user stories into smaller, specific tasks before sprint planning. Each story should have separate tasks for coding, testing, and documentation clearly listed in Jira.

2. **Add Automated Testing**: Set up automated build and test processes to catch problems early. This would work alongside our manual testing to improve code quality.

3. **More Stakeholder Demos**: Schedule regular demos with stakeholders throughout each sprint to get feedback earlier and make sure we're building what they want.

4. **Hold Team Retrospectives**: After each sprint, have a meeting to discuss what went well and what didn't, and document specific improvements for the next sprint.

5. **Better Risk Management**: Consider more types of risks beyond just workload, including technical problems, dependencies on other systems, and stakeholder-related issues. As Poppendieck and Poppendieck (2003) observe, "The primary benefit of iterative development is risk mitigation, and the primary risk in software development is building the wrong thing."

### Theoretical Stakeholder Analysis

In a real-world implementation of the CineScope project, we would need to consider several types of stakeholders with different priorities:

1. **End Users** (High Priority): Movie enthusiasts who would use the platform would be the most important stakeholders. Their needs for intuitive navigation, reliable reviews, and responsive design would directly impact adoption and retention.

2. **Development Team** (High Priority): Any team building this type of platform needs clear requirements, technical freedom, and a manageable workload to deliver quality features on schedule.

3. **Product Owner** (High Priority): In our academic exercise, Rian served as Product Owner, but in a real implementation, this role would represent business interests and prioritize features based on value delivery.

4. **Technical Support** (Medium Priority): Staff who would maintain the system would have requirements regarding architecture, security, and maintenance procedures.

5. **External Services** (Low Priority): Any third-party services or APIs would have requirements that might influence technical decisions, though with less immediate impact.

This hypothetical ranking follows the Agile principle that working software delivering value to users is the primary measure of progress. By prioritizing users, developers, and the Product Owner, an organization would focus on building features that users want through an effective team guided by clear business objectives.

Different stakeholders would have varying levels of influence throughout the project lifecycle. In planning phases, the Product Owner would guide feature prioritization. During development, the team's technical knowledge would shape implementation approaches. After launch, user feedback would become increasingly important for guiding improvements.

Understanding these shifting influences would help teams engage appropriately with different stakeholders throughout a project. As Highsmith (2009) notes, "Agility is the ability to both create and respond to change in order to profit in a turbulent business environment."

## Team Collaboration Experience

Our team worked collaboratively throughout the project, with each person contributing their specific skills. As part of this collaborative effort, I helped create documentation templates and quick reference guides that we all used to maintain consistency in our deliverables.

Our communication was primarily through Discord, where team members typically responded within 24 hours. Everyone tracked their assigned tasks in Jira and updated the team on progress during our regular check-ins. When needed, team members assisted each other with GitHub operations and technical questions.

The team showed great dedication, with everyone remaining available and responsive even during challenging personal circumstances. We maintained a supportive learning environment where team members could develop new skills like Figma design and markdown documentation without judgment. This collaborative approach helped us overcome technical challenges while keeping the project on track.

## References

Highsmith, J. (2009). *Agile Project Management: Creating Innovative Products*. Addison-Wesley Professional.

Poppendieck, M., & Poppendieck, T. (2003). *Lean Software Development: An Agile Toolkit*. Addison-Wesley Professional.

---

*Note: This is a draft of the technical report. The final version will include peer feedback and additional industry sources.*