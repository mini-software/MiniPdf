"""Find cases with text=1.0 but visual < 0.97 to prioritize visual improvements."""
import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

targets = []
for x in d:
    ts = x.get('text_similarity', 0)
    vis = x.get('visual_avg', 0)
    if ts >= 0.999 and vis < 0.97:
        gap = (1.0 - vis) * 0.4  # impact on overall score
        targets.append((gap, x['name'], x['overall_score'], ts, vis))

targets.sort(reverse=True)
print(f"Cases where text=1.0 but visual<0.97 ({len(targets)} cases):")
print(f"(Sorted by potential impact if visual were fixed to 1.0)")
for gap, name, score, ts, vis in targets:
    print(f"  {name:48s}  score={score:.4f}  vis={vis:.4f}  potential_gain={gap:.4f}")
