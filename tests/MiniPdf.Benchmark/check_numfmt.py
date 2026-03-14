"""Check if number formatting differences contribute to text gap."""
import fitz

# classic16_percentage_strings (text=0.9877) 
case = 'classic16_percentage_strings'
ref = fitz.open(f'reference_pdfs/{case}.pdf')
mini = fitz.open(f'..\\MiniPdf.Scripts\\pdf_output\\{case}.pdf')

ref_text = ref[0].get_text().strip()
mini_text = mini[0].get_text().strip()

ref_lines = ref_text.split('\n')
mini_lines = mini_text.split('\n')

print(f'=== {case} ===')
print(f'Ref lines: {len(ref_lines)}, Mini lines: {len(mini_lines)}')

# Show differences
import difflib
diff = list(difflib.unified_diff(ref_lines, mini_lines, lineterm='', n=0))
for line in diff[:40]:
    print(line)

# Check classic125_solid_fills (text=0.9869)
print('\n')
case2 = 'classic125_solid_fills'
ref2 = fitz.open(f'reference_pdfs/{case2}.pdf')
mini2 = fitz.open(f'..\\MiniPdf.Scripts\\pdf_output\\{case2}.pdf')
ref2_text = ref2[0].get_text().strip()
mini2_text = mini2[0].get_text().strip()
diff2 = list(difflib.unified_diff(ref2_text.split('\n'), mini2_text.split('\n'), lineterm='', n=0))
print(f'=== {case2} ===')
for line in diff2[:40]:
    print(line)

# Check classic49_contact_list (text=0.9737)
print('\n')
case3 = 'classic49_contact_list'
ref3 = fitz.open(f'reference_pdfs/{case3}.pdf')
mini3 = fitz.open(f'..\\MiniPdf.Scripts\\pdf_output\\{case3}.pdf')
ref3_text = ref3[0].get_text().strip()
mini3_text = mini3[0].get_text().strip()
diff3 = list(difflib.unified_diff(ref3_text.split('\n'), mini3_text.split('\n'), lineterm='', n=0))
print(f'=== {case3} ===')
for line in diff3[:40]:
    print(line)
