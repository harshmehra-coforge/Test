# Auto Insurance Quote & FNOL Claim App

A self-contained Flask/SQLite scaffold implementing the quote, policy-binding, FNOL, and claim-status boundaries from the approved TSD. It is a local demonstration and is not production-ready insurance software.

## Prerequisites

Python 3.12 and a virtual environment.

## Run locally

```bash
python3.12 -m venv .venv
source .venv/bin/activate  # Windows: .venv\\Scripts\\activate
pip install -r requirements.txt
cp .env.example .env  # optional; export variables in your shell
python run.py
```

Open `http://127.0.0.1:5000`. The database schema is initialized for the local demo. API endpoints are under `/api/v1`: `POST /quotes`, `POST /policies`, `GET /policies/{id}`, `POST /claims`, `GET /claims/{id}`, `/healthz`, and `/readyz`.

## Tests

```bash
pytest
```

The suite contains the three required rating cases plus coverage-before-start and expired-policy/no-write checks, and API smoke tests.

## Implementation notes

Business rules live in `app/services`, not route handlers. Rate values are demonstration assumptions: v1, USD, age 18 minimum, sedan/SUV/sports, and inclusive coverage dates. Replace `Base.metadata.create_all` with Alembic migrations and add authentication, authorization, TLS, backups, privacy controls, and PostgreSQL before any shared or public deployment.

Future P&C enhancements: deductible/coverage-limit calculation and explainable, versioned fraud/anomaly scoring that routes claims for review rather than silently denying them.
