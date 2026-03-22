import json
data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))

# Check previously-poor cases
for name in ['classic71','classic98','classic117']:
    r = [x for x in data if name in x.get('name','')]
    if r:
        d = r[0]
        score = d.get('overall', d.get('visual_avg', '?'))
        print(f"{d['name']}: score={score}, pages={d['minipdf_pages']}/{d['reference_pages']}")
    else:
        print(f"{name}: NOT FOUND")

# Find worst cases
scored = []
for x in data:
    s = x.get('overall', x.get('visual_avg', 0))
    scored.append((s, x['name'], x['minipdf_pages'], x['reference_pages']))
scored.sort()
print("\nWorst 10:")
for s, n, mp, rp in scored[:10]:
    print(f"  {n}: {s:.4f} pages={mp}/{rp}")

# Count by tier
exc = sum(1 for s,_,_,_ in scored if s >= 0.9)
good = sum(1 for s,_,_,_ in scored if 0.7 <= s < 0.9)
poor = sum(1 for s,_,_,_ in scored if s < 0.7)
print(f"\nExcellent (>=0.9): {exc}")
print(f"Good (0.7-0.9): {good}")
print(f"Poor (<0.7): {poor}")
print(f"Total: {len(scored)}")
