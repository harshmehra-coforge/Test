# Technical Specification Document (TSD)

## 1. Overview
This document details the technical system design for the High Value Deal Approval Enhancement project within Salesforce. The solution introduces parallel finance approval, maintains the isolated deal desk approval for high discounts, enforces SLAs, automates notifications, aggregates rejection comments, enables auto-approval for 0% discount renewals, and ensures robust permission handling and deployment rollback.

## 2. Problem Statement & Business Context
Salesforce's current approval process for Opportunities lacks parallelized, value-based approval and timely, multi-channel notifications for internal users. As deal sizes and discount volumes rise, compliance, speed, and auditability are mission-critical.

**Business goals:**
- Control and compliance (high-value/high-discount deals)
- Timely, multi-channel notification and escalation
- SLA-driven, auditable workflow
- Reduce manual overhead for renewals and rejections

## 3. Architecture Context & Key Decisions
- Cloud: Salesforce (core platform + Apex custom logic)
- Notifications: Salesforce Email, MS Teams Webhook (via Apex)
- SLA timings: 24/48h enforcement with escalation
- Data: Salesforce Opportunity, ProcessInstanceStep
- Security: Field-Level Security (FLS), with sharing Apex
- Audit: Full audit trail within Opportunity and approval objects
- Default: Approvals run in parallel if both triggers are met; logic isolation between tracks

## 4. Requirements Mapping
| # | Requirement | Implementation | NFR (quantifiable) |
|---|-------------|----------------|---------------------|
| BR-01 | Finance approval (> $500K) | Flow/Apex/Approval Process | Latency <2s trigger, SLA 48h |
| BR-02 | Dynamic approver by TCV | Metadata/Flow Routing | |
| BR-03 | Isolated deal desk (Discount >10%) | Approval Process | |
| BR-04 | Custom field: TCV | New field | Availability 99.95% |
| BR-05 | Finance notifications | Email, Teams, Apex svc | Delivery >99% |
| BR-06 | SLA enforcement/escalation | Flow + scheduled apex | SLA adherence 99.9%, Audit log 100% |
| BR-07 | Rejection comment consolidation | Process/Flow + update | |
| BR-08 | Restart on resubmission | Record-triggered flow | |
| BR-09 | Auto-approve renewal, 0% | Flow | |
| BR-10 | FLS/Permission compliance | Apex as SECURITY_ENFORCED | 100% adherence |
| BR-11 | Delivery reliability | Retries/logs on failed notif | |
| BR-12 | SLA/Audit metrics | Track in custom fields, logs | |
| BR-13 | Compliance | Change tracking/reportable | |

---

## 5. Solution Architecture Diagram

## 5. Solution Architecture Diagram

*(Diagram rendering failed; see sequence below for logical flow)*

### Opportunity Approval Workflow (Textual Sequence)
1. **Trigger**: Opportunity is created or updated.
2. **Check:**
   - If `Total_Contract_Value__c > $500K` → Start parallel Finance Approval Process.
   - If `Discount > 10%` → Start Deal Desk Approval (remains isolated).
   - If both: both processes run.
   - If `Type = Renewal` and `Discount = 0%`: Auto-approve (skip manual steps).
3. **Finance Approval:**
   - Approver = Director (≤ $2M) or CFO (> $2M).
   - Notification via Email/Teams on initiation.
   - SLA: notify at 24h (Director), escalate to CFO at 48h.
4. **Deal Desk Approval:**
   - Approval for Discount >10% Opportunities only.
   - SLA: Notify at 12h, escalate at 24h.
5. **Rejection Handling:**
   - Consolidate all rejection comments into `Opportunity.Description`.
   - If Opportunity is re-submitted (`StageName = 'Re-Submitted'`), both approvals restart, with prior statuses reset.
6. **Audit & SLA Tracking:**
   - All transitions/time stamps logged.
7. **Permissions/Audit:**
   - All Apex classes, flows, and processes respect Salesforce FLS and sharing model.

---

## 6. Component Design
### A. Opportunity Custom Field (Total_Contract_Value__c)
- **Type:** Currency. Not required by default.
- **Partition Key:** Standard Salesforce Opportunity; no custom partition needed.
- **Access:** FLS; available to approval workflows only.

### B. Approval Processes
#### 1. Finance_Approval_Process (NEW)
- **Trigger Condition:** Opportunity.Total_Contract_Value__c > 500000
- **Approver Assignment:**
  - ≤ $2M → Anita Rao (Finance Director)
  - > $2M → Ravi Krishnan (CFO)
- **SLA**:
  - 24h → Reminder to Anita Rao if pending
  - 48h → Escalate to Ravi Krishnan
- **Action:** On Approval, Opportunity.StageName → 'Proposal Approved'. On rejection, notify owner w/ comment.
- **Audit:** ProcessInstanceStep + custom fields

#### 2. Deal_Desk_Approval_Process (NEW)
- **Trigger Condition:** Opportunity.Discount > 10
- **Approver:** Deal Desk Manager
- **SLA:** 12h remind; 24h escalate
- **Action:** Approval updates StageName; rejection returns w/ comments
- **Audit:** ProcessInstanceStep records

### C. Notification Integration
#### 1. Email Notifications
- **Service:** Salesforce Visualforce template
- **Recipients:** Finance approvers
- **Trigger:** On approval request submission
- **Delivery:** Must log attempts/failures

#### 2. Teams Notification
- **Service:** Custom Apex class `TeamsNotificationService`
- **Security:** `with sharing`, `SECURITY_ENFORCED`, batching for scale
- **Bulk DML:** Batch Opportunities needing notification
- **External API:** MS Teams webhook, retry/failure log
- **Logic:** Construct context-aware payload for Opportunity

---

### D. Rejection Comments Aggregator
- **Service:** Record-triggered update on ProcessInstanceStep (on rejection)
- **Logic:** Aggregates comments from Finance + Deal Desk and pushes to Opportunity.Description
- **Audit:** Tracks which comments were sourced/combined

### E. Restart Approval Flow
- **Trigger:** Record-triggered flow (before save)
- **Entry Condition:** ISCHANGED(StageName) && StageName == 'Re-Submitted'
- **Steps:**
   1. Detect prior approvals
   2. Reset statuses
   3. Sequentially relaunch both tracks
- **Audit:** Track restart reason/time; reset actions

### F. Auto-Approval Updater
- **Trigger:** Entry Criteria: Opportunity.Type == 'Renewal' && Discount == 0
- **Action:** Set StageName = 'Approved' via flow
- **Audit:** Log auto-approval actions

### G. Permission and Security Controls
- All flows/classes are built with `with sharing` and `SECURITY_ENFORCED`
- Enforce FLS at every data access point
- Apex must never perform DML in loops; batched logic for all API calls
- Audit changes via ProcessInstanceStep and Opportunity history
- No direct user access to approval metadata

---

## 7. Non-Functional Requirements (Explicit Values)
- **Latency (Process trigger):** <2s (p99)
- **SLA Adherence:** >99.9% approvals escalated/notified on time
- **Notification Delivery:** Email and Teams notification >99% delivered/attempted within 10s
- **Availability:** 99.95% for approval process functions
- **Audit Coverage:** 100% of approval, rejection, and escalation actions tracked
- **Security Compliance:** 100% FLS, org-wide security policies enforced

---

## 8. Alternatives and Trade-Offs
- **Single-Track vs Parallel Approval:**
  - *Chosen*: Parallel, to reduce bottlenecks. *Trade-off*: slightly greater process complexity, but dramatically improved speed and transparency.
  - *Alternative*: Sequential approval (as per older notes). Would slow velocity and reduce compliance with emerging business needs.
- **Teams (MS) vs Slack Notification:**
  - *Chosen*: MS Teams, matching recipient team usage. *Trade-off*: native Slack integration is faster/more robust but not aligned with Finance/approver group tools.
- **Dynamic Approver Lookup vs Hardcoded:**
  - *Chosen*: Dynamic, to ensure maintainability and compliance with org structure changes. *Trade-off*: Slight increase in metadata/config effort vs hardcoding.
- **Apex Batch + Flow Orchestrations vs Declarative Workflow Only:**
  - *Chosen*: Apex + Flow for batch/retry, scale, error handling. *Trade-off*: Increased codebase size and CI/CD complexity.

---

## 9. Deployment and Rollback
- **Order:** Ensure custom field (Total_Contract_Value__c) is deployed before approvals, then email/Teams notification, comment aggregation, and finally restart/auto-approval flows.
- **Rollback:** Rollback by reverting metadata to previous state, remove or disable triggering flows/classes, and delete related approval/rejection records.
- **CI/CD:** Use Salesforce DX or similar, target <15 min deploy, test in full/partial sandbox first
- **Permission Assignment:** After deploy, assign custom permissions for new fields/approval logic to users via permission sets.
- **Verification:** Smoke test each approval trigger and notification path; check audit log completeness

## 10. Observability and Error Handling
- **Logging:** All notification failures and escalation actions are logged in custom object or Salesforce error log.
- **Retry:** Any notification (email/Teams) failure is retried up to 3 times; on final failure, log and alert admin via Chatter.
- **SLA/Timer Functions:** Scheduled job invoked hourly to monitor approval pendency and fire SLA escalations.
- **Alerting:** If approval process fails (code/flow error), Chatter alerting to IT Admin group within 2 min.

## 11. Rationale & Final Notes
This design directly addresses business and compliance needs by:
- Enforcing dual-track, value-based approval for high-value and high-discount deals
- Providing actionable escalations, cut-and-dry audit trail, and predictable customer experience
- Using robust (with sharing, SECURITY_ENFORCED, no DML in loops) Apex patterns
- Maximizing reliability via parallel track, audit, retries, clear alerting/rollback

---

# End of TSD
