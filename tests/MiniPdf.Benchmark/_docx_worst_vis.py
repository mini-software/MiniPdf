"""Analyze worst DOCX visual cases - check what features they use"""
import json, zipfile, xml.etree.ElementTree as ET, os

data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
worst_vis = sorted([d for d in data if d['visual_avg'] < 0.90], key=lambda x: x['visual_avg'])

for d in worst_vis:
    name = d['name']
    vis = d['visual_avg']
    print(f"\n{name}: vis={vis:.4f} text={d['text_similarity']:.4f} overall={d['overall_score']:.4f} pages={d['minipdf_pages']}/{d['reference_pages']}")
    
    # Check DOCX features
    docx_path = f"../MiniPdf.Scripts/output_docx/{name}.docx"
    if os.path.exists(docx_path):
        try:
            zf = zipfile.ZipFile(docx_path)
            doc_xml = zf.read('word/document.xml').decode('utf-8', errors='replace')
            features = []
            if '<w:tbl>' in doc_xml or '<w:tbl ' in doc_xml:
                features.append('TABLE')
            if '<w:shd' in doc_xml:
                features.append('SHADING')
            if 'w:pBdr' in doc_xml or 'w:tblBorders' in doc_xml:
                features.append('BORDERS')
            if '<wp:inline' in doc_xml or '<wp:anchor' in doc_xml:
                features.append('IMAGES')
            if '<w:numId' in doc_xml:
                features.append('LIST')
            if '<w:sectPr' in doc_xml:
                sect_count = doc_xml.count('<w:sectPr')
                if sect_count > 1:
                    features.append(f'MULTI-SECTION({sect_count})')
            if '<w:pgSz' in doc_xml and ('w:orient="landscape"' in doc_xml):
                features.append('LANDSCAPE')
            if '<w:cols' in doc_xml and 'w:num=' in doc_xml:
                features.append('COLUMNS')
            if '<w:tab' in doc_xml:
                features.append('TABS')
            if '<w:vertAlign' in doc_xml:
                features.append('VERT-ALIGN')
            if 'w:highlight' in doc_xml or 'w:u ' in doc_xml:
                features.append('FORMATTING')
            # Count tables
            tbl_count = doc_xml.count('<w:tbl>') + doc_xml.count('<w:tbl ')
            if tbl_count > 0:
                # Count rows in tables
                row_count = doc_xml.count('<w:tr>') + doc_xml.count('<w:tr ')
                features.append(f'ROWS={row_count}')
            
            print(f"  Features: {', '.join(features)}")
            zf.close()
        except Exception as e:
            print(f"  Error: {e}")
