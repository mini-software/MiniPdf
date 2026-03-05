#!/usr/bin/env python3
"""Update all README files with benchmark scores from comparison_report.json."""

import json
import re
import os

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPORT_PATH = os.path.join(REPO_ROOT, "tests", "MiniPdf.Benchmark", "reports", "comparison_report.json")

# Load report data
with open(REPORT_PATH, "r", encoding="utf-8") as f:
    report_data = json.load(f)

# Build lookup: name -> {score, label, icon}
scores_map = {}
for item in report_data:
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
        # Build human-readable label from name (remove classicNN_ prefix)
        parts = name.split("_", 1)
        label = parts[1].replace("_", " ").title() if len(parts) > 1 else name
        # Fix casing for special words
        label = label.replace("Xml", "xml").replace("Cjk", "Cjk").replace("Kpi", "kpi")
        label = label.replace("Ohlc", "ohlc").replace("3D", "3d").replace("Vs", "vs")
        scores_map[name] = {"score": pct, "icon": icon, "label": label}

# Compute summary statistics
all_scores = [item["overall_score"] for item in report_data if item.get("overall_score") is not None]
total_count = len(all_scores)
excellent_count = sum(1 for s in all_scores if s >= 0.90)
acceptable_count = sum(1 for s in all_scores if 0.70 <= s < 0.90)
needs_improve_count = sum(1 for s in all_scores if s < 0.70)
avg_score = sum(all_scores) / len(all_scores) * 100 if all_scores else 0

print(f"Total: {total_count}, Excellent: {excellent_count}, Acceptable: {acceptable_count}, Needs Improvement: {needs_improve_count}, Avg: {avg_score:.1f}%")

# Define the files to update with their specific patterns
FILES = {
    "README.md": {
        "path": os.path.join(REPO_ROOT, "README.md"),
        "img_prefix": "tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ classic test cases\*\*.*?30 chart cases.*?\)",
            "count_replace": f"**{total_count} classic test cases** (including 30 image-embedding cases, 30 chart cases, 30 styling cases, and 30 multilingual/emoji cases)",
            "table_excellent": (r"(\| 🟢 Excellent \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 Acceptable \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 Needs Improvement \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*Average overall score: [\d.]+%\*\*", f"**Average overall score: {avg_score:.1f}%**"),
        },
    },
    "README.zh-CN.md": {
        "path": os.path.join(REPO_ROOT, "README.zh-CN.md"),
        "img_prefix": "tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ 个经典测试用例\*\*.*?30 个.*?测试.*?）",
            "count_replace": f"**{total_count} 个经典测试用例**（包含 30 个图片嵌入测试、30 个图表测试、30 个样式测试和 30 个多语言/表情符号测试）",
            "table_excellent": (r"(\| 🟢 优秀 \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 可接受 \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 待改进 \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*整体平均分: [\d.]+%\*\*", f"**整体平均分: {avg_score:.1f}%**"),
        },
    },
    "README.zh-TW.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.zh-TW.md"),
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ 個經典測試案例\*\*.*?30 個.*?測試.*?）",
            "count_replace": f"**{total_count} 個經典測試案例**（包含 30 個圖片嵌入測試、30 個圖表測試、30 個樣式測試和 30 個多語言/表情符號測試）",
            "table_excellent": (r"(\| 🟢 優秀 \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 可接受 \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 待改進 \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*整體平均分: [\d.]+%\*\*", f"**整體平均分: {avg_score:.1f}%**"),
        },
    },
    "README.ja.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.ja.md"),
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ 件のクラシックテストケース\*\*.*?チャート 30 件.*?）",
            "count_replace": f"**{total_count} 件のクラシックテストケース**（画像埋め込み 30 件、チャート 30 件、スタイリング 30 件、多言語/絵文字 30 件を含む）",
            "table_excellent": (r"(\| 🟢 優秀 \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 許容範囲 \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 要改善 \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*総合平均スコア: [\d.]+%\*\*", f"**総合平均スコア: {avg_score:.1f}%**"),
        },
    },
    "README.ko.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.ko.md"),
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+개 클래식 테스트 케이스\*\*.*?차트 30개.*?）",
            "count_replace": f"**{total_count}개 클래식 테스트 케이스**（이미지 임베딩 30개, 차트 30개, 스타일링 30개, 다국어/이모지 30개 포함）",
            "table_excellent": (r"(\| 🟢 우수 \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 허용 \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 개선 필요 \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*전체 평균 점수: [\d.]+%\*\*", f"**전체 평균 점수: {avg_score:.1f}%**"),
        },
    },
    "README.it.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.it.md"),
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ casi di test classici\*\*.*?30 casi con grafici.*?\)",
            "count_replace": f"**{total_count} casi di test classici** (inclusi 30 casi con immagini incorporate, 30 casi con grafici, 30 casi di stile e 30 casi multilingue/emoji)",
            "table_excellent": (r"(\| 🟢 Eccellente \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 Accettabile \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 Da migliorare \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*Punteggio medio complessivo: [\d.]+%\*\*", f"**Punteggio medio complessivo: {avg_score:.1f}%**"),
        },
    },
    "README.fr.md": {
        "path": os.path.join(REPO_ROOT, "documents", "README.fr.md"),
        "img_prefix": "../tests/MiniPdf.Benchmark/reports/images/",
        "summary_patterns": {
            "count_line": r"\*\*\d+ cas de test classiques\*\*.*?30 cas de graphiques.*?\)",
            "count_replace": f"**{total_count} cas de test classiques** (dont 30 cas d'images intégrées, 30 cas de graphiques, 30 cas de style et 30 cas multilingues/emoji)",
            "table_excellent": (r"(\| 🟢 Excellent \| )\d+", f"\\g<1>{excellent_count}"),
            "table_acceptable": (r"(\| 🟡 Acceptable \| )\d+", f"\\g<1>{acceptable_count}"),
            "table_needs": (r"(\| 🔴 À améliorer \| )\d+", f"\\g<1>{needs_improve_count}"),
            "avg_score": (r"\*\*Score moyen global : [\d.]+%\*\*", f"**Score moyen global : {avg_score:.1f}%**"),
        },
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


def update_scores_in_content(content):
    """Update all individual test case scores (icon + percentage) in content."""
    # Pattern: matches lines like "  <td>Some description 🟢 99.8%</td>"
    def replace_score(match):
        prefix = match.group(1)
        name_part = match.group(2)
        # Try to find the test case by looking for the classicNN entry before this line
        return f"{prefix}{match.group(2)} {match.group(3)}"

    # Instead, do name-based replacement: find each "<td><b>classicNN</b></td>" followed by score line
    # and update the score
    lines = content.split("\n")
    i = 0
    while i < len(lines):
        # Look for pattern: <td><b>classicNN</b></td>
        m = re.search(r'<td><b>(classic\d+)</b></td>', lines[i])
        if m:
            short_name = m.group(1)
            # Find the matching full name in report data
            full_name = None
            for item in report_data:
                if item["name"].startswith(short_name + "_"):
                    full_name = item["name"]
                    break
            if full_name and full_name in scores_map:
                info = scores_map[full_name]
                display = get_display_name(full_name)
                # The next line in the same <tr> should have the score
                # Check the line with the score (same line or next few lines)
                for j in range(i, min(i + 3, len(lines))):
                    score_match = re.search(r'<td>(.+?)\s+[🟢🟡🔴]\s+[\d.]+%</td>', lines[j])
                    if score_match:
                        lines[j] = f'  <td>{display} {info["icon"]} {info["score"]:.1f}%</td>'
                        break
        i += 1
    return "\n".join(lines)


def process_file(file_key, file_config):
    """Process a single README file."""
    filepath = file_config["path"]
    img_prefix = file_config["img_prefix"]
    patterns = file_config["summary_patterns"]

    print(f"\nProcessing {file_key}...")

    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Update summary count line
    if "count_line" in patterns:
        content = re.sub(patterns["count_line"], patterns["count_replace"], content)

    # 2. Update summary table counts
    for key in ["table_excellent", "table_acceptable", "table_needs"]:
        if key in patterns:
            pattern, replacement = patterns[key]
            content = re.sub(pattern, replacement, content)

    # 3. Update average score
    if "avg_score" in patterns:
        pattern, replacement = patterns["avg_score"]
        content = re.sub(pattern, replacement, content)

    # 4. Update individual test case scores
    content = update_scores_in_content(content)

    # 5. Add missing test cases (classic121+) if not present
    max_classic_in_content = 0
    for m_num in re.finditer(r'classic(\d+)', content):
        num = int(m_num.group(1))
        if num > max_classic_in_content:
            max_classic_in_content = num

    # Find test cases in report that are beyond what's in the content
    new_cases = sorted(
        [item["name"] for item in report_data
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
