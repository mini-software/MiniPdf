import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
wns = ns['w']

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/styles.xml'))
    root = tree.getroot()
    
    # Check BluePrism table style pPr
    for style in root.findall(f'{{{wns}}}style'):
        sid = style.get(f'{{{wns}}}styleId')
        if 'BluePrism' in sid or 'TableNormal' in sid or 'TableGrid' in sid:
            print(f'\nStyle: {sid}')
            pPr = style.find(f'{{{wns}}}pPr')
            if pPr is not None:
                spacing = pPr.find(f'{{{wns}}}spacing')
                if spacing is not None:
                    print(f'  pPr spacing: {dict(spacing.attrib)}')
                else:
                    print('  pPr: no spacing')
            tblPr = style.find(f'{{{wns}}}tblPr')
            if tblPr is not None:
                print(f'  tblPr: {ET.tostring(tblPr, encoding="unicode")[:300]}')
            tcPr = style.find(f'{{{wns}}}tcPr')
            if tcPr is not None:
                print(f'  tcPr: {ET.tostring(tcPr, encoding="unicode")[:200]}')

    # Check docDefaults spacing
    docDefaults = root.find(f'{{{wns}}}docDefaults')
    if docDefaults:
        pPrDefault = docDefaults.find(f'{{{wns}}}pPrDefault')
        if pPrDefault:
            pPr = pPrDefault.find(f'{{{wns}}}pPr')
            if pPr:
                spacing = pPr.find(f'{{{wns}}}spacing')
                if spacing:
                    print(f'\ndocDefaults pPr spacing: {dict(spacing.attrib)}')
