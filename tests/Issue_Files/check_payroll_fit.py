import zipfile, xml.etree.ElementTree as ET

ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
xlsx = r'tests\Issue_Files\xlsx\payroll-calculator_f.xlsx'

with zipfile.ZipFile(xlsx) as z:
    for name in sorted(z.namelist()):
        if 'worksheets/sheet' in name:
            doc = ET.parse(z.open(name))
            root = doc.getroot()
            
            # fitToPage
            spr = root.find(f'{{{ns}}}sheetPr')
            ftp = False
            if spr is not None:
                p = spr.find(f'{{{ns}}}pageSetUpPr')
                if p is not None and p.get('fitToPage') == '1':
                    ftp = True
            
            if not ftp:
                continue
            
            # pageSetup
            ps = root.find(f'.//{{{ns}}}pageSetup')
            scale = int(ps.get('scale', '100')) if ps is not None else 100
            orient = ps.get('orientation', 'portrait') if ps is not None else 'portrait'
            
            # sheetFormatPr
            sfp = root.find(f'{{{ns}}}sheetFormatPr')
            defRH = float(sfp.get('defaultRowHeight', '15')) if sfp is not None else 15.0
            
            # Count rows and total height
            rows = root.findall(f'.//{{{ns}}}row')
            total_h = 0
            for r in rows:
                ht = r.get('ht')
                if ht:
                    total_h += float(ht)
                else:
                    total_h += defRH
            
            # Page dimensions
            if orient == 'landscape':
                page_h = 612  # letter landscape
            else:
                page_h = 792  # letter portrait
            
            # Get margins
            pm = root.find(f'.//{{{ns}}}pageMargins')
            if pm is not None:
                mt = float(pm.get('top', '1')) * 72
                mb = float(pm.get('bottom', '1')) * 72
            else:
                mt = 72
                mb = 72
            
            usable_h = page_h - mt - mb
            required_scale = usable_h / total_h if total_h > 0 else 1.0
            current_scale = scale / 100.0
            scale_drop = 1.0 - required_scale / current_scale if current_scale > 0 else 0
            
            print(f'{name}: fitToPage=True, scale={scale}%, orient={orient}')
            print(f'  rows={len(rows)}, total_h={total_h:.1f}pt, defRH={defRH}pt')
            print(f'  page_h={page_h}, margins=T:{mt:.1f}pt B:{mb:.1f}pt, usable_h={usable_h:.1f}pt')
            print(f'  required_scale={required_scale:.4f}, current_scale={current_scale:.4f}')
            print(f'  scale_drop={scale_drop:.4f} (compression needed: {scale_drop*100:.1f}%)')
            print(f'  shouldCompress = {required_scale < current_scale and scale_drop <= 0.15}')
