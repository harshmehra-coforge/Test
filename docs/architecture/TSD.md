# Technical Specification Document (TSD)

## 1. Overview
The Discount Approval Enhancement (Salesforce Opportunity Management) introduces a second-tier sequential approval workflow for large discounts (>25%)—first routed to Deal Desk, then to VP Sales. SLAs and escalations are enforced with audit logging and finance notifications for governance, compliance, and robust auditability. The solution leverages Salesforce native objects (Apex, LWC, Flow), integrates with existing approval processes, supports mobile/desktop, and automates all notifications.

## 2. Solution Context
- **Platform:** Salesforce (core Opportunity approvals, Apex triggers, LWC, Flows)
- **Scope:** Only Opportunity discounts above 25% require the new multi-level approval. All approvals and escalations must be auditable, actionable, and transparent.
- **Users:** Opportunity Owners, Deal Desk, VP Sales, VP Sales Assistant, Finance (notification), Salesforce Admins
- **Stakeholders:** Sales, Finance, Compliance, Executive Team

## 3. Requirements Traceability
### 3.1. Functional Requirements Mapping
| BRD ID | Requirement                                                                      | Solution Mapping                                    |
|--------|-----------------------------------------------------------------------------------|-----------------------------------------------------|
| BR-01  | Second-tier approval for >25% discounts                                          | Implement sequential workflow via Salesforce Flows   |
| BR-02  | Deal Desk → VP Sales routing                                                      | Flow logic to enforce order, with UI status tracking |
| BR-03  | Notify Finance on second-tier trigger                                             | Auto-email/in-app event via standard Salesforce obj  |
| BR-04  | SLA: 24h (Deal Desk), 48h (VP Sales)                                             | Automated timers & scheduled flows                  |
| BR-05  | Auto-escalate after 48h to VP Sales Assistant                                     | Escalation path logic driven by process & timer      |
| BR-06  | Restart approval chain on rejection                                               | Process restart trigger with history logging         |
| BR-07  | Capture/display combined comments on rejection                                    | Persist/aggregate comments; custom rejection field   |

### 3.2. Non-Functional Requirements
- Use existing Salesforce frameworks (Apex, Flow, LWC).
- Automated notifications for both Finance and escalation.
- Full audit trail of approvals, actions, and comments.
- Support Salesforce Lightning/mobile. No degradation to Opportunity creation/view speed (<500ms addition p99).

## 4. High-Level Architecture
![Discount Approval Process (Mermaid Diagram - see appendix for error)](https://fake-url.com/diagram-error.svg)

- **Discount levels:** ≤10% (no approval), 10%-25% (Deal Desk), >25% (Deal Desk → VP Sales)
- **Finance:** Notified on >25% only (courtesy, not actionable)
- **SLA Management:** Timed Flows and Apex scheduled jobs
- **Escalation:** 48h timeout triggers notification to VP Sales Assistant
- **Audit:** All approval/rejection actions/comments logged to ProcessInstance/Opportunity

## 5. Components
- **Salesforce Opportunity Object**: Core. Custom fields for second-tier flags/comments.
- **Approval Process Objects:** ProcessInstance, ProcessInstanceStep for process and step tracking.
- **Apex/Flow Logic:** For routing, escalation, enforcement, and event triggers.
- **LWC Component:** UX display of approval path/status/comments to deal owners.
- **Notifications:** System events and email notifications to Finance (triggered).
- **Scheduled Jobs:** SLA monitoring and escalation triggers.
- **Audit Layer:** All pathways log to Opportunity & related process tracking objects.

## 6. Solution Design Decisions
- **Framework:** Reuse Salesforce Approval Process plus Apex for logic not natively available (timers, escalations, advanced notification logic).
- **Second-tier notification:** Triggered automatically based on `Discount__c > 25%` logic in Apex/Flow.
- **SLA handling:** 24h/48h timers implemented via Scheduled Apex or Salesforce scheduled flows. SLA breach triggers escalate to Assistant.
- **Rejection restart:** Flows/Apex to roll Opportunity back to Deal Desk step, preserving all comments.
- **Comments aggregation:** Collect all comments from ProcessInstanceSteps; surface on rejection using LWC.

## 7. Data Model & Changes
### 7.1. Objects/Fields
| Object         | Field/Change                          | Type/Description                  |
|----------------|--------------------------------------|------------------------------------|
| Opportunity    | Discount__c                          | % Discount (existing)              |
|                | Approval_Status__c                   | Approval process state (existing)  |
|                | Second_Tier_Approval_Triggered__c    | Checkbox (new; >25%)               |
|                | Second_Tier_Approval_Comments__c     | Long Text Area (new)               |
| ProcessInstance| (native fields, reused)              | Approval process tracking         |
| ProcessInstanceStep| (native fields, reused)           | Step tracking/comments            |
| Notification   | AppvlWorkItemNtfcnEvent              | Event for Finance/SLA (existing)   |

### 7.2. Comments/Audit Trail
- Every decision (approve/reject) and all comments logged to ProcessInstanceStep.
- On any rejection, comments from both tiers are aggregated in the Opportunity.

## 8. Process Flow & Logic
### 8.1. Discount Threshold Routing
- If `Discount__c` ≤10%: No approval triggered.
- If `Discount__c` >10% and ≤25%: Single-tier; route to Deal Desk (existing logic, unchanged).
- If `Discount__c` >25%:
  1. Stage 1: Approval routed to Deal Desk.
  2. On approval, Stage 2: Routed to VP Sales.
  3. If VP Sales approves, close approval and notify Finance (auto-email/event).
  4. If VP Sales rejects, roll back to Opportunity Owner with all comments, require edit/re-submit (restarts from Deal Desk).
  5. SLA logic: 24h timer for Deal Desk, 48h for VP Sales; breach auto-escalates to VP Sales Assistant.

### 8.2. SLA & Escalation Automation
- SLA timers managed by Salesforce Scheduled Flows or Apex batch jobs.
- Notification logic triggers upon approaching/slipping SLA deadline.
- Escalated approvals are routed to `VP Sales Executive Assistant` if VP Sales acts too slowly (>48h).

### 8.3. Notification Engine
- Finance notified via Salesforce event/email when >25% discount approval starts and is completed.
- All escalations and rejections are automatically logged and reported to owner and all prior actors.

### 8.4. Comments & Auditability
- All ProcessInstanceSteps' comments are harvested on rejection and written to Opportunity.Second_Tier_Approval_Comments__c for user visibility.
- Audit logs kept for compliance—every rejection, decision, and notification is traceable.


## 9. Security Considerations
- **RBAC:** Only users in Deal Desk/VP Sales/Assistant roles can approve/reject at each relevant step (enforced by Apex/Flow).
- **Audit Trails:** Immutable log in ProcessInstance/Step ties changes to each user, timestamped.
- **Managed Credentials:** Notification and SLA flows use managed service accounts; no hardcoded emails.
- **Data Privacy:** Approval comments and outcomes visible only to actors on the Opportunity and Finance via notification, not globally visible.
- **Zero Trust:** Standard Salesforce permission sets and sharing rules, no public APIs.

## 10. Non-Functional Requirements (NFRs)
- **Performance:** No more than 500ms additional latency p99 on Opportunity record save/submit.
- **Availability:** Process must be available 99.99% during business hours (reliant on core Salesforce availability SLAs).
- **Auditability:** 100% of approval/rejection actions and comments logged and queryable for 7 years.
- **Scalability:** Supports up to 200 concurrent approvals per hour.
- **SLA Enforcement:** All escalations must be triggered within 5 minutes of SLA breach (measured by timer events logs).
- **Notification:** Finance must always be notified within 1 minute when a >25% discount process starts or finishes.
- **Cost:** Solution must only use bundled Salesforce features/credits (no external compute/storage dependency).
- **Compliance:** Meets internal audit and SOX requirements for change logging, segregation of duties, and traceability.

## 11. Alternatives Considered & Rationale

| Alternative                      | Description                                                | Pros                                                      | Cons                                                           | Rationale for Selection                                  |
|----------------------------------|------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------------------------|-----------------------------------------------------------|
| Custom Apex-Only Logic           | Purely custom Apex triggers & logic                        | Maximum flexibility, fine audit trail                      | Harder maintenance, risk of drift from platform upgrades       | Chose partial native (Flow + Apex) for maintainability   |
| Native Salesforce Approval Only  | Use only out-of-box Approval Process for 2nd tier          | Fully configuration-based, fastest deployment               | Inflexible for SLA/auto-escalation, comment aggregation tricky | Hybrid chosen to allow timed escalation and audit logging |
| External Workflow Engine         | Orchestrate approval via non-SFDC workflow tools (e.g., Mulesoft) | Decouples process, advanced analytics                     | Extra infra, integration & non-compliance risk                | Not considered—SFDC-only per NFRs, compliance            |

- The hybrid Salesforce Approval + Flows + Apex solution is optimal for SLA, audit, and comment handling while minimizing ongoing maintenance and staying within platform compliance.

## 12. Deployment & Operations
- All Flows/Apex/Fields deployed via Salesforce Change Sets.
- All existing Approvals and notifications backward compatible.
- Feature-flag deployment: Only active for `Discount__c > 25%` until full rollout.
- UAT with test users in each approval role; Finance included on notification tests.
- Monitoring of scheduled jobs and timer events.
- Training required for Sales, Deal Desk, and Finance stakeholders.
- Back-out plan: revert package/deactivate flow if issues.

## 13. Appendix
### 13.1. Object Schema
- **Opportunity**: Discount__c (decimal), Approval_Status__c (picklist), Second_Tier_Approval_Triggered__c (checkbox), Second_Tier_Approval_Comments__c (long text)
- **ProcessInstance/Step**: Native, used for tracking status and comments
- **Notifications**: AppvlWorkItemNtfcnEvent (event, reused)

### 13.2. Process Diagram (Textual Version)
  1. Owner enters/edits Opportunity. If Discount__c >25%:
     - Route to Deal Desk for approval (24h SLA).
     - On approve: escalate to VP Sales (48h SLA).
         - On approve, process closes (Finance notified).
         - On reject, comments returned to owner, must resubmit (restarts).
     - If VP Sales doesn’t act in 48h: auto-route to VP Sales Assistant.
     - At each step, Finance notified (FYI only).
  2. Entire approval chain restarts on any rejection.
  3. All approval/rejection actions and comments logged & surfaced.

### 13.3. Compliance Notes
- Auditable by Compliance at any stage, historic logs queryable from Opportunity.
- SOX traceability via immutable ProcessInstance, Opportunity field logs.
- All automation via platform native services for minimal external exposure.

### 13.4. Known Limitations
- Dependent on platform features for timer/escalation latency.
- SLA enforcement only as precise as Salesforce Scheduled Flow granularity.
