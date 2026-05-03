import { Request, Response, NextFunction } from 'express';
import { body, param, query, validationResult, ValidationChain } from 'express-validator';
import { Currency, TransactionType, UserRole, MFAMethod } from '@/types';
import { logger } from '@/utils/logger';

/**
 * Middleware to handle validation errors
 */
export const handleValidationErrors = (req: Request, res: Response, next: NextFunction): void => {
  const errors = validationResult(req);
  
  if (!errors.isEmpty()) {
    logger.warn('Validation errors:', {
      requestId: req.requestId,
      errors: errors.array(),
      path: req.path,
      method: req.method,
    });

    res.status(400).json({
      success: false,
      error: {
        code: 'VALIDATION_ERROR',
        message: 'Invalid request data',
        details: errors.array().map(error => ({
          field: error.type === 'field' ? error.path : 'unknown',
          message: error.msg,
          value: error.type === 'field' ? error.value : undefined,
        })),
      },
    });
    return;
  }

  next();
};

/**
 * Common validation rules
 */
export const commonValidations = {
  uuid: (field: string) => 
    body(field).isUUID().withMessage(`${field} must be a valid UUID`),
  
  email: (field: string = 'email') =>
    body(field).isEmail().normalizeEmail().withMessage('Must be a valid email address'),
  
  password: (field: string = 'password') =>
    body(field)
      .isLength({ min: 8 })
      .withMessage('Password must be at least 8 characters long')
      .matches(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)
      .withMessage('Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character'),
  
  phone: (field: string = 'phone') =>
    body(field)
      .matches(/^\+?[1-9]\d{1,14}$/)
      .withMessage('Must be a valid phone number'),
  
  currency: (field: string) =>
    body(field).isIn(Object.values(Currency)).withMessage('Must be a valid currency'),
  
  amount: (field: string) =>
    body(field)
      .isFloat({ min: 0.01 })
      .withMessage('Amount must be a positive number')
      .custom((value) => {
        // Check for reasonable decimal places (max 2 for currency)
        const decimalPlaces = (value.toString().split('.')[1] || '').length;
        if (decimalPlaces > 2) {
          throw new Error('Amount cannot have more than 2 decimal places');
        }
        return true;
      }),
  
  date: (field: string) =>
    body(field).isISO8601().withMessage('Must be a valid ISO 8601 date'),
  
  pagination: () => [
    query('page').optional().isInt({ min: 1 }).withMessage('Page must be a positive integer'),
    query('limit').optional().isInt({ min: 1, max: 100 }).withMessage('Limit must be between 1 and 100'),
  ],
};

/**
 * Authentication validation rules
 */
export const authValidations = {
  login: [
    commonValidations.email(),
    body('password').notEmpty().withMessage('Password is required'),
    body('mfaCode').optional().isLength({ min: 6, max: 8 }).withMessage('MFA code must be 6-8 characters'),
    handleValidationErrors,
  ],

  register: [
    commonValidations.email(),
    commonValidations.password(),
    body('firstName').trim().isLength({ min: 1, max: 50 }).withMessage('First name is required and must be less than 50 characters'),
    body('lastName').trim().isLength({ min: 1, max: 50 }).withMessage('Last name is required and must be less than 50 characters'),
    body('role').isIn(Object.values(UserRole)).withMessage('Must be a valid user role'),
    handleValidationErrors,
  ],

  refreshToken: [
    body('refreshToken').notEmpty().withMessage('Refresh token is required'),
    handleValidationErrors,
  ],

  mfaSetup: [
    body('method').isIn(Object.values(MFAMethod)).withMessage('Must be a valid MFA method'),
    body('phoneNumber').optional().custom((value, { req }) => {
      if (req.body.method === MFAMethod.SMS && !value) {
        throw new Error('Phone number is required for SMS MFA');
      }
      return true;
    }),
    handleValidationErrors,
  ],
};

/**
 * Payment validation rules
 */
export const paymentValidations = {
  createPayment: [
    commonValidations.uuid('accountId'),
    body('type').isIn(Object.values(TransactionType)).withMessage('Must be a valid transaction type'),
    commonValidations.amount('amount'),
    commonValidations.currency('currency'),
    body('recipientInfo').optional().isObject().withMessage('Recipient info must be an object'),
    body('recipientInfo.name').optional().trim().isLength({ min: 1, max: 100 }).withMessage('Recipient name must be 1-100 characters'),
    body('recipientInfo.accountNumber').optional().trim().isLength({ min: 1, max: 50 }).withMessage('Account number must be 1-50 characters'),
    body('memo').optional().trim().isLength({ max: 500 }).withMessage('Memo must be less than 500 characters'),
    body('scheduledDate').optional().isISO8601().withMessage('Scheduled date must be a valid ISO 8601 date'),
    handleValidationErrors,
  ],

  approvePayment: [
    param('id').isUUID().withMessage('Transaction ID must be a valid UUID'),
    body('approved').isBoolean().withMessage('Approved must be a boolean'),
    body('notes').optional().trim().isLength({ max: 1000 }).withMessage('Notes must be less than 1000 characters'),
    body('mfaCode').optional().isLength({ min: 6, max: 8 }).withMessage('MFA code must be 6-8 characters'),
    handleValidationErrors,
  ],

  getPayments: [
    query('accountId').optional().isUUID().withMessage('Account ID must be a valid UUID'),
    query('status').optional().isString().withMessage('Status must be a string'),
    query('type').optional().isIn(Object.values(TransactionType)).withMessage('Must be a valid transaction type'),
    query('startDate').optional().isISO8601().withMessage('Start date must be a valid ISO 8601 date'),
    query('endDate').optional().isISO8601().withMessage('End date must be a valid ISO 8601 date'),
    ...commonValidations.pagination(),
    handleValidationErrors,
  ],
};

/**
 * Client validation rules
 */
export const clientValidations = {
  createClient: [
    body('personalInfo.firstName').trim().isLength({ min: 1, max: 50 }).withMessage('First name is required and must be less than 50 characters'),
    body('personalInfo.lastName').trim().isLength({ min: 1, max: 50 }).withMessage('Last name is required and must be less than 50 characters'),
    body('personalInfo.email').isEmail().normalizeEmail().withMessage('Must be a valid email address'),
    commonValidations.phone('personalInfo.phone'),
    body('personalInfo.dateOfBirth').isISO8601().withMessage('Date of birth must be a valid ISO 8601 date'),
    body('personalInfo.ssn').matches(/^\d{3}-?\d{2}-?\d{4}$/).withMessage('SSN must be in format XXX-XX-XXXX'),
    body('address.street').trim().isLength({ min: 1, max: 100 }).withMessage('Street address is required'),
    body('address.city').trim().isLength({ min: 1, max: 50 }).withMessage('City is required'),
    body('address.state').trim().isLength({ min: 2, max: 2 }).withMessage('State must be 2 characters'),
    body('address.zipCode').matches(/^\d{5}(-\d{4})?$/).withMessage('ZIP code must be in format XXXXX or XXXXX-XXXX'),
    body('initialDeposit').isFloat({ min: 250000 }).withMessage('Initial deposit must be at least $250,000'),
    handleValidationErrors,
  ],

  updateClient: [
    param('id').isUUID().withMessage('Client ID must be a valid UUID'),
    body('personalInfo.firstName').optional().trim().isLength({ min: 1, max: 50 }),
    body('personalInfo.lastName').optional().trim().isLength({ min: 1, max: 50 }),
    body('personalInfo.email').optional().isEmail().normalizeEmail(),
    body('personalInfo.phone').optional().matches(/^\+?[1-9]\d{1,14}$/),
    handleValidationErrors,
  ],
};

/**
 * Portfolio validation rules
 */
export const portfolioValidations = {
  getPerformance: [
    param('portfolioId').isUUID().withMessage('Portfolio ID must be a valid UUID'),
    query('period').isIn(['1D', '1W', '1M', '3M', '6M', '1Y', 'YTD', 'INCEPTION']).withMessage('Must be a valid period'),
    query('benchmark').optional().isString().withMessage('Benchmark must be a string'),
    handleValidationErrors,
  ],

  rebalance: [
    param('portfolioId').isUUID().withMessage('Portfolio ID must be a valid UUID'),
    body('targetAllocation.equity').isFloat({ min: 0, max: 100 }).withMessage('Equity allocation must be between 0 and 100'),
    body('targetAllocation.fixedIncome').isFloat({ min: 0, max: 100 }).withMessage('Fixed income allocation must be between 0 and 100'),
    body('targetAllocation.cash').isFloat({ min: 0, max: 100 }).withMessage('Cash allocation must be between 0 and 100'),
    body('dryRun').optional().isBoolean().withMessage('Dry run must be a boolean'),
    body().custom((value) => {
      const total = parseFloat(value.targetAllocation.equity) + 
                   parseFloat(value.targetAllocation.fixedIncome) + 
                   parseFloat(value.targetAllocation.cash) +
                   (value.targetAllocation.alternatives ? parseFloat(value.targetAllocation.alternatives) : 0);
      
      if (Math.abs(total - 100) > 0.01) {
        throw new Error('Target allocations must sum to 100%');
      }
      return true;
    }),
    handleValidationErrors,
  ],
};

/**
 * Event validation rules
 */
export const validateEventPayload = [
  body('topic').notEmpty().withMessage('Topic is required'),
  body('eventType').notEmpty().withMessage('Event type is required'),
  body('payload').isObject().withMessage('Payload must be an object'),
  body('correlationId').optional().isUUID().withMessage('Correlation ID must be a valid UUID'),
  body('metadata').optional().isObject().withMessage('Metadata must be an object'),
  handleValidationErrors,
];

/**
 * Compliance validation rules
 */
export const complianceValidations = {
  screenTransaction: [
    body('clientId').optional().isUUID().withMessage('Client ID must be a valid UUID'),
    body('transactionId').optional().isUUID().withMessage('Transaction ID must be a valid UUID'),
    commonValidations.amount('amount').optional(),
    commonValidations.currency('currency').optional(),
    body('recipientName').optional().trim().isLength({ min: 1, max: 100 }),
    body('recipientAccount').optional().trim().isLength({ min: 1, max: 50 }),
    handleValidationErrors,
  ],

  getAlerts: [
    query('status').optional().isString(),
    query('type').optional().isString(),
    query('severity').optional().isIn(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL']),
    query('assignedTo').optional().isUUID(),
    query('startDate').optional().isISO8601(),
    query('endDate').optional().isISO8601(),
    ...commonValidations.pagination(),
    handleValidationErrors,
  ],
};