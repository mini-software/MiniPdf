# MiniPdf

<div align="center">
<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/dt/MiniPdf.svg" alt="NuGet ダウンロード数"></a>
<a href="https://github.com/shps951023/MiniPdf" rel="nofollow"><img src="https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/badge/.NET-%3E%3D%209.0-red.svg" alt=".NET 9.0+"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="ライセンス"></a>
</p>
<p>
<a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | 日本語 | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>
</p>
</div>

---

Excel (.xlsx) ファイルを PDF に変換するための、ミニマルで依存関係ゼロの .NET ライブラリです。

## 機能

- **Excel → PDF 変換** — `.xlsx` ファイルを自動列レイアウトで改ページ付き PDF に変換
- **依存関係ゼロ** — .NET 組み込み API のみを使用（外部パッケージ不要）
- **有効な PDF 1.4** 形式で出力
- **Word → PDF 変換** — 開発中
- **グラフ** — 現在未対応

## はじめに

### NuGet でインストール

```bash
dotnet add package MiniPdf
```

### 動作要件

- .NET 9.0 以降

## 使用方法

```csharp
using MiniPdf;

// ファイルからファイルへ
MiniPdf.ConvertToPdf("data.xlsx", "data.pdf");

// ファイルからバイト配列へ
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// ストリームからバイト配列へ
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytes = MiniPdf.ConvertToPdf(stream);
```

## ベンチマーク

MiniPdf の出力は、LibreOffice を参照レンダラーとして **60 のクラシックテストケース**と比較されています。

| カテゴリ | 件数 | 閾値 |
|---|---|---|
| 🟢 優秀 | 54 | ≥ 90% |
| 🟡 許容範囲 | 5 | 70% – 90% |
| 🔴 要改善 | 1 | < 70% |

**総合平均スコア: 96.2%**（テキスト類似度 40% + 視覚的類似度 40% + ページ数 20%）

### 視覚比較

MiniPdf 出力（左）と LibreOffice 参照（右）を比較した全 60 テストケース。複数ページの場合は 1 ページ目を表示。

<table>
<tr><th>テストケース</th><th>MiniPdf</th><th>LibreOffice（参照）</th><th>スコア</th></tr>
<tr>
  <td><b>classic01</b><br/>ヘッダー付き基本表</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic01_basic_table_with_headers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic02</b><br/>複数ワークシート</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic02_multiple_worksheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic03</b><br/>空のブック</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic03_empty_workbook_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic04</b><br/>単一セル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic04_single_cell_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic05</b><br/>横長テーブル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic05_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic06</b><br/>縦長テーブル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic06_tall_table_p1_reference.png" width="320"/></td>
  <td>🟢 95.1%</td>
</tr>
<tr>
  <td><b>classic07</b><br/>数値のみ</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic07_numbers_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic08</b><br/>テキストと数値の混合</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic08_mixed_text_and_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic09</b><br/>長いテキスト<br/><i>MiniPdf: 1 ページ<br/>参照: 12 ページ</i></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic09_long_text_p1_reference.png" width="320"/></td>
  <td>🔴 20.3%</td>
</tr>
<tr>
  <td><b>classic10</b><br/>特殊 XML 文字</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic10_special_xml_characters_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic11</b><br/>まばらな行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic11_sparse_rows_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic12</b><br/>まばらな列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic12_sparse_columns_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic13</b><br/>日付文字列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic13_date_strings_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic14</b><br/>小数数値</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic14_decimal_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic15</b><br/>負の数</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic15_negative_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic16</b><br/>パーセント文字列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic16_percentage_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic17</b><br/>通貨文字列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic17_currency_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.0%</td>
</tr>
<tr>
  <td><b>classic18</b><br/>大規模データセット</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic18_large_dataset_p1_reference.png" width="320"/></td>
  <td>🟢 91.5%</td>
</tr>
<tr>
  <td><b>classic19</b><br/>単一列リスト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic19_single_column_list_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic20</b><br/>全空セル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic20_all_empty_cells_p1_reference.png" width="320"/></td>
  <td>🟢 100.0%</td>
</tr>
<tr>
  <td><b>classic21</b><br/>ヘッダーのみ</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic21_header_only_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic22</b><br/>長いシート名</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic22_long_sheet_name_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic23</b><br/>Unicode テキスト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic23_unicode_text_p1_reference.png" width="320"/></td>
  <td>🟡 86.1%</td>
</tr>
<tr>
  <td><b>classic24</b><br/>赤色テキスト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic24_red_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic25</b><br/>複数の色</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic25_multiple_colors_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic26</b><br/>インライン文字列</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic26_inline_strings_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic27</b><br/>単一行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic27_single_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic28</b><br/>重複値</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic28_duplicate_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic29</b><br/>数式の結果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic29_formula_results_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic30</b><br/>空シートと入力済みシートの混在</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic31</b><br/>太字ヘッダー行</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic31_bold_header_row_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic32</b><br/>右揃え数値</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic32_right_aligned_numbers_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic33</b><br/>中央揃えテキスト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic33_centered_text_p1_reference.png" width="320"/></td>
  <td>🟢 99.9%</td>
</tr>
<tr>
  <td><b>classic34</b><br/>明示的な列幅</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic34_explicit_column_widths_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic35</b><br/>明示的な行高さ</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic35_explicit_row_heights_p1_reference.png" width="320"/></td>
  <td>🟢 96.8%</td>
</tr>
<tr>
  <td><b>classic36</b><br/>結合セル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic36_merged_cells_p1_reference.png" width="320"/></td>
  <td>🟢 97.8%</td>
</tr>
<tr>
  <td><b>classic37</b><br/>枠の固定</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic37_freeze_panes_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic38</b><br/>ハイパーリンクセル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic38_hyperlink_cell_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic39</b><br/>財務テーブル</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic39_financial_table_p1_reference.png" width="320"/></td>
  <td>🟢 99.6%</td>
</tr>
<tr>
  <td><b>classic40</b><br/>科学的記数法</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic40_scientific_notation_p1_reference.png" width="320"/></td>
  <td>🟢 94.6%</td>
</tr>
<tr>
  <td><b>classic41</b><br/>整数と浮動小数点数</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic41_integer_vs_float_p1_reference.png" width="320"/></td>
  <td>🟢 97.6%</td>
</tr>
<tr>
  <td><b>classic42</b><br/>ブール値</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic42_boolean_values_p1_reference.png" width="320"/></td>
  <td>🟢 92.6%</td>
</tr>
<tr>
  <td><b>classic43</b><br/>在庫レポート</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic43_inventory_report_p1_reference.png" width="320"/></td>
  <td>🟢 99.2%</td>
</tr>
<tr>
  <td><b>classic44</b><br/>従業員名簿</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic44_employee_roster_p1_reference.png" width="320"/></td>
  <td>🟡 81.6%</td>
</tr>
<tr>
  <td><b>classic45</b><br/>地域別売上（4 シート）</td>
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
  <td><b>classic47</b><br/>時系列データ</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic47_time_series_p1_reference.png" width="320"/></td>
  <td>🟢 98.6%</td>
</tr>
<tr>
  <td><b>classic48</b><br/>アンケート結果</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic48_survey_results_p1_reference.png" width="320"/></td>
  <td>🟢 98.8%</td>
</tr>
<tr>
  <td><b>classic49</b><br/>連絡先リスト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic49_contact_list_p1_reference.png" width="320"/></td>
  <td>🟡 86.4%</td>
</tr>
<tr>
  <td><b>classic50</b><br/>予算対実績（3 シート）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic50_budget_vs_actuals_p1_reference.png" width="320"/></td>
  <td>🟢 99.1%</td>
</tr>
<tr>
  <td><b>classic51</b><br/>製品カタログ</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic51_product_catalog_p1_reference.png" width="320"/></td>
  <td>🟡 77.2%</td>
</tr>
<tr>
  <td><b>classic52</b><br/>ピボットサマリー</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic52_pivot_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.3%</td>
</tr>
<tr>
  <td><b>classic53</b><br/>請求書レイアウト</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic53_invoice_p1_reference.png" width="320"/></td>
  <td>🟢 96.0%</td>
</tr>
<tr>
  <td><b>classic54</b><br/>多段ヘッダー</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic54_multi_level_header_p1_reference.png" width="320"/></td>
  <td>🟢 99.5%</td>
</tr>
<tr>
  <td><b>classic55</b><br/>エラー値（#N/A、#DIV/0!）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic55_error_values_p1_reference.png" width="320"/></td>
  <td>🟢 99.7%</td>
</tr>
<tr>
  <td><b>classic56</b><br/>交互行カラー</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic56_alternating_row_colors_p1_reference.png" width="320"/></td>
  <td>🟢 98.9%</td>
</tr>
<tr>
  <td><b>classic57</b><br/>CJK のみシート</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic57_cjk_only_p1_reference.png" width="320"/></td>
  <td>🟡 84.0%</td>
</tr>
<tr>
  <td><b>classic58</b><br/>数値フォーマット混在</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic58_mixed_numeric_formats_p1_reference.png" width="320"/></td>
  <td>🟢 94.8%</td>
</tr>
<tr>
  <td><b>classic59</b><br/>マルチシートサマリー（4 シート）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic59_multi_sheet_summary_p1_reference.png" width="320"/></td>
  <td>🟢 99.8%</td>
</tr>
<tr>
  <td><b>classic60</b><br/>大型横長テーブル（20×50）</td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_minipdf.png" width="320"/></td>
  <td><img src="tests/MiniPdf.Benchmark/reports/images/classic60_large_wide_table_p1_reference.png" width="320"/></td>
  <td>🟢 94.9%</td>
</tr>
</table>

## ライセンス

本プロジェクトは [Apache License 2.0](LICENSE) の下でライセンスされています。
