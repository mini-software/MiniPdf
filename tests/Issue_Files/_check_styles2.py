"""Check styles.xml for default paragraph spacing"""
import zipfile
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)

# Check styles.xml
styles_xml = ET.parse(zf.open('word/styles.xml'))
root = styles_xml.getroot()

# docDefaults
dd = root.find(f'{W}docDefaults')
if dd is not None:
    pPrDefault = dd.find(f'{W}pPrDefault')
    if pPrDefault is not None:
        pPr = pPrDefault.find(f'{W}pPr')
        if pPr is not None:
            spacing = pPr.find(f'{W}spacing')
            if spacing is not None:
                print(f"docDefaults pPr spacing: {spacing.attrib}")
            else:
                print("docDefaults pPr: no spacing")
        else:
            print("docDefaults pPrDefault: no pPr")
    else:
        print("docDefaults: no pPrDefault")
    
    rPrDefault = dd.find(f'{W}rPrDefault')
    if rPrDefault is not None:
        rPr = rPrDefault.find(f'{W}rPr')
        if rPr is not None:
            sz = rPr.find(f'{W}sz')
            lang = rPr.find(f'{W}lang')
            print(f"docDefaults rPr: sz={sz.get(f'{W}val') if sz is not None else 'N/A'}")
        
# Check Normal style
for style in root.findall(f'{W}style'):
    sid = style.get(f'{W}styleId', '')
    stype = style.get(f'{W}type', '')
    if sid == 'Normal' or (stype == 'paragraph' and style.get(f'{W}default') == '1'):
        pPr = style.find(f'{W}pPr')
        rPr = style.find(f'{W}rPr')
        print(f"\nStyle: {sid} (type={stype}, default={style.get(f'{W}default', '')})")
        if pPr is not None:
            spacing = pPr.find(f'{W}spacing')
            if spacing is not None:
                print(f"  pPr spacing: {spacing.attrib}")
            else:
                print(f"  pPr: no spacing")
            # All pPr elements
            for child in pPr:
                tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                print(f"  pPr.{tag}: {child.attrib}")
        else:
            print(f"  no pPr")
        if rPr is not None:
            for child in rPr:
                tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                print(f"  rPr.{tag}: {child.attrib}")

# Check ListParagraph style
for style in root.findall(f'{W}style'):
    sid = style.get(f'{W}styleId', '')
    if sid == 'ListParagraph':
        pPr = style.find(f'{W}pPr')
        print(f"\nStyle: {sid}")
        if pPr is not None:
            for child in pPr:
                tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                print(f"  pPr.{tag}: {child.attrib}")
