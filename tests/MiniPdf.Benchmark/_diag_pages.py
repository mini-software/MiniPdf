import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

for d in sorted(data, key=lambda x: x['visual_avg']):
    if d['visual_avg'] < 0.95 and d['text_similarity'] >= 0.99 and 'chart' not in d['name']:
        mp = d['minipdf_pages']
        rp = d['reference_pages']
        page_issue = ' *** PAGE MISMATCH' if mp != rp else ''
        name = d['name']
        vis = d['visual_avg']
        print(f'{name}: vis={vis:.4f} pages={mp}/{rp}{page_issue}')
        vs = d['visual_scores']
        for i, v in enumerate(vs[:5]):
            print(f'  page {i+1}: {v:.4f}')
