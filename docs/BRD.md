# Business Requirements Document (BRD)

## 1. Document Control

| Item | Value |
|---|---|
| Document title | Large Deal Approval Enhancement |
| Version | 1.0 |
| Status | Draft for stakeholder review |
| Source | Meeting Notes — Large Deal Approval Enhancement |
| Source date | 22 June 2026 |
| Prepared by | Rajesh Menon, BSA |
| Target delivery date stated in source | 25 June 2026 |

## 2. Executive Summary

The current Salesforce Opportunity approval process routes approval only to Deal Desk and is triggered by discount percentage. It does not use the existing `Total_Contract_Value__c` field as an approval trigger. Consequently, Opportunities with high TCV but discount at or below the Deal Desk threshold can close without Finance review.

The business requires a Finance approval track for Opportunities with TCV greater than $500,000. The Finance track must run in parallel with the existing Deal Desk approval process. Within Finance, Opportunities from $500,000 through $2,000,000 must route to Anita Rao, Finance Director, and Opportunities above $2,000,000 must route to Ravi Krishnan, CFO. When both approval triggers apply, both approvals are required before full sign-off.

The enhancement also requires rejection and resubmission behavior, email and Finance Teams channel notifications, and reporting for Finance and Sales Ops. Renewal treatment remains subject to the proposed rule documented in this BRD and stakeholder confirmation.

## 3. Business Problem and Opportunity

### 3.1 Current problem

- Three deals above $750,000 TCV closed without Finance seeing them during FY26 Q1.
- Two of those deals included non-standard revenue recognition terms, requiring approximately two weeks of FP&A cleanup.
- The current approval process has no deal-size trigger.
- A $2 million TCV Opportunity with an 8% discount can be auto-approved because the existing Deal Desk trigger is discount-based.
- `Total_Contract_Value__c` is populated on the Opportunity through CPQ but is not currently used by approval automation.

### 3.2 Business opportunity

A TCV-based Finance approval control will provide Finance visibility and approval rights over large deals before close, reduce the likelihood of post-close revenue-recognition remediation, and create a consistent audit trail for large-deal decisions.

## 4. Business Objectives and Success Measures

| Objective ID | Business objective | Success measure |
|---|---|---|
| OBJ-01 | Ensure Finance reviews large Opportunities before they close. | Every in-scope Opportunity with TCV greater than $500,000 is routed through the Finance approval track before full approval. |
| OBJ-02 | Preserve the existing Deal Desk control while adding Finance control. | The existing Deal Desk approval continues to operate independently on its existing `Discount > 10%` trigger. |
| OBJ-03 | Reduce avoidable approval-cycle delay caused by sequential routing. | When both tracks are triggered, they operate concurrently; total approval duration is governed by the slower active track rather than the sum of both tracks. |
| OBJ-04 | Improve transparency of Finance approval activity and outcomes. | Finance and Sales Ops can report on Finance queue volume, approval time by Finance sub-tier, and rejection reasons. |
| OBJ-05 | Provide an accessible notification trail for Finance reviewers. | Finance approval events generate email notifications and posts in the Finance shared Teams channel `#finance-approvals`. |

The source notes do not provide a numeric target for reduction in approval time, reduction in post-close cleanup, or approval SLA compliance. These measures require stakeholder definition during review.

## 5. Scope

### 5.1 In scope

- Salesforce Opportunity approval-process enhancement.
- TCV-based Finance approval trigger using `Total_Contract_Value__c`.
- Finance internal routing by TCV.
- Parallel operation of Finance and existing Deal Desk approval tracks.
- Combined rejection feedback and clean-slate resubmission behavior.
- Finance email and Teams channel-post notifications.
- Finance and Sales Ops reporting views based on Salesforce approval history and a custom report.
- Definition of renewal handling for high-TCV renewals, subject to the open decision in Section 12.

### 5.2 Out of scope

The source notes do not identify additional scope. The following are not defined as requirements by the source and must not be assumed: changes to discount thresholds, changes to CPQ calculation or population of `Total_Contract_Value__c`, changes to revenue-recognition policy, changes to approval authority outside the named approvers, or changes to Teams channel governance.

## 6. Stakeholders and Roles

| Stakeholder / role | Interest or responsibility |
|---|---|
| Priya Sharma, VP Sales Ops | Sales Operations sponsor; requires visibility in the Sales Ops dashboard. |
| Anita Rao, Finance Director | Finance approver for TCV from $500,000 through $2,000,000; business owner for Finance review control. |
| Ravi Krishnan, CFO | Proposed Finance approver for TCV above $2,000,000; alignment remains to be confirmed. |
| Vikram Patel, Deal Desk | Existing Deal Desk approval process owner/user. |
| Kavita Bansal, FP&A Lead | Reporting stakeholder; to provide specific dashboard fields. |
| Karan, Salesforce Administrator | Salesforce configuration and Teams connector feasibility confirmation. |
| Sales representative / rep | Submits or resubmits Opportunities and receives approval outcomes and comments. |
| Finance approval team | Receives Finance notifications and visibility through the shared Teams channel. |

## 7. Current-State Process

1. An Opportunity approval is routed only to Deal Desk.
2. The existing Deal Desk trigger is `Discount > 10%`.
3. There is no current TCV trigger in the approval automation.
4. `Total_Contract_Value__c` is populated by CPQ but is not used by approval automation.
5. An Opportunity below or at the Deal Desk discount trigger can proceed without Finance review, even when its TCV is high.

## 8. Future-State Business Process

1. The Salesforce approval process evaluates the Opportunity's TCV and discount conditions.
2. If TCV is greater than $500,000, the Finance track is initiated.
3. Finance routes the Opportunity internally according to TCV:
   - $500,000 through $2,000,000: Anita Rao, Finance Director.
   - Above $2,000,000: Ravi Krishnan, CFO.
4. Independently, if discount is greater than 10%, the existing Deal Desk track is initiated.
5. When both conditions are met, Finance and Deal Desk approvals run in parallel.
6. Full approval requires every track triggered for that Opportunity to approve.
7. If either active approver rejects, the Opportunity returns to the rep with comments from the tracks that have started.
8. On resubmission, both approval tracks restart from step 1 and prior-cycle approvals do not carry over.
9. Finance approval activity is communicated by email and posted to `#finance-approvals`.
10. Approval history and reporting data support Finance and Sales Ops monitoring.

## 9. Functional Requirements

### 9.1 Approval trigger and routing

| ID | Requirement | Source / rationale |
|---|---|---|
| BR-01 | The system shall evaluate `Total_Contract_Value__c` on each Opportunity subject to the approval process. | Meeting notes: existing field is populated by CPQ but unused by automation. |
| BR-02 | The system shall initiate the Finance approval track when `Total_Contract_Value__c > $500,000`, regardless of the Opportunity discount percentage. | Confirmed decision; explicitly includes 0% discount. |
| BR-03 | The system shall route Finance approval for Opportunities with TCV from $500,000 through $2,000,000 to Anita Rao, Finance Director. | Confirmed decision. |
| BR-04 | The system shall route Finance approval for Opportunities with TCV above $2,000,000 to Ravi Krishnan, CFO. | Confirmed decision; CFO alignment remains an open dependency. |
| BR-05 | The system shall preserve the existing Deal Desk approval trigger of `Discount > 10%` independently of the Finance TCV trigger. | Confirmed decision. |
| BR-06 | The system shall allow the Finance approval track and Deal Desk approval track to initiate and operate in parallel when both triggers are met. | Confirmed decision. |
| BR-07 | The system shall require approval from every triggered track before recording the Opportunity as fully approved. | Confirmed decision: both approvals are needed when both triggers fire. |
| BR-08 | The system shall not initiate the Finance track solely because an Opportunity has a discount greater than 10% when its TCV is $500,000 or less, unless another approved business rule applies. | Derived directly from the independent trigger definitions; requires validation if exceptions exist. |

### 9.2 Rejection and resubmission

| ID | Requirement | Source / rationale |
|---|---|---|
| BR-09 | If any active approver rejects the Opportunity, the system shall return the Opportunity to the rep for action. | Confirmed decision. |
| BR-10 | Upon rejection, the system shall return combined comments from both approval tracks that have started. | Confirmed decision. |
| BR-11 | When the rep resubmits a rejected Opportunity, the system shall restart all applicable approval tracks from step 1 and run them in parallel where both are applicable. | Confirmed decision. |
| BR-12 | The system shall not carry approvals from a prior approval cycle into a subsequent resubmission cycle. | Confirmed decision: clean slate on resubmit. |
| BR-13 | The system shall re-evaluate the applicable Finance and Deal Desk trigger conditions on resubmission. | Required to ensure the clean-slate cycle reflects the current Opportunity values; confirm during stakeholder review. |

### 9.3 Notifications

| ID | Requirement | Source / rationale |
|---|---|---|
| BR-14 | The system shall send email notifications for Finance approval activity to the applicable Finance approver(s). | Confirmed decision; standard email is acceptable. |
| BR-15 | The system shall post Finance approval notifications in the shared Microsoft Teams channel `#finance-approvals`. | Confirmed decision; channel post, not direct message. |
| BR-16 | The notification process shall use a shared channel post so the Finance team can see the pipeline, rather than a one-to-one Teams direct message. | Confirmed decision. |
| BR-17 | The system shall notify the relevant participants when the Finance track is rejected and when the Opportunity is resubmitted. | Implied by rejection/resubmission process; notification events and recipients require confirmation. |

### 9.4 Reporting and audit visibility

| ID | Requirement | Source / rationale |
|---|---|---|
| BR-18 | The solution shall provide a Finance-side monthly dashboard or report showing Opportunities in the Finance approval queue. | Kavita's reporting ask. |
| BR-19 | The solution shall show Finance approval time by Finance sub-tier: Finance Director and CFO. | Kavita's reporting ask. |
| BR-20 | The solution shall show rejection reasons for Finance approval cycles. | Kavita's reporting ask. |
| BR-21 | The solution shall provide the same Finance approval view on the Sales Ops dashboard. | Priya's reporting ask. |
| BR-22 | Reporting shall use standard Salesforce approval history and a custom report where those sources provide the required data. | Karan's stated approach. |
| BR-23 | The system shall retain an auditable record of the Finance and Deal Desk approval outcomes for each approval cycle. | Business need for control and traceability; confirm retention requirements. |

### 9.5 Renewal handling

| ID | Requirement | Source / rationale |
|---|---|---|
| BR-24 | The solution shall apply an explicitly approved Finance handling rule to high-TCV renewals. | Open item: renewal treatment must be resolved during BRD phase. |
| BR-25 | The proposed business rule is that a standard-priced renewal with 0% additional discount versus the prior term may be auto-approved for the Finance track; all other high-TCV renewals shall require Finance approval. | Proposal requested in meeting notes; not yet confirmed as a final decision. |
| BR-26 | The renewal auto-approval rule shall not bypass the existing Deal Desk approval when its `Discount > 10%` trigger is met. | Consistency with independent Deal Desk trigger; requires stakeholder confirmation. |

## 10. Non-Functional Requirements

| ID | Requirement | Source / rationale |
|---|---|---|
| NFR-01 | The approval solution shall provide an auditable record of approval decisions, approvers, comments, and approval cycles in Salesforce. | Approval control and reporting need. |
| NFR-02 | The approval solution shall support concurrent processing of the Finance and Deal Desk tracks without requiring one track to complete before the other begins. | Confirmed parallel-processing decision. |
| NFR-03 | Finance notifications shall be available through both email and the shared Microsoft Teams channel `#finance-approvals`. | Confirmed notification decision. |
| NFR-04 | Reports and dashboards shall distinguish Finance Director and CFO approval sub-tiers. | Reporting requirement. |
| NFR-05 | The solution shall use the existing Salesforce approval history and custom reporting capability where feasible. | Confirmed proposed reporting approach. |
| NFR-06 | Response-time, availability, data-retention, access-control, and notification-delivery targets shall be defined before final approval of this BRD. | Information not provided in source notes. |

## 11. Business Rules and Boundary Conditions

| Rule ID | Rule |
|---|---|
| RULE-01 | Finance trigger condition: `TCV > $500,000`. An Opportunity at exactly $500,000 does not meet the stated Finance trigger. |
| RULE-02 | Finance approval routing: $500,000 < TCV ≤ $2,000,000 routes to Anita Rao; TCV > $2,000,000 routes to Ravi Krishnan. |
| RULE-03 | Deal Desk trigger remains `Discount > 10%`. |
| RULE-04 | Finance and Deal Desk operate independently and in parallel when both triggers are true. |
| RULE-05 | All triggered approval tracks must approve for full sign-off. |
| RULE-06 | Any rejection returns the Opportunity to the rep. |
| RULE-07 | Resubmission starts a new approval cycle; prior-cycle approvals do not carry forward. |
| RULE-08 | Finance notifications use email and a shared Teams channel post, not a Teams direct message. |
| RULE-09 | Renewal auto-approval is proposed, not finalized, until stakeholders confirm the standard-pricing definition and comparison method. |

## 12. Assumptions and Open Decisions

### 12.1 Assumptions

- `Total_Contract_Value__c` is the authoritative TCV value for this approval decision because it is the existing CPQ-populated Opportunity field.
- Salesforce is the system of record for Opportunity approval status and approval history.
- The existing Deal Desk approval process can remain operational while the Finance track is added.
- The Finance Teams channel is named exactly `#finance-approvals` and is available to the intended Finance audience.
- “Full sign-off” means all approval tracks triggered for the Opportunity have approved.

### 12.2 Open decisions and information required

| ID | Open item | Owner / source |
|---|---|---|
| OD-01 | Confirm Ravi Krishnan's alignment to the above-$2M Finance approver assignment. | Anita Rao |
| OD-02 | Confirm that the Microsoft Teams connector supports a channel post from the Salesforce approval process. | Karan, Salesforce Administrator |
| OD-03 | Provide the specific fields required on the Finance dashboard. | Kavita Bansal; stated due date 24 June 2026 |
| OD-04 | Confirm whether standard-priced high-TCV renewals are auto-approved and define “standard pricing” and “0% additional discount versus prior term.” | Anita Rao / stakeholders |
| OD-05 | Confirm whether renewals are included in the Finance trigger without exception, or whether the proposed auto-approval exception applies. | Business stakeholders |
| OD-06 | Define recipients and content for rejection and resubmission notifications. | Finance, Sales Ops, Deal Desk |
| OD-07 | Define approval-time measurement start/end points and any target reporting period or SLA. | Finance and Sales Ops |
| OD-08 | Define access permissions for Finance and Sales Ops dashboards and approval history. | System and business owners |
| OD-09 | Define data-retention requirements for approval records and comments. | Finance / governance stakeholders |

## 13. Dependencies

| ID | Dependency |
|---|---|
| DEP-01 | CPQ must continue to populate `Total_Contract_Value__c` accurately before approval evaluation. |
| DEP-02 | Salesforce approval automation must support the two independent tracks and parallel execution. |
| DEP-03 | Named approver identities and delegation/backup arrangements must be available in Salesforce. Delegation rules are not provided. |
| DEP-04 | Microsoft Teams integration must support posting to `#finance-approvals`. |
| DEP-05 | Salesforce approval history and custom report capabilities must expose the data needed for the requested dashboards. |
| DEP-06 | Stakeholders must provide the dashboard field list and resolve renewal treatment before final requirements approval. |

## 14. Risks and Mitigations

| ID | Risk | Potential impact | Mitigation / response |
|---|---|---|---|
| RISK-01 | Incorrect or missing TCV values in `Total_Contract_Value__c`. | Large deals may bypass or incorrectly enter Finance approval. | Validate field population and define exception handling before activation. |
| RISK-02 | CFO assignment above $2M is not confirmed. | Approval routing may be rejected or become operationally blocked. | Obtain Anita's confirmation of Ravi's alignment before final sign-off. |
| RISK-03 | Teams connector cannot post from the approval process. | Finance channel visibility requirement may not be met. | Complete connector feasibility assessment and agree an approved alternative only if stakeholders authorize it. |
| RISK-04 | Finance receives approximately 12–15 approvals per month at the $500K threshold, with risk of review becoming perfunctory as volume changes. | Reduced quality of Finance review. | Monitor queue volume and review outcomes through the Finance dashboard; reassess threshold only through formal change control. |
| RISK-05 | Parallel tracks can produce different outcomes. | Rep may receive Deal Desk approval and Finance rejection, or the reverse, causing rework. | Require all triggered tracks to approve and return combined comments on rejection. |
| RISK-06 | Renewal auto-approval is insufficiently defined. | Eligible renewals may be incorrectly bypassed or unnecessarily routed. | Keep the rule proposed until pricing comparison and renewal criteria are explicitly approved. |
| RISK-07 | Reporting fields or permissions are incomplete. | Finance and Sales Ops may lack required visibility. | Obtain field list, define access, and validate report coverage before release. |

## 15. Traceability Matrix

| Business objective | Supporting requirements |
|---|---|
| OBJ-01 | BR-01, BR-02, BR-03, BR-04, BR-07, BR-24 |
| OBJ-02 | BR-05, BR-08, BR-26 |
| OBJ-03 | BR-06, BR-07, BR-11, NFR-02 |
| OBJ-04 | BR-18, BR-19, BR-20, BR-21, BR-22, BR-23, NFR-04, NFR-05 |
| OBJ-05 | BR-14, BR-15, BR-16, BR-17, NFR-03 |

## 16. References

1. `Meeting_Note_02_High_Value_Deal.md`, “Meeting Notes — Large Deal Approval Enhancement,” 22 June 2026.
2. Salesforce Opportunity field: `Total_Contract_Value__c` — identified in the source notes as CPQ-populated and currently unused by approval automation.
3. Existing Deal Desk approval rule: `Discount > 10%` — identified in the source notes.

## 17. Approval and Sign-off

| Role | Name | Status |
|---|---|---|
| Business sponsor, Sales Ops | Priya Sharma | Information not provided |
| Finance owner | Anita Rao | Information not provided |
| Finance executive approver | Ravi Krishnan | Alignment pending per OD-01 |
| Deal Desk representative | Vikram Patel | Information not provided |
| FP&A reporting stakeholder | Kavita Bansal | Information not provided |
| Salesforce administrator | Karan | Information not provided |
