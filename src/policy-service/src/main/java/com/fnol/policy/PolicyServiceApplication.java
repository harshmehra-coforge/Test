package com.fnol.policy;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.netflix.eureka.EnableEurekaClient;
import org.springframework.data.jpa.repository.config.EnableJpaAuditing;

/**
 * Policy Service Application
 * 
 * Manages insurance policies, coverage details, and policy lifecycle
 * including policy creation, updates, renewals, and cancellations.
 */
@SpringBootApplication
@EnableEurekaClient
@EnableJpaAuditing
public class PolicyServiceApplication {

    public static void main(String[] args) {
        SpringApplication.run(PolicyServiceApplication.class, args);
    }
}