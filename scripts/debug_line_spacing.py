import zipfile, xml.etree.ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
z = zipfile.ZipFile(r'tests\Issue_Files\docx\20260318_issue.docx')
styles_xml = ET.parse(z.open('word/styles.xml')).getroot()

# Check docDefaults line spacing
dd = styles_xml.find(f'{W}docDefaults')
dd_pPr = dd.find(f'{W}pPrDefault/{W}pPr/{W}spacing') if dd is not None else None
dd_line = dd_pPr.get(f'{W}line') if dd_pPr is not None else None
dd_lineRule = dd_pPr.get(f'{W}lineRule') if dd_pPr is not None else None
print(f"docDefaults: line={dd_line} ({int(dd_line)/240:.3f}x), lineRule={dd_lineRule}")

# Normal style line spacing
for s in styles_xml.findall(f'{W}style'):
    sid = s.get(f'{W}styleId')
    if sid == 'Normal':
        sp = s.find(f'{W}pPr/{W}spacing')
        if sp is not None:
            line = sp.get(f'{W}line')
            lineRule = sp.get(f'{W}lineRule')
            print(f"Normal: line={line} ({int(line)/240:.3f}x), lineRule={lineRule}")

# What line spacing do data cell paragraphs end up with?
# They have no explicit style -> fall back to Normal 
# Normal has line=259 -> 259/240 = 1.079x
# But table style has line=240 -> 1.0x

# Table style line spacing
for s in styles_xml.findall(f'{W}style'):
    if s.get(f'{W}type') == 'table' and 'BluePrism' in (s.get(f'{W}styleId') or ''):
        sp = s.find(f'{W}pPr/{W}spacing')
        if sp is not None:
            line = sp.get(f'{W}line')
            lineRule = sp.get(f'{W}lineRule')
            print(f"BluePrism table: line={line} ({int(line)/240:.3f}x), lineRule={lineRule}")

# So cell paragraphs with no style:
# ReadParagraph resolves to Normal: line=259/240=1.079x  
# But table style says line=240/240=1.0x
# The table style line spacing is NOT being propagated!
print()
print("Cell paragraph line spacing resolution:")
print(f"  Normal style: 259/240 = {259/240:.4f}x")
print(f"  Table style:  240/240 = {240/240:.4f}x")
print(f"  FontSize=11pt, MetricsFactor=1.17")
print(f"  With Normal lineSpacing: 11 * 1.17 * {259/240:.4f} = {11 * 1.17 * 259/240:.2f}pt") 
print(f"  With Table  lineSpacing: 11 * 1.17 * {240/240:.4f} = {11 * 1.17 * 240/240:.2f}pt")
print(f"  Reference data row: 20.2pt, minus padding 2.8pt = 17.4pt line content")
print(f"  Required factor: {17.4 / 11:.4f} (at 1.0x line spacing)")
