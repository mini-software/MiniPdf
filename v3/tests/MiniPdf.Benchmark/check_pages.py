import json

data = json.load(open('reports/comparison_report.json'))
print("=== Page mismatches ===")
for x in data:
    if x['minipdf_pages'] != x['reference_pages']:
        n = x['name']
        mp = x['minipdf_pages']
        rp = x['reference_pages'] 
        o = x['overall_score']
        t = x['text_similarity']
        v = x['visual_avg']
        print(f"  {n}: {mp}p vs {rp}p  overall={o:.4f} txt={t:.3f} vis={v:.3f}")

print()
print("=== classic09 ===")
for x in data:
    if 'classic09' in x['name']:
        n = x['name']
        mp = x['minipdf_pages']
        rp = x['reference_pages']
        o = x['overall_score']
        t = x['text_similarity']
        v = x['visual_avg']
        print(f"  {n}: {mp}p vs {rp}p  overall={o:.4f} txt={t:.3f} vis={v:.3f}")
