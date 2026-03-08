import json
d = json.load(open("reports/comparison_report.json", encoding="utf-8"))
below = [x for x in d if x["overall_score"] < 0.99]
avg = sum(x["overall_score"] for x in d) / len(d)
print(f"Below99={len(below)}, Avg={avg:.4f}")
page_diff = [x for x in d if x.get("minipdf_pages",0) != x.get("reference_pages",0)]
print(f"Page mismatches: {len(page_diff)}")
