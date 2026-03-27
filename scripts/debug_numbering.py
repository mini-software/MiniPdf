import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/document.xml'))
    root = tree.getroot()
    body = root.find(f'{{{ns["w"]}}}body')
    
    # Find paragraphs with Heading1 style and check for numbering
    for i, p in enumerate(body):
        tag = p.tag.split('}')[1] if '}' in p.tag else p.tag
        if tag != 'p': continue
        ppr = p.find(f'{{{ns["w"]}}}pPr')
        if ppr is None: continue
        style = ppr.find(f'{{{ns["w"]}}}pStyle')
        if style is None: continue
        sval = style.get(f'{{{ns["w"]}}}val')
        if 'Heading' not in sval and 'heading' not in sval: continue
        
        numPr = ppr.find(f'{{{ns["w"]}}}numPr')
        text = ''.join(r.find(f'{{{ns["w"]}}}t').text or '' for r in p.findall(f'.//{{{ns["w"]}}}r') if r.find(f'{{{ns["w"]}}}t') is not None)
        num_info = ''
        if numPr is not None:
            ilvl = numPr.find(f'{{{ns["w"]}}}ilvl')
            numId = numPr.find(f'{{{ns["w"]}}}numId')
            wns = ns['w']
            nid = numId.get(f'{{{wns}}}val') if numId is not None else '?'
            ilv = ilvl.get(f'{{{wns}}}val') if ilvl is not None else '?'
            num_info = f' numId={nid} ilvl={ilv}'
        print(f'E{i}: {sval}{num_info} [{text[:60]}]')

    # Check numbering definitions
    if 'word/numbering.xml' in z.namelist():
        ntree = ET.parse(z.open('word/numbering.xml'))
        nroot = ntree.getroot()
        nums = nroot.findall(f'{{{ns["w"]}}}num')
        print(f'\nNumbering definitions: {len(nums)}')
        for num in nums[:5]:
            numId = num.get(f'{{{ns["w"]}}}numId')
            absRef = num.find(f'{{{ns["w"]}}}abstractNumId')
            absId = absRef.get(f'{{{ns["w"]}}}val') if absRef is not None else '?'
            print(f'  numId={numId} -> abstractNumId={absId}')
        
        # Check abstract numbering
        absNums = nroot.findall(f'{{{ns["w"]}}}abstractNum')
        for an in absNums[:5]:
            anId = an.get(f'{{{ns["w"]}}}abstractNumId')
            lvls = an.findall(f'{{{ns["w"]}}}lvl')
            for lvl in lvls[:3]:
                ilvl = lvl.get(f'{{{ns["w"]}}}ilvl')
                numFmt = lvl.find(f'{{{ns["w"]}}}numFmt')
                fmt = numFmt.get(f'{{{ns["w"]}}}val') if numFmt is not None else '?'
                lvlText = lvl.find(f'{{{ns["w"]}}}lvlText')
                lt = lvlText.get(f'{{{ns["w"]}}}val') if lvlText is not None else '?'
                start = lvl.find(f'{{{ns["w"]}}}start')
                sv = start.get(f'{{{ns["w"]}}}val') if start is not None else '?'
                print(f'  absNum {anId} lvl {ilvl}: fmt={fmt} text=[{lt}] start={sv}')
