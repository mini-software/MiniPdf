import json, sys

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# handle both dict-with-results and list formats
if isinstance(data, list):
    results = data
elif isinstance(data, dict):
    results = data.get('results', data.get('files', []))
else:
    print("Unknown format:", type(data))
    sys.exit(1)

fails = []
for r in results:
    score = r.get('overall_score', 0)
    name = r.get('name', r.get('file', '?'))
    text = r.get('text_similarity', 0)
    vis = r.get('visual_avg', r.get('visual_score', 0))
    if score < 0.99:
        fails.append((name, score, text, vis))

fails.sort(key=lambda x: -x[1])
print(f"Total failures: {len(fails)}")
print()
for name, o, t, v in fails:
    # categorize limiting factor
    if t >= 0.99 and v < 0.99:
        cat = "VIS"
    elif v >= 0.99 and t < 0.99:
        cat = "TXT"
    elif t < 0.99 and v < 0.99:
        cat = "BOTH"
    else:
        cat = "PAGE"
    print(f'{cat:4s} {name}: {o:.4f} text={t:.4f} vis={v:.4f}')
