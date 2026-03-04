import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Page mismatches
page_mismatch = [(x['name'], x['minipdf_pages'], x['reference_pages'], x['overall_score']) 
                 for x in data if x['minipdf_pages'] != x['reference_pages']]

# Under 0.99 cases grouped by category
under = [(x['name'], x['overall_score'], x['text_similarity'], x['visual_avg'],
          1.0 if x['minipdf_pages'] == x['reference_pages'] else 0.5)
         for x in data if x['overall_score'] < 0.99]
under.sort(key=lambda x: x[1], reverse=True)

print("=== Page mismatches ===")
for name, mp, rp, o in sorted(page_mismatch, key=lambda x: x[3]):
    print(f"  {name}: {mp}p vs {rp}p  overall={o:.4f}")

print(f"\n=== Under 0.99 ({len(under)} cases) ===")
print(f"  Near-threshold (0.97-0.99): {sum(1 for x in under if x[1] >= 0.97)}")
print(f"  Mid-range (0.93-0.97): {sum(1 for x in under if 0.93 <= x[1] < 0.97)}")
print(f"  Low (< 0.93): {sum(1 for x in under if x[1] < 0.93)}")

# Group by bottleneck
print(f"\n=== Bottleneck analysis ===")
txt_limited = [(n, o, t, v, p) for n, o, t, v, p in under if t < v and t < 0.98]
vis_limited = [(n, o, t, v, p) for n, o, t, v, p in under if v < t and v < 0.98]
pg_limited = [(n, o, t, v, p) for n, o, t, v, p in under if p < 1.0]

print(f"\nText-limited ({len(txt_limited)}):")
for name, o, t, v, p in sorted(txt_limited, key=lambda x: x[2]):
    print(f"  {name}: o={o:.4f} txt={t:.3f} vis={v:.3f} pg={p}")

print(f"\nVisual-limited ({len(vis_limited)}):")
for name, o, t, v, p in sorted(vis_limited, key=lambda x: x[3]):
    print(f"  {name}: o={o:.4f} txt={t:.3f} vis={v:.3f} pg={p}")

print(f"\nPage-count limited ({len(pg_limited)}):")
for name, o, t, v, p in sorted(pg_limited, key=lambda x: x[1]):
    print(f"  {name}: o={o:.4f} txt={t:.3f} vis={v:.3f} pg={p}")
