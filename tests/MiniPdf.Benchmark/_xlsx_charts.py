import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data.get('results', data)

chart_words = ['chart', 'pie', 'bar_graph', 'line_graph', 'scatter', 'donut', 'doughnut', 
               'area', 'bubble', 'combo', 'stacked', 'histogram', 'waterfall']

chart_cases = []
for r in results:
    name = r.get('name', r.get('file', '')).replace('.pdf','')
    is_chart = any(c in name.lower() for c in chart_words)
    if is_chart:
        chart_cases.append(r)

below99_charts = [r for r in chart_cases if r['overall_score'] < 0.99]
below99_charts.sort(key=lambda r: r['overall_score'], reverse=True)

print(f'Total chart cases: {len(chart_cases)}')
print(f'Below 99 chart cases: {len(below99_charts)}')
print(f'Above 99 chart cases: {len(chart_cases) - len(below99_charts)}')

def get_vis(r):
    return r.get('visual_avg', r.get('visual_similarity', 0))

print('\n--- Chart cases closest to 99 ---')
for r in below99_charts[:15]:
    name = r.get('name', '').replace('.pdf','')
    ts = r.get('text_similarity', 0)
    vs = get_vis(r)
    ov = r['overall_score']
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f}')

print('\n--- Worst chart cases ---')
for r in below99_charts[-10:]:
    name = r.get('name', '').replace('.pdf','')
    ts = r.get('text_similarity', 0)
    vs = get_vis(r)
    ov = r['overall_score']
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f}')
