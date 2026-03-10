import fitz
d1 = fitz.open('tests/Issue_Files/minipdf_xlsx/payroll-calculator_f.pdf')
d2 = fitz.open('tests/Issue_Files/reference_xlsx/payroll-calculator_f.pdf')

print(f"MiniPdf: {d1.page_count} pages, Reference: {d2.page_count} pages\n")

print("=== Reference page sizes ===")
sizes = {}
for i in range(d2.page_count):
    p = d2[i]
    key = f"{p.rect.width:.0f}x{p.rect.height:.0f}"
    sizes.setdefault(key, []).append(i+1)
for k, v in sizes.items():
    print(f"  {k}: pages {v}")

print("\n=== MiniPdf page sizes ===")
sizes = {}
for i in range(d1.page_count):
    p = d1[i]
    key = f"{p.rect.width:.0f}x{p.rect.height:.0f}"
    sizes.setdefault(key, []).append(i+1)
for k, v in sizes.items():
    print(f"  {k}: pages {v}")

print("\n=== Reference first 100 chars per page ===")
for i in range(d2.page_count):
    t = d2[i].get_text()[:150].replace('\n', ' | ')
    print(f"  R{i+1}: {t}")

print("\n=== MiniPdf first 100 chars per page ===")
for i in range(d1.page_count):
    t = d1[i].get_text()[:150].replace('\n', ' | ')
    print(f"  M{i+1}: {t}")
