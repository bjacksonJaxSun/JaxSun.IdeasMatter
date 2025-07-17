#!/usr/bin/env python3
"""
Trigger GitHub workflow using API directly
No GitHub CLI required - just needs a token
"""

import os
import sys
import json
import time
from urllib.request import Request, urlopen
from urllib.error import HTTPError

def trigger_workflow(token, owner, repo, workflow_file, inputs):
    """Trigger a GitHub workflow using the API"""
    
    # API endpoint
    url = f"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow_file}/dispatches"
    
    # Prepare the request
    data = {
        "ref": "master",  # or "main" depending on your default branch
        "inputs": inputs
    }
    
    headers = {
        "Authorization": f"token {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }
    
    # Make the request
    req = Request(url, data=json.dumps(data).encode(), headers=headers, method='POST')
    
    try:
        response = urlopen(req)
        if response.status == 204:
            return True, "Workflow triggered successfully!"
        else:
            return False, f"Unexpected status code: {response.status}"
    except HTTPError as e:
        error_body = e.read().decode()
        return False, f"HTTP Error {e.code}: {error_body}"
    except Exception as e:
        return False, f"Error: {str(e)}"

def get_latest_run(token, owner, repo, workflow_file):
    """Get the latest workflow run"""
    
    url = f"https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow_file}/runs?per_page=1"
    
    headers = {
        "Authorization": f"token {token}",
        "Accept": "application/vnd.github.v3+json"
    }
    
    req = Request(url, headers=headers)
    
    try:
        response = urlopen(req)
        data = json.loads(response.read().decode())
        if data["workflow_runs"]:
            run = data["workflow_runs"][0]
            return {
                "id": run["id"],
                "status": run["status"],
                "url": run["html_url"]
            }
        return None
    except Exception:
        return None

def main():
    print("GitHub Workflow Trigger (API Method)")
    print("====================================")
    print()
    
    # Check for token
    token = os.environ.get("GH_TOKEN") or os.environ.get("GITHUB_TOKEN")
    
    if not token:
        print("ERROR: No GitHub token found!")
        print()
        print("To set up a token:")
        print("1. Go to: https://github.com/settings/tokens")
        print("2. Generate new token (classic)")
        print("3. Select scopes: 'repo' and 'workflow'")
        print("4. Set the token:")
        print("   - Windows: set GH_TOKEN=your-token-here")
        print("   - Linux/Mac: export GH_TOKEN='your-token-here'")
        print()
        print("Or create a .env file with: GH_TOKEN=your-token-here")
        sys.exit(1)
    
    # Repository details
    owner = "bjackson071968"
    repo = "Jackson.Ideas"
    workflow_file = "create-vision.yml"
    
    # Workflow inputs
    inputs = {
        "product_name": "Ideas Matter",
        "vision_file_path": "Commands/docs/visions/ideas-matter/vision.md",
        "preview": "false"
    }
    
    print(f"Repository: {owner}/{repo}")
    print(f"Workflow: {workflow_file}")
    print(f"Product: {inputs['product_name']}")
    print()
    
    # Trigger the workflow
    print("Triggering workflow...")
    success, message = trigger_workflow(token, owner, repo, workflow_file, inputs)
    
    if success:
        print(f"✓ {message}")
        print()
        
        # Wait and get the run URL
        print("Waiting for workflow to start...")
        time.sleep(3)
        
        run_info = get_latest_run(token, owner, repo, workflow_file)
        if run_info:
            print(f"✓ Workflow started!")
            print(f"  Status: {run_info['status']}")
            print(f"  URL: {run_info['url']}")
            print()
            print("The workflow will create:")
            print("  - Vision Issue (pinned)")
            print("  - GitHub Project")
            print("  - All necessary labels")
        else:
            print("! Could not get workflow status, but it should be running")
            print(f"  Check: https://github.com/{owner}/{repo}/actions")
    else:
        print(f"✗ {message}")
        print()
        print("Troubleshooting:")
        print("1. Check your token has 'repo' and 'workflow' scopes")
        print("2. Verify the repository and workflow names")
        print("3. Ensure the workflow file exists in .github/workflows/")

if __name__ == "__main__":
    main()