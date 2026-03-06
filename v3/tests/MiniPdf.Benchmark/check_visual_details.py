import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

if isinstance(data, list):
    cases = data
else:
    cases = data.get('test_cases', data.get('results', []))
targets = [
    'classic06_tall_table',
    'classic18_large_dataset',
    'classic84_travel_destination_cards', 
    'classic81_step_by_step_with_images',
    'classic94_pie_chart',
    'classic97_doughnut_chart',
    'classic91_simple_bar_chart',
]

for c in cases:
    if c['name'] in targets:
        print(f"\n{c['name']}: overall={c['overall_score']:.4f}")
        for k, v in sorted(c.items()):
            if k not in ['name']:
                if isinstance(v, float):
                    print(f"  {k}: {v:.4f}")
                elif isinstance(v, list):
                    print(f"  {k}: {v}")
                elif isinstance(v, int):
                    print(f"  {k}: {v}")
