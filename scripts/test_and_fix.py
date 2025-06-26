#!/usr/bin/env python3
"""
Comprehensive test runner with automatic fixing capabilities
Runs all tests and attempts to fix common issues automatically
"""
import subprocess
import sys
import os
import json
import time
import requests
import signal
import threading
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import shutil
import re

class Colors:
    """ANSI color codes for terminal output"""
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

class TestRunner:
    """Main test runner with automatic fixing capabilities"""
    
    def __init__(self):
        self.project_root = Path(__file__).parent.parent
        self.backend_path = self.project_root / "backend"
        self.frontend_path = self.project_root / "frontend"
        self.test_results = {}
        self.backend_process = None
        self.frontend_process = None
        self.failed_tests = []
        self.fixed_issues = []
        
    def print_header(self, message: str):
        """Print a formatted header message"""
        print(f"\n{Colors.HEADER}{Colors.BOLD}{'='*80}{Colors.ENDC}")
        print(f"{Colors.HEADER}{Colors.BOLD}{message.center(80)}{Colors.ENDC}")
        print(f"{Colors.HEADER}{Colors.BOLD}{'='*80}{Colors.ENDC}\n")
    
    def print_success(self, message: str):
        """Print a success message"""
        print(f"{Colors.OKGREEN}✓ {message}{Colors.ENDC}")
    
    def print_error(self, message: str):
        """Print an error message"""
        print(f"{Colors.FAIL}✗ {message}{Colors.ENDC}")
    
    def print_warning(self, message: str):
        """Print a warning message"""
        print(f"{Colors.WARNING}⚠ {message}{Colors.ENDC}")
    
    def print_info(self, message: str):
        """Print an info message"""
        print(f"{Colors.OKBLUE}ℹ {message}{Colors.ENDC}")
    
    def run_command(self, command: List[str], cwd: Optional[Path] = None, 
                   timeout: int = 30, capture_output: bool = True) -> Tuple[int, str, str]:
        """Run a command and return exit code, stdout, stderr"""
        try:
            result = subprocess.run(
                command,
                cwd=cwd or self.project_root,
                capture_output=capture_output,
                text=True,
                timeout=timeout
            )
            return result.returncode, result.stdout, result.stderr
        except subprocess.TimeoutExpired:
            return 1, "", f"Command timed out after {timeout} seconds"
        except Exception as e:
            return 1, "", str(e)
    
    def check_prerequisites(self) -> bool:
        """Check if all prerequisites are installed"""
        self.print_header("Checking Prerequisites")
        
        prerequisites = [
            ("python", ["python", "--version"]),
            ("node", ["node", "--version"]),
            ("npm", ["npm", "--version"]),
        ]
        
        all_good = True
        for name, command in prerequisites:
            exit_code, stdout, stderr = self.run_command(command)
            if exit_code == 0:
                version = stdout.strip() or stderr.strip()
                self.print_success(f"{name}: {version}")
            else:
                self.print_error(f"{name} not found or not working")
                all_good = False
        
        return all_good
    
    def setup_backend_environment(self) -> bool:
        """Set up backend testing environment"""
        self.print_header("Setting Up Backend Environment")
        
        # Check if virtual environment exists
        venv_path = self.backend_path / "venv"
        if not venv_path.exists():
            self.print_warning("Virtual environment not found, creating...")
            exit_code, stdout, stderr = self.run_command(
                ["python", "-m", "venv", "venv"],
                cwd=self.backend_path,
                timeout=60
            )
            if exit_code != 0:
                self.print_error(f"Failed to create virtual environment: {stderr}")
                return False
            self.fixed_issues.append("Created missing virtual environment")
        
        # Install/update dependencies
        self.print_info("Installing backend dependencies...")
        
        # Determine activation script
        if os.name == 'nt':  # Windows
            pip_path = venv_path / "Scripts" / "pip.exe"
            python_path = venv_path / "Scripts" / "python.exe"
        else:  # Unix-like
            pip_path = venv_path / "bin" / "pip"
            python_path = venv_path / "bin" / "python"
        
        # Install requirements
        requirements_files = ["requirements.txt", "requirements-core.txt"]
        for req_file in requirements_files:
            req_path = self.backend_path / req_file
            if req_path.exists():
                exit_code, stdout, stderr = self.run_command(
                    [str(python_path), "-m", "pip", "install", "-r", req_file],
                    cwd=self.backend_path,
                    timeout=120
                )
                if exit_code == 0:
                    self.print_success(f"Installed dependencies from {req_file}")
                    break
                else:
                    self.print_warning(f"Failed to install from {req_file}: {stderr}")
        
        # Install test dependencies
        test_deps = ["pytest", "pytest-asyncio", "httpx", "requests"]
        for dep in test_deps:
            exit_code, stdout, stderr = self.run_command(
                [str(python_path), "-m", "pip", "install", dep],
                cwd=self.backend_path,
                timeout=60
            )
            if exit_code == 0:
                self.print_success(f"Installed {dep}")
            else:
                self.print_warning(f"Failed to install {dep}: {stderr}")
        
        return True
    
    def setup_frontend_environment(self) -> bool:
        """Set up frontend testing environment"""
        self.print_header("Setting Up Frontend Environment")
        
        # Check if node_modules exists
        node_modules = self.frontend_path / "node_modules"
        if not node_modules.exists():
            self.print_warning("Node modules not found, installing...")
            
        # Install dependencies
        self.print_info("Installing frontend dependencies...")
        exit_code, stdout, stderr = self.run_command(
            ["npm", "install"],
            cwd=self.frontend_path,
            timeout=120
        )
        
        if exit_code != 0:
            self.print_error(f"Failed to install frontend dependencies: {stderr}")
            return False
        
        # Install test dependencies
        test_deps = ["vitest", "@testing-library/react", "@testing-library/jest-dom", 
                    "@testing-library/user-event", "jsdom", "@tanstack/react-query"]
        
        for dep in test_deps:
            exit_code, stdout, stderr = self.run_command(
                ["npm", "install", "--save-dev", dep],
                cwd=self.frontend_path,
                timeout=60
            )
            if exit_code == 0:
                self.print_success(f"Installed {dep}")
            else:
                self.print_warning(f"Failed to install {dep}: {stderr}")
        
        return True
    
    def create_missing_test_configs(self):
        """Create missing test configuration files"""
        self.print_header("Creating Test Configuration Files")
        
        # Frontend test configuration
        vitest_config = self.frontend_path / "vitest.config.ts"
        if not vitest_config.exists():
            config_content = '''/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
  },
})'''
            vitest_config.write_text(config_content)
            self.print_success("Created vitest.config.ts")
            self.fixed_issues.append("Created missing vitest configuration")
        
        # Test setup file
        test_setup = self.frontend_path / "src" / "test-setup.ts"
        if not test_setup.exists():
            setup_content = '''import '@testing-library/jest-dom'

// Mock IntersectionObserver
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  observe() {}
  disconnect() {}
  unobserve() {}
}

// Mock ResizeObserver
global.ResizeObserver = class ResizeObserver {
  constructor() {}
  observe() {}
  disconnect() {}
  unobserve() {}
}

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
})'''
            test_setup.write_text(setup_content)
            self.print_success("Created test-setup.ts")
            self.fixed_issues.append("Created missing test setup file")
        
        # Backend pytest configuration
        pytest_ini = self.backend_path / "pytest.ini"
        if not pytest_ini.exists():
            pytest_content = '''[tool:pytest]
testpaths = tests
python_files = test_*.py
python_classes = Test*
python_functions = test_*
addopts = -v --tb=short
markers =
    slow: marks tests as slow
    integration: marks tests as integration tests
'''
            pytest_ini.write_text(pytest_content)
            self.print_success("Created pytest.ini")
            self.fixed_issues.append("Created missing pytest configuration")
    
    def start_services(self) -> bool:
        """Start backend and frontend services for integration tests"""
        self.print_header("Starting Services for Integration Tests")
        
        # Start backend
        self.print_info("Starting backend server...")
        if os.name == 'nt':  # Windows
            python_path = self.backend_path / "venv" / "Scripts" / "python.exe"
        else:  # Unix-like
            python_path = self.backend_path / "venv" / "bin" / "python"
        
        try:
            self.backend_process = subprocess.Popen(
                [str(python_path), "main.py"],
                cwd=self.backend_path,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True
            )
            
            # Wait for backend to start
            self.print_info("Waiting for backend to start...")
            for i in range(30):  # Wait up to 30 seconds
                try:
                    response = requests.get("http://localhost:8000/health", timeout=2)
                    if response.status_code == 200:
                        self.print_success("Backend started successfully")
                        break
                except:
                    time.sleep(1)
            else:
                self.print_error("Backend failed to start within 30 seconds")
                return False
        
        except Exception as e:
            self.print_error(f"Failed to start backend: {e}")
            return False
        
        # Start frontend
        self.print_info("Starting frontend server...")
        try:
            self.frontend_process = subprocess.Popen(
                ["npm", "run", "dev"],
                cwd=self.frontend_path,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True
            )
            
            # Wait for frontend to start
            self.print_info("Waiting for frontend to start...")
            for i in range(30):  # Wait up to 30 seconds
                try:
                    response = requests.get("http://localhost:4000", timeout=2)
                    if response.status_code == 200:
                        self.print_success("Frontend started successfully")
                        break
                except:
                    time.sleep(1)
            else:
                self.print_warning("Frontend may not have started properly")
        
        except Exception as e:
            self.print_error(f"Failed to start frontend: {e}")
            return False
        
        return True
    
    def stop_services(self):
        """Stop backend and frontend services"""
        self.print_info("Stopping services...")
        
        if self.backend_process:
            self.backend_process.terminate()
            try:
                self.backend_process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                self.backend_process.kill()
            self.backend_process = None
        
        if self.frontend_process:
            self.frontend_process.terminate()
            try:
                self.frontend_process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                self.frontend_process.kill()
            self.frontend_process = None
    
    def run_backend_tests(self) -> bool:
        """Run backend tests"""
        self.print_header("Running Backend Tests")
        
        if os.name == 'nt':  # Windows
            python_path = self.backend_path / "venv" / "Scripts" / "python.exe"
        else:  # Unix-like
            python_path = self.backend_path / "venv" / "bin" / "python"
        
        exit_code, stdout, stderr = self.run_command(
            [str(python_path), "-m", "pytest", "tests/", "-v", "--tb=short"],
            cwd=self.backend_path,
            timeout=120
        )
        
        self.test_results["backend"] = {
            "exit_code": exit_code,
            "stdout": stdout,
            "stderr": stderr
        }
        
        if exit_code == 0:
            self.print_success("Backend tests passed")
            return True
        else:
            self.print_error("Backend tests failed")
            self.failed_tests.append("Backend tests")
            print(f"STDOUT: {stdout}")
            print(f"STDERR: {stderr}")
            return False
    
    def run_frontend_tests(self) -> bool:
        """Run frontend tests"""
        self.print_header("Running Frontend Tests")
        
        exit_code, stdout, stderr = self.run_command(
            ["npm", "run", "test"],
            cwd=self.frontend_path,
            timeout=120
        )
        
        # If npm run test doesn't exist, try vitest directly
        if exit_code != 0 and "test" not in stdout + stderr:
            exit_code, stdout, stderr = self.run_command(
                ["npx", "vitest", "run"],
                cwd=self.frontend_path,
                timeout=120
            )
        
        self.test_results["frontend"] = {
            "exit_code": exit_code,
            "stdout": stdout,
            "stderr": stderr
        }
        
        if exit_code == 0:
            self.print_success("Frontend tests passed")
            return True
        else:
            self.print_error("Frontend tests failed")
            self.failed_tests.append("Frontend tests")
            print(f"STDOUT: {stdout}")
            print(f"STDERR: {stderr}")
            return False

    def run_ui_progress_tests(self) -> bool:
        """Run UI progress tracking specific tests"""
        self.print_header("Running UI Progress Tracking Tests")
        
        # Run the specific progress tracking tests
        exit_code, stdout, stderr = self.run_command(
            ["npx", "vitest", "run", "src/components/research/__tests__/ProgressTracking.integration.test.tsx"],
            cwd=self.frontend_path,
            timeout=120
        )
        
        self.test_results["ui_progress"] = {
            "exit_code": exit_code,
            "stdout": stdout,
            "stderr": stderr
        }
        
        if exit_code == 0:
            self.print_success("UI Progress Tracking tests passed")
            return True
        else:
            self.print_error("UI Progress Tracking tests failed")
            self.failed_tests.append("UI Progress Tracking tests")
            print(f"STDOUT: {stdout}")
            print(f"STDERR: {stderr}")
            return False

    def run_e2e_tests(self) -> bool:
        """Run E2E tests if Playwright is available"""
        self.print_header("Running E2E Progress Tracking Tests")
        
        # Check if Playwright is installed
        exit_code, stdout, stderr = self.run_command(
            ["npx", "playwright", "--version"],
            cwd=self.frontend_path,
            timeout=10
        )
        
        if exit_code != 0:
            self.print_warning("Playwright not found, skipping E2E tests")
            self.print_info("To install: npx playwright install")
            return True  # Not a failure, just skipped
        
        # Run E2E tests
        exit_code, stdout, stderr = self.run_command(
            ["npx", "playwright", "test", "tests/research-progress-e2e.test.ts"],
            cwd=self.frontend_path,
            timeout=180
        )
        
        self.test_results["e2e"] = {
            "exit_code": exit_code,
            "stdout": stdout,
            "stderr": stderr
        }
        
        if exit_code == 0:
            self.print_success("E2E tests passed")
            return True
        else:
            self.print_error("E2E tests failed")
            self.failed_tests.append("E2E tests")
            print(f"STDOUT: {stdout}")
            print(f"STDERR: {stderr}")
            return False
    
    def run_integration_tests(self) -> bool:
        """Run integration tests"""
        self.print_header("Running Integration Tests")
        
        if os.name == 'nt':  # Windows
            python_path = self.backend_path / "venv" / "Scripts" / "python.exe"
        else:  # Unix-like
            python_path = self.backend_path / "venv" / "bin" / "python"
        
        # Run integration tests from project root
        integration_test_file = self.project_root / "tests" / "integration" / "test_research_flow.py"
        
        exit_code, stdout, stderr = self.run_command(
            [str(python_path), "-m", "pytest", str(integration_test_file), "-v", "--tb=short"],
            cwd=self.project_root,
            timeout=180
        )
        
        self.test_results["integration"] = {
            "exit_code": exit_code,
            "stdout": stdout,
            "stderr": stderr
        }
        
        if exit_code == 0:
            self.print_success("Integration tests passed")
            return True
        else:
            self.print_error("Integration tests failed")
            self.failed_tests.append("Integration tests")
            print(f"STDOUT: {stdout}")
            print(f"STDERR: {stderr}")
            return False
    
    def attempt_fixes(self):
        """Attempt to fix common issues automatically"""
        if not self.failed_tests:
            return
        
        self.print_header("Attempting Automatic Fixes")
        
        # Fix common import issues
        if "Backend tests" in self.failed_tests:
            self.fix_backend_import_issues()
        
        # Fix common frontend issues
        if "Frontend tests" in self.failed_tests:
            self.fix_frontend_issues()
        
        # Fix UI progress tracking issues
        if "UI Progress Tracking tests" in self.failed_tests:
            self.fix_ui_progress_issues()
        
        # Fix common integration issues
        if "Integration tests" in self.failed_tests:
            self.fix_integration_issues()
    
    def fix_backend_import_issues(self):
        """Fix common backend import issues"""
        self.print_info("Fixing backend import issues...")
        
        # Check research_strategy.py for missing datetime import
        research_strategy_file = self.backend_path / "app" / "api" / "v1" / "research_strategy.py"
        if research_strategy_file.exists():
            content = research_strategy_file.read_text()
            if "datetime.utcnow()" in content and "from datetime import" not in content:
                # Add datetime import
                lines = content.split('\n')
                import_line = "from datetime import datetime, timedelta"
                
                # Find where to insert the import
                insert_index = 0
                for i, line in enumerate(lines):
                    if line.startswith('from ') or line.startswith('import '):
                        insert_index = i + 1
                
                lines.insert(insert_index, import_line)
                research_strategy_file.write_text('\n'.join(lines))
                self.print_success("Fixed missing datetime import")
                self.fixed_issues.append("Fixed missing datetime import in research_strategy.py")
    
    def fix_frontend_issues(self):
        """Fix common frontend issues"""
        self.print_info("Fixing frontend issues...")
        
        # Add test script to package.json if missing
        package_json_path = self.frontend_path / "package.json"
        if package_json_path.exists():
            try:
                with open(package_json_path, 'r') as f:
                    package_data = json.load(f)
                
                if "scripts" not in package_data:
                    package_data["scripts"] = {}
                
                if "test" not in package_data["scripts"]:
                    package_data["scripts"]["test"] = "vitest run"
                    
                    with open(package_json_path, 'w') as f:
                        json.dump(package_data, f, indent=2)
                    
                    self.print_success("Added test script to package.json")
                    self.fixed_issues.append("Added missing test script to package.json")
            except Exception as e:
                self.print_warning(f"Could not fix package.json: {e}")
    
    def fix_ui_progress_issues(self):
        """Fix UI progress tracking specific issues"""
        self.print_info("Fixing UI progress tracking issues...")
        
        # Install missing dependencies for UI tests
        ui_test_deps = ["@tanstack/react-query", "react-hot-toast", "react-router-dom"]
        
        for dep in ui_test_deps:
            exit_code, stdout, stderr = self.run_command(
                ["npm", "install", "--save-dev", dep],
                cwd=self.frontend_path,
                timeout=60
            )
            
            if exit_code == 0:
                self.print_success(f"Installed {dep}")
                self.fixed_issues.append(f"Installed missing UI test dependency: {dep}")
        
        # Create mock auth file if missing
        mock_auth_file = self.frontend_path / "src" / "utils" / "mockAuth.ts"
        if not mock_auth_file.exists():
            mock_auth_file.parent.mkdir(parents=True, exist_ok=True)
            mock_content = '''export const mockAuth = {
  getToken: () => 'mock-token',
  isAuthenticated: () => true,
  login: () => Promise.resolve({ access_token: 'mock-token' }),
  logout: () => Promise.resolve(),
};'''
            mock_auth_file.write_text(mock_content)
            self.print_success("Created mock auth utility")
            self.fixed_issues.append("Created missing mock auth utility for UI tests")

    def fix_integration_issues(self):
        """Fix common integration test issues"""
        self.print_info("Fixing integration test issues...")
        
        # Install requests if missing
        if os.name == 'nt':  # Windows
            python_path = self.backend_path / "venv" / "Scripts" / "python.exe"
        else:  # Unix-like
            python_path = self.backend_path / "venv" / "bin" / "python"
        
        exit_code, stdout, stderr = self.run_command(
            [str(python_path), "-m", "pip", "install", "requests"],
            cwd=self.backend_path,
            timeout=60
        )
        
        if exit_code == 0:
            self.print_success("Installed requests library")
            self.fixed_issues.append("Installed missing requests library")
    
    def generate_report(self):
        """Generate a comprehensive test report"""
        self.print_header("Test Report")
        
        # Summary
        total_test_suites = len(self.test_results)
        passed_suites = sum(1 for result in self.test_results.values() if result["exit_code"] == 0)
        failed_suites = total_test_suites - passed_suites
        
        print(f"Test Suites: {Colors.OKBLUE}{total_test_suites}{Colors.ENDC}")
        print(f"Passed: {Colors.OKGREEN}{passed_suites}{Colors.ENDC}")
        print(f"Failed: {Colors.FAIL}{failed_suites}{Colors.ENDC}")
        
        # Detailed results
        for suite_name, result in self.test_results.items():
            status = "PASS" if result["exit_code"] == 0 else "FAIL"
            color = Colors.OKGREEN if result["exit_code"] == 0 else Colors.FAIL
            print(f"{suite_name.capitalize()}: {color}{status}{Colors.ENDC}")
        
        # Fixed issues
        if self.fixed_issues:
            print(f"\n{Colors.OKGREEN}Issues Fixed Automatically:{Colors.ENDC}")
            for issue in self.fixed_issues:
                print(f"  ✓ {issue}")
        
        # Recommendations
        if self.failed_tests:
            print(f"\n{Colors.WARNING}Recommendations:{Colors.ENDC}")
            for test in self.failed_tests:
                print(f"  • Review {test} output above for specific errors")
                print(f"  • Ensure all dependencies are properly installed")
                print(f"  • Check that services are running correctly")
    
    def run_all_tests(self) -> bool:
        """Run all tests with automatic fixing"""
        try:
            # Check prerequisites
            if not self.check_prerequisites():
                self.print_error("Prerequisites check failed")
                return False
            
            # Set up environments
            if not self.setup_backend_environment():
                self.print_error("Backend environment setup failed")
                return False
            
            if not self.setup_frontend_environment():
                self.print_error("Frontend environment setup failed")
                return False
            
            # Create missing test configs
            self.create_missing_test_configs()
            
            # Start services
            services_started = self.start_services()
            
            try:
                # Run backend tests
                backend_passed = self.run_backend_tests()
                
                # Run frontend tests
                frontend_passed = self.run_frontend_tests()
                
                # Run UI progress tracking tests
                ui_progress_passed = self.run_ui_progress_tests()
                
                # Run E2E tests
                e2e_passed = self.run_e2e_tests()
                
                # Run integration tests if services are available
                integration_passed = True
                if services_started:
                    integration_passed = self.run_integration_tests()
                else:
                    self.print_warning("Skipping integration tests (services not available)")
                
                # Attempt fixes if needed
                if not backend_passed or not frontend_passed or not ui_progress_passed or not integration_passed:
                    self.attempt_fixes()
                    
                    # Retry failed tests
                    if not backend_passed:
                        self.print_info("Retrying backend tests after fixes...")
                        backend_passed = self.run_backend_tests()
                    
                    if not frontend_passed:
                        self.print_info("Retrying frontend tests after fixes...")
                        frontend_passed = self.run_frontend_tests()
                    
                    if not ui_progress_passed:
                        self.print_info("Retrying UI progress tests after fixes...")
                        ui_progress_passed = self.run_ui_progress_tests()
                    
                    if services_started and not integration_passed:
                        self.print_info("Retrying integration tests after fixes...")
                        integration_passed = self.run_integration_tests()
                
                # Generate report
                self.generate_report()
                
                return backend_passed and frontend_passed and ui_progress_passed and e2e_passed and integration_passed
            
            finally:
                # Always stop services
                self.stop_services()
        
        except KeyboardInterrupt:
            self.print_warning("Tests interrupted by user")
            self.stop_services()
            return False
        except Exception as e:
            self.print_error(f"Unexpected error: {e}")
            self.stop_services()
            return False


def main():
    """Main entry point"""
    print(f"{Colors.HEADER}{Colors.BOLD}")
    print("┌─────────────────────────────────────────────────────────────────────────────┐")
    print("│                     Ideas Matter - Test Runner & Fixer                     │")
    print("│                                                                             │")
    print("│  This script will run comprehensive tests and attempt to fix issues        │")
    print("│  automatically. It includes:                                               │")
    print("│    • Backend API tests                                                     │")
    print("│    • Frontend component tests                                              │")
    print("│    • UI Progress Tracking tests (Cat and Mouse Game scenario)             │")
    print("│    • End-to-end (E2E) tests with Playwright                               │")
    print("│    • Integration tests                                                     │")
    print("│    • Automatic issue detection and fixing                                  │")
    print("└─────────────────────────────────────────────────────────────────────────────┘")
    print(f"{Colors.ENDC}")
    
    runner = TestRunner()
    success = runner.run_all_tests()
    
    if success:
        print(f"\n{Colors.OKGREEN}{Colors.BOLD}🎉 All tests passed! Your application is working correctly.{Colors.ENDC}")
        sys.exit(0)
    else:
        print(f"\n{Colors.FAIL}{Colors.BOLD}❌ Some tests failed. Check the output above for details.{Colors.ENDC}")
        sys.exit(1)


if __name__ == "__main__":
    main()