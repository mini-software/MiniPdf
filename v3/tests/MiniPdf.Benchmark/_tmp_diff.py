import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Look at cases closest to 0.99 with text diffs
targets = ['classic78', 'classic66', 'classic86', 'classic49', 'classic51', 
           'classic83', 'classic81', 'classic64', 'classic65', 'classic44']
for x in data:
    for t in targets:
        if t in x['name']:
            print(f"\n=== {x['name']} overall={x['overall_score']:.4f} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f} ===")
            if x.get('text_diff'):
                diff = x['text_diff']
                lines = diff.split('\n')
                # Show first 15 diff lines
                shown = 0
                for line in lines:
                    if line.startswith('+') or line.startswith('-'):
                        if not line.startswith('+++') and not line.startswith('---'):
                            print(line[:120])
                            shown += 1
                            if shown >= 10:
                                break
