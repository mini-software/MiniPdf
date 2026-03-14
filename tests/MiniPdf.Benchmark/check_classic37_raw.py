"""Dump raw PDF content for classic37 page 1"""
import fitz

for label, path in [("MiniPdf", "../MiniPdf.Scripts/pdf_output/classic37_freeze_panes.pdf"),
                     ("Reference", "reference_pdfs/classic37_freeze_panes.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    
    # Get raw content stream
    content = page.get_text("rawdict")
    
    print(f"\n=== {label} ===")
    print(f"Blocks: {len(content['blocks'])}")
    
    # Show first data row (row 2) blocks/lines/spans
    count = 0
    for block in content['blocks']:
        if block['type'] == 0:  # text
            for line in block['lines']:
                for span in line['spans']:
                    text = span['text']
                    bbox = span['bbox']
                    if count < 20:
                        print(f"  block line span: bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) text={repr(text[:50])} origin=({span.get('origin', (0,0))[0]:.1f},{span.get('origin', (0,0))[1]:.1f})")
                    count += 1
    
    doc.close()
