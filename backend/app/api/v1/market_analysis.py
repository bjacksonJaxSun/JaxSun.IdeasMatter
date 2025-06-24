from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy.orm import Session
from sqlalchemy import select

from app.core.database import get_db
from app.models.research import ResearchSession
from app.models.market_analysis import MarketAnalysis, CompetitorAnalysis
from app.schemas.market_analysis import (
    MarketAnalysisRequest, CompetitorResearchRequest, MarketSizingRequest,
    ComprehensiveMarketAnalysis, MarketSizingAnalysis, CompetitiveLandscape,
    MarketAnalysis as MarketAnalysisSchema, CompetitorAnalysis as CompetitorAnalysisSchema
)
from app.services.market_analysis_service import MarketAnalysisService
from app.services.ai_orchestration_simple import SimpleAIOrchestrator
from app.core.auth import get_current_user_id
import logging

logger = logging.getLogger(__name__)
router = APIRouter()

def get_market_analysis_service(db: Session = Depends(get_db)) -> MarketAnalysisService:
    """Get market analysis service with dependencies"""
    ai_service = SimpleAIOrchestrator()
    return MarketAnalysisService(ai_service)

@router.post("/generate", response_model=ComprehensiveMarketAnalysis)
async def generate_comprehensive_market_analysis(
    request: MarketAnalysisRequest,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    service: MarketAnalysisService = Depends(get_market_analysis_service)
):
    """Generate comprehensive market analysis for a research session"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == request.session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Get idea details from session
        idea_title = session.title or "Business Idea"
        idea_description = session.description or "Market analysis for business concept"
        
        # Generate comprehensive analysis
        analysis = service.generate_comprehensive_market_analysis(
            db, request.session_id, idea_title, idea_description
        )
        
        return analysis
        
    except Exception as e:
        logger.error(f"Error generating market analysis: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to generate market analysis"
        )

@router.get("/{session_id}", response_model=ComprehensiveMarketAnalysis)
async def get_market_analysis(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db)
):
    """Get existing market analysis for a session"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Get market analysis
        market_analysis = db.query(MarketAnalysis).filter(
            MarketAnalysis.session_id == session_id
        ).first()
        
        if not market_analysis:
            raise HTTPException(
                status_code=404, 
                detail="No market analysis found for this session"
            )
        
        # Build comprehensive response
        analysis = ComprehensiveMarketAnalysis(
            market_analysis=MarketAnalysisSchema.from_orm(market_analysis),
            competitors=[
                CompetitorAnalysisSchema.from_orm(comp) 
                for comp in market_analysis.competitors
            ],
            segments=market_analysis.segments,
            trends=[],  # Get from MarketTrendAnalysis
            opportunities=[]  # Get from MarketOpportunity
        )
        
        return analysis
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error retrieving market analysis: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to retrieve market analysis"
        )

@router.post("/sizing", response_model=MarketSizingAnalysis)
async def calculate_market_sizing(
    request: MarketSizingRequest,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    service: MarketAnalysisService = Depends(get_market_analysis_service)
):
    """Calculate TAM, SAM, SOM for a market analysis"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == request.session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Calculate market sizing
        sizing = service.calculate_market_sizing(
            db, 
            request.session_id, 
            request.geographic_scope,
            request.target_segments
        )
        
        return sizing
        
    except Exception as e:
        logger.error(f"Error calculating market sizing: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to calculate market sizing"
        )

@router.get("/{session_id}/competitive-landscape", response_model=CompetitiveLandscape)
async def get_competitive_landscape(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    service: MarketAnalysisService = Depends(get_market_analysis_service)
):
    """Get competitive landscape analysis"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Analyze competitive landscape
        landscape = service.analyze_competitive_landscape(db, session_id)
        
        return landscape
        
    except Exception as e:
        logger.error(f"Error analyzing competitive landscape: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to analyze competitive landscape"
        )

@router.post("/competitors/{competitor_id}/research", response_model=CompetitorAnalysisSchema)
async def research_competitor(
    competitor_id: int,
    request: CompetitorResearchRequest,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    service: MarketAnalysisService = Depends(get_market_analysis_service)
):
    """Research a specific competitor in detail"""
    
    try:
        # Verify competitor exists and belongs to user's session
        competitor = db.query(CompetitorAnalysis).join(MarketAnalysis).join(ResearchSession).filter(
            CompetitorAnalysis.id == competitor_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not competitor:
            raise HTTPException(status_code=404, detail="Competitor not found")
        
        # Research competitor
        updated_competitor = service.research_competitor(
            db,
            competitor.market_analysis_id,
            request.competitor_name,
            request.research_depth
        )
        
        return CompetitorAnalysisSchema.from_orm(updated_competitor)
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error researching competitor: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to research competitor"
        )

@router.post("/competitors/add", response_model=CompetitorAnalysisSchema)
async def add_new_competitor(
    session_id: int,
    request: CompetitorResearchRequest,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    service: MarketAnalysisService = Depends(get_market_analysis_service)
):
    """Add and research a new competitor"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Get or create market analysis
        market_analysis = db.query(MarketAnalysis).filter(
            MarketAnalysis.session_id == session_id
        ).first()
        
        if not market_analysis:
            raise HTTPException(
                status_code=404, 
                detail="No market analysis found. Generate one first."
            )
        
        # Add and research competitor
        competitor = service.research_competitor(
            db,
            market_analysis.id,
            request.competitor_name,
            request.research_depth
        )
        
        return CompetitorAnalysisSchema.from_orm(competitor)
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error adding competitor: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to add competitor"
        )

@router.get("/{session_id}/export/pdf")
async def export_market_analysis_pdf(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db),
    include_detailed_competitors: bool = True,
    include_market_sizing: bool = True,
    include_trends: bool = True
):
    """Export market analysis as PDF report"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Get market analysis
        market_analysis = db.query(MarketAnalysis).filter(
            MarketAnalysis.session_id == session_id
        ).first()
        
        if not market_analysis:
            raise HTTPException(
                status_code=404, 
                detail="No market analysis found for this session"
            )
        
        # TODO: Implement PDF generation
        # For now, return a placeholder response
        raise HTTPException(
            status_code=501, 
            detail="PDF export not yet implemented"
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error exporting market analysis PDF: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to export market analysis"
        )

@router.delete("/{session_id}")
async def delete_market_analysis(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db)
):
    """Delete market analysis for a session"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Delete market analysis (cascades to related data)
        market_analysis = db.query(MarketAnalysis).filter(
            MarketAnalysis.session_id == session_id
        ).first()
        
        if market_analysis:
            db.delete(market_analysis)
            db.commit()
        
        return {"message": "Market analysis deleted successfully"}
        
    except Exception as e:
        logger.error(f"Error deleting market analysis: {str(e)}")
        db.rollback()
        raise HTTPException(
            status_code=500, 
            detail="Failed to delete market analysis"
        )

@router.get("/{session_id}/status")
async def get_market_analysis_status(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db)
):
    """Get the status of market analysis for a session"""
    
    try:
        # Verify session exists and belongs to user
        session = db.query(ResearchSession).filter(
            ResearchSession.id == session_id,
            ResearchSession.user_id == current_user_id
        ).first()
        
        if not session:
            raise HTTPException(status_code=404, detail="Research session not found")
        
        # Check if market analysis exists
        market_analysis = db.query(MarketAnalysis).filter(
            MarketAnalysis.session_id == session_id
        ).first()
        
        if not market_analysis:
            return {
                "has_analysis": False,
                "analysis_date": None,
                "confidence_score": None,
                "competitors_count": 0,
                "segments_count": 0
            }
        
        competitors_count = len(market_analysis.competitors)
        segments_count = len(market_analysis.segments)
        
        return {
            "has_analysis": True,
            "analysis_date": market_analysis.analysis_date,
            "last_updated": market_analysis.last_updated,
            "confidence_score": market_analysis.confidence_score,
            "competitors_count": competitors_count,
            "segments_count": segments_count,
            "industry": market_analysis.industry,
            "market_category": market_analysis.market_category,
            "geographic_scope": market_analysis.geographic_scope,
            "tam_value": market_analysis.tam_value,
            "sam_value": market_analysis.sam_value,
            "som_value": market_analysis.som_value
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting market analysis status: {str(e)}")
        raise HTTPException(
            status_code=500, 
            detail="Failed to get market analysis status"
        )