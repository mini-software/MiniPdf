import json
with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
print(type(data))
if isinstance(data, list):
    results = data
elif isinstance(data, dict):
    print(list(data.keys()))
    results = data.get('results', data.get('comparisons', []))
else:
    results = []
for r in results:
    name = str(r.get('name', r.get('file', '')))
    if 'nthu_article' == name:
        for k, v in r.items():
            if 'page' not in k.lower():
                print(f"{k}: {v}")
        break
