package com.fnol.common.dto;

import io.swagger.v3.oas.annotations.media.Schema;

/**
 * Standard API response wrapper
 */
@Schema(description = "Standard API response wrapper")
public class ApiResponse<T> {

    @Schema(description = "Response status", example = "SUCCESS")
    private String status;

    @Schema(description = "Response message", example = "Operation completed successfully")
    private String message;

    @Schema(description = "Response data")
    private T data;

    @Schema(description = "Error details (if any)")
    private Object errors;

    @Schema(description = "Response timestamp", example = "2024-01-15T10:30:00")
    private String timestamp;

    // Constructors
    public ApiResponse() {
        this.timestamp = java.time.LocalDateTime.now().toString();
    }

    public ApiResponse(String status, String message) {
        this();
        this.status = status;
        this.message = message;
    }

    public ApiResponse(String status, String message, T data) {
        this(status, message);
        this.data = data;
    }

    // Static factory methods
    public static <T> ApiResponse<T> success(T data) {
        return new ApiResponse<>("SUCCESS", "Operation completed successfully", data);
    }

    public static <T> ApiResponse<T> success(String message, T data) {
        return new ApiResponse<>("SUCCESS", message, data);
    }

    public static <T> ApiResponse<T> error(String message) {
        return new ApiResponse<>("ERROR", message);
    }

    public static <T> ApiResponse<T> error(String message, Object errors) {
        ApiResponse<T> response = new ApiResponse<>("ERROR", message);
        response.setErrors(errors);
        return response;
    }

    // Getters and Setters
    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public T getData() {
        return data;
    }

    public void setData(T data) {
        this.data = data;
    }

    public Object getErrors() {
        return errors;
    }

    public void setErrors(Object errors) {
        this.errors = errors;
    }

    public String getTimestamp() {
        return timestamp;
    }

    public void setTimestamp(String timestamp) {
        this.timestamp = timestamp;
    }
}