# Technical Specification Document (TSD)

## 1. Overview

This document specifies the technical system design for the Missed Dose Logging & Adherence Alerting solution within the CarePath Plus care management program, built on Salesforce Health Cloud. It addresses automated, structured missed dose entry, rolling adherence calculations, intelligent alerting with de-duplication, and compliance/audit controls for PHI. Requirements are sourced from Meeting_Note_03_Missed_Dose_Adherence.md, the BRD, and user stories.

## 2. Solution Context and Scope

The solution covers:
- Placement of a custom LWC (`doseLogQuickEntry`) widget on the Care Plan Lightning Record Page.
- Structured logging of missed doses, supporting tab-order, single-click entry, and rapid workflow.
- Server-side validation and audit logging on every missed dose log entry.
- Rolling-window adherence calculations (30/60/90 days) per patient, on-demand and scheduled.
- Record-triggered Flows for configurable, de-duplicated alert creation and daily scheduled recurrence.
- Enforcement of PHI controls and audit requirements.

Excluded: Patient-facing reminders, pharmacy reconciliation, override workflow automation (beyond manual), and cross-widget adherence integration (Phase 2+).

## 3. Architecture View

The system follows a microservices-inspired modularity within the limits of the Salesforce Health Cloud platform. Key modules/components:
- **LWC Frontend**: `doseLogQuickEntry` widget
- **Apex Service Class**: Backend insert, validation, audit logging, adherence calculation
- **Custom Objects**: `Dose_Log__c`, `PHI_Access_Log__c`, `Dose_Schedule__c`, standard `Task`
- **Flows**: Record-triggered for alerting, Scheduled for daily adherence reevaluation

![Architecture Diagram](https://arch-imgs.s3.amazonaws.com/scc-missed-dose-hld-v1.svg)

### Component Diagram (Mermaid)

### Component Diagram (Mermaid)

```
graph TD
  subgraph CarePlan Record Page
    A[LWC doseLogQuickEntry Widget] 
  end
  A --Log Entry--> B[Backend Apex Service (validate, insert Dose_Log__c, log PHI)]
  B --Write--> C[Dose_Log__c]
  B --Write Audit--> D[PHI_Access_Log__c]
  B --Fetch/Update--> E[Dose_Schedule__c]
  B --On Save: Calculate Adherence--> F[Adherence Calculation Engine]
  F --Trigger Thresholds--> G[Record-Triggered Flow: Alert Logic]
  G --Create Task/Comment--> H[Task]
  subgraph Scheduler
    I[Scheduled Flow Daily]
    I --Recalculate Rolling Adherence--> F
  end
  C --Triggers--> G
```

## 4. Logical Data Model

### Primary Objects & Fields
- **Dose_Log__c**
  - Patient (lookup Care Plan / Patient)
  - Dose Date/Time (datetime)
  - Medication (lookup, auto from Care Plan)
  - Reason (picklist; curated set)
  - Notes (text, 500 chars, flagged for PHI review)
  - CreatedBy, CreatedDate
  - LastModifiedBy, LastModifiedDate

- **Dose_Schedule__c**
  - Patient/Care Plan (lookup)
  - Scheduled Dose Datetimes

- **PHI_Access_Log__c**
  - User ID
  - Patient ID
  - Timestamp
  - Action (DOSE_LOG_ENTRY)

- **Task** (Standard, for alerting)
  - Related Case/Patient
  - Assigned to (Care Manager, Clinical Pharmacist)
  - Priority
  - Status
  - Comments
  - Created Date


## 5. Detailed Behaviors

### 5.1 Missed Dose Entry (Frontend & Backend)
- Widget launched with a single click, embedded on Care Plan record page.
- Tab-order accessible, mouse not required.
- Fields pre-filled (date/time defaults, medication)
- Backdating supported up to 14 days (UI + server-side enforcement)
- On submit, invokes Apex insert logic.
- Backend checks:
  - Date not older than 14 days
  - All required fields present
- Audit log written to PHI_Access_Log__c (includes all required details). Any audit write failure aborts save.
- Editable within 24 hours by entry owner; after 24h, read-only except for supervisor via manual override.

### 5.2 On-Demand & Scheduled Adherence Calculation
- On widget render or missed dose log, system invokes adherence calculation in Apex.
- For each window (30/60/90 days):
  - Scheduled doses = count in Dose_Schedule__c window
  - Missed doses = count in Dose_Log__c window
  - Adherence % = (scheduled − missed) / scheduled × 100; handles 0 scheduled dose edge case.
- Scheduled Flow (6:30 AM IST):
  - Selects patients with at least one Dose_Log__c in past 45 days
  - Recalculates 30-day adherence
  - Triggers alerts if thresholds are breached

### 5.3 Alerting & Task Creation (Flows)
- On Dose_Log__c after-insert record-triggered Flow evaluates all thresholds:
  - Rolling 30d <80% → Task to care manager (Normal)
  - 3+ consecutive missed in 7d → Task (High) to care manager
  - Rolling 30d <60% → Task (High) to Dr. Kunal; duplicate for care manager
- De-duplication handled within Flow:
  - Query open Tasks for patient/threshold in past 24 hours
  - If open Task exists, comment on it; else, create new Task
  - Only one Task per patient per 24 hours; highest-severity threshold determines priority/target

### 5.4 Compliance & Audit Controls
- All Dose_Log__c inserts are auditable via PHI_Access_Log__c log entry, enforced at backend
- Reason field must use curated picklist
- Notes field flagged for quarterly PHI review (not enforced in Phase 1 but flagged in reporting/views)
- Server-side and UI controls block backdating beyond 14 days
- Entry edits allowed by owner within 24h only; supervisor manual override allowed per SOP (outside scope for Phase 1)

## 6. Non-Functional Requirements (NFRs)

| Property           | Requirement                                   |
|--------------------|-----------------------------------------------|
| Entry Workflow     | ≤ 15 seconds per missed dose log (measured UAT) |
| Adherence Calc     | ≤ 300ms per patient per recalculation          |
| Widget Uptime      | ≥ 99.9% during working hours                   |
| Alert De-dupe      | No more than 1 alert Task per patient/24h      |
| Compliance         | 100% of records with complete PHI audit trail  |
| Accessibility      | WCAG 2.1 AA, tab-order, screen reader support  |
| Security           | 100% RBAC (platform), PHI encrypted, zero trust via platform config |
| Cost               | On-platform, no incremental license/infra cost |

## 7. Security & Compliance

- All PHI data subject to org platform encryption (Salesforce Shield/Field Level Encryption)
- All `Dose_Log__c` events log to `PHI_Access_Log__c` — logs are non-editable and reviewed quarterly
- Notes field monitored for PHI leakage; quarterly flagged entries sent to Compliance
- Only assigned care managers, clinical pharmacists, and compliance reviewers (RBAC) can create or access logs/tasks
- User edit restrictions enforced at platform & Apex layer
- All cross-object DML operations routed via platform transaction to ensure atomicity and audit consistency

## 8. Operationalization, Logging, Monitoring

- Platform Event or Apex helper/method pattern for PHI log writes (to be confirmed under technical spike)
- All errors (adherence calc, audit log, task creation, flow condition violation) emit error logs to Health Cloud monitoring dashboard
- Pass/fail metrics on all scheduled job runs with notification to admin email group on error
- Scheduled job execution monitored (success, failures, skipped/late runs)
- User interaction events (widget use, field errors, edit attempts) surfaced in standard Health Cloud event log for UAT/QA insight

## 9. Alternatives and Rationale

- **Apex Trigger vs. Flow**: Chose record-triggered Flow for post-log alerting to maximize configuration transparency and admin maintainability; Flow supports de-duplication logic with Get Records + Decision—no Apex trigger required. Would revisit for high-scale eventing or where Flow logic is insufficient.
- **Widget Scope**: Single-click, on-record-page entry as opposed to multi-step workflow or external form. Ensures engagement, proven in UAT and accessibility reviews.
- **Scheduled Flow vs. Batch Apex**: Chose Flow for daily reevaluation due to easier admin support for timing changes, error handling, and reporting in Health Cloud context. Batch Apex reserved for potential Phase 2 at significantly larger scale.
- **Field Validations**: Both UI and server enforce 14-day backdate window and edit-after-24h lock. Dual enforcement to minimize accidental non-compliance.
- **Picklist Reasoning**: Curated reason limits reduce PHI leakage risk and streamline quarterly review compared to free-text rationale field.

## 10. Implementation Guidance

- Confirm Dose_Log__c and Dose_Schedule__c schema; extend objects only per minimum necessary for functional/audit requirements.
- Leverage Lightning Data Service in LWC for performant field prefill and state management.
- Use Apex custom service to encapsulate insert, validation, adherence calculation, and PHI log logic.
- Maintain all error and audit logs at platform level; use standard Health Cloud mechanisms for notification and monitoring.
- Engage UAT care managers early for widget feedback on real-case scenarios and accessibility.
- Strict tab order and accessibility test coverage (screen reader, keyboard-only nav, contrast checks).

## 11. Open Questions & Action Items

- Platform event vs. helper class pattern for PHI_Access_Log__c — confirm standard with Salesforce tech team (see Karan’s action item).
- Final Reason picklist values pending clinical/pharmacovigilance confirmation (Dr. Kunal, due 25 July).
- Supervisor override workflow for post-24h edits is manual in Phase 1 but requires SOP documentation (future automation optional).
- Technical team to verify Dose_Log__c, Dose_Schedule__c, PHI_Access_Log__c current fields; schema adjustment only as required by BRD.
- UAT care manager nominations and UX validation session scheduling (see Priti/Aditi’s actions).

## 12. References

- [Meeting_Note_03_Missed_Dose_Adherence.md]
- [BA-dynamic-agent/BRD.md]
- [PO-dynamic-agent/USER_STORIES.md]
- Salesforce Health Cloud documentation (org-specific)
- Patient Self-Enrollment TDD; Benefit Summary Widget specification
