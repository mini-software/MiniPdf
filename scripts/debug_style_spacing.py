import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
wns = ns['w']

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/styles.xml'))
    root = tree.getroot()
    for style in root.findall(f'{{{wns}}}style'):
        sid = style.get(f'{{{wns}}}styleId')
        if sid in ('Normal', 'TableHeading', 'Footer', 'Header'):
            pPr = style.find(f'{{{wns}}}pPr')
            spacing = pPr.find(f'{{{wns}}}spacing') if pPr is not None else None
            if spacing is not None:
                attrs = {k.split('}')[1] if '}' in k else k: v for k, v in spacing.attrib.items()}
                print(f'{sid}: spacing = {attrs}')
            else:
                print(f'{sid}: no spacing in pPr')
