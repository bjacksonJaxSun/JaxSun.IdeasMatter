/**
 * Mock Authentication Utility
 * Provides authentication functionality when backend is not available
 */

export interface MockUser {
  id: number;
  email: string;
  name: string;
  role: 'user' | 'admin' | 'system_admin';
  permissions: string[];
  tenantId?: string;
  picture?: string;
}

export class MockAuthService {
  private static instance: MockAuthService;
  
  public static getInstance(): MockAuthService {
    if (!MockAuthService.instance) {
      MockAuthService.instance = new MockAuthService();
    }
    return MockAuthService.instance;
  }
  
  /**
   * Create a mock user based on role
   */
  createMockUser(role: 'user' | 'admin' | 'system_admin'): MockUser {
    const users: Record<string, MockUser> = {
      user: {
        id: 2,
        email: 'user@example.com',
        name: 'Test User',
        role: 'user',
        permissions: ['read', 'write'],
        tenantId: 'mock-tenant-001'
      },
      admin: {
        id: 1,
        email: 'admin@example.com',
        name: 'Admin User',
        role: 'admin',
        permissions: ['read', 'write', 'delete', 'admin'],
        tenantId: 'mock-tenant-001'
      },
      system_admin: {
        id: 0,
        email: 'sysadmin@example.com',
        name: 'System Administrator',
        role: 'system_admin',
        permissions: ['read', 'write', 'delete', 'admin', 'system'],
        tenantId: 'mock-tenant-001'
      }
    };
    
    return users[role];
  }
  
  /**
   * Generate mock tokens
   */
  generateMockTokens(role: string): { accessToken: string; refreshToken: string } {
    const timestamp = Date.now();
    const randomSuffix = Math.random().toString(36).substring(7);
    
    return {
      accessToken: `mock_${role}_token_${timestamp}_${randomSuffix}`,
      refreshToken: `mock_refresh_${role}_token_${timestamp}_${randomSuffix}`
    };
  }
  
  /**
   * Store mock authentication in localStorage
   */
  storeMockAuth(role: 'user' | 'admin' | 'system_admin'): MockUser {
    const user = this.createMockUser(role);
    const tokens = this.generateMockTokens(role);
    
    // Store tokens
    localStorage.setItem('access_token', tokens.accessToken);
    localStorage.setItem('refresh_token', tokens.refreshToken);
    localStorage.setItem('mock_auth', 'true');
    localStorage.setItem('mock_user', JSON.stringify(user));
    
    console.log('🔧 Mock authentication stored:', {
      role,
      user: user.email,
      token: tokens.accessToken.substring(0, 20) + '...'
    });
    
    return user;
  }
  
  /**
   * Retrieve mock user from localStorage
   */
  getMockUser(): MockUser | null {
    const isMockAuth = localStorage.getItem('mock_auth') === 'true';
    const token = localStorage.getItem('access_token');
    
    if (!isMockAuth || !token || !token.startsWith('mock_')) {
      return null;
    }
    
    // Try to get stored user first
    const storedUser = localStorage.getItem('mock_user');
    if (storedUser) {
      try {
        return JSON.parse(storedUser);
      } catch (e) {
        console.warn('Failed to parse stored mock user');
      }
    }
    
    // Fallback: reconstruct user from token
    const role = token.includes('admin') ? 'admin' : 'user';
    return this.createMockUser(role);
  }
  
  /**
   * Check if currently using mock authentication
   */
  isMockAuth(): boolean {
    const isMock = localStorage.getItem('mock_auth') === 'true';
    const token = localStorage.getItem('access_token');
    return isMock && token !== null && token.startsWith('mock_');
  }
  
  /**
   * Clear mock authentication
   */
  clearMockAuth(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('mock_auth');
    localStorage.removeItem('mock_user');
    
    console.log('🧹 Mock authentication cleared');
  }
  
  /**
   * Simulate API delay for realistic feel
   */
  async simulateDelay(ms: number = 500): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
  
  /**
   * Mock bypass login that always works
   */
  async performBypassLogin(role: 'user' | 'admin' | 'system_admin'): Promise<MockUser> {
    console.log(`🚀 Performing mock bypass login for role: ${role}`);
    
    // Simulate API delay
    await this.simulateDelay(300);
    
    // Store mock auth
    const user = this.storeMockAuth(role);
    
    console.log('✅ Mock bypass login successful');
    return user;
  }
}

// Export singleton instance
export const mockAuth = MockAuthService.getInstance();

// Export helper functions
export const isMockAuthEnabled = () => mockAuth.isMockAuth();
export const getMockUser = () => mockAuth.getMockUser();
export const performMockLogin = (role: 'user' | 'admin' | 'system_admin') => 
  mockAuth.performBypassLogin(role);