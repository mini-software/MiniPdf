"""Categorize DOCX failures by root cause for targeted fixes."""
import json

d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))

# Classify each below-99 case
categories = {
    'text_only': [],       # text<0.99, vis>=0.99
    'vis_only_identical': [],  # vis<0.99, text identical
    'vis_only_textdiff': [],   # vis<0.99, text has minor diffs
    'both': [],            # both below 0.99
}

# For text issues, categorize by diff pattern
text_patterns = {
    'bullet': [],
    'word_wrap': [],
    'tab_stops': [],
    'cjk': [],
    'superscript': [],
    'missing_text': [],
    'other_text': [],
}

for c in d:
    ovr = c['overall_score']
    if ovr >= 0.99:
        continue
    
    text = c['text_similarity']
    vis = c['visual_avg']
    name = c['name'].replace('docx_', '')
    diff = c.get('text_diff', '')
    pages_m = c['minipdf_pages']
    pages_r = c['reference_pages']
    
    # Determine category
    if text < 0.99 and vis >= 0.99:
        categories['text_only'].append((name, text, vis, ovr))
    elif text >= 0.99 and vis < 0.99:
        if not diff.strip() or diff.strip() == '(identical)':
            categories['vis_only_identical'].append((name, text, vis, ovr))
        else:
            categories['vis_only_textdiff'].append((name, text, vis, ovr, diff[:200]))
    else:
        categories['both'].append((name, text, vis, ovr, pages_m, pages_r, diff[:200]))
    
    # Text pattern classification for text<0.99 cases
    if text < 0.99:
        if 'bullet' in name or 'list' in name:
            text_patterns['bullet'].append((name, text))
        elif 'tab' in name:
            text_patterns['tab_stops'].append((name, text))
        elif 'cjk' in name:
            text_patterns['cjk'].append((name, text))
        elif 'super' in name or 'sub' in name:
            text_patterns['superscript'].append((name, text))
        elif pages_m != pages_r:
            text_patterns['missing_text'].append((name, text, pages_m, pages_r))
        else:
            text_patterns['other_text'].append((name, text))

print("=== SUMMARY ===")
print(f"Text only (text<0.99, vis>=0.99): {len(categories['text_only'])}")
print(f"Visual only (text identical):     {len(categories['vis_only_identical'])}")
print(f"Visual only (text has diffs):     {len(categories['vis_only_textdiff'])}")
print(f"Both issues:                      {len(categories['both'])}")
print()

print("=== TEXT PATTERN BREAKDOWN (text<0.99 cases) ===")
for pat, items in text_patterns.items():
    if items:
        print(f"\n  {pat.upper()}: {len(items)} cases")
        for item in sorted(items, key=lambda x: x[1]):
            if len(item) == 4:
                print(f"    {item[0]:55s} text={item[1]:.4f} pages={item[2]}/{item[3]}")
            else:
                print(f"    {item[0]:55s} text={item[1]:.4f}")

print("\n=== BOTH ISSUES (sorted by overall score) ===")
for name, text, vis, ovr, pm, pr, diff in sorted(categories['both'], key=lambda x: x[3]):
    pg_info = f" PAGES:{pm}/{pr}" if pm != pr else ""
    print(f"  {name:55s} text={text:.4f} vis={vis:.4f} ovr={ovr:.4f}{pg_info}")

print(f"\n=== VIS-ONLY WITH TEXT DIFFS: {len(categories['vis_only_textdiff'])} cases ===")
for name, text, vis, ovr, diff in sorted(categories['vis_only_textdiff'], key=lambda x: x[3])[:10]:
    print(f"  {name:55s} text={text:.4f} vis={vis:.4f} ovr={ovr:.4f}")

# Count how many are just visual (font/color/alignment differences we can't fix with Helvetica)
vis_only_count = len(categories['vis_only_identical']) + len(categories['vis_only_textdiff'])
both_count = len(categories['both'])
text_only_count = len(categories['text_only'])
print(f"\n=== POTENTIAL FOR IMPROVEMENT ===")
print(f"  Visual-only issues (mostly unfixable with Helvetica): {vis_only_count}")
print(f"  Text issues (fixable):                                {text_only_count + both_count}")
print(f"  Total fixable cases:                                  {text_only_count + both_count}")
