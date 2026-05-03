import { Request, Response, NextFunction } from 'express';
import { jwtService } from '@/utils/jwt';
import { UserRole } from '@/types';
import { logger, logSecurity } from '@/utils/logger';

// Extend Request interface to include user information
declare global {
  namespace Express {
    interface Request {
      user?: {
        id: string;
        email: string;
        roles: UserRole[];
        permissions: string[];
        clientId?: string;
        sessionId: string;
      };
      requestId?: string;
    }
  }
}

/**
 * Middleware to authenticate JWT tokens
 */
export const authenticateToken = (req: Request, res: Response, next: NextFunction): void => {
  try {
    const authHeader = req.headers.authorization;
    const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

    if (!token) {
      logSecurity('Missing authentication token', undefined, {
        ip: req.ip,
        userAgent: req.get('User-Agent'),
        path: req.path,
      });

      res.status(401).json({
        success: false,
        error: {
          code: 'MISSING_TOKEN',
          message: 'Access token is required',
        },
      });
      return;
    }

    // Verify the token
    const payload = jwtService.verifyAccessToken(token);

    // Add user information to request
    req.user = {
      id: payload.sub,
      email: payload.sub, // Assuming sub contains email or user ID
      roles: payload.roles,
      permissions: payload.permissions,
      clientId: payload.clientId,
      sessionId: payload.sessionId,
    };

    logger.info(`User authenticated: ${req.user.id}`, {
      userId: req.user.id,
      roles: req.user.roles,
      path: req.path,
      method: req.method,
    });

    next();
  } catch (error) {
    logSecurity('Invalid authentication token', undefined, {
      ip: req.ip,
      userAgent: req.get('User-Agent'),
      path: req.path,
      error: error instanceof Error ? error.message : 'Unknown error',
    });

    res.status(401).json({
      success: false,
      error: {
        code: 'INVALID_TOKEN',
        message: 'Invalid or expired access token',
      },
    });
  }
};

/**
 * Middleware to require specific roles
 */
export const requireRole = (requiredRoles: UserRole[]) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    if (!req.user) {
      res.status(401).json({
        success: false,
        error: {
          code: 'AUTHENTICATION_REQUIRED',
          message: 'Authentication is required',
        },
      });
      return;
    }

    const hasRequiredRole = requiredRoles.some(role => req.user!.roles.includes(role));

    if (!hasRequiredRole) {
      logSecurity('Insufficient permissions', req.user.id, {
        requiredRoles,
        userRoles: req.user.roles,
        path: req.path,
        method: req.method,
      });

      res.status(403).json({
        success: false,
        error: {
          code: 'INSUFFICIENT_PERMISSIONS',
          message: 'Insufficient permissions to access this resource',
          details: {
            required: requiredRoles,
            current: req.user.roles,
          },
        },
      });
      return;
    }

    next();
  };
};

/**
 * Middleware to require specific permissions
 */
export const requirePermission = (requiredPermissions: string[]) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    if (!req.user) {
      res.status(401).json({
        success: false,
        error: {
          code: 'AUTHENTICATION_REQUIRED',
          message: 'Authentication is required',
        },
      });
      return;
    }

    const hasRequiredPermission = requiredPermissions.some(permission => 
      req.user!.permissions.includes(permission)
    );

    if (!hasRequiredPermission) {
      logSecurity('Insufficient permissions', req.user.id, {
        requiredPermissions,
        userPermissions: req.user.permissions,
        path: req.path,
        method: req.method,
      });

      res.status(403).json({
        success: false,
        error: {
          code: 'INSUFFICIENT_PERMISSIONS',
          message: 'Insufficient permissions to access this resource',
          details: {
            required: requiredPermissions,
            current: req.user.permissions,
          },
        },
      });
      return;
    }

    next();
  };
};

/**
 * Middleware to check if user can access specific client data
 */
export const requireClientAccess = (req: Request, res: Response, next: NextFunction): void => {
  if (!req.user) {
    res.status(401).json({
      success: false,
      error: {
        code: 'AUTHENTICATION_REQUIRED',
        message: 'Authentication is required',
      },
    });
    return;
  }

  const clientId = req.params.clientId || req.body.clientId || req.query.clientId;

  // Admin and compliance officers can access all client data
  if (req.user.roles.includes(UserRole.ADMIN) || req.user.roles.includes(UserRole.COMPLIANCE_OFFICER)) {
    next();
    return;
  }

  // Clients can only access their own data
  if (req.user.roles.includes(UserRole.CLIENT)) {
    if (req.user.clientId !== clientId) {
      logSecurity('Unauthorized client data access attempt', req.user.id, {
        requestedClientId: clientId,
        userClientId: req.user.clientId,
        path: req.path,
      });

      res.status(403).json({
        success: false,
        error: {
          code: 'UNAUTHORIZED_ACCESS',
          message: 'You can only access your own data',
        },
      });
      return;
    }
  }

  // Advisors can access their assigned clients (this would require additional database check)
  if (req.user.roles.includes(UserRole.ADVISOR)) {
    // TODO: Implement advisor-client relationship check
    // For now, allow access (in production, you'd check the database)
  }

  next();
};

/**
 * Middleware for optional authentication (doesn't fail if no token)
 */
export const optionalAuth = (req: Request, res: Response, next: NextFunction): void => {
  const authHeader = req.headers.authorization;
  const token = authHeader && authHeader.split(' ')[1];

  if (!token) {
    next();
    return;
  }

  try {
    const payload = jwtService.verifyAccessToken(token);
    req.user = {
      id: payload.sub,
      email: payload.sub,
      roles: payload.roles,
      permissions: payload.permissions,
      clientId: payload.clientId,
      sessionId: payload.sessionId,
    };
  } catch (error) {
    // Log the error but don't fail the request
    logger.warn('Optional auth failed:', error);
  }

  next();
};

/**
 * Middleware to validate API key for service-to-service communication
 */
export const validateApiKey = (req: Request, res: Response, next: NextFunction): void => {
  const apiKey = req.headers['x-api-key'] as string;

  if (!apiKey) {
    res.status(401).json({
      success: false,
      error: {
        code: 'MISSING_API_KEY',
        message: 'API key is required',
      },
    });
    return;
  }

  // TODO: Implement API key validation logic
  // For now, accept any API key (in production, validate against database)
  
  next();
};