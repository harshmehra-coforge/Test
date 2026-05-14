package com.fnol.policy.domain;

/**
 * Policy status enumeration
 */
public enum PolicyStatus {
    ACTIVE("A", "Active"),
    INACTIVE("I", "Inactive"),
    CANCELLED("C", "Cancelled"),
    EXPIRED("E", "Expired"),
    SUSPENDED("S", "Suspended"),
    PENDING("P", "Pending");

    private final String code;
    private final String description;

    PolicyStatus(String code, String description) {
        this.code = code;
        this.description = description;
    }

    public String getCode() {
        return code;
    }

    public String getDescription() {
        return description;
    }

    public static PolicyStatus fromCode(String code) {
        for (PolicyStatus status : values()) {
            if (status.code.equals(code)) {
                return status;
            }
        }
        throw new IllegalArgumentException("Unknown policy status code: " + code);
    }
}