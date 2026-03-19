# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet 다운로드"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://gitee.com/dotnetchina/MiniPdf"><img src="https://img.shields.io/badge/Gitee-red?logo=gitee" alt="Gitee"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="라이선스"></a>
</p>
<p>
<a href="../README.md">English</a> | <a href="../README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | 한국어 | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>
</p>
</div>

---

Office 파일을 PDF로 변환하는 최소한의 경량 .NET 라이브러리입니다.

👉 **[온라인 데모](https://mini-software.github.io/MiniPdf/)** — 설치 없이 브라우저에서 바로 체험하세요.

> ⚠️ 브라우저에서 사용할 수 있는 글꼴이 제한되어 있어 렌더링 결과가 최적이 아닐 수 있습니다. 최상의 결과를 위해 로컬 환경에서 테스트해 주세요.

## 기능

- **Excel → PDF 변환** — `.xlsx` 파일을 PDF로 변환
- **Word → PDF 변환** — `.docx` 파일을 PDF로 변환
- **최소 의존성** — 경량 설계, 거의 .NET 내장 API만 사용
- **서버리스 지원** — COM 불필요, Office 설치 불필요, Adobe Acrobat 불필요 — .NET만 있으면 어디서든 실행
- **유효한 PDF 1.4** 형식 출력
- **100% 오픈소스 & 무료** — Apache 2.0 라이선스, 상업적 사용 자유, 출처 표기만 하면 OK. PR 기여 환영!
- **차트** — 현재 충분히 지원되지 않음

> **참고:** MiniPdf는 현재 기본적인 변환만 처리할 수 있습니다. 복잡하거나 고도로 맞춤 설정된 Office 문서는 완벽하게 변환되지 않을 수 있습니다. 호환성 문제가 발생하면 [Issue를 등록](https://github.com/shps951023/MiniPdf/issues)하거나 Pull Request를 제출해 주세요. 기여를 환영합니다!

## 시작하기

### NuGet으로 설치

```bash
dotnet add package MiniPdf
```

## 사용 방법

```csharp
using MiniSoftware;

// Excel을 PDF로
MiniPdf.ConvertToPdf("data.xlsx", "output.pdf");

// Word를 PDF로
MiniPdf.ConvertToPdf("report.docx", "output.pdf");

// 파일에서 바이트 배열로
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// 스트림에서 바이트 배열로
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## 사용자 지정 글꼴

시스템 글꼴이 제한된 환경(예: 컨테이너, Blazor WASM)에서는 변환 전에 글꼴을 등록하세요.

```csharp
using MiniSoftware;

// 앱 시작 시 하나 이상의 글꼴 등록을 권장
MiniPdf.RegisterFont("NotoSansSC", File.ReadAllBytes("Fonts/NotoSansSC-Regular.ttf"));
MiniPdf.RegisterFont("NotoColorEmoji", File.ReadAllBytes("Fonts/NotoColorEmoji.ttf"));

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

참고:

- 입력 바이트는 TrueType `.ttf` 및 TrueType Collection `.ttc`를 지원합니다.
- 중복 등록을 피하려면 시작 시 한 번만 등록하는 것을 권장합니다.
- 등록된 글꼴은 시스템 글꼴보다 우선 적용됩니다.

## 벤치마크

MiniPdf 출력은 LibreOffice를 참조 렌더러로 사용하여 **373개 테스트 케이스**에서 비교됩니다.

| 리포트 | 케이스 수 | 🟢 우수 (≥90%) | 🟡 허용 (70%–90%) | 🔴 개선 필요 (<70%) | 평균 점수 |
|---|---|---|---|---|---|
| XLSX → PDF | 191 | 175 | 16 | 0 | 96.9% |
| DOCX → PDF | 180 | 178 | 2 | 0 | 97.6% |
| Issue XLSX 파일 | 2 | 1 | 1 | 0 | 83.8% |
| **합계** | **373** | **354** | **19** | **0** | **97.2%** |

점수 산정: 텍스트 유사도 40% + 시각 유사도 40% + 페이지 수 20%

### 상세 비교 보고서

- [XLSX 벤치마크 보고서](../tests/MiniPdf.Benchmark/reports/comparison_report.md) — XLSX 변환 테스트 케이스
- [DOCX 벤치마크 보고서](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) — DOCX 변환 테스트 케이스
- [Issue XLSX 파일 보고서](../tests/Issue_Files/reports_xlsx/comparison_report.md) — 실제 Issue 파일 테스트 케이스
- [Issue DOCX 파일 보고서](../tests/Issue_Files/reports_docx/comparison_report.md) — 실제 Issue 파일 테스트 케이스

### 시각적 비교

모든 테스트 케이스에서 MiniPdf 출력과 LibreOffice 참조를 비교합니다. 여러 페이지인 경우 1페이지만 표시합니다.

<table>
<tr><th>MiniPdf</th><th>LibreOffice (참조)</th></tr>
<tr>
  <td><b>classic01</b></td>
  <td>Basic table with headers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic02</b></td>
  <td>Multiple worksheets 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic03</b></td>
  <td>Empty workbook 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic04</b></td>
  <td>Single cell 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic05</b></td>
  <td>Wide table 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic06</b></td>
  <td>Tall table 🟢 97.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic07</b></td>
  <td>Numbers only 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic08</b></td>
  <td>Mixed text and numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic09</b></td>
  <td>Long text 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic10</b></td>
  <td>Special xml characters 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic11</b></td>
  <td>Sparse rows 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic12</b></td>
  <td>Sparse columns 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic13</b></td>
  <td>Date strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic14</b></td>
  <td>Decimal numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic15</b></td>
  <td>Negative numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic16</b></td>
  <td>Percentage strings 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic17</b></td>
  <td>Currency strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic18</b></td>
  <td>Large dataset 🟢 95.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic19</b></td>
  <td>Single column list 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic20</b></td>
  <td>All empty cells 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic21</b></td>
  <td>Header only 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic22</b></td>
  <td>Long sheet name 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic23</b></td>
  <td>Unicode text 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic24</b></td>
  <td>Red text 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic25</b></td>
  <td>Multiple colors 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic26</b></td>
  <td>Inline strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic27</b></td>
  <td>Single row 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic28</b></td>
  <td>Duplicate values 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic29</b></td>
  <td>Formula results 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic30</b></td>
  <td>Mixed empty and filled sheets 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic31</b></td>
  <td>Bold header row 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic32</b></td>
  <td>Right aligned numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic33</b></td>
  <td>Centered text 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic34</b></td>
  <td>Explicit column widths 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic35</b></td>
  <td>Explicit row heights 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic36</b></td>
  <td>Merged cells 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic37</b></td>
  <td>Freeze panes 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic38</b></td>
  <td>Hyperlink cell 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic39</b></td>
  <td>Financial table 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic40</b></td>
  <td>Scientific notation 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic41</b></td>
  <td>Integer vs float 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic42</b></td>
  <td>Boolean values 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic43</b></td>
  <td>Inventory report 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic44</b></td>
  <td>Employee roster 🟢 98.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic45</b></td>
  <td>Sales by region 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic46</b></td>
  <td>Grade book 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic47</b></td>
  <td>Time series 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic48</b></td>
  <td>Survey results 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic49</b></td>
  <td>Contact list 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic50</b></td>
  <td>Budget vs actuals 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic51</b></td>
  <td>Product catalog 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic52</b></td>
  <td>Pivot summary 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic53</b></td>
  <td>Invoice 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic54</b></td>
  <td>Multi level header 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic55</b></td>
  <td>Error values 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic56</b></td>
  <td>Alternating row colors 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic57</b></td>
  <td>Cjk only 🟢 91.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic58</b></td>
  <td>Mixed numeric formats 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic59</b></td>
  <td>Multi sheet summary 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic60</b></td>
  <td>Large wide table 🟢 97.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic61</b></td>
  <td>Product card with image 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic61_product_card_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic61_product_card_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic62</b></td>
  <td>Company logo header 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic62_company_logo_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic62_company_logo_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic63</b></td>
  <td>Two products side by side 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic63_two_products_side_by_side_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic63_two_products_side_by_side_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic64</b></td>
  <td>Employee directory with photo 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic64_employee_directory_with_photo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic64_employee_directory_with_photo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic65</b></td>
  <td>Inventory with product photos 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic65_inventory_with_product_photos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic65_inventory_with_product_photos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic66</b></td>
  <td>Invoice with logo 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic66_invoice_with_logo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic66_invoice_with_logo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic67</b></td>
  <td>Real estate listing 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic67_real_estate_listing_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic67_real_estate_listing_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic68</b></td>
  <td>Restaurant menu 🟢 98.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic68_restaurant_menu_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic68_restaurant_menu_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic69</b></td>
  <td>Image only sheet 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic69_image_only_sheet_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic69_image_only_sheet_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic70</b></td>
  <td>Product catalog with images 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic70_product_catalog_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic70_product_catalog_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic71</b></td>
  <td>Multi sheet with images 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic71_multi_sheet_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic71_multi_sheet_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic72</b></td>
  <td>Bar chart image with data 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic72_bar_chart_image_with_data_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic72_bar_chart_image_with_data_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic73</b></td>
  <td>Event flyer with banner 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic73_event_flyer_with_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic73_event_flyer_with_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic74</b></td>
  <td>Dashboard with kpi image 🟢 97.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic74_dashboard_with_kpi_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic75</b></td>
  <td>Certificate with seal 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic75_certificate_with_seal_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic75_certificate_with_seal_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic76</b></td>
  <td>Product image grid 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic76_product_image_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic76_product_image_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic77</b></td>
  <td>News article with hero image 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic77_news_article_with_hero_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic77_news_article_with_hero_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic78</b></td>
  <td>Small icon per row 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic78_small_icon_per_row_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic78_small_icon_per_row_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic79</b></td>
  <td>Wide panoramic banner 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic79_wide_panoramic_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic79_wide_panoramic_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic80</b></td>
  <td>Portrait tall image 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic80_portrait_tall_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic80_portrait_tall_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic81</b></td>
  <td>Step by step with images 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic81_step_by_step_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic81_step_by_step_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic82</b></td>
  <td>Before after images 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic82_before_after_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic82_before_after_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic83</b></td>
  <td>Color swatch palette 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic83_color_swatch_palette_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic83_color_swatch_palette_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic84</b></td>
  <td>Travel destination cards 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic84_travel_destination_cards_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic84_travel_destination_cards_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic85</b></td>
  <td>Lab results with image 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic85_lab_results_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic85_lab_results_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic86</b></td>
  <td>Software screenshot features 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic86_software_screenshot_features_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic86_software_screenshot_features_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic87</b></td>
  <td>Sports results with logos 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic87_sports_results_with_logos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic87_sports_results_with_logos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic88</b></td>
  <td>Image after data 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic88_image_after_data_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic88_image_after_data_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic89</b></td>
  <td>Nutrition label with image 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic89_nutrition_label_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic89_nutrition_label_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic90</b></td>
  <td>Project status with milestones 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic90_project_status_with_milestones_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic90_project_status_with_milestones_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic91</b></td>
  <td>Simple bar chart 🟢 96.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic91_simple_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic91_simple_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic92</b></td>
  <td>Horizontal bar chart 🟢 96.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic92_horizontal_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic92_horizontal_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic93</b></td>
  <td>Line chart 🟢 93.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic93_line_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic93_line_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic94</b></td>
  <td>Pie chart 🟢 97.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic94_pie_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic94_pie_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic95</b></td>
  <td>Area chart 🟡 76.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic95_area_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic95_area_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic96</b></td>
  <td>Scatter chart 🟢 94.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic96_scatter_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic96_scatter_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic97</b></td>
  <td>Doughnut chart 🟢 97.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic97_doughnut_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic97_doughnut_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic98</b></td>
  <td>Radar chart 🟢 95.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic98_radar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic98_radar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic99</b></td>
  <td>Bubble chart 🟢 92.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic99_bubble_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic99_bubble_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic100</b></td>
  <td>Stacked bar chart 🟢 95.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic100_stacked_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic100_stacked_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic101</b></td>
  <td>Percent stacked bar 🟢 93.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic101_percent_stacked_bar_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic101_percent_stacked_bar_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic102</b></td>
  <td>Line chart with markers 🟢 93.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic102_line_chart_with_markers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic102_line_chart_with_markers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic103</b></td>
  <td>Pie chart with labels 🟡 86.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic103_pie_chart_with_labels_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic103_pie_chart_with_labels_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic104</b></td>
  <td>Combo bar line chart 🟡 81.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic104_combo_bar_line_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic104_combo_bar_line_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic105</b></td>
  <td>3d bar chart 🟡 85.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic105_3d_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic105_3d_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic106</b></td>
  <td>3d pie chart 🟢 96.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic106_3d_pie_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic106_3d_pie_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic107</b></td>
  <td>Multi series line 🟡 80.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic107_multi_series_line_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic107_multi_series_line_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic108</b></td>
  <td>Stacked area chart 🟢 94.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic108_stacked_area_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic108_stacked_area_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic109</b></td>
  <td>Scatter with trendline 🟢 92.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic109_scatter_with_trendline_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic109_scatter_with_trendline_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic110</b></td>
  <td>Chart with legend 🟡 84.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic110_chart_with_legend_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic110_chart_with_legend_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic111</b></td>
  <td>Chart with axis labels 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic111_chart_with_axis_labels_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic111_chart_with_axis_labels_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic112</b></td>
  <td>Multiple charts 🟡 85.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic112_multiple_charts_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic112_multiple_charts_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic113</b></td>
  <td>Chart sheet 🟡 86.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic113_chart_sheet_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic113_chart_sheet_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic114</b></td>
  <td>Chart large dataset 🟢 91.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic114_chart_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic114_chart_large_dataset_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic115</b></td>
  <td>Chart negative values 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic115_chart_negative_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic115_chart_negative_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic116</b></td>
  <td>Percent stacked area 🟢 93.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic116_percent_stacked_area_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic116_percent_stacked_area_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic117</b></td>
  <td>Stock ohlc chart 🟡 80.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic117_stock_ohlc_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic117_stock_ohlc_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic118</b></td>
  <td>Bar chart custom colors 🟢 95.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic118_bar_chart_custom_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic118_bar_chart_custom_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic119</b></td>
  <td>Dashboard multi charts 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic119_dashboard_multi_charts_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic119_dashboard_multi_charts_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic120</b></td>
  <td>Chart with date axis 🟡 76.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic120_chart_with_date_axis_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic120_chart_with_date_axis_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic121</b></td>
  <td>Thin borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic121_thin_borders_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic121_thin_borders_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic122</b></td>
  <td>Thick outer thin inner 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic122_thick_outer_thin_inner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic122_thick_outer_thin_inner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic123</b></td>
  <td>Dashed borders 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic123_dashed_borders_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic123_dashed_borders_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic124</b></td>
  <td>Colored borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic124_colored_borders_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic124_colored_borders_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic125</b></td>
  <td>Solid fills 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic125_solid_fills_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic125_solid_fills_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic126</b></td>
  <td>Dark header 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic126_dark_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic126_dark_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic127</b></td>
  <td>Font styles 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic127_font_styles_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic127_font_styles_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic128</b></td>
  <td>Font sizes 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic128_font_sizes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic128_font_sizes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic129</b></td>
  <td>Alignment combos 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic129_alignment_combos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic129_alignment_combos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic130</b></td>
  <td>Wrap and indent 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic130_wrap_and_indent_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic130_wrap_and_indent_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic131</b></td>
  <td>Number formats 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic131_number_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic131_number_formats_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic132</b></td>
  <td>Striped table 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic132_striped_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic132_striped_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic133</b></td>
  <td>Gradient rows 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic133_gradient_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic133_gradient_rows_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic134</b></td>
  <td>Heatmap 🟢 98.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic134_heatmap_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic134_heatmap_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic135</b></td>
  <td>Bottom border only 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic135_bottom_border_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic135_bottom_border_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic136</b></td>
  <td>Financial report styled 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic136_financial_report_styled_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic136_financial_report_styled_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic137</b></td>
  <td>Checkerboard 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic137_checkerboard_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic137_checkerboard_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic138</b></td>
  <td>Color grid 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic138_color_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic138_color_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic139</b></td>
  <td>Pattern fills 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic139_pattern_fills_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic139_pattern_fills_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic140</b></td>
  <td>Rotated text 🟢 98.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic140_rotated_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic140_rotated_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic141</b></td>
  <td>Mixed edge borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic141_mixed_edge_borders_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic141_mixed_edge_borders_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic142</b></td>
  <td>Styled invoice 🟢 98.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic142_styled_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic142_styled_invoice_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic143</b></td>
  <td>Colored tabs 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic143_colored_tabs_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic143_colored_tabs_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic144</b></td>
  <td>Note style cells 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic144_note_style_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic144_note_style_cells_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic145</b></td>
  <td>Status badges 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic145_status_badges_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic145_status_badges_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic146</b></td>
  <td>Double border table 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic146_double_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic146_double_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic147</b></td>
  <td>Multi sheet styled 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic147_multi_sheet_styled_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic147_multi_sheet_styled_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic148</b></td>
  <td>Frozen styled grid 🟢 97.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic148_frozen_styled_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic148_frozen_styled_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic149</b></td>
  <td>Merged styled sections 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic149_merged_styled_sections_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic149_merged_styled_sections_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic150</b></td>
  <td>Kitchen sink styles 🟢 97.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic150_kitchen_sink_styles_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic150_kitchen_sink_styles_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic151</b></td>
  <td>Multilingual greetings 🟢 93.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic151_multilingual_greetings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic151_multilingual_greetings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic152</b></td>
  <td>Emoji sampler 🟢 91.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic152_emoji_sampler_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic152_emoji_sampler_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic153</b></td>
  <td>Currency symbols 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic153_currency_symbols_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic153_currency_symbols_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic154</b></td>
  <td>Math symbols 🟢 95.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic154_math_symbols_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic154_math_symbols_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic155</b></td>
  <td>Diacritical marks 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic155_diacritical_marks_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic155_diacritical_marks_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic156</b></td>
  <td>Rtl bidi text 🟡 84.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic156_rtl_bidi_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic156_rtl_bidi_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic157</b></td>
  <td>Cjk extended 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic157_cjk_extended_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic157_cjk_extended_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic158</b></td>
  <td>Emoji skin tones 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic158_emoji_skin_tones_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic158_emoji_skin_tones_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic159</b></td>
  <td>Zwj emoji 🟢 90.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic159_zwj_emoji_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic159_zwj_emoji_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic160</b></td>
  <td>Punctuation marks 🟢 94.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic160_punctuation_marks_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic160_punctuation_marks_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic161</b></td>
  <td>Box drawing 🟢 95.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic161_box_drawing_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic161_box_drawing_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic162</b></td>
  <td>Cjk emoji styled 🟡 86.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic162_cjk_emoji_styled_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic162_cjk_emoji_styled_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic163</b></td>
  <td>Cyrillic alphabets 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic163_cyrillic_alphabets_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic163_cyrillic_alphabets_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic164</b></td>
  <td>Indic scripts 🟡 87.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic164_indic_scripts_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic164_indic_scripts_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic165</b></td>
  <td>Southeast asian 🟢 96.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic165_southeast_asian_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic165_southeast_asian_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic166</b></td>
  <td>Emoji progress 🟢 98.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic166_emoji_progress_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic166_emoji_progress_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic167</b></td>
  <td>Musical symbols 🟢 90.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic167_musical_symbols_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic167_musical_symbols_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic168</b></td>
  <td>Mixed ltr rtl styled 🟢 94.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic168_mixed_ltr_rtl_styled_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic168_mixed_ltr_rtl_styled_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic169</b></td>
  <td>Korean invoice 🟡 88.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic169_korean_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic169_korean_invoice_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic170</b></td>
  <td>Emoji dashboard 🟢 96.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic170_emoji_dashboard_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic170_emoji_dashboard_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic171</b></td>
  <td>Ipa phonetic 🟢 97.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic171_ipa_phonetic_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic171_ipa_phonetic_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic172</b></td>
  <td>Emoji timeline 🟢 95.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic172_emoji_timeline_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic172_emoji_timeline_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic173</b></td>
  <td>African languages 🟢 90.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic173_african_languages_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic173_african_languages_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic174</b></td>
  <td>Technical symbols 🟢 94.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic174_technical_symbols_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic174_technical_symbols_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic175</b></td>
  <td>Multiscript catalog 🟢 92.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic175_multiscript_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic175_multiscript_catalog_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic176</b></td>
  <td>Combining characters 🟢 94.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic176_combining_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic176_combining_characters_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic177</b></td>
  <td>Emoji calendar 🟡 88.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic177_emoji_calendar_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic177_emoji_calendar_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic178</b></td>
  <td>Caucasus ethiopic 🟢 96.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic178_caucasus_ethiopic_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic178_caucasus_ethiopic_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic179</b></td>
  <td>Emoji inventory 🟢 91.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic179_emoji_inventory_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic179_emoji_inventory_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic180</b></td>
  <td>Polyglot paragraph 🟢 93.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic180_polyglot_paragraph_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic180_polyglot_paragraph_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic181</b></td>
  <td>Feedback tracker with images 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic181_feedback_tracker_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic181_feedback_tracker_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic182</b></td>
  <td>Dense long text columns 🟢 96.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic182_dense_long_text_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic182_dense_long_text_columns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic183</b></td>
  <td>Mixed content grid 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic183_mixed_content_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic183_mixed_content_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic184</b></td>
  <td>Wide narrow columns 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic184_wide_narrow_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic184_wide_narrow_columns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic185</b></td>
  <td>Tall rows vertical align 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic185_tall_rows_vertical_align_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic185_tall_rows_vertical_align_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic186</b></td>
  <td>Multi sheet image report 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic186_multi_sheet_image_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic186_multi_sheet_image_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic187</b></td>
  <td>Bug report with screenshots 🟢 98.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic187_bug_report_with_screenshots_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic187_bug_report_with_screenshots_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic188</b></td>
  <td>Merged header with images 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic188_merged_header_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic188_merged_header_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic189</b></td>
  <td>Alternating image text rows 🟢 95.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic189_alternating_image_text_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic189_alternating_image_text_rows_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic190</b></td>
  <td>Dashboard kpi images 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic190_dashboard_kpi_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic190_dashboard_kpi_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic191</b></td>
  <td>Payroll calculator 🟡 89.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic191_payroll_calculator_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic191_payroll_calculator_p1_reference.png" width="320"/></td>
</tr>
</table>

<!-- DOCX_BENCHMARK_START -->

### DOCX 벤치마크

MiniPdf DOCX 출력은 LibreOffice를 참조 렌더러로 사용하여 **60개 클래식 테스트 케이스**에서 비교됩니다.

| 카테고리 | 수량 | 임계값 |
|---|---|---|
| 🟢 우수 | 175 | ≥ 90% |
| 🟡 허용 | 16 | 70% – 90% |
| 🔴 개선 필요 | 0 | < 70% |

**전체 평균 점수: 96.9%** (텍스트 유사도 40% + 시각 유사도 40% + 페이지 수 20%)

#### DOCX 시각적 비교

모든 DOCX 테스트 케이스에서 MiniPdf 출력과 LibreOffice 참조를 비교합니다. 여러 페이지인 경우 1페이지만 표시합니다.

<table>
<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>
<tr>
  <td><b>classic01</b></td>
  <td>Basic table with headers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic01_single_paragraph_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic01_single_paragraph_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic02</b></td>
  <td>Multiple worksheets 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic02_multiple_paragraphs_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic02_multiple_paragraphs_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic03</b></td>
  <td>Empty workbook 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic03_headings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic03_headings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic04</b></td>
  <td>Single cell 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic04_bold_italic_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic04_bold_italic_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic05</b></td>
  <td>Wide table 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic05_font_sizes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic05_font_sizes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic06</b></td>
  <td>Tall table 🟢 97.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic06_font_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic06_font_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic07</b></td>
  <td>Numbers only 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic07_alignment_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic07_alignment_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic08</b></td>
  <td>Mixed text and numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic08_bullet_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic08_bullet_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic09</b></td>
  <td>Long text 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic09_numbered_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic09_numbered_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic10</b></td>
  <td>Special xml characters 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic10_simple_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic10_simple_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic11</b></td>
  <td>Sparse rows 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic11_table_with_shading_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic11_table_with_shading_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic12</b></td>
  <td>Sparse columns 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic12_merged_cells_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic12_merged_cells_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic13</b></td>
  <td>Date strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic13_long_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic13_long_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic14</b></td>
  <td>Decimal numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic14_mixed_content_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic14_mixed_content_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic15</b></td>
  <td>Negative numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic15_indentation_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic15_indentation_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic16</b></td>
  <td>Percentage strings 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic16_line_spacing_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic16_line_spacing_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic17</b></td>
  <td>Currency strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic17_page_break_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic17_page_break_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic18</b></td>
  <td>Large dataset 🟢 95.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic18_embedded_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic18_embedded_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic19</b></td>
  <td>Single column list 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic19_multiple_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic19_multiple_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic20</b></td>
  <td>All empty cells 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic20_table_with_many_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic20_table_with_many_rows_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic21</b></td>
  <td>Header only 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic21_nested_lists_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic21_nested_lists_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic22</b></td>
  <td>Long sheet name 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic22_horizontal_rule_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic22_horizontal_rule_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic23</b></td>
  <td>Unicode text 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic23_mixed_formatting_runs_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic23_mixed_formatting_runs_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic24</b></td>
  <td>Red text 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic24_two_column_table_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic24_two_column_table_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic25</b></td>
  <td>Multiple colors 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic25_title_and_subtitle_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic25_title_and_subtitle_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic26</b></td>
  <td>Inline strings 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic26_table_alignment_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic26_table_alignment_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic27</b></td>
  <td>Single row 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic27_long_paragraph_wrapping_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic27_long_paragraph_wrapping_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic28</b></td>
  <td>Duplicate values 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic28_special_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic28_special_characters_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic29</b></td>
  <td>Formula results 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic29_table_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic29_table_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic30</b></td>
  <td>Mixed empty and filled sheets 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic30_comprehensive_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic30_comprehensive_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic31</b></td>
  <td>Bold header row 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic31_product_card_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic31_product_card_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic31</b></td>
  <td>Bold header row 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic31_strikethrough_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic31_strikethrough_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic32</b></td>
  <td>Right aligned numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic32_company_logo_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic32_company_logo_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic32</b></td>
  <td>Right aligned numbers 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic32_superscript_subscript_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic32_superscript_subscript_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic33</b></td>
  <td>Centered text 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic33_highlighted_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic33_highlighted_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic33</b></td>
  <td>Centered text 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic33_two_products_side_by_side_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic33_two_products_side_by_side_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic34</b></td>
  <td>Explicit column widths 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic34_employee_directory_with_photo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic34_employee_directory_with_photo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic34</b></td>
  <td>Explicit column widths 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic34_paragraph_borders_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic34_paragraph_borders_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic35</b></td>
  <td>Explicit row heights 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic35_inventory_with_product_photos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic35_inventory_with_product_photos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic35</b></td>
  <td>Explicit row heights 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic35_tab_stops_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic35_tab_stops_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic36</b></td>
  <td>Merged cells 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic36_invoice_with_logo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic36_invoice_with_logo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic36</b></td>
  <td>Merged cells 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic36_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic36_wide_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic37</b></td>
  <td>Freeze panes 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic37_nested_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic37_nested_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic37</b></td>
  <td>Freeze panes 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic37_real_estate_listing_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic37_real_estate_listing_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic38</b></td>
  <td>Hyperlink cell 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic38_restaurant_menu_with_photos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic38_restaurant_menu_with_photos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic38</b></td>
  <td>Hyperlink cell 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic38_table_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic38_table_column_widths_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic39</b></td>
  <td>Financial table 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic39_cover_page_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic39_cover_page_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic39</b></td>
  <td>Financial table 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic39_financial_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic39_financial_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic40</b></td>
  <td>Scientific notation 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic40_product_catalog_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic40_product_catalog_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic40</b></td>
  <td>Scientific notation 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic40_resume_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic40_resume_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic41</b></td>
  <td>Integer vs float 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic41_business_letter_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic41_business_letter_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic41</b></td>
  <td>Integer vs float 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic41_newsletter_with_hero_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic41_newsletter_with_hero_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic42</b></td>
  <td>Boolean values 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic42_chart_image_with_data_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic42_chart_image_with_data_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic42</b></td>
  <td>Boolean values 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic42_meeting_minutes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic42_meeting_minutes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic43</b></td>
  <td>Inventory report 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic43_event_flyer_with_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic43_event_flyer_with_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic43</b></td>
  <td>Inventory report 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic43_invoice_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic43_invoice_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic44</b></td>
  <td>Employee roster 🟢 98.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic44_dashboard_with_kpi_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic44_dashboard_with_kpi_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic44</b></td>
  <td>Employee roster 🟢 98.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic44_memo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic44_memo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic45</b></td>
  <td>Sales by region 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic45_certificate_with_seal_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic45_certificate_with_seal_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic45</b></td>
  <td>Sales by region 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic45_project_plan_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic45_project_plan_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic46</b></td>
  <td>Grade book 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic46_comparison_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic46_comparison_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic46</b></td>
  <td>Grade book 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic46_product_image_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic46_product_image_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic47</b></td>
  <td>Time series 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic47_data_dictionary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic47_data_dictionary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic47</b></td>
  <td>Time series 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic47_news_article_with_hero_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic47_news_article_with_hero_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic48</b></td>
  <td>Survey results 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic48_multi_level_headings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic48_multi_level_headings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic48</b></td>
  <td>Survey results 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic48_task_list_with_status_icons_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic48_task_list_with_status_icons_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic49</b></td>
  <td>Contact list 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic49_cjk_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic49_cjk_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic49</b></td>
  <td>Contact list 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic49_wide_panoramic_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic49_wide_panoramic_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic50</b></td>
  <td>Budget vs actuals 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic50_long_table_with_formatting_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic50_long_table_with_formatting_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic50</b></td>
  <td>Budget vs actuals 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic50_portrait_tall_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic50_portrait_tall_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic51</b></td>
  <td>Product catalog 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic51_step_by_step_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic51_step_by_step_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic51</b></td>
  <td>Product catalog 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic51_underline_styles_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic51_underline_styles_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic52</b></td>
  <td>Pivot summary 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic52_before_after_comparison_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic52_before_after_comparison_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic52</b></td>
  <td>Pivot summary 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic52_spacing_before_after_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic52_spacing_before_after_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic53</b></td>
  <td>Invoice 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic53_color_swatch_palette_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic53_color_swatch_palette_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic53</b></td>
  <td>Invoice 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic53_table_merged_complex_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic53_table_merged_complex_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic54</b></td>
  <td>Multi level header 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic54_multi_font_family_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic54_multi_font_family_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic54</b></td>
  <td>Multi level header 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic54_travel_destination_cards_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic54_travel_destination_cards_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic55</b></td>
  <td>Error values 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic55_background_shading_paragraph_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic55_background_shading_paragraph_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic55</b></td>
  <td>Error values 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic55_lab_results_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic55_lab_results_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic56</b></td>
  <td>Alternating row colors 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic56_images_and_tables_mixed_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic56_images_and_tables_mixed_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic56</b></td>
  <td>Alternating row colors 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic56_software_screenshot_features_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic56_software_screenshot_features_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic57</b></td>
  <td>Cjk only 🟢 91.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic57_right_to_left_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic57_right_to_left_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic57</b></td>
  <td>Cjk only 🟢 91.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic57_sports_results_with_logos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic57_sports_results_with_logos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic58</b></td>
  <td>Mixed numeric formats 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic58_dense_paragraph_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic58_dense_paragraph_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic58</b></td>
  <td>Mixed numeric formats 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic58_report_with_footer_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic58_report_with_footer_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic59</b></td>
  <td>Multi sheet summary 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic59_numbered_and_bullet_mixed_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic59_numbered_and_bullet_mixed_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic59</b></td>
  <td>Multi sheet summary 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic59_nutrition_label_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic59_nutrition_label_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic60</b></td>
  <td>Large wide table 🟢 97.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic60_comprehensive_styled_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic60_comprehensive_styled_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic60</b></td>
  <td>Large wide table 🟢 97.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic60_project_status_with_milestones_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic60_project_status_with_milestones_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic61</b></td>
  <td>Product card with image 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic61_header_and_footer_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic61_header_and_footer_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic62</b></td>
  <td>Company logo header 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic62_footnote_references_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic62_footnote_references_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic63</b></td>
  <td>Two products side by side 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic63_toc_style_headings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic63_toc_style_headings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic64</b></td>
  <td>Employee directory with photo 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic64_multi_column_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic64_multi_column_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic65</b></td>
  <td>Inventory with product photos 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic65_code_block_styling_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic65_code_block_styling_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic66</b></td>
  <td>Invoice with logo 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic66_colored_title_page_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic66_colored_title_page_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic67</b></td>
  <td>Real estate listing 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic67_alternating_row_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic67_alternating_row_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic68</b></td>
  <td>Restaurant menu 🟢 98.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic68_sidebar_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic68_sidebar_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic69</b></td>
  <td>Image only sheet 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic69_blockquote_styling_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic69_blockquote_styling_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic70</b></td>
  <td>Product catalog with images 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic70_academic_paper_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic70_academic_paper_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic71</b></td>
  <td>Multi sheet with images 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic71_legal_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic71_legal_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic72</b></td>
  <td>Bar chart image with data 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic72_technical_specification_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic72_technical_specification_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic73</b></td>
  <td>Event flyer with banner 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic73_calendar_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic73_calendar_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic74</b></td>
  <td>Dashboard with kpi image 🟢 97.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic74_org_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic74_org_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic75</b></td>
  <td>Certificate with seal 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic75_newsletter_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic75_newsletter_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic76</b></td>
  <td>Product image grid 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic76_recipe_card_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic76_recipe_card_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic77</b></td>
  <td>News article with hero image 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic77_timeline_layout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic77_timeline_layout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic78</b></td>
  <td>Small icon per row 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic78_faq_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic78_faq_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic79</b></td>
  <td>Wide panoramic banner 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic79_glossary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic79_glossary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic80</b></td>
  <td>Portrait tall image 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic80_matrix_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic80_matrix_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic81</b></td>
  <td>Step by step with images 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic81_budget_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic81_budget_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic82</b></td>
  <td>Before after images 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic82_survey_questionnaire_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic82_survey_questionnaire_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic83</b></td>
  <td>Color swatch palette 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic83_medical_form_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic83_medical_form_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic84</b></td>
  <td>Travel destination cards 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic84_shipping_label_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic84_shipping_label_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic85</b></td>
  <td>Lab results with image 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic85_report_card_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic85_report_card_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic86</b></td>
  <td>Software screenshot features 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic86_checklist_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic86_checklist_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic87</b></td>
  <td>Sports results with logos 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic87_bibliography_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic87_bibliography_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic88</b></td>
  <td>Image after data 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic88_presentation_handout_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic88_presentation_handout_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic89</b></td>
  <td>Nutrition label with image 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic89_multi_image_gallery_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic89_multi_image_gallery_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic90</b></td>
  <td>Project status with milestones 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic90_comprehensive_annual_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic90_comprehensive_annual_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic91</b></td>
  <td>Simple bar chart 🟢 96.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic91_landscape_page_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic91_landscape_page_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic92</b></td>
  <td>Horizontal bar chart 🟢 96.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic92_first_line_indent_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic92_first_line_indent_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic93</b></td>
  <td>Line chart 🟢 93.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic93_hanging_indent_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic93_hanging_indent_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic94</b></td>
  <td>Pie chart 🟢 97.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic94_custom_bullet_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic94_custom_bullet_characters_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic95</b></td>
  <td>Area chart 🟡 76.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic95_contract_template_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic95_contract_template_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic96</b></td>
  <td>Scatter chart 🟢 94.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic96_dense_data_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic96_dense_data_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic97</b></td>
  <td>Doughnut chart 🟢 97.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic97_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic97_product_catalog_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic98</b></td>
  <td>Radar chart 🟢 95.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic98_training_manual_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic98_training_manual_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic99</b></td>
  <td>Bubble chart 🟢 92.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic99_policy_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic99_policy_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic100</b></td>
  <td>Stacked bar chart 🟢 95.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic100_multi_page_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic100_multi_page_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic101</b></td>
  <td>Percent stacked bar 🟢 93.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic101_warranty_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic101_warranty_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic102</b></td>
  <td>Line chart with markers 🟢 93.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic102_curriculum_syllabus_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic102_curriculum_syllabus_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic103</b></td>
  <td>Pie chart with labels 🟡 86.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic103_event_program_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic103_event_program_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic104</b></td>
  <td>Combo bar line chart 🟡 81.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic104_sop_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic104_sop_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic105</b></td>
  <td>3d bar chart 🟡 85.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic105_certificate_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic105_certificate_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic106</b></td>
  <td>3d pie chart 🟢 96.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic106_multi_section_orientation_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic106_multi_section_orientation_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic107</b></td>
  <td>Multi series line 🟡 80.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic107_order_form_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic107_order_form_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic108</b></td>
  <td>Stacked area chart 🟢 94.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic108_comparison_matrix_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic108_comparison_matrix_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic109</b></td>
  <td>Scatter with trendline 🟢 92.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic109_release_notes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic109_release_notes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic110</b></td>
  <td>Chart with legend 🟡 84.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic110_troubleshooting_guide_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic110_troubleshooting_guide_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic111</b></td>
  <td>Chart with axis labels 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic111_meeting_agenda_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic111_meeting_agenda_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic112</b></td>
  <td>Multiple charts 🟡 85.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic112_project_status_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic112_project_status_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic113</b></td>
  <td>Chart sheet 🟡 86.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic113_address_labels_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic113_address_labels_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic114</b></td>
  <td>Chart large dataset 🟢 91.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic114_test_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic114_test_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic115</b></td>
  <td>Chart negative values 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic115_price_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic115_price_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic116</b></td>
  <td>Percent stacked area 🟢 93.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic116_risk_assessment_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic116_risk_assessment_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic117</b></td>
  <td>Stock ohlc chart 🟡 80.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic117_employee_handbook_excerpt_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic117_employee_handbook_excerpt_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic118</b></td>
  <td>Bar chart custom colors 🟢 95.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic118_data_report_with_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic118_data_report_with_summary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic119</b></td>
  <td>Dashboard multi charts 🟢 92.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic119_multi_language_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic119_multi_language_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic120</b></td>
  <td>Chart with date axis 🟡 76.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic120_comprehensive_business_proposal_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic120_comprehensive_business_proposal_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic121</b></td>
  <td>Thin borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic121_thin_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic121_thin_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic122</b></td>
  <td>Thick outer thin inner 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic122_thick_outer_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic122_thick_outer_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic123</b></td>
  <td>Dashed borders 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic123_dashed_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic123_dashed_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic124</b></td>
  <td>Colored borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic124_colored_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic124_colored_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic125</b></td>
  <td>Solid fills 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic125_solid_cell_fills_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic125_solid_cell_fills_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic126</b></td>
  <td>Dark header 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic126_dark_header_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic126_dark_header_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic127</b></td>
  <td>Font styles 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic127_font_styles_showcase_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic127_font_styles_showcase_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic128</b></td>
  <td>Font sizes 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic128_font_sizes_showcase_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic128_font_sizes_showcase_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic129</b></td>
  <td>Alignment combos 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic129_alignment_combinations_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic129_alignment_combinations_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic130</b></td>
  <td>Wrap and indent 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic130_wrap_and_indent_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic130_wrap_and_indent_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic131</b></td>
  <td>Number formats 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic131_number_format_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic131_number_format_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic132</b></td>
  <td>Striped table 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic132_striped_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic132_striped_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic133</b></td>
  <td>Gradient rows 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic133_gradient_rows_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic133_gradient_rows_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic134</b></td>
  <td>Heatmap 🟢 98.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic134_heatmap_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic134_heatmap_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic135</b></td>
  <td>Bottom border only 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic135_bottom_border_paragraphs_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic135_bottom_border_paragraphs_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic136</b></td>
  <td>Financial report styled 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic136_financial_statement_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic136_financial_statement_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic137</b></td>
  <td>Checkerboard 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic137_checkerboard_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic137_checkerboard_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic138</b></td>
  <td>Color grid 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic138_color_grid_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic138_color_grid_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic139</b></td>
  <td>Pattern fills 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic139_paragraph_shading_patterns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic139_paragraph_shading_patterns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic140</b></td>
  <td>Rotated text 🟢 98.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic140_rotated_text_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic140_rotated_text_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic141</b></td>
  <td>Mixed edge borders 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic141_mixed_border_styles_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic141_mixed_border_styles_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic142</b></td>
  <td>Styled invoice 🟢 98.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic142_styled_invoice_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic142_styled_invoice_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic143</b></td>
  <td>Colored tabs 🟢 100.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic143_multi_section_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic143_multi_section_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic144</b></td>
  <td>Note style cells 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic144_note_style_paragraphs_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic144_note_style_paragraphs_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic145</b></td>
  <td>Status badges 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic145_status_badge_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic145_status_badge_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic146</b></td>
  <td>Double border table 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic146_double_border_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic146_double_border_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic147</b></td>
  <td>Multi sheet styled 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic147_multi_section_styled_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic147_multi_section_styled_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic148</b></td>
  <td>Frozen styled grid 🟢 97.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic148_data_grid_document_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic148_data_grid_document_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic149</b></td>
  <td>Merged styled sections 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic149_merged_section_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic149_merged_section_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic150</b></td>
  <td>Kitchen sink styles 🟢 97.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic150_kitchen_sink_styles_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports_docx/images/docx_classic150_kitchen_sink_styles_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic181</b></td>
  <td>Feedback tracker with images 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic181_feedback_tracker_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic181_feedback_tracker_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic182</b></td>
  <td>Dense long text columns 🟢 96.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic182_dense_long_text_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic182_dense_long_text_columns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic183</b></td>
  <td>Mixed content grid 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic183_mixed_content_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic183_mixed_content_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic184</b></td>
  <td>Wide narrow columns 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic184_wide_narrow_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic184_wide_narrow_columns_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic185</b></td>
  <td>Tall rows vertical align 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic185_tall_rows_vertical_align_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic185_tall_rows_vertical_align_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic186</b></td>
  <td>Multi sheet image report 🟢 99.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic186_multi_sheet_image_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic186_multi_sheet_image_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic187</b></td>
  <td>Bug report with screenshots 🟢 98.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic187_bug_report_with_screenshots_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic187_bug_report_with_screenshots_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic188</b></td>
  <td>Merged header with images 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic188_merged_header_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic188_merged_header_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic189</b></td>
  <td>Alternating image text rows 🟢 95.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic189_alternating_image_text_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic189_alternating_image_text_rows_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic190</b></td>
  <td>Dashboard kpi images 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic190_dashboard_kpi_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic190_dashboard_kpi_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic191</b></td>
  <td>Payroll calculator 🟡 89.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic191_payroll_calculator_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic191_payroll_calculator_p1_reference.png" width="320"/></td>
</tr>
</table>

<!-- DOCX_BENCHMARK_END -->

















## 라이선스

이 프로젝트는 [Apache License 2.0](../LICENSE)에 따라 라이선스가 부여됩니다.
