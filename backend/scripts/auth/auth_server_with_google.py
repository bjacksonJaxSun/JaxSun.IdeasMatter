#!/usr/bin/env python3
"""
Enhanced authentication server with Google OAuth support
No external dependencies except built-in Python modules
"""

import http.server
import socketserver
import json
import urllib.parse
from datetime import datetime, timedelta
import secrets
import base64

class AuthHandler(http.server.BaseHTTPRequestHandler):
    def do_OPTIONS(self):
        """Handle CORS preflight requests"""
        self.send_response(200)
        self.send_cors_headers()
        self.end_headers()
    
    def send_cors_headers(self):
        """Send CORS headers"""
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        self.send_header('Access-Control-Max-Age', '86400')
    
    def send_json_response(self, status_code, data):
        """Send JSON response with CORS headers"""
        self.send_response(status_code)
        self.send_header('Content-Type', 'application/json')
        self.send_cors_headers()
        self.end_headers()
        self.wfile.write(json.dumps(data, indent=2).encode())
    
    def do_GET(self):
        """Handle GET requests"""
        path = self.path
        
        if path == '/':
            self.send_json_response(200, {
                "message": "Jackson.Ideas Auth Server",
                "status": "running",
                "endpoints": [
                    "/api/v1/auth/bypass",
                    "/api/v1/auth/google",
                    "/api/v1/auth/me",
                    "/api/v1/auth/login"
                ]
            })
        
        elif path == '/health':
            self.send_json_response(200, {
                "status": "healthy",
                "service": "Ideas Matter",
                "environment": "development"
            })
        
        elif path == '/api/v1/auth/me':
            # Extract token from Authorization header
            auth_header = self.headers.get('Authorization', '')
            if auth_header.startswith('Bearer '):
                token = auth_header[7:]
                
                # Mock user data based on token
                if 'admin' in token:
                    user_data = {
                        "id": 1,
                        "email": "admin@example.com",
                        "name": "Admin User",
                        "role": "admin",
                        "permissions": ["read", "write", "delete", "admin"],
                        "tenantId": "test-tenant"
                    }
                elif 'google' in token:
                    user_data = {
                        "id": 3,
                        "email": "googleuser@gmail.com",
                        "name": "Google User",
                        "picture": "https://lh3.googleusercontent.com/a/default-user",
                        "role": "user",
                        "permissions": ["read", "write"],
                        "tenantId": "test-tenant"
                    }
                else:
                    user_data = {
                        "id": 2,
                        "email": "user@example.com", 
                        "name": "Test User",
                        "role": "user",
                        "permissions": ["read", "write"],
                        "tenantId": "test-tenant"
                    }
                
                self.send_json_response(200, user_data)
            else:
                self.send_json_response(401, {"detail": "Authorization header required"})
        
        else:
            self.send_json_response(404, {"detail": "Endpoint not found"})
    
    def do_POST(self):
        """Handle POST requests"""
        path = self.path
        
        # Read request body
        content_length = int(self.headers.get('Content-Length', 0))
        if content_length > 0:
            body = self.rfile.read(content_length)
            try:
                data = json.loads(body.decode())
            except:
                data = {}
        else:
            data = {}
        
        if path == '/api/v1/auth/google':
            # Google OAuth endpoint
            credential = data.get('credential', '')
            
            if credential:
                # In a real implementation, we would verify the Google token
                # For testing, we'll accept any credential
                print(f"[Google Auth] Received credential: {credential[:50]}...")
                
                # Generate tokens
                access_token = f"google_token_{secrets.token_hex(16)}"
                refresh_token = f"google_refresh_{secrets.token_hex(16)}"
                
                response_data = {
                    "access_token": access_token,
                    "refresh_token": refresh_token,
                    "token_type": "bearer",
                    "expires_in": 3600,
                    "user": {
                        "id": 3,
                        "email": "googleuser@gmail.com",
                        "name": "Google User",
                        "picture": "https://lh3.googleusercontent.com/a/default-user",
                        "role": "user",
                        "permissions": ["read", "write"]
                    }
                }
                
                self.send_json_response(200, response_data)
            else:
                self.send_json_response(400, {"detail": "Google credential required"})
        
        elif path == '/api/v1/auth/bypass':
            # Bypass login endpoint
            role = data.get('role', 'user')
            
            # Generate tokens
            access_token = f"auth_token_{role}_{secrets.token_hex(16)}"
            refresh_token = f"refresh_token_{role}_{secrets.token_hex(16)}"
            
            response_data = {
                "access_token": access_token,
                "refresh_token": refresh_token,
                "token_type": "bearer",
                "expires_in": 3600,
                "user": {
                    "id": 1 if role == 'admin' else 2,
                    "email": f"{role}@example.com",
                    "name": f"{role.title()} User",
                    "role": role,
                    "permissions": ["read", "write", "delete", "admin"] if role == 'admin' else ["read", "write"]
                }
            }
            
            self.send_json_response(200, response_data)
        
        elif path == '/api/v1/auth/login':
            # Regular login endpoint
            # Check if it's form data or JSON
            content_type = self.headers.get('Content-Type', '')
            
            if 'application/x-www-form-urlencoded' in content_type:
                # Parse form data
                form_data = urllib.parse.parse_qs(body.decode())
                email = form_data.get('username', [''])[0]
                password = form_data.get('password', [''])[0]
            else:
                # Parse JSON
                email = data.get('email', data.get('username', ''))
                password = data.get('password', '')
            
            # Mock authentication (accept any email/password for testing)
            role = 'admin' if 'admin' in email else 'user'
            
            access_token = f"login_token_{role}_{secrets.token_hex(16)}"
            refresh_token = f"refresh_token_{role}_{secrets.token_hex(16)}"
            
            response_data = {
                "access_token": access_token,
                "refresh_token": refresh_token,
                "token_type": "bearer",
                "expires_in": 3600
            }
            
            self.send_json_response(200, response_data)
        
        else:
            self.send_json_response(404, {"detail": "Endpoint not found"})
    
    def log_message(self, format, *args):
        """Override to show custom log format"""
        print(f"[{datetime.now().strftime('%H:%M:%S')}] {format % args}")

def start_auth_server(port=8000):
    """Start the enhanced auth server with Google OAuth support"""
    # Allow reuse of address
    socketserver.TCPServer.allow_reuse_address = True
    
    with socketserver.TCPServer(("", port), AuthHandler) as httpd:
        print(f"🚀 Enhanced Auth Server with Google OAuth starting on http://localhost:{port}")
        print(f"📍 Available endpoints:")
        print(f"   GET  / - Server info")
        print(f"   GET  /health - Health check")
        print(f"   POST /api/v1/auth/google - Google OAuth login")
        print(f"   POST /api/v1/auth/bypass - Bypass login")
        print(f"   GET  /api/v1/auth/me - Get user info")
        print(f"   POST /api/v1/auth/login - Regular login")
        print(f"\n🧪 Test commands:")
        print(f"   curl http://localhost:{port}/")
        print(f"   curl -X POST http://localhost:{port}/api/v1/auth/google -H 'Content-Type: application/json' -d '{{\"credential\":\"test-google-token\"}}'")
        print(f"   curl -X POST http://localhost:{port}/api/v1/auth/bypass -H 'Content-Type: application/json' -d '{{\"role\":\"user\"}}'")
        print(f"\n⏹️  Press Ctrl+C to stop")
        print("-" * 50)
        
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print(f"\n🛑 Server stopped")

if __name__ == "__main__":
    start_auth_server()