import React, { useState, useEffect } from 'react';
import { 
  CheckCircleIcon, 
  ClockIcon, 
  ArrowTrendingUpIcon, 
  UsersIcon, 
  AcademicCapIcon,
  ChartBarIcon,
  ArrowRightIcon,
  ArrowDownTrayIcon,
  ArrowPathIcon,
  ExclamationTriangleIcon,
  LightBulbIcon,
  GlobeAltIcon,
  ShieldCheckIcon
} from '@heroicons/react/24/outline';

interface AnalysisPhase {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  status: 'pending' | 'in_progress' | 'completed' | 'error';
  estimated_duration?: number;
}

interface ProgressData {
  strategy_id: number;
  current_phase: string;
  progress_percentage: number;
  estimated_completion_minutes?: number;
}

interface GuidedAnalysisFlowProps {
  strategyId: number;
  approach: 'quick_validation' | 'market_deep_dive' | 'launch_strategy';
  onAnalysisComplete: (results: any) => void;
  onError: (error: string) => void;
}

const GuidedAnalysisFlow: React.FC<GuidedAnalysisFlowProps> = ({
  strategyId,
  approach,
  onAnalysisComplete,
  onError
}) => {
  const [phases, setPhases] = useState<AnalysisPhase[]>([]);
  const [currentProgress, setCurrentProgress] = useState<ProgressData | null>(null);
  const [isExecuting, setIsExecuting] = useState(false);
  const [analysisStarted, setAnalysisStarted] = useState(false);
  const [results, setResults] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  // Initialize phases based on approach
  useEffect(() => {
    const phaseConfigs = {
      quick_validation: [
        {
          id: 'market_context',
          title: 'Market Context',
          description: 'Analyzing industry overview and market opportunity',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 3
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Analysis',
          description: 'Identifying competitors and market positioning',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 5
        },
        {
          id: 'strategic_assessment',
          title: 'Strategic Assessment',
          description: 'SWOT analysis and strategic recommendations',
          icon: <AcademicCapIcon className="w-5 h-5" />,
          estimated_duration: 7
        }
      ],
      market_deep_dive: [
        {
          id: 'market_context',
          title: 'Market Context',
          description: 'Deep industry analysis and market sizing',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 8
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Intelligence',
          description: 'Comprehensive competitive landscape mapping',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 12
        },
        {
          id: 'customer_understanding',
          title: 'Customer Understanding',
          description: 'Customer segmentation and persona development',
          icon: <UsersIcon className="w-5 h-5" />,
          estimated_duration: 15
        },
        {
          id: 'strategic_assessment',
          title: 'Strategic Assessment',
          description: 'SWOT analysis and opportunity scoring',
          icon: <AcademicCapIcon className="w-5 h-5" />,
          estimated_duration: 10
        }
      ],
      launch_strategy: [
        {
          id: 'market_context',
          title: 'Market Context',
          description: 'Comprehensive market analysis and trends',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 15
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Intelligence',
          description: 'Detailed competitive analysis and positioning',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 20
        },
        {
          id: 'customer_understanding',
          title: 'Customer Understanding',
          description: 'In-depth customer research and segmentation',
          icon: <UsersIcon className="w-5 h-5" />,
          estimated_duration: 25
        },
        {
          id: 'strategic_assessment',
          title: 'Strategic Assessment',
          description: 'Complete strategic analysis and planning',
          icon: <AcademicCapIcon className="w-5 h-5" />,
          estimated_duration: 20
        }
      ]
    };

    const initialPhases = phaseConfigs[approach].map(phase => ({
      ...phase,
      status: 'pending' as const
    }));

    setPhases(initialPhases);
  }, [approach]);

  // Helper function to get authenticated headers
  const getAuthHeaders = () => {
    const token = localStorage.getItem('access_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` })
    };
  };

  // Start analysis execution
  const startAnalysis = async () => {
    setIsExecuting(true);
    setAnalysisStarted(true);
    setError(null);

    try {
      const response = await fetch(`/api/v1/research-strategy/execute/${strategyId}`, {
        method: 'POST',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        throw new Error('Failed to start analysis');
      }

      // Start polling for progress
      startProgressPolling();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to start analysis';
      setError(errorMessage);
      onError(errorMessage);
      setIsExecuting(false);
    }
  };

  // Poll for progress updates
  const startProgressPolling = () => {
    const pollInterval = setInterval(async () => {
      try {
        const response = await fetch(`/api/v1/research-strategy/progress/${strategyId}`, {
          headers: getAuthHeaders(),
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch progress');
        }

        const progress: ProgressData = await response.json();
        setCurrentProgress(progress);

        // Update phase statuses based on progress
        updatePhaseStatuses(progress);

        // Check if analysis is complete
        if (progress.progress_percentage >= 100) {
          clearInterval(pollInterval);
          await fetchResults();
        }
      } catch (err) {
        console.error('Progress polling error:', err);
        clearInterval(pollInterval);
        setError('Failed to track progress');
      }
    }, 2000); // Poll every 2 seconds

    // Cleanup interval after 10 minutes max
    setTimeout(() => clearInterval(pollInterval), 600000);
  };

  // Update phase statuses based on current progress
  const updatePhaseStatuses = (progress: ProgressData) => {
    setPhases(currentPhases => {
      return currentPhases.map((phase, index) => {
        const phaseProgress = (index + 1) / currentPhases.length * 100;
        
        if (progress.progress_percentage >= phaseProgress) {
          return { ...phase, status: 'completed' };
        } else if (progress.current_phase === phase.id) {
          return { ...phase, status: 'in_progress' };
        } else {
          return { ...phase, status: 'pending' };
        }
      });
    });
  };

  // Fetch final results
  const fetchResults = async () => {
    try {
      const response = await fetch(`/api/v1/research-strategy/results/${strategyId}`, {
        headers: getAuthHeaders(),
      });
      
      if (!response.ok) {
        throw new Error('Failed to fetch results');
      }

      const analysisResults = await response.json();
      setResults(analysisResults);
      onAnalysisComplete(analysisResults);
      setIsExecuting(false);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch results';
      setError(errorMessage);
      onError(errorMessage);
      setIsExecuting(false);
    }
  };

  const getPhaseStatusIcon = (status: string) => {
    switch (status) {
      case 'completed':
        return <CheckCircleIcon className="w-5 h-5 text-green-500" />;
      case 'in_progress':
        return <ArrowPathIcon className="w-5 h-5 text-blue-500 animate-spin" />;
      case 'error':
        return <ExclamationTriangleIcon className="w-5 h-5 text-red-500" />;
      default:
        return <div className="w-5 h-5 rounded-full border-2 border-gray-300" />;
    }
  };

  const getProgressBarColor = (progress: number) => {
    if (progress < 30) return 'bg-red-500';
    if (progress < 70) return 'bg-yellow-500';
    return 'bg-green-500';
  };

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-6">
        <div className="flex items-center space-x-3">
          <ExclamationTriangleIcon className="w-6 h-6 text-red-500" />
          <div>
            <h3 className="text-lg font-semibold text-red-900">Analysis Error</h3>
            <p className="text-red-700">{error}</p>
          </div>
        </div>
        <button
          onClick={() => window.location.reload()}
          className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center space-y-2">
        <h2 className="text-2xl font-bold text-gray-900">AI-Powered Market Research</h2>
        <p className="text-gray-600">
          Our AI is analyzing your idea across multiple dimensions to provide comprehensive insights.
        </p>
      </div>

      {/* Progress Overview */}
      {currentProgress && (
        <div className="bg-blue-50 rounded-lg p-6 border border-blue-200">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-blue-900">Analysis Progress</h3>
            <div className="text-right">
              <div className="text-2xl font-bold text-blue-900">
                {Math.round(currentProgress.progress_percentage)}%
              </div>
              {currentProgress.estimated_completion_minutes && (
                <div className="text-sm text-blue-700">
                  ~{currentProgress.estimated_completion_minutes} min remaining
                </div>
              )}
            </div>
          </div>
          
          <div className="w-full bg-gray-200 rounded-full h-3 mb-4">
            <div
              className={`h-3 rounded-full transition-all duration-500 ${getProgressBarColor(currentProgress.progress_percentage)}`}
              style={{ width: `${currentProgress.progress_percentage}%` }}
            />
          </div>
          
          <div className="text-sm text-blue-800">
            Current phase: <span className="font-medium">
              {phases.find(p => p.id === currentProgress.current_phase)?.title || 'Processing...'}
            </span>
          </div>
        </div>
      )}

      {/* Analysis Phases */}
      <div className="space-y-4">
        <h3 className="text-lg font-semibold text-gray-900">Analysis Phases</h3>
        
        <div className="space-y-3">
          {phases.map((phase, index) => (
            <div
              key={phase.id}
              className={`flex items-center space-x-4 p-4 rounded-lg border transition-all duration-200 ${
                phase.status === 'completed' ? 'bg-green-50 border-green-200' :
                phase.status === 'in_progress' ? 'bg-blue-50 border-blue-200' :
                phase.status === 'error' ? 'bg-red-50 border-red-200' :
                'bg-gray-50 border-gray-200'
              }`}
            >
              {/* Phase Number */}
              <div className={`flex items-center justify-center w-8 h-8 rounded-full text-sm font-medium ${
                phase.status === 'completed' ? 'bg-green-500 text-white' :
                phase.status === 'in_progress' ? 'bg-blue-500 text-white' :
                phase.status === 'error' ? 'bg-red-500 text-white' :
                'bg-gray-300 text-gray-600'
              }`}>
                {index + 1}
              </div>

              {/* Phase Icon */}
              <div className={
                phase.status === 'completed' ? 'text-green-600' :
                phase.status === 'in_progress' ? 'text-blue-600' :
                phase.status === 'error' ? 'text-red-600' :
                'text-gray-400'
              }>
                {phase.icon}
              </div>

              {/* Phase Info */}
              <div className="flex-1">
                <h4 className={`font-semibold ${
                  phase.status === 'completed' ? 'text-green-900' :
                  phase.status === 'in_progress' ? 'text-blue-900' :
                  phase.status === 'error' ? 'text-red-900' :
                  'text-gray-700'
                }`}>
                  {phase.title}
                </h4>
                <p className={`text-sm ${
                  phase.status === 'completed' ? 'text-green-700' :
                  phase.status === 'in_progress' ? 'text-blue-700' :
                  phase.status === 'error' ? 'text-red-700' :
                  'text-gray-600'
                }`}>
                  {phase.description}
                </p>
                {phase.estimated_duration && (
                  <div className="flex items-center space-x-1 mt-1">
                    <ClockIcon className="w-3 h-3 text-gray-400" />
                    <span className="text-xs text-gray-500">
                      ~{phase.estimated_duration} minutes
                    </span>
                  </div>
                )}
              </div>

              {/* Status Icon */}
              <div>
                {getPhaseStatusIcon(phase.status)}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Action Button */}
      {!analysisStarted && (
        <div className="text-center">
          <button
            onClick={startAnalysis}
            disabled={isExecuting}
            className={`inline-flex items-center space-x-2 px-8 py-4 rounded-lg font-medium text-lg transition-colors ${
              isExecuting
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700'
            }`}
          >
            {isExecuting ? (
              <>
                <ArrowPathIcon className="w-5 h-5 animate-spin" />
                <span>Starting Analysis...</span>
              </>
            ) : (
              <>
                <LightBulbIcon className="w-5 h-5" />
                <span>Start AI Analysis</span>
                <ArrowRightIcon className="w-5 h-5" />
              </>
            )}
          </button>
          
          <p className="text-sm text-gray-500 mt-3">
            Analysis will run automatically. You can navigate away and return to check progress.
          </p>
        </div>
      )}

      {/* Results Available */}
      {results && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <CheckCircleIcon className="w-8 h-8 text-green-500" />
              <div>
                <h3 className="text-lg font-semibold text-green-900">Analysis Complete!</h3>
                <p className="text-green-700">
                  Your comprehensive market research is ready to review.
                </p>
              </div>
            </div>
            <div className="flex space-x-3">
              <button
                onClick={() => onAnalysisComplete(results)}
                className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
              >
                View Results
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Help Text */}
      <div className="text-center text-sm text-gray-500 space-y-1">
        <p>
          The AI is analyzing real market data and industry insights to provide you with 
          actionable recommendations.
        </p>
        <p>
          Each phase builds on previous insights to create a comprehensive understanding 
          of your opportunity.
        </p>
      </div>
    </div>
  );
};

export default GuidedAnalysisFlow;