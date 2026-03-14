"""Find non-chart cases where visual improvement is possible."""
import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

print("Non-chart cases sorted by visual gap (vis < 0.98):")
print(f"{'Case':45s} {'text':>8s} {'vis':>8s} {'overall':>8s} {'pages':>6s} {'vis_gain':>8s}")
for r in sorted(d, key=lambda x: x.get('visual_avg', 0)):
    name = r['name']
    ts = r['text_similarity']
    va = r.get('visual_avg', 0)
    ov = r['overall_score']
    mp = r.get('minipdf_pages', '?')
    rp = r.get('reference_pages', '?')
    
    # Skip if chart case (91-120)
    num = ''.join(c for c in name if c.isdigit())
    num = int(num) if num else 0
    if 91 <= num <= 120:
        continue
    
    if va < 0.98:
        vis_gain = (1.0 - va) * 0.4 / 191  # potential gain to avg score
        pages = f"{mp}/{rp}"
        print(f"{name:45s} {ts:8.4f} {va:8.4f} {ov:8.4f} {pages:>6s} {vis_gain:8.6f}")
