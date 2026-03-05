import json
with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
below = sum(1 for r in data if r['overall_score'] < 0.99)
above = sum(1 for r in data if r['overall_score'] >= 0.99)
print(f'Below: {below}, At/Above: {above}')
# Check key files
for name in ['classic132_striped_table', 'classic42_boolean_values', 'classic134_heatmap', 'classic137_checkerboard', 'classic44_employee_roster', 'classic40_scientific_notation', 'classic131_number_formats', 'classic58_mixed_numeric_formats', 'classic13_date_strings', 'classic129_alignment_combos', 'classic09_long_text']:
    for r in data:
        if r['name'] == name:
            print(f"  {name}: {r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
