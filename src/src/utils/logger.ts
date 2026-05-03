import winston from 'winston';
import { config } from '@/config';

// Define log levels
const levels = {
  error: 0,
  warn: 1,
  info: 2,
  http: 3,
  debug: 4,
};

// Define colors for each level
const colors = {
  error: 'red',
  warn: 'yellow',
  info: 'green',
  http: 'magenta',
  debug: 'white',
};

// Tell winston that you want to link the colors
winston.addColors(colors);

// Define format for logs
const format = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss:ms' }),
  winston.format.colorize({ all: true }),
  winston.format.printf(
    (info) => `${info.timestamp} ${info.level}: ${info.message}`
  )
);

// Define which transports the logger must use
const transports = [
  // Console transport
  new winston.transports.Console({
    format: winston.format.combine(
      winston.format.colorize(),
      winston.format.simple()
    ),
  }),
  
  // File transport for errors
  new winston.transports.File({
    filename: 'logs/error.log',
    level: 'error',
    format: winston.format.combine(
      winston.format.timestamp(),
      winston.format.json()
    ),
  }),
  
  // File transport for all logs
  new winston.transports.File({
    filename: config.logging.file,
    format: winston.format.combine(
      winston.format.timestamp(),
      winston.format.json()
    ),
    maxsize: parseInt(config.logging.maxSize.replace('m', '')) * 1024 * 1024,
    maxFiles: parseInt(config.logging.maxFiles),
  }),
];

// Create the logger
export const logger = winston.createLogger({
  level: config.logging.level,
  levels,
  format,
  transports,
  exitOnError: false,
});

// Create a stream object for Morgan HTTP logger
export const loggerStream = {
  write: (message: string) => {
    logger.http(message.trim());
  },
};

// Add request ID to logs
export const addRequestId = (requestId: string) => {
  return winston.format.printf((info) => {
    return `${info.timestamp} [${requestId}] ${info.level}: ${info.message}`;
  });
};

// Structured logging helper
export const logWithContext = (level: string, message: string, context?: any) => {
  const logData = {
    message,
    timestamp: new Date().toISOString(),
    service: config.serviceName,
    ...context,
  };
  
  logger.log(level, JSON.stringify(logData));
};

// Error logging helper
export const logError = (error: Error, context?: any) => {
  const errorData = {
    message: error.message,
    stack: error.stack,
    name: error.name,
    timestamp: new Date().toISOString(),
    service: config.serviceName,
    ...context,
  };
  
  logger.error(JSON.stringify(errorData));
};

// Performance logging helper
export const logPerformance = (operation: string, duration: number, context?: any) => {
  const perfData = {
    operation,
    duration,
    timestamp: new Date().toISOString(),
    service: config.serviceName,
    ...context,
  };
  
  logger.info(`Performance: ${operation} took ${duration}ms`, perfData);
};

// Security logging helper
export const logSecurity = (event: string, userId?: string, context?: any) => {
  const securityData = {
    event,
    userId,
    timestamp: new Date().toISOString(),
    service: config.serviceName,
    ...context,
  };
  
  logger.warn(`Security Event: ${event}`, securityData);
};

// Audit logging helper
export const logAudit = (action: string, userId: string, resourceType: string, resourceId?: string, context?: any) => {
  const auditData = {
    action,
    userId,
    resourceType,
    resourceId,
    timestamp: new Date().toISOString(),
    service: config.serviceName,
    ...context,
  };
  
  logger.info(`Audit: ${action} on ${resourceType}${resourceId ? ` (${resourceId})` : ''}`, auditData);
};