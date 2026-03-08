import json

# Current results (with avgCharWidth 0.50 for table cells)
d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))

# Find cases where visual score is low (might indicate layout regression)
print("Cases with visual_avg < 0.90 (potential layout regressions):")
for c in sorted(d, key=lambda x: x['visual_avg']):
    if c['visual_avg'] < 0.90:
        print(f"  {c['name']:<50} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} ovr={c['overall_score']:.4f}")

# Show all cases sorted by overall score
print("\nBottom 20 cases by overall score:")
for c in sorted(d, key=lambda x: x['overall_score'])[:20]:
    print(f"  {c['name']:<50} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} ovr={c['overall_score']:.4f}")

# Count by score buckets
s99 = sum(1 for c in d if c['overall_score'] >= 0.99)
s95 = sum(1 for c in d if 0.95 <= c['overall_score'] < 0.99)
s90 = sum(1 for c in d if 0.90 <= c['overall_score'] < 0.95)
s80 = sum(1 for c in d if 0.80 <= c['overall_score'] < 0.90)
slo = sum(1 for c in d if c['overall_score'] < 0.80)
print(f"\nScore distribution: >=0.99:{s99} 0.95-0.99:{s95} 0.90-0.95:{s90} 0.80-0.90:{s80} <0.80:{slo}")
