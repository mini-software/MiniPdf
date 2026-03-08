import json, zipfile, os

# Load report
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data['results']

# Get closest cases to 0.99
below99 = [r for r in results if r['overall_score'] < 0.99]
below99.sort(key=lambda r: r['overall_score'], reverse=True)

docx_dir = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output_docx'

for r in below99[:5]:
    name = r.get('name', '')
    ov = r['overall_score']
    ts = r.get('text_similarity', 0)
    va = r.get('visual_avg', 0)
    td = r.get('text_diff', '')
    
    print(f'=== {name} (overall={ov:.4f}, text={ts:.4f}, vis={va:.4f}) ===')
    
    # Show text diff
    if td:
        for line in td.split('\n')[:15]:
            print(f'  {line}')
    
    # Show visual per-page scores
    vs = r.get('visual_scores', [])
    if vs:
        print(f'  Visual per-page: {[f"{v:.4f}" for v in vs]}')
    
    # Check DOCX content  
    path = os.path.join(docx_dir, name + '.docx')
    if os.path.exists(path):
        with zipfile.ZipFile(path) as z:
            with z.open('word/document.xml') as f:
                content = f.read().decode('utf-8')
                # Find spacing before/after values
                import re
                spacings = re.findall(r'<w:spacing([^/]*)/>', content)
                print(f'  Spacing elements: {len(spacings)}')
                for s in spacings[:5]:
                    print(f'    {s}')
    print()
