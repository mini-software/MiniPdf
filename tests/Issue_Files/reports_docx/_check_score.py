import json
data = json.load(open('comparison_report.json', encoding='utf-8'))
for item in data:
    name = item.get('file', item.get('name', ''))
    if 'SA8000' in name or 'sa8000' in name.lower():
        print(f"{name}:")
        for k, v in item.items():
            print(f"  {k}: {v}")
