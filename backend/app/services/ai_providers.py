from typing import Dict, Any, Optional, List
from abc import ABC, abstractmethod
import os
import json
from openai import AsyncOpenAI
import anthropic
import google.generativeai as genai
from langchain.chat_models import ChatOpenAI, ChatAnthropic
from langchain.schema import HumanMessage, SystemMessage
from cryptography.fernet import Fernet

from app.core.config import get_settings
from app.models.ai_provider import AIProvider

settings = get_settings()

class BaseAIProvider(ABC):
    """Base class for AI providers"""
    
    @abstractmethod
    async def generate(self, prompt: str, **kwargs) -> str:
        """Generate a response from the AI provider"""
        pass
    
    @abstractmethod
    async def validate_api_key(self) -> bool:
        """Validate if the API key is working"""
        pass

class OpenAIProvider(BaseAIProvider):
    def __init__(self, api_key: str, model: str = "gpt-4-turbo-preview", **kwargs):
        self.client = AsyncOpenAI(api_key=api_key)
        self.model = model
        self.temperature = kwargs.get('temperature', 0.7)
        self.max_tokens = kwargs.get('max_tokens', 2000)
    
    async def generate(self, prompt: str, **kwargs) -> str:
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[{"role": "user", "content": prompt}],
                temperature=kwargs.get('temperature', self.temperature),
                max_tokens=kwargs.get('max_tokens', self.max_tokens)
            )
            return response.choices[0].message.content
        except Exception as e:
            raise Exception(f"OpenAI API error: {str(e)}")
    
    async def validate_api_key(self) -> bool:
        try:
            # Try a simple completion to validate the key
            await self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[{"role": "user", "content": "Hi"}],
                max_tokens=5
            )
            return True
        except:
            return False

class AnthropicProvider(BaseAIProvider):
    def __init__(self, api_key: str, model: str = "claude-3-opus-20240229", **kwargs):
        self.client = anthropic.AsyncAnthropic(api_key=api_key)
        self.model = model
        self.temperature = kwargs.get('temperature', 0.7)
        self.max_tokens = kwargs.get('max_tokens', 2000)
    
    async def generate(self, prompt: str, **kwargs) -> str:
        try:
            response = await self.client.messages.create(
                model=self.model,
                messages=[{"role": "user", "content": prompt}],
                max_tokens=kwargs.get('max_tokens', self.max_tokens),
                temperature=kwargs.get('temperature', self.temperature)
            )
            return response.content[0].text
        except Exception as e:
            raise Exception(f"Anthropic API error: {str(e)}")
    
    async def validate_api_key(self) -> bool:
        try:
            await self.client.messages.create(
                model="claude-3-haiku-20240307",
                messages=[{"role": "user", "content": "Hi"}],
                max_tokens=5
            )
            return True
        except:
            return False

class GeminiProvider(BaseAIProvider):
    def __init__(self, api_key: str, model: str = "gemini-pro", **kwargs):
        genai.configure(api_key=api_key)
        self.model = genai.GenerativeModel(model)
        self.temperature = kwargs.get('temperature', 0.7)
        self.max_tokens = kwargs.get('max_tokens', 2000)
    
    async def generate(self, prompt: str, **kwargs) -> str:
        try:
            generation_config = genai.types.GenerationConfig(
                temperature=kwargs.get('temperature', self.temperature),
                max_output_tokens=kwargs.get('max_tokens', self.max_tokens)
            )
            response = await self.model.generate_content_async(
                prompt,
                generation_config=generation_config
            )
            return response.text
        except Exception as e:
            raise Exception(f"Gemini API error: {str(e)}")
    
    async def validate_api_key(self) -> bool:
        try:
            await self.model.generate_content_async("Hi")
            return True
        except:
            return False

class AzureOpenAIProvider(BaseAIProvider):
    def __init__(self, api_key: str, endpoint: str, deployment: str, **kwargs):
        self.client = AsyncOpenAI(
            api_key=api_key,
            base_url=f"{endpoint}/openai/deployments/{deployment}",
            api_version=kwargs.get('api_version', '2024-02-15-preview'),
            default_headers={"api-key": api_key}
        )
        self.model = deployment
        self.temperature = kwargs.get('temperature', 0.7)
        self.max_tokens = kwargs.get('max_tokens', 2000)
    
    async def generate(self, prompt: str, **kwargs) -> str:
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[{"role": "user", "content": prompt}],
                temperature=kwargs.get('temperature', self.temperature),
                max_tokens=kwargs.get('max_tokens', self.max_tokens)
            )
            return response.choices[0].message.content
        except Exception as e:
            raise Exception(f"Azure OpenAI API error: {str(e)}")
    
    async def validate_api_key(self) -> bool:
        try:
            await self.client.chat.completions.create(
                model=self.model,
                messages=[{"role": "user", "content": "Hi"}],
                max_tokens=5
            )
            return True
        except:
            return False

class AIProviderManager:
    """Manages AI provider instances and encryption"""
    
    def __init__(self):
        self.providers: Dict[str, BaseAIProvider] = {}
        self.fernet = Fernet(settings.fernet_key)
    
    def encrypt_api_key(self, api_key: str) -> str:
        """Encrypt an API key for storage"""
        return self.fernet.encrypt(api_key.encode()).decode()
    
    def decrypt_api_key(self, encrypted_key: str) -> str:
        """Decrypt an API key from storage"""
        return self.fernet.decrypt(encrypted_key.encode()).decode()
    
    async def load_provider(self, provider_config: AIProvider) -> BaseAIProvider:
        """Load and initialize an AI provider from database config"""
        # Decrypt API key
        api_key = self.decrypt_api_key(provider_config.config.get('api_key', ''))
        
        if provider_config.type == 'openai':
            return OpenAIProvider(
                api_key=api_key,
                model=provider_config.config.get('model', 'gpt-4-turbo-preview'),
                temperature=provider_config.config.get('temperature', 0.7),
                max_tokens=provider_config.config.get('max_tokens', 2000)
            )
        elif provider_config.type == 'claude':
            return AnthropicProvider(
                api_key=api_key,
                model=provider_config.config.get('model', 'claude-3-opus-20240229'),
                temperature=provider_config.config.get('temperature', 0.7),
                max_tokens=provider_config.config.get('max_tokens', 2000)
            )
        elif provider_config.type == 'gemini':
            return GeminiProvider(
                api_key=api_key,
                model=provider_config.config.get('model', 'gemini-pro'),
                temperature=provider_config.config.get('temperature', 0.7),
                max_tokens=provider_config.config.get('max_tokens', 2000)
            )
        elif provider_config.type == 'azure_openai':
            return AzureOpenAIProvider(
                api_key=api_key,
                endpoint=provider_config.config.get('endpoint'),
                deployment=provider_config.config.get('deployment'),
                api_version=provider_config.config.get('api_version', '2024-02-15-preview'),
                temperature=provider_config.config.get('temperature', 0.7),
                max_tokens=provider_config.config.get('max_tokens', 2000)
            )
        else:
            raise ValueError(f"Unknown provider type: {provider_config.type}")
    
    async def get_provider(self, provider_id: str) -> BaseAIProvider:
        """Get a cached provider instance or load it"""
        if provider_id not in self.providers:
            # Load from database
            # This would fetch the provider config from DB
            # For now, using environment variables as fallback
            raise NotImplementedError("Provider loading from DB not implemented")
        
        return self.providers[provider_id]
    
    async def test_provider(self, provider: BaseAIProvider) -> bool:
        """Test if a provider is working"""
        return await provider.validate_api_key()

# Global instance
ai_provider_manager = AIProviderManager()