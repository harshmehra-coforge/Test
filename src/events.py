"""
Event bus abstraction for BizWeb Flight-Pass.
Defines interfaces for publishing and subscribing to events (EventHub/ServiceBus).
"""
from typing import Any

class EventBus:
    """Abstract event bus interface."""
    def publish(self, topic: str, event: Any):
        # TODO: Implement event publishing to Azure EventHub/ServiceBus
        raise NotImplementedError

    def subscribe(self, topic: str, handler):
        # TODO: Implement event subscription
        raise NotImplementedError
