import React, { useState, useEffect } from 'react';
import { 
  CheckCircleIcon, 
  ClockIcon, 
  ArrowTrendingUpIcon, 
  UsersIcon, 
  AcademicCapIcon,
  ChartBarIcon,
  ArrowRightIcon,
  ArrowPathIcon,
  ExclamationTriangleIcon,
  LightBulbIcon,
  GlobeAltIcon,
  ShieldCheckIcon,
  SparklesIcon,
  BeakerIcon,
  EyeIcon,
  DocumentTextIcon,
  TrophyIcon
} from '@heroicons/react/24/outline';
import researchStrategyService from '../../services/researchStrategy';

interface AnalysisPhase {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  status: 'pending' | 'in_progress' | 'completed' | 'error';
  estimated_duration: number;
  steps: string[];
  progress?: number; // 0-100 for current phase
}

interface ProgressData {
  strategy_id: number;
  current_phase: string;
  progress_percentage: number;
  estimated_completion_minutes?: number;
  current_step?: string;
  steps_completed?: number;
  total_steps?: number;
  messages?: string[];
}

interface ResearchApproachInfo {
  approach: 'quick_validation' | 'market_deep_dive' | 'launch_strategy';
  title: string;
  description: string;
}

interface EnhancedProgressTrackerProps {
  strategyId: number;
  approach: ResearchApproachInfo;
  ideaTitle: string;
  ideaDescription: string;
  onComplete: (results: any) => void;
  onError?: (error: string) => void;
  autoStart?: boolean;
}

const EnhancedProgressTracker: React.FC<EnhancedProgressTrackerProps> = ({
  strategyId,
  approach,
  ideaTitle,
  ideaDescription,
  onComplete,
  onError,
  autoStart = false
}) => {
  const [phases, setPhases] = useState<AnalysisPhase[]>([]);
  const [currentProgress, setCurrentProgress] = useState<ProgressData | null>(null);
  const [isExecuting, setIsExecuting] = useState(false);
  const [analysisStarted, setAnalysisStarted] = useState(false);
  const [results, setResults] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [recentMessages, setRecentMessages] = useState<string[]>([]);
  const [startTime, setStartTime] = useState<Date | null>(null);
  const [elapsedTime, setElapsedTime] = useState<string>('0:00');

  // Initialize phases with detailed steps
  useEffect(() => {
    const phaseConfigs = {
      quick_validation: [
        {
          id: 'market_context',
          title: 'Market Context Analysis',
          description: 'Analyzing industry overview, market size, and growth trends',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 3,
          steps: [
            'Gathering industry data',
            'Analyzing market size',
            'Identifying growth trends',
            'Assessing market maturity'
          ]
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Intelligence',
          description: 'Identifying competitors, analyzing positioning, and market gaps',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 5,
          steps: [
            'Identifying direct competitors',
            'Analyzing indirect competitors',
            'Mapping competitive positioning',
            'Finding market gaps',
            'Assessing competitive advantages'
          ]
        },
        {
          id: 'strategic_assessment',
          title: 'Strategic Assessment',
          description: 'SWOT analysis, risk assessment, and strategic recommendations',
          icon: <TrophyIcon className="w-5 h-5" />,
          estimated_duration: 7,
          steps: [
            'Conducting SWOT analysis',
            'Assessing market risks',
            'Evaluating opportunities',
            'Generating strategic options',
            'Creating recommendations'
          ]
        }
      ],
      market_deep_dive: [
        {
          id: 'market_context',
          title: 'Deep Market Analysis',
          description: 'Comprehensive industry analysis, market sizing, and trend forecasting',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 8,
          steps: [
            'Industry ecosystem mapping',
            'Market size calculation',
            'Growth trend analysis',
            'Regulatory environment',
            'Technology trends',
            'Market maturity assessment'
          ]
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Landscape',
          description: 'Detailed competitive analysis and market positioning',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 12,
          steps: [
            'Direct competitor analysis',
            'Indirect competitor mapping',
            'Competitive positioning',
            'Feature comparison',
            'Pricing analysis',
            'Market share assessment',
            'Competitive advantages'
          ]
        },
        {
          id: 'customer_understanding',
          title: 'Customer Intelligence',
          description: 'Customer segmentation, persona development, and needs analysis',
          icon: <UsersIcon className="w-5 h-5" />,
          estimated_duration: 15,
          steps: [
            'Customer segmentation',
            'Persona development',
            'Needs analysis',
            'Pain point identification',
            'Behavioral patterns',
            'Purchase drivers',
            'Customer journey mapping'
          ]
        },
        {
          id: 'strategic_assessment',
          title: 'Strategic Planning',
          description: 'Comprehensive strategic analysis and opportunity scoring',
          icon: <TrophyIcon className="w-5 h-5" />,
          estimated_duration: 10,
          steps: [
            'SWOT analysis',
            'Risk assessment',
            'Opportunity scoring',
            'Strategic option generation',
            'Go-to-market strategy',
            'Resource requirements'
          ]
        }
      ],
      launch_strategy: [
        {
          id: 'market_context',
          title: 'Market Foundation',
          description: 'Comprehensive market analysis, trends, and opportunity assessment',
          icon: <GlobeAltIcon className="w-5 h-5" />,
          estimated_duration: 15,
          steps: [
            'Market ecosystem analysis',
            'Total addressable market',
            'Serviceable addressable market',
            'Trend forecasting',
            'Regulatory landscape',
            'Technology disruptions',
            'Market timing analysis'
          ]
        },
        {
          id: 'competitive_intelligence',
          title: 'Competitive Strategy',
          description: 'Deep competitive analysis and differentiation strategy',
          icon: <ChartBarIcon className="w-5 h-5" />,
          estimated_duration: 20,
          steps: [
            'Competitive landscape mapping',
            'Feature-by-feature analysis',
            'Pricing strategy analysis',
            'Market positioning',
            'Competitive advantages',
            'Threat assessment',
            'Differentiation opportunities',
            'Competitive response planning'
          ]
        },
        {
          id: 'customer_understanding',
          title: 'Customer Strategy',
          description: 'Advanced customer research, segmentation, and targeting',
          icon: <UsersIcon className="w-5 h-5" />,
          estimated_duration: 25,
          steps: [
            'Customer segmentation',
            'Persona development',
            'Jobs-to-be-done analysis',
            'Customer journey mapping',
            'Pain point prioritization',
            'Value proposition design',
            'Channel preferences',
            'Customer acquisition strategy'
          ]
        },
        {
          id: 'strategic_assessment',
          title: 'Launch Planning',
          description: 'Complete strategic planning and launch roadmap',
          icon: <TrophyIcon className="w-5 h-5" />,
          estimated_duration: 20,
          steps: [
            'Strategic options analysis',
            'Business model design',
            'Go-to-market strategy',
            'Launch timeline',
            'Resource planning',
            'Risk mitigation',
            'Success metrics',
            'Milestone planning'
          ]
        }
      ]
    };

    // Ensure approach is valid before accessing phaseConfigs
    if (!approach?.approach || !phaseConfigs[approach.approach]) {
      console.warn('Invalid approach provided to EnhancedProgressTracker:', approach);
      return; // Exit early if approach is invalid
    }

    const initialPhases = phaseConfigs[approach.approach].map(phase => ({
      ...phase,
      status: 'pending' as const,
      progress: 0
    }));

    setPhases(initialPhases);

    // Auto-start if requested
    if (autoStart) {
      setTimeout(() => startAnalysis(), 1000);
    }
  }, [approach.approach, autoStart]);

  // Update elapsed time
  useEffect(() => {
    if (!startTime) return;

    const interval = setInterval(() => {
      const now = new Date();
      const elapsed = Math.floor((now.getTime() - startTime.getTime()) / 1000);
      const minutes = Math.floor(elapsed / 60);
      const seconds = elapsed % 60;
      setElapsedTime(`${minutes}:${seconds.toString().padStart(2, '0')}`);
    }, 1000);

    return () => clearInterval(interval);
  }, [startTime]);

  // Helper function to handle authentication
  const setAuthHeaders = () => {
    const token = localStorage.getItem('access_token');
    if (token) {
      researchStrategyService['axios'].defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }
  };

  // Start analysis execution
  const startAnalysis = async () => {
    setIsExecuting(true);
    setAnalysisStarted(true);
    setError(null);
    setStartTime(new Date());

    try {
      setAuthHeaders();
      await researchStrategyService.executeStrategy(strategyId);
      
      // Start polling for progress
      startProgressPolling();
    } catch (err: any) {
      const errorMessage = err?.response?.data?.detail || err?.message || 'Failed to start analysis';
      setError(errorMessage);
      if (onError) {
        onError(errorMessage);
      }
      setIsExecuting(false);
    }
  };

  // Poll for progress updates
  const startProgressPolling = () => {
    const pollInterval = setInterval(async () => {
      try {
        setAuthHeaders();
        const progress: ProgressData = await researchStrategyService.getProgress(strategyId);
        setCurrentProgress(progress);

        // Update recent messages
        if (progress.messages && progress.messages.length > 0) {
          setRecentMessages(progress.messages.slice(-3));
        }

        // Update phase statuses based on progress
        updatePhaseStatuses(progress);

        // Check if analysis is complete
        if (progress.progress_percentage >= 100) {
          clearInterval(pollInterval);
          await fetchResults();
        }
      } catch (err: any) {
        console.error('Progress polling error:', err);
        clearInterval(pollInterval);
        const errorMessage = err?.response?.data?.detail || 'Failed to track progress';
        setError(errorMessage);
        if (onError) {
          onError(errorMessage);
        }
      }
    }, 1500); // Poll every 1.5 seconds for more responsive updates

    // Cleanup interval after 15 minutes max
    setTimeout(() => clearInterval(pollInterval), 900000);
  };

  // Update phase statuses with more granular progress
  const updatePhaseStatuses = (progress: ProgressData) => {
    setPhases(currentPhases => {
      return currentPhases.map((phase, index) => {
        const phaseStartProgress = (index / currentPhases.length) * 100;
        const phaseEndProgress = ((index + 1) / currentPhases.length) * 100;
        
        if (progress.progress_percentage >= phaseEndProgress) {
          return { ...phase, status: 'completed', progress: 100 };
        } else if (progress.current_phase === phase.id || 
                   (progress.progress_percentage > phaseStartProgress && progress.progress_percentage < phaseEndProgress)) {
          const phaseProgress = Math.min(100, Math.max(0, 
            ((progress.progress_percentage - phaseStartProgress) / (phaseEndProgress - phaseStartProgress)) * 100
          ));
          return { ...phase, status: 'in_progress', progress: phaseProgress };
        } else if (progress.progress_percentage > phaseStartProgress) {
          return { ...phase, status: 'completed', progress: 100 };
        } else {
          return { ...phase, status: 'pending', progress: 0 };
        }
      });
    });
  };

  // Fetch final results
  const fetchResults = async () => {
    try {
      setAuthHeaders();
      const analysisResults = await researchStrategyService.getResults(strategyId);
      setResults(analysisResults);
      onComplete(analysisResults);
      setIsExecuting(false);
    } catch (err: any) {
      const errorMessage = err?.response?.data?.detail || err?.message || 'Failed to fetch results';
      setError(errorMessage);
      if (onError) {
        onError(errorMessage);
      }
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
    if (progress < 25) return 'bg-red-500';
    if (progress < 50) return 'bg-yellow-500';
    if (progress < 75) return 'bg-blue-500';
    return 'bg-green-500';
  };

  const getTotalEstimatedTime = () => {
    return phases.reduce((total, phase) => total + phase.estimated_duration, 0);
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
    <div className="space-y-8">
      {/* Header with Real-time Stats */}
      <div className="text-center space-y-4">
        <div className="flex items-center justify-center space-x-2">
          <SparklesIcon className="w-8 h-8 text-blue-600" />
          <h2 className="text-3xl font-bold text-gray-900">{approach.title} Analysis</h2>
        </div>
        <div className="space-y-2">
          <h3 className="text-xl font-semibold text-gray-800">{ideaTitle}</h3>
          <p className="text-gray-600 text-lg max-w-2xl mx-auto">
            Our advanced AI is conducting comprehensive market analysis using real-time data sources
          </p>
        </div>
        
        {/* Live Stats */}
        {analysisStarted && (
          <div className="flex items-center justify-center space-x-8 text-sm">
            <div className="flex items-center space-x-2">
              <ClockIcon className="w-4 h-4 text-gray-500" />
              <span className="text-gray-700">Elapsed: <strong>{elapsedTime}</strong></span>
            </div>
            <div className="flex items-center space-x-2">
              <BeakerIcon className="w-4 h-4 text-gray-500" />
              <span className="text-gray-700">Estimated: <strong>{getTotalEstimatedTime()} min</strong></span>
            </div>
            {currentProgress && (
              <div className="flex items-center space-x-2">
                <EyeIcon className="w-4 h-4 text-gray-500" />
                <span className="text-gray-700">
                  Step {currentProgress.steps_completed || 1} of {currentProgress.total_steps || phases.length}
                </span>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Overall Progress Overview */}
      {currentProgress && (
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6 border border-blue-200">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-xl font-bold text-blue-900">Overall Progress</h3>
              <p className="text-blue-700">
                {phases.find(p => p.status === 'in_progress')?.title || 'Processing...'}
              </p>
            </div>
            <div className="text-right">
              <div className="text-4xl font-bold text-blue-900">
                {Math.round(currentProgress.progress_percentage)}%
              </div>
              {currentProgress.estimated_completion_minutes && (
                <div className="text-sm text-blue-700">
                  ~{Math.ceil(currentProgress.estimated_completion_minutes)} min remaining
                </div>
              )}
            </div>
          </div>
          
          {/* Progress Bar */}
          <div className="relative">
            <div className="w-full bg-white rounded-full h-4 shadow-inner">
              <div
                className={`h-4 rounded-full transition-all duration-1000 ${getProgressBarColor(currentProgress.progress_percentage)} relative overflow-hidden`}
                style={{ width: `${currentProgress.progress_percentage}%` }}
              >
                <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white to-transparent opacity-30 animate-pulse"></div>
              </div>
            </div>
            <div className="flex justify-between text-xs text-blue-600 mt-2">
              <span>0%</span>
              <span>25%</span>
              <span>50%</span>
              <span>75%</span>
              <span>100%</span>
            </div>
          </div>
        </div>
      )}

      {/* Live Activity Feed */}
      {recentMessages.length > 0 && (
        <div className="bg-gray-50 rounded-lg p-4 border border-gray-200">
          <h4 className="text-sm font-semibold text-gray-900 mb-3 flex items-center">
            <DocumentTextIcon className="w-4 h-4 mr-2" />
            Recent Activity
          </h4>
          <div className="space-y-2">
            {recentMessages.map((message, index) => (
              <div key={index} className="flex items-start space-x-2 text-sm">
                <div className="w-2 h-2 bg-blue-500 rounded-full mt-2 flex-shrink-0 animate-pulse"></div>
                <span className="text-gray-700">{message}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Detailed Phase Progress */}
      <div className="space-y-6">
        <h3 className="text-xl font-bold text-gray-900 flex items-center">
          <ChartBarIcon className="w-6 h-6 mr-2 text-blue-600" />
          Analysis Phases
        </h3>
        
        <div className="space-y-4">
          {phases.map((phase, index) => (
            <div
              key={phase.id}
              className={`relative overflow-hidden rounded-xl border-2 transition-all duration-300 ${
                phase.status === 'completed' ? 'bg-green-50 border-green-200 shadow-sm' :
                phase.status === 'in_progress' ? 'bg-blue-50 border-blue-300 shadow-md' :
                phase.status === 'error' ? 'bg-red-50 border-red-200' :
                'bg-gray-50 border-gray-200'
              }`}
            >
              {/* Phase Header */}
              <div className="p-6">
                <div className="flex items-center space-x-4">
                  {/* Phase Number */}
                  <div className={`flex items-center justify-center w-12 h-12 rounded-full text-lg font-bold ${
                    phase.status === 'completed' ? 'bg-green-500 text-white' :
                    phase.status === 'in_progress' ? 'bg-blue-500 text-white' :
                    phase.status === 'error' ? 'bg-red-500 text-white' :
                    'bg-gray-300 text-gray-600'
                  }`}>
                    {index + 1}
                  </div>

                  {/* Phase Icon */}
                  <div className={`p-3 rounded-full ${
                    phase.status === 'completed' ? 'bg-green-100 text-green-600' :
                    phase.status === 'in_progress' ? 'bg-blue-100 text-blue-600' :
                    phase.status === 'error' ? 'bg-red-100 text-red-600' :
                    'bg-gray-100 text-gray-400'
                  }`}>
                    {phase.icon}
                  </div>

                  {/* Phase Info */}
                  <div className="flex-1">
                    <div className="flex items-center justify-between mb-2">
                      <h4 className={`text-lg font-bold ${
                        phase.status === 'completed' ? 'text-green-900' :
                        phase.status === 'in_progress' ? 'text-blue-900' :
                        phase.status === 'error' ? 'text-red-900' :
                        'text-gray-700'
                      }`}>
                        {phase.title}
                      </h4>
                      <div className="flex items-center space-x-2">
                        <ClockIcon className="w-4 h-4 text-gray-400" />
                        <span className="text-sm text-gray-600">~{phase.estimated_duration} min</span>
                      </div>
                    </div>
                    <p className={`text-sm mb-3 ${
                      phase.status === 'completed' ? 'text-green-700' :
                      phase.status === 'in_progress' ? 'text-blue-700' :
                      phase.status === 'error' ? 'text-red-700' :
                      'text-gray-600'
                    }`}>
                      {phase.description}
                    </p>

                    {/* Phase Progress Bar */}
                    {phase.status === 'in_progress' && phase.progress !== undefined && (
                      <div className="mb-3">
                        <div className="flex justify-between text-xs text-gray-600 mb-1">
                          <span>Phase Progress</span>
                          <span>{Math.round(phase.progress)}%</span>
                        </div>
                        <div className="w-full bg-white rounded-full h-2">
                          <div
                            className="h-2 bg-blue-500 rounded-full transition-all duration-500"
                            style={{ width: `${phase.progress}%` }}
                          />
                        </div>
                      </div>
                    )}

                    {/* Phase Steps */}
                    {(phase.status === 'in_progress' || phase.status === 'completed') && (
                      <div className="space-y-1">
                        {phase.steps.map((step, stepIndex) => {
                          const stepProgress = phase.progress || 0;
                          const stepCompleted = (stepIndex + 1) / phase.steps.length * 100 <= stepProgress;
                          const stepInProgress = !stepCompleted && stepIndex / phase.steps.length * 100 <= stepProgress;
                          
                          return (
                            <div key={stepIndex} className="flex items-center space-x-2 text-xs">
                              {stepCompleted ? (
                                <CheckCircleIcon className="w-3 h-3 text-green-500" />
                              ) : stepInProgress ? (
                                <ArrowPathIcon className="w-3 h-3 text-blue-500 animate-spin" />
                              ) : (
                                <div className="w-3 h-3 rounded-full border border-gray-300" />
                              )}
                              <span className={
                                stepCompleted ? 'text-green-700' :
                                stepInProgress ? 'text-blue-700' :
                                'text-gray-500'
                              }>
                                {step}
                              </span>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>

                  {/* Status Icon */}
                  <div className="flex-shrink-0">
                    {getPhaseStatusIcon(phase.status)}
                  </div>
                </div>
              </div>

              {/* Active Phase Indicator */}
              {phase.status === 'in_progress' && (
                <div className="absolute inset-0 border-2 border-blue-400 rounded-xl animate-pulse pointer-events-none" />
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Action Button */}
      {!analysisStarted && (
        <div className="text-center space-y-4">
          <button
            onClick={startAnalysis}
            disabled={isExecuting}
            className={`inline-flex items-center space-x-3 px-10 py-5 rounded-xl font-bold text-xl transition-all duration-200 transform ${
              isExecuting
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                : 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white hover:from-blue-700 hover:to-indigo-700 hover:scale-105 shadow-lg hover:shadow-xl'
            }`}
          >
            {isExecuting ? (
              <>
                <ArrowPathIcon className="w-6 h-6 animate-spin" />
                <span>Initializing AI Analysis...</span>
              </>
            ) : (
              <>
                <SparklesIcon className="w-6 h-6" />
                <span>Start AI Market Research</span>
                <ArrowRightIcon className="w-6 h-6" />
              </>
            )}
          </button>
          
          <div className="text-sm text-gray-500 space-y-1">
            <p>AI will automatically analyze your idea across {phases.length} comprehensive phases</p>
            <p>Estimated completion time: <strong>{getTotalEstimatedTime()} minutes</strong></p>
          </div>
        </div>
      )}

      {/* Results Available */}
      {results && (
        <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-200 rounded-xl p-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="p-3 bg-green-100 rounded-full">
                <TrophyIcon className="w-8 h-8 text-green-600" />
              </div>
              <div>
                <h3 className="text-2xl font-bold text-green-900">Analysis Complete!</h3>
                <p className="text-green-700 text-lg">
                  Your comprehensive market research is ready with strategic recommendations
                </p>
                <div className="text-sm text-green-600 mt-1">
                  Analysis completed in {elapsedTime} • {phases.length} phases analyzed
                </div>
              </div>
            </div>
            <button
              onClick={() => onComplete(results)}
              className="px-8 py-4 bg-green-600 text-white rounded-xl hover:bg-green-700 font-bold text-lg transition-colors shadow-lg"
            >
              View Results →
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default EnhancedProgressTracker;