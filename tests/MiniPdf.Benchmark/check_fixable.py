"""Find cases with small gaps that might be fixable."""
import json

d = json.load(open(r'reports\comparison_report.json', encoding='utf-8'))

# Find cases in the 0.96-0.99 range where text_sim < 1.0 and no unicode/chart issues
interesting = []
for x in d:
    name = x['name']
    ts = x.get('text_similarity', 0)
    vis = x.get('visual_avg', 0)
    score = x['overall_score']
    
    # Skip unicode/CJK/emoji and chart cases
    skip_keywords = ['cjk', 'emoji', 'unicode', 'korean', 'indic', 'rtl', 'bidi', 
                     'cyrillic', 'arabic', 'hebrew', 'thai', 'african', 'musical',
                     'combining', 'caucasus', 'ethiopic', 'zwj', 'chart', 'pie', 
                     'bar', 'line', 'area', 'scatter', 'bubble', 'ohlc', 'combo',
                     'dashboard', 'stock']
    if any(kw in name.lower() for kw in skip_keywords):
        continue
    
    if 0.93 <= score < 1.0 and ts < 0.99:
        gap = 1.0 - score
        interesting.append((gap, name, score, ts, vis))

interesting.sort(reverse=True)
print(f'Fixable non-chart, non-unicode cases (0.93-1.0, text_sim < 0.99):')
for gap, name, score, ts, vis in interesting[:25]:
    bottleneck = 'TEXT' if ts < vis else 'VIS'
    print(f'  {name:48s}  score={score:.4f}  text={ts:.4f}  vis={vis:.4f}  gap={gap:.4f}  {bottleneck}')
