import { PrismaClient } from '@prisma/client';
import { Decimal } from 'decimal.js';
import { v4 as uuidv4 } from 'uuid';
import { 
  Transaction, 
  TransactionType, 
  TransactionStatus, 
  Currency, 
  PaymentRequest, 
  PaymentResponse,
  PaymentApprovalRequest,
  RecipientInfo 
} from '@/types';
import { logger, logAudit } from '@/utils/logger';
import { eventBus } from '../event-bus/EventBus';
import { config } from '@/config';
import { CustomError, ComplianceError } from '@/middleware/errorHandler';

export class PaymentService {
  private prisma: PrismaClient;

  constructor() {
    this.prisma = new PrismaClient();
  }

  /**
   * Create a new payment transaction
   */
  async createPayment(paymentData: PaymentRequest, initiatedBy: string): Promise<PaymentResponse> {
    try {
      // Validate account exists and has sufficient funds
      const account = await this.prisma.account.findUnique({
        where: { id: paymentData.accountId },
        include: { client: true },
      });

      if (!account) {
        throw new CustomError('Account not found', 404, 'ACCOUNT_NOT_FOUND');
      }

      if (account.status !== 'ACTIVE') {
        throw new CustomError('Account is not active', 400, 'ACCOUNT_INACTIVE');
      }

      // Check available balance for outgoing transactions
      const amount = new Decimal(paymentData.amount);
      if (this.isOutgoingTransaction(paymentData.type) && account.availableCash.lt(amount)) {
        throw new CustomError('Insufficient funds', 400, 'INSUFFICIENT_FUNDS');
      }

      // Validate transaction limits
      await this.validateTransactionLimits(paymentData.accountId, amount, paymentData.type);

      // Create transaction record
      const transaction = await this.prisma.transaction.create({
        data: {
          id: uuidv4(),
          accountId: paymentData.accountId,
          type: paymentData.type,
          amount: amount,
          currency: paymentData.currency,
          status: this.getInitialStatus(paymentData.type, amount),
          initiatedBy,
          recipientInfo: paymentData.recipientInfo as any,
          complianceNotes: '',
          createdAt: new Date(),
          updatedAt: new Date(),
        },
      });

      // Generate confirmation number
      const confirmationNumber = this.generateConfirmationNumber(transaction.type, transaction.id);
      
      await this.prisma.transaction.update({
        where: { id: transaction.id },
        data: { confirmationNumber },
      });

      // Publish payment initiated event
      const paymentEvent = eventBus.createEvent({
        eventType: 'PAYMENT_INITIATED',
        payload: {
          transactionId: transaction.id,
          accountId: transaction.accountId,
          amount: transaction.amount,
          currency: transaction.currency,
          type: transaction.type,
          recipientInfo: paymentData.recipientInfo,
        },
      });

      await eventBus.publishEvent(config.kafka.topics.payments, paymentEvent);

      // Log audit event
      logAudit('PAYMENT_CREATED', initiatedBy, 'TRANSACTION', transaction.id, {
        amount: amount.toString(),
        currency: paymentData.currency,
        type: paymentData.type,
      });

      return {
        transactionId: transaction.id,
        status: transaction.status,
        confirmationNumber,
        estimatedSettlement: this.calculateEstimatedSettlement(paymentData.type),
        fees: await this.calculateFees(paymentData.type, amount, paymentData.currency),
      };

    } catch (error) {
      logger.error('Error creating payment:', error);
      throw error;
    }
  }

  /**
   * Approve a pending payment
   */
  async approvePayment(
    transactionId: string, 
    approvalData: PaymentApprovalRequest, 
    approvedBy: string
  ): Promise<void> {
    try {
      const transaction = await this.prisma.transaction.findUnique({
        where: { id: transactionId },
        include: { account: { include: { client: true } } },
      });

      if (!transaction) {
        throw new CustomError('Transaction not found', 404, 'TRANSACTION_NOT_FOUND');
      }

      if (!this.canApproveTransaction(transaction.status)) {
        throw new CustomError('Transaction cannot be approved in current status', 400, 'INVALID_STATUS');
      }

      // Update transaction status
      const newStatus = approvalData.approved ? 'APPROVED' : 'REJECTED';
      
      await this.prisma.transaction.update({
        where: { id: transactionId },
        data: {
          status: newStatus,
          approvedBy,
          complianceNotes: approvalData.notes || '',
          updatedAt: new Date(),
        },
      });

      if (approvalData.approved) {
        // Process the approved payment
        await this.processApprovedPayment(transaction);
      }

      // Publish payment approval event
      const approvalEvent = eventBus.createEvent({
        eventType: approvalData.approved ? 'PAYMENT_APPROVED' : 'PAYMENT_REJECTED',
        payload: {
          transactionId,
          approvedBy,
          notes: approvalData.notes,
        },
      });

      await eventBus.publishEvent(config.kafka.topics.payments, approvalEvent);

      // Log audit event
      logAudit(
        approvalData.approved ? 'PAYMENT_APPROVED' : 'PAYMENT_REJECTED',
        approvedBy,
        'TRANSACTION',
        transactionId,
        { notes: approvalData.notes }
      );

    } catch (error) {
      logger.error('Error approving payment:', error);
      throw error;
    }
  }

  /**
   * Get transaction by ID
   */
  async getTransaction(transactionId: string): Promise<Transaction | null> {
    try {
      const transaction = await this.prisma.transaction.findUnique({
        where: { id: transactionId },
        include: { account: { include: { client: true } } },
      });

      return transaction as Transaction | null;
    } catch (error) {
      logger.error('Error getting transaction:', error);
      throw error;
    }
  }

  /**
   * Get transactions for an account
   */
  async getAccountTransactions(
    accountId: string,
    filters: {
      status?: TransactionStatus;
      type?: TransactionType;
      startDate?: Date;
      endDate?: Date;
      page?: number;
      limit?: number;
    } = {}
  ): Promise<{ transactions: Transaction[]; total: number }> {
    try {
      const { page = 1, limit = 50, ...whereFilters } = filters;
      const skip = (page - 1) * limit;

      const where: any = { accountId };

      if (whereFilters.status) {
        where.status = whereFilters.status;
      }

      if (whereFilters.type) {
        where.type = whereFilters.type;
      }

      if (whereFilters.startDate || whereFilters.endDate) {
        where.createdAt = {};
        if (whereFilters.startDate) {
          where.createdAt.gte = whereFilters.startDate;
        }
        if (whereFilters.endDate) {
          where.createdAt.lte = whereFilters.endDate;
        }
      }

      const [transactions, total] = await Promise.all([
        this.prisma.transaction.findMany({
          where,
          orderBy: { createdAt: 'desc' },
          skip,
          take: limit,
          include: { account: { include: { client: true } } },
        }),
        this.prisma.transaction.count({ where }),
      ]);

      return { transactions: transactions as Transaction[], total };
    } catch (error) {
      logger.error('Error getting account transactions:', error);
      throw error;
    }
  }

  /**
   * Cancel a pending transaction
   */
  async cancelTransaction(transactionId: string, cancelledBy: string): Promise<void> {
    try {
      const transaction = await this.prisma.transaction.findUnique({
        where: { id: transactionId },
      });

      if (!transaction) {
        throw new CustomError('Transaction not found', 404, 'TRANSACTION_NOT_FOUND');
      }

      if (!this.canCancelTransaction(transaction.status)) {
        throw new CustomError('Transaction cannot be cancelled in current status', 400, 'INVALID_STATUS');
      }

      await this.prisma.transaction.update({
        where: { id: transactionId },
        data: {
          status: 'CANCELLED',
          updatedAt: new Date(),
        },
      });

      // Publish cancellation event
      const cancellationEvent = eventBus.createEvent({
        eventType: 'PAYMENT_CANCELLED',
        payload: {
          transactionId,
          cancelledBy,
        },
      });

      await eventBus.publishEvent(config.kafka.topics.payments, cancellationEvent);

      // Log audit event
      logAudit('PAYMENT_CANCELLED', cancelledBy, 'TRANSACTION', transactionId);

    } catch (error) {
      logger.error('Error cancelling transaction:', error);
      throw error;
    }
  }

  /**
   * Process exchange rate conversion
   */
  async convertCurrency(
    amount: Decimal,
    fromCurrency: Currency,
    toCurrency: Currency
  ): Promise<{ convertedAmount: Decimal; exchangeRate: Decimal }> {
    try {
      if (fromCurrency === toCurrency) {
        return { convertedAmount: amount, exchangeRate: new Decimal(1) };
      }

      // Get latest exchange rate
      const exchangeRate = await this.prisma.exchangeRate.findFirst({
        where: {
          fromCurrency,
          toCurrency,
        },
        orderBy: { timestamp: 'desc' },
      });

      if (!exchangeRate) {
        throw new CustomError('Exchange rate not available', 400, 'EXCHANGE_RATE_UNAVAILABLE');
      }

      const convertedAmount = amount.mul(exchangeRate.rate);

      return { convertedAmount, exchangeRate: exchangeRate.rate };
    } catch (error) {
      logger.error('Error converting currency:', error);
      throw error;
    }
  }

  /**
   * Private helper methods
   */
  private isOutgoingTransaction(type: TransactionType): boolean {
    return [
      TransactionType.WIRE_TRANSFER,
      TransactionType.ACH,
      TransactionType.CHECK,
      TransactionType.BILL_PAY,
      TransactionType.INVESTMENT_BUY,
    ].includes(type);
  }

  private getInitialStatus(type: TransactionType, amount: Decimal): TransactionStatus {
    // Large transactions require compliance review
    if (amount.gte(config.businessRules.complianceReviewThreshold)) {
      return TransactionStatus.PENDING_COMPLIANCE;
    }

    // Medium transactions require approval
    if (amount.gte(100000)) {
      return TransactionStatus.PENDING_APPROVAL;
    }

    // Small transactions can be processed immediately
    return TransactionStatus.APPROVED;
  }

  private generateConfirmationNumber(type: TransactionType, transactionId: string): string {
    const typePrefix = {
      [TransactionType.WIRE_TRANSFER]: 'WT',
      [TransactionType.ACH]: 'ACH',
      [TransactionType.INTERNAL_TRANSFER]: 'IT',
      [TransactionType.CHECK]: 'CHK',
      [TransactionType.BILL_PAY]: 'BP',
      [TransactionType.INVESTMENT_BUY]: 'IB',
      [TransactionType.INVESTMENT_SELL]: 'IS',
      [TransactionType.DIVIDEND]: 'DIV',
      [TransactionType.INTEREST]: 'INT',
      [TransactionType.FEE]: 'FEE',
    };

    const year = new Date().getFullYear();
    const shortId = transactionId.substring(0, 8).toUpperCase();
    
    return `${typePrefix[type]}-${year}-${shortId}`;
  }

  private calculateEstimatedSettlement(type: TransactionType): string {
    const now = new Date();
    let settlementDate = new Date(now);

    switch (type) {
      case TransactionType.WIRE_TRANSFER:
        settlementDate.setHours(now.getHours() + 2); // Same day if before 2 PM
        break;
      case TransactionType.ACH:
        settlementDate.setDate(now.getDate() + 1); // Next business day
        break;
      case TransactionType.INTERNAL_TRANSFER:
        settlementDate = now; // Immediate
        break;
      default:
        settlementDate.setDate(now.getDate() + 1);
    }

    return settlementDate.toISOString();
  }

  private async calculateFees(
    type: TransactionType,
    amount: Decimal,
    currency: Currency
  ): Promise<Array<{ type: string; amount: string; currency: Currency }>> {
    const fees: Array<{ type: string; amount: string; currency: Currency }> = [];

    switch (type) {
      case TransactionType.WIRE_TRANSFER:
        fees.push({
          type: 'Wire Transfer Fee',
          amount: '25.00',
          currency,
        });
        break;
      case TransactionType.ACH:
        if (amount.gt(1000)) {
          fees.push({
            type: 'ACH Fee',
            amount: '3.00',
            currency,
          });
        }
        break;
      // Add other fee calculations as needed
    }

    return fees;
  }

  private async validateTransactionLimits(
    accountId: string,
    amount: Decimal,
    type: TransactionType
  ): Promise<void> {
    // Check daily transaction limits
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    const dailyTransactions = await this.prisma.transaction.findMany({
      where: {
        accountId,
        createdAt: {
          gte: today,
          lt: tomorrow,
        },
        status: {
          in: ['APPROVED', 'PROCESSING', 'SETTLED'],
        },
      },
    });

    const dailyTotal = dailyTransactions.reduce(
      (sum, tx) => sum.add(tx.amount),
      new Decimal(0)
    );

    if (dailyTotal.add(amount).gt(config.businessRules.maxDailyTransferLimit)) {
      throw new CustomError(
        'Daily transaction limit exceeded',
        400,
        'DAILY_LIMIT_EXCEEDED'
      );
    }

    // Check transaction type specific limits
    switch (type) {
      case TransactionType.WIRE_TRANSFER:
        if (amount.gt(10000000)) {
          throw new CustomError('Wire transfer limit exceeded', 400, 'AMOUNT_LIMIT_EXCEEDED');
        }
        break;
      case TransactionType.ACH:
        if (amount.gt(1000000)) {
          throw new CustomError('ACH transfer limit exceeded', 400, 'AMOUNT_LIMIT_EXCEEDED');
        }
        break;
    }
  }

  private canApproveTransaction(status: TransactionStatus): boolean {
    return [
      TransactionStatus.PENDING_APPROVAL,
      TransactionStatus.PENDING_COMPLIANCE,
    ].includes(status);
  }

  private canCancelTransaction(status: TransactionStatus): boolean {
    return [
      TransactionStatus.DRAFT,
      TransactionStatus.PENDING,
      TransactionStatus.PENDING_APPROVAL,
      TransactionStatus.PENDING_COMPLIANCE,
    ].includes(status);
  }

  private async processApprovedPayment(transaction: any): Promise<void> {
    try {
      // Update account balance for outgoing transactions
      if (this.isOutgoingTransaction(transaction.type)) {
        await this.prisma.account.update({
          where: { id: transaction.accountId },
          data: {
            availableCash: {
              decrement: transaction.amount,
            },
          },
        });
      }

      // Update transaction status to processing
      await this.prisma.transaction.update({
        where: { id: transaction.id },
        data: {
          status: TransactionStatus.PROCESSING,
          settlementDate: new Date(this.calculateEstimatedSettlement(transaction.type)),
        },
      });

      // In a real implementation, this would integrate with external payment processors
      // For now, we'll simulate processing and mark as settled after a delay
      setTimeout(async () => {
        await this.settleTransaction(transaction.id);
      }, 5000); // 5 second delay for demo purposes

    } catch (error) {
      logger.error('Error processing approved payment:', error);
      
      // Mark transaction as failed
      await this.prisma.transaction.update({
        where: { id: transaction.id },
        data: {
          status: TransactionStatus.FAILED,
          failureReason: error instanceof Error ? error.message : 'Processing failed',
        },
      });

      throw error;
    }
  }

  private async settleTransaction(transactionId: string): Promise<void> {
    try {
      const transaction = await this.prisma.transaction.update({
        where: { id: transactionId },
        data: {
          status: TransactionStatus.SETTLED,
          settlementDate: new Date(),
        },
      });

      // Publish settlement event
      const settlementEvent = eventBus.createEvent({
        eventType: 'PAYMENT_SETTLED',
        payload: {
          transactionId,
          settlementDate: new Date(),
          finalAmount: transaction.amount,
        },
      });

      await eventBus.publishEvent(config.kafka.topics.payments, settlementEvent);

      logger.info(`Transaction ${transactionId} settled successfully`);

    } catch (error) {
      logger.error('Error settling transaction:', error);
    }
  }
}

export const paymentService = new PaymentService();