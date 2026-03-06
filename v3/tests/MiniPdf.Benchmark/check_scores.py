import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

if isinstance(data, list):
    cases = data
else:
    cases = data['test_cases']
below99 = [(c['name'], c['overall_score'], c.get('text_similarity', 0), c.get('visual_avg', 0)) for c in cases if c['overall_score'] < 0.99]
below99.sort(key=lambda x: x[1])

above = len(cases) - len(below99)
avg = sum(c['overall_score'] for c in cases) / len(cases)
print(f'Total: {len(cases)}, At/above 0.99: {above}, Below 0.99: {len(below99)}, Avg: {avg:.4f}')
print()
for name, score, txt, vis in below99:
    print(f'  {score:.4f}  txt={txt:.3f} vis={vis:.3f}  {name}')
