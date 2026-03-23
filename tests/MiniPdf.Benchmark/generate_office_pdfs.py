"""
Generate reference PDFs from .xlsx files using Microsoft Excel COM automation.

Prerequisites:
    - Microsoft Excel must be installed on the machine.
    - Windows only (uses COM Automation via win32com).
    - pip install pywin32

Usage:
    python generate_office_pdfs.py [--xlsx-dir ../MiniPdf.Scripts/output] [--pdf-dir ./office_pdfs]

This converts every .xlsx in the input directory to PDF using Excel,
producing the "Office ground truth" reference that MiniPdf output is compared against.
"""

import argparse
import gc
import os
import sys
import time
from pathlib import Path


def _create_excel():
    """Create a fresh Excel COM instance."""
    import win32com.client
    excel = win32com.client.Dispatch("Excel.Application")
    excel.Visible = False
    excel.DisplayAlerts = False
    return excel


def _quit_excel(excel):
    """Safely quit an Excel COM instance."""
    try:
        excel.Quit()
    except Exception:
        pass
    del excel
    gc.collect()


def main():
    try:
        import win32com.client  # noqa: F401
    except ImportError:
        print("ERROR: pywin32 not installed. Install with: pip install pywin32")
        sys.exit(1)

    parser = argparse.ArgumentParser(description="Generate Office (Excel) reference PDFs")
    parser.add_argument("--xlsx-dir", default=os.path.join("..", "MiniPdf.Scripts", "output"),
                        help="Directory containing .xlsx files")
    parser.add_argument("--pdf-dir", default="office_pdfs",
                        help="Output directory for Office-generated PDFs")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only convert files whose name contains this substring")
    parser.add_argument("--force", action="store_true",
                        help="Overwrite existing PDFs (default: skip if PDF already exists)")
    args = parser.parse_args()

    xlsx_dir = os.path.abspath(args.xlsx_dir)
    pdf_dir = os.path.abspath(args.pdf_dir)

    if not os.path.isdir(xlsx_dir):
        print(f"ERROR: xlsx directory not found: {xlsx_dir}")
        sys.exit(1)

    os.makedirs(pdf_dir, exist_ok=True)

    print(f"Excel COM Automation")
    print(f"Input:  {xlsx_dir}")
    print(f"Output: {pdf_dir}")
    print()

    xlsx_files = sorted(Path(xlsx_dir).glob("*.xlsx"))
    if args.filter:
        xlsx_files = [f for f in xlsx_files if args.filter.lower() in f.stem.lower()]
    if not xlsx_files:
        print("No .xlsx files found.")
        sys.exit(1)

    excel = _create_excel()
    passed = 0
    failed = 0
    consecutive_errors = 0

    skipped = 0
    for xlsx in xlsx_files:
        pdf_path = os.path.join(pdf_dir, xlsx.stem + ".pdf")
        if not args.force and os.path.isfile(pdf_path):
            print(f"  Skipping {xlsx.name} (PDF exists)")
            skipped += 1
            continue
        print(f"  Converting {xlsx.name} ...", end=" ", flush=True)
        try:
            wb = excel.Workbooks.Open(os.path.abspath(str(xlsx)))
            wb.ExportAsFixedFormat(0, os.path.abspath(pdf_path))
            wb.Close(False)
            passed += 1
            consecutive_errors = 0
            print("OK")
        except Exception as e:
            failed += 1
            consecutive_errors += 1
            print(f"ERR: {e}")
            # Try to close any open workbook
            try:
                wb.Close(False)
            except Exception:
                pass
            # If too many consecutive errors, restart Excel
            if consecutive_errors >= 3:
                print("    (restarting Excel COM...)")
                _quit_excel(excel)
                time.sleep(1)
                excel = _create_excel()
                consecutive_errors = 0

    _quit_excel(excel)
    msg = f"\nDone: {passed} succeeded, {failed} failed"
    if skipped:
        msg += f", {skipped} skipped"
    msg += f" out of {len(xlsx_files)} files."
    print(msg)


if __name__ == "__main__":
    main()
