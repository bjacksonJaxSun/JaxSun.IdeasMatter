import { createBrowserRouter, RouterProvider, Outlet } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { GoogleOAuthProvider } from '@react-oauth/google'
import LandingPage from './pages/LandingPage'
import Dashboard from './pages/Dashboard'
import AdminDashboard from './pages/AdminDashboard'
import ResearchPage from './pages/ResearchPage'
import GoogleOAuthTest from './pages/GoogleOAuthTest'
import { AuthProvider } from './contexts/AuthContext'

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || ''

function RootLayout() {
  return (
    <AuthProvider>
      <div className="min-h-screen">
        <Outlet />
        <Toaster 
          position="bottom-right"
          toastOptions={{
            duration: 4000,
            style: {
              background: '#363636',
              color: '#fff',
            },
          }}
        />
      </div>
    </AuthProvider>
  )
}

const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />,
    children: [
      {
        index: true,
        element: <LandingPage />
      },
      {
        path: "dashboard",
        element: <Dashboard />
      },
      {
        path: "research/:sessionId",
        element: <ResearchPage />
      },
      {
        path: "admin",
        element: <AdminDashboard />
      },
      {
        path: "admin/*",
        element: <AdminDashboard />
      },
      {
        path: "test-google-oauth",
        element: <GoogleOAuthTest />
      }
    ]
  }
])

function App() {
  console.log('Google Client ID:', googleClientId ? 'Set' : 'Not set')
  
  return (
    <GoogleOAuthProvider clientId={googleClientId}>
      <RouterProvider router={router} />
    </GoogleOAuthProvider>
  )
}

export default App