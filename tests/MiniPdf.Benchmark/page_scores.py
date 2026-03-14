import json

with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

targets = [
    "classic18_large_dataset",
    "classic06_tall_table", 
    "classic148_frozen_styled_grid",
    "classic60_large_wide_table",
    "classic191_payroll_calculator",
    "classic101_percent_stacked_bar",
]
for item in data:
    if item["name"] in targets:
        name = item["name"]
        vis_scores = item["visual_scores"]
        print(f"{name}: avg={sum(vis_scores)/len(vis_scores):.4f}")
        for i, vs in enumerate(vis_scores):
            tag = " <<< LOW" if vs < 0.93 else ""
            print(f"  Page {i+1}: {vs:.4f}{tag}")
        print()
