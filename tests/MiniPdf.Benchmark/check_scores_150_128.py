import json
with open('reports/comparison_report.json') as f:
    data = json.load(f)
for c in data:
    if '150' in c['name'] or '128' in c['name']:
        print(f"{c['name']}: text={c['text_similarity']:.4f} vis={c['visual_score']:.4f} overall={c['overall']:.4f}")
