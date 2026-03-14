"""Show bottom N cases to find improvement opportunities."""
import json

d = json.load(open(r'reports\comparison_report.json', encoding='utf-8'))
d.sort(key=lambda x: x['overall_score'])

print('Bottom 30 cases:')
for x in d[:30]:
    ts = x.get('text_similarity', 0)
    vis = x.get('visual_avg', 0)
    gap = 1.0 - x['overall_score']
    bottleneck = 'TEXT' if ts < vis else 'VIS'
    name = x['name']
    score = x['overall_score']
    print(f'  {name:48s}  overall={score:.4f}  text={ts:.4f}  vis={vis:.4f}  gap={gap:.4f}  {bottleneck}')
