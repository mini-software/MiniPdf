"""Compare MiniPdf vs Reference text for key chart cases."""
import fitz, os, sys

sys.stdout.reconfigure(encoding='utf-8')

charts = ['classic100_stacked_bar_chart', 'classic102_line_chart_with_markers',
          'classic93_line_chart', 'classic110_chart_with_legend',
          'classic95_area_chart', 'classic107_multi_series_line']
for name in charts:
    print(f'\n=== {name} ===')
    for label, d in [('Mini', os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')),
                     ('Ref ', 'reference_pdfs')]:
        fpath = os.path.join(d, f'{name}.pdf')
        if not os.path.exists(fpath):
            print(f'  {label}: NOT FOUND')
            continue
        pdf = fitz.open(fpath)
        text = ''
        for p in pdf:
            text += p.get_text('text')
        pdf.close()
        lines = [l for l in text.strip().split('\n') if l.strip()]
        print(f'  {label} ({len(text)} chars, {len(lines)} lines):')
        # show last 12 lines
        for l in lines[-12:]:
            print(f'    > {l}')
