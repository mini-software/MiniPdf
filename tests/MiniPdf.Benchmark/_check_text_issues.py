import json

with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Focus on cases where text_similarity < 0.99 (where code changes can help most)
text_issues = [x for x in data if x.get('text_similarity', 0) < 0.99 and x.get('overall_score', 0) < 0.99]
text_issues.sort(key=lambda x: x.get('text_similarity', 0))

print(f"Cases with text_similarity < 0.99: {len(text_issues)}")
print()
for x in text_issues[:25]:
    name = x['name']
    txt = x.get('text_similarity', 0)
    vis = x.get('visual_avg', 0)
    overall = x.get('overall_score', 0)
    diff = x.get('text_diff', '')[:500].replace('\n', '\n    ')
    print(f"\n{name}: text={txt:.4f}, visual={vis:.4f}, overall={overall:.4f}")
    if diff and diff != '(identical)':
        print(f"  diff:\n    {diff}")
