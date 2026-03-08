"""Compare filled rectangles between MiniPdf and reference for classic67 to understand the shading gap."""
import fitz

for name in ['docx_classic67_alternating_row_table']:
    mini = fitz.open(f"../MiniPdf.Scripts/pdf_output_docx/{name}.pdf")
    ref = fitz.open(f"reference_pdfs_docx/{name}.pdf")
    
    # Get all filled rects on page 1
    mp = mini[0]
    rp = ref[0]
    
    mini_draws = mp.get_drawings()
    ref_draws = rp.get_drawings()
    
    mini_filled = [d for d in mini_draws if d.get('fill')]
    ref_filled = [d for d in ref_draws if d.get('fill')]
    
    print(f"=== {name} ===")
    print(f"\nMiniPdf filled rectangles ({len(mini_filled)}):")
    for d in sorted(mini_filled, key=lambda x: (-x['rect'][1], x['rect'][0]))[:40]:
        r = d['rect']
        c = tuple(round(x*255) for x in d['fill'])
        print(f"  y={r[1]:.1f} x={r[0]:.1f} w={r[2]-r[0]:.1f} h={r[3]-r[1]:.1f} color={c}")
    
    print(f"\nReference filled rectangles ({len(ref_filled)}):")
    for d in sorted(ref_filled, key=lambda x: (-x['rect'][1], x['rect'][0]))[:40]:
        r = d['rect']
        c = tuple(round(x*255) for x in d['fill'])
        print(f"  y={r[1]:.1f} x={r[0]:.1f} w={r[2]-r[0]:.1f} h={r[3]-r[1]:.1f} color={c}")
    
    mini.close()
    ref.close()
