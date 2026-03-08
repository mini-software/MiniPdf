import json
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
print(type(data))
if isinstance(data, dict):
    print(list(data.keys())[:5])
elif isinstance(data, list):
    print(f'list of {len(data)}')
    if len(data) > 0:
        print(list(data[0].keys())[:10])
