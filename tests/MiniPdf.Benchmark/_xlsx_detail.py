import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Categorize below-99 cases
below99 = []
for d2 in data:
    os2 = d2.get('overall_score')
    if os2 is None or os2 >= 0.99:
        continue
    ts = d2.get('text_similarity', 0) or 0
    va = d2.get('visual_avg', 0) or 0
    ps = 1.0 if d2.get('minipdf_pages') == d2.get('reference_pages') else 0.0
    below99.append({
        'name': d2['name'],
        'overall': os2,
        'text': ts,
        'visual': va,
        'page': ps,
        'text_gap': max(0, 0.99 - ts * 0.4 - va * 0.4 - ps * 0.2),
        'mp_pages': d2.get('minipdf_pages', 0),
        'ref_pages': d2.get('reference_pages', 0),
    })

# Sort by overall score (closest to 99 first = easiest to fix)
below99.sort(key=lambda x: -x['overall'])

print("=== Top 30 closest to 99 (easiest to fix) ===")
print(f"{'Name':45s} {'Overall':>8s} {'Text':>6s} {'Visual':>7s} {'Page':>5s}")
for c in below99[:30]:
    marker = " PAGE!" if c['page'] < 1 else ""
    print(f"{c['name']:45s} {c['overall']:8.4f} {c['text']:6.4f} {c['visual']:7.4f} {c['mp_pages']:2d}/{c['ref_pages']:2d}{marker}")

# Identify pattern groups
print("\n=== Page mismatches (most expensive) ===")
for c in below99:
    if c['page'] < 1:
        print(f"  {c['name']}: {c['mp_pages']} vs {c['ref_pages']} pages, overall={c['overall']:.4f}")

# Text-only failures (visual >= 0.99, text < 0.99)
text_only = [c for c in below99 if c['visual'] >= 0.99 and c['text'] < 0.99]
print(f"\n=== Text-only failures ({len(text_only)}) ===")
for c in text_only[:15]:
    print(f"  {c['name']}: text={c['text']:.4f} vis={c['visual']:.4f}")

# Visual-only failures (text >= 0.99, visual < 0.99)
vis_only = [c for c in below99 if c['text'] >= 0.99 and c['visual'] < 0.99]
print(f"\n=== Visual-only failures ({len(vis_only)}) ===")
for c in vis_only[:15]:
    print(f"  {c['name']}: text={c['text']:.4f} vis={c['visual']:.4f}")

# Both text and visual
both = [c for c in below99 if c['text'] < 0.99 and c['visual'] < 0.99]
print(f"\n=== Both text+visual ({len(both)}) ===")
for c in both[:15]:
    print(f"  {c['name']}: text={c['text']:.4f} vis={c['visual']:.4f}")

# Chart cases
chart_cases = [c for c in below99 if any(k in c['name'] for k in ['chart', 'pie', 'doughnut', 'radar', 'bubble', 'scatter', 'bar', 'line', 'area', 'combo'])]
non_chart = [c for c in below99 if c not in chart_cases]
print(f"\n=== Chart cases: {len(chart_cases)}, Non-chart: {len(non_chart)} ===")
print("Non-chart cases closest to 99:")
for c in sorted(non_chart, key=lambda x: -x['overall'])[:15]:
    print(f"  {c['name']}: overall={c['overall']:.4f} text={c['text']:.4f} vis={c['visual']:.4f}")
