# Wealth Management Platform

A comprehensive microservices-based wealth management application platform built with Node.js, TypeScript, PostgreSQL, Redis, and Apache Kafka.

## Architecture Overview

The platform consists of 7 core microservices:

- **Event Bus Service (Port 3000)** - Central event-driven communication hub
- **Payment Service (Port 3001)** - Multi-currency payment processing with compliance integration
- **Portfolio Service (Port 3002)** - Portfolio management, performance tracking, and goal alignment
- **Compliance Service (Port 3003)** - AML/OFAC compliance monitoring and screening
- **Notification Service (Port 3004)** - Multi-channel notifications (Email, SMS, Push)
- **Collaboration Hub Service (Port 3005)** - External system integration with Northern Trust
- **Audit Service (Port 3006)** - Comprehensive audit logging and compliance reporting

## Technology Stack

### Backend & Runtime
- **Node.js 20+** - JavaScript runtime
- **TypeScript 5.0+** - Type-safe JavaScript
- **Express.js 4.18+** - Web application framework

### Databases & Storage
- **PostgreSQL 15+** - Primary database with ACID compliance
- **Redis 7.0+** - Caching and session management
- **Apache Kafka 3.5+** - Event streaming and message queuing

### Security & Authentication
- **JWT** - Stateless authentication with refresh tokens
- **AES-256** - Data encryption at rest
- **TLS 1.3** - Encryption in transit
- **Multi-Factor Authentication** - TOTP, SMS, Email support
- **Role-Based Access Control** - Fine-grained permissions

### Development & Operations
- **Prisma** - Type-safe database ORM
- **Docker & Docker Compose** - Containerization
- **Kubernetes** - Container orchestration
- **Jest** - Testing framework
- **ESLint & Prettier** - Code quality and formatting
- **Winston** - Structured logging
- **Swagger/OpenAPI** - API documentation

## Quick Start

### Prerequisites

- Node.js 20+
- Docker and Docker Compose
- PostgreSQL 15+ (if running locally)
- Redis 7+ (if running locally)
- Apache Kafka 3.5+ (if running locally)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd wealth-management-platform
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. **Generate Prisma client**
   ```bash
   npm run db:generate
   ```

5. **Run database migrations**
   ```bash
   npm run db:migrate
   ```

6. **Seed the database**
   ```bash
   npm run db:seed
   ```

### Running with Docker Compose (Recommended)

1. **Start all services**
   ```bash
   docker-compose up -d
   ```

2. **View logs**
   ```bash
   docker-compose logs -f
   ```

3. **Stop all services**
   ```bash
   docker-compose down
   ```

### Running Individual Services Locally

1. **Start infrastructure services**
   ```bash
   docker-compose up -d postgres redis kafka
   ```

2. **Start individual services**
   ```bash
   # Event Bus Service
   npm run start:event-bus

   # Payment Service
   npm run start:payment

   # Portfolio Service
   npm run start:portfolio

   # Compliance Service
   npm run start:compliance

   # Notification Service
   npm run start:notification

   # Collaboration Hub Service
   npm run start:collaboration

   # Audit Service
   npm run start:audit
   ```

## API Documentation

Each service provides interactive API documentation via Swagger UI:

- Event Bus: http://localhost:3000/api-docs
- Payment Service: http://localhost:3001/api-docs
- Portfolio Service: http://localhost:3002/api-docs
- Compliance Service: http://localhost:3003/api-docs
- Notification Service: http://localhost:3004/api-docs
- Collaboration Hub: http://localhost:3005/api-docs
- Audit Service: http://localhost:3006/api-docs

## Health Checks

Each service provides health check endpoints:

- Health: `GET /health` - Basic service health
- Ready: `GET /ready` - Service readiness with dependency checks

## Configuration

### Environment Variables

Key environment variables (see `.env.example` for complete list):

```bash
# Service Configuration
SERVICE_NAME=event-bus  # Service to start
PORT=3000              # Service port
NODE_ENV=development   # Environment

# Database
DATABASE_URL=postgresql://username:password@localhost:5432/wealth_management

# Redis
REDIS_URL=redis://localhost:6379

# Kafka
KAFKA_BROKERS=localhost:9092

# Security
JWT_SECRET=your-super-secret-jwt-key
ENCRYPTION_KEY=your-32-character-encryption-key

# External APIs
NORTHERN_TRUST_API_KEY=your-api-key
MARKET_DATA_API_KEY=your-api-key
OFAC_API_KEY=your-api-key
```

### Business Rules Configuration

```bash
# Compliance Thresholds
AML_THRESHOLD_AMOUNT=10000
COMPLIANCE_REVIEW_THRESHOLD=1000000

# Account Limits
MIN_ACCOUNT_BALANCE=250000
MAX_DAILY_TRANSFER_LIMIT=10000000

# Portfolio Management
REBALANCING_THRESHOLD=0.05
GOAL_ALIGNMENT_MIN_SCORE=6
```

## Development

### Code Structure

```
src/
├── config/           # Configuration management
├── middleware/       # Express middleware
├── services/         # Microservices
│   ├── event-bus/    # Event Bus Service
│   ├── payment/      # Payment Service
│   ├── portfolio/    # Portfolio Service
│   ├── compliance/   # Compliance Service
│   ├── notification/ # Notification Service
│   ├── collaboration/ # Collaboration Hub
│   └── audit/        # Audit Service
├── types/            # TypeScript type definitions
├── utils/            # Utility functions
└── index.ts          # Application entry point
```

### Development Commands

```bash
# Development
npm run dev                    # Start in development mode
npm run build                  # Build for production
npm run start                  # Start production build

# Database
npm run db:generate           # Generate Prisma client
npm run db:migrate           # Run database migrations
npm run db:seed              # Seed database with sample data
npm run db:studio            # Open Prisma Studio

# Testing
npm run test                 # Run tests
npm run test:watch          # Run tests in watch mode
npm run test:coverage       # Run tests with coverage

# Code Quality
npm run lint                # Run ESLint
npm run lint:fix           # Fix ESLint issues
npm run format             # Format code with Prettier
npm run type-check         # TypeScript type checking

# Docker
npm run docker:build       # Build Docker images
npm run docker:up          # Start with Docker Compose
npm run docker:down        # Stop Docker Compose
```

### Testing

The platform includes comprehensive testing:

```bash
# Unit Tests
npm run test:unit

# Integration Tests
npm run test:integration

# End-to-End Tests
npm run test:e2e

# Coverage Report
npm run test:coverage
```

### Code Quality

- **ESLint** - Linting with TypeScript support
- **Prettier** - Code formatting
- **Husky** - Git hooks for pre-commit checks
- **TypeScript** - Static type checking

## Security

### Authentication & Authorization

- **JWT Tokens** - Stateless authentication with 4-hour expiry
- **Refresh Tokens** - 7-day expiry for token renewal
- **Multi-Factor Authentication** - TOTP, SMS, and Email support
- **Role-Based Access Control** - CLIENT, ADVISOR, COMPLIANCE_OFFICER, OPERATIONS, ADMIN

### Data Protection

- **Encryption at Rest** - AES-256-GCM for sensitive data
- **Encryption in Transit** - TLS 1.3 for all communications
- **Data Masking** - PII masking in logs and non-production environments
- **Key Management** - Secure key rotation and storage

### Security Monitoring

- **Rate Limiting** - API rate limiting with different tiers
- **Audit Logging** - Comprehensive audit trails
- **Security Events** - Real-time security event monitoring
- **Intrusion Detection** - Automated threat detection

## Compliance

### Regulatory Requirements

- **AML (Anti-Money Laundering)** - Automated transaction monitoring
- **OFAC Sanctions** - Real-time sanctions list screening
- **KYC (Know Your Customer)** - Identity verification workflows
- **SOX Compliance** - Financial reporting controls
- **GDPR/CCPA** - Data privacy and protection

### Audit & Reporting

- **Immutable Audit Logs** - Tamper-proof audit trails
- **Regulatory Reporting** - Automated compliance reports
- **Data Retention** - 7-year retention for compliance records
- **Examination Support** - Tools for regulatory examinations

## Monitoring & Observability

### Logging

- **Structured Logging** - JSON-formatted logs with Winston
- **Log Levels** - DEBUG, INFO, WARN, ERROR, FATAL
- **Request Tracing** - Unique request IDs for correlation
- **Performance Logging** - Response time and resource usage

### Metrics & Monitoring

- **Health Checks** - Service health and dependency monitoring
- **Performance Metrics** - Response times, throughput, error rates
- **Business Metrics** - Transaction volumes, compliance rates
- **Infrastructure Metrics** - CPU, memory, disk, network usage

### Alerting

- **Critical Alerts** - PagerDuty integration for critical issues
- **Warning Alerts** - Slack notifications for warnings
- **Compliance Alerts** - Real-time compliance violation alerts
- **Performance Alerts** - SLA breach notifications

## Deployment

### Docker Deployment

```bash
# Build and deploy all services
docker-compose up -d

# Scale specific services
docker-compose up -d --scale payment-service=3

# Update specific service
docker-compose up -d --no-deps payment-service
```

### Kubernetes Deployment

```bash
# Deploy to Kubernetes
kubectl apply -f k8s/

# Check deployment status
kubectl get pods -n wealth-management

# View logs
kubectl logs -f deployment/payment-service -n wealth-management
```

### Production Considerations

- **Load Balancing** - Nginx or cloud load balancer
- **Auto-Scaling** - Horizontal pod autoscaling based on metrics
- **Database Scaling** - Read replicas and connection pooling
- **Caching Strategy** - Redis cluster for high availability
- **Backup & Recovery** - Automated backups with point-in-time recovery

## API Examples

### Authentication

```bash
# Login
curl -X POST http://localhost:3001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "client@example.com",
    "password": "password123"
  }'

# Response
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "uuid",
      "email": "client@example.com",
      "role": "CLIENT"
    }
  }
}
```

### Create Payment

```bash
curl -X POST http://localhost:3001/api/v1/payments \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "accountId": "account-uuid",
    "type": "WIRE_TRANSFER",
    "amount": "50000.00",
    "currency": "USD",
    "recipientInfo": {
      "name": "John Doe",
      "accountNumber": "123456789",
      "routingNumber": "021000021",
      "bankName": "Chase Bank"
    }
  }'
```

### Get Portfolio Performance

```bash
curl -X GET "http://localhost:3002/api/v1/portfolios/portfolio-uuid/performance?period=1Y" \
  -H "Authorization: Bearer <access_token>"
```

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   ```bash
   # Check database status
   docker-compose ps postgres
   
   # View database logs
   docker-compose logs postgres
   
   # Reset database
   docker-compose down -v
   docker-compose up -d postgres
   ```

2. **Kafka Connection Issues**
   ```bash
   # Check Kafka status
   docker-compose ps kafka
   
   # List Kafka topics
   docker-compose exec kafka kafka-topics --bootstrap-server localhost:9092 --list
   ```

3. **Service Startup Issues**
   ```bash
   # Check service logs
   docker-compose logs service-name
   
   # Restart specific service
   docker-compose restart service-name
   ```

### Performance Tuning

1. **Database Optimization**
   - Connection pooling configuration
   - Query optimization and indexing
   - Read replica configuration

2. **Caching Strategy**
   - Redis cache configuration
   - Cache TTL optimization
   - Cache invalidation strategies

3. **Event Processing**
   - Kafka partition configuration
   - Consumer group optimization
   - Batch processing tuning

## Contributing

### Development Workflow

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Run quality checks
5. Submit a pull request

### Code Standards

- Follow TypeScript best practices
- Write comprehensive tests
- Document API changes
- Follow conventional commit messages
- Ensure security best practices

## License

This project is proprietary software. All rights reserved.

## Support

For technical support and questions:

- **Documentation**: Check API documentation at `/api-docs` endpoints
- **Issues**: Create GitHub issues for bugs and feature requests
- **Security**: Report security issues privately to security@company.com

---

**Note**: This is a production-ready wealth management platform with comprehensive security, compliance, and monitoring capabilities. Ensure proper configuration and security measures before deploying to production environments.