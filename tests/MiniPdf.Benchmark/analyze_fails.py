import json

with open("reports/comparison_report.json") as f:
    results = json.load(f)

under = sorted(
    [r for r in results if r["overall_score"] < 0.99],
    key=lambda r: r["overall_score"],
    reverse=True,
)

# Categorize by bottleneck
txt_limited = []  # txt < vis (text is the main bottleneck)
vis_limited = []  # vis < txt (visual is the main bottleneck)
pg_limited = []   # page count mismatch
both_limited = [] # both contribute

for r in under:
    name = r["name"]
    txt = r["text_similarity"]
    vis = r.get("visual_avg", 0)
    pg = 1.0 if r["minipdf_pages"] == r["reference_pages"] else 0.5
    score = r["overall_score"]
    
    if pg < 1.0:
        pg_limited.append(r)
    elif txt < 0.99 and vis >= 0.99:
        txt_limited.append(r)
    elif vis < 0.99 and txt >= 0.99:
        vis_limited.append(r)
    else:
        both_limited.append(r)

print(f"=== CASES BELOW 0.99: {len(under)} ===\n")
print(f"Page-count mismatch: {len(pg_limited)}")
for r in pg_limited:
    print(f"  {r['name']}: score={r['overall_score']:.4f} pages={r['minipdf_pages']}/{r['reference_pages']}")

print(f"\nText-limited (txt<0.99, vis>=0.99): {len(txt_limited)}")
for r in txt_limited:
    print(f"  {r['name']}: score={r['overall_score']:.4f} txt={r['text_similarity']:.3f} vis={r.get('visual_avg',0):.3f}")

print(f"\nVisual-limited (vis<0.99, txt>=0.99): {len(vis_limited)}")
for r in vis_limited:
    print(f"  {r['name']}: score={r['overall_score']:.4f} txt={r['text_similarity']:.3f} vis={r.get('visual_avg',0):.3f}")

print(f"\nBoth limited (txt<0.99, vis<0.99): {len(both_limited)}")
for r in both_limited:
    print(f"  {r['name']}: score={r['overall_score']:.4f} txt={r['text_similarity']:.3f} vis={r.get('visual_avg',0):.3f}")

# Show text diffs for txt-limited cases
print("\n\n=== TEXT DIFFS FOR TEXT-LIMITED & BOTH-LIMITED CASES ===")
for r in sorted(txt_limited + both_limited, key=lambda x: x['overall_score'], reverse=True):
    diff = r.get('text_diff', '')
    if diff and r['text_similarity'] < 0.99:
        print(f"\n--- {r['name']} (score={r['overall_score']:.4f} txt={r['text_similarity']:.3f}) ---")
        # Show only first few diff lines
        lines = diff.split('\n')
        for line in lines[:15]:
            print(f"  {line}")
        if len(lines) > 15:
            print(f"  ... ({len(lines)-15} more lines)")
