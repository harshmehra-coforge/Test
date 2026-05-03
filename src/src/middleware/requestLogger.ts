import { Request, Response, NextFunction } from 'express';
import { v4 as uuidv4 } from 'uuid';
import { logger, logWithContext } from '@/utils/logger';

/**
 * Middleware to add request ID and log requests
 */
export const requestLogger = (req: Request, res: Response, next: NextFunction): void => {
  // Generate unique request ID
  const requestId = uuidv4();
  req.requestId = requestId;

  // Add request ID to response headers
  res.setHeader('X-Request-ID', requestId);

  // Log request start
  const startTime = Date.now();
  
  logWithContext('info', 'Request started', {
    requestId,
    method: req.method,
    url: req.url,
    path: req.path,
    query: req.query,
    ip: req.ip,
    userAgent: req.get('User-Agent'),
    userId: req.user?.id,
  });

  // Override res.json to log response
  const originalJson = res.json;
  res.json = function(body: any) {
    const duration = Date.now() - startTime;
    
    logWithContext('info', 'Request completed', {
      requestId,
      method: req.method,
      url: req.url,
      path: req.path,
      statusCode: res.statusCode,
      duration,
      userId: req.user?.id,
      responseSize: JSON.stringify(body).length,
    });

    return originalJson.call(this, body);
  };

  // Override res.send to log response
  const originalSend = res.send;
  res.send = function(body: any) {
    const duration = Date.now() - startTime;
    
    logWithContext('info', 'Request completed', {
      requestId,
      method: req.method,
      url: req.url,
      path: req.path,
      statusCode: res.statusCode,
      duration,
      userId: req.user?.id,
      responseSize: typeof body === 'string' ? body.length : JSON.stringify(body).length,
    });

    return originalSend.call(this, body);
  };

  next();
};

/**
 * Middleware to log slow requests
 */
export const slowRequestLogger = (threshold: number = 1000) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    const startTime = Date.now();

    res.on('finish', () => {
      const duration = Date.now() - startTime;
      
      if (duration > threshold) {
        logger.warn('Slow request detected', {
          requestId: req.requestId,
          method: req.method,
          url: req.url,
          path: req.path,
          duration,
          threshold,
          statusCode: res.statusCode,
          userId: req.user?.id,
        });
      }
    });

    next();
  };
};

/**
 * Middleware to log security-sensitive operations
 */
export const securityLogger = (operation: string) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    logWithContext('info', `Security operation: ${operation}`, {
      requestId: req.requestId,
      operation,
      method: req.method,
      path: req.path,
      ip: req.ip,
      userAgent: req.get('User-Agent'),
      userId: req.user?.id,
      timestamp: new Date().toISOString(),
    });

    next();
  };
};

/**
 * Middleware to log audit events
 */
export const auditLogger = (action: string, resourceType: string) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    const resourceId = req.params.id || req.params.clientId || req.params.accountId;
    
    logWithContext('info', `Audit: ${action}`, {
      requestId: req.requestId,
      action,
      resourceType,
      resourceId,
      method: req.method,
      path: req.path,
      ip: req.ip,
      userId: req.user?.id,
      timestamp: new Date().toISOString(),
      requestBody: req.method !== 'GET' ? req.body : undefined,
    });

    next();
  };
};