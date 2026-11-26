# Technical System Design: Auto Insurance Quote & FNOL App

## 1. Introduction
This Technical System Design (TSD) specifies the architecture, data models, APIs, validation rules, operational constraints, and extensibility points for a self-contained Auto Insurance Quote & FNOL (First Notice of Loss) web application. The solution is designed for rapid deployment, minimal dependencies, and strict adherence to business rules outlined in the BRD. Implementation leverages Python 3.11+, FastAPI, SQLite, and a pure JS/HTML SPA with no external libraries or authentication features.

## 2. System Overview
The system provides instant insurance quote calculation, policy binding with unique IDs and status management, and FNOL claim intake restricted to valid, active policies. Business rules and validation are strictly enforced across all boundaries. All data is stored locally in SQLite, and the UI is rendered client-side as a single static HTML/JS file.

**Tech Stack:**
- Backend: Python 3.11+, FastAPI
- Database: SQLite (local file DB)
- Frontend: Single-page HTML & vanilla JavaScript (no external dependencies)
- Hosting: Any platform that supports Python 3.11+ and allows HTTP port binding
- No authentication or external API integrations

## 3. Architecture Diagram

```mermaid
architecture-beta
    title "Auto Insurance Quote & FNOL Claim App Architecture"

    Boundary(Browser, "Browser + SPA") {
        ComponentA(UI, "HTML/JS SPA", "User Interface")
    }
    Boundary(Server, "FastAPI Application") {
        ComponentB(API, "FastAPI App", "REST API & Business Logic")
        ComponentC(DB, "SQLite DB", "Local Data Store")
    }
    UI -rest-> API : HTTP (JSON)
    API -db-> DB : SQL
```

- **Client:** SPA renders forms for quotes, binds, and FNOL claims, communicates with backend via JSON over HTTP
- **API Server:** FastAPI serves endpoints, enforces all business/validation rules, and persists to SQLite
- **Data Store:** SQLite is the system of record for all policies, claims, and audit logs

## 4. Data Model & Schema

### SQLite Schema

```sql
-- Policy Table
CREATE TABLE policy (
    id TEXT PRIMARY KEY,            -- Format: POL-XXXX
    status TEXT NOT NULL,           -- "ACTIVE" only
    quote_amount REAL NOT NULL,     -- Calculated premium
    driver_age INTEGER NOT NULL,    -- Age at binding
    vehicle_type TEXT NOT NULL,     -- Sedan, SUV, Sports
    start_date TEXT NOT NULL,       -- UTC ISO8601 start
    end_date TEXT NOT NULL          -- UTC ISO8601: start + 1yr
);

-- Claim Table
CREATE TABLE claim (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    policy_id TEXT NOT NULL,        -- FK to policy.id
    incident_date TEXT NOT NULL,    -- UTC ISO8601
    status TEXT NOT NULL,           -- "SUBMITTED" only
    created_at TEXT NOT NULL,       -- UTC ISO8601
    FOREIGN KEY(policy_id) REFERENCES policy(id)
);

-- Audit Log Table
CREATE TABLE audit_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    action TEXT NOT NULL,           -- 'QUOTE', 'BIND', 'CLAIM'
    detail TEXT NOT NULL,           -- JSON detail
    created_at TEXT NOT NULL        -- UTC timestamp
);
```

**Rationale:**
- Policy IDs are unique and non-repeating (`POL-XXXX`), ensuring compliance with traceability.
- Policy and claim lifecycles are modeled explicitly; only permissible status states are stored.
- Audit log provides immutable trace of user/system actions for compliance and testability.
- UTC and ISO8601 formatting ensure unambiguous date handling without time zone issues.
- Minimal columns ensure no PII beyond what’s required for insurance rating and claims.

## 5. API Design

The system exposes four public JSON POST/GET endpoints:

### POST /quote
Calculate and return an auto insurance quote or signal rejection by rule.

**Request:**
```json
{
  "driver_age": 25,
  "vehicle_type": "Sedan"
}
```
**Response (success):**
```json
{
  "quote_amount": 500.0
}
```
**Response (fail - age):**
```json
{
  "error": "Driver under 18 is not eligible for coverage."
}
```

### POST /policies
Bind a quoted policy for eligible drivers/vehicles, assign unique ID, and set coverage window.

**Request:**
```json
{
  "driver_age": 25,
  "vehicle_type": "Sedan",
  "quote_amount": 500.0
}
```
**Response:**
```json
{
  "policy_id": "POL-1001",
  "status": "ACTIVE",
  "start_date": "2026-11-26T00:00:00Z",
  "end_date": "2027-11-25T23:59:59Z"
}
```
**Failure:**
```json
{
  "error": "Invalid or mismatched quote amount for provided age/vehicle."
}
```

### POST /claims
Accept FNOL only for active policies and valid coverage window.

**Request:**
```json
{
  "policy_id": "POL-1001",
  "incident_date": "2027-06-01"
}
```
**Response (success):**
```json
{
  "claim_id": 1,
  "status": "SUBMITTED"
}
```
**Response (fail - inactive/invalid):**
```json
{
  "error": "Policy is not active or incident is outside coverage period."
}
```

### GET /claims/{claim_id}
Return status and detail for a single submitted claim.

**Response:**
```json
{
  "claim_id": 1,
  "policy_id": "POL-1001",
  "status": "SUBMITTED",
  "incident_date": "2027-06-01",
  "created_at": "2027-06-01T14:28:00Z"
}
```

**Validation/Error Mapping:** All endpoint inputs are strictly validated per business rules (see section 6). HTTP 400 returned for failed eligibility, validation, or business constraints.

## 6. Business Logic & Validation Rules

### Eligibility and Rating
- **Driver Age**:
  - `<18`: reject with explicit error (no quote, no bind, no claim)
  - `18–24`: age multiplier 1.5
  - `25–64`: age multiplier 1.0
  - `65+`: age multiplier 1.2
- **Vehicle Type Multiplier:**
  - Sedan: 1.0
  - SUV: 1.2
  - Sports: 1.6
- **Quote Formula:**
  - Final quote = $500 x age_multiplier x vehicle_multiplier
- **Quote Failures:** Denied if driver_age < 18 or vehicle_type not recognized

### Policy Binding
- Policy only bound if the driver/vehicle/quote triad passes all eligibility and rating rules
- Policy ID format: `POL-XXXX` (incrementing 4-digit counter, zero-padded as needed)
- Coverage window: start = policy creation date (UTC), end = start + 1yr - 1s
- Status: "ACTIVE" on issue; policy cannot be re-bound or duplicated

### FNOL Claim Intake
- Only permitted for policies with ACTIVE status
- Incident date must be within (inclusive) policy start/end (
  - `start_date <= incident_date <= end_date` in UTC ISO8601)
- Claim assigned unique autoincrement ID; status "SUBMITTED"

### HTTP Validation Matrix
- All endpoints validate input types (age ≥ 18 integer, allowed values, field presence)
- Responses provide explicit error objects and HTTP 400 on business rule failure
- Policy and claims lookup by ID is enforced and returns 404 if not found

## 7. Non-Functional Requirements & Audit

- **Startup Latency**: <500ms on commodity laptop
- **Quote Throughput**: ≥30 quotes/sec (local, single process)
- **Data Consistency**: Strong; SQLite file lock and single-process operation
- **Audit Logging**: All quote, bind, and claim attempts recorded in `audit_log` (detailing action, request, outcome, and timestamp)
- **Traceability**: Record unique IDs for policies, claims, and all business events
- **Security**:
  - No authentication; threat model is prototype only
  - No sensitive/PII data origin or transmission
- **Resilience**: Strict type checks, single-tier, fail-fast error handling; restart safe

## 8. Design Decisions & Trade-offs

- **No External Dependencies:** Eliminates supply chain risk, maximizes portability, but precludes rich UI frameworks or third-party API integrations.
- **SQLite:** Portable, single-file RDBMS ideal for demo and light use; not suited for multi-user concurrency but sufficient for target requirements.
- **Policy/Claim ID Generation:** Incrementing for simplicity; avoids random/UUID to ensure comprehensible test/demo data.
- **Strict Validation:** All business rules enforced at API boundary to prevent leakage of invalid states into persistent store; clear, actionable user errors.
- **Separation of Concerns:** UI and backend communicate only via explicit JSON APIs; no direct DB/frontend coupling, aids testability and maintainability.
- **No Auth:** Reduces complexity but means all API requests are anonymous and not user-linked; limits real-world deployability.

## 9. Extensibility

- **Deductibles:** Policy schema and rating logic can be extended to accept and store deductibles per quote/policy; calculation and validation hooks pre-planned in API/DB structure.
- **Fraud Flagging:** Claim intake and processing can be enhanced with rule-based or heuristic fraud indicators; audit log supports tagging and future cross-linking of suspicious events.
- **Modular Python structure** (models, API, UI) ensures enhancements won’t break baseline flows.

## 10. Implementation Notes

- **SPA/BFF:** SPA loads static HTML/JS and interacts solely via JSON; all app state on server-side, minimizing UI-side logic.
- **Database File:** Stored alongside app, requiring writable disk; suitable for PoC/test/demo, not for production scaling.
- **Testability:** API contracts and SQLite schema make system directly testable via pytest, black-box and white-box.
- **Deployment:** Run with `uvicorn app:app --reload` (development) or via a minimal Dockerfile; all dependencies standard with Python 3.11+.

---

# Appendix A: Summary Table of Core Entities

| Entity    | Key Fields                                        | Notes                                 |
|-----------|---------------------------------------------------|---------------------------------------|
| Policy    | id, driver_age, vehicle_type, quote_amount, status, start_date, end_date | Binds after valid quote; 1-year coverage |
| Claim     | id, policy_id, incident_date, status, created_at  | Only on ACTIVE, in-window policies    |
| AuditLog  | id, action, detail, created_at                    | Action: QUOTE, BIND, CLAIM            |

# Appendix B: Sample Audit Log Entry

```json
{
  "id": 12,
  "action": "QUOTE",
  "detail": "{\"driver_age\":25,\"vehicle_type\":\"Sedan\",\"result\":500.0}",
  "created_at": "2026-11-26T12:00:00Z"
}
```

# Appendix C: Deployment Checklist
- Confirm Python 3.11+ and SQLite available
- All files in same directory: app.py, models.py, index.html (under templates/)
- DB schema auto-created/migrated at app start
- Run: `uvicorn app:app --reload`
- For Docker: Minimal Dockerfile, maps app port

---

# End of Technical System Design
