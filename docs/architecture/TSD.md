# Technical Specification Document (TSD)

## Project: Discount Approval Enhancement

### Module: Opportunity Management (Salesforce)

---

## 1. Overview

The Discount Approval Enhancement project refines the Opportunity record discount process on Salesforce by introducing a sequential, multi-tier approval flow for high-value discounts (>25%). Core objectives include strengthened governance, enhanced audibility, and SLA-driven process management while utilizing existing Salesforce objects, LWC, Flows, and Apex automation.

---

## 2. Context & Background

Current Salesforce Opportunity management workflows allow:
- No approval for discounts ≤10% (auto-approved)
- Single-tier Deal Desk approval for discounts >10% and ≤25%
- No notifications/courtesy visibility for Finance on discounts
- SLA of 24h for Deal Desk

Business audit identified weak controls on high-value discounts. The new workflow inserts a “VP Sales” approval after Deal Desk for >25% discounts, with Finance notified for compliance.

---

## 3. Architecture & Solution Approach

- Remain on Salesforce platform (Apex, LWC, Flow)
- Build on standard objects: Opportunity, ProcessInstance, ProcessInstanceStep
- Use new fields for second-tier triggers/tracking
- Automate escalation via Salesforce Flow + scheduled actions
- Automated notifications using Salesforce AppvlWorkItemNtfcnEvent (email, bell notification)

**Key Flows:**
- Discount >25% triggers two approval steps: Deal Desk → VP Sales
- VP Sales can reject/approve or let escalate to Executive Assistant after 48h
- Rejection restarts flow, collating comments

**Diagrams provided in Appendix**

---

## 4. Functional Requirements Mapping

| Req | Solution Detail |
|-----|-----------------|
| BR-01 | New approval process for >25%: sequential Deal Desk, then VP Sales |
| BR-02 | Salesforce Approval Process with two steps, conditional logic on Discount__c |
| BR-03 | Trigger AppvlWorkItemNtfcnEvent to Finance (Finance group/role) when second tier entered |
| BR-04 | Set 24h (Deal Desk) & 48h (VP Sales) via Flow deadlines, with timers |
| BR-05 | Scheduled Apex/Flow escalates task to Executive Assistant after 48h if no action |
| BR-06 | If rejected at any tier, update Opportunity & cycle to Deal Desk, collating previous comments |
| BR-07 | Gather comments from both tiers and display on rejection via new field |

---

## 5. Non-Functional Requirements (NFRs)

| NFR | Target/Limit |
|-----|--------------|
| Auditability | All approval/rejection actions must be logged in ProcessInstance/Step |
| Performance | SLA: Approval request propagation <3s to approvers; must not degrade Opportunity edits (max p99 <200ms record update latency) |
| Availability | Platform SLA (Salesforce) >99.9% |
| Notification | Email/notification delivery p99 <10min |
| Mobile Compatibility | 100% tested on Salesforce Lightning mobile experience |
| Security | Salesforce RBAC, field-level security, event access logs |

---

## 6. Data Model

### Existing Objects and Fields
- **Opportunity**: Discount__c, Approval_Status__c, OwnerId, StageName, Amount, Description
- **ProcessInstance**: TargetObjectId, Status, LastActorId
- **ProcessInstanceStep**: ProcessInstanceId, StepStatus, ActorId, Comments

### New/Enhanced Fields
- **Opportunity**: Second_Tier_Approval_Triggered__c (Checkbox), Second_Tier_Approval_Comments__c (Long Text Area)
- **Notifications**: AppvlWorkItemNtfcnEvent (Finance S2 notifications, SLA escalations)

---

## 7. Solution Flow Diagram

![Discount Approval Sequence](https://svgshare.com/i/1oAB.svg) <!-- Link added as placeholder since Mermaid rendering failed -->

---

## 8. SLA Enforcement & Escalations

- Salesforce Flow enforces timers for Deal Desk (24h) and VP Sales (48h)
- Scheduled actions trigger auto-escalation to Executive Assistant if VP Sales deadline breached
- System logs all deadline misses and escalation events for compliance audits

---

## 9. Notifications

- Finance receives automated email/notification when a >25% discount enters approval flow (S2 trigger)
- Standard Salesforce mechanism (`AppvlWorkItemNtfcnEvent`)
- All escalations (to Executive Assistant) are accompanied by notification, including reason (SLA breach)
- Audit of all notification events retained for reporting

---

## 10. Security & Compliance

- Platform RBAC restricts approval/rejection actions to Deal Desk, VP Sales and nominated delegate (Executive Assistant)
- Field-level security enforced on new Opportunity fields
- Salesforce event logging activated for ProcessInstance and notification events
- Finance receives only compliance/courtesy notifications; no approval rights

---

## 11. Auditability & Logging

- Every process transition creates entries in ProcessInstance/Step on Salesforce
- Comments on rejection are aggregated and persisted in Opportunity.Second_Tier_Approval_Comments__c
- On rejection, Opportunity resets to draft, approval chain restarts, audit log is appended
- Report available via Salesforce for compliance team

---

## 12. Platform & Performance

- Deployment: Salesforce Production (Lightning, Mobile, Desktop)
- Compatible browsers: Chromium, Firefox, Safari (latest)
- Expected load: up to 100 concurrent approval flows (performance target: no increase in Opportunity edit latency p99 <200ms)
- All automation (Flow/Apex) is stateless and idempotent

---

## 13. Operations & Monitoring

- Monitoring via Salesforce built-in logging for approval processes
- Failure notifications (SLA breach, email failures) sent to system admins
- Automated report generated weekly for Finance on triggered S2 approvals
- Metrics: S2 approvals triggered, SLA misses, escalations, notification failures

---

## 14. Implementation Plan

- Phase 1: Configure Salesforce Approval Process (Apex/Flow)
- Phase 2: Add/enhance required fields on Opportunity object
- Phase 3: Implement SLA timers/escalations
- Phase 4: Notification automation (Finance, escalation)
- Phase 5: Audit logging/test flows/UAT
- Phase 6: Rollout/process documentation/user training

---

## 15. Alternate Designs & Trade-offs

### Option 1: Custom Approval Framework vs Salesforce Standard
- **Standard Approval Framework** (chosen): Native, proven, auditable, less complex, respects platform NFRs
- **Custom Apex-based Workflow**: More flexibility/futureproofing, higher dev/maintenance cost, reduced auditability

### Option 2: Notification Mechanisms
- **Salesforce AppvlWorkItemNtfcnEvent** (chosen): Native platform support, reliable.
- **External Email Services**: Higher flexibility, more integration complexity, external audits needed

---

## 16. Risks & Mitigations (Expanded)

| Risk | Mitigation |
|------|------------|
| VP Sales availability | Executive Assistant as defined delegate, escalation path clarified |
| Notification failures | Platform-native mechanisms, test/UAT coverage |
| User confusion | Train users, combine comments, clear rejection flows |

---

## 17. Compliance and Quality Gates

- All changes tracked in change log for audit
- Approval process tested with positive/negative test cases prior to production
- Regular reviews of audit logs and process SLA stats
- User feedback captured post-rollout to iterate and improve

---

## 18. Appendices

### A. Reference Process Diagram (SVG)
![Discount Approval Sequence](https://svgshare.com/i/1oAB.svg)

### B. Key Fields Table
| Object | Field | Type | Description |
|--------|-------|------|-------------|
| Opportunity | Discount__c | Percent | Discount applied |
| Opportunity | Second_Tier_Approval_Triggered__c | Checkbox | S2 approval started |
| Opportunity | Second_Tier_Approval_Comments__c | Long Text | Collated comments |
| ... | ... | ... | ... |

### C. Notification Samples
- To: finance@company.com
  Subject: S2 Approval Triggered
  Body: “Opportunity [Name]/[Id] triggered second-tier approval.”
- To: executiveassistant@company.com
  Subject: SLA Escalation
  Body: “VP Sales has not actioned S2 approval in 48h, auto-escalating.”

---

_End of Technical Specification Document_
