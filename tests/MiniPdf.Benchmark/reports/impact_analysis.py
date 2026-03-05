#!/usr/bin/env python3
"""Deep analysis of text/visual differences for failing cases."""
import json
import sys

with open("tests/MiniPdf.Benchmark/reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

below = [r for r in data if r.get("overall_score", 0) < 0.99]
below.sort(key=lambda r: r["overall_score"])

# Find what would happen if we could fix only text, only visual, or only page issues
print("=== Impact Analysis ===\n")
print(f"Total cases below 0.99: {len(below)}\n")

# For each case, what score component is the bottleneck?
text_limited = 0   # text improvement would help most
vis_limited = 0    # visual improvement would help most
page_limited = 0   # page count fix would help most
both_limited = 0   # need both text + visual

for r in below:
    ts = r["text_similarity"]
    vs = r["visual_avg"]
    mp = r.get("minipdf_pages", 0)
    rp = r.get("reference_pages", 0)
    ps = 1.0 if mp == rp else 0.5
    
    # What score would we get with perfect text?
    perfect_text = 1.0 * 0.4 + vs * 0.4 + ps * 0.2
    # What score would we get with perfect visual?
    perfect_vis = ts * 0.4 + 1.0 * 0.4 + ps * 0.2
    # What score would we get with perfect pages?
    perfect_page = ts * 0.4 + vs * 0.4 + 1.0 * 0.2
    
    needs_text = perfect_text < 0.99
    needs_vis = perfect_vis < 0.99
    
    if needs_text and needs_vis:
        both_limited += 1
        tag = "BOTH"
    elif needs_text:
        vis_limited += 1  # even perfect text wouldn't reach 0.99, need visual
        tag = "VIS"
    elif needs_vis:
        text_limited += 1  # even perfect visual wouldn't reach 0.99, need text
        tag = "TEXT"
    else:
        tag = "EITHER"  # fixing either would be enough
        if ts < vs:
            text_limited += 1
        else:
            vis_limited += 1

    if mp != rp:
        tag = "PAGE+" + tag
        
    print(f"  {r['name']}: overall={r['overall_score']:.4f} text={ts:.4f} vis={vs:.4f} pages={mp}/{rp} -> [{tag}] "
          f"(fix_text->{perfect_text:.4f} fix_vis->{perfect_vis:.4f})")

print(f"\nSummary:")
print(f"  Text-limited (need better text): {text_limited}")
print(f"  Visual-limited (need better visual): {vis_limited}")  
print(f"  Need both: {both_limited}")
print(f"  Page mismatch: {page_limited}")

# Key question: which cases would pass with JUST visual improvements?
print(f"\n=== Cases that would pass with perfect visual ===")
for r in below:
    ts = r["text_similarity"]  
    ps = 1.0 if r.get("minipdf_pages") == r.get("reference_pages") else 0.5
    score_if_vis_perfect = ts * 0.4 + 1.0 * 0.4 + ps * 0.2
    if score_if_vis_perfect >= 0.99:
        print(f"  {r['name']}: current={r['overall_score']:.4f} -> {score_if_vis_perfect:.4f}")

print(f"\n=== Cases that would pass with perfect text ===")
for r in below:
    vs = r["visual_avg"]
    ps = 1.0 if r.get("minipdf_pages") == r.get("reference_pages") else 0.5
    score_if_text_perfect = 1.0 * 0.4 + vs * 0.4 + ps * 0.2
    if score_if_text_perfect >= 0.99:
        print(f"  {r['name']}: current={r['overall_score']:.4f} -> {score_if_text_perfect:.4f}")
