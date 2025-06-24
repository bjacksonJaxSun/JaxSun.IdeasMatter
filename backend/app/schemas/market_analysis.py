from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field
from datetime import datetime
from enum import Enum

# Enums
class MarketSizeType(str, Enum):
    TAM = "tam"
    SAM = "sam"
    SOM = "som"

class CompetitorTier(str, Enum):
    DIRECT = "direct"
    INDIRECT = "indirect"
    SUBSTITUTE = "substitute"

class MarketTrend(str, Enum):
    GROWING = "growing"
    DECLINING = "declining"
    STABLE = "stable"
    EMERGING = "emerging"
    DISRUPTING = "disrupting"

# Market Analysis Schemas
class MarketAnalysisBase(BaseModel):
    industry: Optional[str] = None
    market_category: Optional[str] = None
    geographic_scope: Optional[str] = Field(None, description="Global, Regional, National, Local")
    target_demographics: Optional[Dict[str, Any]] = None
    
    # Market Size
    tam_value: Optional[float] = Field(None, description="Total Addressable Market in USD")
    sam_value: Optional[float] = Field(None, description="Serviceable Addressable Market in USD")
    som_value: Optional[float] = Field(None, description="Serviceable Obtainable Market in USD")
    market_size_year: Optional[int] = None
    market_size_source: Optional[str] = None
    
    # Growth Metrics
    cagr: Optional[float] = Field(None, description="Compound Annual Growth Rate (%)")
    growth_period: Optional[str] = None
    market_maturity: Optional[str] = None
    
    # Market Dynamics
    market_drivers: Optional[List[str]] = None
    market_barriers: Optional[List[str]] = None
    regulatory_factors: Optional[List[str]] = None
    technology_trends: Optional[List[str]] = None
    
    # Customer Analysis
    customer_segments: Optional[List[Dict[str, Any]]] = None
    customer_pain_points: Optional[List[str]] = None
    buying_behavior: Optional[Dict[str, Any]] = None
    price_sensitivity: Optional[float] = Field(None, ge=0.0, le=1.0)
    
    confidence_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    data_sources: Optional[List[str]] = None

class MarketAnalysisCreate(MarketAnalysisBase):
    session_id: int

class MarketAnalysisUpdate(BaseModel):
    industry: Optional[str] = None
    market_category: Optional[str] = None
    geographic_scope: Optional[str] = None
    target_demographics: Optional[Dict[str, Any]] = None
    tam_value: Optional[float] = None
    sam_value: Optional[float] = None
    som_value: Optional[float] = None
    cagr: Optional[float] = None
    market_maturity: Optional[str] = None
    confidence_score: Optional[float] = Field(None, ge=0.0, le=1.0)

class MarketAnalysis(MarketAnalysisBase):
    id: int
    session_id: int
    analysis_date: datetime
    last_updated: Optional[datetime] = None
    
    class Config:
        from_attributes = True

# Competitor Analysis Schemas
class CompetitorAnalysisBase(BaseModel):
    name: str = Field(..., min_length=1)
    website: Optional[str] = None
    description: Optional[str] = None
    tier: CompetitorTier = CompetitorTier.DIRECT
    
    # Market Position
    market_share: Optional[float] = Field(None, ge=0.0, le=100.0)
    revenue: Optional[float] = None
    employees: Optional[int] = None
    founding_year: Optional[int] = None
    headquarters: Optional[str] = None
    
    # Product/Service Analysis
    products_services: Optional[List[str]] = None
    pricing_model: Optional[str] = None
    price_range: Optional[Dict[str, float]] = None
    target_customers: Optional[List[str]] = None
    
    # Strategic Analysis
    strengths: Optional[List[str]] = None
    weaknesses: Optional[List[str]] = None
    competitive_advantages: Optional[List[str]] = None
    
    # Performance Metrics
    funding_raised: Optional[float] = None
    growth_rate: Optional[float] = None
    threat_level: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    
    # Digital Presence
    website_traffic: Optional[int] = None
    social_media_followers: Optional[Dict[str, int]] = None
    online_ratings: Optional[float] = Field(None, ge=1.0, le=5.0)

class CompetitorAnalysisCreate(CompetitorAnalysisBase):
    market_analysis_id: int

class CompetitorAnalysis(CompetitorAnalysisBase):
    id: int
    market_analysis_id: int
    data_completeness: float
    last_researched: datetime
    
    class Config:
        from_attributes = True

# Market Segment Schemas
class MarketSegmentBase(BaseModel):
    segment_name: str = Field(..., min_length=1)
    description: Optional[str] = None
    size_value: Optional[float] = None
    size_percentage: Optional[float] = Field(None, ge=0.0, le=100.0)
    
    # Demographics
    age_range: Optional[str] = None
    income_range: Optional[str] = None
    geographic_focus: Optional[str] = None
    
    # Behavioral Characteristics
    behavior_traits: Optional[List[str]] = None
    preferred_channels: Optional[List[str]] = None
    decision_factors: Optional[List[str]] = None
    
    # Business Metrics
    acquisition_cost: Optional[float] = None
    lifetime_value: Optional[float] = None
    conversion_rate: Optional[float] = Field(None, ge=0.0, le=1.0)
    
    # Strategic Assessment
    attractiveness_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    accessibility_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    competition_intensity: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    
    priority_level: Optional[str] = None
    entry_strategy: Optional[str] = None

class MarketSegmentCreate(MarketSegmentBase):
    market_analysis_id: int

class MarketSegment(MarketSegmentBase):
    id: int
    market_analysis_id: int
    
    class Config:
        from_attributes = True

# Market Trend Analysis Schemas
class MarketTrendAnalysisBase(BaseModel):
    trend_name: str = Field(..., min_length=1)
    trend_type: MarketTrend = MarketTrend.EMERGING
    description: Optional[str] = None
    
    # Impact Analysis
    impact_level: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    time_horizon: Optional[str] = None
    affected_segments: Optional[List[str]] = None
    
    # Trend Metrics
    growth_velocity: Optional[float] = None
    market_penetration: Optional[float] = Field(None, ge=0.0, le=100.0)
    adoption_barriers: Optional[List[str]] = None
    
    # Strategic Implications
    opportunities: Optional[List[str]] = None
    threats: Optional[List[str]] = None
    strategic_responses: Optional[List[str]] = None
    
    supporting_data: Optional[Dict[str, Any]] = None
    data_sources: Optional[List[str]] = None
    confidence_level: Optional[float] = Field(0.5, ge=0.0, le=1.0)

class MarketTrendAnalysisCreate(MarketTrendAnalysisBase):
    session_id: int

class MarketTrendAnalysis(MarketTrendAnalysisBase):
    id: int
    session_id: int
    trend_start_date: Optional[datetime] = None
    projected_peak_date: Optional[datetime] = None
    analysis_date: datetime
    
    class Config:
        from_attributes = True

# Market Opportunity Schemas
class MarketOpportunityBase(BaseModel):
    title: str = Field(..., min_length=1)
    description: Optional[str] = None
    opportunity_type: Optional[str] = None
    
    # Market Metrics
    market_size: Optional[float] = None
    addressable_percentage: Optional[float] = Field(None, ge=0.0, le=100.0)
    time_to_market: Optional[int] = None
    
    # Competitive Landscape
    current_solutions: Optional[List[str]] = None
    competition_level: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    barriers_to_entry: Optional[List[str]] = None
    
    # Assessment Scores
    attractiveness_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    feasibility_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    urgency_score: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    
    # Strategic Analysis
    required_capabilities: Optional[List[str]] = None
    investment_required: Optional[float] = None
    potential_roi: Optional[float] = None
    
    # Risk Assessment
    risk_factors: Optional[List[str]] = None
    mitigation_strategies: Optional[List[str]] = None
    probability_of_success: Optional[float] = Field(0.5, ge=0.0, le=1.0)
    
    # Action Plan
    recommended_actions: Optional[List[str]] = None
    key_milestones: Optional[List[str]] = None
    
    priority_level: Optional[str] = None
    status: Optional[str] = "identified"

class MarketOpportunityCreate(MarketOpportunityBase):
    session_id: int

class MarketOpportunity(MarketOpportunityBase):
    id: int
    session_id: int
    identified_date: datetime
    
    class Config:
        from_attributes = True

# Combined Analysis Response Schemas
class ComprehensiveMarketAnalysis(BaseModel):
    market_analysis: Optional[MarketAnalysis] = None
    competitors: List[CompetitorAnalysis] = []
    segments: List[MarketSegment] = []
    trends: List[MarketTrendAnalysis] = []
    opportunities: List[MarketOpportunity] = []

class MarketSizingAnalysis(BaseModel):
    tam: Optional[float] = None
    sam: Optional[float] = None
    som: Optional[float] = None
    market_size_breakdown: Dict[str, float] = {}
    growth_projections: Dict[str, float] = {}
    assumptions: List[str] = []
    confidence_level: float = 0.5

class CompetitiveLandscape(BaseModel):
    direct_competitors: List[CompetitorAnalysis] = []
    indirect_competitors: List[CompetitorAnalysis] = []
    substitute_threats: List[CompetitorAnalysis] = []
    market_leader: Optional[CompetitorAnalysis] = None
    competitive_intensity: float = 0.5
    market_concentration: str = "fragmented"  # fragmented, moderately_concentrated, highly_concentrated

# Request Schemas
class MarketAnalysisRequest(BaseModel):
    session_id: int
    analysis_type: str = Field(..., description="Type of analysis: comprehensive, sizing, competitive, trends, or opportunities")
    parameters: Optional[Dict[str, Any]] = None

class CompetitorResearchRequest(BaseModel):
    competitor_name: str
    research_depth: str = Field("standard", description="Research depth: basic, standard, or comprehensive")
    include_financials: bool = True
    include_digital_presence: bool = True

class MarketSizingRequest(BaseModel):
    session_id: int
    geographic_scope: str = "global"
    target_segments: Optional[List[str]] = None
    time_horizon: int = 5  # years