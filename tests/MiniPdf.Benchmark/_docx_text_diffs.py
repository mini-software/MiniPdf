import json

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
if isinstance(data, list):
    results = data
else:
    results = data['results']

# Look at text diffs for the text-failing cases
text_fail = [r for r in results if r.get('text_similarity', 1) < 0.99 and r['overall_score'] < 0.99]
text_fail.sort(key=lambda r: r.get('text_similarity', 0), reverse=True)

print(f'Cases with text < 0.99: {len(text_fail)}\n')
for r in text_fail[:15]:
    name = r.get('name', '')
    ts = r.get('text_similarity', 0)
    td = r.get('text_diff', '')
    print(f'=== {name}: text={ts:.4f} ===')
    # Show first 500 chars of diff
    if td:
        lines = td.split('\n')
        for line in lines[:20]:
            print(f'  {line}')
    print()
