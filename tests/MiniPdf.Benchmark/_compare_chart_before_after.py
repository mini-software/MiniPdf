"""Compare before/after XLSX benchmark results for chart cases."""
import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

# Current results (already updated)
with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    new_data = json.load(f)

new_map = {c['name']: c for c in new_data}

# Baseline from _report.txt embedded earlier
baseline = {
    'classic91_simple_bar_chart': 0.9493,
    'classic93_line_chart': 0.8257,
    'classic94_pie_chart': 0.8780,
    'classic95_area_chart': 0.6441,
    'classic96_scatter_chart': 0.8714,
    'classic97_doughnut_chart': 0.8571,
    'classic98_radar_chart': 0.8876,
    'classic99_bubble_chart': 0.8447,
    'classic100_stacked_bar_chart': 0.9663,
    'classic101_percent_stacked_bar': 0.9623,
    'classic102_line_chart_with_markers': 0.8372,
    'classic103_pie_chart_with_labels': 0.7368,
    'classic104_combo_bar_line_chart': 0.7872,
    'classic105_3d_bar_chart': 0.9032,
    'classic106_3d_pie_chart': 0.8243,
    'classic107_multi_series_line': 0.7375,
    'classic108_stacked_area_chart': 0.9643,
    'classic109_scatter_with_trendline': 0.8226,
    'classic110_chart_with_legend': 0.8372,
    'classic111_chart_with_axis_labels': 0.8267,
    'classic112_multiple_charts': 0.8750,
    'classic113_chart_sheet': 0.9259,
    'classic114_chart_large_dataset': 0.9015,
    'classic115_chart_negative_values': 0.8316,
    'classic116_percent_stacked_area': 0.9649,
    'classic117_stock_ohlc_chart': 0.7938,
    'classic118_bar_chart_custom_colors': 0.9275,
    'classic119_dashboard_multi_charts': 0.8409,
    'classic120_chart_with_date_axis': 0.6195,
    'classic92_horizontal_bar_chart': 0.9563,
}

# Summary
scores_new = [c['overall_score'] for c in new_data]
below99_new = [c for c in new_data if c['overall_score'] < 0.99]
text_fix_new = [c for c in below99_new if c['text_similarity'] < 0.99]

print(f"Total: {len(new_data)}, Below99: {len(below99_new)}, Avg: {sum(scores_new)/len(scores_new):.4f}")
print(f"Text<99: {len(text_fix_new)}")

print("\n=== Chart cases text score changes ===")
improved = 0
degraded = 0
for name, old_text in sorted(baseline.items()):
    if name in new_map:
        new_text = new_map[name]['text_similarity']
        diff = new_text - old_text
        marker = '✓' if diff > 0.001 else ('✗' if diff < -0.001 else '=')
        if diff > 0.001: improved += 1
        if diff < -0.001: degraded += 1
        new_overall = new_map[name]['overall_score']
        print(f"  {marker} {name:50s} text: {old_text:.4f} → {new_text:.4f} ({diff:+.4f})  overall: {new_overall:.4f}")

print(f"\nImproved: {improved}, Degraded: {degraded}")
