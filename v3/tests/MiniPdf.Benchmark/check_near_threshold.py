"""Show text diffs for near-threshold cases where text improvement could push them over 0.99."""
import json

with open('reports/comparison_report.json') as f:
    data = json.load(f)

# Cases close to 0.99 where text < 1.0 and improving text could push them over
targets = []
for c in data:
    if c['overall_score'] >= 0.99:
        continue
    txt = c['text_similarity']
    vis = c['visual_avg']
    pages = 1.0 if c['minipdf_pages'] == c['reference_pages'] else 0.5
    # If we got text perfect (1.0), what would overall be?
    potential = 1.0 * 0.4 + vis * 0.4 + pages * 0.2
    if potential >= 0.99 and txt < 1.0:
        targets.append((c['name'], c['overall_score'], txt, vis, potential, c.get('text_diff', '')))

targets.sort(key=lambda x: -x[4])  # Sort by potential (best first)

print(f"Cases where perfect text would reach 0.99: {len(targets)}")
print()
for name, score, txt, vis, pot, diff in targets[:20]:
    print(f"=== {name} (now={score:.4f}, txt={txt:.3f}, vis={vis:.3f}, potential={pot:.4f}) ===")
    if diff and diff != '(identical)':
        lines = diff.strip().split('\n')
        for i, line in enumerate(lines):
            if i > 25: 
                print(f"  ... ({len(lines) - i} more lines)")
                break
            print(f"  {line}")
    print()
