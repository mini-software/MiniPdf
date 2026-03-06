"""Check axis label formatting in reference PDFs to understand comma usage patterns."""
import fitz
import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
chart_cases = [x for x in data if x['text_similarity'] < 0.97 
               and ('chart' in x['name'] or 'bar' in x['name'] or 'line' in x['name'] 
                    or 'area' in x['name'] or 'pie' in x['name'] or 'scatter' in x['name']
                    or 'radar' in x['name'] or 'bubble' in x['name'] or 'stock' in x['name']
                    or 'combo' in x['name'] or 'stacked' in x['name'] or 'dashboard' in x['name'])]

for r in sorted(chart_cases, key=lambda x: x['text_similarity'])[:15]:
    name = r['name']
    ref_path = f'reference_pdfs/{name}.pdf'
    mini_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
    
    try:
        ref_doc = fitz.open(ref_path)
        mini_doc = fitz.open(mini_path)
        
        ref_text = '\n'.join(p.get_text().strip() for p in ref_doc)
        mini_text = '\n'.join(p.get_text().strip() for p in mini_doc)
        
        # Find numbers with commas in reference
        import re
        ref_comma_nums = re.findall(r'\d{1,3}(?:,\d{3})+', ref_text)
        mini_comma_nums = re.findall(r'\d{1,3}(?:,\d{3})+', mini_text)
        
        # Find words in ref not in mini and vice versa
        ref_words = set(ref_text.split())
        mini_words = set(mini_text.split())
        ref_only = ref_words - mini_words
        mini_only = mini_words - ref_words
        
        print(f"\n{name}: txt={r['text_similarity']:.3f}")
        if ref_comma_nums:
            print(f"  REF commas: {ref_comma_nums[:10]}")
        if mini_comma_nums:
            print(f"  MINI commas: {mini_comma_nums[:10]}")
        if ref_only:
            sorted_ref = sorted(ref_only)[:15]
            print(f"  REF only ({len(ref_only)}): {sorted_ref}")
        if mini_only:
            sorted_mini = sorted(mini_only)[:15]
            print(f"  MINI only ({len(mini_only)}): {sorted_mini}")
        
        ref_doc.close()
        mini_doc.close()
    except Exception as e:
        print(f"  {name}: Error: {e}")
