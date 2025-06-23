import logging
from typing import List, Dict, Any, Optional
from datetime import datetime
from sqlalchemy.orm import Session
from sqlalchemy.sql import func

from app.models.research import ResearchOption, ResearchSession, ResearchInsight
from app.services.ai_orchestration_simple import SimpleAIOrchestrator
from app.schemas.research import SwotAnalysis

logger = logging.getLogger(__name__)

class SwotAnalysisService:
    def __init__(self, ai_service: SimpleAIOrchestrator):
        self.ai_service = ai_service
    
    def generate_swot_analysis(
        self,
        db: Session,
        option_id: int,
        regenerate: bool = False
    ) -> SwotAnalysis:
        """Generate SWOT analysis for a specific option"""
        
        # Get the option
        option = db.query(ResearchOption).filter(ResearchOption.id == option_id).first()
        if not option:
            raise ValueError(f"Option with id {option_id} not found")
        
        # Check if we already have SWOT analysis and regenerate is False
        if not regenerate and option.swot_strengths:
            return SwotAnalysis(
                strengths=option.swot_strengths or [],
                weaknesses=option.swot_weaknesses or [],
                opportunities=option.swot_opportunities or [],
                threats=option.swot_threats or [],
                confidence=option.swot_confidence or 0.7,
                generated_at=option.swot_generated_at
            )
        
        # Get the session and related insights
        session = db.query(ResearchSession).filter(
            ResearchSession.id == option.session_id
        ).first()
        
        # Get all insights for context
        insights = db.query(ResearchInsight).filter(
            ResearchInsight.session_id == option.session_id
        ).all()
        
        # Build context for AI
        context = self._build_swot_context(option, session, insights)
        
        # Generate SWOT analysis using AI
        swot_data = self._generate_swot_with_ai(context)
        
        # Update the option with SWOT data
        option.swot_strengths = swot_data['strengths']
        option.swot_weaknesses = swot_data['weaknesses']
        option.swot_opportunities = swot_data['opportunities']
        option.swot_threats = swot_data['threats']
        option.swot_generated_at = datetime.utcnow()
        option.swot_confidence = swot_data.get('confidence', 0.7)
        
        db.commit()
        
        return SwotAnalysis(
            strengths=swot_data['strengths'],
            weaknesses=swot_data['weaknesses'],
            opportunities=swot_data['opportunities'],
            threats=swot_data['threats'],
            confidence=swot_data.get('confidence', 0.7),
            generated_at=option.swot_generated_at
        )
    
    def _build_swot_context(
        self,
        option: ResearchOption,
        session: ResearchSession,
        insights: List[ResearchInsight]
    ) -> Dict[str, Any]:
        """Build context for SWOT analysis generation"""
        
        context = {
            'option': {
                'title': option.title,
                'description': option.description,
                'category': option.category,
                'pros': option.pros or [],
                'cons': option.cons or [],
                'feasibility_score': option.feasibility_score,
                'impact_score': option.impact_score,
                'risk_score': option.risk_score,
                'metadata': option.option_metadata or {}
            },
            'session': {
                'title': session.title,
                'description': session.description
            },
            'insights': {}
        }
        
        # Organize insights by category
        for insight in insights:
            category = insight.category
            if category not in context['insights']:
                context['insights'][category] = []
            
            context['insights'][category].append({
                'title': insight.title,
                'description': insight.description,
                'data': insight.data,
                'subcategory': insight.subcategory
            })
        
        return context
    
    def _generate_swot_with_ai(self, context: Dict[str, Any]) -> Dict[str, List[str]]:
        """Generate SWOT analysis using AI"""
        
        prompt = f"""
        Based on the following market option and research insights, generate a comprehensive SWOT analysis.
        
        Option Details:
        - Title: {context['option']['title']}
        - Description: {context['option']['description']}
        - Category: {context['option']['category']}
        - Pros: {', '.join(context['option']['pros'])}
        - Cons: {', '.join(context['option']['cons'])}
        - Feasibility Score: {context['option']['feasibility_score']}
        - Impact Score: {context['option']['impact_score']}
        - Risk Score: {context['option']['risk_score']}
        
        Research Context:
        - Idea: {context['session']['title']}
        - Description: {context['session']['description']}
        
        Available Insights:
        {self._format_insights_for_prompt(context['insights'])}
        
        Generate a SWOT analysis with:
        1. Strengths (3-5 internal positive factors)
        2. Weaknesses (3-5 internal negative factors)
        3. Opportunities (3-5 external positive factors)
        4. Threats (3-5 external negative factors)
        
        Each point should be specific, actionable, and directly related to this option.
        Focus on factors that would impact the success of this specific market option.
        
        Return the analysis in JSON format with keys: strengths, weaknesses, opportunities, threats (each as an array of strings).
        Also include a 'confidence' score (0.0-1.0) based on the quality and quantity of available data.
        """
        
        try:
            # Use the AI service to generate SWOT analysis
            response_text = self.ai_service.process_message(prompt, "swot_analysis")
            
            # Try to parse JSON from the response
            import json
            import re
            
            # Extract JSON from the response if it's wrapped in text
            json_match = re.search(r'\{.*\}', response_text, re.DOTALL)
            if json_match:
                response_data = json.loads(json_match.group())
            else:
                # Try to parse the entire response as JSON
                response_data = json.loads(response_text)
            
            # Ensure we have the required structure
            swot_data = {
                'strengths': response_data.get('strengths', []),
                'weaknesses': response_data.get('weaknesses', []),
                'opportunities': response_data.get('opportunities', []),
                'threats': response_data.get('threats', []),
                'confidence': response_data.get('confidence', 0.7)
            }
            
            return swot_data
            
        except Exception as e:
            logger.error(f"Error generating SWOT analysis: {str(e)}")
            
            # Fallback to basic analysis based on pros/cons
            return self._generate_fallback_swot(context)
    
    def _format_insights_for_prompt(self, insights: Dict[str, List[Dict]]) -> str:
        """Format insights for the AI prompt"""
        formatted = []
        
        for category, items in insights.items():
            if items:
                formatted.append(f"\n{category.replace('_', ' ').title()}:")
                for item in items[:3]:  # Limit to top 3 per category
                    formatted.append(f"- {item['title']}: {item.get('description', '')}")
        
        return '\n'.join(formatted) if formatted else "No specific insights available"
    
    def _generate_fallback_swot(self, context: Dict[str, Any]) -> Dict[str, List[str]]:
        """Generate basic SWOT analysis as fallback"""
        
        option = context['option']
        
        # Convert pros to strengths/opportunities
        strengths = []
        opportunities = []
        
        for i, pro in enumerate(option['pros'][:5]):
            if i % 2 == 0:
                strengths.append(pro)
            else:
                opportunities.append(pro)
        
        # Convert cons to weaknesses/threats
        weaknesses = []
        threats = []
        
        for i, con in enumerate(option['cons'][:5]):
            if i % 2 == 0:
                weaknesses.append(con)
            else:
                threats.append(con)
        
        # Add score-based insights
        if option['feasibility_score'] > 0.7:
            strengths.append("High feasibility indicates strong implementation potential")
        elif option['feasibility_score'] < 0.3:
            weaknesses.append("Low feasibility may hinder successful implementation")
        
        if option['impact_score'] > 0.7:
            opportunities.append("High impact potential for significant market presence")
        elif option['impact_score'] < 0.3:
            threats.append("Low impact potential may limit growth opportunities")
        
        if option['risk_score'] > 0.7:
            threats.append("High risk profile requires careful risk management")
        elif option['risk_score'] < 0.3:
            strengths.append("Low risk profile provides stable foundation")
        
        return {
            'strengths': strengths[:5],
            'weaknesses': weaknesses[:5],
            'opportunities': opportunities[:5],
            'threats': threats[:5],
            'confidence': 0.5  # Lower confidence for fallback
        }