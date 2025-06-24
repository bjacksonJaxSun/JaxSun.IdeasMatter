from sqlalchemy import Column, Integer, String, Text, DateTime, ForeignKey, Boolean, Float, JSON, Enum
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.core.database import Base
import enum

class MarketSizeType(enum.Enum):
    TAM = "tam"  # Total Addressable Market
    SAM = "sam"  # Serviceable Addressable Market
    SOM = "som"  # Serviceable Obtainable Market

class CompetitorTier(enum.Enum):
    DIRECT = "direct"
    INDIRECT = "indirect"
    SUBSTITUTE = "substitute"

class MarketTrend(enum.Enum):
    GROWING = "growing"
    DECLINING = "declining"
    STABLE = "stable"
    EMERGING = "emerging"
    DISRUPTING = "disrupting"

class MarketAnalysis(Base):
    __tablename__ = "market_analyses"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    
    # Market Overview
    industry = Column(String(255))
    market_category = Column(String(255))
    geographic_scope = Column(String(100))  # Global, Regional, National, Local
    target_demographics = Column(JSON)  # Age, income, behavior segments
    
    # Market Size Analysis
    tam_value = Column(Float)  # Total Addressable Market in USD
    sam_value = Column(Float)  # Serviceable Addressable Market in USD
    som_value = Column(Float)  # Serviceable Obtainable Market in USD
    market_size_year = Column(Integer)  # Year of market size data
    market_size_source = Column(String(500))  # Source of market data
    
    # Growth Metrics
    cagr = Column(Float)  # Compound Annual Growth Rate (%)
    growth_period = Column(String(50))  # e.g., "2024-2029"
    market_maturity = Column(String(50))  # Emerging, Growth, Mature, Declining
    
    # Market Dynamics
    market_drivers = Column(JSON)  # Key market growth drivers
    market_barriers = Column(JSON)  # Market entry barriers
    regulatory_factors = Column(JSON)  # Regulatory considerations
    technology_trends = Column(JSON)  # Technology disruptions
    
    # Customer Analysis
    customer_segments = Column(JSON)  # Detailed customer segments
    customer_pain_points = Column(JSON)  # Key pain points
    buying_behavior = Column(JSON)  # How customers buy
    price_sensitivity = Column(Float)  # 0.0-1.0 scale
    
    # Confidence and Sources
    confidence_score = Column(Float, default=0.5)  # 0.0-1.0
    data_sources = Column(JSON)  # List of data sources
    analysis_date = Column(DateTime(timezone=True), server_default=func.now())
    last_updated = Column(DateTime(timezone=True), onupdate=func.now())
    
    # Relationships
    session = relationship("ResearchSession")
    competitors = relationship("CompetitorAnalysis", back_populates="market_analysis", cascade="all, delete-orphan")
    segments = relationship("MarketSegment", back_populates="market_analysis", cascade="all, delete-orphan")

class CompetitorAnalysis(Base):
    __tablename__ = "competitor_analyses"
    
    id = Column(Integer, primary_key=True, index=True)
    market_analysis_id = Column(Integer, ForeignKey("market_analyses.id"), nullable=False)
    
    # Competitor Details
    name = Column(String(255), nullable=False)
    website = Column(String(500))
    description = Column(Text)
    tier = Column(Enum(CompetitorTier), default=CompetitorTier.DIRECT)
    
    # Market Position
    market_share = Column(Float)  # Percentage (0-100)
    revenue = Column(Float)  # Annual revenue in USD
    employees = Column(Integer)
    founding_year = Column(Integer)
    headquarters = Column(String(255))
    
    # Product/Service Analysis
    products_services = Column(JSON)  # List of main offerings
    pricing_model = Column(String(255))
    price_range = Column(JSON)  # Min/max pricing
    target_customers = Column(JSON)  # Customer segments
    
    # Strategic Analysis
    strengths = Column(JSON)  # Key strengths
    weaknesses = Column(JSON)  # Key weaknesses
    competitive_advantages = Column(JSON)  # Unique advantages
    
    # Performance Metrics
    funding_raised = Column(Float)  # Total funding in USD
    growth_rate = Column(Float)  # Annual growth rate %
    threat_level = Column(Float, default=0.5)  # 0.0-1.0 scale
    
    # Digital Presence
    website_traffic = Column(Integer)  # Monthly visitors
    social_media_followers = Column(JSON)  # Platform-specific followers
    online_ratings = Column(Float)  # Average rating (1-5)
    
    # Data Quality
    data_completeness = Column(Float, default=0.5)  # 0.0-1.0
    last_researched = Column(DateTime(timezone=True), server_default=func.now())
    
    # Relationships
    market_analysis = relationship("MarketAnalysis", back_populates="competitors")

class MarketSegment(Base):
    __tablename__ = "market_segments"
    
    id = Column(Integer, primary_key=True, index=True)
    market_analysis_id = Column(Integer, ForeignKey("market_analyses.id"), nullable=False)
    
    # Segment Details
    segment_name = Column(String(255), nullable=False)
    description = Column(Text)
    size_value = Column(Float)  # Segment size in USD
    size_percentage = Column(Float)  # Percentage of total market
    
    # Demographics
    age_range = Column(String(50))  # e.g., "25-35"
    income_range = Column(String(100))  # e.g., "$50k-$100k"
    geographic_focus = Column(String(255))
    
    # Behavioral Characteristics
    behavior_traits = Column(JSON)  # Behavioral patterns
    preferred_channels = Column(JSON)  # Sales/marketing channels
    decision_factors = Column(JSON)  # What influences buying decisions
    
    # Business Metrics
    acquisition_cost = Column(Float)  # Customer acquisition cost
    lifetime_value = Column(Float)  # Customer lifetime value
    conversion_rate = Column(Float)  # Expected conversion rate
    
    # Strategic Assessment
    attractiveness_score = Column(Float, default=0.5)  # 0.0-1.0
    accessibility_score = Column(Float, default=0.5)  # How easy to reach
    competition_intensity = Column(Float, default=0.5)  # Competition level
    
    # Priority and Strategy
    priority_level = Column(String(50))  # Primary, Secondary, Tertiary
    entry_strategy = Column(Text)  # How to enter this segment
    
    # Relationships
    market_analysis = relationship("MarketAnalysis", back_populates="segments")

class MarketTrendAnalysis(Base):
    __tablename__ = "market_trend_analyses"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    
    # Trend Details
    trend_name = Column(String(255), nullable=False)
    trend_type = Column(Enum(MarketTrend), default=MarketTrend.EMERGING)
    description = Column(Text)
    
    # Impact Analysis
    impact_level = Column(Float, default=0.5)  # 0.0-1.0 scale
    time_horizon = Column(String(50))  # Short-term, Medium-term, Long-term
    affected_segments = Column(JSON)  # Which market segments are affected
    
    # Trend Metrics
    growth_velocity = Column(Float)  # Speed of trend adoption
    market_penetration = Column(Float)  # Current penetration %
    adoption_barriers = Column(JSON)  # Barriers to adoption
    
    # Strategic Implications
    opportunities = Column(JSON)  # Opportunities created by trend
    threats = Column(JSON)  # Threats from the trend
    strategic_responses = Column(JSON)  # Recommended responses
    
    # Data and Analysis
    supporting_data = Column(JSON)  # Supporting statistics
    data_sources = Column(JSON)  # Sources of trend data
    confidence_level = Column(Float, default=0.5)  # Analysis confidence
    
    # Timeline
    trend_start_date = Column(DateTime(timezone=True))
    projected_peak_date = Column(DateTime(timezone=True))
    analysis_date = Column(DateTime(timezone=True), server_default=func.now())
    
    # Relationships
    session = relationship("ResearchSession")

class MarketOpportunity(Base):
    __tablename__ = "market_opportunities"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    
    # Opportunity Details
    title = Column(String(255), nullable=False)
    description = Column(Text)
    opportunity_type = Column(String(100))  # Gap, Underserved, New, Disruption
    
    # Market Metrics
    market_size = Column(Float)  # Opportunity size in USD
    addressable_percentage = Column(Float)  # % we can realistically address
    time_to_market = Column(Integer)  # Months to capitalize
    
    # Competitive Landscape
    current_solutions = Column(JSON)  # Existing solutions
    competition_level = Column(Float, default=0.5)  # 0.0-1.0
    barriers_to_entry = Column(JSON)  # Entry barriers
    
    # Assessment Scores
    attractiveness_score = Column(Float, default=0.5)  # Overall attractiveness
    feasibility_score = Column(Float, default=0.5)  # Implementation feasibility
    urgency_score = Column(Float, default=0.5)  # How urgent/time-sensitive
    
    # Strategic Analysis
    required_capabilities = Column(JSON)  # Capabilities needed
    investment_required = Column(Float)  # Investment needed
    potential_roi = Column(Float)  # Expected ROI %
    
    # Risk Assessment
    risk_factors = Column(JSON)  # Key risks
    mitigation_strategies = Column(JSON)  # Risk mitigation
    probability_of_success = Column(Float, default=0.5)  # Success probability
    
    # Action Plan
    recommended_actions = Column(JSON)  # Next steps
    key_milestones = Column(JSON)  # Important milestones
    
    # Metadata
    identified_date = Column(DateTime(timezone=True), server_default=func.now())
    priority_level = Column(String(50))  # High, Medium, Low
    status = Column(String(50), default="identified")  # identified, researching, planning, pursuing
    
    # Relationships
    session = relationship("ResearchSession")