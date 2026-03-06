import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
# Check specific chart cases affected by label changes
targets = ['classic93', 'classic95', 'classic107', 'classic102', 'classic108', 
           'classic116', 'classic120', 'classic91', 'classic115', 'classic110']
for x in data:
    for t in targets:
        if x['name'].startswith(t + '_'):
            print(f"{x['name']:45s} {x['overall_score']:.4f} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f}")
