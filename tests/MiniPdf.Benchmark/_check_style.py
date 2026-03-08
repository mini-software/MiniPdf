import zipfile, xml.etree.ElementTree as ET
z = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic08_bullet_list.docx')
doc = ET.parse(z.open('word/styles.xml'))
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
for style in doc.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}style'):
    sid = style.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}styleId')
    if sid and 'list' in sid.lower():
        print(f'styleId={sid}')
        numPr = style.find('.//w:numPr', ns)
        print(f'  numPr={numPr is not None}')
        pPr = style.find('w:pPr', ns)
        if pPr is not None:
            ind = pPr.find('w:ind', ns)
            if ind is not None:
                left = ind.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}left')
                hanging = ind.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}hanging')
                print(f'  ind: left={left}, hanging={hanging}')
