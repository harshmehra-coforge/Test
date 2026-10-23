"""
Business logic for feedback aggregation and delivery.
"""
from src.repository.feedback_repository import FeedbackRepository
from src.models.feedback import Feedback
from typing import List

class FeedbackService:
    def __init__(self, feedback_repo: FeedbackRepository):
        self.feedback_repo = feedback_repo

    async def aggregate_feedback(self, opportunity_id: str) -> Feedback:
        """Aggregate feedback for a given opportunity/story."""
        # TODO: Implement aggregation logic
        raise NotImplementedError

    async def deliver_feedback(self, feedback: Feedback) -> None:
        """Deliver feedback to user (via notification service)."""
        # TODO: Implement delivery logic
        raise NotImplementedError
