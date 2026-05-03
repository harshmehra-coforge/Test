import { Kafka, Producer, Consumer, EachMessagePayload } from 'kafkajs';
import { EventEmitter } from 'events';
import { v4 as uuidv4 } from 'uuid';
import { BaseEvent } from '@/types';
import { logger } from '@/utils/logger';
import { config } from '@/config';

export class EventBus extends EventEmitter {
  private kafka: Kafka;
  private producer: Producer;
  private consumers: Map<string, Consumer> = new Map();
  private isConnected = false;

  constructor() {
    super();
    this.kafka = new Kafka({
      clientId: config.kafka.clientId,
      brokers: config.kafka.brokers,
      retry: {
        initialRetryTime: 100,
        retries: 8,
      },
    });

    this.producer = this.kafka.producer({
      maxInFlightRequests: 1,
      idempotent: true,
      transactionTimeout: 30000,
    });
  }

  async connect(): Promise<void> {
    try {
      await this.producer.connect();
      this.isConnected = true;
      logger.info('Event Bus connected successfully');
    } catch (error) {
      logger.error('Failed to connect Event Bus:', error);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    try {
      // Disconnect all consumers
      for (const [groupId, consumer] of this.consumers) {
        await consumer.disconnect();
        logger.info(`Consumer ${groupId} disconnected`);
      }
      this.consumers.clear();

      // Disconnect producer
      await this.producer.disconnect();
      this.isConnected = false;
      logger.info('Event Bus disconnected successfully');
    } catch (error) {
      logger.error('Error disconnecting Event Bus:', error);
      throw error;
    }
  }

  async publishEvent<T extends BaseEvent>(topic: string, event: T): Promise<void> {
    if (!this.isConnected) {
      throw new Error('Event Bus is not connected');
    }

    try {
      const message = {
        key: event.eventId,
        value: JSON.stringify(event),
        headers: {
          eventType: event.eventType,
          timestamp: event.timestamp.toISOString(),
          version: event.version,
        },
      };

      await this.producer.send({
        topic,
        messages: [message],
      });

      logger.info(`Event published to topic ${topic}:`, {
        eventId: event.eventId,
        eventType: event.eventType,
      });

      // Emit local event for same-process listeners
      this.emit(event.eventType, event);
    } catch (error) {
      logger.error(`Failed to publish event to topic ${topic}:`, error);
      throw error;
    }
  }

  async subscribe(
    topic: string,
    groupId: string,
    handler: (event: BaseEvent) => Promise<void>
  ): Promise<void> {
    try {
      const consumer = this.kafka.consumer({ groupId });
      await consumer.connect();
      await consumer.subscribe({ topic, fromBeginning: false });

      await consumer.run({
        eachMessage: async ({ message }: EachMessagePayload) => {
          try {
            if (!message.value) {
              logger.warn('Received message with no value');
              return;
            }

            const event = JSON.parse(message.value.toString()) as BaseEvent;
            
            logger.info(`Processing event from topic ${topic}:`, {
              eventId: event.eventId,
              eventType: event.eventType,
            });

            await handler(event);
          } catch (error) {
            logger.error(`Error processing message from topic ${topic}:`, error);
            // In production, you might want to send to a dead letter queue
            throw error;
          }
        },
      });

      this.consumers.set(groupId, consumer);
      logger.info(`Subscribed to topic ${topic} with group ${groupId}`);
    } catch (error) {
      logger.error(`Failed to subscribe to topic ${topic}:`, error);
      throw error;
    }
  }

  async createTopics(topics: string[]): Promise<void> {
    try {
      const admin = this.kafka.admin();
      await admin.connect();

      const existingTopics = await admin.listTopics();
      const topicsToCreate = topics.filter(topic => !existingTopics.includes(topic));

      if (topicsToCreate.length > 0) {
        await admin.createTopics({
          topics: topicsToCreate.map(topic => ({
            topic,
            numPartitions: 3,
            replicationFactor: 1, // Adjust based on your Kafka cluster setup
          })),
        });

        logger.info(`Created topics: ${topicsToCreate.join(', ')}`);
      }

      await admin.disconnect();
    } catch (error) {
      logger.error('Failed to create topics:', error);
      throw error;
    }
  }

  // Utility method to create standardized events
  createEvent<T extends Omit<BaseEvent, 'eventId' | 'timestamp' | 'version'>>(
    eventData: T
  ): T & BaseEvent {
    return {
      ...eventData,
      eventId: uuidv4(),
      timestamp: new Date(),
      version: '1.0',
    } as T & BaseEvent;
  }

  // Health check method
  async healthCheck(): Promise<{ status: string; details: any }> {
    try {
      const admin = this.kafka.admin();
      await admin.connect();
      
      const metadata = await admin.fetchTopicMetadata();
      await admin.disconnect();

      return {
        status: 'healthy',
        details: {
          connected: this.isConnected,
          activeConsumers: this.consumers.size,
          topics: metadata.topics.map(t => t.name),
        },
      };
    } catch (error) {
      return {
        status: 'unhealthy',
        details: {
          error: error instanceof Error ? error.message : 'Unknown error',
          connected: this.isConnected,
          activeConsumers: this.consumers.size,
        },
      };
    }
  }
}

// Singleton instance
export const eventBus = new EventBus();