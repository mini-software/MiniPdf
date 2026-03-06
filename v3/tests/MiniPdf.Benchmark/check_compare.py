import fitz

cases = ['classic40_scientific_notation', 'classic41_integer_vs_float', 'classic58_mixed_numeric_formats']

for name in cases:
    mini_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
    ref_path = f'reference_pdfs/{name}.pdf'
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    mini_text = mini_doc[0].get_text()
    ref_text = ref_doc[0].get_text()
    
    mini_doc.close()
    ref_doc.close()
    
    print(f"=== {name} ===")
    print("--- MiniPdf ---")
    print(mini_text[:600])
    print("--- Reference ---")
    print(ref_text[:600])
    print()
