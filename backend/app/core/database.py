from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker
from sqlalchemy.orm import declarative_base, sessionmaker, Session
from sqlalchemy import create_engine
from typing import AsyncGenerator, Generator

from app.core.config import get_settings

settings = get_settings()

# Create async engine
if settings.QUICK_LAUNCH:
    # SQLite settings for quick launch
    engine = create_async_engine(
        settings.DATABASE_URL,
        echo=settings.DEBUG,
        connect_args={"check_same_thread": False},
    )
    # Sync engine for SWOT operations
    sync_database_url = settings.DATABASE_URL.replace("sqlite+aiosqlite", "sqlite")
    sync_engine = create_engine(
        sync_database_url,
        echo=settings.DEBUG,
        connect_args={"check_same_thread": False},
    )
else:
    # MySQL settings for production
    engine = create_async_engine(
        settings.DATABASE_URL,
        echo=settings.DEBUG,
        pool_size=10,
        max_overflow=20,
        pool_pre_ping=True,
    )
    # Sync engine for SWOT operations
    sync_database_url = settings.DATABASE_URL.replace("mysql+aiomysql", "mysql+pymysql")
    sync_engine = create_engine(
        sync_database_url,
        echo=settings.DEBUG,
        pool_size=10,
        max_overflow=20,
        pool_pre_ping=True,
    )

# Create async session factory
AsyncSessionLocal = async_sessionmaker(
    engine,
    class_=AsyncSession,
    expire_on_commit=False,
    autoflush=False,
    autocommit=False,
)

# Create sync session factory
SyncSessionLocal = sessionmaker(
    sync_engine,
    class_=Session,
    expire_on_commit=False,
    autoflush=False,
    autocommit=False,
)

# Create declarative base
Base = declarative_base()

# Dependency to get database session
async def get_db() -> AsyncGenerator[AsyncSession, None]:
    async with AsyncSessionLocal() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise
        finally:
            await session.close()

# Dependency to get sync database session (for SWOT operations)
def get_sync_db() -> Generator[Session, None, None]:
    session = SyncSessionLocal()
    try:
        yield session
        session.commit()
    except Exception:
        session.rollback()
        raise
    finally:
        session.close()