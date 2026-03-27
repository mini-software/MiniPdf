import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
wns = ns['w']

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/styles.xml'))
    root = tree.getroot()
    for style in root.findall(f'{{{wns}}}style'):
        sid = style.get(f'{{{wns}}}styleId')
        if 'Heading1' in sid or 'heading' in sid.lower():
            pPr = style.find(f'{{{wns}}}pPr')
            if pPr is not None:
                numPr = pPr.find(f'{{{wns}}}numPr')
                if numPr is not None:
                    numId = numPr.find(f'{{{wns}}}numId')
                    ilvl = numPr.find(f'{{{wns}}}ilvl')
                    nid = numId.get(f'{{{wns}}}val') if numId is not None else '?'
                    ilv = ilvl.get(f'{{{wns}}}val') if ilvl is not None else '?'
                    print(f'Style {sid}: numId={nid} ilvl={ilv}')
                else:
                    print(f'Style {sid}: no numPr')
                outlineLvl = pPr.find(f'{{{wns}}}outlineLvl')
                if outlineLvl is not None:
                    print(f'  outlineLvl={outlineLvl.get(f"{{{wns}}}val")}')
            # Print full pPr XML for analysis
            if pPr is not None:
                print('  pPr XML:', ET.tostring(pPr, encoding='unicode')[:500])
            rPr = style.find(f'{{{wns}}}rPr')
            if rPr is not None:
                print('  rPr XML:', ET.tostring(rPr, encoding='unicode')[:300])
            
    # Also check section properties for page numbering
    tree2 = ET.parse(z.open('word/document.xml'))
    body = tree2.getroot().find(f'{{{wns}}}body')
    sectPr = body.find(f'{{{wns}}}sectPr')
    if sectPr is not None:
        pgNumType = sectPr.find(f'{{{wns}}}pgNumType')
        if pgNumType is not None:
            print(f'\nsectPr pgNumType: {dict(pgNumType.attrib)}')
        else:
            print('\nsectPr: no pgNumType')

    # Check settings.xml for page number display
    if 'word/settings.xml' in z.namelist():
        stree = ET.parse(z.open('word/settings.xml'))
        sroot = stree.getroot()
        for child in sroot:
            tag = child.tag.split('}')[1] if '}' in child.tag else child.tag
            if 'pgNum' in tag.lower() or 'foot' in tag.lower():
                print(f'settings: {tag} = {dict(child.attrib)}')
