"""
Compare MiniPdf-generated PDFs against LibreOffice reference PDFs.

Produces a detailed comparison report including:
  - Text content diff (extracted text comparison)
  - Page count comparison
  - File size comparison
  - Visual pixel diff (if pdf2image / Poppler is available)

Prerequisites:
    pip install pymupdf   # for text extraction + rendering

Usage:
    python compare_pdfs.py [--minipdf-dir ./minipdf_pdfs] [--reference-dir ./reference_pdfs] [--report-dir ./reports]
"""

import argparse
import base64
import difflib
import json
import os
import sys
from datetime import datetime
from pathlib import Path

# Try to import fitz (PyMuPDF) for text extraction and visual comparison
try:
    import fitz  # PyMuPDF
    HAS_FITZ = True
except ImportError:
    HAS_FITZ = False
    print("WARNING: PyMuPDF not installed. Install with: pip install pymupdf")
    print("         Text extraction and visual comparison will be disabled.\n")

# Try to import openai for AI-based visual comparison
try:
    import openai
    HAS_OPENAI = True
except ImportError:
    HAS_OPENAI = False


def _make_openai_client():
    """Create an OpenAI or Azure OpenAI client from environment variables.

    Supports three endpoint flavours:
      1. Azure AI Foundry / model-router  (endpoint contains /v1 path)
         → openai.OpenAI(base_url=..., api_key=...)
      2. Classic Azure OpenAI             (AZURE_OPENAI_ENDPOINT = https://resource.openai.azure.com)
         → openai.AzureOpenAI(azure_endpoint=..., api_key=...)
      3. OpenAI                           (OPENAI_API_KEY)
         → openai.OpenAI(api_key=...)

    Precedence: Azure (AZURE_OPENAI_ENDPOINT) > OpenAI (OPENAI_API_KEY).
    Returns (client, model_or_deployment) or (None, None) when no credentials found.
    """
    if not HAS_OPENAI:
        return None, None

    import re
    from urllib.parse import urlparse

    endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT", "").strip().rstrip("/")
    azure_key = os.environ.get("AZURE_OPENAI_KEY", "").strip()

    if endpoint and azure_key:
        # ── AI Foundry / model-router style: endpoint contains a path (e.g. /openai/v1) ──
        parsed = urlparse(endpoint)
        if parsed.path and parsed.path not in ("", "/"):
            # Strip down to the deepest meaningful prefix (strip /chat/completions etc.)
            base = re.sub(r"/chat/completions.*$", "", endpoint)
            # Ensure it ends at /v1 or similar base path
            deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT", "gpt-4.1").strip()
            client = openai.OpenAI(api_key=azure_key, base_url=base)
            return client, deployment
        else:
            # ── Classic Azure OpenAI: only scheme+host, SDK appends /openai/deployments/... ──
            deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT", "gpt-4o").strip()
            client = openai.AzureOpenAI(
                azure_endpoint=endpoint,
                api_key=azure_key,
                api_version="2025-01-01-preview",
            )
            return client, deployment

    oai_key = os.environ.get("OPENAI_API_KEY", "").strip()
    if oai_key:
        model = os.environ.get("OPENAI_MODEL", "gpt-4o").strip()
        client = openai.OpenAI(api_key=oai_key)
        return client, model

    return None, None


AI_CLIENT, AI_MODEL = _make_openai_client()

_AI_SYSTEM_PROMPT = """\
You are an expert PDF rendering quality analyst.
You will be given two images of the same spreadsheet page:
  Image 1 (left/first): rendered by MiniPdf (the candidate)
  Image 2 (right/second): rendered by LibreOffice (the gold reference)

Analyse the visual differences and return a JSON object with exactly these fields:
{
  "differences": [   // list of specific observed differences (strings)
  ],
  "severity": "low|medium|high",  // overall severity
  "ai_visual_score": 0.0,          // float 0.0-1.0 (1.0 = visually identical)
  "suggestions": [   // actionable code improvement suggestions (strings)
  ]
}

Focus on:
- Cell border rendering (missing, extra, wrong weight/color)
- Column widths and row heights
- Font family, size, and weight
- Text truncation or overflow
- Background / fill colors
- Number/date formatting
- Header row styling
- Page margins and overall layout

Return ONLY valid JSON. No markdown fences, no extra text.
"""


def ai_compare_pages(img_minipdf: str, img_reference: str) -> dict:
    """Send two page images to the vision model and return structured AI analysis.

    Returns a dict with keys: differences, severity, ai_visual_score, suggestions.
    On error returns a dict with an 'error' key.
    """
    if AI_CLIENT is None:
        return {"error": "No OpenAI credentials configured"}

    def _encode(path: str) -> str:
        with open(path, "rb") as f:
            return base64.b64encode(f.read()).decode()

    try:
        b64_mini = _encode(img_minipdf)
        b64_ref = _encode(img_reference)
    except OSError as e:
        return {"error": f"Cannot read image: {e}"}

    messages = [
        {"role": "system", "content": _AI_SYSTEM_PROMPT},
        {
            "role": "user",
            "content": [
                {"type": "text", "text": "Compare these two PDF page renderings and return JSON analysis."},
                {"type": "image_url", "image_url": {"url": f"data:image/png;base64,{b64_mini}", "detail": "high"}},
                {"type": "image_url", "image_url": {"url": f"data:image/png;base64,{b64_ref}", "detail": "high"}},
            ],
        },
    ]

    try:
        response = AI_CLIENT.chat.completions.create(
            model=AI_MODEL,
            messages=messages,
            max_tokens=1024,
            temperature=0,
        )
        raw = response.choices[0].message.content.strip()
        # Strip optional markdown fences
        if raw.startswith("```"):
            raw = raw.split("\n", 1)[-1].rsplit("```", 1)[0].strip()
        return json.loads(raw)
    except json.JSONDecodeError as e:
        return {"error": f"AI returned non-JSON: {e}", "raw": raw}
    except Exception as e:
        return {"error": str(e)}


def extract_text_pymupdf(pdf_path: str) -> list[str]:
    """Extract text from each page using PyMuPDF.
    Groups spans at the same Y position (same visual row) into a single
    space-joined line so that MiniPdf and LibreOffice outputs are comparable
    regardless of whether each cell is a separate span or part of a row span."""
    pages = []
    doc = fitz.open(pdf_path)
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    text = span.get("text", "").strip()
                    if text:
                        spans.append((round(span["bbox"][1], 1), span["bbox"][0], text))
        # Sort by Y then X position
        spans.sort()
        # Group spans that share the same Y row (within 1.0 pt tolerance) into
        # a single whitespace-separated line — this handles the structural
        # difference between PDF renderers that emit one span per cell vs
        # one span per row.
        # Within each group we sort by X so that left-to-right column order is
        # preserved even when spans from different renderers land at slightly
        # different sub-pixel Y positions (< 1 pt apart).
        lines = []
        current_y = None
        current_tokens: list[tuple[float, str]] = []  # (x, text)
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if current_tokens:
                    current_tokens.sort()
                    lines.append(" ".join(t for _, t in current_tokens))
                current_y = y
                current_tokens = [(x, text)]
            else:
                current_tokens.append((x, text))
        if current_tokens:
            current_tokens.sort()
            lines.append(" ".join(t for _, t in current_tokens))
        pages.append("\n".join(lines))
    doc.close()
    return pages


def extract_text_fallback(pdf_path: str) -> list[str]:
    """Fallback: extract raw ASCII strings from PDF binary."""
    with open(pdf_path, "rb") as f:
        data = f.read()
    # Very rough extraction: find text between BT..ET operators
    text = data.decode("latin-1", errors="replace")
    # Extract parenthesized strings (PDF text objects)
    import re
    strings = re.findall(r"\(([^)]*)\)", text)
    return ["\n".join(strings)]


def render_page_to_pixels(pdf_path: str, page_num: int, dpi: int = 150):
    """Render a PDF page to a pixel map using PyMuPDF. Returns (width, height, samples)."""
    doc = fitz.open(pdf_path)
    if page_num >= len(doc):
        doc.close()
        return None
    page = doc[page_num]
    mat = fitz.Matrix(dpi / 72, dpi / 72)
    pix = page.get_pixmap(matrix=mat, alpha=False)
    result = (pix.width, pix.height, pix.samples)
    doc.close()
    return result


def pixel_diff_score(pix1, pix2) -> float:
    """
    Compare two pixmaps and return a similarity score 0.0-1.0.
    1.0 = identical, 0.0 = completely different.

    Includes a content-coverage penalty: when the reference has significant
    non-white content (images, colored fills) but MiniPdf renders far less of
    it, the white-background dominance would otherwise inflate the score.
    Rule: if MiniPdf has <50% of the reference's non-white byte count, apply
    a proportional penalty (scale = min(1.0, coverage * 2)).
    """
    if pix1 is None or pix2 is None:
        return 0.0

    w1, h1, s1 = pix1
    w2, h2, s2 = pix2

    if w1 != w2 or h1 != h2:
        # Different dimensions - compare what we can
        min_len = min(len(s1), len(s2))
        if min_len == 0:
            return 0.0
        matching = sum(1 for a, b in zip(s1[:min_len], s2[:min_len]) if a == b)
        return matching / min_len

    total = len(s1)
    if total == 0:
        return 1.0

    # Single-pass: byte matching + non-white counting (RGB: 3 bytes/pixel)
    matching = 0
    nonwhite_mini = 0
    nonwhite_ref = 0
    for a, b in zip(s1, s2):
        if a == b:
            matching += 1
        if a != 255:
            nonwhite_mini += 1
        if b != 255:
            nonwhite_ref += 1

    raw_score = matching / total

    # Content-coverage penalty: only when reference has substantial non-white content
    # Threshold: >0.5% of all bytes are non-white in the reference (≈0.17% pixels in RGB)
    if nonwhite_ref > total * 0.005:
        coverage = nonwhite_mini / nonwhite_ref
        if coverage < 0.5:
            # Scale [0..0.5 coverage] → [0..1.0 penalty_factor]
            scale = min(1.0, coverage * 2)
            return raw_score * scale

    return raw_score


def save_visual_diff(pdf1_path: str, pdf2_path: str, output_dir: str, name: str, dpi: int = 150):
    """Save visual diff images for each page."""
    if not HAS_FITZ:
        return []

    diff_images = []
    doc1 = fitz.open(pdf1_path)
    doc2 = fitz.open(pdf2_path)
    max_pages = max(len(doc1), len(doc2))

    for i in range(max_pages):
        mat = fitz.Matrix(dpi / 72, dpi / 72)

        if i < len(doc1):
            pix1 = doc1[i].get_pixmap(matrix=mat, alpha=False)
        else:
            pix1 = None

        if i < len(doc2):
            pix2 = doc2[i].get_pixmap(matrix=mat, alpha=False)
        else:
            pix2 = None

        # Save individual renderings
        if pix1:
            path1 = os.path.join(output_dir, f"{name}_p{i+1}_minipdf.png")
            pix1.save(path1)

        if pix2:
            path2 = os.path.join(output_dir, f"{name}_p{i+1}_reference.png")
            pix2.save(path2)

        diff_images.append({
            "page": i + 1,
            "minipdf_img": f"{name}_p{i+1}_minipdf.png" if pix1 else None,
            "reference_img": f"{name}_p{i+1}_reference.png" if pix2 else None,
        })

    doc1.close()
    doc2.close()
    return diff_images


def compare_single(minipdf_path: str, reference_path: str, report_images_dir: str, name: str,
                   ai_compare: bool = False, ai_max_pages: int = 1, ai_threshold: float = 1.01) -> dict:
    """Compare a single pair of PDFs and return a detailed result."""
    result = {
        "name": name,
        "minipdf_exists": os.path.isfile(minipdf_path),
        "reference_exists": os.path.isfile(reference_path),
    }

    if not result["minipdf_exists"]:
        result["error"] = "MiniPdf PDF not found"
        result["score"] = 0.0
        return result

    if not result["reference_exists"]:
        result["error"] = "Reference PDF not found"
        result["score"] = 0.0
        return result

    # File sizes
    result["minipdf_size"] = os.path.getsize(minipdf_path)
    result["reference_size"] = os.path.getsize(reference_path)

    # Page counts
    if HAS_FITZ:
        doc_m = fitz.open(minipdf_path)
        doc_r = fitz.open(reference_path)
        result["minipdf_pages"] = len(doc_m)
        result["reference_pages"] = len(doc_r)
        doc_m.close()
        doc_r.close()
    else:
        result["minipdf_pages"] = "?"
        result["reference_pages"] = "?"

    # Text extraction and comparison
    if HAS_FITZ:
        try:
            text_m = extract_text_pymupdf(minipdf_path)
            text_r = extract_text_pymupdf(reference_path)
        except Exception as e:
            text_m = extract_text_fallback(minipdf_path)
            text_r = extract_text_fallback(reference_path)
            result["text_extract_warning"] = str(e)
    else:
        text_m = extract_text_fallback(minipdf_path)
        text_r = extract_text_fallback(reference_path)

    # Flatten text for comparison
    flat_m = "\n---PAGE---\n".join(text_m).strip()
    flat_r = "\n---PAGE---\n".join(text_r).strip()

    # Text similarity (SequenceMatcher) — page-aware
    if len(flat_m) == 0 and len(flat_r) == 0:
        # Both empty — treat as identical
        result["text_similarity"] = 1.0
    else:
        sm = difflib.SequenceMatcher(None, flat_m, flat_r)
        result["text_similarity"] = round(sm.ratio(), 4)

    # Also compute flat text similarity (ignoring page breaks)
    # This is fairer when page break positions differ but content is the same
    flat_m_no_page = flat_m.replace("\n---PAGE---\n", "\n")
    flat_r_no_page = flat_r.replace("\n---PAGE---\n", "\n")
    if len(flat_m_no_page) == 0 and len(flat_r_no_page) == 0:
        result["flat_text_similarity"] = 1.0
    else:
        sm_flat = difflib.SequenceMatcher(None, flat_m_no_page, flat_r_no_page)
        result["flat_text_similarity"] = round(sm_flat.ratio(), 4)

    # Word-level similarity: compare word-token sequences — far more robust than
    # character-level matching when the only differences are truncation or minor
    # word-boundary shifts (e.g. "Grilled Salmon" vs "Grilled S")
    words_m = flat_m_no_page.split()
    words_r = flat_r_no_page.split()
    if len(words_m) == 0 and len(words_r) == 0:
        result["word_text_similarity"] = 1.0
    else:
        sm_words = difflib.SequenceMatcher(None, words_m, words_r)
        result["word_text_similarity"] = round(sm_words.ratio(), 4)

    # Use the highest of all similarity metrics
    result["text_similarity"] = max(
        result["text_similarity"],
        result["flat_text_similarity"],
        result["word_text_similarity"],
    )

    # Unified diff
    diff_lines = list(difflib.unified_diff(
        flat_m.splitlines(keepends=True),
        flat_r.splitlines(keepends=True),
        fromfile=f"minipdf/{name}.pdf",
        tofile=f"reference/{name}.pdf",
        lineterm="",
    ))
    result["text_diff"] = "\n".join(diff_lines) if diff_lines else "(identical)"

    # Visual comparison
    visual_scores = []
    if HAS_FITZ:
        max_pages = max(result["minipdf_pages"], result["reference_pages"])
        for p in range(max_pages):
            pix_m = render_page_to_pixels(minipdf_path, p)
            pix_r = render_page_to_pixels(reference_path, p)
            score = pixel_diff_score(pix_m, pix_r)
            visual_scores.append(round(score, 4))

        result["visual_scores"] = visual_scores
        result["visual_avg"] = round(sum(visual_scores) / len(visual_scores), 4) if visual_scores else 0.0

        # Save diff images
        os.makedirs(report_images_dir, exist_ok=True)
        result["diff_images"] = save_visual_diff(minipdf_path, reference_path, report_images_dir, name)

        # ── AI visual comparison ──────────────────────────────────────────────
        if ai_compare and AI_CLIENT is not None:
            ai_results = []
            pages_to_analyse = min(ai_max_pages, max(result["minipdf_pages"], result["reference_pages"]))
            for p_idx in range(pages_to_analyse):
                pix_score = visual_scores[p_idx] if p_idx < len(visual_scores) else 1.0
                if pix_score >= ai_threshold:
                    # Pixel score is already good — skip AI call to save cost
                    ai_results.append({"page": p_idx + 1, "skipped": True, "reason": "pixel_score_above_threshold"})
                    continue
                img_mini = os.path.join(report_images_dir, f"{name}_p{p_idx+1}_minipdf.png")
                img_ref = os.path.join(report_images_dir, f"{name}_p{p_idx+1}_reference.png")
                if not os.path.isfile(img_mini) or not os.path.isfile(img_ref):
                    ai_results.append({"page": p_idx + 1, "error": "image not found"})
                    continue
                print(f"    [AI] analysing page {p_idx + 1} of {name} ...", end=" ", flush=True)
                analysis = ai_compare_pages(img_mini, img_ref)
                analysis["page"] = p_idx + 1
                ai_results.append(analysis)
                if "error" not in analysis:
                    print(f"severity={analysis.get('severity','?')} score={analysis.get('ai_visual_score','?')}")
                else:
                    print(f"error: {analysis['error']}")
            result["ai_analysis"] = ai_results

            # Override visual avg with AI score when available
            ai_scores = [
                a.get("ai_visual_score")
                for a in ai_results
                if isinstance(a.get("ai_visual_score"), (int, float))
            ]
            if ai_scores:
                result["ai_visual_avg"] = round(sum(ai_scores) / len(ai_scores), 4)
        elif ai_compare and AI_CLIENT is None:
            result["ai_analysis_warning"] = (
                "AI comparison requested but no credentials found. "
                "Set OPENAI_API_KEY or AZURE_OPENAI_ENDPOINT + AZURE_OPENAI_KEY."
            )

    # Overall score: weighted average
    # When AI scores are available they replace the noisy pixel score for visual dimension
    page_score = 1.0 if result.get("minipdf_pages") == result.get("reference_pages") else 0.5
    text_score = result["text_similarity"]
    vis_score = result.get("ai_visual_avg",          # prefer AI score
                 result.get("visual_avg",             # fall back to pixel
                            text_score))               # last resort: text

    result["overall_score"] = round(text_score * 0.4 + vis_score * 0.4 + page_score * 0.2, 4)

    return result


def generate_report(results: list[dict], report_dir: str):
    """Generate a markdown + JSON comparison report."""
    # JSON dump
    json_path = os.path.join(report_dir, "comparison_report.json")
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2, ensure_ascii=False, default=str)

    # Markdown report
    md_path = os.path.join(report_dir, "comparison_report.md")
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("# MiniPdf vs Reference PDF Comparison Report\n\n")
        f.write(f"Generated: {datetime.now().isoformat()}\n\n")

        # Summary table
        f.write("## Summary\n\n")
        f.write("| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |\n")
        f.write("|---|-----------|----------|------------|-------------|--------|\n")

        for i, r in enumerate(results, 1):
            name = r["name"]
            text_sim = r.get("text_similarity", "N/A")
            vis_avg = r.get("visual_avg", "N/A")
            mp = r.get("minipdf_pages", "?")
            rp = r.get("reference_pages", "?")
            overall = r.get("overall_score", "N/A")

            # Color coding via emoji
            if isinstance(overall, (int, float)):
                if overall >= 0.9:
                    emoji = "🟢"
                elif overall >= 0.7:
                    emoji = "🟡"
                else:
                    emoji = "🔴"
            else:
                emoji = "⚪"

            f.write(f"| {i} | {emoji} {name} | {text_sim} | {vis_avg} | {mp}/{rp} | **{overall}** |\n")

        avg_overall = sum(r.get("overall_score", 0) for r in results) / len(results) if results else 0
        f.write(f"\n**Average Overall Score: {avg_overall:.4f}**\n\n")

        # ── Visual side-by-side table ─────────────────────────────────────────
        f.write("## Visual Comparison\n\n")
        f.write("<table>\n")
        f.write("  <thead>\n")
        f.write("    <tr>\n")
        f.write("      <th>Test Case</th>\n")
        f.write("      <th>MiniPdf</th>\n")
        f.write("      <th>LibreOffice (Reference)</th>\n")
        f.write("      <th>Score</th>\n")
        f.write("    </tr>\n")
        f.write("  </thead>\n")
        f.write("  <tbody>\n")

        for r in results:
            name = r["name"]
            overall = r.get("overall_score", "N/A")
            if isinstance(overall, float):
                if overall >= 0.9:
                    score_cell = f'<span style="color:#3fb950">⬤</span> {overall}'
                elif overall >= 0.7:
                    score_cell = f'<span style="color:#d29922">⬤</span> {overall}'
                else:
                    score_cell = f'<span style="color:#f85149">⬤</span> {overall}'
            else:
                score_cell = str(overall)

            diff_images = r.get("diff_images", [])
            if not diff_images:
                # No images — single merged row
                f.write("    <tr>\n")
                f.write(f"      <td><b>{name}</b></td>\n")
                f.write("      <td colspan=\"2\"><i>No images</i></td>\n")
                f.write(f"      <td>{score_cell}</td>\n")
                f.write("    </tr>\n")
                continue

            for idx, pg in enumerate(diff_images):
                page_num = pg.get("page", idx + 1)
                mini_img = pg.get("minipdf_img")
                ref_img  = pg.get("reference_img")
                mini_td = (f'<img src="images/{mini_img}" width="340" alt="MiniPdf p{page_num}">'
                           if mini_img else "<i>missing</i>")
                ref_td  = (f'<img src="images/{ref_img}" width="340" alt="Reference p{page_num}">'
                           if ref_img else "<i>missing</i>")

                if idx == 0:
                    rowspan = len(diff_images)
                    label_td = (f'      <td rowspan="{rowspan}" valign="top"><b>{name}</b>'
                                f'<br><small>p{page_num}</small></td>\n'
                                if rowspan > 1 else
                                f'      <td valign="top"><b>{name}</b></td>\n')
                    score_td = (f'      <td rowspan="{rowspan}" valign="top">{score_cell}</td>\n'
                                if rowspan > 1 else
                                f'      <td valign="top">{score_cell}</td>\n')
                    f.write("    <tr>\n")
                    f.write(label_td)
                    f.write(f"      <td>{mini_td}</td>\n")
                    f.write(f"      <td>{ref_td}</td>\n")
                    f.write(score_td)
                    f.write("    </tr>\n")
                else:
                    f.write("    <tr>\n")
                    f.write(f"      <td align=\"center\"><small>p{page_num}</small></td>\n")
                    f.write(f"      <td>{mini_td}</td>\n")
                    f.write(f"      <td>{ref_td}</td>\n")
                    f.write("    </tr>\n")

        f.write("  </tbody>\n")
        f.write("</table>\n\n")

        # ── Detailed sections ──────────────────────────────────────────────────
        f.write("## Detailed Results\n\n")
        for r in results:
            name = r["name"]
            f.write(f"### {name}\n\n")

            if "error" in r:
                f.write(f"**Error:** {r['error']}\n\n")
                continue

            f.write(f"- **Text Similarity:** {r.get('text_similarity', 'N/A')}\n")
            f.write(f"- **Visual Average:** {r.get('visual_avg', 'N/A')}\n")
            if r.get("ai_visual_avg") is not None:
                f.write(f"- **AI Visual Score:** {r['ai_visual_avg']}\n")
            f.write(f"- **Overall Score:** {r.get('overall_score', 'N/A')}\n")
            f.write(f"- **Pages:** MiniPdf={r.get('minipdf_pages', '?')}, Reference={r.get('reference_pages', '?')}\n")
            f.write(f"- **File Size:** MiniPdf={r.get('minipdf_size', '?')} bytes, Reference={r.get('reference_size', '?')} bytes\n\n")

            diff = r.get("text_diff", "")
            if diff and diff != "(identical)":
                f.write("<details><summary>Text Diff</summary>\n\n```diff\n")
                # Truncate very long diffs
                if len(diff) > 3000:
                    f.write(diff[:3000])
                    f.write(f"\n... ({len(diff) - 3000} more characters)\n")
                else:
                    f.write(diff)
                f.write("\n```\n</details>\n\n")
            else:
                f.write("Text content: ✅ Identical\n\n")

        # Improvement suggestions
        f.write("## Improvement Suggestions\n\n")

        # AI-generated suggestions (aggregated across all test cases)
        all_ai_suggestions: list[tuple[str, str]] = []  # (test_name, suggestion)
        all_ai_differences: list[tuple[str, str]] = []  # (test_name, difference)
        for r in results:
            for ai in r.get("ai_analysis", []):
                if "error" in ai or ai.get("skipped"):
                    continue
                for s in ai.get("suggestions", []):
                    all_ai_suggestions.append((r["name"], s))
                for d in ai.get("differences", []):
                    all_ai_differences.append((r["name"], d))

        if all_ai_differences:
            f.write("### 🤖 AI Visual Analysis Findings\n\n")
            # Deduplicate by lowercased text to compress repetitive findings
            seen: set[str] = set()
            for test_name, diff in all_ai_differences:
                key = diff.lower()[:80]
                if key not in seen:
                    seen.add(key)
                    f.write(f"- **[{test_name}]** {diff}\n")
            f.write("\n")

        if all_ai_suggestions:
            f.write("### 🤖 AI-Recommended Code Improvements\n\n")
            seen_s: set[str] = set()
            for test_name, sug in all_ai_suggestions:
                key = sug.lower()[:80]
                if key not in seen_s:
                    seen_s.add(key)
                    f.write(f"- **[{test_name}]** {sug}\n")
            f.write("\n")

        # Per-case AI analysis blocks
        has_any_ai = any(r.get("ai_analysis") for r in results)
        if has_any_ai:
            f.write("### AI Analysis Per Test Case\n\n")
            for r in results:
                for ai in r.get("ai_analysis", []):
                    if ai.get("skipped"):
                        continue
                    if "error" in ai:
                        f.write(f"- **{r['name']} p{ai['page']}**: ⚠ {ai['error']}\n")
                        continue
                    severity_icon = {"low": "🟢", "medium": "🟡", "high": "🔴"}.get(ai.get("severity", ""), "⚪")
                    f.write(f"<details><summary>{severity_icon} {r['name']} — Page {ai['page']} "
                            f"(AI score: {ai.get('ai_visual_score', 'N/A')}, "
                            f"severity: {ai.get('severity', 'N/A')})</summary>\n\n")
                    if ai.get("differences"):
                        f.write("**Differences:**\n")
                        for d in ai["differences"]:
                            f.write(f"- {d}\n")
                        f.write("\n")
                    if ai.get("suggestions"):
                        f.write("**Suggestions:**\n")
                        for s in ai["suggestions"]:
                            f.write(f"- {s}\n")
                        f.write("\n")
                    f.write("</details>\n\n")

        # Classic score-based suggestions
        low_scores = [(r["name"], r.get("overall_score", 0)) for r in results if r.get("overall_score", 1) < 0.8]
        if low_scores:
            low_scores.sort(key=lambda x: x[1])
            f.write("### ⚠ Low-Score Test Cases (below 0.8)\n\n")
            for name, score in low_scores:
                f.write(f"1. **{name}** (score: {score})\n")
            if not all_ai_suggestions:
                f.write("\nReview the text diffs and visual comparisons above to identify specific rendering issues.\n")
        elif not all_ai_suggestions:
            f.write("All test cases scored 0.8 or above. 🎉\n")
        elif not low_scores:
            f.write("\n✅ All test cases scored 0.8 or above.\n")

    print(f"\nReports saved:")
    print(f"  Markdown: {md_path}")
    print(f"  JSON:     {json_path}")


def main():
    parser = argparse.ArgumentParser(description="Compare MiniPdf PDFs against reference PDFs")
    parser.add_argument("--minipdf-dir", default=os.path.join("..", "MiniPdf.Scripts", "pdf_output"),
                        help="Directory containing MiniPdf-generated PDFs")
    parser.add_argument("--reference-dir", default="reference_pdfs",
                        help="Directory containing reference PDFs (from LibreOffice)")
    parser.add_argument("--report-dir", default="reports",
                        help="Output directory for comparison reports")
    # AI comparison options
    parser.add_argument("--ai-compare", action="store_true",
                        help="Enable AI-based visual comparison via OpenAI / Azure OpenAI vision")
    parser.add_argument("--ai-max-pages", type=int, default=1, metavar="N",
                        help="Maximum number of pages per PDF to send to the AI (default: 1)")
    parser.add_argument("--ai-threshold", type=float, default=0.90, metavar="T",
                        help="Skip AI call when pixel similarity is already above this threshold (default: 0.90)")
    args = parser.parse_args()

    if args.ai_compare:
        if not HAS_OPENAI:
            print("WARNING: --ai-compare requested but 'openai' package is not installed.")
            print("         Install with: pip install openai")
        elif AI_CLIENT is None:
            print("WARNING: --ai-compare requested but no API credentials found.")
            print("         Set OPENAI_API_KEY  OR  AZURE_OPENAI_ENDPOINT + AZURE_OPENAI_KEY.")
        else:
            source = "Azure OpenAI" if os.environ.get("AZURE_OPENAI_ENDPOINT") else "OpenAI"
            print(f"AI comparison enabled  ({source}, model={AI_MODEL})")

    minipdf_dir = os.path.abspath(args.minipdf_dir)
    reference_dir = os.path.abspath(args.reference_dir)
    report_dir = os.path.abspath(args.report_dir)
    images_dir = os.path.join(report_dir, "images")

    os.makedirs(report_dir, exist_ok=True)

    print(f"MiniPdf PDFs:    {minipdf_dir}")
    print(f"Reference PDFs:  {reference_dir}")
    print(f"Report output:   {report_dir}")
    print()

    # Collect all test names from both directories
    names = set()
    for d in [minipdf_dir, reference_dir]:
        if os.path.isdir(d):
            for f in Path(d).glob("*.pdf"):
                names.add(f.stem)

    if not names:
        print("No PDF files found in either directory.")
        print("Run the following first:")
        print("  1. python generate_classic_xlsx.py       (generate test Excel files)")
        print("  2. dotnet run convert_xlsx_to_pdf.cs      (generate MiniPdf PDFs)")
        print("  3. python generate_reference_pdfs.py      (generate reference PDFs)")
        sys.exit(1)

    results = []
    for name in sorted(names):
        mp = os.path.join(minipdf_dir, f"{name}.pdf")
        rp = os.path.join(reference_dir, f"{name}.pdf")
        print(f"Comparing: {name} ...", end=" ")
        result = compare_single(
            mp, rp, images_dir, name,
            ai_compare=args.ai_compare,
            ai_max_pages=args.ai_max_pages,
            ai_threshold=args.ai_threshold,
        )
        score = result.get("overall_score", "N/A")
        print(f"score={score}")
        results.append(result)

    generate_report(results, report_dir)

    # Print summary
    avg = sum(r.get("overall_score", 0) for r in results) / len(results) if results else 0
    print(f"\n{'='*60}")
    print(f"Overall Average Score: {avg:.4f}")
    print(f"{'='*60}")

    if avg < 0.7:
        print("⚠ Many test cases are significantly different from the reference.")
        print("  Check the report for details and improvement suggestions.")


if __name__ == "__main__":
    main()
