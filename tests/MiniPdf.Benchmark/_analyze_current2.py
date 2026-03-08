import json

# Analyze DOCX report
with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    docx = json.load(f)

scores = [c['overall_score'] for c in docx]
below99 = [c for c in docx if c['overall_score'] < 0.99]

lines = []
lines.append(f"=== DOCX Report ===")
lines.append(f"Total: {len(docx)}, Below99: {len(below99)}, Avg: {sum(scores)/len(scores):.4f}")

# Cases with text < 0.99 (fixable)
txt_fix = [c for c in below99 if c['text_similarity'] < 0.99]
lines.append(f"\nText < 0.99 ({len(txt_fix)} cases):")
for c in sorted(txt_fix, key=lambda x: x['text_similarity']):
    name = c['name'].replace('docx_', '')
    lines.append(f"  {name:55s} t={c['text_similarity']:.4f} v={c['visual_avg']:.4f} o={c['overall_score']:.4f}")

vis_only = [c for c in below99 if c['text_similarity'] >= 0.99]
lines.append(f"\nVisual-only issues (text>=0.99): {len(vis_only)} cases")

# XLSX
lines.append(f"\n=== XLSX Report ===")
with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    xlsx = json.load(f)

scores2 = [c['overall_score'] for c in xlsx]
below99x = [c for c in xlsx if c['overall_score'] < 0.99]
lines.append(f"Total: {len(xlsx)}, Below99: {len(below99x)}, Avg: {sum(scores2)/len(scores2):.4f}")

txt_fix2 = [c for c in below99x if c['text_similarity'] < 0.99]
lines.append(f"\nText < 0.99 ({len(txt_fix2)} cases):")
for c in sorted(txt_fix2, key=lambda x: x['text_similarity']):
    name = c['name']
    lines.append(f"  {name:55s} t={c['text_similarity']:.4f} v={c['visual_avg']:.4f} o={c['overall_score']:.4f}")

vis_only2 = [c for c in below99x if c['text_similarity'] >= 0.99]
lines.append(f"\nVisual-only issues (text>=0.99): {len(vis_only2)} cases")

# Categorize XLSX text failures
chart_cases = [c for c in txt_fix2 if 'chart' in c['name'].lower() or 'pie' in c['name'] or 'bar' in c['name'] or 'line' in c['name'] or 'scatter' in c['name'] or 'area' in c['name'] or 'bubble' in c['name'] or 'radar' in c['name'] or 'doughnut' in c['name'] or 'combo' in c['name'] or 'ohlc' in c['name'] or 'stock' in c['name']]
unicode_cases = [c for c in txt_fix2 if any(k in c['name'] for k in ['cjk', 'emoji', 'unicode', 'rtl', 'bidi', 'indic', 'korean', 'african', 'zwj', 'musical', 'polyglot', 'multilingual', 'punctuation', 'combining', 'box_drawing', 'math_symbol', 'currency', 'ipa', 'technical', 'southeast', 'caucasus', 'ethiopic', 'multiscript', 'mixed_ltr'])]
other_cases = [c for c in txt_fix2 if c not in chart_cases and c not in unicode_cases]

lines.append(f"\n  Chart-related: {len(chart_cases)}")
lines.append(f"  Unicode/CJK/Emoji: {len(unicode_cases)}")
lines.append(f"  Other: {len(other_cases)}")
for c in other_cases:
    lines.append(f"    {c['name']:55s} t={c['text_similarity']:.4f}")

with open('_report.txt', 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines))
print("Written to _report.txt")
