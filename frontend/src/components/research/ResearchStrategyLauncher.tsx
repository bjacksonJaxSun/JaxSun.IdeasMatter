import React, { useState } from 'react';
import { 
  LightBulbIcon, 
  TrendingUpIcon, 
  RocketLaunchIcon, 
  ArrowRightIcon, 
  SparklesIcon 
} from '@heroicons/react/24/outline';
import ResearchStrategyContainer from './ResearchStrategyContainer';

interface ResearchStrategyLauncherProps {
  sessionId: number;
  ideaTitle: string;
  ideaDescription: string;
}

const ResearchStrategyLauncher: React.FC<ResearchStrategyLauncherProps> = ({
  sessionId,
  ideaTitle,
  ideaDescription
}) => {
  const [showNewResearch, setShowNewResearch] = useState(false);

  if (showNewResearch) {
    return (
      <ResearchStrategyContainer
        sessionId={sessionId}
        ideaTitle={ideaTitle}
        ideaDescription={ideaDescription}
        onClose={() => setShowNewResearch(false)}
      />
    );
  }

  return (
    <div className="bg-gradient-to-br from-blue-50 to-indigo-100 rounded-xl p-6 border border-blue-200">
      <div className="flex items-start space-x-4">
        <div className="flex-shrink-0">
          <div className="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
            <SparklesIcon className="w-6 h-6 text-white" />
          </div>
        </div>
        
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            🚀 Try Our New AI Research Assistant
          </h3>
          <p className="text-gray-700 mb-4">
            Experience our redesigned market research with guided analysis, strategic options, 
            and integrated insights - all powered by advanced AI.
          </p>
          
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-4">
            <div className="bg-white/70 rounded-lg p-3 border border-blue-200">
              <div className="flex items-center space-x-2 mb-1">
                <LightBulbIcon className="w-4 h-4 text-green-600" />
                <span className="text-sm font-medium text-gray-900">Quick Validation</span>
              </div>
              <p className="text-xs text-gray-600">15-min rapid assessment</p>
            </div>
            
            <div className="bg-white/70 rounded-lg p-3 border border-blue-200">
              <div className="flex items-center space-x-2 mb-1">
                <TrendingUpIcon className="w-4 h-4 text-blue-600" />
                <span className="text-sm font-medium text-gray-900">Market Deep-Dive</span>
              </div>
              <p className="text-xs text-gray-600">45-min comprehensive analysis</p>
            </div>
            
            <div className="bg-white/70 rounded-lg p-3 border border-blue-200">
              <div className="flex items-center space-x-2 mb-1">
                <RocketLaunchIcon className="w-4 h-4 text-purple-600" />
                <span className="text-sm font-medium text-gray-900">Launch Strategy</span>
              </div>
              <p className="text-xs text-gray-600">90-min complete roadmap</p>
            </div>
          </div>
          
          <div className="flex items-center space-x-3">
            <button
              onClick={() => setShowNewResearch(true)}
              className="inline-flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium transition-colors"
            >
              <span>Start New Research</span>
              <ArrowRightIcon className="w-4 h-4" />
            </button>
            
            <div className="text-xs text-gray-600">
              <span className="font-medium text-green-600">✨ New!</span> Progressive disclosure, 
              integrated analysis, and strategic options
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ResearchStrategyLauncher;