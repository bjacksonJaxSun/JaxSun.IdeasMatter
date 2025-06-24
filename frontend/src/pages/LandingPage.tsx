import { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import LoginForm from '../components/auth/LoginForm'
import SignupForm from '../components/auth/SignupForm'
import { 
  SparklesIcon, 
  ChartBarIcon, 
  CodeBracketIcon, 
  RocketLaunchIcon,
  LightBulbIcon,
  DocumentTextIcon,
  BeakerIcon,
  CommandLineIcon
} from '@heroicons/react/24/outline'

const LandingPage = () => {
  const [showAuth, setShowAuth] = useState(false)
  const [authMode, setAuthMode] = useState<'login' | 'signup'>('login')

  const features = [
    {
      icon: LightBulbIcon,
      title: 'AI-Powered Ideation',
      description: 'Transform your raw ideas into comprehensive business concepts with AI-driven analysis and enhancement.'
    },
    {
      icon: ChartBarIcon,
      title: 'Market Research & Analysis',
      description: 'Get instant market insights, competitor analysis, and target audience profiles powered by AI.'
    },
    {
      icon: DocumentTextIcon,
      title: 'Business Planning',
      description: 'Generate complete business plans, project roadmaps, and execution strategies automatically.'
    },
    {
      icon: CodeBracketIcon,
      title: 'Technical Architecture',
      description: 'Create software architecture, technical designs, and implementation strategies.'
    },
    {
      icon: BeakerIcon,
      title: 'Automated Development',
      description: 'Generate code, test automation, and CLI tools. Push directly to GitHub repositories.'
    },
    {
      icon: RocketLaunchIcon,
      title: 'Project Management',
      description: 'Break down ideas into epics, features, user stories, and trackable milestones.'
    }
  ]

  const workflow = [
    { step: 1, title: 'Submit Your Idea', description: 'Enter your concept in simple terms' },
    { step: 2, title: 'AI Analysis', description: 'Our AI researches and enhances your idea' },
    { step: 3, title: 'Business Plan', description: 'Get a complete business strategy' },
    { step: 4, title: 'Technical Design', description: 'Receive architecture and implementation plans' },
    { step: 5, title: 'Code Generation', description: 'Auto-generate code and push to GitHub' },
    { step: 6, title: 'Launch', description: 'Execute with clear milestones and metrics' }
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 via-white to-primary-50">
      {/* Navigation */}
      <nav className="fixed top-0 w-full bg-white/80 backdrop-blur-md shadow-sm z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <LightBulbIcon className="w-8 h-8 text-primary-600 mr-2" />
              <h1 className="text-2xl font-bold text-primary-600">Ideas Matter</h1>
            </div>
            <div className="flex items-center space-x-4">
              {import.meta.env.DEV && (
                <a
                  href="/test-google-oauth"
                  className="text-sm text-gray-500 hover:text-gray-700 font-medium transition-colors"
                >
                  Test OAuth
                </a>
              )}
              <button
                onClick={() => {
                  setAuthMode('login')
                  setShowAuth(true)
                }}
                className="text-gray-600 hover:text-gray-900 font-medium transition-colors"
              >
                Login
              </button>
              <button
                onClick={() => {
                  setAuthMode('signup')
                  setShowAuth(true)
                }}
                className="btn-primary"
              >
                Start Building
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="pt-24 pb-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="text-center"
          >
            <h2 className="text-5xl sm:text-6xl font-bold text-gray-900 mb-6">
              Turn Your Ideas Into
              <span className="block text-primary-600 mt-2">Living Products</span>
            </h2>
            <p className="text-xl text-gray-600 mb-8 max-w-3xl mx-auto">
              From concept to code in minutes. Our AI platform transforms your ideas into 
              market-ready products with business plans, technical architecture, and automated development.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <button
                onClick={() => {
                  setAuthMode('signup')
                  setShowAuth(true)
                }}
                className="btn-primary text-lg px-8 py-3"
              >
                Build Your Idea Now
              </button>
              <button className="btn-secondary text-lg px-8 py-3">
                See How It Works
              </button>
            </div>
          </motion.div>

          {/* Hero Visual */}
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.8, delay: 0.2 }}
            className="mt-16 relative"
          >
            <div className="bg-gradient-to-r from-primary-400 to-primary-600 rounded-2xl p-8 text-white">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                <div className="text-center">
                  <SparklesIcon className="w-12 h-12 mx-auto mb-2" />
                  <h3 className="text-lg font-semibold">AI Research</h3>
                  <p className="text-primary-100">Market validation in seconds</p>
                </div>
                <div className="text-center">
                  <CodeBracketIcon className="w-12 h-12 mx-auto mb-2" />
                  <h3 className="text-lg font-semibold">Code Generation</h3>
                  <p className="text-primary-100">Full stack implementation</p>
                </div>
                <div className="text-center">
                  <RocketLaunchIcon className="w-12 h-12 mx-auto mb-2" />
                  <h3 className="text-lg font-semibold">GitHub Deploy</h3>
                  <p className="text-primary-100">Ready to launch products</p>
                </div>
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      {/* Workflow Section */}
      <section className="py-20 px-4 sm:px-6 lg:px-8 bg-gray-50">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h3 className="text-3xl font-bold text-gray-900 mb-4">
              From Idea to Implementation in 6 Steps
            </h3>
            <p className="text-lg text-gray-600">
              Our AI-powered workflow handles everything automatically
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {workflow.map((item, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: index * 0.1 }}
                className="relative"
              >
                <div className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow">
                  <div className="flex items-center mb-4">
                    <div className="w-10 h-10 bg-primary-600 text-white rounded-full flex items-center justify-center font-bold mr-3">
                      {item.step}
                    </div>
                    <h4 className="text-lg font-semibold text-gray-900">{item.title}</h4>
                  </div>
                  <p className="text-gray-600">{item.description}</p>
                </div>
                {index < workflow.length - 1 && (
                  <div className="hidden lg:block absolute top-12 left-full w-8 h-0.5 bg-gray-300 -ml-4 z-10"></div>
                )}
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20 px-4 sm:px-6 lg:px-8 bg-white">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h3 className="text-3xl font-bold text-gray-900 mb-4">
              Everything You Need to Build Your Vision
            </h3>
            <p className="text-lg text-gray-600">
              Comprehensive tools powered by cutting-edge AI
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: index * 0.1 }}
                className="bg-gray-50 rounded-xl p-6 hover:shadow-lg transition-shadow"
              >
                <feature.icon className="w-12 h-12 text-primary-600 mb-4" />
                <h4 className="text-xl font-semibold text-gray-900 mb-2">
                  {feature.title}
                </h4>
                <p className="text-gray-600">
                  {feature.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 px-4 sm:px-6 lg:px-8 bg-primary-600">
        <div className="max-w-4xl mx-auto text-center">
          <h3 className="text-3xl font-bold text-white mb-4">
            Ready to Build Something Amazing?
          </h3>
          <p className="text-xl text-primary-100 mb-8">
            Join innovators who are turning ideas into reality with AI
          </p>
          <button
            onClick={() => {
              setAuthMode('signup')
              setShowAuth(true)
            }}
            className="bg-white text-primary-600 font-semibold py-3 px-8 rounded-lg hover:bg-gray-100 transition-colors"
          >
            Start Building for Free
          </button>
        </div>
      </section>

      {/* Auth Modal */}
      <AnimatePresence>
        {showAuth && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black bg-opacity-50 z-50"
              onClick={() => setShowAuth(false)}
            />
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="fixed inset-0 flex items-center justify-center z-50 p-4"
            >
              <div className="bg-white rounded-2xl shadow-xl max-w-md w-full p-8" onClick={(e) => e.stopPropagation()}>
                <div className="flex justify-between items-center mb-6">
                  <h2 className="text-2xl font-bold text-gray-900">
                    {authMode === 'login' ? 'Welcome Back' : 'Start Building Ideas'}
                  </h2>
                  <button
                    onClick={() => setShowAuth(false)}
                    className="text-gray-400 hover:text-gray-600"
                  >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>

                {authMode === 'login' ? (
                  <LoginForm onSwitchToSignup={() => setAuthMode('signup')} />
                ) : (
                  <SignupForm onSwitchToLogin={() => setAuthMode('login')} />
                )}
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </div>
  )
}

export default LandingPage