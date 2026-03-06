import json

# Previous scores (from before the desiredTicks change)
prev = {
    "classic91": 0.9669, "classic118": 0.9457, "classic115": 0.9380,
    "classic100": 0.9306, "classic93": 0.9286, "classic95": 0.7328,
    "classic102": 0.9141, "classic97": 0.9132, "classic106": 0.9129,
    "classic108": 0.9123, "classic96": 0.8698, "classic116": 0.8622,
    "classic101": 0.8598, "classic92": 0.8429, "classic105": 0.8425,
    "classic111": 0.8376, "classic99": 0.8364, "classic110": 0.8289,
    "classic103": 0.8232, "classic109": 0.8220, "classic113": 0.8185,
    "classic104": 0.8145, "classic107": 0.8001, "classic117": 0.7997,
    "classic120": 0.7633, "classic114": 0.9098, "classic119": 0.9219,
    "classic98": 0.9097, "classic94": 0.9063, "classic112": 0.8473,
}

with open("reports/comparison_report.json") as f:
    results = json.load(f)

print(f"{'Case':<45} {'Before':>8} {'After':>8} {'Delta':>8}")
print("-" * 75)
for r in sorted(results, key=lambda x: x["name"]):
    name = r["name"]
    short = name.split("_")[0]
    if short in prev:
        before = prev[short]
        after = r["overall_score"]
        delta = after - before
        marker = "+" if delta > 0.001 else ("-" if delta < -0.001 else " ")
        print(f"{marker} {name:<43} {before:>8.4f} {after:>8.4f} {delta:>+8.4f}")
