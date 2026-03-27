import fitz

for label, path in [('REF', 'reference_docx/20260318_issue.pdf'), ('MINI', 'minipdf_docx/20260318_issue.pdf')]:
    doc = fitz.open(path)
    page = doc[2]  # page 3
    blocks = page.get_text('dict')['blocks']
    print(f'=== {label} PAGE 3 bottom (y>650) ===')
    for b in blocks:
        if b['type'] == 0:
            if b['bbox'][1] > 650:
                for l in b['lines']:
                    for s in l['spans']:
                        print(f"  y={round(l['bbox'][1],1):>6} size={s['size']:.1f} flags={s['flags']} \"{s['text'][:60]}\"")
    print()
