import json, subprocess

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data.get('results', data)

# Find classic140_rotated_text
for r in results:
    name = r.get('name', r.get('file', ''))
    if 'classic140' in name:
        print(f'Name: {name}')
        print(f'Score: {r["overall_score"]:.4f}')
        print(f'Text: {r.get("text_similarity", 0):.4f}')
        print(f'Visual: {r.get("visual_avg", r.get("visual_similarity", 0)):.4f}')
        print(f'Text diff:')
        td = r.get('text_diff', '')
        for line in td.split('\n')[:30]:
            print(f'  {line}')
        break

# Also extract text from PDFs
print('\n=== MiniPdf text ===')
mp = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic140_rotated_text.pdf'
rp = r'd:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic140_rotated_text.pdf'
for label, path in [('MiniPdf', mp), ('Reference', rp)]:
    try:
        result = subprocess.run(['pdftotext', '-layout', path, '-'], capture_output=True, text=True, timeout=10)
        text = result.stdout
        print(f'\n--- {label} ---')
        print(text[:500])
    except Exception as e:
        print(f'{label}: error {e}')
