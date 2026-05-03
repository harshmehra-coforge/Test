// Request/Response DTOs for API endpoints

import { 
  Currency, 
  TransactionType, 
  RecipientInfo, 
  UserRole, 
  MFAMethod,
  NotificationChannel,
  NotificationPriority,
  ComplianceAlertType,
  AlertSeverity,
  PerformancePeriod,
  RiskTolerance,
  AccountType
} from './index';

// Authentication DTOs
export interface LoginRequest {
  email: string;
  password: string;
  mfaCode?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: UserRole;
    mfaEnabled: boolean;
  };
  expiresIn: number;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface MFASetupRequest {
  method: MFAMethod;
}

export interface MFASetupResponse {
  secret?: string;
  qrCode?: string;
  backupCodes: string[];
}

export interface MFAVerifyRequest {
  code: string;
  method: MFAMethod;
}

// Payment DTOs
export interface PaymentRequest {
  accountId: string;
  type: TransactionType;
  amount: string; // Decimal as string
  currency: Currency;
  recipientInfo?: RecipientInfo;
  memo?: string;
  scheduledDate?: string; // ISO date string
}

export interface PaymentResponse {
  transactionId: string;
  status: string;
  confirmationNumber?: string;
  estimatedSettlement?: string;
  fees?: Array<{
    type: string;
    amount: string;
    currency: Currency;
  }>;
}

export interface PaymentApprovalRequest {
  transactionId: string;
  approved: boolean;
  notes?: string;
  mfaCode?: string;
}

export interface PaymentListQuery {
  accountId?: string;
  status?: string;
  type?: TransactionType;
  startDate?: string;
  endDate?: string;
  page?: number;
  limit?: number;
}

// Portfolio DTOs
export interface PortfolioSummaryResponse {
  portfolioId: string;
  accountId: string;
  totalValue: string;
  dayChange: string;
  dayChangePercent: string;
  allocation: {
    equity: string;
    fixedIncome: string;
    cash: string;
    alternatives?: string;
  };
  goalAlignmentScore: number;
  lastUpdated: string;
}

export interface PerformanceRequest {
  portfolioId: string;
  period: PerformancePeriod;
  benchmark?: string;
}

export interface PerformanceResponse {
  period: PerformancePeriod;
  totalReturn: string;
  annualizedReturn: string;
  benchmarkReturn?: string;
  alpha?: string;
  beta?: string;
  sharpeRatio?: string;
  volatility: string;
  maxDrawdown: string;
  chartData: Array<{
    date: string;
    portfolioValue: string;
    benchmarkValue?: string;
  }>;
}

export interface RebalanceRequest {
  portfolioId: string;
  targetAllocation: {
    equity: string;
    fixedIncome: string;
    cash: string;
    alternatives?: string;
  };
  dryRun?: boolean;
}

export interface RebalanceResponse {
  recommendedTrades: Array<{
    symbol: string;
    action: 'BUY' | 'SELL';
    quantity: string;
    estimatedValue: string;
  }>;
  estimatedCost: string;
  projectedAllocation: {
    equity: string;
    fixedIncome: string;
    cash: string;
    alternatives?: string;
  };
  impactAnalysis: {
    taxImplications: string;
    riskChange: string;
    expectedReturn: string;
  };
}

export interface HoldingsResponse {
  holdings: Array<{
    symbol: string;
    name: string;
    quantity: string;
    unitPrice: string;
    marketValue: string;
    costBasis: string;
    unrealizedGainLoss: string;
    unrealizedGainLossPercent: string;
    allocation: string;
    lastUpdated: string;
  }>;
  totalValue: string;
  totalGainLoss: string;
  totalGainLossPercent: string;
}

// Client DTOs
export interface ClientOnboardingRequest {
  personalInfo: {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    dateOfBirth: string;
    ssn: string;
  };
  address: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
  };
  accountType: AccountType;
  initialDeposit: string;
  riskAssessment: RiskAssessmentRequest;
}

export interface RiskAssessmentRequest {
  investmentExperience: string;
  riskTolerance: RiskTolerance;
  timeHorizon: number;
  liquidityNeeds: string;
  investmentObjectives: string[];
  annualIncome: string;
  netWorth: string;
}

export interface ClientProfileResponse {
  clientId: string;
  personalInfo: {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    dateOfBirth: string;
  };
  address: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
  };
  riskProfile: {
    riskScore: number;
    riskTolerance: RiskTolerance;
    lastAssessment: string;
  };
  kycStatus: string;
  accountStatus: string;
  accounts: Array<{
    accountId: string;
    accountNumber: string;
    accountType: AccountType;
    totalValue: string;
    status: string;
  }>;
}

// Compliance DTOs
export interface ComplianceScreeningRequest {
  clientId?: string;
  transactionId?: string;
  amount?: string;
  currency?: Currency;
  recipientName?: string;
  recipientAccount?: string;
}

export interface ComplianceScreeningResponse {
  screeningId: string;
  status: 'CLEAR' | 'FLAGGED' | 'BLOCKED';
  alerts: Array<{
    type: ComplianceAlertType;
    severity: AlertSeverity;
    description: string;
    requiresReview: boolean;
  }>;
  recommendations: string[];
}

export interface ComplianceAlertQuery {
  status?: string;
  type?: ComplianceAlertType;
  severity?: AlertSeverity;
  assignedTo?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  limit?: number;
}

export interface ComplianceReportRequest {
  reportType: 'AML' | 'OFAC' | 'SAR' | 'CTR' | 'SUSPICIOUS_ACTIVITY';
  startDate: string;
  endDate: string;
  format: 'PDF' | 'CSV' | 'JSON';
  includeDetails?: boolean;
}

// Notification DTOs
export interface NotificationRequest {
  recipientId: string;
  type: string;
  channel: NotificationChannel;
  subject: string;
  message: string;
  priority?: NotificationPriority;
  scheduledAt?: string;
  metadata?: Record<string, any>;
}

export interface NotificationPreferencesRequest {
  clientId: string;
  preferences: {
    email: boolean;
    sms: boolean;
    push: boolean;
    inApp: boolean;
  };
  types: Record<string, {
    enabled: boolean;
    channels: NotificationChannel[];
  }>;
}

export interface NotificationListQuery {
  recipientId?: string;
  type?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  limit?: number;
}

// Audit DTOs
export interface AuditLogQuery {
  userId?: string;
  clientId?: string;
  serviceName?: string;
  action?: string;
  resourceType?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  limit?: number;
}

export interface AuditReportRequest {
  startDate: string;
  endDate: string;
  services?: string[];
  actions?: string[];
  format: 'PDF' | 'CSV' | 'JSON';
}

// Market Data DTOs
export interface MarketDataRequest {
  symbols: string[];
  fields?: string[];
}

export interface MarketDataResponse {
  data: Array<{
    symbol: string;
    price: string;
    change: string;
    changePercent: string;
    volume: number;
    timestamp: string;
  }>;
  timestamp: string;
}

// Goal DTOs
export interface GoalRequest {
  name: string;
  description: string;
  targetAmount: string;
  targetDate: string;
  priority: string;
  riskTolerance: RiskTolerance;
}

export interface GoalResponse {
  goalId: string;
  name: string;
  description: string;
  targetAmount: string;
  currentAmount: string;
  progress: string; // percentage
  targetDate: string;
  projectedCompletion: string;
  onTrack: boolean;
  recommendedMonthlyContribution: string;
}

// Investment Advisory DTOs
export interface InvestmentRecommendationRequest {
  clientId: string;
  portfolioId: string;
  amount?: string;
  timeHorizon?: number;
  riskTolerance?: RiskTolerance;
}

export interface InvestmentRecommendationResponse {
  recommendations: Array<{
    symbol: string;
    name: string;
    recommendedAllocation: string;
    rationale: string;
    riskRating: string;
    expectedReturn: string;
    fees: string;
  }>;
  portfolioImpact: {
    newAllocation: {
      equity: string;
      fixedIncome: string;
      cash: string;
    };
    riskChange: string;
    expectedReturnChange: string;
  };
  implementationPlan: Array<{
    step: number;
    action: string;
    timeline: string;
    cost: string;
  }>;
}

// Error DTOs
export interface ValidationError {
  field: string;
  message: string;
  code: string;
  value?: any;
}

export interface ErrorResponse {
  error: {
    code: string;
    message: string;
    details?: ValidationError[];
    timestamp: string;
    requestId: string;
  };
}

// Health Check DTOs
export interface HealthCheckResponse {
  status: 'healthy' | 'unhealthy' | 'degraded';
  timestamp: string;
  uptime: number;
  version: string;
  service: string;
  dependencies: Array<{
    name: string;
    status: 'healthy' | 'unhealthy';
    responseTime?: number;
    error?: string;
  }>;
}

// Pagination DTOs
export interface PaginationQuery {
  page?: number;
  limit?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginationMeta {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}