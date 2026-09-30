# Technical Specification Document (TSD)

## Project: Discount Approval Enhancement

**Module:** Opportunity Management  
**Platform:** Salesforce  
**Surface:** Internal (Apex / LWC / Flow)

---

## 1. Solution Overview

This document specifies the technical blueprint for enhancing the Salesforce Opportunity discount approval process with a second-tier approval for discounts exceeding 25%. The enhancement introduces sequential approvals (Deal Desk → VP Sales), automated SLA enforcement, courtesy notifications to Finance, auditability, escalation, and a process-reset mechanism on rejection.

## 2. Architecture Context & Scope

### 2.1 Context
- **Current Process:** Single-tier approval for discounts >10% and ≤25% handled by Deal Desk.
- **Required Enhancement:** Discounts >25% require sequential Deal Desk and VP Sales approvals, adding governance and audit trails for high-impact transactions.

### 2.2 Scope
- Trigger and manage second-tier approval workflow
- Enforce 24h/48h SLA timelines and handle auto-escalation
- Notify Finance for second-tier approvals
- Audit all approval/rejection steps and decision rationales
- Seamlessly integrate with both desktop and mobile Lightning experiences

---

## 3. Functional Architecture & Design Decisions

### 3.1 Process Flow (High-Level)
- When an Opportunity has `Discount__c` > 25% and requires approval:
  1. Initiate first-tier Deal Desk approval using a Salesforce Approval Process (Apex/Flow)
  2. Upon approval, automatically trigger second-tier approval assignment to VP Sales
  3. If no VP Sales action in 48h, auto-escalate to Executive Assistant
  4. On approval, Opportunity proceeds. On rejection at any step, the chain restarts from Deal Desk after Deal Owner resubmits
  5. Notify Finance via Salesforce event/email when second-tier approval is triggered; SLA breaches also notify relevant parties
  6. All actions and comments are logged in Opportunity/ProcessInstance records

#### Rationale & Trade-offs
- *Leverage native Salesforce Approval Process:* Ensures robust auditability and simpler maintenance, at the expense of limited UX customization compared to custom-built flows.
- *Sequential (not parallel) approval:* Compliance and accountability are prioritized over speed for high-value transactions.
- *SLA enforcement and escalation via scheduled Apex/Flow logic:* Ensures timely governance but introduces reliance on scheduled jobs.

### 3.2 Detailed Approval Flow Diagram

![Approval Process Sequence](https://www.plantuml.com/plantuml/png/XP1DIi8m48NtESNNegchWqDTVayN3vTSQSmrI4vUoByTsO9AIuG7Ahd7PBmU2SGEEhXLWrkLn45ufnd4k9XYEoBzeQjCVqRQ4mXJI_l5EjTrpAck9JcUOYwpNFut7jq3iwC9Mi5vvAH7NJUVXiyDj6Om_wDtYNvZ8cg80Eqpcv1BWkPKVRPb8akt4_amXAZDvSElhtC8XRE2_Mol2KtrzGayGlTLt6o9h4IjONm00)

---

## 4. Data Model & Integration

### 4.1 Object Model Extensions
- **Opportunity:**
  - Add `Second_Tier_Approval_Triggered__c` (Checkbox)
  - Add `Second_Tier_Approval_Comments__c` (Long Text Area)
- **ProcessInstance** (standard): used for approval workflow logging
- **ProcessInstanceStep** (standard): tracks each approval/rejection/comment
- **AppvlWorkItemNtfcnEvent**: used for notification/event triggers

#### Data Partitioning & Visibility
- Maintain field-level security: restricts approval and comment fields to relevant roles only (Deal Desk, VP Sales, Finance for read-only)
- Ensure new fields are available on all relevant Opportunity record pages (LWC/Flow updates)

---

## 5. Notification, Escalation & SLA Enforcement

### 5.1 Notifications
- **Trigger:** When a discount >25% Opportunity enters approval or escalates, trigger Salesforce native email/app notification to Finance.
- **Implementation:** Use existing `AppvlWorkItemNtfcnEvent` (Apex/Process Builder/Flow as appropriate)
- **Finance:** FYI only, no action link in notification

### 5.2 Escalation & SLA
- Enforce SLA via Scheduled Apex or Lightning Flow on `ProcessInstanceStep`:
  - 24h (Deal Desk): If breached, follow current process; no change
  - 48h (VP Sales): If breached, auto-escalate approval to Executive Assistant (backup approver)
- Scheduled job audits open approvals every 15 minutes for compliance
- VP Sales/Exec Assistant users and delegates defined in metadata/custom settings

---

## 6. Audit, Compliance, Comments & Logging
- Every decision and comment is appended to the Opportunity/ProcessInstanceStep for complete traceability
- On rejection by any tier, all comments (Deal Desk, VP Sales) are aggregated and displayed to Deal Owner
- Audit fields and approval logs are reportable and accessible to compliance teams via standard Salesforce reporting

---

## 7. Security
- Leverage Salesforce's existing Role-Based Access Controls (RBAC) to restrict who can initiate approvals, approve at each tier, or view comments
- Ensure approval and notification logic honors org hierarchy, profile assignment, and field level security (FLS)
- Store no sensitive data outside Salesforce (all logs/comments remain in core objects)
- Use Salesforce Managed Identities for automated jobs and notifications

---

## 8. Non-Functional Requirements (NFRs)
| Requirement                | Value                                  |
|----------------------------|----------------------------------------|
| Approver SLA (Deal Desk)   | <24h per request                       |
| Approver SLA (VP Sales)    | <48h per request                       |
| Notification Latency       | <2 minutes for all Finance notifications|
| Audit Log Retention        | ≥7 years                               |
| System Availability        | ≥99.9% (Salesforce managed)            |
| Opportunity Edit Latency   | <1 sec for save/update at p95          |
| Approver Throughput        | 500 concurrent approval chains         |
| Compliance Reporting       | All logs/actions exportable in <5 min   |
| Security                   | FLS, RBAC, and audit logs for every access |

---

## 9. Implementation Technology
- Salesforce Approval Processes (visual configuration + Apex triggers/Flow orchestrations)
- Apex classes for SLA enforcement and scheduled escalation
- Lightning Flow for notifications and error handling
- LWC for user interface & comments collation on rejection
- Process Builder or equivalent for event triggers
- Immutable audit logs via ProcessInstanceStep

---

## 10. Alternatives & Trade-offs
- **Custom Approval Engine (not chosen):** Increased flexibility, but higher dev/maint cost, riskier for audit/compliance.
- **Fully manual notification:** Lower automation, unacceptable for timely compliance and SLA monitoring.
- **Third-Party Integrations:** Not selected to minimize security, privacy, and maintenance overhead; all features possible natively in Salesforce.

---

## 11. Deployment & Rollout
- Sandbox dev and UAT cycles with test data (including edge cases: no Delegate, multiple rejections, failed notifications)
- User communication: process documentation, training before go-live
- Salesforce admin and InfoSec approvals prior to production deploy
- Post-launch audit with Finance and Sales leadership feedback loop

---

## 12. Appendices
- See BRD and referenced documentation for complete field, role, and notification details
- [Meeting_Note_01_Discount_Depth 2.md]
- Change log, detailed test cases, and data migration plan provided elsewhere for release
