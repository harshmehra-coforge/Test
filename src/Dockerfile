FROM openjdk:21-jre-slim

LABEL maintainer="FNOL Team"
LABEL version="1.0.0"
LABEL description="Base Docker image for FNOL microservices"

# Create app directory
WORKDIR /app

# Create non-root user
RUN groupadd -r fnol && useradd -r -g fnol fnol

# Install required packages
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Copy application jar
ARG JAR_FILE
COPY ${JAR_FILE} app.jar

# Change ownership to non-root user
RUN chown -R fnol:fnol /app

# Switch to non-root user
USER fnol

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/actuator/health || exit 1

# Expose port
EXPOSE 8080

# JVM options for containerized environment
ENV JAVA_OPTS="-XX:+UseContainerSupport -XX:MaxRAMPercentage=75.0 -XX:+UseG1GC -XX:+UseStringDeduplication"

# Run the application
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]