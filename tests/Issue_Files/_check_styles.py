"""Check docx default font size and style properties"""
import zipfile
import xml.etree.ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
zf = zipfile.ZipFile('tests/Issue_Files/docx/SA8000 ch sample.docx')
styles_xml = zf.read('word/styles.xml').decode('utf-8')
root = ET.fromstring(styles_xml)

# docDefaults
dd = root.find(W + 'docDefaults')
if dd:
    rpr = dd.find(W + 'rPrDefault')
    if rpr:
        rp = rpr.find(W + 'rPr')
        if rp:
            sz = rp.find(W + 'sz')
            szCs = rp.find(W + 'szCs')
            print('docDefaults rPr:')
            if sz is not None:
                val = sz.get(W + 'val')
                print(f'  sz={val} ({float(val)/2}pt)')
            if szCs is not None:
                val = szCs.get(W + 'val')
                print(f'  szCs={val} ({float(val)/2}pt)')
    ppr_def = dd.find(W + 'pPrDefault')
    if ppr_def:
        pp = ppr_def.find(W + 'pPr')
        if pp:
            sp = pp.find(W + 'spacing')
            if sp is not None:
                print('docDefaults pPr spacing:')
                for a in ['before', 'after', 'line', 'lineRule']:
                    v = sp.get(W + a)
                    if v:
                        print(f'  {a}={v}')

# All styles
for s in root.findall(W + 'style'):
    sid = s.get(W + 'styleId')
    stype = s.get(W + 'type')
    name_el = s.find(W + 'name')
    sname = name_el.get(W + 'val') if name_el is not None else ''
    
    rpr2 = s.find(W + 'rPr')
    ppr2 = s.find(W + 'pPr')
    
    info = []
    if rpr2 is not None:
        sz2 = rpr2.find(W + 'sz')
        if sz2 is not None:
            info.append(f'sz={sz2.get(W + "val")}')
    if ppr2 is not None:
        sp2 = ppr2.find(W + 'spacing')
        if sp2 is not None:
            for a in ['line', 'lineRule']:
                v = sp2.get(W + a)
                if v:
                    info.append(f'{a}={v}')
    
    if info:
        print(f'Style [{sid}] ({stype}) name="{sname}": {", ".join(info)}')
