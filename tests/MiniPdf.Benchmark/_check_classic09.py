import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for d2 in data:
    if d2['name'] == 'classic09_long_text':
        for k, v in d2.items():
            if k not in ('text_diff', 'diff_images', 'visual_scores'):
                print(f"{k}: {v}")
            elif k == 'visual_scores':
                print(f"visual_scores: {v[:5]}...")
