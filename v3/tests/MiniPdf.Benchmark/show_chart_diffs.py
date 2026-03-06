import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Show text diffs for worst chart cases
cases = ['classic95_area_chart', 'classic110_chart_with_legend', 'classic111_chart_with_axis_labels',
         'classic120_chart_with_date_axis', 'classic103_pie_chart_with_labels',
         'classic93_line_chart', 'classic102_line_chart_with_markers',
         'classic92_horizontal_bar_chart', 'classic104_combo_bar_line_chart',
         'classic109_scatter_with_trendline', 'classic112_multiple_charts']

for name in cases:
    r = [x for x in data if x['name'] == name]
    if not r:
        continue
    r = r[0]
    diff = r.get('text_diff', '')
    print(f"\n{'='*60}")
    print(f"{name}: txt={r['text_similarity']:.3f} vis={r['visual_avg']:.3f}")
    print(f"{'='*60}")
    # Show just the unified diff (first 1500 chars)
    print(diff[:1500])
