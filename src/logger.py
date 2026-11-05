"""Logging setup for the application."""
import logging

def get_logger(name: str) -> logging.Logger:
    """Get a configured logger."""
    # TODO: Configure logger as per app needs
    return logging.getLogger(name)
