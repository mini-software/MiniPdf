import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

for name in ['classic57_cjk_only', 'classic23_unicode_text']:
    r = [x for x in data if x['name'] == name][0]
    ts = r['text_similarity']
    vs = r['visual_avg']
    ws = r.get('word_text_similarity', 0)
    fs = r.get('flat_text_similarity', 0)
    print(f'\n{"="*60}')
    print(f'{name}: txt={ts:.3f} flat={fs:.3f} word={ws:.3f} vis={vs:.3f}')
    print(f'{"="*60}')
    diff = r.get('text_diff', '')
    print(diff[:2000])
