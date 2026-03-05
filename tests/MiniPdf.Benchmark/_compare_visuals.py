"""Compare page sizes and content areas between MiniPdf and reference PDFs for visual-only cases."""
import fitz
import json

with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

# Visual-only failures
cases = ['classic145_status_badges', 'classic132_striped_table', 'classic134_heatmap',
         'classic137_checkerboard', 'classic138_color_grid', 'classic142_styled_invoice',
         'classic148_frozen_styled_grid', 'classic149_merged_styled_sections']

for name in cases[:5]:
    mp = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
    ref = f"reference_pdfs/{name}.pdf"
    try:
        doc_m = fitz.open(mp)
        doc_r = fitz.open(ref)
        
        print(f"\n=== {name} ===")
        for pi in range(min(len(doc_m), len(doc_r), 1)):
            pm = doc_m[pi]
            pr = doc_r[pi]
            print(f"  Page {pi+1}: mini={pm.rect.width:.1f}x{pm.rect.height:.1f} ref={pr.rect.width:.1f}x{pr.rect.height:.1f}")
            
            # Check drawing operations (lines, rectangles) in reference
            drawings_r = pr.get_drawings()
            drawings_m = pm.get_drawings()
            rects_r = [d for d in drawings_r if d['type'] == 'r' or (d.get('fill') and d.get('rect'))]
            rects_m = [d for d in drawings_m if d['type'] == 'r' or (d.get('fill') and d.get('rect'))]
            
            print(f"  Drawings: mini={len(drawings_m)}, ref={len(drawings_r)}")
            
            # Show first few filled rects from each
            filled_r = [d for d in drawings_r if d.get('fill')][:3]
            filled_m = [d for d in drawings_m if d.get('fill')][:3]
            
            if filled_r:
                print(f"  Ref filled rects (first 3):")
                for d in filled_r:
                    r = d['rect']
                    print(f"    pos=({r[0]:.1f},{r[1]:.1f}) size=({r[2]-r[0]:.1f}x{r[3]-r[1]:.1f}) color={d.get('fill')}")
            if filled_m:
                print(f"  Mini filled rects (first 3):")
                for d in filled_m:
                    r = d['rect']
                    print(f"    pos=({r[0]:.1f},{r[1]:.1f}) size=({r[2]-r[0]:.1f}x{r[3]-r[1]:.1f}) color={d.get('fill')}")

        doc_m.close()
        doc_r.close()
    except Exception as e:
        print(f"  ERROR: {e}")
