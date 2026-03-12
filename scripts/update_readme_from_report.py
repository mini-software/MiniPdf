#!/usr/bin/env python3
"""Update all README files with benchmark scores from three comparison_report.json files."""

import json
import re
import os

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# --- Load all three report JSONs ---
REPORT_PATHS = {
    "xlsx": os.path.join(REPO_ROOT, "tests", "MiniPdf.Benchmark", "reports", "comparison_report.json"),
    "docx": os.path.join(REPO_ROOT, "tests", "MiniPdf.Benchmark", "reports_docx", "comparison_report.json"),
    "issues": os.path.join(REPO_ROOT, "tests", "Issue_Files", "reports_xlsx", "comparison_report.json"),
}

report_data = {}  # key -> list of items
for key, path in REPORT_PATHS.items():
    if os.path.exists(path):
        with open(path, "r", encoding="utf-8") as f:
            report_data[key] = json.load(f)
        print(f"Loaded {key}: {len(report_data[key])} items from {os.path.basename(os.path.dirname(path))}")
    else:
        report_data[key] = []
        print(f"Warning: {key} report not found at {path}")

# Build lookup: name -> {score, label, icon}  (xlsx classic only, for visual comparison)
scores_map = {}
for item in report_data.get("xlsx", []):
    name = item["name"]
    score = item.get("overall_score")
    if score is not None:
        pct = score * 100
        if pct >= 90:
            icon = "\U0001f7e2"  # 🟢
        elif pct >= 70:
            icon = "\U0001f7e1"  # 🟡
        else:
            icon = "\U0001f534"  # 🔴
        parts = name.split("_", 1)
        label = parts[1].replace("_", " ").title() if len(parts) > 1 else name
        label = label.replace("Xml", "xml").replace("Cjk", "Cjk").replace("Kpi", "kpi")
        label = label.replace("Ohlc", "ohlc").replace("3D", "3d").replace("Vs", "vs")
        scores_map[name] = {"score": pct, "icon": icon, "label": label}


def compute_stats(items):
    """Compute benchmark stats for a list of report items."""
    scores = [i["overall_score"] for i in items if i.get("overall_score") is not None]
    if not scores:
        return {"count": 0, "excellent": 0, "acceptable": 0, "needs": 0, "avg": 0.0}
    return {
        "count": len(scores),
        "excellent": sum(1 for s in scores if s >= 0.90),
        "acceptable": sum(1 for s in scores if 0.70 <= s < 0.90),
        "needs": sum(1 for s in scores if s < 0.70),
        "avg": sum(scores) / len(scores) * 100,
    }


xlsx_stats = compute_stats(report_data.get("xlsx", []))
docx_stats = compute_stats(report_data.get("docx", []))
issues_stats = compute_stats(report_data.get("issues", []))

# Combined totals
all_items = []
for items in report_data.values():
    all_items.extend(items)
total_stats = compute_stats(all_items)

print(f"\nXLSX:   {xlsx_stats['count']} cases, avg={xlsx_stats['avg']:.1f}%, excellent={xlsx_stats['excellent']}, acceptable={xlsx_stats['acceptable']}, needs={xlsx_stats['needs']}")
print(f"DOCX:   {docx_stats['count']} cases, avg={docx_stats['avg']:.1f}%, excellent={docx_stats['excellent']}, acceptable={docx_stats['acceptable']}, needs={docx_stats['needs']}")
print(f"Issues: {issues_stats['count']} cases, avg={issues_stats['avg']:.1f}%, excellent={issues_stats['excellent']}, acceptable={issues_stats['acceptable']}, needs={issues_stats['needs']}")
print(f"Total:  {total_stats['count']} cases, avg={total_stats['avg']:.1f}%, excellent={total_stats['excellent']}, acceptable={total_stats['acceptable']}, needs={total_stats['needs']}")


def build_benchmark_section(lang):
    """Build the benchmark summary section for a given language."""
    xs, ds, iss, ts = xlsx_stats, docx_stats, issues_stats, total_stats

    if lang == "en":
        return f"""## Benchmark

MiniPdf output is compared against LibreOffice as the reference renderer across **{ts['count']} test cases**.

| Report | Cases | \U0001f7e2 Excellent (\u226590%) | \U0001f7e1 Acceptable (70%\u201390%) | \U0001f534 Needs Improvement (<70%) | Average Score |
|---|---|---|---|---|---|
| XLSX to PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX to PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX Files | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **Total** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

Scoring: text similarity 40% + visual similarity 40% + page count 20%"""

    elif lang == "zh-CN":
        return f"""## 基准测试

MiniPdf 的输出与 LibreOffice 作为参考渲染器进行对比，共 **{ts['count']} 个测试用例**。

| 报告 | 用例数 | \U0001f7e2 优秀 (\u226590%) | \U0001f7e1 可接受 (70%\u201390%) | \U0001f534 待改进 (<70%) | 平均分 |
|---|---|---|---|---|---|
| XLSX 转 PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX 转 PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX 文件 | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **合计** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

评分方式：文本相似度 40% + 视觉相似度 40% + 页数 20%"""

    elif lang == "zh-TW":
        return f"""## 基準測試

MiniPdf 的輸出與 LibreOffice 作為參考渲染器進行對比，共 **{ts['count']} 個測試案例**。

| 報告 | 案例數 | \U0001f7e2 優秀 (\u226590%) | \U0001f7e1 可接受 (70%\u201390%) | \U0001f534 待改進 (<70%) | 平均分 |
|---|---|---|---|---|---|
| XLSX 轉 PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX 轉 PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX 檔案 | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **合計** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

評分方式：文字相似度 40% + 視覺相似度 40% + 頁數 20%"""

    elif lang == "ja":
        return f"""## ベンチマーク

MiniPdf の出力は LibreOffice をリファレンスレンダラーとして **{ts['count']} 件のテストケース**で比較されています。

| レポート | 件数 | \U0001f7e2 優秀 (\u226590%) | \U0001f7e1 許容範囲 (70%\u201390%) | \U0001f534 要改善 (<70%) | 平均スコア |
|---|---|---|---|---|---|
| XLSX → PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX → PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX ファイル | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **合計** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

スコアリング：テキスト類似度 40% + 視覚類似度 40% + ページ数 20%"""

    elif lang == "ko":
        return f"""## 벤치마크

MiniPdf 출력은 LibreOffice를 참조 렌더러로 사용하여 **{ts['count']}개 테스트 케이스**에서 비교됩니다.

| 리포트 | 케이스 수 | \U0001f7e2 우수 (\u226590%) | \U0001f7e1 허용 (70%\u201390%) | \U0001f534 개선 필요 (<70%) | 평균 점수 |
|---|---|---|---|---|---|
| XLSX → PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX → PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX 파일 | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **합계** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

점수 산정: 텍스트 유사도 40% + 시각 유사도 40% + 페이지 수 20%"""

    elif lang == "it":
        return f"""## Benchmark

L'output di MiniPdf viene confrontato con LibreOffice come renderer di riferimento su **{ts['count']} casi di test**.

| Report | Casi | \U0001f7e2 Eccellente (\u226590%) | \U0001f7e1 Accettabile (70%\u201390%) | \U0001f534 Da migliorare (<70%) | Punteggio medio |
|---|---|---|---|---|---|
| XLSX → PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX → PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX File | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **Totale** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

Punteggio: similarità testo 40% + similarità visiva 40% + conteggio pagine 20%"""

    elif lang == "fr":
        return f"""## Benchmark

La sortie de MiniPdf est comparée à LibreOffice comme moteur de rendu de référence sur **{ts['count']} cas de test**.

| Rapport | Cas | \U0001f7e2 Excellent (\u226590%) | \U0001f7e1 Acceptable (70%\u201390%) | \U0001f534 À améliorer (<70%) | Score moyen |
|---|---|---|---|---|---|
| XLSX → PDF | {xs['count']} | {xs['excellent']} | {xs['acceptable']} | {xs['needs']} | {xs['avg']:.1f}% |
| DOCX → PDF | {ds['count']} | {ds['excellent']} | {ds['acceptable']} | {ds['needs']} | {ds['avg']:.1f}% |
| Issue XLSX Fichiers | {iss['count']} | {iss['excellent']} | {iss['acceptable']} | {iss['needs']} | {iss['avg']:.1f}% |
| **Total** | **{ts['count']}** | **{ts['excellent']}** | **{ts['acceptable']}** | **{ts['needs']}** | **{ts['avg']:.1f}%** |

Notation : similarité texte 40% + similarité visuelle 40% + nombre de pages 20%"""

    return ""


# Define the files to update
FILES = {
    "README.md": {
        "path": os.path.join(REPO_ROOT, "README.md"),
        "lang": "en",
        "img_prefix": "tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## Benchmark",
        "visual_heading": r"### Visual Comparison",
    },
    "README.zh-CN.md": {
        "path": os.path.join(REPO_ROOT, "README.zh-CN.md"),
        "lang": "zh-CN",
        "img_prefix": "tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## 基准测试",
        "visual_heading": r"### 视觉对比",
    },
    "README.zh-TW.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.zh-TW.md"),
        "lang": "zh-TW",
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## 基準測試",
        "visual_heading": r"### 視覺對比",
    },
    "README.ja.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.ja.md"),
        "lang": "ja",
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## ベンチマーク",
        "visual_heading": r"### ビジュアル比較",
    },
    "README.ko.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.ko.md"),
        "lang": "ko",
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## 벤치마크",
        "visual_heading": r"### 시각적 비교",
    },
    "README.it.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.it.md"),
        "lang": "it",
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## Benchmark",
        "visual_heading": r"### Confronto visivo",
    },
    "README.fr.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.fr.md"),
        "lang": "fr",
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "benchmark_section_pattern": r"## Benchmark",
        "visual_heading": r"### Comparaison visuelle",
    },
}

def get_display_name(name):
    """Get human-readable display name from test case name."""
    parts = name.split("_", 1)
    if len(parts) > 1:
        return parts[1].replace("_", " ").capitalize()
    return name


def get_short_name(name):
    """Extract classicNN from name."""
    return name.split("_")[0]


def generate_test_case_rows(name, img_prefix):
    """Generate the HTML table rows for a test case."""
    info = scores_map.get(name)
    if not info:
        return ""
    short = get_short_name(name)
    display = get_display_name(name)
    icon = info["icon"]
    pct = info["score"]
    return f"""<tr>
  <td><b>{short}</b></td>
  <td>{display} {icon} {pct:.1f}%</td>
</tr>
<tr>
  <td><img src="{img_prefix}{name}_p1_minipdf.png" width="320"/></td>
  <td><img src="{img_prefix}{name}_p1_reference.png" width="320"/></td>
</tr>"""


def update_scores_in_content(content, xlsx_items):
    """Update all individual test case scores (icon + percentage) in content."""
    lines = content.split("\n")
    i = 0
    while i < len(lines):
        m = re.search(r'<td><b>(classic\d+)</b></td>', lines[i])
        if m:
            short_name = m.group(1)
            full_name = None
            for item in xlsx_items:
                if item["name"].startswith(short_name + "_"):
                    full_name = item["name"]
                    break
            if full_name and full_name in scores_map:
                info = scores_map[full_name]
                display = get_display_name(full_name)
                for j in range(i, min(i + 3, len(lines))):
                    score_match = re.search(r'<td>(.+?)\s+[🟢🟡🔴]\s+[\d.]+%</td>', lines[j])
                    if score_match:
                        lines[j] = f'  <td>{display} {info["icon"]} {info["score"]:.1f}%</td>'
                        break
        i += 1
    return "\n".join(lines)


def replace_benchmark_section(content, lang, file_key):
    """Replace the benchmark summary section with updated content from all 3 reports."""
    new_section = build_benchmark_section(lang)
    if not new_section:
        return content

    config = FILES[file_key]
    # Determine link prefix based on whether file is in documents/ subfolder
    is_documents = "documents" in config["path"]
    link_prefix = "../" if is_documents else ""

    # Build the Detailed Comparison Reports subsection per language
    report_links = {
        "en": (
            "### Detailed Comparison Reports\n\n"
            f"- [XLSX Benchmark Report]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX conversion test cases\n"
            f"- [DOCX Benchmark Report]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX conversion test cases\n"
            f"- [Issue Files Xlsx Report]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — Real-world issue file test cases"
        ),
        "zh-CN": (
            "### 详细对比报告\n\n"
            f"- [XLSX 基准测试报告]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX 转换测试用例\n"
            f"- [DOCX 基准测试报告]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX 转换测试用例\n"
            f"- [Issue XLSX 文件报告]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — 实际 Issue 文件测试用例"
        ),
        "zh-TW": (
            "### 詳細對比報告\n\n"
            f"- [XLSX 基準測試報告]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX 轉換測試案例\n"
            f"- [DOCX 基準測試報告]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX 轉換測試案例\n"
            f"- [Issue XLSX 檔案報告]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — 實際 Issue 檔案測試案例"
        ),
        "ja": (
            "### 詳細比較レポート\n\n"
            f"- [XLSX ベンチマークレポート]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX 変換テストケース\n"
            f"- [DOCX ベンチマークレポート]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX 変換テストケース\n"
            f"- [Issue XLSX ファイルレポート]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — 実際の Issue ファイルテストケース"
        ),
        "ko": (
            "### 상세 비교 보고서\n\n"
            f"- [XLSX 벤치마크 보고서]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX 변환 테스트 케이스\n"
            f"- [DOCX 벤치마크 보고서]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX 변환 테스트 케이스\n"
            f"- [Issue XLSX 파일 보고서]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — 실제 Issue 파일 테스트 케이스"
        ),
        "it": (
            "### Rapporti di confronto dettagliati\n\n"
            f"- [Report Benchmark XLSX]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — Casi di test conversione XLSX\n"
            f"- [Report Benchmark DOCX]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — Casi di test conversione DOCX\n"
            f"- [Report File Issue XLSX]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — Casi di test con file Issue reali"
        ),
        "fr": (
            "### Rapports de comparaison détaillés\n\n"
            f"- [Rapport Benchmark XLSX]({link_prefix}tests/MiniPdf.Benchmark/reports/comparison_report.md) — Cas de test conversion XLSX\n"
            f"- [Rapport Benchmark DOCX]({link_prefix}tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — Cas de test conversion DOCX\n"
            f"- [Rapport Fichiers Issue XLSX]({link_prefix}tests/Issue_Files/reports_xlsx/comparison_report.md) — Cas de test avec fichiers Issue réels"
        ),
    }

    links_section = report_links.get(lang, report_links["en"])

    visual_heading = config.get("visual_heading", r"### Visual Comparison")
    pattern_str = config.get("benchmark_section_pattern")
    if pattern_str:
        # Replace from section header up to (but not including) the visual comparison heading
        old_pattern = pattern_str + r".*?(?=" + visual_heading + r")"
        new_text = new_section + "\n\n" + links_section + "\n\n"
        content = re.sub(old_pattern, new_text, content, count=1, flags=re.DOTALL)

    return content


def process_file(file_key, file_config):
    """Process a single README file."""
    filepath = file_config["path"]
    img_prefix = file_config["img_prefix"]
    lang = file_config["lang"]

    print(f"\nProcessing {file_key} ({lang})...")

    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Replace benchmark summary section
    content = replace_benchmark_section(content, lang, file_key)

    # 2. Update individual test case scores in visual comparison table
    content = update_scores_in_content(content, report_data.get("xlsx", []))

    # 3. Add missing test cases if not present
    max_classic_in_content = 0
    for m_num in re.finditer(r'classic(\d+)', content):
        num = int(m_num.group(1))
        if num > max_classic_in_content:
            max_classic_in_content = num

    xlsx_items = report_data.get("xlsx", [])
    new_cases = sorted(
        [item["name"] for item in xlsx_items
         if item["name"].startswith("classic")
         and int(item["name"].split("_")[0][7:]) > max_classic_in_content],
        key=lambda x: int(x.split("_")[0][7:])
    )
    if new_cases:
        new_rows = []
        for name in new_cases:
            row = generate_test_case_rows(name, img_prefix)
            if row:
                new_rows.append(row)
        if new_rows:
            insert_text = "\n".join(new_rows) + "\n"
            content = content.replace("</table>", insert_text + "</table>")
            print(f"  Added {len(new_rows)} new test case entries (classic{max_classic_in_content+1}+)")

    with open(filepath, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)

    print(f"  Updated {file_key} successfully")


# Process all files
for key, config in FILES.items():
    if os.path.exists(config["path"]):
        process_file(key, config)
    else:
        print(f"  Skipping {key}: file not found at {config['path']}")

print("\nAll README files updated!")
