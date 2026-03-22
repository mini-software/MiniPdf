import json
data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
# Cases in good tier (0.7-0.9) - check page count mismatch
for x in sorted(data, key=lambda d: d.get('overall', d.get('visual_avg', 0))):
    score = x.get('overall', x.get('visual_avg', 0))
    if 0.7 <= score < 0.9:
        mp = x['minipdf_pages']
        rp = x['reference_pages']
        mismatch = '***' if mp != rp else ''
        name = x['name'][:60]
        print(f"{score:.4f}  {mp}/{rp} {mismatch} {name}")
