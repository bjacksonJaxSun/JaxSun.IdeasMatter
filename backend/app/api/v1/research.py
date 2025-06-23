from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import selectinload, Session
from sqlalchemy import select
from typing import List, Dict, Any
import json
import io
from fastapi.responses import StreamingResponse

from app.core.database import get_db, get_sync_db
from app.models.research import (
    ResearchSession, ResearchConversation, ResearchInsight, 
    ResearchOption, ResearchReport, ResearchFactCheck
)
from app.schemas.research import (
    ResearchSessionCreate, ResearchSessionUpdate, ResearchSession as ResearchSessionSchema,
    ResearchSessionDetail, BrainstormRequest, BrainstormResponse,
    AnalysisRequest, ReportRequest, FactCheckCreate,
    ResearchInsightCreate, ResearchOptionCreate, ResearchAnalysis,
    ResearchConversation as ResearchConversationSchema, 
    ResearchInsight as ResearchInsightSchema, 
    ResearchOption as ResearchOptionSchema,
    SwotAnalysisRequest, SwotAnalysisResponse, SwotPdfRequest
)
from app.services.ai_orchestration_simple import ai_orchestrator
from app.services.research_service import ResearchService

router = APIRouter()

# Session Management
@router.post("/sessions", response_model=ResearchSessionSchema)
async def create_research_session(
    session_data: ResearchSessionCreate,
    db: AsyncSession = Depends(get_db)
):
    """Create a new research session with automatic competitive analysis"""
    # Create the research session
    db_session = ResearchSession(
        user_id=1,  # TODO: Get from authenticated user
        **session_data.dict()
    )
    db.add(db_session)
    await db.commit()
    await db.refresh(db_session)
    
    # Initialize the research service
    research_service = ResearchService(db)
    
    # Automatically generate competitive market analysis
    analysis_result = await research_service.generate_competitive_analysis(
        session_id=db_session.id,
        idea_title=session_data.title,
        idea_description=session_data.description
    )
    
    # Log the analysis result
    print(f"Competitive analysis result for session {db_session.id}: {analysis_result}")
    
    return db_session

@router.post("/ideas/submit")
async def submit_idea_with_analysis(
    idea_data: ResearchSessionCreate,
    db: AsyncSession = Depends(get_db)
):
    """Submit a new idea and automatically generate competitive market analysis"""
    try:
        # Create the research session for the idea
        db_session = ResearchSession(
            user_id=1,  # TODO: Get from authenticated user
            idea_id=idea_data.idea_id,
            title=idea_data.title,
            description=idea_data.description,
            status="researching"  # Set status to researching since we're analyzing
        )
        db.add(db_session)
        await db.commit()
        await db.refresh(db_session)
        
        # Initialize the research service
        research_service = ResearchService(db)
        
        # Store initial conversation
        await research_service.store_conversation(
            session_id=db_session.id,
            message_type="user",
            content=f"New idea submitted: {idea_data.title}",
            metadata={"idea_description": idea_data.description}
        )
        
        # Generate competitive market analysis
        analysis_result = await research_service.generate_competitive_analysis(
            session_id=db_session.id,
            idea_title=idea_data.title,
            idea_description=idea_data.description
        )
        
        # Get the updated session with all data
        session_with_data = await research_service.get_session_with_data(db_session.id)
        
        return {
            "session_id": db_session.id,
            "idea_id": db_session.idea_id,
            "title": db_session.title,
            "status": "completed" if analysis_result["status"] == "completed" else "researching",
            "analysis_result": analysis_result,
            "insights_count": len(session_with_data.insights) if session_with_data else 0,
            "options_count": len(session_with_data.options) if session_with_data else 0,
            "message": "Idea submitted successfully. Competitive market analysis has been generated.",
            "next_steps": [
                "Review the competitive analysis insights",
                "Explore strategic options",
                "Continue brainstorming with AI",
                "Generate detailed reports"
            ]
        }
        
    except Exception as e:
        print(f"Error submitting idea: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to submit idea: {str(e)}"
        )

@router.get("/sessions/{session_id}", response_model=ResearchSessionDetail)
async def get_research_session(
    session_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get research session with all related data"""
    result = await db.execute(
        select(ResearchSession)
        .options(
            selectinload(ResearchSession.conversations),
            selectinload(ResearchSession.insights),
            selectinload(ResearchSession.options)
        )
        .where(ResearchSession.id == session_id)
    )
    session = result.scalar_one_or_none()
    
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    
    # Convert SQLAlchemy model to Pydantic model
    return ResearchSessionDetail(
        id=session.id,
        user_id=session.user_id,
        idea_id=session.idea_id,
        title=session.title,
        description=session.description,
        status=session.status,
        created_at=session.created_at,
        updated_at=session.updated_at,
        conversations=[
            ResearchConversationSchema(
                id=conv.id,
                session_id=conv.session_id,
                message_type=conv.message_type,
                content=conv.content,
                metadata=conv.message_metadata,
                created_at=conv.created_at
            ) for conv in session.conversations
        ],
        insights=[
            ResearchInsightSchema(
                id=ins.id,
                session_id=ins.session_id,
                category=ins.category,
                subcategory=ins.subcategory,
                title=ins.title,
                description=ins.description,
                data=ins.data,
                confidence_score=ins.confidence_score,
                sources=ins.sources,
                is_validated=ins.is_validated,
                created_at=ins.created_at,
                updated_at=ins.updated_at
            ) for ins in session.insights
        ],
        options=[
            ResearchOptionSchema(
                id=opt.id,
                session_id=opt.session_id,
                category=opt.category,
                title=opt.title,
                description=opt.description,
                pros=opt.pros,
                cons=opt.cons,
                feasibility_score=opt.feasibility_score,
                impact_score=opt.impact_score,
                risk_score=opt.risk_score,
                recommended=opt.recommended,
                metadata=opt.option_metadata,
                created_at=opt.created_at,
                updated_at=opt.updated_at
            ) for opt in session.options
        ]
    )

@router.get("/sessions", response_model=List[ResearchSessionSchema])
async def list_research_sessions(
    skip: int = 0,
    limit: int = 100,
    db: AsyncSession = Depends(get_db)
):
    """List user's research sessions"""
    result = await db.execute(
        select(ResearchSession)
        .where(ResearchSession.user_id == 1)  # TODO: Get from authenticated user
        .offset(skip)
        .limit(limit)
        .order_by(ResearchSession.created_at.desc())
    )
    sessions = result.scalars().all()
    
    # Convert SQLAlchemy objects to Pydantic models
    return [
        ResearchSessionSchema(
            id=session.id,
            user_id=session.user_id,
            idea_id=session.idea_id,
            title=session.title,
            description=session.description,
            status=session.status,
            created_at=session.created_at,
            updated_at=session.updated_at
        )
        for session in sessions
    ]

@router.put("/sessions/{session_id}", response_model=ResearchSessionSchema)
async def update_research_session(
    session_id: int,
    session_data: ResearchSessionUpdate,
    db: AsyncSession = Depends(get_db)
):
    """Update research session"""
    result = await db.execute(
        select(ResearchSession).where(ResearchSession.id == session_id)
    )
    session = result.scalar_one_or_none()
    
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    
    for field, value in session_data.dict(exclude_unset=True).items():
        setattr(session, field, value)
    
    await db.commit()
    await db.refresh(session)
    return session

@router.delete("/sessions/{session_id}", status_code=204)
async def delete_research_session(
    session_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Delete a research session and all related data"""
    result = await db.execute(
        select(ResearchSession).where(ResearchSession.id == session_id)
    )
    session = result.scalar_one_or_none()
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    await db.delete(session)
    await db.commit()
    return None

# AI-Powered Brainstorming
@router.post("/sessions/{session_id}/brainstorm", response_model=BrainstormResponse)
async def brainstorm(
    session_id: int,
    request: BrainstormRequest,
    db: AsyncSession = Depends(get_db)
):
    """AI-powered brainstorming conversation with automatic categorization"""
    research_service = ResearchService(db)
    
    # Verify session exists
    session = await research_service.get_session_with_data(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    
    # Perform brainstorming with categorization and storage
    return await research_service.perform_brainstorming(session_id, request)

# Analysis and Insights
@router.post("/sessions/{session_id}/analyze")
async def perform_analysis(
    session_id: int,
    request: AnalysisRequest,
    db: AsyncSession = Depends(get_db)
):
    """Perform specific type of analysis"""
    # Get session and insights
    session_result = await db.execute(
        select(ResearchSession).where(ResearchSession.id == session_id)
    )
    session = session_result.scalar_one_or_none()
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    
    insights_result = await db.execute(
        select(ResearchInsight).where(ResearchInsight.session_id == session_id)
    )
    insights = insights_result.scalars().all()
    
    # Prepare insights data for AI
    insights_data = [
        {
            "category": insight.category,
            "title": insight.title,
            "description": insight.description,
            "data": insight.data,
            "confidence_score": insight.confidence_score
        }
        for insight in insights
    ]
    
    # Perform AI analysis
    analysis_result = await ai_orchestrator.perform_analysis(
        analysis_type=request.analysis_type,
        idea=session.description or session.title,
        insights=insights_data,
        parameters=request.parameters
    )
    
    return analysis_result

@router.get("/sessions/{session_id}/analysis")
async def get_research_analysis(
    session_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get comprehensive categorized analysis of all research data"""
    research_service = ResearchService(db)
    return await research_service.get_categorized_analysis(session_id)

# Fact Checking
@router.post("/insights/{insight_id}/fact-check")
async def fact_check_insight(
    insight_id: int,
    request: FactCheckCreate,
    db: AsyncSession = Depends(get_db)
):
    """Fact-check and validate a specific insight"""
    research_service = ResearchService(db)
    
    try:
        return await research_service.validate_insight(insight_id, request.claim)
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))

# Reports
@router.post("/sessions/{session_id}/reports")
async def generate_report(
    session_id: int,
    request: ReportRequest,
    db: AsyncSession = Depends(get_db)
):
    if request.report_type == "executive_summary":
        # Fetch session and related data
        result = await db.execute(
            select(ResearchSession)
            .options(
                selectinload(ResearchSession.insights),
                selectinload(ResearchSession.options)
            )
            .where(ResearchSession.id == session_id)
        )
        session = result.scalar_one_or_none()
        if not session:
            raise HTTPException(status_code=404, detail="Session not found")

        insights = session.insights[:5] if hasattr(session, "insights") else []
        options = session.options[:3] if hasattr(session, "options") else []
        logo_url = "https://placehold.co/120x40?text=LOGO"
        html_content = f"""
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 40px; }}
                .header {{ display: flex; align-items: center; margin-bottom: 32px; }}
                .logo {{ height: 40px; margin-right: 24px; }}
                .title {{ font-size: 2em; font-weight: bold; color: #2d3748; }}
                .section-title {{ font-size: 1.2em; font-weight: bold; margin-top: 32px; color: #4a5568; }}
                .insight, .option {{ margin-bottom: 12px; }}
                .stat {{ margin-right: 24px; display: inline-block; }}
                .footer {{ margin-top: 48px; font-size: 0.9em; color: #888; }}
            </style>
        </head>
        <body>
            <div class="header">
                <img src="{logo_url}" class="logo" />
                <div>
                    <div class="title">Executive Summary</div>
                    <div>Idea: <b>{session.title}</b></div>
                    <div style="font-size: 1em; color: #666;">{session.description}</div>
                </div>
            </div>
            <div>
                <div class="section-title">Key Insights</div>
                {"".join(f'<div class="insight"><b>{i.title}</b>: {i.description}</div>' for i in insights) or "<div>No insights available.</div>"}
            </div>
            <div>
                <div class="section-title">Top Recommendations</div>
                {"".join(f'<div class="option"><b>{o.title}</b>: {o.description}</div>' for o in options) or "<div>No recommendations available.</div>"}
            </div>
            <div>
                <div class="section-title">Summary Statistics</div>
                <div class="stat">Total Insights: <b>{len(session.insights) if hasattr(session, 'insights') else 0}</b></div>
                <div class="stat">Total Options: <b>{len(session.options) if hasattr(session, 'options') else 0}</b></div>
            </div>
            <div class="footer">
                Generated by Ideas Matter &mdash; {session.created_at.strftime('%Y-%m-%d')}
            </div>
        </body>
        </html>
        """
        pdf_bytes = HTML(string=html_content).write_pdf()
        return StreamingResponse(
            io.BytesIO(pdf_bytes),
            media_type="application/pdf",
            headers={"Content-Disposition": "attachment; filename=executive_summary_report.pdf"}
        )
    # ... existing logic for other report types ...

# Helper endpoints
@router.get("/sessions/{session_id}/next-steps")
async def get_next_steps(
    session_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get AI recommendations for next steps"""
    # Get session data
    result = await db.execute(
        select(ResearchSession)
        .options(
            selectinload(ResearchSession.insights),
            selectinload(ResearchSession.options)
        )
        .where(ResearchSession.id == session_id)
    )
    session = result.scalar_one_or_none()
    if not session:
        raise HTTPException(status_code=404, detail="Research session not found")
    
    # Prepare session data for AI
    session_data = {
        "insights": [
            {
                "category": insight.category,
                "title": insight.title,
                "confidence_score": insight.confidence_score
            }
            for insight in session.insights
        ],
        "options": [
            {
                "category": option.category,
                "title": option.title,
                "feasibility_score": option.feasibility_score
            }
            for option in session.options
        ]
    }
    
    recommendations = await ai_orchestrator.recommend_next_steps(session_data)
    
    return {"recommendations": recommendations}

# SWOT Analysis Endpoints
@router.post("/options/{option_id}/swot", response_model=SwotAnalysisResponse)
def generate_swot_analysis(
    option_id: int,
    request: SwotAnalysisRequest,
    db: Session = Depends(get_sync_db)
):
    """Generate or regenerate SWOT analysis for a specific option"""
    from app.services.swot_service import SwotAnalysisService
    from app.schemas.research import SwotAnalysisResponse
    
    # Initialize SWOT service
    swot_service = SwotAnalysisService(ai_orchestrator)
    
    # Generate SWOT analysis
    swot = swot_service.generate_swot_analysis(
        db=db,
        option_id=option_id,
        regenerate=request.regenerate
    )
    
    return SwotAnalysisResponse(option_id=option_id, swot=swot)

@router.get("/options/{option_id}/swot/pdf")
def generate_swot_pdf(
    option_id: int,
    include_metadata: bool = True,
    db: Session = Depends(get_sync_db)
):
    """Generate a PDF report for SWOT analysis"""
    from app.services.pdf_service import PdfService
    from app.services.swot_service import SwotAnalysisService
    
    # Get the option with its session
    option = db.query(ResearchOption).filter(
        ResearchOption.id == option_id
    ).first()
    
    if not option:
        raise HTTPException(status_code=404, detail="Option not found")
    
    # Get the session
    session = db.query(ResearchSession).filter(
        ResearchSession.id == option.session_id
    ).first()
    
    # Get or generate SWOT analysis
    swot_service = SwotAnalysisService(ai_orchestrator)
    swot = swot_service.generate_swot_analysis(db=db, option_id=option_id)
    
    # Generate PDF
    pdf_service = PdfService()
    pdf_bytes = pdf_service.generate_swot_pdf(
        option=option,
        session=session,
        swot=swot,
        include_metadata=include_metadata
    )
    
    # Return PDF as streaming response
    return StreamingResponse(
        io.BytesIO(pdf_bytes),
        media_type="application/pdf",
        headers={
            "Content-Disposition": f"attachment; filename=swot_analysis_{option_id}.pdf"
        }
    )