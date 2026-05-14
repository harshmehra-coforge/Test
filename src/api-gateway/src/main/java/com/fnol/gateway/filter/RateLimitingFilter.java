package com.fnol.gateway.filter;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.cloud.gateway.filter.GatewayFilter;
import org.springframework.cloud.gateway.filter.factory.AbstractGatewayFilterFactory;
import org.springframework.data.redis.core.ReactiveRedisTemplate;
import org.springframework.http.HttpStatus;
import org.springframework.http.server.reactive.ServerHttpResponse;
import org.springframework.stereotype.Component;
import org.springframework.web.server.ServerWebExchange;
import reactor.core.publisher.Mono;

import java.time.Duration;

/**
 * Rate Limiting Filter using Redis
 */
@Component
public class RateLimitingFilter extends AbstractGatewayFilterFactory<RateLimitingFilter.Config> {

    private static final Logger logger = LoggerFactory.getLogger(RateLimitingFilter.class);
    
    private final ReactiveRedisTemplate<String, String> redisTemplate;
    
    @Value("${rate-limit.requests-per-minute:100}")
    private int requestsPerMinute;

    public RateLimitingFilter(ReactiveRedisTemplate<String, String> redisTemplate) {
        super(Config.class);
        this.redisTemplate = redisTemplate;
    }

    @Override
    public GatewayFilter apply(Config config) {
        return (exchange, chain) -> {
            String clientId = getClientId(exchange);
            String key = "rate_limit:" + clientId;
            
            return redisTemplate.opsForValue()
                    .increment(key)
                    .flatMap(count -> {
                        if (count == 1) {
                            // Set expiration for the first request
                            return redisTemplate.expire(key, Duration.ofMinutes(1))
                                    .then(Mono.just(count));
                        }
                        return Mono.just(count);
                    })
                    .flatMap(count -> {
                        if (count > requestsPerMinute) {
                            logger.warn("Rate limit exceeded for client: {}", clientId);
                            return onError(exchange, "Rate limit exceeded", HttpStatus.TOO_MANY_REQUESTS);
                        }
                        
                        // Add rate limit headers
                        exchange.getResponse().getHeaders().add("X-RateLimit-Limit", String.valueOf(requestsPerMinute));
                        exchange.getResponse().getHeaders().add("X-RateLimit-Remaining", String.valueOf(requestsPerMinute - count));
                        
                        return chain.filter(exchange);
                    })
                    .onErrorResume(throwable -> {
                        logger.error("Error in rate limiting", throwable);
                        return chain.filter(exchange);
                    });
        };
    }
    
    private String getClientId(ServerWebExchange exchange) {
        // Use IP address as client identifier
        String clientIp = exchange.getRequest().getRemoteAddress() != null 
                ? exchange.getRequest().getRemoteAddress().getAddress().getHostAddress()
                : "unknown";
        
        // You can also use user ID from JWT token if available
        String userId = exchange.getRequest().getHeaders().getFirst("X-User-Id");
        return userId != null ? userId : clientIp;
    }
    
    private Mono<Void> onError(ServerWebExchange exchange, String message, HttpStatus status) {
        ServerHttpResponse response = exchange.getResponse();
        response.setStatusCode(status);
        response.getHeaders().add("Content-Type", "application/json");
        
        String body = String.format("{\"error\":\"%s\",\"status\":%d}", message, status.value());
        return response.writeWith(Mono.just(response.bufferFactory().wrap(body.getBytes())));
    }

    public static class Config {
        private int requestsPerMinute = 100;
        
        public int getRequestsPerMinute() {
            return requestsPerMinute;
        }
        
        public void setRequestsPerMinute(int requestsPerMinute) {
            this.requestsPerMinute = requestsPerMinute;
        }
    }
}