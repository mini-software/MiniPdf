import json, zipfile, os
import xml.etree.ElementTree as ET

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
results = data if isinstance(data, list) else data.get('results', data)

def get_vis(r):
    return r.get('visual_avg', r.get('visual_similarity', 0))

# Get vis-only XLSX failures
vis_only = [r for r in results if r.get('text_similarity',0) >= 0.99 
            and get_vis(r) < 0.99 and r['overall_score'] < 0.99]
vis_only.sort(key=lambda r: get_vis(r), reverse=True)

print(f'Visual-only failures: {len(vis_only)}\n')

xlsx_dir = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output'
for r in vis_only:
    name = r.get('name', '').replace('.pdf','')
    vs = get_vis(r)
    ov = r['overall_score']
    
    features = []
    xlsx_path = os.path.join(xlsx_dir, name + '.xlsx')
    if os.path.exists(xlsx_path):
        with zipfile.ZipFile(xlsx_path) as z:
            # Check for sheets
            sheets = [n for n in z.namelist() if n.startswith('xl/worksheets/sheet')]
            features.append(f'sheets={len(sheets)}')
            
            # Check for styles (fill patterns, borders)
            if 'xl/styles.xml' in z.namelist():
                with z.open('xl/styles.xml') as f:
                    styles = f.read().decode('utf-8')
                    fill_count = styles.count('<fill>')
                    border_count = styles.count('<border>')
                    features.append(f'fills={fill_count}')
                    features.append(f'borders={border_count}')
            
            # Check for merged cells
            with z.open(sheets[0]) as f:
                sheet = f.read().decode('utf-8')
                merge_count = sheet.count('<mergeCell')
                if merge_count > 0:
                    features.append(f'merged={merge_count}')
                # Count rows
                row_count = sheet.count('<row ')
                features.append(f'rows={row_count}')
                # Check for conditional formatting
                if '<conditionalFormatting' in sheet:
                    features.append('COND_FMT')
    
    print(f'{name}: vis={vs:.4f} overall={ov:.4f} [{", ".join(features)}]')
