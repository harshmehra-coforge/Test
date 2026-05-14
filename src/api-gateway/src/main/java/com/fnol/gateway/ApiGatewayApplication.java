package com.fnol.gateway;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.netflix.eureka.EnableEurekaClient;

/**
 * API Gateway Application
 * 
 * Central entry point for all FNOL microservices providing:
 * - Request routing and load balancing
 * - Authentication and authorization
 * - Rate limiting and circuit breaker
 * - Request/response transformation
 */
@SpringBootApplication
@EnableEurekaClient
public class ApiGatewayApplication {

    public static void main(String[] args) {
        SpringApplication.run(ApiGatewayApplication.class, args);
    }
}