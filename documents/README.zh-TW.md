# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet 下載次數"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/badge/.NET-%3E%3D%209.0-red.svg" alt=".NET 9.0+"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="授權條款"></a>
</p>
<p>
<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | 繁體中文 | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>
</p>
</div>

---

一個極簡、零相依性的 .NET 函式庫，用於將 Office 檔案轉換為 PDF。

## 功能特色

- **Excel 轉 PDF** — 將 `.xlsx` 檔案轉換為分頁 PDF，自動排版欄位
- **嵌入圖片** — Excel 工作表中嵌入的 JPEG 和 PNG 圖片會渲染到 PDF 輸出中
- **零相依性** — 僅使用 .NET 內建 API（不需任何外部套件）
- **標準 PDF 1.4** 格式輸出
- **Word 轉 PDF** — 開發中
- **圖表** — 目前支援尚不完善

## 快速開始

### 透過 NuGet 安裝

```bash
dotnet add package MiniPdf
```

## 使用方式

```csharp
using MiniSoftware;

// 檔案轉檔案
MiniPdf.ConvertToPdf("data.xlsx", "data.pdf");

// 檔案轉位元組陣列
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// 串流轉位元組陣列
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## 基準測試

MiniPdf 的輸出結果以 LibreOffice 作為參考渲染器，針對 **120 個經典測試案例**（包括 30 個圖片嵌入案例和 30 個圖表案例）進行比較。

| 類別 | 數量 | 門檻值 |
|---|---|---|
| 🟢 優秀 | 97 | ≥ 90% |
| 🟡 可接受 | 21 | 70% – 90% |
| 🔴 需要改進 | 2 | < 70% |

**整體平均分數：94.7%** （文字相似度 40% + 視覺相似度 40% + 頁數 20%）

### 視覺比較

以下為全部測試案例的比較，MiniPdf 輸出 vs LibreOffice 參考結果。多頁結果僅顯示第 1 頁。

<table>
<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>
<tr>
  <td><b>classic01</b></td>
  <td>Basic table with headers 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic02</b></td>
  <td>Multiple worksheets 🟢 99.7%</td>
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
  <td>Wide table 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic06</b></td>
  <td>Tall table 🟢 97.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic07</b></td>
  <td>Numbers only 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic08</b></td>
  <td>Mixed text and numbers 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic09</b></td>
  <td>Long text 🔴 50.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic10</b></td>
  <td>Special xml characters 🟢 99.8%</td>
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
  <td>Date strings 🟢 97.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic14</b></td>
  <td>Decimal numbers 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic15</b></td>
  <td>Negative numbers 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic16</b></td>
  <td>Percentage strings 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic17</b></td>
  <td>Currency strings 🟢 98.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic18</b></td>
  <td>Large dataset 🟢 94.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic19</b></td>
  <td>Single column list 🟢 99.8%</td>
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
  <td>Header only 🟢 100.0%</td>
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
  <td>Unicode text 🟢 91.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic24</b></td>
  <td>Red text 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic25</b></td>
  <td>Multiple colors 🟢 99.5%</td>
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
  <td>Duplicate values 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic29</b></td>
  <td>Formula results 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic30</b></td>
  <td>Mixed empty and filled sheets 🟢 99.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic31</b></td>
  <td>Bold header row 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic32</b></td>
  <td>Right aligned numbers 🟢 99.8%</td>
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
  <td>Explicit column widths 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic35</b></td>
  <td>Explicit row heights 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic36</b></td>
  <td>Merged cells 🟢 98.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic37</b></td>
  <td>Freeze panes 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic38</b></td>
  <td>Hyperlink cell 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic39</b></td>
  <td>Financial table 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic40</b></td>
  <td>Scientific notation 🟢 95.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic41</b></td>
  <td>Integer vs float 🟢 97.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic42</b></td>
  <td>Boolean values 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic43</b></td>
  <td>Inventory report 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic44</b></td>
  <td>Employee roster 🟢 97.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic45</b></td>
  <td>Sales by region 🟢 99.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic46</b></td>
  <td>Grade book 🟢 99.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic47</b></td>
  <td>Time series 🟢 98.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic48</b></td>
  <td>Survey results 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic49</b></td>
  <td>Contact list 🟢 95.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic50</b></td>
  <td>Budget vs actuals 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic51</b></td>
  <td>Product catalog 🟢 98.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic52</b></td>
  <td>Pivot summary 🟢 99.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic53</b></td>
  <td>Invoice 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic54</b></td>
  <td>Multi level header 🟢 99.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic55</b></td>
  <td>Error values 🟢 99.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic56</b></td>
  <td>Alternating row colors 🟡 88.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic57</b></td>
  <td>Cjk only 🟡 85.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic58</b></td>
  <td>Mixed numeric formats 🟢 95.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic59</b></td>
  <td>Multi sheet summary 🟢 99.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic60</b></td>
  <td>Large wide table 🟢 96.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic61</b></td>
  <td>Product card with image 🟢 98.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic61_product_card_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic61_product_card_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic62</b></td>
  <td>Company logo header 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic62_company_logo_header_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic62_company_logo_header_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic63</b></td>
  <td>Two products side by side 🟢 98.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic63_two_products_side_by_side_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic63_two_products_side_by_side_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic64</b></td>
  <td>Employee directory with photo 🟢 98.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic64_employee_directory_with_photo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic64_employee_directory_with_photo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic65</b></td>
  <td>Inventory with product photos 🟢 97.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic65_inventory_with_product_photos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic65_inventory_with_product_photos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic66</b></td>
  <td>Invoice with logo 🟢 98.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic66_invoice_with_logo_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic66_invoice_with_logo_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic67</b></td>
  <td>Real estate listing 🟢 98.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic67_real_estate_listing_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic67_real_estate_listing_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic68</b></td>
  <td>Restaurant menu 🟢 96.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic68_restaurant_menu_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic68_restaurant_menu_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic69</b></td>
  <td>Image only sheet 🟢 97.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic69_image_only_sheet_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic69_image_only_sheet_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic70</b></td>
  <td>Product catalog with images 🟢 96.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic70_product_catalog_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic70_product_catalog_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic71</b></td>
  <td>Multi sheet with images 🟢 99.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic71_multi_sheet_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic71_multi_sheet_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic72</b></td>
  <td>Bar chart image with data 🟢 97.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic72_bar_chart_image_with_data_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic72_bar_chart_image_with_data_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic73</b></td>
  <td>Event flyer with banner 🟢 96.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic73_event_flyer_with_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic73_event_flyer_with_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic74</b></td>
  <td>Dashboard with kpi image 🟢 95.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic74_dashboard_with_kpi_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic75</b></td>
  <td>Certificate with seal 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic75_certificate_with_seal_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic75_certificate_with_seal_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic76</b></td>
  <td>Product image grid 🟢 97.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic76_product_image_grid_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic76_product_image_grid_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic77</b></td>
  <td>News article with hero image 🟢 96.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic77_news_article_with_hero_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic77_news_article_with_hero_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic78</b></td>
  <td>Small icon per row 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic78_small_icon_per_row_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic78_small_icon_per_row_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic79</b></td>
  <td>Wide panoramic banner 🟢 96.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic79_wide_panoramic_banner_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic79_wide_panoramic_banner_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic80</b></td>
  <td>Portrait tall image 🟢 98.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic80_portrait_tall_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic80_portrait_tall_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic81</b></td>
  <td>Step by step with images 🟢 97.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic81_step_by_step_with_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic81_step_by_step_with_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic82</b></td>
  <td>Before after images 🟢 96.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic82_before_after_images_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic82_before_after_images_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic83</b></td>
  <td>Color swatch palette 🟢 97.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic83_color_swatch_palette_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic83_color_swatch_palette_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic84</b></td>
  <td>Travel destination cards 🟢 96.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic84_travel_destination_cards_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic84_travel_destination_cards_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic85</b></td>
  <td>Lab results with image 🟢 91.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic85_lab_results_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic85_lab_results_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic86</b></td>
  <td>Software screenshot features 🟢 97.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic86_software_screenshot_features_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic86_software_screenshot_features_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic87</b></td>
  <td>Sports results with logos 🟢 99.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic87_sports_results_with_logos_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic87_sports_results_with_logos_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic88</b></td>
  <td>Image after data 🟢 98.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic88_image_after_data_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic88_image_after_data_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic89</b></td>
  <td>Nutrition label with image 🟢 98.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic89_nutrition_label_with_image_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic89_nutrition_label_with_image_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic90</b></td>
  <td>Project status with milestones 🟢 94.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic90_project_status_with_milestones_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic90_project_status_with_milestones_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic91</b></td>
  <td>Simple bar chart 🟢 96.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic91_simple_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic91_simple_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic92</b></td>
  <td>Horizontal bar chart 🟡 83.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic92_horizontal_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic92_horizontal_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic93</b></td>
  <td>Line chart 🟢 92.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic93_line_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic93_line_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic94</b></td>
  <td>Pie chart 🟡 89.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic94_pie_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic94_pie_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic95</b></td>
  <td>Area chart 🟡 73.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic95_area_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic95_area_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic96</b></td>
  <td>Scatter chart 🟡 87.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic96_scatter_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic96_scatter_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic97</b></td>
  <td>Doughnut chart 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic97_doughnut_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic97_doughnut_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic98</b></td>
  <td>Radar chart 🟡 89.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic98_radar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic98_radar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic99</b></td>
  <td>Bubble chart 🟡 83.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic99_bubble_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic99_bubble_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic100</b></td>
  <td>Stacked bar chart 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic100_stacked_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic100_stacked_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic101</b></td>
  <td>Percent stacked bar 🟡 87.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic101_percent_stacked_bar_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic101_percent_stacked_bar_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic102</b></td>
  <td>Line chart with markers 🟢 91.4%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic102_line_chart_with_markers_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic102_line_chart_with_markers_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic103</b></td>
  <td>Pie chart with labels 🟡 82.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic103_pie_chart_with_labels_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic103_pie_chart_with_labels_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic104</b></td>
  <td>Combo bar line chart 🟡 81.5%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic104_combo_bar_line_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic104_combo_bar_line_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic105</b></td>
  <td>3d bar chart 🟡 84.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic105_3d_bar_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic105_3d_bar_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic106</b></td>
  <td>3d pie chart 🟢 91.3%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic106_3d_pie_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic106_3d_pie_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic107</b></td>
  <td>Multi series line 🟡 80.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic107_multi_series_line_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic107_multi_series_line_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic108</b></td>
  <td>Stacked area chart 🟡 89.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic108_stacked_area_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic108_stacked_area_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic109</b></td>
  <td>Scatter with trendline 🟡 82.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic109_scatter_with_trendline_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic109_scatter_with_trendline_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic110</b></td>
  <td>Chart with legend 🟡 82.9%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic110_chart_with_legend_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic110_chart_with_legend_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic111</b></td>
  <td>Chart with axis labels 🟡 83.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic111_chart_with_axis_labels_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic111_chart_with_axis_labels_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic112</b></td>
  <td>Multiple charts 🟡 84.7%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic112_multiple_charts_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic112_multiple_charts_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic113</b></td>
  <td>Chart sheet 🟡 81.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic113_chart_sheet_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic113_chart_sheet_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic114</b></td>
  <td>Chart large dataset 🟢 91.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic114_chart_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic114_chart_large_dataset_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic115</b></td>
  <td>Chart negative values 🟢 93.8%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic115_chart_negative_values_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic115_chart_negative_values_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic116</b></td>
  <td>Percent stacked area 🟡 86.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic116_percent_stacked_area_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic116_percent_stacked_area_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic117</b></td>
  <td>Stock ohlc chart 🟡 80.0%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic117_stock_ohlc_chart_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic117_stock_ohlc_chart_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic118</b></td>
  <td>Bar chart custom colors 🟢 94.6%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic118_bar_chart_custom_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic118_bar_chart_custom_colors_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic119</b></td>
  <td>Dashboard multi charts 🟢 92.2%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic119_dashboard_multi_charts_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic119_dashboard_multi_charts_p1_reference.png" width="320"/></td>
</tr>
<tr>
  <td><b>classic120</b></td>
  <td>Chart with date axis 🔴 64.1%</td>
</tr>
<tr>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic120_chart_with_date_axis_p1_minipdf.png" width="320"/></td>
  <td><img src="../tests/MiniPdf.Benchmark/reports/images/classic120_chart_with_date_axis_p1_reference.png" width="320"/></td>
</tr>
</table>








## 授權條款

本專案基於 [Apache License 2.0](../LICENSE) 授權。
