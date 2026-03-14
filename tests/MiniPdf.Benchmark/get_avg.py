import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
scores = [r['overall_score'] for r in d]
avg = sum(scores) / len(scores)
print(f'Average Score: {avg:.6f} (n={len(scores)})')
