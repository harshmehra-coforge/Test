# Technical Specification Document
## Missed Dose Logging and Adherence Alerts

**Organization:** CarePath Plus  
**Source:** Meeting_Note_03_Missed_Dose_Adherence.md and upstream BRD/PO artifacts  
**Date:** 2026-10-26  
**Version:** 1.0  
**Status:** Implementation baseline for Phase 1  
**Architecture mode:** STANDARD  

## 1. Executive Summary

### 1.1 Purpose
CarePath Plus will replace free-text missed-dose notes with structured capture on the Health Cloud Care Plan record page. The capability will provide a rapid `doseLogQuickEntry` Lightning Web Component (LWC), server-side validation and persistence through Apex, patient-level rolling adherence calculations, threshold-based Salesforce Tasks, alert de-duplication, daily deterioration evaluation, and PHI audit logging.

The design addresses the current 3–4 week detection delay and eliminates manual spreadsheet calculations for program reviews. The primary entry path is optimized for completion in under 15 seconds without leaving the Care Plan or Case work surface.

### 1.2 Scope

**In scope:**
- Custom LWC embedded on the Care Plan Lightning Record Page.
- Authorized entry by care managers and the assigned pharmacist.
- Structured `Dose_Log__c` creation with medication derived from the active Care Plan.
- 14-day back-date validation and 24-hour creator edit policy.
- Rolling 30-, 60-, and 90-day adherence calculation.
- Post-insert threshold evaluation and Task routing.
- Daily 06:30 IST deterioration evaluation for patients with a dose log in the preceding 45 days.
- PHI audit records through `PHI_Access_Log__c`.

**Out of scope for Phase 1:**
- SMS or portal outreach.
- Pharmacy dispense reconciliation.
- Hospitalization-driven adherence pauses.
- Supervisor override workflow for edits after 24 hours.
- Cross-widget signals with Financial Assistance or Refill workstreams.
- Population-wide recalculation on widget render or individual log insertion.

### 1.3 Key Decisions

1. **Salesforce-native implementation:** LWC, Apex, Salesforce Flow, Care Plans, Cases, Tasks, and custom objects are used instead of introducing an external microservice. This minimizes PHI replication, deployment complexity, and integration latency.
2. **Apex service boundary for writes and calculations:** All inserts, edits, validation, audit writes, and patient-level calculations are centralized in Apex. This provides transaction control and consistent enforcement beyond client-side validation.
3. **Flow for alert orchestration:** An after-save record-triggered Flow handles threshold evaluation, Task lookup, de-duplication, routing, and comments. This follows the meeting decision and avoids an Apex trigger while retaining declarative maintainability.
4. **On-demand patient scope:** Calculations query only the requested Care Plan/patient and rolling window. The design avoids a full-population recalculation and supports the approximately 4,200 active-patient population.
5. **Structured reason taxonomy:** Reason is a controlled picklist; Notes remains optional and limited to 500 characters with quarterly PHI review. This reduces uncontrolled PHI capture while preserving operational context.

## 2. Requirements Baseline

### 2.1 Functional Requirements

| ID | Requirement | Design response |
|---|---|---|
| FR-01 | Open quick entry with one click from Care Plan | LWC action/card on the Care Plan record page; modal or inline panel; no navigation |
| FR-02 | Capture dose date/time | Defaults to current Salesforce user/org time context; permits dates no older than 14 days |
| FR-03 | Auto-populate medication | Apex resolves the active Care Plan medication; client cannot substitute an unmanaged medication |
| FR-04 | Capture reason and notes | Controlled Reason values; optional Notes capped at 500 characters |
| FR-05 | Persist structured log | `DoseLogService.createDoseLog` validates and inserts `Dose_Log__c` atomically |
| FR-06 | Calculate adherence | Apex returns 30-, 60-, and 90-day values from `Dose_Schedule__c` and `Dose_Log__c` |
| FR-07 | Evaluate thresholds | After-save Flow evaluates below-80%, below-60%, and 3+ consecutive misses in 7 days |
| FR-08 | Route Tasks | Tasks link to Case and patient and route to primary care manager and/or clinical pharmacist |
| FR-09 | Suppress duplicates | Patient-wide 24-hour suppression and same-threshold open-Task comment behavior |
| FR-10 | Daily re-evaluation | Scheduled Flow runs at 06:30 IST for patients with logs in the preceding 45 days |
| FR-11 | Audit writes | Every successful log insertion writes `DOSE_LOG_ENTRY` to `PHI_Access_Log__c` |
| FR-12 | Controlled editing | Creator may edit own entry for less than 24 hours; later edits are read-only in Phase 1 |

### 2.2 Non-Functional Requirements

| Metric | Target | Measurement |
|---|---:|---|
| Normal entry completion | <15 seconds | UX timing with six care managers across both hubs |
| Quick-entry server response | p95 <1,000 ms; p99 <2,000 ms | Salesforce transaction and browser telemetry |
| Adherence calculation | p95 <2,000 ms; p99 <4,000 ms | Apex execution logs and LWC performance instrumentation |
| Post-log alert completion | 95% of eligible evaluations finish <60 seconds | Flow interview/runtime monitoring |
| Availability | 99.9% monthly during Salesforce service availability | Salesforce status and synthetic Care Plan checks |
| Audit completeness | 100% of committed log inserts have an audit record | Reconciliation report, daily |
| Duplicate-task prevention | ≥99.9% duplicate suppression in concurrency test | Automated integration test and exception report |
| Daily evaluation completion | 100% of eligible records processed or exception-recorded by 07:30 IST | Scheduled Flow run report |
| Accessibility | All controls keyboard reachable; WCAG 2.1 AA-aligned labels and focus behavior | Accessibility review and UAT |
| Data retention | Follow existing CarePath Plus Salesforce PHI retention policy; audit records retained at least 7 years | Compliance configuration review |

## 3. Actors and Permissions

| Actor | Allowed operations |
|---|---|
| Care manager | Create a log for an assigned/authorized Care Plan; edit own log within 24 hours; view adherence and related Tasks according to Salesforce sharing |
| Assigned pharmacist | Create a log for the assigned Care Plan; view adherence; no cross-patient creation without sharing permission |
| Clinical pharmacist | Receive high-severity clinical-review Tasks; access only permitted patient records |
| Supervisor | No special edit override in Phase 1; uses approved administrative process if correction is needed |
| Salesforce administrator | Configuration, controlled manual correction after 24 hours, monitoring, and break-glass support with auditability |
| Scheduled Flow | Daily evaluation under an integration/automated-process context with least-privilege access |

Authorization is enforced at both the LWC entry point and Apex service boundary. The Apex service must use sharing-aware queries and explicit object/field security checks. Client-supplied Care Plan, medication, patient, or owner identifiers are never trusted without server-side resolution.

## 4. System Architecture

### 4.1 Component Architecture

The design uses four implementation layers within Salesforce:

1. **Presentation:** `doseLogQuickEntry` LWC on the Care Plan Lightning Record Page. It owns focus management, field rendering, client validation, loading states, and accessible error/success messaging.
2. **Application services:** Apex `DoseLogService` handles create/update operations, authorization, server validation, medication resolution, audit coordination, and idempotency. Apex `AdherenceService` calculates rolling adherence and consecutive missed-dose state for one Care Plan/patient.
3. **Automation:** An after-save Flow on `Dose_Log__c` evaluates thresholds and creates/comments Tasks. A scheduled Flow at 06:30 IST evaluates eligible recent-log patients.
4. **Salesforce data:** Care Plan, Case, `Dose_Schedule__c`, `Dose_Log__c`, `PHI_Access_Log__c`, and Task records.

### 4.2 Logical Architecture

The implementation follows this runtime relationship:

`Care Plan record page → LWC → Apex service → Dose_Log__c + PHI_Access_Log__c → after-save Flow → adherence evaluation → Task de-duplication and routing`.

The LWC may request a read-only adherence summary independently on render. The calculation service never performs a population scan. The scheduled Flow first identifies distinct patients with a dose log in the inclusive prior 45-day selection window, then invokes the same patient-level evaluation and de-duplication logic.

### 4.3 Component Responsibilities

| Component | Responsibility | Failure behavior |
|---|---|---|
| `doseLogQuickEntry` LWC | Render form, defaults, keyboard flow, client validation, submit, result messaging | Preserve entered values, show actionable message, do not create partial record |
| `DoseLogService` Apex | Authorization, active Care Plan/medication resolution, validation, insert/update, audit | Atomic rollback on validation/audit failure; safe typed error to LWC |
| `AdherenceService` Apex | Derive scheduled and missed doses, calculate windows and consecutive rule | Return controlled error or `notApplicable` when schedule denominator is zero; no fabricated value |
| `Dose_Log__c` | System-of-record for structured missed-dose events | DML only through approved service for UI-originated writes |
| After-save Flow | Evaluate thresholds, select highest severity, query Tasks, create or comment | Record fault path and admin-visible exception logging |
| Scheduled Flow | Daily patient selection and evaluation | Continue other patients and record individual failures |
| `PHI_Access_Log__c` | Audit actor, patient, timestamp, and action | Audit write is part of the commit policy; no silent unaudited success |
| Salesforce Task | Work item for care manager and clinical pharmacist | No arbitrary owner; unresolved assignment becomes an operational exception |

### 4.4 Architecture Diagram

![System Architecture Diagram](https://architect-diagrams.invalid/missed-dose-system-architecture.svg)

> The diagram represents the validated architecture flow: LWC → Apex services → structured records/audit → Flow evaluation → Tasks, with scheduled Flow sharing the evaluation path.

## 5. Primary System Flows

### 5.1 Missed-Dose Creation

1. User opens quick entry from the Care Plan page; focus moves to the first field.
2. LWC loads active Care Plan medication and current adherence summary.
3. User confirms date/time, selects Reason, optionally enters Notes, and submits.
4. LWC performs immediate validation for required context, 14-day date limit, and 500-character Notes limit.
5. Apex re-resolves Care Plan, patient, medication, and authorization; rejects manipulated or stale identifiers.
6. Apex validates Reason against active metadata/configuration and validates the timestamp in Salesforce server time.
7. Apex inserts `Dose_Log__c` and creates `PHI_Access_Log__c` with `DOSE_LOG_ENTRY` in the same transaction or through the approved transactional audit pattern.
8. Apex returns the created record identifier and adherence summary; LWC displays success without navigation.
9. The after-save Flow evaluates thresholds asynchronously/declaratively and applies Task rules.

### 5.2 Adherence Calculation

For each rolling window `W ∈ {30, 60, 90}`:

`adherencePercent(W) = ((scheduledDoseCount(W) - missedDoseCount(W)) / scheduledDoseCount(W)) × 100`

The window is defined as `[now - W days, now]` using the CarePath Plus operating timezone, IST, after conversion to Salesforce `DateTime`. A dose log is counted only when it is linked to the active Care Plan/patient and its dose timestamp falls in the window. Duplicate dose logs are excluded using the configured idempotency/submission key; distinct clinically recorded misses remain countable.

When `scheduledDoseCount(W) = 0`, the API returns `null` with status `NOT_APPLICABLE`; the UI displays “Not available—no scheduled doses” and no percentage threshold is evaluated for that window.

### 5.3 Threshold and Task Flow

Thresholds are evaluated in severity order:

1. **Critical clinical condition:** 30-day adherence `<60%`; High priority; clinical pharmacist Task plus care-manager copy.
2. **High operational condition:** 3 or more consecutive missed doses within 7 days; High priority; primary care manager.
3. **Normal operational condition:** 30-day adherence `<80%`; Normal priority; primary care manager.

“Below” is strict: exactly 80% and exactly 60% do not breach their corresponding rules. If multiple rules breach, the highest-severity outcome governs. The consecutive rule uses scheduled-dose sequence order, not arbitrary record creation order; the implementation must identify the last three scheduled opportunities and verify each is missed within a seven-day timestamp span.

For each eligible outcome, Flow:
- Queries recent adherence Tasks for the patient using a stable threshold type and creation timestamp.
- If any adherence Task for the patient was created in the preceding 24 hours, suppresses new Task creation and retains the highest-severity context in the existing work item/comment path.
- Otherwise, if an open Task exists for the same patient and threshold type, adds a comment/update to that Task rather than creating a duplicate.
- Otherwise creates the required Task(s), links them to the Case and patient, assigns approved owners, and sets priority.

### 5.4 Daily Deterioration Flow

At 06:30 IST, the scheduled Flow selects distinct patients with at least one `Dose_Log__c` whose dose timestamp is within the preceding 45 days. It evaluates each patient’s 30-day adherence and the same threshold/de-duplication path used after insert. A fault on one patient is written to an exception record/log and does not stop processing of other patients. The run must finish or record all eligible outcomes by 07:30 IST.

## 6. API and Service Contracts

### 6.1 LWC-to-Apex Operations

| Operation | Input | Output | Security |
|---|---|---|---|
| `getEntryContext(carePlanId)` | Care Plan ID | Patient context, active medication, allowed reasons, edit capability | Sharing-aware read and FLS |
| `getAdherence(carePlanId)` | Care Plan ID | 30/60/90 values, denominator counts, calculation timestamp, status | Sharing-aware read |
| `createDoseLog(request)` | Care Plan ID, dose timestamp, Reason, Notes, client request ID | Dose Log ID, audit status, adherence summary | Authorized actor; server validation |
| `updateDoseLog(request)` | Dose Log ID, editable fields, client request ID | Updated record and audit status | Creator and `<24h` check; server validation |

Logical request shape:

```json
{
  "carePlanId": "a-care-plan-id",
  "doseDateTime": "2026-07-22T11:00:00+05:30",
  "reason": "Forgot",
  "notes": "Optional operational context",
  "clientRequestId": "uuid-generated-by-client"
}
```

The service must reject unknown fields, invalid picklist values, missing active medication, stale Care Plan status, timestamps older than 14 days, and Notes longer than 500 characters. The client request ID is stored or deterministically checked so a retry after an uncertain response cannot create an unintended duplicate.

### 6.2 Adherence Response

```json
{
  "carePlanId": "a-care-plan-id",
  "calculatedAt": "2026-07-22T11:00:00+05:30",
  "windows": {
    "30": {"scheduled": 25, "missed": 5, "percent": 80.0, "status": "AVAILABLE"},
    "60": {"scheduled": 50, "missed": 7, "percent": 86.0, "status": "AVAILABLE"},
    "90": {"scheduled": 75, "missed": 8, "percent": 89.33, "status": "AVAILABLE"}
  }
}
```

### 6.3 Error Contract

Errors use stable categories rather than Salesforce internal exception text: `UNAUTHORIZED`, `INVALID_DATE`, `INVALID_REASON`, `NOTES_TOO_LONG`, `MISSING_MEDICATION`, `SCHEDULE_UNAVAILABLE`, `DUPLICATE_REQUEST`, `AUDIT_FAILURE`, and `SYSTEM_ERROR`. User messages must not expose PHI, object internals, or stack traces.

## 7. Data Architecture

### 7.1 Data Model

| Entity | Key relationships | Required fields and rules |
|---|---|---|
| Care Plan | Patient; Case/work surface; active medication; schedule | Must be active and readable by actor; supplies medication and primary care manager |
| `Dose_Log__c` | Care Plan; patient; medication; creator | Dose date/time, Reason, Notes, creator, created timestamp, client request ID/idempotency key |
| `Dose_Schedule__c` | Care Plan | Schedule frequency/definition sufficient to derive scheduled opportunities |
| `PHI_Access_Log__c` | Patient and actor | User ID, patient ID, timestamp, `DOSE_LOG_ENTRY`; follows existing schema |
| Task | Case and patient | Threshold type, priority, owner, status, created timestamp, subject/context |

![Data Model Diagram](https://architect-diagrams.invalid/missed-dose-data-model.svg)

### 7.2 Persistence and Query Strategy

- `Dose_Log__c` is the authoritative event record; entries are not embedded in Case free text.
- Queries are scoped by Care Plan/patient and bounded by the requested rolling window.
- Index/selectivity review is required for dose timestamp, Care Plan, patient, and client request ID fields in the Salesforce org.
- The implementation uses bulk-safe Apex even though the UI creates one record at a time, because scheduled evaluation and administrative operations may process collections.
- No external database or cache is introduced. Salesforce is the system of record, reducing PHI duplication and avoiding synchronization failures.
- The existing `PHI_Access_Log__c` write pattern must be reused after technical confirmation. The service must not silently commit a dose log without the required audit outcome.

### 7.3 Data Quality Rules

1. Medication is derived server-side from the active Care Plan.
2. Reason is validated against the approved controlled set.
3. Notes length is enforced at 500 characters in both LWC and Apex.
4. Dose timestamp cannot be older than 14 days at submission.
5. Creator edits are allowed only for the creator and only before 24 elapsed hours.
6. Threshold calculations use dose date/time, not record creation time.
7. Null/zero scheduled-dose denominators produce `NOT_APPLICABLE`, never zero-percent inference.
8. Patient and Case relationships are mandatory for Task creation; failures are recorded rather than assigned to arbitrary users.

## 8. Salesforce Automation Design

### 8.1 `Dose_Log__c` After-Save Flow

**Trigger:** After record creation; update path is disabled for threshold re-evaluation unless a controlled business decision later requires it.  
**Entry condition:** Valid `Dose_Log__c` linked to an active Care Plan.  
**Actions:**
1. Resolve patient, Case, primary care manager, and clinical pharmacist target.
2. Invoke the reusable adherence evaluation action/service for the patient.
3. Evaluate strict threshold rules.
4. Set one highest-severity outcome.
5. Get recent/open Tasks by patient and threshold type.
6. Apply 24-hour patient suppression and same-threshold open-Task comment logic.
7. Create required Task(s) or update/comment on existing Task.
8. Send fault output to an operational exception mechanism.

The Flow must be bulk-safe and avoid unbounded nested queries or repeated record updates. Where a Flow cannot safely express a reusable calculation without governor-limit risk, it invokes the Apex `AdherenceService` as an invocable action; this preserves the no-trigger decision while keeping calculation logic testable and centralized.

### 8.2 Scheduled Flow

**Schedule:** Daily at 06:30 IST.  
**Selection:** Distinct patients with a dose log in the preceding 45 days.  
**Processing:** Evaluate one patient at a time through the same threshold/de-duplication action.  
**Completion target:** Process or exception-record all eligible patients by 07:30 IST.  
**Observability:** Store run identifier, eligible count, processed count, success count, failure count, and failure identifiers.

### 8.3 Task Templates

| Threshold type | Subject | Owner | Priority | Required context |
|---|---|---|---|---|
| `ADHERENCE_LT_80` | Review declining medication adherence | Primary care manager | Normal | 30-day percentage, window, last dose-log timestamp, Care Plan, Case |
| `CONSECUTIVE_MISSED_3_7D` | Review consecutive missed doses | Primary care manager | High | Missed-dose timestamps, medication, 7-day range, Case |
| `ADHERENCE_LT_60` | Clinical review required for low adherence | Dr. Kunal Bhatt/approved clinical pharmacist | High | 30-day percentage, missed/scheduled counts, Care Plan, Case |
| `ADHERENCE_LT_60_COPY` | Care-manager follow-up for low adherence | Primary care manager | High | Same clinical context and linked clinical-review Task |

## 9. Security, Privacy, and Compliance

### 9.1 Controls

- Enforce Salesforce object permissions, field-level security, sharing rules, and Care Plan/Case access for both LWC and Apex.
- Use `with sharing` service classes and explicit authorization checks for actor-to-Care-Plan assignment.
- Never accept patient, medication, or owner identifiers as authoritative from the browser.
- Protect all Salesforce traffic with platform-standard TLS; do not export PHI to browser storage, logs, analytics vendors, or external queues.
- Redact Notes and patient identifiers from debug logs and Flow fault emails.
- Audit every successful insertion with actor, patient, timestamp, and `DOSE_LOG_ENTRY`.
- Include Notes in the quarterly PHI scrub review; retain curated Reason values to reduce leakage.
- Apply least privilege to scheduled automation and administrator break-glass access.

### 9.2 Audit Failure Policy

The preferred policy is atomic success: if the required audit record cannot be written, the dose-log transaction rolls back and the LWC receives an actionable retry message. If the existing approved audit pattern is asynchronous, the service must use the established durable mechanism and create a monitored reconciliation exception; an unaudited committed log is not an acceptable silent outcome.

### 9.3 Editing and Auditability

A creator update within 24 hours must re-run server validation. The implementation must preserve original creation metadata and record the update according to the existing audit convention. After 24 hours, the LWC exposes read-only state. Administrator manual DML remains the Phase 1 correction path and must be performed under existing administrative audit controls.

## 10. Deployment and Environments

### 10.1 Salesforce Metadata

| Metadata | Deployment unit |
|---|---|
| `doseLogQuickEntry` LWC | Component bundle, labels, accessibility metadata |
| Apex services | `DoseLogService`, `AdherenceService`, DTOs, selectors, tests |
| Custom object metadata | Confirmed/extended `Dose_Log__c` fields and indexes |
| Flow metadata | After-save `Dose_Log__c` Flow and 06:30 IST scheduled Flow |
| Permission sets | Care manager logger and assigned pharmacist logger permissions |
| Custom metadata | Reason values, threshold definitions, severity, owner routing, suppression windows |
| Reports/monitoring | Daily evaluation summary, audit reconciliation, Flow faults |

Thresholds and recipients should be represented in Custom Metadata where practical, while clinical governance retains approval over values. The initial values are 80%, 60%, 3 consecutive misses, 7 days, 24 hours, 14 days, and 45 days.

### 10.2 Release Pipeline

1. Develop in a Salesforce DX source-controlled branch.
2. Run Apex unit tests, LWC Jest tests, static analysis, metadata validation, and security checks.
3. Deploy to integration/sandbox with representative Care Plan, schedule, Case, and Task fixtures.
4. Execute contract, negative, concurrency, Flow, and accessibility tests.
5. Deploy to UAT before 19 August 2026 and run the six-care-manager UX validation.
6. Obtain clinical, compliance, sponsor, and Steering Committee approvals.
7. Deploy through the Benefit Summary release train with a rollback package containing prior LWC, Apex, and Flow metadata.

## 11. Observability and Operations

### 11.1 Operational Metrics

- Quick-entry invocation count, submit success rate, validation rejection rate, and p50/p95/p99 response time.
- Dose-log insert count by hub, actor role, Reason, and Care Plan cohort.
- Audit write success/failure and daily log-to-audit reconciliation count.
- Adherence calculation duration and `NOT_APPLICABLE` rate.
- Flow interview success/fault counts, Task creation count, comment/update count, and suppression count.
- Daily scheduled run eligible/processed/succeeded/failed counts and completion time.
- Open adherence Tasks by threshold, owner, age, and hub.

### 11.2 Alerts and Runbooks

| Alert | Threshold | Severity | Response |
|---|---|---|---|
| Audit reconciliation gap | Any committed dose log without audit record in daily report | Critical | Stop release, investigate audit pattern, reconcile before clinical use |
| Apex service error | >2% of create requests over 15 minutes | High | Review validation/schema/security faults; notify Salesforce on-call |
| Flow fault rate | >1% of eligible evaluations in a run | High | Inspect failed patient IDs and relationships; replay after correction |
| Scheduled run overdue | Not complete by 07:30 IST | High | Inspect Flow limits and Salesforce status; execute controlled replay |
| Duplicate-task exception | >0.1% in validation report | High | Review concurrency/idempotency and Task query criteria |
| Entry latency | p95 >1,000 ms for 15 minutes | Medium | Inspect Apex queries, Flow coupling, and org limits |

## 12. Testing and Verification

### 12.1 Test Scope

- Apex unit tests for authorization, date boundaries, Reason validation, Notes length, medication resolution, idempotency, audit rollback, edit window, and calculation math.
- LWC Jest tests for default, populated, back-date, error, loading, success, cancel, focus, and keyboard tab order.
- Flow tests for all thresholds, exact 80%/60% boundaries, 24-hour suppression, open matching Task comments, closed Task behavior, dual-task clinical routing, and fault paths.
- Integration tests with realistic Care Plan, `Dose_Schedule__c`, `Dose_Log__c`, Case, and Task data.
- Concurrency test with repeated client requests and simultaneous Flow evaluations.
- Accessibility review aligned to WCAG 2.1 AA labels, focus order, keyboard operation, and error announcements.
- Performance test with at least 2x expected daily peak entry/evaluation volume and approximately 4,200 active patients represented in scheduled selection tests.
- UAT with three care managers per hub during the week of 4 August and completion target of 19 August 2026.

### 12.2 Acceptance Gates

The release is not ready until: 100% of committed inserts reconcile to audit records; no critical security or PHI defects remain; all threshold boundary tests pass; duplicate-task prevention meets 99.9%; daily evaluation meets the 07:30 IST completion target; and six care managers confirm the normal entry path is usable within 15 seconds.

## 13. Capacity, Performance, and Cost

### 13.1 Capacity Model

The initial operating population is approximately 4,200 active patients across two hubs, with an average care-manager caseload of 165. The system is designed for 250 dose-log submissions per hour sustained across the organization, 500 submissions per hour peak for 15 minutes, and 4,200 daily scheduled eligibility checks. These targets provide more than 2x headroom over the stated operating population and support release-train growth without introducing an external platform.

Salesforce governor limits are the primary capacity constraint. The implementation must use selective queries, bulk-safe service methods, one adherence calculation per patient evaluation, and bounded Task lookups. Flow interviews must not repeatedly retrieve the same records. Load tests must verify limits in a full-copy or representative sandbox before production activation.

### 13.2 Performance Decisions

- Keep the synchronous create path limited to validation, one log insert, and required audit write; return the user response without waiting for unrelated reporting work.
- Keep adherence calculation patient-scoped and window-bounded.
- Let Flow process alert work after save; use invocable Apex only for reusable, bulk-safe calculations.
- Use client request IDs to eliminate retries rather than performing expensive duplicate searches.
- Avoid a cache in Phase 1 because the data is highly patient-specific, stale adherence would create clinical risk, and Salesforce-native query performance is sufficient at the forecast scale.

### 13.3 Cost Model

No new cloud subscription is required. Estimated incremental monthly platform cost is **$0–$1,500 USD**, depending on existing Salesforce licensing, sandbox tier, and any additional automation/storage entitlements. The selected Salesforce-native design is more cost-efficient than introducing Azure Functions, Cosmos DB, or an external event broker because it avoids PHI data replication, network integration, and another production support boundary. The trade-off is dependence on Salesforce governor limits and Flow execution capacity.

## 14. Disaster Recovery and Continuity

Salesforce platform backup, recycle-bin, sandbox refresh, and the organization’s existing data recovery controls remain authoritative. The solution adds:

- Metadata versioning and rollback package for LWC, Apex, Flow, permission sets, and Custom Metadata.
- Daily reconciliation report for `Dose_Log__c` versus `PHI_Access_Log__c`.
- Exported configuration snapshot for threshold values and routing metadata.
- Controlled replay procedure for failed scheduled evaluations after resolving the underlying data or Flow fault.

Targets for the capability are **RTO ≤4 hours** for a Salesforce org-level recovery event and **RPO ≤24 hours** for recoverable configuration/reporting state, subject to the enterprise Salesforce backup service. A single Flow or Apex failure must not delete committed dose logs; the exception process supports replay.

## 15. Implementation Roadmap

| Phase | Target window | Deliverables | Exit criteria |
|---|---|---|---|
| Schema and clinical confirmation | 24–29 Jul 2026 | Object mapping, audit pattern, Reason decision, relationships | Approved field/API mapping and clinical taxonomy |
| Foundation and service layer | 30 Jul–5 Aug 2026 | LWC shell, Apex DTOs/services, permissions, validation, audit integration | Unit tests pass; security review complete |
| Calculation and automation | 6–12 Aug 2026 | Adherence service, after-save Flow, scheduled Flow, Task templates, de-duplication | Threshold, boundary, and Flow fault tests pass |
| UX validation and UAT | 13–19 Aug 2026 | Six-user validation, accessibility, integration/performance testing | No critical defects; UAT evidence complete by 19 Aug |
| Governance and release | 20 Aug–end Q3 FY26 | Steering approval, release-train deployment, monitoring/runbooks | Go-live checklist approved and rollback package ready |

## 16. Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| `Dose_Schedule__c` cannot derive scheduled opportunities | Incorrect adherence and alerts | Confirm schema and schedule semantics before implementation; block release if denominator cannot be trusted |
| Flow governor limits under daily evaluation | Missed alerts | Select distinct patients, bulk-safe invocable calculation, limit queries, measure full-copy sandbox runtime |
| Race conditions create duplicate Tasks | Alert noise and clinical confusion | Stable threshold keys, recent/open Task query, request idempotency, concurrency tests, reconciliation report |
| Audit pattern is asynchronous or unavailable | Compliance gap | Reuse approved pattern; transactional rollback or durable monitored reconciliation; no silent success |
| Users exceed 15 seconds and revert to notes | Structured data adoption failure | Single-click launch, medication auto-population, focus control, six-user timed validation |
| Clinical interpretation of “Adverse event” remains unresolved | Inconsistent Reason data | Finalize with clinical team before metadata deployment |
| Missing Case/owner relationships | Misrouted clinical work | Explicit fault path, no arbitrary assignment, pre-release data quality report |

## 17. Decisions and Deferred Phase 2 Items

### 17.1 Confirmed Phase 1 Decisions

- Salesforce-native LWC plus Apex and Flow implementation.
- `Dose_Log__c` is the structured source of truth.
- Rolling windows are 30, 60, and 90 days.
- Strict threshold semantics apply below 80% and below 60%.
- No more than one adherence Task per patient per 24 hours.
- Existing open same-threshold Tasks receive comments rather than duplicates.
- Daily evaluation runs at 06:30 IST for patients with logs in the preceding 45 days.
- No Apex trigger is used for alerting.
- Patient outreach, refill reconciliation, hospitalization pauses, and cross-widget signals remain excluded.

### 17.2 Deferred Phase 2

- SMS, WhatsApp, or portal patient outreach.
- Hospitalization pause and schedule adjustment.
- Pharmacy dispense reconciliation integration.
- Supervisor override workflow for post-24-hour edits.
- Cross-workstream adherence signals and population analytics.

## 18. Glossary

| Term | Definition |
|---|---|
| Care Plan | Health Cloud patient program record and primary work surface for this capability |
| Dose Log | Structured `Dose_Log__c` record representing a reported missed dose |
| Rolling window | Time interval ending at the calculation timestamp, measured over 30, 60, or 90 days |
| Adherence | Scheduled doses minus missed doses, divided by scheduled doses, expressed as a percentage |
| Threshold type | Stable identifier for an alert condition used in Task de-duplication |
| PHI | Protected health information |
| LWC | Salesforce Lightning Web Component |
| Flow | Salesforce declarative automation runtime |
| IST | India Standard Time, UTC+05:30 |

## 19. Traceability

| Source requirement group | TSD coverage |
|---|---|
| BR-01–BR-13 quick entry | Sections 2, 4, 5, 6, 9, 12 |
| BR-14–BR-20 adherence | Sections 2, 5, 6, 7, 13 |
| BR-21–BR-29 alerting and daily evaluation | Sections 5, 8, 11, 12, 13 |
| BR-30–BR-37 audit, security, editing | Sections 3, 7, 9, 14 |
| BR-38–BR-41 schema and governance | Sections 2, 10, 15, 16, 17 |

---

**Document summary:** This TSD defines an implementable Salesforce-native Phase 1 architecture for rapid missed-dose capture, patient-level adherence calculation, clinically prioritized alerts, controlled de-duplication, daily deterioration detection, and auditable PHI handling. It is grounded in the meeting note and upstream BRD/backlog artifacts, with explicit production controls, measurable NFRs, deployment gates, and operational runbooks.