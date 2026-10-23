"""Settings/configuration for approval workflow."""
import os

class Settings:
    """Holds configuration for SLA durations and notification endpoints."""
    DEAL_DESK_SLA_HOURS = int(os.getenv("DEAL_DESK_SLA_HOURS", 24))
    VP_SALES_SLA_HOURS = int(os.getenv("VP_SALES_SLA_HOURS", 48))
    SLACK_WEBHOOK_URL = os.getenv("SLACK_WEBHOOK_URL", "")
    FINANCE_EMAIL = os.getenv("FINANCE_EMAIL", "finance@example.com")
    # TODO: Add more config as needed
