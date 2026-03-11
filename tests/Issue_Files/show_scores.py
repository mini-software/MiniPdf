import json, sys, os
base = os.path.dirname(os.path.abspath(__file__))
data = json.load(open(os.path.join(base, 'reports_xlsx', 'comparison_report.json'), encoding='utf-8'))
r = data[0]
print('Score:', r['overall_score'])
print('Pages:', r['minipdf_pages'], 'vs', r['reference_pages'])
print('Text:', r['text_similarity'], 'Flat:', r['flat_text_similarity'], 'Word:', r['word_text_similarity'])
print('Visual avg:', r['visual_avg'])
for i, vs in enumerate(r['visual_scores']):
    print('  p%d: vis=%.3f' % (i+1, vs))
print('Score:', r['score'])
print('Pages:', r['minipdf_pages'], 'vs', r['reference_pages'])
print('Text:', r['text_similarity'])
for p in r['page_details']:
    print('  p%d: vis=%.3f txt=%.3f' % (p['page'], p['visual_score'], p['text_score']))
