# Strategic Account Opportunity Approval Enhancement — Scaffold Plan

## 1. Stack and rationale

- **Platform:** Salesforce Sales Cloud, using Salesforce Metadata API/SFDX source format.
- **Declarative automation:** Record-triggered and scheduled Salesforce Flow for submission routing, Deal Desk handoff, reminders, and SLA escalation. This follows the TSD's native Salesforce design and keeps approval state in the platform.
- **Human approvals:** Native Salesforce Approval Processes for Deal Desk and CRO tiers, preserving delegated approver and approval-history behavior.
- **Programmatic boundary:** Apex service classes provide typed routing, idempotency, snapshot, escalation, notification, and audit contracts where Flow formulas/actions alone are insufficient. Business logic is intentionally stubbed.
- **Configuration:** `Strategic_Approval_Config__mdt` for thresholds, SLA, user references, feature toggle, and notification policy; no hard-coded user IDs.
- **Integrations:** Salesforce Slack integration/Named Credential and email templates/alerts. Salesforce remains authoritative when Slack is unavailable.
- **Testing:** Apex unit-test skeletons plus metadata/configuration test placeholders and a boundary matrix.

## 2. Module breakdown mapped to the TSD

| Scaffold module | TSD responsibilities |
|---|---|
| `routing` | FR-01–05; submission validation, strict thresholds, routing snapshot, and submission-version idempotency. |
| `approval` | FR-04, FR-06, FR-08–09; Deal Desk handoff, CRO approval, rejection/resubmission, delegation and authority ceiling. |
| `sla` | FR-07; 24h/48h reminders, 72h escalation, Rhea ceiling enforcement, scheduled processing. |
| `notification` | FR-10; Slack DM and email dispatch, retry/fallback contracts, operational failure reporting. |
| `audit` | FR-11; immutable action, delegation, escalation, snapshot and correlation audit records. |
| `reporting` | FR-12; monthly Sales Leadership and quarterly CRO/QBR report/dashboard metadata. |
| `config` | Custom Metadata access, feature toggle, environment-specific Meera/Rhea references. |
| `tests` | Boundary, resilience, locking, bulk, duplicate-submit, delegation, and historical snapshot acceptance matrix. |

## 3. Target folder structure

```text
sfdx-project.json                         # SFDX project configuration
README.md                                 # Setup, deployment, and implementation guide
PLAN.md                                   # This engineering plan
force-app/main/default/
  classes/                                # Apex contracts and service stubs
    StrategicApprovalTypes.cls/.cls-meta.xml
    StrategicApprovalRoutingService.cls/.cls-meta.xml
    StrategicApprovalSlaService.cls/.cls-meta.xml
    StrategicApprovalNotificationService.cls/.cls-meta.xml
    StrategicApprovalAuditService.cls/.cls-meta.xml
    StrategicApprovalRoutingServiceTest.cls/.cls-meta.xml
  objects/Account/fields/                 # Strategic Account flag
  objects/Opportunity/fields/             # Commercial and approval snapshot fields
  objects/Strategic_Approval_Execution_Log__c/ # Operational observability object
  customMetadata/                         # Governed thresholds and feature configuration
  flows/                                  # Submission, handoff, SLA, and notification orchestration stubs
  approvalProcesses/                      # Deal Desk and CRO native approval definitions
  email/                                  # Six notification templates
  permissionsets/                         # Sales Ops and approval-admin access
  reports/                                # Monthly and quarterly report metadata
  dashboards/                             # Leadership dashboard metadata
  namedCredentials/                       # Slack integration endpoint contract
  labels/                                 # User-facing validation/error labels
config/                                   # Deployment/configuration notes
  deployment-notes.md
force-app/main/default/tests/              # Metadata test matrix artifact
  strategic-approval-boundary-matrix.md
```

## 4. Dependency direction

`Flow/Approval Process -> Apex service contracts -> Salesforce records/configuration`.
Notification and audit services are downstream side effects and must never determine whether an approval is created. Reports consume persisted Opportunity snapshots and approval history; they do not recalculate historical routing.

![Architecture diagram](https://mermaid.ink/img/pako:eNqdkMFOwzAMhl9l8j2H0H8oJw5QkqRrQ5xA2m1aYpI4JbQxk6b8fE5m2m5dJqS0v9fP7LJxY2qf4wW3Y9iO0x0s5qvQ9qg9z8Q0l0kqLrA1o0Q0p8kYxQnY8a0dQYl6wqVQm8xg3mM5pV9yEwS9r0m1f8rX8Z0c7b8zD0xv9k0mB1x2C3J2wGm0o0Z0qQ1u1fJkV3n9mW1w0s0L3J1h9f3r4q2l8n6v9x0w==)

The renderer was unavailable during generation; the Mermaid source in the TSD and the architecture described above remain the source of truth.

## 5. Build order / milestones

1. **Metadata foundation:** create fields, field-level-security permissions, Custom Metadata, labels, operational log object, and Named Credential placeholder.
2. **Routing contract:** implement typed DTOs and stub `StrategicApprovalRoutingService`; add validation, strict boundary tests, snapshot schema, and idempotency guard.
3. **Native approval wiring:** configure existing Deal Desk preservation, CRO Approval Process, controlled handoff Flow, delegation, locking, and rejection comment validation.
4. **SLA and notifications:** wire Scheduled Flow, 24h/48h reminders, 72h escalation branches, email alerts, Slack action, retries, and operational logging.
5. **Audit and reporting:** enable history tracking, immutable event capture, reports, dashboards, and snapshot-based historical reporting.
6. **Verification and release:** run Apex/Flow tests, 10k submission and 100k pending-record performance tests, UAT matrix, sandbox historical simulation, deployment checklist, and rollback drill.

## 6. Key risks and open questions

- Native Approval Process handoff behavior must be validated in the target org, especially duplicate prevention and approval lock transitions.
- Exact Salesforce metadata names/versions for existing Deal Desk process and Slack actions must be mapped before deployment.
- User references in Custom Metadata require environment-specific deployment mapping for Meera and Rhea.
- Salesforce Flow scheduled-path/query limits must be benchmarked against 100,000 pending records; batch strategy may be required.
- Approval history is platform-managed; confirm the compliance archive export covers delegate/effective approver fields for seven years.
- Confirm whether the operational log is a custom object or platform-event-backed implementation in the production org.
- Confirm notification retry mechanism available in the org (Flow async path, platform event, or queueable Apex).

## 7. Definition of scaffold completion

This scaffold supplies deployable source-format placeholders, typed Apex contracts, native automation metadata stubs, configuration points, test boundaries, and operator documentation. It deliberately excludes production business logic and environment-specific IDs/secrets.
