# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet 下载次数"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/badge/.NET-%3E%3D%209.0-red.svg" alt=".NET 9.0+"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="许可证"></a>
</p>
<p>
<a href="README.md">English</a> | 简体中文 | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>
</p>
</div>

---

一个极简、零依赖的 .NET 库，用于将 Excel (.xlsx) 文件转换为 PDF。

## 功能特性

- **Excel 转 PDF** — 将 `.xlsx` 文件转换为分页 PDF，自动进行列布局
- **零依赖** — 仅使用 .NET 内置 API（无需任何外部包）
- **标准 PDF 1.4** 格式输出
- **Word 转 PDF** — 开发中
- **图表** — 暂不支持

## 快速开始

### 通过 NuGet 安装

```bash
dotnet add package MiniPdf
```

### 系统要求

- .NET 9.0 或更高版本

## 使用方式

```csharp
using MiniPdf;

// 文件转文件
MiniPdf.ConvertToPdf("data.xlsx", "data.pdf");

// 文件转字节数组
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// 流转字节数组
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## 基准测试

MiniPdf 的输出结果以 LibreOffice 作为参考渲染器，针对 **60 个经典测试用例**进行比较。

| 类别 | 数量 | 阈值 |
|---|---|---|
| 🟢 优秀 | 54 | ≥ 90% |
| 🟡 可接受 | 5 | 70% – 90% |
| 🔴 需要改进 | 1 | < 70% |

**整体平均分数：96.2%**（文字相似度 40% + 视觉相似度 40% + 页数 20%）

### 视觉对比

以下为全部 60 个测试用例的对比，左侧为 MiniPdf 输出，右侧为 LibreOffice 参考结果。多页结果仅显示第 1 页。

<table>
<tr><th>测试用例</th><th>MiniPdf</th><th>LibreOffice（参考）</th><th>分数</th></tr>
<tr>
  <td><b>classic01</b><br/>基本表格含标题</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic02</b><br/>多个工作表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic03</b><br/>空白工作簿</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic04</b><br/>单个单元格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic05</b><br/>宽表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic06</b><br/>长型表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
  <td>🟢 95.1%</td>
</tr>
<tr>
  <td><b>classic07</b><br/>纯数字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic08</b><br/>文字与数字混合</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic09</b><br/>长文本<br/><i>MiniPdf：1 页<br/>参考：12 页</i></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
  <td>🔴 20.3%</td>
</tr>
<tr>
  <td><b>classic10</b><br/>特殊 XML 字符</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic11</b><br/>稀疏行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic12</b><br/>稀疏列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic13</b><br/>日期字符串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic14</b><br/>小数数字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic15</b><br/>负数</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic16</b><br/>百分比字符串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic17</b><br/>货币字符串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.0%</td>
</tr>
<tr>
  <td><b>classic18</b><br/>大型数据集</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
  <td>🟢 91.5%</td>
</tr>
<tr>
  <td><b>classic19</b><br/>单列列表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic20</b><br/>全空单元格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic21</b><br/>仅标题行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic22</b><br/>长工作表名称</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic23</b><br/>Unicode 文本</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
  <td>🟡 86.1%</td>
</tr>
<tr>
  <td><b>classic24</b><br/>红色文本</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic25</b><br/>多种颜色</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic26</b><br/>内嵌字符串</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic27</b><br/>单行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic28</b><br/>重复值</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic29</b><br/>公式结果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic30</b><br/>空白与填充工作表混合</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic31</b><br/>粗体标题行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic32</b><br/>数字右对齐</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic33</b><br/>居中文本</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic34</b><br/>明确列宽</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic35</b><br/>明确行高</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
  <td>🟢 96.8%</td>
</tr>
<tr>
  <td><b>classic36</b><br/>合并单元格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
  <td>🟢 97.8%</td>
</tr>
<tr>
  <td><b>classic37</b><br/>冻结窗格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic38</b><br/>超链接单元格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic39</b><br/>财务表格</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic40</b><br/>科学计数法</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
  <td>🟢 94.6%</td>
</tr>
<tr>
  <td><b>classic41</b><br/>整数与浮点数</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
  <td>🟢 97.6%</td>
</tr>
<tr>
  <td><b>classic42</b><br/>布尔值</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
  <td>🟢 92.6%</td>
</tr>
<tr>
  <td><b>classic43</b><br/>库存报告</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic44</b><br/>员工花名册</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
  <td>🟡 81.6%</td>
</tr>
<tr>
  <td><b>classic45</b><br/>按地区销售（4 个工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic45_sales_by_region_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic46</b><br/>成绩册</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic46_grade_book_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic47</b><br/>时序数据</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
  <td>🟢 98.6%</td>
</tr>
<tr>
  <td><b>classic48</b><br/>问卷结果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic49</b><br/>联系人列表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
  <td>🟡 86.4%</td>
</tr>
<tr>
  <td><b>classic50</b><br/>预算与实际（3 个工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic51</b><br/>产品目录</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
  <td>🟡 77.2%</td>
</tr>
<tr>
  <td><b>classic52</b><br/>数据透视摘要</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.3%</td>
</tr>
<tr>
  <td><b>classic53</b><br/>发票版式</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
  <td>🟢 96.0%</td>
</tr>
<tr>
  <td><b>classic54</b><br/>多级标题</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic55</b><br/>错误值（#N/A、#DIV/0!）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic56</b><br/>交替行颜色</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
  <td>🟢 98.9%</td>
</tr>
<tr>
  <td><b>classic57</b><br/>纯 CJK 工作表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
  <td>🟡 84.0%</td>
</tr>
<tr>
  <td><b>classic58</b><br/>混合数字格式</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
  <td>🟢 94.8%</td>
</tr>
<tr>
  <td><b>classic59</b><br/>多工作表摘要（4 个工作表）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic60</b><br/>大型宽表格（20×50）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 94.9%</td>
</tr>
</table>

## 许可证

本项目基于 [Apache License 2.0](LICENSE) 许可证。
