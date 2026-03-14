"""Analyze structure of Support_Letter.docx"""
from docx import Document
import os

docx_path = os.path.join(os.path.dirname(__file__), 'Support_Letter.docx')
doc = Document(docx_path)

print('=== Document Sections ===')
for i, section in enumerate(doc.sections):
    print(f'Section {i}: width={section.page_width}, height={section.page_height}')
    print(f'  margins: left={section.left_margin}, right={section.right_margin}, top={section.top_margin}, bottom={section.bottom_margin}')

print('\n=== Paragraphs ===')
for i, para in enumerate(doc.paragraphs):
    style = para.style.name if para.style else 'None'
    align = para.alignment
    indent_left = para.paragraph_format.left_indent
    indent_right = para.paragraph_format.right_indent
    first_indent = para.paragraph_format.first_line_indent
    spacing_before = para.paragraph_format.space_before
    spacing_after = para.paragraph_format.space_after
    line_spacing = para.paragraph_format.line_spacing
    line_rule = para.paragraph_format.line_spacing_rule
    
    runs_info = []
    for r in para.runs:
        font = r.font
        runs_info.append({
            'text': r.text[:80],
            'bold': font.bold,
            'italic': font.italic,
            'size': str(font.size) if font.size else None,
            'name': font.name,
        })
    
    text_preview = para.text[:100]
    print(f'\nPara[{i}]: style={style}, align={align}')
    print(f'  indent: left={indent_left}, right={indent_right}, first={first_indent}')
    print(f'  spacing: before={spacing_before}, after={spacing_after}, line={line_spacing}, rule={line_rule}')
    print(f'  text: "{text_preview}"')
    if runs_info:
        for ri, r in enumerate(runs_info):
            print(f'  run[{ri}]: {r}')
