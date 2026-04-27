import pdfplumber, sys
for label, path in [('REF','D:/git/MiniPdf/tests/Issue_Files/reference_docx/MODERN LIVING.pdf'),('GEN','D:/git/MiniPdf/tests/Issue_Files/minipdf_docx/MODERN LIVING.pdf')]:
    print('===',label,'===')
    with pdfplumber.open(path) as pdf:
        if len(pdf.pages)<2:continue
        page=pdf.pages[1]
        for w in page.extract_words():
            print(round(w['x0'],1), round(w['top'],1), round(w['bottom'],1), w['text'])
        print('PAGE size',page.width,page.height)
