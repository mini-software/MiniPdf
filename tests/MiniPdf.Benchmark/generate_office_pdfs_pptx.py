"""Generate reference PDFs from .pptx files using Microsoft PowerPoint COM automation.

Prerequisites:
    - Microsoft PowerPoint must be installed on Windows.
    - pip install pywin32

Usage:
    python generate_office_pdfs_pptx.py --pptx-dir ./pptx --pdf-dir ./office_pptx
"""

import argparse
import gc
import os
import sys
from pathlib import Path


def main():
    try:
        import win32com.client
    except ImportError:
        print("ERROR: pywin32 not installed. Install with: pip install pywin32")
        sys.exit(1)

    parser = argparse.ArgumentParser(description="Generate Office (PowerPoint) reference PDFs")
    parser.add_argument("--pptx-dir", required=True, help="Directory containing .pptx files")
    parser.add_argument("--pdf-dir", required=True, help="Output directory for Office-generated PDFs")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only convert files whose name contains this substring")
    parser.add_argument("--force", action="store_true",
                        help="Overwrite existing PDFs (default: skip existing PDFs)")
    args = parser.parse_args()

    pptx_dir = os.path.abspath(args.pptx_dir)
    pdf_dir = os.path.abspath(args.pdf_dir)
    if not os.path.isdir(pptx_dir):
        print(f"ERROR: pptx directory not found: {pptx_dir}")
        sys.exit(1)

    os.makedirs(pdf_dir, exist_ok=True)
    pptx_files = sorted(Path(pptx_dir).glob("*.pptx"))
    if args.filter:
        pptx_files = [path for path in pptx_files if args.filter.lower() in path.stem.lower()]
    if not pptx_files:
        print("No .pptx files found.")
        sys.exit(1)

    powerpoint = win32com.client.DispatchEx("PowerPoint.Application")
    passed = 0
    failed = 0
    skipped = 0
    try:
        for pptx_path in pptx_files:
            pdf_path = os.path.join(pdf_dir, pptx_path.stem + ".pdf")
            if not args.force and os.path.isfile(pdf_path):
                print(f"  Skipping {pptx_path.name} (PDF exists)")
                skipped += 1
                continue

            print(f"  Converting {pptx_path.name} ...", end=" ", flush=True)
            presentation = None
            try:
                presentation = powerpoint.Presentations.Open(
                    os.path.abspath(str(pptx_path)), -1, 0, 0
                )
                presentation.SaveAs(os.path.abspath(pdf_path), 32)
                passed += 1
                print("OK")
            except Exception as error:
                failed += 1
                print(f"ERR: {error}")
            finally:
                if presentation is not None:
                    presentation.Close()
    finally:
        powerpoint.Quit()
        del powerpoint
        gc.collect()

    print(f"\nDone: {passed} succeeded, {failed} failed, {skipped} skipped out of {len(pptx_files)} files.")
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()