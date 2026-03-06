import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Show per-page visual scores for chart cases with low visual similarity
targets = [
    'classic100_stacked_bar_chart', 'classic101_percent_stacked_bar',
    'classic105_3d_bar_chart', 'classic108_stacked_area_chart',
    'classic116_percent_stacked_area', 'classic104_combo_bar_line_chart',
    'classic107_multi_series_line', 'classic110_chart_with_legend',
    'classic112_multiple_charts', 'classic117_stock_ohlc_chart',
    'classic113_chart_sheet', 'classic96_scatter_chart'
]

for name in targets:
    r = [x for x in data if x['name'] == name]
    if not r: continue
    r = r[0]
    vs = r.get('visual_scores', [])
    print(f"{name}: avg={r['visual_avg']:.3f} pages={[f'{v:.3f}' for v in vs]}")
