# Technical Specification Document (TSD) — Missed Dose Logging & Adherence Alerts

## 1. Overview
The solution provides a quick-entry widget and backend logic to capture missed dose events, calculate adherence, and trigger care-team alerts within CarePath Plus (Salesforce Health Cloud). Design guarantees PHI compliance and minimizes workflow disruption for care managers.

## 2. Solution Architecture
- **Frontend:** Custom LWC (`doseLogQuickEntry`) on Care Plan Lightning page
- **Backend:** Apex service for logging, validation, adherence calculations, audit log
- **Data:** Dose_Log__c (new/custom), Dose_Schedule__c (existing), PHI_Access_Log__c (existing)
- **Alerting:** Record-triggered Flow for Task creation (de-duplication logic)
- **Scheduled:** Daily Flow for adherence evaluation across cohort

## 3. Key Functional Components
- **Missed Dose Logging:** LWC widget enables rapid entry with enforced field capture; back-date up to 14 days; auto-populate medication; curated Reason picklist; optional Notes
- **Adherence Calculation:** Apex calculates rolling (30d/60d/90d) adherence, using scheduled vs. missed doses from Dose_Schedule__c and Dose_Log__c
- **Alerting:** Flow creates Tasks with severity/priority mapping; de-duplicates to a single Task/patient/24h; scheduled daily re-evaluation detects silent deterioration
- **Compliance & Audit:** Every Dose_Log__c insert writes PHI_Access_Log__c; field-level audit compliance; Notes field flagged for PHI scrub review

## 4. Detailed Requirements
### Data Design
- **Dose_Log__c**
    - Fields: DoseDateTime (datetime), Medication (reference), Reason (picklist), Notes (text, 500 chars), CreatedBy, PatientId
    - Edit rules: care manager edits <24h, supervisor override after
    - Server-side validation: back-date limit, audit log write
- **PHI_Access_Log__c**
    - Logged for every Dose_Log__c insert: user, patient, timestamp, action

### Widget UI (LWC)
- Single-click to open, pre-filled defaults
- Tab-order friendly, quick-submit
- Medication auto-selected from active plan
- Reason picklist locked to curated values
- Notes field optional, flagged for PHI scrub

### Backend Processing
- Apex service logic:
    - Insert Dose_Log__c, validate, log PHI_Access_Log__c
    - Calculate adherence % over 30d/60d/90d
    - Trigger alert if thresholds breached

### Alerting Flows
- Record-triggered Flow:
    - Runs on Dose_Log__c insert
    - Implements de-duplication: queries open Tasks, posts comment if Task exists
    - Priority/severity per thresholds:
        - <80% adherence: Task, priority Normal
        - ≥3 missed doses in 7d: Task, priority High
        - <60% adherence: Task to clinical pharmacist, priority High
- Scheduled daily Flow:
    - Runs at 6:30 AM IST, evaluates rolling adherence for recent cohorts

## 5. Non-Functional Requirements
- **Performance:** Widget load <2s, entry submission <15s (p95)
- **Scalability:** Designed for 4,200+ patients, with 1.5x growth
- **Availability:** 99.9% (Salesforce SLA)
- **Cost:** Minimal incremental, leverages native Salesforce components
- **Security:** PHI controls, audit logging, picklist lock, field scrub

## 6. Integration
- Reuses Dose_Schedule__c, Care Plan, PHI_Access_Log__c objects
- Extensible for Phase 2 outreach and hospitalization logic

## 7. Alternatives & Trade-offs
- **Pure Apex vs. Flow:** Flow with record de-duplication sufficient, avoids custom triggers
- **Patient Outreach:** Deferred — would require SMS/portal, increases compliance risk

## 8. Diagram

## 8. Architecture Diagram
![Missed Dose Logging & Adherence Alerts](https://svgshare.com/i/yXY.svg)

## 9. Compliance & Regulatory Controls
- PHI audit trail on every Dose_Log__c event
- Notes flagged for quarterly PHI review
- Server-side validation blocks back-date >14 days
- Supervisor override on edits after 24h (manual)

## 10. Acceptance Criteria
- <15s missed dose event entry (UAT, 95th percentile)
- Real-time adherence calculation visible in widget
- Alerting flows generate accurate, non-duplicated Tasks
- Audit log entries for every event, compliance verified

## 11. Timeline & Deployment
- Draft BRD: 26 July 2026
- Widget UAT: 19 August 2026
- Go-live: End Q3 FY26