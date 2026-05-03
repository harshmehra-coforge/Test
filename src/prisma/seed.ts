import { PrismaClient } from '@prisma/client';
import bcrypt from 'bcryptjs';
import { v4 as uuidv4 } from 'uuid';

const prisma = new PrismaClient();

async function main(): Promise<void> {
  console.log('Starting database seeding...');

  // Create admin user
  const adminPassword = await bcrypt.hash('Admin123!@#', 12);
  const adminUser = await prisma.user.upsert({
    where: { email: 'admin@wealthmanagement.com' },
    update: {},
    create: {
      id: uuidv4(),
      email: 'admin@wealthmanagement.com',
      password: adminPassword,
      firstName: 'System',
      lastName: 'Administrator',
      role: 'ADMIN',
      isActive: true,
      mfaEnabled: false,
    },
  });

  // Create compliance officer
  const compliancePassword = await bcrypt.hash('Compliance123!@#', 12);
  const complianceUser = await prisma.user.upsert({
    where: { email: 'compliance@wealthmanagement.com' },
    update: {},
    create: {
      id: uuidv4(),
      email: 'compliance@wealthmanagement.com',
      password: compliancePassword,
      firstName: 'Jennifer',
      lastName: 'Rodriguez',
      role: 'COMPLIANCE_OFFICER',
      isActive: true,
      mfaEnabled: true,
    },
  });

  // Create financial advisor
  const advisorPassword = await bcrypt.hash('Advisor123!@#', 12);
  const advisorUser = await prisma.user.upsert({
    where: { email: 'advisor@wealthmanagement.com' },
    update: {},
    create: {
      id: uuidv4(),
      email: 'advisor@wealthmanagement.com',
      password: advisorPassword,
      firstName: 'Michael',
      lastName: 'Chen',
      role: 'ADVISOR',
      isActive: true,
      mfaEnabled: true,
    },
  });

  // Create sample client
  const sampleClient = await prisma.client.upsert({
    where: { email: 'sarah.thompson@email.com' },
    update: {},
    create: {
      id: uuidv4(),
      firstName: 'Sarah',
      lastName: 'Thompson',
      email: 'sarah.thompson@email.com',
      phone: '+1-555-0123',
      dateOfBirth: new Date('1978-05-15'),
      ssnEncrypted: 'encrypted_ssn_123456789', // In real implementation, this would be properly encrypted
      street: '123 Wealth Avenue',
      city: 'New York',
      state: 'NY',
      zipCode: '10001',
      country: 'US',
      riskScore: 7,
      riskTolerance: 'MODERATE',
      investmentExperience: 'Experienced investor with 15+ years',
      timeHorizon: 20,
      liquidityNeeds: 'Moderate - need access to 10% within 6 months',
      investmentObjectives: ['Growth', 'Income', 'Tax Efficiency'],
      kycStatus: 'VERIFIED',
      accountStatus: 'ACTIVE',
    },
  });

  // Create account for sample client
  const sampleAccount = await prisma.account.create({
    data: {
      id: uuidv4(),
      clientId: sampleClient.id,
      accountNumber: 'WM-2024-001',
      accountType: 'INDIVIDUAL',
      baseCurrency: 'USD',
      totalValue: 2500000.00,
      availableCash: 150000.00,
      status: 'ACTIVE',
      openDate: new Date('2024-01-15'),
    },
  });

  // Create portfolio for the account
  const samplePortfolio = await prisma.portfolio.create({
    data: {
      id: uuidv4(),
      accountId: sampleAccount.id,
      totalValue: 2500000.00,
      equityAllocation: 65.00,
      fixedIncomeAllocation: 25.00,
      cashAllocation: 10.00,
      targetEquity: 60.00,
      targetFixedIncome: 30.00,
      targetCash: 10.00,
      goalAlignmentScore: 8,
      var95: -0.0250,
      beta: 0.85,
      sharpeRatio: 1.25,
      volatility: 0.12,
      maxDrawdown: -0.08,
      lastRebalanceDate: new Date('2024-01-15'),
    },
  });

  // Create sample holdings
  const holdings = [
    {
      securityId: 'SPY',
      securityType: 'ETF',
      symbol: 'SPY',
      name: 'SPDR S&P 500 ETF Trust',
      quantity: 2000,
      unitPrice: 450.00,
      marketValue: 900000.00,
      costBasis: 850000.00,
    },
    {
      securityId: 'BND',
      securityType: 'ETF',
      symbol: 'BND',
      name: 'Vanguard Total Bond Market ETF',
      quantity: 6000,
      unitPrice: 75.00,
      marketValue: 450000.00,
      costBasis: 480000.00,
    },
    {
      securityId: 'VTI',
      securityType: 'ETF',
      symbol: 'VTI',
      name: 'Vanguard Total Stock Market ETF',
      quantity: 3000,
      unitPrice: 220.00,
      marketValue: 660000.00,
      costBasis: 600000.00,
    },
    {
      securityId: 'VXUS',
      securityType: 'ETF',
      symbol: 'VXUS',
      name: 'Vanguard Total International Stock ETF',
      quantity: 4000,
      unitPrice: 55.00,
      marketValue: 220000.00,
      costBasis: 240000.00,
    },
  ];

  for (const holding of holdings) {
    await prisma.holding.create({
      data: {
        id: uuidv4(),
        portfolioId: samplePortfolio.id,
        ...holding,
        unrealizedGainLoss: holding.marketValue - holding.costBasis,
      },
    });
  }

  // Create sample investment goal
  await prisma.investmentGoal.create({
    data: {
      id: uuidv4(),
      clientId: sampleClient.id,
      name: 'Retirement Fund',
      description: 'Build retirement nest egg for comfortable retirement at age 65',
      targetAmount: 5000000.00,
      currentAmount: 2500000.00,
      targetDate: new Date('2043-05-15'),
      priority: 'HIGH',
      status: 'ACTIVE',
      riskTolerance: 'MODERATE',
    },
  });

  // Create sample transactions
  const transactions = [
    {
      type: 'WIRE_TRANSFER',
      amount: 100000.00,
      currency: 'USD',
      status: 'SETTLED',
      confirmationNumber: 'WT-2024-001',
      settlementDate: new Date('2024-01-20'),
    },
    {
      type: 'INVESTMENT_BUY',
      amount: 50000.00,
      currency: 'USD',
      status: 'SETTLED',
      confirmationNumber: 'IB-2024-001',
      settlementDate: new Date('2024-01-22'),
    },
    {
      type: 'DIVIDEND',
      amount: 2500.00,
      currency: 'USD',
      status: 'SETTLED',
      confirmationNumber: 'DIV-2024-001',
      settlementDate: new Date('2024-01-25'),
    },
  ];

  for (const transaction of transactions) {
    await prisma.transaction.create({
      data: {
        id: uuidv4(),
        accountId: sampleAccount.id,
        initiatedBy: sampleClient.id,
        ...transaction,
      },
    });
  }

  // Create compliance rules
  const complianceRules = [
    {
      name: 'Large Transaction Threshold',
      description: 'Flag transactions over $1,000,000 for compliance review',
      ruleType: 'THRESHOLD',
      conditions: {
        amount: { gte: 1000000 },
        currency: 'USD',
      },
      actions: {
        createAlert: true,
        requireReview: true,
        severity: 'HIGH',
      },
    },
    {
      name: 'OFAC Sanctions Screening',
      description: 'Screen all transactions against OFAC sanctions list',
      ruleType: 'OFAC',
      conditions: {
        transactionTypes: ['WIRE_TRANSFER', 'ACH'],
      },
      actions: {
        screenRecipient: true,
        blockIfMatch: true,
        severity: 'CRITICAL',
      },
    },
    {
      name: 'AML Suspicious Activity',
      description: 'Detect patterns indicative of money laundering',
      ruleType: 'AML',
      conditions: {
        dailyTransactionCount: { gte: 10 },
        dailyTransactionAmount: { gte: 50000 },
      },
      actions: {
        createAlert: true,
        requireInvestigation: true,
        severity: 'MEDIUM',
      },
    },
  ];

  for (const rule of complianceRules) {
    await prisma.complianceRule.create({
      data: {
        id: uuidv4(),
        ...rule,
      },
    });
  }

  // Create notification templates
  const notificationTemplates = [
    {
      name: 'transaction_confirmation',
      type: 'TRANSACTION_CONFIRMATION',
      channel: 'EMAIL',
      subject: 'Transaction Confirmation - {{transactionType}}',
      template: `
        Dear {{clientName}},
        
        Your {{transactionType}} transaction has been processed successfully.
        
        Transaction Details:
        - Amount: {{amount}} {{currency}}
        - Confirmation Number: {{confirmationNumber}}
        - Settlement Date: {{settlementDate}}
        
        Thank you for choosing our wealth management services.
        
        Best regards,
        Wealth Management Team
      `,
    },
    {
      name: 'compliance_alert',
      type: 'COMPLIANCE_ALERT',
      channel: 'EMAIL',
      subject: 'Compliance Alert - {{alertType}}',
      template: `
        A compliance alert has been generated:
        
        Alert Type: {{alertType}}
        Severity: {{severity}}
        Description: {{description}}
        
        Please review and take appropriate action.
      `,
    },
    {
      name: 'portfolio_update',
      type: 'PORTFOLIO_UPDATE',
      channel: 'EMAIL',
      subject: 'Portfolio Performance Update',
      template: `
        Dear {{clientName}},
        
        Your portfolio performance summary:
        
        - Current Value: {{currentValue}}
        - Day Change: {{dayChange}} ({{dayChangePercent}}%)
        - Goal Alignment Score: {{goalAlignmentScore}}/10
        
        View detailed performance in your client portal.
        
        Best regards,
        Your Advisory Team
      `,
    },
  ];

  for (const template of notificationTemplates) {
    await prisma.notificationTemplate.create({
      data: {
        id: uuidv4(),
        ...template,
      },
    });
  }

  // Create system configuration
  const systemConfigs = [
    {
      key: 'AML_THRESHOLD_AMOUNT',
      value: '10000',
      description: 'Threshold amount for AML monitoring',
      category: 'compliance',
    },
    {
      key: 'COMPLIANCE_REVIEW_THRESHOLD',
      value: '1000000',
      description: 'Amount threshold requiring compliance review',
      category: 'compliance',
    },
    {
      key: 'REBALANCING_THRESHOLD',
      value: '0.05',
      description: 'Portfolio drift threshold for rebalancing alerts',
      category: 'portfolio',
    },
    {
      key: 'MIN_ACCOUNT_BALANCE',
      value: '250000',
      description: 'Minimum account opening balance',
      category: 'account',
    },
    {
      key: 'MAX_DAILY_TRANSFER_LIMIT',
      value: '10000000',
      description: 'Maximum daily transfer limit per client',
      category: 'transaction',
    },
  ];

  for (const config of systemConfigs) {
    await prisma.systemConfig.create({
      data: {
        id: uuidv4(),
        ...config,
      },
    });
  }

  // Create sample market data
  const marketDataEntries = [
    { symbol: 'SPY', price: 450.00, change: 2.50, changePercent: 0.0056, volume: 50000000 },
    { symbol: 'BND', price: 75.00, change: -0.25, changePercent: -0.0033, volume: 5000000 },
    { symbol: 'VTI', price: 220.00, change: 1.80, changePercent: 0.0082, volume: 25000000 },
    { symbol: 'VXUS', price: 55.00, change: 0.75, changePercent: 0.0138, volume: 15000000 },
  ];

  for (const marketData of marketDataEntries) {
    await prisma.marketData.create({
      data: {
        id: uuidv4(),
        ...marketData,
      },
    });
  }

  // Create exchange rates
  const exchangeRates = [
    { fromCurrency: 'USD', toCurrency: 'EUR', rate: 0.85, source: 'ECB' },
    { fromCurrency: 'USD', toCurrency: 'GBP', rate: 0.78, source: 'BOE' },
    { fromCurrency: 'USD', toCurrency: 'CHF', rate: 0.92, source: 'SNB' },
    { fromCurrency: 'EUR', toCurrency: 'USD', rate: 1.18, source: 'ECB' },
    { fromCurrency: 'GBP', toCurrency: 'USD', rate: 1.28, source: 'BOE' },
    { fromCurrency: 'CHF', toCurrency: 'USD', rate: 1.09, source: 'SNB' },
  ];

  for (const rate of exchangeRates) {
    await prisma.exchangeRate.create({
      data: {
        id: uuidv4(),
        ...rate,
      },
    });
  }

  console.log('Database seeding completed successfully!');
  console.log('Created:');
  console.log('- 3 users (admin, compliance officer, advisor)');
  console.log('- 1 sample client with account and portfolio');
  console.log('- 4 portfolio holdings');
  console.log('- 1 investment goal');
  console.log('- 3 sample transactions');
  console.log('- 3 compliance rules');
  console.log('- 3 notification templates');
  console.log('- 5 system configuration entries');
  console.log('- 4 market data entries');
  console.log('- 6 exchange rate entries');
}

main()
  .catch(e => {
    console.error('Error during seeding:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });