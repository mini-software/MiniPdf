import fitz, os, json

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

targets = ['classic56', 'classic30', 'classic50', 'classic35', 'classic53', 'classic46', 'classic49']
for d in data:
    for t in targets:
        if t in d['name']:
            print(f"{d['name']}: score={d['overall_score']:.4f} text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} pages={d['minipdf_pages']}/{d['reference_pages']}")
            break
