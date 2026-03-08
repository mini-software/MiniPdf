"""Find the most improvable DOCX cases — focus on those closest to 0.99 threshold"""
import json

data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
below99 = [d for d in data if d['overall_score'] < 0.99]

# Group by failure type
text_only = [d for d in below99 if d['text_similarity'] < 0.99 and d['visual_avg'] >= 0.99]
vis_only = [d for d in below99 if d['text_similarity'] >= 0.99 and d['visual_avg'] < 0.99]
both = [d for d in below99 if d['text_similarity'] < 0.99 and d['visual_avg'] < 0.99]
page_only = [d for d in below99 if d['text_similarity'] >= 0.99 and d['visual_avg'] >= 0.99 and 
             d['minipdf_pages'] != d['reference_pages']]

print(f"Below 99: {len(below99)}")
print(f"  Text-only: {len(text_only)}")
print(f"  Vis-only: {len(vis_only)}")
print(f"  Both: {len(both)}")
print(f"  Page-only: {len(page_only)}")

# Show cases closest to 0.99 (easiest to fix)
print("\n=== Near-miss cases (0.98-0.99) ===")
near_miss = sorted([d for d in below99 if d['overall_score'] >= 0.98], key=lambda x: -x['overall_score'])
for d in near_miss:
    cat = "T" if d in text_only else "V" if d in vis_only else "B" if d in both else "P"
    print(f"  {d['name']}: overall={d['overall_score']:.4f} text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} [{cat}]")

# Show worst cases  
print(f"\n=== Worst 15 cases ===")
worst = sorted(below99, key=lambda x: x['overall_score'])[:15]
for d in worst:
    cat = "T" if d in text_only else "V" if d in vis_only else "B" if d in both else "P"
    print(f"  {d['name']}: overall={d['overall_score']:.4f} text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} pages={d['minipdf_pages']}/{d['reference_pages']} [{cat}]")

# Text-only failures
print(f"\n=== Text-only failures ===")
for d in sorted(text_only, key=lambda x: x['text_similarity']):
    print(f"  {d['name']}: text={d['text_similarity']:.4f} overall={d['overall_score']:.4f}")

# Vis-only near 0.975 (would make overall >= 0.99)
print(f"\n=== Vis-only closest to 0.975 threshold ===")
vis_scores = [(d, d['visual_avg']) for d in vis_only]
vis_scores.sort(key=lambda x: -x[1])
for d, v in vis_scores[:20]:
    print(f"  {d['name']}: vis={v:.4f} (need 0.975 for 0.99)")
