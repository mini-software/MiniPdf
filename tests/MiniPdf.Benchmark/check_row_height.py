"""Check default row height for classic18."""
import zipfile, xml.etree.ElementTree as ET

xlsx_path = r'..\MiniPdf.Scripts\pdf_output\..\..\..\tests\Issue_Files\classic18_large_dataset.xlsx'
# Actually, let's find the xlsx
import glob
files = glob.glob(r'..\..\tests\Issue_Files\classic18*.xlsx')
if not files:
    files = glob.glob(r'..\..\tests\MiniPdf.Benchmark\reference_xlsx\classic18*.xlsx')
if not files:
    # Try MiniPdf.Scripts area
    files = glob.glob(r'..\MiniPdf.Scripts\**\classic18*.xlsx', recursive=True)
print(f"Found: {files}")
if not files:
    # Search more broadly
    import os
    for root, dirs, fnames in os.walk(r'..\..\tests'):
        for f in fnames:
            if 'classic18' in f and f.endswith('.xlsx'):
                files.append(os.path.join(root, f))
                break
    print(f"Found2: {files}")

if files:
    with zipfile.ZipFile(files[0]) as z:
        with z.open('xl/worksheets/sheet1.xml') as f:
            tree = ET.parse(f)
            root = tree.getroot()
            ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
            fmt = root.find('.//s:sheetFormatPr', ns)
            if fmt is not None:
                print(f"defaultRowHeight: {fmt.get('defaultRowHeight')}")
                print(f"customHeight: {fmt.get('customHeight')}")
                print(f"defaultColWidth: {fmt.get('defaultColWidth')}")
                print(f"baseColWidth: {fmt.get('baseColWidth')}")
            else:
                print("No sheetFormatPr found")
