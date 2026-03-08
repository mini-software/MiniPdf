"""Compare visual details of worst DOCX case — classic88_presentation_handout"""
import fitz
import numpy as np

ref_path = "reference_pdfs_docx/docx_classic88_presentation_handout.pdf"
mini_path = "../MiniPdf.Scripts/pdf_output_docx/docx_classic88_presentation_handout.pdf"

for label, path in [("REFERENCE", ref_path), ("MINIPDF", mini_path)]:
    doc = fitz.open(path)
    print(f"\n=== {label}: {doc.page_count} pages ===")
    for i in range(doc.page_count):
        page = doc[i]
        # Extract drawings/shapes
        drawings = page.get_drawings()
        rects = [d for d in drawings if d['type'] == 'r']  # rectangles
        # Print first 20 colored rectangles
        colored_rects = [d for d in drawings if d.get('fill')]
        print(f"  Page {i+1}: {len(drawings)} drawings, {len(colored_rects)} filled")
        for d in colored_rects[:10]:
            r = d['rect']
            fill = d.get('fill')
            print(f"    rect={r} fill={fill}")
        
        # Get page as image and look at unique colors
        pix = page.get_pixmap(matrix=fitz.Matrix(1, 1))  # low res
        img = np.frombuffer(pix.samples, dtype=np.uint8).reshape(pix.h, pix.w, pix.n)
        # Find unique non-white, non-black colors
        mask = ~((img[:,:,0] > 250) & (img[:,:,1] > 250) & (img[:,:,2] > 250))
        mask &= ~((img[:,:,0] < 5) & (img[:,:,1] < 5) & (img[:,:,2] < 5))
        if mask.any():
            unique_colors = set()
            colored_pixels = img[mask]
            # Sample to avoid too many
            step = max(1, len(colored_pixels) // 100)
            for px in colored_pixels[::step]:
                unique_colors.add(tuple(px[:3]))
            print(f"    Unique non-BW colors: {len(unique_colors)} (sampled)")
            for c in sorted(unique_colors)[:10]:
                print(f"      RGB({c[0]}, {c[1]}, {c[2]})")
    doc.close()
