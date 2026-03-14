"""Compare column positions for classic18"""
import fitz
import collections

mini_path = "../MiniPdf.Scripts/pdf_output/classic18_large_dataset.pdf"
ref_path = "reference_pdfs/classic18_large_dataset.pdf"

mini_doc = fitz.open(mini_path)
ref_doc = fitz.open(ref_path)

# Page 1 - collect all unique X positions of text
for label, doc in [("MiniPdf", mini_doc), ("Reference", ref_doc)]:
    page = doc[0]
    text = page.get_text("dict")
    
    x_positions = []
    spans_by_x = collections.defaultdict(list)
    for block in text['blocks']:
        if block['type'] == 0:
            for line in block['lines']:
                for span in line['spans']:
                    x0 = round(span['bbox'][0], 1)
                    x_positions.append(x0)
                    spans_by_x[x0].append(span['text'][:20])
    
    unique_x = sorted(set(x_positions))
    print(f"\n{label} - Page 1 unique X positions ({len(unique_x)}):")
    for x in unique_x:
        samples = spans_by_x[x][:3]
        sample_text = ', '.join(f'"{s}"' for s in samples)
        print(f"  x={x:7.1f}  ({len(spans_by_x[x]):3d} spans) e.g. {sample_text}")
    
    # Also show column widths (gaps between consecutive X positions)
    print(f"\n{label} - Column gaps:")
    for i in range(len(unique_x) - 1):
        gap = unique_x[i+1] - unique_x[i]
        print(f"  {unique_x[i]:7.1f} -> {unique_x[i+1]:7.1f}  gap={gap:.1f}")

mini_doc.close()
ref_doc.close()
