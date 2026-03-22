import zipfile, xml.etree.ElementTree as ET
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
zf = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic71_legal_document.docx')
doc = ET.parse(zf.open('word/styles.xml'))

# Find docDefaults
docDefaults = doc.find(f'.//{W}docDefaults')
if docDefaults is not None:
    print("docDefaults found:")
    pPrDefault = docDefaults.find(f'{W}pPrDefault')
    if pPrDefault is not None:
        pPr = pPrDefault.find(f'{W}pPr')
        if pPr is not None:
            spacing = pPr.find(f'{W}spacing')
            if spacing is not None:
                print(f"  pPr/spacing: {dict(spacing.attrib)}")
            else:
                print("  pPr/spacing: NOT FOUND")
            # Print all pPr children
            for child in pPr:
                tag = child.tag.split('}')[-1]
                print(f"  pPr child: {tag} = {dict(child.attrib)}")
        else:
            print("  pPrDefault/pPr: NOT FOUND")
    else:
        print("  pPrDefault: NOT FOUND")
    
    rPrDefault = docDefaults.find(f'{W}rPrDefault')
    if rPrDefault:
        rPr = rPrDefault.find(f'{W}rPr')
        if rPr:
            for child in rPr:
                tag = child.tag.split('}')[-1]
                print(f"  rPr child: {tag} = {dict(child.attrib)}")
else:
    print("docDefaults NOT FOUND")

# Check Normal style
for style in doc.getroot().iter(f'{W}style'):
    sid = style.get(f'{W}styleId', '')
    if sid == 'Normal':
        print(f"\nNormal style:")
        pPr = style.find(f'{W}pPr')
        rPr = style.find(f'{W}rPr')
        if pPr:
            for child in pPr:
                tag = child.tag.split('}')[-1]
                print(f"  pPr/{tag}: {dict(child.attrib)}")
        else:
            print("  pPr: NOT FOUND")
        if rPr:
            for child in rPr:
                tag = child.tag.split('}')[-1]
                print(f"  rPr/{tag}: {dict(child.attrib)}")
        else:
            print("  rPr: NOT FOUND")
zf.close()
