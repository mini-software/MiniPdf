import fitz, json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Find closest non-chart cases where text < 0.99
text_fixable = []
chart_kws = ['chart', 'pie', 'doughnut', 'radar', 'bubble', 'scatter', 'bar_chart', 'line_chart', 'area_chart', 'combo', 'sparkline']
for d2 in data:
    os2 = d2.get('overall_score')
    ts = d2.get('text_similarity', 0) or 0
    if os2 is None or os2 >= 0.99:
        continue
    if any(k in d2['name'] for k in chart_kws):
        continue
    if ts >= 0.99:
        continue
    text_fixable.append(d2)

text_fixable.sort(key=lambda x: -x.get('text_similarity', 0))
print(f"Non-chart cases with text < 0.99: {len(text_fixable)}")
print()

# For top 5, show text diffs
for d2 in text_fixable[:8]:
    name = d2['name']
    ts = d2.get('text_similarity', 0)
    print(f"=== {name}: text={ts:.4f} ===")
    
    mp_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
    ref_path = f'reference_pdfs/{name}.pdf'
    try:
        mp_doc = fitz.open(mp_path)
        ref_doc = fitz.open(ref_path)
        
        mp_text = '\n'.join(p.get_text() for p in mp_doc)
        ref_text = '\n'.join(p.get_text() for p in ref_doc)
        
        mp_words = mp_text.split()
        ref_words = ref_text.split()
        
        # Find first 3 differences
        diffs_found = 0
        ref_set = set(ref_words)
        mp_set = set(mp_words)
        
        # Words in reference but not MiniPdf
        missing = [w for w in ref_words if w not in mp_set][:5]
        # Words in MiniPdf but not reference
        extra = [w for w in mp_words if w not in ref_set][:5]
        
        if missing:
            print(f"  Missing from MiniPdf: {missing}")
        if extra:
            print(f"  Extra in MiniPdf: {extra}")
        print(f"  MiniPdf words: {len(mp_words)}, Reference words: {len(ref_words)}")
        
        mp_doc.close()
        ref_doc.close()
    except Exception as e:
        print(f"  ERROR: {e}")
    print()
