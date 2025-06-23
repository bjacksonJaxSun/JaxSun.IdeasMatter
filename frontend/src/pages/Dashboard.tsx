import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { ideasService } from '../services/ideas'
import toast from 'react-hot-toast'
import { 
  PlusCircleIcon,
  LightBulbIcon,
  ChartBarIcon,
  CodeBracketIcon,
  DocumentTextIcon,
  ClockIcon,
  CheckCircleIcon,
  ArrowRightIcon,
  ArrowPathIcon
} from '@heroicons/react/24/outline'

interface Idea {
  id: string
  title: string
  description: string
  status: 'draft' | 'researching' | 'planning' | 'developing' | 'completed'
  createdAt: string
  progress: {
    research: boolean
    businessPlan: boolean
    architecture: boolean
    code: boolean
    deployment: boolean
  }
}

const Dashboard = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [showNewIdea, setShowNewIdea] = useState(false)
  const [ideaTitle, setIdeaTitle] = useState('')
  const [ideaDescription, setIdeaDescription] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [ideas, setIdeas] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)

  // Fetch user's research sessions on mount and set up periodic refresh
  useEffect(() => {
    fetchIdeas()
    
    // Set up periodic refresh to check for research completion
    const interval = setInterval(() => {
      fetchIdeas()
    }, 30000) // Refresh every 30 seconds
    
    return () => clearInterval(interval)
  }, [])

  const fetchIdeas = async () => {
    try {
      setIsLoading(true)
      const sessions = await ideasService.listResearchSessions()
      
      // Map research sessions to idea format
      const mappedIdeas = sessions.map((session: any) => ({
        id: session.id,
        title: session.title,
        description: session.description,
        status: session.status,
        createdAt: new Date(session.created_at).toLocaleDateString(),
        progress: {
          research: session.status === 'completed' || session.status === 'planning' || session.status === 'developing',
          businessPlan: session.status === 'planning' || session.status === 'developing' || session.status === 'completed',
          architecture: session.status === 'developing' || session.status === 'completed',
          code: session.status === 'completed',
          deployment: false
        }
      }))
      
      setIdeas(mappedIdeas)
    } catch (error) {
      console.error('Error fetching ideas:', error)
      toast.error('Failed to load ideas')
    } finally {
      setIsLoading(false)
    }
  }

  // Keep mock data as fallback for UI demonstration
  const mockIdeas = [
    {
      id: '1',
      title: 'AI-Powered Recipe App',
      description: 'An app that generates personalized recipes based on dietary preferences and available ingredients',
      status: 'developing',
      createdAt: '2024-01-13',
      progress: {
        research: true,
        businessPlan: true,
        architecture: true,
        code: false,
        deployment: false
      }
    },
    {
      id: '2',
      title: 'Sustainable Fashion Marketplace',
      description: 'Platform connecting eco-conscious consumers with sustainable fashion brands',
      status: 'planning',
      createdAt: '2024-01-12',
      progress: {
        research: true,
        businessPlan: false,
        architecture: false,
        code: false,
        deployment: false
      }
    }
  ]

  const getStatusColor = (status: Idea['status']) => {
    switch (status) {
      case 'draft': return 'bg-gray-100 text-gray-800'
      case 'researching': return 'bg-blue-100 text-blue-800'
      case 'planning': return 'bg-yellow-100 text-yellow-800'
      case 'developing': return 'bg-purple-100 text-purple-800'
      case 'completed': return 'bg-green-100 text-green-800'
    }
  }

  const getStatusLabel = (status: Idea['status']) => {
    switch (status) {
      case 'draft': return 'Draft'
      case 'researching': return 'Researching'
      case 'planning': return 'Planning'
      case 'developing': return 'Developing'
      case 'completed': return 'Completed'
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <LightBulbIcon className="w-8 h-8 text-primary-600 mr-2" />
              <h1 className="text-2xl font-bold text-primary-600">Ideas Matter</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-gray-700">Welcome, {user?.name}</span>
              <button
                onClick={logout}
                className="text-gray-500 hover:text-gray-700"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Dashboard Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Dashboard Header */}
        <div className="mb-8 flex justify-between items-center">
          <div>
            <h2 className="text-3xl font-bold text-gray-900">Your Ideas</h2>
            <p className="text-gray-600">Transform your concepts into reality</p>
          </div>
          <button
            onClick={() => setShowNewIdea(true)}
            className="btn-primary flex items-center"
          >
            <PlusCircleIcon className="w-5 h-5 mr-2" />
            New Idea
          </button>
        </div>

        {/* Stats Overview */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex items-center">
              <LightBulbIcon className="h-8 w-8 text-primary-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Total Ideas</p>
                <p className="text-2xl font-semibold text-gray-900">{ideas.length}</p>
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex items-center">
              <ChartBarIcon className="h-8 w-8 text-green-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">In Development</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {ideas.filter(i => i.status === 'developing').length}
                </p>
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex items-center">
              <CheckCircleIcon className="h-8 w-8 text-blue-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Completed</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {ideas.filter(i => i.status === 'completed').length}
                </p>
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex items-center">
              <ClockIcon className="h-8 w-8 text-yellow-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">This Week</p>
                <p className="text-2xl font-semibold text-gray-900">2</p>
              </div>
            </div>
          </div>
        </div>

        {/* Ideas List */}
        <div className="space-y-6">
          {isLoading ? (
            <div className="text-center py-12">
              <ArrowPathIcon className="w-8 h-8 animate-spin mx-auto text-gray-400" />
              <p className="mt-2 text-gray-500">Loading ideas...</p>
            </div>
          ) : ideas.length > 0 ? (
            ideas.map((idea) => (
            <div key={idea.id} className="bg-white rounded-lg shadow-sm p-6">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900">{idea.title}</h3>
                  <p className="text-gray-600 mt-1">{idea.description}</p>
                  <div className="flex items-center mt-2 text-sm text-gray-500">
                    <ClockIcon className="w-4 h-4 mr-1" />
                    Created {idea.createdAt}
                  </div>
                </div>
                <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(idea.status)}`}>
                  {getStatusLabel(idea.status)}
                </span>
              </div>

              {/* Progress Steps */}
              <div className="border-t pt-4">
                <h4 className="text-sm font-medium text-gray-700 mb-3">Progress</h4>
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-2">
                    <div className={`flex items-center ${idea.progress?.research ? 'text-green-600' : 'text-gray-400'}`}>
                      <CheckCircleIcon className="w-5 h-5" />
                      <span className="ml-1 text-sm">Research</span>
                    </div>
                    <ArrowRightIcon className="w-4 h-4 text-gray-400" />
                    <div className={`flex items-center ${idea.progress?.businessPlan ? 'text-green-600' : 'text-gray-400'}`}>
                      <CheckCircleIcon className="w-5 h-5" />
                      <span className="ml-1 text-sm">Business Plan</span>
                    </div>
                    <ArrowRightIcon className="w-4 h-4 text-gray-400" />
                    <div className={`flex items-center ${idea.progress?.architecture ? 'text-green-600' : 'text-gray-400'}`}>
                      <CheckCircleIcon className="w-5 h-5" />
                      <span className="ml-1 text-sm">Architecture</span>
                    </div>
                    <ArrowRightIcon className="w-4 h-4 text-gray-400" />
                    <div className={`flex items-center ${idea.progress?.code ? 'text-green-600' : 'text-gray-400'}`}>
                      <CheckCircleIcon className="w-5 h-5" />
                      <span className="ml-1 text-sm">Code</span>
                    </div>
                    <ArrowRightIcon className="w-4 h-4 text-gray-400" />
                    <div className={`flex items-center ${idea.progress?.deployment ? 'text-green-600' : 'text-gray-400'}`}>
                      <CheckCircleIcon className="w-5 h-5" />
                      <span className="ml-1 text-sm">Deployed</span>
                    </div>
                  </div>
                  <div className="flex space-x-2">
                    <button 
                      onClick={() => navigate(`/research/${idea.id}`)}
                      className="btn-primary text-sm"
                    >
                      View Research
                    </button>
                    <button
                      className="btn-danger text-sm"
                      onClick={async () => {
                        if (window.confirm('Are you sure you want to delete this idea? This action cannot be undone.')) {
                          try {
                            await ideasService.deleteIdea(Number(idea.id))
                            toast.success('Idea deleted successfully')
                            fetchIdeas()
                          } catch (error: any) {
                            toast.error(error.message || 'Failed to delete idea')
                          }
                        }
                      }}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))
          ) : (
            <div className="text-center py-12 bg-white rounded-lg shadow-sm">
              <LightBulbIcon className="w-12 h-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No ideas yet</h3>
              <p className="text-gray-500 mb-4">Start by submitting your first idea!</p>
              <button
                onClick={() => setShowNewIdea(true)}
                className="btn-primary inline-flex items-center"
              >
                <PlusCircleIcon className="w-5 h-5 mr-2" />
                New Idea
              </button>
            </div>
          )}
        </div>

        {/* New Idea Modal */}
        {showNewIdea && (
          <>
            <div className="fixed inset-0 bg-black bg-opacity-50 z-40" onClick={() => setShowNewIdea(false)} />
            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6">
                <h3 className="text-2xl font-bold text-gray-900 mb-6">Submit Your Idea</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Idea Title
                    </label>
                    <input
                      type="text"
                      value={ideaTitle}
                      onChange={(e) => setIdeaTitle(e.target.value)}
                      className="input-field"
                      placeholder="Give your idea a catchy title"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Describe Your Idea
                    </label>
                    <textarea
                      value={ideaDescription}
                      onChange={(e) => setIdeaDescription(e.target.value)}
                      rows={4}
                      className="input-field"
                      placeholder="Explain your idea in detail. What problem does it solve? Who is it for?"
                    />
                  </div>
                  <div className="bg-primary-50 p-4 rounded-lg">
                    <h4 className="font-medium text-primary-900 mb-2">What happens next?</h4>
                    <ul className="text-sm text-primary-800 space-y-1">
                      <li>• AI will analyze market potential and competition</li>
                      <li>• Generate a comprehensive business plan</li>
                      <li>• Create technical architecture and design</li>
                      <li>• Write production-ready code</li>
                      <li>• Deploy to GitHub with full documentation</li>
                    </ul>
                  </div>
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      onClick={() => setShowNewIdea(false)}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={async () => {
                        if (!ideaTitle || !ideaDescription) {
                          toast.error('Please provide both title and description')
                          return
                        }
                        
                        setIsSubmitting(true)
                        try {
                          // Submit idea with automatic competitive analysis
                          const response = await ideasService.submitIdea({
                            title: ideaTitle,
                            description: ideaDescription,
                            idea_id: Date.now().toString()
                          })
                          
                          toast.success(response.message)
                          
                          // Clear form and close modal
                          setShowNewIdea(false)
                          setIdeaTitle('')
                          setIdeaDescription('')
                          
                          // Refresh ideas list to show new idea
                          await fetchIdeas()
                          
                          // Navigate to the research page
                          navigate(`/research/${response.session_id}`)
                        } catch (error: any) {
                          toast.error(error.message || 'Failed to submit idea')
                        } finally {
                          setIsSubmitting(false)
                        }
                      }}
                      disabled={isSubmitting || !ideaTitle || !ideaDescription}
                      className="btn-primary flex items-center"
                    >
                      {isSubmitting ? (
                        <>
                          <ArrowPathIcon className="w-4 h-4 mr-2 animate-spin" />
                          Analyzing...
                        </>
                      ) : (
                        'Start Research'
                      )}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}
      </main>
    </div>
  )
}

export default Dashboard