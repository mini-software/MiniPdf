import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
if isinstance(data, list):
    results = data
else:
    results = data.get('results', data)

below99 = [r for r in results if r['overall_score'] < 0.99]
below99.sort(key=lambda r: r['overall_score'], reverse=True)

# Check field names
print(f'Keys: {list(results[0].keys())[:10]}')

# Get correct field names
name_key = 'file' if 'file' in results[0] else 'name'
vis_key = 'visual_avg' if 'visual_avg' in results[0] else 'visual_similarity'

# Categorize
def get_vis(r):
    return r.get('visual_avg', r.get('visual_similarity', 0))

text_only = [r for r in below99 if r.get('text_similarity',0) < 0.99 and get_vis(r) >= 0.99]
vis_only = [r for r in below99 if r.get('text_similarity',0) >= 0.99 and get_vis(r) < 0.99]
both = [r for r in below99 if r.get('text_similarity',0) < 0.99 and get_vis(r) < 0.99]
page_only = [r for r in below99 if r.get('text_similarity',0) >= 0.99 and get_vis(r) >= 0.99]

print(f'Below99: {len(below99)}')
print(f'Text-only: {len(text_only)}, Vis-only: {len(vis_only)}, Both: {len(both)}, Page-only: {len(page_only)}')

# Distribution of vis-only by visual score
print('\n--- Visual-only by score range ---')
for lo, hi in [(0.98,0.99),(0.97,0.98),(0.96,0.97),(0.95,0.96),(0.90,0.95),(0.0,0.90)]:
    count = len([r for r in vis_only if lo <= get_vis(r) < hi])
    if count > 0:
        print(f'  {lo:.2f}-{hi:.2f}: {count}')

# Top 20 closest to 99 
print('\n--- Top 20 closest to 99 ---')
for r in below99[:20]:
    name = r.get(name_key, '').replace('.pdf','')
    ts = r.get('text_similarity', 0)
    vs = get_vis(r)
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    ov = r['overall_score']
    is_chart = any(c in name for c in ['chart', 'pie', 'bar', 'line_graph', 'scatter', 'donut', 'area', 'bubble', 'combo', 'stacked'])
    cat = ('chart ' if is_chart else '') + ('T' if ts < 0.99 else '') + ('V' if vs < 0.99 else '') + ('P' if mp != rp else '')
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f} p={mp}/{rp} [{cat}]')

# Show text-only failures (easily identifiable fix targets)
print('\n--- Text-only failures ---')
for r in text_only:
    name = r.get(name_key, '').replace('.pdf','')
    ts = r.get('text_similarity', 0)
    vs = get_vis(r)
    ov = r['overall_score']
    is_chart = any(c in name for c in ['chart', 'pie', 'bar', 'line_graph', 'scatter', 'donut', 'area', 'bubble', 'combo', 'stacked'])
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f} [{"chart" if is_chart else "non-chart"}]')
