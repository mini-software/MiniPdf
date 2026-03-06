"""Analyze chart text differences for chart cases below 0.99."""
import json
with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

# Chart cases below 0.99 sorted by score
chart_names = ['chart', 'pie', 'bar', 'line', 'scatter', 'doughnut', 'radar', 
               'area', 'bubble', 'combo', 'stock']
chart_cases = [r for r in data if r.get('overall_score',0) < 0.99 and 
               any(cn in r['name'].lower() for cn in chart_names)]
chart_cases.sort(key=lambda r: r['overall_score'], reverse=True)

print(f"Chart cases below 0.99: {len(chart_cases)}")
print(f"\n=== Top chart cases (closest to 0.99) ===")
for r in chart_cases[:15]:
    ts = r.get('text_similarity', 0)
    vs = r.get('visual_avg', 0)
    diff = r.get('text_diff', '')
    # Count diff lines
    diff_lines = [l for l in diff.split('\n') if l.startswith('+') or l.startswith('-')]
    diff_lines = [l for l in diff_lines if not l.startswith('---') and not l.startswith('+++')]
    print(f"\n  {r['overall_score']:.4f} t={ts:.4f} v={vs:.4f} {r['name']}")
    for dl in diff_lines[:6]:
        print(f"    {dl[:100]}")
    if len(diff_lines) > 6:
        print(f"    ... ({len(diff_lines)} total diff lines)")
