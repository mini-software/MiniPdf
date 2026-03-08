import json

# Analyze DOCX report
with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    docx = json.load(f)

scores = [c['overall_score'] for c in docx]
below99 = [c for c in docx if c['overall_score'] < 0.99]
print(f"=== DOCX Report ===")
print(f"Total: {len(docx)}, Below99: {len(below99)}, Avg: {sum(scores)/len(scores):.4f}")

# Cases with text < 0.99 (fixable)
txt_fix = [c for c in below99 if c['text_similarity'] < 0.99]
print(f"\nText < 0.99 ({len(txt_fix)} cases):")
for c in sorted(txt_fix, key=lambda x: x['text_similarity']):
    name = c['name'].replace('docx_', '')
    print(f"  {name:50s} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")

# Cases with only visual issues (text >= 0.99)
vis_only = [c for c in below99 if c['text_similarity'] >= 0.99]
print(f"\nVisual-only issues (text>=0.99): {len(vis_only)} cases")

# Analyze XLSX report
print(f"\n=== XLSX Report ===")
with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    xlsx = json.load(f)

scores2 = [c['overall_score'] for c in xlsx]
below99x = [c for c in xlsx if c['overall_score'] < 0.99]
print(f"Total: {len(xlsx)}, Below99: {len(below99x)}, Avg: {sum(scores2)/len(scores2):.4f}")

txt_fix2 = [c for c in below99x if c['text_similarity'] < 0.99]
print(f"\nText < 0.99 ({len(txt_fix2)} cases):")
for c in sorted(txt_fix2, key=lambda x: x['text_similarity']):
    name = c['name']
    print(f"  {name:50s} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")

vis_only2 = [c for c in below99x if c['text_similarity'] >= 0.99]
print(f"\nVisual-only issues (text>=0.99): {len(vis_only2)} cases")

# Page count mismatches
page_mis = [c for c in below99x if c.get('page_score', 1.0) < 1.0]
print(f"\nPage count mismatches: {len(page_mis)} cases")
for c in sorted(page_mis, key=lambda x: x['overall_score']):
    name = c['name']
    print(f"  {name:50s} pages={c.get('page_count_minipdf','?')}/{c.get('page_count_reference','?')} text={c['text_similarity']:.4f} vis={c['visual_avg']:.4f} overall={c['overall_score']:.4f}")
