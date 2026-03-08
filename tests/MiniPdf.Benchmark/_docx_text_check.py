import json

cases = [
    "docx_classic94_custom_bullet_characters",
    "docx_classic24_two_column_table_layout",
    "docx_classic59_numbered_and_bullet_mixed",
    "docx_classic34_employee_directory_with_photo",
]

with open("reports_docx/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

for item in data:
    if item["name"] not in cases:
        continue
    print(f"\n{'='*70}")
    print(f"{item['name']}: text={item.get('text_similarity',0):.4f} vis={item.get('visual_avg',0):.4f}")
    diff = item.get("text_diff", "")
    if diff:
        lines = diff.split('\n')
        diff_lines = [l for l in lines if l.startswith('+') or l.startswith('-')]
        for l in diff_lines[:30]:
            print(l)
