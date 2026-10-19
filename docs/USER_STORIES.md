Assumption: Salesforce Approval Processes and existing Strategic Account field/security controls remain the implementation foundation; exact Salesforce configuration objects and Slack integration mechanism remain open for refinement.

# EPIC: Strategic Account Deal Governance

**Scope:**  
Approval visibility, CRO approval routing, escalation, delegation, notifications, resubmission controls, and reporting for Opportunities associated with Strategic Accounts.

**Business Objectives:**
- Ensure CRO visibility into meaningful Strategic Account deals.
- Require CRO approval for Strategic Account Opportunities above $100K TCV, regardless of discount.
- Preserve the existing Deal Desk approval behavior for discounts above 10%.
- Provide controlled executive delegation and SLA escalation.
- Improve auditability and QBR/Sales leadership reporting.

**Personas:**
- Sales Representative / Opportunity Owner
- Deal Desk Manager
- CRO
- CRO Chief of Staff
- Sales Operations
- Salesforce Platform Administrator
- QA / Test Analyst
- Sales Leadership

**Considerations:**
- `Strategic_Account__c` is maintained by Sales Ops and is view-only for reps.
- Routing is evaluated at submission time.
- Deal Desk and CRO routing is conditional and sequential.
- CRO approval SLA is 72 hours.
- Chief of Staff delegation cannot approve Opportunities with TCV of $500K or more.
- Rejected submissions restart the complete approval chain.

## Epic 1: Strategic Account Deal Governance

### Feature 1.1: Conditional Strategic Account Approval Routing

**Description:**  
Route Strategic Account Opportunities with TCV above $100K to the CRO for approval, while preserving the existing Deal Desk approval rule for discounts above 10%. Where both approvals apply, Deal Desk approval must precede CRO approval. Where Deal Desk does not apply, the CRO approval must begin directly.

**Definition of Done (DoD):**
- Strategic Account status is evaluated from `Account.Strategic_Account__c`.
- CRO approval triggers when Strategic Account is true and TCV is greater than $100K, including 0% discount.
- Opportunities at exactly $100K TCV do not trigger CRO approval unless another existing rule applies.
- Deal Desk remains the first approver when discount is greater than 10%.
- CRO approval follows Deal Desk approval when both conditions apply.
- CRO approval routes directly when Strategic Account and TCV conditions apply but discount is 10% or less.
- A rejection prevents downstream approval.
- Routing conditions are evaluated at submission time and remain unchanged for the in-flight request.
- Non-Strategic Account Opportunities retain existing behavior.
- Automated tests cover all routing combinations.

# 1.1 STORY: Route Strategic Account Opportunities to CRO Based on TCV

**Developer Persona:** BE

**User Story:**  
As a Sales Operations administrator, I want Strategic Account Opportunities above $100K TCV to route to the CRO for approval regardless of discount, so that meaningful Strategic Account deals receive executive governance.

**Business Value:**  
Prevents high-value Strategic Account deals from closing without CRO visibility or approval, including deals with no discount.

**Scenarios:**

**Scenario: Happy path — Strategic Account above threshold with no discount**  
Given an Opportunity is associated with an Account where `Strategic_Account__c = true`  
And the Opportunity TCV is greater than $100,000  
And the discount is 0%  
When the Opportunity is submitted for approval  
Then a CRO approval request is created  
And the Opportunity is not approved solely because the discount is below the existing Deal Desk threshold.

**Scenario: Boundary — TCV exactly at the threshold**  
Given an Opportunity is associated with a Strategic Account  
And the Opportunity TCV is exactly $100,000  
When the Opportunity is submitted for approval  
Then the Strategic Account CRO approval rule does not trigger based on TCV  
And any applicable existing approval rule is evaluated independently.

**Scenario: Boundary — TCV just above the threshold**  
Given an Opportunity is associated with a Strategic Account  
And the Opportunity TCV is $100,000.01  
When the Opportunity is submitted for approval  
Then a CRO approval request is created.

**Scenario: Error — Strategic Account indicator is unavailable**  
Given the Account cannot be evaluated because `Strategic_Account__c` is unavailable or invalid  
When the Opportunity is submitted for approval  
Then the submission is prevented or routed to an administrative error path  
And the system records an actionable error  
And the Opportunity is not silently approved.

**Definition of Ready (DoR):**
- TCV field and currency behavior are confirmed.
- The CRO approver identity or approval group is configured.
- The threshold comparison rule is approved as strictly greater than $100K.
- Existing approval processes and non-Strategic routing are documented.
- Test data exists for Strategic and non-Strategic Accounts.

**Definition of Done (DoD):**
- Routing works for 0%, below-threshold, exact-threshold, and above-threshold TCV cases.
- Invalid or unavailable trigger data cannot result in silent approval.
- Automated and regression tests pass.
- Approval history records the CRO request and submission timestamp.
- Configuration is documented and reviewed by Sales Operations.

**Dependencies:**
- Existing Salesforce Opportunity approval process.
- Account field `Strategic_Account__c`.
- TCV field and currency configuration.
- CRO approver configuration.
- Salesforce approval history.

**References:**
- [1] Meeting_Note_03_Strategic_Account.md  
  > Strategic Account approval threshold and trigger rules
- [2] Meeting_Note_01_Discount_Depth 2.md  
  > Conditional approval routing patterns

---

# 1.1 STORY: Orchestrate Conditional Deal Desk and CRO Approval Sequence

**Developer Persona:** BE

**User Story:**  
As a Deal Desk Manager, I want the existing Deal Desk approval rule and the new CRO approval rule to execute conditionally and in the correct order, so that Strategic Account governance does not change my current discount-review behavior.

**Business Value:**  
Maintains current commercial controls while ensuring CRO approval is added only where required and never bypasses a required Deal Desk review.

**Scenarios:**

**Scenario: Happy path — Both Deal Desk and CRO approvals apply**  
Given the Opportunity belongs to a Strategic Account  
And the TCV is greater than $100,000  
And the discount is greater than 10%  
When the Opportunity is submitted  
Then the Opportunity is routed first to Deal Desk  
And the CRO approval request is not actionable before Deal Desk approves  
And after Deal Desk approves, the request is routed to the CRO.

**Scenario: Happy path — CRO approval applies without Deal Desk**  
Given the Opportunity belongs to a Strategic Account  
And the TCV is greater than $100,000  
And the discount is 10% or less  
When the Opportunity is submitted  
Then the Opportunity is routed directly to the CRO  
And no Deal Desk approval request is created.

**Scenario: Boundary — Deal Desk rejects**  
Given both Deal Desk and CRO approval conditions apply  
When Deal Desk rejects the Opportunity  
Then the CRO approval request is not created or activated  
And the Opportunity is returned to the rep with the Deal Desk comments.

**Scenario: Boundary — Non-Strategic Account**  
Given the Opportunity belongs to an Account where `Strategic_Account__c = false`  
And the discount is greater than 10%  
When the Opportunity is submitted  
Then the existing Deal Desk process operates as it does today  
And no Strategic Account CRO approval is added.

**Scenario: Error — Approval chain cannot determine predecessor state**  
Given both Deal Desk and CRO conditions appear to apply  
When the system cannot determine whether Deal Desk approval is required or complete  
Then the Opportunity is not routed to CRO prematurely  
And the submission is flagged for administrative resolution  
And an error is logged.

**Definition of Ready (DoR):**
- Existing Deal Desk threshold is confirmed as strictly greater than 10%.
- Approval precedence rules are approved by Sales Operations and Deal Desk.
- The direct-CRO path and sequential path are separately testable.
- Rejection behavior is defined for each approval tier.

**Definition of Done (DoD):**
- Both routing paths are implemented and demonstrable.
- CRO cannot approve before required Deal Desk approval.
- Deal Desk rejection prevents CRO routing.
- Non-Strategic Account behavior passes regression testing.
- Approval history clearly identifies each approval tier and decision order.
- Automated tests cover all combinations of Strategic status, TCV, discount, and Deal Desk outcome.

**Dependencies:**
- Feature 1.1 Strategic Account trigger evaluation.
- Existing Deal Desk approval process.
- Salesforce approval-process predecessor configuration.
- Approval history and audit logging.

**References:**
- [1] Meeting_Note_03_Strategic_Account.md  
  > Conditional predecessor and sequential Deal Desk/CRO routing
- [2] Meeting_Note_01_Discount_Depth 2.md  
  > Conditional sequential approval routing

---

# 1.1 STORY: Preserve Submission-Time Routing Decisions

**Developer Persona:** BE

**User Story:**  
As a Sales Operations administrator, I want approval routing to be determined when an Opportunity is submitted, so that later changes to the Strategic Account flag do not alter an in-flight approval request.

**Business Value:**  
Creates predictable governance, prevents mid-process routing changes, and preserves an auditable record of the conditions used for the approval decision.

**Scenarios:**

**Scenario: Happy path — Strategic flag remains unchanged**  
Given an Opportunity is submitted while its Account is Strategic  
And the Opportunity meets the CRO TCV threshold  
When the approval process is active  
Then the Opportunity remains in the CRO approval path  
And the routing decision is retained in approval history.

**Scenario: Boundary — Account becomes Strategic after submission**  
Given an Opportunity is submitted while its Account is not Strategic  
And the Opportunity is not routed to CRO  
When Sales Ops later changes `Strategic_Account__c` to true  
Then the in-flight approval request is not retroactively routed to CRO  
And the new Strategic status applies only to a subsequent Opportunity submission.

**Scenario: Boundary — Strategic flag is removed after submission**  
Given an Opportunity is submitted while its Account is Strategic  
And the Opportunity is routed to CRO  
When Sales Ops later changes `Strategic_Account__c` to false  
Then the existing CRO approval request remains active  
And the approval chain is not removed or downgraded.

**Scenario: Error — Account data changes during submission**  
Given the Strategic flag changes concurrently with submission  
When the system evaluates routing  
Then one deterministic submission-time value is used  
And the selected value and routing outcome are auditable  
And the system does not create duplicate approval requests.

**Definition of Ready (DoR):**
- Submission-time evaluation behavior is approved.
- The required audit fields or approval-history evidence are identified.
- Concurrency and duplicate-submission behavior are defined.
- Test accounts and Opportunities are available.

**Definition of Done (DoD):**
- Routing is immutable for each in-flight approval request.
- Post-submission Strategic flag changes affect only later submissions.
- Concurrent updates do not produce duplicate or contradictory approval chains.
- Approval history identifies the routing outcome at submission.
- Regression and concurrency-oriented tests pass.

**Dependencies:**
- Strategic Account trigger evaluation.
- Salesforce approval submission transaction behavior.
- Account field-level security and Sales Ops maintenance process.
- Approval history reporting.

**References:**
- [1] Meeting_Note_03_Strategic_Account.md  
  > Routing determined at submission time
- [2] Meeting_Note_01_Discount_Depth 2.md  
  > Approval request validation and auditability

---

# Remaining Feature Structure

## Feature 1.2: CRO Delegation, SLA Escalation, and Notifications

**Definition of Done (DoD):**
- CRO approval has a 72-hour SLA.
- SLA breach automatically escalates to the CRO’s Chief of Staff.
- Chief of Staff can approve only Opportunities below $500K TCV.
- Opportunities at or above $500K cannot be approved by the Chief of Staff.
- CRO and delegated approver receive Email and Slack DM notifications.
- Approval, escalation, delegation, and notification events are auditable.
- Rejection comments are returned to the rep.
- Resubmission restarts the full approval chain.

## Feature 1.3: Strategic Account Approval Reporting and Governance

**Definition of Done (DoD):**
- Quarterly reporting includes every Strategic Account deal, approver decisions, and cycle time.
- Monthly reporting supports internal Sales leadership review.
- Reports use Salesforce approval history.
- Reports distinguish direct CRO routing from Deal Desk-then-CRO routing.
- Rejections, resubmissions, escalations, and final decisions are reportable.
- Sales Ops-only ownership of `Strategic_Account__c` is preserved.
- Reps retain view-only access to the Strategic Account indicator.

Which should be expanded next: **Feature 1.2 (delegation, SLA, notifications)** or **Feature 1.3 (reporting and governance)**?
