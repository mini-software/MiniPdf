from __future__ import annotations

import argparse
import sys
from collections.abc import Sequence
from pathlib import Path

from .api import convert_to_pdf
from .errors import MiniPdfError
from .options import ConversionOptions, PageSize


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="minipdf", description="Convert Office files to PDF.")
    parser.add_argument("input", type=Path)
    parser.add_argument("-o", "--output", type=Path)
    parser.add_argument("--paper-size", choices=("a4", "letter"))
    parser.add_argument("--page-width", type=float)
    parser.add_argument("--page-height", type=float)
    return parser


def _options(args: argparse.Namespace, parser: argparse.ArgumentParser) -> ConversionOptions:
    custom_supplied = args.page_width is not None or args.page_height is not None
    if args.paper_size and custom_supplied:
        parser.error("use either --paper-size or --page-width/--page-height, not both")
    if (args.page_width is None) != (args.page_height is None):
        parser.error("--page-width and --page-height must be specified together")
    if args.paper_size == "a4":
        return ConversionOptions(PageSize.A4)
    if args.paper_size == "letter":
        return ConversionOptions(PageSize.LETTER)
    if custom_supplied:
        return ConversionOptions(PageSize(args.page_width, args.page_height))
    return ConversionOptions()


def main(argv: Sequence[str] | None = None) -> int:
    arguments = list(sys.argv[1:] if argv is None else argv)
    if arguments[:1] == ["convert"]:
        arguments.pop(0)
    parser = _parser()
    args = parser.parse_args(arguments)
    output = args.output or args.input.with_suffix(".pdf")
    try:
        convert_to_pdf(args.input, output, _options(args, parser))
    except (MiniPdfError, OSError, ValueError) as error:
        parser.exit(1, f"Error: {error}\n")
    print(output)
    return 0
