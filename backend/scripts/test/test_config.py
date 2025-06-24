#!/usr/bin/env python3
"""Test if the config loads properly"""

try:
    print("Testing config import...")
    from app.core.config import get_settings
    
    print("✅ Config import successful")
    
    settings = get_settings()
    print(f"✅ Settings loaded: {settings.APP_NAME}")
    print(f"✅ Database URL: {settings.DATABASE_URL}")
    print(f"✅ Quick Launch Mode: {settings.QUICK_LAUNCH}")
    
    print("\n✅ All config tests passed!")
    
except Exception as e:
    print(f"❌ Config test failed: {e}")
    import traceback
    traceback.print_exc()