"""
Repository for Feedback entity (Cosmos DB access).
"""
from typing import Optional
from src.models.feedback import Feedback

class FeedbackRepository:
    def __init__(self, db_client):
        self.db_client = db_client

    async def get_feedback(self, feedback_id: str) -> Optional[Feedback]:
        """Fetch feedback by id."""
        # TODO: Implement Cosmos DB fetch logic
        raise NotImplementedError

    async def save_feedback(self, feedback: Feedback) -> None:
        """Save or update feedback."""
        # TODO: Implement Cosmos DB upsert logic
        raise NotImplementedError
