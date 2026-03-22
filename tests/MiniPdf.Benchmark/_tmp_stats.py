import json, sys
with open(r'd:\git\MiniPdf-v2\tests\MiniPdf.Benchmark\reports_docx\comparison_report.json') as f:
    data = json.load(f)
total = len(data)
scores = [d['overall_score'] for d in data if d.get('overall_score') is not None]
scored = len(scores)
nones = total - scored
avg = sum(scores) / scored if scored else 0
exc = sum(1 for s in scores if s >= 0.9)
good = sum(1 for s in scores if 0.7 <= s < 0.9)
poor = sum(1 for s in scores if s < 0.7)
ranked = sorted([(d['name'], d['overall_score']) for d in data if d.get('overall_score') is not None], key=lambda x: x[1])
sys.stderr.write(f"=== STATS ===\n")
sys.stderr.write(f"Total: {total}\n")
sys.stderr.write(f"Scored: {scored}, N/A: {nones}\n")
sys.stderr.write(f"Average: {avg:.4f}\n")
sys.stderr.write(f"Excellent(>=0.9): {exc}\n")
sys.stderr.write(f"Good(0.7-0.9): {good}\n")
sys.stderr.write(f"Poor(<0.7): {poor}\n")
sys.stderr.write(f"Bottom 10:\n")
for i, (n, s) in enumerate(ranked[:10], 1):
    sys.stderr.write(f"  {i:2}. {n}: {s}\n")
sys.stderr.write(f"=== END ===\n")
