import React, { createContext, useContext, useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import toast from 'react-hot-toast'
import { mockAuth, isMockAuthEnabled, getMockUser, performMockLogin } from '../utils/mockAuth'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000/api/v1'

interface User {
  id: number
  email: string
  name: string
  picture?: string
  role: 'user' | 'admin' | 'system_admin'
  tenantId?: string
  permissions: string[]
}

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  signup: (email: string, password: string, name: string) => Promise<void>
  loginWithGoogle: (credential: string) => Promise<void>
  bypassLogin: (role: 'user' | 'admin') => Promise<void>
  logout: () => void
  isAdmin: () => boolean
  isSystemAdmin: () => boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

// Configure axios defaults
const API_BASE = import.meta.env.VITE_API_URL || import.meta.env.VITE_API_BASE_URL || 'http://localhost:8000/api/v1'
axios.defaults.baseURL = API_BASE

// Log the configuration for debugging
console.log('API Base URL configured:', API_BASE)
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  const isMockAuth = localStorage.getItem('mock_auth') === 'true'
  
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  
  // For mock auth, skip certain API calls
  if (isMockAuth && token && token.startsWith('mock_')) {
    // Allow the request but it will fail - that's OK for mock mode
    console.log('Mock auth mode - API call will fail:', config.url)
  }
  
  return config
})

axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      try {
        const refreshToken = localStorage.getItem('refresh_token')
        if (refreshToken) {
          const response = await axios.post('/auth/refresh', { refresh_token: refreshToken })
          const { access_token, refresh_token: newRefreshToken } = response.data
          localStorage.setItem('access_token', access_token)
          localStorage.setItem('refresh_token', newRefreshToken)
          originalRequest.headers.Authorization = `Bearer ${access_token}`
          return axios(originalRequest)
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        localStorage.removeItem('access_token')
        localStorage.removeItem('refresh_token')
        window.location.href = '/'
      }
    }
    return Promise.reject(error)
  }
)

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const navigate = useNavigate()

  useEffect(() => {
    // Check if user is logged in on mount
    const token = localStorage.getItem('access_token')
    if (token) {
      // Fetch current user info
      fetchCurrentUser()
    } else {
      setLoading(false)
    }
  }, [])

  const fetchCurrentUser = async () => {
    try {
      const response = await axios.get('/auth/me', { timeout: 5000 })
      setUser(response.data)
      console.log('✅ User fetched from API:', response.data.email)
    } catch (error: any) {
      console.error('🚨 Failed to fetch user from API:', error?.message || error)
      
      // Check if we're in mock auth mode
      if (isMockAuthEnabled()) {
        const mockUser = getMockUser()
        if (mockUser) {
          setUser(mockUser)
          console.log('✅ Restored mock user session:', mockUser.email)
        } else {
          console.warn('⚠️ Mock auth enabled but no mock user found')
          mockAuth.clearMockAuth()
        }
      } else {
        // Clear invalid tokens
        console.log('🧹 Clearing invalid authentication tokens')
        localStorage.removeItem('access_token')
        localStorage.removeItem('refresh_token')
        localStorage.removeItem('mock_auth')
        localStorage.removeItem('mock_user')
      }
    } finally {
      setLoading(false)
    }
  }

  const login = async (email: string, password: string) => {
    try {
      const response = await axios.post('/auth/login', 
        new URLSearchParams({
          username: email,
          password: password
        }),
        {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        }
      )
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/auth/me')
      setUser(userResponse.data)
      
      toast.success('Login successful!')
      
      // Navigate based on role
      if (userResponse.data.role === 'system_admin') {
        navigate('/admin')
      } else {
        navigate('/dashboard')
      }
    } catch (error: any) {
      toast.error(error.response?.data?.detail || 'Login failed')
      throw error
    }
  }

  const signup = async (email: string, password: string, name: string) => {
    try {
      const response = await axios.post('/auth/register', {
        email,
        password,
        name
      })
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/auth/me')
      setUser(userResponse.data)
      
      toast.success('Account created successfully!')
      navigate('/dashboard')
    } catch (error: any) {
      toast.error(error.response?.data?.detail || 'Signup failed')
      throw error
    }
  }

  const loginWithGoogle = async (credential: string) => {
    try {
      console.log('🔐 Starting Google login process...');
      console.log('📝 Credential length:', credential.length);
      
      const response = await axios.post('/auth/google', {
        credential
      })
      
      console.log('✅ Google login API response received:', response.data);
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      console.log('💾 Tokens stored in localStorage');
      
      // Fetch user info
      console.log('👤 Fetching user info...');
      const userResponse = await axios.get('/auth/me')
      setUser(userResponse.data)
      
      console.log('✅ User info received:', userResponse.data);
      
      toast.success('Google login successful!')
      
      // Navigate based on role
      if (userResponse.data.role === 'system_admin') {
        console.log('🚀 Navigating to admin dashboard');
        navigate('/admin')
      } else {
        console.log('🚀 Navigating to user dashboard');
        navigate('/dashboard')
      }
    } catch (error: any) {
      console.error('🚨 Google login error:', error);
      console.error('🚨 Error response:', error.response?.data);
      console.error('🚨 Error status:', error.response?.status);
      toast.error(error.response?.data?.detail || 'Google login failed')
      throw error
    }
  }

  const bypassLogin = async (role: 'user' | 'admin') => {
    try {
      console.log(`🔐 Attempting bypass login for role: ${role}`);
      
      const response = await axios.post('/auth/bypass', {
        role
      }, { timeout: 5000 }) // 5 second timeout
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/auth/me')
      setUser(userResponse.data)
      
      console.log('✅ User info received:', userResponse.data);
      
      toast.success(`Logged in as ${role}!`)
      
      // Navigate based on actual user role from API response
      if (userResponse.data.role === 'system_admin') {
        console.log('🚀 Navigating to admin dashboard');
        navigate('/admin')
      } else {
        console.log('🚀 Navigating to user dashboard');
        navigate('/dashboard')
      }
    } catch (error: any) {
      console.error('🚨 Bypass login API failed:', error?.message || error);
      console.log('🔧 Switching to mock authentication...');
      
      try {
        // Use improved mock authentication
        const mockUser = await performMockLogin(role);
        
        // Set user in state
        setUser(mockUser);
        
        // Show success message
        toast.success(`Logged in as ${role} (offline mode)`);
        
        // Navigate based on role
        if (role === 'admin') {
          navigate('/admin');
        } else {
          navigate('/dashboard');
        }
      } catch (mockError) {
        console.error('🚨 Mock authentication failed:', mockError);
        toast.error('Login failed. Please try again.');
      }
    }
  }

  const logout = async () => {
    const isMockAuth = localStorage.getItem('mock_auth') === 'true'
    
    if (!isMockAuth) {
      try {
        await axios.post('/auth/logout')
      } catch (error) {
        // Logout anyway even if API call fails
        console.log('Logout API failed, clearing local session')
      }
    }
    
    localStorage.removeItem('access_token')
    localStorage.removeItem('refresh_token')
    localStorage.removeItem('mock_auth')
    setUser(null)
    toast.success('Logged out successfully')
    navigate('/')
  }

  const isAdmin = () => {
    return user?.role === 'admin' || user?.role === 'system_admin'
  }

  const isSystemAdmin = () => {
    return user?.role === 'system_admin'
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, signup, loginWithGoogle, bypassLogin, logout, isAdmin, isSystemAdmin }}>
      {children}
    </AuthContext.Provider>
  )
}