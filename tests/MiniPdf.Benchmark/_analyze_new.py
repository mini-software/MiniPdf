import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

passing = [d for d in data if d['overall_score'] >= 0.99]
failing = [d for d in data if d['overall_score'] < 0.99]
print(f'Passing (>=0.99): {len(passing)}/150')
print(f'Failing (<0.99): {len(failing)}/150')
print()

# Baseline scores (from before fixes)
baseline = {
    'classic132_striped_table': 0.9899,
    'classic128_font_sizes': 0.9886,
    'classic44_employee_roster': 0.9868,
    'classic134_heatmap': 0.9857,
    'classic131_number_formats': 0.9936,
    'classic150_kitchen_sink_styles': 0.9503,
    'classic142_styled_invoice': 0.9391,
    'classic149_merged_styled_sections': 0.9392,
    'classic137_checkerboard': 0.9792,
    'classic148_frozen_styled_grid': 0.9707,
}

print('Target files (before -> after):')
for d in sorted(data, key=lambda x: x['name']):
    name = d['name']
    if name in baseline:
        old = baseline[name]
        new = d['overall_score']
        delta = new - old
        sign = '+' if delta >= 0 else ''
        status = 'PASS' if new >= 0.99 else 'FAIL'
        print(f'  {name}: {old:.4f} -> {new:.4f} ({sign}{delta:.4f}) [{status}]')

print()
print('All failing files (sorted by score desc):')
for d in sorted(failing, key=lambda x: x['overall_score'], reverse=True):
    print(f'  {d["name"]}: {d["overall_score"]:.4f} (text={d["text_similarity"]:.4f}, vis={d["visual_avg"]:.4f})')
