import json
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
b = [r for r in data if r['overall_score'] < 0.99]
print(f"Below 99: {len(b)}")
avg = sum(r['overall_score'] for r in data) / len(data)
print(f"Avg: {avg:.4f}")

for name_part in ['classic08_bullet', 'classic21_nested', 'classic59_numbered', 'classic109_release', 'classic60_project_status']:
    for r in data:
        if name_part in r['name']:
            print(f"  {r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
            break
