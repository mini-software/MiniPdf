"""Analyze the visual differences for top DOCX visual failures.
Try to identify the dominant visual features causing score drops."""
import json
import fitz  # PyMuPDF

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Get worst visual-only cases (text >= 0.99, low visual)
vis_failures = sorted(
    [r for r in data if r['text_similarity'] >= 0.99 and r['visual_avg'] < 0.90],
    key=lambda x: x['visual_avg']
)

for r in vis_failures[:10]:
    name = r['name']
    mini_path = f"../MiniPdf.Scripts/pdf_output_docx/{name}.pdf"
    ref_path = f"reference_pdfs_docx/{name}.pdf"
    
    print(f"\n{'='*60}")
    print(f"{name}: vis={r['visual_avg']:.4f}")
    try:
        mini = fitz.open(mini_path)
        ref = fitz.open(ref_path)
        
        # Compare page sizes
        for i in range(min(len(mini), len(ref))):
            mp = mini[i]
            rp = ref[i]
            print(f"  Page {i+1}: mini={mp.rect.width:.0f}x{mp.rect.height:.0f} ref={rp.rect.width:.0f}x{rp.rect.height:.0f}")
        
        # Check for images in reference
        ref_images = 0
        mini_images = 0
        for page in ref:
            ref_images += len(page.get_images())
        for page in mini:
            mini_images += len(page.get_images())
        print(f"  Images: mini={mini_images} ref={ref_images}")
        
        # Check for drawings/paths in reference (colored rectangles = shading)
        for i in range(min(1, len(ref))):
            rp = ref[i]
            mp = mini[i]
            # Get drawings
            ref_draws = rp.get_drawings()
            mini_draws = mp.get_drawings()
            ref_rects = [d for d in ref_draws if d['type'] == 'rect' or (d.get('items') and any(it[0] == 're' for it in d['items']))]
            mini_rects = [d for d in mini_draws if d['type'] == 'rect' or (d.get('items') and any(it[0] == 're' for it in d['items']))]
            print(f"  Page 1 drawings: mini={len(mini_draws)} ref={len(ref_draws)}")
            
            # Check filled vs stroked
            ref_filled = [d for d in ref_draws if d.get('fill')]
            mini_filled = [d for d in mini_draws if d.get('fill')]
            print(f"  Page 1 filled shapes: mini={len(mini_filled)} ref={len(ref_filled)}")
            
            # Show unique fill colors
            ref_colors = set()
            mini_colors = set()
            for d in ref_filled:
                c = d.get('fill')
                if c:
                    ref_colors.add(tuple(round(x*255) for x in c))
            for d in mini_filled:
                c = d.get('fill')
                if c:
                    mini_colors.add(tuple(round(x*255) for x in c))
            if ref_colors:
                print(f"  Ref fill colors: {sorted(ref_colors)[:10]}")
            if mini_colors:
                print(f"  Mini fill colors: {sorted(mini_colors)[:10]}")
            if ref_colors - mini_colors:
                print(f"  Missing in mini: {sorted(ref_colors - mini_colors)[:10]}")

        mini.close()
        ref.close()
    except Exception as e:
        print(f"  Error: {e}")
