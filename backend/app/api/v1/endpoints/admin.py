from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List, Any, Dict, Optional
from pydantic import BaseModel

from app.core.database import get_db

router = APIRouter()

# Pydantic models for AI Provider management
class AIProviderConfig(BaseModel):
    apiKey: Optional[str] = None
    endpoint: Optional[str] = None
    model: Optional[str] = None
    temperature: Optional[float] = None
    maxTokens: Optional[int] = None
    region: Optional[str] = None
    version: Optional[str] = None

class AIProviderCreate(BaseModel):
    name: str
    type: str  # 'openai', 'azure_openai', 'claude', 'gemini', 'llama', 'custom'
    enabled: bool = True
    config: AIProviderConfig

class AIProviderUpdate(BaseModel):
    name: Optional[str] = None
    enabled: Optional[bool] = None
    config: Optional[AIProviderConfig] = None

class TenantInfo(BaseModel):
    id: str
    name: str
    domain: str
    users: int
    aiProvider: str
    plan: str
    status: str
    createdAt: str

# Mock data for demo
MOCK_AI_PROVIDERS = {
    "openai-1": {
        "id": "openai-1",
        "name": "OpenAI GPT-4",
        "type": "openai",
        "enabled": True,
        "config": {
            "apiKey": "sk-proj-***************",
            "model": "gpt-4-turbo-preview",
            "temperature": 0.7,
            "maxTokens": 4000
        },
        "status": "active",
        "lastTested": "2024-01-13T10:30:00Z"
    },
    "azure-1": {
        "id": "azure-1",
        "name": "Azure OpenAI",
        "type": "azure_openai",
        "enabled": False,
        "config": {
            "apiKey": "***************",
            "endpoint": "https://your-resource.openai.azure.com/",
            "model": "gpt-4",
            "version": "2023-12-01-preview"
        },
        "status": "inactive"
    },
    "claude-1": {
        "id": "claude-1",
        "name": "Anthropic Claude",
        "type": "claude",
        "enabled": True,
        "config": {
            "apiKey": "sk-ant-***************",
            "model": "claude-3-opus-20240229",
            "temperature": 0.7,
            "maxTokens": 2000
        },
        "status": "error",
        "lastTested": "2024-01-13T09:15:00Z"
    }
}

MOCK_TENANTS = {
    "tenant_1": {
        "id": "tenant_1",
        "name": "Acme Corp",
        "domain": "acme.com",
        "users": 45,
        "aiProvider": "openai-1",
        "plan": "enterprise",
        "status": "active",
        "createdAt": "2024-01-01"
    },
    "tenant_2": {
        "id": "tenant_2",
        "name": "StartupXYZ",
        "domain": "startupxyz.com",
        "users": 12,
        "aiProvider": "claude-1",
        "plan": "pro",
        "status": "active",
        "createdAt": "2024-01-10"
    }
}

@router.get("/ai-providers")
async def get_ai_providers(db: AsyncSession = Depends(get_db)) -> List[Dict[str, Any]]:
    """Get all AI providers"""
    return list(MOCK_AI_PROVIDERS.values())

@router.post("/ai-providers")
async def create_ai_provider(
    provider: AIProviderCreate,
    db: AsyncSession = Depends(get_db)
) -> Dict[str, Any]:
    """Create a new AI provider"""
    provider_id = f"{provider.type}-{len(MOCK_AI_PROVIDERS) + 1}"
    new_provider = {
        "id": provider_id,
        "name": provider.name,
        "type": provider.type,
        "enabled": provider.enabled,
        "config": provider.config.dict(),
        "status": "inactive",
        "lastTested": None
    }
    MOCK_AI_PROVIDERS[provider_id] = new_provider
    return new_provider

@router.put("/ai-providers/{provider_id}")
async def update_ai_provider(
    provider_id: str,
    provider_update: AIProviderUpdate,
    db: AsyncSession = Depends(get_db)
) -> Dict[str, Any]:
    """Update an AI provider"""
    if provider_id not in MOCK_AI_PROVIDERS:
        raise HTTPException(status_code=404, detail="Provider not found")
    
    provider = MOCK_AI_PROVIDERS[provider_id]
    update_data = provider_update.dict(exclude_unset=True)
    
    for field, value in update_data.items():
        if field == "config" and value:
            provider["config"].update(value)
        else:
            provider[field] = value
    
    return provider

@router.delete("/ai-providers/{provider_id}")
async def delete_ai_provider(
    provider_id: str,
    db: AsyncSession = Depends(get_db)
) -> Dict[str, str]:
    """Delete an AI provider"""
    if provider_id not in MOCK_AI_PROVIDERS:
        raise HTTPException(status_code=404, detail="Provider not found")
    
    del MOCK_AI_PROVIDERS[provider_id]
    return {"message": "Provider deleted successfully"}

@router.post("/ai-providers/{provider_id}/test")
async def test_ai_provider(
    provider_id: str,
    db: AsyncSession = Depends(get_db)
) -> Dict[str, Any]:
    """Test an AI provider connection"""
    if provider_id not in MOCK_AI_PROVIDERS:
        raise HTTPException(status_code=404, detail="Provider not found")
    
    provider = MOCK_AI_PROVIDERS[provider_id]
    
    # Mock testing logic - in real implementation, test the actual connection
    import datetime
    provider["status"] = "active"
    provider["lastTested"] = datetime.datetime.now().isoformat()
    
    return {"success": True, "message": "Provider test successful", "provider": provider}

@router.get("/tenants")
async def get_tenants(db: AsyncSession = Depends(get_db)) -> List[TenantInfo]:
    """Get all tenants"""
    return list(MOCK_TENANTS.values())

@router.get("/tenants/{tenant_id}")
async def get_tenant(
    tenant_id: str,
    db: AsyncSession = Depends(get_db)
) -> TenantInfo:
    """Get a specific tenant"""
    if tenant_id not in MOCK_TENANTS:
        raise HTTPException(status_code=404, detail="Tenant not found")
    return MOCK_TENANTS[tenant_id]

@router.put("/tenants/{tenant_id}/ai-provider")
async def update_tenant_ai_provider(
    tenant_id: str,
    provider_id: str,
    db: AsyncSession = Depends(get_db)
) -> Dict[str, str]:
    """Update a tenant's AI provider"""
    if tenant_id not in MOCK_TENANTS:
        raise HTTPException(status_code=404, detail="Tenant not found")
    
    if provider_id not in MOCK_AI_PROVIDERS:
        raise HTTPException(status_code=404, detail="AI Provider not found")
    
    MOCK_TENANTS[tenant_id]["aiProvider"] = provider_id
    return {"message": "Tenant AI provider updated successfully"}

@router.get("/stats")
async def get_admin_stats(db: AsyncSession = Depends(get_db)) -> Dict[str, Any]:
    """Get admin dashboard statistics"""
    total_tenants = len(MOCK_TENANTS)
    total_users = sum(tenant["users"] for tenant in MOCK_TENANTS.values())
    total_providers = len(MOCK_AI_PROVIDERS)
    active_providers = len([p for p in MOCK_AI_PROVIDERS.values() if p["status"] == "active"])
    
    return {
        "totalTenants": total_tenants,
        "totalUsers": total_users,
        "totalProviders": total_providers,
        "activeProviders": active_providers,
        "recentActivity": [
            {
                "action": "OpenAI GPT-4 provider tested successfully",
                "timestamp": "2024-01-13T10:30:00Z"
            },
            {
                "action": "New tenant 'TechCo' added",
                "timestamp": "2024-01-12T15:20:00Z"
            }
        ]
    }