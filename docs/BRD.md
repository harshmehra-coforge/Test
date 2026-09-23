# Business Requirements Document (BRD)

## Project: Discount Approval Enhancement

**Module:** Opportunity Management  
**Platform:** Salesforce  
**Surface:** Internal (Apex / LWC / Flow on record pages)

---

## 1. Executive Summary

The Discount Approval Enhancement project improves the Opportunity discount approval process by introducing a second-tier approval for discounts exceeding 25%. The current single-tier approval process for discounts between 10% and 25% will remain unchanged. Sequential approvals will be enforced: first Deal Desk, then escalation to the VP Sales for final approval. Finance will receive courtesy notifications when second-tier approvals are triggered. This enhancement ensures better governance, auditability, and accountability for high-value discounts.

---

## 2. Business Objectives

- Enhance governance for high-value discount approvals by adding an executive approval tier.
- Retain current processes for small- and mid-size discounts.
- Provide Finance improved visibility for compliance and reporting.
- Enforce SLAs to prevent process delays.
- Restart approval flow on any rejection to ensure process integrity.

---

## 3. Scope

### In Scope
- Second-tier approval for Opportunity discounts greater than 25%.
- Sequential approval workflow: Deal Desk → VP Sales.
- Courtesy notifications to Finance when second-tier approval is triggered.
- SLA enforcement: 24h for Deal Desk, 48h for VP Sales approver, with auto-escalation.
- Restart of full approval chain on rejection at any tier.
- Capture and relay all approver comments on rejection.

### Out of Scope
- Changes to existing single-tier approval (10%-25%).
- Modifying SLA for Deal Desk.
- Additional approval tiers.
- Guest user/Experience Cloud/community requirements.

---

## 4. Functional Requirements

| **ID**   | **Requirement**                                                                                   |
|----------|---------------------------------------------------------------------------------------------------|
| BR-01    | Implement a second-tier approval process for Opportunity discounts greater than 25%.              |
| BR-02    | Route approvals sequentially: Deal Desk first, then VP Sales executive sign-off.                  |
| BR-03    | Notify Finance via email whenever second-tier approvals are triggered.                            |
| BR-04    | Enforce SLA: 24h (Deal Desk, unchanged), 48h (VP Sales, new).                                    |
| BR-05    | Auto-escalate to VP Sales Executive Assistant if VP Sales SLA is breached.                       |
| BR-06    | Upon rejection, restart the full approval chain from Deal Desk.                                  |
| BR-07    | Capture and display combined comments from Deal Desk and VP Sales upon rejection.                |

---

## 5. Non-Functional Requirements

- Solution must leverage existing Salesforce objects and approval framework (Apex, LWC, Flow).
- All notifications (Finance, escalation) must be automated via standard Salesforce email/app notification mechanisms.
- Process must be auditable and all approval/rejection actions logged for compliance.
- System performance must not degrade existing Opportunity record handling.
- Solution must be compatible with mobile and desktop Salesforce Lightning interfaces.

---

## 6. Business Rules

| **ID**   | **Business Rule**                                                                                 |
|----------|---------------------------------------------------------------------------------------------------|
| BR-08    | Discounts ≤10%: No approval required (existing process).                                          |
| BR-09    | Discounts >10% and ≤25%: Deal Desk approval only (existing process).                              |
| BR-10    | Discounts >25%: Requires sequential approval: Deal Desk then VP Sales.                            |
| BR-11    | If VP Sales rejects, Opportunity returns to Deal Owner with all comments.                         |
| BR-12    | Deal Owner must edit and re-submit, which restarts the approval chain from Deal Desk.             |
| BR-13    | Finance receives courtesy notification on all >25% discount approvals.                            |
| BR-14    | Auto-escalate to VP Sales Executive Assistant after 48h with no VP Sales action.                  |

---

## 7. Data Requirements

### Existing Objects/Fields
- **Opportunity**               
  - `Discount__c`  
  - `Approval_Status__c`  
  - `OwnerId`  
  - `StageName`  
  - `Amount`  
  - `Description`
- **ProcessInstance**  
  - `TargetObjectId`  
  - `Status`  
  - `LastActorId`
- **ProcessInstanceStep**  
  - `ProcessInstanceId`  
  - `StepStatus`  
  - `ActorId`  
  - `Comments`

### New/Enhanced Fields
- **Opportunity**  
  - `Second_Tier_Approval_Triggered__c` (Checkbox)
  - `Second_Tier_Approval_Comments__c` (Long Text Area)
- **Notifications**  
  - Use existing `AppvlWorkItemNtfcnEvent` for Finance S2 notifications and SLA escalations

---

## 8. Assumptions

1. VP Sales is available and agrees to serve as the executive approver for >25% discounts.
2. VP Sales may nominate a delegate (Executive Assistant) for escalations.
3. `Discount__c` field is always accurate.
4. Finance notifications are FYI only; no action/response required.
5. Deal Desk SLA is unchanged (24h).

---

## 9. Dependencies

- Confirmation from VP Sales on participation and delegate/backup.
- Salesforce admin availability for Apex/Flow configuration and production deployment.
- Process documentation delivered to users before rollout.
- Timely resolution of all open questions before sign-off.

---

## 10. Risks & Mitigations

- Risk: VP Sales does not confirm participation/delays.  
  Mitigation: Defined delegate, auto-escalation flow, clarify escalation path in training.
- Risk: Notification email failures.  
  Mitigation: Leverage standard Salesforce notification events, test thoroughly in UAT.
- Risk: User confusion on process restart.  
  Mitigation: Clear rejection comments bundled, user training before go-live.

---

## 11. References

- Discount Approval Enhancement stakeholder meeting note 2026-06-18
- Salesforce Opportunity Management documentation
- [1] Meeting_Note_01_Discount_Depth 2.md

---