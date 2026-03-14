"""Dump raw PDF content for classic37 page 1 - using rawdict correctly"""
import fitz

for label, path in [("MiniPdf", "../MiniPdf.Scripts/pdf_output/classic37_freeze_panes.pdf"),
                     ("Reference", "reference_pdfs/classic37_freeze_panes.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    
    # Get text with dict format (not rawdict)
    content = page.get_text("dict")
    
    print(f"\n=== {label} ===")
    print(f"Blocks: {len(content['blocks'])}")
    
    # Show blocks structure
    count = 0
    for bi, block in enumerate(content['blocks']):
        if block['type'] == 0:  # text
            for li, line in enumerate(block['lines']):
                for si, span in enumerate(line['spans']):
                    text = span['text']
                    bbox = span['bbox']
                    if count < 20:
                        print(f"  b{bi}/l{li}/s{si}: bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) text={repr(text[:50])} origin=({span['origin'][0]:.1f},{span['origin'][1]:.1f})")
                    count += 1
    
    doc.close()
