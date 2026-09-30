# Business Requirements Document (BRD)

## Executive Summary

CarePath Plus requires a streamlined, compliant, and auditable "Missed Dose Logging & Adherence Alerting" feature within Salesforce Health Cloud. The solution aims to reduce manual reporting delays, enable timely intervention for high-risk patients, and support clinical and operational efficiency. This feature will capture missed doses via a quick-entry UI, calculate adherence per clinical protocols, and automate care team alerts while minimizing noise and ensuring regulatory compliance.

## Business Objectives

- Enable care managers and pharmacists to log missed doses in under 15 seconds from the Care Plan record page.
- Automate rolling adherence calculation (30/60/90 days) to replace manual spreadsheet processes.
- Trigger timely, right-priority alerts to care teams and clinicians, governed by severity and deduplication rules.
- Ensure compliance with patient data privacy (PHI), audit trails, and user access controls.
- Minimize care manager time away from core workflow (inline/embedded app, single-click access).

## Business Scope

### In Scope
- Salesforce Health Cloud LWC widget for dose logging on Care Plan record
- Backend logic (Apex) for validations, audit logging, rolling adherence calculation
- Flow automation for alerts, deduplication, and daily re-evaluation
- UI/UX for quick entry (15s target per missed dose)
- Edit-within-24h by same user, supervisor override process (Phase 2)
- Compliance: audit record, curated Reason picklist, Notes PHI review

### Out of Scope
- Patient-facing reminders or outreach (deferred to Phase 2)
- Hospitalization-driven adherence pause (deferred to Phase 2)
- Pharmacy dispense reconciliation (handled elsewhere)

## Functional Requirements

### BR-01: Missed Dose Log Entry
- Capture: Date/Time (default=now, backdate<=14 days), Medication (auto), Reason (picklist), Notes (optional, 500 chars).
- Entry via LWC on Care Plan page; tab-friendly, single-click open; no page navigation required.
- Fails if backdating >14 days; enforce in UI and server.
- Notes field PHI-flagged for quarterly review.

### BR-02: Adherence Calculation
- Compute adherence % for 30, 60, 90-day rolling windows per patient:
    - Formula: (scheduled doses − missed doses) / scheduled doses × 100
- Calculated on-demand (render of widget; after log entry); never calculated across patient base in batch.
- Data sourced from `Dose_Schedule__c` and new `Dose_Log__c`.

### BR-03: Threshold Alerting & Task Generation
- Severity thresholds:
    - 30-day <80%: normal-priority Task to primary care manager
    - 3+ missed doses in 7 days: high-priority Task to primary care manager
    - 30-day <60%: high-priority Task to Dr. Kunal (clinical), plus normal Task to care manager
- Deduplication:
    - Max one Task per patient per 24h; highest breached threshold wins
    - Open Task for same patient/threshold type: do not create; add comment instead

### BR-04: Compliance & Security
- Every `Dose_Log__c` insert: log to `PHI_Access_Log__c` with user, patient, timestamp, action.
- Reason: curated picklist, no free-text (prevents PHI leakage).
- Care manager edit allowed for same user, within 24h; after that, read-only/edit by admin (manual for Phase 1).

### BR-05: Daily Re-evaluation
- Scheduled Flow (6:30 AM IST): re-check rolling adherence for all patients with a recent log in last 45 days.
- Out-of-compliance triggers appropriate threshold alert.

## Non-Functional Requirements

- Changes adhere to Salesforce platform security and PHI compliance policies.
- Widget designed for sub-15s workflow (UX measured in UAT).
- Alert volume per manager (average caseload 165): daily Task generation capped by deduplication, monitored in UAT.
- Audit trail integrity: audit log written on every entry or edit event.
- Scalability: on-demand calculation only, explicitly not implemented batch-wise over all patients.
- Support for 4,200+ active patients.

## Assumptions

- Dose schedule for each patient is already modeled and reliable for calculations.
- UI component will be embedded on Care Plan Lightning Record Page.
- All users are licensed/certified for PHI access as per org standards.

## Dependencies

- Existing Dose_Schedule__c object integrity
- PHI_Access_Log__c audit implementation
- Flow automation for alerting and daily batch
- Clinical review for Reason picklist (Dr. Kunal)

## Risks

- Under-notification if alert deduplication logic is over-aggressive
- Over-notification if threshold logic is too sensitive
- UI friction could result in fallback to non-structured notes
- Delay if required fields are missing in Dose_Schedule__c object
- Compliance finding if PHI audit not triggered on all events

## References

- Meeting Notes: "Missed Dose Logging & Adherence Alerts" (22 July 2026)
- Patient Self-Enrollment TDD, CarePath Plus
- CarePath Plus Health Cloud implementation documentation

---