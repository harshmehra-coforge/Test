# Business Requirements Document

## 1. Document Control

| Field | Value |
|---|---|
| Document title | Discount Approval Enhancement — Second-Tier Executive Review |
| Source | Meeting Note — Discount Approval Enhancement (Discovery) |
| Meeting date | 20 June 2026 |
| Business sponsor | Priya Sharma, VP Sales Operations |
| Business analyst / author | Rajesh Menon, Salesforce BSA |
| Target BRD sign-off | 26 June 2026 |
| Target go-live | End of Q2 FY26 |
| Status | Draft for review |

## 2. Executive Summary

The current Salesforce Opportunity discount approval process routes New Business and Renewal Opportunities with discounts above 10% to Deal Desk as the sole approver. The process does not differentiate approval routing by discount depth and provides no executive review path.

The business requires a sequential second-tier approval process for deeper discounts. Deal Desk approval will remain the first step. When the discount exceeds 25%, the Opportunity will subsequently route to Suresh Iyer, VP Sales — Americas, for executive sign-off. Finance will receive an informational notification when this second tier is triggered; Finance is not an approver in this enhancement. The existing 10–25% approval band remains unchanged. If an approval is rejected, the Opportunity must return to the Deal Owner with combined comments and the entire approval chain must restart upon resubmission.

## 3. Business Context and Problem Statement

Deal Desk processes approximately 180–200 discount requests per month across all deal sizes and discount depths. The current process treats a 12% discount and a 45% discount equivalently from an approval-routing perspective.

The Q1 FY26 audit identified four Opportunities with discounts of 30% or higher that were approved by Deal Desk alone, without executive visibility. The combined revenue giveaway on those deals was approximately $1.2M against list price. The audit committee requested an executive review path for deeper discounts before the end of Q2 FY26.

## 4. Business Objectives

| Objective ID | Objective |
|---|---|
| BO-01 | Introduce executive review for Opportunity discounts exceeding 25%. |
| BO-02 | Preserve the existing Deal Desk approval process for discounts above 10% and maintain the current 10–25% approval behavior. |
| BO-03 | Ensure Deal Desk validates the commercial justification and quote build before executive review. |
| BO-04 | Provide Finance with visibility into deep-discount approval volume without making Finance an approval step. |
| BO-05 | Strengthen audit governance by requiring a complete approval-chain restart after rejection and resubmission. |
| BO-06 | Implement the enhancement by the agreed target go-live of end of Q2 FY26. |

## 5. Scope

### 5.1 In Scope

- Salesforce Opportunity approval routing for New Business and Renewal Opportunities.
- Existing Deal Desk approval for discount requests above 10%.
- Sequential VP Sales approval when the discount exceeds 25% and Deal Desk has approved.
- Routing to Suresh Iyer, VP Sales — Americas, as the VP Sales approver.
- A 24-hour SLA for the Deal Desk tier.
- A 48-hour SLA for the VP Sales tier.
- Email and Slack notifications for the VP Sales approval tier.
- Automatic escalation to Sunita Rao, VP Sales Executive Assistant, when the VP Sales SLA is breached without action.
- Courtesy notification to Finance when the second approval tier is triggered.
- Rejection handling, combined comments, resubmission, and full-chain restart.
- Preservation of existing behavior for the 10–25% discount band.
- Salesforce technical review by Karan Verma before sign-off circulation.

### 5.2 Out of Scope

- Replacing or redesigning the existing Deal Desk commercial validation process.
- Making Finance an approver for the deep-discount workflow.
- Changing the approval routing for discounts from 10% through 25%.
- Approval routing for Opportunity types other than New Business and Renewal, because no such requirement was provided.
- Changes to discount calculation or quote-build calculation logic.
- Any notification channel other than those explicitly stated in this document.
- Delegate coverage design for VP Sales; a delegate is to be nominated, but the delegation requirement was not defined in the meeting note.

## 6. Stakeholders and Roles

| Stakeholder / Role | Responsibility or interest |
|---|---|
| Priya Sharma, VP Sales Operations | Business sponsor and decision owner. |
| Deal Owner / Sales Representative | Submits and resubmits the Opportunity approval request and addresses rejection comments. |
| Deal Desk | Performs the first approval step, including commercial sanity check of discount justification and quote build. |
| Suresh Iyer, VP Sales — Americas | Performs executive approval for discounts exceeding 25% after Deal Desk approval. |
| Sunita Rao, VP Sales Executive Assistant | Receives automatic escalation when the VP Sales SLA is breached without action. |
| Finance | Receives informational visibility into second-tier approval triggers; does not approve under this enhancement. |
| Rajesh Menon, Salesforce BSA | Drafts the BRD and incorporates current Salesforce state. |
| Karan Verma, Salesforce Admin | Performs technical review before sign-off circulation. |
| Audit Committee | Governance stakeholder requesting executive review for deeper discounts. |

## 7. Business Rules

| Rule ID | Rule |
|---|---|
| BRULE-01 | The approval process applies to New Business and Renewal Opportunities. |
| BRULE-02 | A discount above 10% routes to Deal Desk for approval. |
| BRULE-03 | A discount from 10% through 25% remains subject to the existing single-tier Deal Desk approval process. |
| BRULE-04 | A discount exceeding 25% requires Deal Desk approval first and then VP Sales approval. |
| BRULE-05 | VP Sales must not receive the approval request until Deal Desk has approved it. |
| BRULE-06 | Finance receives a courtesy notification when the VP Sales tier is triggered and does not approve the Opportunity in this process. |
| BRULE-07 | A rejection at any approval tier returns the Opportunity to the Deal Owner with combined comments from the Deal Desk and VP Sales tracks that started. |
| BRULE-08 | Resubmission after rejection restarts the complete applicable approval chain from Deal Desk; no prior approval carries forward. |
| BRULE-09 | The VP Sales approval SLA is 48 hours. If no action occurs within that SLA, the request is automatically escalated to Sunita Rao. |
| BRULE-10 | The Deal Desk approval SLA remains 24 hours. |

## 8. Functional Requirements

### 8.1 Approval Eligibility and Routing

| Requirement ID | Requirement | Traceability |
|---|---|---|
| BR-01 | The system shall identify New Business and Renewal Opportunities with a discount above 10% as requiring Deal Desk approval. | BRULE-01, BRULE-02; Current process review |
| BR-02 | The system shall preserve the existing single-tier Deal Desk approval behavior for discounts from 10% through 25%. | BRULE-03; Decisions |
| BR-03 | The system shall identify an Opportunity with a discount exceeding 25% as requiring a second approval tier after Deal Desk approval. | BRULE-04; Proposed second-tier design |
| BR-04 | The system shall route the first approval tier for an eligible Opportunity to Deal Desk. | BRULE-02, BRULE-04; Proposed second-tier design |
| BR-05 | The system shall route an Opportunity exceeding 25% to Suresh Iyer, VP Sales — Americas, only after Deal Desk approves. | BRULE-04, BRULE-05; Decisions |
| BR-06 | The system shall prevent the VP Sales approval request from being presented before Deal Desk approval is completed. | BRULE-05; Proposed second-tier design |

### 8.2 Approval Review

| Requirement ID | Requirement | Traceability |
|---|---|---|
| BR-07 | The Deal Desk approval step shall support review of the discount justification and quote build for commercial sanity. | Proposed second-tier design |
| BR-08 | The VP Sales approval step shall support executive sign-off for an Opportunity with a discount exceeding 25%. | BRULE-04; Decisions |
| BR-09 | The system shall retain the existing Deal Desk approval step and shall not alter its stated 24-hour SLA. | BRULE-10; SLA and notifications |
| BR-10 | The system shall apply a 48-hour SLA to the VP Sales approval step. | BRULE-09; SLA and notifications |

### 8.3 Notifications and Escalation

| Requirement ID | Requirement | Traceability |
|---|---|---|
| BR-11 | When the VP Sales tier is triggered, the system shall send the VP Sales approval notification by email and Slack. | SLA and notifications |
| BR-12 | When the VP Sales tier is triggered, the system shall send Finance a courtesy notification for informational visibility. | BRULE-06; Decisions |
| BR-13 | The Finance notification shall not create an approval task or approval decision requirement for Finance. | BRULE-06; Proposed second-tier design |
| BR-14 | If the VP Sales approval remains without action after 48 hours, the system shall automatically escalate the request to Sunita Rao, Executive Assistant to VP Sales. | BRULE-09; SLA and notifications |
| BR-15 | The system shall preserve the existing notification behavior for the 10–25% approval band unless a separate approved requirement changes it. | BRULE-03; Decisions |

### 8.4 Rejection and Resubmission

| Requirement ID | Requirement | Traceability |
|---|---|---|
| BR-16 | If Deal Desk rejects an approval request, the system shall return the Opportunity to the Deal Owner with the rejection comments. | Rejection handling |
| BR-17 | If VP Sales rejects an approval request, the system shall return the Opportunity to the Deal Owner with combined comments from Deal Desk and VP Sales where both tracks have started. | BRULE-07; Rejection handling |
| BR-18 | The system shall require the Deal Owner to edit and resubmit an Opportunity rejected at any approval tier. | Rejection handling |
| BR-19 | On resubmission after rejection, the system shall restart the complete applicable approval chain from Deal Desk. | BRULE-08; Decisions |
| BR-20 | The system shall not carry forward or reuse a prior approval decision after resubmission. | BRULE-08; Rejection handling |

### 8.5 Audit and Governance

| Requirement ID | Requirement | Traceability |
|---|---|---|
| BR-21 | The system shall record the applicable approval tiers for each eligible Opportunity approval request. | Audit findings; Business objectives |
| BR-22 | The system shall preserve approval, rejection, resubmission, escalation, and notification events sufficient to demonstrate whether the required approval sequence occurred. | Audit findings; BR-08, BR-14, BR-19 |
| BR-23 | The approval process shall provide visibility into deep-discount Opportunities for Finance month-end reporting through the courtesy notification. | Proposed second-tier design |

## 9. Non-Functional Requirements

| Requirement ID | Requirement |
|---|---|
| NFR-01 | The approval workflow shall enforce sequential processing so that VP Sales review cannot occur before Deal Desk approval. |
| NFR-02 | The workflow shall enforce the stated 24-hour Deal Desk and 48-hour VP Sales SLAs. |
| NFR-03 | The workflow shall maintain an auditable history of approval decisions, comments, resubmissions, escalations, and Finance courtesy notifications. |
| NFR-04 | The solution shall preserve the current 10–25% approval behavior and avoid unnecessary impact to the approximately 80% of current volume represented by that band. |
| NFR-05 | The solution shall support notification delivery through email and Slack for VP Sales approval requests. |
| NFR-06 | The solution shall be reviewed by the Salesforce Admin before BRD sign-off circulation. |

Performance, availability, security, retention, accessibility, and detailed notification-content targets were not provided in the meeting note and require confirmation before final baseline approval.

## 10. Assumptions

- The existing Salesforce approval process can identify Opportunity type and discount percentage.
- Deal Desk remains the current sole approver for the existing approval tier unless an approved requirement states otherwise.
- Suresh Iyer has agreed to serve as VP Sales approver; confirmation was assigned to Priya Sharma and was not recorded as completed in the meeting note.
- A delegate for VP Sales coverage will be nominated; the delegate's identity and routing rules were not provided.
- Email and Slack integrations are available or can be made available within the delivery scope.
- “Discount exceeds 25%” means strictly greater than 25%; confirmation of boundary behavior at exactly 25% is recommended before build approval.
- Target go-live is interpreted exactly as recorded: end of Q2 FY26.

## 11. Dependencies

| Dependency ID | Dependency |
|---|---|
| DEP-01 | Confirmation of current Salesforce approval configuration and discount field behavior. |
| DEP-02 | Priya Sharma confirms Suresh Iyer's agreement to serve as tier-2 approver. |
| DEP-03 | Priya Sharma nominates a VP Sales delegate for coverage. |
| DEP-04 | Vikram Patel provides the last six months of approval requests with discount above 25% by 22 June 2026. |
| DEP-05 | Karan Verma completes Salesforce technical review before sign-off circulation. |
| DEP-06 | Availability and configuration of Slack and email notification channels. |

## 12. Risks

| Risk ID | Risk | Mitigation / Response |
|---|---|---|
| R-01 | VP Sales travel or limited availability may cause approval delays. | Enforce the 48-hour SLA and automatic escalation to Sunita Rao. |
| R-02 | Incomplete rejection-chain restart could permit audit gaming or approval carryover. | Enforce full-chain restart from Deal Desk and retain audit events. |
| R-03 | Notification delivery failure could reduce executive visibility. | Confirm email and Slack integration behavior and retain notification event history. |
| R-04 | Ambiguous discount boundary handling could route Opportunities incorrectly. | Confirm treatment of exactly 25% before final requirement baseline. |
| R-05 | Delegate coverage may be undefined at go-live. | Complete delegate nomination and document delegation routing before implementation approval. |
| R-06 | The target go-live phrase “end of Q2 FY26” may require calendar-date confirmation. | Confirm the governing fiscal calendar and deployment date during sign-off. |

## 13. Acceptance of Business Requirements

Approval of this BRD requires review by the business sponsor, Deal Desk, VP Sales or delegate, Finance representative, and Salesforce Admin. The named approvers and formal sign-off record were not provided in the meeting note.

## 14. References

1. Meeting Note — Discount Approval Enhancement (Discovery), dated 20 June 2026.
2. Q1 FY26 internal audit findings as summarized in the meeting note.
3. Existing Salesforce Opportunity discount approval process as described by Deal Desk during the meeting.

## 15. Open Information Required

- Confirmation that Suresh Iyer has accepted the tier-2 approver role.
- Name and rules for the VP Sales delegate.
- Confirmation of exact handling for a discount of exactly 25%.
- Confirmation of the fiscal-calendar date represented by “end of Q2 FY26.”
- Detailed email, Slack, Finance courtesy-notification, and escalation message content.
- Current Salesforce approval configuration and relevant field/API names.
- Required audit retention period and reporting format.
- Performance, availability, security, and access-control targets for the workflow.
