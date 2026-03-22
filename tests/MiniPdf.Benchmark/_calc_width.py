"""Calculate text width using MiniPdf's EstimateCalibrTextWidth logic."""

CalibrWidths = [
    226, 326, 401, 498, 507, 715, 682, 221, 303, 303  ,  # ' ' to )
    498, 498, 250, 306, 252, 386,                        # * to /
    507, 507, 507, 507, 507, 507, 507, 507, 507, 507,  # 0-9
    268, 268, 498, 498, 498, 463, 894,                  # : to @
    579, 544, 533, 615, 488, 459, 631, 623, 252,        # A-I
    319, 520, 420, 855, 646, 662, 517, 673, 543, 459,  # J-S
    487, 642, 567, 890, 519, 487, 468,                  # T-Z
    307, 386, 307, 498, 498, 291,                        # [ to `
    479, 525, 423, 525, 498, 305, 471, 525, 229,        # a-i
    239, 455, 229, 799, 525, 527, 525, 525, 349, 391,  # j-s
    335, 525, 452, 715, 433, 453, 395,                  # t-z
    314, 460, 314, 498,                                  # { to ~
]

CalibriWidthScale = 0.87

def get_calibr_char_width(ch):
    code = ord(ch)
    if code < 0x20: return 0
    if 0x20 <= code <= 0x7E:
        return CalibrWidths[code - 0x20]
    # For smart quotes and other unicode, use Helvetica * 0.87
    # Helvetica width for quote-like chars is ~333
    return int(333 * CalibriWidthScale)

def estimate_width(text, fontSize, bold=False):
    latin_units = 0
    for ch in text:
        latin_units += get_calibr_char_width(ch)
    if bold:
        latin_units *= 1.03
    latin_units *= 0.977  # kerning
    return fontSize * latin_units / 1000

# The line that overflows
line_full = 'This Software License Agreement (the "Agreement") is entered into as of March 1, 2026, by'
line_break = 'This Software License Agreement (the "Agreement") is entered into as of March 1, 2026,'
line_break2 = 'This Software License Agreement (the "Agreement") is entered into as of March 1, 2026, by'

# Using ASCII quotes as in the DOCX
line_full_ascii = 'This Software License Agreement (the "Agreement") is entered into as of March 1, 2026, by'
line_break_ascii = 'This Software License Agreement (the "Agreement") is entered into as of March 1, 2026,'

fontSize = 11
textWidth = 432

print(f"Available width: {textWidth}pt")
print(f"Font size: {fontSize}pt")
print()

for label, line in [
    ('Full line (with "by")', line_full_ascii),
    ('Break line (without "by")', line_break_ascii),
]:
    w = estimate_width(line, fontSize)
    units = sum(get_calibr_char_width(ch) for ch in line)
    kerned = units * 0.977
    print(f'{label}:')
    print(f'  raw units: {units}, kerned: {kerned:.0f}, width: {w:.2f}pt, over: {w - textWidth:.2f}pt')
    print()

# Now let's check what different kerning values would give
print("=== Sensitivity to kerning factor ===")
for k in [0.977, 0.975, 0.973, 0.970, 0.965, 0.960]:
    units = sum(get_calibr_char_width(ch) for ch in line_full_ascii) * k
    w = fontSize * units / 1000
    print(f"kerning={k}: width={w:.2f}pt (delta={w - textWidth:.2f}pt)")
