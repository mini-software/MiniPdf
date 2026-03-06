import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Chart cases with text score < 0.95 that could benefit from systematic fixes
targets = ['classic91', 'classic93', 'classic95', 'classic102', 'classic107', 
           'classic110', 'classic111', 'classic115', 'classic120', 'classic103']
for x in data:
    for t in targets:
        if x['name'].startswith(t + '_'):
            print(f"\n=== {x['name']} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f} ===")
            if x.get('text_diff'):
                diff = x['text_diff']
                lines = diff.split('\n')
                shown = 0
                for line in lines:
                    if line.startswith('+') or line.startswith('-'):
                        if not line.startswith('+++') and not line.startswith('---'):
                            print(line[:150])
                            shown += 1
                            if shown >= 20:
                                print("... (truncated)")
                                break
