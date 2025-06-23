from typing import List, Dict, Any, Optional
from pydantic import BaseModel, Field
import json
import asyncio
from datetime import datetime
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

try:
    from langchain.prompts import ChatPromptTemplate, SystemMessagePromptTemplate, HumanMessagePromptTemplate
    from langchain.output_parsers import PydanticOutputParser, OutputFixingParser
    from langchain.chat_models import ChatOpenAI
    from langchain.schema import BaseChatModel
    LANGCHAIN_AVAILABLE = True
except ImportError:
    LANGCHAIN_AVAILABLE = False
    # Fallback for when LangChain is not available
    class BaseChatModel:
        pass

from app.services.ai_providers import ai_provider_manager
from app.models.ai_provider import AIProvider as AIProviderModel
from app.core.config import get_settings

settings = get_settings()

# Response models for structured output
class InsightModel(BaseModel):
    category: str = Field(description="One of: target_market, customer_profile, problem_solution, growth_targets, cost_model, revenue_model")
    title: str = Field(description="Brief title for the insight")
    description: str = Field(description="Detailed description of the insight")
    confidence_score: float = Field(description="Confidence score between 0 and 1")
    subcategory: Optional[str] = Field(default=None, description="Optional subcategory")

class OptionModel(BaseModel):
    category: str = Field(description="Category of the option (e.g., business_model, pricing_strategy)")
    title: str = Field(description="Brief title for the option")
    description: str = Field(description="Detailed description of the option")
    pros: List[str] = Field(description="List of advantages")
    cons: List[str] = Field(description="List of disadvantages")
    feasibility_score: float = Field(description="How feasible is this option (0-1)")
    impact_score: float = Field(description="Potential impact score (0-1)")
    risk_score: float = Field(description="Risk level (0-1, where 1 is highest risk)")

class BrainstormResponseModel(BaseModel):
    response: str = Field(description="Conversational response to the user")
    insights: List[InsightModel] = Field(description="List of categorized insights")
    options: List[OptionModel] = Field(description="List of strategic options")
    follow_up_questions: List[str] = Field(description="Relevant follow-up questions")

class FactCheckResponseModel(BaseModel):
    verification_status: str = Field(description="One of: verified, disputed, unverified")
    confidence_level: str = Field(description="One of: high, medium, low")
    sources: List[str] = Field(description="List of verification sources")
    notes: str = Field(description="Explanation of the verification")

class AIOrchestrator:
    def __init__(self, db: Optional[AsyncSession] = None):
        self.db = db
        self._llm: Optional[BaseChatModel] = None
    
    async def _get_llm(self) -> Optional[BaseChatModel]:
        """Get the configured LLM instance"""
        if not LANGCHAIN_AVAILABLE:
            return None
            
        if self._llm is None:
            # Try to load from database if available
            if self.db:
                result = await self.db.execute(
                    select(AIProviderModel)
                    .where(AIProviderModel.enabled == True)
                    .where(AIProviderModel.status == "active")
                    .order_by(AIProviderModel.created_at.desc())
                )
                provider = result.scalar_one_or_none()
                
                if provider:
                    # Decrypt API key and create LLM
                    api_key = ai_provider_manager.decrypt_api_key(provider.config.get('api_key', ''))
                    
                    if provider.type == 'openai':
                        self._llm = ChatOpenAI(
                            api_key=api_key,
                            model=provider.config.get('model', 'gpt-4-turbo-preview'),
                            temperature=provider.config.get('temperature', 0.7)
                        )
            
            # Fallback to environment variables
            if self._llm is None:
                if settings.OPENAI_API_KEY:
                    self._llm = ChatOpenAI(
                        api_key=settings.OPENAI_API_KEY,
                        model="gpt-4-turbo-preview",
                        temperature=0.7
                    )
                elif settings.CLAUDE_API_KEY:
                    self._llm = ChatAnthropic(
                        api_key=settings.CLAUDE_API_KEY,
                        model="claude-3-opus-20240229",
                        temperature=0.7
                    )
                elif settings.GEMINI_API_KEY:
                    self._llm = ChatGoogleGenerativeAI(
                        google_api_key=settings.GEMINI_API_KEY,
                        model="gemini-pro",
                        temperature=0.7
                    )
                else:
                    raise ValueError("No AI provider configured. Please set up an AI provider in the admin panel or environment variables.")
        
        return self._llm
    
    async def brainstorm(self, message: str, context: Dict[str, Any] = None, insights: List[Dict] = None) -> Dict[str, Any]:
        """Generate brainstorming response with insights and options"""
        llm = await self._get_llm()
        
        # Create parser for structured output
        parser = PydanticOutputParser(pydantic_object=BrainstormResponseModel)
        
        # Create prompt template
        system_template = """You are an AI business advisor helping to brainstorm and develop ideas. 
        You provide actionable insights categorized by: target_market, customer_profile, problem_solution, 
        growth_targets, cost_model, revenue_model. You also suggest strategic options with pros/cons analysis.
        
        Previous insights context:
        {insights_context}
        
        {format_instructions}
        """
        
        human_template = """User message: {message}
        
        Additional context: {context}
        
        Please provide a helpful response with categorized insights, strategic options, and follow-up questions."""
        
        prompt = ChatPromptTemplate.from_messages([
            SystemMessagePromptTemplate.from_template(system_template),
            HumanMessagePromptTemplate.from_template(human_template)
        ])
        
        # Format the prompt
        formatted_prompt = prompt.format_messages(
            insights_context=json.dumps(insights or []),
            format_instructions=parser.get_format_instructions(),
            message=message,
            context=json.dumps(context or {})
        )
        
        try:
            # Get response from LLM
            response = await llm.ainvoke(formatted_prompt)
            
            # Parse the response
            parsed_response = parser.parse(response.content)
            
            return parsed_response.dict()
        except Exception as e:
            # Fallback response if parsing fails
            return {
                "response": f"I'm here to help you brainstorm your idea. {message}",
                "insights": [],
                "options": [],
                "follow_up_questions": [
                    "What specific problem are you trying to solve?",
                    "Who is your target audience?",
                    "What makes your idea unique?"
                ]
            }
    
    async def categorize_insights(self, idea: str, context: Dict[str, Any] = None) -> Dict[str, List[str]]:
        """Categorize business insights automatically"""
        llm = await self._get_llm()
        
        prompt = ChatPromptTemplate.from_messages([
            ("system", "You are an expert at categorizing business insights. Analyze the idea and categorize insights into: target_market, customer_profile, problem_solution, growth_targets, cost_model, revenue_model."),
            ("human", "Idea: {idea}\nContext: {context}\n\nProvide categorized insights as JSON.")
        ])
        
        response = await llm.ainvoke(
            prompt.format_messages(idea=idea, context=json.dumps(context or {}))
        )
        
        try:
            return json.loads(response.content)
        except:
            return {"categories": {}}
    
    async def perform_analysis(self, analysis_type: str, idea: str, insights: List[Dict] = None, parameters: Dict = None) -> Dict[str, Any]:
        """Perform specific type of business analysis"""
        llm = await self._get_llm()
        
        analysis_prompts = {
            "market_analysis": "Analyze the market potential, competition, and opportunities for this idea.",
            "competitive_analysis": "Identify competitors, their strengths/weaknesses, and positioning strategies.",
            "financial_modeling": "Create financial projections including revenue, costs, and profitability.",
            "risk_assessment": "Identify potential risks, challenges, and mitigation strategies."
        }
        
        prompt = ChatPromptTemplate.from_messages([
            ("system", f"You are a business analyst. {analysis_prompts.get(analysis_type, 'Perform analysis.')}"),
            ("human", "Idea: {idea}\nInsights: {insights}\nParameters: {parameters}\n\nProvide detailed analysis.")
        ])
        
        response = await llm.ainvoke(
            prompt.format_messages(
                idea=idea,
                insights=json.dumps(insights or []),
                parameters=json.dumps(parameters or {})
            )
        )
        
        return {
            "analysis": response.content,
            "analysis_type": analysis_type,
            "timestamp": datetime.utcnow().isoformat()
        }
    
    async def fact_check(self, claim: str, context: Dict[str, Any] = None) -> Dict[str, Any]:
        """Fact-check a specific claim"""
        llm = await self._get_llm()
        
        parser = PydanticOutputParser(pydantic_object=FactCheckResponseModel)
        
        prompt = ChatPromptTemplate.from_messages([
            ("system", "You are a fact-checker. Verify claims and provide verification status with sources.\n\n{format_instructions}"),
            ("human", "Claim: {claim}\nContext: {context}\n\nVerify this claim.")
        ])
        
        response = await llm.ainvoke(
            prompt.format_messages(
                format_instructions=parser.get_format_instructions(),
                claim=claim,
                context=json.dumps(context or {})
            )
        )
        
        try:
            parsed = parser.parse(response.content)
            return parsed.dict()
        except:
            return {
                "verification_status": "unverified",
                "confidence_level": "low",
                "sources": [],
                "notes": "Unable to verify at this time"
            }
    
    async def generate_options(self, category: str, context: Dict[str, Any] = None) -> List[Dict[str, Any]]:
        """Generate strategic options for a specific category"""
        llm = await self._get_llm()
        
        parser = PydanticOutputParser(pydantic_object=OptionModel)
        
        prompt = ChatPromptTemplate.from_messages([
            ("system", f"Generate strategic {category} options with pros/cons analysis.\n\n{{format_instructions}}"),
            ("human", "Context: {context}\n\nGenerate 3 strategic options.")
        ])
        
        response = await llm.ainvoke(
            prompt.format_messages(
                format_instructions=parser.get_format_instructions(),
                context=json.dumps(context or {})
            )
        )
        
        # Parse multiple options from response
        options = []
        try:
            # Try to parse as list first
            content = response.content
            if content.startswith('['):
                options_data = json.loads(content)
                for opt_data in options_data:
                    options.append(OptionModel(**opt_data).dict())
            else:
                # Single option
                option = parser.parse(content)
                options.append(option.dict())
        except:
            pass
        
        return options
    
    async def recommend_next_steps(self, session_data: Dict[str, Any]) -> List[str]:
        """Recommend next steps based on current session data"""
        llm = await self._get_llm()
        
        prompt = ChatPromptTemplate.from_messages([
            ("system", "You are a business advisor. Based on the current progress, recommend the next 3-5 actionable steps."),
            ("human", "Session data: {session_data}\n\nWhat should be the next steps?")
        ])
        
        response = await llm.ainvoke(
            prompt.format_messages(session_data=json.dumps(session_data))
        )
        
        # Extract recommendations from response
        recommendations = []
        for line in response.content.split('\n'):
            line = line.strip()
            if line and (line[0].isdigit() or line.startswith('-') or line.startswith('•')):
                # Clean up the line
                clean_line = line.lstrip('0123456789.-•) ').strip()
                if clean_line:
                    recommendations.append(clean_line)
        
        return recommendations[:5]  # Return top 5 recommendations

# Global orchestrator instance (without DB)
ai_orchestrator = AIOrchestrator()