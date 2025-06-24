#!/usr/bin/env python3
"""Simple HTTP server to serve the test login page"""
import http.server
import socketserver
import os

# Change to the directory containing test_login.html
os.chdir(os.path.dirname(os.path.abspath(__file__)))

PORT = 8080

Handler = http.server.SimpleHTTPRequestHandler

print(f"Starting test server on http://localhost:{PORT}")
print(f"Open http://localhost:{PORT}/test_login.html in your browser")
print("Press Ctrl+C to stop")

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    httpd.serve_forever()