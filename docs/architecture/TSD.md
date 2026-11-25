# Auto Insurance Quote & FNOL Claim App: Technical System Design (TSD)

## 1. Introduction & Project Scope

This document provides the technical system design for a lightweight Auto Insurance Quote & FNOL (First Notice of Loss) Claim Application. The system enables users to calculate an auto premium, bind a policy, submit a basic FNOL claim, and view claim status. The solution targets single-environment, developer-friendly deployment, minimizing infrastructure and operational overhead while ensuring clear data modeling and robust edge case handling.

### Project Objectives
- Deliver a simple, self-contained web application supporting four core auto insurance workflows:
    1. Calculate auto premium based on driver age and vehicle type
    2. Bind a policy
    3. Submit a basic FNOL claim
    4. View claim status
- Address critical domain edge cases such as: underage drivers, expired polices, and basic validation rules.
- Ensure foundations for future P&C features while maintaining lightweight maintainability.

### Target Users
- Consumers seeking insurance premium estimation and claims submission
- Insurance administrators (for possible future enhancements)

### In Scope
- Premium rating logic using basic business rules
- Policy CRUD and lifecycle
- FNOL claim capture and status tracking
- Simple UI and REST API endpoints

### Out of Scope
- Advanced rating or underwriting (e.g., third party integration)
- Payment processing
- Full policy administration
- Fraud scoring (proposed as enhancement)

## 2. User Stories (Functional Overview)

1. **As a driver, I want to enter my age and vehicle type to get an auto insurance quote, so I can know my estimated premium.**
2. **As a driver, I want to bind an insurance policy based on my quote details and personal information.**
3. **As a policyholder, I want to submit a simple FNOL (First Notice of Loss) claim against my policy by providing incident details.**
4. **As a policyholder, I want to view the status of my submitted claim, so I can know its progress.**

#### Domain Edge Cases
- Underage driver attempts (e.g., age < 18)
- Policy expiration (claims submission outside of coverage period)
- Duplicate policy creation for the same user/vehicle
- Attempt to submit claim for non-existent policy

## 3. Technology Stack & Rationale

| Layer              | Technology               | Rationale                                   |
|--------------------|--------------------------|----------------------------------------------|
| Web/App Framework  | Python Flask             | Minimal, rapid development, clear routing    |
| Database           | SQLite                   | Zero-setup, file-based, self-contained DB    |
| Frontend           | HTML + Vanilla JS        | Zero build tooling, easily deployable        |
| API Layer          | Flask RESTful endpoints  | Easy to implement, direct mapping to UX      |
| Packaging          | pip, virtualenv          | Standard Python packaging                    |

**Justification:** This stack supports one-file, in-memory, or basic file-based deployments suitable for hackathons, PoCs, lightweight insurtech demos, or developer onboarding. SQLite avoids needing separate DB provisioning; Flask is well-documented and expressive for rapid MVPs. Optional separation (e.g., JS fetch calls vs Flask server-rendered pages) supported if UI is to be extended later.

### Alternatives Considered
- **Django + PostgreSQL:** More overhead for a simple scenario; better for future scaling but not justified for current requirements.
- **Node.js + Express + MongoDB:** Popular for JS-heavy teams but less efficient for simple backend-first flows.

## 4. System Architecture Overview

The application follows a simple MVC-inspired separation. All flows are synchronous and self-contained. Minimalistic API endpoints are paired with HTML forms or fetch calls for interactivity. No external authentication or service dependencies. All data is stored locally via SQLite.

```mermaid
flowchart TD
    UI["HTML/JS Frontend"] -- REST or Form POST --> API["Flask REST Endpoints"]
    API -- ORM/SQL --> DB["SQLite Database (Policy, Claim)"]
    subgraph App
        API
        DB
    end
    UI <-- Response/Render -- API
```

### Main Components
- **Frontend:** Static HTML/JS for user inputs and status display
- **Backend:** Flask app exposing endpoints for rating, policy management, claim intake, and status retrieval
- **Database:** SQLite with two core tables (Policy, Claim)

### Service Decomposition
All flows reside in a monolith Flask app with clear modular separation:
- `/quote` (POST: premium calculation)
- `/policy` (POST: create/bind, GET: details)
- `/claim` (POST: submit FNOL)
- `/claim/<claim_id>/status` (GET: status lookup)

## 5. Core Data Models

### 5.1 Policy Model
| Field             | Type     | Description                                   |
|-------------------|----------|-----------------------------------------------|
| id                | Integer  | Unique primary key                            |
| policy_number     | String   | Policy reference (unique)                     |
| insured_name      | String   | Name of the insured driver                    |
| driver_age        | Integer  | Driver's age                                  |
| vehicle_type      | String   | Car/SUV/Truck/Other                           |
| premium           | Float    | Calculated premium                            |
| effective_date    | Date     | Policy start date                             |
| expiry_date       | Date     | Policy end date                               |
| status            | String   | 'Active', 'Expired', etc.                     |

### 5.2 Claim Model
| Field             | Type     | Description                                   |
|-------------------|----------|-----------------------------------------------|
| id                | Integer  | Unique primary key                            |
| claim_number      | String   | FNOL claim reference (unique)                 |
| policy_number     | String   | Associated policy                             |
| incident_date     | Date     | Date of accident/event                        |
| description       | String   | Incident narrative                            |
| claim_status      | String   | 'Submitted', 'Under Review', 'Closed', etc.   |
| created_at        | DateTime | FNOL submission timestamp                     |

### Data Model Constraints & Rules
- **Underage Restriction:** Policies must not be created for drivers under 18 (validate at submission/API level).
- **Duplicate Policy:** Disallow duplicate active policies for same driver & vehicle (enforce DB and app logic).
- **Claim Policy Check:** FNOL claims only accepted for in-force (active) policies; incident date must be within coverage dates.

## 6. API/UI Design and Flow Mapping

### 6.1 Calculate Premium (Quote)
- **Endpoint:** `POST /quote`
- **Input:** `{ "driver_age": int, "vehicle_type": str }`
- **Logic:** Apply rating rules: base premium modified by age and vehicle type. Reject underage entries.
- **Response:** `{ "premium": float, "eligible": bool, "message": str }`
- **UI:** Simple form (age, vehicle dropdown, show premium or error)

### 6.2 Bind Policy
- **Endpoint:** `POST /policy`
- **Input:** `{ "insured_name": str, "driver_age": int, "vehicle_type": str, "premium": float, "effective_date": date, "expiry_date": date }`
- **Logic:** Validate age, prevent duplicate policies, assign unique policy number.
- **Response:** `{ "policy_number": str, "status": "Active", ... }`
- **UI:** Policy summary page

### 6.3 Submit FNOL Claim
- **Endpoint:** `POST /claim`
- **Input:** `{ "policy_number": str, "incident_date": date, "description": str }`
- **Logic:** Confirm policy exists, is active, and incident date within effective/expiry window; assign claim number.
- **Response:** `{ "claim_number": str, "claim_status": "Submitted", ... }`
- **UI:** Confirmation screen

### 6.4 View Claim Status
- **Endpoint:** `GET /claim/<claim_id>/status`
- **Logic:** Fetch claim by ID and return status or error if not found.
- **Response:** `{ "claim_status": str, ... }`
- **UI:** Status displayed; error for unauthorized/invalid claim

#### General Considerations
- All endpoints return user-oriented error messages and validation feedback for edge cases.
- HTML/JS UI works directly with API or via Flask server-side rendering.

## 7. Non-Functional Requirements (NFRs) & Edge Case Handling

| NFR                             | Value / Approach                                      |
|----------------------------------|------------------------------------------------------|
| Availability                    | ≥99.5% (developer local, minimal downtime)            |
| Latency (UI ⇄ API)               | <200ms (local)                                       |
| Throughput                       | <5 concurrent users                                  |
| Data Consistency                 | Strong: SQLite transactions enforce ACID             |
| Cost                             | Zero (open source stack, no cloud usage)              |
| Security                         | Minimal: input validation, basic error handling       |
| Data Loss Tolerance              | Low (deployed locally, SQLite file backup recommended)|

### Edge Case Handling
- **Underage Drivers:** Reject and explain if age < 18
- **Expired Policies:** Block claim submission and inform user
- **Duplicate Policies:** Check for existing policies per driver/vehicle before create
- **Non-existent Policy Claims:** 404 error with message
- **Claim Date Outside Coverage:** Validate incident date within policy active range, return error with guidance
- **Input Validation:** All user fields validated with clear messages (empty, invalid formats, etc.)

## 8. Deployment & Maintenance Guidelines

- **Environment:** Python 3.8+, Flask, SQLite (cross-platform, no dependencies)
- **Run Local:**
    1. `python3 -m venv venv && source venv/bin/activate`
    2. `pip install flask sqlalchemy`
    3. `python app.py`
    4. Open browser to http://localhost:5000
- **Database:** SQLite DB file created automatically; run migrations via SQLAlchemy if data model changes
- **Backup:** Copy `app.db` to backup policy/claim data
- **Extensibility:** Modular design enables ease of future expansion

## 9. Future Enhancements

1. **Deductible Calculations:** Add support for adjustable deductibles affecting the premium and claim payouts
2. **Fraud Scoring:** Implement basic rules (e.g., frequency of FNOL submissions per policy, suspicious patterns)

---

# End of Technical System Design
