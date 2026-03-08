"""Quick check pie/doughnut scores."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for r in d:
    if 'pie' in r['name'] or 'doughnut' in r['name']:
        print(f"  t={r.get('text_similarity',1.0):.4f} o={r['overall_score']:.4f} {r['name']}")
