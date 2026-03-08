"""Check DOCX bullet-related cases before/after."""
import json

with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

new_map = {c['name']: c for c in data}

# Previous text scores for bullet cases
baseline = {
    'docx_classic08_bullet_list': 0.9242,
    'docx_classic21_nested_lists': 0.9408,
    'docx_classic44_memo': 0.9580,
    'docx_classic59_numbered_and_bullet_mixed': 0.9825,
    'docx_classic76_recipe_card': 0.9889,
    'docx_classic92_first_line_indent': 0.9868,
    'docx_classic50_long_table_with_formatting': 0.8153,
    'docx_classic64_multi_column_layout': 0.8837,
    'docx_classic49_cjk_document': 0.9057,
    'docx_classic104_sop_document': 0.9687,
    'docx_classic109_release_notes': 0.9785,
}

scores = [c['overall_score'] for c in data]
b99 = [c for c in data if c['overall_score'] < 0.99]
t99 = [c for c in b99 if c['text_similarity'] < 0.99]
print(f"Total:{len(data)} Below99:{len(b99)} Avg:{sum(scores)/len(scores):.4f} Text<99:{len(t99)}")

for name, old in sorted(baseline.items()):
    if name in new_map:
        new = new_map[name]['text_similarity']
        diff = new - old
        marker = '+' if diff > 0.005 else ('-' if diff < -0.005 else '=')
        no = new_map[name]['overall_score']
        print(f"{marker} {name:55s} text: {old:.4f} -> {new:.4f} ({diff:+.4f}) overall: {no:.4f}")
