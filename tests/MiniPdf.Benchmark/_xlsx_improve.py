"""XLSX improvement analysis"""
import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
below99 = [d for d in data if d['overall_score'] < 0.99]

text_only = [d for d in below99 if d['text_similarity'] < 0.99 and d['visual_avg'] >= 0.99]
vis_only = [d for d in below99 if d['text_similarity'] >= 0.99 and d['visual_avg'] < 0.99]
both = [d for d in below99 if d['text_similarity'] < 0.99 and d['visual_avg'] < 0.99]

print(f"Below 99: {len(below99)}, Text-only: {len(text_only)}, Vis-only: {len(vis_only)}, Both: {len(both)}")

print("\n=== Near-miss XLSX (0.98+) ===")
near = sorted([d for d in below99 if d['overall_score'] >= 0.98], key=lambda x: -x['overall_score'])
for d in near:
    cat = "T" if d in text_only else "V" if d in vis_only else "B"
    print(f"  {d['name']}: overall={d['overall_score']:.4f} text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} [{cat}]")

print("\n=== Text-only XLSX failures ===")
for d in sorted(text_only, key=lambda x: x['text_similarity']):
    print(f"  {d['name']}: text={d['text_similarity']:.4f}")

print("\n=== Both-failures near miss (top 15) ===")
for d in sorted(both, key=lambda x: -x['overall_score'])[:15]:
    print(f"  {d['name']}: text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} overall={d['overall_score']:.4f}")
