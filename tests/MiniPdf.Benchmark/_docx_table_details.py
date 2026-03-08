import zipfile, os

docx_dir = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output_docx'
worst = [
    'docx_classic88_presentation_handout',
    'docx_classic30_comprehensive_report', 
    'docx_classic96_dense_data_table',
    'docx_classic148_data_grid_document',
    'docx_classic67_alternating_row_table',
    'docx_classic80_matrix_grid',
    'docx_classic115_price_list',
    'docx_classic81_budget_table',
]

for name in worst:
    path = os.path.join(docx_dir, name + '.docx')
    if not os.path.exists(path):
        print(f'{name}: NOT FOUND')
        continue
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as f:
            content = f.read().decode('utf-8')
    
    has_tblCellMar = 'tblCellMar' in content
    has_tcMar = 'tcMar' in content
    has_trHeight = 'trHeight' in content
    
    # Extract cell margin values if present
    margins = []
    if has_tblCellMar:
        import re
        m = re.search(r'<w:tblCellMar>(.*?)</w:tblCellMar>', content, re.DOTALL)
        if m:
            margins.append('tbl:' + m.group(1)[:200])
    
    # Check for row heights
    heights = []
    if has_trHeight:
        import re
        for m in re.finditer(r'w:trHeight[^/]*w:val="(\d+)"', content):
            heights.append(m.group(1))
    
    # Check what shd values are used
    import re
    shd_fills = set()
    for m in re.finditer(r'<w:shd[^>]*w:fill="([^"]*)"', content):
        shd_fills.add(m.group(1))
    
    print(f'{name}:')
    print(f'  tblCellMar={has_tblCellMar} tcMar={has_tcMar} trHeight={has_trHeight}')
    if margins:
        print(f'  cellMar: {margins[0]}')
    if heights:
        print(f'  rowHeights: {heights[:5]}')
    if shd_fills:
        print(f'  shading colors: {shd_fills}')
    print()
