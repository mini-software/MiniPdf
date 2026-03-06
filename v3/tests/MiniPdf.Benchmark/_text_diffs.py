import json

with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

# Near-miss cases with text issues (text_sim < 0.99 and score >= 0.95)
near = [r for r in data if 0.95 <= r.get('overall_score',0) < 0.99 and r.get('text_similarity',0) < 0.99]
near.sort(key=lambda r: r['overall_score'], reverse=True)

for r in near[:20]:
    name = r['name']
    ts = r.get('text_similarity',0)
    vs = r.get('visual_avg',0)
    diff = r.get('text_diff', '')
    print(f"\n=== {name} (score={r['overall_score']:.4f}, text={ts:.4f}, vis={vs:.4f}) ===")
    # Show only the diff lines (truncated)
    lines = diff.split('\n')
    shown = 0
    for line in lines:
        if line.startswith('+') or line.startswith('-'):
            if not line.startswith('---') and not line.startswith('+++'):
                print(f"  {line[:120]}")
                shown += 1
                if shown > 10:
                    print(f"  ... ({len([l for l in lines if l.startswith('+') or l.startswith('-')])} total diff lines)")
                    break
