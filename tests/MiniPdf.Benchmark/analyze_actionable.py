import json, os

report = os.path.join('reports', 'comparison_report.json')
with open(report, encoding='utf-8') as f:
    results = json.load(f)

# Filter to actionable cases: exclude chart, unicode, CJK, emoji, RTL, indic, scripts, etc.
skip_words = ['chart', 'unicode', 'cjk', 'emoji', 'rtl', 'bidi', 'indic', 'korean', 'musical',
              'african', 'multilingual', 'polyglot', 'multiscript', 'ltr_rtl', 'punctuation',
              'combining', 'box_drawing', 'math_symbols', 'ipa_phonetic', 'currency_symbols',
              'caucasus', 'southeast_asian', 'technical_symbols', 'area_chart', 'pie_chart',
              'bar_chart', 'scatter', 'bubble', 'trendline', 'stacked', 'combo', 'stock',
              'dashboard_multi', 'legend', 'axis_labels', 'negative_values', 'large_dataset',
              'chart_sheet', 'multiple_charts', '3d_bar', 'date_axis']

scored = []
for r in results:
    name = r['name']
    ts = r.get('text_similarity', 1.0)
    vs = r.get('visual_avg', 1.0)
    pm = 1.0 if r.get('pages_match', True) else 0.0
    overall = ts * 0.4 + vs * 0.4 + pm * 0.2
    scored.append((overall, ts, vs, pm, name))

scored.sort()

# Actionable cases: no skip words, overall < 0.99
print('ACTIONABLE cases with overall < 0.99 (no chart/unicode/CJK/emoji/RTL):')
actionable = []
for overall, ts, vs, pm, name in scored:
    if overall >= 0.99:
        continue
    if any(w in name.lower() for w in skip_words):
        continue
    actionable.append((overall, ts, vs, pm, name))

for overall, ts, vs, pm, name in actionable:
    gap = 1.0 - overall
    # Identify main bottleneck
    text_gap = (1.0 - ts) * 0.4
    vis_gap = (1.0 - vs) * 0.4
    pg_gap = (1.0 - pm) * 0.2
    bottleneck = 'TEXT' if text_gap >= vis_gap and text_gap >= pg_gap else ('VIS' if vis_gap >= pg_gap else 'PAGE')
    print(f'  {name:45s} gap={gap:.4f} text={ts:.4f} vis={vs:.4f} pg={pm:.0f} [{bottleneck}]')
print(f'\n  Total gap: {sum(1.0 - o for o,t,v,p,n in actionable):.4f}')
print(f'  Count: {len(actionable)}')

# Also show top 10 cases with text_sim=1.0 but visual < 0.97 (pure visual targets)
print('\nPure VISUAL targets (text=1.0, vis<0.97):')
vis_targets = [(o,t,v,p,n) for o,t,v,p,n in scored if t == 1.0 and v < 0.97
               and not any(w in n.lower() for w in skip_words)]
for overall, ts, vs, pm, name in vis_targets:
    print(f'  {name:45s} vis={vs:.4f} gap={(1.0-vs)*0.4:.4f}')

# Top text targets (visual > 0.97 but text < 0.97)
print('\nPure TEXT targets (vis>0.97, text<0.97):')
text_targets = [(o,t,v,p,n) for o,t,v,p,n in scored if v > 0.97 and t < 0.97
                and not any(w in n.lower() for w in skip_words)]
for overall, ts, vs, pm, name in text_targets:
    print(f'  {name:45s} text={ts:.4f} gap={(1.0-ts)*0.4:.4f}')
