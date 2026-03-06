import json

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

targets = ['classic17', 'classic40', 'classic41', 'classic58', 'classic56',
           'classic91', 'classic92', 'classic93', 'classic103', 'classic111']

for item in data:
    name = item['name']
    for t in targets:
        if t + '_' in name or name.endswith(t):
            ov = item['overall_score']
            ts = max(item['text_similarity'], item.get('flat_text_similarity',0), item.get('word_text_similarity',0))
            vs = item.get('visual_avg', 0)
            mp = item.get('minipdf_pages', 0)
            rp = item.get('reference_pages', 0)
            ps = 1.0 if mp == rp else 0.5
            print(f"{name}: overall={ov:.3f} ts={ts:.3f} vs={vs:.3f} pages={mp}/{rp}")

print("\n--- All below 0.99 ---")
below = [(item['name'], item['overall_score']) for item in data if item['overall_score'] < 0.99]
below.sort(key=lambda x: x[1])
for name, score in below:
    print(f"  {name}: {score:.3f}")
print(f"\nTotal below 0.99: {len(below)}")
print(f"Total at or above 0.99: {len(data) - len(below)}")
