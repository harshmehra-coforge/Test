package com.fnol.common.exception;

/**
 * Exception thrown when a requested resource is not found
 */
public class ResourceNotFoundException extends BusinessException {

    public ResourceNotFoundException(String message) {
        super("RESOURCE_NOT_FOUND", message);
    }

    public ResourceNotFoundException(String resourceType, String identifier) {
        super("RESOURCE_NOT_FOUND", 
              String.format("%s not found with identifier: %s", resourceType, identifier));
    }
}