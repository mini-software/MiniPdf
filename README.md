# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet Downloads"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/badge/.NET-%3E%3D%209.0-red.svg" alt=".NET 9.0+"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>
<p>
English | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>
</p>
</div>

---

A minimal, zero-dependency .NET library for converting Excel (.xlsx) files to PDF.

## Features

- **Excel-to-PDF** — Convert `.xlsx` files to paginated PDF with automatic column layout
- **Zero dependencies** — Uses only built-in .NET APIs (no external packages)
- **Valid PDF 1.4** output
- **Word-to-PDF** — In development
- **Chart** — Not currently supported

## Getting Started

### Install via NuGet

```bash
dotnet add package MiniPdf
```

### Requirements

- .NET 9.0 or later

## Usage

```csharp
using MiniPdf;

// File to file
MiniPdf.ConvertToPdf("data.xlsx", "data.pdf");

// File to byte array
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// Stream to byte array
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## Benchmark

MiniPdf output is compared against LibreOffice as the reference renderer across **60 classic test cases**.

| Category | Count | Threshold |
|---|---|---|
| 🟢 Excellent | 54 | ≥ 90% |
| 🟡 Acceptable | 5 | 70% – 90% |
| 🔴 Needs Improvement | 1 | < 70% |

**Average overall score: 96.2%** (text similarity 40% + visual similarity 40% + page count 20%)

### Visual Comparison

All 60 test cases comparing MiniPdf output (left) vs LibreOffice reference (right). Page 1 shown for multi-page results.

<table>
<tr><th>Test Case</th><th>MiniPdf</th><th>LibreOffice (Reference)</th><th>Score</th></tr>
<tr>
  <td><b>classic01</b><br/>Basic table with headers</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic02</b><br/>Multiple worksheets</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic03</b><br/>Empty workbook</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic04</b><br/>Single cell</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic05</b><br/>Wide table</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic06</b><br/>Tall table</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
  <td>🟢 95.1%</td>
</tr>
<tr>
  <td><b>classic07</b><br/>Numbers only</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic08</b><br/>Mixed text and numbers</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic09</b><br/>Long text<br/><i>MiniPdf: 1 page<br/>Reference: 12 pages</i></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
  <td>🔴 20.3%</td>
</tr>
<tr>
  <td><b>classic10</b><br/>Special XML characters</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic11</b><br/>Sparse rows</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic12</b><br/>Sparse columns</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic13</b><br/>Date strings</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic14</b><br/>Decimal numbers</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic15</b><br/>Negative numbers</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic16</b><br/>Percentage strings</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic17</b><br/>Currency strings</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.0%</td>
</tr>
<tr>
  <td><b>classic18</b><br/>Large dataset</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
  <td>🟢 91.5%</td>
</tr>
<tr>
  <td><b>classic19</b><br/>Single column list</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic20</b><br/>All empty cells</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic21</b><br/>Header only</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic22</b><br/>Long sheet name</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic23</b><br/>Unicode text</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
  <td>🟡 86.1%</td>
</tr>
<tr>
  <td><b>classic24</b><br/>Red text</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic25</b><br/>Multiple colors</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic26</b><br/>Inline strings</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic27</b><br/>Single row</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic28</b><br/>Duplicate values</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic29</b><br/>Formula results</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic30</b><br/>Mixed empty and filled sheets</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic31</b><br/>Bold header row</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic32</b><br/>Right-aligned numbers</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic33</b><br/>Centered text</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic34</b><br/>Explicit column widths</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic35</b><br/>Explicit row heights</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
  <td>🟢 96.8%</td>
</tr>
<tr>
  <td><b>classic36</b><br/>Merged cells</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
  <td>🟢 97.8%</td>
</tr>
<tr>
  <td><b>classic37</b><br/>Freeze panes</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic38</b><br/>Hyperlink cell</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic39</b><br/>Financial table</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic40</b><br/>Scientific notation</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
  <td>🟢 94.6%</td>
</tr>
<tr>
  <td><b>classic41</b><br/>Integer vs float</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
  <td>🟢 97.6%</td>
</tr>
<tr>
  <td><b>classic42</b><br/>Boolean values</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
  <td>🟢 92.6%</td>
</tr>
<tr>
  <td><b>classic43</b><br/>Inventory report</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic44</b><br/>Employee roster</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
  <td>🟡 81.6%</td>
</tr>
<tr>
  <td><b>classic45</b><br/>Sales by region (4 sheets)</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic46</b><br/>Grade book</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic47</b><br/>Time series</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
  <td>🟢 98.6%</td>
</tr>
<tr>
  <td><b>classic48</b><br/>Survey results</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic49</b><br/>Contact list</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
  <td>🟡 86.4%</td>
</tr>
<tr>
  <td><b>classic50</b><br/>Budget vs actuals (3 sheets)</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic51</b><br/>Product catalog</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
  <td>🟡 77.2%</td>
</tr>
<tr>
  <td><b>classic52</b><br/>Pivot summary</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.3%</td>
</tr>
<tr>
  <td><b>classic53</b><br/>Invoice layout</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
  <td>🟢 96.0%</td>
</tr>
<tr>
  <td><b>classic54</b><br/>Multi-level header</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic55</b><br/>Error values (#N/A, #DIV/0!)</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic56</b><br/>Alternating row colors</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
  <td>🟢 98.9%</td>
</tr>
<tr>
  <td><b>classic57</b><br/>CJK-only sheet</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
  <td>🟡 84.0%</td>
</tr>
<tr>
  <td><b>classic58</b><br/>Mixed numeric formats</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
  <td>🟢 94.8%</td>
</tr>
<tr>
  <td><b>classic59</b><br/>Multi-sheet summary (4 sheets)</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic60</b><br/>Large wide table (20×50)</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 94.9%</td>
</tr>
</table>

## License

This project is licensed under the [Apache License 2.0](LICENSE).
