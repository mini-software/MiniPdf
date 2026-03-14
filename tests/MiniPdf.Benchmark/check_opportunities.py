"""Identify the biggest improvement opportunities by category."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Categorize by type
categories = {
    'chart': [],
    'unicode_cjk_emoji': [],
    'text_data': [],
}

for c in d:
    name = c['name']
    ts = max(c['text_similarity'], c.get('flat_text_similarity', 0), c.get('word_text_similarity', 0))
    vs = c['visual_avg']
    overall = c['overall_score']
    gap = 1.0 - overall
    
    if gap < 0.01:
        continue  # Already near perfect
    
    if any(x in name for x in ['chart', 'pie', 'bar', 'line', 'scatter', 'area', 'radar', 'bubble',
                                 'doughnut', 'stock', 'combo', 'dashboard', 'stacked']):
        categories['chart'].append((name, ts, vs, gap))
    elif any(x in name for x in ['cjk', 'emoji', 'unicode', 'korean', 'indic', 'rtl', 'bidi',
                                    'african', 'musical', 'zwj', 'multilingual', 'polyglot',
                                    'multiscript', 'punctuation', 'box_drawing', 'combining',
                                    'math_symbol', 'technical', 'ethiopic', 'caucasus',
                                    'southeast_asian', 'mixed_ltr']):
        categories['unicode_cjk_emoji'].append((name, ts, vs, gap))
    else:
        categories['text_data'].append((name, ts, vs, gap))

for cat, items in categories.items():
    items.sort(key=lambda x: -x[3])
    total_gap = sum(g for _, _, _, g in items)
    print(f"\n=== {cat} ({len(items)} cases, total gap={total_gap:.4f}) ===")
    for name, ts, vs, gap in items[:10]:
        bottleneck = "TEXT" if ts < vs else "VIS"
        print(f"  {name}: text={ts:.4f} vis={vs:.4f} gap={gap:.4f} [{bottleneck}]")
