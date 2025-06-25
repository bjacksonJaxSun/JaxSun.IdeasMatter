import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface SwotData {
  strengths: string[];
  weaknesses: string[];
  opportunities: string[];
  threats: string[];
  confidence: number;
  generated_at: string | null;
}

interface SwotAnalysisProps {
  optionId: number;
  optionTitle: string;
  onClose?: () => void;
}

const SwotAnalysis: React.FC<SwotAnalysisProps> = ({ optionId, optionTitle, onClose }) => {
  const [swotData, setSwotData] = useState<SwotData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [regenerating, setRegenerating] = useState(false);

  const fetchSwotAnalysis = async (regenerate = false) => {
    setLoading(true);
    setError(null);
    try {
      const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000';
      const response = await axios.post(`${API_BASE_URL}/api/v1/research/options/${optionId}/swot`, {
        option_id: optionId,
        regenerate
      });
      setSwotData(response.data.swot);
    } catch (err) {
      setError('Failed to generate SWOT analysis');
      console.error('Error fetching SWOT analysis:', err);
    } finally {
      setLoading(false);
      setRegenerating(false);
    }
  };

  useEffect(() => {
    fetchSwotAnalysis();
  }, [optionId]);

  const handleRegenerateSwot = () => {
    setRegenerating(true);
    fetchSwotAnalysis(true);
  };

  const handleDownloadPdf = async () => {
    try {
      const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000';
      const response = await axios.get(
        `${API_BASE_URL}/api/v1/research/options/${optionId}/swot/pdf`,
        {
          responseType: 'blob',
          params: { include_metadata: true }
        }
      );
      
      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `swot_analysis_${optionId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Error downloading PDF:', err);
      alert('Failed to download SWOT analysis PDF');
    }
  };

  const getQuadrantColor = (type: string) => {
    switch (type) {
      case 'strengths':
        return 'bg-green-50 border-green-200';
      case 'weaknesses':
        return 'bg-red-50 border-red-200';
      case 'opportunities':
        return 'bg-blue-50 border-blue-200';
      case 'threats':
        return 'bg-yellow-50 border-yellow-200';
      default:
        return 'bg-gray-50 border-gray-200';
    }
  };

  const getQuadrantIcon = (type: string) => {
    switch (type) {
      case 'strengths':
        return '💪';
      case 'weaknesses':
        return '⚠️';
      case 'opportunities':
        return '🎯';
      case 'threats':
        return '🚨';
      default:
        return '📊';
    }
  };

  if (loading && !swotData) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Generating SWOT analysis...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
        <p className="text-red-800">{error}</p>
        <button
          onClick={() => fetchSwotAnalysis()}
          className="mt-2 text-sm text-red-600 hover:text-red-700 underline"
        >
          Try again
        </button>
      </div>
    );
  }

  if (!swotData) return null;

  return (
    <div className="bg-white rounded-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">SWOT Analysis</h3>
          <p className="text-sm text-gray-600 mt-1">{optionTitle}</p>
        </div>
        <div className="flex items-center space-x-2">
          <button
            onClick={handleDownloadPdf}
            className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md transition-colors flex items-center space-x-1"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <span>Download PDF</span>
          </button>
          <button
            onClick={handleRegenerateSwot}
            disabled={regenerating}
            className="px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors flex items-center space-x-1 disabled:opacity-50"
          >
            <svg className={`w-4 h-4 ${regenerating ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <span>{regenerating ? 'Regenerating...' : 'Regenerate'}</span>
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

      <div className="grid grid-cols-2 gap-4">
        {/* Strengths */}
        <div className={`border rounded-lg p-4 ${getQuadrantColor('strengths')}`}>
          <div className="flex items-center mb-3">
            <span className="text-2xl mr-2">{getQuadrantIcon('strengths')}</span>
            <h4 className="font-semibold text-green-800">Strengths</h4>
          </div>
          <ul className="space-y-2">
            {swotData.strengths.map((strength, idx) => (
              <li key={idx} className="text-sm text-gray-700 flex items-start">
                <span className="text-green-600 mr-2">•</span>
                <span>{strength}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Weaknesses */}
        <div className={`border rounded-lg p-4 ${getQuadrantColor('weaknesses')}`}>
          <div className="flex items-center mb-3">
            <span className="text-2xl mr-2">{getQuadrantIcon('weaknesses')}</span>
            <h4 className="font-semibold text-red-800">Weaknesses</h4>
          </div>
          <ul className="space-y-2">
            {swotData.weaknesses.map((weakness, idx) => (
              <li key={idx} className="text-sm text-gray-700 flex items-start">
                <span className="text-red-600 mr-2">•</span>
                <span>{weakness}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Opportunities */}
        <div className={`border rounded-lg p-4 ${getQuadrantColor('opportunities')}`}>
          <div className="flex items-center mb-3">
            <span className="text-2xl mr-2">{getQuadrantIcon('opportunities')}</span>
            <h4 className="font-semibold text-blue-800">Opportunities</h4>
          </div>
          <ul className="space-y-2">
            {swotData.opportunities.map((opportunity, idx) => (
              <li key={idx} className="text-sm text-gray-700 flex items-start">
                <span className="text-blue-600 mr-2">•</span>
                <span>{opportunity}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Threats */}
        <div className={`border rounded-lg p-4 ${getQuadrantColor('threats')}`}>
          <div className="flex items-center mb-3">
            <span className="text-2xl mr-2">{getQuadrantIcon('threats')}</span>
            <h4 className="font-semibold text-yellow-800">Threats</h4>
          </div>
          <ul className="space-y-2">
            {swotData.threats.map((threat, idx) => (
              <li key={idx} className="text-sm text-gray-700 flex items-start">
                <span className="text-yellow-600 mr-2">•</span>
                <span>{threat}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>

      {/* Confidence Score */}
      <div className="mt-6 pt-4 border-t border-gray-200">
        <div className="flex items-center justify-between text-sm">
          <span className="text-gray-600">Analysis Confidence</span>
          <div className="flex items-center">
            <div className="w-32 bg-gray-200 rounded-full h-2 mr-2">
              <div 
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${swotData.confidence * 100}%` }}
              ></div>
            </div>
            <span className="font-medium text-gray-700">
              {Math.round(swotData.confidence * 100)}%
            </span>
          </div>
        </div>
        {swotData.generated_at && (
          <p className="text-xs text-gray-500 mt-2">
            Generated: {new Date(swotData.generated_at).toLocaleString()}
          </p>
        )}
      </div>
    </div>
  );
};

export default SwotAnalysis;