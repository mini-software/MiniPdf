import zipfile, os
from lxml import etree

cases = [
    "docx_classic02_multiple_paragraphs",
    "docx_classic15_indentation", 
    "docx_classic16_line_spacing",
    "docx_classic32_company_logo_header",
    "docx_classic69_blockquote_styling",
    "docx_classic129_alignment_combinations",
    "docx_classic127_font_styles_showcase",
]

docx_dir = "../MiniPdf.Scripts/output_docx"

W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"

for name in cases:
    docx_name = name + ".docx"
    docx_path = os.path.join(docx_dir, docx_name)
    if not os.path.exists(docx_path):
        # Try with docx_ prefix
        docx_path = os.path.join(docx_dir, name + ".docx")
    if not os.path.exists(docx_path):
        print(f"\n{name}: DOCX not found ({docx_name})")
        continue
    
    with zipfile.ZipFile(docx_path) as z:
        with z.open("word/document.xml") as f:
            tree = etree.parse(f)
            root = tree.getroot()
    
    nsmap = {"w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main"}
    
    # Find section properties (body > sectPr)
    body = root.find(f"{W}body")
    sect_prs = body.findall(f".//{W}sectPr")
    
    print(f"\n{'='*60}")
    print(f"{name}")
    
    for i, sp in enumerate(sect_prs):
        pgSz = sp.find(f"{W}pgSz")
        pgMar = sp.find(f"{W}pgMar")
        
        if pgSz is not None:
            w = pgSz.get(f"{W}w")
            h = pgSz.get(f"{W}h")
            print(f"  Section {i}: pageSize w={w} h={h} ({float(w)/20:.1f}pt x {float(h)/20:.1f}pt)")
        
        if pgMar is not None:
            top = pgMar.get(f"{W}top")
            bottom = pgMar.get(f"{W}bottom")
            left = pgMar.get(f"{W}left")
            right = pgMar.get(f"{W}right")
            header = pgMar.get(f"{W}header")
            footer = pgMar.get(f"{W}footer")
            print(f"  Section {i}: margins top={top}({float(top)/20:.1f}pt) bottom={bottom}({float(bottom)/20:.1f}pt) left={left}({float(left)/20:.1f}pt) right={right}({float(right)/20:.1f}pt)")
            if header: print(f"  Section {i}: header={header}({float(header)/20:.1f}pt)")
            if footer: print(f"  Section {i}: footer={footer}({float(footer)/20:.1f}pt)")
    
    # Also check first paragraph's spacing
    first_p = body.find(f"{W}p")
    if first_p is not None:
        pPr = first_p.find(f"{W}pPr")
        if pPr is not None:
            spacing = pPr.find(f"{W}spacing")
            if spacing is not None:
                before = spacing.get(f"{W}before")
                after = spacing.get(f"{W}after")
                line = spacing.get(f"{W}line")
                lineRule = spacing.get(f"{W}lineRule")
                print(f"  First para spacing: before={before} after={after} line={line} lineRule={lineRule}")
            
            # Check style reference
            pStyle = pPr.find(f"{W}pStyle")
            if pStyle is not None:
                print(f"  First para style: {pStyle.get(f'{W}val')}")
