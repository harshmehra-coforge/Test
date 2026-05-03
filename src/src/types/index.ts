import { Decimal } from 'decimal.js';

// Base Types
export type UUID = string;
export type Currency = 'USD' | 'EUR' | 'GBP' | 'CHF';
export type DecimalValue = Decimal;

// User and Authentication Types
export interface User {
  id: UUID;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  lastLogin?: Date;
  mfaEnabled: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export enum UserRole {
  CLIENT = 'CLIENT',
  ADVISOR = 'ADVISOR',
  COMPLIANCE_OFFICER = 'COMPLIANCE_OFFICER',
  OPERATIONS = 'OPERATIONS',
  ADMIN = 'ADMIN',
}

export interface JWTPayload {
  sub: string; // User ID
  iat: number; // Issued at
  exp: number; // Expiration
  aud: string; // Audience
  iss: string; // Issuer
  roles: UserRole[];
  permissions: string[];
  clientId?: string;
  sessionId: string;
}

export interface MFAChallenge {
  challengeId: string;
  userId: string;
  method: MFAMethod;
  expiresAt: Date;
  attempts: number;
  maxAttempts: number;
}

export enum MFAMethod {
  SMS = 'SMS',
  EMAIL = 'EMAIL',
  TOTP = 'TOTP',
  PUSH = 'PUSH',
}

// Client Types
export interface Client {
  clientId: UUID;
  personalInfo: PersonalInfo;
  address: Address;
  riskProfile: RiskProfile;
  kycStatus: KYCStatus;
  accountStatus: AccountStatus;
  createdAt: Date;
  updatedAt: Date;
}

export interface PersonalInfo {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: Date;
  ssn: string; // Encrypted
}

export interface Address {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

export interface RiskProfile {
  riskScore: number; // 1-10
  riskTolerance: RiskTolerance;
  investmentExperience: string;
  timeHorizon: number; // years
  liquidityNeeds: string;
  investmentObjectives: string[];
}

export enum RiskTolerance {
  CONSERVATIVE = 'CONSERVATIVE',
  MODERATE = 'MODERATE',
  AGGRESSIVE = 'AGGRESSIVE',
}

export enum KYCStatus {
  PENDING = 'PENDING',
  VERIFIED = 'VERIFIED',
  EXPIRED = 'EXPIRED',
  REJECTED = 'REJECTED',
}

export enum AccountStatus {
  ACTIVE = 'ACTIVE',
  SUSPENDED = 'SUSPENDED',
  CLOSED = 'CLOSED',
  PENDING = 'PENDING',
}

// Account Types
export interface Account {
  accountId: UUID;
  clientId: UUID;
  accountNumber: string;
  accountType: AccountType;
  baseCurrency: Currency;
  totalValue: DecimalValue;
  availableCash: DecimalValue;
  status: AccountStatus;
  openDate: Date;
  closeDate?: Date;
}

export enum AccountType {
  INDIVIDUAL = 'INDIVIDUAL',
  JOINT = 'JOINT',
  TRUST = 'TRUST',
  CORPORATE = 'CORPORATE',
}

// Transaction Types
export interface Transaction {
  transactionId: UUID;
  accountId: UUID;
  type: TransactionType;
  amount: DecimalValue;
  currency: Currency;
  status: TransactionStatus;
  initiatedBy: UUID;
  approvedBy?: UUID;
  settlementDate?: Date;
  confirmationNumber?: string;
  metadata: TransactionMetadata;
  createdAt: Date;
  updatedAt: Date;
}

export enum TransactionType {
  WIRE_TRANSFER = 'WIRE_TRANSFER',
  ACH = 'ACH',
  INTERNAL_TRANSFER = 'INTERNAL_TRANSFER',
  CHECK = 'CHECK',
  BILL_PAY = 'BILL_PAY',
  INVESTMENT_BUY = 'INVESTMENT_BUY',
  INVESTMENT_SELL = 'INVESTMENT_SELL',
  DIVIDEND = 'DIVIDEND',
  INTEREST = 'INTEREST',
  FEE = 'FEE',
}

export enum TransactionStatus {
  DRAFT = 'DRAFT',
  PENDING = 'PENDING',
  PENDING_APPROVAL = 'PENDING_APPROVAL',
  PENDING_COMPLIANCE = 'PENDING_COMPLIANCE',
  APPROVED = 'APPROVED',
  PROCESSING = 'PROCESSING',
  SETTLED = 'SETTLED',
  FAILED = 'FAILED',
  REJECTED = 'REJECTED',
  CANCELLED = 'CANCELLED',
}

export interface TransactionMetadata {
  recipientInfo?: RecipientInfo;
  complianceNotes?: string;
  failureReason?: string;
  exchangeRate?: DecimalValue;
  fees?: TransactionFee[];
  externalTransactionId?: string;
}

export interface RecipientInfo {
  name: string;
  accountNumber: string;
  routingNumber: string;
  bankName: string;
  address?: Address;
}

export interface TransactionFee {
  type: string;
  amount: DecimalValue;
  currency: Currency;
  description: string;
}

// Portfolio Types
export interface Portfolio {
  portfolioId: UUID;
  accountId: UUID;
  totalValue: DecimalValue;
  allocation: AllocationBreakdown;
  goalAlignmentScore: number; // 1-10
  riskMetrics: RiskMetrics;
  lastRebalanceDate: Date;
  targetAllocation: AllocationTarget;
  holdings: Holding[];
}

export interface AllocationBreakdown {
  equity: DecimalValue; // percentage
  fixedIncome: DecimalValue; // percentage
  cash: DecimalValue; // percentage
  alternatives?: DecimalValue; // percentage
}

export interface AllocationTarget {
  equity: DecimalValue;
  fixedIncome: DecimalValue;
  cash: DecimalValue;
  alternatives?: DecimalValue;
  rebalanceThreshold: DecimalValue; // percentage
}

export interface RiskMetrics {
  var95: DecimalValue; // Value at Risk 95% confidence
  beta: DecimalValue;
  sharpeRatio: DecimalValue;
  volatility: DecimalValue;
  maxDrawdown: DecimalValue;
}

export interface Holding {
  holdingId: UUID;
  portfolioId: UUID;
  securityId: string;
  securityType: SecurityType;
  symbol: string;
  name: string;
  quantity: DecimalValue;
  unitPrice: DecimalValue;
  marketValue: DecimalValue;
  costBasis: DecimalValue;
  unrealizedGainLoss: DecimalValue;
  lastUpdated: Date;
}

export enum SecurityType {
  EQUITY = 'EQUITY',
  BOND = 'BOND',
  MUTUAL_FUND = 'MUTUAL_FUND',
  ETF = 'ETF',
  CASH = 'CASH',
  ALTERNATIVE = 'ALTERNATIVE',
}

// Performance Types
export interface PerformanceData {
  portfolioId: UUID;
  period: PerformancePeriod;
  totalReturn: DecimalValue;
  annualizedReturn: DecimalValue;
  benchmarkReturn: DecimalValue;
  alpha: DecimalValue;
  beta: DecimalValue;
  sharpeRatio: DecimalValue;
  volatility: DecimalValue;
  maxDrawdown: DecimalValue;
  startDate: Date;
  endDate: Date;
}

export enum PerformancePeriod {
  ONE_DAY = '1D',
  ONE_WEEK = '1W',
  ONE_MONTH = '1M',
  THREE_MONTHS = '3M',
  SIX_MONTHS = '6M',
  ONE_YEAR = '1Y',
  YEAR_TO_DATE = 'YTD',
  INCEPTION = 'INCEPTION',
}

// Compliance Types
export interface ComplianceAlert {
  alertId: UUID;
  clientId: UUID;
  transactionId?: UUID;
  alertType: ComplianceAlertType;
  severity: AlertSeverity;
  description: string;
  status: AlertStatus;
  assignedTo?: UUID;
  requiresReview: boolean;
  dueDate?: Date;
  resolution?: string;
  createdAt: Date;
  updatedAt: Date;
}

export enum ComplianceAlertType {
  AML = 'AML',
  OFAC = 'OFAC',
  THRESHOLD = 'THRESHOLD',
  CONCENTRATION = 'CONCENTRATION',
  SUITABILITY = 'SUITABILITY',
  KYC = 'KYC',
}

export enum AlertSeverity {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL',
}

export enum AlertStatus {
  OPEN = 'OPEN',
  IN_REVIEW = 'IN_REVIEW',
  RESOLVED = 'RESOLVED',
  FALSE_POSITIVE = 'FALSE_POSITIVE',
  ESCALATED = 'ESCALATED',
}

// Notification Types
export interface Notification {
  notificationId: UUID;
  recipientId: UUID;
  type: NotificationType;
  channel: NotificationChannel;
  subject: string;
  message: string;
  status: NotificationStatus;
  priority: NotificationPriority;
  scheduledAt?: Date;
  sentAt?: Date;
  readAt?: Date;
  metadata?: Record<string, any>;
  createdAt: Date;
}

export enum NotificationType {
  TRANSACTION_CONFIRMATION = 'TRANSACTION_CONFIRMATION',
  PAYMENT_APPROVAL = 'PAYMENT_APPROVAL',
  COMPLIANCE_ALERT = 'COMPLIANCE_ALERT',
  PORTFOLIO_UPDATE = 'PORTFOLIO_UPDATE',
  SECURITY_ALERT = 'SECURITY_ALERT',
  SYSTEM_MAINTENANCE = 'SYSTEM_MAINTENANCE',
  WELCOME = 'WELCOME',
  PASSWORD_RESET = 'PASSWORD_RESET',
  MFA_CODE = 'MFA_CODE',
}

export enum NotificationChannel {
  EMAIL = 'EMAIL',
  SMS = 'SMS',
  PUSH = 'PUSH',
  IN_APP = 'IN_APP',
}

export enum NotificationStatus {
  PENDING = 'PENDING',
  SENT = 'SENT',
  DELIVERED = 'DELIVERED',
  READ = 'READ',
  FAILED = 'FAILED',
}

export enum NotificationPriority {
  LOW = 'LOW',
  NORMAL = 'NORMAL',
  HIGH = 'HIGH',
  URGENT = 'URGENT',
}

// Audit Types
export interface AuditLog {
  auditId: UUID;
  userId?: UUID;
  clientId?: UUID;
  serviceName: string;
  action: string;
  resourceType: string;
  resourceId?: string;
  beforeState?: Record<string, any>;
  afterState?: Record<string, any>;
  ipAddress?: string;
  userAgent?: string;
  sessionId?: string;
  success: boolean;
  errorMessage?: string;
  metadata?: Record<string, any>;
  createdAt: Date;
}

// Event Types
export interface BaseEvent {
  eventId: UUID;
  eventType: string;
  timestamp: Date;
  version: string;
  correlationId?: string;
  causationId?: string;
  metadata?: Record<string, any>;
}

export interface PaymentInitiatedEvent extends BaseEvent {
  eventType: 'PAYMENT_INITIATED';
  payload: {
    transactionId: UUID;
    accountId: UUID;
    amount: DecimalValue;
    currency: Currency;
    type: TransactionType;
    recipientInfo?: RecipientInfo;
  };
}

export interface PaymentCompletedEvent extends BaseEvent {
  eventType: 'PAYMENT_COMPLETED';
  payload: {
    transactionId: UUID;
    confirmationNumber: string;
    settlementDate: Date;
    finalAmount: DecimalValue;
  };
}

export interface PortfolioUpdatedEvent extends BaseEvent {
  eventType: 'PORTFOLIO_UPDATED';
  payload: {
    portfolioId: UUID;
    accountId: UUID;
    previousValue: DecimalValue;
    currentValue: DecimalValue;
    allocation: AllocationBreakdown;
  };
}

export interface ComplianceAlertEvent extends BaseEvent {
  eventType: 'COMPLIANCE_ALERT';
  payload: {
    alertId: UUID;
    clientId: UUID;
    alertType: ComplianceAlertType;
    severity: AlertSeverity;
    description: string;
    requiresReview: boolean;
  };
}

// API Response Types
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  error?: ApiError;
  metadata?: {
    timestamp: Date;
    requestId: string;
    version: string;
  };
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, any>;
  stack?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
    hasNext: boolean;
    hasPrev: boolean;
  };
}

// Configuration Types
export interface ServiceConfig {
  name: string;
  port: number;
  version: string;
  environment: string;
  database: DatabaseConfig;
  redis: RedisConfig;
  kafka: KafkaConfig;
  security: SecurityConfig;
  logging: LoggingConfig;
}

export interface DatabaseConfig {
  url: string;
  poolSize: number;
  timeout: number;
  ssl: boolean;
}

export interface RedisConfig {
  url: string;
  password?: string;
  db: number;
  ttl: {
    session: number;
    cache: number;
    marketData: number;
  };
}

export interface KafkaConfig {
  brokers: string[];
  clientId: string;
  groupId: string;
  topics: {
    events: string;
    payments: string;
    portfolio: string;
    compliance: string;
    notifications: string;
    audit: string;
  };
}

export interface SecurityConfig {
  jwt: {
    secret: string;
    expiresIn: string;
    refreshSecret: string;
    refreshExpiresIn: string;
  };
  encryption: {
    key: string;
    algorithm: string;
  };
  mfa: {
    issuer: string;
    window: number;
  };
  rateLimit: {
    windowMs: number;
    maxRequests: number;
  };
}

export interface LoggingConfig {
  level: string;
  file: string;
  maxSize: string;
  maxFiles: string;
}

// Health Check Types
export interface HealthCheck {
  status: 'healthy' | 'unhealthy' | 'degraded';
  timestamp: Date;
  uptime: number;
  version: string;
  dependencies: HealthCheckDependency[];
}

export interface HealthCheckDependency {
  name: string;
  status: 'healthy' | 'unhealthy';
  responseTime?: number;
  error?: string;
}

// Market Data Types
export interface MarketData {
  symbol: string;
  price: DecimalValue;
  change: DecimalValue;
  changePercent: DecimalValue;
  volume: number;
  marketCap?: DecimalValue;
  timestamp: Date;
}

export interface ExchangeRate {
  fromCurrency: Currency;
  toCurrency: Currency;
  rate: DecimalValue;
  timestamp: Date;
  source: string;
}

// Goal Types
export interface InvestmentGoal {
  goalId: UUID;
  clientId: UUID;
  name: string;
  description: string;
  targetAmount: DecimalValue;
  currentAmount: DecimalValue;
  targetDate: Date;
  priority: GoalPriority;
  status: GoalStatus;
  riskTolerance: RiskTolerance;
  createdAt: Date;
  updatedAt: Date;
}

export enum GoalPriority {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL',
}

export enum GoalStatus {
  ACTIVE = 'ACTIVE',
  PAUSED = 'PAUSED',
  COMPLETED = 'COMPLETED',
  CANCELLED = 'CANCELLED',
}