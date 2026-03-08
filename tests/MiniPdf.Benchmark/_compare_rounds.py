import json

# Load the current benchmark results
with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Key cases to check
key_cases = [
    'docx_classic08_bullet_list',
    'docx_classic21_nested_lists', 
    'docx_classic35_tab_stops',
    'docx_classic49_cjk_document',
    'docx_classic59_numbered_and_bullet_mixed',
    'docx_classic109_release_notes',
    'docx_classic110_troubleshooting_guide',
    'docx_classic108_comparison_matrix',
    'docx_classic142_styled_invoice_document',
    'docx_classic67_alternating_row_table',
    'docx_classic43_invoice_document',
    'docx_classic107_order_form',
    'docx_classic50_long_table_with_formatting',
]

# Previous scores (from analysis before changes)
prev = {
    'docx_classic08_bullet_list': (0.918, 0.9945, 0.9650),
    'docx_classic21_nested_lists': (0.9371, 0.9902, 0.9709),
    'docx_classic35_tab_stops': (0.7619, 0.9896, 0.9006),
    'docx_classic49_cjk_document': (0.9057, 0.9538, 0.9438),
    'docx_classic59_numbered_and_bullet_mixed': (0.9823, 0.9718, 0.9816),
    'docx_classic109_release_notes': (0.9780, 0.9619, 0.9760),
    'docx_classic110_troubleshooting_guide': (0.9529, 0.9619, 0.9659),
    'docx_classic108_comparison_matrix': (0.9630, 0.9009, 0.9456),
    'docx_classic142_styled_invoice_document': (0.9647, 0.8874, 0.9408),
    'docx_classic67_alternating_row_table': (0.9832, 0.7969, 0.9120),
    'docx_classic43_invoice_document': (0.9775, 0.9355, 0.9652),
    'docx_classic107_order_form': (0.9787, 0.9190, 0.9591),
    'docx_classic50_long_table_with_formatting': (0.7774, 0.7770, 0.8218),
}

print(f"{'Case':<50} {'Prev Text':>10} {'Now Text':>10} {'Prev Vis':>10} {'Now Vis':>10} {'Prev Ovr':>10} {'Now Ovr':>10} {'Delta':>8}")
print("-" * 160)

for case in data:
    name = case['case_name']
    if name in prev:
        pt, pv, po = prev[name]
        nt = case.get('text_similarity', 0)
        nv = case.get('visual_avg', 0)
        no = case.get('overall_score', 0)
        delta = no - po
        marker = '++' if delta > 0.005 else '--' if delta < -0.005 else '  '
        print(f"{name:<50} {pt:>10.4f} {nt:>10.4f} {pv:>10.4f} {nv:>10.4f} {po:>10.4f} {no:>10.4f} {delta:>+8.4f} {marker}")

# Also show all cases that changed significantly
print("\n\nAll cases with overall score change > 0.005:")
overall_prev_avg = sum(v[2] for v in prev.values()) / len(prev)
for case in data:
    name = case['case_name']
    if name in prev:
        po = prev[name][2]
        no = case.get('overall_score', 0)
        if abs(no - po) > 0.005:
            print(f"  {name}: {po:.4f} -> {no:.4f} ({no-po:+.4f})")

# Show new below-99 count and avg
scores = [c['overall_score'] for c in data]
below99 = [c for c in data if c['overall_score'] < 0.99]
above99 = [c for c in data if c['overall_score'] >= 0.99]
print(f"\nBelow 0.99: {len(below99)}, Above 0.99: {len(above99)}")
print(f"Average: {sum(scores)/len(scores):.4f}")
