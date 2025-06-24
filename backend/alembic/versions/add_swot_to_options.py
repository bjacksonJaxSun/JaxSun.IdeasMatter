"""Add SWOT analysis to research options

Revision ID: add_swot_to_options
Revises: 
Create Date: 2025-06-23

"""
from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import sqlite

# revision identifiers, used by Alembic.
revision = 'add_swot_to_options'
down_revision = None
branch_labels = None
depends_on = None


def upgrade():
    # Add SWOT analysis columns to research_options table
    with op.batch_alter_table('research_options', schema=None) as batch_op:
        batch_op.add_column(sa.Column('swot_strengths', sa.JSON(), nullable=True))
        batch_op.add_column(sa.Column('swot_weaknesses', sa.JSON(), nullable=True))
        batch_op.add_column(sa.Column('swot_opportunities', sa.JSON(), nullable=True))
        batch_op.add_column(sa.Column('swot_threats', sa.JSON(), nullable=True))
        batch_op.add_column(sa.Column('swot_generated_at', sa.DateTime(timezone=True), nullable=True))
        batch_op.add_column(sa.Column('swot_confidence', sa.Float(), nullable=True))


def downgrade():
    # Remove SWOT analysis columns from research_options table
    with op.batch_alter_table('research_options', schema=None) as batch_op:
        batch_op.drop_column('swot_confidence')
        batch_op.drop_column('swot_generated_at')
        batch_op.drop_column('swot_threats')
        batch_op.drop_column('swot_opportunities')
        batch_op.drop_column('swot_weaknesses')
        batch_op.drop_column('swot_strengths')