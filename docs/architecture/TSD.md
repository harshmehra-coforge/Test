# Technical Specification Document
## Salesforce Discount Approval Enhancement
**Date:** 2026-10-08  
**Version:** 1.0  
**Status:** Draft

## 1. Executive Summary

### 1.1 Purpose
The Discount Approval Enhancement introduces a robust two-tier approval workflow for high-value (over 25%) discounts in Salesforce Opportunity Management. This ensures proper governance, auditability, and accountability, especially for large deals, while keeping the process seamless for standard discounts.

### 1.2 Scope
**In Scope:**
- Sequential second-tier approval for Opportunity discounts >25%: Deal Desk → VP Sales
- Courtesy notifications to Finance for >25% discount flows
- SLA enforcement for Deal Desk (24h) and VP Sales (48h), including auto-escalation to the Executive Assistant
- Full approval restart with comments on any rejection

**Out of Scope:**
- Changes to approvals for discounts ≤25%
- Additional approval tiers or alterations to existing Deal Desk SLAs
- Community/external user integration

### 1.3 Key Decisions
1. **Leverage Native Salesforce Approval Framework (Apex, Flow, LWC):** Extends standard objects for maintainability and compliance.
2. **Sequential Approval with SLA Enforcement:** Chose a sequential tiered path (Deal Desk → VP Sales) over parallel for clear accountability.
3. **Automated Notifications:** All alerts, escalations, and FYI emails handled by Salesforce notifications for auditability and low ops overhead.
4. **Audit Logging and Comments Relay:** Uses ProcessInstance and ProcessInstanceStep to capture details, ensuring compliance.
5. **Restart-on-Rejection:** Any rejection restarts the flow from Deal Desk, preserving business integrity and feedback.

---

## 2. System Architecture

### 2.1 High-Level Architecture
![Architecture Diagram](ARCHITECT_TOOL_ERROR)

#### Component Breakdown
**Presentation Layer**
- Opportunity Record Page (Lightning Record Page)
- Discount Approval LWC (Lightning Web Component)

**Application Layer**
- Salesforce Approval Process (Process Builder/Flow)
- Apex Trigger and Apex Classes for orchestration, SLA checks, and escalation logic

**Integration & Notification Layer**
- Salesforce Notification System (email/app notification for Finance and SLA escalation)

**Data & Audit Layer**
- Opportunity Object (including new custom fields)
- ProcessInstance & ProcessInstanceStep (approval event records)

---

## 3. System Flows

### 3.1 Primary Flow: Discount >25% Approval
#### Steps:
1. Opportunity Owner edits `Discount__c` (>25%) and submits.
2. Approval starts. Deal Desk receives approval request (SLA: 24h).
3. If approved, escalates to VP Sales (SLA: 48h).
   - If VP Sales breaches SLA, auto-escalate to Executive Assistant.
4. Finance notified (courtesy) via email upon triggering >25% approval.
5. On any rejection, capture rejection comments, return to owner for edit, and restart approval from Deal Desk.

### 3.2 Supporting Flows
- For ≤25%: existing process (≤10%: auto-approve, >10%–25%: Deal Desk only).
- All approval/rejection events, comments, and notification logs are tied to related records for auditability.

### 3.3 Error & Retry Handling
- Approval process errors (e.g., notification failure) are surfaced to Salesforce admin (standard monitoring).
- Missed SLA triggers auto-escalation workflow.
- If delegate (EA) unavailable, error elevates per escalation matrix.

---

## 4. Data Architecture

### 4.1 Entity & Data Model
- **Opportunity**: Key source of truth; stores `Discount__c`, `Approval_Status__c`, `OwnerId`, etc.
- **ProcessInstance**: Represents approval processes; links to Opportunity.
- **ProcessInstanceStep**: Individual approval actions within a process.
- **New/Enhanced Opportunity Fields:**
  - `Second_Tier_Approval_Triggered__c` (Checkbox)
  - `Second_Tier_Approval_Comments__c` (Long Text Area)

#### Data Storage & Retention
| Data Type                       | Storage   | Retention | Backup Strategy             |
|----------------------------------|-----------|-----------|-----------------------------|
| Opportunity records              | Standard  | Lifespan  | Salesforce standard backup  |
| Approval events and comments     | Standard  | 5 years   | Salesforce backup & export  |
| Notification event logs          | Standard  | 1 year    | Salesforce backup/export    |

### 4.2 Data Access Patterns
- LWC reads Opportunity fields for UI display, progress indicators, and comments.
- Approval flows and Apex classes update both Opportunity and related ProcessInstance records.
- Reports generated on approval SLA compliance and finance notifications.

---

## 5. Technology Stack

| Component          | Technology                    | Purpose                                      | Rationale & Alternatives                                 |
|--------------------|------------------------------|----------------------------------------------|----------------------------------------------------------|
| Approval Framework | Salesforce Approval Process   | Core approval routing                        | Standard, auditable, minimal maintenance vs. custom code |
| Workflow Orchestration | Salesforce Flow & Apex    | SLA, escalation, process control             | Declarative plus code for flexibility                    |
| UI                 | Lightning Web Component (LWC) | Custom UI for approval steps/comments        | Reusable, Lightning-compatible                           |
| Notification       | Salesforce Email + App        | Finance & escalation notifications           | Native, logged, supports audit requirements              |
| Audit              | ProcessInstance, Step         | Track actions, comments, times               | Native, reportable, compliant                            |

**Alternatives Considered:**
- Full custom Apex (higher maintenance/cost, less future-proof)
- Third-party workflow apps (less control/audit)

---

## 6. Integration Architecture

### 6.1 Patterns & Events
- Uses native event handling within Salesforce (approval process events, custom fields for triggers).
- All notifications routed via Salesforce Notification Builder and App Notification framework.

### 6.2 APIs & Endpoints
- No external REST API exposure required; standard Salesforce UI.
- Internal extension points:
  - Apex Trigger on Opportunity object for `Discount__c` changes >25%
  - Flow/Process Builder for workflow orchestration

### 6.3 Notification/Integration Table
| Trigger                                  | Consumer  | Channel         |
|------------------------------------------|-----------|-----------------|
| Approval step (SLA breach)               | Exec Asst | Email/App alert |
| Second-tier approval triggered           | Finance   | Email           |
| Approval/rejection/comments              | Opportunity Owner | Salesforce UI |

---

## 7. Deployment Architecture

### 7.1 Infrastructure & Deployment
- All configuration deployed via Salesforce Change Sets (Sandbox → UAT → Production)
- Apex, LWC, Flow metadata versioned using Salesforce DX and Git
- UAT includes end-to-end tests with test users for each tier (Deal Desk, VP Sales, Exec Asst, Finance, Owner)

### 7.2 Scaling, Reliability, and Compliance
- 100% SaaS/multi-tenant (Salesforce scalability, monitored by Salesforce Health)
- Standard Salesforce DR: daily backup/restore, replication across SFDC infra
- All managed via Salesforce administration UI; no external infra needed

### 7.3 CI/CD & Automation
- DevOps via Salesforce DX commands (source push/pull/test)
- PR validation branch for Apex and LWC changes, auto-run tests
- Automated regression suite (Apex, LWC, Flows)

---

## 8. Security Architecture

| Layer        | Control                                | Implementation                                        |
|--------------|----------------------------------------|------------------------------------------------------|
| Network      | Salesforce platform isolation          | All activity within Salesforce secured perimeter      |
| Application  | RBAC, Profile & Permission Sets        | Only assigned users can submit/approve/override       |
| Data         | Field-level security (FLS)             | Sensitive info (discount, approval comments) restricted|
| Data         | Audit logging                          | All approval/rejection actions written to history     |
| Application  | MFA & SSO enforcement                  | Salesforce Org SSO for Deal Desk & Execs             |

- All notifications and comments are subject to Salesforce data protection, compliance, and retention policies.

---

## 9. Performance & Capacity

| Metric                        | Target                        | Measurement                      |
|-------------------------------|-------------------------------|----------------------------------|
| Approval UI Latency (p99)     | < 500ms                       | Salesforce Lightning report      |
| Workflow Start → First Action | < 5 seconds                   | Approval audit log               |
| End-to-End Approval (best case - both approve) | < 4 hours                     | Log time difference              |
| Max concurrent approvals      | 200 (per region, Salesforce managed)    | Platform limits + logs           |
| SLA violation auto-escalation | < 2 minutes from breach       | Email log                        |
| Monthly downtime (planned + unplanned) | < 12 minutes (99.98%)           | Salesforce Trust                 |

- Solution must not degrade Opportunity record page perf by >50ms
- All changes and UI components optimized for desktop/mobile

**Capacity Planning:**
- User base: ~300 Direct Users (20 Deal Desk, 5 VP Sales, 2 Exec Asst, 15 Finance, 250 Owners)
- Peak active approvals: estimate 60/hour
- Storage growth: <0.5GB/year for new audit fields/records

---

## 10. Monitoring & Observability

- All approval/rejection actions, SLA violations, and escalations captured in ProcessInstance logs.
- Lightning dashboards for SLA compliance (Deal Desk, VP Sales action times)
- Automated reports for approval rates, notification delivery (email/app status), and escalation trends
- Error reporting integrated with Salesforce admin alerts; critical failures escalate to admin support
- SLOs: 100% approval/rejection events logged, <60s alert lag for failed notifications, SLA breach detection 100% reliable

---

## 11. Disaster Recovery

| Scenario                   | RTO         | RPO         | Recovery Procedure                      |
|----------------------------|-------------|-------------|------------------------------------------|
| Individual approval failure| < 5 minutes | 0           | Workflow restart by admin or owner       |
| Notification delivery fail | < 10 minutes| 0           | Manual resend by admin                   |
| Data corruption            | < 1 hour    | < 24 hours  | Restore from Salesforce backup/export    |
| SFDC region outage         | < 30 min    | < 1 hour    | Failover by Salesforce Platform Ops      |

- Backup via standard Salesforce export/restore (daily automated, on-demand by admin)
- All audit fields included in backup scope

---

## 12. Implementation Roadmap

| Phase            | Duration  | Deliverables                                          | Dependencies  |
|------------------|-----------|------------------------------------------------------|---------------|
| Design & Config  | 1 week    | Approval flow, SLA logic, new fields added            | BRD signoff   |
| Development      | 2 weeks   | Apex triggers/classes, LWC, notifications             | Salesforce admin, users available for UAT |
| Testing          | 1 week    | UAT with all approval tiers, negative/edge cases      | Sample records, test users  |
| Rollout & Training| 1 week   | User training, production deploy, support ready       | Documentation, go-live window |

- Document and record all flows, rollback strategies, and escalation plans during implementation

---

## 13. Appendices

### 13.1 Glossary
- **LWC:** Lightning Web Component
- **SLA:** Service-Level Agreement
- **SFDC:** Salesforce.com
- **ProcessInstance:** Salesforce object recording approval process

### 13.2 References
- BRD (5).md (source)
- Discount Approval Enhancement stakeholder meeting notes
- Salesforce Opportunity Management Documentation

### 13.3 Risks & Mitigations
| Risk                               | Impact     | Probability | Mitigation                                                 |
|------------------------------------|------------|-------------|------------------------------------------------------------|
| VP Sales unavailable for approval  | High       | Medium      | Auto-escalate to delegated Exec Asst                        |
| Notification delivery of finance   | Medium     | Low         | Use native notification event, test in UAT                 |
| User confusion on rejection/retry  | Medium     | Medium      | Comment bundling, process user training                     |
| Delays due to misconfiguration     | High       | Low         | Sandbox/UAT, change set validation                          |

### 13.4 Testing Strategy
- Apex unit tests: 90%+ coverage on all logic
- UAT scripts for each approval scenario (approve, reject, SLA breach, notify)
- Notification delivery verification: test emails/app events for all Finance, escalation recipients
- End-to-end smoke test before go-live

---