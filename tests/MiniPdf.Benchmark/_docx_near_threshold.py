"""Investigate near-threshold DOCX cases (text 0.96-0.99)."""
import fitz, os, sys, difflib

sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output_docx')
REF_DIR = 'reference_pdfs_docx'

cases = [
    'docx_classic60_project_status_with_milestones',
    'docx_classic105_certificate',
    'docx_classic100_multi_page_table',
    'docx_classic108_comparison_matrix',
    'docx_classic109_release_notes',
    'docx_classic24_two_column_table_layout',
    'docx_classic45_project_plan',
    'docx_classic92_first_line_indent',
]

for name in cases:
    mp = os.path.join(MINI_DIR, f'{name}.pdf')
    rp = os.path.join(REF_DIR, f'{name}.pdf')
    if not os.path.exists(mp) or not os.path.exists(rp):
        continue
    md = fitz.open(mp)
    rd = fitz.open(rp)
    mt = ''.join(p.get_text('text') for p in md)
    rt = ''.join(p.get_text('text') for p in rd)
    print(f'\n=== {name} ===')
    print(f'  Mini: {len(mt)} chars, {len(md)} pages | Ref: {len(rt)} chars, {len(rd)} pages')
    
    # Show key diffs
    ml = mt.splitlines(keepends=True)
    rl = rt.splitlines(keepends=True)
    d = list(difflib.unified_diff(rl, ml, n=0))
    added = [l[1:].rstrip() for l in d if l.startswith('+') and not l.startswith('+++')]
    removed = [l[1:].rstrip() for l in d if l.startswith('-') and not l.startswith('---')]
    if added[:5]:
        print(f'  In MiniPdf only: {added[:5]}')
    if removed[:5]:
        print(f'  In Ref only: {removed[:5]}')
    md.close()
    rd.close()
