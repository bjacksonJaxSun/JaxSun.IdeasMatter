"""
Authentication dependencies and utilities
"""
from typing import Optional
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from jose import JWTError, jwt
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.config import get_settings
from app.core.database import get_db
from app.core.security import SECRET_KEY, ALGORITHM
from app.models.user import User

settings = get_settings()

# OAuth2 scheme for token authentication
oauth2_scheme = OAuth2PasswordBearer(tokenUrl=f"{settings.API_PREFIX}/auth/login")

# Optional OAuth2 scheme (allows unauthenticated access)
oauth2_scheme_optional = OAuth2PasswordBearer(tokenUrl=f"{settings.API_PREFIX}/auth/login", auto_error=False)


async def get_current_user(
    db: AsyncSession = Depends(get_db),
    token: str = Depends(oauth2_scheme)
) -> User:
    """Get the current authenticated user"""
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    
    try:
        payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
        user_id: str = payload.get("sub")
        if user_id is None:
            raise credentials_exception
    except JWTError:
        raise credentials_exception
    
    # Get user from database
    result = await db.execute(select(User).where(User.id == user_id))
    user = result.scalar_one_or_none()
    
    if user is None:
        raise credentials_exception
    
    if not user.is_active:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Inactive user"
        )
    
    return user


async def get_current_user_optional(
    db: AsyncSession = Depends(get_db),
    token: Optional[str] = Depends(oauth2_scheme_optional)
) -> Optional[User]:
    """Get the current user if authenticated, otherwise return None"""
    if not token:
        return None
    
    try:
        return await get_current_user(db, token)
    except HTTPException:
        return None


async def get_current_user_id(
    current_user: User = Depends(get_current_user)
) -> str:
    """Get the current user's ID"""
    return str(current_user.id)


async def get_current_user_id_optional(
    current_user: Optional[User] = Depends(get_current_user_optional)
) -> Optional[str]:
    """Get the current user's ID if authenticated"""
    return str(current_user.id) if current_user else None


async def require_admin(
    current_user: User = Depends(get_current_user)
) -> User:
    """Require the current user to be an admin"""
    if current_user.role not in ["admin", "system_admin"]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Admin access required"
        )
    return current_user


async def require_system_admin(
    current_user: User = Depends(get_current_user)
) -> User:
    """Require the current user to be a system admin"""
    if current_user.role != "system_admin":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="System admin access required"
        )
    return current_user