# Technical Specification Document

## Discount Approval Enhancement for Salesforce Opportunity Management

**Date:** 2026-10-12  
**Version:** 1.0  
**Status:** Draft

---

## 1. Executive Summary

### 1.1 Purpose
This document specifies the technical architecture for enhancing the Opportunity discount approval workflow in Salesforce by introducing a two-tier sequential approval for discounts greater than 25%. The goal is to reinforce governance, auditability, and SLAs, while maintaining existing processes for lower discounts and increasing Finance visibility.

### 1.2 Scope
**In Scope:**
- Sequential, two-tier approval (Deal Desk → VP Sales) for Opportunity discounts >25%.
- Automatic, managed notifications for Finance on high-value discounts.
- SLA enforcement and auto-escalation within the approval workflow.
- Full approval chain restarts on rejection and centralized audit logging.

**Out of Scope:**
- Changes to existing single-tier approval (10-25%).
- Additional approval tiers or new user interfaces outside Salesforce.
- Changes to non-Opportunity objects.

### 1.3 Key Decisions
1. **Salesforce-Native Workflow Orchestration:** Used standard Approval Process, Apex, and Flow for maintainability and low TCO. Alternative: custom middleware, but increases complexity.
2. **SLA & Escalation Automation:** Leverages Apex Scheduled Jobs for time-bound escalation, ensuring compliance without manual tracking. Trade-off: higher governor limit usage.
3. **Notification Integration:** Employs built-in Salesforce email/app notifications to minimize integration complexity. Alternative: Slack/Teams for Finance, but more config overhead.
4. **Audit-First Logging:** All approval, rejection, and escalation events are captured centrally for compliance and audit, supporting regulatory requirements.
5. **Minimal Surface Impact:** LWC/Flow enhancements are targeted to approval/rejection UX only, limiting risk to Opportunity screens.

---

## 2. System Architecture

### 2.1 High-Level Architecture

**Components:**
- Lightning Web Components (LWC) on Opportunity record page for user-initiated approvals
- Salesforce Flow for orchestrating sequential approval logic and rejection restarts
- Apex triggers/classes for SLA tracking, escalation, and notification dispatch
- Salesforce Approval Process engine managing core approval steps and chain restarts
- Automated email/app notifications for Finance and escalation
- Data storage using existing Salesforce objects (Opportunity, ProcessInstance, ProcessInstanceStep)
- Audit logging via custom object or enhanced Field History/Process Audit Trail

*(Architecture diagram follows in next section; diagram rendering deferred due to temporary tool limitation)*

### 2.2 Component Descriptions
| Component               | Technology         | Purpose                                                         |
|-------------------------|-------------------|-----------------------------------------------------------------|
| LWC Approval Widget     | LWC               | Initiate/review approvals and display status/comments            |
| Approval Orchestration  | Salesforce Flow   | Orchestrate sequential tiers, restarts, notifications            |
| SLA Handler             | Apex Scheduled    | Track timeouts, trigger escalation to delegate                   |
| Approval Engine         | Salesforce Native | Route approvals, enforce order, chain restart on rejection      |
| Notification Service    | Apex/Email        | Notify Finance and approvers, with SLA/escalation logic         |
| Audit Logging           | Custom/AuditTrail | Persist approval, rejection, escalation, and notification events |
| Data Storage            | Salesforce Objects| Store Opportunity, approval state, steps, comments, audit data  |

### 2.3 Architecture Diagram

*(Diagram unavailable due to rendering constraint, but intended structure: User interacts with LWC/Flow → Apex Controller → Approval Engine → SLA/Escalation Handler → Notifications/Audit)*

---

## 3. System Flows

### 3.1 Primary Approval Flow
- User (Deal Owner) submits Opportunity for discount approval (>25%).
- Approval routed to Deal Desk for first-level decision (24h SLA).
- If approved, routed to VP Sales for second-tier sign-off (48h SLA).
- If VP Sales acts within SLA:
    - On approve: opportunity closed/won process continues.
    - On reject: Opportunity returned to Deal Owner with combined comments.
- If VP Sales SLA exceeded: auto-escalate to Executive Assistant.
- Finance notified by email upon second-tier trigger (FYI only).
- Any rejection restarts the full approval chain after edit/resubmission by Deal Owner.

### 3.2 Supporting Flows
- **Rejection and restart**: On rejection at any tier, all prior comments are bundled, Opportunity must be edited and resubmitted (approval chain restarts from Deal Desk).
- **Finance notification**: Automated notification sent at point of second-tier trigger; no action required by Finance.
- **Escalation**: If VP Sales does not act in 48h, approval delegated to Executive Assistant; appropriate notifications sent and logged.

### 3.3 Error and Retry Handling
- Approval orchestration via Flow and Apex ensures failures (e.g., notification delivery) are logged, retried up to 3 times with exponential backoff.
- Notification failures and SLA tracking errors escalate to a system admin queue for manual intervention.
- All audit records preserved during errors for post-mortem analysis.

---

## 4. Data Architecture

### 4.1 Entity/Data Model
| Entity                  | Attributes/Fields                                                          |
|-------------------------|----------------------------------------------------------------------------|
| Opportunity             | Discount__c, Approval_Status__c, OwnerId, StageName, Amount, Description, Second_Tier_Approval_Triggered__c, Second_Tier_Approval_Comments__c |
| ProcessInstance         | TargetObjectId (Opportunity), Status, LastActorId                          |
| ProcessInstanceStep     | ProcessInstanceId, StepStatus, ActorId, Comments                            |
| Audit Log (custom/Trail)| EventType, Timestamp, UserId, Details, RelatedObjectId                      |

### 4.2 Data Storage Strategy
- **Opportunity:** Enhanced with boolean and comments fields for second-tier tracking and context.
- **Approval Chain:** Uses ProcessInstance/ProcessInstanceStep standard Salesforce objects.
- **Audit Trail:** All approval, rejection, escalation, and notifications appended to native or custom audit object for compliance (retention: 7 years).
- **Notification events:** Use existing Salesforce event/message objects for Finance/escalations.

| Data Type     | Storage Object            | Retention    | Backup Strategy                 |
|---------------|--------------------------|--------------|----------------------------------|
| Approvals     | ProcessInstance          | 7 years      | Salesforce standard backups      |
| Steps/Comments| ProcessInstanceStep      | 7 years      | Salesforce standard backups      |
| Audit Events  | Custom/AuditTrail        | 7 years      | Export to secure data lake monthly |
| Finance Notif | AppvlWorkItemNtfcnEvent  | 2 years      | Salesforce standard backups      |

---

## 5. Technology Stack
| Component             | Technology          | Purpose                              | Rationale & Alternatives                           |
|-----------------------|---------------------|--------------------------------------|----------------------------------------------------|
| UI/Workflow           | LWC, Salesforce Flow| UX + approval orchestration          | Native, upgradable; alt: Aura, more legacy upkeep  |
| Approval Logic        | Apex, Approval Proc | Sequential routing/SLA/restart       | Standard, maintainable; alt: custom, higher admin  |
| Notifications         | Apex, Standard notif| Finance/escalation alerts, SLAs      | Uses built-ins, no new dependencies                |
| Audit Logging         | AuditTrail/Custom   | Regulatory compliance                | Native supports most audit needs                   |
| Data Model            | Opportunity std obj | Discount mgmt, approval status       | Maximizes reuse, preserves reporting               |

---

## 6. Integration Architecture
- Native Salesforce integration for all approval and notification events.
- No external API endpoints needed; internal Slack/Teams integration possible via Apex (future option).
- Notifications via Salesforce standard email/app notification for Finance, escalation, and SLAs.
- Approval initiation and status via LWC component using Apex controller.
- All events logged to central audit object/standard audit trail.

| Integration         | Method         | Description                                           | Auth                | Rate Limit   |
|---------------------|---------------|-------------------------------------------------------|---------------------|--------------|
| Approval Submission | Apex/LWC       | Starts approval, posts status/comments                | Salesforce session  |  N/A         |
| Notifications       | Apex/Email/App | Automated Finance & SLA notifications                 | Salesforce internal |  N/A         |
| Audit Trail         | Custom/Audit   | Persist approval/rejection/escalation events          | N/A                 |  N/A         |

---

## 7. Deployment Architecture
- All logic and configuration deployed within Salesforce (no external hosting needed).
- Apex scheduled jobs for SLA monitoring and escalation.
- Standard migration process (sandbox → UAT → production) using Salesforce change sets.
- No additional infrastructure or cloud resources required beyond Salesforce licensing.
- Testing uses full environment copies; rollback via change sets.
- Scaling leverages Salesforce platform SLAs (99.9% uptime; capacity managed by Salesforce).

---

## 8. Security Architecture
| Layer          | Control                  | Implementation                                    |
|--------------- |--------------------------|---------------------------------------------------|
| Application    | Field-level security     | Controlled for new/modified fields via profiles    |
| Application    | Approval access          | Only designated users/roles may approve/escalate   |
| Data          | Audit/immutability        | Approval/AuditTrail objects are append-only        |
| Data          | Encryption               | Platform encryption at rest + in transit enabled   |
| Network       | Standard Salesforce      | All user access via secured, managed cloud infra   |

---

## 9. Performance & Capacity
| Metric                | Target                                   | Measurement                          |
|-----------------------|------------------------------------------|--------------------------------------|
| LWC Approval Launch   | <1s (p95) per user event                 | Salesforce debug logs                |
| Approval Processing   | <2s (p95) workflow routing               | Apex debug + Approval audit logs     |
| VP Sales SLA Breach   | <1min to auto-escalate                   | System debug logs                    |
| Availability          | 99.9% (Salesforce SLA)                   | System monitoring                    |
| Audit Log Retention   | 7 years                                  | Data lake exports / SFDC reporting   |

### Capacity Planning:
- Expected submissions: up to 500 high-discount Opportunities per month.
- Concurrent approvals: up to 10 per hour in peak sales periods.
- Notification throughput: up to 20/min (well within Salesforce limits).
- Storage: <5MB annual incremental growth (overhead negligible).

### Cost Model:
| Resource              | Tier/Notes                | Monthly Cost (USD)          |
|-----------------------|---------------------------|-----------------------------|
| Salesforce Platform   | Enterprise/Custom Objects | Covered by existing license |
| Custom/Audit Logging  | Data Export Storage       | $30/month (data lake export)|
| Notification/Email    | Standard/Included         | No additional cost          |
| **Total**             |                           | **$30/month (incremental)** |

---

## 10. Monitoring & Observability
- All approval activity, SLAs, and notifications logged in Salesforce audit trail or custom audit object.
- Notification failures, auto-escalation, and error states generate system admin alerts via Salesforce app notification.
- Weekly dashboard for compliance: SLA violations, escalation rates, Finance notification delivery, and approval cycle times.
- Key process SLOs:
    - 99.9% availability (Salesforce uptime)
    - 0 missed SLA escalations/month
    - 100% notification delivery (with admin alert on failure)

---

## 11. Disaster Recovery
| Scenario                      | RTO    | RPO     | Recovery Procedure                                |
|-------------------------------|--------|---------|--------------------------------------------------|
| User/admin error (config/API)  | <1h    | <1h     | Restore via sandbox backup/change set reversion   |
| Opportunity/Audit data loss    | <4h    | <12h    | Restore from Salesforce/exported CSV backups      |
| Full Salesforce outage         | <24h   | <24h    | Wait for Salesforce restore (contractual SLA)     |

All data/backups remain within Salesforce's compliant infrastructure; external data-lake export for audit logs (monthly) ensures redundancy.

---

## 12. Implementation Roadmap
| Phase             | Duration   | Deliverables                                        | Dependencies                  |
|-------------------|------------|-----------------------------------------------------|-------------------------------|
| Phase 1: Design   | 1 week     | Detailed workflow & field design, VP Sales confirmation | BRD, VP Sales input           |
| Phase 2: Build    | 2 weeks    | Flows, Apex classes, approval process, notifs         | Phase 1 complete              |
| Phase 3: Testing  | 1 week     | End-to-end UAT: flows, SLAs, notifications            | Phase 2 complete              |
| Phase 4: Training | 0.5 week   | User training, process/role documentation             | Phase 3 complete              |
| Phase 5: Deploy   | 1 week     | Deploy to production, post-go-live monitoring         | Phase 4 complete              |

---

## 13. Appendices

### 13.1 Glossary
- **LWC:** Lightning Web Components
- **UAT:** User Acceptance Testing
- **SLA:** Service-Level Agreement
- **Audit Trail:** Salesforce native or custom object logging all relevant actions
- **AppvlWorkItemNtfcnEvent:** Standard Salesforce notification event object

### 13.2 Design Best Practices and References
- Approval logic handled with maximum use of declarative (Flow) patterns; Apex only for timer/escalation routines.
- All new objects/fields have strict field-level access controls.
- VP Sales delegate setup enables robust SLA compliance.
- All notifications managed via standard platform mechanisms for reliability.
- Compliance logs retained for 7 years; all notifications are audit-logged.

**References:**
- BRD (5).md (current)
- KB search: Salesforce two-tier approvals, SLA auto-escalation, audit logging best practices
- [1] Meeting_Note_01_Discount_Depth 2.md

### 13.3 Risks
| Risk                                   | Impact   | Likelihood | Mitigation                                                  |
|----------------------------------------|----------|------------|-------------------------------------------------------------|
| VP Sales non-participation/delays      | High     | Medium     | Defined delegate, strong escalation/notification, training   |
| Notification delivery failures         | Medium   | Low        | Use native Salesforce events only, test thoroughly           |
| User confusion on restart behavior     | Medium   | Medium     | Bundle comments, clear instructions, pre-go-live training    |

---