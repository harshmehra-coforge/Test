"""
RetryManager handles automatic retry orchestration for transient payment failures.
"""
from src.models.retry import RetryRequest

class RetryManager:
    def __init__(self):
        # TODO: Inject repositories, config, messaging
        pass

    async def schedule_retry(self, retry_request: RetryRequest) -> None:
        """Schedule a retry for a failed payment via Azure Service Bus."""
        # TODO: Implement retry scheduling
        raise NotImplementedError
