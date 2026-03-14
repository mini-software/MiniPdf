import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
targets = ['classic04_', 'classic09_', 'classic10_', 'classic11_', 'classic19_', 'classic77_']
for c in d:
    for t in targets:
        if c['name'].startswith(t):
            name = c['name']
            text = c['text_similarity']
            vis = c['visual_avg']
            overall = c['overall_score']
            print(f'{name:50s} text={text:.4f} vis={vis:.4f} overall={overall:.4f}')
