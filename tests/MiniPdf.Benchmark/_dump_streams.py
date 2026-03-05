"""Extract full REF stream content for near-miss files to understand LibreOffice's rendering."""
import fitz
import re

MINI_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"


def get_full_stream(doc, page_idx=0):
    page = doc[page_idx]
    contents = page.get_contents()
    all_text = []
    for c in contents:
        stream = doc.xref_stream(c)
        text = stream.decode('latin-1', errors='replace')
        all_text.append(text)
    return "\n".join(all_text)


# ===== 1. classic134_heatmap =====
print("=" * 70)
print("classic134_heatmap REF stream (first 150 lines)")
print("=" * 70)
ref = fitz.open(f"{REF_DIR}\\classic134_heatmap.pdf")
stream = get_full_stream(ref)
lines = stream.split('\n')
for i, line in enumerate(lines[:150]):
    print(f"  {i:4d}: {line.rstrip()}")
ref.close()

print("\n" + "=" * 70)
print("classic134_heatmap MINI stream (first 60 lines)")
print("=" * 70)
mini = fitz.open(f"{MINI_DIR}\\classic134_heatmap.pdf")
stream = get_full_stream(mini)
lines = stream.split('\n')
for i, line in enumerate(lines[:60]):
    print(f"  {i:4d}: {line.rstrip()}")
mini.close()


# ===== 2. classic149_merged_styled_sections =====
print("\n" + "=" * 70)
print("classic149_merged_styled_sections REF stream (first 120 lines)")
print("=" * 70)
ref = fitz.open(f"{REF_DIR}\\classic149_merged_styled_sections.pdf")
stream = get_full_stream(ref)
lines = stream.split('\n')
for i, line in enumerate(lines[:120]):
    print(f"  {i:4d}: {line.rstrip()}")
ref.close()

print("\n" + "=" * 70)
print("classic149_merged_styled_sections MINI stream (first 120 lines)")
print("=" * 70)
mini = fitz.open(f"{MINI_DIR}\\classic149_merged_styled_sections.pdf")
stream = get_full_stream(mini)
lines = stream.split('\n')
for i, line in enumerate(lines[:120]):
    print(f"  {i:4d}: {line.rstrip()}")
mini.close()


# ===== 3. classic142_styled_invoice REF =====
print("\n" + "=" * 70)
print("classic142_styled_invoice REF stream (first 120 lines)")
print("=" * 70)
ref = fitz.open(f"{REF_DIR}\\classic142_styled_invoice.pdf")
stream = get_full_stream(ref)
lines = stream.split('\n')
for i, line in enumerate(lines[:120]):
    print(f"  {i:4d}: {line.rstrip()}")
ref.close()

print("\n" + "=" * 70)
print("classic142_styled_invoice MINI stream (first 120 lines)")
print("=" * 70)
mini = fitz.open(f"{MINI_DIR}\\classic142_styled_invoice.pdf")
stream = get_full_stream(mini)
lines = stream.split('\n')
for i, line in enumerate(lines[:120]):
    print(f"  {i:4d}: {line.rstrip()}")
mini.close()
