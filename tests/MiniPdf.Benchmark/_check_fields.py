import json
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
print(list(data[0].keys()))
print()
# Print one sample entry
for k, v in data[0].items():
    print(f'  {k}: {v}')
