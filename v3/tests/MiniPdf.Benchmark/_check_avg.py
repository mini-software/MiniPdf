import json
with open('reports/comparison_report.json', encoding='utf-8') as f:
    results = json.load(f)
scores = [r['overall_score'] for r in results]
avg = sum(scores)/len(scores)
above99 = sum(1 for s in scores if s >= 0.99)
for name in ['classic42_boolean_values', 'classic13_date_strings', 'classic132_striped_table', 'classic128_font_sizes', 'classic140_rotated_text']:
    for r in results:
        if r['name'] == name:
            t = r.get('text_similarity', 0)
            v = r.get('visual_avg', 0)
            print(f'{name}: {r["overall_score"]:.6f} text={t:.6f} vis={v:.6f}')
print(f'Average: {avg:.6f}, >=0.99: {above99}/150')
