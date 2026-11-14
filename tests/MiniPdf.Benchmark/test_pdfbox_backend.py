import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from zipfile import ZipFile

from pdfbox_backend import EVALUATOR, JAR, PdfDocument, check_backend, extract_text
from compare_pdfs import compare_single, generate_report, render_page_to_pixels


def write_fixture(path: Path, page_count: int = 3) -> None:
    content = (
        b"BT /F1 12 Tf 20 260 Td (Left) Tj ET\n"
        b"BT /F1 12 Tf 110 260.5 Td (Right) Tj ET\n"
        b"BT /F1 12 Tf 20 230 Td (Caf\\351) Tj ET\n"
    )
    objects = [
        b"<< /Type /Catalog /Pages 2 0 R >>",
        (b"<< /Type /Pages /Kids ["
         + b" ".join(f"{page + 3} 0 R".encode("ascii") for page in range(page_count))
         + f"] /Count {page_count} >>".encode("ascii")),
        b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 300] "
        b"/Resources << /Font << /F1 6 0 R >> >> /Contents 7 0 R >>",
        b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 300] "
        b"/CropBox [10 20 110 220] /Rotate 90 /Resources << >> /Contents 8 0 R >>",
        b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 300] "
        b"/Resources << >> /Contents 8 0 R >>",
        b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
        b"<< /Length " + str(len(content)).encode("ascii") + b" >>\nstream\n" + content + b"\nendstream",
        b"<< /Length 0 >>\nstream\n\nendstream",
    ]
    data = bytearray(b"%PDF-1.4\n")
    offsets = [0]
    for number, obj in enumerate(objects, 1):
        offsets.append(len(data))
        data.extend(f"{number} 0 obj\n".encode("ascii") + obj + b"\nendobj\n")
    xref = len(data)
    data.extend(f"xref\n0 {len(objects) + 1}\n0000000000 65535 f \n".encode("ascii"))
    for offset in offsets[1:]:
        data.extend(f"{offset:010d} 00000 n \n".encode("ascii"))
    data.extend(
        f"trailer\n<< /Size {len(objects) + 1} /Root 1 0 R >>\n"
        f"startxref\n{xref}\n%%EOF\n".encode("ascii")
    )
    path.write_bytes(data)


class PdfBoxBackendTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        check_backend()

    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        self.pdf = self.directory / "fixture \u6d4b\u8bd5.pdf"
        write_fixture(self.pdf)

    def test_text_rows_unicode_and_blank_pages(self):
        self.assertEqual(extract_text(str(self.pdf)), ["Left Right\nCaf\u00e9", "", ""])

    def test_rgb_dimensions_rotation_crop_and_dpi(self):
        with PdfDocument(str(self.pdf)) as document:
            self.assertEqual(document.page_count, 3)
            pix = document.render(0, 72)
            self.assertEqual((pix.width, pix.height, pix.n), (200, 300, 3))
            self.assertEqual(len(pix.samples), 200 * 300 * 3)
            self.assertEqual(pix.samples[:3], b"\xff\xff\xff")
            self.assertLess(min(pix.samples), 255)
            rotated = document.render(1, 72)
            self.assertEqual((rotated.width, rotated.height), (200, 100))
            scaled = document.render(2, 144)
            self.assertEqual((scaled.width, scaled.height), (400, 600))
            self.assertEqual(set(scaled.samples), {255})
            saved = self.directory / "page.png"
            pix.save(str(saved))
            from PIL import Image
            with Image.open(saved) as image:
                self.assertEqual((image.mode, image.size), ("RGB", (200, 300)))
        self.assertIsNotNone(document._process.poll())

    def test_page_and_dpi_validation(self):
        with PdfDocument(str(self.pdf)) as document:
            for index in (-1, 3):
                with self.assertRaises(IndexError):
                    document.render(index)
            for dpi in (0, -72, 1.5):
                with self.assertRaises(ValueError):
                    document.render(0, dpi)
        self.assertIsNone(render_page_to_pixels(str(self.pdf), 3))

    def test_invalid_pdf_is_an_error_not_an_empty_document(self):
        invalid = self.directory / "invalid.pdf"
        invalid.write_text("not a PDF", encoding="ascii")
        with self.assertRaisesRegex(RuntimeError, "PDFBox"):
            PdfDocument(str(invalid))
        with self.assertRaisesRegex(RuntimeError, "PDFBox"):
            compare_single(str(invalid), str(self.pdf), str(self.directory), "invalid")

    def test_missing_pdf_is_an_explicit_case_error(self):
        result = compare_single(str(self.pdf) + ".missing", str(self.pdf), str(self.directory), "missing")
        self.assertEqual(result["error"], "MiniPdf PDF not found")
        self.assertEqual(result["evaluator"], EVALUATOR)

    def test_identical_pdf_scores_and_all_report_images(self):
        images = self.directory / "images"
        composites = self.directory / "side-by-side"
        result = compare_single(
            str(self.pdf), str(self.pdf), str(images), "identical",
            auxiliary_path=str(self.pdf), composite_images=True,
            composite_output_dir=str(composites), heatmaps=True,
        )
        self.assertTrue(result["pdf_valid"])
        self.assertEqual(result["minipdf_pages"], 3)
        self.assertEqual(result["reference_pages"], 3)
        self.assertEqual(result["visual_scores"], [1.0, 1.0, 1.0])
        self.assertEqual(result["text_similarity"], 1.0)
        self.assertEqual(result["overall_score"], 1.0)
        self.assertEqual(result["evaluator"], EVALUATOR)
        self.assertEqual(len(result["diff_images"]), 3)
        for page in result["diff_images"]:
            for key in ("minipdf_img", "reference_img", "auxiliary_img", "heatmap_img"):
                self.assertTrue((images / page[key]).is_file(), key)
            self.assertTrue((composites / page["composite_img"]).is_file())
        generate_report([result], str(self.directory))
        stored = json.loads((self.directory / "comparison_report.json").read_text(encoding="utf-8"))
        self.assertEqual(stored[0]["evaluator"], EVALUATOR)
        self.assertIn("Apache PDFBox", (self.directory / "comparison_report.md").read_text(encoding="utf-8"))

    def test_cli_rejects_mixing_legacy_and_pdfbox_scores(self):
        report = self.directory / "report"
        report.mkdir()
        (report / "comparison_report.json").write_text(
            json.dumps([{"name": self.pdf.stem, "overall_score": 1.0}]), encoding="utf-8"
        )
        process = subprocess.run(
            [sys.executable, str(Path(__file__).with_name("compare_pdfs.py")),
             "--minipdf-dir", str(self.directory), "--reference-dir", str(self.directory),
             "--report-dir", str(report), "--filter", "fixture"],
            capture_output=True, text=True, encoding="utf-8",
            env={**os.environ, "PYTHONIOENCODING": "utf-8"},
        )
        self.assertNotEqual(process.returncode, 0)
        self.assertIn("Cannot merge filtered results from different evaluators", process.stderr)

    def test_page_mismatch_keeps_penalty_and_missing_page_images(self):
        shorter = self.directory / "short.pdf"
        write_fixture(shorter, page_count=1)
        images = self.directory / "mismatch"
        result = compare_single(str(self.pdf), str(shorter), str(images), "mismatch")
        self.assertEqual((result["minipdf_pages"], result["reference_pages"]), (3, 1))
        self.assertEqual(result["visual_scores"][1:], [0.0, 0.0])
        self.assertEqual(len(result["diff_images"]), 3)
        self.assertIsNone(result["diff_images"][1]["reference_img"])
        expected = round(
            result["text_similarity"] * 0.4 + result["visual_avg"] * 0.4 + 0.5 * 0.2, 4
        )
        self.assertEqual(result["overall_score"], expected)

    def test_report_only_does_not_relabel_legacy_results(self):
        legacy = {
            "name": "legacy", "minipdf_pages": 1, "reference_pages": 1,
            "overall_score": 1.0, "text_similarity": 1.0, "visual_avg": 1.0,
        }
        generate_report([legacy], str(self.directory))
        stored = json.loads((self.directory / "comparison_report.json").read_text(encoding="utf-8"))
        self.assertNotIn("evaluator", stored[0])
        markdown = (self.directory / "comparison_report.md").read_text(encoding="utf-8")
        self.assertIn("unversioned", markdown)
        self.assertNotIn("Apache PDFBox", markdown)

    def test_missing_runtime_and_helper_are_actionable_errors(self):
        with patch("pdfbox_backend.shutil.which", return_value=None):
            with self.assertRaisesRegex(RuntimeError, "Java 11"):
                check_backend()
        with patch("pdfbox_backend.JAR", self.directory / "missing.jar"):
            with self.assertRaisesRegex(RuntimeError, "mvn"):
                check_backend()

    def test_helper_preserves_dependency_licenses_and_notices(self):
        with ZipFile(JAR) as jar:
            license_text = jar.read("META-INF/LICENSE").decode("utf-8")
            notices = jar.read("META-INF/NOTICE").decode("utf-8")
            self.assertIn("Apache License", license_text)
            self.assertIn("SIL OPEN FONT LICENSE", license_text)
            self.assertIn("Apache PDFBox", notices)
            self.assertIn("Apache FontBox", notices)
            self.assertIn("Apache Commons Logging", notices)


if __name__ == "__main__":
    unittest.main()
