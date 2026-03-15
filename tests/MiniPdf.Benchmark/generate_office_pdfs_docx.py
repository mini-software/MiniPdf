"""
Generate reference PDFs from .docx files using Microsoft Word COM automation.

Prerequisites:
    - Microsoft Word must be installed on the machine.
    - Windows only (uses COM Automation via win32com).
    - pip install pywin32

Usage:
    python generate_office_pdfs_docx.py [--docx-dir ../MiniPdf.Scripts/output_docx] [--pdf-dir ./office_pdfs_docx]
"""

import argparse
import gc
import os
import sys
import time
from pathlib import Path


def _create_word():
    """Create a fresh Word COM instance."""
    import win32com.client
    word = win32com.client.Dispatch("Word.Application")
    word.Visible = False
    word.DisplayAlerts = False
    return word


def _quit_word(word):
    """Safely quit a Word COM instance."""
    try:
        word.Quit()
    except Exception:
        pass
    del word
    gc.collect()


def main():
    try:
        import win32com.client  # noqa: F401
    except ImportError:
        print("ERROR: pywin32 not installed. Install with: pip install pywin32")
        sys.exit(1)

    parser = argparse.ArgumentParser(description="Generate Office (Word) reference PDFs")
    parser.add_argument("--docx-dir", default=os.path.join("..", "MiniPdf.Scripts", "output_docx"),
                        help="Directory containing .docx files")
    parser.add_argument("--pdf-dir", default="office_pdfs_docx",
                        help="Output directory for Office-generated PDFs")
    args = parser.parse_args()

    docx_dir = os.path.abspath(args.docx_dir)
    pdf_dir = os.path.abspath(args.pdf_dir)

    if not os.path.isdir(docx_dir):
        print(f"ERROR: docx directory not found: {docx_dir}")
        sys.exit(1)

    os.makedirs(pdf_dir, exist_ok=True)

    print(f"Word COM Automation")
    print(f"Input:  {docx_dir}")
    print(f"Output: {pdf_dir}")
    print()

    docx_files = sorted(Path(docx_dir).glob("*.docx"))
    if not docx_files:
        print("No .docx files found.")
        sys.exit(1)

    word = _create_word()
    passed = 0
    failed = 0
    consecutive_errors = 0

    for docx in docx_files:
        pdf_path = os.path.join(pdf_dir, docx.stem + ".pdf")
        print(f"  Converting {docx.name} ...", end=" ", flush=True)
        try:
            doc = word.Documents.Open(os.path.abspath(str(docx)))
            # ExportAsFixedFormat: OutputFileName, ExportFormat (wdExportFormatPDF=17)
            doc.ExportAsFixedFormat(os.path.abspath(pdf_path), 17)
            doc.Close(False)
            passed += 1
            consecutive_errors = 0
            print("OK")
        except Exception as e:
            failed += 1
            consecutive_errors += 1
            print(f"ERR: {e}")
            try:
                doc.Close(False)
            except Exception:
                pass
            if consecutive_errors >= 3:
                print("    (restarting Word COM...)")
                _quit_word(word)
                time.sleep(1)
                word = _create_word()
                consecutive_errors = 0

    _quit_word(word)
    print(f"\nDone: {passed} succeeded, {failed} failed out of {len(docx_files)} files.")


if __name__ == "__main__":
    main()
