import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Look at pages with visual details
for r in data:
    if r['overall_score'] >= 0.98 and r['overall_score'] < 0.99:
        name = r['name']
        text = r['text_similarity']
        vis = r['visual_avg']
        pages = r.get('minipdf_pages', 0)
        ref_pages = r.get('reference_pages', 0)
        
        # Check diff_images for details
        diffs = r.get('diff_images', [])
        
        print(f"{name}: overall={r['overall_score']:.4f} text={text:.4f} vis={vis:.4f} pages={pages}/{ref_pages}")
        if diffs:
            for d in diffs[:3]:
                if isinstance(d, dict):
                    print(f"  page: {d}")
                else:
                    print(f"  diff: {d}")
