import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for r in sorted(data, key=lambda x: x['overall_score']):
    o = r['overall_score']
    if 0.990 <= o <= 0.996:
        print(f"{r['name']}: {o:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
