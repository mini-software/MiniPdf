import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
charts = [r for r in d if any(kw in r['name'].lower() for kw in ['chart','pie','stacked','scatter','line_chart','area','doughnut','bar_chart','radar','bubble','stock','dashboard_multi'])]
for r in sorted(charts, key=lambda x: x['overall_score']):
    n = r['name']
    ts = r['text_similarity']
    vis = r.get('visual_avg', 0)
    sc = r['overall_score']
    print(f'{n:50s} text={ts:.4f} vis={vis:.4f} score={sc:.4f}')
