import json
with open('reports/comparison_report.json') as f:
    data = json.load(f)
targets = ['classic73','classic92','classic13','classic17','classic120','classic49','classic90','classic51','classic35','classic50','classic71','classic78']
for c in data:
    for t in targets:
        if c['name'].startswith(t):
            print(f"  {c['overall_score']:.4f} txt={c['text_similarity']:.3f} vis={c['visual_avg']:.3f}  {c['name']}")
