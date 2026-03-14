"""Check text extraction for classic37"""
import fitz

mini_path = "../MiniPdf.Scripts/pdf_output/classic37_freeze_panes.pdf"
ref_path = "reference_pdfs/classic37_freeze_panes.pdf"

for label, path in [("MiniPdf", mini_path), ("Reference", ref_path)]:
    doc = fitz.open(path)
    page = doc[0]
    text = page.get_text()
    lines = text.strip().split('\n')
    print(f"\n{label} text ({len(lines)} lines):")
    for line in lines[:20]:
        print(f"  {repr(line)}")
    
    # Also show text dict for position details
    td = page.get_text("dict")
    print(f"\n{label} spans details (first 10 spans):")
    count = 0
    for block in td['blocks']:
        if block['type'] == 0:
            for line in block['lines']:
                for span in line['spans']:
                    if count < 10:
                        print(f"  bbox=({span['bbox'][0]:.1f},{span['bbox'][1]:.1f},{span['bbox'][2]:.1f},{span['bbox'][3]:.1f}) text={repr(span['text'][:40])}")
                    count += 1
    doc.close()
