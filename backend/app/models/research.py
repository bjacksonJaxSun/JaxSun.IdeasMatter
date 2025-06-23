from sqlalchemy import Column, Integer, String, Text, DateTime, ForeignKey, Boolean, Float, JSON
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.core.database import Base

class ResearchSession(Base):
    __tablename__ = "research_sessions"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, index=True)
    idea_id = Column(String, index=True)
    title = Column(String(255), nullable=False)
    description = Column(Text)
    status = Column(String(50), default="active")  # active, completed, archived
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())
    
    # Relationships
    conversations = relationship("ResearchConversation", back_populates="session", cascade="all, delete-orphan")
    insights = relationship("ResearchInsight", back_populates="session", cascade="all, delete-orphan")
    options = relationship("ResearchOption", back_populates="session", cascade="all, delete-orphan")

class ResearchConversation(Base):
    __tablename__ = "research_conversations"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    message_type = Column(String(20), nullable=False)  # user, assistant, system
    content = Column(Text, nullable=False)
    message_metadata = Column(JSON)  # Store additional context, sources, etc.
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    
    # Relationships
    session = relationship("ResearchSession", back_populates="conversations")

class ResearchInsight(Base):
    __tablename__ = "research_insights"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    category = Column(String(50), nullable=False)  # target_market, customer_profile, problem_solution, growth_targets, cost_model, revenue_model
    subcategory = Column(String(100))
    title = Column(String(255), nullable=False)
    description = Column(Text)
    data = Column(JSON)  # Structured data for the insight
    confidence_score = Column(Float, default=0.5)  # 0.0 to 1.0
    sources = Column(JSON)  # Array of source URLs/references
    is_validated = Column(Boolean, default=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())
    
    # Relationships
    session = relationship("ResearchSession", back_populates="insights")

class ResearchOption(Base):
    __tablename__ = "research_options"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    category = Column(String(50), nullable=False)  # business_model, target_market, pricing_strategy, etc.
    title = Column(String(255), nullable=False)
    description = Column(Text)
    pros = Column(JSON)  # Array of pros
    cons = Column(JSON)  # Array of cons
    feasibility_score = Column(Float, default=0.5)  # 0.0 to 1.0
    impact_score = Column(Float, default=0.5)  # 0.0 to 1.0
    risk_score = Column(Float, default=0.5)  # 0.0 to 1.0
    recommended = Column(Boolean, default=False)
    option_metadata = Column(JSON)  # Additional structured data
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())
    
    # Relationships
    session = relationship("ResearchSession", back_populates="options")

class ResearchReport(Base):
    __tablename__ = "research_reports"
    
    id = Column(Integer, primary_key=True, index=True)
    session_id = Column(Integer, ForeignKey("research_sessions.id"), nullable=False)
    report_type = Column(String(50), nullable=False)  # summary, detailed, competitive_analysis, financial_projection
    title = Column(String(255), nullable=False)
    content = Column(Text)
    data = Column(JSON)  # Chart data, metrics, etc.
    generated_at = Column(DateTime(timezone=True), server_default=func.now())
    
    # Relationships
    session = relationship("ResearchSession")

class ResearchFactCheck(Base):
    __tablename__ = "research_fact_checks"
    
    id = Column(Integer, primary_key=True, index=True)
    insight_id = Column(Integer, ForeignKey("research_insights.id"), nullable=False)
    claim = Column(Text, nullable=False)
    verification_status = Column(String(20), default="pending")  # verified, disputed, unverified, pending
    sources = Column(JSON)  # Verification sources
    confidence_level = Column(String(20), default="medium")  # high, medium, low
    notes = Column(Text)
    checked_at = Column(DateTime(timezone=True), server_default=func.now())
    
    # Relationships
    insight = relationship("ResearchInsight")