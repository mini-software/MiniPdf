import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic13_date_strings"

for label, path in [("MINI", f"../MiniPdf.Scripts/pdf_output/{name}.pdf"),
                     ("REF", f"reference_pdfs/{name}.pdf")]:
    d = fitz.open(path)
    print(f"=== {label} ===")
    for pi in range(d.page_count):
        for b in d[pi].get_text('dict')['blocks']:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        ox = s['origin'][0]
                        oy = s['origin'][1]
                        txt = s['text']
                        print(f"  x={ox:7.2f} y={oy:7.2f} [{txt}]")
    print()
