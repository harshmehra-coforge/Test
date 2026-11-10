"""
Event Hubs publisher/subscriber stubs for async domain events.
"""
# TODO: Implement Azure Event Hubs integration for pass creation/update events

def publish_event(event_type: str, payload: dict):
    """Publish an event to Event Hubs."""
    raise NotImplementedError

def subscribe_events(event_type: str):
    """Subscribe to domain events of a given type."""
    raise NotImplementedError
