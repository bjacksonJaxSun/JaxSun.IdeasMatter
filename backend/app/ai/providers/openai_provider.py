from typing import List, Dict, Any, Optional
import openai
from openai import AsyncOpenAI

from app.ai.providers import AIProvider
from app.core.config import get_settings

settings = get_settings()

class OpenAIProvider(AIProvider):
    """OpenAI API provider implementation"""
    
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)
        self.model = "gpt-4"
        self.embedding_model = "text-embedding-ada-002"
    
    async def complete(self, prompt: str, **kwargs) -> str:
        """Generate text completion using OpenAI"""
        response = await self.client.chat.completions.create(
            model=kwargs.get("model", self.model),
            messages=[{"role": "user", "content": prompt}],
            temperature=kwargs.get("temperature", 0.7),
            max_tokens=kwargs.get("max_tokens", 2000)
        )
        return response.choices[0].message.content
    
    async def embed(self, text: str) -> List[float]:
        """Generate embeddings using OpenAI"""
        response = await self.client.embeddings.create(
            model=self.embedding_model,
            input=text
        )
        return response.data[0].embedding
    
    async def analyze(self, text: str, analysis_type: str) -> Dict[str, Any]:
        """Analyze text for specific purposes"""
        prompts = {
            "requirements": f"Analyze the following text and extract key requirements:\n\n{text}",
            "features": f"Suggest features based on the following description:\n\n{text}",
            "stories": f"Create user stories based on the following requirements:\n\n{text}"
        }
        
        prompt = prompts.get(analysis_type, f"Analyze the following text:\n\n{text}")
        result = await self.complete(prompt)
        
        return {"type": analysis_type, "result": result}
    
    async def generate(self, task: str, context: Dict[str, Any]) -> str:
        """Generate content based on task and context"""
        context_str = "\n".join([f"{k}: {v}" for k, v in context.items()])
        prompt = f"Task: {task}\n\nContext:\n{context_str}\n\nPlease complete the task based on the provided context."
        
        return await self.complete(prompt)