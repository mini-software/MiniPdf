import json
d = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
below = sum(1 for x in d if x['overall_score'] < 0.99)
avg = sum(x['overall_score'] for x in d) / len(d)
pm = sum(1 for x in d if x.get('minipdf_pages',0) != x.get('reference_pages',0))
print(f'DOCX: Below99={below} Avg={avg:.4f} PM={pm}')
