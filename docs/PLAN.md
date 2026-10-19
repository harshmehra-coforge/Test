# Implementation Plan: Salesforce Opportunity Parallel Approval Workflow

## Stack & Rationale
- **Platform:** Salesforce Lightning/Flow (primary), with RESTful API endpoints for integration (Python/FastAPI for API stubs, Teams/email notification, and audit log patterns)
- **Notification:** MS Teams (channel post) and Email (Salesforce workflow)
- **Audit Logging:** Custom Salesforce objects and API-accessible logs
- **Why:** Native Salesforce for core workflow, Python/FastAPI for extensibility, integration, and reporting APIs

## Module/Service Breakdown
- **api/**: REST endpoints for triggering approvals, status checks, submitting decisions, listing approvals, and notifications
- **models/**: Data models for Opportunity, ApprovalHistory, NotificationLog
- **service/**: Business logic for approval routing, parallel execution, SLA/escalation, and aggregation of comments
- **repository/**: Data access layer for Salesforce and custom objects
- **integrations/**: Teams and Email notification senders, fallback logic
- **config/**: Environment and integration settings
- **tests/**: Unit and integration test skeletons
- **scripts/**: Migration and setup scripts

## Folder Structure

```
.
├── src/
│   ├── api/
│   │   ├── routes/
│   │   │   ├── approvals.py
│   │   │   └── notifications.py
│   │   └── main.py
│   ├── models/
│   │   ├── opportunity.py
│   │   ├── approval_history.py
│   │   └── notification_log.py
│   ├── service/
│   │   ├── approval_service.py
│   │   ├── notification_service.py
│   │   └── sla_service.py
│   ├── repository/
│   │   ├── salesforce_repo.py
│   │   └── approval_repo.py
│   ├── integrations/
│   │   ├── teams.py
│   │   └── email.py
│   ├── config/
│   │   └── settings.py
│   └── scripts/
│       └── migrate.py
├── tests/
│   ├── api/
│   │   ├── test_approvals.py
│   │   └── test_notifications.py
│   ├── service/
│   │   ├── test_approval_service.py
│   │   ├── test_notification_service.py
│   │   └── test_sla_service.py
│   ├── repository/
│   │   ├── test_salesforce_repo.py
│   │   └── test_approval_repo.py
│   └── conftest.py
├── requirements.txt
├── Dockerfile
├── .env.example
└── README.md
```

## Build Order / Milestones
1. Scaffold models and config
2. Implement repository layer (Salesforce and custom objects)
3. Build service layer (approval routing, notification, SLA)
4. Expose API routes
5. Integrate Teams/email notification logic
6. Add test skeletons
7. Dockerize and provide run instructions

## Key Risks / Open Questions
- Salesforce API access and permissions for custom objects
- Teams connector reliability and fallback logic
- Handling of resubmissions and audit log retention
- SLA escalation routing (manager/delegate mapping)

## Mermaid Component Diagram

![Diagram](https://svgshare.com/i/13Yj.svg)

---

This plan is grounded in the TSD and meeting notes, ensuring parallel approval, robust notification, and auditability. The folder structure and milestones enable rapid, testable implementation.