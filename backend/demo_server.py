#!/usr/bin/env python3
"""
Demo server that binds to all interfaces for WSL access
"""
import json
from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.parse
import socket

class APIHandler(BaseHTTPRequestHandler):
    def _send_response(self, status=200, data=None):
        self.send_response(status)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()
        if data:
            self.wfile.write(json.dumps(data, indent=2).encode())
    
    def do_OPTIONS(self):
        self._send_response()
    
    def do_GET(self):
        parsed_path = urllib.parse.urlparse(self.path)
        path = parsed_path.path
        
        routes = {
            '/': {
                "message": "Welcome to AI-First Payroll & HR API",
                "version": "1.0.0",
                "endpoints": {
                    "employees": "/api/v1/employees",
                    "payroll": "/api/v1/payroll/history",
                    "health": "/health",
                    "docs": "/docs"
                },
                "status": "Running in demo mode"
            },
            '/health': {
                "status": "healthy",
                "service": "AI-First Payroll & HR",
                "environment": "development"
            },
            '/api/v1/employees': {
                "employees": [
                    {"id": 1, "name": "John Doe", "department": "Engineering", "position": "Senior Developer", "salary": 75000, "hire_date": "2022-01-15"},
                    {"id": 2, "name": "Jane Smith", "department": "HR", "position": "HR Manager", "salary": 65000, "hire_date": "2021-06-20"},
                    {"id": 3, "name": "Bob Johnson", "department": "Sales", "position": "Sales Lead", "salary": 70000, "hire_date": "2023-03-10"}
                ],
                "total": 3
            },
            '/api/v1/payroll/history': {
                "history": [
                    {"id": 1, "employee_id": 1, "employee_name": "John Doe", "period": "2024-01", "gross": 6250, "deductions": 1375, "net": 4875},
                    {"id": 2, "employee_id": 2, "employee_name": "Jane Smith", "period": "2024-01", "gross": 5417, "deductions": 1192, "net": 4225},
                    {"id": 3, "employee_id": 3, "employee_name": "Bob Johnson", "period": "2024-01", "gross": 5833, "deductions": 1283, "net": 4550}
                ],
                "total": 3
            },
            '/docs': {
                "openapi": "3.0.0",
                "info": {
                    "title": "AI-First Payroll & HR API",
                    "version": "1.0.0",
                    "description": "Demo API for AI-powered Payroll and HR management"
                },
                "paths": {
                    "/": {"get": {"summary": "Root endpoint with API info"}},
                    "/health": {"get": {"summary": "Health check endpoint"}},
                    "/api/v1/employees": {"get": {"summary": "List all employees"}},
                    "/api/v1/payroll/history": {"get": {"summary": "Get payroll history"}}
                }
            }
        }
        
        if path in routes:
            self._send_response(200, routes[path])
        else:
            self._send_response(404, {"error": "Not found", "available_endpoints": list(routes.keys())})

def get_ip():
    """Get the IP address of the machine"""
    try:
        # Connect to an external host to get the local IP
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
        return ip
    except:
        return "localhost"

def main():
    # Bind to all interfaces (0.0.0.0) instead of just localhost
    server_address = ('0.0.0.0', 8000)
    local_ip = get_ip()
    
    print("=" * 60)
    print("🎯 AI-First Payroll & HR - Demo Server")
    print("=" * 60)
    print("\n⚡ Starting server on all interfaces...")
    print("\n🌐 Server is accessible at:")
    print(f"   • http://localhost:8000")
    print(f"   • http://{local_ip}:8000")
    print(f"   • http://127.0.0.1:8000")
    print("\n📱 For WSL users, try:")
    print("   • From Windows: http://localhost:8000")
    print("   • Get WSL IP: wsl hostname -I")
    print("\n🔍 Available endpoints:")
    print("   • GET / - API information")
    print("   • GET /api/v1/employees - List employees")
    print("   • GET /api/v1/payroll/history - Payroll history")
    print("   • GET /health - Health check")
    print("   • GET /docs - API documentation")
    print("\n⚡ Press Ctrl+C to stop")
    print("=" * 60)
    
    server = HTTPServer(server_address, APIHandler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n\n👋 Server stopped. Goodbye!")

if __name__ == "__main__":
    main()