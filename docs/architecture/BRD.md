# Business Requirements Document (BRD) — Missed Dose Logging & Adherence Alerts

## Executive Summary
CarePath Plus is launching a structured missed-dose logging and adherence alert system to improve medication management for oncology and autoimmune therapy patients. The current approach relies on free-text notes, delaying adherence calculations and clinical intervention. The new solution will enable near-real-time detection of adherence deterioration, prompt care team action, and ensure compliance with PHI audit protocols.

## Business Objectives
- Enable rapid, structured logging of missed doses by care managers and pharmacists
- Calculate patient adherence in real-time and rolling windows (30/60/90 days)
- Trigger timely and de-duplicated alerts for adherence deterioration
- Ensure PHI compliance for all dose logs and audit trail entries
- Support operational UAT with realistic widget mockups and care manager feedback

## Scope
### In Scope
- LWC widget for missed dose quick-entry on Care Plan record
- Backend Apex for logging, validation, audit, and adherence calculation
- Alerting Flow for Task creation with de-duplication rules
- Daily scheduled Flow for rolling adherence evaluation
- Compliance with PHI audit requirements
### Out of Scope
- SMS/portal patient outreach
- Dispense reconciliation (managed separately)
- Hospitalization-driven adherence pause

## User Stories
- As a care manager, I want to log a missed dose on a patient’s Care Plan in less than 15 seconds, so that I capture every event without risking workflow delays.
- As a clinical pharmacist, I want to receive high-priority review Tasks when a patient’s adherence drops below critical thresholds, so I can intervene promptly.
- As a business analyst, I need adherence calculations to be precise, auditable, and available in real-time for program reporting.

## Requirements
### Functional Requirements
1. **Missed Dose Logger Widget**
    - Quick-entry LWC with single-click access, tab-friendly, zero navigation away from Case
    - Fields: Dose date/time (default: now, back-date up to 14 days), Medication (auto-select), Reason (curated picklist), Notes (free-text, 500 chars max)
    - Care managers can edit entries for 24h; after that, supervisor override required (manual handling, phase 1)
2. **Backend Processing & Audit**
    - Apex service validates entries, enforces date limits, and writes PHI_Access_Log
3. **Adherence Calculation**
    - Rolling 30/60/90-day calculation on-demand, uses Dose_Schedule__c and new Dose_Log__c
4. **Alerting Logic**
    - Threshold-based Task creation, de-duplication, no more than 1 Task/day/patient, comments instead of duplicate Tasks where applicable
    - Scheduled daily re-evaluation for missed logs
5. **Compliance & Auditability**
    - All Dose_Log__c inserts audited; Reason picklist locked, Notes field flagged for PHI review

### Non-Functional Requirements (NFRs)
- Entry latency: <2s for widget load, <15s for entry submission (p95)
- Adherence calculation accuracy: 100%, audit log write for every event
- Alerting volume control: max 1 Task per patient/day
- PHI audit compliance: 100% for Dose_Log__c, audit log, Reason picklist
- Scalability: 4,200 active patients, up to 165/patient care managers (future growth factor 1.5)
- Availability: 99.9% uptime for widget and backend (aligned with Salesforce SLA)

## Compliance & Regulatory Requirements
- Any PHI captured, processed, or reported must be logged via PHI_Access_Log__c
- Notes field subject to quarterly PHI scrub
- All controls consistent with org’s compliance policy (Patient Self-Enrollment TDD, Benefit Summary)

## Acceptance Criteria
- Care manager logs missed dose in <15s in >95% cases (UAT)
- Adherence calculation is real-time, accurate, and includes audit trail
- Tasks fire for adherence breaches with no duplicates; highest severity rules enforced
- PHI audit log entries present for every Dose_Log__c creation

## Timeline
- Draft BRD: 26 July 2026
- Widget UAT: 19 August 2026
- Go-live: End of Q3 FY26

## Appendix: Decisions
- Custom LWC widget, backend Apex, Flow-based alerting (with de-duplication)
- Scheduled adherence re-evaluation flow
- Excludes patient outreach, dispense reconciliation, and hospitalization logic (phase 2)
- Sign-off target: 29 July 2026
