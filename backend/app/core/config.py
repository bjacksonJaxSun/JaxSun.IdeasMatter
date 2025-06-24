from typing import List, Optional
from pydantic_settings import BaseSettings
from pydantic import Field
import os
import secrets
from functools import lru_cache
from cryptography.fernet import Fernet

class Settings(BaseSettings):
    # Application
    APP_NAME: str = Field(default="Ideas Matter")
    APP_ENV: str = Field(default="development")
    DEBUG: bool = Field(default=True)
    API_PREFIX: str = Field(default="/api/v1")
    
    # Quick Launch Mode
    QUICK_LAUNCH: bool = Field(default=True)
    
    # Database
    DB_USERNAME: str = Field(default="root")
    DB_PASSWORD: str = Field(default="")
    DB_HOST: str = Field(default="localhost")
    DB_PORT: int = Field(default=3306)
    DB_NAME: str = Field(default="ideas_matter_db")
    
    # Redis
    REDIS_HOST: str = Field(default="localhost")
    REDIS_PORT: int = Field(default=6379)
    REDIS_PASSWORD: Optional[str] = Field(default=None)
    
    # Security
    JWT_SECRET_KEY: str = Field(default="")
    JWT_ALGORITHM: str = Field(default="HS256")
    ACCESS_TOKEN_EXPIRE_MINUTES: int = Field(default=30)
    REFRESH_TOKEN_EXPIRE_DAYS: int = Field(default=7)
    
    # Encryption for API keys
    ENCRYPTION_KEY: str = Field(default="")
    
    # Admin emails (comma-separated or list)
    ADMIN_EMAILS: str = Field(default="")
    
    # CORS
    CORS_ORIGINS: List[str] = Field(default=["http://localhost:4000"])
    
    # OAuth
    GOOGLE_CLIENT_ID: Optional[str] = Field(default=None)
    GOOGLE_CLIENT_SECRET: Optional[str] = Field(default=None)
    FACEBOOK_CLIENT_ID: Optional[str] = Field(default=None)
    FACEBOOK_CLIENT_SECRET: Optional[str] = Field(default=None)
    
    # AI Providers - These will be stored encrypted in the database
    # Default values here are for initial setup only
    OPENAI_API_KEY: Optional[str] = Field(default=None)
    AZURE_OPENAI_API_KEY: Optional[str] = Field(default=None)
    AZURE_OPENAI_ENDPOINT: Optional[str] = Field(default=None)
    AZURE_OPENAI_DEPLOYMENT: Optional[str] = Field(default=None)
    CLAUDE_API_KEY: Optional[str] = Field(default=None)
    GEMINI_API_KEY: Optional[str] = Field(default=None)
    
    # Email
    SMTP_HOST: str = Field(default="smtp.gmail.com")
    SMTP_PORT: int = Field(default=587)
    SMTP_USERNAME: Optional[str] = Field(default=None)
    SMTP_PASSWORD: Optional[str] = Field(default=None)
    
    # Rate Limiting
    RATE_LIMIT_ENABLED: bool = Field(default=True)
    RATE_LIMIT_PER_MINUTE: int = Field(default=60)
    
    # Logging
    LOG_LEVEL: str = Field(default="INFO")
    LOG_FILE: str = Field(default="app.log")
    
    @property
    def DATABASE_URL(self) -> str:
        if self.QUICK_LAUNCH:
            # Use SQLite for quick launch
            return "sqlite+aiosqlite:///./ideas_matter.db"
        return f"mysql+aiomysql://{self.DB_USERNAME}:{self.DB_PASSWORD}@{self.DB_HOST}:{self.DB_PORT}/{self.DB_NAME}"
    
    @property
    def REDIS_URL(self) -> str:
        if self.REDIS_PASSWORD:
            return f"redis://:{self.REDIS_PASSWORD}@{self.REDIS_HOST}:{self.REDIS_PORT}/0"
        return f"redis://{self.REDIS_HOST}:{self.REDIS_PORT}/0"
    
    @property
    def admin_email_list(self) -> List[str]:
        """Parse admin emails into a list"""
        if isinstance(self.ADMIN_EMAILS, str):
            return [email.strip() for email in self.ADMIN_EMAILS.split(",") if email.strip()]
        return self.ADMIN_EMAILS if self.ADMIN_EMAILS else []
    
    @property
    def fernet_key(self) -> bytes:
        """Get or generate Fernet encryption key"""
        if not self.ENCRYPTION_KEY:
            # Generate a new key if not provided
            return Fernet.generate_key()
        # The ENCRYPTION_KEY is already base64-encoded, so return as bytes
        if isinstance(self.ENCRYPTION_KEY, str):
            return self.ENCRYPTION_KEY.encode()
        return self.ENCRYPTION_KEY
    
    def __init__(self, **kwargs):
        """Initialize values after model creation"""
        super().__init__(**kwargs)
        # Generate JWT secret if not provided
        if not self.JWT_SECRET_KEY:
            self.JWT_SECRET_KEY = secrets.token_urlsafe(32)
        
        # Generate encryption key if not provided
        if not self.ENCRYPTION_KEY:
            self.ENCRYPTION_KEY = Fernet.generate_key().decode()
    
    class Config:
        env_file = ".env"
        case_sensitive = True

@lru_cache()
def get_settings() -> Settings:
    return Settings()