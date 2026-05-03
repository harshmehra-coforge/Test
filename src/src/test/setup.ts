import { Request, Response, NextFunction } from 'express';
import { v4 as uuidv4 } from 'uuid';
import { logger } from '@/utils/logger';

/**
 * Test setup file for Jest
 */

// Mock environment variables for testing
process.env.NODE_ENV = 'test';
process.env.DATABASE_URL = 'postgresql://test:test@localhost:5432/test_db';
process.env.REDIS_URL = 'redis://localhost:6379/1';
process.env.JWT_SECRET = 'test-jwt-secret';
process.env.ENCRYPTION_KEY = 'test-encryption-key-32-characters';

// Mock external services
jest.mock('@/utils/logger', () => ({
  logger: {
    info: jest.fn(),
    warn: jest.fn(),
    error: jest.fn(),
    debug: jest.fn(),
  },
  logAudit: jest.fn(),
  logSecurity: jest.fn(),
  logError: jest.fn(),
}));

// Mock Prisma Client
jest.mock('@prisma/client', () => ({
  PrismaClient: jest.fn().mockImplementation(() => ({
    $connect: jest.fn(),
    $disconnect: jest.fn(),
    $queryRaw: jest.fn(),
    user: {
      findUnique: jest.fn(),
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
    },
    client: {
      findUnique: jest.fn(),
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
    },
    account: {
      findUnique: jest.fn(),
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
    },
    transaction: {
      findUnique: jest.fn(),
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
      count: jest.fn(),
    },
    portfolio: {
      findUnique: jest.fn(),
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
    },
    holding: {
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      delete: jest.fn(),
    },
    complianceAlert: {
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      count: jest.fn(),
    },
    notification: {
      findMany: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      count: jest.fn(),
    },
    auditLog: {
      findMany: jest.fn(),
      create: jest.fn(),
      count: jest.fn(),
    },
  })),
}));

// Mock Redis
jest.mock('redis', () => ({
  createClient: jest.fn(() => ({
    connect: jest.fn(),
    disconnect: jest.fn(),
    get: jest.fn(),
    set: jest.fn(),
    del: jest.fn(),
    exists: jest.fn(),
    expire: jest.fn(),
  })),
}));

// Mock Kafka
jest.mock('kafkajs', () => ({
  Kafka: jest.fn(() => ({
    producer: jest.fn(() => ({
      connect: jest.fn(),
      disconnect: jest.fn(),
      send: jest.fn(),
    })),
    consumer: jest.fn(() => ({
      connect: jest.fn(),
      disconnect: jest.fn(),
      subscribe: jest.fn(),
      run: jest.fn(),
    })),
    admin: jest.fn(() => ({
      connect: jest.fn(),
      disconnect: jest.fn(),
      createTopics: jest.fn(),
      listTopics: jest.fn(() => []),
      fetchTopicMetadata: jest.fn(() => ({ topics: [] })),
    })),
  })),
}));

// Mock external API calls
jest.mock('axios', () => ({
  get: jest.fn(),
  post: jest.fn(),
  put: jest.fn(),
  delete: jest.fn(),
  create: jest.fn(() => ({
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  })),
}));

// Global test utilities
global.generateTestUser = () => ({
  id: uuidv4(),
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'User',
  role: 'CLIENT',
  isActive: true,
  mfaEnabled: false,
  createdAt: new Date(),
  updatedAt: new Date(),
});

global.generateTestClient = () => ({
  id: uuidv4(),
  firstName: 'John',
  lastName: 'Doe',
  email: 'john.doe@example.com',
  phone: '+1234567890',
  dateOfBirth: new Date('1980-01-01'),
  ssnEncrypted: 'encrypted-ssn',
  street: '123 Main St',
  city: 'New York',
  state: 'NY',
  zipCode: '10001',
  country: 'US',
  riskScore: 7,
  riskTolerance: 'MODERATE',
  kycStatus: 'VERIFIED',
  accountStatus: 'ACTIVE',
  createdAt: new Date(),
  updatedAt: new Date(),
});

global.generateTestAccount = () => ({
  id: uuidv4(),
  clientId: uuidv4(),
  accountNumber: 'ACC-123456',
  accountType: 'INDIVIDUAL',
  baseCurrency: 'USD',
  totalValue: 1000000,
  availableCash: 50000,
  status: 'ACTIVE',
  openDate: new Date(),
  createdAt: new Date(),
  updatedAt: new Date(),
});

global.generateTestTransaction = () => ({
  id: uuidv4(),
  accountId: uuidv4(),
  type: 'WIRE_TRANSFER',
  amount: 10000,
  currency: 'USD',
  status: 'PENDING',
  initiatedBy: uuidv4(),
  confirmationNumber: 'WT-2024-12345',
  createdAt: new Date(),
  updatedAt: new Date(),
});

// Setup and teardown
beforeAll(async () => {
  // Global setup
});

afterAll(async () => {
  // Global teardown
});

beforeEach(() => {
  // Clear all mocks before each test
  jest.clearAllMocks();
});

afterEach(() => {
  // Cleanup after each test
});