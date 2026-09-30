# Business Requirements Document (BRD)

## Project Title
CarePath Plus: Missed Dose Logging & Adherence Alerting (Phase 1)

## Executive Summary
CarePath Plus experiences clinical and operational risk due to unstructured missed dose capture and delayed adherence detection. This BRD defines the requirements for a Salesforce Health Cloud-integrated solution enabling quick, structured logging of missed doses and near-real-time adherence monitoring for oncology and autoimmune cohorts. The solution will improve care manager efficiency and timeliness of clinical intervention through a custom record page widget, on-demand rolling adherence calculations, and care team alerting with de-duplication controls and compliance audit logging.

## Business Objectives
- Eliminate manual, error-prone monthly adherence calculations driven by spreadsheet exports
- Reduce time to clinical intervention by detecting adherence deterioration within days, not weeks
- Ensure all missed doses are captured in a structured, reportable format linked to each Care Plan
- Provide actionable, de-duplicated adherence Tasks to care managers and clinical team
- Meet compliance requirements for PHI-access logging and audit controls

## Scope
**IN SCOPE:**
- Custom Lightning Web Component (`doseLogQuickEntry`) for logging missed doses directly on Care Plan records
- Structured capture of dose date/time, medication, standardized reason picklist, and optional notes
- Care team and pharmacist accessible entry (single-click, 15s or less interaction time)
- Rolling-window adherence calculations (30/60/90 days) driven by scheduled vs. missed doses
- On-demand calculation and alert thresholds: 30-day <80%, 30-day <60%, 3+ consecutive in 7 days
- Automated Task creation, assignment, and de-duplication logic (no duplicates, highest-severity threshold wins)
- Scheduled Flow (daily 6:30 AM IST) for 30-day adherence
- Compliance: audit log entry for every missed dose, curated reasons, back-date/edit controls as described
- Admin-only edits >24h after entry (Phase 1 manual overwrite accepted)

**OUT OF SCOPE (Phase 1):**
- Patient-facing interventions (e.g., SMS, patient portal updates)
- Pharmacy stock/dispense reconciliation
- Hospitalization-driven adherence pause logic

## Functional Requirements

### BR-01: Structured Missed Dose Logging
**Requirement:** Care managers and pharmacists can log a missed dose in ≤15 seconds via a quick-entry widget on the Care Plan Lightning Record Page. Entry captures:
- Dose date and time (defaults to now; user can back-date up to 14 days)
- Medication (auto-populated from active Care Plan)
- Reason (picklist: Forgot, Side effects, Cost / access, Feeling better, Travel, Hospitalization, Other)
- Notes (optional free text, max 500 chars)

### BR-02: Tab-Order and UX Flow
**Requirement:** Entry workflow is single-click to open, tab-order friendly, and does not require navigation away from the Case or Care Plan.

### BR-03: Rolling Adherence Calculation
**Requirement:** For any patient, adherence % is calculated over 30, 60, and 90-day rolling windows as:
`(Scheduled doses − Missed doses) / Scheduled doses × 100`
- Scheduled dose count is sourced from `Dose_Schedule__c` object
- Missed doses from new `Dose_Log__c` records
- Calculation runs on-demand (widget render, new log entry, daily Flow)

### BR-04: Adherence Alerting & Task Creation
**Requirement:**
- If 30-day adherence drops <80%, create Task on Case to care manager (priority: Normal)
- If 3+ consecutive missed doses within 7 days, Task to care manager (priority: High)
- If 30-day adherence drops <60%, Task to Dr. Kunal (Clinical Pharmacist, priority: High) and care manager
- No more than 1 adherence Task per patient per 24 hr; highest-severity threshold applies
- If Task is already open for this patient/threshold, do not duplicate; post comment to existing Task

### BR-05: Audit Logging & Compliance
**Requirement:**
- Every `Dose_Log__c` insert writes to `PHI_Access_Log__c` with user ID, patient ID, timestamp, and action = DOSE_LOG_ENTRY
- Reason is curated picklist (no free text except Notes)
- Back-dating limited to 14 days, enforced client/server
- Entry editable for 24 hr by creator only; >24 hr edits require admin

### BR-06: Scheduled Adherence Reevaluation
**Requirement:**
- A scheduled Flow runs daily at 6:30 AM IST to recalculate adherence for all patients with a `Dose_Log__c` in past 45 days; triggers alerts as in BR-04

### BR-07: System Integration and Security
**Requirement:**
- Solution integrates with Salesforce Health Cloud
- LWC frontend, Apex backend, Flows for automation
- Follows organization’s PHI and security guidelines

## Non-Functional Requirements
- Widget must load and be actionable <2 seconds on Care Plan page
- Adherence calculation must return result in <1.5 seconds per patient
- System supports minimum 4,200 concurrent patient records without degradation
- All access and modifications logged for compliance
- Solution tested for accessibility (tab-order, screen reader)

## Assumptions
- Primary use case is care manager and pharmacist entry; patient self-entry is out of scope
- Current schema is compatible with planned custom objects
- Program stakeholders will sign off by 29 July 2026, UAT on 19 August 2026, Go-Live by end Q3 FY26

## Dependencies
- `Dose_Schedule__c` and `PHI_Access_Log__c` standard in Salesforce org
- Final Reason picklist to be confirmed by clinical team
- Existing admin DML pattern for post-24h edits
- UAT participant nomination by Priti; widget UX validation scheduling

## Risks
- Low adoption if entry flow takes >15 seconds/user
- Excessive alert volume if de-duplication fails
- Integration/compatibility with existing Salesforce objects must be confirmed
- Delay in Reason field finalization or audit process alignment

## References
- Meeting Note: 22 July 2026, “Missed Dose Logging & Adherence Alerts”
- CarePath Plus Patient Self-Enrollment TDD
- Benefit Summary widget BRD and technical design

---

*Prepared by: Rajesh Menon, Salesforce BSA
Business Sponsor: Aditi Kapoor, CarePath Plus Clinical Adherence Lead
Date: 26 July 2026*