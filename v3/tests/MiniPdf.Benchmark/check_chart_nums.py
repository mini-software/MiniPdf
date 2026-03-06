"""Check chart axis label formatting in reference vs MiniPdf PDFs."""
import fitz
import re

chart_cases = [
    'classic91_simple_bar_chart',
    'classic92_horizontal_bar_chart', 
    'classic93_line_chart',
    'classic100_stacked_bar_chart',
    'classic111_chart_with_axis_labels',
    'classic114_chart_large_dataset',
    'classic115_chart_negative_values',
    'classic118_bar_chart_custom_colors',
]

for name in chart_cases:
    mini_doc = fitz.open(f'../MiniPdf.Scripts/pdf_output/{name}.pdf')
    ref_doc = fitz.open(f'reference_pdfs/{name}.pdf')
    
    # Get chart page (usually page 2, index 1)
    mp = mini_doc[-1].get_text() if mini_doc.page_count > 1 else mini_doc[0].get_text()
    rp = ref_doc[-1].get_text() if ref_doc.page_count > 1 else ref_doc[0].get_text()
    
    mini_doc.close()
    ref_doc.close()
    
    # Find number-like tokens
    mini_nums = re.findall(r'-?[\d,]+(?:\.\d+)?', mp)
    ref_nums = re.findall(r'-?[\d,]+(?:\.\d+)?', rp)
    
    print(f"=== {name} ===")
    print(f"  MiniPdf nums: {mini_nums[:15]}")
    print(f"  Reference nums: {ref_nums[:15]}")
    
    # Check if reference uses commas
    has_commas = any(',' in n for n in ref_nums)
    print(f"  Ref uses commas: {has_commas}")
    print()
