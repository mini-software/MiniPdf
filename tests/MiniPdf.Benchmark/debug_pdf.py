import fitz
doc = fitz.open(r'..\MiniPdf.Scripts\pdf_output\classic01_basic_table_with_headers.pdf')
pg = doc[0]
cs = pg.get_contents()
for c in cs:
    raw = doc.xref_stream(c)
    if raw:
        txt = raw.decode('latin-1')[:4000]
        print(txt)
        break
