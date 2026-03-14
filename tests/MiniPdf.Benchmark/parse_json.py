import json

with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

# data is a list
if data:
    print(f"Total: {len(data)} cases")
    print(f"Keys: {list(data[0].keys())}")
    print(f"Sample: {data[0]}")
