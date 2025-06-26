import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import axios from 'axios';
import { EnhancedProgressTracker } from '../EnhancedProgressTracker';
import { OneClickResearchSelector } from '../OneClickResearchSelector';
import { ResearchStrategyContainer } from '../ResearchStrategyContainer';
import { toast } from 'react-hot-toast';

// Mock axios
vi.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

// Mock toast
vi.mock('react-hot-toast', () => ({
  toast: {
    loading: vi.fn(),
    dismiss: vi.fn(),
    success: vi.fn(),
    error: vi.fn()
  }
}));

// Helper to create test wrapper
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {children}
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('Research Progress Tracking Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Setup axios instance mock
    mockedAxios.create.mockReturnValue({
      ...mockedAxios,
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() }
      }
    } as any);
  });

  describe('Cat and Mouse Game Scenario', () => {
    const catMouseIdea = {
      title: 'Cat and Mouse Game',
      description: 'This is a game that 4 - 10 year olds can play and the user choses their favorite cat and mouse. The game allows the user to learn how strategy works by being either the cat or mouse.'
    };

    it('should successfully track progress for Cat and Mouse Game', async () => {
      const strategyId = 12345;
      
      // Mock successful strategy initiation
      mockedAxios.post.mockResolvedValueOnce({
        data: {
          strategy: {
            id: strategyId,
            approach: 'market_deep_dive',
            status: 'pending'
          },
          estimated_completion_time: '45 minutes'
        }
      });

      // Mock successful execution
      mockedAxios.post.mockResolvedValueOnce({
        data: {
          message: 'Research strategy execution started',
          strategy_id: strategyId
        }
      });

      // Mock progress updates
      const progressUpdates = [
        {
          strategy_id: strategyId,
          current_phase: 'market_context',
          progress_percentage: 15,
          estimated_completion_minutes: 38,
          current_step: 'Analyzing market size',
          messages: ['Started analysis', 'Gathering market data']
        },
        {
          strategy_id: strategyId,
          current_phase: 'competitive_intelligence',
          progress_percentage: 45,
          estimated_completion_minutes: 25,
          current_step: 'Identifying competitors',
          messages: ['Market analysis complete', 'Starting competitive analysis']
        },
        {
          strategy_id: strategyId,
          current_phase: 'strategic_assessment',
          progress_percentage: 85,
          estimated_completion_minutes: 7,
          current_step: 'Generating recommendations',
          messages: ['Competitive analysis complete', 'Finalizing strategy']
        }
      ];

      let progressCallCount = 0;
      mockedAxios.get.mockImplementation((url) => {
        if (url.includes('/progress/')) {
          const update = progressUpdates[Math.min(progressCallCount++, progressUpdates.length - 1)];
          return Promise.resolve({ data: update });
        }
        return Promise.reject(new Error('Unknown endpoint'));
      });

      // Render the progress tracker
      render(
        <EnhancedProgressTracker
          strategyId={strategyId}
          approach={{ 
            approach: 'market_deep_dive',
            title: 'Market Deep-Dive',
            description: 'Comprehensive market analysis'
          }}
          ideaTitle={catMouseIdea.title}
          ideaDescription={catMouseIdea.description}
          onComplete={vi.fn()}
          autoStart={true}
        />,
        { wrapper: createWrapper() }
      );

      // Verify initial state
      expect(screen.getByText(/Market Deep-Dive Analysis/i)).toBeInTheDocument();
      expect(screen.getByText(catMouseIdea.title)).toBeInTheDocument();

      // Wait for progress updates
      await waitFor(() => {
        expect(screen.getByText(/market_context/i)).toBeInTheDocument();
      }, { timeout: 5000 });

      // Verify progress is being tracked
      await waitFor(() => {
        const progressElements = screen.getAllByText(/%/);
        expect(progressElements.length).toBeGreaterThan(0);
      });

      // Verify no error messages
      expect(screen.queryByText(/Failed to track progress/i)).not.toBeInTheDocument();
      expect(screen.queryByText(/Analysis Error/i)).not.toBeInTheDocument();

      // Verify API calls
      expect(mockedAxios.get).toHaveBeenCalledWith(
        expect.stringContaining(`/progress/${strategyId}`)
      );
    });

    it('should handle progress tracking errors gracefully', async () => {
      const strategyId = 12346;
      
      // Mock the API error that users are experiencing
      mockedAxios.get.mockRejectedValue({
        response: {
          status: 500,
          data: { detail: 'Failed to track progress' }
        }
      });

      render(
        <EnhancedProgressTracker
          strategyId={strategyId}
          approach={{ 
            approach: 'quick_validation',
            title: 'Quick Validation',
            description: 'Rapid validation'
          }}
          ideaTitle={catMouseIdea.title}
          ideaDescription={catMouseIdea.description}
          onComplete={vi.fn()}
          autoStart={true}
        />,
        { wrapper: createWrapper() }
      );

      // Wait for error to appear
      await waitFor(() => {
        expect(screen.getByText(/Failed to track progress/i)).toBeInTheDocument();
      }, { timeout: 5000 });

      // Verify error UI elements
      expect(screen.getByText(/Analysis Error/i)).toBeInTheDocument();
      expect(screen.getByText(/Try Again/i)).toBeInTheDocument();
    });

    it('should successfully start research with one-click selector', async () => {
      const onStrategySelected = vi.fn();
      
      // Mock successful initiation
      mockedAxios.post.mockResolvedValueOnce({
        data: {
          strategy: {
            id: 12347,
            approach: 'market_deep_dive',
            status: 'pending'
          }
        }
      });

      render(
        <OneClickResearchSelector
          sessionId={1}
          ideaTitle={catMouseIdea.title}
          ideaDescription={catMouseIdea.description}
          onStrategySelected={onStrategySelected}
        />,
        { wrapper: createWrapper() }
      );

      // Find and click Market Deep-Dive button
      const deepDiveButton = screen.getByText(/Start Market Deep-Dive/i);
      expect(deepDiveButton).toBeInTheDocument();
      
      fireEvent.click(deepDiveButton);

      // Verify loading state
      expect(toast.loading).toHaveBeenCalledWith(
        expect.stringContaining('Initializing AI research analysis'),
        expect.any(Object)
      );

      // Wait for success
      await waitFor(() => {
        expect(onStrategySelected).toHaveBeenCalled();
      });

      // Verify API call
      expect(mockedAxios.post).toHaveBeenCalledWith(
        '/research-strategy/initiate',
        expect.objectContaining({
          session_id: 1,
          approach: 'market_deep_dive',
          custom_parameters: {
            idea_title: catMouseIdea.title,
            idea_description: catMouseIdea.description
          }
        })
      );
    });
  });

  describe('API Integration Tests', () => {
    it('should correctly handle authentication in progress endpoints', async () => {
      const strategyId = 12348;
      
      // Test with missing auth token
      delete mockedAxios.defaults.headers.common['Authorization'];
      
      mockedAxios.get.mockResolvedValueOnce({
        data: {
          strategy_id: strategyId,
          progress_percentage: 50,
          current_phase: 'market_context'
        }
      });

      render(
        <EnhancedProgressTracker
          strategyId={strategyId}
          approach={{ 
            approach: 'quick_validation',
            title: 'Quick Validation',
            description: 'Test'
          }}
          ideaTitle="Test"
          ideaDescription="Test"
          onComplete={vi.fn()}
          autoStart={true}
        />,
        { wrapper: createWrapper() }
      );

      // Should still work with optional auth
      await waitFor(() => {
        expect(mockedAxios.get).toHaveBeenCalled();
      });
      
      // Should not show authentication errors
      expect(screen.queryByText(/401/i)).not.toBeInTheDocument();
      expect(screen.queryByText(/Unauthorized/i)).not.toBeInTheDocument();
    });

    it('should handle network timeouts gracefully', async () => {
      const strategyId = 12349;
      
      // Mock network timeout
      mockedAxios.get.mockRejectedValueOnce(new Error('Network timeout'));

      render(
        <EnhancedProgressTracker
          strategyId={strategyId}
          approach={{ 
            approach: 'quick_validation',
            title: 'Quick Validation',
            description: 'Test'
          }}
          ideaTitle="Test"
          ideaDescription="Test"
          onComplete={vi.fn()}
          autoStart={true}
        />,
        { wrapper: createWrapper() }
      );

      // Should show error state
      await waitFor(() => {
        expect(screen.getByText(/Error/i)).toBeInTheDocument();
      }, { timeout: 5000 });
    });
  });
});