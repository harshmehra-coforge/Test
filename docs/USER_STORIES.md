# EPIC: Large Deal Approval Enhancement

## Feature 1: Parallel Finance Approval for High-TCV Opportunities

### User Story 1.1 (UI)
**As** a Sales Operations user,
**I want** the Opportunity approval process to trigger a parallel Finance approval when TCV > $500K,
**so that** Finance can review and approve large deals before closure.

**Acceptance Criteria / Scenarios:**
- Scenario 1 (Happy Path): Opportunity with TCV $600K triggers both Deal Desk and Finance approval in parallel; both approve, deal closes.
- Scenario 2 (Boundary): Opportunity with TCV exactly $500K triggers Finance approval.
- Scenario 3 (Error): If either Deal Desk or Finance rejects, deal returns to rep with combined comments.
- Scenario 4 (Re-submission): On resubmission, both approvals restart from scratch.

**Definition of Ready (DoR):**
- TCV field is reliably populated on Opportunity.
- Finance approvers (Anita, Ravi) are confirmed.
- Teams channel and email notification endpoints are available.

**Definition of Done (DoD):**
- Parallel approval logic is live in production.
- All scenarios tested (including rejections and resubmissions).
- Notifications sent as specified.

**Business Value:**
- Ensures Finance oversight on all large deals, preventing audit/compliance issues.

**Dependencies:**
- MS Teams connector for channel posts.
- Accurate TCV calculation from CPQ.

**Developer Persona:** UI

---

### User Story 1.2 (BE)
**As** a Salesforce backend developer,
**I want** to implement automation that triggers Finance approval for Opportunities with TCV > $500K, with internal routing based on TCV,
**so that** the correct Finance approver is assigned and approvals run in parallel with Deal Desk.

**Acceptance Criteria / Scenarios:**
- Scenario 1 (Happy Path): TCV $1M triggers Anita; TCV $2.5M triggers Ravi.
- Scenario 2 (Boundary): TCV exactly $2M routes to Ravi.
- Scenario 3 (Error): If TCV is missing, approval does not trigger and error is logged.
- Scenario 4 (Parallel): Both Deal Desk and Finance approvals can be in progress independently.

**DoR:**
- Approver assignments confirmed.
- CPQ populates TCV reliably.

**DoD:**
- Automation deployed and tested.
- Routing logic matches requirements.

**Business Value:**
- Prevents large deals from bypassing Finance.

**Dependencies:**
- CPQ integration.
- Approver directory.

**Developer Persona:** BE

---

### User Story 1.3 (QA)
**As** a QA engineer,
**I want** to validate that parallel approval and routing logic work as specified,
**so that** all edge cases and error scenarios are covered before go-live.

**Acceptance Criteria / Scenarios:**
- Scenario 1: Test all TCV thresholds and routing.
- Scenario 2: Simulate rejections and resubmissions.
- Scenario 3: Validate notifications (email + Teams).

**DoR:**
- Test environment with sample Opportunities.

**DoD:**
- All test cases pass.
- No regression on existing approval flows.

**Business Value:**
- Ensures robust, error-free deployment.

**Dependencies:**
- Test data.

**Developer Persona:** QA

---

## Feature 2: Notification and Reporting Enhancements

### User Story 2.1 (BE)
**As** a backend developer,
**I want** to send Finance approval notifications via email and Teams channel post when triggered,
**so that** the Finance team is promptly informed of pending approvals.

**Acceptance Criteria / Scenarios:**
- Scenario 1: Email sent to Finance approver when TCV > $500K.
- Scenario 2: Teams channel post created for each triggered approval.
- Scenario 3: On rejection, combined comments sent to rep.

**DoR:**
- Email and Teams endpoints available.

**DoD:**
- Notifications sent as specified.

**Business Value:**
- Improves Finance team awareness and response time.

**Dependencies:**
- Teams connector.

**Developer Persona:** BE

---

### User Story 2.2 (UI)
**As** a Sales Ops or Finance user,
**I want** to view a dashboard/report of deals in Finance approval, approval times, and rejection reasons,
**so that** I can monitor process efficiency and identify bottlenecks.

**Acceptance Criteria / Scenarios:**
- Scenario 1: Dashboard shows all deals in Finance queue.
- Scenario 2: Approval time by sub-tier (Director/CFO) is visible.
- Scenario 3: Rejection reasons are tracked and reportable.

**DoR:**
- Reporting requirements finalized by Kavita.

**DoD:**
- Dashboard live and accessible to both Finance and Sales Ops.

**Business Value:**
- Enables process improvement and audit readiness.

**Dependencies:**
- Salesforce reporting tools.

**Developer Persona:** UI

---

### User Story 2.3 (QA)
**As** a QA engineer,
**I want** to test notification delivery and dashboard/report accuracy,
**so that** all stakeholders receive timely and correct information.

**Acceptance Criteria / Scenarios:**
- Scenario 1: Simulate approval triggers and verify notifications.
- Scenario 2: Validate dashboard data matches approval history.

**DoR:**
- Test data and notification endpoints.

**DoD:**
- All notification and reporting tests pass.

**Business Value:**
- Ensures reliable communication and reporting.

**Dependencies:**
- Test environment.

**Developer Persona:** QA

---

## Feature 3: Renewal Auto-Approval Logic (Placeholder)

### User Story 3.1 (BE)
**As** a backend developer,
**I want** to propose and implement logic for auto-approving standard renewals with high TCV but no additional discount,
**so that** Finance is not burdened with unnecessary approvals.

**Acceptance Criteria / Scenarios:**
- Scenario 1: Renewal with TCV > $500K and 0% additional discount auto-approves.
- Scenario 2: Renewal with non-standard terms triggers Finance approval.

**DoR:**
- Renewal logic requirements finalized.

**DoD:**
- Logic implemented and tested.

**Business Value:**
- Reduces Finance workload on standard renewals.

**Dependencies:**
- Renewal identification in Opportunity data.

**Developer Persona:** BE

---

**References:**
- Meeting_Note_02_High_Value_Deal 2.md
