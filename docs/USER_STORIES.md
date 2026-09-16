# Large Deal Approval Enhancement — Product Backlog

> Grounded in `BRD (4).md`, version 1.0. Assumptions: Salesforce remains the system of record; `Total_Contract_Value__c` is authoritative; renewal exceptions remain disabled until explicitly approved; unresolved BRD open decisions are tracked as dependencies rather than invented behavior.

## Backlog Structure

### EPIC 1 — Finance Approval Triggering and Parallel Routing
- **F1.1** Evaluate TCV and route Finance approval by threshold
  - US-001 Evaluate TCV-based Finance trigger
  - US-002 Route mid-tier Finance approvals to Finance Director
  - US-003 Route high-tier Finance approvals to CFO
- **F1.2** Preserve Deal Desk control and coordinate approval tracks
  - US-004 Preserve independent Deal Desk trigger
  - US-005 Run triggered approval tracks in parallel and require full sign-off

### EPIC 2 — Approval Outcomes and Resubmission
- **F2.1** Handle rejection feedback
  - US-006 Return rejected Opportunity with combined comments
- **F2.2** Restart approval on resubmission
  - US-007 Re-evaluate triggers and start a clean approval cycle

### EPIC 3 — Notifications, Reporting and Auditability
- **F3.1** Notify Finance approval participants
  - US-008 Notify Finance approvers by email
  - US-009 Post Finance events to the shared Teams channel
  - US-010 Notify relevant participants on rejection and resubmission
- **F3.2** Provide Finance and Sales Ops visibility
  - US-011 Provide Finance approval queue and tier-time reporting
  - US-012 Provide rejection-reason reporting and Sales Ops view
- **F3.3** Retain approval audit history
  - US-013 Maintain auditable approval-cycle records

---

# EPIC 1 — Finance Approval Triggering and Parallel Routing

## Feature F1.1 — Evaluate TCV and route Finance approval by threshold

### US-001 — Evaluate the Finance approval trigger from TCV
**Developer persona:** BE

**User story**  
As a Salesforce approval automation, I want to evaluate `Total_Contract_Value__c` for each Opportunity entering approval, so that every Opportunity above the Finance threshold receives Finance review regardless of discount.

**Business Value**  
Prevents high-value Opportunities from closing without Finance visibility, including deals with 0% or otherwise low discount.

**Scenarios / Acceptance Criteria**
- **Given** an Opportunity has TCV greater than $500,000, **when** the approval process evaluates it, **then** the Finance approval track is initiated regardless of discount percentage.
- **Given** an Opportunity has TCV exactly $500,000, **when** the approval process evaluates it, **then** the Finance track is not initiated by the stated TCV rule.
- **Given** an Opportunity has TCV below $500,000, **when** the approval process evaluates it, **then** the Finance track is not initiated by the stated TCV rule.
- **Given** TCV is missing or cannot be evaluated, **when** the approval process is submitted, **then** the system must not silently treat the value as an eligible or ineligible value; it must follow an agreed exception/error path and record the outcome. *(Dependency: TCV exception handling is not defined in the BRD.)*

**Definition of Ready (DoR)**
- TCV field API name, data type, currency handling, and CPQ population behavior are confirmed.
- Exact behavior for missing/invalid TCV is approved.
- Entry point and submission permissions are identified.

**Definition of Done (DoD)**
- Trigger logic is configured and peer-reviewed.
- Boundary and error cases are automated-tested.
- Salesforce test evidence confirms the trigger is independent of discount.
- Deployment and rollback steps are documented.

**Dependencies**  
DEP-01, DEP-02; RISK-01.

---

### US-002 — Route mid-tier Finance approvals to Anita Rao
**Developer persona:** BE

**User story**  
As Finance, I want Opportunities above $500,000 and up to $2,000,000 to route to Anita Rao, so that the Finance Director reviews the defined mid-tier deals.

**Business Value**  
Creates consistent approval ownership for the primary large-deal range.

**Scenarios / Acceptance Criteria**
- **Given** TCV is greater than $500,000 and less than or equal to $2,000,000, **when** Finance routing occurs, **then** the approval is assigned to Anita Rao, Finance Director.
- **Given** TCV is exactly $2,000,000, **when** Finance routing occurs, **then** the approval is assigned to Anita Rao.
- **Given** TCV is exactly $500,000, **when** Finance routing occurs, **then** this Finance track is not created by the stated trigger and no Anita assignment is made by this rule.
- **Given** the approver identity is unavailable or inactive, **when** routing occurs, **then** the system must surface a routing error and prevent an unassigned approval from being presented as successfully routed. *(Dependency: approver identity/delegation policy.)*

**Definition of Ready (DoR)**
- Anita Rao’s active Salesforce user, role, and delegation/backup arrangement are confirmed.
- Currency and threshold comparison rules are approved.
- Test data for both boundaries is available.

**Definition of Done (DoD)**
- Routing is configured to Anita for the complete mid-tier range.
- Boundary, inactive-user, and unauthorized-routing tests pass.
- Approval assignment is visible in Salesforce history.

**Dependencies**  
DEP-02, DEP-03; OD-08.

---

### US-003 — Route high-tier Finance approvals to Ravi Krishnan
**Developer persona:** BE

**User story**  
As Finance leadership, I want Opportunities above $2,000,000 to route to Ravi Krishnan, so that CFO-level review is applied to the highest-value deals.

**Business Value**  
Ensures the approval authority matches the financial exposure of the Opportunity.

**Scenarios / Acceptance Criteria**
- **Given** TCV is greater than $2,000,000, **when** Finance routing occurs, **then** the approval is assigned to Ravi Krishnan, CFO.
- **Given** TCV is exactly $2,000,000, **when** Finance routing occurs, **then** the approval is assigned to Anita Rao rather than Ravi Krishnan.
- **Given** Ravi’s assignment is not approved or his Salesforce identity is unavailable, **when** a high-tier Opportunity is submitted, **then** the system must not route it to an unconfirmed or inactive approver; release is blocked pending OD-01 resolution.

**Definition of Ready (DoR)**
- OD-01 is resolved and Ravi’s assignment is approved.
- Ravi’s active user, role, delegation, and access are configured.
- High-tier boundary test data is available.

**Definition of Done (DoD)**
- Above-$2M routing is configured and peer-reviewed.
- Exact-$2M and above-$2M tests pass.
- Assignment and outcome appear in auditable Salesforce history.

**Dependencies**  
DEP-02, DEP-03, OD-01, OD-08.

---

## Feature F1.2 — Preserve Deal Desk control and coordinate approval tracks

### US-004 — Preserve the independent Deal Desk trigger
**Developer persona:** BE

**User story**  
As Deal Desk, I want the existing `Discount > 10%` approval trigger to remain independent, so that the Finance enhancement does not weaken the current discount control.

**Business Value**  
Maintains the existing governance control while adding TCV-based Finance oversight.

**Scenarios / Acceptance Criteria**
- **Given** discount is greater than 10%, **when** an Opportunity is evaluated, **then** the Deal Desk track is initiated regardless of TCV.
- **Given** discount is exactly 10% or less, **when** an Opportunity is evaluated, **then** the Deal Desk track is not initiated by the existing rule.
- **Given** TCV is greater than $500,000 and discount is 10% or less, **when** evaluated, **then** Finance starts and Deal Desk does not start solely due to TCV.
- **Given** TCV is $500,000 or less and discount is greater than 10%, **when** evaluated, **then** Deal Desk starts and Finance does not start solely due to TCV.

**Definition of Ready (DoR)**
- Existing Deal Desk process and current threshold are baseline-tested.
- Ownership of the existing process is confirmed.

**Definition of Done (DoD)**
- Existing Deal Desk configuration is unchanged except for required orchestration.
- All four trigger combinations are tested and pass.
- Regression evidence confirms existing Deal Desk behavior.

**Dependencies**  
DEP-02.

---

### US-005 — Run approval tracks in parallel and require all triggered approvals
**Developer persona:** BE

**User story**  
As a Sales representative, I want applicable Finance and Deal Desk approvals to run concurrently and determine full approval only after every triggered track approves, so that I avoid unnecessary sequential delay without losing control.

**Business Value**  
Reduces avoidable cycle time while ensuring all required approval authorities sign off.

**Scenarios / Acceptance Criteria**
- **Given** both TCV and discount triggers are true, **when** the Opportunity is submitted, **then** Finance and Deal Desk initiate without either waiting for the other.
- **Given** only Finance is triggered, **when** Finance approves, **then** the Opportunity can reach full approval without a Deal Desk approval.
- **Given** only Deal Desk is triggered, **when** Deal Desk approves, **then** the Opportunity can reach full approval without a Finance approval.
- **Given** both tracks are triggered and one approves while the other is pending, **when** status is evaluated, **then** the Opportunity is not fully approved.
- **Given** both tracks are triggered and both approve, **when** the final approval is recorded, **then** the Opportunity is fully approved.
- **Given** one triggered track rejects, **when** the rejection is recorded, **then** the Opportunity cannot reach full approval in that cycle.

**Definition of Ready (DoR)**
- Salesforce orchestration approach supports concurrent execution.
- Approval status model and full-sign-off state are agreed.
- Timing measurement boundaries are defined or explicitly deferred (OD-07).

**Definition of Done (DoD)**
- Parallel orchestration is implemented without sequential dependency.
- Mixed pending/approved/rejected combinations are tested.
- Performance and audit evidence demonstrate both tracks start independently.

**Dependencies**  
DEP-02, OD-07; RISK-05.

# EPIC 2 — Approval Outcomes and Resubmission

## Feature F2.1 — Handle rejection feedback

### US-006 — Return a rejected Opportunity with combined comments
**Developer persona:** BE

**User story**  
As a Sales representative, I want a rejected Opportunity returned with comments from every approval track that started, so that I can address all identified issues before resubmitting.

**Business Value**  
Prevents fragmented feedback and reduces rework across approval cycles.

**Scenarios / Acceptance Criteria**
- **Given** Finance rejects an Opportunity and Deal Desk was not triggered, **when** rejection completes, **then** the Opportunity returns to the rep with the Finance comments.
- **Given** Deal Desk rejects an Opportunity and Finance was not triggered, **when** rejection completes, **then** the Opportunity returns to the rep with the Deal Desk comments.
- **Given** both tracks started and Finance rejects while Deal Desk is pending or approved, **when** the cycle is rejected, **then** the Opportunity returns to the rep and includes Finance comments plus any available Deal Desk comments from that cycle.
- **Given** both tracks started and Deal Desk rejects while Finance is pending or approved, **when** the cycle is rejected, **then** the Opportunity returns to the rep and includes Deal Desk comments plus any available Finance comments from that cycle.
- **Given** an approver rejects without comments, **when** rejection is recorded, **then** the system preserves the rejection outcome and clearly indicates that no comment was supplied rather than fabricating one.

**Definition of Ready (DoR)**
- Combined-comment format, ordering, visibility, and field limits are agreed.
- Recipients for rejection notification are defined (OD-06).
- Salesforce rejection semantics for parallel tracks are confirmed.

**Definition of Done (DoD)**
- Rejection returns the Opportunity to the rep in every track combination.
- Combined comments are retained and visible to authorized users.
- Tests cover one-track, two-track, pending, approved, and blank-comment cases.

**Dependencies**  
DEP-02, OD-06, OD-08, OD-09; RISK-05.

---

## Feature F2.2 — Restart approval on resubmission

### US-007 — Re-evaluate triggers and start a clean approval cycle
**Developer persona:** BE

**User story**  
As a Sales representative, I want resubmission to start a fresh approval cycle based on current Opportunity values, so that prior approvals cannot bypass required review.

**Business Value**  
Maintains control integrity when deal values, discounts, or terms change after rejection.

**Scenarios / Acceptance Criteria**
- **Given** a rejected Opportunity is resubmitted, **when** the approval process starts, **then** all applicable tracks restart from step 1.
- **Given** both triggers are true on resubmission, **when** the new cycle starts, **then** Finance and Deal Desk restart in parallel.
- **Given** a prior cycle had one or more approvals, **when** a new cycle starts, **then** no prior approval satisfies a new-cycle approval requirement.
- **Given** TCV or discount changed between rejection and resubmission, **when** the new cycle evaluates, **then** Finance and Deal Desk eligibility is determined from the current values.
- **Given** a prior Finance-triggered Opportunity is resubmitted with TCV exactly $500,000 or less, **when** the new cycle evaluates, **then** Finance does not start solely from the prior cycle.
- **Given** the Opportunity is resubmitted without required rep action or required data, **when** submission is attempted, **then** the system prevents submission and explains the validation failure.

**Definition of Ready (DoR)**
- New-cycle identifier/history behavior is defined.
- Current-value re-evaluation and validation rules are agreed.
- Rep permissions and resubmission UI/API path are available.

**Definition of Done (DoD)**
- Clean-slate resubmission is implemented.
- Trigger changes and prior-approval non-carryover are automated-tested.
- Cycle history distinguishes the original cycle from each resubmission.

**Dependencies**  
DEP-02, OD-09; RISK-01, RISK-05.

# EPIC 3 — Notifications, Reporting and Auditability

## Feature F3.1 — Notify Finance approval participants

### US-008 — Send Finance approval email notifications
**Developer persona:** BE

**User story**  
As a Finance approver, I want an email notification when a Finance approval is assigned or has relevant status activity, so that I can act on the approval promptly.

**Business Value**  
Improves Finance responsiveness and creates an accessible notification trail.

**Scenarios / Acceptance Criteria**
- **Given** a Finance track is initiated, **when** an approver is assigned, **then** an email is sent to the applicable Finance approver.
- **Given** a Finance approval is rejected or the Opportunity is resubmitted, **when** the event occurs, **then** the configured relevant participants receive the required email notification.
- **Given** the notification service fails, **when** an approval event occurs, **then** the approval outcome is not falsely reported as notified and the failure is logged for support follow-up.
- **Given** only Deal Desk is triggered, **when** the Opportunity enters approval, **then** no Finance assignment email is sent.

**Definition of Ready (DoR)**
- Event list, recipients, templates, links, and sender identity are approved.
- OD-06 is resolved.
- Email delivery and failure logging approach are confirmed.

**Definition of Done (DoD)**
- Required Finance emails are configured and tested.
- Recipient, subject, content, and Salesforce record-link tests pass.
- Delivery failure is observable without blocking unrelated approval history.

**Dependencies**  
DEP-03, OD-06, OD-08; NFR-03.

---

### US-009 — Post Finance approval events to the shared Teams channel
**Developer persona:** BE

**User story**  
As the Finance approval team, I want Finance approval events posted to `#finance-approvals`, so that the team has shared visibility into the pipeline without relying on direct messages.

**Business Value**  
Makes Finance approval activity visible to the intended team and supports operational coordination.

**Scenarios / Acceptance Criteria**
- **Given** a Finance approval event is configured for channel visibility, **when** the event occurs, **then** a post is sent to the shared `#finance-approvals` channel.
- **Given** both Finance and Deal Desk tracks are active, **when** a Finance event occurs, **then** the Teams post represents the Finance event and does not imply Deal Desk approval.
- **Given** the Teams connector cannot post, **when** a Finance event occurs, **then** the connector failure is logged and surfaced; no unauthorized direct-message fallback is used.
- **Given** only Deal Desk is triggered, **when** the Opportunity enters approval, **then** no Finance channel post is generated solely for that event.

**Definition of Ready (DoR)**
- OD-02 is resolved; channel identity and connector authentication are available.
- Post event types, content, sensitive-data rules, and retry behavior are approved.

**Definition of Done (DoD)**
- Shared-channel posting is implemented for approved Finance events.
- Connector success, failure, retry/idempotency, and no-DM tests pass.
- Operational logging and support ownership are documented.

**Dependencies**  
DEP-04, OD-02, OD-06; RISK-03.

---

### US-010 — Notify participants on rejection and resubmission
**Developer persona:** BE

**User story**  
As an approval participant, I want rejection and resubmission events communicated consistently, so that I know when action is required and which approval cycle is active.

**Business Value**  
Reduces missed actions and ambiguity after a parallel approval outcome.

**Scenarios / Acceptance Criteria**
- **Given** any active track rejects, **when** the Opportunity returns to the rep, **then** the configured participants are notified of rejection and can access the comments.
- **Given** a rejected Opportunity is resubmitted, **when** the new cycle starts, **then** the configured participants are notified that a new cycle is active.
- **Given** both tracks started, **when** one rejects, **then** the notification distinguishes the rejecting track and includes combined available comments.
- **Given** recipients or content are not approved, **when** the solution is prepared for release, **then** notification behavior remains pending OD-06 rather than using assumed recipients.

**Definition of Ready (DoR)**
- OD-06 is resolved with recipients, event matrix, and content.
- New-cycle identification and comment visibility are defined.

**Definition of Done (DoD)**
- Rejection/resubmission event matrix is implemented.
- All one-track and two-track scenarios are tested.
- Duplicate and failed notifications are logged and supportable.

**Dependencies**  
OD-06, OD-08, DEP-04; NFR-03.

## Feature F3.2 — Provide Finance and Sales Ops visibility

### US-011 — Report Finance queue volume and approval time by tier
**Developer persona:** BE

**User story**  
As a Finance stakeholder, I want a monthly view of Opportunities in the Finance approval queue and approval time by Finance sub-tier, so that I can monitor workload and responsiveness.

**Business Value**  
Supports Finance capacity planning and identifies delays between Director- and CFO-level reviews.

**Scenarios / Acceptance Criteria**
- **Given** Opportunities have entered the Finance track, **when** the Finance report is run for a selected month, **then** it shows Opportunities currently or recently in the Finance queue according to the approved reporting definition.
- **Given** Finance approvals are assigned to Anita or Ravi, **when** approval time is calculated, **then** the report distinguishes Finance Director and CFO sub-tiers.
- **Given** an approval is pending, approved, or rejected, **when** the report is filtered, **then** the status is represented accurately.
- **Given** approval-time start/end points or SLA are not defined, **when** the report is designed, **then** the metric definition remains an explicit dependency rather than an invented target.
- **Given** a user lacks Finance report permission, **when** the report is opened, **then** access is denied or restricted according to the approved access model.

**Definition of Ready (DoR)**
- OD-03 provides required dashboard fields.
- OD-07 defines timing start/end points and reporting period.
- OD-08 defines access permissions.

**Definition of Done (DoD)**
- Monthly Finance queue view and tier segmentation are available.
- Calculations are reconciled against Salesforce approval history.
- Permission, empty-result, date-boundary, and pending-item tests pass.

**Dependencies**  
DEP-05, OD-03, OD-07, OD-08, OD-09; NFR-04, NFR-05.

---

### US-012 — Report rejection reasons and expose the view to Sales Ops
**Developer persona:** BE

**User story**  
As Sales Ops and FP&A, I want Finance rejection reasons and the Finance approval view available in reporting, so that I can identify recurring issues and monitor the control.

**Business Value**  
Improves transparency, supports trend analysis, and gives Sales Ops the requested operational visibility.

**Scenarios / Acceptance Criteria**
- **Given** a Finance cycle has a rejection comment, **when** reporting data is refreshed, **then** the rejection reason is available against the relevant Opportunity and approval cycle.
- **Given** a cycle has no rejection, **when** the report is viewed, **then** it is not incorrectly represented as rejected.
- **Given** a Finance approval view is published for Sales Ops, **when** an authorized Sales Ops user opens it, **then** the user can access the agreed Finance queue, tier-time, and rejection information.
- **Given** a user is not authorized for the report, **when** access is attempted, **then** Salesforce enforces the approved permission model.
- **Given** multiple approval cycles exist for one Opportunity, **when** rejection reasons are displayed, **then** each reason is attributable to the correct cycle and track.

**Definition of Ready (DoR)**
- OD-03 dashboard field list and OD-08 access rules are approved.
- Rejection reason source, normalization, and cycle-level association are defined.
- Finance and Sales Ops report owners are identified.

**Definition of Done (DoD)**
- Finance rejection reason reporting is available.
- Sales Ops dashboard contains the agreed Finance view.
- Multi-cycle, blank-reason, permission, and data-reconciliation tests pass.

**Dependencies**  
DEP-05, OD-03, OD-08, OD-09; RISK-07.

---

## Feature F3.3 — Retain approval audit history

### US-013 — Maintain auditable records for each approval cycle
**Developer persona:** BE

**User story**  
As Finance and Sales Ops, I want each Finance and Deal Desk decision, approver, comment, status, and cycle retained in Salesforce, so that large-deal approvals are traceable and reportable.

**Business Value**  
Provides evidence of control operation and supports investigation, reporting, and revenue-governance oversight.

**Scenarios / Acceptance Criteria**
- **Given** an approval track starts, **when** its status changes, **then** the system records the track, approver, status, timestamp, and associated Opportunity/cycle.
- **Given** an approver supplies comments, **when** the decision is recorded, **then** the comments are retained with the corresponding decision.
- **Given** both tracks operate in parallel, **when** either track changes status, **then** the history distinguishes Finance from Deal Desk and does not overwrite the other track.
- **Given** a resubmission starts, **when** the new cycle begins, **then** it is distinguishable from prior cycles and prior approvals are not reused as current approvals.
- **Given** an authorized report user queries approval history, **when** results are returned, **then** the results support the Finance queue, tier-time, rejection, and full-sign-off reporting needs.
- **Given** an unauthorized user queries approval history, **when** access is attempted, **then** Salesforce enforces approved access restrictions.

**Definition of Ready (DoR)**
- Required history fields and cycle identifier are defined.
- OD-08 access and OD-09 retention requirements are resolved.
- Standard approval history/custom report feasibility is confirmed.

**Definition of Done (DoD)**
- Audit records are retained for all approval tracks and cycles.
- Data is immutable or change-controlled according to approved governance.
- Traceability, security, retention, and reporting reconciliation tests pass.

**Dependencies**  
DEP-05, OD-08, OD-09; NFR-01, NFR-05, NFR-06.

---

# Cross-Backlog Notes

## Explicitly deferred decision
High-TCV renewal handling is not converted into an executable story because BR-24–BR-26 are proposed/open. Before implementation, stakeholders must define whether standard-priced renewals with 0% additional discount may bypass Finance, what “standard pricing” means, how prior-term comparison works, and how Deal Desk remains independent. Once approved, renewal behavior can be added without changing the core trigger/routing stories.

## Shared dependencies and risks
- **DEP-01 / RISK-01:** CPQ must populate `Total_Contract_Value__c` accurately; missing/invalid-value behavior requires approval.
- **DEP-02:** Salesforce must support two independent parallel tracks.
- **DEP-03 / OD-01:** named approver identities, CFO alignment, and delegation must be available.
- **DEP-04 / OD-02 / RISK-03:** Teams connector must support a shared channel post to `#finance-approvals`.
- **OD-03 / OD-07 / OD-08 / OD-09:** reporting fields, timing definitions, permissions, and retention remain prerequisites.
- **OD-06:** rejection/resubmission notification recipients and content remain undefined.

## INVEST validation summary
- **Independent:** stories are separated by trigger, route, orchestration, outcome, notifications, reporting, and audit concern.
- **Negotiable:** unresolved decisions are explicitly dependencies, not hidden implementation commitments.
- **Valuable:** each story maps to BR-01–BR-23 and supporting objectives.
- **Estimable/Testable:** scenarios include happy paths, boundaries, errors, permissions, and parallel-track combinations.
- **Small:** each story targets one coherent behavior or reporting outcome.

## References
- `BRD (4).md`, **Large Deal Approval Enhancement**, v1.0, especially BR-01–BR-23, NFR-01–NFR-06, RULE-01–RULE-09, OD-01–OD-09, and DEP-01–DEP-06.
- Salesforce Opportunity field `Total_Contract_Value__c`.
- Existing Deal Desk approval rule `Discount > 10%`.
