import json

# Load old and new reports
with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    new_data = json.load(f)

# Count below 99
below99 = [x for x in new_data if x.get('overall_score', 0) < 0.99]
above99 = [x for x in new_data if x.get('overall_score', 0) >= 0.99]
avg = sum(x.get('overall_score', 0) for x in new_data) / len(new_data) if new_data else 0

print(f"DOCX New Results: Total={len(new_data)}, Below99={len(below99)}, Above99={len(above99)}, Avg={avg:.4f}")
print()

# Show worst cases
print("=== Cases below 0.99 (sorted by score) ===")
for x in sorted(below99, key=lambda x: x.get('overall_score', 0)):
    name = x['name']
    overall = x.get('overall_score', 0)
    vis = x.get('visual_avg', 0)
    txt = x.get('text_similarity', 0)
    print(f"  {name}: overall={overall:.4f}, visual={vis:.4f}, text={txt:.4f}")
