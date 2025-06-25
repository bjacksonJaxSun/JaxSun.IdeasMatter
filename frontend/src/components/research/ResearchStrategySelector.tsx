import React, { useState, useEffect } from 'react';
import { 
  ClockIcon, 
  ArrowTrendingUpIcon, 
  AcademicCapIcon,
  CheckCircleIcon, 
  ArrowRightIcon,
  LightBulbIcon,
  ChartBarIcon,
  RocketLaunchIcon,
  UsersIcon,
  CurrencyDollarIcon,
  ShieldCheckIcon
} from '@heroicons/react/24/outline';

interface ResearchApproach {
  approach: 'quick_validation' | 'market_deep_dive' | 'launch_strategy';
  title: string;
  description: string;
  duration_minutes: number;
  complexity: 'beginner' | 'intermediate' | 'advanced';
  best_for: string[];
  includes: string[];
  deliverables: string[];
}

interface ResearchStrategySelectorProps {
  sessionId: number;
  ideaTitle: string;
  ideaDescription: string;
  onStrategySelected: (approach: ResearchApproach, strategyId: number) => void;
  isLoading?: boolean;
}

const ResearchStrategySelector: React.FC<ResearchStrategySelectorProps> = ({
  sessionId,
  ideaTitle,
  ideaDescription,
  onStrategySelected,
  isLoading = false
}) => {
  const [approaches, setApproaches] = useState<ResearchApproach[]>([]);
  const [selectedApproach, setSelectedApproach] = useState<ResearchApproach | null>(null);
  const [isInitiating, setIsInitiating] = useState(false);
  const [loadingApproaches, setLoadingApproaches] = useState(true);

  useEffect(() => {
    fetchApproaches();
  }, []);

  const fetchApproaches = async () => {
    try {
      const response = await fetch('/api/v1/research-strategy/approaches');
      
      if (!response.ok) {
        throw new Error(`Failed to fetch research approaches: ${response.status}`);
      }
      
      const data = await response.json();
      setApproaches(data);
    } catch (error) {
      console.error('Failed to fetch research approaches:', error);
      // Show error to user instead of silently falling back
      throw error;
    } finally {
      setLoadingApproaches(false);
    }
  };

  const getApproachIcon = (approach: string) => {
    switch (approach) {
      case 'quick_validation':
        return <LightBulbIcon className="w-8 h-8" />;
      case 'market_deep_dive':
        return <ChartBarIcon className="w-8 h-8" />;
      case 'launch_strategy':
        return <RocketLaunchIcon className="w-8 h-8" />;
      default:
        return <AcademicCapIcon className="w-8 h-8" />;
    }
  };

  const getComplexityColor = (complexity: string) => {
    switch (complexity) {
      case 'beginner':
        return 'text-green-600 bg-green-100';
      case 'intermediate':
        return 'text-blue-600 bg-blue-100';
      case 'advanced':
        return 'text-purple-600 bg-purple-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  };

  const getDurationColor = (minutes: number) => {
    if (minutes <= 20) return 'text-green-600';
    if (minutes <= 60) return 'text-blue-600';
    return 'text-purple-600';
  };

  const handleApproachSelect = (approach: ResearchApproach) => {
    setSelectedApproach(approach);
  };

  const handleInitiateResearch = async () => {
    if (!selectedApproach) return;

    setIsInitiating(true);
    try {
      const response = await fetch('/api/v1/research-strategy/initiate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          session_id: sessionId,
          approach: selectedApproach.approach,
          custom_parameters: {
            idea_title: ideaTitle,
            idea_description: ideaDescription
          }
        }),
      });

      if (!response.ok) {
        throw new Error(`Failed to initiate research strategy: ${response.status}`);
      }

      const result = await response.json();
      onStrategySelected(selectedApproach, result.strategy.id);
    } catch (error) {
      console.error('Failed to initiate research:', error);
      // Handle error appropriately - could show toast notification
      alert(`Error: ${error instanceof Error ? error.message : 'Could not initiate research strategy'}`);
    } finally {
      setIsInitiating(false);
    }
  };

  if (loadingApproaches) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading research approaches...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center space-y-2">
        <h2 className="text-2xl font-bold text-gray-900">Choose Your Research Approach</h2>
        <p className="text-gray-600 max-w-2xl mx-auto">
          Select the research strategy that best fits your needs and timeline. Each approach provides 
          progressively deeper insights with tailored recommendations.
        </p>
      </div>

      {/* Idea Context */}
      <div className="bg-gray-50 rounded-lg p-4 border-l-4 border-blue-500">
        <h3 className="font-semibold text-gray-900 mb-1">Your Idea</h3>
        <p className="text-gray-700 font-medium">{ideaTitle}</p>
        {ideaDescription && (
          <p className="text-gray-600 text-sm mt-1">{ideaDescription}</p>
        )}
      </div>

      {/* Approach Cards */}
      <div className="grid md:grid-cols-1 lg:grid-cols-3 gap-6">
        {approaches.map((approach) => (
          <div
            key={approach.approach}
            className={`relative cursor-pointer transition-all duration-200 ${
              selectedApproach?.approach === approach.approach
                ? 'ring-2 ring-blue-500 shadow-lg scale-105'
                : 'hover:shadow-md hover:scale-102'
            }`}
            onClick={() => handleApproachSelect(approach)}
          >
            <div className="bg-white rounded-lg border border-gray-200 p-6 h-full">
              {/* Header */}
              <div className="flex items-start justify-between mb-4">
                <div className="flex items-center space-x-3">
                  <div className="text-blue-600">
                    {getApproachIcon(approach.approach)}
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-900">
                      {approach.title}
                    </h3>
                    <p className="text-gray-600 text-sm">
                      {approach.description}
                    </p>
                  </div>
                </div>
                {selectedApproach?.approach === approach.approach && (
                  <CheckCircleIcon className="w-6 h-6 text-green-500 flex-shrink-0" />
                )}
              </div>

              {/* Metadata */}
              <div className="flex items-center space-x-4 mb-4">
                <div className="flex items-center space-x-1">
                  <ClockIcon className="w-4 h-4 text-gray-400" />
                  <span className={`text-sm font-medium ${getDurationColor(approach.duration_minutes)}`}>
                    {approach.duration_minutes} min
                  </span>
                </div>
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${getComplexityColor(approach.complexity)}`}>
                  {approach.complexity}
                </span>
              </div>

              {/* Best For */}
              <div className="mb-4">
                <h4 className="text-sm font-semibold text-gray-900 mb-2">Best for:</h4>
                <ul className="space-y-1">
                  {approach.best_for.map((item, index) => (
                    <li key={index} className="text-sm text-gray-600 flex items-start">
                      <div className="w-1.5 h-1.5 bg-blue-500 rounded-full mt-2 mr-2 flex-shrink-0"></div>
                      {item}
                    </li>
                  ))}
                </ul>
              </div>

              {/* Includes */}
              <div className="mb-4">
                <h4 className="text-sm font-semibold text-gray-900 mb-2">Analysis includes:</h4>
                <ul className="space-y-1">
                  {approach.includes.slice(0, 3).map((item, index) => (
                    <li key={index} className="text-sm text-gray-600 flex items-center">
                      <CheckCircleIcon className="w-3 h-3 text-green-500 mr-2 flex-shrink-0" />
                      {item}
                    </li>
                  ))}
                  {approach.includes.length > 3 && (
                    <li className="text-sm text-gray-500">
                      +{approach.includes.length - 3} more...
                    </li>
                  )}
                </ul>
              </div>

              {/* Deliverables */}
              <div>
                <h4 className="text-sm font-semibold text-gray-900 mb-2">You'll get:</h4>
                <div className="space-y-1">
                  {approach.deliverables.slice(0, 2).map((item, index) => (
                    <div key={index} className="text-sm text-gray-600 flex items-center">
                      <AcademicCapIcon className="w-3 h-3 text-blue-500 mr-2 flex-shrink-0" />
                      {item}
                    </div>
                  ))}
                  {approach.deliverables.length > 2 && (
                    <div className="text-sm text-gray-500">
                      +{approach.deliverables.length - 2} more deliverables
                    </div>
                  )}
                </div>
              </div>

              {/* Selection Indicator */}
              {selectedApproach?.approach === approach.approach && (
                <div className="absolute inset-0 bg-blue-50 bg-opacity-50 rounded-lg pointer-events-none"></div>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Selection Summary & Action */}
      {selectedApproach && (
        <div className="bg-blue-50 rounded-lg p-6 border border-blue-200">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-blue-900 mb-2">
                Ready to start: {selectedApproach.title}
              </h3>
              <div className="grid md:grid-cols-2 gap-4 mb-4">
                <div className="flex items-center space-x-2">
                  <ClockIcon className="w-4 h-4 text-blue-600" />
                  <span className="text-sm text-blue-800">
                    Estimated time: {selectedApproach.duration_minutes} minutes
                  </span>
                </div>
                <div className="flex items-center space-x-2">
                  <ArrowTrendingUpIcon className="w-4 h-4 text-blue-600" />
                  <span className="text-sm text-blue-800">
                    Complexity: {selectedApproach.complexity}
                  </span>
                </div>
              </div>
              <div className="grid md:grid-cols-3 gap-4 text-sm">
                <div>
                  <span className="font-medium text-blue-900">Analysis phases:</span>
                  <div className="text-blue-700 mt-1">
                    {selectedApproach.includes.length} comprehensive phases
                  </div>
                </div>
                <div>
                  <span className="font-medium text-blue-900">Strategic options:</span>
                  <div className="text-blue-700 mt-1">
                    {selectedApproach.approach === 'quick_validation' ? '2' : 
                     selectedApproach.approach === 'market_deep_dive' ? '3' : '5'} options with scoring
                  </div>
                </div>
                <div>
                  <span className="font-medium text-blue-900">Deliverables:</span>
                  <div className="text-blue-700 mt-1">
                    {selectedApproach.deliverables.length} detailed reports
                  </div>
                </div>
              </div>
            </div>
            <button
              onClick={handleInitiateResearch}
              disabled={isInitiating || isLoading}
              className={`ml-6 flex items-center space-x-2 px-6 py-3 rounded-lg font-medium transition-colors ${
                isInitiating || isLoading
                  ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  : 'bg-blue-600 text-white hover:bg-blue-700'
              }`}
            >
              {isInitiating ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  <span>Starting...</span>
                </>
              ) : (
                <>
                  <span>Start Research</span>
                  <ArrowRightIcon className="w-4 h-4" />
                </>
              )}
            </button>
          </div>
        </div>
      )}

      {/* Help Text */}
      <div className="text-center text-sm text-gray-500">
        <p>
          Not sure which approach to choose? Start with <strong>Quick Validation</strong> to test 
          your core assumptions, then upgrade to deeper analysis if needed.
        </p>
      </div>
    </div>
  );
};

export default ResearchStrategySelector;