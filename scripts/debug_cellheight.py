import zipfile, xml.etree.ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
z = zipfile.ZipFile(r'tests\Issue_Files\docx\20260318_issue.docx')
styles_xml = ET.parse(z.open('word/styles.xml')).getroot()

# BluePrism cell margins
for s in styles_xml.findall(f'{W}style'):
    if s.get(f'{W}type') == 'table' and 'BluePrism' in (s.get(f'{W}styleId') or ''):
        tblPr = s.find(f'{W}tblPr')
        cellMar = tblPr.find(f'{W}tblCellMar') if tblPr is not None else None
        if cellMar is not None:
            for side in ('top', 'bottom', 'left', 'right'):
                el = cellMar.find(f'{W}{side}')
                if el is not None:
                    w = el.get(f'{W}w')
                    print(f"  {side}: {w}tw = {int(w)/20:.1f}pt")
        # Also check basedOn
        basedOn = s.find(f'{W}basedOn')
        if basedOn is not None:
            basedOnSid = basedOn.get(f'{W}val')
            print(f"  basedOn: {basedOnSid}")
            # Find base style
            for s2 in styles_xml.findall(f'{W}style'):
                if s2.get(f'{W}styleId') == basedOnSid:
                    tblPr2 = s2.find(f'{W}tblPr')
                    cellMar2 = tblPr2.find(f'{W}tblCellMar') if tblPr2 is not None else None
                    if cellMar2 is not None:
                        for side in ('top', 'bottom', 'left', 'right'):
                            el = cellMar2.find(f'{W}{side}')
                            if el is not None:
                                w = el.get(f'{W}w')
                                print(f"  {basedOnSid} {side}: {w}tw = {int(w)/20:.1f}pt")

# Check the document-level table properties
doc = ET.parse(z.open('word/document.xml')).getroot()
body = doc.find(f'{W}body')
first_table = body.findall(f'.//{W}tbl')[0]
tblPr = first_table.find(f'{W}tblPr')
cellMar = tblPr.find(f'{W}tblCellMar') if tblPr is not None else None
if cellMar is not None:
    print("\nDocument-level table cellMar:")
    for side in ('top', 'bottom', 'left', 'right'):
        el = cellMar.find(f'{W}{side}')
        if el is not None:
            w = el.get(f'{W}w')
            print(f"  {side}: {w}tw = {int(w)/20:.1f}pt")
else:
    print("\nNo document-level table cellMar (uses style)")

# Now calculate expected data row height
# With BluePrism style: top=28tw(1.4pt), bottom=28tw(1.4pt)
# With TableNormal: top=0, bottom=0, left=108(5.4), right=108(5.4)
# BluePrism basedOn TableNormal, but BluePrism has its own cellMar
# So effective: top=1.4pt, bottom=1.4pt, left=5.4pt, right=5.4pt

print("\n--- Row height calculations ---")
cellPadV = 1.4  # top + bottom = 2.8pt
fontSize = 11.0
metricsFactor = 1.17

# With table style line=240 (1.0x)
lineH_tbl = fontSize * metricsFactor * 1.0
print(f"LinH (table 1.0x): {lineH_tbl:.2f}pt")
print(f"  Data row = {cellPadV*2 + lineH_tbl:.2f}pt (SA=0)")

# With Normal line=259 (1.079x)
lineH_norm = fontSize * metricsFactor * 259/240
print(f"\nLinH (Normal 1.079x): {lineH_norm:.2f}pt")
print(f"  Data row = {cellPadV*2 + lineH_norm:.2f}pt (SA=0)")
print(f"  Data row = {cellPadV*2 + lineH_norm + 8:.2f}pt (SA=8)")

# Reference data row
print(f"\nReference data row: 20.2pt")
print(f"  Implied content = 20.2 - 2.8 = 17.4pt")
print(f"  Implied factor at 1.0x lnSp: {17.4/11:.4f}")
print(f"  Implied factor at 1.079x lnSp: {17.4/11/1.079:.4f}")
