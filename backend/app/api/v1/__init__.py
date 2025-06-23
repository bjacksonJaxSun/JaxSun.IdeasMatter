from fastapi import APIRouter

from app.api.v1.endpoints import auth, admin
from app.api.v1 import research

api_router = APIRouter()

# Include all endpoint routers
api_router.include_router(auth.router, prefix="/auth", tags=["authentication"])
api_router.include_router(admin.router, prefix="/admin", tags=["administration"])
api_router.include_router(research.router, prefix="/research", tags=["research"])