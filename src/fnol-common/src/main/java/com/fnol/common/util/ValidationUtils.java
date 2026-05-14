package com.fnol.common.util;

import org.springframework.util.StringUtils;

import java.util.regex.Pattern;

/**
 * Utility class for validation operations
 */
public class ValidationUtils {

    private static final Pattern EMAIL_PATTERN = 
        Pattern.compile("^[A-Za-z0-9+_.-]+@([A-Za-z0-9.-]+\\.[A-Za-z]{2,})$");
    
    private static final Pattern PHONE_PATTERN = 
        Pattern.compile("^\\+?[1-9]\\d{1,14}$");
    
    private static final Pattern POLICY_NUMBER_PATTERN = 
        Pattern.compile("^[A-Z]{2}\\d{8}$");

    private ValidationUtils() {
        // Utility class
    }

    /**
     * Validates email format
     */
    public static boolean isValidEmail(String email) {
        return StringUtils.hasText(email) && EMAIL_PATTERN.matcher(email).matches();
    }

    /**
     * Validates phone number format
     */
    public static boolean isValidPhoneNumber(String phoneNumber) {
        return StringUtils.hasText(phoneNumber) && PHONE_PATTERN.matcher(phoneNumber).matches();
    }

    /**
     * Validates policy number format
     */
    public static boolean isValidPolicyNumber(String policyNumber) {
        return StringUtils.hasText(policyNumber) && POLICY_NUMBER_PATTERN.matcher(policyNumber).matches();
    }

    /**
     * Checks if string is not null and not empty
     */
    public static boolean isNotEmpty(String value) {
        return StringUtils.hasText(value);
    }

    /**
     * Validates that a value is within a specified range
     */
    public static boolean isInRange(Number value, Number min, Number max) {
        if (value == null || min == null || max == null) {
            return false;
        }
        double val = value.doubleValue();
        return val >= min.doubleValue() && val <= max.doubleValue();
    }
}