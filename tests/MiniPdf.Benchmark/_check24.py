import json
d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))
for c in d:
    if 'classic24' in c['name']:
        print(f"{c['name']}: text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} ovr={c['overall_score']:.4f}")
