import crypto from 'crypto';
import { config } from '@/config';

export class EncryptionService {
  private readonly algorithm = config.security.encryption.algorithm;
  private readonly key = Buffer.from(config.security.encryption.key, 'utf8');

  /**
   * Encrypt sensitive data using AES-256-GCM
   */
  encrypt(text: string): { encryptedData: string; iv: string; authTag: string } {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipher(this.algorithm, this.key);
    
    let encrypted = cipher.update(text, 'utf8', 'hex');
    encrypted += cipher.final('hex');
    
    const authTag = cipher.getAuthTag();
    
    return {
      encryptedData: encrypted,
      iv: iv.toString('hex'),
      authTag: authTag.toString('hex'),
    };
  }

  /**
   * Decrypt sensitive data using AES-256-GCM
   */
  decrypt(encryptedData: string, iv: string, authTag: string): string {
    const decipher = crypto.createDecipher(this.algorithm, this.key);
    decipher.setAuthTag(Buffer.from(authTag, 'hex'));
    
    let decrypted = decipher.update(encryptedData, 'hex', 'utf8');
    decrypted += decipher.final('utf8');
    
    return decrypted;
  }

  /**
   * Hash data using SHA-256
   */
  hash(data: string): string {
    return crypto.createHash('sha256').update(data).digest('hex');
  }

  /**
   * Generate a secure random token
   */
  generateToken(length: number = 32): string {
    return crypto.randomBytes(length).toString('hex');
  }

  /**
   * Generate a secure random UUID
   */
  generateUUID(): string {
    return crypto.randomUUID();
  }

  /**
   * Encrypt SSN for storage
   */
  encryptSSN(ssn: string): string {
    const { encryptedData, iv, authTag } = this.encrypt(ssn);
    return `${encryptedData}:${iv}:${authTag}`;
  }

  /**
   * Decrypt SSN from storage
   */
  decryptSSN(encryptedSSN: string): string {
    const [encryptedData, iv, authTag] = encryptedSSN.split(':');
    return this.decrypt(encryptedData, iv, authTag);
  }

  /**
   * Mask sensitive data for logging
   */
  maskSensitiveData(data: string, visibleChars: number = 4): string {
    if (data.length <= visibleChars) {
      return '*'.repeat(data.length);
    }
    
    const masked = '*'.repeat(data.length - visibleChars);
    return masked + data.slice(-visibleChars);
  }

  /**
   * Generate HMAC signature
   */
  generateHMAC(data: string, secret: string): string {
    return crypto.createHmac('sha256', secret).update(data).digest('hex');
  }

  /**
   * Verify HMAC signature
   */
  verifyHMAC(data: string, signature: string, secret: string): boolean {
    const expectedSignature = this.generateHMAC(data, secret);
    return crypto.timingSafeEqual(
      Buffer.from(signature, 'hex'),
      Buffer.from(expectedSignature, 'hex')
    );
  }
}

export const encryptionService = new EncryptionService();