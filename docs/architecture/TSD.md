# Technical Specification Document (TSD)

## 1. Overview
This Technical Specification Document defines the enhanced Salesforce Opportunity discount approval workflow as per the BRD and discovery meeting. The solution introduces a second-tier executive approval for discounts above 25%, along with SLA management, cross-channel notification, rejection handling, and auditable traceability. The workflow targets compliance with Q1 FY26 audit findings and strengthens governance for high-value deals.

## 2. Context & Background
Recent internal audits uncovered governance gaps in the discount approval process—high-value deals with discounts above 30% were approved by a single Deal Desk resource, bypassing executive oversight. The enhancement ensures:
- Executive visibility and accountability for deep discounting
- Robust auditability of approval actions, decisions, and escalations
- SLA-driven response times to support deal velocity

**References:**
- [BRD, June 2026] — Business Requirements for Discount Approval Workflow Enhancement
- [Meeting Minutes, 20 June 2026] — Discovery session confirming sequential approval routing, notification, and SLA specification

## 3. Solution Requirements
### Functional
- Discounts 10–25%: Approval by Deal Desk (no change to current)
- Discounts >25%: Sequential approval — Deal Desk, then VP Sales (Suresh Iyer)
- Triggered courtesy notification to Finance Director (on >25% escalation)
- Approval/rejection via both email and Slack
- 24h SLA for Deal Desk, 48h for VP Sales
- SLA breach auto-escalates to Executive Assistant (Sunita Rao)
- On VP rejection, Opportunity and combined comments return to owner; resubmission restarts at Deal Desk

### Non-Functional
- All actions are logged in Salesforce for audit traceability
- Notifications are reliable, SLA-driven, and logged
- Modular configuration for future approver/notification changes
- Go-live by end of Q2 FY26

---

## 4. Architecture & Workflow

### 4.1 Logical Process Flow

The enhanced approval workflow is orchestrated by Salesforce Flow and integrated notification services:

1. Opportunity discount entered (>10%)
2. For 10–25%: Route to Deal Desk (Vikram Patel) for approval
3. For >25%:
   - Route to Deal Desk for approval
   - Upon approval, escalate to VP Sales (Suresh Iyer) via Slack and email
   - Finance Director (Anita Rao) receives informational notification immediately
4. SLAs enforced for each approval step (24h, 48h)
5. On SLA breach (VP step), escalate via Slack/email to Executive Assistant (Sunita Rao)
6. On rejection at any tier: inform Deal Owner, require resubmission, restart full chain

### 4.2 Workflow Mermaid Diagram

### 4.3 Process Steps
- Entry: Opportunity owner requests discount >10% (New Business/Renewal)
- Process branches at 25% threshold; >25% triggers two-stage approval
- Deal Desk (first stage) validates business need & compliance
- Upon Deal Desk approval:
  - If discount ≤25%: process complete
  - If discount >25%: escalate
    - VP Sales notified for approval (Slack & email; 48h SLA)
    - Finance receives courtesy notification (email, immediate)
- Rejections (any stage): All comments passed back to owner
- On SLA breach (VP stage): auto-notify Exec Assistant
- Resubmission always restarts full chain

---

## 5. Technical Architecture

### 5.1 System Components
- **Salesforce Flow:** Orchestrates Opportunity approval routing, triggers escalations, and tracks SLA timers.
- **Notification Service:** Integrates with Salesforce for outbound emails and Slack messages; handles delivery, logging, and retry.
- **SLA Engine:** Monitors flow steps, triggers escalation on deadline breach.
- **Audit Log Module:** Captures approval/rejection events, notification deliveries, and escalation triggers inside Salesforce for auditability.
- **Admin Configuration Panel:** Allows adding/changing approvers/notification targets by role, not hard-coded users.

### 5.2 Integration Points
- **Salesforce ↔ Email & Slack:** Outbound notification to users; responses update approval status
- **Salesforce User Roles:** Approver assignments respect org chart and delegate coverage
- **Administrative:** Tier-2 approver and finance targets configurable by Salesforce Admin

### 5.3 Security & Compliance
- **Authentication:** All user actions authenticated via Salesforce Single Sign-On
- **Authorization:** Only assigned approvers can act at their tier; audit trail enforced
- **Data Handling:** All workflow actions, notifications, and SLA triggers logged
- **RBAC:** Role-Based Access Controls for configuration management
- **Data Residency:** All data/assets stored in compliance with corporate policies

### 5.4 Deployment & Operations
- **CI/CD:** All Flow and integration code deployed via GitHub Actions into Salesforce lower and production orgs
- **Observability:** All errors, delays, and notification failures raised as Service Desk tickets and monitored via Salesforce report dashboards
---

## 6. Data Model

### 6.1 Object Model Changes
- **Opportunity Extension:**
  - Custom fields: approval status, current tier, approval/rejection comments, SLA start/end timestamps
  - Lookup relationships to enable multi-stage routing records
- **Audit Log Table:**
  - Tracks all approval actions, notification delivery, escalation events; timestamped and reportable
- **Notification Template Table:**
  - Templates for each notification type (approval, escalation, finance info), multi-channel

### 6.2 Data Flow
- All approval/rejection actions update Opportunity and Audit Log
- SLA timers written against approval chain; resets on each resubmission
- Notification delivery results stored for each triggered step
---

## 7. Non-Functional Requirements (NFRs)

| NFR Category          | Requirement                                       |
|-----------------------|---------------------------------------------------|
| Latency               | Notifications delivered < 1 minute from event     |
| SLA Compliance        | 99.5% of approvals/escalations within 5m of SLA   |
| Auditability          | 100% approval/rejection actions logged & reportable|
| Availability          | Workflow components ≥ 99.9% uptime                |
| Notification Delivery | ≥ 99.0% successful first-attempt delivery rate    |
| Security              | Conforms to company RBAC, SSO, audit policies     |
| Modifiability         | Approver/notify targets updatable w/o code deploy |
| Cost                  | Incremental run cost ≤ $400/mo (notification+SLA) |
| Scalability           | Up to 400 approval requests/month                 |

---

## 8. Tradeoffs & Alternatives

- **Alternative – Parallel or direct-to-exec approvals:**
  - Rejected to protect executive bandwidth and maintain Deal Desk diligence
- **Alternative – VP-only restart on rejection:**
  - Rejected for audit integrity; full chain restart closes loopholes
- **Workflow Engine – Managed Salesforce Flow vs. Custom App:**
  - Chose Salesforce Flow for no/low code, modularity, compliance
- **Notification via Email Only:**
  - Rejected, as Slack use greatly increases executive response times

---

## 9. Implementation Guidance

- Use Salesforce Flow for all routing, decision, and timer logic.
- Implement notification integrations via standard Salesforce email action plus Slack API/Webhook (using Salesforce Slack App or Workato if available).
- SLA timers tracked using Salesforce Flow wait/schedule/timeout steps; escalate via Flow trigger and notification action.
- Ensure notification failures (e.g., Slack API errors) create Service Desk ticket automatically.
- Test with historical data set to verify >25% path coverage, notification accuracy, and rejection resubmission enforcement.
- Configuration UI (admin panel) should use dynamic lists for executive/finance targets (no hardcoded emails/user IDs).
- Audit reports should be available to process owner and internal audit team via Salesforce dashboard.

---

## 10. Appendices

### 10.1 Reference Material
- [BRD] BA-dynamic-agent/BRD.md
- [Meeting Notes] Meeting_Note_01_Discount_Depth 2.md
- [Q1 FY26 Audit] (internal doc)
- [Current Workflow Doc] (internal doc)

### 10.2 Change Log
- v1.0 (23 Oct 2026): Initial version based on BRD and discovery meeting
---
