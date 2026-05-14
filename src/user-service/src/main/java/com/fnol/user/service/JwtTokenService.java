package com.fnol.user.service;

import com.auth0.jwt.JWT;
import com.auth0.jwt.algorithms.Algorithm;
import com.auth0.jwt.exceptions.JWTVerificationException;
import com.auth0.jwt.interfaces.DecodedJWT;
import com.fnol.user.domain.User;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.Date;
import java.util.stream.Collectors;

/**
 * JWT Token Service for token generation and validation
 */
@Service
public class JwtTokenService {

    @Value("${jwt.secret}")
    private String jwtSecret;

    @Value("${jwt.expiration:86400}") // 24 hours in seconds
    private Long jwtExpiration;

    @Value("${jwt.refresh-expiration:604800}") // 7 days in seconds
    private Long refreshTokenExpiration;

    /**
     * Generate JWT access token for user
     */
    public String generateToken(User user) {
        Date expirationDate = Date.from(
            LocalDateTime.now().plusSeconds(jwtExpiration)
                .atZone(ZoneId.systemDefault()).toInstant()
        );

        String roles = user.getRoles().stream()
                .map(role -> role.getName())
                .collect(Collectors.joining(","));

        return JWT.create()
                .withSubject(user.getId().toString())
                .withClaim("username", user.getUsername())
                .withClaim("email", user.getEmail())
                .withClaim("roles", roles)
                .withClaim("fullName", user.getFullName())
                .withIssuedAt(new Date())
                .withExpiresAt(expirationDate)
                .withIssuer("fnol-user-service")
                .sign(Algorithm.HMAC256(jwtSecret));
    }

    /**
     * Generate refresh token for user
     */
    public String generateRefreshToken(User user) {
        Date expirationDate = Date.from(
            LocalDateTime.now().plusSeconds(refreshTokenExpiration)
                .atZone(ZoneId.systemDefault()).toInstant()
        );

        return JWT.create()
                .withSubject(user.getId().toString())
                .withClaim("type", "refresh")
                .withIssuedAt(new Date())
                .withExpiresAt(expirationDate)
                .withIssuer("fnol-user-service")
                .sign(Algorithm.HMAC256(jwtSecret));
    }

    /**
     * Validate JWT token
     */
    public boolean validateToken(String token) {
        try {
            Algorithm algorithm = Algorithm.HMAC256(jwtSecret);
            JWT.require(algorithm)
                .withIssuer("fnol-user-service")
                .build()
                .verify(token);
            return true;
        } catch (JWTVerificationException e) {
            return false;
        }
    }

    /**
     * Extract user ID from token
     */
    public Long getUserIdFromToken(String token) {
        try {
            DecodedJWT decodedJWT = JWT.decode(token);
            return Long.parseLong(decodedJWT.getSubject());
        } catch (Exception e) {
            return null;
        }
    }

    /**
     * Extract username from token
     */
    public String getUsernameFromToken(String token) {
        try {
            DecodedJWT decodedJWT = JWT.decode(token);
            return decodedJWT.getClaim("username").asString();
        } catch (Exception e) {
            return null;
        }
    }

    /**
     * Check if token is expired
     */
    public boolean isTokenExpired(String token) {
        try {
            DecodedJWT decodedJWT = JWT.decode(token);
            return decodedJWT.getExpiresAt().before(new Date());
        } catch (Exception e) {
            return true;
        }
    }

    /**
     * Get token expiration time
     */
    public Date getExpirationDateFromToken(String token) {
        try {
            DecodedJWT decodedJWT = JWT.decode(token);
            return decodedJWT.getExpiresAt();
        } catch (Exception e) {
            return null;
        }
    }
}