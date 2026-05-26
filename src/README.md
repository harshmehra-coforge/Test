# SSP Inbound Client System

A production-ready .NET Core 8 microservice for processing inbound client requests with comprehensive validation, transformation, and routing capabilities.

## 🏗️ Architecture Overview

The SSP Inbound Client System implements a microservices architecture with the following key components:

- **API Gateway Layer**: Request routing, rate limiting, and middleware
- **Processing Engine**: Orchestrates validation, transformation, and routing
- **Validation Service**: Business rules and data validation
- **Transformation Service**: Data mapping and format conversion
- **Routing Service**: Destination routing (HTTP, Message Queue, Database, File)
- **Caching Layer**: Multi-level caching with Redis and in-memory cache
- **Data Layer**: Entity Framework Core with SQL Server
- **Monitoring**: Application Insights integration with structured logging

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- SQL Server (or use Docker container)
- Redis (or use Docker container)

### Development Setup

1. **Clone and navigate to the project**
   ```bash
   cd code
   ```

2. **Copy environment configuration**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration values
   ```

3. **Start infrastructure services**
   ```bash
   docker-compose up -d sqlserver redis
   ```

4. **Run database migrations**
   ```bash
   cd src/SSPInboundClient
   dotnet ef database update
   ```

5. **Start the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Health Check: http://localhost:5000/health

### Docker Setup

1. **Start all services**
   ```bash
   docker-compose up -d
   ```

2. **View logs**
   ```bash
   docker-compose logs -f ssp-inbound-client
   ```

3. **Stop services**
   ```bash
   docker-compose down
   ```

## 📋 API Documentation

### Core Endpoints

#### Process Inbound Request
```http
POST /api/v1/inbound/process
Content-Type: application/json
Authorization: Bearer <token>

{
  "correlationId": "12345-67890",
  "clientId": "1",
  "timestamp": "2024-01-01T12:00:00Z",
  "requestType": "payment",
  "payload": {
    "amount": 100.00,
    "currency": "USD",
    "customerId": "CUST001"
  },
  "headers": {
    "X-Source-System": "WebPortal"
  },
  "priority": "Normal",
  "requiresCallback": false
}
```

#### Get Processing Status
```http
GET /api/v1/inbound/status/{correlationId}
Authorization: Bearer <token>
```

#### Batch Processing
```http
POST /api/v1/inbound/batch
Content-Type: application/json
Authorization: Bearer <token>

[
  {
    "correlationId": "batch-001",
    "clientId": "1",
    "requestType": "notification",
    // ... other fields
  }
]
```

#### Client Configuration
```http
GET /api/v1/clients/{clientId}/config
PUT /api/v1/clients/{clientId}/config
Authorization: Bearer <token>
```

#### System Health
```http
GET /api/v1/system/health
GET /api/v1/system/metrics
GET /api/v1/system/version
```

### Response Format

#### Success Response
```json
{
  "correlationId": "12345-67890",
  "status": "Success",
  "message": "Request processed successfully",
  "data": {
    "routingResult": "Processed",
    "destination": "https://api.payment-service.com/v1/process",
    "statusCode": 200
  },
  "processedAt": "2024-01-01T12:00:01Z",
  "processingTimeMs": 150,
  "transactionId": "TXN-789"
}
```

#### Error Response
```json
{
  "code": "VALIDATION_FAILED",
  "message": "Request validation failed",
  "correlationId": "12345-67890",
  "errors": [
    {
      "field": "amount",
      "code": "REQUIRED_FIELD",
      "message": "Amount is required",
      "severity": "Critical"
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## ⚙️ Configuration

### Application Settings

Key configuration sections in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SSPInboundClientDb;Trusted_Connection=true;",
    "Redis": "localhost:6379"
  },
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id"
  },
  "RateLimiting": {
    "DefaultRequestsPerMinute": 1000,
    "BurstRequestsPerMinute": 2000
  },
  "Processing": {
    "MaxConcurrentRequests": 10,
    "RequestTimeoutMs": 30000,
    "RetryAttempts": 3
  },
  "Cache": {
    "DefaultExpirationMinutes": 60,
    "ClientConfigExpirationMinutes": 30
  }
}
```

### Environment Variables

For production deployment, use environment variables:

```bash
# Database
ConnectionStrings__DefaultConnection="Server=prod-sql;Database=SSPInboundClientDb;..."
ConnectionStrings__Redis="prod-redis:6379"

# Azure Services
AzureAd__TenantId="prod-tenant-id"
ApplicationInsights__ConnectionString="InstrumentationKey=prod-key"

# Application Settings
RateLimiting__DefaultRequestsPerMinute=5000
Processing__MaxConcurrentRequests=50
```

## 🔒 Security

### Authentication & Authorization

The system uses Azure Entra (Azure AD) for authentication:

1. **JWT Token Authentication**: All API endpoints require valid JWT tokens
2. **API Key Authentication**: Alternative authentication for service-to-service calls
3. **Role-Based Access Control**: Different access levels for different client types

### Security Headers

Implemented security headers:
- `X-Frame-Options: SAMEORIGIN`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`

### Rate Limiting

- Default: 1000 requests/minute per client
- Burst: 2000 requests/minute for short periods
- IP-based limiting for unauthenticated requests

## 📊 Monitoring & Observability

### Application Insights Integration

- **Request Tracking**: All HTTP requests are tracked
- **Dependency Tracking**: Database, Redis, and HTTP calls
- **Exception Tracking**: Automatic exception logging
- **Custom Metrics**: Business metrics and performance counters

### Structured Logging

Using Serilog with structured logging:

```csharp
_logger.LogInformation("Request processed for {CorrelationId} in {Duration}ms", 
    correlationId, duration);
```

### Health Checks

- **Database**: SQL Server connectivity
- **Cache**: Redis connectivity
- **External Services**: Dependent service health

Access health checks at `/health`

### Metrics

Key metrics tracked:
- Request count by client and status
- Processing time by operation
- Error rates by component
- Cache hit rates
- Throughput (requests/second)

## 🗄️ Database Schema

### Core Tables

- **CLIENT**: Client information and configuration
- **REQUEST_LOG**: All processed requests
- **PROCESSING_RULE**: Validation and transformation rules
- **CLIENT_CONFIGURATION**: Client-specific settings
- **AUDIT_LOG**: System audit trail
- **ERROR_LOG**: Error tracking
- **TRANSFORMATION_MAPPING**: Data transformation rules

### Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

## 🔄 Processing Flow

1. **Request Reception**: API receives inbound request
2. **Authentication**: Validate client credentials
3. **Rate Limiting**: Check request limits
4. **Validation**: Apply business rules and data validation
5. **Transformation**: Convert data formats and apply mappings
6. **Routing**: Route to appropriate destination (HTTP/Queue/DB/File)
7. **Response**: Return processing result
8. **Logging**: Record request and audit information

## 🚀 Deployment

### Development
```bash
docker-compose up -d
```

### Production
```bash
# Using production compose file
docker-compose -f docker-compose.prod.yml up -d

# Or deploy to Azure Container Instances
az container create --resource-group myResourceGroup \
  --name ssp-inbound-client \
  --image myregistry.azurecr.io/ssp-inbound-client:latest
```

### Azure Deployment

The application is designed for Azure deployment with:
- **Azure App Service**: For hosting the API
- **Azure SQL Database**: For data persistence
- **Azure Redis Cache**: For caching
- **Azure Service Bus**: For message queuing
- **Azure Blob Storage**: For file storage
- **Azure Key Vault**: For secrets management
- **Application Insights**: For monitoring

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run integration tests
dotnet test --filter Category=Integration
```

### Load Testing
```bash
# Using Artillery.js
npm install -g artillery
artillery run load-test.yml
```

## 📈 Performance

### Optimization Features

- **Multi-level Caching**: Memory + Redis caching
- **Connection Pooling**: Optimized database connections
- **Async Processing**: Non-blocking operations
- **Batch Processing**: Efficient bulk operations
- **Circuit Breaker**: Fault tolerance for external calls
- **Retry Policies**: Automatic retry with exponential backoff

### Performance Targets

- **Response Time**: < 500ms (95th percentile)
- **Throughput**: 1000+ requests/second
- **Availability**: 99.9% uptime
- **Error Rate**: < 0.1%

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Issues**
   ```bash
   # Check connection string
   # Verify SQL Server is running
   docker-compose logs sqlserver
   ```

2. **Redis Connection Issues**
   ```bash
   # Check Redis connectivity
   docker-compose logs redis
   redis-cli ping
   ```

3. **Authentication Issues**
   ```bash
   # Verify Azure AD configuration
   # Check JWT token validity
   ```

### Logging

View application logs:
```bash
# Docker logs
docker-compose logs -f ssp-inbound-client

# Application Insights
# Check Azure portal for detailed telemetry
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Submit a pull request

### Code Standards

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation
- Use structured logging
- Follow SOLID principles

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation wiki

---

**Version**: 1.0.0  
**Last Updated**: 2024-01-01  
**Maintainer**: SSP Development Team