import json, sys
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
name = sys.argv[1] if len(sys.argv) > 1 else 'classic09_long_text'
for d in data:
    if d['name'] == name:
        print(f"name: {d['name']}")
        print(f"pages: mini={d['minipdf_pages']}, ref={d['reference_pages']}")
        print(f"text_sim={d['text_similarity']:.4f}, vis_avg={d['visual_avg']:.4f}, overall={d['overall_score']:.4f}")
        diff = d.get('text_diff', '')
        if diff:
            print("Text diff:")
            for line in diff.split('\n'):
                print(f"  {line}")
        break
