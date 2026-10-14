# Technical Specification Document (TSD)
## Medication Refill Request Tracking — CarePath Plus
**Date:** 2026-10-14  
**Version:** 1.0  
**Status:** Draft

## 1. Executive Summary

### 1.1 Purpose
This system provides a structured, easily accessible record of medication refill requests for patients enrolled in CarePath Plus. It enables care coordinators to capture new requests, view previous requests and statuses in seconds, and report operational metrics such as fulfillment time and pharmacy performance.

### 1.2 Scope
**In Scope:**
- Custom Lightning Web Component (LWC) on CareProgramEnrollee record page, "Refill Requests" section
- Patient-centric view of all refill requests (medication, date, quantity, status, last update)
- Modal interface for new request and status capture
- Role-based access control (Salesforce sharing model)

**Out of Scope:**
- Direct integration with pharmacy systems (Phase 2)
- Patient self-service refill requests (Phase 2)
- Prescription generation, clinical authoring
- Medication adherence calculation, refill reminders

### 1.3 Key Decisions
1. **Native Salesforce LWC UI:** Enables rapid deployment, seamless access control, and responsive rendering for both desktop/web and Salesforce mobile app.
   - Trade-off: Less UI flexibility than custom web frontend, but high maintainability and compliance.
2. **Refill Request custom object:** Provides structured schema for refill lifecycle; avoids cluttering case notes and enables reporting.
   - Trade-off: Slight increase in schema complexity vs. reusing generic objects.
3. **Status lifecycle tracking:** Chose a 5-status model (Requested, Sent to Pharmacy, Shipped, Delivered, Issue) for clarity and operational reporting.
   - Trade-off: Simple state transitions but does not support re-opening closed requests.

## 2. System Architecture

### 2.1 High-Level Architecture
![Refer to architecture diagram: LWC interfaces with RefillRequestService and MedicationService, both backed by Salesforce custom object storage. Mobile app connects via native Salesforce APIs.]

#### Component Breakdown
- **Presentation Layer:**
  - Custom LWC — renders refill request grid, modal for request capture, status update interface
  - Salesforce Mobile App — ensures responsive rendering, uses same LWC component
- **Application Services:**
  - Refill Request Service (Apex logic, triggers, validations)
  - Medication Service (Lists available meds, enforces picklist sourcing)
  - Enrollee Service (Permission checks, enrollee context)
- **Data Layer:**
  - Salesforce Custom Object(s): Refill Request, Medication, Enrollee
  - No external database; all data on Salesforce platform

## 3. System Flows

### 3.1 Primary Business Flow: New Refill Request
1. Coordinator clicks "New Refill Request".
2. Modal opens, showing medication picklist, quantity, urgency, notes.
3. User enters fields and submits; validation enforces positive integer quantity, medication is prefiltered by patient's active meds.
4. System creates Refill Request record (status: Requested), linked to enrollee and medication.
5. Request list refreshes; new request appears at top.

### 3.2 Status Update Flow
1. Coordinator clicks "Update Status" on a request row.
2. Status dropdown advances current state (Requested → Sent to Pharmacy → Shipped → Delivered/Issue).
3. On save, system records status, change date, user id, optional status note; if status is Delivered or Issue, request is closed.

### 3.3 Supporting/Edge Flows
- If no meds exist, "New Refill Request" is disabled and tooltip explains why.
- If no refill requests on file, grid displays "No refill requests on file" message, but capture action is still available if patient has meds.

### 3.4 Error and Retry Handling
- Save failures (network, permission, validation) display actionable error messages to coordinators; no silent data loss.
- Retry logic is not required (single-platform in-app workflow).

## 4. Data Architecture

### 4.1 Entity & Data Model
| Entity | Key Fields | Relationships |
|--------|------------|---------------|
| Enrollee | enrolleeId (PK), demographics, medications | 1:N with RefillRequest, N:1 with Medication |
| Medication | medicationId (PK), name | 1:N with RefillRequest, N:1 with Enrollee |
| RefillRequest | refillRequestId (PK), enrolleeId (FK), medicationId (FK), requestedQty, status, urgency, notes, createdOn, statusUpdateOn, statusHistory [] | N:1 with Enrollee, N:1 with Medication |

#### Data Storage Strategy
- All objects reside on Salesforce (custom and standard objects). No Cosmos DB/Postgres required.
- Data retention: Permanent unless governed by org retention policies.
- Backups: Handled through Salesforce platform backup and export routines.

## 5. Technology Stack

| Component | Technology | Purpose | Rationale & Alternatives |
|-----------|------------|---------|--------------------------|
| UI | Salesforce LWC | User interface within CareProgramEnrollee | **Chosen:** Rapid build, native mobile/desktop support. *Alt*: Custom React UI (not feasible for Salesforce-only surface). |
| Business Logic | Apex classes, triggers | Request and status orchestration | **Chosen:** Native, highly maintainable. *Alt*: Flow Builder (less flexible, harder to enforce multi-entity logic). |
| Data Store | Salesforce custom objects | Persistence | **Chosen:** No extra infra or cost. *Alt*: External DB (overkill, out of scope). |
| Deployment | Salesforce Change Sets | Move components from sandbox to prod | **Chosen:** Built-in releases, governance. *Alt*: SFDX/APEX-only pipelines (more complex, not justified here). |

## 6. Integration Architecture

| Integration Point | Method | Description | Auth | Rate Limit |
|-------------------|--------|-------------|------|------------|
| LWC <-> Apex | Platform Events, Aura/LWC wire API | UI invokes Apex for request and status updates | Native | Salesforce limits |
| Medication Picklist | SOQL Query | Populates new request modal picklist | Native | N/A |

No external integrations, batch jobs, or middleware in scope.

## 7. Deployment Architecture
- All components reside natively in Salesforce org (no Azure/AWS infra required)
- Change Sets or SFDX pipelines for source control and CI/CD
- Sandbox → UAT → Production deployment pipeline
- Scaling is handled by Salesforce multi-tenant platform
- No direct infrastructure/VM scaling controls required

## 8. Security Architecture

| Layer       | Control          | Implementation                                      |
|-------------|------------------|----------------------------------------------------|
| Network     | Org-level Isolation | Salesforce tenant separation, HTTPS enforced      |
| Application | RBAC/Sharing      | Salesforce object and record-level sharing        |
| Application | Input Validation  | All fields validated in LWC and Apex              |
| Data        | Audit Trail       | All request/status actions auditable by default   |
| Data        | Encryption        | Platform encryption at rest & in transit          |

All user actions and data accesses subject to standard Salesforce audit and sharing controls.

## 9. Performance & Capacity

| Metric                    | Target              | Measurement                  |
|---------------------------|---------------------|------------------------------|
| UI load time (desktop)    | < 1.5 sec (95%ile)  | Salesforce page analytics    |
| UI load time (mobile)     | < 2.5 sec (95%ile)  | Salesforce mobile analytics  |
| Action latency (refill ops)| < 500ms (95%ile)   | Platform transaction logs    |
| Monthly request volume    | 10,000+ requests    | Audit logs, usage dashboards |
| Availability              | 99.9%+              | Salesforce SLA               |
| Data errors/losses        | 0                   | Audits, exception logging    |

## 10. Monitoring & Observability

- Audit fields auto-captured for all request/status actions (createdBy, createdOn, lastModifiedBy, lastModifiedOn)
- Structured logs in Salesforce org; monitored via standard org dashboards
- Exception logs: Apex errors, validation failures, user error messages
- Metrics: Number of refill requests per coordinator, avg. fulfillment time, % issues (reportable via custom dashboards)
- Alerting: Admins can configure dashboard alerts for high issue rates or delayed fulfillment
- Distributed trace: Not applicable (single-platform)
- SLOs: Availability 99.9%, UI p95 latency <1.5s desktop, <2.5s mobile

## 11. Disaster Recovery

| Scenario                    | RTO   | RPO   | Recovery Procedure                              |
|-----------------------------|-------|-------|------------------------------------------------|
| User error/deletion         | <5min | 0     | Salesforce undelete, recycle bin                |
| Data/service outage         | <1hr  | <1hr  | Salesforce platform restores                    |
| Complete org issue          | <4hr  | <24hr | Salesforce support restores from backup         |

Backup and recovery managed via Salesforce platform capabilities; no custom logic required.

## 12. Implementation Roadmap

| Phase                | Duration | Deliverables                          | Dependencies         |
|----------------------|----------|---------------------------------------|----------------------|
| Phase 1: Object Model| 1 week   | Custom Refill Request object & fields | None                 |
| Phase 2: LWC         | 2 weeks  | Refill Requests section, modal, grid  | Object model ready   |
| Phase 3: Apex Logic  | 1 week   | Create/update workflows, validations  | LWC, object complete |
| Phase 4: Testing     | 1 week   | UAT, mobile verification, bug fixes   | Phase 1-3 complete   |
| Phase 5: Deployment  | 3 days   | Change set promotion to production    | Phase 4 complete     |

## 13. Appendices

### 13.1 Glossary
- **LWC**: Lightning Web Component (Salesforce web UI framework)
- **Apex**: Salesforce object-oriented programming language
- **SOQL**: Salesforce Object Query Language
- **RBAC**: Role-Based Access Control
- **RTO**: Recovery Time Objective
- **RPO**: Recovery Point Objective

### 13.2 References
- Salesforce Life Sciences Cloud documentation
- Salesforce LWC/Apex developer guides
- Internal org governance standards

### 13.3 Risks
| Risk                                  | Impact | Probability | Mitigation                                         |
|----------------------------------------|--------|-------------|----------------------------------------------------|
| Med list not captured up front         | High   | Medium      | Onboarding reminders, admins monitor null meds      |
| Coordinator error during status update | Medium | Low         | Audit trail, user feedback loop                    |
| Data loss via deletion                 | Medium | Low         | Leverage Salesforce recycle bin, platform backup   |
| Org-level outage                      | High   | Rare        | Salesforce support, platform hotfail, routine backup|

### 13.4 Testing Strategy
- Unit tests: Apex logic, UI functionality (80%+ coverage)
- Integration: LWC-Apex, object save, status update
- Manual UAT: Coordinator workflows, mobile validation
- Security: Field-level access, audit trails

---

**Summary:**
TSD generated successfully. Key highlights:
- Native Salesforce LWC for structured refill request tracking
- Custom refill request object, status lifecycle, auditable flows
- Fully compliant with Salesforce security/RBAC, mobile-ready
- Performance targets: UI load <1.5s desktop, <2.5s mobile, 99.9%+ availability
- Clear phased implementation roadmap for rapid delivery
