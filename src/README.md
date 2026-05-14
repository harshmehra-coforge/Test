# FNOL (First Notice of Loss) System

A comprehensive Property & Casualty Insurance FNOL system built with modern microservices architecture using Java 21, Spring Boot 3.4.5, and cloud-native technologies.

## 🏗️ Architecture Overview

The FNOL system is designed as a cloud-native microservices architecture that modernizes legacy COBOL-based insurance claim processing. The system enables efficient management of insurance policies, claims processing, adjuster assignments, coverage validation, and comprehensive reporting capabilities.

### Key Features

- **Microservices Architecture**: Domain-driven design with loosely coupled services
- **Event-Driven Architecture**: Asynchronous communication using CQRS and Event Sourcing
- **Cloud-Native Design**: Containerized deployment with Kubernetes orchestration
- **API-First Approach**: RESTful APIs with OpenAPI 3.1.0 specifications
- **Security by Design**: Multi-layered security with JWT authentication and OAuth2
- **Scalability & Resilience**: Horizontal scaling with circuit breaker patterns

## 🛠️ Technology Stack

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Runtime** | Java | 21 (LTS) | Application runtime environment |
| **Framework** | Spring Boot | 3.4.5 | Microservices framework |
| **Web Framework** | Spring WebMVC | 6.2.7 | REST API development |
| **Data Access** | Spring Data JPA | 3.4.5 | Database abstraction layer |
| **Security** | Spring Security | 6.4.4 | Authentication & authorization |
| **Service Discovery** | Netflix Eureka | Latest | Service registration & discovery |
| **API Gateway** | Spring Cloud Gateway | Latest | Request routing & load balancing |
| **Configuration** | Spring Cloud Config | Latest | Centralized configuration |
| **Database** | MySQL | 9.4.0 | Primary data storage |
| **Caching** | Redis | 8.2.1 | In-memory caching |
| **Messaging** | RabbitMQ | 3.13.0 | Event-driven communication |
| **Authentication** | JWT | java-jwt | Stateless authentication |
| **Documentation** | OpenAPI/Swagger | 3.1.0 | API documentation |
| **Build Tool** | Maven | 3.9.11 | Dependency management |
| **Containerization** | Docker | Latest | Application packaging |
| **Orchestration** | Kubernetes | 1.28+ | Container orchestration |

## 🏢 System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web Client    │    │  Mobile Client  │    │  API Clients    │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │      API Gateway          │
                    │  (Spring Cloud Gateway)   │
                    └─────────────┬─────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        │                         │                         │
┌───────▼────────┐    ┌──────────▼──────────┐    ┌─────────▼────────┐
│ User Service   │    │   Policy Service    │    │  Claim Service   │
│ (Auth & Users) │    │ (Policy Management) │    │ (FNOL Processing)│
└────────────────┘    └─────────────────────┘    └──────────────────┘
        │                         │                         │
        └─────────────────────────┼─────────────────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        │                         │                         │
┌───────▼────────┐    ┌──────────▼──────────┐    ┌─────────▼────────┐
│Adjuster Service│    │  Coverage Service   │    │ Report Service   │
│ (Assignments)  │    │ (Validation Rules)  │    │ (Analytics)      │
└────────────────┘    └─────────────────────┘    └──────────────────┘
        │                         │                         │
        └─────────────────────────┼─────────────────────────┘
                                  │
                    ┌─────────────▼─────────────┐
                    │  Notification Service     │
                    │ (Multi-channel Alerts)    │
                    └───────────────────────────┘
```

## 📋 Prerequisites

- **Java 21** (OpenJDK or Oracle JDK)
- **Maven 3.9+**
- **Docker 20.10+**
- **Docker Compose 2.0+**
- **Kubernetes 1.28+** (for production deployment)
- **kubectl** (for Kubernetes management)

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/fnol-system.git
cd fnol-system
```

### 2. Build All Services

```bash
# Build all microservices
mvn clean package -DskipTests

# Or build with Docker images
mvn clean package -Pdocker
```

### 3. Start with Docker Compose (Recommended for Development)

```bash
# Start all services and infrastructure
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f api-gateway
```

### 4. Access the System

- **API Gateway**: http://localhost:8080
- **Eureka Dashboard**: http://localhost:8761
- **Config Server**: http://localhost:8888
- **Grafana Dashboard**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## 🔧 Development Setup

### Local Development Environment

1. **Start Infrastructure Services**
   ```bash
   # Start only infrastructure (MySQL, Redis, RabbitMQ)
   docker-compose up -d mysql redis rabbitmq
   ```

2. **Run Services Individually**
   ```bash
   # Start Eureka Server
   cd eureka-server && mvn spring-boot:run

   # Start Config Server
   cd config-server && mvn spring-boot:run

   # Start API Gateway
   cd api-gateway && mvn spring-boot:run

   # Start individual microservices
   cd user-service && mvn spring-boot:run
   cd policy-service && mvn spring-boot:run
   ```

### Environment Configuration

Create environment-specific configuration files:

```bash
# Development environment
export SPRING_PROFILES_ACTIVE=dev
export MYSQL_HOST=localhost
export REDIS_HOST=localhost
export RABBITMQ_HOST=localhost

# Docker environment
export SPRING_PROFILES_ACTIVE=docker

# Kubernetes environment
export SPRING_PROFILES_ACTIVE=kubernetes
```

## 🐳 Docker Deployment

### Build Docker Images

```bash
# Build all service images
docker-compose build

# Build specific service
docker build -t fnol/user-service:1.0.0 \
  --build-arg JAR_FILE=user-service/target/user-service-1.0.0.jar .
```

### Docker Compose Profiles

```bash
# Development profile (minimal services)
docker-compose --profile dev up -d

# Full profile (all services + monitoring)
docker-compose --profile full up -d

# Production profile (optimized for production)
docker-compose --profile prod up -d
```

## ☸️ Kubernetes Deployment

### Prerequisites

```bash
# Create namespaces
kubectl apply -f k8s/config/namespaces.yaml

# Apply secrets and config maps
kubectl apply -f k8s/config/secrets.yaml
```

### Deploy Infrastructure

```bash
# Deploy MySQL
kubectl apply -f k8s/infrastructure/mysql.yaml

# Deploy Redis
kubectl apply -f k8s/infrastructure/redis.yaml

# Deploy RabbitMQ
kubectl apply -f k8s/infrastructure/rabbitmq.yaml

# Deploy Eureka and Config Server
kubectl apply -f k8s/infrastructure/eureka-config.yaml
```

### Deploy Application Services

```bash
# Deploy API Gateway
kubectl apply -f k8s/services/api-gateway.yaml

# Deploy microservices
kubectl apply -f k8s/services/user-service.yaml
kubectl apply -f k8s/services/policy-service.yaml
kubectl apply -f k8s/services/claim-service.yaml
kubectl apply -f k8s/services/adjuster-service.yaml
kubectl apply -f k8s/services/coverage-service.yaml
kubectl apply -f k8s/services/report-service.yaml
kubectl apply -f k8s/services/notification-service.yaml
```

### Monitoring and Observability

```bash
# Deploy Prometheus and Grafana
kubectl apply -f k8s/monitoring/prometheus.yaml
kubectl apply -f k8s/monitoring/grafana.yaml

# Deploy Jaeger for distributed tracing
kubectl apply -f k8s/monitoring/jaeger.yaml
```

## 🔐 Security Configuration

### JWT Configuration

```yaml
jwt:
  secret: ${JWT_SECRET:your-secret-key}
  expiration: 86400 # 24 hours
  refresh-expiration: 604800 # 7 days
```

### Database Security

```yaml
spring:
  datasource:
    url: jdbc:mysql://${MYSQL_HOST:localhost}:${MYSQL_PORT:3306}/${MYSQL_DATABASE:fnol_db}?useSSL=true&requireSSL=true
    username: ${MYSQL_USERNAME:fnol_user}
    password: ${MYSQL_PASSWORD:fnol_password}
```

### API Security

All API endpoints are secured except:
- `/api/v1/auth/login`
- `/api/v1/auth/register`
- `/actuator/health`
- `/swagger-ui/**`
- `/v3/api-docs/**`

## 📊 Monitoring and Observability

### Metrics Collection

- **Application Metrics**: Micrometer + Prometheus
- **Infrastructure Metrics**: Node Exporter
- **Custom Business Metrics**: Policy creation rate, claim processing time

### Distributed Tracing

- **Jaeger**: End-to-end request tracing
- **Correlation IDs**: Request correlation across services

### Logging

- **Structured Logging**: JSON format with correlation IDs
- **Log Aggregation**: ELK Stack (Elasticsearch, Logstash, Kibana)
- **Log Levels**: INFO, WARN, ERROR with appropriate filtering

### Health Checks

```bash
# Check service health
curl http://localhost:8080/actuator/health

# Check detailed health with authentication
curl -H "Authorization: Bearer <token>" \
  http://localhost:8080/actuator/health/detailed
```

## 🧪 Testing

### Unit Tests

```bash
# Run unit tests for all services
mvn test

# Run tests for specific service
cd user-service && mvn test
```

### Integration Tests

```bash
# Run integration tests with TestContainers
mvn verify -Pintegration-tests
```

### API Testing

```bash
# Import Postman collection
# File: docs/postman/FNOL-API-Collection.json

# Or use curl examples
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

## 📈 Performance Tuning

### JVM Optimization

```bash
# Production JVM settings
JAVA_OPTS="-XX:+UseContainerSupport \
           -XX:MaxRAMPercentage=75.0 \
           -XX:+UseG1GC \
           -XX:+UseStringDeduplication \
           -XX:+PrintGCDetails \
           -XX:+PrintGCTimeStamps"
```

### Database Optimization

```sql
-- MySQL optimization settings
SET GLOBAL innodb_buffer_pool_size = 1073741824; -- 1GB
SET GLOBAL max_connections = 200;
SET GLOBAL query_cache_size = 268435456; -- 256MB
```

### Redis Configuration

```redis
# Redis optimization
maxmemory 512mb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

```yaml
name: FNOL CI/CD Pipeline
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-java@v3
        with:
          java-version: '21'
      - run: mvn clean test
      
  build:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - run: mvn clean package -DskipTests
      - run: docker build -t fnol/app:${{ github.sha }} .
      
  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - run: kubectl apply -f k8s/
```

## 🐛 Troubleshooting

### Common Issues

1. **Service Discovery Issues**
   ```bash
   # Check Eureka registration
   curl http://localhost:8761/eureka/apps
   
   # Restart service with debug logging
   java -Dlogging.level.com.netflix.eureka=DEBUG -jar app.jar
   ```

2. **Database Connection Issues**
   ```bash
   # Test MySQL connection
   mysql -h localhost -u fnol_user -p fnol_db
   
   # Check connection pool metrics
   curl http://localhost:8080/actuator/metrics/hikaricp.connections
   ```

3. **Memory Issues**
   ```bash
   # Check JVM memory usage
   curl http://localhost:8080/actuator/metrics/jvm.memory.used
   
   # Generate heap dump
   jcmd <pid> GC.run_finalization
   jcmd <pid> VM.gc
   ```

### Log Analysis

```bash
# Follow application logs
docker-compose logs -f user-service

# Search for errors
docker-compose logs user-service | grep ERROR

# Check specific time range
docker-compose logs --since="2024-01-01T10:00:00" user-service
```

## 📚 API Documentation

### Swagger UI

Access interactive API documentation at:
- **API Gateway**: http://localhost:8080/swagger-ui.html
- **Individual Services**: http://localhost:808X/swagger-ui.html

### API Endpoints

| Service | Base URL | Description |
|---------|----------|-------------|
| User Service | `/api/v1/auth`, `/api/v1/users` | Authentication & user management |
| Policy Service | `/api/v1/policies` | Policy management |
| Claim Service | `/api/v1/claims`, `/api/v1/fnol` | Claim processing |
| Adjuster Service | `/api/v1/adjusters`, `/api/v1/assignments` | Adjuster management |
| Coverage Service | `/api/v1/coverage` | Coverage validation |
| Report Service | `/api/v1/reports` | Report generation |
| Notification Service | `/api/v1/notifications` | Notification management |

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards

- Follow Java coding conventions
- Write unit tests for new features
- Update documentation for API changes
- Use conventional commit messages

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- **Email**: support@fnol-system.com
- **Documentation**: https://docs.fnol-system.com
- **Issues**: https://github.com/your-org/fnol-system/issues

## 🗺️ Roadmap

- [ ] **Phase 1**: Core FNOL functionality (Q1 2024)
- [ ] **Phase 2**: Advanced analytics and ML integration (Q2 2024)
- [ ] **Phase 3**: Mobile application (Q3 2024)
- [ ] **Phase 4**: Third-party integrations (Q4 2024)

---

**Built with ❤️ by the FNOL Development Team**