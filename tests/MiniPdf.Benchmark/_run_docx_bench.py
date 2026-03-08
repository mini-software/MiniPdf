import json, subprocess, sys

# Run comparison
print("Running DOCX comparison...")
result = subprocess.run(
    [sys.executable, "compare_pdfs.py",
     "--minipdf-dir", "../MiniPdf.Scripts/pdf_output_docx",
     "--reference-dir", "reference_pdfs_docx",
     "--report-dir", "reports_docx"],
    capture_output=True, text=True, encoding='utf-8', errors='replace'
)
lines = result.stdout.strip().split('\n')
for line in lines[-5:]:
    print(line)

# Stats
data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
scores = [x['overall_score'] for x in data if x.get('overall_score') is not None]
below99 = [s for s in scores if s < 0.99]
page_mismatch = [x for x in data if x.get('minipdf_pages') and x.get('reference_pages') and x['minipdf_pages'] != x['reference_pages']]

print(f"\n=== DOCX Results ===")
print(f"Total: {len(scores)}")
print(f"Below 99: {len(below99)}")
print(f"Average: {sum(scores)/len(scores):.4f}")
if below99:
    print(f"Below99 Avg: {sum(below99)/len(below99):.4f}")
print(f"Page mismatches: {len(page_mismatch)}")
for p in page_mismatch:
    print(f"  {p['name']}: {p['minipdf_pages']} vs {p['reference_pages']}")
