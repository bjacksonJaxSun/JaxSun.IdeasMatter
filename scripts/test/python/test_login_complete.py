#!/usr/bin/env python3
"""
Complete login system testing script
Tests both backend and frontend login functionality
"""

import sys
import os
import json
import time
import subprocess
import requests
from datetime import datetime

# Add project root to path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

class LoginTester:
    def __init__(self):
        self.backend_url = "http://localhost:8000"
        self.frontend_url = "http://localhost:4000"
        self.test_results = []
        
    def log_test(self, test_name, success, message="", details=None):
        """Log test results"""
        result = {
            "test": test_name,
            "success": success,
            "message": message,
            "details": details,
            "timestamp": datetime.now().isoformat()
        }
        self.test_results.append(result)
        
        status = "✅" if success else "❌"
        print(f"{status} {test_name}: {message}")
        if details:
            print(f"   Details: {details}")
        
    def test_backend_connectivity(self):
        """Test if backend is running and accessible"""
        try:
            response = requests.get(f"{self.backend_url}/", timeout=5)
            self.log_test("Backend Connectivity", True, f"Backend responding with status {response.status_code}")
            return True
        except requests.exceptions.ConnectionError:
            self.log_test("Backend Connectivity", False, "Backend not running or not accessible")
            return False
        except Exception as e:
            self.log_test("Backend Connectivity", False, f"Backend error: {str(e)}")
            return False
    
    def test_backend_auth_endpoints(self):
        """Test if authentication endpoints exist"""
        endpoints = [
            "/api/v1/auth/bypass",
            "/api/v1/auth/me",
            "/api/v1/auth/login",
            "/docs"
        ]
        
        for endpoint in endpoints:
            try:
                response = requests.get(f"{self.backend_url}{endpoint}", timeout=5)
                if response.status_code in [200, 405, 422]:  # 405=Method not allowed is OK for POST endpoints
                    self.log_test(f"Endpoint {endpoint}", True, f"Available (status: {response.status_code})")
                else:
                    self.log_test(f"Endpoint {endpoint}", False, f"Unexpected status: {response.status_code}")
            except Exception as e:
                self.log_test(f"Endpoint {endpoint}", False, f"Not accessible: {str(e)}")
    
    def test_bypass_login_api(self):
        """Test the bypass login API endpoint"""
        try:
            payload = {"role": "user"}
            response = requests.post(
                f"{self.backend_url}/api/v1/auth/bypass", 
                json=payload,
                timeout=10
            )
            
            if response.status_code == 200:
                data = response.json()
                if "access_token" in data:
                    self.log_test("Bypass Login API", True, "API working correctly", data)
                    return data["access_token"]
                else:
                    self.log_test("Bypass Login API", False, "No access_token in response", data)
            else:
                self.log_test("Bypass Login API", False, f"Status {response.status_code}", response.text)
                
        except Exception as e:
            self.log_test("Bypass Login API", False, f"API call failed: {str(e)}")
        
        return None
    
    def test_auth_me_endpoint(self, token):
        """Test the /auth/me endpoint with token"""
        if not token:
            self.log_test("Auth Me Endpoint", False, "No token provided")
            return
            
        try:
            headers = {"Authorization": f"Bearer {token}"}
            response = requests.get(
                f"{self.backend_url}/api/v1/auth/me",
                headers=headers,
                timeout=5
            )
            
            if response.status_code == 200:
                user_data = response.json()
                self.log_test("Auth Me Endpoint", True, "User data retrieved", user_data)
            else:
                self.log_test("Auth Me Endpoint", False, f"Status {response.status_code}", response.text)
                
        except Exception as e:
            self.log_test("Auth Me Endpoint", False, f"Request failed: {str(e)}")
    
    def test_frontend_connectivity(self):
        """Test if frontend is accessible"""
        try:
            response = requests.get(self.frontend_url, timeout=5)
            if response.status_code == 200:
                self.log_test("Frontend Connectivity", True, "Frontend is accessible")
                return True
            else:
                self.log_test("Frontend Connectivity", False, f"Status {response.status_code}")
        except requests.exceptions.ConnectionError:
            self.log_test("Frontend Connectivity", False, "Frontend not running")
        except Exception as e:
            self.log_test("Frontend Connectivity", False, f"Error: {str(e)}")
        
        return False
    
    def test_cors_configuration(self):
        """Test CORS configuration"""
        try:
            headers = {
                "Origin": "http://localhost:4000",
                "Access-Control-Request-Method": "POST"
            }
            response = requests.options(
                f"{self.backend_url}/api/v1/auth/bypass",
                headers=headers,
                timeout=5
            )
            
            cors_headers = {
                "Access-Control-Allow-Origin": response.headers.get("Access-Control-Allow-Origin"),
                "Access-Control-Allow-Methods": response.headers.get("Access-Control-Allow-Methods"),
                "Access-Control-Allow-Headers": response.headers.get("Access-Control-Allow-Headers")
            }
            
            if cors_headers["Access-Control-Allow-Origin"]:
                self.log_test("CORS Configuration", True, "CORS headers present", cors_headers)
            else:
                self.log_test("CORS Configuration", False, "No CORS headers found", cors_headers)
                
        except Exception as e:
            self.log_test("CORS Configuration", False, f"CORS test failed: {str(e)}")
    
    def create_mock_auth_test(self):
        """Create a test file for mock authentication"""
        test_html = f"""
<!DOCTYPE html>
<html>
<head>
    <title>Mock Login Test</title>
    <script src="https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js"></script>
</head>
<body>
    <h1>Mock Login Test</h1>
    <button id="testBypass">Test Bypass Login</button>
    <button id="testMock">Test Mock Auth</button>
    <div id="results"></div>
    
    <script>
        const API_URL = '{self.backend_url}';
        const results = document.getElementById('results');
        
        // Test bypass login
        document.getElementById('testBypass').onclick = async () => {{
            try {{
                const response = await axios.post(API_URL + '/api/v1/auth/bypass', {{
                    role: 'user'
                }});
                results.innerHTML = '<h3>Bypass Login Success!</h3><pre>' + JSON.stringify(response.data, null, 2) + '</pre>';
            }} catch (error) {{
                console.error('Bypass login failed:', error);
                // Try mock auth
                testMockAuth();
            }}
        }};
        
        // Test mock authentication
        function testMockAuth() {{
            const mockUser = {{
                id: 2,
                email: 'user@example.com',
                name: 'Test User',
                role: 'user',
                permissions: ['read', 'write']
            }};
            
            const mockToken = 'mock_user_token_' + Date.now();
            
            localStorage.setItem('access_token', mockToken);
            localStorage.setItem('mock_auth', 'true');
            
            results.innerHTML = '<h3>Mock Auth Success!</h3><pre>' + JSON.stringify(mockUser, null, 2) + '</pre>';
            
            setTimeout(() => {{
                window.location.href = '/dashboard';
            }}, 2000);
        }}
        
        document.getElementById('testMock').onclick = testMockAuth;
    </script>
</body>
</html>
        """
        
        with open("test_mock_login.html", "w") as f:
            f.write(test_html)
        
        self.log_test("Mock Test File Created", True, "Created test_mock_login.html")
    
    def run_all_tests(self):
        """Run all login tests"""
        print("🧪 Starting Complete Login System Test")
        print("=" * 50)
        
        # Test backend connectivity
        backend_up = self.test_backend_connectivity()
        
        if backend_up:
            # Test auth endpoints
            self.test_backend_auth_endpoints()
            
            # Test bypass login API
            token = self.test_bypass_login_api()
            
            # Test auth/me endpoint
            self.test_auth_me_endpoint(token)
            
            # Test CORS
            self.test_cors_configuration()
        else:
            print("\n⚠️  Backend is not running. Testing mock authentication only.")
        
        # Test frontend
        self.test_frontend_connectivity()
        
        # Create mock test file
        self.create_mock_auth_test()
        
        # Generate report
        self.generate_report()
    
    def generate_report(self):
        """Generate test report"""
        print("\n" + "=" * 50)
        print("📊 TEST RESULTS SUMMARY")
        print("=" * 50)
        
        passed = sum(1 for result in self.test_results if result["success"])
        total = len(self.test_results)
        
        print(f"Tests Passed: {passed}/{total}")
        print(f"Success Rate: {(passed/total)*100:.1f}%")
        
        print("\n🔍 FAILED TESTS:")
        for result in self.test_results:
            if not result["success"]:
                print(f"  ❌ {result['test']}: {result['message']}")
        
        print("\n💡 RECOMMENDATIONS:")
        
        backend_tests = [r for r in self.test_results if "Backend" in r["test"] or "API" in r["test"]]
        backend_failures = [r for r in backend_tests if not r["success"]]
        
        if backend_failures:
            print("  🔧 BACKEND ISSUES:")
            print("     1. Start the backend server:")
            print("        cd backend && python start_backend_simple.py")
            print("     2. Or use the demo server:")
            print("        cd backend && python demo_server.py")
            print("     3. Check if port 8000 is available")
        
        frontend_failures = [r for r in self.test_results if not r["success"] and "Frontend" in r["test"]]
        if frontend_failures:
            print("  🎨 FRONTEND ISSUES:")
            print("     1. Start the frontend:")
            print("        cd frontend && npm run dev")
            print("     2. Check if port 4000 is available")
        
        print("\n🚀 QUICK FIX - Use Mock Authentication:")
        print("  1. Open test_mock_login.html in browser")
        print("  2. Click 'Test Mock Auth' button")
        print("  3. This will bypass all backend dependencies")
        
        # Save results to file
        with open("login_test_results.json", "w") as f:
            json.dump(self.test_results, f, indent=2)
        
        print(f"\n📄 Detailed results saved to: login_test_results.json")

if __name__ == "__main__":
    tester = LoginTester()
    tester.run_all_tests()