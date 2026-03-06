import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

below = [r for r in data if r['overall_score'] < 0.99]
below.sort(key=lambda r: r['overall_score'], reverse=True)

print(f"{'Name':45s} {'overall':>7s} {'text':>6s} {'flat':>6s} {'word':>6s} {'best':>6s}")
for r in below:
    name = r['name'][:45]
    overall = r['overall_score']
    ts = r.get('text_similarity', 0)
    flat = r.get('flat_text_similarity', 0)
    word = r.get('word_text_similarity', 0)
    best_metric = 'text'
    if flat > ts and flat >= word: best_metric = 'flat'
    elif word > ts and word >= flat: best_metric = 'word'
    # text_similarity already is max of all three in compare_pdfs
    print(f'{name:45s} {overall:7.4f} {ts:6.3f} {flat:6.3f} {word:6.3f} {best_metric:>6s}')
