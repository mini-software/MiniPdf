# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-02T13:33:44.352472

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9954 | 1/1 | **0.9982** |
| 2 | 🟢 classic02_multiple_worksheets | 0.9914 | 0.9968 | 3/3 | **0.9953** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9996 | 1/1 | **0.9998** |
| 5 | 🟢 classic05_wide_table | 1.0 | 0.9918 | 3/3 | **0.9967** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.8779 | 5/5 | **0.9512** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9965 | 1/1 | **0.9986** |
| 9 | 🔴 classic09_long_text | 0.2222 | 0.0757 | 1/12 | **0.2192** |
| 10 | 🟢 classic10_special_xml_characters | 1.0 | 0.9937 | 1/1 | **0.9975** |
| 11 | 🟢 classic11_sparse_rows | 1.0 | 0.999 | 2/2 | **0.9996** |
| 12 | 🟢 classic12_sparse_columns | 1.0 | 0.9975 | 1/1 | **0.999** |
| 13 | 🟢 classic13_date_strings | 0.9751 | 0.9938 | 1/1 | **0.9876** |
| 14 | 🟢 classic14_decimal_numbers | 1.0 | 0.9956 | 1/1 | **0.9982** |
| 15 | 🟢 classic15_negative_numbers | 1.0 | 0.9953 | 1/1 | **0.9981** |
| 16 | 🟢 classic16_percentage_strings | 0.9939 | 0.9949 | 1/1 | **0.9955** |
| 17 | 🟢 classic17_currency_strings | 0.9615 | 0.9934 | 1/1 | **0.982** |
| 18 | 🟢 classic18_large_dataset | 1.0 | 0.7878 | 24/24 | **0.9151** |
| 19 | 🟢 classic19_single_column_list | 1.0 | 0.9921 | 1/1 | **0.9968** |
| 20 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 21 | 🟢 classic21_header_only | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 22 | 🟢 classic22_long_sheet_name | 1.0 | 0.9981 | 1/1 | **0.9992** |
| 23 | 🟡 classic23_unicode_text | 0.6857 | 0.9914 | 1/1 | **0.8708** |
| 24 | 🟢 classic24_red_text | 1.0 | 0.9952 | 1/1 | **0.9981** |
| 25 | 🟢 classic25_multiple_colors | 0.9955 | 0.992 | 1/1 | **0.995** |
| 26 | 🟢 classic26_inline_strings | 1.0 | 0.9962 | 1/1 | **0.9985** |
| 27 | 🟢 classic27_single_row | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 28 | 🟢 classic28_duplicate_values | 1.0 | 0.9959 | 1/1 | **0.9984** |
| 29 | 🟢 classic29_formula_results | 1.0 | 0.9957 | 1/1 | **0.9983** |
| 30 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.9982 | 2/2 | **0.9993** |
| 31 | 🟢 classic31_bold_header_row | 1.0 | 0.9923 | 1/1 | **0.9969** |
| 32 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.9964 | 1/1 | **0.9986** |
| 33 | 🟢 classic33_centered_text | 1.0 | 0.9975 | 1/1 | **0.999** |
| 34 | 🟢 classic34_explicit_column_widths | 0.9615 | 0.9942 | 1/1 | **0.9823** |
| 35 | 🟢 classic35_explicit_row_heights | 0.9231 | 0.9971 | 1/1 | **0.9681** |
| 36 | 🟢 classic36_merged_cells | 0.963 | 0.9939 | 1/1 | **0.9828** |
| 37 | 🟢 classic37_freeze_panes | 1.0 | 0.9781 | 1/1 | **0.9912** |
| 38 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9954 | 1/1 | **0.9982** |
| 39 | 🟢 classic39_financial_table | 1.0 | 0.991 | 1/1 | **0.9964** |
| 40 | 🟢 classic40_scientific_notation | 0.8711 | 0.9928 | 1/1 | **0.9456** |
| 41 | 🟢 classic41_integer_vs_float | 0.9453 | 0.9941 | 1/1 | **0.9758** |
| 42 | 🟢 classic42_boolean_values | 0.8202 | 0.9948 | 1/1 | **0.926** |
| 43 | 🟢 classic43_inventory_report | 0.9984 | 0.9803 | 1/1 | **0.9915** |
| 44 | 🟡 classic44_employee_roster | 0.7143 | 0.9684 | 1/1 | **0.8731** |
| 45 | 🟢 classic45_sales_by_region | 1.0 | 0.9967 | 4/4 | **0.9987** |
| 46 | 🟢 classic46_grade_book | 1.0 | 0.987 | 1/1 | **0.9948** |
| 47 | 🟢 classic47_time_series | 1.0 | 0.9647 | 1/1 | **0.9859** |
| 48 | 🟢 classic48_survey_results | 0.9803 | 0.9897 | 1/1 | **0.988** |
| 49 | 🟡 classic49_contact_list | 0.6845 | 0.9766 | 1/1 | **0.8644** |
| 50 | 🟢 classic50_budget_vs_actuals | 0.9934 | 0.9849 | 3/3 | **0.9913** |
| 51 | 🟡 classic51_product_catalog | 0.5786 | 0.9694 | 1/1 | **0.8192** |
| 52 | 🟢 classic52_pivot_summary | 0.9978 | 0.9839 | 1/1 | **0.9927** |
| 53 | 🟢 classic53_invoice | 0.9207 | 0.9789 | 1/1 | **0.9598** |
| 54 | 🟢 classic54_multi_level_header | 1.0 | 0.988 | 1/1 | **0.9952** |
| 55 | 🟢 classic55_error_values | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 56 | 🟡 classic56_alternating_row_colors | 1.0 | 0.3848 | 1/1 | **0.7539** |
| 57 | 🟡 classic57_cjk_only | 0.6383 | 0.9925 | 1/1 | **0.8523** |
| 58 | 🟢 classic58_mixed_numeric_formats | 0.8795 | 0.9901 | 1/1 | **0.9478** |
| 59 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9947 | 4/4 | **0.9979** |
| 60 | 🟢 classic60_large_wide_table | 1.0 | 0.8737 | 4/4 | **0.9495** |
| 61 | 🟢 classic61_product_card_with_image | 1.0 | 0.9872 | 1/1 | **0.9949** |
| 62 | 🟢 classic62_company_logo_header | 0.988 | 0.9786 | 1/1 | **0.9866** |
| 63 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9782 | 1/1 | **0.9913** |
| 64 | 🟢 classic64_employee_directory_with_photo | 0.977 | 0.9756 | 1/1 | **0.981** |
| 65 | 🟢 classic65_inventory_with_product_photos | 0.9786 | 0.9744 | 1/1 | **0.9812** |
| 66 | 🟢 classic66_invoice_with_logo | 0.9259 | 0.9792 | 1/1 | **0.962** |
| 67 | 🟢 classic67_real_estate_listing | 1.0 | 0.9777 | 1/1 | **0.9911** |
| 68 | 🟢 classic68_restaurant_menu | 0.875 | 0.9327 | 1/1 | **0.9231** |
| 69 | 🟢 classic69_image_only_sheet | 1.0 | 0.9773 | 1/1 | **0.9909** |
| 70 | 🟢 classic70_product_catalog_with_images | 0.947 | 0.9563 | 1/1 | **0.9613** |
| 71 | 🟢 classic71_multi_sheet_with_images | 0.9898 | 0.9906 | 3/3 | **0.9922** |
| 72 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.943 | 1/1 | **0.9772** |
| 73 | 🟢 classic73_event_flyer_with_banner | 0.9006 | 0.9616 | 1/1 | **0.9449** |
| 74 | 🟢 classic74_dashboard_with_kpi_image | 0.9571 | 0.9557 | 1/1 | **0.9651** |
| 75 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9705 | 1/1 | **0.9882** |
| 76 | 🟢 classic76_product_image_grid | 1.0 | 0.9527 | 1/1 | **0.9811** |
| 77 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.9564 | 1/1 | **0.9826** |
| 78 | 🟢 classic78_small_icon_per_row | 0.9125 | 0.9848 | 1/1 | **0.9589** |
| 79 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9545 | 1/1 | **0.9818** |
| 80 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9825 | 1/1 | **0.993** |
| 81 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.9588 | 1/1 | **0.9835** |
| 82 | 🟢 classic82_before_after_images | 0.9815 | 0.9431 | 1/1 | **0.9698** |
| 83 | 🟢 classic83_color_swatch_palette | 0.9626 | 0.9665 | 1/1 | **0.9716** |
| 84 | 🟢 classic84_travel_destination_cards | 1.0 | 0.9347 | 1/1 | **0.9739** |
| 85 | 🟢 classic85_lab_results_with_image | 0.9559 | 0.9612 | 1/1 | **0.9668** |
| 86 | 🟢 classic86_software_screenshot_features | 0.9669 | 0.9786 | 1/1 | **0.9782** |
| 87 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.985 | 1/1 | **0.994** |
| 88 | 🟢 classic88_image_after_data | 0.997 | 0.9818 | 1/1 | **0.9915** |
| 89 | 🟢 classic89_nutrition_label_with_image | 0.9379 | 0.98 | 1/1 | **0.9672** |
| 90 | 🟡 classic90_project_status_with_milestones | 0.7312 | 0.9475 | 1/1 | **0.8715** |

**Average Overall Score: 0.9644**

## Detailed Results

### classic01_basic_table_with_headers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9954
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1267 bytes, Reference=30311 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 0.9914
- **Visual Average:** 0.9968
- **Overall Score:** 0.9953
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2234 bytes, Reference=36003 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic02_multiple_worksheets.pdf
+++ reference/classic02_multiple_worksheets.pdf
@@ -10,6 +10,6 @@
 Utilities 200

 ---PAGE---

 Metric Value

-Total Revenue 1130

+Total Reve 1130

 Total Costs 3700

 Net -2570
```
</details>

### classic03_empty_workbook

- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=576 bytes, Reference=7283 bytes

Text content: ✅ Identical

### classic04_single_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9996
- **Overall Score:** 0.9998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=623 bytes, Reference=19860 bytes

Text content: ✅ Identical

### classic05_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9918
- **Overall Score:** 0.9967
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=8636 bytes, Reference=62308 bytes

Text content: ✅ Identical

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.8779
- **Overall Score:** 0.9512
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=37084 bytes, Reference=185703 bytes

Text content: ✅ Identical

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1092 bytes, Reference=24806 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9965
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1132 bytes, Reference=27336 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.2222
- **Visual Average:** 0.0757
- **Overall Score:** 0.2192
- **Pages:** MiniPdf=1, Reference=12
- **File Size:** MiniPdf=3667 bytes, Reference=29170 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,26 +1,26 @@
 Long Text Column

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-XXXXXXXXXXXXXXX

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

-AAAAAAAAA

-BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

-BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

-BBBBBB

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

 Short

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---

+

+---PAGE---
```
</details>

### classic10_special_xml_characters

- **Text Similarity:** 1.0
- **Visual Average:** 0.9937
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=968 bytes, Reference=27644 bytes

Text content: ✅ Identical

### classic11_sparse_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.999
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1037 bytes, Reference=23538 bytes

Text content: ✅ Identical

### classic12_sparse_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.9975
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=860 bytes, Reference=24923 bytes

Text content: ✅ Identical

### classic13_date_strings

- **Text Similarity:** 0.9751
- **Visual Average:** 0.9938
- **Overall Score:** 0.9876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1168 bytes, Reference=29104 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic13_date_strings.pdf
+++ reference/classic13_date_strings.pdf
@@ -1,6 +1,6 @@
 Date Event

-2025-01-15 Launch

-2025-06-30 Release

-2025-12-25 Holiday

-2026-01-01 New Year

-2026-02-23 Today
+2025-01-1 Launch

+2025-06-3 Release

+2025-12-2 Holiday

+2026-01-0 New Year

+2026-02-2 Today
```
</details>

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9956
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1146 bytes, Reference=29057 bytes

Text content: ✅ Identical

### classic15_negative_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9953
- **Overall Score:** 0.9981
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1230 bytes, Reference=28526 bytes

Text content: ✅ Identical

### classic16_percentage_strings

- **Text Similarity:** 0.9939
- **Visual Average:** 0.9949
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1147 bytes, Reference=29888 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic16_percentage_strings.pdf
+++ reference/classic16_percentage_strings.pdf
@@ -1,5 +1,5 @@
 Metric Rate

-Conversion 12.5%

+Conversion12.5%

 Bounce 45.3%

 Retention 88.7%

 Churn 3.2%

```
</details>

### classic17_currency_strings

- **Text Similarity:** 0.9615
- **Visual Average:** 0.9934
- **Overall Score:** 0.982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1253 bytes, Reference=29862 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic17_currency_strings.pdf
+++ reference/classic17_currency_strings.pdf
@@ -3,5 +3,5 @@
 Gadget $149.00

 Premium $1,299.99

 Budget $4.50

-Euro Item EUR49.99

-Yen Item JPY5000
+Euro Item €49.99

+Yen Item ¥5000
```
</details>

### classic18_large_dataset

- **Text Similarity:** 1.0
- **Visual Average:** 0.7878
- **Overall Score:** 0.9151
- **Pages:** MiniPdf=24, Reference=24
- **File Size:** MiniPdf=483743 bytes, Reference=2487195 bytes

Text content: ✅ Identical

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9921
- **Overall Score:** 0.9968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1576 bytes, Reference=29688 bytes

Text content: ✅ Identical

### classic20_all_empty_cells

- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=576 bytes, Reference=7283 bytes

Text content: ✅ Identical

### classic21_header_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=807 bytes, Reference=22034 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9981
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=850 bytes, Reference=23683 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.6857
- **Visual Average:** 0.9914
- **Overall Score:** 0.8708
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1559 bytes, Reference=67722 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic23_unicode_text.pdf
+++ reference/classic23_unicode_text.pdf
@@ -1,7 +1,9 @@
 Language Greeting Extra

 English Hello World

-Chinese ?? ??

-Japanese ????? ??

-Korean ????? ??

-Arabic ????? ??????

-Emoji ???? ??
+你好 世界

+Chinese

+こんにちは世界

+Japanese

+Korean 안녕하세요세계

+Arabicمرحبا العالم

+Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9952
- **Overall Score:** 0.9981
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1044 bytes, Reference=39031 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 0.9955
- **Visual Average:** 0.992
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1726 bytes, Reference=43116 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic25_multiple_colors.pdf
+++ reference/classic25_multiple_colors.pdf
@@ -1,4 +1,4 @@
-Color Name Sample Text

+Color NamSample Text

 Red This is red text

 Green This is green text

 Blue This is blue text

```
</details>

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1007 bytes, Reference=25018 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=926 bytes, Reference=23681 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9959
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1459 bytes, Reference=24729 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9957
- **Overall Score:** 0.9983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1377 bytes, Reference=27548 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1347 bytes, Reference=27418 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9923
- **Overall Score:** 0.9969
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1521 bytes, Reference=40714 bytes

Text content: ✅ Identical

### classic32_right_aligned_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9964
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=958 bytes, Reference=27582 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9975
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1235 bytes, Reference=26648 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 0.9615
- **Visual Average:** 0.9942
- **Overall Score:** 0.9823
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1231 bytes, Reference=28834 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic34_explicit_column_widths.pdf
+++ reference/classic34_explicit_column_widths.pdf
@@ -1,5 +1,4 @@
 ID Description Value

 1 Short 10

-2 A much longer description text 200

-here

+2 A much longer description text here 200

 3 Medium length description 55
```
</details>

### classic35_explicit_row_heights

- **Text Similarity:** 0.9231
- **Visual Average:** 0.9971
- **Overall Score:** 0.9681
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=871 bytes, Reference=25108 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
-Tall Header Value

-Extra Tall Row 42

-Normal Row 10
+Tall HeadeValue

+Extra Tall 42

+Normal Ro 10
```
</details>

### classic36_merged_cells

- **Text Similarity:** 0.963
- **Visual Average:** 0.9939
- **Overall Score:** 0.9828
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1072 bytes, Reference=27256 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic36_merged_cells.pdf
+++ reference/classic36_merged_cells.pdf
@@ -1,4 +1,4 @@
-Merged Header Spanning Three Columns

+Merged Header Spanning Three

 Col1 Col2 Col3

 Row2A Row2B Row2C

 Row3A Row3B Row3C
```
</details>

### classic37_freeze_panes

- **Text Similarity:** 1.0
- **Visual Average:** 0.9781
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4353 bytes, Reference=46420 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9954
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=892 bytes, Reference=26279 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic38_hyperlink_cell.pdf
+++ reference/classic38_hyperlink_cell.pdf
@@ -1,3 +1,4 @@
 Resource URL

-GitHub https://github.com

+GitHub

+https://github.com

 Docs https://docs.microsoft.com
```
</details>

### classic39_financial_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.991
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1942 bytes, Reference=43383 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 0.8711
- **Visual Average:** 0.9928
- **Overall Score:** 0.9456
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1189 bytes, Reference=30852 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic40_scientific_notation.pdf
+++ reference/classic40_scientific_notation.pdf
@@ -1,6 +1,6 @@
 Label Value

-Avogadro 6.022E+23

+Avogadro 6.02E+23

 Planck 6.626E-34

-Speed of Light 299800000

-Electron mass 9.109E-31

-Pi approx 3.14159265358979
+Speed of L 3E+08

+Electron m9.109E-31

+Pi approx 3.141593
```
</details>

### classic41_integer_vs_float

- **Text Similarity:** 0.9453
- **Visual Average:** 0.9941
- **Overall Score:** 0.9758
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1408 bytes, Reference=29637 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic41_integer_vs_float.pdf
+++ reference/classic41_integer_vs_float.pdf
@@ -6,4 +6,4 @@
 Zero 0

 ZeroFloat 0

 Large 1000000

-Small 1E-06
+Small 0.000001
```
</details>

### classic42_boolean_values

- **Text Similarity:** 0.8202
- **Visual Average:** 0.9948
- **Overall Score:** 0.926
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1148 bytes, Reference=28631 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

-Dark Mode 1

-Notifications 0

-Auto-save 1

-Analytics 0

-Beta Features 1
+Dark Mode TRUE

+Notificatio FALSE

+Auto-save TRUE

+Analytics FALSE

+Beta Featu TRUE
```
</details>

### classic43_inventory_report

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9803
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2845 bytes, Reference=49849 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic43_inventory_report.pdf
+++ reference/classic43_inventory_report.pdf
@@ -1,4 +1,4 @@
-SKU Name Category Qty Unit Price Total Value

+SKU Name Category Qty Unit PriceTotal Value

 SKU001 Widget A Widgets 100 5.99 599

 SKU002 Widget B Widgets 250 3.49 872.5

 SKU003 Gadget X Gadgets 50 29.99 1499.5

```
</details>

### classic44_employee_roster

- **Text Similarity:** 0.7143
- **Visual Average:** 0.9684
- **Overall Score:** 0.8731
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3299 bytes, Reference=43656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

-1001 Alice Smith Engineering Senior Engineer alice@example.com

-1002 Bob Jones Marketing Marketing Manager bob@example.com

-1003 Carol Williams HR HR Specialist carol@example.com

-1004 David Brown Engineering Junior Engineer david@example.com

-1005 Eve Davis Finance Financial Analyst eve@example.com

-1006 Frank Miller Sales Sales Representative frank@example.com

-1007 Grace Wilson Engineering Tech Lead grace@example.com

-1008 Henry Moore Support Support Specialist henry@example.com
+1001 Alice Smith EngineerinSenior Engalice@example.com

+1002 Bob Jones MarketingMarketingbob@example.com

+1003 Carol Williams HR HR Specialcarol@example.com

+1004 David Brown EngineerinJunior Engdavid@example.com

+1005 Eve Davis Finance Financial Aeve@example.com

+1006 Frank Miller Sales Sales Reprfrank@example.com

+1007 Grace Wilson EngineerinTech Lead grace@example.com

+1008 Henry Moore Support Support Sphenry@example.com
```
</details>

### classic45_sales_by_region

- **Text Similarity:** 1.0
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3074 bytes, Reference=37087 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.987
- **Overall Score:** 0.9948
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3349 bytes, Reference=40993 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.9647
- **Overall Score:** 0.9859
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6461 bytes, Reference=55976 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9803
- **Visual Average:** 0.9897
- **Overall Score:** 0.988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2232 bytes, Reference=36069 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
-Question StrongAgree Agree Neutral Disagree StrongDisagree

-Easy to use 30 45 15 7 3

-Recommend 25 40 20 10 5

+Question StrongAgr Agree Neutral Disagree StrongDisagree

+Easy to us 30 45 15 7 3

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

-Good support 35 40 15 7 3

+Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.6845
- **Visual Average:** 0.9766
- **Overall Score:** 0.8644
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2641 bytes, Reference=41523 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smith +1-555-0101 alice@example.com New York USA

-Bob Jones +44-20-7946-0958 bob@example.co.uk London UK

-Carol Wang +86-10-1234-5678 carol@example.cn Beijing China

-David Muller +49-30-1234567 david@example.de Berlin Germany

-Eve Martin +33-1-23-45-67-89 eve@example.fr Paris France

-Frank Tanaka +81-3-1234-5678 frank@example.jp Tokyo Japan

-Grace Kim +82-2-1234-5678 grace@example.kr Seoul Korea
+Alice Smith+1-555-01 alice@exa New York USA

+Bob Jones +44-20-79 bob@examLondon UK

+Carol Wan+86-10-12 carol@exaBeijing China

+David Mull+49-30-12 david@exaBerlin Germany

+Eve Martin+33-1-23-4eve@examParis France

+Frank Tana+81-3-123 frank@exaTokyo Japan

+Grace Kim +82-2-123 grace@exaSeoul Korea
```
</details>

### classic50_budget_vs_actuals

- **Text Similarity:** 0.9934
- **Visual Average:** 0.9849
- **Overall Score:** 0.9913
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=6103 bytes, Reference=54986 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic50_budget_vs_actuals.pdf
+++ reference/classic50_budget_vs_actuals.pdf
@@ -1,19 +1,19 @@
-Department Q1 Q2 Q3 Q4 Annual

-Engineering 200000 200000 210000 220000 830000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin 200000 200000 210000 220000 830000

 Marketing 80000 90000 85000 95000 350000

 Sales 120000 130000 140000 150000 540000

 HR 40000 40000 42000 43000 165000

 Finance 35000 35000 37000 38000 145000

 ---PAGE---

-Department Q1 Q2 Q3 Q4 Annual

-Engineering 195000 205000 215000 225000 840000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin 195000 205000 215000 225000 840000

 Marketing 82000 88000 91000 97000 358000

 Sales 118000 135000 142000 148000 543000

 HR 39000 41000 41500 44000 165500

 Finance 34000 36000 37500 39000 146500

 ---PAGE---

-Department Q1 Q2 Q3 Q4 Annual

-Engineering -5000 5000 5000 5000 10000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin -5000 5000 5000 5000 10000

 Marketing 2000 -2000 6000 2000 8000

 Sales -2000 5000 2000 -2000 3000

 HR -1000 1000 -500 1000 500

```
</details>

### classic51_product_catalog

- **Text Similarity:** 0.5786
- **Visual Average:** 0.9694
- **Overall Score:** 0.8192
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3427 bytes, Reference=44297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
-Part# Name Description Weight(g) Price

-P-001 Basic Widget Standard widget for everyday use 150 4.99

-P-002 Pro Widget Enhanced widget with premium features 180 12.99

-P-003 Mini Gadget Compact gadget for mobile use 90 19.99

-P-004 Max Gadget Full-size gadget, industrial grade 450 89.99

-P-005 Connector A Type-A connector cable, 1m 80 7.49

-P-006 Connector B Type-B connector cable, 2m 110 9.99

-P-007 Adapter X Universal power adapter 200 15.99

-P-008 Adapter Y Travel power adapter 120 11.99

-P-009 Mount Bracket Wall mount bracket, steel 600 24.99

-P-010 Carry Case Padded carry case, waterproof 350 34.99
+Part# Name Descriptio Weight(g) Price

+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgeEnhanced 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size g 450 89.99

+P-005 ConnectorType-A con 80 7.49

+P-006 ConnectorType-B con 110 9.99

+P-007 Adapter X Universal 200 15.99

+P-008 Adapter Y Travel pow 120 11.99

+P-009 Mount BraWall moun 600 24.99

+P-010 Carry CasePadded ca 350 34.99
```
</details>

### classic52_pivot_summary

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9839
- **Overall Score:** 0.9927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2277 bytes, Reference=44493 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic52_pivot_summary.pdf
+++ reference/classic52_pivot_summary.pdf
@@ -1,4 +1,4 @@
-Region Electronics Furniture Clothing Food Total

+Region ElectronicsFurniture Clothing Food Total

 North 45000 12000 8000 22000 87000

 South 38000 15000 11000 25000 89000

 East 52000 9000 14000 18000 93000

```
</details>

### classic53_invoice

- **Text Similarity:** 0.9207
- **Visual Average:** 0.9789
- **Overall Score:** 0.9598
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2424 bytes, Reference=53425 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -6,11 +6,11 @@
 ACME Corporation

 123 Business Rd, Suite 400

 New York, NY 10001

-Item Qty Unit Price Total

-Consulting Services 10 150 1500

-Software License 5 99 495

+Item Qty Unit PriceTotal

+Consulting 10 150 1500

+Software L 5 99 495

 Hardware 2 249.99 499.98

-Support Plan (annual) 1 1200 1200

+Support Pl 1 1200 1200

 Subtotal 3694.98

 Tax (8%) 295.6

 Total Due 3990.58
```
</details>

### classic54_multi_level_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.988
- **Overall Score:** 0.9952
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2161 bytes, Reference=38782 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1580 bytes, Reference=34677 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.3848
- **Overall Score:** 0.7539
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2080 bytes, Reference=32363 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Text Similarity:** 0.6383
- **Visual Average:** 0.9925
- **Overall Score:** 0.8523
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1644 bytes, Reference=88207 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic57_cjk_only.pdf
+++ reference/classic57_cjk_only.pdf
@@ -1,6 +1,11 @@
-?? ???? ?? ??

-1 ????? 5999 100

-2 ???? 2999 250

-3 ???? 1999 150

-4 ???? 299 500

-5 ??? 99 1000
+序号 产品名称价格 库存

+笔记本电脑

+1 5999 100

+智能手机

+2 2999 250

+平板电脑

+3 1999 150

+蓝牙耳机

+4 299 500

+充电器

+5 99 1000
```
</details>

### classic58_mixed_numeric_formats

- **Text Similarity:** 0.8795
- **Visual Average:** 0.9901
- **Overall Score:** 0.9478
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1564 bytes, Reference=32815 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic58_mixed_numeric_formats.pdf
+++ reference/classic58_mixed_numeric_formats.pdf
@@ -2,9 +2,9 @@
 Integer 1000000

 Float 2dp 3.14

 Float 5dp 3.14159

-Negative int -42

-Negative float -3.14

+Negative in -42

+Negative fl -3.14

 Very small 0.0001

-Very large 9999999.99

+Very large 10000000

 Zero 0

-Scientific approx 12300000000
+Scientific 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9947
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=4136 bytes, Reference=44781 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.8737
- **Overall Score:** 0.9495
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=50059 bytes, Reference=263350 bytes

Text content: ✅ Identical

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9872
- **Overall Score:** 0.9949
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2026 bytes, Reference=36974 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 0.988
- **Visual Average:** 0.9786
- **Overall Score:** 0.9866
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2657 bytes, Reference=42880 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic62_company_logo_header.pdf
+++ reference/classic62_company_logo_header.pdf
@@ -1,6 +1,6 @@
 ACME Corporation

 Annual Report 2025

-Department Q1 Q2 Q3 Q4

+DepartmenQ1 Q2 Q3 Q4

 Sales 120 135 142 160

-Engineering 85 90 95 100

+Engineerin 85 90 95 100

 Marketing 60 65 70 75
```
</details>

### classic63_two_products_side_by_side

- **Text Similarity:** 1.0
- **Visual Average:** 0.9782
- **Overall Score:** 0.9913
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3052 bytes, Reference=28933 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.977
- **Visual Average:** 0.9756
- **Overall Score:** 0.981
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4319 bytes, Reference=43408 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title Department Email

-Alice Chen Engineer R&D alice@example.com

-Bob Smith Manager Sales bob@example.com

-Carol Wang Designer UX carol@example.com
+Photo Name Title DepartmeEmail

+Alice ChenEngineer R&D alice@example.com

+Bob SmithManager Sales bob@example.com

+Carol WanDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9786
- **Visual Average:** 0.9744
- **Overall Score:** 0.9812
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6477 bytes, Reference=48227 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic65_inventory_with_product_photos.pdf
+++ reference/classic65_inventory_with_product_photos.pdf
@@ -1,6 +1,6 @@
 Image SKU Name Qty Price

-SKU-001 Red Widget 50 9.99

-SKU-002 Blue Gadget 30 14.99

-SKU-003 Green Tool 100 4.49

-SKU-004 Yellow Device 25 29.99

-SKU-005 Purple Gear 75 7.99
+SKU-001 Red Widge 50 9.99

+SKU-002 Blue Gadge 30 14.99

+SKU-003 Green Too 100 4.49

+SKU-004 Yellow Dev 25 29.99

+SKU-005 Purple Gea 75 7.99
```
</details>

### classic66_invoice_with_logo

- **Text Similarity:** 0.9259
- **Visual Average:** 0.9792
- **Overall Score:** 0.962
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2646 bytes, Reference=45034 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -1,8 +1,8 @@
 INVOICE

 Invoice #: INV-20250301

 Date: 2025-03-01

-Description Qty Unit Price Total

-Consulting Services 8 150 1200

-Software License 1 299 299

-Support Package 1 99 99

+DescriptiQty Unit PriceTotal

+Consulting 8 150 1200

+Software L 1 299 299

+Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Text Similarity:** 1.0
- **Visual Average:** 0.9777
- **Overall Score:** 0.9911
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2694 bytes, Reference=44030 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Text Similarity:** 0.875
- **Visual Average:** 0.9327
- **Overall Score:** 0.9231
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5242 bytes, Reference=47320 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,9 +1,9 @@
 Today's Menu

-Grilled Salmon $18.99

+Grilled S $18.99

 Fresh Atlantic salmon with herbs

-Caesar Salad $12.99

+Caesar Sa $12.99

 Romaine lettuce, croutons, parmesan

-Beef Burger $14.99

+Beef Burg $14.99

 8oz Angus beef, brioche bun

-Pasta Primavera $13.99

+Pasta Pri $13.99

 Seasonal vegetables, olive oil
```
</details>

### classic69_image_only_sheet

- **Text Similarity:** 1.0
- **Visual Average:** 0.9773
- **Overall Score:** 0.9909
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2459 bytes, Reference=8905 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Text Similarity:** 0.947
- **Visual Average:** 0.9563
- **Overall Score:** 0.9613
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4376 bytes, Reference=44156 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

-Classic Pen $3.99

+Classic P $3.99

 A reliable ballpoint pen

-Leather Notebook $12.99

+Leather $12.99

 Premium A5 notebook

-Desk Organizer $24.99

+Desk Orga $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 0.9898
- **Visual Average:** 0.9906
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=4977 bytes, Reference=37419 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic71_multi_sheet_with_images.pdf
+++ reference/classic71_multi_sheet_with_images.pdf
@@ -6,6 +6,6 @@
 Digital 50000

 Print 20000

 ---PAGE---

-Department Headcount

-Engineering 45

+DepartmenHeadcount

+Engineerin 45

 Sales 30
```
</details>

### classic72_bar_chart_image_with_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.943
- **Overall Score:** 0.9772
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3006 bytes, Reference=41342 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9006
- **Visual Average:** 0.9616
- **Overall Score:** 0.9449
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3044 bytes, Reference=44512 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -3,7 +3,7 @@
 Venue: Convention Center Hall A

 Speakers: 20+ Industry Leaders

 Time Session Speaker

-09:00 Opening Keynote Dr. Jane Kim

-10:30 AI in Practice Prof. Mark Liu

-13:00 Cloud Architecture Eng. Sara Patel

-15:00 Panel Discussion All Speakers
+09:00 Opening KDr. Jane Kim

+10:30 AI in Pract Prof. Mark Liu

+13:00 Cloud ArchEng. Sara Patel

+15:00 Panel Disc All Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.9571
- **Visual Average:** 0.9557
- **Overall Score:** 0.9651
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2877 bytes, Reference=48755 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -1,6 +1,6 @@
 Executive Dashboard Q1 2025

 KPI Target Actual Status

-Revenue 500000 523000 / Above

-New Customers 200 187 x Below

-NPS Score 70 74 / Above

-Churn Rate < 3% 2.8% / Above
+Revenue 500000 523000 ✓ Above

+New Custo 200 187  Below ✗

+NPS Score 70 74 ✓ Above

+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Text Similarity:** 1.0
- **Visual Average:** 0.9705
- **Overall Score:** 0.9882
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1951 bytes, Reference=39135 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9527
- **Overall Score:** 0.9811
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4958 bytes, Reference=39017 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9564
- **Overall Score:** 0.9826
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2719 bytes, Reference=52664 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9125
- **Visual Average:** 0.9848
- **Overall Score:** 0.9589
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5999 bytes, Reference=41646 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login bug Alice Done

-Write unit tests Bob In Progress

-Deploy to staging Carol Pending

-Code review PR #42 Alice Done

-Update docs Dave In Progress
+Fix login b Alice Done

+Write unit Bob In Progress

+Deploy to Carol Pending

+Code revieAlice Done

+Update doDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9545
- **Overall Score:** 0.9818
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2844 bytes, Reference=43015 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9825
- **Overall Score:** 0.993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2290 bytes, Reference=39079 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9588
- **Overall Score:** 0.9835
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5070 bytes, Reference=47175 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9815
- **Visual Average:** 0.9431
- **Overall Score:** 0.9698
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3780 bytes, Reference=42486 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -1,5 +1,5 @@
 Before After

-Old design - legacy UI New design - modern UI

+Old design – legacy UI New design – modern UI

 Metric Before After Delta

 Load time 4.2s 1.1s -74%

-Conversion 2.1% 4.8% +129%
+Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Text Similarity:** 0.9626
- **Visual Average:** 0.9665
- **Overall Score:** 0.9716
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6863 bytes, Reference=45933 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

-Primary Blue RGB(0, 82, 165)

-Primary Red RGB(197, 27, 50)

-Accent Green RGB(0, 163, 108)

-Neutral Grey RGB(128, 128, 128)

-Warm Yellow RGB(255, 193, 7)

+Primary BlRGB(0, 82, 165)

+Primary ReRGB(197, 27, 50)

+Accent Gr RGB(0, 163, 108)

+Neutral GrRGB(128, 128, 128)

+Warm YellRGB(255, 193, 7)

 Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Text Similarity:** 1.0
- **Visual Average:** 0.9347
- **Overall Score:** 0.9739
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4347 bytes, Reference=42524 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.9559
- **Visual Average:** 0.9612
- **Overall Score:** 0.9668
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3150 bytes, Reference=47866 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,7 +1,7 @@
 Sample Analysis Report

-Parameter Value Unit Reference Range Flag

-pH 7.35 7.35 - 7.45 Normal

-Glucose 5.2 mmol/L 3.9 - 5.5 Normal

-Sodium 142 mEq/L 136 - 145 Normal

-Potassium 5 mEq/L 3.5 - 5.0 Normal

-Creatinine 1.4 mg/dL 0.6 - 1.2 High
+ParameteValue Unit ReferenceFlag

+pH 7.35 7.35 – 7.45Normal

+Glucose 5.2 mmol/L 3.9 – 5.5 Normal

+Sodium 142 mEq/L 136 – 145 Normal

+Potassium 5 mEq/L 3.5 – 5.0 Normal

+Creatinine 1.4 mg/dL 0.6 – 1.2 High
```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.9669
- **Visual Average:** 0.9786
- **Overall Score:** 0.9782
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2797 bytes, Reference=41961 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic86_software_screenshot_features.pdf
+++ reference/classic86_software_screenshot_features.pdf
@@ -1,9 +1,9 @@
 MiniApp v2.0

 The fastest lightweight app

 Feature Available

-Dark Mode Yes

+Dark ModeYes

 Auto Save Yes

-Cloud Sync Yes

-Offline Mode Yes

-API Access Pro only

-Export to PDF Yes
+Cloud SyncYes

+Offline MoYes

+API AccessPro only

+Export to Yes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.985
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5459 bytes, Reference=47076 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 0.997
- **Visual Average:** 0.9818
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2795 bytes, Reference=43273 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic88_image_after_data.pdf
+++ reference/classic88_image_after_data.pdf
@@ -1,4 +1,4 @@
-Quarter Revenue Expenses Profit

+Quarter Revenue ExpensesProfit

 Q1 120000 80000 40000

 Q2 135000 88000 47000

 Q3 142000 91000 51000

```
</details>

### classic89_nutrition_label_with_image

- **Text Similarity:** 0.9379
- **Visual Average:** 0.98
- **Overall Score:** 0.9672
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3013 bytes, Reference=47194 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount per serving % Daily Value

+Nutrient Amount p% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

-Saturated Fat 0.5g 3%

+Saturated 0.5g 3%

 Sodium 160mg 7%

-Total Carbohydrate 22g 8%

-Dietary Fiber 3g 11%

+Total Carb22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.7312
- **Visual Average:** 0.9475
- **Overall Score:** 0.8715
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3112 bytes, Reference=47112 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
-Project Orion - Status Report

+Project Orion – Status Report

 Reporting Period: Q1 2025

-Milestone Due Date Owner Status

-Requirements Freeze Jan 15 PM Team Complete

-Architecture Review Feb 1 Tech Lead Complete

-Alpha Release Feb 28 Dev Team In Progress

-Beta Testing Mar 31 QA Team Not Started

-Production Deploy Apr 15 DevOps Not Started
+MilestoneDue DateOwner Status

+RequiremeJan 15 PM Team Complete

+ArchitectuFeb 1 Tech Lead Complete

+Alpha ReleFeb 28 Dev Team In Progress

+Beta Testi Mar 31 QA Team Not Started

+ProductionApr 15 DevOps Not Started
```
</details>

## Improvement Suggestions

The following test cases scored below 0.8 and need attention:

1. **classic09_long_text** (score: 0.2192)
1. **classic56_alternating_row_colors** (score: 0.7539)

Review the text diffs and visual comparisons above to identify specific rendering issues.
