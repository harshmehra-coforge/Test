# Technical Specification Document
## Strategic Account Opportunity Approval Enhancement

**Date:** 2026-10-21  
**Version:** 1.0  
**Status:** Implementation-ready design  
**Platform:** Salesforce Sales Cloud with Flow/Apex extension and Slack/email integrations  
**Architecture mode:** LITE (approval enhancement within an existing CRM platform)

## 1. Executive Summary

### 1.1 Purpose
This design adds governed CRO approval and executive visibility for Salesforce Opportunities associated with Strategic Accounts. It closes the current control gap in which a Strategic Account deal can bypass executive review when its discount is 10% or less, including materially sized deals such as a $300,000 TCV Opportunity at 8% discount.

### 1.2 Scope
**In scope:** submission-time routing, Strategic Account eligibility, TCV and discount thresholds, conditional Deal Desk/CRO sequencing, delegation, 72-hour escalation, Rhea's $500,000 authority boundary, Slack/email notifications, rejection/resubmission behavior, audit history, and monthly/quarterly reporting.

**Out of scope:** changing the existing Deal Desk threshold or behavior, retroactive routing of in-flight submissions, Signal workflow integration, new approval tiers, and broader Account governance.

### 1.3 Key Decisions
1. **Use Salesforce Flow as the orchestration boundary.** It integrates natively with Opportunity, Account, Approval Process, history, permissions, and dashboards; Apex is reserved for transaction-safe edge cases and external notification reliability.
2. **Freeze routing inputs at submission.** Capture Strategic flag, TCV, discount, and routing branch in an immutable submission snapshot so later Account or Opportunity edits cannot alter an active approval chain.
3. **Model conditional sequencing explicitly.** Discount above 10% creates Deal Desk first; Deal Desk approval unlocks CRO. Discount at or below 10% skips Deal Desk and creates CRO directly.
4. **Enforce Rhea's authority in the routing service, not only UI.** Any delegated/escalated action with TCV >= $500,000 is rejected server-side and remains assigned to the CRO.
5. **Use Salesforce Approval History as the audit source.** This avoids a duplicate approval ledger while enabling cycle-time and decision reporting.

## 2. Requirements and Traceability

### 2.1 Business Rules
- Strategic eligibility is determined by Account.`Strategic_Account__c`.
- Sales Operations alone can edit the Strategic flag; representatives are read-only.
- CRO approval applies when Strategic is true and TCV is strictly greater than $100,000, regardless of discount.
- Deal Desk approval applies when discount is strictly greater than 10%.
- If both conditions apply, Deal Desk precedes CRO. If only CRO applies, CRO routes directly.
- Deal Desk rejection terminates the chain before CRO.
- CRO SLA is 72 hours. Breach escalates to Rhea.
- Rhea may approve only when TCV is strictly below $500,000; TCV at or above $500,000 remains with the CRO.
- Rejection returns comments to the representative. Resubmission restarts the complete applicable chain.
- A Strategic flag set after submission does not change an in-flight request.

### 2.2 Functional Requirements
| ID | Requirement | Design response |
|---|---|---|
| FR-01 | Evaluate Account Strategic status at submission | Submission Flow reads Account and stores `RoutingSnapshot__c` |
| FR-02 | Require CRO for Strategic TCV > $100K | Eligibility formula uses `Strategic_Account__c = true` and `TCV__c > 100000` |
| FR-03 | Preserve Deal Desk >10% behavior | Existing Deal Desk approval remains the first tier when applicable |
| FR-04 | Route direct to CRO when discount <=10% | Flow creates CRO request without Deal Desk predecessor |
| FR-05 | Support delegation at launch | Salesforce delegated approver configuration is enabled before production release |
| FR-06 | Escalate after 72 hours | Scheduled Flow/Platform Event evaluates due requests and routes eligible requests to Rhea |
| FR-07 | Enforce Rhea limit | Apex invocable guard and approval assignment validation reject TCV >= $500K |
| FR-08 | Notify through Slack and email | Notification adapter publishes both channels with correlation ID |
| FR-09 | Restart chain on resubmission | New submission version and new approval request are created after rejection |
| FR-10 | Report decisions and cycle time | Reports use approval history, request timestamps, decision timestamps, and routing snapshot |

## 3. System Architecture

### 3.1 Component Overview
The solution extends the existing Salesforce transaction rather than introducing a standalone microservice. Salesforce remains system of record and approval authority. A small integration boundary handles Slack notifications; email uses Salesforce's approved notification mechanism. Scheduled escalation is implemented with a Salesforce Scheduled Flow, with an Apex invocable guard for strict authority enforcement.

### 3.2 Logical Components
| Component | Responsibility |
|---|---|
| Account and Opportunity objects | Source of Strategic flag, TCV, discount, owner, and submission state |
| Submission Flow | Validates submission, captures routing snapshot, determines branch, and creates approval request |
| Existing Deal Desk approval | Reviews discounts above 10%; unchanged behavior |
| CRO approval process | Reviews eligible Strategic Opportunities after Deal Desk where applicable, or directly |
| Escalation Scheduled Flow | Finds CRO requests older than 72 hours and initiates controlled escalation |
| Approval Guard Apex action | Prevents unauthorized Rhea decisions for TCV >= $500,000 and validates snapshot integrity |
| Notification adapter | Sends Slack DM and email notifications, with retry and dead-letter logging |
| Approval History | Authoritative record of submission, approver, decision, comments, and timestamps |
| Strategic Accounts dashboard | Monthly leadership and quarterly QBR reporting |

### 3.3 Architecture Diagram
Diagram rendering was unavailable in the architecture tool during generation. The implementation topology is:

`Sales Representative -> Salesforce Opportunity/Account -> Submission Flow -> (Deal Desk when discount >10%) -> CRO Approval -> (Rhea only when escalated and TCV <$500K) -> Approval History/Dashboards`

`CRO Approval -> Notification Adapter -> Slack DM + Email`

### 3.4 Design Trade-offs
- **Flow over a new external workflow engine:** lowest operational cost and native audit/security integration; trade-off is Salesforce-specific orchestration and platform limits.
- **Apex guard over configuration-only validation:** provides server-side enforcement against UI/API bypass; trade-off is custom-code deployment and test maintenance.
- **Salesforce approval history over a custom data warehouse:** immediate reporting with no duplicate source; trade-off is less flexible analytics for long-term historical trends.
- **Parallel Slack and email:** improves executive reach; trade-off is duplicate notifications and the need for delivery monitoring.

## 4. System Flows

### 4.1 Submission and Routing
1. Representative submits the Opportunity for approval.
2. Salesforce validates required fields: Account, TCV, discount, owner, and current submission version.
3. Submission Flow reads `Account.Strategic_Account__c`, `Opportunity.TCV__c`, and `Opportunity.Discount__c`.
4. Flow stores a routing snapshot containing Strategic status, TCV, discount, threshold outcomes, submitter, submission timestamp, and version.
5. If Strategic and TCV > $100,000 and discount >10%, Flow starts Deal Desk approval.
6. If Strategic and TCV > $100,000 and discount <=10%, Flow starts CRO approval directly.
7. If Deal Desk rejects, Flow marks the request rejected and never creates the CRO tier.
8. If Deal Desk approves, Flow creates the CRO approval request.
9. Eligible CRO requests generate Slack DM and email notifications.
10. Approval or rejection is recorded in Salesforce Approval History.

### 4.2 Escalation
A Scheduled Flow runs every 15 minutes. It selects open CRO requests where `DueAt <= NOW()` and no decision exists. For TCV below $500,000, it assigns/escalates to Rhea and records the escalation timestamp. For TCV at or above $500,000, it retains the CRO as the only authorized approver, sends an overdue notification to the CRO and Sales Operations, and records the blocked-delegate reason. The job is idempotent using `EscalationProcessed__c` and a request correlation ID.

### 4.3 Rejection and Resubmission
A rejection stores comments and returns the Opportunity to the representative. Any material correction or explicit resubmission creates a new submission version. The prior request remains immutable for audit, and the new version recalculates all routing conditions from current values. This prevents approval of a version not reviewed by the approver.

### 4.4 Error and Retry Handling
- Salesforce record-lock errors: retry three times with 2, 5, and 15-minute delays; then create an admin task.
- Slack/API transient failures: retry three times with exponential backoff; email remains independently attempted.
- Duplicate submission: reject when an open request exists for the same Opportunity/version.
- Missing TCV or discount: block submission with a field-level error.
- Unauthorized Rhea decision: reject transaction, log security event, and notify Sales Operations.

## 5. Data Architecture

### 5.1 Salesforce Data Model
| Object/field | Type | Purpose |
|---|---|---|
| Account.`Strategic_Account__c` | Checkbox | Sales Operations-controlled eligibility source |
| Opportunity.`TCV__c` | Currency/decimal | CRO threshold and Rhea authority evaluation |
| Opportunity.`Discount__c` | Percentage | Existing Deal Desk threshold evaluation |
| Opportunity.`Approval_Status__c` | Picklist | Draft, Submitted, Deal Desk Pending, CRO Pending, Escalated, Approved, Rejected |
| Opportunity.`Submission_Version__c` | Number | Monotonic version for resubmission isolation |
| Approval Request.`RoutingSnapshot__c` | Long text/JSON | Immutable input and branch outcomes at submission |
| Approval Request.`DueAt__c` | DateTime | Submission time + 72 hours for CRO tier |
| Approval Request.`EscalationProcessed__c` | Checkbox | Idempotent escalation marker |
| Approval Request.`CorrelationId__c` | Text | Cross-system notification and audit correlation |

### 5.2 Data Integrity Rules
- Currency thresholds are evaluated in the Opportunity's corporate reporting currency after Salesforce conversion; the normalized value is saved in the snapshot.
- `TCV__c > 100000` activates CRO eligibility; exactly 100,000 does not.
- `TCV__c < 500000` is required for Rhea authority; exactly 500,000 does not qualify.
- Snapshot fields are not editable by representatives, approvers, or integration users after submission.
- Approval requests are retained with the Opportunity and Salesforce audit history for 7 years to support quarterly board reporting and governance review.

### 5.3 Reporting Measures
- Eligible Strategic Opportunities submitted.
- Deal Desk approval count and rejection rate.
- CRO approval count, rejection rate, and escalation rate.
- Median and p95 approval cycle time, measured from tier creation to decision.
- Count and value of Opportunities at or above $500,000 that breached SLA.
- Resubmission count by Opportunity and approval version.

## 6. Integration Architecture

### 6.1 API and Event Boundaries
No public REST API is introduced. Salesforce Flow invokes a named notification adapter through the approved Salesforce integration mechanism. The adapter accepts an idempotent notification command:

| Field | Requirement |
|---|---|
| `correlationId` | Unique per approval request and channel attempt |
| `opportunityId` | Salesforce Opportunity identifier |
| `requestType` | `CRO_APPROVAL`, `RHEA_ESCALATION`, or `DECISION` |
| `recipient` | Salesforce user mapped to Slack and email |
| `dueAt` | 72-hour deadline for CRO requests |
| `tcv` | Normalized TCV for display and policy checks |
| `approvalUrl` | Deep link to Salesforce approval action |

The adapter returns an accepted status within 2 seconds and processes channel delivery asynchronously. It must treat repeated `correlationId` and channel pairs as duplicates.

### 6.2 Notification Content
Slack and email include Opportunity name/ID, Account, TCV, discount, due date, current tier, submitter, and a Salesforce approval link. Rejection notifications include approver comments. Notifications never include credentials or sensitive tokens.

### 6.3 Security and Integration Failure
Salesforce-to-adapter authentication uses a named credential with OAuth 2.0 client credentials or managed integration identity. Secrets are stored in Salesforce protected configuration or Azure Key Vault if the adapter is hosted in Azure. Delivery failures are written to an integration log and surfaced on the Sales Operations dashboard.

## 7. Deployment and Operations

### 7.1 Deployment Architecture
The primary deployment is Salesforce metadata: Flow definitions, approval-process metadata, custom fields, permission sets, reports, dashboards, notification templates, and Apex guard/test classes. Changes move through Salesforce Dev, QA/UAT, and Production using Salesforce DX packages and GitHub Actions or an equivalent CI pipeline.

If the notification adapter is externalized, deploy it as an Azure Function on a Premium plan behind a private endpoint, with Application Insights, managed identity, Key Vault, and a durable queue for retries. The adapter is not authoritative for approval decisions.

### 7.2 Environments and Release Controls
- Dev: developer sandboxes with synthetic Accounts and Opportunities.
- QA: full-copy or representative sandbox with Slack test workspace and mail sink.
- UAT: business-owned validation by CRO delegate, Deal Desk, Sales Operations, and Platform Admin.
- Production: change set/package promotion after automated tests and approval-process activation review.
- Rollback: deactivate the new Flow version, restore prior active version, and preserve existing approval history; no destructive data migration is required.

### 7.3 Capacity and Limits
The design supports the current 38 Strategic Accounts and the observed 23 quarterly Opportunities with headroom for 10x growth: 230 quarterly Strategic Opportunities and 1,000 approval actions per quarter. Scheduled escalation processes at least 10,000 open requests per run using indexed filters and a cursor/batch pattern. Notification delivery targets 99.9% successful acceptance within 5 minutes.

### 7.4 Estimated Operating Cost
For a native Salesforce implementation, incremental license cost is $0 if existing Salesforce Flow, approvals, reports, and Slack/email entitlements are available. Expected implementation/operations overhead is 0.2 FTE platform administration monthly. If an Azure notification adapter is required, budget approximately $50-$150/month for Azure Functions Premium/consumption, Application Insights, queue, and Key Vault at the stated volume; Salesforce integration licensing remains a separate commercial dependency.

## 8. Security Architecture

### 8.1 Access Controls
| Control | Implementation |
|---|---|
| Strategic flag governance | Field-level security: Sales Operations write; representatives read-only |
| Approval authority | Salesforce approval assignment plus permission sets; no shared accounts |
| Rhea boundary | Server-side Apex guard requires TCV < $500,000 |
| Data access | Opportunity sharing rules and role hierarchy restrict records to authorized users |
| Integration identity | Named credential/managed identity with least-privilege scopes |
| Audit | Salesforce field history, approval history, integration logs, and security event logs |

### 8.2 Data Protection
TLS 1.2 or higher is required for external calls. Salesforce platform encryption is enabled for sensitive fields where licensed. Slack messages contain only business data necessary for action and link back to Salesforce. No approval decision is accepted from Slack; Slack is notification-only. Administrative actions require MFA and are logged.

### 8.3 Threat Controls
- Prevent representatives from clearing `Strategic_Account__c` to bypass CRO review.
- Re-evaluate snapshot consistency before each approval transition.
- Reject stale or duplicate submission versions.
- Prevent API users from invoking Rhea approval without the same server-side threshold check.
- Alert on repeated unauthorized-decision attempts and notification delivery failures.

## 9. Performance, Availability, and SLOs

| Metric | Target | Measurement |
|---|---:|---|
| Submission routing completion | p95 < 5 seconds | Salesforce Flow transaction logs |
| Approval request creation | p95 < 3 seconds after submission | Approval History timestamps |
| Notification acceptance | 99.9% within 5 minutes | Adapter/Application Insights and Salesforce integration log |
| Escalation detection | 100% of due requests within 15 minutes | Scheduled Flow run history |
| Unauthorized Rhea approval prevention | 100% blocked for TCV >= $500K | Security event and Apex test telemetry |
| Salesforce availability dependency | 99.9% monthly | Salesforce Trust and platform monitoring |
| Dashboard query time | p95 < 5 seconds for quarterly view | Salesforce report execution metrics |
| Data loss for approval actions | 0 accepted decisions lost | Salesforce transaction/audit validation |

## 10. Monitoring and Observability

### 10.1 Operational Metrics
Track submission volume, branch distribution, open requests by tier, age of pending requests, SLA breaches, escalation count, Rhea-boundary blocks, rejection/resubmission rate, Slack delivery success, email delivery success, duplicate submissions, and Flow/Apex failures.

### 10.2 Alerts and Runbooks
| Alert | Threshold | Severity | Response |
|---|---|---|---|
| CRO SLA breach | Any request overdue >15 minutes | High | Notify Sales Operations; inspect assignment and escalation run |
| Rhea boundary violation attempt | Any TCV >= $500K attempt | Critical | Block action; review actor and access logs |
| Notification failure | >1% failed delivery in 15 minutes | High | Retry queue; verify Slack/email credentials and provider status |
| Routing failure | 3 failed submissions in 10 minutes | High | Disable new Flow version if systemic; inspect field/lock errors |
| Escalation backlog | >10 due requests unprocessed after 15 minutes | High | Run scheduled job manually and inspect limits |
| Dashboard freshness | Data older than 30 minutes | Medium | Recalculate report and inspect approval history indexing |

## 11. Disaster Recovery and Business Continuity

Salesforce remains the authoritative platform and is protected by the organization's Salesforce backup and recovery service. Metadata is versioned in Git. Approval history is not recreated after a rollback; completed decisions remain immutable.

| Scenario | RTO | RPO | Recovery action |
|---|---:|---:|---|
| Flow failure | 30 minutes | 0 accepted decisions | Disable faulty version, activate prior version, replay only uncreated requests |
| Notification adapter outage | 60 minutes | 0 approval decisions | Queue/retry notifications; approvals continue in Salesforce |
| Salesforce service outage | 4 hours | 0 committed records | Follow Salesforce incident procedure; do not accept approvals out of band |
| Metadata deployment failure | 60 minutes | 0 production data | Roll back package and reactivate prior metadata |
| Corrupted configuration | 4 hours | 24 hours for configuration restore | Restore from version-controlled metadata and validate thresholds |

## 12. Implementation Roadmap

| Phase | Duration | Deliverables |
|---|---:|---|
| Design and field validation | 3 business days | Confirm TCV currency behavior, exact threshold semantics, delegation configuration, and Salesforce feature availability |
| Salesforce configuration | 5 business days | Fields, permissions, submission Flow, approval stages, snapshots, rejection/resubmission states |
| Apex and notification integration | 5 business days | Rhea authority guard, scheduled escalation, Slack/email adapter, retry and idempotency handling |
| Reporting and observability | 3 business days | Monthly leadership view, quarterly QBR dashboard, alerts, integration/error logs |
| Testing and UAT | 5 business days | Routing matrix, security tests, SLA simulation, notification tests, resubmission tests |
| Production rollout | 2 business days | Metadata promotion, activation checklist, pilot monitoring, rollback readiness |

### 12.1 Acceptance Test Matrix
| Scenario | Expected result |
|---|---|
| Strategic, TCV $300K, discount 8% | Direct CRO approval; no Deal Desk step |
| Strategic, TCV $300K, discount 12% | Deal Desk first, then CRO after Deal Desk approval |
| Strategic, TCV $300K, discount 12%, Deal Desk rejects | Rejected to representative; no CRO request |
| Strategic, TCV $100K exactly | No CRO tier under strict threshold interpretation |
| Non-Strategic, TCV $300K, discount 8% | No Strategic CRO tier |
| CRO request reaches 72 hours, TCV $400K | Escalate to Rhea; Rhea may decide |
| CRO request reaches 72 hours, TCV $500K | Rhea blocked; CRO remains authority and receives overdue notification |
| Account flagged Strategic after submission | Existing request unchanged; next submission uses new flag |
| Rejected Opportunity resubmitted | New version restarts all applicable approval tiers |
| Representative edits Strategic flag | Update denied by field-level security |

## 13. Risks and Trade-offs

| Risk | Impact | Mitigation |
|---|---|---|
| Conditional approval configuration bypasses direct CRO path | High | Automated tests for both discount branches and production monitoring of branch counts |
| Approval volume exceeds CRO capacity | Medium | Launch delegation, track p95 cycle time, and review monthly volume |
| TCV currency normalization differs across regions | High | Normalize to corporate currency in immutable snapshot and test multi-currency records |
| Slack/email integration outage | Medium | Parallel channels, retries, delivery alerts; Salesforce remains usable |
| Rhea boundary misconfiguration | Critical | Apex guard, negative tests at $499,999/$500,000, and security alerting |
| Salesforce limits during batch escalation | Medium | Indexed query, 15-minute batches, idempotent cursor processing, limit telemetry |

## 14. Appendix

### 14.1 Traceability
This design is grounded in `Meeting_Note_03_Strategic_Account.md` and the upstream `BA-dynamic-agent/BRD.md`. It implements the stated 38 Strategic Accounts context, $100,000 CRO threshold, >10% Deal Desk condition, sequential dependency, 72-hour SLA, Rhea's under-$500,000 authority, Slack/email notification requirement, full-chain resubmission, submission-time routing, and monthly/quarterly reporting.

### 14.2 Glossary
- **TCV:** Total Contract Value.
- **CRO:** Chief Revenue Officer.
- **Deal Desk:** Existing commercial approval tier for discount greater than 10%.
- **Routing snapshot:** Immutable copy of submission-time values and branch decisions.
- **SLA:** Service-level agreement for a decision deadline.
- **QBR:** Quarterly Business Review.
- **Flow:** Salesforce declarative automation/orchestration capability.

### 14.3 References
- Meeting note: `Meeting_Note_03_Strategic_Account.md`, 24 June 2026.
- Upstream requirements: `BA-dynamic-agent/BRD.md`.
- Salesforce platform metadata, Approval Processes, Flow, Field-Level Security, and Approval History capabilities.
