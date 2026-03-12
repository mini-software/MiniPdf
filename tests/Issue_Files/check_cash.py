import json
with open('reports_xlsx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data.get('results', data)
for r in results:
    if 'cash' in r.get('name','').lower():
        print(f"Name: {r['name']}")
        print(f"Score: {r['overall_score']}")
        print(f"Text: {r['text_similarity']}")
        print(f"Visual avg: {r['visual_avg']}")
        print(f"Pages: MP={r['minipdf_pages']}, Ref={r['reference_pages']}")
        vs = r.get('visual_scores', [])
        for i, v in enumerate(vs):
            print(f"  Page {i+1} visual: {v:.4f}")
