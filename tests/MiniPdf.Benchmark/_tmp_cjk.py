import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

for x in data:
    if x['name'] == 'classic57_cjk_only':
        print(f"=== {x['name']} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f} pages: mini={x['minipdf_pages']} ref={x['reference_pages']} ===")
        if x.get('text_diff'):
            diff = x['text_diff']
            lines = diff.split('\n')
            for i, line in enumerate(lines):
                if i > 80: 
                    print("... (truncated)")
                    break
                print(line[:200])
