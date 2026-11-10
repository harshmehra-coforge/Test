# Engineering Plan: Missed Dose Logging & Adherence Alert Widget

## Stack & Rationale
- **Platform:** Salesforce Health Cloud (Lightning Web Components, Apex, Flows)
- **Language:** Apex (backend), JavaScript (LWC frontend)
- **Data:** Salesforce Custom Objects (`Dose_Log__c`, `Dose_Schedule__c`, `PHI_Access_Log__c`)
- **Packaging:** Salesforce DX project structure
- **Testing:** Apex test classes, Jest for LWC, manual UAT

**Rationale:**
- Native Salesforce stack ensures compliance, auditability, and seamless integration with Care Plan records and user roles. LWC for fast, accessible UI; Apex for business logic and audit; Flows for alerting and scheduled jobs.

## Module/Service Breakdown
- **LWC Widget (`doseLogQuickEntry`):** UI for missed dose entry, validation, and adherence display
- **Apex Service Layer:** Handles log validation, persistence, audit logging, adherence calculation
- **Custom Objects:**
    - `Dose_Log__c`: Missed dose records
    - `Dose_Schedule__c`: Patient dose schedules
    - `PHI_Access_Log__c`: Audit trail
- **Salesforce Flows:**
    - Adherence Alert Flow: Triggers on new log, applies thresholds, creates/updates Tasks
    - Scheduled Daily Flow: Re-evaluates adherence for recent-log patients
- **Security:** Role-based access, field-level security, audit trail

## Target Folder Structure
```
force-app/
  main/
    default/
      lwc/
        doseLogQuickEntry/         # LWC: Missed dose entry widget
      classes/
        DoseLogService.cls         # Apex: Log handler, validation, audit
        DoseLogServiceTest.cls     # Apex test class
        AdherenceCalculator.cls    # Apex: Rolling adherence logic
        AdherenceCalculatorTest.cls# Apex test class
      objects/
        Dose_Log__c/               # Custom object: Missed dose log
        Dose_Schedule__c/          # Custom object: Dose schedule
        PHI_Access_Log__c/         # Custom object: PHI audit log
      flows/
        AdherenceAlert.flow-meta.xml   # Flow: Alerting logic
        DailyAdherenceCheck.flow-meta.xml # Flow: Scheduled daily check
      permissionsets/
        DoseLogWidgetAccess.permissionset-meta.xml # Widget access
      labels/
        CustomLabels.labels-meta.xml    # UI labels
      staticresources/
        WidgetAssets.resource-meta.xml  # (Optional) widget assets
scripts/
  jest/                                # LWC Jest tests
  uat/                                 # UAT scripts, test data
README.md
```

## Build Order & Milestones
1. Confirm field mapping for `Dose_Log__c`, `Dose_Schedule__c`
2. Scaffold LWC widget (UI, validation, UX)
3. Implement Apex service (log handler, audit, adherence calc)
4. Build Flows (alerting, scheduled)
5. Configure permissions, FLS, audit
6. Write Apex and LWC tests
7. UAT scripts, manual test data
8. Documentation, deployment

**Key Risks/Open Questions:**
- Performance of rolling adherence calculation at scale
- Alert de-duplication logic in Flow (complexity)
- PHI audit log reliability
- UAT feedback on widget usability

## Component/Dependency Diagram
![Diagram](https://mermaid.ink/svg/pako:eNqdk81qwzAMhl9FscMiCqnPDtRpLJQaJZT0GLCbrbRXDLZ-AO1uXv1PLD-PjsnKuiN8VHUR71X5gNL68KECJwIlSncsCD07cQUFDM7Y-Rx2UJkoTzsIyeZxktvdBvyyvuW7vszDNedbjYfq1guyb6dxgvjgW4j3IdX8PB9r4h8FQnk4QnCcCJ_ib2ZJvWRw09loqQssUlNwsoJZ4lZL1KmppQU8O_lEm02LSz41gt4QBX4mvfca_NFPxhYFHHyPYBOV8zljH1nqPj2iipTE9imnPRKu0xd58-RRGMprqyInnZNs9KNOyH6-ySi2po-m2bFUlXvNbPS5wR-nonbCzbvK92iGpaD7SU12NNgBurONHoFRaeir1M4_YDTG7RfrBOkbXGLfUqfoDj0PqgU=)
