# PLAN.md — Strategic Account Approval Enhancement

## Stack & Rationale
- **Platform:** Salesforce (Apex, Flows, Process Builder, Approval Processes)
- **Language:** Apex (for custom logic), Salesforce declarative tools
- **Notification:** Slack (via integration), Email (Salesforce standard)
- **Data:** Uses existing Account and Opportunity objects; leverages `Strategic_Account__c` field
- **Reporting:** Salesforce Reports/Dashboards

**Rationale:**
- Salesforce is the system of record and workflow engine for Opportunity approvals.
- Apex is required for conditional routing and escalation logic not possible with declarative tools alone.
- Slack/Email for notifications as per CRO preference.

## Module/Component Breakdown
- **Apex Trigger/Flow:** Detects Opportunity submission, checks Strategic Account and TCV criteria
- **Approval Process:**
  - Step 1: Deal Desk (Vikram) if discount >10%
  - Step 2: CRO (Meera) if Strategic Account and TCV >$100K
  - Delegation: CRO can delegate to Chief of Staff (Rhea)
  - Auto-escalation: If CRO does not respond in 72h, escalate to Rhea (with TCV cap)
- **Notification Service:** Sends Slack DM and Email to approvers
- **Reporting:** Dashboard/report for Strategic Account approvals, cycle time, outcomes
- **Config:** List of Strategic Accounts (via `Strategic_Account__c`), delegation rules, thresholds
- **Tests:** Unit tests for Apex logic, integration tests for approval routing

## Target Folder Structure
```
strategic-approval/
  src/
    triggers/
      OpportunityApprovalTrigger.trigger      # Entry point for approval routing
    flows/
      StrategicAccountApprovalFlow.flow       # Declarative flow for main logic
    apex/
      ApprovalRoutingHandler.cls              # Apex class for routing/criteria
      NotificationService.cls                 # Apex class for Slack/Email
      DelegationHandler.cls                   # Handles delegation/escalation
    config/
      strategic_accounts.md                   # Maintainers, field usage, update SOP
      thresholds.md                           # TCV, discount, escalation values
    reports/
      strategic_approval_dashboard.md         # Reporting requirements
  tests/
    test_approval_routing.cls                # Unit tests for routing logic
    test_delegation_escalation.cls           # Unit tests for delegation/escalation
  README.md                                  # Setup, deployment, usage
```

## Build Order & Milestones
1. **Config & Field Validation:**
   - Confirm `Strategic_Account__c` field, access controls, and update SOP
2. **Approval Routing Logic:**
   - Implement Apex trigger/handler for submission detection and routing
   - Build Flow for sequential/conditional approval steps
3. **Delegation & Escalation:**
   - Implement delegation logic and auto-escalation (72h SLA, TCV cap)
4. **Notification Service:**
   - Integrate Slack and Email notifications
5. **Reporting:**
   - Build dashboard/report for Strategic Account approvals
6. **Testing:**
   - Write and run unit/integration tests
7. **Documentation:**
   - README, config SOPs, reporting docs

## Key Risks & Open Questions
- Complexity of conditional approval routing (sequential/parallel logic)
- Slack integration (API access, permissions)
- Ensuring delegation/escalation logic is robust and auditable
- Handling edge cases (e.g., Strategic flag changes post-submission)

## Component/Dependency Diagram

![Diagram](TO_BE_REPLACED_BY_URL)

---

## Mermaid Diagram (Strategic Account Approval Flow)

```mermaid
graph TD
    A[Opportunity Submission] --> B{Is Account Strategic?}
    B -- No --> Z[Standard Approval Process]
    B -- Yes --> C{TCV > $100K?}
    C -- No --> Z
    C -- Yes --> D{Discount > 10%?}
    D -- Yes --> E[Deal Desk Approval (Vikram)]
    D -- No --> F[CRO Approval (Meera)]
    E -- Approved --> F
    E -- Rejected --> Y[Rejected to Rep]
    F -- Approved --> G[Deal Closed]
    F -- Rejected --> Y
    F -- No Response 72h --> H[Auto-Escalate to Chief of Staff (Rhea)]
    H -- TCV <= $500K --> G
    H -- TCV > $500K --> I[Wait for Meera]
    Z --> G
```

*If diagram rendering fails, use the above Mermaid code in a compatible viewer.*
