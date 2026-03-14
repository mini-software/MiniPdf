"""Compare text content between reference and MiniPdf for chart cases."""
import fitz

def get_text_words(pdf_path, page=0):
    doc = fitz.open(pdf_path)
    pg = doc[page]
    words = pg.get_text("words")
    return [(round(w[0],1), round(w[1],1), w[4]) for w in words]

case = 'classic103_pie_chart_with_labels'
ref = get_text_words(f'reference_pdfs/{case}.pdf')
mini = get_text_words(f'..\\MiniPdf.Scripts\\pdf_output\\{case}.pdf')

ref_texts = set(w[2] for w in ref)
mini_texts = set(w[2] for w in mini)
print(f'=== {case} ===')
print(f'Ref words: {len(ref)}, Mini words: {len(mini)}')
print(f'In ref only: {ref_texts - mini_texts}')
print(f'In mini only: {mini_texts - ref_texts}')
print(f'Ref positions:')
for w in sorted(ref, key=lambda x: (x[1], x[0])):
    print(f'  x={w[0]:7.1f} y={w[1]:7.1f} "{w[2]}"')
print(f'Mini positions:')
for w in sorted(mini, key=lambda x: (x[1], x[0])):
    print(f'  x={w[0]:7.1f} y={w[1]:7.1f} "{w[2]}"')

# Check chart case with better score
print('\n')
case3 = 'classic91_simple_bar_chart'
ref3 = get_text_words(f'reference_pdfs/{case3}.pdf')
mini3 = get_text_words(f'..\\MiniPdf.Scripts\\pdf_output\\{case3}.pdf')
print(f'=== {case3} ===')
print(f'Ref words: {len(ref3)}, Mini words: {len(mini3)}')
print(f'In ref only: {set(w[2] for w in ref3) - set(w[2] for w in mini3)}')
print(f'In mini only: {set(w[2] for w in mini3) - set(w[2] for w in ref3)}')
