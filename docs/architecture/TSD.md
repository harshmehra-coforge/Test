# Technical Specification Document (TSD): Enhanced Discount Approval Workflow

---

## 1. Solution Overview

This document describes the technical design for enhancing the Salesforce Opportunity discount approval workflow to introduce a dual-tier sequential approval process, enforce SLAs and escalation policies, enable multi-channel notifications, and ensure robust audit logging as mandated by Q1 FY26 audit findings. It is grounded in “Meeting_Note_01_Discount_Depth 2.md,” the BA-dynamic-agent/BRD.md, and aligned user stories.

---

## 2. Architecture Context

- **System:** Salesforce CRM (Cloud-based, extensible via Flow, Apex, and AppExchange packages)
- **Integration Point:** Salesforce Process Builder/Flow, email integration, Slack integration (via Slack API/Service)
- **User Roles:** Deal Desk Manager, VP Sales, Finance Director, Executive Assistant, Opportunity Owner (Sales Rep)
- **Approval Chain Levels:**
  - Tier 1: Deal Desk (10–100%)
  - Tier 2: VP Sales (only if discount >25%)
  - Informational: Finance Director (not an approver)

---

## 3. Requirements Traceability

The technical design implements all business requirements and user stories from BA-dynamic-agent/BRD.md and referenced meeting notes. All cases of discount >25% must trigger new sequential workflow, SLA enforcement, notifications, and audit logging.

---

## 4. Key Components

1. **Approval Routing Engine (Salesforce Flow/Apex Logic):** Implements sequential routing rules based on discount thresholds.
2. **Notification Service:** Dispatches emails and Slack messages to targeted users/roles. Handles finance notification and automatic escalation.
3. **SLA & Escalation Timer Engine:** Monitors action SLA on each approval record; triggers notifications/escalation if times are exceeded.
4. **Audit Logging Mechanism:** Persists routing actions, notifications, outcomes, and SLAs.
5. **Delegate and Coverage Module:** Supports delegate assignment for VP Sales and handles OOO (Out-of-Office) logic.

---

## 5. High-Level System Architecture

**Process Flow**

1. **Opportunity Created/Updated**
2. **Discount Level Evaluated:**
    - ≤10%: No special approval.
    - 10–25%: Routes to Deal Desk for single approver review.
    - >25%: Sequential two-step approval — Deal Desk, then VP Sales; triggers Finance notification.
3. **SLA Enforcement:**
    - Deal Desk: 24h; no escalation needed
    - VP Sales: 48h; if not approved/rejected in time, auto-escalate to Executive Assistant
4. **Notification Layer:**
    - VP Sales: Email, Slack (prefer Slack)
    - Finance: Email (informational)
    - Escalation: EA via Email/Slack
5. **Rejection/Re-submission:**
    - If rejected at any level, returns to Owner with all comments; resubmission restarts chain at Deal Desk

<!-- Mermaid: Visual architecture diagrams will be included once rendering is functional. -->

---

## 6. Component Design & Responsibilities

### 6.1 Approval Routing Engine (Salesforce Flow/Apex)
- Evaluates Opportunity discount % on create/update
- If >25%, routes for Deal Desk, then on approval, triggers VP Sales approval
- Only after Deal Desk step is approved does VP Sales receive record
- Restart approval chain on reject/re-submit

### 6.2 Notification Service
- Sends actionable notifications to:
  - Deal Desk (when triggered, 10%+)
  - VP Sales (if >25%, after Deal Desk approval; both Email and Slack)
  - Executive Assistant (only on VP SLA breach)
  - Finance (copy on deep discount, email, no action required)
- Uses native Salesforce email, Slack via Salesforce-Slack connector/API

### 6.3 SLA & Escalation Timer Engine
- Monitors outstanding approval records
- If SLA expires: auto-escalates to Executive Assistant
- SLA config: Deal Desk = 24h, VP Sales = 48h

### 6.4 Audit Logging
- All approval actions, comments, SLA time stamps, notification delivery, and escalation events logged per record
- Immutable journal for audit/review (Salesforce field history + custom object or platform event)

### 6.5 Delegate & Coverage
- Support delegate assignment for VP Sales
- OOO logic, availability check, delegate fallback if primary is unavailable

---

## 7. Data Model & Integration

**Opportunity Object Enhancements:**
- Approval Stage
- Discount Percent/Depth
- Approval Status (per tier)
- Approver(s): Fields for Deal Desk, VP Sales, Delegate
- SLA timestamps (entered/approved/rejected)
- Rejection Reason (comments from both tiers)
- Audit Log Reference (journal entries)

**Integration Points:**
- **Slack Integration**: Slack App/Connector + Apex for notification API
- **Notification Emails**: Salesforce native/email relay

---

## 8. Security, Access Control, & Compliance
- Approvers (Deal Desk, VP Sales, Delegates, EA): CRUD access only to approve/reject/comment
- Finance: Read-only notification access (no action rights)
- Audit logs accessible only by Admins/Auditors
- All user actions authenticated by Salesforce SSO/Managed Identity (Zero Trust enforcement)
- RBAC: Role-based field and object-level permissions; no exposure of approval data to other reps

---

## 9. Non-Functional Requirements (NFRs)
- **Latency:**
    - Approval routing/notification <5s per workflow transaction (p99)
    - SLA breach escalation process <60s from timer expiry to EA notification
- **Throughput:**
    - Support >300 approval requests/month (scalable)
- **Availability:**
    - Workflow and notification services ≥99.9% uptime monthly
- **Auditability:**
    - 100% of approval/rejection/notification events logged; retention ≥7 years
- **Security:**
    - All data in motion/encrypted at rest; MFA for all direct approval actions
- **Cost:**
    - Salesforce flows, notification, integration, and logging solution total run rate < $800/month

---

## 10. Alternatives & Trade-Offs

### a. Parallel vs. Sequential Routing
- **Chosen:** Sequential — enforces executive review only after commercial vetting (minimizes exec distraction)
- **Alternative:** Parallel routing would add load to VP/Finance, undermine governance. Not chosen.

### b. Notification Channels (Slack, Email)
- **Chosen:** Slack + Email for VP, email for Finance, escalation to EA
- **Alternative:** Use only Email — not aligned with modern workflow, delays action

### c. Salesforce-Only vs. Integration Extensions
- **Chosen:** Leverage Salesforce + Slack API integration for robust comms
- **Alternative:** Pure email is less auditable, limited user engagement

---

## 11. Implementation Guidance
- Use Salesforce Flow as primary routing engine; augment with Apex as needed for custom logic and OOO handling
- Slack API/AppExchange for real-time, auditable notification delivery (direct to user/channel, fallback to email)
- Timer-based record triggers for SLA enforcement; platform events for audit logging
- Unit/integration test approval chain; simulate SLA breaches and rejection chain restart
- Monitor workflow with integrated dashboards (Salesforce reports, Slack bot summaries if needed)

---

## 12. Future Enhancements
- Configurable discount thresholds and tier escalation matrix
- Automated monthly compliance reporting to Finance and Audit
- In-app notification on Salesforce mobile

---

## 13. References
- Meeting_Note_01_Discount_Depth 2.md
- BA-dynamic-agent/BRD.md
- PO-dynamic-agent/USER_STORIES.md
