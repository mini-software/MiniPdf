"""Check which DOCX reference PDFs have border-like drawings that MiniPdf is missing."""
import json
import fitz

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

vis_failures = [r for r in data if r['visual_avg'] < 0.99]

results = []
for r in vis_failures:
    name = r['name']
    try:
        mini = fitz.open(f"../MiniPdf.Scripts/pdf_output_docx/{name}.pdf")
        ref = fitz.open(f"reference_pdfs_docx/{name}.pdf")
        
        mp = mini[0]
        rp = ref[0]
        
        mini_draws = mp.get_drawings()
        ref_draws = rp.get_drawings()
        
        # Count stroked-only paths (borders) vs filled paths
        ref_stroked = [d for d in ref_draws if d.get('color') and not d.get('fill')]
        mini_stroked = [d for d in mini_draws if d.get('color') and not d.get('fill')]
        
        ref_filled = [d for d in ref_draws if d.get('fill')]
        mini_filled = [d for d in mini_draws if d.get('fill')]
        
        # Check for unmatched stroked paths (potential borders)
        missing_stroked = len(ref_stroked) - len(mini_stroked)
        missing_filled = len(ref_filled) - len(mini_filled)
        
        if missing_stroked > 0 or missing_filled > 0:
            results.append((name, r['visual_avg'], len(mini_draws), len(ref_draws), 
                          len(mini_stroked), len(ref_stroked), len(mini_filled), len(ref_filled)))
        
        mini.close()
        ref.close()
    except Exception as e:
        pass

print(f"Cases with missing drawings ({len(results)} cases):")
print(f"{'Name':<55} {'Vis':>6} {'M_drw':>5} {'R_drw':>5} {'M_str':>5} {'R_str':>5} {'M_fil':>5} {'R_fil':>5}")
for name, vis, md, rd, ms, rs, mf, rf in sorted(results, key=lambda x: x[1]):
    print(f"{name:<55} {vis:>6.4f} {md:>5} {rd:>5} {ms:>5} {rs:>5} {mf:>5} {rf:>5}")
