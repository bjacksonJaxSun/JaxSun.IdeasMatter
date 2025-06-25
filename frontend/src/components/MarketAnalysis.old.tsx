import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface MarketAnalysisData {
  market_analysis: {
    id: number;
    industry: string;
    market_category: string;
    geographic_scope: string;
    tam_value: number;
    sam_value: number;
    som_value: number;
    cagr: number;
    market_maturity: string;
    customer_segments: any[];
    market_drivers: string[];
    market_barriers: string[];
    confidence_score: number;
    analysis_date: string;
  };
  competitors: Competitor[];
  segments: MarketSegment[];
  trends: any[];
  opportunities: any[];
}

interface Competitor {
  id: number;
  name: string;
  description: string;
  tier: 'direct' | 'indirect' | 'substitute';
  market_share: number;
  threat_level: number;
  strengths: string[];
  weaknesses: string[];
}

interface MarketSegment {
  id: number;
  segment_name: string;
  description: string;
  size_percentage: number;
  attractiveness_score: number;
  priority_level: string;
}

interface MarketAnalysisProps {
  sessionId: number;
  ideaTitle: string;
  onClose?: () => void;
}

const MarketAnalysis: React.FC<MarketAnalysisProps> = ({ sessionId, ideaTitle, onClose }) => {
  const [analysisData, setAnalysisData] = useState<MarketAnalysisData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [generating, setGenerating] = useState(false);
  const [activeTab, setActiveTab] = useState<'overview' | 'competitors' | 'segments' | 'sizing'>('overview');
  const [isDemoMode, setIsDemoMode] = useState(false);

  const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000';

  const fetchMarketAnalysis = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.get(`${API_BASE_URL}/api/v1/market-analysis/${sessionId}`);
      setAnalysisData(response.data);
    } catch (err: any) {
      console.error('Error fetching market analysis:', err);
      console.error('Error response:', err.response?.data);
      console.error('Error status:', err.response?.status);
      
      if (err.response?.status === 404) {
        // Don't show error if no analysis exists yet - this is normal
        setError(null);
        setAnalysisData(null);
      } else {
        const errorMessage = err.response?.data?.detail || 
                            err.response?.data?.message || 
                            'Failed to load market analysis';
        setError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const generateMarketAnalysis = async () => {
    setGenerating(true);
    setError(null);
    try {
      console.log('🚀 Generating market analysis for session:', sessionId);
      console.log('📡 API URL:', `${API_BASE_URL}/api/v1/market-analysis/generate`);
      
      const response = await axios.post(`${API_BASE_URL}/api/v1/market-analysis/generate`, {
        session_id: sessionId,
        analysis_type: 'comprehensive'
      });
      
      console.log('✅ Market analysis generated successfully:', response.data);
      setAnalysisData(response.data);
    } catch (err: any) {
      console.error('❌ Error generating market analysis:', err);
      console.error('📋 Error response:', err.response?.data);
      console.error('🔢 Error status:', err.response?.status);
      console.error('🌐 Network error:', err.code);
      
      let errorMessage = 'Failed to generate market analysis';
      
      if (err.code === 'ERR_NETWORK') {
        errorMessage = 'Cannot connect to server. Using demo data instead.';
        // Use fallback demo data
        generateDemoMarketAnalysis();
        return;
      } else if (err.response?.status === 401) {
        errorMessage = 'Authentication required. Please log in again.';
      } else if (err.response?.status === 404) {
        errorMessage = 'Research session not found or market analysis endpoint not available. Using demo data instead.';
        // Use fallback demo data
        generateDemoMarketAnalysis();
        return;
      } else if (err.response?.status === 500) {
        errorMessage = 'Server error. Using demo data instead.';
        // Use fallback demo data
        generateDemoMarketAnalysis();
        return;
      } else if (err.response?.data?.detail) {
        errorMessage = err.response.data.detail;
      }
      
      setError(errorMessage);
    } finally {
      setGenerating(false);
    }
  };

  const generateDemoMarketAnalysis = () => {
    console.log('📊 Generating demo market analysis data...');
    setIsDemoMode(true);
    setGenerating(false);
    
    // Add some variation to demo data
    const variation = Math.random();
    const tamBase = 10000000000 + (variation * 5000000000); // $10B-15B
    const samBase = tamBase * (0.15 + variation * 0.1); // 15-25% of TAM
    const somBase = samBase * (0.03 + variation * 0.04); // 3-7% of SAM
    
    const demoData: MarketAnalysisData = {
      market_analysis: {
        id: 1,
        industry: "Technology",
        market_category: "Software/Services",
        geographic_scope: "Global",
        tam_value: tamBase,
        sam_value: samBase,
        som_value: somBase,
        cagr: 15 + (variation * 10), // 15-25% CAGR
        market_maturity: "Growth",
        customer_segments: [
          {
            name: "Enterprise Clients",
            size_percentage: 45,
            attractiveness_score: 0.85
          },
          {
            name: "SMB Market",
            size_percentage: 35,
            attractiveness_score: 0.65
          },
          {
            name: "Startups",
            size_percentage: 20,
            attractiveness_score: 0.75
          }
        ],
        market_drivers: [
          "Digital transformation acceleration",
          "AI and automation adoption",
          "Remote work paradigm shift",
          "Customer experience focus",
          "Data-driven decision making"
        ],
        market_barriers: [
          "High customer acquisition costs",
          "Intense competition",
          "Data privacy regulations",
          "Technical complexity",
          "Market saturation in some segments"
        ],
        confidence_score: 0.75,
        analysis_date: new Date().toISOString()
      },
      competitors: [
        {
          id: 1,
          name: "TechCorp Solutions",
          description: "Leading enterprise software provider with comprehensive business solutions",
          tier: "direct" as const,
          market_share: 15.2,
          threat_level: 0.8,
          strengths: [
            "Strong brand recognition",
            "Extensive enterprise client base",
            "Robust feature set",
            "24/7 customer support"
          ],
          weaknesses: [
            "High pricing",
            "Complex implementation",
            "Limited SMB focus",
            "Legacy technology stack"
          ]
        },
        {
          id: 2,
          name: "InnovateTech Inc",
          description: "Fast-growing startup with innovative AI-powered solutions",
          tier: "direct" as const,
          market_share: 8.7,
          threat_level: 0.6,
          strengths: [
            "Cutting-edge AI technology",
            "Agile development",
            "Competitive pricing",
            "Modern user interface"
          ],
          weaknesses: [
            "Limited market presence",
            "Smaller support team",
            "Fewer integrations",
            "Unproven scalability"
          ]
        },
        {
          id: 3,
          name: "Generic Business Tools",
          description: "General productivity suite with business management features",
          tier: "indirect" as const,
          market_share: 12.1,
          threat_level: 0.4,
          strengths: [
            "Wide adoption",
            "Low cost",
            "Easy to use",
            "Multiple business tools"
          ],
          weaknesses: [
            "Lack of specialization",
            "Limited advanced features",
            "Generic approach",
            "Poor customer support"
          ]
        }
      ],
      segments: [
        {
          id: 1,
          segment_name: "Enterprise Clients",
          description: "Large corporations with 1000+ employees requiring comprehensive business solutions",
          size_percentage: 45,
          attractiveness_score: 0.85,
          priority_level: "Primary"
        },
        {
          id: 2,
          segment_name: "SMB Market", 
          description: "Small to medium businesses (50-999 employees) seeking scalable solutions",
          size_percentage: 35,
          attractiveness_score: 0.65,
          priority_level: "Secondary"
        },
        {
          id: 3,
          segment_name: "Startups",
          description: "Early-stage companies looking for cost-effective, flexible solutions",
          size_percentage: 20,
          attractiveness_score: 0.75,
          priority_level: "Secondary"
        }
      ],
      trends: [],
      opportunities: []
    };
    
    setAnalysisData(demoData);
    setError(null); // Clear any errors since we have demo data
    console.log('✅ Demo market analysis generated');
  };

  useEffect(() => {
    fetchMarketAnalysis();
  }, [sessionId]);

  const formatCurrency = (value: number | null | undefined): string => {
    if (!value) return 'N/A';
    if (value >= 1e9) return `$${(value / 1e9).toFixed(1)}B`;
    if (value >= 1e6) return `$${(value / 1e6).toFixed(1)}M`;
    if (value >= 1e3) return `$${(value / 1e3).toFixed(1)}K`;
    return `$${value.toFixed(0)}`;
  };

  const getTierColor = (tier: string): string => {
    switch (tier) {
      case 'direct': return 'bg-red-100 text-red-800';
      case 'indirect': return 'bg-yellow-100 text-yellow-800';
      case 'substitute': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getThreatLevelColor = (level: number): string => {
    if (level >= 0.7) return 'text-red-600';
    if (level >= 0.4) return 'text-yellow-600';
    return 'text-green-600';
  };

  if (loading && !analysisData) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading market analysis...</p>
        </div>
      </div>
    );
  }

  if (!loading && !analysisData) {
    return (
      <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <h3 className="text-lg font-semibold text-gray-900 mb-2">Market Analysis</h3>
        <p className="text-gray-700 mb-4">
          {error || 'No market analysis generated yet. Click below to create one or view a demo.'}
        </p>
        <div className="space-x-3">
          <button
            onClick={generateMarketAnalysis}
            disabled={generating}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors disabled:opacity-50"
          >
            {generating ? 'Generating Analysis...' : 'Generate Market Analysis'}
          </button>
          <button
            onClick={generateDemoMarketAnalysis}
            disabled={generating}
            className="px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-md transition-colors disabled:opacity-50"
          >
            View Demo Analysis
          </button>
        </div>
        {error && (
          <div className="mt-4 text-sm text-red-600">
            <p className="font-medium">Error Details:</p>
            <p>{error}</p>
          </div>
        )}
      </div>
    );
  }

  if (!analysisData) return null;

  const { market_analysis } = analysisData;

  return (
    <div className="bg-white rounded-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">
            Market Analysis
            {isDemoMode && (
              <span className="ml-2 text-sm font-normal text-yellow-600 bg-yellow-100 px-2 py-1 rounded">
                Demo Mode
              </span>
            )}
          </h3>
          <p className="text-sm text-gray-600 mt-1">{ideaTitle}</p>
        </div>
        <div className="flex items-center space-x-2">
          <button
            onClick={isDemoMode ? generateDemoMarketAnalysis : generateMarketAnalysis}
            disabled={generating}
            className="px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors disabled:opacity-50"
          >
            {generating ? 'Regenerating...' : isDemoMode ? 'Refresh Demo' : 'Regenerate'}
          </button>
          {onClose && (
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex space-x-8">
          {['overview', 'competitors', 'segments', 'sizing'].map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab as any)}
              className={`py-2 px-1 border-b-2 font-medium text-sm capitalize ${
                activeTab === tab
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <div className="space-y-6">
          {/* Market Overview */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-2">Industry</h4>
              <p className="text-sm text-gray-600">{market_analysis.industry || 'N/A'}</p>
            </div>
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-2">Market Category</h4>
              <p className="text-sm text-gray-600">{market_analysis.market_category || 'N/A'}</p>
            </div>
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-2">Geographic Scope</h4>
              <p className="text-sm text-gray-600">{market_analysis.geographic_scope || 'N/A'}</p>
            </div>
          </div>

          {/* Market Drivers & Barriers */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="border rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 flex items-center">
                <span className="text-green-600 mr-2">📈</span>
                Market Drivers
              </h4>
              <ul className="space-y-2">
                {market_analysis.market_drivers?.map((driver, idx) => (
                  <li key={idx} className="text-sm text-gray-700 flex items-start">
                    <span className="text-green-600 mr-2">•</span>
                    <span>{driver}</span>
                  </li>
                )) || <li className="text-sm text-gray-500">No drivers identified</li>}
              </ul>
            </div>

            <div className="border rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3 flex items-center">
                <span className="text-red-600 mr-2">🚧</span>
                Market Barriers
              </h4>
              <ul className="space-y-2">
                {market_analysis.market_barriers?.map((barrier, idx) => (
                  <li key={idx} className="text-sm text-gray-700 flex items-start">
                    <span className="text-red-600 mr-2">•</span>
                    <span>{barrier}</span>
                  </li>
                )) || <li className="text-sm text-gray-500">No barriers identified</li>}
              </ul>
            </div>
          </div>

          {/* Market Maturity & Growth */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
              <h4 className="font-medium text-blue-900 mb-2">Market Maturity</h4>
              <p className="text-lg font-semibold text-blue-800">
                {market_analysis.market_maturity || 'N/A'}
              </p>
            </div>
            <div className="bg-green-50 rounded-lg p-4 border border-green-200">
              <h4 className="font-medium text-green-900 mb-2">Growth Rate (CAGR)</h4>
              <p className="text-lg font-semibold text-green-800">
                {market_analysis.cagr ? `${market_analysis.cagr}%` : 'N/A'}
              </p>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'competitors' && (
        <div className="space-y-4">
          {analysisData.competitors.length > 0 ? (
            analysisData.competitors.map((competitor) => (
              <div key={competitor.id} className="border rounded-lg p-4">
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h4 className="font-medium text-gray-900">{competitor.name}</h4>
                    <p className="text-sm text-gray-600 mt-1">{competitor.description}</p>
                  </div>
                  <div className="flex items-center space-x-2">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${getTierColor(competitor.tier)}`}>
                      {competitor.tier}
                    </span>
                    <span className={`text-sm font-medium ${getThreatLevelColor(competitor.threat_level)}`}>
                      {Math.round(competitor.threat_level * 100)}% threat
                    </span>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <h5 className="font-medium text-green-800 mb-2">Strengths</h5>
                    <ul className="space-y-1">
                      {competitor.strengths?.map((strength, idx) => (
                        <li key={idx} className="text-sm text-gray-700 flex items-start">
                          <span className="text-green-600 mr-2">✓</span>
                          <span>{strength}</span>
                        </li>
                      )) || <li className="text-sm text-gray-500">No strengths identified</li>}
                    </ul>
                  </div>

                  <div>
                    <h5 className="font-medium text-red-800 mb-2">Weaknesses</h5>
                    <ul className="space-y-1">
                      {competitor.weaknesses?.map((weakness, idx) => (
                        <li key={idx} className="text-sm text-gray-700 flex items-start">
                          <span className="text-red-600 mr-2">✗</span>
                          <span>{weakness}</span>
                        </li>
                      )) || <li className="text-sm text-gray-500">No weaknesses identified</li>}
                    </ul>
                  </div>
                </div>

                {competitor.market_share && (
                  <div className="mt-4 pt-4 border-t border-gray-200">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-gray-700">Market Share</span>
                      <span className="text-sm font-semibold text-gray-900">
                        {competitor.market_share}%
                      </span>
                    </div>
                  </div>
                )}
              </div>
            ))
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>No competitors identified yet.</p>
            </div>
          )}
        </div>
      )}

      {activeTab === 'segments' && (
        <div className="space-y-4">
          {analysisData.segments.length > 0 ? (
            analysisData.segments.map((segment) => (
              <div key={segment.id} className="border rounded-lg p-4">
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h4 className="font-medium text-gray-900">{segment.segment_name}</h4>
                    <p className="text-sm text-gray-600 mt-1">{segment.description}</p>
                  </div>
                  <div className="text-right">
                    <div className="text-lg font-semibold text-blue-600">
                      {segment.size_percentage}%
                    </div>
                    <div className="text-xs text-gray-500">of market</div>
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <div className="text-xs font-medium text-gray-700 mb-1">Attractiveness</div>
                    <div className="flex items-center">
                      <div className="w-full bg-gray-200 rounded-full h-2 mr-2">
                        <div 
                          className="bg-blue-600 h-2 rounded-full"
                          style={{ width: `${segment.attractiveness_score * 100}%` }}
                        />
                      </div>
                      <span className="text-xs font-medium">
                        {Math.round(segment.attractiveness_score * 100)}%
                      </span>
                    </div>
                  </div>

                  <div className="text-center">
                    <div className="text-xs font-medium text-gray-700 mb-1">Priority</div>
                    <span className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                      segment.priority_level === 'Primary' ? 'bg-green-100 text-green-800' :
                      segment.priority_level === 'Secondary' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {segment.priority_level}
                    </span>
                  </div>

                  <div className="text-right">
                    <div className="text-xs font-medium text-gray-700 mb-1">Size</div>
                    <div className="text-sm font-semibold text-gray-900">
                      {segment.size_percentage}%
                    </div>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>No market segments identified yet.</p>
            </div>
          )}
        </div>
      )}

      {activeTab === 'sizing' && (
        <div className="space-y-6">
          {/* TAM, SAM, SOM */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-blue-50 rounded-lg p-6 border border-blue-200 text-center">
              <h4 className="font-medium text-blue-900 mb-2">TAM</h4>
              <div className="text-2xl font-bold text-blue-800">
                {formatCurrency(market_analysis.tam_value)}
              </div>
              <p className="text-xs text-blue-600 mt-1">Total Addressable Market</p>
            </div>

            <div className="bg-green-50 rounded-lg p-6 border border-green-200 text-center">
              <h4 className="font-medium text-green-900 mb-2">SAM</h4>
              <div className="text-2xl font-bold text-green-800">
                {formatCurrency(market_analysis.sam_value)}
              </div>
              <p className="text-xs text-green-600 mt-1">Serviceable Addressable Market</p>
            </div>

            <div className="bg-purple-50 rounded-lg p-6 border border-purple-200 text-center">
              <h4 className="font-medium text-purple-900 mb-2">SOM</h4>
              <div className="text-2xl font-bold text-purple-800">
                {formatCurrency(market_analysis.som_value)}
              </div>
              <p className="text-xs text-purple-600 mt-1">Serviceable Obtainable Market</p>
            </div>
          </div>

          {/* Market Size Visualization */}
          <div className="border rounded-lg p-6">
            <h4 className="font-medium text-gray-900 mb-4">Market Size Breakdown</h4>
            <div className="space-y-4">
              <div>
                <div className="flex justify-between items-center mb-2">
                  <span className="text-sm font-medium text-gray-700">Total Addressable Market (TAM)</span>
                  <span className="text-sm font-semibold">{formatCurrency(market_analysis.tam_value)}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div className="bg-blue-600 h-3 rounded-full" style={{ width: '100%' }}></div>
                </div>
              </div>

              <div>
                <div className="flex justify-between items-center mb-2">
                  <span className="text-sm font-medium text-gray-700">Serviceable Addressable Market (SAM)</span>
                  <span className="text-sm font-semibold">{formatCurrency(market_analysis.sam_value)}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div 
                    className="bg-green-600 h-3 rounded-full" 
                    style={{ 
                      width: `${market_analysis.sam_value && market_analysis.tam_value 
                        ? (market_analysis.sam_value / market_analysis.tam_value) * 100 
                        : 0}%` 
                    }}
                  ></div>
                </div>
              </div>

              <div>
                <div className="flex justify-between items-center mb-2">
                  <span className="text-sm font-medium text-gray-700">Serviceable Obtainable Market (SOM)</span>
                  <span className="text-sm font-semibold">{formatCurrency(market_analysis.som_value)}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div 
                    className="bg-purple-600 h-3 rounded-full" 
                    style={{ 
                      width: `${market_analysis.som_value && market_analysis.tam_value 
                        ? (market_analysis.som_value / market_analysis.tam_value) * 100 
                        : 0}%` 
                    }}
                  ></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Confidence Score */}
      <div className="mt-6 pt-4 border-t border-gray-200">
        <div className="flex items-center justify-between text-sm">
          <span className="text-gray-600">Analysis Confidence</span>
          <div className="flex items-center">
            <div className="w-32 bg-gray-200 rounded-full h-2 mr-2">
              <div 
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${market_analysis.confidence_score * 100}%` }}
              ></div>
            </div>
            <span className="font-medium text-gray-700">
              {Math.round(market_analysis.confidence_score * 100)}%
            </span>
          </div>
        </div>
        <p className="text-xs text-gray-500 mt-2">
          Generated: {new Date(market_analysis.analysis_date).toLocaleString()}
        </p>
      </div>
    </div>
  );
};

export default MarketAnalysis;