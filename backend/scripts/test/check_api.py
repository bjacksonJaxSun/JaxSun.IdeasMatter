#!/usr/bin/env python3
"""Check which API is actually running"""
import requests
import json

print("Checking API at http://localhost:8000...")
print("="*50)

try:
    # Check root endpoint
    response = requests.get("http://localhost:8000/")
    print("\n1. Root endpoint (/):")
    print(json.dumps(response.json(), indent=2))
    
    # Check if /docs returns the OpenAPI spec
    response = requests.get("http://localhost:8000/openapi.json")
    openapi = response.json()
    print(f"\n2. API Title: {openapi.get('info', {}).get('title')}")
    print(f"   Version: {openapi.get('info', {}).get('version')}")
    
    # Check available paths
    paths = list(openapi.get('paths', {}).keys())
    print(f"\n3. Available endpoints ({len(paths)} total):")
    for path in sorted(paths)[:10]:  # Show first 10
        print(f"   - {path}")
    if len(paths) > 10:
        print(f"   ... and {len(paths) - 10} more")
    
    # Check for auth endpoints
    auth_endpoints = [p for p in paths if 'auth' in p]
    print(f"\n4. Auth endpoints found: {len(auth_endpoints)}")
    for endpoint in auth_endpoints:
        print(f"   - {endpoint}")
    
    # Check for research endpoints  
    research_endpoints = [p for p in paths if 'research' in p]
    print(f"\n5. Research endpoints found: {len(research_endpoints)}")
    for endpoint in research_endpoints[:5]:
        print(f"   - {endpoint}")
        
except Exception as e:
    print(f"\n❌ Error: {e}")
    print("\nMake sure the API is running with: python main.py")