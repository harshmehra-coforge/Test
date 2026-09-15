# Business Requirements Document

## High-Value Deal Approval Enhancement

| Document Item | Value |
|---|---|
| Document date | 2026-09-15 |
| Source meeting | Working session, 22 June 2026, 3:00–4:00 PM IST |
| Business area | Salesforce Opportunity approval process |
| Target stakeholder sign-off | 26 June 2026 |
| Target go-live | End of Q2 FY26 |
| Status | Drafted from provided meeting notes |

## 1. Executive Summary

The current Salesforce Opportunity approval process routes deals only to Deal Desk based on discount percentage. Consequently, high-value deals with low or no discount can close without Finance review. This gap resulted in three deals above $750K TCV closing without Finance visibility and created post-close revenue-recognition remediation effort. The proposed enhancement adds a Finance approval track for Opportunities with Total Contract Value (TCV) greater than $500K. The Finance track will operate in parallel with the existing Deal Desk approval process, route internally to the Finance Director or CFO according to TCV, notify Finance through email and the shared Microsoft Teams channel, and provide monthly reporting for Finance and Sales Operations.

## 2. Business Objectives

1. Ensure Finance approval rights and visibility for every Opportunity with TCV greater than $500K before close.
2. Prevent high-value deals from bypassing Finance solely because their discount is at or below the Deal Desk threshold.
3. Preserve parallel processing so total approval cycle time is governed by the slower active approval track rather than the sum of sequential tracks.
4. Ensure rejected Opportunities return to the sales representative with comments from applicable approval tracks and restart with no approval carryover upon resubmission.
5. Provide Finance and Sales Operations with monthly visibility into Finance queue volume, approval time by Finance sub-tier, and rejection reasons.
6. Support the agreed target of stakeholder sign-off on 26 June 2026 and go-live by the end of Q2 FY26, subject to completion of dependencies and approvals.

## 3. Stakeholders

| Stakeholder / Role | Responsibility or Interest |
|---|---|
| Priya Sharma, VP Sales Ops | Sales Operations business owner; requires equivalent reporting visibility. |
| Anita Rao, Finance Director | Finance business owner; approver for TCV $500K–$2M; confirms Finance review need. |
| Ravi Krishnan, CFO | Approver for TCV above $2M; alignment confirmation is an open action. |
| Vikram Patel, Deal Desk | Existing Deal Desk process owner and approver. |
| Kavita Bansal, FP&A Lead | Reporting stakeholder; to provide detailed dashboard fields. |
| Karan, Salesforce Administrator | Salesforce configuration and connector feasibility input. |
| Rajesh Menon, BSA | BRD author and requirements coordination. |
| Sales representative | Receives rejected Opportunities and submits corrected/resubmitted Opportunities. |
| Finance team | Reviews Finance approval queue and receives notifications. |

## 4. Scope

### 4.1 In Scope

- Salesforce Opportunity approval enhancement.
- Finance approval trigger when `Total_Contract_Value__c` is greater than $500K, independent of discount percentage.
- Finance internal routing to Anita Rao for TCV from $500K through $2M and Ravi Krishnan for TCV above $2M.
- Parallel execution of the Finance track and the existing Deal Desk track.
- Existing Deal Desk trigger based on `Discount > 10%`.
- Combined rejection comments from approval tracks that have started.
- Full restart of applicable approval tracks after rejection and resubmission, without approval carryover.
- Finance notifications by standard email and a post to Microsoft Teams channel `#finance-approvals`.
- Standard Salesforce approval history and custom reporting.
- Monthly Finance-side and Sales Ops dashboards covering the agreed reporting measures.

### 4.2 Out of Scope

- Replacement of the existing Deal Desk approval process.
- Sequential execution of Deal Desk and Finance approvals.
- Direct-message notifications to individual Finance users.
- A finalized standard-renewal auto-approval rule; this was parked for BRD-phase proposal and remains unresolved in the source notes.
- Any reporting fields not yet specified by FP&A beyond Finance queue deals, approval time by Finance sub-tier, and rejection reasons.
- Changes to CPQ population of `Total_Contract_Value__c`; the source notes state the field is already populated.

## 5. Current State / Problem Statement

Salesforce Opportunity approval currently routes only to Deal Desk and uses discount percentage as its trigger. There is no deal-size trigger. A $2M TCV Opportunity with an 8% discount can therefore be auto-approved without Finance visibility. The source meeting notes document three deals above $750K TCV that closed without Finance seeing them and two cases requiring two weeks of post-close revenue-recognition cleanup.

## 6. Future State Overview

When an Opportunity is submitted for approval, Salesforce evaluates the existing Deal Desk rule and the new Finance TCV rule independently. Opportunities with discount greater than 10% enter the Deal Desk track. Opportunities with TCV greater than $500K enter the Finance track. If both conditions are met, both tracks run simultaneously and both must approve. Finance routes the Opportunity to the Finance Director or CFO based on TCV. A rejection from either active track returns the Opportunity to the sales representative with combined available comments. Resubmission starts the applicable tracks again from step 1, with no prior-cycle approval retained. Finance receives email and a shared Teams channel notification, and approval history feeds the required monthly dashboards.

## 7. Functional Requirements

| ID | Requirement |
|---|---|
| BR-01 | The solution shall evaluate each Salesforce Opportunity's `Total_Contract_Value__c` when the Opportunity enters the approval process. |
| BR-02 | The solution shall trigger the Finance approval track when `Total_Contract_Value__c` is greater than $500K, regardless of the Opportunity discount percentage. |
| BR-03 | The existing Deal Desk approval track shall continue to trigger independently when Opportunity discount is greater than 10%. |
| BR-04 | When both BR-02 and BR-03 conditions are true, Salesforce shall initiate the Finance and Deal Desk approval tracks in parallel. |
| BR-05 | An Opportunity requiring both tracks shall receive final approval only after both applicable tracks approve it. |
| BR-06 | The Finance track shall route Opportunities with TCV from $500K through $2M to Anita Rao, Finance Director. |
| BR-07 | The Finance track shall route Opportunities with TCV above $2M to Ravi Krishnan, CFO. |
| BR-08 | The Finance approval track shall generate a standard email notification to the relevant Finance approver or Finance approval audience when approval is required. |
| BR-09 | The Finance approval track shall post a notification to the Microsoft Teams channel `#finance-approvals` when approval is required. |
| BR-10 | The solution shall not send a direct message as the required Teams notification mechanism. |
| BR-11 | If either active approval track rejects an Opportunity, the solution shall return the Opportunity to the sales representative. |
| BR-12 | On rejection, the solution shall present combined comments from the approval tracks that have started and have comments available. |
| BR-13 | When a rejected Opportunity is resubmitted, the solution shall restart all applicable approval tracks from step 1. |
| BR-14 | The solution shall not carry approvals from a prior approval cycle into a subsequent resubmission cycle. |
| BR-15 | The solution shall retain standard Salesforce approval history for Finance and Deal Desk activity, including approval outcomes and timing information available through the platform. |
| BR-16 | The solution shall support a monthly Finance dashboard showing Opportunities in the Finance approval queue. |
| BR-17 | The solution shall support a monthly Finance dashboard showing approval time by Finance sub-tier. |
| BR-18 | The solution shall support a monthly Finance dashboard showing rejection reasons. |
| BR-19 | The solution shall provide Sales Operations with an equivalent view of the Finance queue, Finance sub-tier approval time, and rejection reasons. |
| BR-20 | The solution shall support the agreed target stakeholder sign-off date of 26 June 2026 and target go-live by the end of Q2 FY26, subject to stakeholder, connector, and implementation readiness. |

## 8. Non-Functional Requirements

| ID | Requirement |
|---|---|
| NFR-01 | Approval routing and trigger evaluation shall use the authoritative Salesforce Opportunity value in `Total_Contract_Value__c`. |
| NFR-02 | Approval activity shall be auditable through Salesforce approval history and reportable by approval track, Finance sub-tier, outcome, and timing where the underlying Salesforce data supports those dimensions. |
| NFR-03 | Notifications shall be delivered through the agreed channels: email and the shared Teams channel `#finance-approvals`; direct messaging is not required. |
| NFR-04 | The parallel approval design shall avoid introducing sequential dependency between Deal Desk and Finance tracks. |
| NFR-05 | Access to approval records and dashboards shall follow the organization's existing Salesforce and Finance/Sales Ops access controls. Specific permission mappings were not provided. |
| NFR-06 | Dashboard data shall be available on a monthly reporting basis and distinguish Finance approval sub-tiers. |
| NFR-07 | The solution shall preserve an auditable distinction between approval cycles so prior-cycle approvals are not confused with resubmission-cycle approvals. |

## 9. Assumptions

- `Total_Contract_Value__c` is populated on Salesforce Opportunities by CPQ before approval evaluation.
- The existing Deal Desk approval rule and its current ownership remain unchanged except for coexistence with the Finance track.
- TCV values are denominated in US dollars, consistent with the meeting notes.
- “From $500K through $2M” means TCV greater than or equal to $500K and less than or equal to $2M; “above $2M” means strictly greater than $2M.
- Standard Salesforce approval history and a custom report can provide the agreed reporting measures.
- Finance approvers and the Teams channel remain available and authorized for the target release.
- Both tracks can be active simultaneously and Salesforce can aggregate their outcomes for final disposition.

## 10. Dependencies

- Confirmation from Ravi Krishnan that he is aligned with the above-$2M Finance approval assignment.
- Confirmation from Karan that the Microsoft Teams connector supports channel posts from the approval process.
- FP&A delivery of the specific dashboard fields requested by Kavita Bansal.
- Salesforce configuration of Finance approval routing, parallel process behavior, notifications, rejection comments, and resubmission reset.
- CPQ availability and accuracy of `Total_Contract_Value__c` at approval time.
- Stakeholder review and sign-off by 26 June 2026.

## 11. Risks

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| Teams connector cannot post from the approval process. | Finance may miss the agreed shared-channel notification. | Medium | Validate connector capability before configuration; retain email as the agreed notification channel. |
| CFO alignment on the above-$2M sub-tier is not obtained. | High-value Opportunities may lack an approved Finance owner. | Medium | Obtain explicit confirmation from Ravi before release. |
| TCV is missing or inaccurate on an Opportunity. | High-value deals could bypass or incorrectly enter Finance approval. | Medium | Validate CPQ field population and define exception handling during design confirmation. |
| Combined comments are not consistently available across tracks. | Sales representatives may receive incomplete rejection guidance. | Medium | Confirm comment aggregation behavior and ensure available comments from started tracks are retained. |
| Parallel-track reset behavior is configured incorrectly. | Prior-cycle approvals could be carried forward, creating control and audit risk. | Medium | Validate clean-cycle behavior using approval history and resubmission scenarios. |
| Dashboard field requirements arrive late. | Monthly reporting may not meet FP&A and Sales Ops needs by go-live. | Medium | Obtain Kavita's field list by 24 June 2026 and baseline the report design against BR-16–BR-19. |
| Standard-renewal treatment remains unresolved. | Renewals may be handled inconsistently after launch. | Medium | Resolve as a separate business decision before including any renewal auto-approval logic. |

## 12. Open Questions

1. Does Finance approval apply to renewals with high TCV and standard terms? The source notes record differing views and no final decision.
2. If renewal auto-approval is desired, what exact rule defines “standard pricing” and “0% additional discount versus prior term”?
3. What specific additional dashboard fields does FP&A require beyond the three measures recorded in this BRD?
4. What is the required handling when `Total_Contract_Value__c` is blank, unavailable, or updated while an approval is in progress?
5. What business time zone and timestamp convention should be used for approval-time reporting? Information not provided.
6. What formal exception or delegation process applies when Anita Rao or Ravi Krishnan is unavailable? Information not provided.

## 13. References

- `Meeting_Note_02_High_Value_Deal.md`, “Large Deal Approval Enhancement,” working session dated 22 June 2026.
- Knowledge-base retrieval: no relevant contextual content returned; project requirements are grounded in the provided meeting note and stated durable project context.

## 14. Traceability Summary

The core control objective is addressed by BR-01–BR-07. Notification requirements are addressed by BR-08–BR-10 and NFR-03. Rejection and clean resubmission behavior are addressed by BR-11–BR-14 and NFR-07. Audit and reporting are addressed by BR-15–BR-19 and NFR-02/NFR-06. Schedule alignment is addressed by BR-20 and the Dependencies section.
