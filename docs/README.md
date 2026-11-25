# Auto Insurance Quote & FNOL Claim App

Self-contained MVP scaffold for four journeys: calculate a deterministic auto premium, bind a policy, submit a basic First Notice of Loss (FNOL), and retrieve claim status. It follows the upstream TSD's Flask + SQLite modular-monolith design.

## Prerequisites

- Python 3.12+
- SQLite 3 (included with standard Python builds)

## Local setup

```bash
python -m venv .venv
source .venv/bin/activate       # Windows: .venv\\Scripts\\activate
pip install -r requirements.txt
cp .env.example .env            # Windows: copy .env.example .env
python run.py
```

Open `http://127.0.0.1:5000`. The schema is created on first startup and records are stored in `insurance.db` by default. The API is under `/api/v1`; `GET /healthz` is the health check.

## API journeys

- `POST /api/v1/quotes` with `{"driver_age":40,"vehicle_type":"suv"}`
- `POST /api/v1/policies` with age, vehicle, `effective_date`, and `expiration_date`
- `POST /api/v1/claims` with `policy_id`, `loss_date`, `loss_type`, and `description`
- `GET /api/v1/claims/<claim_id>` for read-only status
- `GET /api/v1/policies/<policy_id>` for policy details

The server recalculates premiums during bind. Coverage dates are inclusive. Underage drivers (<18), unsupported vehicles, invalid date ranges, unknown/inactive policies, future losses, and out-of-coverage losses are rejected with stable error codes.

## Tests

```bash
pytest -q
```

The skeleton includes the three required rating examples and the two required claim validation checks, plus endpoint smoke tests. Extend fixtures with a controlled clock and explicit database row-count assertions as implementation proceeds.

## Container

```bash
docker build -t auto-insurance-mvp .
docker run --rm -p 5000:5000 -v "$PWD/data:/app/data" auto-insurance-mvp
```

## Production boundary and next P&C enhancements

This is a local MVP: Flask's development server and SQLite are not production controls, and authentication/authorization is intentionally absent. Before shared deployment, use Gunicorn behind TLS, add authentication and policyholder/operator authorization, migrate to PostgreSQL, define retention and backups, and add structured logging/security headers.

Two future P&C enhancements are **deductible calculation** (coverage/deductible configuration and estimated net loss) and **explainable fraud scoring** (risk signals with human review, model governance, and auditability). Neither should automatically deny a claim without approved governance.
