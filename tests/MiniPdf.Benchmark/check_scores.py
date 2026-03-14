import re

OLD_SCORES = {
    'classic18_large_dataset': 0.9572,
    'classic191_payroll_calculator': 0.9054,
    'classic148_frozen_styled_grid': 0.9733,
    'classic06_tall_table': 0.9779,
    'classic60_large_wide_table': 0.9738,
    'classic189_alternating_image_text_rows': 0.9526,
    'classic182_dense_long_text_columns': 0.9639,
    'classic49_contact_list': 0.9737,
    'classic150_kitchen_sink_styles': 0.9745,
    'classic140_rotated_text': 0.9810,
}

lines = open('reports/comparison_report.md', encoding='utf-8').readlines()
scores = {}
all_scores = []
for line in lines:
    m = re.search(r'\|\s*\d+\s*\|.*?(classic\S+)\s*\|\s*([\d.]+)\s*\|\s*([\d.]+)\s*\|\s*(\d+)/(\d+)\s*\|\s*\*\*([\d.]+)\*\*', line)
    if m:
        name = m.group(1)
        scores[name] = (float(m.group(6)), float(m.group(2)), float(m.group(3)), int(m.group(4)), int(m.group(5)))
        all_scores.append(float(m.group(6)))

print(f'Average: {sum(all_scores)/len(all_scores):.4f} ({len(all_scores)} cases)')

print('\n=== Key cases ===')
for name, old in sorted(OLD_SCORES.items()):
    if name in scores:
        new, ts, vs, pm, pr = scores[name]
        delta = new - old
        marker = '++' if delta > 0.002 else ('--' if delta < -0.002 else '==')
        print(f'{marker} {name}: {new:.4f} (was {old:.4f}, delta={delta:+.4f}) text={ts:.4f} vis={vs:.4f} pages={pm}/{pr}')

# Find ALL regressions (any drop > 0.001)
print('\n=== All changes > 0.1% ===')
for name, (new, ts, vs, pm, pr) in sorted(scores.items()):
    if name in OLD_SCORES:
        continue
    # Can't compare, skip
# Actually compare all old vs new
old_all = {}
# Re-read old scores from the previous run
# We'll just compare the tracked ones
for name, old in sorted(OLD_SCORES.items()):
    if name in scores:
        new = scores[name][0]
        delta = new - old
        if abs(delta) > 0.001:
            print(f'  {name}: {new:.4f} (was {old:.4f}, delta={delta:+.4f})')

print('\n=== 10 Lowest ===')
by_score = sorted(scores.items(), key=lambda x: x[1][0])
for name, (s, ts, vs, pm, pr) in by_score[:10]:
    print(f'{s:.4f}  text={ts:.4f} vis={vs:.4f} pages={pm}/{pr}  {name}')
