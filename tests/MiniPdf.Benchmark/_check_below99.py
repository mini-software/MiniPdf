import json

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

below99 = [x for x in data if x['overall_score'] < 0.99]
print(f'Total cases: {len(data)}, Below 99: {len(below99)}')
for x in sorted(below99, key=lambda x: x['overall_score']):
    print(f"\n{x['name']}: overall={x['overall_score']}, visual_avg={x['visual_avg']}, text_sim={x['text_similarity']}")
    if x.get('text_diff') and x['text_diff'] != '(identical)':
        print(f"  text_diff: {x['text_diff'][:300]}")
    if x.get('suggestions'):
        print(f"  suggestions: {x['suggestions'][:300]}")
