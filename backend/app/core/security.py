from datetime import datetime, timedelta
from typing import Optional, Any
from jose import JWTError, jwt
from passlib.context import CryptContext
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
import secrets
import os

from app.core.config import get_settings
from app.models.user import User

settings = get_settings()

# Password hashing
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# JWT settings
SECRET_KEY = os.getenv("JWT_SECRET_KEY", secrets.token_urlsafe(32))
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 30
REFRESH_TOKEN_EXPIRE_DAYS = 7

def verify_password(plain_password: str, hashed_password: str) -> bool:
    """Verify a password against its hash"""
    return pwd_context.verify(plain_password, hashed_password)

def get_password_hash(password: str) -> str:
    """Hash a password"""
    return pwd_context.hash(password)

def create_access_token(data: dict, expires_delta: Optional[timedelta] = None) -> str:
    """Create a JWT access token"""
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.utcnow() + expires_delta
    else:
        expire = datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    
    to_encode.update({"exp": expire, "type": "access"})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt

def create_refresh_token(data: dict) -> str:
    """Create a JWT refresh token"""
    to_encode = data.copy()
    expire = datetime.utcnow() + timedelta(days=REFRESH_TOKEN_EXPIRE_DAYS)
    to_encode.update({"exp": expire, "type": "refresh"})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt

async def verify_token(token: str, token_type: str = "access") -> Optional[dict]:
    """Verify and decode a JWT token"""
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        if payload.get("type") != token_type:
            return None
        return payload
    except JWTError:
        return None

async def authenticate_user(db: AsyncSession, email: str, password: str) -> Optional[User]:
    """Authenticate a user by email and password"""
    result = await db.execute(select(User).where(User.email == email))
    user = result.scalar_one_or_none()
    
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    
    return user

async def create_user(
    db: AsyncSession,
    email: str,
    password: str,
    name: str,
    role: str = "user",
    tenant_id: Optional[str] = None
) -> User:
    """Create a new user with hashed password"""
    hashed_password = get_password_hash(password)
    
    user = User(
        email=email,
        hashed_password=hashed_password,
        name=name,
        role=role,
        tenant_id=tenant_id,
        is_active=True,
        permissions=get_default_permissions(role)
    )
    
    db.add(user)
    await db.commit()
    await db.refresh(user)
    
    return user

def get_default_permissions(role: str) -> list[str]:
    """Get default permissions based on role"""
    if role == "system_admin":
        return ["*"]  # All permissions
    elif role == "admin":
        return [
            "ideas:*",
            "research:*",
            "users:read",
            "users:update",
            "settings:read",
            "settings:update"
        ]
    else:  # regular user
        return [
            "ideas:create",
            "ideas:read:own",
            "ideas:update:own",
            "ideas:delete:own",
            "research:create",
            "research:read:own",
            "research:update:own"
        ]

async def verify_google_token(credential: str) -> Optional[dict]:
    """Verify Google OAuth token and extract user info"""
    try:
        # Use google-auth library to properly verify the token
        from google.oauth2 import id_token
        from google.auth.transport import requests
        
        # Create a request object
        request = requests.Request()
        
        # Verify the token with Google
        if not settings.GOOGLE_CLIENT_ID:
            print("Warning: GOOGLE_CLIENT_ID not configured")
            return None
            
        idinfo = id_token.verify_oauth2_token(
            credential, request, settings.GOOGLE_CLIENT_ID
        )
        
        # Verify the issuer
        if idinfo.get('iss') not in ['accounts.google.com', 'https://accounts.google.com']:
            print("Wrong issuer in Google token")
            return None
        
        # Extract user information
        return {
            'sub': idinfo.get('sub'),  # Google user ID
            'email': idinfo.get('email'),
            'name': idinfo.get('name'),
            'picture': idinfo.get('picture'),
            'email_verified': idinfo.get('email_verified', False),
            'given_name': idinfo.get('given_name'),
            'family_name': idinfo.get('family_name')
        }
        
    except Exception as e:
        print(f"Error verifying Google token: {e}")
        # Fallback to basic decoding for development (NOT secure for production)
        if settings.DEBUG:
            print("Falling back to basic token decoding (development only)")
            try:
                import base64
                import json
                
                # Split the JWT token
                parts = credential.split('.')
                if len(parts) != 3:
                    return None
                    
                # Decode the payload (middle part)
                payload = parts[1]
                # Add padding if needed
                payload += '=' * (4 - len(payload) % 4)
                
                decoded = base64.urlsafe_b64decode(payload)
                user_info = json.loads(decoded)
                
                return {
                    'sub': user_info.get('sub'),
                    'email': user_info.get('email'),
                    'name': user_info.get('name'),
                    'picture': user_info.get('picture'),
                    'email_verified': user_info.get('email_verified', False)
                }
            except Exception as fallback_error:
                print(f"Fallback token decoding also failed: {fallback_error}")
        
        return None