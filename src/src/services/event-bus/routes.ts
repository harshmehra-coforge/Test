import express from 'express';
import { eventBus } from './EventBus';
import { logger } from '@/utils/logger';
import { BaseEvent } from '@/types';
import { validateEventPayload } from '@/middleware/validation';
import { authenticateToken, requireRole } from '@/middleware/auth';

const router = express.Router();

/**
 * @swagger
 * /api/v1/events/publish:
 *   post:
 *     summary: Publish an event to the event bus
 *     tags: [Events]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               topic:
 *                 type: string
 *                 description: The topic to publish to
 *               eventType:
 *                 type: string
 *                 description: Type of event
 *               payload:
 *                 type: object
 *                 description: Event payload
 *               correlationId:
 *                 type: string
 *                 description: Correlation ID for tracing
 *               metadata:
 *                 type: object
 *                 description: Additional metadata
 *     responses:
 *       201:
 *         description: Event published successfully
 *       400:
 *         description: Invalid event payload
 *       401:
 *         description: Unauthorized
 *       500:
 *         description: Internal server error
 */
router.post('/publish', 
  authenticateToken,
  requireRole(['ADMIN', 'OPERATIONS']),
  validateEventPayload,
  async (req, res) => {
    try {
      const { topic, eventType, payload, correlationId, metadata } = req.body;

      const event = eventBus.createEvent({
        eventType,
        payload,
        correlationId,
        metadata,
      });

      await eventBus.publishEvent(topic, event);

      res.status(201).json({
        success: true,
        data: {
          eventId: event.eventId,
          timestamp: event.timestamp,
        },
      });
    } catch (error) {
      logger.error('Error publishing event:', error);
      res.status(500).json({
        success: false,
        error: {
          code: 'EVENT_PUBLISH_ERROR',
          message: 'Failed to publish event',
        },
      });
    }
  }
);

/**
 * @swagger
 * /api/v1/events/health:
 *   get:
 *     summary: Get event bus health status
 *     tags: [Events]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Health status retrieved successfully
 *       401:
 *         description: Unauthorized
 *       500:
 *         description: Internal server error
 */
router.get('/health',
  authenticateToken,
  requireRole(['ADMIN', 'OPERATIONS']),
  async (req, res) => {
    try {
      const health = await eventBus.healthCheck();
      
      res.json({
        success: true,
        data: health,
      });
    } catch (error) {
      logger.error('Error checking event bus health:', error);
      res.status(500).json({
        success: false,
        error: {
          code: 'HEALTH_CHECK_ERROR',
          message: 'Failed to check event bus health',
        },
      });
    }
  }
);

/**
 * @swagger
 * /api/v1/events/topics:
 *   post:
 *     summary: Create new topics
 *     tags: [Events]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               topics:
 *                 type: array
 *                 items:
 *                   type: string
 *                 description: List of topic names to create
 *     responses:
 *       201:
 *         description: Topics created successfully
 *       400:
 *         description: Invalid request
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       500:
 *         description: Internal server error
 */
router.post('/topics',
  authenticateToken,
  requireRole(['ADMIN']),
  async (req, res) => {
    try {
      const { topics } = req.body;

      if (!Array.isArray(topics) || topics.length === 0) {
        return res.status(400).json({
          success: false,
          error: {
            code: 'INVALID_TOPICS',
            message: 'Topics must be a non-empty array',
          },
        });
      }

      await eventBus.createTopics(topics);

      res.status(201).json({
        success: true,
        data: {
          message: 'Topics created successfully',
          topics,
        },
      });
    } catch (error) {
      logger.error('Error creating topics:', error);
      res.status(500).json({
        success: false,
        error: {
          code: 'TOPIC_CREATION_ERROR',
          message: 'Failed to create topics',
        },
      });
    }
  }
);

export default router;