# Market Analysis Feature Troubleshooting Guide

## Error: "Failed to Generate Market Analysis"

### Quick Fixes

1. **Try Demo Data First**: Click the "View Demo Analysis" button to see the feature working with sample data
2. **Check Browser Console**: Open Developer Tools (F12) and look at the Console tab for detailed error messages
3. **Verify Backend Status**: Make sure the backend server is running on `http://localhost:8000`

### Detailed Troubleshooting Steps

#### Step 1: Check Backend Server

1. **Start Backend Server**:
   ```bash
   cd backend
   python main.py
   # OR
   uvicorn main:app --host 0.0.0.0 --port 8000 --reload
   ```

2. **Verify API Endpoints**:
   - Open browser to `http://localhost:8000/docs`
   - Look for `/api/v1/market-analysis/` endpoints
   - If missing, check if the market analysis router is properly included

#### Step 2: Check Authentication

1. **Verify Login Status**:
   - Make sure you're logged into the application
   - Check localStorage for `access_token`
   - Try logging out and back in

2. **Test Authentication**:
   ```javascript
   // In browser console
   console.log(localStorage.getItem('access_token'));
   ```

#### Step 3: Check Database Tables

1. **Create Market Analysis Tables**:
   ```bash
   cd backend
   python create_market_analysis_tables.py
   ```

2. **Verify Tables Exist**:
   ```sql
   -- Check if tables exist in SQLite
   .tables
   -- Should show: market_analyses, competitor_analyses, market_segments, etc.
   ```

#### Step 4: Check Dependencies

1. **Backend Dependencies**:
   ```bash
   cd backend
   pip install -r requirements.txt
   ```

2. **Common Missing Dependencies**:
   - `fastapi`
   - `sqlalchemy`
   - `pydantic`
   - `uvicorn`

#### Step 5: Check API Integration

1. **Test API Manually**:
   ```bash
   curl -X POST "http://localhost:8000/api/v1/market-analysis/generate" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"session_id": 1, "analysis_type": "comprehensive"}'
   ```

### Common Error Messages and Solutions

#### "Cannot connect to server"
- **Cause**: Backend server not running
- **Solution**: Start the backend server on port 8000

#### "Authentication required"
- **Cause**: Not logged in or token expired
- **Solution**: Log out and log back in

#### "Research session not found"
- **Cause**: Invalid session ID or session doesn't belong to user
- **Solution**: Verify the correct research session is selected

#### "Server error" (500)
- **Cause**: Backend error, possibly missing dependencies or database issues
- **Solution**: Check backend logs and ensure all dependencies are installed

#### "Endpoint not available" (404)
- **Cause**: Market analysis API endpoints not registered
- **Solution**: Verify the market analysis router is included in the main API router

### Backend Code Verification

#### Check API Router Registration
File: `/backend/app/api/v1/__init__.py`
```python
from app.api.v1 import research, market_analysis

api_router.include_router(market_analysis.router, prefix="/market-analysis", tags=["market-analysis"])
```

#### Check Database Models
File: `/backend/app/models/market_analysis.py`
- Verify all tables are defined correctly
- Check for import errors

#### Check Service Dependencies
File: `/backend/app/services/market_analysis_service.py`
- Verify SimpleAIOrchestrator import
- Check that all methods exist

### Demo Data Features

When backend issues prevent real API calls, the frontend will automatically:

1. **Fallback to Demo Data**: Provides realistic sample market analysis
2. **Detailed Error Messages**: Console logs show exact error details
3. **Manual Demo Trigger**: "View Demo Analysis" button for testing

### Demo Data Includes:

- **Market Sizing**: TAM ($10.5B), SAM ($2.1B), SOM ($105M)
- **Competitor Analysis**: 3 sample competitors with detailed profiles
- **Market Segments**: Enterprise, SMB, and Startup segments
- **Market Drivers & Barriers**: Realistic industry factors
- **Interactive Visualizations**: All tabs working with sample data

### Contact Information

If you continue experiencing issues:

1. **Check Browser Console**: Look for detailed error messages
2. **Check Backend Logs**: Review server logs for Python errors
3. **Try Demo Mode**: Use "View Demo Analysis" to verify frontend functionality
4. **Backend Dependencies**: Ensure all Python packages are installed

### File Locations

- **Frontend Component**: `/frontend/src/components/MarketAnalysis.tsx`
- **Backend API**: `/backend/app/api/v1/market_analysis.py`
- **Database Models**: `/backend/app/models/market_analysis.py`
- **Service Logic**: `/backend/app/services/market_analysis_service.py`
- **Schemas**: `/backend/app/schemas/market_analysis.py`

### Success Indicators

✅ Backend server running on port 8000  
✅ Market analysis API endpoints visible in `/docs`  
✅ User authenticated (token in localStorage)  
✅ Market analysis tables exist in database  
✅ Demo data loads successfully  
✅ Console shows successful API calls or informative error messages