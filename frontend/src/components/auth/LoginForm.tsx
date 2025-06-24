import { useState } from 'react'
import { useAuth } from '../../contexts/AuthContext'
import { GoogleLogin } from '@react-oauth/google'
import toast from 'react-hot-toast'
import GoogleLoginBypass from './GoogleLoginBypass'

const LoginForm: React.FC = () => {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const { login, loginWithGoogle } = useAuth()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    try {
      await login(email, password)
    } catch (error) {
      // Error handled in AuthContext
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-gray-800 text-center mb-2">
          Welcome to Ideas Matter
        </h2>
        <p className="text-sm text-gray-600 text-center mb-8">
          Sign in to your account
        </p>
      </div>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
            Email Address
          </label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            className="input-field"
            placeholder="you@example.com"
            required
          />
        </div>
        <div>
          <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
            Password
          </label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            className="input-field"
            placeholder="••••••••"
            required
          />
        </div>
        <button
          type="submit"
          disabled={isLoading}
          className="w-full btn-primary"
        >
          {isLoading ? 'Signing in...' : 'Sign In'}
        </button>
      </form>
      <div className="relative mt-6">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-gray-300" />
        </div>
        <div className="relative flex justify-center text-sm">
          <span className="px-2 bg-white text-gray-500">Or continue with</span>
        </div>
      </div>
      <div className="flex justify-center mt-4">
        <GoogleLogin
          onSuccess={credentialResponse => {
            if (credentialResponse.credential) {
              loginWithGoogle(credentialResponse.credential)
            }
          }}
          onError={() => {
            toast.error('Google login failed')
          }}
          useOneTap={false}
          theme="outline"
          size="large"
          type="standard"
          locale="en"
        />
      </div>
      
      {/* Development bypass login */}
      {import.meta.env.DEV && (
        <GoogleLoginBypass />
      )}
    </div>
  )
}

export default LoginForm