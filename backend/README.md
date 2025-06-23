# Backend Structure

## Directory Layout

```
backend/
├── app/                      # Main application code
│   ├── api/                  # API endpoints and routing
│   │   ├── v1/              # API version 1
│   │   │   ├── endpoints/   # Endpoint modules
│   │   │   └── dependencies/# Shared dependencies
│   │   └── middleware/      # Custom middleware
│   ├── core/                # Core functionality
│   ├── models/              # SQLAlchemy models
│   ├── schemas/             # Pydantic schemas
│   ├── services/            # Business logic
│   ├── repositories/        # Data access layer
│   ├── utils/               # Utility functions
│   └── ai/                  # AI integration layer
│       ├── providers/       # AI provider implementations
│       ├── agents/          # AI agent workflows
│       └── templates/       # Prompt templates
├── tests/                   # Test suite
│   ├── unit/               # Unit tests
│   ├── integration/        # Integration tests
│   └── fixtures/           # Test fixtures
├── alembic/                # Database migrations
│   └── versions/           # Migration files
├── scripts/                # Utility scripts
├── requirements.txt        # Python dependencies
├── .env.example           # Environment variables template
└── main.py                # Application entry point
```

## Module Descriptions

### `/app/api/`
RESTful API endpoints organized by version. Each endpoint module handles a specific resource.

### `/app/core/`
Core application components:
- Configuration management
- Security utilities
- Database connection
- Logging setup

### `/app/models/`
SQLAlchemy ORM models representing database tables.

### `/app/schemas/`
Pydantic models for request/response validation and serialization.

### `/app/services/`
Business logic layer containing the core application logic.

### `/app/repositories/`
Data access layer implementing the repository pattern for database operations.

### `/app/ai/`
AI integration layer with pluggable providers and workflow management.

### `/tests/`
Comprehensive test suite with unit and integration tests.

### `/alembic/`
Database migration management using Alembic.

## Quick Start

1. Create virtual environment:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

2. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

3. Set up environment variables:
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. Run migrations:
   ```bash
   alembic upgrade head
   ```

5. Start the server:
   ```bash
   python main.py
   ```