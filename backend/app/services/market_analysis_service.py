import logging
from typing import List, Dict, Any, Optional, Tuple
from datetime import datetime, timedelta
from sqlalchemy.orm import Session
from sqlalchemy import select, and_, or_

from app.models.research import ResearchSession, ResearchInsight
from app.models.market_analysis import (
    MarketAnalysis, CompetitorAnalysis, MarketSegment, 
    MarketTrendAnalysis, MarketOpportunity, CompetitorTier, MarketTrend
)
from app.schemas.market_analysis import (
    MarketAnalysisCreate, CompetitorAnalysisCreate, MarketSegmentCreate,
    MarketTrendAnalysisCreate, MarketOpportunityCreate,
    ComprehensiveMarketAnalysis, MarketSizingAnalysis, CompetitiveLandscape
)
from app.services.ai_orchestration_simple import SimpleAIOrchestrator
import json
import re

logger = logging.getLogger(__name__)

class MarketAnalysisService:
    def __init__(self, ai_service: SimpleAIOrchestrator):
        self.ai_service = ai_service
        
    def generate_comprehensive_market_analysis(
        self, 
        db: Session, 
        session_id: int, 
        idea_title: str, 
        idea_description: str
    ) -> ComprehensiveMarketAnalysis:
        """Generate a comprehensive market analysis including all components"""
        
        try:
            # Get existing research insights for context
            insights = db.query(ResearchInsight).filter(
                ResearchInsight.session_id == session_id
            ).all()
            
            # Generate market analysis using AI
            market_data = self._generate_market_analysis_data(
                idea_title, idea_description, insights
            )
            
            # Create main market analysis
            market_analysis = self._create_market_analysis(db, session_id, market_data)
            
            # Generate and create competitors
            competitors = self._generate_competitors(db, market_analysis.id, market_data)
            
            # Generate and create market segments
            segments = self._generate_market_segments(db, market_analysis.id, market_data)
            
            # Generate and create trend analysis
            trends = self._generate_trend_analysis(db, session_id, market_data)
            
            # Generate and create opportunities
            opportunities = self._generate_opportunities(db, session_id, market_data)
            
            return ComprehensiveMarketAnalysis(
                market_analysis=market_analysis,
                competitors=competitors,
                segments=segments,
                trends=trends,
                opportunities=opportunities
            )
            
        except Exception as e:
            logger.error(f"Error generating comprehensive market analysis: {str(e)}")
            # Return fallback analysis
            return self._generate_fallback_analysis(db, session_id, idea_title, idea_description)
    
    def calculate_market_sizing(
        self, 
        db: Session, 
        session_id: int, 
        geographic_scope: str = "global",
        target_segments: Optional[List[str]] = None
    ) -> MarketSizingAnalysis:
        """Calculate TAM, SAM, SOM with detailed breakdown"""
        
        try:
            # Get market analysis data
            market_analysis = db.query(MarketAnalysis).filter(
                MarketAnalysis.session_id == session_id
            ).first()
            
            if not market_analysis:
                return self._generate_fallback_sizing()
            
            # Calculate market sizing using various approaches
            tam = self._calculate_tam(market_analysis, geographic_scope)
            sam = self._calculate_sam(tam, market_analysis, target_segments)
            som = self._calculate_som(sam, market_analysis)
            
            # Generate growth projections
            growth_projections = self._generate_growth_projections(market_analysis)
            
            return MarketSizingAnalysis(
                tam=tam,
                sam=sam,
                som=som,
                market_size_breakdown={
                    "total_addressable_market": tam,
                    "serviceable_addressable_market": sam,
                    "serviceable_obtainable_market": som,
                    "penetration_rate": (som / sam * 100) if sam > 0 else 0
                },
                growth_projections=growth_projections,
                assumptions=[
                    f"Geographic scope: {geographic_scope}",
                    f"Analysis based on {market_analysis.market_size_year or 'current'} market data",
                    "Assumes consistent market growth rates",
                    "Excludes regulatory and economic disruptions"
                ],
                confidence_level=market_analysis.confidence_score or 0.6
            )
            
        except Exception as e:
            logger.error(f"Error calculating market sizing: {str(e)}")
            return self._generate_fallback_sizing()
    
    def analyze_competitive_landscape(
        self, 
        db: Session, 
        session_id: int
    ) -> CompetitiveLandscape:
        """Analyze the competitive landscape comprehensively"""
        
        try:
            # Get market analysis
            market_analysis = db.query(MarketAnalysis).filter(
                MarketAnalysis.session_id == session_id
            ).first()
            
            if not market_analysis:
                return CompetitiveLandscape()
            
            # Get all competitors
            competitors = db.query(CompetitorAnalysis).filter(
                CompetitorAnalysis.market_analysis_id == market_analysis.id
            ).all()
            
            # Categorize competitors
            direct = [c for c in competitors if c.tier == CompetitorTier.DIRECT]
            indirect = [c for c in competitors if c.tier == CompetitorTier.INDIRECT]
            substitutes = [c for c in competitors if c.tier == CompetitorTier.SUBSTITUTE]
            
            # Find market leader (highest market share)
            market_leader = None
            if direct:
                market_leader = max(direct, key=lambda x: x.market_share or 0)
            
            # Calculate competitive intensity
            competitive_intensity = self._calculate_competitive_intensity(competitors)
            
            # Determine market concentration
            market_concentration = self._determine_market_concentration(direct)
            
            return CompetitiveLandscape(
                direct_competitors=direct,
                indirect_competitors=indirect,
                substitute_threats=substitutes,
                market_leader=market_leader,
                competitive_intensity=competitive_intensity,
                market_concentration=market_concentration
            )
            
        except Exception as e:
            logger.error(f"Error analyzing competitive landscape: {str(e)}")
            return CompetitiveLandscape()
    
    def research_competitor(
        self, 
        db: Session, 
        market_analysis_id: int, 
        competitor_name: str, 
        research_depth: str = "standard"
    ) -> CompetitorAnalysis:
        """Research a specific competitor in detail"""
        
        try:
            # Generate competitor data using AI
            competitor_data = self._research_competitor_with_ai(competitor_name, research_depth)
            
            # Create competitor analysis record
            competitor = CompetitorAnalysis(
                market_analysis_id=market_analysis_id,
                name=competitor_name,
                **competitor_data
            )
            
            db.add(competitor)
            db.commit()
            db.refresh(competitor)
            
            return competitor
            
        except Exception as e:
            logger.error(f"Error researching competitor {competitor_name}: {str(e)}")
            # Return basic competitor record
            competitor = CompetitorAnalysis(
                market_analysis_id=market_analysis_id,
                name=competitor_name,
                tier=CompetitorTier.DIRECT,
                data_completeness=0.3
            )
            db.add(competitor)
            db.commit()
            db.refresh(competitor)
            return competitor
    
    def _generate_market_analysis_data(
        self, 
        idea_title: str, 
        idea_description: str, 
        insights: List[ResearchInsight]
    ) -> Dict[str, Any]:
        """Generate comprehensive market analysis data using AI"""
        
        # Build context from existing insights
        context = self._build_market_context(insights)
        
        prompt = f"""
        Analyze the market for this business idea and provide comprehensive market analysis data:
        
        Idea: {idea_title}
        Description: {idea_description}
        
        Existing Research Context:
        {context}
        
        Provide a detailed market analysis including:
        
        1. MARKET OVERVIEW:
        - Industry classification
        - Market category
        - Geographic scope
        - Target demographics
        
        2. MARKET SIZE (in USD):
        - TAM (Total Addressable Market)
        - SAM (Serviceable Addressable Market) 
        - SOM (Serviceable Obtainable Market)
        - CAGR (Compound Annual Growth Rate %)
        - Market maturity stage
        
        3. MARKET DYNAMICS:
        - Key market drivers (3-5 items)
        - Market barriers (3-5 items)
        - Regulatory factors
        - Technology trends
        
        4. CUSTOMER ANALYSIS:
        - Customer segments (3-4 segments)
        - Key pain points (5-7 items)
        - Buying behavior patterns
        - Price sensitivity (0.0-1.0 scale)
        
        5. COMPETITIVE LANDSCAPE:
        - Direct competitors (5-8 companies)
        - Indirect competitors (3-5 companies)
        - Market leaders
        
        6. MARKET TRENDS:
        - Emerging trends (3-5 trends)
        - Growth drivers
        - Disruptive forces
        
        7. OPPORTUNITIES:
        - Market gaps (3-4 opportunities)
        - Underserved segments
        - Growth opportunities
        
        Return as JSON with structured data. Be specific with numbers and provide realistic estimates.
        """
        
        try:
            response = self.ai_service.process_message(prompt, "market_analysis")
            
            # Parse JSON response
            if response.startswith('{'):
                return json.loads(response)
            else:
                # Extract JSON from response
                json_match = re.search(r'\{.*\}', response, re.DOTALL)
                if json_match:
                    return json.loads(json_match.group())
                    
        except Exception as e:
            logger.error(f"Error generating market analysis data: {str(e)}")
        
        # Return fallback data
        return self._generate_fallback_market_data(idea_title, idea_description)
    
    def _create_market_analysis(
        self, 
        db: Session, 
        session_id: int, 
        market_data: Dict[str, Any]
    ) -> MarketAnalysis:
        """Create market analysis record from generated data"""
        
        overview = market_data.get('market_overview', {})
        sizing = market_data.get('market_size', {})
        dynamics = market_data.get('market_dynamics', {})
        customers = market_data.get('customer_analysis', {})
        
        market_analysis = MarketAnalysis(
            session_id=session_id,
            industry=overview.get('industry'),
            market_category=overview.get('market_category'),
            geographic_scope=overview.get('geographic_scope', 'Global'),
            target_demographics=overview.get('target_demographics', {}),
            
            tam_value=sizing.get('tam'),
            sam_value=sizing.get('sam'),
            som_value=sizing.get('som'),
            market_size_year=datetime.now().year,
            cagr=sizing.get('cagr'),
            market_maturity=sizing.get('market_maturity'),
            
            market_drivers=dynamics.get('market_drivers', []),
            market_barriers=dynamics.get('market_barriers', []),
            regulatory_factors=dynamics.get('regulatory_factors', []),
            technology_trends=dynamics.get('technology_trends', []),
            
            customer_segments=customers.get('customer_segments', []),
            customer_pain_points=customers.get('customer_pain_points', []),
            buying_behavior=customers.get('buying_behavior', {}),
            price_sensitivity=customers.get('price_sensitivity', 0.5),
            
            confidence_score=0.7,
            data_sources=["AI Analysis", "Market Research"]
        )
        
        db.add(market_analysis)
        db.commit()
        db.refresh(market_analysis)
        
        return market_analysis
    
    def _generate_competitors(
        self, 
        db: Session, 
        market_analysis_id: int, 
        market_data: Dict[str, Any]
    ) -> List[CompetitorAnalysis]:
        """Generate competitor analysis records"""
        
        competitors = []
        competitive_data = market_data.get('competitive_landscape', {})
        
        # Process direct competitors
        for comp_data in competitive_data.get('direct_competitors', []):
            competitor = CompetitorAnalysis(
                market_analysis_id=market_analysis_id,
                name=comp_data.get('name', 'Unknown Competitor'),
                description=comp_data.get('description'),
                tier=CompetitorTier.DIRECT,
                market_share=comp_data.get('market_share'),
                revenue=comp_data.get('revenue'),
                strengths=comp_data.get('strengths', []),
                weaknesses=comp_data.get('weaknesses', []),
                threat_level=comp_data.get('threat_level', 0.5),
                data_completeness=0.6
            )
            db.add(competitor)
            competitors.append(competitor)
        
        # Process indirect competitors
        for comp_data in competitive_data.get('indirect_competitors', []):
            competitor = CompetitorAnalysis(
                market_analysis_id=market_analysis_id,
                name=comp_data.get('name', 'Unknown Competitor'),
                description=comp_data.get('description'),
                tier=CompetitorTier.INDIRECT,
                threat_level=comp_data.get('threat_level', 0.3),
                data_completeness=0.5
            )
            db.add(competitor)
            competitors.append(competitor)
        
        db.commit()
        
        for competitor in competitors:
            db.refresh(competitor)
        
        return competitors
    
    def _generate_market_segments(
        self, 
        db: Session, 
        market_analysis_id: int, 
        market_data: Dict[str, Any]
    ) -> List[MarketSegment]:
        """Generate market segment records"""
        
        segments = []
        customer_data = market_data.get('customer_analysis', {})
        
        for segment_data in customer_data.get('customer_segments', []):
            segment = MarketSegment(
                market_analysis_id=market_analysis_id,
                segment_name=segment_data.get('name', 'Market Segment'),
                description=segment_data.get('description'),
                size_percentage=segment_data.get('size_percentage'),
                age_range=segment_data.get('age_range'),
                income_range=segment_data.get('income_range'),
                behavior_traits=segment_data.get('behavior_traits', []),
                preferred_channels=segment_data.get('preferred_channels', []),
                decision_factors=segment_data.get('decision_factors', []),
                attractiveness_score=segment_data.get('attractiveness_score', 0.5),
                accessibility_score=segment_data.get('accessibility_score', 0.5),
                competition_intensity=segment_data.get('competition_intensity', 0.5),
                priority_level=segment_data.get('priority_level', 'Secondary')
            )
            db.add(segment)
            segments.append(segment)
        
        db.commit()
        
        for segment in segments:
            db.refresh(segment)
        
        return segments
    
    def _generate_trend_analysis(
        self, 
        db: Session, 
        session_id: int, 
        market_data: Dict[str, Any]
    ) -> List[MarketTrendAnalysis]:
        """Generate market trend analysis records"""
        
        trends = []
        trend_data = market_data.get('market_trends', {})
        
        for trend_info in trend_data.get('emerging_trends', []):
            trend = MarketTrendAnalysis(
                session_id=session_id,
                trend_name=trend_info.get('name', 'Market Trend'),
                trend_type=MarketTrend.EMERGING,
                description=trend_info.get('description'),
                impact_level=trend_info.get('impact_level', 0.5),
                time_horizon=trend_info.get('time_horizon', 'Medium-term'),
                opportunities=trend_info.get('opportunities', []),
                threats=trend_info.get('threats', []),
                confidence_level=0.6
            )
            db.add(trend)
            trends.append(trend)
        
        db.commit()
        
        for trend in trends:
            db.refresh(trend)
        
        return trends
    
    def _generate_opportunities(
        self, 
        db: Session, 
        session_id: int, 
        market_data: Dict[str, Any]
    ) -> List[MarketOpportunity]:
        """Generate market opportunity records"""
        
        opportunities = []
        opp_data = market_data.get('opportunities', {})
        
        for opp_info in opp_data.get('market_gaps', []):
            opportunity = MarketOpportunity(
                session_id=session_id,
                title=opp_info.get('title', 'Market Opportunity'),
                description=opp_info.get('description'),
                opportunity_type=opp_info.get('type', 'Gap'),
                market_size=opp_info.get('market_size'),
                attractiveness_score=opp_info.get('attractiveness_score', 0.6),
                feasibility_score=opp_info.get('feasibility_score', 0.5),
                urgency_score=opp_info.get('urgency_score', 0.5),
                required_capabilities=opp_info.get('required_capabilities', []),
                risk_factors=opp_info.get('risk_factors', []),
                recommended_actions=opp_info.get('recommended_actions', []),
                priority_level=opp_info.get('priority_level', 'Medium')
            )
            db.add(opportunity)
            opportunities.append(opportunity)
        
        db.commit()
        
        for opportunity in opportunities:
            db.refresh(opportunity)
        
        return opportunities
    
    def _calculate_tam(self, market_analysis: MarketAnalysis, geographic_scope: str) -> float:
        """Calculate Total Addressable Market"""
        base_tam = market_analysis.tam_value or 1000000000  # Default $1B
        
        # Adjust for geographic scope
        scope_multipliers = {
            "global": 1.0,
            "regional": 0.3,
            "national": 0.1,
            "local": 0.01
        }
        
        return base_tam * scope_multipliers.get(geographic_scope.lower(), 1.0)
    
    def _calculate_sam(self, tam: float, market_analysis: MarketAnalysis, target_segments: Optional[List[str]]) -> float:
        """Calculate Serviceable Addressable Market"""
        if market_analysis.sam_value:
            return market_analysis.sam_value
        
        # Estimate SAM as percentage of TAM based on targeting
        sam_percentage = 0.3  # Default 30% of TAM
        
        if target_segments:
            # Adjust based on number of target segments
            segment_factor = min(len(target_segments) * 0.15, 0.5)
            sam_percentage = segment_factor
        
        return tam * sam_percentage
    
    def _calculate_som(self, sam: float, market_analysis: MarketAnalysis) -> float:
        """Calculate Serviceable Obtainable Market"""
        if market_analysis.som_value:
            return market_analysis.som_value
        
        # Estimate SOM as percentage of SAM (realistic market share)
        som_percentage = 0.05  # Default 5% market share target
        
        # Adjust based on market maturity
        if market_analysis.market_maturity:
            maturity_adjustments = {
                "emerging": 0.1,    # Higher share possible in emerging markets
                "growth": 0.08,     # Good opportunity in growth markets
                "mature": 0.03,     # Harder to gain share in mature markets
                "declining": 0.02   # Limited opportunity in declining markets
            }
            som_percentage = maturity_adjustments.get(market_analysis.market_maturity.lower(), 0.05)
        
        return sam * som_percentage
    
    def _generate_growth_projections(self, market_analysis: MarketAnalysis) -> Dict[str, float]:
        """Generate 5-year growth projections"""
        cagr = (market_analysis.cagr or 5.0) / 100  # Default 5% CAGR
        current_year = datetime.now().year
        
        projections = {}
        base_value = market_analysis.tam_value or 1000000000
        
        for i in range(5):
            year = current_year + i
            projected_value = base_value * ((1 + cagr) ** i)
            projections[str(year)] = projected_value
        
        return projections
    
    def _calculate_competitive_intensity(self, competitors: List[CompetitorAnalysis]) -> float:
        """Calculate competitive intensity score"""
        if not competitors:
            return 0.3  # Low competition if no competitors identified
        
        # Factor in number of competitors and their threat levels
        num_competitors = len(competitors)
        avg_threat = sum(c.threat_level or 0.5 for c in competitors) / num_competitors
        
        # Scale based on number of competitors
        if num_competitors >= 10:
            competitor_factor = 1.0
        elif num_competitors >= 5:
            competitor_factor = 0.8
        else:
            competitor_factor = 0.6
        
        return min(avg_threat * competitor_factor, 1.0)
    
    def _determine_market_concentration(self, direct_competitors: List[CompetitorAnalysis]) -> str:
        """Determine market concentration level"""
        if not direct_competitors:
            return "fragmented"
        
        # Calculate market share of top competitors
        market_shares = [c.market_share or 0 for c in direct_competitors]
        market_shares.sort(reverse=True)
        
        if len(market_shares) >= 3:
            top_3_share = sum(market_shares[:3])
            if top_3_share >= 70:
                return "highly_concentrated"
            elif top_3_share >= 40:
                return "moderately_concentrated"
        
        return "fragmented"
    
    def _research_competitor_with_ai(self, competitor_name: str, research_depth: str) -> Dict[str, Any]:
        """Research competitor using AI"""
        
        prompt = f"""
        Research the competitor "{competitor_name}" and provide detailed analysis:
        
        Research depth: {research_depth}
        
        Provide information on:
        1. Company overview and description
        2. Market position and market share (if available)
        3. Revenue and financial metrics
        4. Products and services offered
        5. Target customers and markets
        6. Pricing model and strategy
        7. Key strengths and competitive advantages
        8. Weaknesses and vulnerabilities
        9. Recent funding or growth metrics
        10. Digital presence and online reputation
        
        Return as JSON with structured data. Use realistic estimates if exact data isn't available.
        """
        
        try:
            response = self.ai_service.process_message(prompt, "competitor_research")
            
            if response.startswith('{'):
                return json.loads(response)
            else:
                json_match = re.search(r'\{.*\}', response, re.DOTALL)
                if json_match:
                    return json.loads(json_match.group())
                    
        except Exception as e:
            logger.error(f"Error researching competitor with AI: {str(e)}")
        
        # Return fallback data
        return {
            "description": f"Competitor in the market space",
            "tier": "direct",
            "threat_level": 0.5,
            "data_completeness": 0.3
        }
    
    def _build_market_context(self, insights: List[ResearchInsight]) -> str:
        """Build context string from existing insights"""
        context_parts = []
        
        for insight in insights:
            if insight.category in ['target_market', 'customer_profile', 'problem_solution']:
                context_parts.append(f"{insight.category}: {insight.title} - {insight.description}")
        
        return '\n'.join(context_parts) if context_parts else "No existing market insights available"
    
    def _generate_fallback_analysis(
        self, 
        db: Session, 
        session_id: int, 
        idea_title: str, 
        idea_description: str
    ) -> ComprehensiveMarketAnalysis:
        """Generate fallback analysis when AI fails"""
        
        # Create basic market analysis
        market_analysis = MarketAnalysis(
            session_id=session_id,
            industry="Technology",
            market_category="Software/Services",
            geographic_scope="Global",
            tam_value=10000000000,  # $10B
            sam_value=1000000000,   # $1B
            som_value=50000000,     # $50M
            cagr=15.0,
            market_maturity="Growth",
            confidence_score=0.4
        )
        
        db.add(market_analysis)
        db.commit()
        db.refresh(market_analysis)
        
        return ComprehensiveMarketAnalysis(
            market_analysis=market_analysis,
            competitors=[],
            segments=[],
            trends=[],
            opportunities=[]
        )
    
    def _generate_fallback_market_data(self, idea_title: str, idea_description: str) -> Dict[str, Any]:
        """Generate fallback market data structure"""
        return {
            "market_overview": {
                "industry": "Technology",
                "market_category": "Software/Services",
                "geographic_scope": "Global",
                "target_demographics": {"primary_age": "25-45", "income_level": "Middle to High"}
            },
            "market_size": {
                "tam": 10000000000,
                "sam": 1000000000,
                "som": 50000000,
                "cagr": 15.0,
                "market_maturity": "Growth"
            },
            "market_dynamics": {
                "market_drivers": ["Digital transformation", "Increasing demand", "Technology adoption"],
                "market_barriers": ["Competition", "Regulatory requirements", "Customer acquisition costs"],
                "regulatory_factors": ["Data privacy regulations", "Industry standards"],
                "technology_trends": ["AI/ML adoption", "Cloud migration", "Mobile-first approach"]
            },
            "customer_analysis": {
                "customer_segments": [
                    {"name": "Early Adopters", "size_percentage": 15, "attractiveness_score": 0.8},
                    {"name": "Mainstream Market", "size_percentage": 65, "attractiveness_score": 0.6},
                    {"name": "Late Adopters", "size_percentage": 20, "attractiveness_score": 0.4}
                ],
                "customer_pain_points": ["Cost concerns", "Complexity", "Time constraints", "Integration challenges"],
                "price_sensitivity": 0.6
            },
            "competitive_landscape": {
                "direct_competitors": [],
                "indirect_competitors": []
            },
            "market_trends": {
                "emerging_trends": []
            },
            "opportunities": {
                "market_gaps": []
            }
        }
    
    def _generate_fallback_sizing(self) -> MarketSizingAnalysis:
        """Generate fallback market sizing"""
        return MarketSizingAnalysis(
            tam=10000000000,  # $10B
            sam=1000000000,   # $1B  
            som=50000000,     # $50M
            market_size_breakdown={
                "total_addressable_market": 10000000000,
                "serviceable_addressable_market": 1000000000,
                "serviceable_obtainable_market": 50000000,
                "penetration_rate": 5.0
            },
            growth_projections={
                "2024": 10000000000,
                "2025": 11500000000,
                "2026": 13225000000,
                "2027": 15208750000,
                "2028": 17490062500
            },
            assumptions=["Estimated market sizing", "15% CAGR assumed", "Global market scope"],
            confidence_level=0.4
        )