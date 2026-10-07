# Technical Specification Document (TSD)

## 1. Overview

The Discount Approval Enhancement Project aims to improve governance and executive oversight for business opportunities offering significant discounts. The project introduces a second-tier approval process for discounts above 25%, sequential routing, robust stakeholder notification, and SLA-driven escalation mechanisms. The solution leverages Salesforce as the process orchestrator, integrating email/Slack notifications and audit trail capabilities. This TSD is grounded in indexed knowledge base requirements, solution architecture descriptions, and business context as captured in Meeting Note 01 (20 June 2026).

## 2. Context and Background

- **Project Sponsor:** Priya Sharma (Sales Operations)
- **Solution Lead:** Karan Verma (Salesforce Admin)
- **BRD Author:** Rajesh
- **Primary Stakeholders:** Deal Desk, VP Sales, Finance, Executive Assistants, Deal Owners
- **Target Date:** BRD sign-off by 26 June 2026; Go-live end Q2 FY26
- **Rationale:** Audit findings reveal insufficient executive oversight of large discounts. Enhancements ensure visibility, process compliance, and mitigate financial risk.

## 3. Scope

**In Scope:**
- Approval flows for New Business/Renewal Opportunities with discount >10%
- Second-tier approval by VP Sales for >25% discounts
- Courtesy notifications to Finance on second-tier triggers
- SLA/resubmission/Audit restart rules
- Stakeholder notifications (email and Slack)
- Salesforce automation and process documentation

**Out of Scope:**
- Discounts ≤10% (remain auto-approved)
- Product catalog, pricing engine changes
- Non-Salesforce CRM integrations

## 4. Requirements

### 4.1 Functional Requirements
- Sequential approval for >25%: Deal Desk → (if approved) VP Sales
- SLA for each tier: 24h (Deal Desk), 48h (VP Sales)
- Automatic escalation to Executive Assistant upon SLA breach
- Retain single-tier approval (Deal Desk only) for 10–25%
- Opportunity record updated with full audit trail, comments captured
- Finance notified on 2nd-tier trigger (no approval required for them)
- End-to-end Slack/email notifications for each stage
- Opportunity returned to Deal Owner on rejection, with chain restart

### 4.2 Non-Functional Requirements
- Maximum process latency: ≤72 hours end-to-end (p95)
- Notification success rate: 99.9%
- Process SLA adherence: 98%+ cases resolved within SLA
- Salesforce workflow uptime: 99.95%
- Audit trail completeness: 100% of approval flows
- Security: All notifications use secure channels (OAuth/SSO for Slack, O365 for email); Only authorized users may act as approvers

---

## 5. Architecture Overview

### 5.1 High-Level Flow

1. Opportunity Created in Salesforce
2. Discount >10% triggers approval process
3. If 10–25%: Routed to Deal Desk only
4. If >25%: Sequential routing—Deal Desk approval, then VP Sales
5. Finance notified if second-tier triggered
6. Notifications sent via email/Slack for each transition/event
7. SLA timers enforced; escalation to Executive Assistant on breach
8. Any rejection restarts the chain from Deal Desk with comments unified for Deal Owner

### 5.2 Diagram

System workflow:
```mermaid
graph TD
    A[Opportunity Created]
    B{Discount > 10\%?}
    C{Discount > 25\%?}
    D[Deal Desk Approval]
    E[VP Sales Approval]
    F[Finance Notification]
    G[Send Notification (Email/Slack)]
    H[Executive Assistant Escalation]
    I[Opportunity Approved]
    J[Opportunity Rejected]
    K[Chain Restart/Comments to Deal Owner]

    A --> B
    B -- No --> I
    B -- Yes --> D
    D -- SLA Breach --> H
    D -- Reject --> J --> K --> D
    D -- Approve --> C
    C -- No --> I
    C -- Yes --> E
    E -- SLA Breach --> H
    E -- Reject --> J --> K --> D
    E -- Approve --> I
    C -- Yes --> F
    G -.-> D
    G -.-> E
    G -.-> F
```
![Workflow Diagram](https://mermaid.ink/img/pako:eNplkEEKgCAMRX8l5WZnYdyVbViLG2i6kKHfYCMQSe5vGJTZw3tsrVwf1KtWeGg6HCEGpM99T4RiHN1yYXce0HkmMYLM4nOJnKjM4grqcDoQQTiB24EClVpDhRSCOq8yNOHR_gO3HH9tkciBf5RThRjKtIolEp8K6oqf4PRt2Hk)

### 5.3 Component Breakdown
- **Salesforce:** Manages opportunity/approval state, routing, audit logging
- **Email/Slack Notification Services:** Send real-time updates/triggers to stakeholders
- **SLA Tracking Engine:** Monitors elapsed time, triggers escalations
- **Finance Notification Module:** Ensures Finance insight on all deep-discount cases

---

## 6. Detailed Solution Design

### 6.1 Salesforce Automation
- Leverage Salesforce Flow or Process Builder for approval routing logic
- Custom Objects or fields for approval tiers, timestamps, SLA tracking
- Triggered Flows for notification payloads (webhooks/integrations)
- Rejection-case logic to combine comments and reset chain on re-submission

### 6.2 Notification Integration
- Email integration via O365 with templated notifications
- Slack integration via secure webhook; fallback to email if Slack fails
- Encrypted notification payloads; PII never included in notifications

### 6.3 SLA & Escalation
- SLA monitored using Salesforce scheduled flows/batch jobs
- On timer expiry, automatic escalation and notification to Executive Assistant role
- SLA counters reset on rejection/restart events

### 6.4 Security & Audit
- All approval actions authenticated (SSO/OAuth)
- Record-level permissions: Only designated approvers see/click their assigned approval
- All approval, rejection, and escalation actions logged in Salesforce Audit Trail
- Comments immutable post-action for audit integrity

---

## 7. Alternatives and Trade-offs

### 7.1 Alternatives Considered
- **Parallel, not sequential, approvals**: Rejected for executive focus—sequential ensures VP Sales reviews only validated requests
- **Manual escalation/notification:** Automated chosen for SLA and audit guarantees
- **Custom notification channel instead of Email/Slack:** Decided against, favoring stakeholder familiarity and zero-change communication

### 7.2 Trade-offs
- **Sequential routing increases process time versus parallel but improves executive bandwidth and governance focus**
- **Full chain restart on rejection can add processing time but ensures audit trail and prevents gaming**
- **Auto-escalation adds minor notification noise but ensures no approval deadends**

---

## 8. Non-Functional Requirements (Expanded & Quantified)
- **End-to-end SLA:** ≤72h (p95) from submission to final approval/rejection
- **Deal Desk SLA:** ≤24h (p99); **VP Sales SLA:** ≤48h (p99)
- **Notification Delivery Success:** 99.9% within 60s of triggering event
- **Escalation Response:** 100% of SLA breaches result in auto-escalation within 10min
- **Audit Trail Accuracy:** All actions (approval, reject, comment, escalate, restart) logged for 100% of flows
- **Security:** SSO/OAuth enforced for all interaction/approval actions; email and webhook traffic TLS 1.2+ encrypted
- **System Availability:** 99.95% monthly

## 9. Implementation, Deployment, and Operations

### 9.1 Implementation
- Salesforce Flows, Process Builder, or Apex (if workflow exceeds declarative limits)
- Email/Slack integration via standard connectors, ensuring fallback between channels
- SLA tracking as scheduled jobs/batch processes in Salesforce
- Full regression and user acceptance testing before go-live

### 9.2 Deployment
- Deploy changes in Salesforce sandbox, UAT, and then production
- Smoke testing at each stage; rollback procedures via Salesforce Change Sets

### 9.3 Operations & Support
- Daily SLA monitoring, alerting stakeholders on systemic SLA trends
- Weekly audit report for Sales Operations
- Security review quarterly by Salesforce Admin

## 10. Appendix
- Knowledge base reference: 'Meeting Note 01, Discount Depth, 20 June 2026'
- Document version: v1.0 (7 Oct 2026)
- Authors: Architect-dynamic-agent (TSD), Rajesh (BRD - source)
