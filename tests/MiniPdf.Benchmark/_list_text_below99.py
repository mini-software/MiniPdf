"""List all XLSX/DOCX cases below 99 with text < 99, sorted by text score."""
import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

# XLSX
with open('reports/comparison_report.json', encoding='utf-8') as f:
    xlsx = json.load(f)

below = []
for r in xlsx:
    ts = r.get('text_similarity', 1.0)
    os_ = r['overall_score']
    vs = r.get('visual_similarity', 1.0)
    if os_ < 0.99 and ts < 0.99:
        below.append((r['name'], ts, vs, os_))

below.sort(key=lambda x: x[1])
print('=== XLSX text<99 cases (sorted by text score) ===')
for name, ts, vs, os_ in below:
    print(f'  t={ts:.4f} v={vs:.4f} o={os_:.4f}  {name}')
print(f'Count: {len(below)}')

# Group by category
chart = [x for x in below if 'chart' in x[0]]
cjk = [x for x in below if any(k in x[0] for k in ['cjk', 'chinese', 'japanese', 'korean', 'unicode', 'emoji', 'arabic', 'hebrew', 'thai', 'hindi', 'devanagari', 'mixed_lang'])]
other = [x for x in below if x not in chart and x not in cjk]

print(f'\nChart: {len(chart)}, CJK/Unicode: {len(cjk)}, Other: {len(other)}')
print(f'Chart avg text: {sum(x[1] for x in chart)/len(chart):.4f}' if chart else '')
print(f'CJK avg text: {sum(x[1] for x in cjk)/len(cjk):.4f}' if cjk else '')
print(f'Other avg text: {sum(x[1] for x in other)/len(other):.4f}' if other else '')

# DOCX
print('\n')
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    docx = json.load(f)

below_d = []
for r in docx:
    ts = r.get('text_similarity', 1.0)
    os_ = r['overall_score']
    vs = r.get('visual_similarity', 1.0)
    if os_ < 0.99 and ts < 0.99:
        below_d.append((r['name'], ts, vs, os_))

below_d.sort(key=lambda x: x[1])
print('=== DOCX text<99 cases (sorted by text score) ===')
for name, ts, vs, os_ in below_d[:30]:  # top 30 worst
    print(f'  t={ts:.4f} v={vs:.4f} o={os_:.4f}  {name}')
if len(below_d) > 30:
    print(f'  ... and {len(below_d)-30} more')
print(f'Count: {len(below_d)}')
