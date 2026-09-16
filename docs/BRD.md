# Business Requirements Document

## Missed Dose Logging & Adherence Alerts — CarePath Plus

**Document date:** 2026-09-16  
**Source discovery meeting:** 22 July 2026  
**Business sponsor:** Aditi Kapoor, Clinical Adherence Lead  
**Status:** Draft generated from discovery meeting notes  
**Target sign-off:** 29 July 2026  
**Target UAT:** 19 August 2026  
**Target go-live:** End of Q3 FY26, aligned with the Benefit Summary widget release train

## 1. Executive Summary

CarePath Plus currently records patient-reported missed doses as unstructured Case timeline notes received through calls, WhatsApp, and email. The absence of structured dose records prevents timely adherence monitoring and requires analysts to manually extract data for monthly reporting. This creates reporting delays and can postpone clinical intervention by three to four weeks.

Phase 1 will introduce a quick-entry missed-dose logger on the Salesforce Health Cloud Care Plan record page. Care managers and assigned pharmacists will be able to capture a missed dose in under 15 seconds. The solution will calculate rolling 30-, 60-, and 90-day adherence, evaluate defined clinical thresholds, and create prioritized Case Tasks for care-team follow-up while controlling duplicate alert volume. Each dose-log insertion will be audited for PHI access, and validation will prevent entries dated more than 14 days in the past.

## 2. Business Objectives

1. Replace free-text missed-dose capture with structured `Dose_Log__c` records.
2. Enable care managers and assigned pharmacists to record a missed dose during a patient interaction without leaving the Care Plan or Case work surface.
3. Provide on-demand 30-, 60-, and 90-day rolling adherence calculations for an individual patient.
4. Detect defined adherence deterioration and consecutive missed-dose patterns promptly through prioritized Tasks.
5. Reduce alert noise through severity prioritization and duplicate prevention.
6. Establish an auditable PHI access trail for every dose-log insertion.
7. Improve the timeliness and reliability of adherence reporting by eliminating manual Case-note extraction as the primary structured-data source.

## 3. Stakeholders

| Stakeholder | Role / interest | Decision or input ownership |
|---|---|---|
| Aditi Kapoor | Clinical Adherence Lead; business sponsor | Phase 1 scope, governance sign-off, cross-workstream alignment |
| Rajesh Menon | Salesforce BSA | BRD, field-level mapping, widget mockups |
| Dr. Kunal Bhatt | Clinical Pharmacist; clinical SME | Adherence rules, threshold interpretation, Reason values, clinical review ownership |
| Priti Deshmukh | Senior Care Manager; end-user representative | Workflow usability, entry-speed and accessibility feedback, UAT participation |
| Karan Verma | Salesforce Technical Lead | Existing object/schema confirmation, PHI audit-write pattern, technical feasibility |
| Care managers | Operational users | Missed-dose entry and Task follow-up |
| Assigned pharmacists | Operational users | Missed-dose entry and clinical workflow participation |
| CarePath Plus Steering Committee | Governance body | Phase 1 sign-off through governance process |

## 4. Current State / Problem Statement

- Patients report missed doses through inbound calls, WhatsApp, or follow-up email.
- Care managers record the information as free-text notes on the Case timeline.
- There is no structured missed-dose field capture or adherence rollup.
- Three analysts manually scrape Case notes into a spreadsheet for monthly reviews.
- The Q2 FY26 program report was delayed by 11 days when two analysts were unavailable.
- Clinical teams may learn of deteriorating adherence only at a monthly review, commonly three to four weeks after the first missed dose.
- Several therapies lose efficacy after two consecutive missed doses; the autoimmune biologic protocol requires re-titration when three or more doses are missed in a 30-day window. These clinical impacts are the rationale for earlier detection; the meeting did not specify additional automated clinical actions beyond Task creation.

## 5. Future State Overview

1. A care manager or assigned pharmacist opens `doseLogQuickEntry` from the Care Plan Lightning Record Page.
2. The component presents the active Care Plan medication automatically and defaults dose date/time to the current time.
3. The user selects a curated reason, optionally enters notes, and saves the entry without navigating away from the record page.
4. The Apex service validates the entry, inserts `Dose_Log__c`, and writes the required `PHI_Access_Log__c` audit record.
5. The widget calculates the patient's 30-, 60-, and 90-day rolling adherence on render.
6. An after-insert Flow evaluates alert thresholds, checks existing open Tasks, and creates or updates the appropriate Task outcome.
7. A scheduled Flow runs daily at 6:30 AM IST for patients with at least one `Dose_Log__c` entry in the previous 45 days to identify deterioration not caused by a new log entry.
8. Entries remain editable by their creator for 24 hours; after that they are read-only for normal users. Supervisor override is deferred from Phase 1, with manual administrator DML accepted for now.

## 6. Scope

### 6.1 In Scope

- Custom Salesforce LWC named `doseLogQuickEntry` on the Care Plan Lightning Record Page.
- Missed-dose entry by care managers and assigned pharmacists.
- Dose date/time capture with current-time default and back-dating up to 14 days.
- Automatic medication population from the active Care Plan.
- Curated missed-dose Reason values.
- Optional free-text Notes with a 500-character maximum.
- Structured `Dose_Log__c` creation through an Apex service class.
- Server-side validation, including the 14-day back-date limit.
- On-demand 30-, 60-, and 90-day rolling adherence calculation for an individual patient.
- After-insert Flow threshold evaluation and Task creation.
- Daily scheduled adherence re-evaluation at 6:30 AM IST for the defined recent-log population.
- Task de-duplication and highest-severity selection.
- `PHI_Access_Log__c` audit write on every `Dose_Log__c` insert.
- Usability requirements for single-click launch, tab order, and no navigation away from the Case/record work surface.
- Phase 1 field-level mapping and widget states identified in the action items.

### 6.2 Out of Scope

- Patient-facing SMS reminders or outreach.
- Patient portal integration.
- Pharmacy dispense reconciliation, which belongs to the Medication Refill workstream.
- Hospitalization-driven adherence pause logic.
- Supervisor override workflow for edits after 24 hours; manual administrator DML is accepted for Phase 1.
- Batch recalculation across the entire active patient population.
- Cross-widget adherence signals with the Financial Assistance and Refill workstreams.
- Any clinical intervention beyond the specified Salesforce Task creation and assignment.

## 7. Functional Requirements

### 7.1 Missed-Dose Entry

| ID | Requirement | Traceability |
|---|---|---|
| BR-01 | The solution shall provide a custom LWC named `doseLogQuickEntry` on the Care Plan Lightning Record Page. | Decisions; Proposed missed dose logger |
| BR-02 | The LWC shall allow a care manager or the patient's assigned pharmacist to open the missed-dose entry surface with a single click. | Proposed missed dose logger; UX discussion |
| BR-03 | The LWC shall capture dose date and time and shall default the value to the current date and time. | Proposed missed dose logger |
| BR-04 | The LWC shall allow a user to back-date a dose entry by no more than 14 days. | Proposed missed dose logger; Compliance and audit |
| BR-05 | The service shall reject any dose entry dated more than 14 days in the past, and this rule shall be enforced server-side in addition to the LWC validation. | Compliance and audit |
| BR-06 | The solution shall automatically populate the medication from the active Care Plan and shall not require manual medication selection from a picklist. | Proposed missed dose logger |
| BR-07 | The LWC shall provide the following curated Reason values: `Forgot`, `Side effects`, `Cost / access`, `Feeling better`, `Travel`, `Hospitalization`, and `Other`. | Proposed missed dose logger |
| BR-08 | The LWC shall provide an optional free-text Notes field with a maximum length of 500 characters. | Proposed missed dose logger |
| BR-09 | On save, the solution shall create a structured `Dose_Log__c` record through the Apex service class. | Current state; Decisions |
| BR-10 | The entry surface shall support completion in under 15 seconds during a normal care-manager or pharmacist interaction, shall open with a single click, shall support tab order, and shall not require navigation away from the record work surface. | Proposed missed dose logger; UX discussion |

### 7.2 Adherence Calculation

| ID | Requirement | Traceability |
|---|---|---|
| BR-11 | The Apex service class shall calculate adherence on demand for a single patient when the widget renders. | Adherence calculation; Decisions |
| BR-12 | The solution shall provide rolling 30-day, 60-day, and 90-day adherence values. | Adherence calculation |
| BR-13 | The solution shall calculate adherence as `(scheduled doses − missed doses) / scheduled doses × 100`. | Adherence calculation |
| BR-14 | Scheduled doses shall be derived from the Care Plan's existing `Dose_Schedule__c`; missed doses shall be derived from `Dose_Log__c` records. | Adherence calculation |
| BR-15 | The solution shall not perform batch recalculation across the entire active patient population as part of widget rendering or post-log processing. | Adherence calculation; Decisions |

### 7.3 Alerting and Task Management

| ID | Requirement | Traceability |
|---|---|---|
| BR-16 | An after-insert record-triggered Flow on `Dose_Log__c` shall evaluate adherence alert thresholds after a missed-dose record is inserted. | Decisions |
| BR-17 | When rolling 30-day adherence drops below 80%, the solution shall create an adherence Task on the Case assigned to the primary care manager with Normal priority. | Alert thresholds |
| BR-18 | When three or more consecutive missed doses occur in a seven-day window, the solution shall create an adherence Task assigned to the primary care manager with High priority. | Alert thresholds |
| BR-19 | When rolling 30-day adherence drops below 60%, the solution shall create a High-priority Task assigned to Dr. Kunal Bhatt for clinical review and a copy Task assigned to the primary care manager. | Alert thresholds |
| BR-20 | If more than one threshold is breached for the same patient, the solution shall create no more than one adherence Task per patient in a 24-hour period, and the highest-severity threshold shall determine the resulting Task handling. | De-duplication rules |
| BR-21 | If an open, not-closed Task already exists for the same patient and threshold type, the solution shall not create a duplicate and shall post a comment to the existing Task instead. | De-duplication rules |
| BR-22 | The Flow shall use existing open-Task retrieval and conditional decision logic to implement de-duplication; an Apex trigger is not required for the alerting layer. | Technical discussion |
| BR-23 | A scheduled Flow shall run daily at 6:30 AM IST and re-evaluate rolling 30-day adherence for patients with at least one `Dose_Log__c` entry in the preceding 45 days. | Decisions |
| BR-24 | The scheduled evaluation shall support detection of adherence deterioration that has not been triggered by a new dose-log entry, including patients who have stopped logging. | Decisions |

### 7.4 Compliance, Audit, and Editing

| ID | Requirement | Traceability |
|---|---|---|
| BR-25 | Every `Dose_Log__c` insertion shall write to the existing `PHI_Access_Log__c` audit trail. | Compliance and audit |
| BR-26 | Each dose-log audit record shall include user ID, patient ID, timestamp, and action value `DOSE_LOG_ENTRY`. | Compliance and audit |
| BR-27 | A care manager shall be able to edit their own dose-log entries within 24 hours of creation. | Compliance and audit |
| BR-28 | After 24 hours, dose-log entries shall be read-only for normal users. | Compliance and audit |
| BR-29 | Supervisor override editing shall not be implemented in Phase 1; manual DML by administrators is the accepted interim process. | Compliance and audit; Decisions |
| BR-30 | The solution shall retain the curated Reason field rather than introducing unrestricted free text for the reason, to reduce the risk of PHI leakage into an unaudited field. | Compliance and audit |

## 8. Non-Functional Requirements

| ID | Requirement | Traceability / source |
|---|---|---|
| NFR-01 | The entry interaction shall be usable from the Care Plan record page without navigation away from the active Case/record work surface. | Meeting UX discussion |
| NFR-02 | The entry interaction shall support keyboard tab order. | Meeting UX discussion |
| NFR-03 | The normal entry workflow shall be designed to complete in under 15 seconds. | Meeting UX discussion |
| NFR-04 | The server-side service shall enforce the 14-day date restriction independently of client-side validation. | Compliance and audit |
| NFR-05 | Each dose-log insertion shall create the specified PHI audit record with the required identity, patient, time, and action data. | Compliance and audit |
| NFR-06 | The Notes field shall enforce a 500-character maximum. | Proposed missed dose logger |
| NFR-07 | The solution shall avoid full-population batch recalculation during on-demand widget rendering and post-log evaluation. | Performance concern |
| NFR-08 | The scheduled adherence evaluation shall execute at 6:30 AM IST daily. | Decisions |
| NFR-09 | Notes shall be included in the quarterly PHI scrub review process. | Compliance and audit |
| NFR-10 | The implementation shall reuse the established `PHI_Access_Log__c` write pattern where confirmed by the Technical Lead. | Action item; existing control pattern |

Performance, availability, detailed security access model, retention duration, and recovery objectives were not provided in the meeting notes and require confirmation before final approval.

## 9. Data and Field Requirements

| Data element | Source / object | Business requirement |
|---|---|---|
| Dose date and time | `Dose_Log__c` | Defaults to now; back-date limited to 14 days |
| Medication | Active Care Plan | Auto-populated; no manual picklist entry |
| Reason | `Dose_Log__c` | Curated values defined in BR-07 |
| Notes | `Dose_Log__c` | Optional; maximum 500 characters; quarterly PHI scrub |
| Scheduled doses | `Dose_Schedule__c` on Care Plan | Used as the scheduled-dose input to adherence calculation |
| Missed doses | `Dose_Log__c` | Used as the missed-dose input to adherence calculation |
| Audit user | `PHI_Access_Log__c` | User ID required for each insertion |
| Audit patient | `PHI_Access_Log__c` | Patient ID required for each insertion |
| Audit timestamp | `PHI_Access_Log__c` | Timestamp required for each insertion |
| Audit action | `PHI_Access_Log__c` | Must be `DOSE_LOG_ENTRY` |

The meeting notes require confirmation of the existing `Dose_Log__c` object and `Dose_Schedule__c` schema, including which existing fields can be reused and which additions are needed.

## 10. Assumptions

1. Salesforce Health Cloud and Care Plans are available for the CarePath Plus program.
2. The active Care Plan contains a usable `Dose_Schedule__c` representation of scheduled doses.
3. A Case, primary care manager, assigned pharmacist, and patient relationship can be identified for the applicable Care Plan.
4. Salesforce Tasks support assignment, priority, open/closed status, and comments for the required workflow.
5. The existing `PHI_Access_Log__c` object and its write mechanism are available for reuse.
6. The existing Benefit Summary widget release train remains the target deployment alignment.
7. Normal users can be restricted from editing dose-log entries after 24 hours using Salesforce security or service-layer controls; exact mechanism is not specified.
8. The phrase “same threshold type” can be represented consistently for duplicate detection across Tasks.

## 11. Dependencies

| Dependency | Description | Owner / source |
|---|---|---|
| Care Plan data model | Active medication and `Dose_Schedule__c` must be available and correctly related to the patient. | Salesforce Technical Lead / CarePath Plus org |
| `Dose_Log__c` schema | Existing object and fields must be confirmed; additions may be required. | Karan Verma; due 24 July 2026 |
| PHI audit pattern | Existing helper class or Platform Event write pattern must be identified and reused where applicable. | Karan Verma; due 24 July 2026 |
| Reason taxonomy | Clinical team must finalize whether “Adverse event” is distinct or handled by the existing Adverse Event Quick Logger. | Dr. Kunal Bhatt; due 25 July 2026 |
| UAT participants | Six care managers, three per hub, must be nominated. | Priti Deshmukh; week of 4 August 2026 |
| Governance alignment | Financial Assistance and Refill owners must confirm Phase 1 excludes cross-widget signals. | Aditi Kapoor |
| Steering Committee approval | Phase 1 sign-off is expected through the 28 July governance meeting. | CarePath Plus Steering Committee |
| Salesforce release train | Go-live depends on alignment with the Benefit Summary widget release train. | CarePath Plus PMO |

## 12. Risks

| Risk | Impact | Likelihood | Mitigation |
|---|---|---:|---|
| Existing `Dose_Log__c` or `Dose_Schedule__c` schema does not support the required fields or relationships. | High | Medium | Complete schema confirmation and field mapping before sign-off. |
| PHI audit-write pattern is not reusable as expected. | High | Medium | Confirm helper class or Platform Event pattern and validate audit behavior early. |
| Entry workflow exceeds 15 seconds or is difficult to use during calls. | High | Medium | Validate default, populated, back-date, and error states with six UAT users. |
| Duplicate prevention fails, creating alert fatigue. | High | Medium | Test same-day multi-threshold breaches and existing open Tasks with Flow decision paths. |
| Scheduled dose data is incomplete or inaccurate. | High | Medium | Validate Care Plan dose schedules with the clinical and technical SMEs before adherence calculations are approved. |
| Curated Reason taxonomy is not finalized by the target date. | Medium | Medium | Resolve the “Adverse event” decision with the clinical team before UAT configuration is fixed. |
| Notes field contains PHI despite audit controls. | Medium | Medium | Retain quarterly PHI scrub review and maintain the curated Reason field. |
| Manual administrator DML for post-24-hour corrections creates operational inconsistency. | Medium | Medium | Document the Phase 1 administrative process and capture supervisor override as a future scope candidate. |
| Release-train or governance timing shifts. | High | Medium | Track 29 July sign-off, 19 August UAT, 28 July governance, and end-Q3 release dependencies. |

## 13. Open Questions

1. What are the current fields and relationships on `Dose_Log__c` and `Dose_Schedule__c`, and which additions are required?
2. Is the existing `PHI_Access_Log__c` write pattern implemented through a helper class or Platform Event, and what standard must the Apex service use?
3. Should `Adverse event` be a distinct Reason value, or must it be captured through the existing Adverse Event Quick Logger?
4. What Salesforce security and service-layer controls will enforce editability by the creator within 24 hours and read-only status thereafter?
5. How should the solution behave when scheduled doses are zero, missing, or invalid for a rolling window? Information not provided.
6. What exact Task comment content is required when a duplicate open Task is found? Information not provided.
7. What are the required performance targets for widget rendering, Apex calculation, Flow completion, and scheduled processing? Information not provided.
8. What availability, data-retention, access-control, and recovery requirements apply? Information not provided.
9. Does the assigned pharmacist need any permissions or Task-routing behavior beyond entering a dose log? Information not provided.
10. What reporting outputs or dashboards, if any, are required after structured capture is introduced? Information not provided.

## 14. References

1. `Meeting_Note_03_Missed_Dose_Adherence.md` — Formal discovery meeting minutes, 22 July 2026.
2. CarePath Plus Patient Self-Enrollment TDD — referenced in meeting notes for established PHI controls; document content was not provided.
3. CarePath Plus Benefit Summary widget — referenced for release-train alignment; detailed artifact was not provided.
4. Salesforce Health Cloud Care Plan, `Dose_Schedule__c`, `Dose_Log__c`, and `PHI_Access_Log__c` — system entities referenced by the meeting notes; current schema details require confirmation.

## 15. Requirement Traceability Summary

| Requirement group | IDs | Business outcome |
|---|---|---|
| Quick entry and structured capture | BR-01–BR-10 | Faster, consistent missed-dose capture |
| Adherence calculation | BR-11–BR-15 | Timely patient-level adherence visibility |
| Alerts and Tasks | BR-16–BR-24 | Earlier clinical and care-manager follow-up with controlled volume |
| Audit and editing | BR-25–BR-30 | PHI accountability and controlled record correction |
| Non-functional controls | NFR-01–NFR-10 | Usability, validation, auditability, and processing constraints |

**Key requirement IDs:** BR-01–BR-30; NFR-01–NFR-10.