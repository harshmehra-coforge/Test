# Technical Specification Document (TSD)

## Title
Rejection Handling & Resubmission – Feature 5.4 (KAN-497)

## Purpose
Enable end-to-end rejection feedback and resubmission workflows for user-submitted stories/opportunities in accord with governance and compliance, supporting audit traceability, user improvement, and transparent error handling.

## Scope
- Combined feedback delivery on rejection (UI + backend)
- Resubmission workflow and logic (UI + backend)
- Notification system integration (Email, Slack)
- Full approval chain restart upon resubmission
- Multiple rejection/resubmission cycles
- Audit traceability for compliance

## References
- [BRD.md]
- [USER_STORIES.md]
- [JIRA_FETCH_CONTEXT.md]
- [Meeting_Note_01_Discount_Depth 2.md]
- Jira: KAN-497

## High-Level Architecture Overview

The system applies a microservices and event-driven model (Azure-native stack):
- UI microservice for user-facing feedback and resubmission
- Feedback aggregation service for comment collation
- Workflow Engine for managing approval and rejection flow logic
- Notification Service (Email, Slack)
- Audit Logging Service (centralized, queryable)
- Azure Cosmos DB for storing workflow state, feedback, and audit logs
- Integration with existing databases and workflow engines as required

Approval chains always restart from the first tier after rejection, and each action is logged. The architecture supports concurrent, independently audited resubmission cycles without risk of process circumvention.

---

## 1. Functional Requirements

### 1.1 Rejection Feedback & Resubmission
- Upon any rejection (Deal Desk, VP Sales), feedback from all reviewers is aggregated and delivered to the submitter via UI and notifications.
- Submitter can edit and resubmit; entire approval chain restarts from Deal Desk tier.
- The system must support multiple rejection/resubmission cycles for the same record, with each cycle fully audited.

### 1.2 Notification Workflow
- Notify users of rejection feedback and resubmission via Email and Slack.
- Notify reviewers (including escalation per SLA); finance receives courtesy notification upon VP Sales engagement.
- Notification delivery must be audited, retried on failure, and have SLA-bound escalation (e.g., 24h for Deal Desk, 48h for VP Sales with auto-escalation to EA).

### 1.3 UI Feedback Presentation
- Feedback is displayed in the UI immediately after rejection.
- UI supports: feedback detail, source (review tier), edit/resubmit controls, and error messaging.

### 1.4 Error & Boundary Handling
- Any submission/resubmission error results in an immediate notification to the user.
- Workflow errors (backend logic, notification delivery failure, data issues) are to be logged, traced, and must not compromise audit traceability.

### 1.5 Audit Logging
- Every submission, rejection, feedback delivery, resubmission, and notification is centrally logged for compliance.

---

## 2. System Architecture & Component Design

### 2.1 Microservices Overview
- **UI Service (React/Angular)**: Presents feedback and resubmission; interacts asynchronously with backend.
- **Feedback Service**: Aggregates rejection comments; exposes REST API for feedback retrieval (FastAPI, OpenAPI 3.0+).
- **Workflow Engine (Azure Logic Apps/Functions)**: Orchestrates approval/rejection logic, triggers notifications, restarts chains on resubmission.
- **Notification Service**: Sends notifications (Email via Azure Communication Services, Slack integration); handles retries, escalation, and audit logging.
- **Audit Log Service**: Persists all auditable events in Cosmos DB, enforcing immutable event sourcing.
- **Data Store (Azure Cosmos DB)**: Stores workflow instances, feedback records, and audit logs. High-cardinality partition keys: tenantId + opportunityId.

### 2.2 Event Flow (Summary)
- Story submitted > Approval Chain triggered (Deal Desk → VP Sales)
- On rejection at any tier: comments aggregated > Feedback delivered > UI updated > User can resubmit
- Resubmission triggers full chain restart
- Each cycle/events recorded in Audit Log

### 2.3 Component Interactions
- All inter-service communication is over secure, authenticated APIs (Azure Managed Identities)
- Async callbacks/events published to Azure Event Hubs/Service Bus for notification delivery and audit pipelines

---

## 3. Data Models & Storage

### 3.1 Core Entities
- **Opportunity/Story Document**: Unique identifier, current status, submission history, last reviewer tier, resubmission count, references to feedback records
- **Feedback Record**: Opportunity/story id, reviewers/comments, timestamp, delivery status, correlation id
- **Audit Log Entry**: Entity type, action, actor, timestamp, event detail (immutable, GRC-compliant)
- **Notification Event**: Event id, recipient(s), channel, delivery status, escalation status, timestamp

### 3.2 Cosmos DB Design
- Partition key: tenantId|opportunityId (guarantees high cardinality, avoids hot partitions)
- Feedback embedded in story only when <2MB; else referenced by id (for audit scalability)
- Index on (status, lastReviewer, resubmissionCount) for reporting and compliance queries
- All approval/rejection cycles versioned via history log for traceability

### 3.3 Audit & Change Feed
- Every document update triggers Cosmos DB Change Feed
- Change events streamed to Audit Log Service via Event Hubs
- Immutable retention (7+ years, configurable)

---

## 4. Interfaces & Integrations

### 4.1 API Endpoints (Sample)
- `POST /stories/submit` – Submit new story/opportunity for approval
- `POST /stories/{id}/resubmit` – Resubmit after rejection
- `GET /stories/{id}/feedback` – Retrieve feedback for specific submission
- `POST /notifications/send` – Trigger notification event
- All endpoints require JWT authentication and Azure Managed Identity validation

### 4.2 Notification Channels
- **Azure Communication Services**: Email notifications (with delivery status callback)
- **Slack (Official App Integration)**: Resubmission/rejection notifications via Slack channels; auto-escalation handled via Slack workflow

### 4.3 Approval Engine Integration
- Workflow engine triggers feedback aggregation on rejection, notification service for alerts, and audit logging for all state changes
- SLA monitoring for escalation (timer-based events)

### 4.4 UI Integration
- UI displays feedback, enables resubmission, and shows accurate status for each workflow instance

---

## 5. Non-Functional Requirements (NFRs)

- **Auditability**: 100% of submission, feedback, resubmission, and notification events are logged. Log queries for compliance available within 2s (p95).
- **Performance**: Feedback/resubmission UI actions and notification delivery must complete in <3000ms p95 for 99% of users within 150k MAU scale.
- **Availability**: 99.9% uptime SLA for core approval and resubmission workflows.
- **Reliability**: At least 3x automatic retry for notification/message delivery. No event may be lost without an explicit error surface to the user/compliance.
- **Security**: All APIs protected with RBAC, Azure AD, and Managed Identities. Data-at-rest encrypted with Azure Key Vault. Audit logs are immutable.
- **Capacity**: Cosmos DB provisioned for 5k RU/s baseline, auto-scale to 20k RU/s on demand (supporting up to 10k concurrent active users).
- **Cost**: Monthly Cosmos DB cost not to exceed $2k at max scale as provisioned. Alerted if >70% of RU limit is hit for more than 1 hour.
- **Compliance**: 7-year audit log retention, GDPR/SOX-aligned.

---

## 6. Key Design Decisions & Rationale

- **Full Approval Chain Restart**: Required to prevent audit gaming—partial restarts would compromise governance (see Meeting Note 01_Discount_Depth 2.md).
- **Event-Driven Microservices**: Promotes decoupled, scalable handling of UI, feedback, notifications, and logging (Azure Event Hubs + Functions).
- **Immutable Audit Logging**: Centralized, always-on audit trace of all state changes/events is essential for compliance (Cosmos DB + Change Feed).
- **Feedback Aggregation**: Feedback for each cycle is collated before presentation, ensuring the submitter understands all required changes at once (prevents piecemeal corrections/loops).

## 7. Alternatives & Trade-offs

- **Monolithic Implementation**: Simpler initial delivery but limits scale, testability, and audit/compliance (not chosen).
- **No Full Chain Restart**: Lower user friction but enables process circumvention; fails audit requirements (rejected).
- **Audit in SQL/Blob Only**: Higher performance for OLAP but not as flexible for event source compliance; Cosmos DB chosen for event streaming and flexible indexing.

## 8. Security & Compliance

- All endpoints/application components require Azure Managed Identity or AD role-based access.
- All data is encrypted using Azure Key Vault keys.
- Data access is logged for every read/write on sensitive objects (stories, feedback, audit log).
- Audit log and feedback events preserved for 7 years, immutable, with regular compliance review.

## 9. Observability & Monitoring

- Centralized logging via Audit Log Service (Cosmos DB Change Feed, Event Hubs forwarding to Azure Monitor).
- Alerting for:
  - Notification delivery failures
  - SLA breach on approvals (Deal Desk >24h, VP Sales >48h, auto-escalation)
  - Audit gap or data integrity issue

- Exposed Prometheus/OpenTelemetry endpoints for metrics scraping (latency, throughput, error count).

---

## 10. Migration/Deployment & CI/CD

- Infrastructure as Code: Terraform for all resource provisioning (Cosmos DB, Functions, Event Hubs, Communication Services, Key Vault, RBAC policies)
- CI/CD: GitHub Actions auto-build, test, and deploy all workflow logic. Canary environments for approval and rollback. Manual approval required for production promotion.
- Database migration scripts versioned in repository; schema validated on CI build.
- Managed identities assigned strictly via Terraform RBAC definitions.

## 11. Appendix: Sample Event Flows

### Submission/Resubmission Cycle
1. User submits story/opportunity via UI
2. Workflow Engine triggers: Deal Desk review
3. If rejected: Feedback is aggregated, user is notified, can resubmit
4. On resubmission: Approval chain restarts (Deal Desk > VP Sales if required)
5. Each rejection/resubmission event logged
6. Multiple cycles supported, all events auditable

### Notification SLA Escalation
- Deal Desk (24h), VP Sales (48h) SLAs monitored; missed SLAs auto-escalate notifications and log breach event
- Finance receives informational notification for VP Sales-level events

---
