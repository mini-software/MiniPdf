"""Find page count mismatches."""
import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for x in d:
    pc = str(x.get('page_count_match', ''))
    if '/' in pc:
        a, b = pc.split('/')
        if a.strip() != b.strip():
            print(f"  {x['name']:48s}  pages={pc}  score={x['overall_score']:.4f}")
