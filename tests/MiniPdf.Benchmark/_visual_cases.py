"""Analyze specific visual-only failures to find implementable patterns."""
import fitz

cases = {
    'docx_classic58_dense_paragraph_document': 'text=1.0 vis=0.89 — dense text',
    'docx_classic65_code_block_styling': 'text=1.0 vis=0.91 — code blocks',
    'docx_classic69_blockquote_styling': 'text=1.0 vis=0.97 — blockquotes', 
    'docx_classic33_highlighted_text': 'text=1.0 vis=0.98 — text highlight',
    'docx_classic55_background_shading_paragraph': 'text=1.0 vis=0.87 — para shading',
    'docx_classic88_presentation_handout': 'text=1.0 vis=0.70 — presentation',
}

for name, desc in cases.items():
    print(f"\n{'='*60}")
    print(f"{name}: {desc}")
    try:
        mini = fitz.open(f"../MiniPdf.Scripts/pdf_output_docx/{name}.pdf")
        ref = fitz.open(f"reference_pdfs_docx/{name}.pdf")
        
        mp = mini[0]
        rp = ref[0]
        
        # Compare text content presence
        mt = mp.get_text()
        rt = rp.get_text()
        
        # Focus on structural differences
        mini_draws = mp.get_drawings()
        ref_draws = rp.get_drawings()
        mini_filled = [d for d in mini_draws if d.get('fill')]
        ref_filled = [d for d in ref_draws if d.get('fill')]
        mini_stroked = [d for d in mini_draws if d.get('color') and not d.get('fill')]
        ref_stroked = [d for d in ref_draws if d.get('color') and not d.get('fill')]
        
        print(f"  MiniPdf: {len(mini_filled)} filled, {len(mini_stroked)} stroked")
        print(f"  Ref:     {len(ref_filled)} filled, {len(ref_stroked)} stroked")
        
        # Show ref filled rect locations/colors
        if ref_filled:
            print(f"  Ref filled rects (first 5):")
            for d in ref_filled[:5]:
                r = d['rect']
                c = tuple(round(x*255) for x in d['fill'])
                print(f"    y={r[1]:.1f} x={r[0]:.1f} {r[2]-r[0]:.1f}×{r[3]-r[1]:.1f} color={c}")
        
        if ref_stroked and not mini_stroked:
            print(f"  Ref stroked paths (first 5):")
            for d in ref_stroked[:5]:
                r = d['rect']
                c = tuple(round(x*255) for x in d['color'])
                w = d.get('width', 1.0)
                print(f"    y={r[1]:.1f} x={r[0]:.1f} {r[2]-r[0]:.1f}×{r[3]-r[1]:.1f} color={c} w={w:.2f}")
        
        # First 200 chars of text comparison
        print(f"\n  Mini text (first 150 chars): {repr(mt[:150])}")
        print(f"  Ref text (first 150 chars):  {repr(rt[:150])}")
        
        mini.close()
        ref.close()
    except Exception as e:
        print(f"  Error: {e}")
