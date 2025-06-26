import axios from 'axios';

export interface ResearchApproach {
  approach: 'quick_validation' | 'market_deep_dive' | 'launch_strategy';
  title: string;
  description: string;
  duration_minutes: number;
  complexity: 'beginner' | 'intermediate' | 'advanced';
  best_for: string[];
  includes: string[];
  deliverables: string[];
}

export interface ResearchStrategy {
  id: number;
  session_id: number;
  approach: string;
  status: string;
  created_at: string;
  started_at?: string;
  completed_at?: string;
  estimated_duration_minutes: number;
}

export interface ResearchStrategyRequest {
  session_id: number;
  approach: string;
  custom_parameters?: {
    idea_title?: string;
    idea_description?: string;
  };
}

export interface ResearchStrategyResponse {
  strategy: ResearchStrategy;
  estimated_completion_time: string;
  included_analyses: string[];
  next_steps: string[];
}

class ResearchStrategyService {
  private baseUrl = '/api/v1/research-strategy';

  async getApproaches(): Promise<ResearchApproach[]> {
    const response = await axios.get(`${this.baseUrl}/approaches`);
    return response.data;
  }

  async initiateStrategy(request: ResearchStrategyRequest): Promise<ResearchStrategyResponse> {
    const response = await axios.post(`${this.baseUrl}/initiate`, request);
    return response.data;
  }

  async executeStrategy(strategyId: number): Promise<{ message: string; strategy_id: number }> {
    const response = await axios.post(`${this.baseUrl}/execute/${strategyId}`);
    return response.data;
  }

  async getProgress(strategyId: number): Promise<any> {
    const response = await axios.get(`${this.baseUrl}/progress/${strategyId}`);
    return response.data;
  }

  async getResults(strategyId: number): Promise<any> {
    const response = await axios.get(`${this.baseUrl}/results/${strategyId}`);
    return response.data;
  }

  async getSessionStrategies(sessionId: number): Promise<ResearchStrategy[]> {
    const response = await axios.get(`${this.baseUrl}/strategies/${sessionId}`);
    return response.data;
  }

  async compareOptions(strategyId: number, comparisonCriteria?: string[]): Promise<any> {
    const response = await axios.post(`${this.baseUrl}/compare-options`, {
      strategy_id: strategyId,
      comparison_criteria: comparisonCriteria
    });
    return response.data;
  }

  async exportResults(sessionId: number, exportFormat: string, strategyId?: number): Promise<any> {
    const response = await axios.post(`${this.baseUrl}/export`, {
      session_id: sessionId,
      export_format: exportFormat,
      strategy_id: strategyId
    });
    return response.data;
  }

  async deleteStrategy(strategyId: number): Promise<void> {
    await axios.delete(`${this.baseUrl}/strategies/${strategyId}`);
  }

  async getDemoAnalysis(approach: string): Promise<any> {
    const response = await axios.get(`${this.baseUrl}/demo/${approach}`);
    return response.data;
  }
}

export default new ResearchStrategyService();