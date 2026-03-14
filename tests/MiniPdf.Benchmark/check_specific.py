import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for c in d:
    if '150' in c['name'] or '128' in c['name']:
        print(f"{c['name']}: text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")

# Also show cases that changed significantly from baseline 0.9689
total = sum(c['overall_score'] for c in d)
avg = total / len(d)
print(f"\nOverall: {avg:.4f} ({len(d)} cases)")
