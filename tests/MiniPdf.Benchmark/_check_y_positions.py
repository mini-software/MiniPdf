"""Compare row positions for multiple files to check descent accuracy."""
import fitz, json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
# Check files that are near-misses with text=1.0 (pure visual)
targets = [r['name'] for r in data if r['text_similarity'] == 1.0 and 0.97 <= r['overall_score'] < 0.99]

for name in targets[:5]:
    ref_path = f"reference_pdfs/{name}.pdf"
    mini_path = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
    try:
        ref_doc = fitz.open(ref_path)
        mini_doc = fitz.open(mini_path)
        ref_page = ref_doc[0]
        mini_page = mini_doc[0]
        ref_data = ref_page.get_text('dict', sort=True)
        mini_data = mini_page.get_text('dict', sort=True)
        
        ref_spans = []
        for b in ref_data.get('blocks', []):
            if b.get('type', 0) != 0: continue
            for l in b.get('lines', []):
                for s in l.get('spans', []):
                    if s.get('text', '').strip():
                        ref_spans.append(round(s['origin'][1], 2))
        
        mini_spans = []
        for b in mini_data.get('blocks', []):
            if b.get('type', 0) != 0: continue
            for l in b.get('lines', []):
                for s in l.get('spans', []):
                    if s.get('text', '').strip():
                        mini_spans.append(round(s['origin'][1], 2))
        
        ref_first = min(ref_spans) if ref_spans else 0
        mini_first = min(mini_spans) if mini_spans else 0
        delta = mini_first - ref_first
        print(f"{name}: REF_Y1={ref_first:.1f} MINI_Y1={mini_first:.1f} delta={delta:+.1f}")
        
        ref_doc.close()
        mini_doc.close()
    except Exception as e:
        print(f"{name}: ERROR - {e}")
