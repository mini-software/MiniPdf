"""Check for regressions by comparing current text with reference for specific cases."""
import fitz

# Check cases that might have regressed
cases = ['classic15_negative_numbers', 'classic111_chart_with_axis_labels', 'classic49_contact_list']

for name in cases:
    mini_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
    ref_path = f'reference_pdfs/{name}.pdf'
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    mini_text = mini_doc[0].get_text()[:500]
    ref_text = ref_doc[0].get_text()[:500]
    
    mini_doc.close()
    ref_doc.close()
    
    # Find differences
    mini_lines = mini_text.strip().split('\n')
    ref_lines = ref_text.strip().split('\n')
    
    print(f"=== {name} ===")
    diffs = []
    max_lines = max(len(mini_lines), len(ref_lines))
    for i in range(max_lines):
        ml = mini_lines[i] if i < len(mini_lines) else '<missing>'
        rl = ref_lines[i] if i < len(ref_lines) else '<missing>'
        if ml != rl:
            diffs.append(f"  line {i}: mini='{ml}' ref='{rl}'")
    if diffs:
        for d in diffs[:10]:
            print(d)
    else:
        print("  No differences")
    print()
