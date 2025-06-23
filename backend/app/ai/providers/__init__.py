from abc import ABC, abstractmethod
from typing import List, Dict, Any, Optional

class AIProvider(ABC):
    """Abstract base class for AI providers"""
    
    @abstractmethod
    async def complete(self, prompt: str, **kwargs) -> str:
        """Generate text completion"""
        pass
    
    @abstractmethod
    async def embed(self, text: str) -> List[float]:
        """Generate embeddings for text"""
        pass
    
    @abstractmethod
    async def analyze(self, text: str, analysis_type: str) -> Dict[str, Any]:
        """Analyze text for specific purposes"""
        pass
    
    @abstractmethod
    async def generate(self, task: str, context: Dict[str, Any]) -> str:
        """Generate content based on task and context"""
        pass