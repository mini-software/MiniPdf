"""Check MiniPdf chart output text for classic110."""
import fitz, os

for label, path in [('MiniPdf', os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic110_chart_with_legend.pdf')),
                     ('Ref', os.path.join('reference_pdfs', 'classic110_chart_with_legend.pdf'))]:
    if not os.path.exists(path):
        continue
    doc = fitz.open(path)
    page = doc[0]
    print(f"\n=== {label} - text ===")
    print(page.get_text("text"))
