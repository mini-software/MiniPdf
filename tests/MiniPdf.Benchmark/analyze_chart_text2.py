"""Analyze chart cases - what text is missing/different."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'

# Chart cases with moderate scores (potential for improvement) 
charts = [
    'classic91_simple_bar_chart',
    'classic93_line_chart',
    'classic94_pie_chart',
    'classic97_doughnut_chart',
    'classic100_stacked_bar_chart',
]

for name in charts:
    mini_path = os.path.join(mini_dir, f'{name}.pdf')
    ref_path = os.path.join(ref_dir, f'{name}.pdf')
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    print(f'\n=== {name} ===')
    print(f'  Pages: mini={len(mini_doc)}, ref={len(ref_doc)}')
    
    for pi in range(min(len(mini_doc), len(ref_doc))):
        mp = mini_doc[pi]
        rp = ref_doc[pi]
        
        def get_spans(page):
            data = page.get_text("dict", sort=True)
            spans = []
            for block in data.get("blocks", []):
                if block.get("type", 0) != 0:
                    continue
                for line in block.get("lines", []):
                    for span in line.get("spans", []):
                        text = span.get("text", "").strip()
                        if text:
                            spans.append((span["bbox"], text, span.get("size", 0)))
            return spans
        
        m_spans = get_spans(mp)
        r_spans = get_spans(rp)
        
        print(f'  P{pi+1}: mini_spans={len(m_spans)}, ref_spans={len(r_spans)}')
        
        m_text = [s[1] for s in m_spans]
        r_text = [s[1] for s in r_spans]
        
        m_set = set(m_text)
        r_set = set(r_text)
        
        missing = r_set - m_set
        extra = m_set - r_set
        
        if missing:
            print(f'  Missing from mini: {sorted(missing)[:10]}')
        if extra:
            print(f'  Extra in mini: {sorted(extra)[:10]}')

        # Show all ref text
        print(f'  Ref text: {r_text[:20]}')
        print(f'  Mini text: {m_text[:20]}')
    
    mini_doc.close()
    ref_doc.close()
