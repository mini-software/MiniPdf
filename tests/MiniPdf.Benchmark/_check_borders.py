import json

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

below99 = [r for r in data if r['overall_score'] < 0.99]
print(f"Below 99: {len(below99)}")

# Check specific border cases
border_cases = ['classic135', 'classic34_paragraph', 'classic141', 'classic69', 'classic123', 'classic65_code', 'classic33_highlighted']
for r in data:
    for bc in border_cases:
        if bc in r['name']:
            print(f"  {r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

# Show all 0.99+ cases to count
above99 = [r for r in data if r['overall_score'] >= 0.99]
print(f"\nAbove 99: {len(above99)}")
