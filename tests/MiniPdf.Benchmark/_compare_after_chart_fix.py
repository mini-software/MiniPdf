"""Compare current XLSX report with baseline to find improvements/regressions."""
import json, sys

sys.stdout.reconfigure(encoding='utf-8')

with open('reports/comparison_report.json', encoding='utf-8') as f:
    current = json.load(f)

# Build lookup by name
cur = {r['name']: r for r in current}

# Load baseline from _report.txt or just compare text_similarity changes
# For now, let's print all cases sorted by overall_score and text_similarity
improved = []
regressed = []

# Previous known scores from the session
baseline = {
    'classic94_pie_chart': {'text': 1.0000, 'overall': 1.0000},
    'classic97_doughnut_chart': {'text': 1.0000, 'overall': 1.0000},
    'classic106_3d_pie_chart': {'text': 0.9505, 'overall': 0.9669},
    'classic100_stacked_bar_chart': {'text': 0.9663, 'overall': 0.9495},
    'classic102_line_chart_with_markers': {'text': 0.8372, 'overall': 0.9302},
    'classic108_stacked_area_chart': {'text': 0.9643, 'overall': 0.9380},
    'classic93_line_chart': {'text': 0.8257, 'overall': 0.9247},
    'classic91_simple_bar_chart': {'text': 0.9493, 'overall': 0.9640},
    'classic95_area_chart': {'text': 0.6441, 'overall': 0.7637},
    'classic110_chart_with_legend': {'text': 0.8372, 'overall': 0.8466},
    'classic107_multi_series_line': {'text': 0.7375, 'overall': 0.8056},
}

print("=== Cases with known baseline ===")
for name, base in sorted(baseline.items()):
    if name in cur:
        r = cur[name]
        ts_diff = r.get('text_similarity', 1.0) - base['text']
        os_diff = r['overall_score'] - base['overall']
        if abs(ts_diff) > 0.001 or abs(os_diff) > 0.001:
            print(f"  {name}: text {base['text']:.4f} -> {r.get('text_similarity',1.0):.4f} ({ts_diff:+.4f}), overall {base['overall']:.4f} -> {r['overall_score']:.4f} ({os_diff:+.4f})")

# Show all chart cases current scores
print("\n=== All chart cases current scores ===")
chart_cases = [r for r in current if 'chart' in r['name'] or 'scatter' in r['name'] or 'bubble' in r['name']]
chart_cases.sort(key=lambda r: r.get('text_similarity', 1.0))
for r in chart_cases:
    print(f"  t={r.get('text_similarity',1.0):.4f} o={r['overall_score']:.4f}  {r['name']}")

# Overall stats
total = len(current)
below99 = sum(1 for r in current if r['overall_score'] < 0.99)
avg = sum(r['overall_score'] for r in current) / total
print(f"\nTotal: {total}, Below99: {below99}, Avg: {avg:.4f}")
