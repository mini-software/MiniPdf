import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
results = data if isinstance(data, list) else data.get('results', data)
mismatches = [(r['name'], r['minipdf_pages'], r['reference_pages']) for r in results if r.get('minipdf_pages', 0) != r.get('reference_pages', 0)]
print(f'Page count mismatches: {len(mismatches)}')
for n, m, r in mismatches:
    print(f'  {n}: MP={m}, REF={r}')
poor = [r for r in results if r['overall_score'] < 0.8]
print(f'Poor (<0.8): {len(poor)}')
for r in poor:
    print(f"  {r['name']}: {r['overall_score']}")
