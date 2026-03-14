import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for c in d:
    if 'classic132' in c['name']:
        print(f"classic132: vis={c.get('visual_avg',0):.4f} text={c.get('text_similarity',0):.4f}")
    if 'classic37' in c['name']:
        print(f"classic37: vis={c.get('visual_avg',0):.4f} text={c.get('text_similarity',0):.4f}")
    if 'classic18' in c['name'] and 'large' in c['name']:
        print(f"classic18: vis={c.get('visual_avg',0):.4f} text={c.get('text_similarity',0):.4f}")
scores = [c.get('text_similarity',0)*0.4 + c.get('visual_avg',0)*0.4 + (1.0 if c.get('minipdf_pages',0)==c.get('reference_pages',0) else 0.0)*0.2 for c in d]
avg = sum(scores)/len(scores)
print(f"\nAverage: {avg:.4f}, Count: {len(scores)}")
