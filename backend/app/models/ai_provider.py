from sqlalchemy import Column, Integer, String, Boolean, DateTime, JSON, Text
from sqlalchemy.sql import func
from app.core.database import Base

class AIProvider(Base):
    __tablename__ = "ai_providers"
    
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String(255), nullable=False)
    type = Column(String(50), nullable=False)  # openai, claude, gemini, azure_openai
    enabled = Column(Boolean, default=True)
    config = Column(JSON, default=dict)  # Encrypted API keys and settings
    tenant_id = Column(String(100), nullable=True, index=True)  # null = system-wide
    
    # Status tracking
    status = Column(String(20), default="inactive")  # active, inactive, error
    last_tested = Column(DateTime(timezone=True), nullable=True)
    last_error = Column(Text, nullable=True)
    
    # Usage tracking
    total_requests = Column(Integer, default=0)
    total_tokens = Column(Integer, default=0)
    total_cost = Column(Integer, default=0)  # In cents
    
    # Metadata
    created_by = Column(Integer, nullable=True)  # User ID
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())