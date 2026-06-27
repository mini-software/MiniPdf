"""
Automated benchmark test for PPTX-to-PDF conversion: converts existing PPTX
files to PDF via MiniPdf and LibreOffice, compares the results.

Prerequisites:
    pip install pymupdf Pillow
    LibreOffice installed (for reference PDF generation)
    .NET 9 SDK (for MiniPdf)

Usage:
    python run_benchmark_pptx.py                   # full pipeline for existing PPTX files
    python run_benchmark_pptx.py --skip-reference  # skip LibreOffice conversion
    python run_benchmark_pptx.py --skip-minipdf    # skip MiniPdf conversion
    python run_benchmark_pptx.py --compare-only    # only run comparison
"""

import argparse
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).parent.resolve()
PPTX_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "output_pptx"
MINIPDF_PDF_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "pdf_output_pptx"
REFERENCE_PDF_DIR = SCRIPT_DIR / "reference_pdfs_pptx"
REPORT_DIR = SCRIPT_DIR / "reports_pptx"


def banner(msg: str):
    print(f"\n{'='*60}")
    print(f"  {msg}")
    print(f"{'='*60}\n")


def run(cmd: list[str], cwd: str = None, check: bool = True) -> int:
    print(f"  > {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=cwd)
    if check and result.returncode != 0:
        print(f"  WARNING: Command exited with code {result.returncode}")
    return result.returncode


def step_generate_minipdf_pdfs(filter_pattern: str = None):
    banner("Step 1: Convert PPTX -> PDF (MiniPdf)")
    scripts_dir = SCRIPT_DIR / ".." / "MiniPdf.Scripts"
    cmd = ["dotnet", "run", "convert_pptx_to_pdf.cs"]
    if filter_pattern:
        cmd += ["--", str(PPTX_DIR.resolve()), str(MINIPDF_PDF_DIR.resolve()), filter_pattern]
    return run(cmd, cwd=str(scripts_dir))


def step_generate_reference_pdfs(filter_pattern: str = None):
    banner("Step 2: Convert PPTX -> PDF (LibreOffice Reference)")
    cmd = [sys.executable, "generate_reference_pdfs_pptx.py",
           "--pptx-dir", str(PPTX_DIR.resolve()),
           "--pdf-dir", str(REFERENCE_PDF_DIR.resolve())]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    return run(cmd, cwd=str(SCRIPT_DIR), check=False)


def step_compare(ai_compare: bool = False, ai_max_pages: int = 1, ai_threshold: float = 0.90,
                 filter_pattern: str = None):
    banner("Step 3: Compare MiniPdf vs Reference")
    cmd = [
        sys.executable, "compare_pdfs.py",
        "--minipdf-dir", str(MINIPDF_PDF_DIR.resolve()),
        "--reference-dir", str(REFERENCE_PDF_DIR.resolve()),
        "--report-dir", str(REPORT_DIR.resolve()),
    ]
    if ai_compare:
        cmd += ["--ai-compare", "--ai-max-pages", str(ai_max_pages), "--ai-threshold", str(ai_threshold)]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    return run(cmd, cwd=str(SCRIPT_DIR))


def step_analyze_report():
    banner("Step 4: Analysis Summary")
    json_path = REPORT_DIR / "comparison_report.json"
    md_path = REPORT_DIR / "comparison_report.md"

    if json_path.exists():
        import json
        with open(json_path, "r", encoding="utf-8") as handle:
            results = json.load(handle)

        total = len(results)
        scores = [r.get("overall_score", 0) for r in results]
        avg = sum(scores) / total if total else 0
        excellent = sum(1 for score in scores if score >= 0.9)
        good = sum(1 for score in scores if 0.7 <= score < 0.9)
        poor = sum(1 for score in scores if score < 0.7)

        print(f"  Total test cases: {total}")
        print(f"  Average score:    {avg:.4f}")
        print(f"  Excellent (>=0.9): {excellent}")
        print(f"  Good (0.7-0.9):   {good}")
        print(f"  Poor (<0.7):      {poor}")
        print()
        print(f"  Full report: {md_path}")
        print(f"  JSON data:   {json_path}")
    else:
        print("  No report found. Run the full pipeline first.")


def main():
    parser = argparse.ArgumentParser(description="MiniPdf PPTX Benchmark Pipeline")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only process files matching this substring")
    parser.add_argument("--skip-minipdf", action="store_true", help="Skip MiniPdf PDF conversion")
    parser.add_argument("--skip-reference", action="store_true", help="Skip reference conversion")
    parser.add_argument("--compare-only", action="store_true", help="Only run comparison step")
    parser.add_argument("--ai-compare", action="store_true",
                        help="Enable AI visual comparison (requires openai package + API key)")
    parser.add_argument("--ai-max-pages", type=int, default=1, metavar="N",
                        help="Max pages per PDF to send to AI (default: 1)")
    parser.add_argument("--ai-threshold", type=float, default=0.97, metavar="T",
                        help="Skip AI call when pixel score >= threshold (default: 0.97)")
    args = parser.parse_args()

    banner("MiniPdf PPTX Benchmark Pipeline")
    print(f"  PPTX dir:       {PPTX_DIR.resolve()}")
    print(f"  MiniPdf PDFs:   {MINIPDF_PDF_DIR.resolve()}")
    print(f"  Reference PDFs: {REFERENCE_PDF_DIR.resolve()}")
    print(f"  Reports:        {REPORT_DIR.resolve()}")

    compare_kwargs = dict(
        ai_compare=args.ai_compare,
        ai_max_pages=args.ai_max_pages,
        ai_threshold=args.ai_threshold,
        filter_pattern=args.filter)

    if args.compare_only:
        step_compare(**compare_kwargs)
        step_analyze_report()
        return

    if not args.skip_minipdf:
        step_generate_minipdf_pdfs(filter_pattern=args.filter)

    if not args.skip_reference:
        step_generate_reference_pdfs(filter_pattern=args.filter)

    step_compare(**compare_kwargs)
    step_analyze_report()

    banner("PPTX Pipeline Complete")


if __name__ == "__main__":
    main()