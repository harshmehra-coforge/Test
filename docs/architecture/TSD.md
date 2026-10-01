# Technical Specification Document
## Discount Approval Enhancement for Salesforce Opportunity Management
**Date:** 2026-10-01  
**Version:** 1.0  
**Status:** Draft

---

## 1. Executive Summary

### 1.1 Purpose
The Discount Approval Enhancement addresses the business need for improved governance and auditability over high-value discounts in Salesforce Opportunity Management. By adding a second-tier approval and automating SLAs and escalations, it ensures process integrity, accountability, and compliance.

### 1.2 Scope
**In Scope:**
- Second-tier sequential approval (Deal Desk, then VP Sales) for >25% discounts.
- Finance notifications for >25% approvals.
- SLA enforcement (24h/48h), auto-escalations, and process restart on rejection.
- Enhanced comments tracking and audit logging via existing Salesforce features.

**Out of Scope:**
- Changes to existing single-tier approval (10-25%).
- Any changes outside internal Salesforce users or Opportunity Discount approval processes.
- Additional approval tiers beyond two.

### 1.3 Key Decisions
1. **Extend native Salesforce approvals:** Uses Apex, Flow, and LWC on record pages for easy adoption and auditability.
2. **Sequential, rule-driven workflow:** Ensures Deal Desk approves first, followed by VP Sales, reducing risk of improper high-value discounting.
3. **Automated SLA & escalation logic:** SLA timers and escalations are managed in Apex/Flow, reducing manual monitoring workload.
4. **Finance notification as FYI:** Maintains compliance and reporting standards without requiring Finance action.

**Trade-offs:**
- Salesforce-centric solution simplifies execution but ties logic to Salesforce platform.
- Two-tier approval increases time-to-approval but increases governance and control.

---

## 2. System Architecture

### 2.1 High-Level Architecture

![Diagram](https://svgshare.com/i/16Ki.svg)

#### Components
- **Opportunity UI (LWC/Record Page):** User-facing discount submission
- **Apex Approval Logic:** Business rules, SLA, escalations
- **Flow Orchestration:** Stepwise approvals using Salesforce native tools
- **Deal Desk:** First-level approver
- **VP Sales:** Executive/second-level approver
- **Executive Assistant:** Escalation recipient for breached SLAs
- **Finance Notification (Email/App):** Courtesy notifications using existing channels

### 2.2 Component Breakdown
- **Presentation Layer:**
  - Salesforce Lightning Web Components (LWC), desktop and mobile
- **Application Layer:**
  - Apex classes for custom logic (approval routing, escalation, SLA monitoring)
  - Salesforce Flows for sequential approval orchestration
- **Notification Layer:**
  - Salesforce email engine, App notifications for Finance and escalation
- **Data Layer:**
  - Salesforce Opportunity, ProcessInstance, ProcessInstanceStep standard and custom fields

---

## 3. System Flows

### 3.1 Primary Approval Flow
The following sequence diagram describes the end-to-end process for submitting and routing an Opportunity discount above 25%:

**Approvers:** Deal Desk (first-tier), VP Sales (second-tier), Executive Assistant (escalation)
**Notifications:** Finance, Opportunity Owner

#### Discount Submission & Approval (Above 25%)

**Sequence Steps:**
1. Opportunity Owner submits for discount above 25%.
2. System triggers sequential approval: Deal Desk → VP Sales.
3. If VP Sales does not respond in 48h, escalate to Executive Assistant.
4. If any tier rejects, path restarts from Deal Desk upon resubmission; combined rejection comments are relayed.
5. Finance receives notification upon triggering second-tier approval.

---

### 3.2 Error Handling & SLA Enforcement
- Auto-approve not supported; all responses require explicit approval/rejection.
- SLA timers managed by Apex scheduler/Flow.
- Rejections at either tier result in process reset from Deal Desk.
- All rejection comments are bundled and shown to Opportunity Owner for action.
- Escalation notifications are sent after 48h VP Sales inaction.
- Audit logs maintained via ProcessInstance and ProcessInstanceStep.

---

## 4. Data Architecture

### 4.1 Data Model

#### Main Entities and Relationships
- **Opportunity** (core record): Tracks discount, approval status, owner
- **ProcessInstance**: Tracks approval flow execution state
- **ProcessInstanceStep**: Tracks individual approver steps and comments

#### Enhanced Custom Fields
| Object       | Field                                 | Type        | Description                           |
|--------------|---------------------------------------|-------------|---------------------------------------|
| Opportunity  | Discount__c                           | Percent     | Discount applied to the opportunity   |
| Opportunity  | Second_Tier_Approval_Triggered__c     | Checkbox    | Indicates if second-tier is triggered |
| Opportunity  | Second_Tier_Approval_Comments__c      | Long Text   | Stores rejection/comments             |
| ProcessInstanceStep | Comments                        | Long Text   | Approval/rejection comments           |

### 4.2 Storage, Retention, and Backup
| Data Type                          | Storage                | Retention    | Backup Strategy          |
|------------------------------------|------------------------|--------------|--------------------------|
| Opportunity records                | Salesforce CRM         | 7 years      | Salesforce Nightly Export|
| Approval process logs              | Salesforce ProcessInst | 7 years      | Included in backups      |
| Notification email logs            | Salesforce Email Logs  | 1 year       | Weekly export            |

---

## 5. Technology Stack

| Component              | Technology         | Purpose                                   | Rationale & Alternatives                                                        |
|------------------------|-------------------|-------------------------------------------|---------------------------------------------------------------------------------|
| Workflow Orchestration | Salesforce Flows  | Sequential approval, SLA, escalation      | **Chosen**: Native, maintainable. **Alt**: External BPM—adds integration/complexity.|
| Approval Logic         | Apex Triggers/Classes| Approval rules, custom notifications   | **Chosen**: Full access to Salesforce data/events                              |
| UI Components          | Lightning Web Components (LWC)| User entry, resubmit interface      | **Chosen**: Consistent UX, mobile-optimized                                    |
| Notification Engine    | Salesforce Email/AppNotif      | Automated emails/app alerts          | **Chosen**: Reuse existing infrastructure; no external dependencies            |

---

## 6. Integration Architecture

### 6.1 Integration Patterns & Events
- **Internal Salesforce Objects:** All process orchestration and record updates are managed via Apex and Flows—no external integration required.
- **Notifications:** Leverages Salesforce Notification/Event framework.
- **Escalation Engine:** Uses Salesforce Scheduled Actions for auto-escalation to Executive Assistant.

### 6.2 API Endpoints
Not applicable—logic resides within Salesforce, surfaced via UI actions and triggers.

### 6.3 Events
| Event Name       | Producer           | Consumers              | Schema Description                                         |
|------------------|--------------------|------------------------|-----------------------------------------------------------|
| DiscountApproval | Apex Approval      | Deal Desk, VP Sales    | {opptyId, discount, step, comments, owner}                |
| SLA_Escalation   | Scheduled Action   | Exec Assistant, Owner  | {opptyId, breachedStep, elapsedTime, escalationRecipient}  |
| FinanceNotify    | Apex/Flow          | Finance                | {opptyId, discount, time, owner, comments}                |

---

## 7. Deployment Architecture

### 7.1 Infrastructure Components
- **Salesforce Org:**  Production and Sandbox
- **Apex Codebase:** Version-controlled in Salesforce DX, deployed via Change Sets/CI
- **Flow Definitions:** Versioned and deployed alongside Apex
- **Notification Templates:** Managed in Salesforce Setup

### 7.2 Deployment Pipeline
- **Sandbox integration, User Acceptance Testing, Production deployment**
- **Automated testing:** Apex unit tests ≥80% coverage
- **Change Management:** Strict versioning, rollback via Change Sets

### 7.3 Scaling & Performance
- Salesforce SaaS platform handles elastic scale for process and workflow execution
- Approvals/Flows must process up to 2,000 concurrent opportunities with high SLAs
- End-user impact negligible on Opportunity record load/edit (<100ms added latency)

---

## 8. Security Architecture

### 8.1 Security Layers & Controls
| Layer         | Control                       | Implementation                                        |
|---------------|------------------------------|-------------------------------------------------------|
| Application   | RBAC, field-level security   | Profiles/permission sets for access control            |
| Data          | Encryption at rest/in transit| Salesforce platform-wide, built-in                    |
| Audit         | Approval/rejection logging   | ProcessInstance & ProcessInstanceStep, field tracking |
| Notification  | Secure email/app channel     | Salesforce notification infra, restrict to domain      |

### 8.2 Compliance & Auditability
- Full traceability of each approval step, action, and comment
- Audit trails accessible to compliance teams
- All notification triggers logged and auditable from Opportunity record

---

## 9. Performance & Capacity

### 9.1 Non-Functional Requirements (NFRs)
| Metric                                 | Target                 | Measurement                          |
|-----------------------------------------|------------------------|--------------------------------------|
| Approval latency (p95, end-to-end)      | < 2 minutes            | Field audit trails, Salesforce logs  |
| Max concurrent submissions              | 2,000/process instance | Salesforce bulk process test         |
| SLA enforcement (VP Sales)              | 48h, then escalate     | Field tracking, automated flags      |
| Opportunity record edit performance     | <100ms additional load | Lightning page perf monitoring       |
| Audit log accessibility                 | 100%                   | Compliance report, field tracking    |
| Availability                            | 99.9%                  | Salesforce platform SLOs             |
| Mobile/Desktop compatibility            | 100%                   | Lightning UI test coverage           |

### 9.2 Capacity Planning
- **Expected active users:** 500 (Deal Desk, Sales Reps, VP Sales, Finance)
- **Peak process volume:** 2,000 open discount approvals concurrently
- **Record growth:** <1,000 new discounted opportunities/month
- **Storage impact:** Negligible relative to standard Salesforce org limits

### 9.3 Cost Model
- Salesforce licensing (existing)
- Implementation: 40-60h Salesforce admin/dev/configuration (~$10,000 one-time)
- No incremental monthly platform cost (reuse existing Salesforce infrastructure)

---

## 10. Monitoring & Observability

### 10.1 Logging & Metrics
- **Approval actions:** Written to ProcessInstance/ProcessInstanceStep with timestamps
- **SLA/Escalation triggers:** Logged as custom events/fields
- **Opportunity & approval status:** Reportable in Salesforce Reports

### 10.2 Alerts & Runbooks
- **SLA alert:** Automated notification to Exec Assistant on 48h breach
- **Audit gap detection:** Weekly report for missing/late approvals
- **Health checks:** Scheduled process audits by admin/report

### 10.3 SLO/SLAs
- VP Sales approval must occur within 48h or escalation fires
- 99.9% process availability (Salesforce SLO)

---

## 11. Disaster Recovery

| Scenario                | RTO     | RPO  | Recovery Procedure                   |
|-------------------------|---------|------|--------------------------------------|
| Apex/Flow code issue    | <1h     | 0    | Rollback deployment via Change Set   |
| Data corruption         | <4h     | <24h | Salesforce restore from backup       |
| Process misfire         | <30min  | 0    | Admin reruns/repairs flow            |

**Backup Strategy:** Salesforce’s scheduled nightly export; weekly validation of backup integrity.

---

## 12. Implementation Roadmap

| Phase                | Duration | Deliverables                                 | Dependencies                          |
|----------------------|----------|----------------------------------------------|----------------------------------------|
| Phase 1: Design      | 1 week   | Detailed spec, diagrams, deployment plan     | BRD sign-off                          |
| Phase 2: Config & Dev| 2 weeks  | Salesforce Flows, Apex classes, LWC changes  | Phase 1, developer access             |
| Phase 3: Test/UAT    | 1 week   | Unit & UAT test coverage, SLA test results   | Phase 2 completion                    |
| Phase 4: Deploy      | 3 days   | Change set moves, comms, enablement          | UAT pass, user training               |
| Phase 5: Post-GoLive | 1 week   | Monitoring & hypercare, feedback collection  | Deployment, enablement                 |

---

## 13. Appendices

### 13.1 Glossary
- **SLA:** Service Level Agreement (maximum approval window)
- **UAT:** User Acceptance Testing
- **LWC:** Lightning Web Component
- **Apex:** Salesforce server-side programming language
- **ProcessInstance:** Salesforce object for workflow execution tracking

### 13.2 References
- Discount Approval Enhancement stakeholder meeting 2026-06-18
- Salesforce Opportunity Management documentation
- BRD (intake)

### 13.3 Testing Strategy
- **Unit tests:** 80%+ coverage on custom Apex logic, validation of Flow routing
- **UAT:** All scenarios tested—approvals, rejections, escalations, notifications
- **Performance:** Bulk approval load test (2,000+ concurrent)
- **Audit:** Rejection comment aggregation, notification delivery verified

### 13.4 Risks
| Risk                                          | Impact | Probability | Mitigation                            |
|-----------------------------------------------|--------|-------------|----------------------------------------|
| VP Sales/delegate unavailable                 | High   | Medium      | Escalation logic, clarify backup path  |
| Notification email failure                    | Medium | Low         | Use Salesforce native notification infra|
| User confusion on resubmission/restart        | Medium | Medium      | Enhanced messages, training            |
| Opportunity data inaccuracy                   | Low    | Low         | Data validation on submission          |

---

# End of Document