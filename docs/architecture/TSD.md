# Technical Specification Document (TSD): Discount Approval Escalation Enhancement for Salesforce

## 1. Solution Overview

The solution introduces a two-tier sequential approval process in Salesforce for Opportunities with discounts above 25%, directly addressing governance gaps identified in audits. It integrates automated notifications for Finance and robust SLA management, leveraging Salesforce Flows, custom approval logic, and notification integrations (Email, Slack). Existing single-tier approval (10–25%) remains unchanged.

## 2. Architecture Context and Scope

### 2.1 Scope
- Applies to New Business and Renewal Opportunities in Salesforce with discounts >10%
- Enhances approvals for >25% discounts (Deal Desk → VP Sales)
- Adds Finance notification for deep-discount cases
- Integrates with Salesforce, Slack, and email

### 2.2 Constraints
- Implementation within Salesforce (using Flows or Apex if needed)
- VP Sales/Delegate and Executive Assistant as active users
- Notification must not delay approval chain

## 3. High-Level Architecture Diagram

![Architecture Diagram](diagram-failed-to-render)

## 4. Component Breakdown

### 4.1 Discount Approval Flow (Salesforce)
- **Type:** Salesforce Flow/Process Builder or Apex
- **Logic:**
  - If discount >10% and ≤25%: Route to Deal Desk (existing)
  - If discount >25%: Sequential — Deal Desk → VP Sales
  - On Deal Desk rejection: Return to Deal Owner with comments
  - On VP Sales rejection: Return to Deal Owner with comments from both tiers

### 4.2 Notification Engine
- **Channels:**
  - Email (Deal Desk, VP Sales, Finance)
  - Slack (VP Sales, EA)
- **Triggers:**
  - Finance notification for >25% discounts upon VP Sales assignment
  - SLA start for each approval tier; escalation if threshold breached

### 4.3 SLA Escalation Logic
- **Deal Desk SLA:** 24h
- **VP Sales SLA:** 48h (escalates to EA on breach)

### 4.4 Audit/Tracking Store (Salesforce)
- **Tracks:**
  - SLA start/stop and breaches (per Opportunity)
  - Approval/rejection decision time stamps
  - Notification delivery tags

## 5. Key Data Models

### 5.1 Opportunity Object Extension
- Field: Discount Percentage (existing)
- Field: Approval Status [Not Started, Deal Desk Pending, VP Sales Pending, EA Escalated, Approved, Rejected]
- Field: Approval Chain Summary (with comments)
- Field: SLA Timestamps

### 5.2 Notification Log (Custom Object)
- Opportunity reference
- Notified to (User/Group)
- Channel [Email/Slack]
- Notification time/status

## 6. API, Integration, and Automation Design

- Salesforce Flow / Apex handles main process
- Slack notification: Use Salesforce Slack App or Azure Logic Apps endpoint
- Email via Salesforce email integration
- Notification to Finance: Address group email/Slack channel configured per Finance team
- SLA and Escalation: Time-based workflow triggers (Flow, Scheduled Apex, or Azure Logic Apps if external needed)

## 7. Non-Functional Requirements (NFRs)

- **Approval Latency:**
  - Deal Desk: <24h p99
  - VP Sales: <48h p99 (auto-escalate if exceeded)
- **System Availability:**
  - Approval workflow available 99.9% during business hours (M-F, 8am–6pm user time)
- **Notification Delivery:**
  - Email/Slack delivery attempts within 60 seconds of workflow trigger
- **Audit Logging:**
  - 100% of approval chains with >25% discount logged to audit record, retrievable for 7 years
- **Cost Impact:**
  - <2% incremental Salesforce automation cost vs current baseline (for added workflow steps)
- **Security & Compliance:**
  - Only authorized approvers/Finance may access deep-discount approval data (role-based visibility enforced in Salesforce)
  - All notification endpoints (Slack, Email) integrated via approved connectors, secured with OAuth or managed identity

## 8. Security, Identity, and Audit

- All approval decisions, comments, timestamps, and notification events are audit-logged (Salesforce custom object)
- Managed identities used for Slack & Email integrations (no hardcoded secrets)
- Salesforce field-level security restricts sensitive data to approvers, Finance, Deal Desk, and admins
- Access to audit logs restricted to Sales Ops, Legal, and Audit per RBAC

## 9. Alternatives Considered & Trade-Offs

### 9.1 Parallel Approval (vs Sequential)
- *Rejected*: VP Sales only reviews post Deal Desk screening; avoids executive overload on marginal cases (10–25%). Sequential process keeps VP focused on strategic deals.

### 9.2 Manual Finance Review (vs Notification Only)
- *Rejected*: Audit/Finance only need visibility, not blocking power. Automated notification meets reporting/audit needs with no process bottleneck.

### 9.3 External Workflow Engine (vs Salesforce Native)
- *Rejected for Phase 1*: Salesforce Flows/Apex sufficient for SLA/logging/integration. Only consider Azure Logic Apps if future cross-system workflow/signal complexity increases.

## 10. Implementation Plan & Rollout

1. Update Opportunity and Approval objects per data model section
2. Implement/modify Salesforce Flow (deal approval chains, SLA logic, comments, and re-submission handling)
3. Integrate Slack, Email notification steps (Salesforce Apps/Logic Apps as needed)
4. Build audit log/reporting queries for Sales Ops/Finance
5. Validate process in UAT with >25% test Opportunities (incl. SLA breach scenario)
6. Train Deal Desk, VP Sales, Finance, Sales users on new logic/notifications
7. Go-live: End Q2 FY26, with rollback plan to single-tier flow if major breakage

## 11. Rationale & Traceability

- Core workflow aligns directly with audit findings, BRD, and user stories
- Sequential routing maintains compliance, minimizes executive/Finance overload
- Use of native Salesforce automation ensures lowest TCO and fastest path to go-live
- SLA/enforcement choices tied to risk exposure noted in meeting/audit
- Security/notification integrations meet Zero Trust, auditability, and minimal surface area principles

## 12. References

- Meeting_Note_01_Discount_Depth 2.md (source of business/audit context)
- BA-dynamic-agent/BRD.md (requirements)
- PO-dynamic-agent/USER_STORIES.md (user stories and scenarios)
