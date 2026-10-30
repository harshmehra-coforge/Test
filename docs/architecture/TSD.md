# Technical Specification Document
## Large Deal Approval Enhancement — Parallel Finance and Deal Desk Approval
**Date:** 2026-10-30
**Version:** 1.0
**Status:** Draft

## 1. Executive Summary

### 1.1 Purpose
This design introduces a parallel, multi-tier approval workflow for high-value Salesforce Opportunities, ensuring robust Finance oversight for deals above $500K Total Contract Value (TCV). It addresses prior audit gaps and supports efficient approvals for sales operations, eliminating risk of unreviewed non-standard revenue recognition terms.

### 1.2 Scope
**In Scope:**
- Salesforce Opportunity approval flow with parallel Deal Desk and Finance tracks
- Finance track with Director ($500K–$2M) and CFO (> $2M) sub-tiers
- TCV and discount-based approval triggers
- MS Teams channel integration for notification
- Finance and Sales dashboards for approval reporting

**Out of Scope:**
- Renewal auto-approval logic (to be proposed separately)
- External ERP integration
- Payment reconciliation or invoicing automation

### 1.3 Key Decisions
1. **Parallel Approval Tracks:** Finance and Deal Desk approvals run concurrently, reducing cycle time by up to 48 hours.
2. **TCV Threshold ($500K):** Chosen based on volume and audit trade-offs; sub-tier split between Director and CFO for larger deals.
3. **Event-driven Integration:** Salesforce flows/events trigger notifications to MS Teams and dashboards, enabling real-time Finance visibility.

## 2. System Architecture
### 2.1 High-Level Architecture

**Approval triggers:**
- Deal Desk approval: Discount > 10% on Opportunity
- Finance approval: TCV > $500K; sub-tier routing by TCV band

**Notification & Reporting:**
- Email notifications to approvers
- Channel post (MS Teams) for Finance team awareness
- Salesforce dashboards for approval status, SLA, and rejection reasons

**Component Breakdown:**
- Presentation: Salesforce UI (Opportunities, Approval History, Dashboards)
- Application: Approval Process, Flow Builder, Workflow automation
- Integration: MS Teams HTTP callout (webhook), Email, Custom dashboard

*(Architecture diagram will be included upon successful rendering; see component breakdown above)*

## 3. System Flows
### 3.1 Primary Approval Flow
- Opportunity created/updated with TCV and discount.
- If discount >10%, Deal Desk approval triggered.
- If TCV >$500K, Finance approval triggered (Director: $500K–$2M; CFO: >$2M).
- Both tracks run in parallel.
- Both must approve for deal to proceed; rejection from either returns combined comments to rep.
- Re-submission restarts both tracks; approvals do not carry over.

### 3.2 Notifications
- Email sent to relevant approver(s) upon trigger.
- MS Teams channel post upon Finance track trigger/rejection/approval.

### 3.3 Reporting
- Monthly dashboards for Finance: approval queue metrics, time by sub-tier, rejection reasons.
- Same metrics available to Sales Ops dashboard.

### 3.4 Error & Retry Handling
- On rejection, Opportunity is updated with comments, resubmission restarts both tracks.
- Audit log maintained for all approvals, rejections, and comments.

## 4. Data Architecture
### 4.1 Entity & Data Model
| Entity              | Fields                      |
|---------------------|-----------------------------|
| Opportunity         | Id, TCV, Discount, Stage, ApprovalStatus |
| ApprovalRequest     | OpportunityId, Approver (Deal Desk/Finance), SubTier, Status, Comments, Timestamp |
| NotificationEvent   | ApprovalRequestId, Type (Email, Teams), Status, Timestamp |
| ApprovalHistory     | OpportunityId, ApprovalStage, Comments, DateTime |

### 4.2 Storage & Backup
| Data Type                | Storage           | Retention | Backup Strategy          |
|--------------------------|-------------------|-----------|--------------------------|
| Opportunity              | Salesforce DB     | 7 years   | Daily backup, 30-day retention |
| Approval History         | Salesforce DB     | 7 years   | Daily backup, 30-day retention |
| Notification Events      | Azure Blob Storage| 1 year    | Weekly archiving             |
| Dashboard Aggregates     | Salesforce DB     | 2 years   | Weekly backups               |

## 5. Technology Stack
| Component          | Technology               | Purpose                       | Rationale & Alternatives                 |
|--------------------|-------------------------|-------------------------------|------------------------------------------|
| Approval Workflow  | Salesforce Flow Builder  | Flexible multi-criteria routing| Native, visual, extensible; Apex possible|
| Integration        | Salesforce HTTP Callout  | MS Teams webhook notification  | Real-time, scalable; Zapier possible     |
| Database           | Salesforce DB            | Primary record/storage         | Native, secure, audit compliant          |
| Reporting          | Salesforce Reports/Dashbd| SLA monitoring, queue analysis | Native, customizable; Power BI alt.      |
| Notification Arch  | Azure Blob Storage       | Notification event archive     | Durable, cost-effective                  |
| Monitoring         | Azure Monitor            | Notification delivery, errors  | Unified, cross-cloud                     |
| IaC                | Terraform                | Azure storage provisioning     | Standard, repeatable                     |

## 6. Integration Architecture

### 6.1 Integration Patterns & Events
- Salesforce Platform Events/fire approval updates
- HTTP Callout via Flow Builder triggers MS Teams channel post with deal, approval status, reason
- Email notification via Salesforce standard email template system
- Event log archived to Azure Blob Storage for compliance and audit

### 6.2 API Endpoints
| Endpoint                   | Method | Description                        | Auth      | Rate Limit   |
|----------------------------|--------|------------------------------------|-----------|--------------|
| /opportunity/{id}/approve  | POST   | Trigger approval (Deal Desk/Finance)| Salesforce | 100/min      |
| /notification/send         | POST   | Send notification to Teams/email    | OAuth2    | 1000/min     |
| /dashboard/finance         | GET    | Approvals dashboard metrics         | Salesforce | 100/min      |
| /approval/history/{id}     | GET    | Retrieve approval history           | Salesforce | 100/min      |

### 6.3 Events
| Event Name | Producer      | Consumers    | Schema                                         |
|------------|-------------- |------------- |------------------------------------------------|
| ApprovalTriggered | Salesforce Flow | Deal Desk/Finance | {OpportunityId, TriggerType, Approver, Timestamp} |
| ApprovalCompleted | Deal Desk/Finance | Notification Service | {OpportunityId, Status, Approver, Decision, Comments, Timestamp} |
| NotificationSent | Notification Service | Archive/Audit      | {ApprovalRequestId, Type, Status, Timestamp}       |

## 7. Deployment Architecture

**Infrastructure:**
- Salesforce app and workflow configuration
- Azure Blob Storage (for notification archive)
- Azure Monitor (for alerting and monitoring)
- MS Teams connector (Teams channel webhook)

**Scaling Strategy:**
- Workflow and notification integration scale automatically with Salesforce limits; Azure Blob Storage scales elastically; MS Teams channel posts rely on webhook throughput (SLA: <1s/call)

**IaC & CI/CD:**
- Terraform for Azure resource provisioning
- Versioned pipeline for Salesforce workflow metadata
- GitHub Actions for CI of integration glue code

## 8. Security Architecture

| Layer       | Control                      | Implementation                                      |
|------------ |----------------------------- |-----------------------------------------------------|
| Network     | Private endpoints            | Azure Blob Storage accessed via private endpoint     |
| Application | Approval auth                | Salesforce internal user permissions, RBAC           |
| Application | Teams webhook token          | Rotating secrets, managed in Azure Key Vault         |
| Data        | Encryption at rest           | Salesforce native, Azure Storage CMK                 |
| Data        | Encryption in transit        | TLS 1.3 everywhere                                  |
| Audit       | Approval log                 | Immutable audit history in Salesforce                |

## 9. Performance & Capacity

| Metric                  | Target                 | Measurement                  |
|------------------------ |----------------------- |---------------------------- |
| Approval Latency (p95)  | <48 hours              | Salesforce/Integration logs  |
| Notification Latency    | <5s                    | Azure Monitor                |
| Monthly Throughput      | 40 deals/month (TCV >$500K) | Salesforce approval stats |
| Notification Scalability| 1000 notifications/mo  | Azure Monitor                |
| Approval SLA            | 99.95%                 | Uptime monitors              |
| Audit log retention     | 7 years                | Salesforce/Blob Storage      |

**Capacity Planning:**
- Initial: 12–15 high-value deals per month, 40 notifications/month
- Storage: <10GB/year in Azure Blob Storage
- Notification SLA: <5s end-to-end

**Cost Model:**
| Resource                | SKU/Tier          | Monthly Cost (USD) |
|------------------------ |-------------------|------------------- |
| Azure Blob Storage      | Standard Hot      | $30                |
| Azure Monitor           | Standard          | $10                |
| MS Teams Webhook        | Included (O365)   | $0                 |
| Salesforce Platform     | Included          | $0                 |
| Terraform Pipeline      | Included          | $0                 |
| **Total**               |                   | **$40/month**      |

## 10. Monitoring & Observability

- Logging: All approval actions and notification calls recorded in Salesforce and Azure
- Metrics: Approval times (per tier), rejection rates, notification delivery rates
- Distributed tracing: Approval-to-notification journey tracked via logs
- Alerts: High rejection rates, notification failures
- SLAs: Approval workflow uptime 99.95%, notification delivery <5s

## 11. Disaster Recovery

| Scenario               | RTO      | RPO        | Recovery Procedure                                 |
|----------------------- |----------|------------|---------------------------------------------------|
| Approval process error | <1 hour  | 0          | Manual remediation in Salesforce; retry submission |
| Notification failure   | <5 min   | 0          | Retry from log, alert team                         |
| Data corruption        | <24 hrs  | <24 hrs    | Restore Salesforce/Blob from backup                 |
| Integration outage     | <30 min  | 0          | Manual fallback (email), restore from IaC           |

- Backups: Daily Salesforce & Azure Blob, 30-day retention
- IaC/template restore for Azure resource recovery

## 12. Implementation Roadmap

| Phase                        | Duration  | Deliverables                                  | Dependencies          |
|------------------------------|-----------|-----------------------------------------------|-----------------------|
| 1. Salesforce Flow/Approval  | 1 week    | Parallel approval flow implemented, roles set  | None                  |
| 2. Teams/Webhook Integration | 1 week    | Notification connected to Finance channel      | Phase 1               |
| 3. Dashboard Reporting       | 1 week    | Finance/SalesOps approval dashboards live      | Phase 1               |
| 4. Backup, Monitoring & DR   | 3 days    | Backup and observability configured            | Phases 1–3            |
| 5. Testing & UAT             | 1 week    | Integration, SLA, permission tests             | All prior             |
| 6. Go-live & Training        | 3 days    | System live, handover, user training           | All prior             |

## 13. Appendices

### Glossary
- **TCV:** Total Contract Value (Salesforce Opportunity field)
- **BRD:** Business Requirements Document
- **SLA:** Service-Level Agreement
- **RTO:** Recovery Time Objective
- **RPO:** Recovery Point Objective

### References
- Meeting_Note_02_High_Value_Deal.md
- Salesforce Approval Workflow Best Practices (internal knowledge base)

### Testing Strategy
- UAT for all approval scenarios (parallel, sequential, rejection flow)
- Integration tests with MS Teams webhook (failure, latency, content)
- Dashboard report consistency tests
- Security test: permission denial for unauthorized approval

### Risks
| Risk                                   | Impact   | Probability | Mitigation                                  |
|---------------------------------------- |----------|-------------|----------------------------------------------|
| Notification integration fails          | Medium   | Low         | Manual email fallback, scheduled retries     |
| Finance team overload                   | Medium   | Medium      | Threshold re-evaluation every 6 months       |
| Audit log data loss                     | High     | Low         | Daily/weekly backup, test restores           |
| Approval delegation not configured      | High     | Low         | Regular permission reviews, test submissions |