"""Deep analysis of how Tz scaling creates mixed effective font sizes.
For each non-chart case with text<1.0, check if Tz is causing line splits."""
import fitz, os, json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Focus on non-chart cases with text_sim between 0.93-0.99
for x in sorted(d, key=lambda x: x['overall_score']):
    name = x['name']
    ts = x.get('text_similarity', 0)
    
    skip = ['chart', 'pie_chart', 'bar_chart', 'line_chart', 'area_chart', 'scatter', 
            'bubble', 'ohlc', 'combo', 'stock', 'dashboard', 'radar',
            'cjk', 'emoji', 'unicode', 'korean', 'indic', 'rtl', 'bidi',
            'cyrillic', 'arabic', 'hebrew', 'thai', 'african', 'musical',
            'combining', 'caucasus', 'ethiopic', 'zwj', 'multilingual',
            'polyglot', 'technical_symbols', 'punctuation', 'box_drawing',
            'math_symbols', 'southeast_asian', 'ipa_phonetic', 'currency_symbols',
            'pattern_fills']
    if any(kw in name for kw in skip):
        continue
    if ts >= 0.99 or ts < 0.93:
        continue
    
    mini_path = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{name}.pdf')
    ref_path = os.path.join('reference_pdfs', f'{name}.pdf')
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        continue
    
    md = fitz.open(mini_path)
    rd = fitz.open(ref_path)
    
    # Compare line grouping on page 1
    def get_lines(page):
        blocks = page.get_text("dict", sort=True)
        lines = {}
        for block in blocks['blocks']:
            if block['type'] != 0:
                continue
            for line in block['lines']:
                for span in line['spans']:
                    if not span['text'].strip():
                        continue
                    y_key = round(span['bbox'][1])
                    if y_key not in lines:
                        lines[y_key] = []
                    lines[y_key].append((span['text'].strip(), round(span['size'], 1)))
        return lines
    
    m_lines = get_lines(md[0])
    r_lines = get_lines(rd[0])
    
    # Count how many MiniPdf lines have Tz-varied font sizes (not standard 11pt)
    tz_affected = 0
    for y, spans in m_lines.items():
        sizes = set(s for _, s in spans)
        if len(sizes) > 1:
            tz_affected += 1
    
    if tz_affected > 0:
        print(f"\n{name}  text={ts:.4f}  score={x['overall_score']:.4f}  tz_mixed_lines={tz_affected}")
        print(f"  MiniPdf lines: {len(m_lines)}, Ref lines: {len(r_lines)}")
        # Show a few mixed lines
        for y in sorted(m_lines.keys()):
            spans = m_lines[y]
            sizes = set(s for _, s in spans)
            if len(sizes) > 1:
                texts = [f"{t[:15]}({s}pt)" for t, s in spans[:4]]
                print(f"    Y={y}: {', '.join(texts)}")
