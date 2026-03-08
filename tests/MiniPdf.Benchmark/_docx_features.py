import json
import zipfile
import os
import xml.etree.ElementTree as ET

with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data['results']

# Get visual-only failures sorted by visual score
vis_only = [r for r in results if r.get('text_similarity',0) >= 0.99 
            and r.get('visual_avg',0) < 0.99 and r['overall_score'] < 0.99]
vis_only.sort(key=lambda r: r.get('visual_avg',0))

docx_dir = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output_docx'

print(f'Visual-only failures: {len(vis_only)}')
print()

# Check DOCX features for worst 30 cases
for r in vis_only[:30]:
    name = r.get('name','')
    va = r.get('visual_avg',0)
    docx_path = os.path.join(docx_dir, name + '.docx')
    
    features = []
    if os.path.exists(docx_path):
        try:
            with zipfile.ZipFile(docx_path) as z:
                with z.open('word/document.xml') as f:
                    content = f.read().decode('utf-8')
                    if '<w:tbl' in content: features.append('TABLE')
                    if '<w:shd' in content: features.append('SHADING')
                    if '<w:drawing' in content or '<wp:inline' in content: features.append('IMAGE')
                    if '<w:numId' in content: features.append('LIST')
                    if '<w:ind ' in content: features.append('INDENT')
                    if '<w:jc w:val="center"' in content: features.append('CENTER')
                    if '<w:tab' in content: features.append('TAB')
                    if '<w:bdr' in content or '<w:pBdr' in content: features.append('BORDER')
                    if 'w:val="Heading' in content: features.append('HEADING')
                    if '<w:sectPr' in content.split('</w:body>')[0].rsplit('<w:sectPr',1)[0]: features.append('SECTION_BREAK')
                    # Count tables
                    tbl_count = content.count('<w:tbl>')
                    if tbl_count > 0: features.append(f'TBL_COUNT={tbl_count}')
                    # Count rows
                    row_count = content.count('<w:tr>')
                    if row_count > 0: features.append(f'ROWS={row_count}')
        except:
            features.append('READ_ERROR')
    else:
        features.append('NOT_FOUND')
    
    print(f'{name}: vis={va:.4f} [{", ".join(features)}]')

# Feature frequency for worst 30
print('\n--- Feature frequency (worst 30 visual-only) ---')
feature_counts = {}
for r in vis_only[:30]:
    name = r.get('name','')
    docx_path = os.path.join(docx_dir, name + '.docx')
    if os.path.exists(docx_path):
        try:
            with zipfile.ZipFile(docx_path) as z:
                with z.open('word/document.xml') as f:
                    content = f.read().decode('utf-8')
                    for feat in ['TABLE', 'SHADING', 'IMAGE', 'LIST', 'TAB', 'BORDER', 'HEADING']:
                        tag = {'TABLE':'<w:tbl','SHADING':'<w:shd','IMAGE':'<w:drawing','LIST':'<w:numId',
                               'TAB':'<w:tab','BORDER':'<w:bdr','HEADING':'w:val="Heading'}[feat]
                        if tag in content:
                            feature_counts[feat] = feature_counts.get(feat, 0) + 1
        except:
            pass

for feat, count in sorted(feature_counts.items(), key=lambda x: -x[1]):
    print(f'  {feat}: {count}/30')
