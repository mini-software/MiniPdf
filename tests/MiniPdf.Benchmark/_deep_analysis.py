import json

# Load both reports
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    docx_data = json.load(f)

with open('reports/comparison_report.json', encoding='utf-8') as f:
    xlsx_data = json.load(f)

print("=" * 70)
print("DOCX ANALYSIS")
print("=" * 70)

docx_below99 = [r for r in docx_data if r['overall_score'] < 0.99]
print(f"Total: {len(docx_data)}, Below 99: {len(docx_below99)}, Avg: {sum(r['overall_score'] for r in docx_data)/len(docx_data):.4f}")

# Categorize DOCX below-99 by failure type
text_only = []
vis_only = []
both = []
page_only = []

for r in docx_below99:
    t = r['text_similarity']
    v = r['visual_avg']
    p = 1.0 if r['minipdf_pages'] == r['reference_pages'] else 0.0
    if t < 0.99 and v < 0.99:
        both.append(r)
    elif t < 0.99:
        text_only.append(r)
    elif v < 0.99:
        vis_only.append(r)
    else:
        page_only.append(r)

print(f"\nText-only failures: {len(text_only)}")
for r in sorted(text_only, key=lambda x: x['text_similarity']):
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

print(f"\nBoth text+visual failures: {len(both)}")
for r in sorted(both, key=lambda x: x['overall_score']):
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

print(f"\nVisual-only failures: {len(vis_only)}")
for r in sorted(vis_only, key=lambda x: x['visual_avg'])[:30]:
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
if len(vis_only) > 30:
    print(f"  ... and {len(vis_only)-30} more")

print(f"\nPage-only failures: {len(page_only)}")
for r in page_only:
    print(f"  {r['name']}: pages={r['minipdf_pages']}vs{r['reference_pages']} overall={r['overall_score']:.4f}")

print("\n" + "=" * 70)
print("XLSX ANALYSIS")
print("=" * 70)

xlsx_below99 = [r for r in xlsx_data if r['overall_score'] < 0.99]
print(f"Total: {len(xlsx_data)}, Below 99: {len(xlsx_below99)}, Avg: {sum(r['overall_score'] for r in xlsx_data)/len(xlsx_data):.4f}")

text_only_x = []
vis_only_x = []
both_x = []
page_only_x = []

for r in xlsx_below99:
    t = r['text_similarity']
    v = r['visual_avg']
    if t < 0.99 and v < 0.99:
        both_x.append(r)
    elif t < 0.99:
        text_only_x.append(r)
    elif v < 0.99:
        vis_only_x.append(r)
    else:
        page_only_x.append(r)

print(f"\nText-only failures: {len(text_only_x)}")
for r in sorted(text_only_x, key=lambda x: x['text_similarity']):
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

print(f"\nBoth text+visual failures: {len(both_x)}")
for r in sorted(both_x, key=lambda x: x['overall_score']):
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")

print(f"\nVisual-only failures: {len(vis_only_x)}")
for r in sorted(vis_only_x, key=lambda x: x['visual_avg'])[:20]:
    print(f"  {r['name']}: text={r['text_similarity']:.4f} vis={r['visual_avg']:.4f}")
if len(vis_only_x) > 20:
    print(f"  ... and {len(vis_only_x)-20} more")

print(f"\nPage-only failures: {len(page_only_x)}")
for r in page_only_x:
    print(f"  {r['name']}: pages={r['minipdf_pages']}vs{r['reference_pages']} overall={r['overall_score']:.4f}")

# Impact analysis: if we could fix all text-only to 0.99+
print("\n" + "=" * 70)
print("POTENTIAL IMPACT")
print("=" * 70)
for label, data_list, text_only_list, both_list in [
    ("DOCX", docx_data, text_only, both),
    ("XLSX", xlsx_data, text_only_x, both_x)
]:
    total_score = sum(r['overall_score'] for r in data_list)
    text_impact = 0
    for r in text_only_list + both_list:
        improvement = min(1.0, 0.99) - r['text_similarity']
        if improvement > 0:
            text_impact += improvement * 0.4
    new_avg = (total_score + text_impact) / len(data_list)
    print(f"{label}: Current avg={total_score/len(data_list):.4f}, If all text fixed: ~{new_avg:.4f} (+{text_impact/len(data_list):.4f})")
