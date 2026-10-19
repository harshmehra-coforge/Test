# Business Requirements Document

## 1. Executive Summary

CarePath Plus currently captures missed doses through unstructured Case timeline notes, preventing timely adherence monitoring and forcing analysts to calculate adherence manually. This creates delayed clinical intervention and delayed program reporting. The proposed Phase 1 capability will provide a structured missed-dose quick-entry experience on the Salesforce Health Cloud Care Plan record page, calculate rolling 30-, 60-, and 90-day adherence for an individual patient, and create prioritized care-team Tasks when defined adherence thresholds are breached. The capability will also apply alert de-duplication and PHI audit controls. The expected business value is earlier detection of adherence deterioration, reduced manual reporting effort, improved consistency of clinical follow-up, and auditable dose-event capture.

## 2. Business Objectives

1. Enable care managers and assigned pharmacists to record a missed dose from the patient Care Plan record page without leaving the working record.
2. Replace free-text-only missed-dose capture with structured dose-log data that supports reliable adherence calculation.
3. Detect adherence deterioration through rolling 30-, 60-, and 90-day calculations and defined threshold evaluation.
4. Notify the appropriate care-team users when adherence thresholds are breached while limiting duplicate and excessive Tasks.
5. Maintain an auditable record of every missed-dose entry and protect against uncontrolled free-text reason capture.
6. Support Phase 1 delivery against the target BRD sign-off of 29 July 2026, UAT on 19 August 2026, and go-live by the end of Q3 FY26, aligned with the Benefit Summary widget release train.

## 3. Stakeholders

| Stakeholder | Role and responsibility |
|---|---|
| Aditi Kapoor, Clinical Adherence Lead | Business Sponsor; confirms scope, cross-workstream alignment, and Steering Committee sign-off. |
| Rajesh Menon, Salesforce BSA | Requirements owner; drafts the BRD, field-level mapping, and widget state mockups. |
| Dr. Kunal Bhatt, Clinical Pharmacist | Clinical SME; confirms adherence rules, alert severity, and final Reason values. |
| Priti Deshmukh, Senior Care Manager | End-user representative; validates usability and coordinates care-manager UAT participation. |
| Karan Verma, Salesforce Technical Lead | Confirms existing object schemas and PHI audit write pattern. |
| Primary care managers | Receive and act on adherence Tasks for assigned patients. |
| Dr. Kunal Bhatt / Clinical Pharmacist | Receives high-severity clinical-review Tasks for patients below the 60% rolling 30-day threshold. |
| CarePath Plus Steering Committee | Governance stakeholder for Phase 1 sign-off. |

## 4. Scope

### 4.1 In Scope

- A custom `doseLogQuickEntry` component on the Patient Care Plan Lightning Record Page.
- Structured creation of `Dose_Log__c` records by a care manager or the patient’s assigned pharmacist.
- Dose date and time capture with a default of the current date/time and permitted back-dating up to 14 days.
- Medication auto-population from the active Care Plan.
- Curated missed-dose Reason values: `Forgot`, `Side effects`, `Cost / access`, `Feeling better`, `Travel`, `Hospitalization`, and `Other`.
- Optional Notes with a maximum length of 500 characters.
- Rolling 30-, 60-, and 90-day adherence calculation using the Care Plan dose schedule and missed-dose logs.
- On-demand adherence calculation for one patient when the widget renders.
- Post-log threshold evaluation.
- Daily re-evaluation at 6:30 AM IST for patients with at least one `Dose_Log__c` entry in the preceding 45 days.
- Creation and de-duplication of adherence Tasks on Cases.
- PHI audit logging for every `Dose_Log__c` insert.
- Editing by care managers of their own entries within 24 hours of creation.
- Read-only behavior after the 24-hour edit period for normal users.
- UAT participation by six nominated care managers, three per hub.

### 4.2 Out of Scope

- Patient-facing SMS reminders, outreach, or portal integration; deferred to Phase 2.
- Pharmacy dispense reconciliation; handled by the Medication Refill workstream.
- Hospitalization-driven adherence pause logic; deferred to Phase 2.
- Supervisor-override workflow for edits after 24 hours; manual administrator DML is acceptable for Phase 1.
- Cross-widget adherence signals with the Financial Assistance and Refill workstreams.
- Batch recalculation of rolling adherence for the entire active-patient population.

## 5. Current State / Problem Statement

Patients report missed doses through inbound calls, WhatsApp, or follow-up email. Care managers record the information as free-text Case timeline notes, with no structured missed-dose fields or adherence rollup. Monthly program reviews require three analysts to extract Case notes into a spreadsheet and calculate adherence manually. The Q2 FY26 report was delayed by 11 days when two analysts were unavailable. Clinical teams may not identify deteriorating adherence until the monthly review, potentially 3–4 weeks after the first missed dose. The current process does not provide timely threshold-based Tasks, reliable structured data, or a consistent audit trail for dose-log events.

## 6. Future State Overview

A care manager or assigned pharmacist opens the quick-entry component from the patient’s Care Plan record page and records a missed dose in a single, focused interaction. The component defaults the dose date/time, derives the medication from the active Care Plan, requires selection from the curated Reason list, and permits optional notes up to 500 characters. The service validates the entry, creates the structured dose log, and writes the required PHI audit record.

When the widget renders, the patient’s 30-, 60-, and 90-day adherence values are calculated on demand from the Care Plan dose schedule and dose logs. After a new dose log, threshold evaluation determines whether an adherence Task is needed. A daily 6:30 AM IST evaluation also checks eligible patients to detect deterioration when no new log has been entered. Task creation applies severity precedence and de-duplication rules so that care teams receive actionable rather than repetitive alerts.

## 7. Functional Requirements

### 7.1 Missed-Dose Entry

| ID | Requirement |
|---|---|
| BR-01 | The business solution shall provide a custom `doseLogQuickEntry` component on the Patient Care Plan Lightning Record Page. |
| BR-02 | The component shall be accessible from the Care Plan record page without requiring navigation away from the patient’s working record. |
| BR-03 | The component shall support missed-dose entry by a care manager and by the patient’s assigned pharmacist. |
| BR-04 | The component shall default Dose date and time to the current date and time. |
| BR-05 | The component shall allow a user to back-date a Dose date and time by no more than 14 days. |
| BR-06 | The component shall prevent a user from submitting a dose entry back-dated beyond 14 days, and the same restriction shall be enforced server-side. |
| BR-07 | The component shall auto-populate Medication from the active Care Plan and shall not require manual picklist selection of the medication. |
| BR-08 | The component shall provide the following curated Reason values: `Forgot`, `Side effects`, `Cost / access`, `Feeling better`, `Travel`, `Hospitalization`, and `Other`. |
| BR-09 | The component shall provide an optional free-text Notes field limited to 500 characters. |
| BR-10 | The component shall create a structured `Dose_Log__c` record for a valid submitted missed-dose entry. |
| BR-11 | The entry interaction shall be single-click to open and tab-order friendly. |
| BR-12 | The entry experience shall be designed to support completion in under 15 seconds during a call. |

### 7.2 Validation, Editing, and Audit

| ID | Requirement |
|---|---|
| BR-13 | The Apex service shall handle dose-log insertion and server-side validation. |
| BR-14 | Every successful `Dose_Log__c` insertion shall write to the existing `PHI_Access_Log__c` audit trail. |
| BR-15 | The audit record for each dose-log insertion shall contain the user ID, patient ID, timestamp, and action value `DOSE_LOG_ENTRY`. |
| BR-16 | A care manager shall be able to edit their own dose-log entries within 24 hours of creation. |
| BR-17 | After 24 hours, dose-log entries shall be read-only to normal users. |
| BR-18 | Phase 1 shall not provide a supervisor-override workflow for edits after 24 hours; manual administrator DML remains the approved interim approach. |

### 7.3 Adherence Calculation

| ID | Requirement |
|---|---|
| BR-19 | The business solution shall calculate adherence over rolling 30-day, 60-day, and 90-day windows. |
| BR-20 | The solution shall calculate adherence as `(scheduled doses − missed doses) / scheduled doses × 100`. |
| BR-21 | Scheduled doses shall be derived from the Care Plan’s existing `Dose_Schedule__c`. |
| BR-22 | Missed doses shall be derived from `Dose_Log__c` records created through the missed-dose logging process. |
| BR-23 | The Apex service shall calculate adherence on demand for a single patient when the widget renders. |
| BR-24 | The solution shall evaluate adherence thresholds after a new missed-dose log is inserted. |
| BR-25 | The solution shall not perform batch recalculation across the entire active-patient population as part of the widget-render calculation. |

### 7.4 Alert Thresholds and Task Routing

| ID | Requirement |
|---|---|
| BR-26 | When rolling 30-day adherence drops below 80%, the solution shall create a Normal-priority Task on the Case and assign it to the primary care manager. |
| BR-27 | When three or more consecutive missed doses occur within a seven-day window, the solution shall create a High-priority Task and assign it to the primary care manager. |
| BR-28 | When rolling 30-day adherence drops below 60%, the solution shall create a High-priority Task assigned to Dr. Kunal Bhatt for clinical review and a copy Task assigned to the primary care manager. |
| BR-29 | Alert evaluation and Task creation shall be handled by a record-triggered after-insert Flow on `Dose_Log__c`. |
| BR-30 | The after-insert alerting process shall evaluate existing open Tasks and apply conditional branching before creating or updating an alert Task. |
| BR-31 | The solution shall enforce a maximum of one adherence Task per patient in any 24-hour period, regardless of the number of thresholds breached. |
| BR-32 | When multiple thresholds are breached for the same patient, the solution shall apply the highest-severity threshold. |
| BR-33 | If an open Task exists for the same patient and threshold type, the solution shall not create a duplicate Task and shall instead post a comment to the existing Task. |
| BR-34 | A scheduled Flow shall run daily at 6:30 AM IST and re-evaluate rolling 30-day adherence for patients with at least one `Dose_Log__c` entry in the preceding 45 days. |
| BR-35 | The scheduled evaluation shall support detection of adherence deterioration where a patient has stopped logging new missed-dose events. |

## 8. Non-Functional Requirements

| ID | Requirement |
|---|---|
| BR-36 | The quick-entry component shall support keyboard tab order and shall not require navigation away from the Care Plan record page. |
| BR-37 | The quick-entry interaction shall be optimized for completion in under 15 seconds under normal operating conditions. |
| BR-38 | Dose-log creation shall enforce the 14-day back-date restriction both in the user interface and on the server. |
| BR-39 | The solution shall preserve auditability of every successful dose-log insertion through `PHI_Access_Log__c`. |
| BR-40 | The Reason field shall use curated values rather than unrestricted free text to reduce the risk of PHI leakage into an unaudited field. |
| BR-41 | The Notes field shall be included in the quarterly PHI scrub review because it permits free text. |
| BR-42 | The alerting design shall control operational noise through the 24-hour patient-level limit, highest-severity precedence, and same-threshold open-Task de-duplication. |
| BR-43 | The solution shall support the current operating context of approximately 4,200 active patients across two hubs without requiring a full-population recalculation when an individual widget renders. |

## 9. Assumptions

- The Salesforce Health Cloud Care Plan is the primary work surface for the CarePath Plus program.
- `Dose_Schedule__c` exists on the Care Plan and contains sufficient information to derive scheduled doses; the exact schema is subject to technical confirmation.
- `Dose_Log__c` exists or can be confirmed as the structured record for missed-dose events; existing fields and required additions are subject to technical confirmation.
- `PHI_Access_Log__c` exists and can receive the required audit data using the organization’s approved write pattern.
- Case records have an identifiable primary care manager to support Task assignment.
- Dr. Kunal Bhatt is the named recipient for the below-60% clinical-review Task unless the business confirms a different operational assignment.
- Normal-user permissions can distinguish a care manager’s own dose-log entries from entries created by other users.
- The existing Benefit Summary widget release train provides the delivery alignment for the target go-live.
- Manual administrator DML is acceptable for post-24-hour corrections during Phase 1.

## 10. Dependencies

- Confirmation of the current `Dose_Log__c` object and field schema by the Salesforce Technical Lead.
- Confirmation of the current `Dose_Schedule__c` schema and its ability to derive scheduled doses.
- Confirmation of the standard `PHI_Access_Log__c` write pattern, including whether it uses a helper class or Platform Event.
- Final clinical confirmation of the Reason picklist values, including whether `Adverse event` should be a distinct value or handled by the existing Adverse Event Quick Logger.
- Availability of the Salesforce Health Cloud Care Plan record page and existing Case relationship.
- Confirmation of Task assignment configuration for primary care managers and Dr. Kunal Bhatt.
- Nomination of three care managers per hub and scheduling of the widget UX validation session during the week of 4 August.
- Alignment with Financial Assistance and Refill workstream owners that Phase 1 excludes cross-widget adherence signals.
- CarePath Plus Steering Committee sign-off at the 28 July governance meeting.
- Alignment with the Benefit Summary widget release train for go-live.

## 11. Risks

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| Existing `Dose_Log__c` or `Dose_Schedule__c` schema does not support the required fields or calculation inputs. | Delayed design confirmation and inaccurate adherence results. | Medium | Complete schema confirmation by 24 July and document required field additions or mappings. |
| Unrestricted Notes content introduces PHI into a field requiring additional review. | Compliance exposure and additional operational review effort. | Medium | Keep Reason curated, cap Notes at 500 characters, and include Notes in quarterly PHI scrub review. |
| Alert volume creates Task fatigue for care managers. | Important alerts may be ignored or operational adoption may decline. | High | Enforce highest-severity selection, one Task per patient per 24 hours, and open-Task de-duplication with comments. |
| Incorrect or incomplete dose schedules produce inaccurate adherence percentages. | Clinical decisions and program reporting may be affected. | Medium | Validate `Dose_Schedule__c` data and confirm calculation rules with the Clinical Pharmacist before UAT. |
| The 15-second entry target is not met. | Users may revert to free-text Case notes, reducing structured-data adoption. | Medium | Validate default, populated, back-date, and error states with six care managers during UX validation and UAT. |
| Post-24-hour corrections rely on manual administrator DML. | Corrections may be delayed or inconsistently governed. | Medium | Explicitly document the Phase 1 limitation and assess supervisor override in a later phase. |
| Delayed Steering Committee or workstream alignment affects the release train. | Sign-off, UAT, or go-live may slip. | Medium | Secure governance confirmation on 28 July and confirm dependencies with Financial Assistance, Refill, and Benefit Summary owners. |

## 12. Open Questions

1. What are the confirmed existing fields and required additions for `Dose_Log__c`?
2. What is the confirmed schema and dose-frequency representation in `Dose_Schedule__c`?
3. Which approved write pattern should be reused for `PHI_Access_Log__c`: helper class or Platform Event?
4. Should `Adverse event` be added as a distinct Reason value, or should such events be captured through the existing Adverse Event Quick Logger?
5. What are the exact Salesforce users, queues, or assignment rules for the primary care manager and Dr. Kunal Bhatt Task recipients?
6. What are the approved mockup details for the default, populated, back-date, and error states?
7. What business behavior applies when the scheduled-dose denominator is zero or unavailable for a rolling window?
8. What is the final operational definition of “consecutive” missed doses when multiple medications or dose schedules apply to a patient?

## 13. Delivery Milestones

| Milestone | Target |
|---|---|
| Technical schema and audit-pattern confirmation | 24 July 2026 |
| Clinical Reason picklist confirmation | 25 July 2026 |
| BRD draft completion | 26 July 2026 |
| Steering Committee governance sign-off | 28 July 2026 |
| Target BRD sign-off | 29 July 2026 |
| Widget UX validation session | Week of 4 August 2026 |
| UAT | 19 August 2026 |
| Go-live | End of Q3 FY26 |

## 14. References

- `Meeting_Note_03_Missed_Dose_Adherence.md`, “Meeting Notes — Missed Dose Logging & Adherence Alerts (Discovery),” 22 July 2026.
- Patient Self-Enrollment TDD, referenced in the meeting note for established PHI controls; document contents were not provided.
- Benefit Summary widget workstream, referenced in the meeting note for release-train alignment; detailed documentation was not provided.
- Medication Refill workstream, referenced in the meeting note for pharmacy dispense reconciliation ownership; detailed documentation was not provided.

## 15. Traceability Summary

This BRD contains functional requirements `BR-01` through `BR-35` and non-functional requirements `BR-36` through `BR-43`, covering structured missed-dose capture, validation and audit, adherence calculation, threshold alerting, de-duplication, scheduled evaluation, usability, compliance, and scale considerations.
