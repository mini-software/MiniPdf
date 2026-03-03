#!/usr/bin/env python3
"""Analyze what text our generated PDFs contain for specific test cases."""
import subprocess
import os
import sys

# Generate PDFs using the test scripts then analyze what text they contain
# First, let's use the benchmark run output PDFs
import fitz

xlsx_dir = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output'
pdf_dir = r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reports\minipdf'

tests = [
    'classic06_tall_table',
    'classic24_red_text',
    'classic44_employee_roster',
    'classic35_explicit_row_heights',
    'classic38_hyperlink_cell',
    'classic48_survey_results',
    'classic49_contact_list',
    'classic50_budget_vs_actuals',
    'classic51_product_catalog',
    'classic52_pivot_summary',
    'classic53_invoice',
    'classic56_alternating_row_colors',
]

for t in tests:
    pdf_path = os.path.join(pdf_dir, f'{t}.pdf')
    if not os.path.exists(pdf_path):
        print(f'{t}: PDF not found')
        continue
    doc = fitz.open(pdf_path)
    text = doc[0].get_text()
    # Show first 200 chars
    print(f'{t}: {repr(text[:300])}')
    print()
