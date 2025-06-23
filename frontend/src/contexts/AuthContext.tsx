import React, { createContext, useContext, useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import toast from 'react-hot-toast'

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

// Configure axios defaults - remove /api/v1 since we'll include it in the endpoint paths
const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000'
axios.defaults.baseURL = BASE_URL
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
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
          const response = await axios.post('/api/v1/auth/refresh', { refresh_token: refreshToken })
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
      const response = await axios.get('/api/v1/auth/me')
      setUser(response.data)
    } catch (error) {
      // Token might be invalid
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
    } finally {
      setLoading(false)
    }
  }

  const login = async (email: string, password: string) => {
    try {
      const response = await axios.post('/api/v1/auth/login', 
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
      const userResponse = await axios.get('/api/v1/auth/me')
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
      const response = await axios.post('/api/v1/auth/register', {
        email,
        password,
        name
      })
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/api/v1/auth/me')
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
      const response = await axios.post('/api/v1/auth/google', {
        credential
      })
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/api/v1/auth/me')
      setUser(userResponse.data)
      
      toast.success('Google login successful!')
      
      // Navigate based on role
      if (userResponse.data.role === 'system_admin') {
        navigate('/admin')
      } else {
        navigate('/dashboard')
      }
    } catch (error: any) {
      console.error('Google login error:', error)
      toast.error(error.response?.data?.detail || 'Google login failed')
      throw error
    }
  }

  const bypassLogin = async (role: 'user' | 'admin') => {
    try {
      const response = await axios.post('/api/v1/auth/bypass', {
        role
      })
      
      const { access_token, refresh_token } = response.data
      localStorage.setItem('access_token', access_token)
      localStorage.setItem('refresh_token', refresh_token)
      
      // Fetch user info
      const userResponse = await axios.get('/api/v1/auth/me')
      setUser(userResponse.data)
      
      toast.success(`Logged in as ${role}!`)
      
      // Navigate based on role
      if (role === 'admin') {
        navigate('/admin')
      } else {
        navigate('/dashboard')
      }
    } catch (error: any) {
      console.error('Bypass login error:', error)
      toast.error(error.response?.data?.detail || 'Bypass login failed')
      throw error
    }
  }

  const logout = async () => {
    try {
      await axios.post('/api/v1/auth/logout')
    } catch (error) {
      // Logout anyway even if API call fails
    }
    
    localStorage.removeItem('access_token')
    localStorage.removeItem('refresh_token')
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