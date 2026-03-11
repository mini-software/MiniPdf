import fitz, os, re, sys
from difflib import SequenceMatcher

base = os.path.dirname(os.path.abspath(__file__))
m = fitz.open(os.path.join(base, 'minipdf_xlsx', 'payroll-calculator_f.pdf'))
r = fitz.open(os.path.join(base, 'reference_xlsx', 'payroll-calculator_f.pdf'))

# Extract all text, normalized
def words(pdf):
    all_w = []
    for p in pdf:
        t = p.get_text()
        all_w.extend(t.split())
    return all_w

mw = words(m)
rw = words(r)

# Find words in LO but not in MP (missing text)
ms = set(mw)
rs = set(rw)
missing = sorted(rs - ms)
extra = sorted(ms - rs)

print("MP total words:", len(mw), "unique:", len(ms))
print("LO total words:", len(rw), "unique:", len(rs))
print("Common unique:", len(ms & rs))
print()

# Group missing words by type
nums_missing = [w for w in missing if re.match(r'^[\d,.$%()-]+$', w)]
text_missing = [w for w in missing if not re.match(r'^[\d,.$%()-]+$', w)]
nums_extra = [w for w in extra if re.match(r'^[\d,.$%()-]+$', w)]
text_extra = [w for w in extra if not re.match(r'^[\d,.$%()-]+$', w)]

print("Missing numbers (in LO, not MP):", len(nums_missing))
for w in nums_missing[:30]:
    print(" ", w)
print()
print("Extra numbers (in MP, not LO):", len(nums_extra))
for w in nums_extra[:30]:
    print(" ", w)
print()
print("Missing text (in LO, not MP):", len(text_missing))
for w in text_missing[:30]:
    print(" ", w)
print()
print("Extra text (in MP, not LO):", len(text_extra))
for w in text_extra[:30]:
    print(" ", w)
