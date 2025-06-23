import axios from 'axios'

const API_URL = '/api/v1/auth'

// Create axios instance with defaults
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add token to requests if it exists
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

export const authService = {
  login: (email: string, password: string) => {
    // FastAPI OAuth2PasswordRequestForm expects form data, not JSON
    const formData = new FormData()
    formData.append('username', email) // OAuth2 uses 'username' field for email
    formData.append('password', password)
    
    return api.post('/login', formData, {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
    })
  },

  signup: (email: string, password: string, name: string) => {
    return api.post('/register', { email, password, name })
  },

  logout: () => {
    return api.post('/logout')
  },

  validateToken: () => {
    return api.get('/validate')
  },

  forgotPassword: (email: string) => {
    return api.post('/forgot-password', { email })
  },

  resetPassword: (token: string, password: string) => {
    return api.post('/reset-password', { token, password })
  },
}