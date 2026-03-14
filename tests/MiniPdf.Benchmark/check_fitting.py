"""Manually compute FittingChars for X at 11pt in different widths."""
# Helvetica X = 667
# CalibriFittingScale = 0.86
char_w = 667
font_size = 11.0
scale = 0.86

per_x = char_w * font_size / 1000.0 * scale
print(f"Per X: {per_x:.4f}pt")

# How many fit in various widths?
for w in [541.28, 559.28, 565.0, 47.38]:
    n = int(w / per_x)
    print(f"  Width {w:.2f}pt -> {n} X chars fit, total={n*per_x:.2f}pt")

# Font size might be scaled for fit?
# Let me check what MiniPdf would do
# The render font size is options.FontSize (default 11) but cellFontSizeForFit uses cell font size
print()
print(f"At fontSize=10.56 (printScale?): {667*10.56/1000*0.86:.4f}pt per X")

# Actual MiniPdf PDF bbox: text starts at 54.0 and ends at 618.9
# That's 564.9pt for 77 chars
# 564.9 / 77 = 7.33pt per X (rendered width)
# But in Helvetica unscaled: 667*11/1000 = 7.337pt per X
# So the text is rendered at full Helvetica width, no Tz scaling
print()
print(f"Helvetica X rendered: {667*11/1000:.4f}pt")
print(f"77 X at full Helvetica: {77*667*11/1000:.2f}pt")
print(f"83 X at Calibri (reference): ref bbox width = {528.8-55.0:.1f}pt")
print(f"Per X in reference: {(528.8-55.0)/83:.4f}pt")
