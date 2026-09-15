# Kanban Test Cases

These Jira stories represent the supplied Care Plan medication-dose logging and adherence test cases. Each story preserves the test-case ID, traceability, scenario, and expected result.

## Stories

### TC-01 — Authorized Care Plan component load
**Traceability:** BR-01, BR-02, BR-06, BR-09, BR-26–27

**Scenario:** Load component for an authorized user on an active Care Plan.

**Expected result:** Component renders on the Care Plan surface; medication is server-sourced; 30/60/90-day summaries are available; no navigation is required.

### TC-02 — Keyboard-only form navigation
**Traceability:** BR-03–04, NFR-02

**Scenario:** Open form by one click and navigate using keyboard only.

**Expected result:** Form opens in one activation, focus is usable, tab order is logical, and no keyboard trap exists.

### TC-03 — Entry defaults and curated clinical controls
**Traceability:** BR-06–11, BRULE-05, BRULE-08, BRULE-24

**Scenario:** Verify default timestamp, server-derived medication, required reason, curated reason options, and no arbitrary medication.

**Expected result:** Defaults and curated controls are correct; medication cannot be freely substituted. Final clinical reason list must be approved.

### TC-04 — Valid current-time dose entry and audit
**Traceability:** BR-06, BR-11, BR-14–18, BRULE-14

**Scenario:** Submit a valid current-time entry with reason and blank Notes.

**Expected result:** One `Dose_Log__c` is persisted with server-derived relationships and one `PHI_Access_Log__c` event containing user, patient, timestamp, and `DOSE_LOG_ENTRY`.

### TC-05 — Accept exact 14-day lower boundary
**Traceability:** BR-08, BR-12, BRULE-06–07

**Scenario:** Submit exactly at the permitted 14-day lower boundary through UI and Apex.

**Expected result:** Boundary is accepted and audited. Confirm whether boundary means calendar days or exact 14×24 hours.

### TC-06 — Reject timestamp beyond 14-day lower boundary
**Traceability:** BR-12, BR-16, NFR-03, BRULE-07

**Scenario:** Submit one minute beyond the permitted lower boundary through UI and direct Apex.

**Expected result:** Both layers reject with controlled validation; no Dose Log or audit event is created.

### TC-07 — Reject future timestamp
**Traceability:** TSD §§6.2, 8

**Scenario:** Submit a future timestamp.

**Expected result:** UI and Apex reject it; no DML occurs. Confirm this TSD rule as an approved business rule.

### TC-08 — Validate reason and Notes length
**Traceability:** BR-10–11, BR-24, NFR-07

**Scenario:** Test blank/invalid reason, 500-character Notes, and 501-character Notes.

**Expected result:** Invalid reason and 501 chars are rejected; exactly 500 chars is accepted.

### TC-09 — Preserve form data after create-service failure
**Traceability:** BR-12; TSD §6.2

**Scenario:** Force create-service failure after entering valid data.

**Expected result:** Non-PHI error is shown and entered values remain in the form.

### TC-10 — Handle missing active medication or schedule
**Traceability:** BR-09, BR-18, BR-28; TSD §6.1

**Scenario:** Load Care Plans without active medication or schedule.

**Expected result:** Configuration error is shown and invalid logging is blocked.

### TC-11 — Reject tampered or unauthorized relationships
**Traceability:** BR-18–19, BR-43–44; TSD §§6.2, 10

**Scenario:** Tamper with relationship IDs or submit an unrelated/inactive Care Plan.

**Expected result:** Patient/medication are derived server-side; tampering and unauthorized context are rejected.

### TC-12 — Enforce role and record-sharing access
**Traceability:** BR-43–45, NFR-08; TSD §10

**Scenario:** Test care manager, assigned pharmacist, unauthorized user, Clinical Pharmacist, and administrator access.

**Expected result:** Access matches role and record-sharing rules; unauthorized creation/read is prevented.

### TC-13 — Honor CRUD, FLS, and sharing permissions
**Traceability:** NFR-08; TSD §§8, 10

**Scenario:** Remove CRUD/FLS/sharing permissions and attempt operations/cross-patient access.

**Expected result:** Apex honors CRUD/FLS/sharing and performs no unauthorized DML.

### TC-14 — Allow creator edit before 24 hours
**Traceability:** BR-20, BRULE-09; TSD §6.3

**Scenario:** Creator edits own entry before 24 hours.

**Expected result:** Update succeeds with server-side validation; no new insert audit event is created.

### TC-15 — Enforce edit authorization and 24-hour window
**Traceability:** BR-21, BRULE-10; TSD §6.3

**Scenario:** Creator edits at/after 24 hours and non-creator edits at any time.

**Expected result:** Controlled authorization/edit-window error; record remains unchanged. Confirm exact boundary inclusivity.

### TC-16 — Fail closed on PHI audit write failure
**Traceability:** BR-17, BR-22, NFR-04; TSD §§6.2, 8, 11

**Scenario:** Force PHI audit write failure during valid insert.

**Expected result:** Transaction fails closed; no un-audited Dose Log or downstream alert exists; controlled `AUDIT_FAILURE`.

### TC-17 — Prevent PHI leakage into telemetry and tasks
**Traceability:** BR-24–25, NFR-07; TSD §§7, 10, 12

**Scenario:** Enter PHI-like Notes and inspect UI errors, logs, Flow faults, Task text, and audit data.

**Expected result:** Notes are not leaked into operational telemetry, Task subjects/comments, or fault messages.

### TC-18 — Calculate adherence windows and boundaries
**Traceability:** BR-26–29, BRULE-01–04; TSD §§5–6.4

**Scenario:** Calculate windows with in/out boundary schedules and logs.

**Expected result:** Counts use `Dose_Schedule__c` and applicable `Dose_Log__c`; formula and rolling boundaries are correct.

### TC-19 — Handle zero scheduled doses and integrity anomalies
**Traceability:** BRULE-01–03; TSD §§5, 6.4

**Scenario:** Calculate zero scheduled doses and missed-greater-than-scheduled integrity case.

**Expected result:** Zero yields null/no percentage threshold; integrity anomaly is reported, not silently clamped. Confirm product behavior.

### TC-20 — Deduplicate dose events
**Traceability:** BR-29; TSD §6.4

**Scenario:** Provide duplicate records for one Care Plan/medication/dose timestamp.

**Expected result:** Duplicate dose event is counted once according to confirmed identity semantics.

### TC-21 — Apply threshold and rounding policy
**Traceability:** BR-27, BR-32, BR-34; TSD §§5, 7

**Scenario:** Test percentages exactly at and just below 60%/80%, including display rounding.

**Expected result:** Display uses approved precision; thresholds use unrounded value; “below” excludes exact boundary. Confirm rounding policy.

### TC-22 — Create normal task for moderate adherence risk
**Traceability:** BR-32, BR-35–38, BRULE-11–12

**Scenario:** Cause 30-day adherence below 80% but at least 60%.

**Expected result:** One Normal Task on Case to primary care manager with `ADHERENCE_LT_80`.

### TC-23 — Create high task for consecutive misses
**Traceability:** BR-33, BR-35–38

**Scenario:** Cause ≥3 consecutive missed doses in seven days.

**Expected result:** One High Task to primary care manager with `CONSECUTIVE_MISSES_3_7D`; non-consecutive misses do not qualify.

### TC-24 — Create clinical-review task for severe adherence risk
**Traceability:** BR-34, BR-45; TSD §7

**Scenario:** Cause 30-day adherence below 60%.

**Expected result:** High clinical-review Task to Clinical Pharmacist and required care-manager copy; `ADHERENCE_LT_60`; no lower severity duplicate.

### TC-25 — Resolve multiple thresholds by highest severity
**Traceability:** BR-37–38, BRULE-11–12; TSD §7

**Scenario:** Cause all thresholds in one evaluation.

**Expected result:** Highest severity wins and patient 24-hour cap is respected.

### TC-26 — Enforce 24-hour patient alert cap
**Traceability:** BR-37, BRULE-11; TSD §7

**Scenario:** Trigger another threshold within 24 hours after an adherence Task.

**Expected result:** No more than one adherence Task per patient in the period; existing alert is retained/commented per approved policy.

### TC-27 — Deduplicate against open and closed Tasks
**Traceability:** BR-39, BRULE-13; TSD §7

**Scenario:** Trigger a threshold where same patient/Case/threshold has an open Task; repeat with closed Task.

**Expected result:** Open Task receives comment and no duplicate; closed Task does not suppress a new Task.

### TC-28 — Deduplicate dual-recipient severe alerts
**Traceability:** BR-34, BR-37, BR-39; TSD §7

**Scenario:** Test below-60% dual-recipient deduplication with one/both recipient Tasks existing.

**Expected result:** Existing matching Tasks are commented; missing recipients are handled consistently with approved one-task cap. Clarify apparent requirement conflict.

### TC-29 — Execute daily scheduled evaluation
**Traceability:** BR-40–42; TSD §9

**Scenario:** Execute daily Flow at 06:30 IST with qualifying and non-qualifying Care Plans.

**Expected result:** Only Care Plans with a log in prior 45 days are evaluated; same rules apply; run metrics are captured.

### TC-30 — Scale targeted evaluation with chunking
**Traceability:** BR-30, BR-41, BR-46, NFR-05; TSD §§9, 11

**Scenario:** Execute targeted scheduled evaluation for 4,200 patients/≥200 qualifying Care Plans.

**Expected result:** Chunking prevents governor failure; ≤200 Care Plans/transaction or equivalent; completes within 45 minutes; no full-population render recalculation.

### TC-31 — Process collection-based Flow Apex requests
**Traceability:** TSD §§8–9, 11

**Scenario:** Invoke collection-based Flow Apex with 200 mixed requests.

**Expected result:** One result per request, correct decisions, no SOQL/DML in loops, and isolated/reportable failures.

### TC-32 — Meet keyboard accessibility during validation recovery
**Traceability:** BR-04, NFR-02; TSD §§11, 13

**Scenario:** Complete creation and recover from validation error using keyboard only.

**Expected result:** Labels, focus, error association/announcement, keyboard submission, and recovery meet WCAG-aligned behavior.

### TC-33 — Meet care-manager entry time objective
**Traceability:** BR-05, NFR-01; TSD §§11, 13

**Scenario:** Time standard entry for six UAT care managers.

**Expected result:** At least 95% of trained users complete a standard entry within 15 seconds.

### TC-34 — Meet latency targets under load
**Traceability:** NFR-05; TSD §11

**Scenario:** Measure UI, create, and single-Care-Plan calculation latency under stated load.

**Expected result:** Meets p95/p99 targets: open 1.0s/—, create 2.0s/4.0s, calculation 1.5s/3.0s; no full-population recalculation.

### TC-35 — Sustain throughput and controlled fault handling
**Traceability:** NFR-04–06; TSD §§8, 11–12

**Scenario:** Sustain 60 creates/min for 10 minutes and inject audit/Flow faults.

**Expected result:** Throughput, audit completeness, controlled failure, deduplication, and no-PHI telemetry targets hold.

### TC-36 — Rehearse rollback and operational monitoring
**Traceability:** TSD §§12, 14–15

**Scenario:** Rehearse rollback and operational monitoring.

**Expected result:** Flows stop in documented order; LWC/permission rollback works; existing logs/audit remain; support detects and can handle failures.
