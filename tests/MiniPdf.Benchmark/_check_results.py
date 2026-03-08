import json

d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))
scores = [c['overall_score'] for c in d]
below99 = [c for c in d if c['overall_score'] < 0.99]
print(f'Total: {len(d)}, Below99: {len(below99)}, Avg: {sum(scores)/len(scores):.4f}')

# Check key cases
key_names = [
    'docx_classic08_bullet_list',
    'docx_classic21_nested_lists',
    'docx_classic43_invoice_document', 
    'docx_classic107_order_form',
    'docx_classic108_comparison_matrix',
    'docx_classic50_long_table_with_formatting',
    'docx_classic35_tab_stops',
    'docx_classic109_release_notes',
    'docx_classic59_numbered_and_bullet_mixed',
    'docx_classic49_cjk_document',
    'docx_classic67_alternating_row_table',
    'docx_classic142_styled_invoice_document',
]
for c in d:
    if c['name'] in key_names:
        ts = c['text_similarity']
        vs = c['visual_avg'] 
        ov = c['overall_score']
        print(f'  {c["name"]:<50} text={ts:.4f} vis={vs:.4f} ovr={ov:.4f}')

# Count cases that improved vs previous (154 below-99, avg 0.9691))
# Previous numbers from conversation summary
prev_below99 = 154
prev_avg = 0.9691
print(f'\nChange: below99 {prev_below99}->{len(below99)} ({len(below99)-prev_below99:+d}), avg {prev_avg:.4f}->{sum(scores)/len(scores):.4f} ({sum(scores)/len(scores)-prev_avg:+.4f})')
