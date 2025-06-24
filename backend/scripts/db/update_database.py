#!/usr/bin/env python3
"""
Simple database update script to add SWOT analysis columns
"""
import sqlite3
import os
import sys

def update_database():
    """Add SWOT analysis columns to research_options table"""
    
    db_path = "ideas_matter.db"
    if not os.path.exists(db_path):
        print(f"Database file {db_path} not found!")
        return False
    
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Check if SWOT columns already exist
        cursor.execute("PRAGMA table_info(research_options)")
        columns = [row[1] for row in cursor.fetchall()]
        
        swot_columns = [
            'swot_strengths', 'swot_weaknesses', 'swot_opportunities', 
            'swot_threats', 'swot_generated_at', 'swot_confidence'
        ]
        
        missing_columns = [col for col in swot_columns if col not in columns]
        
        if not missing_columns:
            print("✅ All SWOT columns already exist in database")
            return True
        
        print(f"Adding missing columns: {missing_columns}")
        
        # Add missing SWOT columns
        for column in missing_columns:
            if column in ['swot_strengths', 'swot_weaknesses', 'swot_opportunities', 'swot_threats']:
                # JSON columns
                cursor.execute(f"ALTER TABLE research_options ADD COLUMN {column} TEXT")
                print(f"✅ Added {column} column")
            elif column == 'swot_generated_at':
                # DateTime column
                cursor.execute(f"ALTER TABLE research_options ADD COLUMN {column} DATETIME")
                print(f"✅ Added {column} column")
            elif column == 'swot_confidence':
                # Float column with default
                cursor.execute(f"ALTER TABLE research_options ADD COLUMN {column} REAL DEFAULT 0.7")
                print(f"✅ Added {column} column")
        
        conn.commit()
        print("✅ Database update completed successfully!")
        
        # Verify the changes
        cursor.execute("PRAGMA table_info(research_options)")
        new_columns = [row[1] for row in cursor.fetchall()]
        print(f"Current columns: {new_columns}")
        
        return True
        
    except Exception as e:
        print(f"❌ Error updating database: {e}")
        return False
    finally:
        if conn:
            conn.close()

def check_database_integrity():
    """Check database tables and relationships"""
    
    db_path = "ideas_matter.db"
    if not os.path.exists(db_path):
        print(f"Database file {db_path} not found!")
        return False
    
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Check main tables exist
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
        tables = [row[0] for row in cursor.fetchall()]
        
        required_tables = [
            'research_sessions', 'research_conversations', 'research_insights',
            'research_options', 'research_reports', 'research_fact_checks'
        ]
        
        missing_tables = [table for table in required_tables if table not in tables]
        
        if missing_tables:
            print(f"❌ Missing tables: {missing_tables}")
            return False
        
        print("✅ All required tables exist")
        
        # Check for data integrity
        cursor.execute("SELECT COUNT(*) FROM research_sessions")
        session_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM research_options")
        option_count = cursor.fetchone()[0]
        
        print(f"Database contents: {session_count} sessions, {option_count} options")
        
        return True
        
    except Exception as e:
        print(f"❌ Error checking database: {e}")
        return False
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    print("=== Database Update Script ===")
    
    # Change to backend directory
    if not os.path.exists("ideas_matter.db"):
        backend_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(backend_dir)
    
    print("1. Checking database integrity...")
    if not check_database_integrity():
        print("❌ Database integrity check failed")
        sys.exit(1)
    
    print("\n2. Updating database schema...")
    if not update_database():
        print("❌ Database update failed")
        sys.exit(1)
    
    print("\n3. Final integrity check...")
    if check_database_integrity():
        print("\n✅ Database update completed successfully!")
        print("\nNext steps:")
        print("1. Restart the backend server")
        print("2. Test idea submission and SWOT analysis")
    else:
        print("❌ Post-update integrity check failed")
        sys.exit(1)