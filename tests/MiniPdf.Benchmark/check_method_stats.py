"""Check the text extraction method stats - how many cases are lifted by which method."""
import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

count_methods = {'page': 0, 'flat': 0, 'word': 0}
lifted_by_page = []
lifted_by_word = []
lifted_by_flat = []

for x in d:
    ts_page = x.get('text_similarity', 0)
    ts_flat = x.get('flat_text_similarity', 0)
    ts_word = x.get('word_text_similarity', 0)
    
    best = max(ts_page, ts_flat, ts_word)
    name = x['name']
    
    if ts_page >= ts_flat and ts_page >= ts_word:
        count_methods['page'] += 1
    elif ts_flat >= ts_page and ts_flat >= ts_word:
        count_methods['flat'] += 1
        if ts_flat > ts_page + 0.01:
            lifted_by_flat.append((ts_flat - ts_page, name, ts_page, ts_flat, ts_word))
    else:
        count_methods['word'] += 1
        if ts_word > ts_page + 0.01:
            lifted_by_word.append((ts_word - ts_page, name, ts_page, ts_flat, ts_word))

print("Method wins:")
for m, c in count_methods.items():
    print(f"  {m}: {c}")

print(f"\nCases where flat > page by >0.01 ({len(lifted_by_flat)}):")
lifted_by_flat.sort(reverse=True)
for d, name, p, f, w in lifted_by_flat[:10]:
    print(f"  {name:48s}  page={p:.4f}  flat={f:.4f}  word={w:.4f}  lift={d:.4f}")

print(f"\nCases where word > page by >0.01 ({len(lifted_by_word)}):")
lifted_by_word.sort(reverse=True)
for d, name, p, f, w in lifted_by_word[:10]:
    print(f"  {name:48s}  page={p:.4f}  flat={f:.4f}  word={w:.4f}  lift={d:.4f}")
