"""Check the 9 mixed-font affected cases for text_sim changes."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

affected = ['classic107', 'classic114', 'classic117', 'classic120', 'classic128',
           'classic150', 'classic95', 'classic96', 'classic98']

for c in d:
    prefix = c['name'].split('_')[0]
    if prefix in affected:
        ts = c['text_similarity']
        flat = c.get('flat_text_similarity', 0)
        word = c.get('word_text_similarity', 0)
        actual = max(ts, flat, word)
        print(f"{c['name']}: text={actual:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")
