import zipfile, xml.etree.ElementTree as ET, json
ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main','wp':'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing','wps':'http://schemas.microsoft.com/office/word/2010/wordprocessingShape'}
out=[]
with zipfile.ZipFile('tests/Issue_Files/docx/Confirmatory_Affidavit.docx') as z:
    root=ET.fromstring(z.read('word/document.xml'))
p1=root.find('.//w:body/w:p',ns)
anchors=p1.findall('.//wp:anchor',ns)
out.append({'anchors':len(anchors)})
for ai,a in enumerate(anchors,1):
    t=a.find('.//wps:txbx/w:txbxContent',ns)
    rec={'anchor':ai,'has_txbx':t is not None,'paras':[]}
    if t is not None:
        paras=t.findall('./w:p',ns)
        rec['para_count']=len(paras)
        for pi,p in enumerate(paras,1):
            txt=''.join((n.text or '') for n in p.findall('.//w:t',ns))
            sp=p.find('./w:pPr/w:spacing',ns)
            bef=sp.attrib.get('{%s}before'%ns['w'],'') if sp is not None else ''
            rec['paras'].append({'i':pi,'len':len(txt),'before':bef,'text':txt})
    out.append(rec)
open('tests/Issue_Files/docx/_inspect_tbx.json','w',encoding='utf-8').write(json.dumps(out,ensure_ascii=False,indent=2))
print('wrote json')
