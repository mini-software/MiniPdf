import json
with open('reports/comparison_report.json', encoding='utf-8') as f:
    d = json.load(f)
b = [r for r in d if r['overall_score'] < 0.99]
avg = sum(r['overall_score'] for r in d) / len(d)
print(f'XLSX: Below99={len(b)} Avg={avg:.4f}')

# Check specific cases
for name in ['classic44_employee_roster', 'classic86_software_screenshot_features', 
             'classic49_contact_list', 'classic51_product_catalog', 'classic09_long_text']:
    r = [x for x in d if x['name'] == name][0]
    print(f"  {name}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f} overall={r['overall_score']:.4f}")
