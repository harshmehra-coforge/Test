import express from 'express';
import { paymentService } from './PaymentService';
import { authenticateToken, requireRole, requireClientAccess } from '@/middleware/auth';
import { paymentValidations } from '@/middleware/validation';
import { paymentRateLimiter } from '@/middleware/rateLimiter';
import { auditLogger } from '@/middleware/requestLogger';
import { asyncHandler } from '@/middleware/errorHandler';
import { logger } from '@/utils/logger';
import { UserRole } from '@/types';

const router = express.Router();

/**
 * @swagger
 * /api/v1/payments:
 *   post:
 *     summary: Create a new payment transaction
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - accountId
 *               - type
 *               - amount
 *               - currency
 *             properties:
 *               accountId:
 *                 type: string
 *                 format: uuid
 *               type:
 *                 type: string
 *                 enum: [WIRE_TRANSFER, ACH, INTERNAL_TRANSFER, CHECK, BILL_PAY]
 *               amount:
 *                 type: string
 *                 description: Amount as decimal string
 *               currency:
 *                 type: string
 *                 enum: [USD, EUR, GBP, CHF]
 *               recipientInfo:
 *                 type: object
 *                 properties:
 *                   name:
 *                     type: string
 *                   accountNumber:
 *                     type: string
 *                   routingNumber:
 *                     type: string
 *                   bankName:
 *                     type: string
 *               memo:
 *                 type: string
 *               scheduledDate:
 *                 type: string
 *                 format: date-time
 *     responses:
 *       201:
 *         description: Payment created successfully
 *       400:
 *         description: Invalid request data
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       429:
 *         description: Rate limit exceeded
 */
router.post('/',
  authenticateToken,
  requireRole([UserRole.CLIENT, UserRole.ADVISOR, UserRole.OPERATIONS]),
  paymentRateLimiter,
  ...paymentValidations.createPayment,
  auditLogger('CREATE_PAYMENT', 'TRANSACTION'),
  asyncHandler(async (req, res) => {
    const paymentData = req.body;
    const initiatedBy = req.user!.id;

    const result = await paymentService.createPayment(paymentData, initiatedBy);

    res.status(201).json({
      success: true,
      data: result,
    });
  })
);

/**
 * @swagger
 * /api/v1/payments/{id}:
 *   get:
 *     summary: Get payment transaction by ID
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Payment details retrieved successfully
 *       404:
 *         description: Payment not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 */
router.get('/:id',
  authenticateToken,
  requireRole([UserRole.CLIENT, UserRole.ADVISOR, UserRole.COMPLIANCE_OFFICER, UserRole.OPERATIONS]),
  asyncHandler(async (req, res) => {
    const { id } = req.params;

    const transaction = await paymentService.getTransaction(id);

    if (!transaction) {
      return res.status(404).json({
        success: false,
        error: {
          code: 'TRANSACTION_NOT_FOUND',
          message: 'Transaction not found',
        },
      });
    }

    res.json({
      success: true,
      data: transaction,
    });
  })
);

/**
 * @swagger
 * /api/v1/payments/{id}/approve:
 *   put:
 *     summary: Approve or reject a payment transaction
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - approved
 *             properties:
 *               approved:
 *                 type: boolean
 *               notes:
 *                 type: string
 *               mfaCode:
 *                 type: string
 *     responses:
 *       200:
 *         description: Payment approval processed successfully
 *       400:
 *         description: Invalid request data
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       404:
 *         description: Payment not found
 */
router.put('/:id/approve',
  authenticateToken,
  requireRole([UserRole.ADVISOR, UserRole.COMPLIANCE_OFFICER, UserRole.OPERATIONS]),
  ...paymentValidations.approvePayment,
  auditLogger('APPROVE_PAYMENT', 'TRANSACTION'),
  asyncHandler(async (req, res) => {
    const { id } = req.params;
    const approvalData = req.body;
    const approvedBy = req.user!.id;

    await paymentService.approvePayment(id, approvalData, approvedBy);

    res.json({
      success: true,
      data: {
        message: `Payment ${approvalData.approved ? 'approved' : 'rejected'} successfully`,
      },
    });
  })
);

/**
 * @swagger
 * /api/v1/payments/{id}/cancel:
 *   put:
 *     summary: Cancel a pending payment transaction
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Payment cancelled successfully
 *       400:
 *         description: Cannot cancel payment in current status
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       404:
 *         description: Payment not found
 */
router.put('/:id/cancel',
  authenticateToken,
  requireRole([UserRole.CLIENT, UserRole.ADVISOR, UserRole.OPERATIONS]),
  auditLogger('CANCEL_PAYMENT', 'TRANSACTION'),
  asyncHandler(async (req, res) => {
    const { id } = req.params;
    const cancelledBy = req.user!.id;

    await paymentService.cancelTransaction(id, cancelledBy);

    res.json({
      success: true,
      data: {
        message: 'Payment cancelled successfully',
      },
    });
  })
);

/**
 * @swagger
 * /api/v1/payments/account/{accountId}:
 *   get:
 *     summary: Get payments for an account
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: accountId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *       - in: query
 *         name: status
 *         schema:
 *           type: string
 *       - in: query
 *         name: type
 *         schema:
 *           type: string
 *       - in: query
 *         name: startDate
 *         schema:
 *           type: string
 *           format: date-time
 *       - in: query
 *         name: endDate
 *         schema:
 *           type: string
 *           format: date-time
 *       - in: query
 *         name: page
 *         schema:
 *           type: integer
 *           minimum: 1
 *       - in: query
 *         name: limit
 *         schema:
 *           type: integer
 *           minimum: 1
 *           maximum: 100
 *     responses:
 *       200:
 *         description: Account payments retrieved successfully
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 */
router.get('/account/:accountId',
  authenticateToken,
  requireRole([UserRole.CLIENT, UserRole.ADVISOR, UserRole.COMPLIANCE_OFFICER, UserRole.OPERATIONS]),
  requireClientAccess,
  ...paymentValidations.getPayments,
  asyncHandler(async (req, res) => {
    const { accountId } = req.params;
    const filters = {
      status: req.query.status as any,
      type: req.query.type as any,
      startDate: req.query.startDate ? new Date(req.query.startDate as string) : undefined,
      endDate: req.query.endDate ? new Date(req.query.endDate as string) : undefined,
      page: req.query.page ? parseInt(req.query.page as string, 10) : 1,
      limit: req.query.limit ? parseInt(req.query.limit as string, 10) : 50,
    };

    const result = await paymentService.getAccountTransactions(accountId, filters);

    res.json({
      success: true,
      data: {
        transactions: result.transactions,
        pagination: {
          page: filters.page,
          limit: filters.limit,
          total: result.total,
          totalPages: Math.ceil(result.total / filters.limit),
          hasNext: filters.page * filters.limit < result.total,
          hasPrev: filters.page > 1,
        },
      },
    });
  })
);

/**
 * @swagger
 * /api/v1/payments/convert:
 *   post:
 *     summary: Convert currency amount
 *     tags: [Payments]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - amount
 *               - fromCurrency
 *               - toCurrency
 *             properties:
 *               amount:
 *                 type: string
 *               fromCurrency:
 *                 type: string
 *                 enum: [USD, EUR, GBP, CHF]
 *               toCurrency:
 *                 type: string
 *                 enum: [USD, EUR, GBP, CHF]
 *     responses:
 *       200:
 *         description: Currency conversion successful
 *       400:
 *         description: Invalid request data
 *       401:
 *         description: Unauthorized
 */
router.post('/convert',
  authenticateToken,
  requireRole([UserRole.CLIENT, UserRole.ADVISOR, UserRole.OPERATIONS]),
  asyncHandler(async (req, res) => {
    const { amount, fromCurrency, toCurrency } = req.body;

    const result = await paymentService.convertCurrency(
      new (require('decimal.js'))(amount),
      fromCurrency,
      toCurrency
    );

    res.json({
      success: true,
      data: {
        originalAmount: amount,
        fromCurrency,
        toCurrency,
        convertedAmount: result.convertedAmount.toString(),
        exchangeRate: result.exchangeRate.toString(),
        timestamp: new Date().toISOString(),
      },
    });
  })
);

export default router;