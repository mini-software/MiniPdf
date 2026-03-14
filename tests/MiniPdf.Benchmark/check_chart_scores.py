import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
charts = ['classic120','classic95','classic103','classic107','classic104','classic117',
          'classic109','classic111','classic115','classic102','classic110','classic99','classic93']
for r in d:
    n = r['name']
    for c in charts:
        if n.startswith(c):
            ts = r['text_similarity']
            vs = r['visual_avg']
            ov = r['overall_score']
            print(f"{n:45s} text={ts:.4f} vis={vs:.4f} overall={ov:.4f}")
