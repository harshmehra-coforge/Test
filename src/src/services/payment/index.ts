import express from 'express';
import cors from 'cors';
import helmet from 'helmet';
import compression from 'compression';
import morgan from 'morgan';
import swaggerJsdoc from 'swagger-jsdoc';
import swaggerUi from 'swagger-ui-express';

import { config } from '@/config';
import { logger } from '@/utils/logger';
import { errorHandler } from '@/middleware/errorHandler';
import { rateLimiter } from '@/middleware/rateLimiter';
import { requestLogger } from '@/middleware/requestLogger';
import { eventBus } from '../event-bus/EventBus';
import paymentRoutes from './routes';

class PaymentService {
  private app: express.Application;
  private server: any;

  constructor() {
    this.app = express();
    this.setupMiddleware();
    this.setupRoutes();
    this.setupSwagger();
    this.setupErrorHandling();
    this.setupEventHandlers();
  }

  private setupMiddleware(): void {
    // Security middleware
    this.app.use(helmet());
    this.app.use(cors({
      origin: config.cors.origin.split(','),
      credentials: config.cors.credentials,
    }));

    // Compression and parsing
    this.app.use(compression());
    this.app.use(express.json({ limit: '10mb' }));
    this.app.use(express.urlencoded({ extended: true }));

    // Logging and rate limiting
    this.app.use(morgan('combined', { stream: { write: (message) => logger.info(message.trim()) } }));
    this.app.use(requestLogger);
    this.app.use(rateLimiter);
  }

  private setupRoutes(): void {
    // Health check endpoint
    this.app.get('/health', (req, res) => {
      res.json({
        status: 'healthy',
        service: 'payment-service',
        timestamp: new Date().toISOString(),
        uptime: process.uptime(),
        version: process.env.npm_package_version || '1.0.0',
      });
    });

    // Ready check endpoint
    this.app.get('/ready', async (req, res) => {
      try {
        // Check database connection
        const { PrismaClient } = await import('@prisma/client');
        const prisma = new PrismaClient();
        await prisma.$queryRaw`SELECT 1`;
        await prisma.$disconnect();

        // Check event bus connection
        const eventBusHealth = await eventBus.healthCheck();

        res.json({
          status: 'ready',
          service: 'payment-service',
          timestamp: new Date().toISOString(),
          dependencies: {
            database: 'healthy',
            eventBus: eventBusHealth.status,
          },
        });
      } catch (error) {
        res.status(503).json({
          status: 'not ready',
          service: 'payment-service',
          timestamp: new Date().toISOString(),
          error: error instanceof Error ? error.message : 'Unknown error',
        });
      }
    });

    // API routes
    this.app.use('/api/v1/payments', paymentRoutes);

    // 404 handler
    this.app.use('*', (req, res) => {
      res.status(404).json({
        success: false,
        error: {
          code: 'NOT_FOUND',
          message: 'Endpoint not found',
        },
      });
    });
  }

  private setupSwagger(): void {
    const options = {
      definition: {
        openapi: '3.0.0',
        info: {
          title: 'Payment Service API',
          version: '1.0.0',
          description: 'Multi-currency payment processing service for the Wealth Management Platform',
        },
        servers: [
          {
            url: `http://localhost:${config.port}`,
            description: 'Development server',
          },
        ],
        components: {
          securitySchemes: {
            bearerAuth: {
              type: 'http',
              scheme: 'bearer',
              bearerFormat: 'JWT',
            },
          },
        },
      },
      apis: ['./src/services/payment/routes.ts'],
    };

    const specs = swaggerJsdoc(options);
    this.app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(specs));
  }

  private setupErrorHandling(): void {
    this.app.use(errorHandler);
  }

  private setupEventHandlers(): void {
    // Subscribe to compliance events
    eventBus.subscribe(
      config.kafka.topics.compliance,
      'payment-service-compliance',
      async (event) => {
        if (event.eventType === 'COMPLIANCE_ALERT') {
          await this.handleComplianceAlert(event);
        }
      }
    );

    // Subscribe to portfolio events for investment transactions
    eventBus.subscribe(
      config.kafka.topics.portfolio,
      'payment-service-portfolio',
      async (event) => {
        if (event.eventType === 'PORTFOLIO_REBALANCE_REQUIRED') {
          await this.handlePortfolioRebalance(event);
        }
      }
    );
  }

  private async handleComplianceAlert(event: any): Promise<void> {
    try {
      logger.info('Processing compliance alert for payment service:', event);
      
      // Handle compliance alerts related to payments
      if (event.payload.transactionId) {
        const { PrismaClient } = await import('@prisma/client');
        const prisma = new PrismaClient();
        
        // Update transaction status based on compliance alert
        await prisma.transaction.update({
          where: { id: event.payload.transactionId },
          data: {
            status: event.payload.severity === 'CRITICAL' ? 'REJECTED' : 'PENDING_COMPLIANCE',
            complianceNotes: event.payload.description,
          },
        });

        await prisma.$disconnect();
      }
    } catch (error) {
      logger.error('Error handling compliance alert:', error);
    }
  }

  private async handlePortfolioRebalance(event: any): Promise<void> {
    try {
      logger.info('Processing portfolio rebalance event:', event);
      
      // This would trigger investment transactions for rebalancing
      // Implementation would depend on specific business requirements
      
    } catch (error) {
      logger.error('Error handling portfolio rebalance:', error);
    }
  }

  async start(): Promise<void> {
    try {
      // Connect to event bus
      await eventBus.connect();

      // Start HTTP server
      this.server = this.app.listen(config.port, () => {
        logger.info(`Payment Service started on port ${config.port}`);
        logger.info(`API Documentation available at http://localhost:${config.port}/api-docs`);
      });

      // Graceful shutdown handlers
      process.on('SIGTERM', () => this.shutdown());
      process.on('SIGINT', () => this.shutdown());

    } catch (error) {
      logger.error('Failed to start Payment Service:', error);
      process.exit(1);
    }
  }

  private async shutdown(): Promise<void> {
    logger.info('Shutting down Payment Service...');

    try {
      // Close HTTP server
      if (this.server) {
        await new Promise<void>((resolve) => {
          this.server.close(() => resolve());
        });
      }

      // Disconnect event bus
      await eventBus.disconnect();

      logger.info('Payment Service shut down successfully');
      process.exit(0);
    } catch (error) {
      logger.error('Error during shutdown:', error);
      process.exit(1);
    }
  }
}

// Start the service if this file is run directly
if (require.main === module) {
  const service = new PaymentService();
  service.start().catch((error) => {
    logger.error('Failed to start service:', error);
    process.exit(1);
  });
}

export default PaymentService;