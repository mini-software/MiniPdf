"""Quick text check for specific cases."""
import fitz, sys, os

cases = ['classic13_date_strings', 'classic73_event_flyer_with_banner', 'classic49_contact_list', 'classic68_restaurant_menu']

for case in cases:
    mini = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{case}.pdf')
    ref = os.path.join('reference_pdfs', f'{case}.pdf')
    if not os.path.exists(mini) or not os.path.exists(ref):
        print(f"SKIP {case}")
        continue
    
    md = fitz.open(mini)
    rd = fitz.open(ref)
    
    mt = md[0].get_text("text").strip()[:200]
    rt = rd[0].get_text("text").strip()[:200]
    
    print(f"\n=== {case} ===")
    print(f"MiniPdf text: {repr(mt)}")
    print(f"Ref     text: {repr(rt)}")
    
    # Check line-by-line differences
    ml = mt.split('\n')
    rl = rt.split('\n')
    for i, (m, r) in enumerate(zip(ml, rl)):
        if m != r:
            print(f"  Line {i} diff:")
            print(f"    MINI: {repr(m)}")
            print(f"    REF:  {repr(r)}")
