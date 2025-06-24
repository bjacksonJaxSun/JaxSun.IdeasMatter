#!/usr/bin/env python3
"""
Create market analysis database tables
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from sqlalchemy import create_engine, text
from app.core.config import get_settings
from app.models.market_analysis import MarketAnalysis, CompetitorAnalysis, MarketSegment, MarketTrendAnalysis, MarketOpportunity
from app.core.database import Base
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def create_market_analysis_tables():
    """Create market analysis tables if they don't exist"""
    settings = get_settings()
    
    # Create engine
    engine = create_engine(settings.database_url)
    
    try:
        # Create all tables defined in market_analysis models
        logger.info("Creating market analysis tables...")
        
        # Create the tables
        MarketAnalysis.metadata.create_all(bind=engine, checkfirst=True)
        CompetitorAnalysis.metadata.create_all(bind=engine, checkfirst=True)
        MarketSegment.metadata.create_all(bind=engine, checkfirst=True)
        MarketTrendAnalysis.metadata.create_all(bind=engine, checkfirst=True)
        MarketOpportunity.metadata.create_all(bind=engine, checkfirst=True)
        
        logger.info("Market analysis tables created successfully!")
        
        # Verify tables were created
        with engine.connect() as conn:
            result = conn.execute(text("SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '%market%' OR name LIKE '%competitor%'"))
            tables = [row[0] for row in result.fetchall()]
            logger.info(f"Created tables: {tables}")
        
    except Exception as e:
        logger.error(f"Error creating market analysis tables: {str(e)}")
        raise
    finally:
        engine.dispose()

if __name__ == "__main__":
    create_market_analysis_tables()
    print("Market analysis tables setup complete!")