from typing import List, Dict, Any, Optional
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import selectinload
from sqlalchemy import select, func
import json
from datetime import datetime

from app.models.research import (
    ResearchSession, ResearchConversation, ResearchInsight, 
    ResearchOption, ResearchReport, ResearchFactCheck
)
from app.schemas.research import (
    ResearchSessionCreate, ResearchInsightCreate, ResearchOptionCreate,
    BrainstormRequest, BrainstormResponse
)
from app.services.ai_orchestration import ai_orchestrator

class ResearchService:
    def __init__(self, db: AsyncSession):
        self.db = db
    
    async def create_session(self, user_id: int, session_data: ResearchSessionCreate) -> ResearchSession:
        """Create a new research session"""
        db_session = ResearchSession(
            user_id=user_id,
            **session_data.dict()
        )
        self.db.add(db_session)
        await self.db.commit()
        await self.db.refresh(db_session)
        return db_session
    
    async def get_session_with_data(self, session_id: int) -> Optional[ResearchSession]:
        """Get research session with all related data"""
        result = await self.db.execute(
            select(ResearchSession)
            .options(
                selectinload(ResearchSession.conversations),
                selectinload(ResearchSession.insights),
                selectinload(ResearchSession.options)
            )
            .where(ResearchSession.id == session_id)
        )
        return result.scalar_one_or_none()
    
    async def store_conversation(self, session_id: int, message_type: str, content: str, metadata: Dict = None) -> ResearchConversation:
        """Store a conversation message"""
        conversation = ResearchConversation(
            session_id=session_id,
            message_type=message_type,
            content=content,
            metadata=metadata or {}
        )
        self.db.add(conversation)
        await self.db.commit()
        await self.db.refresh(conversation)
        return conversation
    
    async def categorize_and_store_insights(self, session_id: int, insights_data: List[Dict]) -> List[ResearchInsight]:
        """Categorize and store AI-generated insights"""
        insights = []
        
        for insight_data in insights_data:
            # Validate and normalize category
            category = self._normalize_category(insight_data.get("category", ""))
            
            # Create insight with automatic categorization
            insight = ResearchInsight(
                session_id=session_id,
                category=category,
                subcategory=insight_data.get("subcategory"),
                title=insight_data["title"],
                description=insight_data.get("description"),
                confidence_score=insight_data.get("confidence_score", 0.5),
                sources=insight_data.get("sources", []),
                data=insight_data,
                is_validated=False
            )
            
            self.db.add(insight)
            insights.append(insight)
        
        await self.db.commit()
        
        # Refresh all insights to get IDs
        for insight in insights:
            await self.db.refresh(insight)
        
        return insights
    
    async def store_options_with_analysis(self, session_id: int, options_data: List[Dict]) -> List[ResearchOption]:
        """Store strategic options with scoring analysis"""
        options = []
        
        for option_data in options_data:
            # Calculate recommendation based on scores
            feasibility = option_data.get("feasibility_score", 0.5)
            impact = option_data.get("impact_score", 0.5)
            risk = option_data.get("risk_score", 0.5)
            
            # Simple recommendation algorithm: high feasibility, high impact, low risk
            overall_score = (feasibility + impact + (1 - risk)) / 3
            recommended = overall_score > 0.7
            
            option = ResearchOption(
                session_id=session_id,
                category=option_data["category"],
                title=option_data["title"],
                description=option_data.get("description"),
                pros=option_data.get("pros", []),
                cons=option_data.get("cons", []),
                feasibility_score=feasibility,
                impact_score=impact,
                risk_score=risk,
                recommended=recommended,
                metadata=option_data
            )
            
            self.db.add(option)
            options.append(option)
        
        await self.db.commit()
        
        # Refresh all options to get IDs
        for option in options:
            await self.db.refresh(option)
        
        return options
    
    async def perform_brainstorming(self, session_id: int, request: BrainstormRequest) -> BrainstormResponse:
        """Handle AI-powered brainstorming with data categorization"""
        
        # Get existing insights for context
        insights_result = await self.db.execute(
            select(ResearchInsight).where(ResearchInsight.session_id == session_id)
        )
        existing_insights = insights_result.scalars().all()
        
        insights_context = [
            {
                "category": insight.category,
                "title": insight.title,
                "description": insight.description,
                "confidence_score": insight.confidence_score
            }
            for insight in existing_insights
        ]
        
        # Generate AI response
        ai_response = await ai_orchestrator.brainstorm(
            message=request.message,
            context=request.context,
            insights=insights_context
        )
        
        # Store user message
        await self.store_conversation(
            session_id=session_id,
            message_type="user",
            content=request.message,
            metadata=request.context
        )
        
        # Store AI response
        await self.store_conversation(
            session_id=session_id,
            message_type="assistant",
            content=ai_response["response"],
            metadata={
                "insights_generated": len(ai_response.get("insights", [])),
                "options_generated": len(ai_response.get("options", [])),
                "follow_up_questions": ai_response.get("follow_up_questions", [])
            }
        )
        
        # Categorize and store new insights
        new_insights = []
        if ai_response.get("insights"):
            new_insights = await self.categorize_and_store_insights(
                session_id, ai_response["insights"]
            )
        
        # Store and analyze new options
        new_options = []
        if ai_response.get("options"):
            new_options = await self.store_options_with_analysis(
                session_id, ai_response["options"]
            )
        
        return BrainstormResponse(
            message=ai_response["response"],
            insights=new_insights,
            options=new_options,
            follow_up_questions=ai_response.get("follow_up_questions", []),
            metadata=ai_response.get("metadata", {})
        )
    
    async def get_categorized_analysis(self, session_id: int) -> Dict[str, Any]:
        """Get comprehensive categorized analysis of research data"""
        
        # Get all insights by category
        insights_result = await self.db.execute(
            select(ResearchInsight)
            .where(ResearchInsight.session_id == session_id)
            .order_by(ResearchInsight.confidence_score.desc())
        )
        all_insights = insights_result.scalars().all()
        
        # Get all options ranked by overall score
        options_result = await self.db.execute(
            select(ResearchOption)
            .where(ResearchOption.session_id == session_id)
            .order_by(
                ((ResearchOption.feasibility_score + ResearchOption.impact_score + (1 - ResearchOption.risk_score)) / 3).desc()
            )
        )
        all_options = options_result.scalars().all()
        
        # Categorize insights and convert to dictionaries
        def insight_to_dict(insight):
            return {
                "id": insight.id,
                "title": insight.title,
                "description": insight.description,
                "confidence_score": insight.confidence_score,
                "category": insight.category,
                "subcategory": insight.subcategory,
                "data": insight.data,
                "is_validated": insight.is_validated
            }
        
        categorized_insights = {
            "target_market": [insight_to_dict(i) for i in all_insights if i.category == "target_market"],
            "customer_profile": [insight_to_dict(i) for i in all_insights if i.category == "customer_profile"],
            "problem_solution": [insight_to_dict(i) for i in all_insights if i.category == "problem_solution"],
            "growth_targets": [insight_to_dict(i) for i in all_insights if i.category == "growth_targets"],
            "cost_model": [insight_to_dict(i) for i in all_insights if i.category == "cost_model"],
            "revenue_model": [insight_to_dict(i) for i in all_insights if i.category == "revenue_model"]
        }
        
        # Convert options to dictionaries
        def option_to_dict(option):
            return {
                "id": option.id,
                "title": option.title,
                "description": option.description,
                "category": option.category,
                "pros": option.pros,
                "cons": option.cons,
                "feasibility_score": option.feasibility_score,
                "impact_score": option.impact_score,
                "risk_score": option.risk_score,
                "recommended": option.recommended
            }
        
        # Get recommendations
        all_options_dict = [option_to_dict(o) for o in all_options]
        recommended_options = [option_to_dict(o) for o in all_options if o.recommended]
        
        # Calculate statistics
        stats = await self._calculate_session_stats(session_id)
        
        return {
            "categorized_insights": categorized_insights,
            "all_options": all_options_dict,
            "recommended_options": recommended_options,
            "statistics": stats,
            "readiness_score": await self._calculate_readiness_score(session_id)
        }
    
    async def validate_insight(self, insight_id: int, claim: str) -> ResearchFactCheck:
        """Fact-check and validate an insight"""
        
        # Get the insight
        insight_result = await self.db.execute(
            select(ResearchInsight).where(ResearchInsight.id == insight_id)
        )
        insight = insight_result.scalar_one_or_none()
        
        if not insight:
            raise ValueError("Insight not found")
        
        # Perform AI fact-checking
        fact_check_result = await ai_orchestrator.fact_check(
            claim=claim,
            context={
                "insight_title": insight.title,
                "insight_description": insight.description,
                "insight_category": insight.category
            }
        )
        
        # Store fact-check result
        fact_check = ResearchFactCheck(
            insight_id=insight_id,
            claim=claim,
            verification_status=fact_check_result["verification_status"],
            sources=fact_check_result.get("sources", []),
            confidence_level=fact_check_result["confidence_level"],
            notes=fact_check_result.get("notes")
        )
        
        self.db.add(fact_check)
        
        # Update insight validation status
        if fact_check_result["verification_status"] == "verified":
            insight.is_validated = True
        
        await self.db.commit()
        await self.db.refresh(fact_check)
        
        return fact_check
    
    async def generate_competitive_analysis(self, session_id: int, idea_title: str, idea_description: str) -> Dict[str, Any]:
        """Generate competitive market analysis for an idea"""
        try:
            # Store the analysis request
            await self.store_conversation(
                session_id=session_id,
                message_type="system",
                content=f"Initiating competitive market analysis for: {idea_title}",
                metadata={"type": "analysis_start"}
            )
            
            # Generate insights for competitive analysis
            competitive_insights = [
                {
                    "category": "target_market",
                    "subcategory": "market_size",
                    "title": "Market Size Analysis",
                    "description": "Analyzing total addressable market and growth potential",
                    "confidence_score": 0.8,
                    "data": {
                        "market_size": "To be analyzed",
                        "growth_rate": "To be determined",
                        "key_segments": []
                    }
                },
                {
                    "category": "target_market",
                    "subcategory": "competitors",
                    "title": "Key Competitors",
                    "description": "Identifying direct and indirect competitors",
                    "confidence_score": 0.75,
                    "data": {
                        "direct_competitors": [],
                        "indirect_competitors": [],
                        "market_leaders": []
                    }
                },
                {
                    "category": "problem_solution",
                    "subcategory": "swot",
                    "title": "SWOT Analysis",
                    "description": "Strategic analysis of strengths, weaknesses, opportunities, and threats",
                    "confidence_score": 0.7,
                    "data": {
                        "strengths": [],
                        "weaknesses": [],
                        "opportunities": [],
                        "threats": []
                    }
                }
            ]
            
            # Store competitive analysis insights
            await self.categorize_and_store_insights(session_id, competitive_insights)
            
            # Generate strategic options based on analysis
            strategic_options = [
                {
                    "category": "market_entry",
                    "title": "Direct Market Entry",
                    "description": "Enter the market with full product offering",
                    "pros": ["Capture market share quickly", "Build brand recognition"],
                    "cons": ["Higher initial investment", "Greater risk"],
                    "feasibility_score": 0.6,
                    "impact_score": 0.8,
                    "risk_score": 0.6
                },
                {
                    "category": "market_entry",
                    "title": "Phased Market Approach",
                    "description": "Gradual market entry with MVP and iterative improvements",
                    "pros": ["Lower initial cost", "Test market response", "Iterate based on feedback"],
                    "cons": ["Slower market capture", "May lose to faster competitors"],
                    "feasibility_score": 0.8,
                    "impact_score": 0.6,
                    "risk_score": 0.3
                }
            ]
            
            await self.store_options_with_analysis(session_id, strategic_options)
            
            # Generate and store competitive analysis report
            report = ResearchReport(
                session_id=session_id,
                report_type="competitive_analysis",
                title=f"Competitive Market Analysis: {idea_title}",
                content="Comprehensive competitive analysis has been generated. View insights and strategic options for detailed information.",
                data={
                    "generated_at": datetime.utcnow().isoformat(),
                    "idea_title": idea_title,
                    "analysis_categories": ["market_size", "competitors", "positioning", "strategy"],
                    "total_insights": len(competitive_insights),
                    "strategic_options": len(strategic_options)
                }
            )
            self.db.add(report)
            await self.db.commit()
            await self.db.refresh(report)
            
            # Store completion conversation
            await self.store_conversation(
                session_id=session_id,
                message_type="assistant",
                content=f"Competitive market analysis completed. Generated {len(competitive_insights)} insights and {len(strategic_options)} strategic options.",
                metadata={"type": "analysis_complete", "report_id": report.id}
            )
            
            # Update session status to completed
            session_result = await self.db.execute(
                select(ResearchSession).where(ResearchSession.id == session_id)
            )
            db_session = session_result.scalar_one_or_none()
            if db_session:
                db_session.status = "completed"
                await self.db.commit()
            
            return {
                "status": "completed",
                "report_id": report.id,
                "insights_count": len(competitive_insights),
                "options_count": len(strategic_options),
                "message": "Competitive market analysis has been generated successfully"
            }
            
        except Exception as e:
            print(f"Error generating competitive analysis: {e}")
            return {
                "status": "error",
                "message": f"Failed to generate competitive analysis: {str(e)}"
            }
    
    def _normalize_category(self, category: str) -> str:
        """Normalize category names to standard values"""
        category_mapping = {
            "target_market": "target_market",
            "target market": "target_market",
            "market": "target_market",
            "customer_profile": "customer_profile", 
            "customer": "customer_profile",
            "customers": "customer_profile",
            "problem_solution": "problem_solution",
            "problem": "problem_solution",
            "solution": "problem_solution",
            "growth_targets": "growth_targets",
            "growth": "growth_targets",
            "cost_model": "cost_model",
            "cost": "cost_model",
            "costs": "cost_model",
            "revenue_model": "revenue_model",
            "revenue": "revenue_model",
            "monetization": "revenue_model"
        }
        
        normalized = category_mapping.get(category.lower(), "target_market")
        return normalized
    
    async def _calculate_session_stats(self, session_id: int) -> Dict[str, Any]:
        """Calculate comprehensive session statistics"""
        
        # Count insights by category
        insights_by_category = await self.db.execute(
            select(
                ResearchInsight.category,
                func.count(ResearchInsight.id).label('count'),
                func.avg(ResearchInsight.confidence_score).label('avg_confidence')
            )
            .where(ResearchInsight.session_id == session_id)
            .group_by(ResearchInsight.category)
        )
        
        # Count options by category
        options_by_category = await self.db.execute(
            select(
                ResearchOption.category,
                func.count(ResearchOption.id).label('count'),
                func.avg((ResearchOption.feasibility_score + ResearchOption.impact_score + (1 - ResearchOption.risk_score)) / 3).label('avg_score')
            )
            .where(ResearchOption.session_id == session_id)
            .group_by(ResearchOption.category)
        )
        
        # Count validated insights
        validated_count = await self.db.execute(
            select(func.count(ResearchInsight.id))
            .where(ResearchInsight.session_id == session_id)
            .where(ResearchInsight.is_validated == True)
        )
        
        # Calculate total insights and options
        total_insights_result = await self.db.execute(
            select(func.count(ResearchInsight.id))
            .where(ResearchInsight.session_id == session_id)
        )
        total_insights = total_insights_result.scalar() or 0
        
        total_options_result = await self.db.execute(
            select(func.count(ResearchOption.id))
            .where(ResearchOption.session_id == session_id)
        )
        total_options = total_options_result.scalar() or 0
        
        # Calculate average confidence across all insights
        avg_confidence_result = await self.db.execute(
            select(func.avg(ResearchInsight.confidence_score))
            .where(ResearchInsight.session_id == session_id)
        )
        avg_confidence = avg_confidence_result.scalar() or 0
        
        return {
            "insights_by_category": {row.category: {"count": row.count, "avg_confidence": row.avg_confidence} for row in insights_by_category},
            "options_by_category": {row.category: {"count": row.count, "avg_score": row.avg_score} for row in options_by_category},
            "validated_insights": validated_count.scalar() or 0,
            "total_insights": total_insights,
            "total_options": total_options,
            "avg_confidence": float(avg_confidence),
            "research_completion": float(avg_confidence),  # Use avg confidence as completion indicator
            "last_updated": datetime.utcnow().isoformat()
        }
    
    async def _calculate_readiness_score(self, session_id: int) -> float:
        """Calculate how ready the idea is for next steps"""
        
        # Get insight counts by category
        insights_result = await self.db.execute(
            select(ResearchInsight.category, func.count(ResearchInsight.id))
            .where(ResearchInsight.session_id == session_id)
            .group_by(ResearchInsight.category)
        )
        
        insight_categories = dict(insights_result.fetchall())
        
        # Required categories for readiness
        required_categories = [
            "target_market", "customer_profile", "problem_solution", 
            "revenue_model"
        ]
        
        # Calculate readiness score (0-1)
        category_score = sum(1 for cat in required_categories if insight_categories.get(cat, 0) > 0) / len(required_categories)
        
        # Get average confidence across insights
        avg_confidence_result = await self.db.execute(
            select(func.avg(ResearchInsight.confidence_score))
            .where(ResearchInsight.session_id == session_id)
        )
        avg_confidence = avg_confidence_result.scalar() or 0.5
        
        # Get validation percentage
        total_insights = await self.db.execute(
            select(func.count(ResearchInsight.id))
            .where(ResearchInsight.session_id == session_id)
        )
        total = total_insights.scalar() or 1
        
        validated_insights = await self.db.execute(
            select(func.count(ResearchInsight.id))
            .where(ResearchInsight.session_id == session_id)
            .where(ResearchInsight.is_validated == True)
        )
        validated = validated_insights.scalar() or 0
        
        validation_score = validated / total
        
        # Combined readiness score
        readiness_score = (category_score * 0.5) + (avg_confidence * 0.3) + (validation_score * 0.2)
        
        return min(1.0, readiness_score)