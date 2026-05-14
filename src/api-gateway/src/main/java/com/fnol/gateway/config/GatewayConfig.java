package com.fnol.gateway.config;

import org.springframework.cloud.gateway.route.RouteLocator;
import org.springframework.cloud.gateway.route.builder.RouteLocatorBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Gateway routing configuration
 */
@Configuration
public class GatewayConfig {

    @Bean
    public RouteLocator customRouteLocator(RouteLocatorBuilder builder) {
        return builder.routes()
                // User Service Routes
                .route("user-service", r -> r
                        .path("/api/v1/auth/**", "/api/v1/users/**")
                        .uri("lb://user-service"))
                
                // Policy Service Routes
                .route("policy-service", r -> r
                        .path("/api/v1/policies/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("policy-service-cb")
                                        .setFallbackUri("forward:/fallback/policy"))
                                .retry(config -> config
                                        .setRetries(3)
                                        .setBackoff(java.time.Duration.ofMillis(100), 
                                                   java.time.Duration.ofMillis(1000), 2, false)))
                        .uri("lb://policy-service"))
                
                // Claim Service Routes
                .route("claim-service", r -> r
                        .path("/api/v1/claims/**", "/api/v1/fnol/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("claim-service-cb")
                                        .setFallbackUri("forward:/fallback/claim"))
                                .retry(config -> config
                                        .setRetries(3)
                                        .setBackoff(java.time.Duration.ofMillis(100), 
                                                   java.time.Duration.ofMillis(1000), 2, false)))
                        .uri("lb://claim-service"))
                
                // Adjuster Service Routes
                .route("adjuster-service", r -> r
                        .path("/api/v1/adjusters/**", "/api/v1/assignments/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("adjuster-service-cb")
                                        .setFallbackUri("forward:/fallback/adjuster")))
                        .uri("lb://adjuster-service"))
                
                // Coverage Service Routes
                .route("coverage-service", r -> r
                        .path("/api/v1/coverage/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("coverage-service-cb")
                                        .setFallbackUri("forward:/fallback/coverage")))
                        .uri("lb://coverage-service"))
                
                // Report Service Routes
                .route("report-service", r -> r
                        .path("/api/v1/reports/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("report-service-cb")
                                        .setFallbackUri("forward:/fallback/report")))
                        .uri("lb://report-service"))
                
                // Notification Service Routes
                .route("notification-service", r -> r
                        .path("/api/v1/notifications/**")
                        .filters(f -> f
                                .circuitBreaker(config -> config
                                        .setName("notification-service-cb")
                                        .setFallbackUri("forward:/fallback/notification")))
                        .uri("lb://notification-service"))
                
                .build();
    }
}