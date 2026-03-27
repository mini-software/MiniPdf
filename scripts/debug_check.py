import json
with open(r'tests/Issue_Files/reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
for x in data:
    if '20260318' in x.get('name', ''):
        print('pages:', x['minipdf_pages'], 'vs', x['reference_pages'])
        print('visual:', x['visual_scores'])
        print('text_sim:', x.get('text_similarity'))
