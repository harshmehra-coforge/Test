import jwt from 'jsonwebtoken';
import { JWTPayload, UserRole } from '@/types';
import { config } from '@/config';
import { logger } from './logger';

export class JWTService {
  private readonly secret = config.security.jwt.secret;
  private readonly refreshSecret = config.security.jwt.refreshSecret;
  private readonly expiresIn = config.security.jwt.expiresIn;
  private readonly refreshExpiresIn = config.security.jwt.refreshExpiresIn;

  /**
   * Generate access token
   */
  generateAccessToken(payload: Omit<JWTPayload, 'iat' | 'exp' | 'aud' | 'iss'>): string {
    const tokenPayload: JWTPayload = {
      ...payload,
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + this.parseExpirationTime(this.expiresIn),
      aud: 'wealth-management',
      iss: 'wealth-management-platform',
    };

    return jwt.sign(tokenPayload, this.secret, {
      algorithm: 'HS256',
    });
  }

  /**
   * Generate refresh token
   */
  generateRefreshToken(userId: string, sessionId: string): string {
    const payload = {
      sub: userId,
      sessionId,
      type: 'refresh',
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + this.parseExpirationTime(this.refreshExpiresIn),
    };

    return jwt.sign(payload, this.refreshSecret, {
      algorithm: 'HS256',
    });
  }

  /**
   * Verify access token
   */
  verifyAccessToken(token: string): JWTPayload {
    try {
      const decoded = jwt.verify(token, this.secret) as JWTPayload;
      return decoded;
    } catch (error) {
      logger.error('JWT verification failed:', error);
      throw new Error('Invalid or expired token');
    }
  }

  /**
   * Verify refresh token
   */
  verifyRefreshToken(token: string): { sub: string; sessionId: string; type: string } {
    try {
      const decoded = jwt.verify(token, this.refreshSecret) as any;
      return decoded;
    } catch (error) {
      logger.error('Refresh token verification failed:', error);
      throw new Error('Invalid or expired refresh token');
    }
  }

  /**
   * Decode token without verification (for debugging)
   */
  decodeToken(token: string): any {
    return jwt.decode(token);
  }

  /**
   * Check if token is expired
   */
  isTokenExpired(token: string): boolean {
    try {
      const decoded = jwt.decode(token) as any;
      if (!decoded || !decoded.exp) {
        return true;
      }
      
      return decoded.exp < Math.floor(Date.now() / 1000);
    } catch (error) {
      return true;
    }
  }

  /**
   * Get token expiration time
   */
  getTokenExpiration(token: string): Date | null {
    try {
      const decoded = jwt.decode(token) as any;
      if (!decoded || !decoded.exp) {
        return null;
      }
      
      return new Date(decoded.exp * 1000);
    } catch (error) {
      return null;
    }
  }

  /**
   * Parse expiration time string to seconds
   */
  private parseExpirationTime(expiresIn: string): number {
    const unit = expiresIn.slice(-1);
    const value = parseInt(expiresIn.slice(0, -1), 10);

    switch (unit) {
      case 's':
        return value;
      case 'm':
        return value * 60;
      case 'h':
        return value * 60 * 60;
      case 'd':
        return value * 24 * 60 * 60;
      default:
        throw new Error(`Invalid expiration time format: ${expiresIn}`);
    }
  }

  /**
   * Extract user ID from token
   */
  extractUserId(token: string): string | null {
    try {
      const decoded = this.decodeToken(token);
      return decoded?.sub || null;
    } catch (error) {
      return null;
    }
  }

  /**
   * Extract user roles from token
   */
  extractUserRoles(token: string): UserRole[] {
    try {
      const decoded = this.decodeToken(token);
      return decoded?.roles || [];
    } catch (error) {
      return [];
    }
  }

  /**
   * Check if user has required role
   */
  hasRole(token: string, requiredRole: UserRole): boolean {
    const roles = this.extractUserRoles(token);
    return roles.includes(requiredRole);
  }

  /**
   * Check if user has any of the required roles
   */
  hasAnyRole(token: string, requiredRoles: UserRole[]): boolean {
    const roles = this.extractUserRoles(token);
    return requiredRoles.some(role => roles.includes(role));
  }
}

export const jwtService = new JWTService();