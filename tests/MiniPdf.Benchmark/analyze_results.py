import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

failing = []
for r in data:
    score = r.get('overall_score', 0)
    if score < 0.97:
        failing.append((r['name'], score, r.get('text_similarity', 0), r.get('visual_avg', 0)))

failing.sort(key=lambda x: x[1])
for name, s, ts, vs in failing:
    print(f'{name}: overall={s:.4f}  text={ts:.4f}  vis={vs:.4f}')
print(f'Total failing: {len(failing)}')
