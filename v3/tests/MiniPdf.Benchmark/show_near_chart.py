import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

names = ['classic91_simple_bar_chart', 'classic92_horizontal_bar_chart',
         'classic93_line_chart', 'classic102_line_chart_with_markers',
         'classic118_bar_chart_custom_colors']

for name in names:
    r = [x for x in data if x['name'] == name][0]
    ts = r['text_similarity']
    vs = r['visual_avg']
    print(f'\n{"="*60}')
    print(f'{name}: txt={ts:.3f} vis={vs:.3f}')
    print(f'{"="*60}')
    diff = r.get('text_diff', '')
    print(diff[:2000])
