import json

with open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reports\comparison_report.json') as f:
    data = json.load(f)

below = []
for d in data:
    score = d['overall_score']
    if score < 0.99:
        ts = d.get('text_similarity', 0)
        fts = d.get('flat_text_similarity', 0)
        wts = d.get('word_text_similarity', 0)
        va = d.get('visual_avg', 0)
        mp = d.get('minipdf_pages', 0)
        rp = d.get('reference_pages', 0)
        below.append((d['name'], score, ts, fts, wts, va, mp, rp))

below.sort(key=lambda x: x[1])
print(f'Below 0.99: {len(below)} cases')
print(f'At or above 0.99: {len(data) - len(below)} cases')
print()
for name, score, ts, fts, wts, va, mp, rp in below:
    pginfo = f' pg={mp}/{rp}' if mp != rp else ''
    print(f'  {score:.3f} ts={ts:.3f} fts={fts:.3f} wts={wts:.3f} va={va:.3f}{pginfo} {name}')
