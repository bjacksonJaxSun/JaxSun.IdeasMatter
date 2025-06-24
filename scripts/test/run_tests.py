#!/usr/bin/env python3
"""
Unified test runner for Ideas Matter project
Runs various test suites with proper environment setup
"""

import sys
import os
import subprocess
import argparse
from pathlib import Path

# Add backend to path
backend_path = Path(__file__).parent.parent.parent / "backend"
sys.path.insert(0, str(backend_path))

def run_pytest(test_path=None, verbose=False):
    """Run pytest tests"""
    cmd = ["pytest"]
    if verbose:
        cmd.append("-v")
    if test_path:
        cmd.append(test_path)
    else:
        cmd.append(str(backend_path / "tests"))
    
    print(f"Running: {' '.join(cmd)}")
    return subprocess.run(cmd, cwd=backend_path).returncode

def run_specific_test(test_name):
    """Run a specific test module"""
    test_mapping = {
        "auth": "test_auth.py",
        "swot": "test_swot_api.py",
        "market": "test_market_analysis_simple.py",
        "idea": "test_idea_submission.py",
        "config": "test_config.py",
        "imports": "test_imports.py",
    }
    
    if test_name in test_mapping:
        test_file = backend_path / test_mapping[test_name]
        if test_file.exists():
            return run_pytest(str(test_file), verbose=True)
        else:
            print(f"Test file not found: {test_file}")
            return 1
    else:
        print(f"Unknown test: {test_name}")
        print(f"Available tests: {', '.join(test_mapping.keys())}")
        return 1

def run_frontend_tests():
    """Run frontend linting and tests"""
    frontend_path = Path(__file__).parent.parent.parent / "frontend"
    
    print("\nRunning frontend lint...")
    lint_result = subprocess.run(["npm", "run", "lint"], cwd=frontend_path).returncode
    
    if lint_result == 0:
        print("Frontend lint passed!")
    else:
        print("Frontend lint failed!")
    
    return lint_result

def main():
    parser = argparse.ArgumentParser(description="Run Ideas Matter tests")
    parser.add_argument("test", nargs="?", help="Specific test to run (auth, swot, market, idea, config, imports)")
    parser.add_argument("-v", "--verbose", action="store_true", help="Verbose output")
    parser.add_argument("--frontend", action="store_true", help="Run frontend tests")
    parser.add_argument("--all", action="store_true", help="Run all tests")
    
    args = parser.parse_args()
    
    if args.all:
        print("Running all tests...\n")
        
        # Backend tests
        print("=== Backend Tests ===")
        backend_result = run_pytest(verbose=args.verbose)
        
        # Frontend tests
        print("\n=== Frontend Tests ===")
        frontend_result = run_frontend_tests()
        
        if backend_result == 0 and frontend_result == 0:
            print("\n✅ All tests passed!")
            return 0
        else:
            print("\n❌ Some tests failed!")
            return 1
    
    elif args.frontend:
        return run_frontend_tests()
    
    elif args.test:
        return run_specific_test(args.test)
    
    else:
        # Default: run backend tests
        return run_pytest(verbose=args.verbose)

if __name__ == "__main__":
    sys.exit(main())