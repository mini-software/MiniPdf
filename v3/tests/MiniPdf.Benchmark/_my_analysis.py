import json

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

below = [x for x in data if x.get('overall_score', 0) < 0.99]
below.sort(key=lambda x: x.get('overall_score', 0))

for x in below:
    name = x['name']
    score = x.get('overall_score', 0)
    vis = x.get('visual_avg', 0)
    txt = x.get('text_similarity', 0)
    mp = x.get('minipdf_pages', 0)
    rp = x.get('reference_pages', 0)
    td = x.get('text_diff', '')

    print(f"=== {name} (score={score:.4f}, vis={vis:.4f}, txt={txt:.4f}, pages={mp}/{rp}) ===")
    if td and td != '(identical)':
        lines = td.split('\n')[:30]
        for l in lines:
            print(f"  | {l}")
    else:
        print("  (text identical)")
    print()
