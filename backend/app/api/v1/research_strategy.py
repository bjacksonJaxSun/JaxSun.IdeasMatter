"""
Research Strategy API Endpoints
Provides approach-based market research with progressive disclosure.
"""
from typing import List, Optional, Dict, Any
from fastapi import APIRouter, HTTPException, Depends, BackgroundTasks
from sqlalchemy.orm import Session

from app.schemas.research_strategy import (
    ResearchApproach, AnalysisPhase,
    ResearchStrategy, ResearchStrategyRequest, ResearchStrategyResponse,
    ResearchAnalysisComplete, AnalysisProgressUpdate,
    StrategicOptionComparison, ResearchExportRequest, ResearchExportResponse
)
from app.services.research_strategy_service import ResearchStrategyService
from app.core.database import get_db
from app.core.auth import get_current_user_id

router = APIRouter(prefix="/research-strategy", tags=["research-strategy"])

# Global service instance
research_service = ResearchStrategyService()

# In-memory storage for progress tracking (in production, use Redis or database)
progress_storage: Dict[int, Dict[str, Any]] = {}

@router.post("/approaches", response_model=List[Dict[str, Any]])
async def get_available_approaches():
    """Get available research approaches with descriptions and capabilities."""
    
    approaches = [
        {
            "approach": ResearchApproach.QUICK_VALIDATION,
            "title": "Quick Validation",
            "description": "Rapid validation of core business assumptions",
            "duration_minutes": 15,
            "complexity": "beginner",
            "best_for": [
                "Early-stage ideas needing validation",
                "Quick go/no-go decisions",
                "Limited time or resources"
            ],
            "includes": [
                "Market opportunity assessment",
                "Basic competitive analysis", 
                "Strategic recommendation",
                "Go/no-go decision framework"
            ],
            "deliverables": [
                "Market context overview",
                "Competitive landscape summary",
                "2 strategic options",
                "Recommendation with reasoning"
            ]
        },
        {
            "approach": ResearchApproach.MARKET_DEEP_DIVE,
            "title": "Market Deep-Dive",
            "description": "Comprehensive market analysis with strategic recommendations",
            "duration_minutes": 45,
            "complexity": "intermediate",
            "best_for": [
                "Well-defined business ideas",
                "Strategic planning and positioning",
                "Investor presentations"
            ],
            "includes": [
                "Detailed market analysis",
                "Comprehensive competitive intelligence",
                "Customer segment analysis",
                "SWOT analysis",
                "Strategic options evaluation"
            ],
            "deliverables": [
                "Market sizing and growth analysis",
                "Competitive positioning map",
                "Customer segment priorities",
                "3 strategic options with SWOT",
                "Implementation recommendations"
            ]
        },
        {
            "approach": ResearchApproach.LAUNCH_STRATEGY,
            "title": "Launch Strategy",
            "description": "Complete launch strategy with implementation roadmap",
            "duration_minutes": 90,
            "complexity": "advanced",
            "best_for": [
                "Pre-launch businesses",
                "Detailed business planning",
                "Funding and investment decisions"
            ],
            "includes": [
                "Everything in Market Deep-Dive plus:",
                "Go-to-market strategy",
                "Revenue model analysis",
                "Risk assessment & mitigation",
                "Resource planning",
                "Success metrics definition"
            ],
            "deliverables": [
                "Complete market research report",
                "5 strategic options with detailed analysis",
                "Go-to-market roadmap",
                "Financial projections",
                "Risk mitigation strategies",
                "Implementation timeline"
            ]
        }
    ]
    
    return approaches

@router.post("/initiate", response_model=ResearchStrategyResponse)
async def initiate_research_strategy(
    request: ResearchStrategyRequest,
    current_user_id: str = Depends(get_current_user_id),
    db: Session = Depends(get_db)
):
    """Initiate a new research strategy for an idea."""
    
    try:
        # In a real implementation, you'd fetch the idea details from the database
        # For now, we'll use mock data
        idea_title = f"Idea for Session {request.session_id}"
        idea_description = "Mock idea description for demonstration"
        
        strategy = await research_service.initiate_research_strategy(
            session_id=request.session_id,
            idea_title=idea_title,
            idea_description=idea_description,
            approach=request.approach,
            custom_parameters=request.custom_parameters
        )
        
        # Store strategy (in production, save to database)
        progress_storage[strategy.id] = {
            "strategy": strategy,
            "idea_title": idea_title,
            "idea_description": idea_description,
            "current_phase": None,
            "progress": 0.0
        }
        
        config = research_service.strategy_configs[request.approach]
        
        response = ResearchStrategyResponse(
            strategy=strategy,
            estimated_completion_time=f"{config['duration_minutes']} minutes",
            included_analyses=config["phases"],
            next_steps=[
                "Review the analysis approach",
                "Start the automated research process",
                "Monitor progress through the dashboard"
            ]
        )
        
        return response
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to initiate research strategy: {str(e)}")

@router.post("/execute/{strategy_id}")
async def execute_research_strategy(
    strategy_id: int,
    background_tasks: BackgroundTasks,
    current_user_id: str = Depends(get_current_user_id)
):
    """Execute the research strategy in the background."""
    
    if strategy_id not in progress_storage:
        raise HTTPException(status_code=404, detail="Strategy not found")
    
    stored_data = progress_storage[strategy_id]
    strategy = stored_data["strategy"]
    
    if strategy.status != "pending":
        raise HTTPException(status_code=400, detail="Strategy is not in pending state")
    
    # Define progress callback
    async def progress_callback(sid: int, phase: AnalysisPhase, progress: float):
        if sid in progress_storage:
            progress_storage[sid]["current_phase"] = phase
            progress_storage[sid]["progress"] = progress
    
    # Execute strategy in background
    background_tasks.add_task(
        execute_strategy_background,
        strategy,
        stored_data["idea_title"],
        stored_data["idea_description"],
        progress_callback
    )
    
    return {"message": "Research strategy execution started", "strategy_id": strategy_id}

async def execute_strategy_background(
    strategy: ResearchStrategy,
    idea_title: str,
    idea_description: str,
    progress_callback: callable
):
    """Background task to execute research strategy."""
    try:
        result = await research_service.execute_research_strategy(
            strategy=strategy,
            idea_title=idea_title,
            idea_description=idea_description,
            progress_callback=progress_callback
        )
        
        # Store results
        if strategy.id in progress_storage:
            progress_storage[strategy.id]["result"] = result
            progress_storage[strategy.id]["strategy"] = strategy
            
    except Exception as e:
        if strategy.id in progress_storage:
            progress_storage[strategy.id]["error"] = str(e)
            strategy.status = "error"

@router.get("/progress/{strategy_id}", response_model=AnalysisProgressUpdate)
async def get_research_progress(
    strategy_id: int,
    current_user_id: str = Depends(get_current_user_id)
):
    """Get the current progress of research strategy execution."""
    
    if strategy_id not in progress_storage:
        raise HTTPException(status_code=404, detail="Strategy not found")
    
    stored_data = progress_storage[strategy_id]
    strategy = stored_data["strategy"]
    
    # Check for errors
    if "error" in stored_data:
        raise HTTPException(status_code=500, detail=stored_data["error"])
    
    # Calculate estimated completion time
    if strategy.status == "in_progress" and stored_data["progress"] > 0:
        elapsed_time = (datetime.utcnow() - strategy.started_at).total_seconds() / 60
        completion_rate = stored_data["progress"] / 100
        estimated_total_time = elapsed_time / completion_rate if completion_rate > 0 else strategy.estimated_duration_minutes
        estimated_remaining = max(0, estimated_total_time - elapsed_time)
    else:
        estimated_remaining = strategy.estimated_duration_minutes if strategy.status == "pending" else 0
    
    return AnalysisProgressUpdate(
        strategy_id=strategy_id,
        current_phase=stored_data.get("current_phase", AnalysisPhase.MARKET_CONTEXT),
        progress_percentage=stored_data.get("progress", 0.0),
        estimated_completion_minutes=int(estimated_remaining)
    )

@router.get("/results/{strategy_id}", response_model=ResearchAnalysisComplete)
async def get_research_results(
    strategy_id: int,
    current_user_id: str = Depends(get_current_user_id)
):
    """Get the complete research analysis results."""
    
    if strategy_id not in progress_storage:
        raise HTTPException(status_code=404, detail="Strategy not found")
    
    stored_data = progress_storage[strategy_id]
    strategy = stored_data["strategy"]
    
    if strategy.status != "completed":
        raise HTTPException(status_code=400, detail="Research strategy is not completed yet")
    
    if "result" not in stored_data:
        raise HTTPException(status_code=404, detail="Research results not found")
    
    return stored_data["result"]

@router.get("/strategies/{session_id}", response_model=List[ResearchStrategy])
async def get_session_strategies(
    session_id: int,
    current_user_id: str = Depends(get_current_user_id)
):
    """Get all research strategies for a session."""
    
    strategies = []
    for stored_data in progress_storage.values():
        strategy = stored_data["strategy"]
        if strategy.session_id == session_id:
            strategies.append(strategy)
    
    return strategies

@router.post("/compare-options", response_model=StrategicOptionComparison)
async def compare_strategic_options(
    strategy_id: int,
    comparison_criteria: Optional[List[str]] = None,
    current_user_id: str = Depends(get_current_user_id)
):
    """Compare strategic options from a completed analysis."""
    
    if strategy_id not in progress_storage:
        raise HTTPException(status_code=404, detail="Strategy not found")
    
    stored_data = progress_storage[strategy_id]
    
    if "result" not in stored_data:
        raise HTTPException(status_code=400, detail="Analysis not completed")
    
    result: ResearchAnalysisComplete = stored_data["result"]
    
    if not result.strategic_options:
        raise HTTPException(status_code=404, detail="No strategic options found")
    
    # Default comparison criteria
    if not comparison_criteria:
        comparison_criteria = [
            "Investment Required",
            "Time to Market", 
            "Success Probability",
            "Risk Level",
            "Market Potential"
        ]
    
    # Generate trade-off analysis
    trade_offs = {}
    for i, option in enumerate(result.strategic_options):
        trade_offs[f"Option {i+1}"] = f"{option.title}: {option.success_probability_percent}% success probability, {option.timeline_to_market_months} months to market"
    
    comparison = StrategicOptionComparison(
        session_id=stored_data["strategy"].session_id,
        options=result.strategic_options,
        comparison_criteria=comparison_criteria,
        recommendation_reasoning=f"Based on analysis, {result.recommended_option.title if result.recommended_option else 'the top option'} offers the best balance of opportunity and feasibility.",
        trade_off_analysis=trade_offs
    )
    
    return comparison

@router.post("/export", response_model=ResearchExportResponse)
async def export_research_results(
    request: ResearchExportRequest,
    current_user_id: str = Depends(get_current_user_id)
):
    """Export research results in various formats."""
    
    # Find strategy for session
    strategy_result = None
    for stored_data in progress_storage.values():
        strategy = stored_data["strategy"]
        if strategy.session_id == request.session_id:
            if request.strategy_id is None or strategy.id == request.strategy_id:
                if "result" in stored_data:
                    strategy_result = stored_data["result"]
                    break
    
    if not strategy_result:
        raise HTTPException(status_code=404, detail="No completed research found for export")
    
    # Generate export file (mock implementation)
    file_name = f"research_analysis_{request.session_id}_{datetime.utcnow().strftime('%Y%m%d_%H%M%S')}.{request.export_format}"
    
    # In real implementation, generate actual file and upload to storage
    download_url = f"/api/v1/files/download/{file_name}"
    file_size = 1024 * 1024  # Mock 1MB file
    expires_at = datetime.utcnow() + timedelta(hours=24)
    
    return ResearchExportResponse(
        download_url=download_url,
        file_name=file_name,
        file_size_bytes=file_size,
        expires_at=expires_at
    )

@router.delete("/strategies/{strategy_id}")
async def delete_research_strategy(
    strategy_id: int,
    current_user_id: str = Depends(get_current_user_id)
):
    """Delete a research strategy and its results."""
    
    if strategy_id not in progress_storage:
        raise HTTPException(status_code=404, detail="Strategy not found")
    
    del progress_storage[strategy_id]
    
    return {"message": "Research strategy deleted successfully"}

# Helper endpoint for development/testing
@router.get("/demo/{approach}")
async def get_demo_analysis(approach: ResearchApproach):
    """Get a demo analysis for testing the UI components."""
    
    # Create a mock completed analysis
    from app.schemas.research_strategy import (
        MarketContext, CompetitiveIntelligence, CustomerUnderstanding, 
        StrategicAssessment, StrategicOption, StrategicApproach,
        SwotAnalysisEnhanced, OpportunityScoring, CustomerSegment
    )
    
    # Mock data for demonstration
    demo_analysis = ResearchAnalysisComplete(
        strategy_id=99999,
        approach=approach,
        market_context=MarketContext(
            industry_overview="Demo industry analysis for UI testing",
            market_size_usd=5000000000,
            growth_rate_cagr=12.5,
            maturity_stage="growth",
            key_trends=["Digital transformation", "AI adoption", "Sustainability"],
            regulatory_environment="Moderate regulation with upcoming changes",
            technological_factors=["Cloud computing", "Machine learning", "IoT"],
            confidence_score=0.85
        ),
        competitive_intelligence=CompetitiveIntelligence(
            competitive_landscape_summary="Competitive landscape shows moderate competition with room for innovation",
            confidence_score=0.80
        ),
        customer_understanding=CustomerUnderstanding(
            primary_target_segment="Tech-savvy professionals",
            customer_segments=[
                CustomerSegment(
                    name="Early Adopters",
                    description="Technology enthusiasts willing to try new solutions",
                    size_estimate=100000,
                    priority_score=0.9
                )
            ],
            confidence_score=0.75
        ),
        strategic_assessment=StrategicAssessment(
            swot_analysis=SwotAnalysisEnhanced(
                strengths=["Innovation", "Technical expertise", "Market timing"],
                weaknesses=["Limited brand recognition", "Resource constraints"],
                opportunities=["Growing market", "Technology trends", "Unmet needs"],
                threats=["Competition", "Market changes", "Regulatory risks"],
                confidence_score=0.8
            ),
            opportunity_scoring=OpportunityScoring(
                market_opportunity_score=8.0,
                competitive_position_score=6.5,
                execution_feasibility_score=7.5,
                financial_potential_score=7.8,
                overall_score=7.4,
                risk_level="medium"
            ),
            strategic_fit_analysis="Strong strategic fit with market opportunities",
            go_no_go_recommendation="go",
            reasoning="Market opportunity outweighs risks with proper execution",
            confidence_score=0.82
        ),
        strategic_options=[
            StrategicOption(
                approach=StrategicApproach.NICHE_DOMINATION,
                title="Niche Market Leader",
                description="Focus on specialized market segment with tailored solution",
                target_customer_segment="Early Adopters",
                value_proposition="Specialized solution for specific needs",
                go_to_market_strategy="Direct sales and content marketing",
                timeline_to_market_months=12,
                success_probability_percent=75,
                overall_score=8.2,
                recommended=True
            ),
            StrategicOption(
                approach=StrategicApproach.MARKET_LEADER_CHALLENGE,
                title="Market Challenger",
                description="Compete directly with market leaders",
                target_customer_segment="Mainstream market",
                value_proposition="Better features at competitive price",
                go_to_market_strategy="Digital marketing and partnerships",
                timeline_to_market_months=18,
                success_probability_percent=60,
                overall_score=7.1,
                recommended=False
            )
        ],
        recommended_option=StrategicOption(
            approach=StrategicApproach.NICHE_DOMINATION,
            title="Niche Market Leader",
            description="Focus on specialized market segment",
            target_customer_segment="Early Adopters",
            value_proposition="Specialized solution",
            go_to_market_strategy="Direct sales",
            timeline_to_market_months=12,
            success_probability_percent=75,
            overall_score=8.2,
            recommended=True
        ),
        analysis_confidence=0.81,
        analysis_completeness=100.0,
        next_steps=[
            "Validate assumptions with target customers",
            "Develop MVP prototype",
            "Create detailed business plan",
            "Secure initial funding"
        ],
        generated_at=datetime.utcnow()
    )
    
    return demo_analysis