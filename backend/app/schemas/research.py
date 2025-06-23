from typing import List, Optional, Dict, Any, Union
from pydantic import BaseModel, Field
from datetime import datetime

# Base schemas
class ResearchSessionBase(BaseModel):
    title: str = Field(..., min_length=1, max_length=255)
    description: Optional[str] = None
    idea_id: Optional[str] = None

class ResearchSessionCreate(ResearchSessionBase):
    pass

class ResearchSessionUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    status: Optional[str] = None

class ResearchSession(ResearchSessionBase):
    id: int
    user_id: int
    status: str
    created_at: datetime
    updated_at: Optional[datetime] = None
    
    class Config:
        from_attributes = True

# Conversation schemas
class ConversationMessage(BaseModel):
    message_type: str = Field(..., regex="^(user|assistant|system)$")
    content: str = Field(..., min_length=1)
    metadata: Optional[Dict[str, Any]] = None

class ResearchConversation(ConversationMessage):
    id: int
    session_id: int
    created_at: datetime
    
    class Config:
        from_attributes = True

# Insight schemas
class ResearchInsightBase(BaseModel):
    category: str = Field(..., regex="^(target_market|customer_profile|problem_solution|growth_targets|cost_model|revenue_model)$")
    subcategory: Optional[str] = None
    title: str = Field(..., min_length=1, max_length=255)
    description: Optional[str] = None
    data: Optional[Dict[str, Any]] = None
    confidence_score: Optional[float] = Field(default=0.5, ge=0.0, le=1.0)
    sources: Optional[List[str]] = None

class ResearchInsightCreate(ResearchInsightBase):
    session_id: int

class ResearchInsightUpdate(BaseModel):
    category: Optional[str] = None
    subcategory: Optional[str] = None
    title: Optional[str] = None
    description: Optional[str] = None
    data: Optional[Dict[str, Any]] = None
    confidence_score: Optional[float] = Field(None, ge=0.0, le=1.0)
    sources: Optional[List[str]] = None
    is_validated: Optional[bool] = None

class ResearchInsight(ResearchInsightBase):
    id: int
    session_id: int
    is_validated: bool
    created_at: datetime
    updated_at: Optional[datetime] = None
    
    class Config:
        from_attributes = True

# SWOT schemas
class SwotAnalysis(BaseModel):
    strengths: List[str] = Field(default_factory=list)
    weaknesses: List[str] = Field(default_factory=list)
    opportunities: List[str] = Field(default_factory=list)
    threats: List[str] = Field(default_factory=list)
    confidence: float = Field(default=0.7, ge=0.0, le=1.0)
    generated_at: Optional[datetime] = None

# Option schemas
class ResearchOptionBase(BaseModel):
    category: str = Field(..., min_length=1, max_length=50)
    title: str = Field(..., min_length=1, max_length=255)
    description: Optional[str] = None
    pros: Optional[List[str]] = None
    cons: Optional[List[str]] = None
    feasibility_score: Optional[float] = Field(default=0.5, ge=0.0, le=1.0)
    impact_score: Optional[float] = Field(default=0.5, ge=0.0, le=1.0)
    risk_score: Optional[float] = Field(default=0.5, ge=0.0, le=1.0)
    metadata: Optional[Dict[str, Any]] = None

class ResearchOptionCreate(ResearchOptionBase):
    session_id: int

class ResearchOptionUpdate(BaseModel):
    category: Optional[str] = None
    title: Optional[str] = None
    description: Optional[str] = None
    pros: Optional[List[str]] = None
    cons: Optional[List[str]] = None
    feasibility_score: Optional[float] = Field(None, ge=0.0, le=1.0)
    impact_score: Optional[float] = Field(None, ge=0.0, le=1.0)
    risk_score: Optional[float] = Field(None, ge=0.0, le=1.0)
    recommended: Optional[bool] = None
    metadata: Optional[Dict[str, Any]] = None

class ResearchOption(ResearchOptionBase):
    id: int
    session_id: int
    recommended: bool
    swot_strengths: Optional[List[str]] = None
    swot_weaknesses: Optional[List[str]] = None
    swot_opportunities: Optional[List[str]] = None
    swot_threats: Optional[List[str]] = None
    swot_generated_at: Optional[datetime] = None
    swot_confidence: Optional[float] = None
    created_at: datetime
    updated_at: Optional[datetime] = None
    
    class Config:
        from_attributes = True

# Report schemas
class ResearchReportBase(BaseModel):
    report_type: str = Field(..., regex="^(summary|detailed|competitive_analysis|financial_projection)$")
    title: str = Field(..., min_length=1, max_length=255)
    content: Optional[str] = None
    data: Optional[Dict[str, Any]] = None

class ResearchReportCreate(ResearchReportBase):
    session_id: int

class ResearchReport(ResearchReportBase):
    id: int
    session_id: int
    generated_at: datetime
    
    class Config:
        from_attributes = True

# Fact check schemas
class FactCheckCreate(BaseModel):
    insight_id: int
    claim: str = Field(..., min_length=1)

class FactCheck(BaseModel):
    id: int
    insight_id: int
    claim: str
    verification_status: str
    sources: Optional[List[str]] = None
    confidence_level: str
    notes: Optional[str] = None
    checked_at: datetime
    
    class Config:
        from_attributes = True

# Combined response schemas
class ResearchSessionDetail(ResearchSession):
    conversations: List[ResearchConversation] = []
    insights: List[ResearchInsight] = []
    options: List[ResearchOption] = []

class ResearchAnalysis(BaseModel):
    target_markets: List[ResearchInsight]
    customer_profiles: List[ResearchInsight]
    problems_solutions: List[ResearchInsight]
    growth_targets: List[ResearchInsight]
    cost_models: List[ResearchInsight]
    revenue_models: List[ResearchInsight]
    options: List[ResearchOption]
    recommendations: List[ResearchOption]

# AI interaction schemas
class BrainstormRequest(BaseModel):
    session_id: int
    message: str = Field(..., min_length=1)
    context: Optional[Dict[str, Any]] = None

class BrainstormResponse(BaseModel):
    message: str
    insights: Optional[List[ResearchInsight]] = None
    options: Optional[List[ResearchOption]] = None
    follow_up_questions: Optional[List[str]] = None
    metadata: Optional[Dict[str, Any]] = None

class AnalysisRequest(BaseModel):
    session_id: int
    analysis_type: str = Field(..., regex="^(market_analysis|competitive_analysis|financial_modeling|risk_assessment)$")
    parameters: Optional[Dict[str, Any]] = None

class ReportRequest(BaseModel):
    report_type: str
    include_charts: Optional[bool] = True
    format: Optional[str] = "json"

# SWOT analysis request
class SwotAnalysisRequest(BaseModel):
    option_id: int
    regenerate: Optional[bool] = False

class SwotAnalysisResponse(BaseModel):
    option_id: int
    swot: SwotAnalysis
    
class SwotPdfRequest(BaseModel):
    option_id: int
    include_metadata: Optional[bool] = True