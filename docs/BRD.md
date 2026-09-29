# Business Requirements Document (BRD)

## Project Title
Large Deal Approval Enhancement for Salesforce Opportunity Management

## Date
Draft as of 22 June 2026

## Executive Summary

This BRD outlines enhancements to the Salesforce Opportunity approval process to address gaps in Finance oversight for high-value deals. The objective is to add a parallel Finance approval track, triggered by Total Contract Value (TCV) thresholds, to ensure proper review of large deals and prevent future audit/control issues. This process will complement, not replace, the existing Deal Desk approval for high-discount deals.

## Business Objectives

- Ensure Finance oversight on all large deals before closure.
- Prevent deals with non-standard terms from bypassing Finance review.
- Preserve sales cycle time by running Finance and Deal Desk approvals in parallel, not sequentially.
- Improve transparency and audit-ability via consolidated rejection feedback and reporting.

## Scope

- Salesforce Opportunity approval process
- Parallel Finance approval for TCV > $500K (two-tier internal: Director $500K-$2M, CFO >$2M)
- Parallelization with existing Deal Desk approval (triggered by Discount >10%)
- Notification via email and MS Teams channel
- Salesforce reporting/dashboard for Finance queue & Sales Ops

## Out of Scope

- Patient-facing communications
- Pharmacy or hospital adherence logic
- Renewals auto-approve logic (to be proposed in future BRD phase)

## Functional Requirements

### BR-01: Parallel Finance Approval Track
- For Opportunities where TCV > $500,000, trigger a parallel Finance approval alongside any existing Deal Desk approval.

### BR-02: Finance Approval Internal Routing
- For TCV between $500,000 and $2,000,000, assign approval to the Finance Director (Anita Rao).
- For TCV above $2,000,000, assign approval to the CFO (Ravi Krishnan).

### BR-03: Parallel Approval Logic
- Both Deal Desk (Discount >10%) and Finance (TCV > $500K) triggers may fire on the same Opportunity.
- Both approval tracks must complete for full sign-off; rejection in either equals Opportunity rejection.
- Approvals operate concurrently; total cycle time = max duration of the two, not sum.

### BR-04: Rejection and Resubmission Handling
- If either track rejects, present combined rejection comments to the Sales rep.
- Upon resubmission, restart both approval tracks from the beginning (no carryover approvals).

### BR-05: Notification Mechanism
- On each approval/rejection event, send notification email to relevant parties.
- Post the same information to the MS Teams channel `#finance-approvals`.

### BR-06: Reporting Requirements
- Provide a Finance dashboard tracking:
  - Opportunities in the Finance approval queue
  - Time to approval by sub-tier
  - Rejection reasons
- Provide Sales Ops with a similar view for their dashboard.
- Leverage Salesforce approval history and custom reports where needed.

### BR-07: Field Usage
- Use `Total_Contract_Value__c` field on Opportunity (populated via CPQ) as TCV trigger reference.

### BR-08: Standard Renewals Logic (Parking Lot)
- Standard renewals with TCV above threshold: initial take is to require Finance approval, but consider auto-approval if 0% additional discount and no non-standard terms. To be finalized in next BRD iteration.

## Non-Functional Requirements

- Notification integrations must work with both email and Microsoft Teams shared channels.
- Approvals and notifications must comply with SOX and internal control requirements.
- Salesforce automation must not degrade Opportunity page performance or approval cycle times (>10s added latency not acceptable).
- Custom reports must be accessible from both Finance and Sales Ops dashboards.

## Assumptions

- `Total_Contract_Value__c` is populated accurately via CPQ process.
- All approvers have required Salesforce access and are licensed for automated notifications.
- MS Teams connector is available and supports channel post actions.
- Renewal handling logic will be clarified and finalized in a future BRD.

## Dependencies

- MS Teams Salesforce connector configuration.
- List of specific fields for Finance dashboard (pending from Kavita).
- Confirmation of CFO approval routing above $2M (pending Anita/Ravi alignment).

## Risks

- Risk of notification failures on Teams/email channels.
- Potential workload spike for Finance if deal volume increases unexpectedly.
- Delays if renewal auto-approval logic is not defined before rollout.

## References

- Meeting Notes: Large Deal Approval Enhancement (22 June 2026)
- Stakeholder list: Priya Sharma, Anita Rao, Vikram Patel, Rajesh Menon, Kavita Bansal
