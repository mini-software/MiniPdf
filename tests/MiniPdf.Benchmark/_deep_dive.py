"""Deep-dive on worst text failures to understand root causes."""
import json

d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))

# Target the worst text cases 
targets = [
    'classic35_tab_stops',
    'classic50_long_table',
    'classic64_multi_column',
    'classic49_cjk',
    'classic08_bullet',
    'classic21_nested_lists',
    'classic57_right_to_left',
    'classic68_sidebar',
    'classic44_memo',
    'classic32_superscript',
    'classic110_troubleshooting',
    'classic140_rotated',
    'classic24_two_column',
    'classic100_multi_page',
    'classic104_sop',
    'classic92_first_line',
    'classic105_certificate',
]

for c in d:
    name = c['name'].replace('docx_', '')
    matched = any(t in name for t in targets)
    if not matched:
        continue
    
    text = c['text_similarity']
    if text >= 0.99:
        continue
    
    diff = c.get('text_diff', '')
    flat = c.get('flat_text_similarity', 0)
    word = c.get('word_text_similarity', 0)
    
    print(f"\n{'='*80}")
    print(f"  {name}  text={text:.4f} flat={flat:.4f} word={word:.4f}  pages={c['minipdf_pages']}/{c['reference_pages']}")
    print(f"{'='*80}")
    if diff:
        lines = diff.strip().split('\n')
        for line in lines[:30]:
            print(f"  {line}")
        if len(lines) > 30:
            print(f"  ... ({len(lines)} total lines)")
    else:
        print("  (no diff)")
