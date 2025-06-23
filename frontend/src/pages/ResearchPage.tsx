import React, { useState, useEffect, useRef } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { 
  ChatBubbleLeftRightIcon, 
  LightBulbIcon, 
  ChartBarIcon,
  DocumentTextIcon,
  CheckBadgeIcon,
  ArrowPathIcon,
  PaperAirplaneIcon
} from '@heroicons/react/24/outline'
import toast from 'react-hot-toast'
import ResearchReports from '../components/ResearchReports'
import SwotAnalysis from '../components/SwotAnalysis'

interface Message {
  id: number
  type: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
  metadata?: any
}

interface Insight {
  id: number
  category: string
  title: string
  description: string
  confidence_score: number
  is_validated: boolean
}

interface Option {
  id: number
  category: string
  title: string
  description: string
  pros: string[]
  cons: string[]
  feasibility_score: number
  impact_score: number
  risk_score: number
  recommended: boolean
}

interface ResearchSession {
  id: number
  title: string
  description: string
  status: string
  created_at: string
  conversations: Message[]
  insights: Insight[]
  options: Option[]
}

const REPORT_TYPES = [
  { value: 'executive_summary', label: 'Executive Summary' },
  { value: 'financial_analysis', label: 'Financial Analysis' },
  { value: 'target_market_analysis', label: 'Target Market Analysis' },
  { value: 'options', label: 'Options' },
  { value: 'research_details', label: 'Research Details' },
  { value: 'research_overview', label: 'Research Overview' },
]

const ResearchPage: React.FC = () => {
  const { sessionId } = useParams<{ sessionId: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [session, setSession] = useState<ResearchSession | null>(null)
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState('chat')
  const [message, setMessage] = useState('')
  const [sending, setSending] = useState(false)
  const [messages, setMessages] = useState<Message[]>([])
  const [insights, setInsights] = useState<Insight[]>([])
  const [options, setOptions] = useState<Option[]>([])
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const [showDropdown, setShowDropdown] = useState(false)
  const [selectedReportType, setSelectedReportType] = useState(REPORT_TYPES[0].value)
  const [downloading, setDownloading] = useState(false)
  const downloadLinkRef = useRef<HTMLAnchorElement>(null)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const [selectedSwotOption, setSelectedSwotOption] = useState<{ id: number; title: string } | null>(null)
  const [showSwotModal, setShowSwotModal] = useState(false)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  useEffect(() => {
    if (sessionId) {
      loadSession()
    }
  }, [sessionId])

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowDropdown(false)
      }
    }
    if (showDropdown) {
      document.addEventListener('mousedown', handleClickOutside)
    } else {
      document.removeEventListener('mousedown', handleClickOutside)
    }
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [showDropdown])

  const loadSession = async () => {
    try {
      setLoading(true)
      
      // Fetch actual session data from API
      const response = await fetch(`/api/v1/research/sessions/${sessionId}`)
      
      if (!response.ok) {
        throw new Error(`Failed to load research session: ${response.status} ${response.statusText}`)
      }
      
      const sessionData = await response.json()
      
      // Transform the data to match our interface
      const transformedSession: ResearchSession = {
        id: sessionData.id,
        title: sessionData.title,
        description: sessionData.description,
        status: sessionData.status,
        created_at: sessionData.created_at,
        conversations: sessionData.conversations?.map((conv: any) => ({
          id: conv.id,
          type: conv.message_type,
          content: conv.content,
          timestamp: new Date(conv.created_at),
          metadata: conv.message_metadata
        })) || [],
        insights: sessionData.insights || [],
        options: sessionData.options || []
      }
      
      setSession(transformedSession)
      setMessages(transformedSession.conversations)
      setInsights(transformedSession.insights)
      setOptions(transformedSession.options)
      
      // Add welcome message if no conversations exist
      if (transformedSession.conversations.length === 0) {
        const welcomeMessage: Message = {
          id: 1,
          type: 'system',
          content: 'Welcome to your research session! I\'m here to help you brainstorm and develop your idea. What would you like to explore?',
          timestamp: new Date()
        }
        setMessages([welcomeMessage])
      }
      
      setLoading(false)
    } catch (error) {
      console.error('Failed to load session:', error)
      toast.error('Failed to load research session')
      setLoading(false)
      setSession(null)
    }
  }

  const sendMessage = async () => {
    if (!message.trim() || sending) return

    setSending(true)
    const userMessage: Message = {
      id: Date.now(),
      type: 'user',
      content: message.trim(),
      timestamp: new Date()
    }

    setMessages(prev => [...prev, userMessage])
    setMessage('')

    try {
      // Call the brainstorming API
      const response = await fetch(`/api/v1/research/sessions/${sessionId}/brainstorm`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          message: message.trim(),
          context: {
            insights_count: insights.length,
            options_count: options.length
          }
        })
      })
      
      if (!response.ok) {
        throw new Error('Failed to get AI response')
      }
      
      const brainstormResponse = await response.json()
      
      const aiResponse: Message = {
        id: Date.now() + 1,
        type: 'assistant',
        content: brainstormResponse.message,
        timestamp: new Date(),
        metadata: brainstormResponse.metadata
      }
      
      setMessages(prev => [...prev, aiResponse])
      
      // Update insights and options if returned
      if (brainstormResponse.insights && brainstormResponse.insights.length > 0) {
        setInsights(prev => [...prev, ...brainstormResponse.insights])
      }
      
      if (brainstormResponse.options && brainstormResponse.options.length > 0) {
        setOptions(prev => [...prev, ...brainstormResponse.options])
      }
      
      setSending(false)
    } catch (error) {
      console.error('Failed to get AI response:', error)
      toast.error('Failed to get AI response')
      setSending(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      sendMessage()
    }
  }

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'target_market':
        return '🎯'
      case 'customer_profile':
        return '👥'
      case 'problem_solution':
        return '💡'
      case 'growth_targets':
        return '📈'
      case 'cost_model':
        return '💰'
      case 'revenue_model':
        return '💵'
      default:
        return '📊'
    }
  }

  const getScoreColor = (score: number) => {
    if (score >= 0.7) return 'text-green-600'
    if (score >= 0.4) return 'text-yellow-600'
    return 'text-red-600'
  }

  const handleDropdownSelect = async (type: string) => {
    setShowDropdown(false)
    setSelectedReportType(type)
    await generateReport(type)
  }

  const generateReport = async (type: string) => {
    setDownloading(true)
    try {
      const response = await fetch(`/api/v1/research/sessions/${sessionId}/reports`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ report_type: type })
      })
      if (!response.ok) throw new Error('Failed to generate report')
      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      if (downloadLinkRef.current) {
        downloadLinkRef.current.href = url
        downloadLinkRef.current.download = `${type}_report.pdf`
        downloadLinkRef.current.click()
        setTimeout(() => window.URL.revokeObjectURL(url), 1000)
      }
    } catch (error) {
      console.error('Failed to generate report:', error)
      alert('Failed to generate report')
    } finally {
      setDownloading(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="flex items-center space-x-2">
          <ArrowPathIcon className="w-6 h-6 animate-spin text-primary-600" />
          <span className="text-gray-600">Loading research session...</span>
        </div>
      </div>
    )
  }

  if (!session) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Session not found</h2>
          <button 
            onClick={() => navigate('/dashboard')}
            className="btn-primary"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div>
              <h1 className="text-xl font-semibold text-gray-900">{session.title}</h1>
              <p className="text-sm text-gray-500">{session.description}</p>
            </div>
            <div className="flex items-center space-x-3">
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                {session.status}
              </span>
              <button 
                onClick={() => navigate('/dashboard')}
                className="btn-secondary"
              >
                Back to Dashboard
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Main Chat Area */}
          <div className="lg:col-span-3">
            <div className="bg-white rounded-lg shadow-sm border h-[600px] flex flex-col">
              {/* Tab Navigation */}
              <div className="flex border-b">
                <button
                  onClick={() => setActiveTab('chat')}
                  className={`flex items-center px-4 py-3 text-sm font-medium border-b-2 ${
                    activeTab === 'chat' 
                      ? 'border-primary-500 text-primary-600' 
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <ChatBubbleLeftRightIcon className="w-4 h-4 mr-2" />
                  Brainstorm
                </button>
                <button
                  onClick={() => setActiveTab('insights')}
                  className={`flex items-center px-4 py-3 text-sm font-medium border-b-2 ${
                    activeTab === 'insights' 
                      ? 'border-primary-500 text-primary-600' 
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <LightBulbIcon className="w-4 h-4 mr-2" />
                  Insights ({session.insights.length})
                </button>
                <button
                  onClick={() => setActiveTab('options')}
                  className={`flex items-center px-4 py-3 text-sm font-medium border-b-2 ${
                    activeTab === 'options' 
                      ? 'border-primary-500 text-primary-600' 
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <ChartBarIcon className="w-4 h-4 mr-2" />
                  Options ({session.options.length})
                </button>
                <button
                  onClick={() => setActiveTab('reports')}
                  className={`flex items-center px-4 py-3 text-sm font-medium border-b-2 ${
                    activeTab === 'reports' 
                      ? 'border-primary-500 text-primary-600' 
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <DocumentTextIcon className="w-4 h-4 mr-2" />
                  Reports
                </button>
              </div>

              {/* Tab Content */}
              <div className="flex-1 overflow-hidden">
                {activeTab === 'chat' && (
                  <div className="h-full flex flex-col">
                    {/* Messages */}
                    <div className="flex-1 overflow-y-auto p-4 space-y-4">
                      {messages.map((msg) => (
                        <div
                          key={msg.id}
                          className={`flex ${msg.type === 'user' ? 'justify-end' : 'justify-start'}`}
                        >
                          <div
                            className={`max-w-3xl p-3 rounded-lg ${
                              msg.type === 'user'
                                ? 'bg-primary-600 text-white'
                                : msg.type === 'system'
                                ? 'bg-yellow-50 text-yellow-800 border border-yellow-200'
                                : 'bg-gray-100 text-gray-900'
                            }`}
                          >
                            <p className="whitespace-pre-wrap">{msg.content}</p>
                            {msg.metadata?.follow_up_questions && (
                              <div className="mt-3 pt-3 border-t border-gray-200">
                                <p className="text-sm font-medium mb-2">Follow-up questions:</p>
                                <ul className="text-sm space-y-1">
                                  {msg.metadata.follow_up_questions.map((question: string, idx: number) => (
                                    <li key={idx} className="text-gray-600">• {question}</li>
                                  ))}
                                </ul>
                              </div>
                            )}
                          </div>
                        </div>
                      ))}
                      {sending && (
                        <div className="flex justify-start">
                          <div className="bg-gray-100 text-gray-900 p-3 rounded-lg">
                            <div className="flex items-center space-x-2">
                              <ArrowPathIcon className="w-4 h-4 animate-spin" />
                              <span>AI is thinking...</span>
                            </div>
                          </div>
                        </div>
                      )}
                      <div ref={messagesEndRef} />
                    </div>

                    {/* Message Input */}
                    <div className="border-t p-4">
                      <div className="flex space-x-3">
                        <textarea
                          value={message}
                          onChange={(e) => setMessage(e.target.value)}
                          onKeyDown={handleKeyPress}
                          placeholder="Share your thoughts, ask questions, or explore new directions..."
                          className="flex-1 resize-none border-gray-300 rounded-lg focus:ring-primary-500 focus:border-primary-500"
                          rows={2}
                        />
                        <button
                          onClick={sendMessage}
                          disabled={!message.trim() || sending}
                          className="btn-primary flex items-center px-4"
                        >
                          <PaperAirplaneIcon className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                {activeTab === 'insights' && (
                  <div className="p-4 h-full overflow-y-auto">
                    <div className="space-y-4">
                      {session.insights.map((insight) => (
                        <div key={insight.id} className="border rounded-lg p-4">
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <div className="flex items-center space-x-2 mb-2">
                                <span className="text-lg">{getCategoryIcon(insight.category)}</span>
                                <span className="text-xs uppercase tracking-wide text-gray-500 font-medium">
                                  {insight.category.replace('_', ' ')}
                                </span>
                                <span className={`text-sm font-medium ${getScoreColor(insight.confidence_score)}`}>
                                  {Math.round(insight.confidence_score * 100)}% confidence
                                </span>
                              </div>
                              <h3 className="font-medium text-gray-900 mb-1">{insight.title}</h3>
                              <p className="text-gray-600 text-sm">{insight.description}</p>
                            </div>
                            <div className="flex items-center space-x-2 ml-4">
                              {insight.is_validated ? (
                                <CheckBadgeIcon className="w-5 h-5 text-green-500" />
                              ) : (
                                <button className="text-sm text-primary-600 hover:text-primary-700">
                                  Validate
                                </button>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {activeTab === 'options' && (
                  <div className="p-4 h-full overflow-y-auto">
                    <div className="space-y-4">
                      {session.options.map((option) => (
                        <div key={option.id} className="border rounded-lg p-4">
                          <div className="flex items-start justify-between mb-3">
                            <div>
                              <div className="flex items-center space-x-2 mb-1">
                                <span className="text-xs uppercase tracking-wide text-gray-500 font-medium">
                                  {option.category.replace('_', ' ')}
                                </span>
                                {option.recommended && (
                                  <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-green-100 text-green-800">
                                    Recommended
                                  </span>
                                )}
                              </div>
                              <h3 className="font-medium text-gray-900">{option.title}</h3>
                              <p className="text-gray-600 text-sm mt-1">{option.description}</p>
                            </div>
                          </div>
                          
                          <div className="grid grid-cols-3 gap-4 mb-3">
                            <div className="text-center">
                              <div className={`text-lg font-semibold ${getScoreColor(option.feasibility_score)}`}>
                                {Math.round(option.feasibility_score * 100)}%
                              </div>
                              <div className="text-xs text-gray-500">Feasibility</div>
                            </div>
                            <div className="text-center">
                              <div className={`text-lg font-semibold ${getScoreColor(option.impact_score)}`}>
                                {Math.round(option.impact_score * 100)}%
                              </div>
                              <div className="text-xs text-gray-500">Impact</div>
                            </div>
                            <div className="text-center">
                              <div className={`text-lg font-semibold ${getScoreColor(1 - option.risk_score)}`}>
                                {Math.round((1 - option.risk_score) * 100)}%
                              </div>
                              <div className="text-xs text-gray-500">Low Risk</div>
                            </div>
                          </div>

                          <div className="grid grid-cols-2 gap-4 mb-4">
                            <div>
                              <h4 className="text-sm font-medium text-green-700 mb-2">Pros</h4>
                              <ul className="text-sm text-gray-600 space-y-1">
                                {option.pros.map((pro, idx) => (
                                  <li key={idx}>• {pro}</li>
                                ))}
                              </ul>
                            </div>
                            <div>
                              <h4 className="text-sm font-medium text-red-700 mb-2">Cons</h4>
                              <ul className="text-sm text-gray-600 space-y-1">
                                {option.cons.map((con, idx) => (
                                  <li key={idx}>• {con}</li>
                                ))}
                              </ul>
                            </div>
                          </div>
                          
                          <div className="flex justify-end">
                            <button
                              onClick={() => {
                                setSelectedSwotOption({ id: option.id, title: option.title });
                                setShowSwotModal(true);
                              }}
                              className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors flex items-center space-x-2"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                              </svg>
                              <span>View SWOT Analysis</span>
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {activeTab === 'reports' && (
                  <div className="p-4 h-full overflow-y-auto">
                    <ResearchReports sessionId={sessionId || '1'} />
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Quick Stats */}
            <div className="bg-white rounded-lg shadow-sm border p-4">
              <h3 className="font-medium text-gray-900 mb-3">Research Progress</h3>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-sm text-gray-600">Insights</span>
                  <span className="text-sm font-medium">{session.insights.length}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-gray-600">Options</span>
                  <span className="text-sm font-medium">{session.options.length}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-gray-600">Validated</span>
                  <span className="text-sm font-medium">
                    {session.insights.filter(i => i.is_validated).length}
                  </span>
                </div>
              </div>
            </div>

            {/* Quick Actions */}
            <div className="bg-white rounded-lg shadow-sm border p-4">
              <h3 className="font-medium text-gray-900 mb-3">Quick Actions</h3>
              <div className="space-y-2">
                <div className="relative" ref={dropdownRef}>
                  <button
                    className="w-full btn-secondary flex items-center justify-between"
                    onClick={() => setShowDropdown((v) => !v)}
                    disabled={downloading}
                    type="button"
                  >
                    <span className="flex items-center">
                      <DocumentTextIcon className="w-4 h-4 mr-2" />
                      {downloading ? 'Generating...' : REPORT_TYPES.find(rt => rt.value === selectedReportType)?.label || 'Generate Report'}
                    </span>
                    <svg className="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" /></svg>
                  </button>
                  {showDropdown && (
                    <div className="absolute z-10 mt-2 w-full bg-white border border-gray-200 rounded shadow-lg">
                      {REPORT_TYPES.map(rt => (
                        <button
                          key={rt.value}
                          className={`block w-full text-left px-4 py-2 text-sm hover:bg-primary-50 ${selectedReportType === rt.value ? 'bg-primary-100' : ''}`}
                          onClick={() => handleDropdownSelect(rt.value)}
                          disabled={downloading}
                        >
                          {rt.label}
                        </button>
                      ))}
                    </div>
                  )}
                  <a ref={downloadLinkRef} style={{ display: 'none' }}>Download</a>
                </div>
                <button className="w-full btn-secondary text-left flex items-center">
                  <ChartBarIcon className="w-4 h-4 mr-2" />
                  View Analytics
                </button>
                <button className="w-full btn-secondary text-left flex items-center">
                  <CheckBadgeIcon className="w-4 h-4 mr-2" />
                  Fact Check All
                </button>
              </div>
            </div>

            {/* Next Steps */}
            <div className="bg-white rounded-lg shadow-sm border p-4">
              <h3 className="font-medium text-gray-900 mb-3">Suggested Next Steps</h3>
              <ul className="text-sm text-gray-600 space-y-2">
                <li>• Validate target market assumptions</li>
                <li>• Research competitive landscape</li>
                <li>• Define MVP features</li>
                <li>• Create financial projections</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      
      {/* SWOT Analysis Modal */}
      {showSwotModal && selectedSwotOption && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg max-w-4xl w-full max-h-[90vh] overflow-y-auto">
            <SwotAnalysis
              optionId={selectedSwotOption.id}
              optionTitle={selectedSwotOption.title}
              onClose={() => {
                setShowSwotModal(false);
                setSelectedSwotOption(null);
              }}
            />
          </div>
        </div>
      )}
    </div>
  )
}

export default ResearchPage