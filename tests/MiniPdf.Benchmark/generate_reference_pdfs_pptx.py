"""
Generate reference PDFs from .pptx files using LibreOffice (soffice).

Prerequisites:
    1. Install LibreOffice: https://www.libreoffice.org/download/
    2. Ensure 'soffice' is on PATH, or set LIBREOFFICE_PATH env var.

Usage:
    python generate_reference_pdfs_pptx.py [--pptx-dir ../MiniPdf.Scripts/output_pptx] [--pdf-dir ./reference_pdfs_pptx]
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
    for candidate in candidates:
        if os.path.isfile(candidate):
            return candidate

    which = shutil.which("soffice") or shutil.which("libreoffice")
    if which:
        return which

    print("ERROR: LibreOffice not found. Install it or set LIBREOFFICE_PATH env var.")
    sys.exit(1)


def convert_pptx_to_pdf(soffice: str, pptx_path: str, output_dir: str) -> bool:
    """Convert a single .pptx to PDF via LibreOffice."""
    try:
        with tempfile.TemporaryDirectory() as tmp_profile:
            cmd = [
                soffice,
                "--headless",
                "--norestore",
                f"-env:UserInstallation=file:///{tmp_profile.replace(os.sep, '/')}",
                "--convert-to", "pdf",
                "--outdir", output_dir,
                pptx_path,
            ]
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=120,
            )
            if result.returncode != 0:
                print(f"  ERR {Path(pptx_path).name}: {result.stderr.strip()}")
                return False
            return True
    except subprocess.TimeoutExpired:
        print(f"  TIMEOUT {Path(pptx_path).name}")
        return False
    except Exception as exc:
        print(f"  ERR {Path(pptx_path).name}: {exc}")
        return False


def main():
    parser = argparse.ArgumentParser(description="Generate reference PDFs from PPTX via LibreOffice")
    parser.add_argument("--pptx-dir", default=os.path.join("..", "MiniPdf.Scripts", "output_pptx"),
                        help="Directory containing .pptx files")
    parser.add_argument("--pdf-dir", default="reference_pdfs_pptx",
                        help="Output directory for reference PDFs")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only convert files whose name contains this substring")
    parser.add_argument("--force", action="store_true",
                        help="Overwrite existing PDFs (default: skip if PDF already exists)")
    args = parser.parse_args()

    pptx_dir = os.path.abspath(args.pptx_dir)
    pdf_dir = os.path.abspath(args.pdf_dir)

    if not os.path.isdir(pptx_dir):
        print(f"ERROR: pptx directory not found: {pptx_dir}")
        print("Create or copy PPTX files into the input directory first.")
        sys.exit(1)

    os.makedirs(pdf_dir, exist_ok=True)

    soffice = find_libreoffice()
    print(f"LibreOffice: {soffice}")
    print(f"Input:  {pptx_dir}")
    print(f"Output: {pdf_dir}")
    print()

    pptx_files = sorted(f for f in Path(pptx_dir).glob("*.pptx") if not f.name.startswith("~$"))
    if args.filter:
        pptx_files = [f for f in pptx_files if args.filter.lower() in f.stem.lower()]
    if not pptx_files:
        print("No .pptx files found.")
        sys.exit(1)

    passed = 0
    failed = 0
    skipped = 0
    for pptx in pptx_files:
        pdf_path = os.path.join(pdf_dir, pptx.stem + ".pdf")
        if not args.force and os.path.isfile(pdf_path):
            print(f"  Skipping {pptx.name} (PDF exists)")
            skipped += 1
            continue
        ok = convert_pptx_to_pdf(soffice, str(pptx), pdf_dir)
        if ok:
            print(f"  OK  {pptx.stem}.pdf")
            passed += 1
        else:
            failed += 1

    msg = f"\nDone! Passed: {passed}, Failed: {failed}"
    if skipped:
        msg += f", Skipped: {skipped}"
    msg += f", Total: {len(pptx_files)}"
    print(msg)


if __name__ == "__main__":
    main()