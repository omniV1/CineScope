# CineScope Non-Functional Requirements

## UC-1: Feature Movies on Landing Page

### Purpose
To ensure the landing page meets performance, scalability, and compatibility requirements.

### Requirements

| NFR ID | Description |
|--------|-------------|
| NFR-1.1 | The system shall load the landing page within 3 seconds under normal network conditions |
| NFR-1.2 | The system shall support at least 100 concurrent users browsing the landing page without degradation in performance |
| NFR-1.3 | The system shall be fully responsive, supporting both mobile and desktop views seamlessly |
| NFR-1.4 | The system shall use modular design principles, allowing easy updates to specific sections without impacting the rest of the page |
| NFR-1.5 | The system shall invalidate user sessions securely when an error occurs and log the user out |
| NFR-1.6 | The system shall support the latest two versions of all major browsers |

## UC-2: Filtering Reviews

### Purpose
To maintain efficient and reliable review filtering operations.

### Requirements

| NFR ID | Description |
|--------|-------------|
| NFR-2.1 | The system shall update the filtered or sorted reviews within 2 seconds of user input |
| NFR-2.2 | The system shall support at least 100 concurrent users applying filters and sorting reviews |
| NFR-2.3 | The system shall perform review filtering and sorting operations with a maximum latency of 300ms for queries returning up to 1000 reviews |
| NFR-2.4 | The system shall allow users to apply filters and sorting without exceeding a rate limit of 100 requests per user per hour |
| NFR-2.5 | The system shall maintain an audit log of all filtering and sorting actions, retained for 90 days |

## UC-3: CRUD Operations

### Purpose
To ensure reliable and efficient review management operations.

### Requirements

| NFR ID | Description |
|--------|-------------|
| NFR-3.1 | The system shall process all review submissions within 2 seconds of user input |
| NFR-3.2 | The system shall support at least 100 concurrent users submitting reviews |
| NFR-3.3 | The database shall perform read operations with a maximum latency of 300ms for queries returning up to 1000 reviews |
| NFR-3.4 | The system shall implement rate limiting of 100 review submissions per user per hour |
| NFR-3.5 | The system shall maintain an audit log of all review modifications for 90 days |
| NFR-3.6 | The system shall support a minimum of 10,000 review submissions per hour |

## UC-4: Login/Logout Operations

### Purpose
To provide secure and responsive authentication services.

### Requirements

| NFR ID | Description |
|--------|-------------|
| NFR-4.1 | The system shall respond to login requests within 2 seconds on average |
| NFR-4.2 | The system shall respond to logout requests within 1 second on average |
| NFR-4.3 | The system shall ensure that all login requests are encrypted using HTTPS |
| NFR-4.4 | The system shall store user credentials securely using hashed passwords |
| NFR-4.5 | The system shall provide clear and concise error messages for invalid login attempts |
| NFR-4.6 | The system shall display a visual indicator to indicate that the user is logged in or logged out |
| NFR-4.7 | The system shall allow users to remember their login credentials for convenience |

## UC-5: Content Filter

### Purpose
To maintain effective and efficient content moderation.

### Requirements

| NFR ID | Description |
|--------|-------------|
| NFR-5.1 | The content filter shall process review text with a maximum latency of 200ms for reviews up to 5000 characters |
| NFR-5.2 | The system shall maintain a content filter accuracy rate of 99.9% for detecting inappropriate content |
| NFR-5.3 | The banned word list shall be cached in memory with updates propagating within 30 seconds |
| NFR-5.4 | The system shall provide detailed error logs for content filter rejections with 100ms max latency |
| NFR-5.5 | The system shall maintain a backup of the banned word list with 5-second failover |