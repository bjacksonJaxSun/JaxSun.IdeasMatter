import React, { useState, useEffect } from 'react';
import { 
  ArrowTrendingUpIcon, 
  CurrencyDollarIcon, 
  ClockIcon, 
  AcademicCapIcon, 
  ShieldCheckIcon, 
  UsersIcon, 
  StarIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  TrophyIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  ArrowRightIcon,
  ChartBarIcon,
  BoltIcon
} from '@heroicons/react/24/outline';

interface ResourceRequirement {
  category: string;
  description: string;
  estimated_cost_usd?: number;
  timeline_months?: number;
  criticality: string;
}

interface SuccessMetric {
  metric_name: string;
  target_value: string | number;
  timeframe: string;
  measurement_method: string;
}

interface SwotAnalysisEnhanced {
  strengths: string[];
  weaknesses: string[];
  opportunities: string[];
  threats: string[];
  strategic_implications: string[];
  critical_success_factors: string[];
  confidence_score: number;
}

interface StrategicOption {
  approach: string;
  title: string;
  description: string;
  target_customer_segment: string;
  value_proposition: string;
  go_to_market_strategy: string;
  estimated_investment_usd?: number;
  timeline_to_market_months: number;
  timeline_to_profitability_months?: number;
  success_probability_percent: number;
  risk_factors: string[];
  mitigation_strategies: string[];
  resource_requirements: ResourceRequirement[];
  success_metrics: SuccessMetric[];
  swot_analysis?: SwotAnalysisEnhanced;
  competitive_positioning: string;
  overall_score: number;
  recommended: boolean;
}

interface StrategicOptionsComparisonProps {
  sessionId: number;
  strategyId: number;
  analysisResults: any;
  onOptionSelected: (option: StrategicOption) => void;
}

const StrategicOptionsComparison: React.FC<StrategicOptionsComparisonProps> = ({
  sessionId,
  strategyId,
  analysisResults,
  onOptionSelected
}) => {
  const [options, setOptions] = useState<StrategicOption[]>([]);
  const [selectedOption, setSelectedOption] = useState<StrategicOption | null>(null);
  const [expandedOptions, setExpandedOptions] = useState<Set<string>>(new Set());
  const [viewMode, setViewMode] = useState<'overview' | 'detailed'>('overview');
  const [comparisonCriteria, setComparisonCriteria] = useState<string[]>([
    'Investment Required',
    'Time to Market',
    'Success Probability',
    'Risk Level',
    'Market Potential'
  ]);

  useEffect(() => {
    if (analysisResults?.strategic_options) {
      setOptions(analysisResults.strategic_options);
      
      // Auto-select recommended option
      const recommended = analysisResults.strategic_options.find((opt: StrategicOption) => opt.recommended);
      if (recommended) {
        setSelectedOption(recommended);
      }
    }
  }, [analysisResults]);

  const toggleOptionExpansion = (optionTitle: string) => {
    const newExpanded = new Set(expandedOptions);
    if (newExpanded.has(optionTitle)) {
      newExpanded.delete(optionTitle);
    } else {
      newExpanded.add(optionTitle);
    }
    setExpandedOptions(newExpanded);
  };

  const getApproachIcon = (approach: string) => {
    switch (approach.toLowerCase()) {
      case 'niche_domination':
        return <AcademicCapIcon className="w-5 h-5" />;
      case 'market_leader_challenge':
        return <ArrowTrendingUpIcon className="w-5 h-5" />;
      case 'platform_play':
        return <ChartBarIcon className="w-5 h-5" />;
      case 'disruptive_innovation':
        return <BoltIcon className="w-5 h-5" />;
      default:
        return <StarIcon className="w-5 h-5" />;
    }
  };

  const getScoreColor = (score: number) => {
    if (score >= 8) return 'text-green-600 bg-green-100';
    if (score >= 6) return 'text-blue-600 bg-blue-100';
    if (score >= 4) return 'text-yellow-600 bg-yellow-100';
    return 'text-red-600 bg-red-100';
  };

  const getRiskLevelColor = (riskCount: number) => {
    if (riskCount <= 2) return 'text-green-600 bg-green-100';
    if (riskCount <= 4) return 'text-yellow-600 bg-yellow-100';
    return 'text-red-600 bg-red-100';
  };

  const getProbabilityColor = (probability: number) => {
    if (probability >= 70) return 'text-green-600';
    if (probability >= 50) return 'text-yellow-600';
    return 'text-red-600';
  };

  const formatCurrency = (amount?: number) => {
    if (!amount) return 'TBD';
    if (amount >= 1000000) return `$${(amount / 1000000).toFixed(1)}M`;
    if (amount >= 1000) return `$${(amount / 1000).toFixed(0)}K`;
    return `$${amount.toLocaleString()}`;
  };

  const formatApproachName = (approach: string) => {
    return approach.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Strategic Options Analysis</h2>
          <p className="text-gray-600 mt-1">
            Compare different strategic approaches for your idea implementation
          </p>
        </div>
        
        <div className="flex items-center space-x-3">
          <div className="flex bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode('overview')}
              className={`px-3 py-1 rounded text-sm font-medium transition-colors ${
                viewMode === 'overview' 
                  ? 'bg-white text-gray-900 shadow-sm' 
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              Overview
            </button>
            <button
              onClick={() => setViewMode('detailed')}
              className={`px-3 py-1 rounded text-sm font-medium transition-colors ${
                viewMode === 'detailed' 
                  ? 'bg-white text-gray-900 shadow-sm' 
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              Detailed
            </button>
          </div>
        </div>
      </div>

      {/* Quick Comparison Table */}
      {viewMode === 'overview' && (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Strategy
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Investment
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Time to Market
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Success Rate
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Risk Level
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Overall Score
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Action
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {options.map((option, index) => (
                  <tr 
                    key={index}
                    className={`hover:bg-gray-50 ${option.recommended ? 'bg-blue-50' : ''}`}
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center space-x-3">
                        {option.recommended && (
                          <TrophyIcon className="w-4 h-4 text-yellow-500" />
                        )}
                        <div className="text-blue-600">
                          {getApproachIcon(option.approach)}
                        </div>
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {option.title}
                          </div>
                          <div className="text-sm text-gray-500">
                            {formatApproachName(option.approach)}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">
                        {formatCurrency(option.estimated_investment_usd)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center space-x-1">
                        <ClockIcon className="w-4 h-4 text-gray-400" />
                        <span className="text-sm text-gray-900">
                          {option.timeline_to_market_months} months
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className={`text-sm font-medium ${getProbabilityColor(option.success_probability_percent)}`}>
                        {option.success_probability_percent}%
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getRiskLevelColor(option.risk_factors.length)}`}>
                        {option.risk_factors.length <= 2 ? 'Low' : 
                         option.risk_factors.length <= 4 ? 'Medium' : 'High'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex px-2 py-1 text-sm font-medium rounded ${getScoreColor(option.overall_score)}`}>
                        {option.overall_score.toFixed(1)}/10
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <button
                        onClick={() => setSelectedOption(option)}
                        className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                      >
                        Select
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Detailed Option Cards */}
      {viewMode === 'detailed' && (
        <div className="space-y-6">
          {options.map((option, index) => (
            <div
              key={index}
              className={`bg-white rounded-lg border transition-all duration-200 ${
                selectedOption?.title === option.title 
                  ? 'border-blue-500 shadow-lg' 
                  : 'border-gray-200 hover:border-gray-300'
              } ${option.recommended ? 'ring-2 ring-yellow-200' : ''}`}
            >
              {/* Card Header */}
              <div className="p-6 border-b border-gray-200">
                <div className="flex items-start justify-between">
                  <div className="flex items-center space-x-4">
                    {option.recommended && (
                      <div className="flex items-center space-x-2 px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm font-medium">
                        <TrophyIcon className="w-4 h-4" />
                        Recommended
                      </div>
                    )}
                    <div className="text-blue-600">
                      {getApproachIcon(option.approach)}
                    </div>
                    <div>
                      <h3 className="text-xl font-semibold text-gray-900">
                        {option.title}
                      </h3>
                      <p className="text-gray-600">{option.description}</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-3">
                    <span className={`px-3 py-1 text-sm font-medium rounded ${getScoreColor(option.overall_score)}`}>
                      Score: {option.overall_score.toFixed(1)}/10
                    </span>
                    <button
                      onClick={() => toggleOptionExpansion(option.title)}
                      className="p-2 text-gray-400 hover:text-gray-600"
                    >
                      {expandedOptions.has(option.title) ? 
                        <ChevronUpIcon className="w-5 h-5" /> : 
                        <ChevronDownIcon className="w-5 h-5" />
                      }
                    </button>
                  </div>
                </div>

                {/* Key Metrics */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-gray-900">
                      {formatCurrency(option.estimated_investment_usd)}
                    </div>
                    <div className="text-sm text-gray-500">Investment</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-gray-900">
                      {option.timeline_to_market_months}mo
                    </div>
                    <div className="text-sm text-gray-500">Time to Market</div>
                  </div>
                  <div className="text-center">
                    <div className={`text-2xl font-bold ${getProbabilityColor(option.success_probability_percent)}`}>
                      {option.success_probability_percent}%
                    </div>
                    <div className="text-sm text-gray-500">Success Rate</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-gray-900">
                      {option.risk_factors.length}
                    </div>
                    <div className="text-sm text-gray-500">Risk Factors</div>
                  </div>
                </div>
              </div>

              {/* Expanded Content */}
              {expandedOptions.has(option.title) && (
                <div className="p-6 space-y-6">
                  {/* Strategy Details */}
                  <div className="grid md:grid-cols-2 gap-6">
                    <div>
                      <h4 className="font-semibold text-gray-900 mb-3">Strategy Overview</h4>
                      <div className="space-y-3 text-sm">
                        <div>
                          <span className="font-medium text-gray-700">Target Segment:</span>
                          <p className="text-gray-600 mt-1">{option.target_customer_segment}</p>
                        </div>
                        <div>
                          <span className="font-medium text-gray-700">Value Proposition:</span>
                          <p className="text-gray-600 mt-1">{option.value_proposition}</p>
                        </div>
                        <div>
                          <span className="font-medium text-gray-700">Go-to-Market:</span>
                          <p className="text-gray-600 mt-1">{option.go_to_market_strategy}</p>
                        </div>
                      </div>
                    </div>

                    <div>
                      <h4 className="font-semibold text-gray-900 mb-3">Risk Assessment</h4>
                      <div className="space-y-3">
                        <div>
                          <span className="text-sm font-medium text-gray-700">Risk Factors:</span>
                          <ul className="mt-1 space-y-1">
                            {option.risk_factors.map((risk, idx) => (
                              <li key={idx} className="flex items-start space-x-2 text-sm">
                                <ExclamationTriangleIcon className="w-4 h-4 text-red-500 mt-0.5 flex-shrink-0" />
                                <span className="text-gray-600">{risk}</span>
                              </li>
                            ))}
                          </ul>
                        </div>
                        <div>
                          <span className="text-sm font-medium text-gray-700">Mitigation Strategies:</span>
                          <ul className="mt-1 space-y-1">
                            {option.mitigation_strategies.map((strategy, idx) => (
                              <li key={idx} className="flex items-start space-x-2 text-sm">
                                <ShieldCheckIcon className="w-4 h-4 text-green-500 mt-0.5 flex-shrink-0" />
                                <span className="text-gray-600">{strategy}</span>
                              </li>
                            ))}
                          </ul>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* SWOT Analysis */}
                  {option.swot_analysis && (
                    <div>
                      <h4 className="font-semibold text-gray-900 mb-3">SWOT Analysis</h4>
                      <div className="grid grid-cols-2 gap-4">
                        <div className="bg-green-50 p-4 rounded-lg">
                          <h5 className="font-medium text-green-900 mb-2">Strengths</h5>
                          <ul className="space-y-1">
                            {option.swot_analysis.strengths.map((item, idx) => (
                              <li key={idx} className="text-sm text-green-800 flex items-start">
                                <CheckCircleIcon className="w-3 h-3 text-green-600 mt-1 mr-2 flex-shrink-0" />
                                {item}
                              </li>
                            ))}
                          </ul>
                        </div>
                        <div className="bg-red-50 p-4 rounded-lg">
                          <h5 className="font-medium text-red-900 mb-2">Weaknesses</h5>
                          <ul className="space-y-1">
                            {option.swot_analysis.weaknesses.map((item, idx) => (
                              <li key={idx} className="text-sm text-red-800 flex items-start">
                                <ExclamationTriangleIcon className="w-3 h-3 text-red-600 mt-1 mr-2 flex-shrink-0" />
                                {item}
                              </li>
                            ))}
                          </ul>
                        </div>
                        <div className="bg-blue-50 p-4 rounded-lg">
                          <h5 className="font-medium text-blue-900 mb-2">Opportunities</h5>
                          <ul className="space-y-1">
                            {option.swot_analysis.opportunities.map((item, idx) => (
                              <li key={idx} className="text-sm text-blue-800 flex items-start">
                                <ArrowTrendingUpIcon className="w-3 h-3 text-blue-600 mt-1 mr-2 flex-shrink-0" />
                                {item}
                              </li>
                            ))}
                          </ul>
                        </div>
                        <div className="bg-yellow-50 p-4 rounded-lg">
                          <h5 className="font-medium text-yellow-900 mb-2">Threats</h5>
                          <ul className="space-y-1">
                            {option.swot_analysis.threats.map((item, idx) => (
                              <li key={idx} className="text-sm text-yellow-800 flex items-start">
                                <ShieldCheckIcon className="w-3 h-3 text-yellow-600 mt-1 mr-2 flex-shrink-0" />
                                {item}
                              </li>
                            ))}
                          </ul>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* Action Button */}
              <div className="px-6 py-4 bg-gray-50 border-t border-gray-200 flex justify-between items-center">
                <div className="text-sm text-gray-600">
                  <span className="font-medium">Positioning:</span> {option.competitive_positioning}
                </div>
                <button
                  onClick={() => {
                    setSelectedOption(option);
                    onOptionSelected(option);
                  }}
                  className={`inline-flex items-center space-x-2 px-4 py-2 rounded-lg font-medium transition-colors ${
                    selectedOption?.title === option.title
                      ? 'bg-blue-600 text-white'
                      : 'bg-white text-blue-600 border border-blue-600 hover:bg-blue-50'
                  }`}
                >
                  <span>{selectedOption?.title === option.title ? 'Selected' : 'Select Strategy'}</span>
                  {selectedOption?.title !== option.title && <ArrowRightIcon className="w-4 h-4" />}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Selected Option Summary */}
      {selectedOption && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
          <div className="flex items-start justify-between">
            <div>
              <h3 className="text-lg font-semibold text-blue-900 mb-2">
                Selected Strategy: {selectedOption.title}
              </h3>
              <p className="text-blue-800 mb-4">{selectedOption.description}</p>
              
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div>
                  <span className="font-medium text-blue-900">Investment:</span>
                  <div className="text-blue-800">{formatCurrency(selectedOption.estimated_investment_usd)}</div>
                </div>
                <div>
                  <span className="font-medium text-blue-900">Timeline:</span>
                  <div className="text-blue-800">{selectedOption.timeline_to_market_months} months</div>
                </div>
                <div>
                  <span className="font-medium text-blue-900">Success Rate:</span>
                  <div className="text-blue-800">{selectedOption.success_probability_percent}%</div>
                </div>
                <div>
                  <span className="font-medium text-blue-900">Overall Score:</span>
                  <div className="text-blue-800">{selectedOption.overall_score.toFixed(1)}/10</div>
                </div>
              </div>
            </div>
            
            <button
              onClick={() => onOptionSelected(selectedOption)}
              className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium"
            >
              Proceed with Strategy
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default StrategicOptionsComparison;