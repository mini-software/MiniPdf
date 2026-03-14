"""Identify improvement opportunities by analyzing score gaps.
Focus on cases where we can realistically improve."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Categories of cases
chart_words = [
    'chart', 'pie', 'bar', 'line', 'area', 'donut', 'radar', 'scatter',
    'combo', 'stacked', 'series', 'histogram', 'waterfall', 'treemap',
    'sunburst', 'funnel', 'stock', 'bubble', 'percent', 'ohlc',
    'dashboard_multi', '3d_',
]
unicode_words = [
    'cjk', 'emoji', 'rtl', 'bidi', 'indic', 'korean', 'african',
    'musical', 'southeast_asian', 'multilingual', 'multiscript',
    'polyglot', 'zwj', 'caucasus', 'combining', 'ipa_phonetic',
    'box_drawing', 'cyrillic',
]

improvable = []
for c in d:
    name = c['name']
    vis = c.get('visual_avg', 0)
    text = c.get('text_similarity', 0)
    mp = c.get('minipdf_pages', 0)
    rp = c.get('reference_pages', 0)
    overall = c.get('overall_score', 0)
    
    is_chart = any(w in name.lower() for w in chart_words)
    is_unicode = any(w in name.lower() for w in unicode_words)
    
    if is_chart or is_unicode:
        continue
    
    # Cases with visual score < 0.99 — potential visual improvement
    if vis < 0.99:
        gap = 0.99 - vis
        improvable.append((gap, name, vis, text, mp, rp, overall))

improvable.sort(reverse=True)
print(f"Non-chart, non-unicode cases with visual < 0.99:")
print(f"{'Name':55s} {'vis':>7s} {'text':>7s} {'pages':>8s} {'overall':>8s} {'gap':>7s}")
for gap, name, vis, text, mp, rp, overall in improvable:
    pages = f"{mp}/{rp}"
    print(f"  {name:55s} {vis:7.4f} {text:7.4f} {pages:>8s} {overall:8.4f} {gap:7.4f}")

total_gap = sum(g for g, *_ in improvable)
print(f"\nTotal gap: {total_gap:.4f}")
print(f"Potential avg improvement if all fixed to 0.99:")
print(f"  {total_gap * 0.4 / 191:.6f} per case = {total_gap * 0.4:.4f} total visual improvement")
