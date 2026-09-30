# Business Requirements Document (BRD)

## Project Title: Salesforce Opportunity Discount Approval Enhancement

### Version: 1.0
### Date: 30 September 2026

---

## Executive Summary

This Business Requirements Document specifies the introduction of a second-tier approval process for Opportunity discounts exceeding 25% in Salesforce. The proposed enhancement addresses governance gaps identified in the Q1 FY26 audit, which highlighted significant revenue giveaways due to lack of executive oversight on deep-discount deals. The solution establishes a sequential, multi-tiered approval workflow involving Deal Desk and VP Sales, with Finance notification and robust SLAs. The document also outlines risk mitigation, notification handling, and the full chain restart mechanism to improve auditability and compliance.

---

## Business Objectives
- Close audit-identified governance gaps for high-discount Opportunities.
- Ensure executive visibility and sign-off for any Opportunity discount exceeding 25%.
- Preserve efficiency for standard deals by maintaining the existing single-tier approval for 10–25% discounts.
- Improve audit trail and compliance integrity, especially on rejected and re-submitted Opportunities.
- Enable Finance visibility into deep-discount deal volume through informational notifications.

---

## Scope
### In Scope
- Salesforce Opportunity approval workflow changes for discount approvals above 25%.
- Sequential routing: Deal Desk approval first, then VP Sales approval for high-discount Opportunities.
- Automated notifications: Email and Slack for VP Sales; auto-escalation to Executive Assistant on SLA breach.
- Informational notification to Finance when second-tier approval triggers.
- SLAs for both approval tiers.
- Rejection handling with full chain restart.

### Out of Scope
- Changes to approvals for discounts <=10% or 10–25% (remains single-tier, Deal Desk only).
- Deal Desk and Finance process changes outside Opportunity discount approvals.
- UI changes unrelated to the approval workflow.
---

## Functional Requirements

| Requirement ID | Description |
|----------------|-------------|
| BR-01 | The system MUST trigger the existing Deal Desk approval workflow for any Opportunity with a discount >10%. |
| BR-02 | For Opportunities where discount exceeds 25%, the system MUST initiate a sequential, two-tier approval process: first Deal Desk (Vikram Patel), then VP Sales (Suresh Iyer). |
| BR-03 | VP Sales MUST NOT receive or review an Opportunity until Deal Desk has approved/validated the request. |
| BR-04 | The approval workflow MUST route the Opportunity to VP Sales for secondary approval only if the discount threshold of >25% is met, post Deal Desk approval. |
| BR-05 | Upon triggering of the second-tier (discount >25%), the system MUST send an informational notification to Finance Director (Anita Rao) via email. |
| BR-06 | The system MUST notify VP Sales via both email and Slack for requests requiring second-tier approval. |
| BR-07 | If VP Sales does not act within 48 hours, the system MUST auto-escalate the approval request to VP Sales' Executive Assistant (Sunita Rao). |
| BR-08 | Deal Desk tier must retain the current 24-hour SLA. |
| BR-09 | If VP Sales rejects the Opportunity, the system MUST return the Opportunity to the Deal Owner with comments from both Deal Desk and VP Sales, requiring a full chain restart beginning with Deal Desk on resubmission. |
| BR-10 | Partial restarts or shortcutting of the approval chain after rejection MUST NOT be permitted. |
| BR-11 | All process flows, notifications, and comment histories MUST be logged for audit purposes. |

---

## Non-Functional Requirements

| Requirement ID | Description |
|----------------|-------------|
| NFR-01 | The system MUST guarantee that approval SLAs (24h for Deal Desk, 48h for VP Sales) are monitored and enforced. |
| NFR-02 | The system MUST support integration with Slack for actionable notifications. |
| NFR-03 | The system MUST be configurable to allow changes to the threshold percentages, approver names, notification recipients, and channels by System Admin. |
| NFR-04 | The workflow solution MUST not degrade Opportunity object performance for users. |
| NFR-05 | All approval actions and notifications MUST be traceable in Salesforce for audit review. |

---

## Assumptions
- VP Sales (Suresh Iyer) has agreed to serve as second-tier approver; Executive Assistant (Sunita Rao) is available for escalation.
- Finance Director will not participate as an approver but only receives notification.
- Salesforce environment supports automated notifications and escalation workflows (Email, Slack).
- Opportunity object is the only scope of this approval change.

---

## Dependencies
- Availability of current Salesforce approval process automation.
- Timely provision of historical approval data for process validation (Vikram to provide six months’ data).
- Confirmation and onboarding of VP Sales and Executive Assistant for notification mechanisms.
- Karan Verma (Salesforce Admin) participation for technical review and deployment.

---

## Risks
- Risk of delayed deal closure if SLAs are missed and auto-escalation fails; mitigated via redundant channels.
- Potential for circumvention via incorrect resubmission handling; full chain restart mandated.
- Audit gaming risk if resubmissions are not fully validated through both tiers.
- Failure to notify Finance could impact revenue reporting accuracy at period end.

---

## References
- Meeting_Note_01_Discount_Depth 2.md (Discovery meeting 20 June 2026) [1]
- Q1 FY26 audit findings (Sales Ops)

---

[1]: Meeting_Note_01_Discount_Depth 2.md, 20 June 2026
