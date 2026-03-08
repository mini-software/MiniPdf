"""Analyze all DOCX benchmark failures below 0.99, categorized by issue type."""
import json

d = json.load(open('reports_docx/comparison_report.json', 'r', encoding='utf-8'))

# Group by text vs visual issues
text_issues = []
vis_issues = []
both_issues = []
passing = []

for c in d:
    ovr = c['overall_score']
    text = c['text_similarity']
    vis = c['visual_avg']
    name = c['name'].replace('docx_', '')
    
    if ovr >= 0.99:
        passing.append(c)
        continue
    
    text_bad = text < 0.99
    vis_bad = vis < 0.99
    
    # Get text diff for diagnosis
    diff = c.get('text_diff', '')
    
    if text_bad and vis_bad:
        both_issues.append((name, text, vis, ovr, diff))
    elif text_bad:
        text_issues.append((name, text, vis, ovr, diff))
    elif vis_bad:
        vis_issues.append((name, text, vis, ovr, diff))

print(f"Passing (>=0.99): {len(passing)}")
print(f"Below 0.99: {len(text_issues) + len(vis_issues) + len(both_issues)}")
print(f"  Text only: {len(text_issues)}")
print(f"  Visual only: {len(vis_issues)}")
print(f"  Both: {len(both_issues)}")
print()

# Sort each group by overall score (ascending = worst first)
for label, items in [("TEXT ISSUES (text<0.99, vis>=0.99)", text_issues), 
                      ("VISUAL ISSUES (text>=0.99, vis<0.99)", vis_issues),
                      ("BOTH (text<0.99, vis<0.99)", both_issues)]:
    items.sort(key=lambda x: x[3])
    print(f"\n{'='*80}")
    print(f"  {label}: {len(items)} cases")
    print(f"{'='*80}")
    for name, text, vis, ovr, diff in items:
        print(f"  {name:55s} text={text:.4f} vis={vis:.4f} ovr={ovr:.4f}")
        # Show first few diff lines
        if diff:
            lines = diff.strip().split('\n')
            for line in lines[:6]:
                print(f"    {line}")
            if len(lines) > 6:
                print(f"    ... ({len(lines)} total lines)")
        print()
