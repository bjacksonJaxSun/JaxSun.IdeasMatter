#!/usr/bin/env python3
"""
Minimal launcher for demo purposes without dependencies
"""
import json
from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.parse

class APIHandler(BaseHTTPRequestHandler):
    def _send_response(self, status=200, data=None):
        self.send_response(status)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()
        if data:
            self.wfile.write(json.dumps(data).encode())
    
    def do_OPTIONS(self):
        self._send_response()
    
    def do_GET(self):
        parsed_path = urllib.parse.urlparse(self.path)
        path = parsed_path.path
        
        routes = {
            '/': {
                "message": "Welcome to AI-First Payroll & HR API",
                "version": "1.0.0",
                "docs": "/docs",
                "status": "Running in minimal mode (no dependencies)"
            },
            '/health': {
                "status": "healthy",
                "service": "AI-First Payroll & HR",
                "environment": "development",
                "mode": "minimal"
            },
            '/api/v1/employees': {
                "employees": [
                    {"id": 1, "name": "John Doe", "department": "Engineering", "salary": 75000},
                    {"id": 2, "name": "Jane Smith", "department": "HR", "salary": 65000},
                    {"id": 3, "name": "Bob Johnson", "department": "Sales", "salary": 70000}
                ],
                "total": 3
            },
            '/api/v1/payroll/history': {
                "history": [
                    {"id": 1, "employee_id": 1, "period": "2024-01", "gross": 6250, "net": 4875},
                    {"id": 2, "employee_id": 2, "period": "2024-01", "gross": 5417, "net": 4225}
                ],
                "total": 2
            },
            '/docs': {
                "openapi": "3.0.0",
                "info": {
                    "title": "AI-First Payroll & HR API",
                    "version": "1.0.0"
                },
                "paths": {
                    "/": {"get": {"summary": "Root endpoint"}},
                    "/health": {"get": {"summary": "Health check"}},
                    "/api/v1/employees": {"get": {"summary": "List employees"}},
                    "/api/v1/payroll/history": {"get": {"summary": "Payroll history"}}
                }
            }
        }
        
        if path in routes:
            self._send_response(200, routes[path])
        else:
            self._send_response(404, {"error": "Not found", "path": path})

def main():
    print("=" * 60)
    print("🎯 AI-First Payroll & HR - Minimal Demo Server")
    print("=" * 60)
    print("\n⚡ Starting minimal server (no dependencies required)...")
    print("\n🌐 Server running at:")
    print("   • API: http://localhost:8000")
    print("   • Try: http://localhost:8000/api/v1/employees")
    print("   • Docs: http://localhost:8000/docs")
    print("\n📝 This is a demo server with mock data")
    print("⚡ Press Ctrl+C to stop")
    print("=" * 60)
    
    server = HTTPServer(('localhost', 8000), APIHandler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n\n👋 Server stopped. Goodbye!")

if __name__ == "__main__":
    main()