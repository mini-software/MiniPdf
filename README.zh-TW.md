# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet 下載次數"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/badge/.NET-%3E%3D%209.0-red.svg" alt=".NET 9.0+"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="授權條款"></a>
</p>
<p>
<a href="README.md">English</a> | 繁體中文
</p>
</div>

---

一個極簡、零相依性的 .NET 函式庫，用於將 Excel (.xlsx) 檔案轉換為 PDF。

> **安全性**：所有 PR 均由 Copilot AI 與 Azure AI 自動進行安全漏洞審查。

## 功能特色

- **Excel 轉 PDF** — 將 `.xlsx` 檔案轉換為分頁 PDF，自動排版欄位
- **零相依性** — 僅使用 .NET 內建 API（不需任何外部套件）
- **標準 PDF 1.4** 格式輸出

## 快速開始

### 透過 NuGet 安裝

```bash
dotnet add package MiniPdf
```

### 系統需求

- .NET 9.0 或更新版本

## 使用方式

```csharp
using MiniPdf;

// 檔案轉檔案
MiniPdf.ConvertToPdf("data.xlsx", "data.pdf");

// 檔案轉位元組陣列
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// 串流轉位元組陣列
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## 基準測試

MiniPdf 的輸出結果以 LibreOffice 作為參考渲染器，針對 **60 個經典測試案例**進行比較。

| 類別 | 數量 | 門檻值 |
|---|---|---|
| 🟢 優秀 | 54 | ≥ 90% |
| 🟡 可接受 | 5 | 70% – 90% |
| 🔴 需要改進 | 1 | < 70% |

**整體平均分數：96.2%**（文字相似度 40% + 視覺相似度 40% + 頁數 20%）

### 視覺比較

以下為全部 60 個測試案例的比較，左側為 MiniPdf 輸出，右側為 LibreOffice 參考結果。多頁結果僅顯示第 1 頁。

<table>
<tr><th>測試案例</th><th>MiniPdf</th><th>LibreOffice（參考）</th><th>分數</th></tr>
<tr>
  <td><b>classic01</b><br/>基本表格含標題</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic02</b><br/>多個工作表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic03</b><br/>空白活頁簿</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic04</b><br/>單一儲存格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic05</b><br/>寬版表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic06</b><br/>長型表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
  <td>🟢 95.1%</td>
</tr>
<tr>
  <td><b>classic07</b><br/>純數字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic08</b><br/>文字與數字混合</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic09</b><br/>長文字<br/><i>MiniPdf：1 頁<br/>參考：12 頁</i></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
  <td>🔴 20.3%</td>
</tr>
<tr>
  <td><b>classic10</b><br/>特殊 XML 字元</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic11</b><br/>稀疏列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic12</b><br/>稀疏欄</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic13</b><br/>日期字串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic14</b><br/>小數數字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic15</b><br/>負數</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic16</b><br/>百分比字串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic17</b><br/>貨幣字串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.0%</td>
</tr>
<tr>
  <td><b>classic18</b><br/>大型資料集</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
  <td>🟢 91.5%</td>
</tr>
<tr>
  <td><b>classic19</b><br/>單欄清單</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic20</b><br/>全空儲存格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic21</b><br/>僅標題列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic22</b><br/>長工作表名稱</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic23</b><br/>Unicode 文字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
  <td>🟡 86.1%</td>
</tr>
<tr>
  <td><b>classic24</b><br/>紅色文字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic25</b><br/>多種顏色</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic26</b><br/>內嵌字串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic27</b><br/>單一列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic28</b><br/>重複值</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic29</b><br/>公式結果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic30</b><br/>空白與填滿工作表混合</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic31</b><br/>粗體標題列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic32</b><br/>數字靠右對齊</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic33</b><br/>置中文字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic34</b><br/>明確欄寬</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic35</b><br/>明確列高</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
  <td>🟢 96.8%</td>
</tr>
<tr>
  <td><b>classic36</b><br/>合併儲存格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
  <td>🟢 97.8%</td>
</tr>
<tr>
  <td><b>classic37</b><br/>凍結窗格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic38</b><br/>超連結儲存格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic39</b><br/>財務表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic40</b><br/>科學記號</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
  <td>🟢 94.6%</td>
</tr>
<tr>
  <td><b>classic41</b><br/>整數與浮點數</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
  <td>🟢 97.6%</td>
</tr>
<tr>
  <td><b>classic42</b><br/>布林值</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
  <td>🟢 92.6%</td>
</tr>
<tr>
  <td><b>classic43</b><br/>庫存報表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic44</b><br/>員工名冊</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
  <td>🟡 81.6%</td>
</tr>
<tr>
  <td><b>classic45</b><br/>依地區銷售（4 個工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic46</b><br/>成績簿</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic47</b><br/>時序資料</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
  <td>🟢 98.6%</td>
</tr>
<tr>
  <td><b>classic48</b><br/>問卷結果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic49</b><br/>聯絡人清單</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
  <td>🟡 86.4%</td>
</tr>
<tr>
  <td><b>classic50</b><br/>預算與實際（3 個工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic51</b><br/>產品目錄</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
  <td>🟡 77.2%</td>
</tr>
<tr>
  <td><b>classic52</b><br/>樞紐摘要</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.3%</td>
</tr>
<tr>
  <td><b>classic53</b><br/>發票版面</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
  <td>🟢 96.0%</td>
</tr>
<tr>
  <td><b>classic54</b><br/>多層標題</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic55</b><br/>錯誤值（#N/A、#DIV/0!）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic56</b><br/>交替列色</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
  <td>🟢 98.9%</td>
</tr>
<tr>
  <td><b>classic57</b><br/>純 CJK 工作表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
  <td>🟡 84.0%</td>
</tr>
<tr>
  <td><b>classic58</b><br/>混合數字格式</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
  <td>🟢 94.8%</td>
</tr>
<tr>
  <td><b>classic59</b><br/>多工作表摘要（4 個工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic60</b><br/>大型寬版表格（20×50）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 94.9%</td>
</tr>
</table>
