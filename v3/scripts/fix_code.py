#!/usr/bin/env python3
"""Apply targeted fixes to MiniPdf source files."""
import os, re

_script_dir = os.path.dirname(os.path.abspath(__file__))
_repo_root = os.path.dirname(_script_dir)

def fix_file(path, old, new, expect_found=True):
    with open(path, encoding='utf-8') as f:
        content = f.read()
    if old not in content:
        print(f"  WARNING: Pattern not found in {path}")
        if expect_found:
            return False
    else:
        content = content.replace(old, new, 1)
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"  OK: replaced in {path}")
    return True


# ── 1. PdfWriter.cs: en/em-dash -> WinAnsiEncoding bytes ─────────────────────
fix_file(
    os.path.join(_repo_root, 'src/MiniPdf/PdfWriter.cs'),
    r"'\u2013' or '\u2014' or '\u2012' => '-',   // en-dash, em-dash",
    r"'\u2013' or '\u2012' => (char)0x96,  // en-dash -> WinAnsiEncoding 0x96" + "\n" + 
    r"                '\u2014' => (char)0x97,                // em-dash -> WinAnsiEncoding 0x97"
)

# ── 2. ExcelToPdfConverter.cs: Replace WrapCellText with clip ────────────────
old_wrap = """\
                        var maxChars = Math.Max(1, (int)(colWidths[i] / avgCharWidth));
                        var wrapped = WrapCellText(cellText, maxChars);
                        cellLines[i] = wrapped;
                        if (wrapped.Length > maxLinesInRow) maxLinesInRow = wrapped.Length;"""

new_clip = """\
                        var maxChars = Math.Max(1, (int)(colWidths[i] / avgCharWidth));
                        // Clip to column width - matches LibreOffice default cell-overflow behaviour.
                        // Long text is truncated at the boundary (no word-wrap; wrap_text not set).
                        var clipped = cellText.Length > maxChars ? cellText[..maxChars] : cellText;
                        cellLines[i] = new[] { clipped };"""

fix_file(os.path.join(_repo_root, 'src/MiniPdf/ExcelToPdfConverter.cs'), old_wrap, new_clip)

print("All done.")
