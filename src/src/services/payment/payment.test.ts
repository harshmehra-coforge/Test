import request from 'supertest';
import express from 'express';
import { paymentService } from '../services/payment/PaymentService';
import paymentRoutes from '../services/payment/routes';
import { jwtService } from '../utils/jwt';

// Mock the payment service
jest.mock('../services/payment/PaymentService');

describe('Payment Service API', () => {
  let app: express.Application;
  let mockToken: string;

  beforeAll(() => {
    app = express();
    app.use(express.json());
    app.use('/api/v1/payments', paymentRoutes);

    // Generate mock JWT token
    mockToken = jwtService.generateAccessToken({
      sub: 'test-user-id',
      roles: ['CLIENT'],
      permissions: ['payment:create', 'payment:read'],
      sessionId: 'test-session-id',
    });
  });

  describe('POST /api/v1/payments', () => {
    it('should create a payment successfully', async () => {
      const paymentData = {
        accountId: 'test-account-id',
        type: 'WIRE_TRANSFER',
        amount: '10000.00',
        currency: 'USD',
        recipientInfo: {
          name: 'John Doe',
          accountNumber: '123456789',
          routingNumber: '021000021',
          bankName: 'Chase Bank',
        },
      };

      const mockResponse = {
        transactionId: 'test-transaction-id',
        status: 'PENDING',
        confirmationNumber: 'WT-2024-12345',
        estimatedSettlement: new Date().toISOString(),
        fees: [],
      };

      (paymentService.createPayment as jest.Mock).mockResolvedValue(mockResponse);

      const response = await request(app)
        .post('/api/v1/payments')
        .set('Authorization', `Bearer ${mockToken}`)
        .send(paymentData)
        .expect(201);

      expect(response.body.success).toBe(true);
      expect(response.body.data).toEqual(mockResponse);
      expect(paymentService.createPayment).toHaveBeenCalledWith(
        paymentData,
        'test-user-id'
      );
    });

    it('should return 400 for invalid payment data', async () => {
      const invalidPaymentData = {
        accountId: 'invalid-uuid',
        type: 'INVALID_TYPE',
        amount: 'invalid-amount',
        currency: 'INVALID_CURRENCY',
      };

      const response = await request(app)
        .post('/api/v1/payments')
        .set('Authorization', `Bearer ${mockToken}`)
        .send(invalidPaymentData)
        .expect(400);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('VALIDATION_ERROR');
    });

    it('should return 401 for missing authentication', async () => {
      const paymentData = {
        accountId: 'test-account-id',
        type: 'WIRE_TRANSFER',
        amount: '10000.00',
        currency: 'USD',
      };

      const response = await request(app)
        .post('/api/v1/payments')
        .send(paymentData)
        .expect(401);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('MISSING_TOKEN');
    });
  });

  describe('GET /api/v1/payments/:id', () => {
    it('should get payment by ID successfully', async () => {
      const mockTransaction = {
        id: 'test-transaction-id',
        accountId: 'test-account-id',
        type: 'WIRE_TRANSFER',
        amount: 10000,
        currency: 'USD',
        status: 'SETTLED',
        confirmationNumber: 'WT-2024-12345',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      (paymentService.getTransaction as jest.Mock).mockResolvedValue(mockTransaction);

      const response = await request(app)
        .get('/api/v1/payments/test-transaction-id')
        .set('Authorization', `Bearer ${mockToken}`)
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(response.body.data).toEqual(mockTransaction);
    });

    it('should return 404 for non-existent payment', async () => {
      (paymentService.getTransaction as jest.Mock).mockResolvedValue(null);

      const response = await request(app)
        .get('/api/v1/payments/non-existent-id')
        .set('Authorization', `Bearer ${mockToken}`)
        .expect(404);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('TRANSACTION_NOT_FOUND');
    });
  });

  describe('PUT /api/v1/payments/:id/approve', () => {
    it('should approve payment successfully', async () => {
      const approvalData = {
        approved: true,
        notes: 'Approved after review',
      };

      (paymentService.approvePayment as jest.Mock).mockResolvedValue(undefined);

      // Generate advisor token
      const advisorToken = jwtService.generateAccessToken({
        sub: 'advisor-user-id',
        roles: ['ADVISOR'],
        permissions: ['payment:approve'],
        sessionId: 'advisor-session-id',
      });

      const response = await request(app)
        .put('/api/v1/payments/test-transaction-id/approve')
        .set('Authorization', `Bearer ${advisorToken}`)
        .send(approvalData)
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(paymentService.approvePayment).toHaveBeenCalledWith(
        'test-transaction-id',
        approvalData,
        'advisor-user-id'
      );
    });

    it('should return 403 for insufficient permissions', async () => {
      const approvalData = {
        approved: true,
        notes: 'Approved after review',
      };

      const response = await request(app)
        .put('/api/v1/payments/test-transaction-id/approve')
        .set('Authorization', `Bearer ${mockToken}`) // CLIENT role, not ADVISOR
        .send(approvalData)
        .expect(403);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('INSUFFICIENT_PERMISSIONS');
    });
  });

  describe('PUT /api/v1/payments/:id/cancel', () => {
    it('should cancel payment successfully', async () => {
      (paymentService.cancelTransaction as jest.Mock).mockResolvedValue(undefined);

      const response = await request(app)
        .put('/api/v1/payments/test-transaction-id/cancel')
        .set('Authorization', `Bearer ${mockToken}`)
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(paymentService.cancelTransaction).toHaveBeenCalledWith(
        'test-transaction-id',
        'test-user-id'
      );
    });
  });

  describe('GET /api/v1/payments/account/:accountId', () => {
    it('should get account payments successfully', async () => {
      const mockResult = {
        transactions: [
          {
            id: 'transaction-1',
            type: 'WIRE_TRANSFER',
            amount: 10000,
            status: 'SETTLED',
          },
          {
            id: 'transaction-2',
            type: 'ACH',
            amount: 5000,
            status: 'PENDING',
          },
        ],
        total: 2,
      };

      (paymentService.getAccountTransactions as jest.Mock).mockResolvedValue(mockResult);

      const response = await request(app)
        .get('/api/v1/payments/account/test-account-id')
        .set('Authorization', `Bearer ${mockToken}`)
        .query({ page: '1', limit: '10' })
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(response.body.data.transactions).toEqual(mockResult.transactions);
      expect(response.body.data.pagination.total).toBe(2);
    });
  });

  describe('POST /api/v1/payments/convert', () => {
    it('should convert currency successfully', async () => {
      const conversionData = {
        amount: '1000.00',
        fromCurrency: 'USD',
        toCurrency: 'EUR',
      };

      const mockResult = {
        convertedAmount: new (require('decimal.js'))('850.00'),
        exchangeRate: new (require('decimal.js'))('0.85'),
      };

      (paymentService.convertCurrency as jest.Mock).mockResolvedValue(mockResult);

      const response = await request(app)
        .post('/api/v1/payments/convert')
        .set('Authorization', `Bearer ${mockToken}`)
        .send(conversionData)
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(response.body.data.convertedAmount).toBe('850.00');
      expect(response.body.data.exchangeRate).toBe('0.85');
    });
  });
});