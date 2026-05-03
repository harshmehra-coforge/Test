import speakeasy from 'speakeasy';
import QRCode from 'qrcode';
import { config } from '@/config';
import { MFAMethod } from '@/types';
import { logger } from './logger';

export class MFAService {
  private readonly issuer = config.security.mfa.issuer;
  private readonly window = config.security.mfa.window;

  /**
   * Generate MFA secret for TOTP
   */
  generateSecret(userEmail: string): { secret: string; qrCode: string } {
    const secret = speakeasy.generateSecret({
      name: userEmail,
      issuer: this.issuer,
      length: 32,
    });

    const qrCodeUrl = speakeasy.otpauthURL({
      secret: secret.base32,
      label: userEmail,
      issuer: this.issuer,
      encoding: 'base32',
    });

    return {
      secret: secret.base32,
      qrCode: qrCodeUrl,
    };
  }

  /**
   * Generate QR code image for TOTP setup
   */
  async generateQRCodeImage(qrCodeUrl: string): Promise<string> {
    try {
      return await QRCode.toDataURL(qrCodeUrl);
    } catch (error) {
      logger.error('Error generating QR code:', error);
      throw new Error('Failed to generate QR code');
    }
  }

  /**
   * Verify TOTP token
   */
  verifyTOTP(token: string, secret: string): boolean {
    try {
      return speakeasy.totp.verify({
        secret,
        encoding: 'base32',
        token,
        window: this.window,
      });
    } catch (error) {
      logger.error('Error verifying TOTP:', error);
      return false;
    }
  }

  /**
   * Generate backup codes
   */
  generateBackupCodes(count: number = 10): string[] {
    const codes: string[] = [];
    
    for (let i = 0; i < count; i++) {
      // Generate 8-character alphanumeric code
      const code = Math.random().toString(36).substring(2, 10).toUpperCase();
      codes.push(code);
    }
    
    return codes;
  }

  /**
   * Generate SMS code
   */
  generateSMSCode(): string {
    // Generate 6-digit numeric code
    return Math.floor(100000 + Math.random() * 900000).toString();
  }

  /**
   * Generate email code
   */
  generateEmailCode(): string {
    // Generate 8-character alphanumeric code
    return Math.random().toString(36).substring(2, 10).toUpperCase();
  }

  /**
   * Verify SMS/Email code (time-based verification)
   */
  verifyCode(providedCode: string, expectedCode: string, generatedAt: Date, validityMinutes: number = 5): boolean {
    // Check if code matches
    if (providedCode !== expectedCode) {
      return false;
    }

    // Check if code is still valid (within time window)
    const now = new Date();
    const expirationTime = new Date(generatedAt.getTime() + validityMinutes * 60 * 1000);
    
    return now <= expirationTime;
  }

  /**
   * Get MFA method display name
   */
  getMethodDisplayName(method: MFAMethod): string {
    switch (method) {
      case MFAMethod.SMS:
        return 'SMS Text Message';
      case MFAMethod.EMAIL:
        return 'Email';
      case MFAMethod.TOTP:
        return 'Authenticator App';
      case MFAMethod.PUSH:
        return 'Push Notification';
      default:
        return 'Unknown Method';
    }
  }

  /**
   * Validate MFA setup requirements
   */
  validateMFASetup(method: MFAMethod, data: any): { valid: boolean; error?: string } {
    switch (method) {
      case MFAMethod.TOTP:
        if (!data.secret || !data.token) {
          return { valid: false, error: 'Secret and verification token are required for TOTP setup' };
        }
        
        if (!this.verifyTOTP(data.token, data.secret)) {
          return { valid: false, error: 'Invalid verification token' };
        }
        
        return { valid: true };

      case MFAMethod.SMS:
        if (!data.phoneNumber) {
          return { valid: false, error: 'Phone number is required for SMS setup' };
        }
        
        // Basic phone number validation
        const phoneRegex = /^\+?[1-9]\d{1,14}$/;
        if (!phoneRegex.test(data.phoneNumber.replace(/\s/g, ''))) {
          return { valid: false, error: 'Invalid phone number format' };
        }
        
        return { valid: true };

      case MFAMethod.EMAIL:
        if (!data.email) {
          return { valid: false, error: 'Email address is required for email setup' };
        }
        
        // Basic email validation
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(data.email)) {
          return { valid: false, error: 'Invalid email address format' };
        }
        
        return { valid: true };

      default:
        return { valid: false, error: 'Unsupported MFA method' };
    }
  }

  /**
   * Check if MFA is required for operation
   */
  isMFARequired(operation: string, amount?: number): boolean {
    const mfaRequiredOperations = [
      'payment_approval',
      'account_modification',
      'security_settings_change',
      'large_transaction',
    ];

    // Always require MFA for sensitive operations
    if (mfaRequiredOperations.includes(operation)) {
      return true;
    }

    // Require MFA for large transactions
    if (operation === 'transaction' && amount && amount >= 100000) {
      return true;
    }

    return false;
  }
}

export const mfaService = new MFAService();