# Business Requirements Document: Strategic Account Approval Enhancement

## Executive Summary
This Business Requirements Document (BRD) defines the requirements for enhancing the Salesforce Opportunity approval workflow for Strategic Accounts, introducing CRO-tier visibility and approval, conditional routing based on deal size and discount, built-in delegation, breach escalation, and robust reporting. The objective is to improve governance and visibility for Strategic Account deals, in alignment with stakeholder preferences and operational best practices.

## Business Objectives
- Improve visibility and control over Strategic Account deals.
- Enable CRO-tier approval for high-value deals.
- Ensure approvals are consistent, auditable, and aligned with business-defined triggers.
- Support delegation for CRO tier.
- Establish clear breach escalation and reporting requirements.

## Scope
**In Scope:** 
- Enhancements to Salesforce Opportunity approval process for accounts designated as Strategic (`Strategic_Account__c` field).
- CRO-tier approval for Strategic Account Opportunities above $100K TCV.
- Delegation functionality for CRO approval tier.
- SLA and breach escalation for CRO tier.
- Slack and Email notifications for CRO tier.
- Quarterly and monthly reporting requirements for Strategic Account approvals.

**Out of Scope:** 
- Changes to non-Strategic Account approval processes.
- Modifications to opportunity submission mechanics outside approval routing.

## Functional Requirements

### BR-01: Strategic Account Trigger
- For any Salesforce Opportunity where Account has `Strategic_Account__c` set to true, initiate enhanced approval workflow at submission time.

### BR-02: CRO Approval Tier
- If Opportunity TCV > $100,000, assign Meera Krishnan (CRO) as second-tier approver.
- CRO approval triggers regardless of Opportunity discount applied (0%+).
- If Opportunity TCV ≤ $100,000, no CRO approval required.

### BR-03: Approval Routing Logic
- If Opportunity meets Deal Desk discount trigger (discount >10%), existing Deal Desk approval (Vikram Patel) fires first.
    - CRO approval follows Deal Desk approval (sequential).
- If Opportunity does not meet Deal Desk discount trigger, CRO approval fires directly (no predecessor).

### BR-04: Delegation
- Allow Meera Krishnan to delegate CRO-tier approval to Chief of Staff (Rhea) from Day 1.
- Delegation to be managed via Salesforce user assignment.

### BR-05: SLA for CRO-Tier Approval
- SLA for CRO approval: 72 hours from request.
- If breached (approval not given in 72 hours), auto-escalate to Chief of Staff (Rhea).
- Chief of Staff (Rhea) may approve if TCV ≤ $500,000. Above $500,000, must wait for CRO.

### BR-06: Notifications
- Send approval request notifications to Meera Krishnan via Slack DM and Email.

### BR-07: Rejection Handling
- If CRO tier rejects Opportunity, return to sales rep with CRO comments.
- Resubmission restarts routing/approval process (entire chain).

### BR-08: Reporting
- Provide quarterly report of Strategic Account Opportunities, approver decisions, and approval cycle times for QBR.
- Provide monthly summary for Sales Leadership.
- Reports to be displayed in Strategic Accounts dashboard.

### BR-09: Change of Strategic Designation Post-Submission
- If Account is flagged as Strategic after Opportunity submission, no retroactive approval routing. Applies only to subsequent Opportunities.

### BR-10: Field Governance
- `Strategic_Account__c` field is write-access restricted to Sales Ops team. Reps have view-only.

## Non-Functional Requirements
- Workflow must execute reliably with high accuracy; no approval steps lost or bypassed.
- Response times: Approval SLA and notification must be strictly adhered to.
- Security: Role-based access enforced for delegation and field updates.
- Auditability: All approvals, escalations, rejections logged for audit; traceable history retained.

## Assumptions
- All 38 Strategic Accounts are correctly designated via `Strategic_Account__c` field.
- Sales Ops continue to own and maintain Strategic designation.
- Slack and Email are supported as notification channels.
- CRO and Chief of Staff user assignments managed in Salesforce.

## Dependencies
- Salesforce workflow customization.
- Notification integrations for Slack and Email.
- Accurate maintenance of Strategic Account field by Sales Ops.
- Existing Deal Desk approval workflow operational and unchanged for non-Strategic Accounts.

## Risks
- Delegation not implemented from launch may delay approvals.
- SLA breaches if CRO is unavailable and escalation not functioning as required.
- Incorrect Strategic Account designation may yield false negatives/positives in approval routing.
- Reporting accuracy dependent on approval history logging.

## References
- Meeting_Note_03_Strategic_Account (24 June 2026)
- Existing Salesforce Opportunity approval workflows
