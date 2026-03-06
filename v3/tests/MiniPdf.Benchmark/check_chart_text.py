"""Compare full chart page text between MiniPdf and reference."""
import fitz

chart_cases = [
    'classic91_simple_bar_chart',
    'classic111_chart_with_axis_labels',
    'classic100_stacked_bar_chart',
    'classic118_bar_chart_custom_colors',
]

for name in chart_cases:
    mini_doc = fitz.open(f'../MiniPdf.Scripts/pdf_output/{name}.pdf')
    ref_doc = fitz.open(f'reference_pdfs/{name}.pdf')
    
    # Chart pages
    for pi in range(mini_doc.page_count):
        mp = mini_doc[pi].get_text()
        rp = ref_doc[pi].get_text() if pi < ref_doc.page_count else ""
        
        if mp.strip() or rp.strip():
            print(f"=== {name} page {pi+1} ===")
            print(f"--- MiniPdf ({len(mp.strip())} chars) ---")
            print(mp.strip()[:400])
            print(f"--- Reference ({len(rp.strip())} chars) ---")
            print(rp.strip()[:400])
            print()
    
    mini_doc.close()
    ref_doc.close()
