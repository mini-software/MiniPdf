import json, sys

def analyze(path, label):
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    below99 = [x for x in data if x.get('overall_score', 0) < 0.99]
    print(f"\n{'='*60}")
    print(f"{label}: Total={len(data)}, Below99={len(below99)}")
    print(f"{'='*60}")
    for x in sorted(below99, key=lambda x: x.get('overall_score', 0)):
        name = x['name']
        overall = x.get('overall_score', 0)
        vis = x.get('visual_avg', 0)
        txt = x.get('text_similarity', 0)
        print(f"\n  {name}: overall={overall}, visual={vis}, text={txt}")
        if x.get('text_diff') and x['text_diff'] != '(identical)':
            diff = x['text_diff'][:400].replace('\n', '\n    ')
            print(f"    diff: {diff}")
        if x.get('suggestions'):
            sugg = str(x['suggestions'])[:400]
            print(f"    suggestions: {sugg}")

analyze('reports/comparison_report.json', 'XLSX Report')
analyze('reports_docx/comparison_report.json', 'DOCX Report')
