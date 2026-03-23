"""
Generate reference PDFs from .docx files using LibreOffice (soffice).

Prerequisites:
    1. Install LibreOffice: https://www.libreoffice.org/download/
    2. Ensure 'soffice' is on PATH, or set LIBREOFFICE_PATH env var.

Usage:
    python generate_reference_pdfs_docx.py [--docx-dir ../MiniPdf.Scripts/output_docx] [--pdf-dir ./reference_pdfs_docx]
"""

import argparse
import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path


def find_libreoffice() -> str:
    """Locate the LibreOffice soffice executable."""
    env_path = os.environ.get("LIBREOFFICE_PATH")
    if env_path and os.path.isfile(env_path):
        return env_path

    candidates = [
        r"C:\Program Files\LibreOffice\program\soffice.exe",
        r"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
        "/Applications/LibreOffice.app/Contents/MacOS/soffice",
        "/usr/bin/soffice",
        "/usr/bin/libreoffice",
    ]
    for c in candidates:
        if os.path.isfile(c):
            return c

    which = shutil.which("soffice") or shutil.which("libreoffice")
    if which:
        return which

    print("ERROR: LibreOffice not found. Install it or set LIBREOFFICE_PATH env var.")
    sys.exit(1)


def convert_docx_to_pdf(soffice: str, docx_path: str, output_dir: str) -> bool:
    """Convert a single .docx to PDF via LibreOffice."""
    try:
        with tempfile.TemporaryDirectory() as tmp_profile:
            cmd = [
                soffice,
                "--headless",
                "--norestore",
                f"-env:UserInstallation=file:///{tmp_profile.replace(os.sep, '/')}",
                "--convert-to", "pdf",
                "--outdir", output_dir,
                docx_path,
            ]
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=120,
            )
            if result.returncode != 0:
                print(f"  ERR {Path(docx_path).name}: {result.stderr.strip()}")
                return False
            return True
    except subprocess.TimeoutExpired:
        print(f"  TIMEOUT {Path(docx_path).name}")
        return False
    except Exception as e:
        print(f"  ERR {Path(docx_path).name}: {e}")
        return False


def main():
    parser = argparse.ArgumentParser(description="Generate reference PDFs from DOCX via LibreOffice")
    parser.add_argument("--docx-dir", default=os.path.join("..", "MiniPdf.Scripts", "output_docx"),
                        help="Directory containing .docx files")
    parser.add_argument("--pdf-dir", default="reference_pdfs_docx",
                        help="Output directory for reference PDFs")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only convert files whose name contains this substring")
    parser.add_argument("--force", action="store_true",
                        help="Overwrite existing PDFs (default: skip if PDF already exists)")
    args = parser.parse_args()

    docx_dir = os.path.abspath(args.docx_dir)
    pdf_dir = os.path.abspath(args.pdf_dir)

    if not os.path.isdir(docx_dir):
        print(f"ERROR: docx directory not found: {docx_dir}")
        print("Run generate_classic_docx.py first to create test DOCX files.")
        sys.exit(1)

    os.makedirs(pdf_dir, exist_ok=True)

    soffice = find_libreoffice()
    print(f"LibreOffice: {soffice}")
    print(f"Input:  {docx_dir}")
    print(f"Output: {pdf_dir}")
    print()

    docx_files = sorted(Path(docx_dir).glob("*.docx"))
    if args.filter:
        docx_files = [f for f in docx_files if args.filter.lower() in f.stem.lower()]
    if not docx_files:
        print("No .docx files found.")
        sys.exit(1)

    passed = 0
    failed = 0
    skipped = 0
    for docx in docx_files:
        pdf_path = os.path.join(pdf_dir, docx.stem + ".pdf")
        if not args.force and os.path.isfile(pdf_path):
            print(f"  Skipping {docx.name} (PDF exists)")
            skipped += 1
            continue
        ok = convert_docx_to_pdf(soffice, str(docx), pdf_dir)
        if ok:
            pdf_name = docx.stem + ".pdf"
            print(f"  OK  {pdf_name}")
            passed += 1
        else:
            failed += 1

    msg = f"\nDone! Passed: {passed}, Failed: {failed}"
    if skipped:
        msg += f", Skipped: {skipped}"
    msg += f", Total: {len(docx_files)}"
    print(msg)


if __name__ == "__main__":
    main()
