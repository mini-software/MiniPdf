import json
d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))
for c in d:
    name = c['name'].replace('docx_', '')
    if any(k in name for k in ['classic35', 'classic24', 'classic64', 'classic38', 'classic68', 'classic110', 'classic50_long', 'classic60_project', 'classic82', 'classic92', 'classic117', 'classic120']):
        print(f"{name:55s} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} ovr={c['overall_score']:.4f}")
