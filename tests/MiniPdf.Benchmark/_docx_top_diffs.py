"""Detailed text diffs for top DOCX failing cases."""
import fitz
import os
import sys
import difflib

sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output_docx')
REF_DIR = 'reference_pdfs_docx'

cases = [
    'docx_classic50_long_table_with_formatting',
    'docx_classic35_tab_stops',
    'docx_classic64_multi_column_layout',
    'docx_classic49_cjk_document',
    'docx_classic08_bullet_list',
    'docx_classic140_rotated_text_table',
    'docx_classic57_right_to_left_text',
    'docx_classic21_nested_lists',
    'docx_classic32_superscript_subscript',
]

for name in cases:
    mini_path = os.path.join(MINI_DIR, f'{name}.pdf')
    ref_path = os.path.join(REF_DIR, f'{name}.pdf')
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print(f'\n=== {name} === SKIP (missing files)')
        continue

    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)

    mini_text = ''.join(p.get_text('text') for p in mini_doc)
    ref_text = ''.join(p.get_text('text') for p in ref_doc)

    print(f'\n=== {name} ===')
    print(f'  Mini: {len(mini_text)} chars, {len(mini_doc)} pages')
    print(f'  Ref:  {len(ref_text)} chars, {len(ref_doc)} pages')

    # Show unified diff (first 40 lines)
    mini_lines = mini_text.splitlines(keepends=True)
    ref_lines = ref_text.splitlines(keepends=True)
    diff = list(difflib.unified_diff(ref_lines, mini_lines, lineterm='',
                                     fromfile='reference', tofile='minipdf', n=1))
    if diff:
        print('  Diff (first 40 lines):')
        for line in diff[:40]:
            print(f'    {line.rstrip()[:100]}')
        if len(diff) > 40:
            print(f'    ... ({len(diff)-40} more diff lines)')
    else:
        print('  No text differences')

    mini_doc.close()
    ref_doc.close()
