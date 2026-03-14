import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Chart cases (contain 'chart' in name)
charts = [r for r in data if 'chart' in r['name']]
charts.sort(key=lambda x: x['overall_score'])
total_gap = sum(1 - r['overall_score'] for r in charts)
print(f'Chart cases: {len(charts)}, total gap: {total_gap:.4f}, avg gap: {total_gap/len(charts):.4f}')
for r in charts:
    print(f'  {r["name"]:50s} t={r["text_similarity"]:.4f} v={r["visual_avg"]:.4f} s={r["overall_score"]:.4f}')

# Non-chart, non-CJK/emoji cases with room to improve
print('\n--- Non-chart/CJK cases with text < 0.99 ---')
text_room = [r for r in data 
             if 'chart' not in r['name'] 
             and 'cjk' not in r['name'] 
             and 'emoji' not in r['name']
             and 'unicode' not in r['name']
             and 'korean' not in r['name']
             and 'indic' not in r['name']
             and 'rtl' not in r['name']
             and 'musical' not in r['name']
             and 'african' not in r['name']
             and 'zwj' not in r['name']
             and 'polyglot' not in r['name']
             and 'multiscript' not in r['name']
             and 'caucas' not in r['name']
             and r['text_similarity'] < 0.99]
text_room.sort(key=lambda x: x['text_similarity'])
for r in text_room:
    tgap = 1 - r['text_similarity']
    print(f'  {r["name"]:50s} t={r["text_similarity"]:.4f} (gap={tgap:.4f})')
