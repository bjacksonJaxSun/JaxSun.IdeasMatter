import io
import logging
from datetime import datetime
from typing import Dict, Any, Optional
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter, A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import inch
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer, PageBreak, Image
from reportlab.platypus.flowables import HRFlowable
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_JUSTIFY

from app.models.research import ResearchOption, ResearchSession
from app.schemas.research import SwotAnalysis

logger = logging.getLogger(__name__)

class PdfService:
    def __init__(self):
        self.styles = getSampleStyleSheet()
        self._setup_custom_styles()
    
    def _setup_custom_styles(self):
        """Setup custom styles for the PDF"""
        self.styles.add(ParagraphStyle(
            name='CustomTitle',
            parent=self.styles['Title'],
            fontSize=24,
            textColor=colors.HexColor('#1f2937'),
            spaceAfter=12,
            alignment=TA_CENTER
        ))
        
        self.styles.add(ParagraphStyle(
            name='SectionHeading',
            parent=self.styles['Heading2'],
            fontSize=16,
            textColor=colors.HexColor('#374151'),
            spaceAfter=10,
            spaceBefore=15
        ))
        
        self.styles.add(ParagraphStyle(
            name='QuadrantHeading',
            parent=self.styles['Heading3'],
            fontSize=14,
            textColor=colors.HexColor('#4b5563'),
            spaceAfter=8
        ))
        
        self.styles.add(ParagraphStyle(
            name='ItemText',
            parent=self.styles['BodyText'],
            fontSize=11,
            leading=16,
            textColor=colors.HexColor('#374151')
        ))
        
        self.styles.add(ParagraphStyle(
            name='MetadataText',
            parent=self.styles['BodyText'],
            fontSize=10,
            textColor=colors.HexColor('#6b7280'),
            alignment=TA_CENTER
        ))
    
    def generate_swot_pdf(
        self,
        option: ResearchOption,
        session: ResearchSession,
        swot: SwotAnalysis,
        include_metadata: bool = True
    ) -> bytes:
        """Generate a PDF report for SWOT analysis"""
        
        buffer = io.BytesIO()
        doc = SimpleDocTemplate(
            buffer,
            pagesize=letter,
            rightMargin=72,
            leftMargin=72,
            topMargin=72,
            bottomMargin=72
        )
        
        # Build the story
        story = []
        
        # Title
        story.append(Paragraph("SWOT Analysis Report", self.styles['CustomTitle']))
        story.append(Spacer(1, 0.2 * inch))
        
        # Option details
        story.append(Paragraph(f"<b>Option:</b> {option.title}", self.styles['SectionHeading']))
        if option.description:
            story.append(Paragraph(option.description, self.styles['ItemText']))
        story.append(Spacer(1, 0.1 * inch))
        
        # Session context
        story.append(Paragraph(f"<b>Idea:</b> {session.title}", self.styles['BodyText']))
        if session.description:
            story.append(Paragraph(session.description, self.styles['ItemText']))
        story.append(Spacer(1, 0.2 * inch))
        
        # Add horizontal line
        story.append(HRFlowable(width="100%", thickness=1, color=colors.HexColor('#e5e7eb')))
        story.append(Spacer(1, 0.2 * inch))
        
        # SWOT Matrix as a table
        story.append(Paragraph("SWOT Matrix", self.styles['SectionHeading']))
        story.append(Spacer(1, 0.1 * inch))
        
        # Create SWOT table
        swot_data = [
            ['Strengths', 'Weaknesses'],
            [self._format_list_for_table(swot.strengths), self._format_list_for_table(swot.weaknesses)],
            ['Opportunities', 'Threats'],
            [self._format_list_for_table(swot.opportunities), self._format_list_for_table(swot.threats)]
        ]
        
        swot_table = Table(swot_data, colWidths=[3.5*inch, 3.5*inch])
        swot_table.setStyle(TableStyle([
            # Headers
            ('BACKGROUND', (0, 0), (1, 0), colors.HexColor('#10b981')),
            ('BACKGROUND', (0, 2), (1, 2), colors.HexColor('#3b82f6')),
            ('TEXTCOLOR', (0, 0), (1, 0), colors.white),
            ('TEXTCOLOR', (0, 2), (1, 2), colors.white),
            ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
            ('VALIGN', (0, 0), (-1, -1), 'TOP'),
            ('FONTNAME', (0, 0), (1, 0), 'Helvetica-Bold'),
            ('FONTNAME', (0, 2), (1, 2), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (1, 0), 12),
            ('FONTSIZE', (0, 2), (1, 2), 12),
            ('BOTTOMPADDING', (0, 0), (-1, -1), 12),
            ('TOPPADDING', (0, 0), (-1, -1), 12),
            ('GRID', (0, 0), (-1, -1), 1, colors.HexColor('#e5e7eb')),
            # Content cells
            ('BACKGROUND', (0, 1), (0, 1), colors.HexColor('#f0fdf4')),
            ('BACKGROUND', (1, 1), (1, 1), colors.HexColor('#fef2f2')),
            ('BACKGROUND', (0, 3), (0, 3), colors.HexColor('#eff6ff')),
            ('BACKGROUND', (1, 3), (1, 3), colors.HexColor('#fefce8')),
        ]))
        
        story.append(swot_table)
        story.append(Spacer(1, 0.3 * inch))
        
        # Detailed sections
        story.append(PageBreak())
        story.append(Paragraph("Detailed Analysis", self.styles['CustomTitle']))
        story.append(Spacer(1, 0.2 * inch))
        
        # Strengths section
        self._add_swot_section(story, "Strengths", swot.strengths, colors.HexColor('#10b981'))
        
        # Weaknesses section
        self._add_swot_section(story, "Weaknesses", swot.weaknesses, colors.HexColor('#ef4444'))
        
        # Opportunities section
        self._add_swot_section(story, "Opportunities", swot.opportunities, colors.HexColor('#3b82f6'))
        
        # Threats section
        self._add_swot_section(story, "Threats", swot.threats, colors.HexColor('#f59e0b'))
        
        # Metadata
        if include_metadata:
            story.append(Spacer(1, 0.5 * inch))
            story.append(HRFlowable(width="100%", thickness=1, color=colors.HexColor('#e5e7eb')))
            story.append(Spacer(1, 0.2 * inch))
            
            # Option scores
            story.append(Paragraph("Option Metrics", self.styles['SectionHeading']))
            
            metrics_data = [
                ['Metric', 'Score', 'Rating'],
                ['Feasibility', f"{option.feasibility_score * 100:.0f}%", self._get_rating(option.feasibility_score)],
                ['Impact', f"{option.impact_score * 100:.0f}%", self._get_rating(option.impact_score)],
                ['Risk', f"{option.risk_score * 100:.0f}%", self._get_risk_rating(option.risk_score)],
                ['SWOT Confidence', f"{swot.confidence * 100:.0f}%", self._get_rating(swot.confidence)]
            ]
            
            metrics_table = Table(metrics_data, colWidths=[2.5*inch, 1.5*inch, 1.5*inch])
            metrics_table.setStyle(TableStyle([
                ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#374151')),
                ('TEXTCOLOR', (0, 0), (-1, 0), colors.white),
                ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
                ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
                ('FONTSIZE', (0, 0), (-1, -1), 11),
                ('BOTTOMPADDING', (0, 0), (-1, -1), 8),
                ('TOPPADDING', (0, 0), (-1, -1), 8),
                ('GRID', (0, 0), (-1, -1), 1, colors.HexColor('#e5e7eb')),
                ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.white, colors.HexColor('#f9fafb')])
            ]))
            
            story.append(metrics_table)
            
            # Footer
            story.append(Spacer(1, 0.5 * inch))
            story.append(Paragraph(
                f"Generated on: {datetime.utcnow().strftime('%B %d, %Y at %I:%M %p UTC')}",
                self.styles['MetadataText']
            ))
            
            if option.recommended:
                story.append(Paragraph(
                    "<b>This option is recommended based on the analysis</b>",
                    self.styles['MetadataText']
                ))
        
        # Build PDF
        doc.build(story)
        
        buffer.seek(0)
        return buffer.read()
    
    def _format_list_for_table(self, items: list) -> Paragraph:
        """Format a list of items for table cell"""
        if not items:
            return Paragraph("No items identified", self.styles['ItemText'])
        
        bullet_items = []
        for item in items:
            bullet_items.append(f"• {item}")
        
        return Paragraph("<br/>".join(bullet_items), self.styles['ItemText'])
    
    def _add_swot_section(self, story: list, title: str, items: list, color: colors.Color):
        """Add a detailed SWOT section"""
        # Section header with colored background
        header_table = Table([[title]], colWidths=[7*inch])
        header_table.setStyle(TableStyle([
            ('BACKGROUND', (0, 0), (-1, -1), color),
            ('TEXTCOLOR', (0, 0), (-1, -1), colors.white),
            ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
            ('FONTNAME', (0, 0), (-1, -1), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, -1), 14),
            ('LEFTPADDING', (0, 0), (-1, -1), 12),
            ('RIGHTPADDING', (0, 0), (-1, -1), 12),
            ('TOPPADDING', (0, 0), (-1, -1), 8),
            ('BOTTOMPADDING', (0, 0), (-1, -1), 8),
        ]))
        
        story.append(header_table)
        story.append(Spacer(1, 0.1 * inch))
        
        if items:
            for i, item in enumerate(items, 1):
                story.append(Paragraph(f"{i}. {item}", self.styles['ItemText']))
                story.append(Spacer(1, 0.05 * inch))
        else:
            story.append(Paragraph("No items identified in this category", self.styles['ItemText']))
        
        story.append(Spacer(1, 0.2 * inch))
    
    def _get_rating(self, score: float) -> str:
        """Convert score to rating"""
        if score >= 0.8:
            return "Excellent"
        elif score >= 0.6:
            return "Good"
        elif score >= 0.4:
            return "Fair"
        else:
            return "Poor"
    
    def _get_risk_rating(self, score: float) -> str:
        """Convert risk score to rating (inverted)"""
        if score >= 0.8:
            return "Very High"
        elif score >= 0.6:
            return "High"
        elif score >= 0.4:
            return "Moderate"
        else:
            return "Low"