import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
below = [x for x in data if x['overall_score'] < 0.99]
below.sort(key=lambda x: x['overall_score'])
for x in below:
    n = x['name']
    o = x['overall_score']
    t = x['text_similarity']
    v = x['visual_avg']
    print(f"{n:45s} {o:.4f} txt={t:.4f} vis={v:.4f}")
print(f"\nTotal below 0.99: {len(below)}")
