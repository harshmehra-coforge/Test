# Technical Specification Document (TSD)

## Project: Discount Approval Enhancement

### Context & Overview
This document specifies the technical design for enhancing the Opportunity discount approval workflow in Salesforce. The enhancement introduces a second-tier sequential approval for discounts exceeding 25%, retaining the existing single-tier process for smaller discounts. The workflow is to be implemented using Apex, Lightning Web Components (LWC), and Flow automation, leveraging native Salesforce record pages.

---

## Functional Requirements Mapping
- Sequential second-tier approval: Deal Desk → VP Sales (>25% discounts).
- Automated SLA logic (24h Deal Desk, 48h VP Sales). Auto-escalation handled via Flow.
- Finance receives courtesy notifications when second-tier triggered.
- Approval chain restarts from Deal Desk on any rejection, preserving combined comments.
- Approval actions must be logged for audit/compliance.
- Solution must use native Salesforce notification/event mechanisms.

---

## Component Architecture

### Summary Table
| Component                      | Technology         | Purpose                                     |
|------------------------------- |------------------  |---------------------------------------------|
| Approval Flow Controller       | Apex/Flow          | Manages sequential routing, tier logic       |
| Discount Audit Logger          | Apex/Flow          | Logs all approval/rejection for compliance   |
| Notification Dispatcher        | Apex/Flow          | Email, App Notification to Finance/escalated |
| Approval Comments Aggregator   | LWC                | Displays combined comments on rejection      |
| SLA Escalation Engine          | Flow/Apex          | Timer-based auto-escalation                  |
| Opportunity Object Extensions  | Salesforce Objects | Enhanced fields for second-tier tracking     |

---

### Workflow Diagram (Mermaid)

## Workflow Diagram

![Diagram](https://mermaid.ink/svg/eyJjb2RlIjoiU2VxZW5jZURpYWdyYW0KICAgIHBhcnRpY2lwYW50IERPIHN0ZWFtIERlYWwgT3duZXIKICAgIHBhcnRpY2lwYW50IEREIHN0ZWFtIERlYWwgRGVzawogICAgcGFydGljaXBhbnQgVlAgc3RyZWFtIFZQIFNhbGVzCiAgICBwYXJ0aWNpcGFudCBFQSBzdHJlYW0gRXhlY3V0aXZlIEFzc2lzdGFudAogICAgcGFydGljaXBhbnQgRk4gc3RyZWFtIEZpbmFuY2UKCiAgICBETy0-PkREOiBTdWJtaXQgT3Bwb3J0dW5pdHkgKERpc2NvdW50ICgyNSUpKQogICAgREQtPj5WUDogQXBwcm92ZSAod2l0aGluIDI0aCkKICAgIFZQLT4+Rk46IFRyaWdnZXIgRmluYW5jZSBOb3RpZmljYXRpb24KICAgIFZQLT4+RUE6IEVzY2FsYXRlIGlmID48NDhoCiAgICBWUC0-PkRPOiBBcHByb3ZlL1JlamVjdAogICAgTm90ZSBvdmVyIERPLERELFZQOiBJZiByZWplY3RlZCwgcmVzdGFydCBhcHByb3ZhbCBmbG93OyBzaG93IGNvbW1lbnRzIiwgZmFjdW5rZWQ6dHJ1ZX0)

---

## Data Model

### Salesforce Opportunity Enhancements
- **Opportunity**
  - `Discount__c`: Numeric, used for tier determination
  - `Second_Tier_Approval_Triggered__c`: Checkbox, set when >25% discount
  - `Second_Tier_Approval_Comments__c`: Long Text Area, stores aggregated comments
  - `Approval_Status__c`: Enum (Pending, Approved, Rejected)

### Process Tracking
- **ProcessInstance**: Tracks approval process instance
  - `TargetObjectId`, `Status`, `LastActorId`
- **ProcessInstanceStep**: Steps within the approval
  - `StepStatus`, `ActorId`, `Comments`

### Notification Event
- **AppvlWorkItemNtfcnEvent**: Used for both Finance notification and SLA escalation

---

## Key Use Cases & Flows

### 1. High Discount Submission (>25%)
- Opportunity owner submits record
- Approval routed to Deal Desk (24h SLA)
- If approved, routed to VP Sales (48h SLA)
- On VP Sales approval, Finance notified (FYI)
- If VP Sales does not act within 48h, auto-escalate to Executive Assistant
- On rejection, full chain restarts from Deal Desk, comments concatenated/displayed to Deal Owner

### 2. Lower Discount (<10%, 10–25%)
  - <10%: No approval (existing)
  - 10–25%: Deal Desk only (existing)

---

## SLA Enforcement & Escalation Logic

- SLA timers implemented via Flow and Apex: 24h for Deal Desk, 48h for VP Sales
- Escalation event triggers notification to Executive Assistant if VP Sales action exceeds SLA
- Audit trail preserved in ProcessInstance/ProcessInstanceStep

## Notification Mechanism

- Finance notified via AppvlWorkItemNtfcnEvent on high-discount approval initiation
- Salesforce email/app notifications for all escalations
- Notification content includes Opportunity details and context

---

## Auditability & Compliance

- Approval/rejection actions logged with actor, timestamp, comments
- Combined rejection comments stored in `Second_Tier_Approval_Comments__c`
- Opportunity record includes audit fields, visible to admin users
- System must not add >50ms average latency per Opportunity record load

---

## Security & Access Control

- Approvers (Deal Desk, VP Sales, Executive Assistant) must be assigned via Salesforce Roles
- RBAC enforced at object and field level (reviewed by admin pre-deployment)
- Zero Trust principles: only required users can action/approve
- Notification events do not expose sensitive deal data to Finance beyond context required for compliance

---

## Mobile & Lightning Compatibility

- LWC components rendered on both mobile and desktop Salesforce Lightning interfaces
- Responsive UI tested for approval comments aggregator
- Use standard Salesforce UI patterns for record mutation

## Performance, Reliability & NFRs

- Latency: Approval flow adds ≤50ms p99 to record handling
- Throughput: Support up to 5,000 approval chains/month
- Availability: 99.99% (leverages Salesforce native uptime)
- Cost: No additional license cost; leverages existing Salesforce platform

---

## Alternatives & Rationale

### Option 1: Custom Apex, LWC, Flow (Selected)
- Pros: Tight Salesforce integration, leverages native workflow
- Cons: Requires admin/config effort

### Option 2: Third-party Approval App
- Pros: Fast setup
- Cons: Less control, extra cost, integration risk

### Option 3: Manual Email/Spreadsheet
- Pros: Simple
- Cons: No auditability, error-prone, not scalable

Selected Option provides best governance, auditability, SLA enforcement, and native integration.

---

## Deployment & Rollout

- Configuration and deployment via Salesforce admin tools
- Flow/Apex/LWC tested in sandbox, UAT required before go-live
- All new fields and Flow logic documented for support
- Process documentation delivered to end users pre-rollout
- Training for Deal Desk, VP Sales, and Finance on process changes

---

## Risks & Mitigations

| Risk                                     | Mitigation                                               |
|------------------------------------------|----------------------------------------------------------|
| VP Sales unresponsive/delay               | Delegate auto-escalation, document process, backup EA    |
| Notification failure                     | Use standard Salesforce notifications, UAT testing       |
| User confusion on restart                 | Bundle comments, training pre-rollout                    |

---

## Appendices

**References**:
- BRD (Discount Approval Enhancement)
- Salesforce Opportunity Management documentation
- Stakeholder meeting notes
- [1] Meeting_Note_01_Discount_Depth 2.md

---

