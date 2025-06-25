import React, { useState, useEffect, useRef } from 'react'
import { 
  ChartBarIcon, 
  DocumentTextIcon, 
  ArrowDownTrayIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon
} from '@heroicons/react/24/outline'

interface ReportProps {
  sessionId: string
}

interface AnalysisData {
  categorized_insights: Record<string, any[]>
  all_options: any[]
  recommended_options: any[]
  statistics: {
    insights_by_category: Record<string, { count: number; avg_confidence: number }>
    options_by_category?: Record<string, { count: number; avg_score: number }>
    validated_insights?: number
    total_insights: number
    total_options: number
    avg_confidence: number
    research_completion: number
  }
  readiness_score?: number
}

const REPORT_TYPES = [
  { value: 'executive_summary', label: 'Executive Summary' },
  { value: 'financial_analysis', label: 'Financial Analysis' },
  { value: 'target_market_analysis', label: 'Target Market Analysis' },
  { value: 'options', label: 'Options' },
  { value: 'research_details', label: 'Research Details' },
  { value: 'research_overview', label: 'Research Overview' },
]

const ResearchReports: React.FC<ReportProps> = ({ sessionId }) => {
  const [activeTab, setActiveTab] = useState('overview')
  const [analysisData, setAnalysisData] = useState<AnalysisData | null>(null)
  const [loading, setLoading] = useState(true)
  const [selectedReportType, setSelectedReportType] = useState(REPORT_TYPES[0].value)
  const [downloading, setDownloading] = useState(false)
  const downloadLinkRef = useRef<HTMLAnchorElement>(null)
  const [showDropdown, setShowDropdown] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const [showTargetMarketDetails, setShowTargetMarketDetails] = useState(false)

  useEffect(() => {
    loadAnalysisData()
  }, [sessionId])

  // Close dropdown on outside click
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

  const handleDropdownSelect = async (type: string) => {
    setShowDropdown(false)
    setSelectedReportType(type)
    await generateReport(type)
  }

  const loadAnalysisData = async () => {
    try {
      // Fetch actual analysis data from API
      const response = await fetch(`/api/v1/research/sessions/${sessionId}/analysis`)
      
      if (!response.ok) {
        throw new Error('Failed to load analysis data')
      }
      
      const data = await response.json()
      
      // Transform the API response to match the expected format
      const transformedData: AnalysisData = {
        categorized_insights: data.categorized_insights || {},
        all_options: data.all_options || [],
        recommended_options: data.recommended_options || [],
        statistics: data.statistics || {
          insights_by_category: {},
          total_insights: 0,
          total_options: 0,
          avg_confidence: 0,
          research_completion: 0
        },
        readiness_score: data.statistics?.research_completion || 0
      }
      
      setAnalysisData(transformedData)
      setLoading(false)
    } catch (error) {
      console.error('Failed to load analysis data:', error)
      
      // If API fails, provide empty data structure
      setAnalysisData({
        categorized_insights: {},
        all_options: [],
        recommended_options: [],
        statistics: {
          insights_by_category: {},
          total_insights: 0,
          total_options: 0,
          avg_confidence: 0,
          research_completion: 0
        },
        readiness_score: 0
      })
      
      setLoading(false)
    }
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

  const getCategoryDisplayName = (category: string): string => {
    const names: Record<string, string> = {
      target_markets: 'Target Markets',
      customer_profiles: 'Customer Profiles', 
      problems_solutions: 'Problems & Solutions',
      growth_targets: 'Growth Targets',
      cost_models: 'Cost Models',
      revenue_models: 'Revenue Models'
    }
    return names[category] || category
  }

  const getReadinessLevel = (score: number): { label: string; color: string; icon: any } => {
    if (score >= 0.8) return { label: 'Ready', color: 'text-green-600', icon: CheckCircleIcon }
    if (score >= 0.6) return { label: 'Nearly Ready', color: 'text-yellow-600', icon: ExclamationTriangleIcon }
    return { label: 'Needs Work', color: 'text-red-600', icon: ExclamationTriangleIcon }
  }

  const renderChart = (data: Record<string, { count: number; avg_confidence?: number; avg_score?: number }>, type: 'insights' | 'options') => {
    const maxCount = Math.max(...Object.values(data).map(d => d.count))
    
    return (
      <div className="space-y-3">
        {Object.entries(data).map(([category, stats]) => {
          const percentage = (stats.count / maxCount) * 100
          const score = type === 'insights' ? stats.avg_confidence : stats.avg_score
          
          return (
            <div key={category} className="space-y-1">
              <div className="flex justify-between text-sm">
                <span className="font-medium capitalize">{category.replace('_', ' ')}</span>
                <span className="text-gray-500">
                  {stats.count} items • {score ? `${Math.round(score * 100)}% avg` : 'N/A'}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div 
                  className="bg-primary-600 h-2 rounded-full transition-all duration-500"
                  style={{ width: `${percentage}%` }}
                />
              </div>
            </div>
          )
        })}
      </div>
    )
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading analysis...</div>
      </div>
    )
  }

  if (!analysisData) {
    return (
      <div className="text-center text-gray-500 py-8">
        No analysis data available. Start by adding some insights and options.
      </div>
    )
  }

  const readiness = getReadinessLevel(analysisData.readiness_score || 0)

  return (
    <div className="space-y-6">
      {/* Quick Actions: Generate Report Dropdown */}
      <div className="flex justify-end">
        <div className="relative inline-block" ref={dropdownRef}>
          <button
            className="btn-secondary flex items-center"
            onClick={() => setShowDropdown((v) => !v)}
            disabled={downloading}
            type="button"
          >
            <DocumentTextIcon className="w-4 h-4 mr-2" />
            {downloading ? 'Generating...' : REPORT_TYPES.find(rt => rt.value === selectedReportType)?.label || 'Generate Report'}
            <svg className="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" /></svg>
          </button>
          {showDropdown && (
            <div className="absolute z-10 mt-2 w-56 bg-white border border-gray-200 rounded shadow-lg">
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
      </div>
      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('overview')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'overview'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Overview
          </button>
          <button
            onClick={() => setActiveTab('insights')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'insights'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Insights Analysis
          </button>
          <button
            onClick={() => setActiveTab('options')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'options'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Options Comparison
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Readiness Score */}
          <div className="bg-white p-6 rounded-lg border">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Idea Readiness</h3>
            <div className="flex items-center space-x-3">
              <div className="flex-shrink-0">
                <readiness.icon className={`w-8 h-8 ${readiness.color}`} />
              </div>
              <div>
                <div className={`text-2xl font-bold ${readiness.color}`}>
                  {Math.round((analysisData.readiness_score || 0) * 100)}%
                </div>
                <div className="text-sm text-gray-500">{readiness.label}</div>
              </div>
            </div>
            <div className="mt-4">
              <div className="w-full bg-gray-200 rounded-full h-3">
                <div 
                  className="bg-primary-600 h-3 rounded-full transition-all duration-500"
                  style={{ width: `${(analysisData.readiness_score || 0) * 100}%` }}
                />
              </div>
            </div>
          </div>

          {/* Key Metrics */}
          <div className="bg-white p-6 rounded-lg border">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Key Metrics</h3>
            <div className="grid grid-cols-2 gap-4">
              <div className="text-center">
                <div className="text-2xl font-bold text-primary-600">
                  {Object.values(analysisData.categorized_insights).reduce((sum, items) => sum + items.length, 0)}
                </div>
                <div className="text-sm text-gray-500">Total Insights</div>
              </div>
              <div className="text-center">
                <div className="text-2xl font-bold text-green-600">
                  {analysisData.statistics.validated_insights || 0}
                </div>
                <div className="text-sm text-gray-500">Validated</div>
              </div>
              <div className="text-center">
                <div className="text-2xl font-bold text-blue-600">
                  {analysisData.all_options.length}
                </div>
                <div className="text-sm text-gray-500">Options</div>
              </div>
              <div className="text-center">
                <div className="text-2xl font-bold text-purple-600">
                  {analysisData.recommended_options.length}
                </div>
                <div className="text-sm text-gray-500">Recommended</div>
              </div>
            </div>
          </div>

          {/* Category Breakdown */}
          <div className="lg:col-span-2 bg-white p-6 rounded-lg border">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Insights by Category</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {Object.entries(analysisData.categorized_insights).map(([category, insights]) => (
                <div key={category} className="text-center p-4 border rounded-lg">
                  <div className="text-2xl font-bold text-primary-600">{insights.length}</div>
                  <div className="text-sm text-gray-500">{getCategoryDisplayName(category)}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {activeTab === 'insights' && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-lg border">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Insights Distribution</h3>
            {renderChart(analysisData.statistics.insights_by_category, 'insights')}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {Object.entries(analysisData.categorized_insights).map(([category, insights]) => (
              insights.length > 0 && (
                <div key={category} className="bg-white p-6 rounded-lg border">
                  <div className="flex justify-between items-center mb-3">
                    <h4 className="font-medium text-gray-900">{getCategoryDisplayName(category)}</h4>
                    {category === 'target_market' && (
                      <button
                        onClick={() => setShowTargetMarketDetails(true)}
                        className="text-primary-600 hover:text-primary-800 text-sm font-medium"
                      >
                        View Details →
                      </button>
                    )}
                  </div>
                  <div className="space-y-2">
                    {insights.slice(0, 3).map((insight: any) => (
                      <div key={insight.id} className="text-sm">
                        <div className="font-medium">{insight.title}</div>
                        <div className="text-gray-500">
                          Confidence: {Math.round(insight.confidence_score * 100)}%
                        </div>
                      </div>
                    ))}
                    {insights.length > 3 && (
                      <div className="text-sm text-gray-500">
                        +{insights.length - 3} more insights
                      </div>
                    )}
                  </div>
                </div>
              )
            ))}
          </div>
        </div>
      )}

      {activeTab === 'options' && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-lg border">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Options Performance</h3>
            <div className="space-y-4">
              {analysisData.all_options.map((option: any) => (
                <div key={option.id} className="border rounded-lg p-4">
                  <div className="flex justify-between items-start mb-3">
                    <h4 className="font-medium">{option.title}</h4>
                    {option.recommended && (
                      <span className="bg-green-100 text-green-800 text-xs font-medium px-2 py-1 rounded">
                        Recommended
                      </span>
                    )}
                  </div>
                  <div className="grid grid-cols-3 gap-4">
                    <div>
                      <div className="text-sm text-gray-500">Feasibility</div>
                      <div className="flex items-center">
                        <div className="w-full bg-gray-200 rounded-full h-2 mr-2">
                          <div 
                            className="bg-blue-600 h-2 rounded-full"
                            style={{ width: `${option.feasibility_score * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-medium">{Math.round(option.feasibility_score * 100)}%</span>
                      </div>
                    </div>
                    <div>
                      <div className="text-sm text-gray-500">Impact</div>
                      <div className="flex items-center">
                        <div className="w-full bg-gray-200 rounded-full h-2 mr-2">
                          <div 
                            className="bg-green-600 h-2 rounded-full"
                            style={{ width: `${option.impact_score * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-medium">{Math.round(option.impact_score * 100)}%</span>
                      </div>
                    </div>
                    <div>
                      <div className="text-sm text-gray-500">Risk</div>
                      <div className="flex items-center">
                        <div className="w-full bg-gray-200 rounded-full h-2 mr-2">
                          <div 
                            className="bg-red-600 h-2 rounded-full"
                            style={{ width: `${option.risk_score * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-medium">{Math.round(option.risk_score * 100)}%</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Target Market Details Modal */}
      {showTargetMarketDetails && (
        <>
          <div className="fixed inset-0 bg-black bg-opacity-50 z-40" onClick={() => setShowTargetMarketDetails(false)} />
          <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
              <div className="p-6">
                <div className="flex justify-between items-center mb-6">
                  <h2 className="text-2xl font-bold text-gray-900">Target Market Analysis</h2>
                  <button
                    onClick={() => setShowTargetMarketDetails(false)}
                    className="text-gray-400 hover:text-gray-600"
                  >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                
                <div className="space-y-6">
                  {/* Competitive Analysis Overview */}
                  {analysisData?.categorized_insights.target_market && analysisData.categorized_insights.target_market.length > 0 ? (
                    <>
                      {/* Quick Competitive Overview */}
                      {(() => {
                        const competitorInsight = analysisData.categorized_insights.target_market.find((insight: any) => 
                          insight.subcategory === 'competitors'
                        );
                        
                        if (competitorInsight && competitorInsight.data) {
                          const { direct_competitors = [], indirect_competitors = [], market_leaders = [] } = competitorInsight.data;
                          
                          return (
                            <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-200">
                              <h3 className="text-xl font-semibold text-gray-900 mb-4">🏁 Competitive Landscape Overview</h3>
                              <div className="grid grid-cols-3 gap-4 mb-6">
                                <div className="text-center">
                                  <div className="text-3xl font-bold text-red-600">{direct_competitors.length}</div>
                                  <div className="text-sm text-gray-600">Direct Competitors</div>
                                </div>
                                <div className="text-center">
                                  <div className="text-3xl font-bold text-yellow-600">{indirect_competitors.length}</div>
                                  <div className="text-sm text-gray-600">Indirect Competitors</div>
                                </div>
                                <div className="text-center">
                                  <div className="text-3xl font-bold text-blue-600">{market_leaders.length}</div>
                                  <div className="text-sm text-gray-600">Market Leaders</div>
                                </div>
                              </div>
                              
                              {/* Competitive Threat Level */}
                              <div className="bg-white rounded-lg p-4">
                                <h4 className="font-medium text-gray-900 mb-2">Overall Competitive Intensity</h4>
                                <div className="flex items-center space-x-2">
                                  <div className="flex-1 bg-gray-200 rounded-full h-3">
                                    <div 
                                      className={`h-3 rounded-full ${
                                        direct_competitors.length >= 5 ? 'bg-red-500' :
                                        direct_competitors.length >= 3 ? 'bg-yellow-500' : 'bg-green-500'
                                      }`}
                                      style={{ width: `${Math.min(100, (direct_competitors.length / 5) * 100)}%` }}
                                    />
                                  </div>
                                  <span className={`text-sm font-medium ${
                                    direct_competitors.length >= 5 ? 'text-red-600' :
                                    direct_competitors.length >= 3 ? 'text-yellow-600' : 'text-green-600'
                                  }`}>
                                    {direct_competitors.length >= 5 ? 'High' :
                                     direct_competitors.length >= 3 ? 'Medium' : 'Low'}
                                  </span>
                                </div>
                              </div>
                            </div>
                          );
                        }
                        return null;
                      })()}

                      {/* Detailed Competitor Analysis */}
                      <div className="grid grid-cols-1 gap-6">
                        {analysisData.categorized_insights.target_market.map((insight: any) => (
                          <div key={insight.id} className="border rounded-lg p-6">
                            {insight.subcategory === 'competitors' ? (
                              // Enhanced Competitor Display
                              <div>
                                <div className="flex justify-between items-start mb-6">
                                  <h3 className="text-lg font-semibold text-gray-900">🔍 {insight.title}</h3>
                                  <div className="flex items-center space-x-2">
                                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                                      insight.confidence_score >= 0.8 ? 'bg-green-100 text-green-800' :
                                      insight.confidence_score >= 0.6 ? 'bg-yellow-100 text-yellow-800' :
                                      'bg-red-100 text-red-800'
                                    }`}>
                                      {Math.round(insight.confidence_score * 100)}% Confidence
                                    </span>
                                    {insight.is_validated && (
                                      <CheckCircleIcon className="w-5 h-5 text-green-600" />
                                    )}
                                  </div>
                                </div>
                                
                                <p className="text-gray-700 mb-6">{insight.description}</p>

                                {/* Competitive Analysis Sections */}
                                {insight.data && (
                                  <div className="space-y-6">
                                    {/* Direct Competitors */}
                                    {insight.data.direct_competitors && insight.data.direct_competitors.length > 0 && (
                                      <div className="bg-red-50 rounded-lg p-5 border border-red-200">
                                        <h4 className="font-semibold text-red-800 mb-4 flex items-center">
                                          🎯 Direct Competitors
                                          <span className="ml-2 bg-red-200 text-red-800 text-xs px-2 py-1 rounded-full">
                                            {insight.data.direct_competitors.length}
                                          </span>
                                        </h4>
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                          {insight.data.direct_competitors.map((competitor: any, index: number) => (
                                            <div key={index} className="bg-white rounded-lg p-4 border border-red-200">
                                              <div className="flex justify-between items-start mb-3">
                                                <h5 className="font-medium text-gray-900">{competitor.name || `Competitor ${index + 1}`}</h5>
                                                {competitor.market_share && (
                                                  <span className="bg-red-100 text-red-800 text-xs px-2 py-1 rounded">
                                                    {competitor.market_share}
                                                  </span>
                                                )}
                                              </div>
                                              
                                              {competitor.strengths && competitor.strengths.length > 0 && (
                                                <div className="mb-3">
                                                  <div className="text-xs font-medium text-green-700 mb-1">Strengths:</div>
                                                  <ul className="text-xs text-gray-600 space-y-1">
                                                    {competitor.strengths.map((strength: string, i: number) => (
                                                      <li key={i} className="flex items-start">
                                                        <span className="text-green-500 mr-1">✓</span>
                                                        {strength}
                                                      </li>
                                                    ))}
                                                  </ul>
                                                </div>
                                              )}
                                              
                                              {competitor.weaknesses && competitor.weaknesses.length > 0 && (
                                                <div className="mb-3">
                                                  <div className="text-xs font-medium text-red-700 mb-1">Weaknesses:</div>
                                                  <ul className="text-xs text-gray-600 space-y-1">
                                                    {competitor.weaknesses.map((weakness: string, i: number) => (
                                                      <li key={i} className="flex items-start">
                                                        <span className="text-red-500 mr-1">✗</span>
                                                        {weakness}
                                                      </li>
                                                    ))}
                                                  </ul>
                                                </div>
                                              )}
                                              
                                              {competitor.pricing && (
                                                <div className="text-xs">
                                                  <span className="font-medium text-gray-700">Pricing:</span>
                                                  <span className="text-gray-600 ml-1">{competitor.pricing}</span>
                                                </div>
                                              )}
                                            </div>
                                          ))}
                                        </div>
                                      </div>
                                    )}

                                    {/* Indirect Competitors */}
                                    {insight.data.indirect_competitors && insight.data.indirect_competitors.length > 0 && (
                                      <div className="bg-yellow-50 rounded-lg p-5 border border-yellow-200">
                                        <h4 className="font-semibold text-yellow-800 mb-4 flex items-center">
                                          ⚡ Indirect Competitors
                                          <span className="ml-2 bg-yellow-200 text-yellow-800 text-xs px-2 py-1 rounded-full">
                                            {insight.data.indirect_competitors.length}
                                          </span>
                                        </h4>
                                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                          {insight.data.indirect_competitors.map((competitor: any, index: number) => (
                                            <div key={index} className="bg-white rounded-lg p-4 border border-yellow-200">
                                              <h5 className="font-medium text-gray-900 mb-2">{competitor.name || `Competitor ${index + 1}`}</h5>
                                              {competitor.category && (
                                                <div className="text-xs text-gray-600 mb-2">
                                                  <span className="font-medium">Category:</span> {competitor.category}
                                                </div>
                                              )}
                                              {competitor.threat_level && (
                                                <div className="flex items-center">
                                                  <span className="text-xs font-medium text-gray-700 mr-2">Threat Level:</span>
                                                  <span className={`text-xs px-2 py-1 rounded ${
                                                    competitor.threat_level.toLowerCase() === 'high' ? 'bg-red-100 text-red-800' :
                                                    competitor.threat_level.toLowerCase() === 'medium' ? 'bg-yellow-100 text-yellow-800' :
                                                    'bg-green-100 text-green-800'
                                                  }`}>
                                                    {competitor.threat_level}
                                                  </span>
                                                </div>
                                              )}
                                            </div>
                                          ))}
                                        </div>
                                      </div>
                                    )}

                                    {/* Market Leaders */}
                                    {insight.data.market_leaders && insight.data.market_leaders.length > 0 && (
                                      <div className="bg-blue-50 rounded-lg p-5 border border-blue-200">
                                        <h4 className="font-semibold text-blue-800 mb-4 flex items-center">
                                          👑 Market Leaders
                                          <span className="ml-2 bg-blue-200 text-blue-800 text-xs px-2 py-1 rounded-full">
                                            {insight.data.market_leaders.length}
                                          </span>
                                        </h4>
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                          {insight.data.market_leaders.map((leader: any, index: number) => (
                                            <div key={index} className="bg-white rounded-lg p-4 border border-blue-200">
                                              <h5 className="font-medium text-gray-900 mb-2">{leader.name || `Leader ${index + 1}`}</h5>
                                              {leader.market_position && (
                                                <p className="text-sm text-gray-600 mb-3">{leader.market_position}</p>
                                              )}
                                              {leader.key_advantages && leader.key_advantages.length > 0 && (
                                                <div>
                                                  <div className="text-xs font-medium text-blue-700 mb-1">Key Advantages:</div>
                                                  <ul className="text-xs text-gray-600 space-y-1">
                                                    {leader.key_advantages.map((advantage: string, i: number) => (
                                                      <li key={i} className="flex items-start">
                                                        <span className="text-blue-500 mr-1">★</span>
                                                        {advantage}
                                                      </li>
                                                    ))}
                                                  </ul>
                                                </div>
                                              )}
                                            </div>
                                          ))}
                                        </div>
                                      </div>
                                    )}
                                  </div>
                                )}
                              </div>
                            ) : (
                              // Regular Target Market Insight Display
                              <div>
                                <div className="flex justify-between items-start mb-4">
                                  <h3 className="text-lg font-semibold text-gray-900">{insight.title}</h3>
                                  <div className="flex items-center space-x-2">
                                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                                      insight.confidence_score >= 0.8 ? 'bg-green-100 text-green-800' :
                                      insight.confidence_score >= 0.6 ? 'bg-yellow-100 text-yellow-800' :
                                      'bg-red-100 text-red-800'
                                    }`}>
                                      {Math.round(insight.confidence_score * 100)}% Confidence
                                    </span>
                                    {insight.is_validated && (
                                      <CheckCircleIcon className="w-5 h-5 text-green-600" />
                                    )}
                                  </div>
                                </div>
                                
                                <p className="text-gray-700 mb-4">{insight.description}</p>
                                
                                {/* Additional insight data if available */}
                                {insight.data && typeof insight.data === 'object' && (
                                  <div className="bg-gray-50 rounded-lg p-4">
                                    <h4 className="font-medium text-gray-900 mb-2">Additional Details</h4>
                                    <div className="space-y-2 text-sm text-gray-600">
                                      {Object.entries(insight.data).map(([key, value]) => (
                                        <div key={key} className="flex justify-between">
                                          <span className="font-medium capitalize">{key.replace(/_/g, ' ')}:</span>
                                          <span>{typeof value === 'object' ? JSON.stringify(value) : String(value)}</span>
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                )}
                                
                                {insight.subcategory && (
                                  <div className="mt-3">
                                    <span className="inline-block bg-primary-100 text-primary-800 text-xs px-2 py-1 rounded">
                                      {insight.subcategory.replace(/_/g, ' ')}
                                    </span>
                                  </div>
                                )}
                              </div>
                            )}
                          </div>
                        ))}
                      </div>
                    </>
                  ) : (
                    <div className="text-center py-8 text-gray-500">
                      <ChartBarIcon className="w-12 h-12 mx-auto mb-4 text-gray-400" />
                      <p>No target market insights available yet.</p>
                      <p className="text-sm">Submit your idea for analysis to see detailed target market research.</p>
                    </div>
                  )}
                </div>
                
                <div className="mt-6 flex justify-end">
                  <button
                    onClick={() => setShowTargetMarketDetails(false)}
                    className="btn-secondary"
                  >
                    Close
                  </button>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

export default ResearchReports