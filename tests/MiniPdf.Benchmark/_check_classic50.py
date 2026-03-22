import json
data = json.load(open('reports_docx/comparison_report.json', encoding='utf-8'))
r = [x for x in data if 'classic50' in x.get('name','')]
if r:
    d = r[0]
    name = d['name']
    pages_mp = d['minipdf_pages']
    pages_ref = d['reference_pages']
    vis = d.get('visual_scores', '?')
    txt = d.get('text_similarity', '?')
    word = d.get('word_text_similarity', '?')
    print(f"Name: {name}")
    print(f"Pages: MiniPdf={pages_mp}, Ref={pages_ref}")
    print(f"Visual scores: {vis}")
    print(f"Text sim: {txt}")
    print(f"Word sim: {word}")
