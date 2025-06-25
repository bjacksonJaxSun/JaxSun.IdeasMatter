from typing import List, Optional, Dict, Any, Union
from pydantic import BaseModel, Field
from datetime import datetime
from enum import Enum

# Research Strategy Types
class ResearchApproach(str, Enum):
    QUICK_VALIDATION = "quick_validation"
    MARKET_DEEP_DIVE = "market_deep_dive" 
    LAUNCH_STRATEGY = "launch_strategy"

class AnalysisPhase(str, Enum):
    MARKET_CONTEXT = "market_context"
    COMPETITIVE_INTELLIGENCE = "competitive_intelligence"
    CUSTOMER_UNDERSTANDING = "customer_understanding"
    STRATEGIC_ASSESSMENT = "strategic_assessment"

class StrategicApproach(str, Enum):
    MARKET_LEADER_CHALLENGE = "market_leader_challenge"
    NICHE_DOMINATION = "niche_domination"
    PLATFORM_PLAY = "platform_play"
    DISRUPTIVE_INNOVATION = "disruptive_innovation"
    PARTNERSHIP_STRATEGY = "partnership_strategy"

# Base Research Strategy
class ResearchStrategyBase(BaseModel):
    approach: ResearchApproach
    title: str = Field(..., min_length=1, max_length=255)
    description: Optional[str] = None
    estimated_duration_minutes: int = Field(..., gt=0)
    complexity_level: str = Field(..., pattern="^(beginner|intermediate|advanced)$")
    
class ResearchStrategyCreate(ResearchStrategyBase):
    session_id: int

class ResearchStrategy(ResearchStrategyBase):
    id: int
    session_id: int
    status: str = Field(default="pending")  # pending, in_progress, completed, error
    progress_percentage: float = Field(default=0.0, ge=0.0, le=100.0)
    started_at: Optional[datetime] = None
    completed_at: Optional[datetime] = None
    created_at: datetime
    
    class Config:
        from_attributes = True

# Market Context Analysis
class MarketContext(BaseModel):
    industry_overview: str
    market_size_usd: Optional[float] = None
    growth_rate_cagr: Optional[float] = None
    maturity_stage: str = Field(..., pattern="^(emerging|growth|mature|declining)$")
    key_trends: List[str] = Field(default_factory=list)
    regulatory_environment: str
    technological_factors: List[str] = Field(default_factory=list)
    confidence_score: float = Field(default=0.7, ge=0.0, le=1.0)

# Competitive Intelligence
class CompetitorProfile(BaseModel):
    name: str
    category: str = Field(..., pattern="^(direct|indirect|substitute)$")
    market_share_percent: Optional[float] = Field(None, ge=0.0, le=100.0)
    strengths: List[str] = Field(default_factory=list)
    weaknesses: List[str] = Field(default_factory=list)
    threat_level: str = Field(..., pattern="^(low|medium|high|critical)$")
    differentiation_opportunities: List[str] = Field(default_factory=list)

class CompetitiveIntelligence(BaseModel):
    competitive_landscape_summary: str
    direct_competitors: List[CompetitorProfile] = Field(default_factory=list)
    indirect_competitors: List[CompetitorProfile] = Field(default_factory=list)
    substitute_solutions: List[CompetitorProfile] = Field(default_factory=list)
    competitive_advantages: List[str] = Field(default_factory=list)
    barriers_to_entry: List[str] = Field(default_factory=list)
    confidence_score: float = Field(default=0.7, ge=0.0, le=1.0)

# Customer Understanding
class CustomerSegment(BaseModel):
    name: str
    description: str
    size_estimate: Optional[int] = None
    demographics: Dict[str, Any] = Field(default_factory=dict)
    pain_points: List[str] = Field(default_factory=list)
    jobs_to_be_done: List[str] = Field(default_factory=list)
    value_propositions: List[str] = Field(default_factory=list)
    willingness_to_pay: Optional[str] = None
    acquisition_channels: List[str] = Field(default_factory=list)
    priority_score: float = Field(default=0.5, ge=0.0, le=1.0)

class CustomerUnderstanding(BaseModel):
    primary_target_segment: str
    customer_segments: List[CustomerSegment] = Field(default_factory=list)
    customer_journey_insights: List[str] = Field(default_factory=list)
    unmet_needs: List[str] = Field(default_factory=list)
    market_validation_evidence: List[str] = Field(default_factory=list)
    confidence_score: float = Field(default=0.7, ge=0.0, le=1.0)

# Strategic Assessment
class SwotAnalysisEnhanced(BaseModel):
    strengths: List[str] = Field(default_factory=list)
    weaknesses: List[str] = Field(default_factory=list)
    opportunities: List[str] = Field(default_factory=list)
    threats: List[str] = Field(default_factory=list)
    strategic_implications: List[str] = Field(default_factory=list)
    critical_success_factors: List[str] = Field(default_factory=list)
    confidence_score: float = Field(default=0.7, ge=0.0, le=1.0)

class OpportunityScoring(BaseModel):
    market_opportunity_score: float = Field(..., ge=0.0, le=10.0)
    competitive_position_score: float = Field(..., ge=0.0, le=10.0)
    execution_feasibility_score: float = Field(..., ge=0.0, le=10.0)
    financial_potential_score: float = Field(..., ge=0.0, le=10.0)
    overall_score: float = Field(..., ge=0.0, le=10.0)
    risk_level: str = Field(..., pattern="^(low|medium|high|critical)$")

class StrategicAssessment(BaseModel):
    swot_analysis: SwotAnalysisEnhanced
    opportunity_scoring: OpportunityScoring
    strategic_fit_analysis: str
    key_assumptions: List[str] = Field(default_factory=list)
    validation_requirements: List[str] = Field(default_factory=list)
    go_no_go_recommendation: str = Field(..., pattern="^(go|no_go|conditional)$")
    reasoning: str
    confidence_score: float = Field(default=0.7, ge=0.0, le=1.0)

# Strategic Options
class ResourceRequirement(BaseModel):
    category: str = Field(..., pattern="^(financial|human|technical|operational|partnership)$")
    description: str
    estimated_cost_usd: Optional[float] = None
    timeline_months: Optional[int] = None
    criticality: str = Field(..., pattern="^(nice_to_have|important|critical)$")

class SuccessMetric(BaseModel):
    metric_name: str
    target_value: Union[str, float, int]
    timeframe: str
    measurement_method: str

class StrategicOption(BaseModel):
    approach: StrategicApproach
    title: str
    description: str
    target_customer_segment: str
    value_proposition: str
    go_to_market_strategy: str
    
    # Investment & Timeline
    estimated_investment_usd: Optional[float] = None
    timeline_to_market_months: int = Field(..., gt=0)
    timeline_to_profitability_months: Optional[int] = None
    
    # Success Probability
    success_probability_percent: float = Field(..., ge=0.0, le=100.0)
    risk_factors: List[str] = Field(default_factory=list)
    mitigation_strategies: List[str] = Field(default_factory=list)
    
    # Resources & Metrics
    resource_requirements: List[ResourceRequirement] = Field(default_factory=list)
    success_metrics: List[SuccessMetric] = Field(default_factory=list)
    
    # Analysis Components
    swot_analysis: Optional[SwotAnalysisEnhanced] = None
    customer_segments: List[CustomerSegment] = Field(default_factory=list)
    competitive_positioning: str
    
    # Scoring
    overall_score: float = Field(default=0.0, ge=0.0, le=10.0)
    recommended: bool = Field(default=False)

# Unified Analysis Result
class ResearchAnalysisComplete(BaseModel):
    strategy_id: int
    approach: ResearchApproach
    
    # Analysis Components
    market_context: Optional[MarketContext] = None
    competitive_intelligence: Optional[CompetitiveIntelligence] = None
    customer_understanding: Optional[CustomerUnderstanding] = None
    strategic_assessment: Optional[StrategicAssessment] = None
    
    # Strategic Options
    strategic_options: List[StrategicOption] = Field(default_factory=list)
    recommended_option: Optional[StrategicOption] = None
    
    # Metadata
    analysis_confidence: float = Field(default=0.7, ge=0.0, le=1.0)
    analysis_completeness: float = Field(default=0.0, ge=0.0, le=100.0)
    next_steps: List[str] = Field(default_factory=list)
    generated_at: datetime

# Request/Response Schemas
class ResearchStrategyRequest(BaseModel):
    session_id: int
    approach: ResearchApproach
    custom_parameters: Optional[Dict[str, Any]] = None

class ResearchStrategyResponse(BaseModel):
    strategy: ResearchStrategy
    estimated_completion_time: str
    included_analyses: List[AnalysisPhase]
    next_steps: List[str]

class AnalysisProgressUpdate(BaseModel):
    strategy_id: int
    current_phase: AnalysisPhase
    progress_percentage: float = Field(..., ge=0.0, le=100.0)
    phase_results: Optional[Dict[str, Any]] = None
    estimated_completion_minutes: Optional[int] = None

class StrategicOptionComparison(BaseModel):
    session_id: int
    options: List[StrategicOption]
    comparison_criteria: List[str] = Field(default_factory=list)
    recommendation_reasoning: str
    trade_off_analysis: Dict[str, str] = Field(default_factory=dict)

# Export Schemas
class ResearchExportRequest(BaseModel):
    session_id: int
    strategy_id: Optional[int] = None
    export_format: str = Field(..., pattern="^(pdf|docx|json|csv)$")
    include_sections: List[str] = Field(default_factory=list)
    
class ResearchExportResponse(BaseModel):
    download_url: str
    file_name: str
    file_size_bytes: int
    expires_at: datetime