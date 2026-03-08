import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
scores = [x['overall_score'] for x in data if x.get('overall_score') is not None]
below99 = [s for s in scores if s < 0.99]
page_mismatch = [x for x in data if x.get('minipdf_pages') and x.get('reference_pages') and x['minipdf_pages'] != x['reference_pages']]

print(f"Total: {len(scores)}")
print(f"Below 99: {len(below99)}")
print(f"Average: {sum(scores)/len(scores):.4f}")
print(f"Below99 Avg: {sum(below99)/len(below99):.4f}")
print(f"Page mismatches: {len(page_mismatch)}")
for p in page_mismatch:
    print(f"  {p['name']}: {p['minipdf_pages']} vs {p['reference_pages']}")

# Show specific cases
print("\nSample cases:")
for d2 in data:
    name = d2['name']
    if any(k in name for k in ['heatmap', 'classic01_', 'classic134', 'classic44', 'classic149', 'classic09_']):
        os2 = d2.get('overall_score', 0)
        ts = d2.get('text_similarity', 0)
        va = d2.get('visual_avg', 0)
        mp = d2.get('minipdf_pages', 0)
        rp = d2.get('reference_pages', 0)
        print(f"  {name}: overall={os2:.4f} text={ts:.4f} vis={va:.4f} pages={mp}/{rp}")
