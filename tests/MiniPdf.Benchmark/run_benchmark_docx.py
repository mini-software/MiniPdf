"""
Automated benchmark test for DOCX-to-PDF conversion: generates DOCX files,
converts them to PDF via MiniPdf and LibreOffice, compares the results.

This mirrors the XLSX benchmark pipeline but for Word documents.

Prerequisites:
    pip install python-docx pymupdf Pillow
    LibreOffice installed (for reference PDF generation)
    .NET 9 SDK (for MiniPdf)

Usage:
    python run_benchmark_docx.py                   # full pipeline
    python run_benchmark_docx.py --skip-generate   # skip DOCX generation
    python run_benchmark_docx.py --skip-reference  # skip LibreOffice conversion
    python run_benchmark_docx.py --skip-minipdf    # skip MiniPdf conversion
    python run_benchmark_docx.py --compare-only    # only run comparison
"""

import argparse
import os
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).parent.resolve()
DOCX_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "output_docx"
MINIPDF_PDF_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "pdf_output_docx"
REFERENCE_PDF_DIR = SCRIPT_DIR / "reference_pdfs_docx"
OFFICE_PDF_DIR = SCRIPT_DIR / "office_pdfs_docx"
REPORT_DIR = SCRIPT_DIR / "reports_docx"


def banner(msg: str):
    print(f"\n{'='*60}")
    print(f"  {msg}")
    print(f"{'='*60}\n")


def run(cmd: list[str], cwd: str = None, check: bool = True) -> int:
    """Run a command and return exit code."""
    print(f"  > {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=cwd)
    if check and result.returncode != 0:
        print(f"  WARNING: Command exited with code {result.returncode}")
    return result.returncode


def step_generate_docx():
    """Step 1: Generate test DOCX files using python-docx."""
    banner("Step 1: Generate Test DOCX Files")
    scripts_dir = SCRIPT_DIR / ".." / "MiniPdf.Scripts"
    return run(
        [sys.executable, "generate_classic_docx.py"],
        cwd=str(scripts_dir),
    )


def step_generate_minipdf_pdfs(filter_pattern: str = None):
    """Step 2: Convert DOCX files to PDF using MiniPdf."""
    banner("Step 2: Convert DOCX -> PDF (MiniPdf)")
    scripts_dir = SCRIPT_DIR / ".." / "MiniPdf.Scripts"
    cmd = ["dotnet", "run", "convert_docx_to_pdf.cs"]
    if filter_pattern:
        cmd += ["--", str(DOCX_DIR.resolve()), str(MINIPDF_PDF_DIR.resolve()), filter_pattern]
    return run(cmd, cwd=str(scripts_dir))


def step_generate_reference_pdfs(filter_pattern: str = None, engine: str = "libre"):
    """Step 3: Convert DOCX files to PDF using the chosen reference engine."""
    if engine == "office":
        banner("Step 3: Convert DOCX -> PDF (Office / Word COM Reference)")
        cmd = [sys.executable, "generate_office_pdfs_docx.py",
               "--docx-dir", str(DOCX_DIR.resolve()),
               "--pdf-dir", str(REFERENCE_PDF_DIR.resolve())]
    else:
        banner("Step 3: Convert DOCX -> PDF (LibreOffice Reference)")
        cmd = [sys.executable, "generate_reference_pdfs_docx.py",
               "--docx-dir", str(DOCX_DIR.resolve()),
               "--pdf-dir", str(REFERENCE_PDF_DIR.resolve())]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    return run(cmd, cwd=str(SCRIPT_DIR), check=False)


def step_generate_office_pdfs(filter_pattern: str = None):
    """Step 3b: Convert DOCX files to PDF using Office (Word COM)."""
    banner("Step 3b: Convert DOCX -> PDF (Office / Word COM)")
    cmd = [sys.executable, "generate_office_pdfs_docx.py",
           "--docx-dir", str(DOCX_DIR.resolve()),
           "--pdf-dir", str(OFFICE_PDF_DIR.resolve())]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    return run(cmd, cwd=str(SCRIPT_DIR), check=False)


def step_compare(ai_compare: bool = False, ai_max_pages: int = 1, ai_threshold: float = 0.90,
                 use_office: bool = False, filter_pattern: str = None):
    """Step 4: Compare MiniPdf PDFs against reference PDFs."""
    banner("Step 4: Compare MiniPdf vs Reference")
    cmd = [
        sys.executable, "compare_pdfs.py",
        "--minipdf-dir", str(MINIPDF_PDF_DIR.resolve()),
        "--reference-dir", str(REFERENCE_PDF_DIR.resolve()),
        "--report-dir", str(REPORT_DIR.resolve()),
    ]
    if use_office and OFFICE_PDF_DIR.is_dir():
        cmd += ["--office-dir", str(OFFICE_PDF_DIR.resolve())]
    if ai_compare:
        cmd += ["--ai-compare", "--ai-max-pages", str(ai_max_pages), "--ai-threshold", str(ai_threshold)]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    return run(cmd, cwd=str(SCRIPT_DIR))


def step_analyze_report():
    """Step 5: Print key findings from the report."""
    banner("Step 5: Analysis Summary")
    json_path = REPORT_DIR / "comparison_report.json"
    md_path = REPORT_DIR / "comparison_report.md"

    if json_path.exists():
        import json
        with open(json_path, "r", encoding="utf-8") as f:
            results = json.load(f)

        total = len(results)
        scores = [r.get("overall_score", 0) for r in results]
        avg = sum(scores) / total if total else 0
        excellent = sum(1 for s in scores if s >= 0.9)
        good = sum(1 for s in scores if 0.7 <= s < 0.9)
        poor = sum(1 for s in scores if s < 0.7)

        print(f"  Total test cases: {total}")
        print(f"  Average score:    {avg:.4f}")
        print(f"  Excellent (>=0.9): {excellent}")
        print(f"  Good (0.7-0.9):   {good}")
        print(f"  Poor (<0.7):      {poor}")
        print()

        if poor > 0:
            print("  Cases needing improvement:")
            for r in sorted(results, key=lambda x: x.get("overall_score", 0)):
                score = r.get("overall_score", 0)
                if score < 0.7:
                    print(f"    - {r['name']}: {score:.4f}")
            print()

        print(f"  Full report: {md_path}")
        print(f"  JSON data:   {json_path}")
    else:
        print("  No report found. Run the full pipeline first.")


def main():
    parser = argparse.ArgumentParser(description="MiniPdf DOCX Benchmark Pipeline")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only process files matching this substring (e.g. 'heading' or 'table_border')")
    parser.add_argument("--skip-generate", action="store_true", help="Skip DOCX generation")
    parser.add_argument("--skip-minipdf", action="store_true", help="Skip MiniPdf PDF conversion")
    parser.add_argument("--skip-reference", action="store_true", help="Skip reference conversion")
    parser.add_argument("--engine", choices=["libre", "office"], default="office",
                        help="Reference engine: office (MS Office COM, default) or libre (LibreOffice)")
    parser.add_argument("--with-office", action="store_true",
                        help="Also convert via Office (Word COM) and include in comparison")
    parser.add_argument("--skip-office", action="store_true", help="Skip Office conversion (when --with-office)")
    parser.add_argument("--compare-only", action="store_true", help="Only run comparison step")
    parser.add_argument("--ai-compare", action="store_true",
                        help="Enable AI visual comparison (requires openai package + API key)")
    parser.add_argument("--ai-max-pages", type=int, default=1, metavar="N",
                        help="Max pages per PDF to send to AI (default: 1)")
    parser.add_argument("--ai-threshold", type=float, default=0.97, metavar="T",
                        help="Skip AI call when pixel score >= threshold (default: 0.97)")
    args = parser.parse_args()

    banner("MiniPdf DOCX Benchmark Pipeline")
    print(f"  DOCX dir:       {DOCX_DIR.resolve()}")
    print(f"  MiniPdf PDFs:   {MINIPDF_PDF_DIR.resolve()}")
    print(f"  Reference PDFs: {REFERENCE_PDF_DIR.resolve()}")
    print(f"  Ref engine:     {args.engine}")
    if args.with_office:
        print(f"  Office PDFs:    {OFFICE_PDF_DIR.resolve()}")
    print(f"  Reports:        {REPORT_DIR.resolve()}")

    ai_kwargs = dict(ai_compare=args.ai_compare, ai_max_pages=args.ai_max_pages, ai_threshold=args.ai_threshold)
    compare_kwargs = dict(**ai_kwargs, use_office=args.with_office)
    filt = args.filter

    if args.compare_only:
        step_compare(**compare_kwargs, filter_pattern=filt)
        step_analyze_report()
        return

    if not args.skip_generate and not filt:
        step_generate_docx()

    if not args.skip_minipdf:
        step_generate_minipdf_pdfs(filter_pattern=filt)

    if not args.skip_reference:
        step_generate_reference_pdfs(filter_pattern=filt, engine=args.engine)

    if args.with_office and not args.skip_office:
        step_generate_office_pdfs(filter_pattern=filt)

    step_compare(**compare_kwargs, filter_pattern=filt)
    step_analyze_report()

    banner("DOCX Pipeline Complete")


if __name__ == "__main__":
    main()
