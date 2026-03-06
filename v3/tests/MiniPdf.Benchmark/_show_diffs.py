#!/usr/bin/env python3
"""Show text diffs for specific failing cases."""
import json

with open('d:/git/MiniPdf/tests/MiniPdf.Benchmark/reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

cases = [
    'classic15_negative_numbers',
    'classic40_scientific_notation', 
    'classic58_mixed_numeric_formats',
    'classic136_financial_report_styled',
    'classic129_alignment_combos',
    'classic57_cjk_only',
    'classic23_unicode_text',
    'classic150_kitchen_sink_styles',
]

for name in cases:
    matches = [x for x in data if x['name'] == name]
    if not matches:
        print(f'=== {name} NOT FOUND ===')
        continue
    r = matches[0]
    score = r['overall_score']
    tsim = r.get('text_similarity', 0)
    vavg = r.get('visual_avg', 0)
    print(f'=== {name} (score={score:.4f} text={tsim:.4f} vis={vavg:.4f}) ===')
    diff = r.get('text_diff', '')
    lines = diff.split('\n')[:30]
    for l in lines:
        print(f'  {l}')
    print()
