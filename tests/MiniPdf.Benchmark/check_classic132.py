"""Check classic132 text extraction"""
import fitz

for label, path in [("MiniPdf", "../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf"),
                     ("Reference", "reference_pdfs/classic132_striped_table.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    text = page.get_text()
    lines = text.strip().split('\n')
    print(f"\n{label} text ({len(lines)} lines):")
    for line in lines[:15]:
        print(f"  {repr(line)}")
    
    td = page.get_text("dict")
    spans = []
    for block in td['blocks']:
        if block['type'] == 0:
            for line in block['lines']:
                for span in line['spans']:
                    spans.append(span)
    print(f"\nFirst 15 spans:")
    for s in spans[:15]:
        bbox = s['bbox']
        print(f"  bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) text={repr(s['text'][:40])}")
    doc.close()
