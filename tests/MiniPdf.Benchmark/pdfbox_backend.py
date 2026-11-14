"""Apache PDFBox transport for the shared visual benchmark (Java 11+)."""

import base64
import shutil
import struct
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path
from types import TracebackType
from typing import BinaryIO, Optional


EVALUATOR = {
    "name": "Apache PDFBox",
    "version": "3.0.8",
    "text_normalization": "baseline-rows-v1",
    "scoring": "text40-visual40-pages20-v1",
    "dpi": 150,
}
JAR = Path(__file__).parent / "pdfbox" / "target" / "benchmark-pdfbox.jar"


def check_backend() -> str:
    java = shutil.which("java")
    if java is None:
        raise RuntimeError("The PDFBox benchmark requires Java 11+ on PATH.")
    if not JAR.is_file():
        raise RuntimeError(
            "PDFBox benchmark helper is not built. Run: "
            f'mvn -f "{JAR.parent.parent / "pom.xml"}" package'
        )
    return java


@dataclass(frozen=True)
class Pixmap:
    width: int
    height: int
    samples: bytes
    n: int = 3

    def save(self, path: str) -> None:
        from PIL import Image

        Image.frombytes("RGB", (self.width, self.height), self.samples).save(path)


def _read_exact(stream: BinaryIO, size: int) -> bytes:
    chunks = bytearray()
    while len(chunks) < size:
        chunk = stream.read(size - len(chunks))
        if not chunk:
            raise RuntimeError("PDFBox helper terminated before completing its response.")
        chunks.extend(chunk)
    return bytes(chunks)


class PdfDocument:
    def __init__(self, path: str):
        java = check_backend()
        self._log = tempfile.TemporaryFile()
        encoded_path = base64.b64encode(str(Path(path).resolve()).encode("utf-8")).decode("ascii")
        try:
            self._process = subprocess.Popen(
                [java, "-Djava.awt.headless=true", "-jar", JAR.name, encoded_path],
                cwd=JAR.parent,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=self._log,
            )
        except OSError:
            self._log.close()
            raise
        try:
            self.page_count = int(self._response().decode("ascii"))
        except (RuntimeError, ValueError):
            self.close()
            raise

    def _response(self) -> bytes:
        assert self._process.stdout is not None
        try:
            status, length = struct.unpack(">BI", _read_exact(self._process.stdout, 5))
            payload = _read_exact(self._process.stdout, length)
        except RuntimeError as error:
            self._log.seek(0)
            details = self._log.read().decode("utf-8", errors="replace")
            raise RuntimeError(f"{error}\n{details}") from error
        if status != 0:
            raise RuntimeError(f"PDFBox: {payload.decode('utf-8', errors='replace')}")
        return payload

    def _request(self, command: str, page: int) -> bytes:
        if not 0 <= page < self.page_count:
            raise IndexError(f"PDF page index out of range: {page}")
        assert self._process.stdin is not None
        self._process.stdin.write((command + "\n").encode("ascii"))
        self._process.stdin.flush()
        return self._response()

    def text_spans(self, page: int) -> list[tuple[float, float, str]]:
        payload = self._request(f"text {page}", page)
        count = struct.unpack_from(">I", payload)[0]
        offset = 4
        spans = []
        for _ in range(count):
            y, x, length = struct.unpack_from(">ffI", payload, offset)
            offset += 12
            text = payload[offset:offset + length].decode("utf-8").strip()
            offset += length
            if text:
                spans.append((round(y, 1), x, text))
        return spans

    def render(self, page: int, dpi: int = 150) -> Pixmap:
        if not isinstance(dpi, int) or dpi <= 0:
            raise ValueError("DPI must be a positive integer.")
        payload = self._request(f"render {page} {dpi}", page)
        width, height = struct.unpack_from(">II", payload)
        samples = payload[8:]
        if len(samples) != width * height * 3:
            raise RuntimeError("PDFBox returned an invalid RGB image payload.")
        return Pixmap(width, height, samples)

    def close(self) -> None:
        process = self._process
        try:
            if process.stdin is not None:
                process.stdin.close()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()
        finally:
            if process.stdout is not None:
                process.stdout.close()
            self._log.close()

    def __enter__(self) -> "PdfDocument":
        return self

    def __exit__(
        self,
        exc_type: Optional[type[BaseException]],
        exc_value: Optional[BaseException],
        traceback: Optional[TracebackType],
    ) -> None:
        self.close()


def extract_text(pdf_path: str) -> list[str]:
    pages = []
    with PdfDocument(pdf_path) as document:
        for page in range(document.page_count):
            spans = sorted(document.text_spans(page))
            lines = []
            current_y = None
            tokens: list[tuple[float, str]] = []
            for y, x, text in spans:
                if current_y is None or abs(y - current_y) > 1.0:
                    if tokens:
                        lines.append(" ".join(text for _, text in sorted(tokens)))
                    current_y = y
                    tokens = [(x, text)]
                else:
                    tokens.append((x, text))
            if tokens:
                lines.append(" ".join(text for _, text in sorted(tokens)))
            pages.append("\n".join(lines))
    return pages
