import json

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

if isinstance(data, list):
    results = data
else:
    results = data['results']

def get_name(r):
    return r.get('file', r.get('name', '')).replace('.pdf','')

below99 = [r for r in results if r['overall_score'] < 0.99]
below99.sort(key=lambda r: r['overall_score'], reverse=True)

ranges = [(0.99, 1.0), (0.98, 0.99), (0.97, 0.98), (0.96, 0.97), (0.95, 0.96), (0.90, 0.95), (0.0, 0.90)]
for lo, hi in ranges:
    count = len([r for r in results if lo <= r['overall_score'] < hi])
    print(f'{lo:.2f}-{hi:.2f}: {count}')

print(f'\nTotal below 99: {len(below99)}')
print(f'Average: {sum(r["overall_score"] for r in results)/len(results):.4f}')

# Categorize
text_only = [r for r in below99 if r.get('text_similarity',0) < 0.99 and r.get('visual_avg', r.get('visual_similarity',0)) >= 0.99]
vis_only = [r for r in below99 if r.get('text_similarity',0) >= 0.99 and r.get('visual_avg', r.get('visual_similarity',0)) < 0.99]
both = [r for r in below99 if r.get('text_similarity',0) < 0.99 and r.get('visual_avg', r.get('visual_similarity',0)) < 0.99]
page_only = [r for r in below99 if r.get('text_similarity',0) >= 0.99 and r.get('visual_avg', r.get('visual_similarity',0)) >= 0.99]
print(f'\nText-only: {len(text_only)}, Vis-only: {len(vis_only)}, Both: {len(both)}, Page-only: {len(page_only)}')

# Visual similarity distribution for vis_only cases
print('\n--- Visual-only failures (text>=0.99, vis<0.99) ---')
vis_ranges = [(0.98, 0.99), (0.97, 0.98), (0.96, 0.97), (0.95, 0.96), (0.90, 0.95), (0.0, 0.90)]
for lo, hi in vis_ranges:
    count = len([r for r in vis_only if lo <= r.get('visual_avg', r.get('visual_similarity',0)) < hi])
    print(f'  vis {lo:.2f}-{hi:.2f}: {count}')

# Top 25 closest to 99
print('\n--- Top 25 closest to 99 ---')
for r in below99[:25]:
    name = get_name(r)
    ts = r.get('text_similarity', 0)
    vs = r.get('visual_avg', r.get('visual_similarity', 0))
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    ov = r['overall_score']
    cat = 'T' if ts < 0.99 else ''
    cat += 'V' if vs < 0.99 else ''
    cat += 'P' if mp != rp else ''
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f} p={mp}/{rp} [{cat}]')

# Bottom 10 worst
print('\n--- Bottom 10 worst ---')
for r in below99[-10:]:
    name = get_name(r)
    ts = r.get('text_similarity', 0)
    vs = r.get('visual_avg', r.get('visual_similarity', 0))
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    ov = r['overall_score']
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f} p={mp}/{rp}')
    rp = r.get('reference_pages', 0)
    ov = r['overall_score']
    print(f'  {name}: {ov:.4f} t={ts:.4f} v={vs:.4f} p={mp}/{rp}')
