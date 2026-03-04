import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Check which text metric is being used for failing cases near threshold
below = [x for x in data if x['overall_score'] < 0.99]
below.sort(key=lambda x: x['overall_score'], reverse=True)
for x in below[:20]:
    txt = x['text_similarity']
    flat = x.get('flat_text_similarity', 0)
    word = x.get('word_text_similarity', 0)
    best = max(txt, flat, word)
    used = 'page' if txt == best else ('flat' if flat == best else 'word')
    print(f"{x['name']:45s} best_txt={best:.4f} (page={txt:.4f} flat={flat:.4f} word={word:.4f}) using={used}")
