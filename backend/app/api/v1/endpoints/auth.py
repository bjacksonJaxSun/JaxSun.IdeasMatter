from fastapi import APIRouter, Depends, HTTPException, status, Response, Request
from fastapi.security import OAuth2PasswordBearer, OAuth2PasswordRequestForm
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from datetime import timedelta, datetime
from typing import Optional, Dict, Any
from pydantic import BaseModel, EmailStr

from app.core.database import get_db
from app.core.config import get_settings
from app.core.security import (
    create_access_token, create_refresh_token, verify_token,
    authenticate_user, create_user, verify_google_token,
    get_password_hash
)
from app.models.user import User

router = APIRouter()
settings = get_settings()

# OAuth2 scheme for token authentication
oauth2_scheme = OAuth2PasswordBearer(tokenUrl=f"{settings.API_PREFIX}/auth/login")

# Request/Response models
class UserRegister(BaseModel):
    email: EmailStr
    password: str
    name: str

class GoogleAuthRequest(BaseModel):
    credential: str

class BypassAuthRequest(BaseModel):
    role: str  # "admin" or "user"

class TokenResponse(BaseModel):
    access_token: str
    refresh_token: str
    token_type: str = "bearer"
    expires_in: int

class UserResponse(BaseModel):
    id: int
    email: str
    name: str
    role: str
    tenant_id: Optional[str]
    permissions: list[str]
    picture: Optional[str]

# Dependency to get current user
async def get_current_user(
    token: str = Depends(oauth2_scheme),
    db: AsyncSession = Depends(get_db)
) -> User:
    """Get current authenticated user"""
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    
    payload = await verify_token(token)
    if payload is None:
        raise credentials_exception
    
    user_id = payload.get("sub")
    if user_id is None:
        raise credentials_exception
    
    result = await db.execute(select(User).where(User.id == int(user_id)))
    user = result.scalar_one_or_none()
    
    if user is None:
        raise credentials_exception
    
    return user

# Dependency to check if user is admin
async def require_admin(current_user: User = Depends(get_current_user)) -> User:
    """Require admin or system_admin role"""
    if current_user.role not in ["admin", "system_admin"]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Not enough permissions"
        )
    return current_user

# Dependency to check if user is system admin
async def require_system_admin(current_user: User = Depends(get_current_user)) -> User:
    """Require system_admin role"""
    if current_user.role != "system_admin":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="System administrator access required"
        )
    return current_user

@router.post("/register", response_model=TokenResponse)
async def register(
    user_data: UserRegister,
    db: AsyncSession = Depends(get_db)
):
    """Register a new user"""
    # Check if user already exists
    result = await db.execute(select(User).where(User.email == user_data.email))
    if result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Email already registered"
        )
    
    # Create new user
    user = await create_user(
        db=db,
        email=user_data.email,
        password=user_data.password,
        name=user_data.name,
        role="user"
    )
    
    # Update last login
    user.last_login = datetime.utcnow()
    await db.commit()
    
    # Create tokens
    access_token = create_access_token(data={"sub": str(user.id)})
    refresh_token = create_refresh_token(data={"sub": str(user.id)})
    
    return {
        "access_token": access_token,
        "refresh_token": refresh_token,
        "token_type": "bearer",
        "expires_in": settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60
    }

@router.post("/login", response_model=TokenResponse)
async def login(
    form_data: OAuth2PasswordRequestForm = Depends(),
    db: AsyncSession = Depends(get_db)
):
    """Login with email and password"""
    user = await authenticate_user(db, form_data.username, form_data.password)
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # Update last login
    user.last_login = datetime.utcnow()
    await db.commit()
    
    # Create tokens
    access_token = create_access_token(data={"sub": str(user.id)})
    refresh_token = create_refresh_token(data={"sub": str(user.id)})
    
    return {
        "access_token": access_token,
        "refresh_token": refresh_token,
        "token_type": "bearer",
        "expires_in": settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60
    }

@router.post("/google", response_model=TokenResponse)
async def google_auth(
    auth_data: GoogleAuthRequest,
    db: AsyncSession = Depends(get_db)
):
    """Authenticate with Google OAuth"""
    # Verify Google token
    google_user = await verify_google_token(auth_data.credential)
    if not google_user or not google_user.get('email_verified'):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid Google credentials"
        )
    
    email = google_user['email']
    
    # Check if user exists
    result = await db.execute(select(User).where(User.email == email))
    user = result.scalar_one_or_none()
    
    if not user:
        # Create new user from Google data
        # Determine role based on email domain or specific emails
        role = "user"
        if email.endswith("@admin.ideasmatter.com") or email in settings.ADMIN_EMAILS:
            role = "system_admin"
        
        user = User(
            email=email,
            name=google_user['name'],
            picture=google_user.get('picture'),
            role=role,
            auth_provider="google",
            is_active=True,
            is_verified=True,
            permissions=get_default_permissions(role)
        )
        db.add(user)
        await db.commit()
        await db.refresh(user)
    else:
        # Update existing user info
        user.name = google_user['name']
        user.picture = google_user.get('picture')
        user.is_verified = True
    
    # Update last login
    user.last_login = datetime.utcnow()
    await db.commit()
    
    # Create tokens
    access_token = create_access_token(data={"sub": str(user.id)})
    refresh_token = create_refresh_token(data={"sub": str(user.id)})
    
    return {
        "access_token": access_token,
        "refresh_token": refresh_token,
        "token_type": "bearer",
        "expires_in": settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60
    }

@router.post("/refresh", response_model=TokenResponse)
async def refresh_token(
    refresh_token: str,
    db: AsyncSession = Depends(get_db)
):
    """Refresh access token using refresh token"""
    payload = await verify_token(refresh_token, token_type="refresh")
    if not payload:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid refresh token"
        )
    
    user_id = payload.get("sub")
    if not user_id:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid refresh token"
        )
    
    # Verify user still exists and is active
    result = await db.execute(select(User).where(User.id == int(user_id)))
    user = result.scalar_one_or_none()
    
    if not user or not user.is_active:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="User not found or inactive"
        )
    
    # Create new tokens
    new_access_token = create_access_token(data={"sub": str(user.id)})
    new_refresh_token = create_refresh_token(data={"sub": str(user.id)})
    
    return {
        "access_token": new_access_token,
        "refresh_token": new_refresh_token,
        "token_type": "bearer",
        "expires_in": settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60
    }

@router.get("/me", response_model=UserResponse)
async def get_current_user_info(
    current_user: User = Depends(get_current_user)
):
    """Get current user information"""
    return UserResponse(
        id=current_user.id,
        email=current_user.email,
        name=current_user.name,
        role=current_user.role,
        tenant_id=current_user.tenant_id,
        permissions=current_user.permissions,
        picture=current_user.picture
    )

@router.post("/logout")
async def logout(
    response: Response,
    current_user: User = Depends(get_current_user)
):
    """Logout current user"""
    # In a production app, you might want to blacklist the token
    # For now, we'll just return success
    response.delete_cookie("access_token")
    response.delete_cookie("refresh_token")
    return {"message": "Successfully logged out"}

@router.post("/bypass", response_model=TokenResponse)
async def bypass_auth(
    auth_data: BypassAuthRequest,
    db: AsyncSession = Depends(get_db)
):
    """Bypass authentication for development - DO NOT USE IN PRODUCTION"""
    # Validate role
    if auth_data.role not in ["admin", "user"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Role must be 'admin' or 'user'"
        )
    
    # Map 'admin' to 'system_admin' for full access
    actual_role = "system_admin" if auth_data.role == "admin" else "user"
    
    # Create or get bypass user
    email = f"bypass_{auth_data.role}@dev.local"
    name = f"Bypass {auth_data.role.title()}"
    
    # Check if bypass user exists
    result = await db.execute(select(User).where(User.email == email))
    user = result.scalar_one_or_none()
    
    if not user:
        # Create bypass user
        user = User(
            email=email,
            name=name,
            role=actual_role,
            auth_provider="bypass",
            is_active=True,
            is_verified=True,
            hashed_password=get_password_hash("bypass_password"),
            permissions=get_default_permissions(actual_role)
        )
        db.add(user)
        await db.commit()
        await db.refresh(user)
    else:
        # Update existing user's role to match the new mapping
        user.role = actual_role
        user.permissions = get_default_permissions(actual_role)
    
    # Update last login
    user.last_login = datetime.utcnow()
    await db.commit()
    
    # Create tokens
    access_token = create_access_token(data={"sub": str(user.id)})
    refresh_token = create_refresh_token(data={"sub": str(user.id)})
    
    return {
        "access_token": access_token,
        "refresh_token": refresh_token,
        "token_type": "bearer",
        "expires_in": settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60
    }

# Helper function for security.py
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