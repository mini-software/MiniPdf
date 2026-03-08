"""Simulate virtual overflow for classic09_long_text with different parameters"""

# Helvetica char widths (1/1000 em)
def helv_width(ch):
    WIDTHS = {
        'X': 667, 'A': 667, 'Y': 667, 'L': 556, 'o': 556, 'n': 556, 'g': 556,
        ' ': 278, 'T': 611, 'e': 556, 'x': 500, 't': 278, 'C': 722, 'l': 222,
        'u': 556, 'm': 833, 'S': 667, 'h': 556, 'r': 333,
    }
    return WIDTHS.get(ch, 556)

def fitting_chars(text, width_pts, font_size, scale=0.86):
    used = 0
    for i, ch in enumerate(text):
        used += helv_width(ch) * font_size / 1000.0 * scale
        if used > width_pts:
            return max(1, i)
    return len(text)

# classic09 data
rows = [
    "Long Text Column",      # 16 chars
    "X" * 500,               # 500 chars
    "A" * 501,               # 501 chars
    "Short",                 # 5 chars
    "Y" * 1000,              # 1000 chars
]

font_size = 11.0
default_col_chars = 8.43
char_to_pts = 5.62
default_col_pts = default_col_chars * char_to_pts  # 47.38
line_height = 15.0
page_height = 792.0
margin_top = 72.0
margin_bottom = 72.0
usable = page_height - margin_top - margin_bottom  # 648

print(f"Default column pts: {default_col_pts:.2f}")
print(f"Usable page height: {usable:.2f}")
print()

# Test different configurations
configs = [
    ("Current (scale=0.86, no padding)", 0.86, 0),
    ("No scale, no padding", 1.0, 0),
    ("No scale, padding=4.25pt", 1.0, 4.25),
    ("No scale, padding=6pt", 1.0, 6),
    ("No scale, padding=8pt", 1.0, 8),
    ("No scale, padding=10pt", 1.0, 10),
    ("No scale, padding=11pt", 1.0, 11),
    ("No scale, padding=12pt", 1.0, 12),
    ("Scale=0.86, padding=4.25pt", 0.86, 4.25),
    ("Scale=0.86, padding=8pt", 0.86, 8),
    ("Scale=0.86, padding=12pt", 0.86, 12),
    ("Scale=0.86, padding=16pt", 0.86, 16),
    ("Scale=0.86, padding=18pt", 0.86, 18),
]

for label, scale, padding in configs:
    wrap_width = default_col_pts - padding
    # Use inverse of scale to pass to FittingChars (which internally applies CalibriFittingScale=0.86)
    # Actually, just compute directly without the function
    total_extras = 0
    details = []
    for row_text in rows:
        # Check if this row triggers overflow (single column, text overflows page width)
        page_width = 612
        margin_left = 54
        page_clip_width = page_width - margin_left
        fit = fitting_chars(row_text, page_clip_width, font_size, scale=0.86)  # rendering always uses 0.86
        if fit >= len(row_text):
            continue  # text fits, no overflow
        
        # Calculate wrap chars at narrower width
        used = 0
        wrap_chars = 0
        for ci, ch in enumerate(row_text):
            used += helv_width(ch) * font_size / 1000.0 * scale
            if used > wrap_width:
                wrap_chars = max(1, ci)
                break
        if wrap_chars == 0:
            wrap_chars = len(row_text)
        
        import math
        virtual_lines = math.ceil(len(row_text) / wrap_chars)
        extras = virtual_lines - 1
        total_extras += extras
        details.append(f"{len(row_text)}c→wrap{wrap_chars}→{extras}extra")
    
    overflow = line_height * total_extras
    # Calculate pages
    content_y = page_height - margin_top - 5 * line_height  # 5 rows at 15pt each
    space_left = content_y - margin_bottom
    if overflow <= space_left:
        total_pages = 1
    else:
        remaining = overflow - space_left
        extra_pages = math.ceil(remaining / usable)
        total_pages = 1 + extra_pages
    
    marker = " ✓" if total_pages == 12 else ""
    print(f"{label:45s}: wrap_w={wrap_width:6.1f} extras={total_extras:4d} overflow={overflow:7.0f} pages={total_pages:2d}{marker}")
    if total_pages >= 10:
        for d in details:
            print(f"    {d}")
