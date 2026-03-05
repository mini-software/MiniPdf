"""Categorize all failures by improvement type."""
import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
results = []
for r in data:
    name = r['name']
    o = r['overall_score']
    t = r['text_similarity']
    v = r['visual_avg']
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    p = 1.0 if mp == rp else 0.0
    if o < 0.99:
        results.append((name, o, t, v, p))

results.sort(key=lambda x: -x[1])

# What would happen if text became 1.0 for each?
print("=== If text became 1.0 ===")
for name, o, t, v, p in results:
    new_o = 0.4 * 1.0 + 0.4 * v + 0.2 * p
    if new_o >= 0.99 and t < 1.0:
        print(f"  WOULD PASS: {name}: cur_text={t:.4f} vis={v:.4f} new_overall={new_o:.4f}")

# What would happen if vis became 1.0 for each?
print("\n=== If vis became 1.0 ===")
for name, o, t, v, p in results:
    new_o = 0.4 * t + 0.4 * 1.0 + 0.2 * p
    if new_o >= 0.99 and v < 1.0:
        print(f"  WOULD PASS: {name}: cur_vis={v:.4f} text={t:.4f} new_overall={new_o:.4f}")

# What if both text and vis improved by 0.01?
print("\n=== Closest to threshold (need small improvements in either) ===")
for name, o, t, v, p in results:
    deficit = 0.99 - o
    if deficit <= 0.02:
        # What combination would work?
        needed_vis = (0.99 - 0.4*t - 0.2*p) / 0.4
        needed_text = (0.99 - 0.4*v - 0.2*p) / 0.4
        print(f"  {name}: overall={o:.4f} text={t:.4f} vis={v:.4f}")
        print(f"    Need vis>={needed_vis:.4f} (gap={max(0,needed_vis-v):.4f}) OR text>={needed_text:.4f} (gap={max(0,needed_text-t):.4f})")
