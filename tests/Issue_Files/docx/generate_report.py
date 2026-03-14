"""Generate comparison_report.md and comparison_report.json for docx files."""
import sys, os, json, datetime
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..', 'MiniPdf.Benchmark'))

from compare_pdfs import compare_single

BASE_DIR = os.path.dirname(__file__)
REPORT_DIR = os.path.join(BASE_DIR, 'output_report')
IMAGES_DIR = os.path.join(REPORT_DIR, 'images')
os.makedirs(IMAGES_DIR, exist_ok=True)

# Test cases to compare
test_cases = ['Invoice', 'Support_Letter']
results = []

for name in test_cases:
    minipdf_pdf = os.path.join(BASE_DIR, 'output', f'{name}.pdf')
    reference_pdf = os.path.join(BASE_DIR, 'output_reference', f'{name}.pdf')
    if os.path.exists(minipdf_pdf) and os.path.exists(reference_pdf):
        result = compare_single(minipdf_pdf, reference_pdf, IMAGES_DIR, name)
        results.append(result)
        print(f"  {name}: overall={result.get('overall_score', 'N/A'):.4f}")

# ── Write JSON ───────────────────────────────────────────────────────────
json_path = os.path.join(REPORT_DIR, 'comparison_report.json')
with open(json_path, 'w', encoding='utf-8') as f:
    json.dump(results, f, indent=2, ensure_ascii=False)
print(f"Written: {json_path}")

# ── Write Markdown ───────────────────────────────────────────────────────
md_path = os.path.join(REPORT_DIR, 'comparison_report.md')

def score_icon(score):
    if score is None or score == 'N/A':
        return '⚪'
    if score >= 0.90:
        return '🟢'
    if score >= 0.80:
        return '🟡'
    return '🔴'

def fmt_score(val):
    if val is None:
        return 'N/A'
    if isinstance(val, float):
        return f'{val:.4f}'
    return str(val)

now = datetime.datetime.now().isoformat()

lines = []
lines.append('# MiniPdf vs Reference PDF Comparison Report\n')
lines.append(f'Generated: {now}\n')
lines.append('## Summary\n')
lines.append('| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |')
lines.append('|---|-----------|----------|------------|-------------|--------|')

valid_scores = []
for i, r in enumerate(results):
    name = r.get('name', '?')
    text_sim = r.get('text_similarity')
    vis_avg = r.get('visual_avg')
    mp = r.get('minipdf_pages', '?')
    rp = r.get('reference_pages', '?')
    overall = r.get('overall_score')
    icon = score_icon(overall)
    valid_scores.append(overall) if overall is not None else None
    lines.append(f'| {i+1} | {icon} {name} | {fmt_score(text_sim)} | {fmt_score(vis_avg)} | {mp}/{rp} | **{fmt_score(overall)}** |')

avg = sum(s for s in valid_scores if s) / len(valid_scores) if valid_scores else 0
lines.append(f'\n**Average Overall Score: {avg:.4f}**\n')

# Visual comparison section
lines.append('## Visual Comparison\n')
lines.append('<table>')

for r in results:
    name = r.get('name', '?')
    overall = r.get('overall_score')
    pct = f'{overall*100:.1f}%' if overall is not None else 'N/A'
    icon_color = '#3fb950' if overall and overall >= 0.90 else ('#d29922' if overall and overall >= 0.80 else '#f85149')
    score_span = f'<span style="color:{icon_color}">⬤</span> {pct}' if overall else 'N/A'

    lines.append(f'<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>')
    lines.append(f'<tr>')
    lines.append(f'  <td><b>{name}</b></td>')
    lines.append(f'  <td>{name} {score_span}</td>')
    lines.append(f'</tr>')

    diff_images = r.get('diff_images', [])
    if not diff_images:
        lines.append('<tr>')
        lines.append('  <td colspan="2"><i>No images</i></td>')
        lines.append('</tr>')
    else:
        for di in diff_images:
            mini_img = di.get('minipdf_img', '')
            ref_img = di.get('reference_img', '')
            lines.append('<tr>')
            lines.append(f'  <td><img src="images/{mini_img}" width="340" alt="MiniPdf"></td>')
            lines.append(f'  <td><img src="images/{ref_img}" width="340" alt="Reference"></td>')
            lines.append('</tr>')

lines.append('</table>\n')

with open(md_path, 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines) + '\n')
print(f"Written: {md_path}")
print(f"\nOverall score: {avg:.4f}")
