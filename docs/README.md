# CarePath Plus Missed Dose Logging & Adherence Alerts

Salesforce Health Cloud scaffold for the Phase 1 `doseLogQuickEntry` capability described by the approved TSD. It is intentionally a contract-first scaffold: implementation teams must complete org-specific fields, schedule semantics, audit integration, Flow metadata, and tests before deployment.

## Included

- Care Plan record-page LWC for structured missed-dose entry.
- Apex service, DTO, selector, adherence, threshold, and PHI-audit seams.
- Record-triggered and scheduled Flow placeholders.
- Salesforce DX project metadata, permission set, layout, package manifest, CI validation, and test skeletons.

## Prerequisites

- Salesforce CLI (`sf`) authenticated to a Health Cloud sandbox.
- Node.js and Salesforce LWC Jest dependencies for UI tests.
- Confirmed API names and relationships for `Dose_Log__c`, `Dose_Schedule__c`, `PHI_Access_Log__c`, Care Plan, Patient, Case, and Task.

## Local setup

```bash
sf org login web --alias carepath-scratch
sf org create scratch --definition-file config/project-scratch-def.json --alias carepath-scratch --set-default
sf project deploy start --source-dir force-app --target-org carepath-scratch
```

Do not use real patient data in scratch, CI, logs, fixtures, or local telemetry.

## Implementation order

1. Complete schema and compliance confirmation; replace placeholder selector fields.
2. Implement Apex validation, authorization, CRUD/FLS checks, audit transaction, bounded adherence, and update-window behavior.
3. Implement LWC date validation, accessible states, refresh behavior, and non-PHI errors.
4. Build the two Flows: after-insert alert orchestration and 06:30 Asia/Kolkata targeted reevaluation.
5. Add Flow/Apex/LWC/integration/security/performance tests and validate with six UAT care managers.
6. Deploy through Salesforce DX with pilot permission assignment and rollback readiness.

## Commands

```bash
sf project deploy start --source-dir force-app --target-org carepath-scratch
sf apex run test --target-org carepath-scratch --test-level RunLocalTests --wait 30
npm test -- --runInBand
```

## Non-goals

Patient SMS/portal outreach, pharmacy dispense reconciliation, hospitalization pause logic, cross-widget signals, full-population render recalculation, and supervisor-override UX are Phase 2 or operational follow-up and are not scaffolded.

See `PLAN.md` for architecture mapping, target tree, milestones, risks, and open implementation decisions.
