import json

with open('../Issue_Files/reports_xlsx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

if isinstance(data, list):
    data = {'results': data}
print(type(data))
if 'results' in data:
    for r in data['results']:
        print(f"pages: {r['minipdf_pages']} vs {r['reference_pages']}")
        for p in r.get('page_scores', []):
            print(f"  pg {p['page']} px={p['pixel_score']:.3f} tx={p['text_score']:.3f}")
elif 'files' in data:
    for r in data['files']:
        print(f"pages: {r.get('minipdf_pages','?')} vs {r.get('reference_pages','?')}")
else:
    print(json.dumps(data, indent=2, ensure_ascii=False)[:3000])
