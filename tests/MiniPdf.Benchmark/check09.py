import json
d = json.load(open('reports/comparison_report.json'))
if isinstance(d, list):
    for item in d:
        name = item.get('name', '')
        if 'classic09' in name:
            print(f"classic09: overall={item.get('overall_score',0):.4f} txt={item.get('text_similarity',0):.3f} vis={item.get('visual_avg',0):.3f} pg={item.get('page_score',0):.1f} pages={item.get('minipdf_pages','?')}/{item.get('reference_pages','?')}")
            break
