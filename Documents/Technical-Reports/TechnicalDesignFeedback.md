# CineScope Technical Design Document Issue Tracker

## AUTHORS

| Name | Role | Department |
|------|------|------------|
| Carter Wright | Scrum Master | Development |
| Rian Smart | Product Owner | Management |
| Owen Lindsey | Developer | Development |
| Andrew Mack | Developer | Development |

Date: 3/23/2025

CST-326 


## Architecture Issues 

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| ARCH-001 |  System Architecture            |     Design flaw in diagram figure 1.       |   Figure 1 created by Owen Lindsey dsiplays a deployment platform however this app has not deployed.                |     Andrew - fix this by removing the deployment square from the diagram or moving it to out of scope for sprint 2 as it is not required for sprint completion.            |
| ARCH-002 |    System Architecture              |    Documentation of N-layer requires update.         |   Our N-layer description created by Andrew Mack lacks a Server layer for our DTOs in our MongoDB.                |  Owen - Add a server layer for DTOs because we are using a mongoDB.               |
| ARCH-003 |   User Interface Design           |   Design flaw in diagram figure 3         |  Figure 3 created by Owen Lindsey landing page needs to display top rated and recently added movies.                       |Andrew -  Movies by category actually filters by search, genre, and rating and the landing page shows the most recently added and top rated movies. Update the site map to reflect this flow.               |
| ARCH-004 |              |            |                   |                |
| ARCH-005 |              |            |                   |                |











## Database Issues 

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| DB-001   | Databases / Users Collection             |  Documentation error         |     The user collection initalized by Owen Lindsey is missing the field ProfilePicture.              |       Andrew - Update the collection to have a string field that holds ProfilePictures.         |
| DB-002   |              |            |                   |                |
| DB-003   |              |            |                   |                |
| DB-004   |              |            |                   |                |
| DB-005   |              |            |                   |                |











## UI/UX Issues

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| UI-001   |              |            |                   |                |
| UI-002   |              |            |                   |                |
| UI-003   |              |            |                   |                |
| UI-004   |              |            |                   |                |
| UI-005   |              |            |                   |                |











## Security Issues

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| SEC-001  |              |            |                   |                |
| SEC-002  |              |            |                   |                |
| SEC-003  |              |            |                   |                |
| SEC-004  |              |            |                   |                |
| SEC-005  |              |            |                   |                |











## Testing Issues - Michael

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| TEST-001 | Testing Strategy |  Incomplete Detail | Performance testing is mentioned, but there is no reference to tools or thresholds for acceptable performance | Michael - Include target performance benchmarks (e.g., response time < 2s for login, support for 100 concurrent users) and tools |
| TEST-002 | Testing Strategy |  Omission | Test data generation strategy is outlined using AutoFixture, but there is no guidance on test data for banned word detection or content filtering edge cases. | Michael - Add a subsection specifying how banned word cases (edge and typical) will be covered during unit and integration testing. |
| TEST-003 | Authentication Flow | Edge Case Testing Missing | The test plan does not address boundary testing for failed login attempts, such as testing exactly 3 failures triggering lockout and timing reset. | Michael - Add test cases to verify lockout after 3 failed logins and lockout duration expiration handling, matching the defined security policy. |
| TEST-004 |              |            |                   |                |
| TEST-005 |              |            |                   |                |











## Documentation Issues - Michael

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| DOC-001  | Review Creation Flow | Missing Reference | The diagram for review creation does not mention how flagged words are reviewed or stored after detection. This leaves ambiguity in how content filtering interacts with storage. | Michael - Add a step in the sequence diagram or annotate the flow to show flaggedWords being passed to the database when content is approved or flagged. |
| DOC-002  | Appendix - Technology Stack | Missing Justification | The tech stack lists CI/CD (GitHub Actions), but the document provides no mention of how CI integrates with the testing or deployment process. | Michael - Add a sentence or diagram explaining CI/CD pipeline stages, especially how automated tests (unit/integration) are triggered via GitHub Actions. |
| DOC-003  |              |            |                   |                |
| DOC-004  |              |            |                   |                |
| DOC-005  |              |            |                   |                |











## Implementation Issues

| Issue ID | Section/Page | Issue Type | Issue Description | Fix Description |
|----------|--------------|------------|-------------------|----------------|
| IMP-001  |              |            |                   |                |
| IMP-002  |              |            |                   |                |
| IMP-003  |              |            |                   |                |
| IMP-004  |              |            |                   |                |
| IMP-005  |              |            |                   |                |
