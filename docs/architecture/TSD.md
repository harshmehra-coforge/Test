# Technical Specification Document (TSD)
## Missed Dose Logging & Adherence Alerts for CarePath Plus
**Date:** 2026-10-09  
**Version:** 1.0  
**Status:** Draft

---

## 1. Executive Summary

### 1.1 Purpose

This system enables structured capture and alerting of patient medication adherence in CarePath Plus by embedding a rapid-entry missed dose logger for care managers, supporting real-time adherence detection, clinical workflow integration, and audit/compliance requirements. Timelier identification of deteriorating adherence will drive clinical interventions and reduce adverse outcomes tied to delayed detection.

### 1.2 Scope
**In Scope:**
- Salesforce LWC widget for missed dose logging on Care Plan records
- Rolling-window adherence calculation (30/60/90d)
- Rule-based threshold alerting (task assignment, severity deduplication)
- PHI audit logging and enforcement of data access policies
- Daily scheduled reevaluation for silent deteriorations

**Out of Scope:**
- Patient-facing channels (SMS/email reminders, portal)
- Pharmacy dispense data integration
- Hospitalization/admission-driven adherence logic
- Supervisor override UI (admin manual DML only in Phase 1)

### 1.3 Key Decisions
1. **LWC Quick-Entry Widget:** Chosen for sub-15s entry, native to Care Plan page, minimizes workflow friction; alternate patient communication deferred.
2. **Apex Service + Flow for Orchestration:** Chosen over custom triggers for declarative deduplication; avoids Apex for eligibility logic, lowers maintenance/tech debt.
3. **Server + Client Validation:** Both enforced to block >14-day backdates, ensure audit completeness; alternate single-side validation rejected due to compliance risk.
4. **PHI Audit with Existing Pattern:** Reuses established `PHI_Access_Log__c` write structure for regulatory alignment, minimizing new compliance exposure.

---

## 2. System Architecture

### 2.1 High-Level Architecture

**Component Breakdown:**
- **Presentation Layer:** LWC widget (doseLogQuickEntry) on Salesforce Health Cloud Care Plan
- **Application Services:**
    - Apex Service for creation/validation/audit/rolling-window adherence
    - Record-Triggered Flow for alert Task creation, with deduplication logic
    - Scheduled Flow (daily) for adherence reevaluation, catch missed cases
- **Data Layer:**
    - Custom Objects: Care Plan, Dose_Log__c, Dose_Schedule__c
    - Audit: PHI_Access_Log__c
    - Task records

**Architecture Diagram:**

*(Diagram rendering failed; textual flow provided below)*

**Flow Outline:**
- LWC widget captures missed dose data (date/time, medication, reason, notes)
- Apex service validates, inserts Dose_Log__c, logs to PHI_Access_Log__c
- On insertion, Flow checks adherence thresholds, de-duplicates, creates Tasks as needed
- Scheduled Flow (daily) checks 30-day adherence for patients with Dose_Log__c in past 45d

### 2.2 Component Details

- **LWC Widget:** Single-click entry, tab-order optimized, back-dating <=14d, enforced in UI and server (Apex)
- **Apex Service:** Validates, writes Dose_Log__c, calculates rolling adherence for Care Plan, writes PHI audit trail
- **Record-Triggered Flow:** Triggers on Dose_Log__c insert, checks thresholds (80%/60%, consecutive missed), assigns/updates Tasks, implements deduplication:
    - Only one open/highest-severity Task per patient per 24h per threshold
    - Posts comment on open Task if rule already triggered
- **Scheduled Flow:** Daily at 6:30 AM IST, re-evaluates rolling adherence for patients with recent logs, fires Task if new threshold crossed
- **PHI_Access_Log__c:** Captures all Dose_Log__c changes for compliance; enforces no free-text PHI in reason field

---

## 3. System Flows

### 3.1 Primary Business Flow: Missed Dose Logging
1. Care manager (or pharmacist) opens Care Plan, single-clicks 'Log Missed Dose'
2. Enters required data (date/time, auto-medication, reason, notes)
3. LWC widget sends to Apex Service: validates, inserts Dose_Log__c, logs to PHI_Access_Log__c
4. Record-Triggered Flow checks for thresholds, de-duplicates, creates/updates/annotates Task(s)
5. UI displays rolling adherence and latest status

### 3.2 Threshold Alerting Flow
- Dose_Log__c insert triggers Flow
- Evaluates:
    - 30-day adherence <80%: Task to primary CM
    - 3+ consecutive missed in 7d: Task, High priority
    - 30-day adherence <60%: Tasks to Clinical Pharmacist (Dr. Kunal) and CM
- Deduplication ensures only one active Task per patient/threshold/24h
- Existing open Task → post comment rather than duplicate

### 3.3 Scheduled Adherence Reevaluation Flow
- Daily job selects patients with recent Dose_Log__c
- Invokes Apex to recalculate adherence, triggers alert Task(s) if thresholds crossed not already flagged

### 3.4 Error & Retry
- All service insertions retried up to 3x on transient Apex/db error
- PHI audit log failures abort transaction (atomic); UI shows hard error
- Scheduled Flow errors logged to admin queue with SRE alert if >5 consecutive failures

---

## 4. Data Architecture

### 4.1 Entity & Data Model

| Entity             | Storage            | Retention     | Backup               |
|--------------------|-------------------|--------------|----------------------|
| Dose_Log__c        | Salesforce Custom | 5 years      | Daily snapshot/SF default |
| Dose_Schedule__c   | Salesforce Custom | 5 years      | As above                 |
| PHI_Access_Log__c  | Salesforce Custom | 7 years      | As above                 |
| Task               | Salesforce        | 2 years      | As above                 |
| Case/Care Plan     | Salesforce        | 7 years      | As above                 |

**Dose_Log__c Fields:**
- Date/time (datetime, default now, backdate≤14d)
- Medication (lookup to active plan)
- Reason (constrained picklist)
- Notes (free text, 500 chars)
- CreatedBy/LastModifiedBy
- AuditLogId (PHI access log ref)

**Data Flow:**
- Widget --> Apex --> Dose_Log__c (+audit)
- Flows read Dose_Log__c for calculation, check Task entries
- Scheduled Flow traverses Dose_Log__c for rolling status

### 4.2 Data Storage & Compliance
- Retention aligns with medical and regulatory (India, US HIPAA export) needs
- All free-text periodically scrubbed for PHI (quarterly review)
- Backdate/edit enforcement via UI + server, no direct user DML past 24h
- Reason is picklist, not free text (prevents PHI leaks)

---

## 5. Technology Stack

| Component                    | Technology                    | Purpose                                             | Rationale & Alternatives                        |
|------------------------------|-------------------------------|-----------------------------------------------------|-------------------------------------------------|
| UI Widget                    | Salesforce LWC                | Care Plan embedded, rapid data entry                | Fastest for SF native, lowest friction; alt: Aura (deprecated) |
| Backend Service              | Apex Class                    | Validation, adherence calculation, audit            | Native to platform, avoids extra compute/API calls |
| Task/Alert Orchestration     | Salesforce Flow               | Declarative rules, deduplication                    | Easier to maintain, non-code; alt: Apex triggers (rejected) |
| Audit Logging                | PHI_Access_Log__c             | Compliance audit trail                              | Standardized, aligns with org's control framework |
| Scheduling                   | Scheduled Flow                | Daily reevaluation, coverage for silent deteriorations | Codified re-eval logic; alt: external scheduler (not needed) |

## 6. Integration Architecture

### 6.1 Integration Patterns
- UI-to-Apex via Salesforce native API (LWC wire/adapters)
- Apex to custom object insert/audit
- Flows trigger off Dose_Log__c DML, create/annotate Task
- Scheduled Flow invokes batch adherence check

### 6.2 Sample API/Trigger Points
| Trigger/Event             | Description                                       |
|--------------------------|---------------------------------------------------|
| LWC -> Apex: submit       | Creates Dose_Log__c, fires audit, adherence recalc|
| Flow: record-triggered    | Task logic, deduplication                         |
| Flow: scheduled           | Daily job for reevaluation                        |
| Apex: PHI_Access_Log__c   | Compliance log write                              |


## 7. Deployment Architecture

### 7.1 Infrastructure Overview
- All code runs in Salesforce Health Cloud instance (CarePath Plus org)
- LWC deployed to Care Plan Lightning record page
- Flows, Apex code, object model versioned via Salesforce packaging
- No external infrastructure required (meet in-region, in-org compliance)

### 7.2 Scaling, IaC/CI-CD
- User/transaction volumes well within platform limits (4,200 active patients, 165 care managers)
- SF Release Management for all deployments (versioned, auditable)
- Unit/integration tests via Apex/Jest (for LWC)
- Manual UAT in lower sandboxes; prod go-live aligned with Benefit Summary widget train
- Standard Salesforce monitoring (Platform Events, Debug Logs, Field Audit Trail)

---

## 8. Security Architecture

| Layer       | Control                         | Implementation                                            |
|-------------|----------------------------------|-----------------------------------------------------------|
| Network     | SF internal only                | No external access — all on-platform                      |
| Application | Field-level/view perms, FLS     | Profile-based; Care Managers, Pharmacists only            |
| Application | UI validation                   | No over-backdating, strict picklist, char limit           |
| Application | Audit log enforcement           | PHI_Access_Log__c written per insert/update               |
| Data        | Retention & PHI scrubbing       | Quarterly scheduled reviews of Notes field                |
| Data        | No PHI free text in Reason      | Enforced by picklist and LWC validation                   |
| Data        | Audit trail integrity           | Salesforce Field Audit Trail, PHI log review              |
| Data        | Encryption at Rest/In Transit   | Salesforce native encryption (Org-wide)                   |

## 9. Performance & Capacity

| Metric              | Target/Constraint                          | Measurement                     |
|---------------------|--------------------------------------------|----------------------------------|
| Widget Entry Time   | <15 seconds, 99% cases                     | UX test logs                     |
| Adherence Calc p95  | <200ms                                      | Salesforce debug logs, APM       |
| Daily Reevaluation  | <1 hour (4,200 patients), 100% complete    | Job run log                      |
| Alert Duplication   | <1 duplicate per 500 Tasks                  | UAT measurement, prod audit      |
| Availability        | 99.9%                                      | SF Platform Metrics              |
| Compliance Audit    | 100% auditable for PHI log, all changes    | Quarterly internal review        |

Capacity:
- Designed for up to 10,000 patients, 250 concurrent care managers
- Daily logs/alerts: ~100 inserts, ~25 threshold breaches/day
- Storage well within SF limits; expected <2GB/year for Dose_Log__c

Cost:
- Included in SF Health Cloud license; no incremental infra/SaaS cost

---

## 10. Monitoring & Observability

- All Dose_Log__c DML logged via PHI_Access_Log__c audit
- Standard Salesforce platform logging (object history, debug, Error logs)
- Task/alert volumes tracked with scheduled batch for reporting
- Key metrics (entry times, dedupe ratio, adherence calc latency) reviewed quarterly
- SRE notified if scheduled Flow failures >5 consecutive days
- SLO: 99.9% availability, <15s UI entry, 100% audit coverage

## 11. Disaster Recovery

| Scenario                    | RTO   | RPO   | Recovery                                 |
|-----------------------------|-------|-------|------------------------------------------|
| Salesforce Instance Outage  | 1 hr  | 0     | Salesforce org-level HA                  |
| Object Deletion             | 1 hr  | 24 hr | Restore from backup/snapshot             |
| Field Audit/Audit Log Lost  | 2 hr  | 1 hr  | Restore from Field Audit Trail           |
| Data Corruption             | 2 hr  | 4 hr  | Rollback/recover from prior snapshot     |

- All data recoverable via Salesforce native backup/undo
- Internally documented runbook for audit reconstruction

## 12. Implementation Roadmap

| Phase                       | Duration | Deliverables                                 | Dependencies              |
|-----------------------------|----------|----------------------------------------------|---------------------------|
| 1: Object/Field Analysis    | 0.5 wk   | Confirm Dose_Log__c & Dose_Schedule__c fields| None                      |
| 2: LWC/UX Design            | 1 wk     | Widget functional mockups, tab-flow spec      | Phase 1                   |
| 3: Apex Service/Validation  | 1.5 wks  | Service class, server-side validation, audit  | Phase 1, 2                |
| 4: Alerting Flow Build      | 1 wk     | Record-triggered Flow, deduplication          | Phase 3                   |
| 5: Scheduled Flow           | 0.5 wk   | Daily job logic, batch adherence reeval       | Phase 4                   |
| 6: UAT                      | 1 wk     | End-user test: 6 nominated CMs, feedback      | Phases 1-5                |
| 7: Go-live                  | ---      | Widget/flow deployment, doc, training         | Phase 6, sign-off         |

Target dates:
- BRD sign-off: 2026-07-29
- UAT: 2026-08-19
- Go-live: End of Q3 FY26

---

## 13. Appendices

**Glossary:**
- LWC: Lightning Web Component (Salesforce UI technology)
- DML: Data Manipulation Language (database inserts/updates)
- PHI: Protected Health Information
- UAT: User Acceptance Testing
- SRE: Site Reliability Engineering

**Testing Strategy:**
- Unit tests for Apex (≥85% coverage for triggers/services)
- Jest tests for LWC behaviors
- Manual UAT by end-users (care managers, 2 hubs)
- Integration tests: all Flow entry/exit points
- Security: quarterly review of free-text fields for PHI leakage

**Risks:**
| Risk                       | Impact   | Probability | Mitigation                                |
|----------------------------|----------|-------------|--------------------------------------------|
| LWC entry >15s             | High     | Med         | Iterative UX testing, single-action workflow|
| Dedupe logic missed        | High     | Low         | Manual/automated UAT review                |
| Salesforce platform limits | Med      | Low         | Monitor volumes, archiving policy          |
| Audit log violation        | High     | Low         | No insert w/o audit, field-level controls  |
| Reason field PHI           | High     | Med         | Picklist, periodic review, SRE alerts      |

**References:**
- Salesforce Health Cloud technical docs
- Org field audit/Phi audit controls
- Adherence workflow workshop (meeting notes 03, 2026-07-22)
- CarePath Plus program reporting SOPs

---

TSD generated successfully.
Key highlights:
- Lightning Web Component for rapid missed dose logging within Care Plan, sub-15s entry
- Rule-driven alerting with deduplication (only one task/patient/threshold/24h), all via low-code Flows
- Complete field-level audit, compliant with PHI retention and review
- Scalable design (10,000+ patients), proven fit with 4,200 live patients
- Zero incremental cost/runout risk: leverages existing Salesforce instance and objects
