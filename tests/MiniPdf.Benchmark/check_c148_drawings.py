"""Analyze drawing types in classic148."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for label, path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic148_frozen_styled_grid.pdf'),
                     ('Ref', 'reference_pdfs/classic148_frozen_styled_grid.pdf')]:
    doc = fitz.open(path)
    p = doc[0]
    drawings = p.get_drawings()
    print(f'\n{label}: {len(drawings)} drawings')
    
    # Categorize by type
    types = {}
    for d in drawings:
        items = d.get('items', [])
        key = tuple(item[0] for item in items)
        types[key] = types.get(key, 0) + 1
    
    for k, v in sorted(types.items(), key=lambda x: -x[1]):
        print(f'  {k}: {v}')
    
    # Show first few drawings
    for i, d in enumerate(drawings[:5]):
        rect = d.get('rect', None)
        color = d.get('color', None)
        fill = d.get('fill', None)
        width = d.get('width', None)
        items = d.get('items', [])
        print(f'  [{i}] rect={rect} color={color} fill={fill} width={width} items={items[:3]}')
    doc.close()
