import { test, expect } from '@playwright/test';

test.describe('Research Progress Tracking E2E Test', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('http://localhost:4000');
    
    // Handle authentication if needed
    const bypassButton = page.locator('button:has-text("Continue as Guest")').first();
    if (await bypassButton.isVisible({ timeout: 5000 })) {
      await bypassButton.click();
    }
  });

  test('Cat and Mouse Game - Progress Tracking Flow', async ({ page }) => {
    console.log('Starting Cat and Mouse Game progress tracking test...');
    
    // Step 1: Navigate to research page or create idea
    // Look for existing Cat and Mouse Game card or create new
    const catMouseCard = page.locator('text="Cat and Mouse Game"').first();
    
    if (await catMouseCard.isVisible({ timeout: 5000 })) {
      console.log('Found existing Cat and Mouse Game idea');
      await catMouseCard.click();
    } else {
      console.log('Creating new Cat and Mouse Game idea');
      // Create new idea if not found
      await page.locator('button:has-text("New Idea")').click();
      await page.fill('input[name="title"]', 'Cat and Mouse Game');
      await page.fill('textarea[name="description"]', 'This is a game that 4 - 10 year olds can play and the user choses their favorite cat and mouse. The game allows the user to learn how strategy works by being either the cat or mouse.');
      await page.locator('button:has-text("Save")').click();
    }
    
    // Step 2: Start research analysis
    console.log('Looking for research/analysis buttons...');
    
    // Wait for page to load and look for analysis options
    await page.waitForTimeout(2000);
    
    // Check for different possible button texts
    const analysisButtons = [
      'button:has-text("Analyze")',
      'button:has-text("Start Analysis")',
      'button:has-text("Start Research")',
      'button:has-text("Start AI Analysis")',
      'button:has-text("Start Market Deep-Dive")'
    ];
    
    let analysisStarted = false;
    
    for (const selector of analysisButtons) {
      const button = page.locator(selector).first();
      if (await button.isVisible({ timeout: 2000 })) {
        console.log(`Found button: ${selector}`);
        await button.click();
        analysisStarted = true;
        break;
      }
    }
    
    if (!analysisStarted) {
      throw new Error('Could not find any analysis start button');
    }
    
    // Step 3: Monitor for progress tracking
    console.log('Monitoring for progress tracking...');
    
    // Wait for either progress or error
    const progressIndicators = [
      'text="AI Research in Progress"',
      'text="Analysis in Progress"',
      'text="Progress:"',
      'text="%"', // Progress percentage
      '[data-testid="progress-tracker"]',
      '.progress-bar',
      'text="Market Context Analysis"',
      'text="Competitive Intelligence"',
      'text="Strategic Assessment"'
    ];
    
    const errorIndicators = [
      'text="Failed to track progress"',
      'text="Analysis Error"',
      'text="Error:"',
      '[data-testid="error-message"]',
      '.error-message'
    ];
    
    // Check for errors first
    let errorFound = false;
    for (const selector of errorIndicators) {
      const errorElement = page.locator(selector).first();
      if (await errorElement.isVisible({ timeout: 5000 })) {
        console.error(`ERROR FOUND: ${selector}`);
        errorFound = true;
        
        // Capture error details
        const errorText = await errorElement.textContent();
        console.error(`Error text: ${errorText}`);
        
        // Take screenshot of error
        await page.screenshot({ 
          path: 'research-progress-error.png',
          fullPage: true 
        });
        
        // Check network tab for failed requests
        page.on('response', response => {
          if (response.status() >= 400) {
            console.error(`Failed request: ${response.url()} - Status: ${response.status()}`);
          }
        });
        
        break;
      }
    }
    
    if (errorFound) {
      // Capture console logs
      page.on('console', msg => {
        if (msg.type() === 'error') {
          console.error('Browser console error:', msg.text());
        }
      });
      
      // Wait a bit to capture any additional errors
      await page.waitForTimeout(2000);
      
      // Try to get more error details
      const tryAgainButton = page.locator('button:has-text("Try Again")').first();
      if (await tryAgainButton.isVisible()) {
        console.log('Found "Try Again" button - error confirmed');
      }
      
      throw new Error('Progress tracking failed - "Failed to track progress" error displayed');
    }
    
    // Check for progress indicators
    let progressFound = false;
    for (const selector of progressIndicators) {
      const progressElement = page.locator(selector).first();
      if (await progressElement.isVisible({ timeout: 5000 })) {
        console.log(`Progress indicator found: ${selector}`);
        progressFound = true;
        
        // Monitor progress updates
        for (let i = 0; i < 5; i++) {
          await page.waitForTimeout(2000);
          
          // Check for percentage updates
          const percentageElements = await page.locator('text="%"').all();
          for (const elem of percentageElements) {
            const text = await elem.textContent();
            console.log(`Progress update: ${text}`);
          }
          
          // Check for phase updates
          const phaseElements = await page.locator('[data-testid*="phase"]').all();
          for (const elem of phaseElements) {
            const text = await elem.textContent();
            console.log(`Phase update: ${text}`);
          }
        }
        
        break;
      }
    }
    
    if (!progressFound) {
      await page.screenshot({ 
        path: 'no-progress-indicators.png',
        fullPage: true 
      });
      throw new Error('No progress tracking indicators found');
    }
    
    // Verify progress tracking is working
    expect(progressFound).toBe(true);
    expect(errorFound).toBe(false);
    
    console.log('Progress tracking test completed successfully!');
  });

  test('API Progress Endpoint Direct Test', async ({ page, request }) => {
    console.log('Testing progress API endpoint directly...');
    
    // First, get authentication token
    let authToken = '';
    
    try {
      // Try bypass auth
      const authResponse = await request.post('http://localhost:8000/api/v1/auth/bypass', {
        data: { role: 'user' }
      });
      
      if (authResponse.ok()) {
        const authData = await authResponse.json();
        authToken = authData.access_token;
        console.log('Got auth token via bypass');
      }
    } catch (error) {
      console.error('Auth error:', error);
    }
    
    // Create a test strategy
    const strategyResponse = await request.post('http://localhost:8000/api/v1/research-strategy/initiate', {
      headers: authToken ? { 'Authorization': `Bearer ${authToken}` } : {},
      data: {
        session_id: 1,
        approach: 'quick_validation',
        custom_parameters: {
          idea_title: 'Cat and Mouse Game',
          idea_description: 'This is a game that 4 - 10 year olds can play'
        }
      }
    });
    
    console.log('Strategy initiation response:', strategyResponse.status());
    
    if (!strategyResponse.ok()) {
      const errorText = await strategyResponse.text();
      console.error('Strategy initiation failed:', errorText);
      throw new Error(`Failed to initiate strategy: ${strategyResponse.status()}`);
    }
    
    const strategyData = await strategyResponse.json();
    const strategyId = strategyData.strategy.id;
    console.log('Strategy created with ID:', strategyId);
    
    // Execute the strategy
    const execResponse = await request.post(`http://localhost:8000/api/v1/research-strategy/execute/${strategyId}`, {
      headers: authToken ? { 'Authorization': `Bearer ${authToken}` } : {}
    });
    
    console.log('Execute response:', execResponse.status());
    
    // Test progress endpoint multiple times
    for (let i = 0; i < 5; i++) {
      await page.waitForTimeout(1000);
      
      const progressResponse = await request.get(`http://localhost:8000/api/v1/research-strategy/progress/${strategyId}`, {
        headers: authToken ? { 'Authorization': `Bearer ${authToken}` } : {}
      });
      
      console.log(`Progress check ${i + 1}: Status ${progressResponse.status()}`);
      
      if (progressResponse.ok()) {
        const progressData = await progressResponse.json();
        console.log('Progress data:', JSON.stringify(progressData, null, 2));
      } else {
        const errorText = await progressResponse.text();
        console.error('Progress error:', errorText);
        throw new Error(`Progress tracking failed: ${progressResponse.status()} - ${errorText}`);
      }
    }
  });
});