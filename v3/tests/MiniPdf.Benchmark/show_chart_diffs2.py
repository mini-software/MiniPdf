import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

names = ['classic117_stock_ohlc_chart', 'classic104_combo_bar_line_chart',
         'classic113_chart_sheet', 'classic95_area_chart', 'classic120_chart_with_date_axis',
         'classic107_multi_series_line', 'classic110_chart_with_legend']

for name in names:
    r = [x for x in data if x['name'] == name][0]
    ts = r['text_similarity']
    vs = r['visual_avg']
    print(f'\n{"="*60}')
    print(f'{name}: txt={ts:.3f} vis={vs:.3f}')
    print(f'{"="*60}')
    diff = r.get('text_diff', '')
    print(diff[:1500])
