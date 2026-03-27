import zipfile, xml.etree.ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
z = zipfile.ZipFile(r'tests\Issue_Files\docx\20260318_issue.docx')

# Check all styles for HasExplicitSpacingAfter
styles_xml = ET.parse(z.open('word/styles.xml')).getroot()

# Get docDefaults SA
dd = styles_xml.find(f'{W}docDefaults')
dd_pPr = dd.find(f'{W}pPrDefault/{W}pPr/{W}spacing') if dd is not None else None
dd_after = dd_pPr.get(f'{W}after') if dd_pPr is not None else None
print(f"docDefaults after={dd_after}")

# Track styles with explicit SA
for s in styles_xml.findall(f'{W}style'):
    stype = s.get(f'{W}type')
    sid = s.get(f'{W}styleId')
    if stype != 'paragraph': continue
    pPr = s.find(f'{W}pPr')
    spacing = pPr.find(f'{W}spacing') if pPr is not None else None
    has_after = spacing is not None and spacing.get(f'{W}after') is not None
    after_val = spacing.get(f'{W}after') if spacing is not None else None
    basedOn = s.find(f'{W}basedOn')
    basedOnVal = basedOn.get(f'{W}val') if basedOn is not None else None
    if has_after or sid in ('Normal', 'TableHeading', 'Header', 'Footer', 'Heading1', 'Heading2'):
        print(f"  Style '{sid}': basedOn={basedOnVal}, hasExplicitSA={has_after}, after={after_val}")

print()
# Now simulate the override logic for first table
doc = ET.parse(z.open('word/document.xml')).getroot()
body = doc.find(f'{W}body')
first_table = body.findall(f'.//{W}tbl')[0]
tblPr = first_table.find(f'{W}tblPr')
tblStyle = tblPr.find(f'{W}tblStyle')
print(f"Table style: {tblStyle.get(f'{W}val') if tblStyle is not None else 'none'}")

# Table style pPr
for s in styles_xml.findall(f'{W}style'):
    if s.get(f'{W}type') == 'table' and s.get(f'{W}styleId') == tblStyle.get(f'{W}val'):
        pPr = s.find(f'{W}pPr')
        spacing = pPr.find(f'{W}spacing') if pPr is not None else None
        print(f"  Table style pPr after={spacing.get(f'{W}after') if spacing is not None else 'N/A'}")

print()
for ri, tr in enumerate(first_table.findall(f'{W}tr')[:4]):
    for ci, tc in enumerate(tr.findall(f'{W}tc')[:2]):
        for p in tc.findall(f'{W}p'):
            pPr = p.find(f'{W}pPr')
            pStyle = pPr.find(f'{W}pStyle') if pPr is not None else None
            spacing = pPr.find(f'{W}spacing') if pPr is not None else None
            direct_after = spacing is not None and spacing.get(f'{W}after') is not None
            style_val = pStyle.get(f'{W}val') if pStyle is not None else None
            
            # Simulate override logic
            would_override = False
            if not direct_after:
                if style_val is None:
                    would_override = True
                elif style_val == 'Normal':
                    # Normal has no explicit SA
                    would_override = True
                elif style_val == 'TableHeading':
                    # TableHeading has explicit SA
                    would_override = False
            
            print(f"  Row{ri} Cell{ci}: style={style_val}, directAfter={direct_after}, wouldOverride={would_override}")
