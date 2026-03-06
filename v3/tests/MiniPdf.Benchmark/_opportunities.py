"""Analyze specific improvement opportunities for remaining failures."""
import json
with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

below = [r for r in data if r.get('overall_score',0) < 0.99]
below.sort(key=lambda r: r['overall_score'], reverse=True)

# Group by improvement category
text_trunc = []   # text truncation / merging
num_fmt = []      # number formatting
visual_only = []  # text ok, visual different
chart = []        # chart issues
other = []        # other

for r in below:
    name = r['name']
    ts = r.get('text_similarity', 0)
    vs = r.get('visual_avg', 0)
    diff = r.get('text_diff', '')
    
    if 'chart' in name.lower() or 'pie' in name.lower() or 'bar' in name.lower() or 'line' in name.lower() or 'scatter' in name.lower() or 'doughnut' in name.lower() or 'radar' in name.lower() or 'area' in name.lower() or 'bubble' in name.lower() or 'combo' in name.lower() or 'stock' in name.lower():
        chart.append(r)
    elif ts >= 0.995:
        visual_only.append(r)
    elif 'E+' in diff or 'E-' in diff or '-100000' in diff or '-99999' in diff:
        num_fmt.append(r)
    elif ts < 0.99:
        text_trunc.append(r)
    else:
        other.append(r)

print(f"=== Remaining below 0.99: {len(below)} ===")
print(f"  Chart: {len(chart)}")
print(f"  Visual-only (text>=0.995): {len(visual_only)}")
print(f"  Number formatting: {len(num_fmt)}")
print(f"  Text truncation/merging: {len(text_trunc)}")
print(f"  Other: {len(other)}")

# Show the quickest wins (score > 0.97)
print(f"\n=== Quick wins (score > 0.97) ===")
for r in below:
    if r['overall_score'] > 0.97:
        ts = r.get('text_similarity', 0)
        vs = r.get('visual_avg', 0)
        print(f"  {r['overall_score']:.4f} t={ts:.4f} v={vs:.4f} {r['name']}")

print(f"\n=== Cases where small text fix would push over 0.99 ===")
# If text went to 1.0, what would score be?
for r in below:
    vs = r.get('visual_avg', 0)
    ps = 1.0 if r.get('minipdf_pages') == r.get('reference_pages') else 0.5
    potential = 1.0 * 0.4 + vs * 0.4 + ps * 0.2
    if potential >= 0.99 and r['overall_score'] < 0.99:
        ts = r.get('text_similarity', 0)
        print(f"  {r['overall_score']:.4f} → {potential:.4f} (text {ts:.4f}→1.0) {r['name']}")
