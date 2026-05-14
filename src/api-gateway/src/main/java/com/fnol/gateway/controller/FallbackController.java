package com.fnol.gateway.controller;

import com.fnol.common.dto.ApiResponse;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

/**
 * Fallback controller for circuit breaker
 */
@RestController
@RequestMapping("/fallback")
public class FallbackController {

    @GetMapping("/policy")
    public ResponseEntity<ApiResponse<String>> policyFallback() {
        ApiResponse<String> response = ApiResponse.error("Policy service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }

    @GetMapping("/claim")
    public ResponseEntity<ApiResponse<String>> claimFallback() {
        ApiResponse<String> response = ApiResponse.error("Claim service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }

    @GetMapping("/adjuster")
    public ResponseEntity<ApiResponse<String>> adjusterFallback() {
        ApiResponse<String> response = ApiResponse.error("Adjuster service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }

    @GetMapping("/coverage")
    public ResponseEntity<ApiResponse<String>> coverageFallback() {
        ApiResponse<String> response = ApiResponse.error("Coverage service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }

    @GetMapping("/report")
    public ResponseEntity<ApiResponse<String>> reportFallback() {
        ApiResponse<String> response = ApiResponse.error("Report service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }

    @GetMapping("/notification")
    public ResponseEntity<ApiResponse<String>> notificationFallback() {
        ApiResponse<String> response = ApiResponse.error("Notification service is currently unavailable. Please try again later.");
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(response);
    }
}