import fitz, sys

c = 'classic13_date_strings'
mp = fitz.open(f'..\\MiniPdf.Scripts\\pdf_output\\{c}.pdf')
d = mp[0].get_text('dict', sort=True)

for bi, b in enumerate(d.get('blocks', [])):
    if b.get('type', 0) != 0:
        continue
    for li, l in enumerate(b.get('lines', [])):
        for si, s in enumerate(l.get('spans', [])):
            t = s.get('text', '')
            bbox = s.get('bbox', ())
            font = s.get('font', '')
            size = s.get('size', 0)
            if len(t.strip()) > 0:
                sys.stdout.write(f'span: [{t.strip()}] bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) font={font} size={size:.1f}\n')

mp.close()

mp.close()
