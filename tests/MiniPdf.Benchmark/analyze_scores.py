import json

with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

# Sort by visual_avg ascending
items = sorted(data, key=lambda x: x["visual_avg"])

print("=== Lowest visual scores (non-chart focus) ===")
for item in items[:30]:
    n = item["name"]
    vis = item["visual_avg"]
    text = item["text_similarity"]
    mp = item["minipdf_pages"]
    rp = item["reference_pages"]
    score = item["overall_score"]
    chart = "chart" in n.lower()
    tag = " [CHART]" if chart else ""
    print(f"  vis={vis:.4f} text={text:.4f} pages={mp}/{rp} score={score:.4f}  {n}{tag}")

print("\n=== Lowest text similarity (excluding charts) ===")
non_chart = [i for i in data if "chart" not in i["name"].lower()]
for item in sorted(non_chart, key=lambda x: x["text_similarity"])[:15]:
    n = item["name"]
    vis = item["visual_avg"]
    text = item["text_similarity"]
    mp = item["minipdf_pages"]
    rp = item["reference_pages"]
    score = item["overall_score"]
    print(f"  text={text:.4f} vis={vis:.4f} pages={mp}/{rp} score={score:.4f}  {n}")
