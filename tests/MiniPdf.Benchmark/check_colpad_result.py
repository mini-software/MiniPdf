"""Compare benchmark results after column padding fix."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Previous scores for key cases (from the 0.969069 benchmark)
prev = {
    'classic18_large_dataset': {'vis': 0.8934, 'text': 1.0, 'overall': 0.9574},
    'classic148_frozen_styled_grid': {'vis': 0.9332, 'text': 1.0, 'overall': 0.9733},
    'classic60_large_wide_table': {'vis': 0.9371, 'text': 1.0, 'overall': 0.9748},
    'classic06_tall_table': {'vis': 0.9447, 'text': 1.0, 'overall': 0.9779},
    'classic191_payroll_calculator': {'vis': 0.9041, 'text': 0.8965, 'overall': 0.9202},
    'classic184_wide_narrow_columns': {'vis': 0.9601, 'text': 1.0, 'overall': 0.9840},
    'classic187_bug_report_with_screenshots': {'vis': 0.9542, 'text': 1.0, 'overall': 0.9817},
}

print("Target cases (before -> after):")
print(f"{'Case':45s} {'vis_before':>10s} {'vis_after':>10s} {'vis_delta':>10s} {'overall_before':>14s} {'overall_after':>14s} {'overall_delta':>14s}")
for r in d:
    name = r['name']
    if name in prev:
        va = r.get('visual_avg', 0)
        ts = r['text_similarity']
        ov = r['overall_score']
        p = prev[name]
        vd = va - p['vis']
        od = ov - p['overall']
        print(f"{name:45s} {p['vis']:10.4f} {va:10.4f} {vd:+10.4f} {p['overall']:14.4f} {ov:14.4f} {od:+14.4f}")

# Find biggest regressions
print("\nBiggest overall regressions (overall < prev):")
# I don't have prev data for all cases, so check low scores
allscores = [(r['name'], r['overall_score']) for r in d]
allscores.sort(key=lambda x: x[1])
print("\nLowest 15 overall scores:")
for name, score in allscores[:15]:
    print(f"  {name}: {score:.4f}")

# Overall average
avg = sum(r['overall_score'] for r in d) / len(d)
print(f"\nOverall average: {avg:.6f}")
print(f"Target: 0.969069")
print(f"Delta: {avg - 0.969069:+.6f}")
