import fitz, sys, difflib
name = sys.argv[1] if len(sys.argv) > 1 else "classic129_alignment_combos"
mini = fitz.open(f"../MiniPdf.Scripts/pdf_output/{name}.pdf")
ref = fitz.open(f"reference_pdfs/{name}.pdf")
for pi in range(min(mini.page_count, ref.page_count)):
    mt = mini[pi].get_text().strip().split('\n')
    rt = ref[pi].get_text().strip().split('\n')
    d = list(difflib.unified_diff(mt, rt, lineterm='', n=1))
    if d:
        print(f"=== Page {pi+1} ===")
        for line in d[:40]:
            print(f"  {line}")
        print()
