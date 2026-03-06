import json
with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
targets = ['classic42','classic13','classic128','classic140','classic51','classic70','classic73','classic40','classic49','classic23','classic90']
for r in data:
    name = r['name']
    if any(name.startswith(t) for t in targets):
        o = r['overall_score']
        t = r['text_similarity']
        v = r['visual_avg']
        print(f"{name}: {o:.4f} text={t:.4f} vis={v:.4f}")
