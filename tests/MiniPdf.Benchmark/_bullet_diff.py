"""Examine text differences in classic08 and classic21 (bullet/list text failures)."""
import fitz

for name in ['docx_classic08_bullet_list', 'docx_classic21_nested_lists']:
    mini = fitz.open(f"../MiniPdf.Scripts/pdf_output_docx/{name}.pdf")
    ref = fitz.open(f"reference_pdfs_docx/{name}.pdf")
    
    print(f"\n{'='*60}")
    print(f"{name}")
    
    # Extract using get_text() for readable comparison
    mini_text = ""
    ref_text = ""
    for p in mini:
        mini_text += p.get_text()
    for p in ref:
        ref_text += p.get_text()
    
    print(f"  Chars: mini={len(mini_text)} ref={len(ref_text)}")
    print(f"\n  Mini text:\n{repr(mini_text[:500])}")
    print(f"\n  Ref text:\n{repr(ref_text[:500])}")
    
    # Show diffs
    from difflib import unified_diff
    mini_lines = mini_text.splitlines(keepends=True)
    ref_lines = ref_text.splitlines(keepends=True)
    diff = list(unified_diff(mini_lines, ref_lines, fromfile='MiniPdf', tofile='Reference', n=1))
    print(f"\n  Diff (first 30 lines):")
    for line in diff[:30]:
        print(f"    {line.rstrip()}")
    
    mini.close()
    ref.close()
