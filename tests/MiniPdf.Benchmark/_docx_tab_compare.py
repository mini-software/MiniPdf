"""Compare DOCX scores before and after tab fix."""
import json, sys

sys.stdout.reconfigure(encoding='utf-8')

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    current = json.load(f)

# Known baseline scores for key cases
baseline = {
    'docx_classic35_tab_stops': {'text': 0.8694, 'overall': 0.9416},
    'docx_classic08_bullet_list': {'text': 0.9180, 'overall': 0.9650},
    'docx_classic21_nested_lists': {'text': 0.9371, 'overall': 0.9708},
    'docx_classic50_long_table_with_formatting': {'text': 0.8153, 'overall': 0.8370},
    'docx_classic64_multi_column_layout': {'text': 0.8837, 'overall': 0.9297},
    'docx_classic44_memo': {'text': 0.9785, 'overall': 0.9765},
    'docx_classic104_sop_document': {'text': 0.9798, 'overall': 0.9413},
    'docx_classic49_cjk_document': {'text': 0.9057, 'overall': 0.9438},
}

cur = {r['name']: r for r in current}
print("=== DOCX score changes ===")
for name, base in sorted(baseline.items()):
    if name in cur:
        r = cur[name]
        ts = r.get('text_similarity', 1.0)
        os_ = r['overall_score']
        vs = r.get('visual_similarity', 1.0)
        ts_diff = ts - base['text']
        os_diff = os_ - base['overall']
        if abs(ts_diff) > 0.0001 or abs(os_diff) > 0.0001:
            print(f"  {name}: text {base['text']:.4f}->{ts:.4f} ({ts_diff:+.4f}), vis={vs:.4f}, overall {base['overall']:.4f}->{os_:.4f} ({os_diff:+.4f})")
        else:
            print(f"  {name}: unchanged t={ts:.4f} o={os_:.4f}")

# Also check all cases that changed from baseline (report != 0.9701)
total = len(current)
below99 = sum(1 for r in current if r['overall_score'] < 0.99)
avg = sum(r['overall_score'] for r in current) / total
print(f"\nTotal: {total}, Below99: {below99}, Avg: {avg:.4f}")
