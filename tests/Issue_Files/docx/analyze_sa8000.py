"""Analyze SA8000 ch sample.docx structure"""
import zipfile
import xml.etree.ElementTree as ET

zf = zipfile.ZipFile('SA8000 ch sample.docx')
doc = zf.read('word/document.xml').decode('utf-8')
root = ET.fromstring(doc)
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

paras = root.findall('.//' + W + 'p')
print(f"Total paragraphs: {len(paras)}")

# Check section breaks
for i, p in enumerate(paras):
    ppr = p.find(W + 'pPr')
    if ppr is not None:
        sect = ppr.find(W + 'sectPr')
        if sect is not None:
            print(f"\nSection break at paragraph {i}")
            pgSz = sect.find(W + 'pgSz')
            pgMar = sect.find(W + 'pgMar')
            if pgSz is not None:
                w_val = pgSz.get(W + 'w')
                h_val = pgSz.get(W + 'h')
                print(f"  pgSz: w={w_val} ({float(w_val)/20:.1f}pt), h={h_val} ({float(h_val)/20:.1f}pt)")
            if pgMar is not None:
                for attr in ['top', 'bottom', 'left', 'right']:
                    val = pgMar.get(W + attr)
                    if val:
                        print(f"  margin-{attr}: {val} ({float(val)/20:.1f}pt)")

# Body section (last section)
body = root.find(W + 'body')
sect = body.find(W + 'sectPr')
if sect is not None:
    print(f"\nBody section (final):")
    pgSz = sect.find(W + 'pgSz')
    pgMar = sect.find(W + 'pgMar')
    if pgSz is not None:
        w_val = pgSz.get(W + 'w')
        h_val = pgSz.get(W + 'h')
        print(f"  pgSz: w={w_val} ({float(w_val)/20:.1f}pt), h={h_val} ({float(h_val)/20:.1f}pt)")
    if pgMar is not None:
        for attr in ['top', 'bottom', 'left', 'right']:
            val = pgMar.get(W + attr)
            if val:
                print(f"  margin-{attr}: {val} ({float(val)/20:.1f}pt)")

# Show paragraph text and spacing
print("\n--- Paragraph details ---")
for i, p in enumerate(paras):
    texts = []
    for t in p.iter(W + 't'):
        if t.text:
            texts.append(t.text)
    text = ''.join(texts)
    
    ppr = p.find(W + 'pPr')
    spacing_info = ""
    indent_info = ""
    style_id = ""
    if ppr is not None:
        pstyle = ppr.find(W + 'pStyle')
        if pstyle is not None:
            style_id = pstyle.get(W + 'val', '')
        spacing = ppr.find(W + 'spacing')
        if spacing is not None:
            before = spacing.get(W + 'before', '')
            after = spacing.get(W + 'after', '')
            line = spacing.get(W + 'line', '')
            lineRule = spacing.get(W + 'lineRule', '')
            if before or after or line:
                spacing_info = f" spacing(before={before}, after={after}, line={line}, rule={lineRule})"
        ind = ppr.find(W + 'ind')
        if ind is not None:
            left = ind.get(W + 'left', '')
            first = ind.get(W + 'firstLine', '')
            hanging = ind.get(W + 'hanging', '')
            if left or first or hanging:
                indent_info = f" indent(left={left}, first={first}, hanging={hanging})"
    
    display = text[:80] if text else "(empty)"
    print(f"  P{i}: [{style_id}]{spacing_info}{indent_info} {display}")
