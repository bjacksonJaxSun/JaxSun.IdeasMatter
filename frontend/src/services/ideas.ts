import axios from 'axios'

interface IdeaSubmissionData {
  title: string
  description: string
  idea_id: string
}

interface IdeaSubmissionResponse {
  session_id: number
  idea_id: string
  title: string
  status: string
  analysis_result: {
    status: string
    report_id?: number
    insights_count: number
    options_count: number
    message: string
  }
  insights_count: number
  options_count: number
  message: string
  next_steps: string[]
}

export const ideasService = {
  /**
   * Submit a new idea with automatic competitive market analysis
   */
  async submitIdea(data: IdeaSubmissionData): Promise<IdeaSubmissionResponse> {
    try {
      const response = await axios.post<IdeaSubmissionResponse>(
        '/research/ideas/submit',
        data
      )
      return response.data
    } catch (error: any) {
      console.error('Error submitting idea:', error)
      throw new Error(error.response?.data?.detail || 'Failed to submit idea')
    }
  },

  /**
   * Get research session details
   */
  async getResearchSession(sessionId: number) {
    try {
      const response = await axios.get(`/research/sessions/${sessionId}`)
      return response.data
    } catch (error: any) {
      console.error('Error fetching research session:', error)
      throw new Error(error.response?.data?.detail || 'Failed to fetch research session')
    }
  },

  /**
   * List all research sessions for the user
   */
  async listResearchSessions() {
    try {
      const response = await axios.get('/research/sessions')
      return response.data
    } catch (error: any) {
      console.error('Error listing research sessions:', error)
      throw new Error(error.response?.data?.detail || 'Failed to list research sessions')
    }
  },

  /**
   * Delete a research session (idea) by sessionId
   */
  async deleteIdea(sessionId: number) {
    try {
      await axios.delete(`/research/sessions/${sessionId}`)
    } catch (error: any) {
      console.error('Error deleting idea:', error)
      throw new Error(error.response?.data?.detail || 'Failed to delete idea')
    }
  }
}