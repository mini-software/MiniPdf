import subprocess, json, os, sys

os.chdir(os.path.dirname(os.path.abspath(__file__)))

# Run comparison
print("Running comparison...")
result = subprocess.run(
    [sys.executable, "compare_pdfs.py", "--report-dir", "reports"],
    capture_output=True, text=True, encoding='utf-8', errors='replace'
)
# Print last few lines of stdout
lines = result.stdout.strip().split('\n')
for line in lines[-5:]:
    print(line)
if result.returncode != 0:
    print(f"STDERR: {result.stderr[-500:]}")

# Read report and compute stats
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
scores = [x['overall_score'] for x in data if x.get('overall_score') is not None]
below99 = [s for s in scores if s < 0.99]
page_mismatch = [x for x in data if x.get('minipdf_pages') and x.get('reference_pages') and x['minipdf_pages'] != x['reference_pages']]

print(f"\n=== XLSX Results ===")
print(f"Total: {len(scores)}")
print(f"Below 99: {len(below99)}")
print(f"Average: {sum(scores)/len(scores):.4f}")
if below99:
    print(f"Below99 Avg: {sum(below99)/len(below99):.4f}")
print(f"Page mismatches: {len(page_mismatch)}")

# Show biggest improvements/regressions vs baseline (0.9661 avg)
print("\nSample cases:")
for d2 in data:
    name = d2['name']
    if any(k in name for k in ['classic01_basic', 'classic134_heatmap', 'classic149_merged', 'classic44_emp']):
        print(f"  {name}: overall={d2.get('overall_score',0):.4f} text={d2.get('text_similarity',0):.4f} vis={d2.get('visual_avg',0):.4f}")
