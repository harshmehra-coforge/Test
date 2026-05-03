import dotenv from 'dotenv';

// Load environment variables
dotenv.config();

export const config = {
  // Service configuration
  serviceName: process.env.SERVICE_NAME || 'wealth-management',
  port: parseInt(process.env.PORT || '3000', 10),
  environment: process.env.NODE_ENV || 'development',
  version: process.env.npm_package_version || '1.0.0',

  // Database configuration
  database: {
    url: process.env.DATABASE_URL || 'postgresql://localhost:5432/wealth_management',
    poolSize: parseInt(process.env.DATABASE_POOL_SIZE || '20', 10),
    timeout: parseInt(process.env.DATABASE_TIMEOUT || '30000', 10),
  },

  // Redis configuration
  redis: {
    url: process.env.REDIS_URL || 'redis://localhost:6379',
    password: process.env.REDIS_PASSWORD,
    db: parseInt(process.env.REDIS_DB || '0', 10),
    ttl: {
      session: parseInt(process.env.CACHE_TTL_SESSION_DATA || '14400', 10), // 4 hours
      cache: parseInt(process.env.CACHE_TTL_CLIENT_DATA || '1800', 10), // 30 minutes
      marketData: parseInt(process.env.CACHE_TTL_MARKET_DATA || '300', 10), // 5 minutes
      portfolioData: parseInt(process.env.CACHE_TTL_PORTFOLIO_DATA || '3600', 10), // 1 hour
    },
  },

  // Kafka configuration
  kafka: {
    brokers: (process.env.KAFKA_BROKERS || 'localhost:9092').split(','),
    clientId: process.env.KAFKA_CLIENT_ID || 'wealth-management-platform',
    groupId: process.env.KAFKA_GROUP_ID || 'wealth-management-group',
    topics: {
      events: 'wealth.events',
      payments: 'payment.events',
      portfolio: 'portfolio.events',
      compliance: 'compliance.events',
      notifications: 'notification.events',
      audit: 'audit.events',
    },
  },

  // Security configuration
  security: {
    jwt: {
      secret: process.env.JWT_SECRET || 'your-super-secret-jwt-key',
      expiresIn: process.env.JWT_EXPIRES_IN || '4h',
      refreshSecret: process.env.JWT_REFRESH_SECRET || 'your-super-secret-refresh-key',
      refreshExpiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '7d',
    },
    encryption: {
      key: process.env.ENCRYPTION_KEY || 'your-32-character-encryption-key',
      algorithm: process.env.ENCRYPTION_ALGORITHM || 'aes-256-gcm',
    },
    mfa: {
      issuer: process.env.MFA_ISSUER || 'Wealth Management Platform',
      window: parseInt(process.env.MFA_WINDOW || '2', 10),
    },
    bcrypt: {
      rounds: parseInt(process.env.BCRYPT_ROUNDS || '12', 10),
    },
    rateLimit: {
      windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS || '900000', 10), // 15 minutes
      maxRequests: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS || '100', 10),
    },
  },

  // CORS configuration
  cors: {
    origin: process.env.CORS_ORIGIN || 'http://localhost:3000',
    credentials: process.env.CORS_CREDENTIALS === 'true',
  },

  // External API configuration
  externalApis: {
    northernTrust: {
      url: process.env.NORTHERN_TRUST_API_URL || 'https://api.northerntrust.com',
      apiKey: process.env.NORTHERN_TRUST_API_KEY,
      clientId: process.env.NORTHERN_TRUST_CLIENT_ID,
      clientSecret: process.env.NORTHERN_TRUST_CLIENT_SECRET,
    },
    marketData: {
      url: process.env.MARKET_DATA_API_URL || 'https://api.marketdata.com',
      apiKey: process.env.MARKET_DATA_API_KEY,
    },
    ofac: {
      url: process.env.OFAC_API_URL || 'https://api.treasury.gov/ofac',
      apiKey: process.env.OFAC_API_KEY,
    },
    creditBureau: {
      url: process.env.CREDIT_BUREAU_API_URL || 'https://api.creditbureau.com',
      apiKey: process.env.CREDIT_BUREAU_API_KEY,
    },
  },

  // Notification configuration
  notifications: {
    email: {
      host: process.env.EMAIL_HOST || 'smtp.gmail.com',
      port: parseInt(process.env.EMAIL_PORT || '587', 10),
      user: process.env.EMAIL_USER,
      password: process.env.EMAIL_PASSWORD,
      from: process.env.EMAIL_FROM || 'noreply@wealthmanagement.com',
    },
    sms: {
      accountSid: process.env.TWILIO_ACCOUNT_SID,
      authToken: process.env.TWILIO_AUTH_TOKEN,
      phoneNumber: process.env.TWILIO_PHONE_NUMBER,
    },
  },

  // Logging configuration
  logging: {
    level: process.env.LOG_LEVEL || 'info',
    file: process.env.LOG_FILE || 'logs/app.log',
    maxSize: process.env.LOG_MAX_SIZE || '10m',
    maxFiles: process.env.LOG_MAX_FILES || '5',
  },

  // Business rules configuration
  businessRules: {
    amlThresholdAmount: parseFloat(process.env.AML_THRESHOLD_AMOUNT || '10000'),
    complianceReviewThreshold: parseFloat(process.env.COMPLIANCE_REVIEW_THRESHOLD || '1000000'),
    minAccountBalance: parseFloat(process.env.MIN_ACCOUNT_BALANCE || '250000'),
    maxDailyTransferLimit: parseFloat(process.env.MAX_DAILY_TRANSFER_LIMIT || '10000000'),
    rebalancingThreshold: parseFloat(process.env.REBALANCING_THRESHOLD || '0.05'),
    goalAlignmentMinScore: parseInt(process.env.GOAL_ALIGNMENT_MIN_SCORE || '6', 10),
  },

  // File upload configuration
  upload: {
    maxFileSize: parseInt(process.env.MAX_FILE_SIZE || '10485760', 10), // 10MB
    uploadPath: process.env.UPLOAD_PATH || 'uploads/',
    allowedFileTypes: (process.env.ALLOWED_FILE_TYPES || 'pdf,jpg,jpeg,png,doc,docx').split(','),
  },

  // Health check configuration
  healthCheck: {
    interval: parseInt(process.env.HEALTH_CHECK_INTERVAL || '30000', 10), // 30 seconds
  },

  // Monitoring configuration
  monitoring: {
    prometheusPort: parseInt(process.env.PROMETHEUS_PORT || '9090', 10),
  },
};