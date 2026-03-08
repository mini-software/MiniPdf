import zipfile, fitz, os
from lxml import etree

W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"

# Check the 2 page mismatch cases
cases = [
    "docx_classic56_images_and_tables_mixed",
    "docx_classic82_survey_questionnaire",
]

mini_dir = "../MiniPdf.Scripts/pdf_output_docx"
ref_dir = "reference_pdfs_docx"
docx_dir = "../MiniPdf.Scripts/output_docx"

for name in cases:
    print(f"\n{'='*60}")
    print(f"{name}")
    
    # Check reference PDF
    ref_doc = fitz.open(os.path.join(ref_dir, name + ".pdf"))
    print(f"  Reference: {len(ref_doc)} pages")
    for i in range(len(ref_doc)):
        p = ref_doc[i]
        blocks = p.get_text("dict", sort=True)["blocks"]
        text_blocks = [b for b in blocks if b["type"] == 0]
        last_y = max(b["bbox"][3] for b in text_blocks) if text_blocks else 0
        print(f"    Page {i}: last text Y={last_y:.1f}, page height={p.rect.height:.1f}, remaining={p.rect.height-last_y:.1f}")
    ref_doc.close()
    
    # Check DOCX details
    docx_path = os.path.join(docx_dir, name + ".docx")
    if os.path.exists(docx_path):
        with zipfile.ZipFile(docx_path) as z:
            with z.open("word/document.xml") as f:
                tree = etree.parse(f)
                body = tree.getroot().find(f".//{W}body")
                paras = body.findall(f".//{W}p")
                tables = body.findall(f".//{W}tbl")
                print(f"  DOCX: {len(paras)} paragraphs, {len(tables)} tables")
                
                # Check first paragraph style
                first_p = body.find(f"{W}p")
                if first_p is not None:
                    pPr = first_p.find(f"{W}pPr")
                    if pPr is not None:
                        pStyle = pPr.find(f"{W}pStyle")
                        if pStyle is not None:
                            print(f"  First para style: {pStyle.get(f'{W}val')}")
