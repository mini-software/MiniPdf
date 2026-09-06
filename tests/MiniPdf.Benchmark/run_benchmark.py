"""
Automated benchmark test: generates classic Excel files, combines them with
issue fixtures, converts them to PDF
via MiniPdf and Microsoft 365 by default, compares the results, and produces a report.

This is the single entry point for the full "self-evolution" pipeline.

Prerequisites:
    pip install openpyxl pymupdf pywin32
    Microsoft 365 Excel installed (default reference engine)
    .NET 9 SDK (for MiniPdf)

Usage:
    python run_benchmark.py                   # all classic + issue XLSX fixtures
    python run_benchmark.py --suite classic   # classic fixtures only
    python run_benchmark.py --suite issue     # issue fixtures only
    python run_benchmark.py --skip-generate   # skip Excel generation
    python run_benchmark.py --skip-reference   # skip reference conversion
    python run_benchmark.py --skip-minipdf     # skip MiniPdf conversion
    python run_benchmark.py --compare-only     # only run comparison (assumes PDFs exist)
"""

import argparse
import json
import os
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).parent.resolve()
XLSX_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "output"
ISSUE_XLSX_DIR = SCRIPT_DIR / ".." / "Issue_Files" / "xlsx"
MINIPDF_PDF_DIR = SCRIPT_DIR / ".." / "MiniPdf.Scripts" / "pdf_output"
REFERENCE_PDF_DIR = SCRIPT_DIR / "reference_pdfs"
OFFICE_PDF_DIR = SCRIPT_DIR / "office_pdfs"
REPORT_DIR = SCRIPT_DIR / "reports"


def configure_paths(args):
    """Override default benchmark paths without changing existing defaults."""
    global XLSX_DIR, ISSUE_XLSX_DIR, MINIPDF_PDF_DIR, REFERENCE_PDF_DIR, OFFICE_PDF_DIR, REPORT_DIR

    if args.source_dir:
        XLSX_DIR = Path(args.source_dir).resolve()
    if args.issue_source_dir:
        ISSUE_XLSX_DIR = Path(args.issue_source_dir).resolve()
    if args.minipdf_dir:
        MINIPDF_PDF_DIR = Path(args.minipdf_dir).resolve()
    if args.office_dir:
        OFFICE_PDF_DIR = Path(args.office_dir).resolve()
    if args.reference_dir:
        REFERENCE_PDF_DIR = Path(args.reference_dir).resolve()
    elif args.engine in {"o365", "office"}:
        REFERENCE_PDF_DIR = OFFICE_PDF_DIR
    if args.report_dir:
        REPORT_DIR = Path(args.report_dir).resolve()


def selected_source_dirs(suite: str) -> list[tuple[str, Path]]:
    """Return the source roots included in a benchmark suite."""
    sources = []
    if suite in {"all", "classic"}:
        sources.append(("classic", XLSX_DIR))
    if suite in {"all", "issue"}:
        sources.append(("issue", ISSUE_XLSX_DIR))
    return sources


def write_comparison_manifest(suite: str) -> Path:
    """Write one manifest for all selected classic and issue fixtures."""
    repo_root = SCRIPT_DIR.parents[1]
    cases = []
    case_sources = {}

    for case_suite, source_dir in selected_source_dirs(suite):
        for source_path in sorted(source_dir.glob("*.xlsx"), key=lambda path: path.name.lower()):
            name = source_path.stem
            if name in case_sources:
                raise ValueError(
                    f"Duplicate XLSX fixture name '{name}' in {case_sources[name]} and {source_path}"
                )
            case_sources[name] = source_path
            try:
                relative_source = source_path.resolve().relative_to(repo_root).as_posix()
            except ValueError:
                relative_source = source_path.resolve().as_posix()
            cases.append({
                "name": name,
                "case_id": name,
                "format": "xlsx",
                "source_path": relative_source,
                "suite": case_suite,
                "tags": [case_suite, "xlsx"],
            })

    if not cases:
        raise ValueError(f"No XLSX fixtures found for suite '{suite}'")

    REPORT_DIR.mkdir(parents=True, exist_ok=True)
    manifest_path = REPORT_DIR / "comparison_manifest.json"
    with open(manifest_path, "w", encoding="utf-8") as manifest_file:
        json.dump({"cases": cases}, manifest_file, ensure_ascii=False, indent=2)
        manifest_file.write("\n")
    return manifest_path


def banner(msg: str):
    print(f"\n{'='*60}")
    print(f"  {msg}")
    print(f"{'='*60}\n")


def run(cmd: list[str], cwd: str = None, check: bool = True) -> int:
    """Run a command and return exit code."""
    print(f"  > {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=cwd)
    if check and result.returncode != 0:
        raise subprocess.CalledProcessError(result.returncode, cmd)
    return result.returncode


def step_generate_xlsx():
    """Step 1: Generate test Excel files using openpyxl."""
    banner("Step 1: Generate Test Excel Files")
    scripts_dir = SCRIPT_DIR / ".." / "MiniPdf.Scripts"
    return run(
        [sys.executable, "generate_classic_xlsx.py"],
        cwd=str(scripts_dir),
    )


def step_generate_minipdf_pdfs(suite: str, filter_pattern: str = None):
    """Step 2: Convert Excel files to PDF using MiniPdf."""
    banner("Step 2: Convert Excel -> PDF (MiniPdf)")
    scripts_dir = SCRIPT_DIR / ".." / "MiniPdf.Scripts"

    for case_suite, source_dir in selected_source_dirs(suite):
        print(f"  [{case_suite}] {source_dir.resolve()}")
        cmd = ["dotnet", "run", "--configuration", "Release", "--no-cache", "convert_xlsx_to_pdf.cs", "--",
               str(source_dir.resolve()), str(MINIPDF_PDF_DIR.resolve())]
        if filter_pattern:
            cmd += [filter_pattern]
        run(cmd, cwd=str(scripts_dir))


def step_generate_reference_pdfs(suite: str, filter_pattern: str = None, engine: str = "o365", force: bool = False):
    """Step 3: Convert Excel files to PDF using the chosen reference engine."""
    if engine in {"o365", "office"}:
        banner("Step 3: Convert Excel -> PDF (Office / Excel COM Reference)")
        reference_script = "generate_office_pdfs.py"
    else:
        banner("Step 3: Convert Excel -> PDF (LibreOffice Reference)")
        reference_script = "generate_reference_pdfs.py"

    result = 0
    for case_suite, source_dir in selected_source_dirs(suite):
        print(f"  [{case_suite}] {source_dir.resolve()}")
        cmd = [sys.executable, reference_script,
               "--xlsx-dir", str(source_dir.resolve()),
               "--pdf-dir", str(REFERENCE_PDF_DIR.resolve())]
        if filter_pattern:
            cmd += ["--filter", filter_pattern]
        if force:
            cmd += ["--force"]
        result = max(result, run(cmd, cwd=str(SCRIPT_DIR), check=False))
    return result


def step_generate_office_pdfs(suite: str, filter_pattern: str = None):
    """Step 3b: Convert Excel files to PDF using Office (Excel COM)."""
    banner("Step 3b: Convert Excel -> PDF (Office / Excel COM)")
    result = 0
    for case_suite, source_dir in selected_source_dirs(suite):
        print(f"  [{case_suite}] {source_dir.resolve()}")
        cmd = [sys.executable, "generate_office_pdfs.py",
               "--xlsx-dir", str(source_dir.resolve()),
               "--pdf-dir", str(OFFICE_PDF_DIR.resolve())]
        if filter_pattern:
            cmd += ["--filter", filter_pattern]
        result = max(result, run(cmd, cwd=str(SCRIPT_DIR), check=False))
    return result


def step_compare(ai_compare: bool = False, ai_max_pages: int = 1, ai_threshold: float = 0.90,
                 use_office: bool = False, filter_pattern: str = None, manifest: str = None,
                 report_scope: str = "shared", composite_images: bool = False,
                 candidate_label: str = "MiniPdf", reference_label: str = "Reference",
                 office_label: str = "Office", heatmaps: bool = False,
                 heatmap_threshold: int = 12, heatmap_gain: float = 5.0,
                 max_compare_pages: int = 15):
    """Step 4: Compare MiniPdf PDFs against reference PDFs."""
    banner("Step 4: Compare MiniPdf vs Reference")
    cmd = [
        sys.executable, "compare_pdfs.py",
        "--minipdf-dir", str(MINIPDF_PDF_DIR.resolve()),
        "--reference-dir", str(REFERENCE_PDF_DIR.resolve()),
        "--report-dir", str(REPORT_DIR.resolve()),
        "--max-pages", str(max_compare_pages),
    ]
    if use_office and OFFICE_PDF_DIR.is_dir():
        cmd += ["--office-dir", str(OFFICE_PDF_DIR.resolve())]
    if ai_compare:
        cmd += ["--ai-compare", "--ai-max-pages", str(ai_max_pages), "--ai-threshold", str(ai_threshold)]
    if filter_pattern:
        cmd += ["--filter", filter_pattern]
    if manifest:
        cmd += ["--manifest", str(Path(manifest).resolve()), "--report-scope", report_scope]
    if composite_images:
        cmd += [
            "--composite-images",
            "--candidate-label", candidate_label,
            "--reference-label", reference_label,
            "--office-label", office_label,
        ]
    if heatmaps:
        cmd += [
            "--heatmaps",
            "--heatmap-threshold", str(heatmap_threshold),
            "--heatmap-gain", str(heatmap_gain),
        ]
    return run(cmd, cwd=str(SCRIPT_DIR))


def step_analyze_report():
    """Step 5: Print key findings from the report."""
    banner("Step 5: Analysis Summary")
    json_path = REPORT_DIR / "comparison_report.json"
    md_path = REPORT_DIR / "comparison_report.md"

    if json_path.exists():
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
            print(f"  [!] Cases needing improvement:")
            for r in sorted(results, key=lambda x: x.get("overall_score", 0)):
                score = r.get("overall_score", 0)
                if score < 0.7:
                    print(f"    - {r['name']}: {score}")
            print()

        print(f"  Full report: {md_path}")
        print(f"  JSON data:   {json_path}")
    else:
        print("  No report found. Run the full pipeline first.")


def main():
    parser = argparse.ArgumentParser(description="MiniPdf Benchmark Pipeline")
    parser.add_argument("--suite", choices=["all", "classic", "issue"], default="all",
                        help="Fixture suite to run (default: all classic and issue XLSX files)")
    parser.add_argument("--filter", default=None, metavar="PATTERN",
                        help="Only process files matching this substring (e.g. 'border' or 'chart_bar')")
    parser.add_argument("--skip-generate", action="store_true", help="Skip Excel generation")
    parser.add_argument("--skip-minipdf", action="store_true", help="Skip MiniPdf PDF conversion")
    parser.add_argument("--skip-reference", action="store_true", help="Skip reference conversion")
    parser.add_argument("--force-reference", action="store_true",
                        help="Overwrite existing reference PDFs instead of reusing them")
    parser.add_argument("--engine", choices=["libre", "office", "o365"], default="o365",
                        help="Reference engine: o365 (Microsoft 365 COM, default) or libre (LibreOffice); office is an alias for o365")
    parser.add_argument("--with-office", action="store_true",
                        help="Also convert via Office (Excel COM) and include in comparison")
    parser.add_argument("--skip-office", action="store_true", help="Skip Office conversion (when --with-office)")
    parser.add_argument("--compare-only", action="store_true", help="Only run comparison step")
    # AI comparison options (forwarded to compare_pdfs.py)
    parser.add_argument("--ai-compare", action="store_true",
                        help="Enable AI visual comparison (requires openai package + API key)")
    parser.add_argument("--ai-max-pages", type=int, default=1, metavar="N",
                        help="Max pages per PDF to send to AI (default: 1)")
    parser.add_argument("--ai-threshold", type=float, default=0.97, metavar="T",
                        help="Skip AI call when pixel score >= threshold (default: 0.97)")
    parser.add_argument("--source-dir", default=None, metavar="DIR",
                        help="Classic XLSX source directory (default: tests/MiniPdf.Scripts/output)")
    parser.add_argument("--issue-source-dir", default=None, metavar="DIR",
                        help="Issue XLSX source directory (default: tests/Issue_Files/xlsx)")
    parser.add_argument("--minipdf-dir", default=None, metavar="DIR",
                        help="MiniPdf PDF output directory override")
    parser.add_argument("--reference-dir", default=None, metavar="DIR",
                        help="Reference PDF output directory override")
    parser.add_argument("--office-dir", default=None, metavar="DIR",
                        help="Office PDF output directory override")
    parser.add_argument("--report-dir", default=None, metavar="DIR",
                        help="Report output directory override")
    parser.add_argument("--manifest", default=None, metavar="JSON",
                        help="Benchmark manifest forwarded to compare_pdfs.py")
    parser.add_argument("--report-scope", default="shared", metavar="NAME",
                        help="Report scope metadata forwarded to compare_pdfs.py")
    parser.add_argument("--composite-images", action="store_true",
                        help="Generate labeled side-by-side comparison images")
    parser.add_argument("--heatmaps", action="store_true",
                        help="Generate contextual per-page difference heatmaps")
    parser.add_argument("--heatmap-threshold", type=int, default=12, metavar="N",
                        help="Heatmap difference threshold (default: 12)")
    parser.add_argument("--heatmap-gain", type=float, default=5.0, metavar="G",
                        help="Heatmap difference amplification (default: 5.0)")
    parser.add_argument("--max-pages", type=int, default=15, metavar="N",
                        help="Compare at most N pages per PDF (default: 15); 0 compares all pages")
    parser.add_argument("--candidate-label", default="MiniPdf",
                        help="Candidate renderer label for composite images")
    parser.add_argument("--reference-label", default=None,
                        help="Reference renderer label for composite images")
    parser.add_argument("--office-label", default="Office",
                        help="Office renderer label for composite images")
    args = parser.parse_args()

    configure_paths(args)

    banner("MiniPdf Self-Evolution Benchmark Pipeline")
    print(f"  Suite:         {args.suite}")
    for case_suite, source_dir in selected_source_dirs(args.suite):
        print(f"  {case_suite.title()} XLSX: {source_dir.resolve()}")
    print(f"  MiniPdf PDFs:  {MINIPDF_PDF_DIR.resolve()}")
    print(f"  Reference PDFs:{REFERENCE_PDF_DIR.resolve()}")
    print(f"  Ref engine:    {args.engine}")
    if args.with_office:
        print(f"  Office PDFs:   {OFFICE_PDF_DIR.resolve()}")
    print(f"  Reports:       {REPORT_DIR.resolve()}")

    reference_label = args.reference_label or ("Microsoft 365 Excel Reference" if args.engine in {"o365", "office"} else "LibreOffice Reference")
    ai_kwargs = dict(ai_compare=args.ai_compare, ai_max_pages=args.ai_max_pages, ai_threshold=args.ai_threshold)
    compare_kwargs = dict(**ai_kwargs, use_office=args.with_office)
    compare_kwargs.update(
        manifest=args.manifest or str(write_comparison_manifest(args.suite)),
        report_scope=args.report_scope if args.report_scope != "shared" else f"xlsx-{args.suite}",
        composite_images=args.composite_images,
        candidate_label=args.candidate_label,
        reference_label=reference_label,
        office_label=args.office_label,
        heatmaps=args.heatmaps,
        heatmap_threshold=args.heatmap_threshold,
        heatmap_gain=args.heatmap_gain,
        max_compare_pages=args.max_pages,
    )
    filt = args.filter

    if args.compare_only:
        step_compare(**compare_kwargs, filter_pattern=filt)
        step_analyze_report()
        return

    if args.suite in {"all", "classic"} and not args.skip_generate and not filt:
        step_generate_xlsx()

    if not args.skip_minipdf:
        step_generate_minipdf_pdfs(args.suite, filter_pattern=filt)

    if not args.skip_reference:
        step_generate_reference_pdfs(args.suite, filter_pattern=filt, engine=args.engine, force=args.force_reference)

    if args.with_office and not args.skip_office:
        step_generate_office_pdfs(args.suite, filter_pattern=filt)

    step_compare(**compare_kwargs, filter_pattern=filt)
    step_analyze_report()

    banner("Pipeline Complete")


if __name__ == "__main__":
    main()
