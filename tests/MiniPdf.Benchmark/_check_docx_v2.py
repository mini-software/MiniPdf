import json

# Load new report 
with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    new_data = json.load(f)

new_map = {x['name']: x for x in new_data}

below99 = [x for x in new_data if x.get('overall_score', 0) < 0.99]
above99 = [x for x in new_data if x.get('overall_score', 0) >= 0.99]
avg = sum(x.get('overall_score', 0) for x in new_data) / len(new_data)

print(f"New DOCX: Total={len(new_data)}, Below99={len(below99)}, Above99={len(above99)}, Avg={avg:.4f}")
print()

# Focus on the top failing cases (most impactful to fix)
print("=== Worst 30 cases (most potential for improvement) ===")
for x in sorted(below99, key=lambda x: x.get('overall_score', 0))[:30]:
    name = x['name']
    overall = x.get('overall_score', 0)
    vis = x.get('visual_avg', 0)
    txt = x.get('text_similarity', 0)
    # Identify main issue
    issue = "visual" if vis < 0.95 and txt >= 0.99 else "text" if txt < 0.95 and vis >= 0.95 else "both"
    print(f"  {name}: overall={overall:.4f}, visual={vis:.4f}, text={txt:.4f} [{issue}]")
    if txt < 0.99 and x.get('text_diff') and x['text_diff'] != '(identical)':
        diff_lines = x['text_diff'].split('\n')[:8]
        for d in diff_lines:
            print(f"    {d[:100]}")
