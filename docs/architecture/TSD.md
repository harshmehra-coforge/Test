# Technical Specification Document (TSD)

## Solution Title
Discount Approval Enhancement – Salesforce Opportunity Management

---

## 1. Overview

This TSD outlines the technical solution for enhancing the Opportunity discount approval process within Salesforce, adding a second-tier approval for discounts >25%. The goal is improved governance, auditability, and compliance while retaining the existing workflow for smaller discounts.

---

## 2. Context & Rationale

- Current process is single-tier (Deal Desk) for 10%-25% discounts.
- Governance gaps exist for high-value (>25%) discounts.
- Sequential executive approval (VP Sales) adds accountability.
- Finance visibility is increased via notification (no response required).
- Process restart on rejection ensures auditability and integrity.

---

## 3. Functional Requirements Mapping

| ID     | Requirement Description                                          |
|--------|------------------------------------------------------------------|
| BR-01  | Second-tier approval for >25% Opportunity discounts              |
| BR-02  | Sequential routing: Deal Desk → VP Sales                         |
| BR-03  | Finance notified on second-tier triggers                         |
| BR-04  | SLA: Deal Desk (24h), VP Sales (48h), auto-escalation supported  |
| BR-05  | Auto-escalate to VP Sales EA on SLA breach                       |
| BR-06  | Rejection restarts chain from Deal Desk                          |
| BR-07  | Rejection comments combined and shown to Deal Owner              |

Refer to BRD (5).md for full business rules and NFR mappings.

---

## 4. Solution Architecture

**Platform:** Salesforce Lightning (Apex, LWC, Flow)
**Surface:** Opportunity object record pages, notifications via AppvlWorkItemNtfcnEvent
**Integration:** No external integrations required, all native Salesforce

### Key Components
- Apex Classes: Approval orchestration, SLA timer logic, notification trigger, escalation
- Lightning Web Components (LWC): UI for comments and approval/rejection review
- Flows (Process Builder / Approval Process): Sequential routing, process restarts
- Email Notification: Finance & escalation with templated content

---

## 5. Main Workflow (Sequential Approval)

Discounts ≤10%:
- No approval required (existing process)

Discounts >10% and ≤25%:
- Deal Desk approval only (existing process)

Discounts >25%:
- Initiate approval process
- Route to Deal Desk for initial review
- If approved, escalate to VP Sales (or delegate)
- If SLA breached, auto-escalate to VP Sales EA
- If any tier rejects, Opportunity returns to Deal Owner; approval chain restarts
- All rejection comments captured and displayed
- All actions logged for audit
- Finance receives notification automatically when second tier is triggered

---

## 6. Data Model & Object Enhancements

### Opportunity Object
- `Discount__c`: Discount value (source field)
- `Approval_Status__c`: Tracks current approval stage (new values: SECOND_TIER_PENDING, SECOND_TIER_APPROVED, SECOND_TIER_REJECTED)
- `Second_Tier_Approval_Triggered__c`: Checkbox (new)
- `Second_Tier_Approval_Comments__c`: Long Text Area (new)

### Notification Event
- Use existing `AppvlWorkItemNtfcnEvent` for Finance/SLAs

### Approval Workflow
- `ProcessInstance`, `ProcessInstanceStep`: Approval path, actor, and comments

---

## 7. SLA, Escalation & Notification Logic

- SLA timer started on VP Sales assignment (Apex Scheduler or Process Builder)
- If VP Sales does not approve within 48h, auto-route to Executive Assistant
- Finance notified (email) at initiation of second-tier approval (template-driven)
- All notifications use native Salesforce mechanisms

---

## 8. Auditability, Logging & Compliance

- Every approval/rejection step logged in `ProcessInstance`, including timestamp, actor, comments
- Combined rejection comments stored on Opportunity (`Second_Tier_Approval_Comments__c`)
- Restart history appended for compliance
- All log records accessible to internal audit/compliance teams

---

## 9. User Interface Enhancements

- LWC: Approval/rejection screen on Opportunity record, showing sequential progress
- All rejection comments visible to Deal Owner before re-submit
- Notifications summarized in record page sidebar/panel

---

## 10. Non-Functional Requirements (NFRs)

- **Performance**: Approval flow steps must complete end-to-end in <2 seconds per user action (except for SLA waiting periods).
- **Availability**: Workflow engine and notification system must maintain 99.9% uptime during business hours (Mon–Fri 08:00–20:00 UTC).
- **Auditability**: 100% of approval/rejection events are logged with actor, timestamp, and comments—reviewable by compliance in <24h.
- **Scalability**: Must support at least 10,000 Opportunities processed per month without performance degradation.
- **Usability**: Solution must integrate seamlessly with Lightning Experience (web & mobile) for 95%+ of current Opportunity users (Sales, Finance).
- **Notification SLA**: All email notifications (Finance & SLA) are sent within 30 seconds of workflow trigger.
- **Security**: Only assigned approvers and compliance/audit group users can view approval logs and comments. RBAC enforced using Salesforce permission sets. All communication (UI, notifications) uses secure Salesforce infrastructure.

---

## 11. Security & Compliance

- **Access Control**: Approval responsibilities assigned using Salesforce roles and permission sets.
- **Data Integrity**: Changes to Opportunity, approval workflow objects, and comments are tracked using Salesforce field history tracking.
- **Authentication/Authorization**: Only authenticated internal users can trigger or approve workflows; enforced via Org-wide sharing & managed identities.
- **Confidentiality**: Comments, logs, and approval records are never emailed to unauthorized users—notifications to Finance are summary only (no details).
- **Compliance Mappings**: Solution aligns with SOX and internal audit policy requirements for delegated authority and electronic approvals.

---

## 12. Alternatives & Rationale

- **Alternative: Manual Multi-Tier Approval**
  - Cons: Prone to error, audit gaps, SLA breaching, lacks automation.
- **Alternative: Third-party workflow add-on**
  - Cons: Higher cost, integration complexity, lower maintainability, potential performance/security concerns.
- **Chosen: Native Salesforce Enhancement (Apex, LWC, Flow)**
  - Pros: Seamless integration, low TCO, robust audit/compliance, leverages existing investment.

---

## 13. Trade-offs & Decisions

- **Native Salesforce approach**: Preferred for maintainability, audit, and compliance vs. custom middleware or third-party workflows.
- **Automated vs. Manual Notifications**: Fully automated; manual approach rejected due to risk of missed steps and SLA violation.
- **Delegate Handling**: SLA auto-escalation to Executive Assistant provides coverage vs. risk of single point of approval failure.

---

## 14. Implementation Plan

- Enhance Approval Process in Salesforce: Add second-tier logic and new fields via setup, Apex triggers, LWC, and Flow.
- Configure SLAs using Apex Scheduler and Process Builder automation.
- Update Opportunity record page UI (LWC components) for comments visibility.
- Deploy notification templates for Finance and escalations.
- Conduct UAT with key business users; iterate based on feedback.
- Go-live following user training and documentation rollout.

---

## 15. Operations & Support

- **Monitoring**: Use Salesforce admin dashboards to track process bottlenecks, notification delivery, and SLA delays.
- **Support**: Tier 1 (Help Desk) and Tier 2 (Salesforce Admins) for troubleshooting.
- **Issue Escalation**: Route to Salesforce DevOps in case of automation failure or SLA escalation failure.
- **Audit & Reporting**: Monthly compliance review on audit log completeness and SLA performance.

---

## 16. Deployment & Change Management

- **Environments**: Dev, QA, UAT, Production (Salesforce Org)
- **Deployment Method**: Salesforce Change Sets or SFDX (recommended for CI/CD), following master branch merge
- **Rollback Strategy**: Change set reversal or restore via metadata backup
- **Change Communication**: Release notes circulated; user training sessions prior to production deployment

---

## 17. Key Risks & Mitigation

| Risk                                    | Mitigation                                      |
|------------------------------------------|--------------------------------------------------|
| VP Sales unavailable                     | SLA auto-escalation to Executive Assistant       |
| Notification failures                    | Use native Salesforce event, UAT testing         |
| User confusion (process restart)         | Inline UI commentary, clear rejection notes      |
| Deployment delays (DevOps/Admin)         | Planning buffer, advance resource scheduling     |

---

## 18. Open Questions & Dependencies

- Confirmation: VP Sales and delegate participation confirmed before go-live
- Admin: Salesforce admin resource available for configuration
- Training: User training and guidance scheduled pre-launch

---

## 19. Appendices

- BRD (5).md — Business Requirements Document source
- Opportunity Management Stakeholder Meeting Notes

---

## 20. Diagrams

### Solution Workflow (Mermaid)

```
graph TD
  Opportunity[Opportunity Record]
  CheckDiscount[Check Discount Value]
  DealDesk[Deal Desk Approval Tier]
  VPSales[VP Sales Approval Tier]
  Finance[Finance Notification]
  VPExecutiveAssistant[Executive Assistant (Escalation)]
  Rejection[Rejection: Comments to Deal Owner]
  Restart[Owner Re-Submits: Restart from Deal Desk]
  Approved[Discount Approved]

  Opportunity --> CheckDiscount
  CheckDiscount -->|≤10%| Approved
  CheckDiscount -->|>10% & ≤25%| DealDesk
  DealDesk -->|Approved| Approved
  DealDesk -->|Rejected| Rejection
  CheckDiscount -->|>25%| DealDesk
  DealDesk -->|Approved| VPSales
  VPSales -->|Approved| Approved
  VPSales -->|Rejected| Rejection
  VPSales -->|SLA Breach| VPExecutiveAssistant
  VPExecutiveAssistant --> Approved
  DealDesk -.-> Finance
  VPSales -.-> Finance
  VPSales --> Finance
  Rejection --> Restart
```

### Solution Workflow Diagram

![Discount Approval Workflow](error rendering diagram: see below)

---

## End of TSD
