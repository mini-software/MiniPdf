"""Find cases where page count doesn't match — worth 0.2 per case."""
import json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))
for r in d:
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    if mp != rp:
        gain = 0.2 / 191
        print(f'{r["name"]:50s} minipdf={mp} ref={rp} score={r["overall_score"]:.4f} gain={gain:.6f}')
