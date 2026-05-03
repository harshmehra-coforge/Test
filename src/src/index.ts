import dotenv from 'dotenv';
import { config } from './config';
import { logger } from './utils/logger';

// Load environment variables
dotenv.config();

// Import services based on SERVICE_NAME environment variable
const serviceName = config.serviceName;

async function startService(): Promise<void> {
  try {
    logger.info(`Starting ${serviceName} service...`);

    switch (serviceName) {
      case 'event-bus': {
        const EventBusService = (await import('./services/event-bus')).default;
        const service = new EventBusService();
        await service.start();
        break;
      }
      case 'payment': {
        const PaymentService = (await import('./services/payment')).default;
        const service = new PaymentService();
        await service.start();
        break;
      }
      case 'portfolio': {
        const PortfolioService = (await import('./services/portfolio')).default;
        const service = new PortfolioService();
        await service.start();
        break;
      }
      case 'compliance': {
        const ComplianceService = (await import('./services/compliance')).default;
        const service = new ComplianceService();
        await service.start();
        break;
      }
      case 'notification': {
        const NotificationService = (await import('./services/notification')).default;
        const service = new NotificationService();
        await service.start();
        break;
      }
      case 'collaboration': {
        const CollaborationService = (await import('./services/collaboration')).default;
        const service = new CollaborationService();
        await service.start();
        break;
      }
      case 'audit': {
        const AuditService = (await import('./services/audit')).default;
        const service = new AuditService();
        await service.start();
        break;
      }
      default:
        throw new Error(`Unknown service: ${serviceName}`);
    }

    logger.info(`${serviceName} service started successfully`);
  } catch (error) {
    logger.error(`Failed to start ${serviceName} service:`, error);
    process.exit(1);
  }
}

// Handle uncaught exceptions and unhandled rejections
process.on('uncaughtException', (error: Error) => {
  logger.error('Uncaught Exception:', error);
  process.exit(1);
});

process.on('unhandledRejection', (reason: any, promise: Promise<any>) => {
  logger.error('Unhandled Rejection at:', promise, 'reason:', reason);
  process.exit(1);
});

// Start the service
startService();