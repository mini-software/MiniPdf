"""Analyze 1pt cap (no subtraction) results."""
import json

data = json.load(open('reports/cap1_nofull_report.json', encoding='utf-8'))
data.sort(key=lambda r: r['overall_score'])

print("25 worst cases with 1pt padding cap (no subtraction):")
for r in data[:25]:
    name = r['name']
    ts = r['text_similarity']
    va = r.get('visual_avg', 0)
    ov = r['overall_score']
    mp = r.get('minipdf_pages', '?')
    rp = r.get('reference_pages', '?')
    print(f"  {name:45s} text={ts:.4f} vis={va:.4f} overall={ov:.4f} pages={mp}/{rp}")
