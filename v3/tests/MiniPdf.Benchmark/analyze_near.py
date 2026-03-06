"""Analyze text differences for near-threshold cases (0.97-0.99)."""
import fitz
import json
import difflib

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
near = [x for x in data if 0.97 <= x['overall_score'] < 0.99]
near.sort(key=lambda x: x['overall_score'], reverse=True)

for r in near[:12]:
    name = r['name']
    txt_sim = r['text_similarity']
    vis_avg = r['visual_avg']
    
    mini_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
    ref_path = f'reference_pdfs/{name}.pdf'
    
    try:
        mini_doc = fitz.open(mini_path)
        ref_doc = fitz.open(ref_path)
        
        # Extract text from first page
        mini_text = mini_doc[0].get_text().strip().split('\n')
        ref_text = ref_doc[0].get_text().strip().split('\n')
        
        # Show unified diff (just the differences)
        diff = list(difflib.unified_diff(ref_text, mini_text, n=0, lineterm=''))
        if diff:
            print(f"\n{name}: o={r['overall_score']:.4f} txt={txt_sim:.3f} vis={vis_avg:.3f}")
            # Show first 15 diff lines
            for line in diff[2:17]:  # skip --- and +++ headers
                print(f"  {line}")
            if len(diff) > 17:
                print(f"  ... ({len(diff)-2} diff lines total)")
        
        mini_doc.close()
        ref_doc.close()
    except Exception as e:
        print(f"  {name}: Error: {e}")
