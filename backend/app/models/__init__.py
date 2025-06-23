# Import all models to ensure they are registered with SQLAlchemy
from app.models.user import User
from app.models.research import (
    ResearchSession, 
    ResearchConversation, 
    ResearchInsight, 
    ResearchOption, 
    ResearchReport, 
    ResearchFactCheck
)
from app.models.ai_provider import AIProvider

__all__ = [
    "User",
    "ResearchSession",
    "ResearchConversation", 
    "ResearchInsight",
    "ResearchOption",
    "ResearchReport",
    "ResearchFactCheck",
    "AIProvider"
]