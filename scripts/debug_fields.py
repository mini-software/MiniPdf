import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    # Check all headers
    for name in z.namelist():
        if 'header' in name or 'footer' in name:
            tree = ET.parse(z.open(name))
            root = tree.getroot()
            # Look for fldChar and fldSimple
            fields = root.findall('.//{%s}fldChar' % ns['w'])
            simples = root.findall('.//{%s}fldSimple' % ns['w'])
            instrTexts = root.findall('.//{%s}instrText' % ns['w'])
            print(f'{name}: fldChar={len(fields)} fldSimple={len(simples)} instrText={len(instrTexts)}')
            for it in instrTexts:
                print(f'  instrText: [{it.text}]')
            for fs in simples:
                wns = ns['w']
                print(f'  fldSimple: instr=[{fs.get(f"{{{wns}}}instr")}]')

    # Check document body for PAGE fields
    tree = ET.parse(z.open('word/document.xml'))
    root = tree.getroot()
    instrTexts = root.findall('.//{%s}instrText' % ns['w'])
    print(f'\ndocument.xml: instrText count={len(instrTexts)}')
    for it in instrTexts:
        print(f'  instrText: [{it.text}]')
