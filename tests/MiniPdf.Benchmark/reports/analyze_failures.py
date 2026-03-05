#!/usr/bin/env python3
"""Analyze comparison report failures."""
import json
import sys

with open("tests/MiniPdf.Benchmark/reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

below = [r for r in data if r.get("overall_score", 0) < 0.99]
below.sort(key=lambda r: r["overall_score"])

mode = sys.argv[1] if len(sys.argv) > 1 else "summary"

if mode == "summary":
    print(f"Total below 0.99: {len(below)}\n")
    for r in below:
        print(f"{r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f} pages={r['minipdf_pages']}/{r['reference_pages']}")

elif mode == "diffs":
    # Show text diffs for specified cases or all
    targets = sys.argv[2:] if len(sys.argv) > 2 else [r["name"] for r in below]
    for r in data:
        if r["name"] in targets:
            print(f"\n{'='*60}")
            print(f"{r['name']} (text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f})")
            print(f"{'='*60}")
            diff = r.get("text_diff", "")
            if diff:
                lines = diff.split("\n")
                # Show first 40 lines of diff
                for line in lines[:40]:
                    print(line)
                if len(lines) > 40:
                    print(f"... ({len(lines)-40} more lines)")
            else:
                print("(no text diff)")

elif mode == "categories":
    charts = []
    text_only = []
    vis_only = []
    page_mismatch = []
    both = []
    for r in below:
        ts = r["text_similarity"]
        vs = r["visual_avg"]
        mp = r["minipdf_pages"]
        rp = r["reference_pages"]
        if mp != rp:
            page_mismatch.append(r)
        elif ts < 0.99 and vs < 0.99:
            both.append(r)
        elif ts < 0.99:
            text_only.append(r)
        else:
            vis_only.append(r)
    
    print(f"Page mismatch ({len(page_mismatch)}):")
    for r in page_mismatch:
        print(f"  {r['name']}: pages={r['minipdf_pages']}/{r['reference_pages']} overall={r['overall_score']:.4f}")
    print(f"\nText+Visual failures ({len(both)}):")
    for r in both:
        print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
    print(f"\nText-only failures ({len(text_only)}):")
    for r in text_only:
        print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
    print(f"\nVisual-only failures ({len(vis_only)}):")
    for r in vis_only:
        print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
