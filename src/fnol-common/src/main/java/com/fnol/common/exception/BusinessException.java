package com.fnol.common.exception;

/**
 * Base exception class for all business exceptions
 */
public class BusinessException extends RuntimeException {

    private final String errorCode;
    private final Object details;

    public BusinessException(String message) {
        super(message);
        this.errorCode = "BUSINESS_ERROR";
        this.details = null;
    }

    public BusinessException(String errorCode, String message) {
        super(message);
        this.errorCode = errorCode;
        this.details = null;
    }

    public BusinessException(String errorCode, String message, Object details) {
        super(message);
        this.errorCode = errorCode;
        this.details = details;
    }

    public BusinessException(String errorCode, String message, Throwable cause) {
        super(message, cause);
        this.errorCode = errorCode;
        this.details = null;
    }

    public String getErrorCode() {
        return errorCode;
    }

    public Object getDetails() {
        return details;
    }
}