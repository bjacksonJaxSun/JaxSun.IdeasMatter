import React, { useState, useEffect } from 'react';
import { 
  ArrowLeftIcon, 
  ArrowDownTrayIcon, 
  ShareIcon, 
  BookOpenIcon 
} from '@heroicons/react/24/outline';
import ResearchStrategySelector from './ResearchStrategySelector';
import GuidedAnalysisFlow from './GuidedAnalysisFlow';
import StrategicOptionsComparison from './StrategicOptionsComparison';

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

interface ResearchStrategyContainerProps {
  sessionId: number;
  ideaTitle: string;
  ideaDescription: string;
  onClose?: () => void;
}

type FlowStep = 'selection' | 'analysis' | 'results' | 'comparison';

const ResearchStrategyContainer: React.FC<ResearchStrategyContainerProps> = ({
  sessionId,
  ideaTitle,
  ideaDescription,
  onClose
}) => {
  const [currentStep, setCurrentStep] = useState<FlowStep>('selection');
  const [selectedApproach, setSelectedApproach] = useState<ResearchApproach | null>(null);
  const [strategyId, setStrategyId] = useState<number | null>(null);
  const [analysisResults, setAnalysisResults] = useState<any>(null);
  const [selectedStrategicOption, setSelectedStrategicOption] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  // Handle strategy selection
  const handleStrategySelected = (approach: ResearchApproach, strategyIdValue: number) => {
    setSelectedApproach(approach);
    setStrategyId(strategyIdValue);
    setCurrentStep('analysis');
    setError(null);
  };

  // Handle analysis completion
  const handleAnalysisComplete = (results: any) => {
    setAnalysisResults(results);
    setCurrentStep('results');
  };

  // Handle analysis error
  const handleAnalysisError = (errorMessage: string) => {
    setError(errorMessage);
    // Could show error state or go back to selection
  };

  // Handle strategic option selection
  const handleStrategicOptionSelected = (option: any) => {
    setSelectedStrategicOption(option);
    setCurrentStep('comparison');
  };

  // Handle navigation back to previous steps
  const handleGoBack = () => {
    switch (currentStep) {
      case 'analysis':
        setCurrentStep('selection');
        setSelectedApproach(null);
        setStrategyId(null);
        break;
      case 'results':
        setCurrentStep('analysis');
        setAnalysisResults(null);
        break;
      case 'comparison':
        setCurrentStep('results');
        setSelectedStrategicOption(null);
        break;
      default:
        if (onClose) onClose();
    }
    setError(null);
  };

  // Helper function to get authenticated headers
  const getAuthHeaders = () => {
    const token = localStorage.getItem('access_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` })
    };
  };

  // Export functionality
  const handleExport = async () => {
    try {
      const response = await fetch('/api/v1/research-strategy/export', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          session_id: sessionId,
          strategy_id: strategyId,
          export_format: 'pdf',
          include_sections: ['all']
        }),
      });

      if (!response.ok) {
        throw new Error('Failed to export results');
      }

      const exportData = await response.json();
      
      // Trigger download
      window.open(exportData.download_url, '_blank');
    } catch (err) {
      console.error('Export failed:', err);
      setError('Failed to export results');
    }
  };

  // Get step title and description
  const getStepInfo = () => {
    switch (currentStep) {
      case 'selection':
        return {
          title: 'Research Strategy Selection',
          description: 'Choose the research approach that best fits your needs'
        };
      case 'analysis':
        return {
          title: 'AI-Powered Analysis',
          description: `Running ${selectedApproach?.title} analysis for your idea`
        };
      case 'results':
        return {
          title: 'Research Results',
          description: 'Comprehensive analysis with strategic recommendations'
        };
      case 'comparison':
        return {
          title: 'Strategic Options',
          description: 'Compare and select your preferred strategic approach'
        };
      default:
        return {
          title: 'Market Research',
          description: 'AI-powered market research and strategy development'
        };
    }
  };

  const stepInfo = getStepInfo();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Left side - Navigation */}
            <div className="flex items-center space-x-4">
              <button
                onClick={handleGoBack}
                className="p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
              >
                <ArrowLeftIcon className="w-5 h-5" />
              </button>
              
              <div>
                <h1 className="text-lg font-semibold text-gray-900">{stepInfo.title}</h1>
                <p className="text-sm text-gray-600">{stepInfo.description}</p>
              </div>
            </div>

            {/* Right side - Actions */}
            <div className="flex items-center space-x-3">
              {/* Progress indicator */}
              <div className="hidden md:flex items-center space-x-2 text-sm text-gray-500">
                <div className={`w-2 h-2 rounded-full ${currentStep === 'selection' ? 'bg-blue-500' : 'bg-gray-300'}`} />
                <span className={currentStep === 'selection' ? 'text-blue-600 font-medium' : ''}>Select</span>
                
                <div className={`w-2 h-2 rounded-full ${currentStep === 'analysis' ? 'bg-blue-500' : 'bg-gray-300'}`} />
                <span className={currentStep === 'analysis' ? 'text-blue-600 font-medium' : ''}>Analyze</span>
                
                <div className={`w-2 h-2 rounded-full ${currentStep === 'results' ? 'bg-blue-500' : 'bg-gray-300'}`} />
                <span className={currentStep === 'results' ? 'text-blue-600 font-medium' : ''}>Results</span>
                
                <div className={`w-2 h-2 rounded-full ${currentStep === 'comparison' ? 'bg-blue-500' : 'bg-gray-300'}`} />
                <span className={currentStep === 'comparison' ? 'text-blue-600 font-medium' : ''}>Strategy</span>
              </div>

              {/* Action buttons */}
              {(currentStep === 'results' || currentStep === 'comparison') && (
                <>
                  <button
                    onClick={handleExport}
                    className="inline-flex items-center space-x-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                  >
                    <ArrowDownTrayIcon className="w-4 h-4" />
                    <span>Export</span>
                  </button>
                  
                  <button
                    onClick={() => {/* Share functionality */}}
                    className="inline-flex items-center space-x-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                  >
                    <ShareIcon className="w-4 h-4" />
                    <span>Share</span>
                  </button>
                </>
              )}

              {onClose && (
                <button
                  onClick={onClose}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Close
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Error Display */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
            <div className="flex items-center space-x-2 text-red-700">
              <div className="text-sm font-medium">Error: {error}</div>
              <button
                onClick={() => setError(null)}
                className="text-red-400 hover:text-red-600"
              >
                ×
              </button>
            </div>
          </div>
        )}

        {/* Step Content */}
        {currentStep === 'selection' && (
          <ResearchStrategySelector
            sessionId={sessionId}
            ideaTitle={ideaTitle}
            ideaDescription={ideaDescription}
            onStrategySelected={handleStrategySelected}
          />
        )}

        {currentStep === 'analysis' && selectedApproach && strategyId && (
          <GuidedAnalysisFlow
            strategyId={strategyId}
            approach={selectedApproach.approach}
            onAnalysisComplete={handleAnalysisComplete}
            onError={handleAnalysisError}
          />
        )}

        {currentStep === 'results' && analysisResults && (
          <div className="space-y-8">
            {/* Analysis Summary */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">Analysis Summary</h2>
              
              <div className="grid md:grid-cols-3 gap-6">
                <div className="text-center">
                  <div className="text-3xl font-bold text-blue-600">
                    {Math.round(analysisResults.analysis_confidence * 100)}%
                  </div>
                  <div className="text-sm text-gray-600">Confidence Score</div>
                </div>
                
                <div className="text-center">
                  <div className="text-3xl font-bold text-green-600">
                    {analysisResults.strategic_options?.length || 0}
                  </div>
                  <div className="text-sm text-gray-600">Strategic Options</div>
                </div>
                
                <div className="text-center">
                  <div className="text-3xl font-bold text-purple-600">
                    {selectedApproach?.duration_minutes}min
                  </div>
                  <div className="text-sm text-gray-600">Analysis Time</div>
                </div>
              </div>

              {/* Key Insights */}
              {analysisResults.next_steps && (
                <div className="mt-6 p-4 bg-blue-50 rounded-lg">
                  <h3 className="font-semibold text-blue-900 mb-2">Key Next Steps</h3>
                  <ul className="space-y-1">
                    {analysisResults.next_steps.slice(0, 3).map((step: string, index: number) => (
                      <li key={index} className="text-sm text-blue-800 flex items-start">
                        <div className="w-1.5 h-1.5 bg-blue-500 rounded-full mt-2 mr-2 flex-shrink-0" />
                        {step}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>

            {/* Strategic Options Preview */}
            {analysisResults.strategic_options && analysisResults.strategic_options.length > 0 && (
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-xl font-semibold text-gray-900">Strategic Options</h2>
                  <button
                    onClick={() => setCurrentStep('comparison')}
                    className="inline-flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    <span>Compare Options</span>
                    <ArrowLeft className="w-4 h-4 rotate-180" />
                  </button>
                </div>
                
                <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {analysisResults.strategic_options.slice(0, 3).map((option: any, index: number) => (
                    <div
                      key={index}
                      className={`p-4 rounded-lg border-2 transition-colors ${
                        option.recommended 
                          ? 'border-yellow-300 bg-yellow-50' 
                          : 'border-gray-200 bg-gray-50'
                      }`}
                    >
                      <div className="flex items-center justify-between mb-2">
                        <h3 className="font-semibold text-gray-900">{option.title}</h3>
                        {option.recommended && (
                          <span className="px-2 py-1 bg-yellow-200 text-yellow-800 text-xs rounded-full">
                            Recommended
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-600 mb-3">{option.description}</p>
                      
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-gray-500">Success Rate:</span>
                          <span className="font-medium">{option.success_probability_percent}%</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-500">Timeline:</span>
                          <span className="font-medium">{option.timeline_to_market_months} months</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Continue Button */}
            <div className="text-center">
              <button
                onClick={() => setCurrentStep('comparison')}
                className="inline-flex items-center space-x-2 px-8 py-4 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium text-lg"
              >
                <span>Compare Strategic Options</span>
                <ArrowLeftIcon className="w-5 h-5 rotate-180" />
              </button>
            </div>
          </div>
        )}

        {currentStep === 'comparison' && analysisResults && strategyId && (
          <StrategicOptionsComparison
            sessionId={sessionId}
            strategyId={strategyId}
            analysisResults={analysisResults}
            onOptionSelected={handleStrategicOptionSelected}
          />
        )}
      </div>

      {/* Help Documentation Link */}
      <div className="fixed bottom-6 right-6">
        <button
          onClick={() => {/* Open help documentation */}}
          className="p-3 bg-blue-600 text-white rounded-full shadow-lg hover:bg-blue-700 transition-colors"
          title="Help & Documentation"
        >
          <BookOpenIcon className="w-5 h-5" />
        </button>
      </div>
    </div>
  );
};

export default ResearchStrategyContainer;