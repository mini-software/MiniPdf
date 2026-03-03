# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-03T02:13:17.312509

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9924 | 1/1 | **0.997** |
| 2 | 🟢 classic02_multiple_worksheets | 0.9971 | 0.9959 | 3/3 | **0.9972** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9997 | 1/1 | **0.9999** |
| 5 | 🟢 classic05_wide_table | 1.0 | 0.9868 | 3/3 | **0.9947** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.9309 | 5/5 | **0.9724** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.9972 | 1/1 | **0.9989** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9943 | 1/1 | **0.9977** |
| 9 | 🔴 classic09_long_text | 0.9319 | 0.0805 | 1/12 | **0.505** |
| 10 | 🟢 classic10_special_xml_characters | 1.0 | 0.994 | 1/1 | **0.9976** |
| 11 | 🟢 classic11_sparse_rows | 1.0 | 0.9988 | 2/2 | **0.9995** |
| 12 | 🟢 classic12_sparse_columns | 1.0 | 0.9979 | 1/1 | **0.9992** |
| 13 | 🟢 classic13_date_strings | 0.949 | 0.9901 | 1/1 | **0.9756** |
| 14 | 🟢 classic14_decimal_numbers | 1.0 | 0.9937 | 1/1 | **0.9975** |
| 15 | 🟢 classic15_negative_numbers | 1.0 | 0.9946 | 1/1 | **0.9978** |
| 16 | 🟢 classic16_percentage_strings | 1.0 | 0.993 | 1/1 | **0.9972** |
| 17 | 🟢 classic17_currency_strings | 0.9662 | 0.9917 | 1/1 | **0.9832** |
| 18 | 🟢 classic18_large_dataset | 1.0 | 0.8676 | 24/24 | **0.947** |
| 19 | 🟢 classic19_single_column_list | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 20 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 21 | 🟢 classic21_header_only | 1.0 | 0.9988 | 1/1 | **0.9995** |
| 22 | 🟢 classic22_long_sheet_name | 1.0 | 0.9984 | 1/1 | **0.9994** |
| 23 | 🟢 classic23_unicode_text | 0.8017 | 0.9928 | 1/1 | **0.9178** |
| 24 | 🟢 classic24_red_text | 1.0 | 0.9924 | 1/1 | **0.997** |
| 25 | 🟢 classic25_multiple_colors | 0.9978 | 0.989 | 1/1 | **0.9947** |
| 26 | 🟢 classic26_inline_strings | 1.0 | 0.9966 | 1/1 | **0.9986** |
| 27 | 🟢 classic27_single_row | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 28 | 🟢 classic28_duplicate_values | 1.0 | 0.993 | 1/1 | **0.9972** |
| 29 | 🟢 classic29_formula_results | 1.0 | 0.9925 | 1/1 | **0.997** |
| 30 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.9986 | 2/2 | **0.9994** |
| 31 | 🟢 classic31_bold_header_row | 0.996 | 0.9896 | 1/1 | **0.9942** |
| 32 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.9954 | 1/1 | **0.9982** |
| 33 | 🟢 classic33_centered_text | 1.0 | 0.998 | 1/1 | **0.9992** |
| 34 | 🟢 classic34_explicit_column_widths | 1.0 | 0.9913 | 1/1 | **0.9965** |
| 35 | 🟢 classic35_explicit_row_heights | 0.9647 | 0.9943 | 1/1 | **0.9836** |
| 36 | 🟢 classic36_merged_cells | 0.963 | 0.9911 | 1/1 | **0.9816** |
| 37 | 🟢 classic37_freeze_panes | 1.0 | 0.9824 | 1/1 | **0.993** |
| 38 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9961 | 1/1 | **0.9984** |
| 39 | 🟢 classic39_financial_table | 1.0 | 0.9872 | 1/1 | **0.9949** |
| 40 | 🟢 classic40_scientific_notation | 0.8991 | 0.9893 | 1/1 | **0.9554** |
| 41 | 🟢 classic41_integer_vs_float | 0.9453 | 0.9932 | 1/1 | **0.9754** |
| 42 | 🟢 classic42_boolean_values | 0.9947 | 0.9913 | 1/1 | **0.9944** |
| 43 | 🟢 classic43_inventory_report | 0.9984 | 0.9774 | 1/1 | **0.9903** |
| 44 | 🟢 classic44_employee_roster | 0.9683 | 0.967 | 1/1 | **0.9741** |
| 45 | 🟢 classic45_sales_by_region | 1.0 | 0.9943 | 4/4 | **0.9977** |
| 46 | 🟢 classic46_grade_book | 1.0 | 0.9841 | 1/1 | **0.9936** |
| 47 | 🟢 classic47_time_series | 1.0 | 0.9728 | 1/1 | **0.9891** |
| 48 | 🟢 classic48_survey_results | 0.9828 | 0.9858 | 1/1 | **0.9874** |
| 49 | 🟢 classic49_contact_list | 0.9005 | 0.9738 | 1/1 | **0.9497** |
| 50 | 🟢 classic50_budget_vs_actuals | 0.9978 | 0.9788 | 3/3 | **0.9906** |
| 51 | 🟢 classic51_product_catalog | 0.9771 | 0.9747 | 1/1 | **0.9807** |
| 52 | 🟢 classic52_pivot_summary | 0.9956 | 0.9784 | 1/1 | **0.9896** |
| 53 | 🟢 classic53_invoice | 0.9984 | 0.9836 | 1/1 | **0.9928** |
| 54 | 🟢 classic54_multi_level_header | 1.0 | 0.9812 | 1/1 | **0.9925** |
| 55 | 🟢 classic55_error_values | 1.0 | 0.9878 | 1/1 | **0.9951** |
| 56 | 🟡 classic56_alternating_row_colors | 1.0 | 0.7141 | 1/1 | **0.8856** |
| 57 | 🟡 classic57_cjk_only | 0.7624 | 0.8689 | 1/1 | **0.8525** |
| 58 | 🟢 classic58_mixed_numeric_formats | 0.9028 | 0.9904 | 1/1 | **0.9573** |
| 59 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9923 | 4/4 | **0.9969** |
| 60 | 🟢 classic60_large_wide_table | 1.0 | 0.9091 | 4/4 | **0.9636** |
| 61 | 🟢 classic61_product_card_with_image | 1.0 | 0.9624 | 1/1 | **0.985** |
| 62 | 🟢 classic62_company_logo_header | 0.996 | 0.9714 | 1/1 | **0.987** |
| 63 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9569 | 1/1 | **0.9828** |
| 64 | 🟢 classic64_employee_directory_with_photo | 0.9835 | 0.9698 | 1/1 | **0.9813** |
| 65 | 🟢 classic65_inventory_with_product_photos | 0.9779 | 0.9554 | 1/1 | **0.9733** |
| 66 | 🟢 classic66_invoice_with_logo | 0.9868 | 0.9721 | 1/1 | **0.9836** |
| 67 | 🟢 classic67_real_estate_listing | 1.0 | 0.9508 | 1/1 | **0.9803** |
| 68 | 🟢 classic68_restaurant_menu | 0.9881 | 0.927 | 1/1 | **0.966** |
| 69 | 🟢 classic69_image_only_sheet | 1.0 | 0.9321 | 1/1 | **0.9728** |
| 70 | 🟢 classic70_product_catalog_with_images | 0.9793 | 0.9408 | 1/1 | **0.968** |
| 71 | 🟢 classic71_multi_sheet_with_images | 0.9966 | 0.9819 | 3/3 | **0.9914** |
| 72 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.9358 | 1/1 | **0.9743** |
| 73 | 🟢 classic73_event_flyer_with_banner | 0.9939 | 0.92 | 1/1 | **0.9656** |
| 74 | 🟢 classic74_dashboard_with_kpi_image | 0.947 | 0.9439 | 1/1 | **0.9564** |
| 75 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9686 | 1/1 | **0.9874** |
| 76 | 🟢 classic76_product_image_grid | 1.0 | 0.9464 | 1/1 | **0.9786** |
| 77 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.9178 | 1/1 | **0.9671** |
| 78 | 🟢 classic78_small_icon_per_row | 0.9863 | 0.9799 | 1/1 | **0.9865** |
| 79 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9099 | 1/1 | **0.964** |
| 80 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9646 | 1/1 | **0.9858** |
| 81 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.9432 | 1/1 | **0.9773** |
| 82 | 🟢 classic82_before_after_images | 0.9704 | 0.9352 | 1/1 | **0.9622** |
| 83 | 🟢 classic83_color_swatch_palette | 0.9862 | 0.9608 | 1/1 | **0.9788** |
| 84 | 🟢 classic84_travel_destination_cards | 1.0 | 0.912 | 1/1 | **0.9648** |
| 85 | 🟢 classic85_lab_results_with_image | 0.8277 | 0.9594 | 1/1 | **0.9148** |
| 86 | 🟢 classic86_software_screenshot_features | 0.9932 | 0.9513 | 1/1 | **0.9778** |
| 87 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.9834 | 1/1 | **0.9934** |
| 88 | 🟢 classic88_image_after_data | 0.997 | 0.9714 | 1/1 | **0.9874** |
| 89 | 🟢 classic89_nutrition_label_with_image | 0.9878 | 0.9695 | 1/1 | **0.9829** |
| 90 | 🟢 classic90_project_status_with_milestones | 0.9067 | 0.9463 | 1/1 | **0.9412** |

**Average Overall Score: 0.9757**

## Visual Comparison

<table>
  <thead>
    <tr>
      <th>Test Case</th>
      <th>MiniPdf</th>
      <th>LibreOffice (Reference)</th>
      <th>Score</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td valign="top"><b>classic01_basic_table_with_headers</b></td>
      <td><img src="images/classic01_basic_table_with_headers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic01_basic_table_with_headers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.997</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic02_multiple_worksheets</b><br><small>p1</small></td>
      <td><img src="images/classic02_multiple_worksheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic02_multiple_worksheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic02_multiple_worksheets_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic02_multiple_worksheets_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic02_multiple_worksheets_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic02_multiple_worksheets_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic03_empty_workbook</b></td>
      <td><img src="images/classic03_empty_workbook_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic03_empty_workbook_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 1.0</td>
    </tr>
    <tr>
      <td valign="top"><b>classic04_single_cell</b></td>
      <td><img src="images/classic04_single_cell_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic04_single_cell_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9999</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic05_wide_table</b><br><small>p1</small></td>
      <td><img src="images/classic05_wide_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic05_wide_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9947</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic05_wide_table_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic05_wide_table_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic05_wide_table_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic05_wide_table_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td rowspan="5" valign="top"><b>classic06_tall_table</b><br><small>p1</small></td>
      <td><img src="images/classic06_tall_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic06_tall_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="5" valign="top"><span style="color:#3fb950">⬤</span> 0.9724</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic06_tall_table_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic06_tall_table_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic06_tall_table_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic06_tall_table_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic06_tall_table_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic06_tall_table_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td align="center"><small>p5</small></td>
      <td><img src="images/classic06_tall_table_p5_minipdf.png" width="340" alt="MiniPdf p5"></td>
      <td><img src="images/classic06_tall_table_p5_reference.png" width="340" alt="Reference p5"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic07_numbers_only</b></td>
      <td><img src="images/classic07_numbers_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic07_numbers_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9989</td>
    </tr>
    <tr>
      <td valign="top"><b>classic08_mixed_text_and_numbers</b></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9977</td>
    </tr>
    <tr>
      <td rowspan="12" valign="top"><b>classic09_long_text</b><br><small>p1</small></td>
      <td><img src="images/classic09_long_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic09_long_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="12" valign="top"><span style="color:#f85149">⬤</span> 0.505</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td align="center"><small>p5</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p5_reference.png" width="340" alt="Reference p5"></td>
    </tr>
    <tr>
      <td align="center"><small>p6</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p6_reference.png" width="340" alt="Reference p6"></td>
    </tr>
    <tr>
      <td align="center"><small>p7</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p7_reference.png" width="340" alt="Reference p7"></td>
    </tr>
    <tr>
      <td align="center"><small>p8</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p8_reference.png" width="340" alt="Reference p8"></td>
    </tr>
    <tr>
      <td align="center"><small>p9</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p9_reference.png" width="340" alt="Reference p9"></td>
    </tr>
    <tr>
      <td align="center"><small>p10</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p10_reference.png" width="340" alt="Reference p10"></td>
    </tr>
    <tr>
      <td align="center"><small>p11</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p11_reference.png" width="340" alt="Reference p11"></td>
    </tr>
    <tr>
      <td align="center"><small>p12</small></td>
      <td><i>missing</i></td>
      <td><img src="images/classic09_long_text_p12_reference.png" width="340" alt="Reference p12"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic10_special_xml_characters</b></td>
      <td><img src="images/classic10_special_xml_characters_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic10_special_xml_characters_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic11_sparse_rows</b><br><small>p1</small></td>
      <td><img src="images/classic11_sparse_rows_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic11_sparse_rows_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9995</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic11_sparse_rows_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic11_sparse_rows_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic12_sparse_columns</b></td>
      <td><img src="images/classic12_sparse_columns_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic12_sparse_columns_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
    </tr>
    <tr>
      <td valign="top"><b>classic13_date_strings</b></td>
      <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9756</td>
    </tr>
    <tr>
      <td valign="top"><b>classic14_decimal_numbers</b></td>
      <td><img src="images/classic14_decimal_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic14_decimal_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic15_negative_numbers</b></td>
      <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic16_percentage_strings</b></td>
      <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td valign="top"><b>classic17_currency_strings</b></td>
      <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9832</td>
    </tr>
    <tr>
      <td rowspan="24" valign="top"><b>classic18_large_dataset</b><br><small>p1</small></td>
      <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="24" valign="top"><span style="color:#3fb950">⬤</span> 0.947</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic18_large_dataset_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic18_large_dataset_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic18_large_dataset_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic18_large_dataset_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic18_large_dataset_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic18_large_dataset_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td align="center"><small>p5</small></td>
      <td><img src="images/classic18_large_dataset_p5_minipdf.png" width="340" alt="MiniPdf p5"></td>
      <td><img src="images/classic18_large_dataset_p5_reference.png" width="340" alt="Reference p5"></td>
    </tr>
    <tr>
      <td align="center"><small>p6</small></td>
      <td><img src="images/classic18_large_dataset_p6_minipdf.png" width="340" alt="MiniPdf p6"></td>
      <td><img src="images/classic18_large_dataset_p6_reference.png" width="340" alt="Reference p6"></td>
    </tr>
    <tr>
      <td align="center"><small>p7</small></td>
      <td><img src="images/classic18_large_dataset_p7_minipdf.png" width="340" alt="MiniPdf p7"></td>
      <td><img src="images/classic18_large_dataset_p7_reference.png" width="340" alt="Reference p7"></td>
    </tr>
    <tr>
      <td align="center"><small>p8</small></td>
      <td><img src="images/classic18_large_dataset_p8_minipdf.png" width="340" alt="MiniPdf p8"></td>
      <td><img src="images/classic18_large_dataset_p8_reference.png" width="340" alt="Reference p8"></td>
    </tr>
    <tr>
      <td align="center"><small>p9</small></td>
      <td><img src="images/classic18_large_dataset_p9_minipdf.png" width="340" alt="MiniPdf p9"></td>
      <td><img src="images/classic18_large_dataset_p9_reference.png" width="340" alt="Reference p9"></td>
    </tr>
    <tr>
      <td align="center"><small>p10</small></td>
      <td><img src="images/classic18_large_dataset_p10_minipdf.png" width="340" alt="MiniPdf p10"></td>
      <td><img src="images/classic18_large_dataset_p10_reference.png" width="340" alt="Reference p10"></td>
    </tr>
    <tr>
      <td align="center"><small>p11</small></td>
      <td><img src="images/classic18_large_dataset_p11_minipdf.png" width="340" alt="MiniPdf p11"></td>
      <td><img src="images/classic18_large_dataset_p11_reference.png" width="340" alt="Reference p11"></td>
    </tr>
    <tr>
      <td align="center"><small>p12</small></td>
      <td><img src="images/classic18_large_dataset_p12_minipdf.png" width="340" alt="MiniPdf p12"></td>
      <td><img src="images/classic18_large_dataset_p12_reference.png" width="340" alt="Reference p12"></td>
    </tr>
    <tr>
      <td align="center"><small>p13</small></td>
      <td><img src="images/classic18_large_dataset_p13_minipdf.png" width="340" alt="MiniPdf p13"></td>
      <td><img src="images/classic18_large_dataset_p13_reference.png" width="340" alt="Reference p13"></td>
    </tr>
    <tr>
      <td align="center"><small>p14</small></td>
      <td><img src="images/classic18_large_dataset_p14_minipdf.png" width="340" alt="MiniPdf p14"></td>
      <td><img src="images/classic18_large_dataset_p14_reference.png" width="340" alt="Reference p14"></td>
    </tr>
    <tr>
      <td align="center"><small>p15</small></td>
      <td><img src="images/classic18_large_dataset_p15_minipdf.png" width="340" alt="MiniPdf p15"></td>
      <td><img src="images/classic18_large_dataset_p15_reference.png" width="340" alt="Reference p15"></td>
    </tr>
    <tr>
      <td align="center"><small>p16</small></td>
      <td><img src="images/classic18_large_dataset_p16_minipdf.png" width="340" alt="MiniPdf p16"></td>
      <td><img src="images/classic18_large_dataset_p16_reference.png" width="340" alt="Reference p16"></td>
    </tr>
    <tr>
      <td align="center"><small>p17</small></td>
      <td><img src="images/classic18_large_dataset_p17_minipdf.png" width="340" alt="MiniPdf p17"></td>
      <td><img src="images/classic18_large_dataset_p17_reference.png" width="340" alt="Reference p17"></td>
    </tr>
    <tr>
      <td align="center"><small>p18</small></td>
      <td><img src="images/classic18_large_dataset_p18_minipdf.png" width="340" alt="MiniPdf p18"></td>
      <td><img src="images/classic18_large_dataset_p18_reference.png" width="340" alt="Reference p18"></td>
    </tr>
    <tr>
      <td align="center"><small>p19</small></td>
      <td><img src="images/classic18_large_dataset_p19_minipdf.png" width="340" alt="MiniPdf p19"></td>
      <td><img src="images/classic18_large_dataset_p19_reference.png" width="340" alt="Reference p19"></td>
    </tr>
    <tr>
      <td align="center"><small>p20</small></td>
      <td><img src="images/classic18_large_dataset_p20_minipdf.png" width="340" alt="MiniPdf p20"></td>
      <td><img src="images/classic18_large_dataset_p20_reference.png" width="340" alt="Reference p20"></td>
    </tr>
    <tr>
      <td align="center"><small>p21</small></td>
      <td><img src="images/classic18_large_dataset_p21_minipdf.png" width="340" alt="MiniPdf p21"></td>
      <td><img src="images/classic18_large_dataset_p21_reference.png" width="340" alt="Reference p21"></td>
    </tr>
    <tr>
      <td align="center"><small>p22</small></td>
      <td><img src="images/classic18_large_dataset_p22_minipdf.png" width="340" alt="MiniPdf p22"></td>
      <td><img src="images/classic18_large_dataset_p22_reference.png" width="340" alt="Reference p22"></td>
    </tr>
    <tr>
      <td align="center"><small>p23</small></td>
      <td><img src="images/classic18_large_dataset_p23_minipdf.png" width="340" alt="MiniPdf p23"></td>
      <td><img src="images/classic18_large_dataset_p23_reference.png" width="340" alt="Reference p23"></td>
    </tr>
    <tr>
      <td align="center"><small>p24</small></td>
      <td><img src="images/classic18_large_dataset_p24_minipdf.png" width="340" alt="MiniPdf p24"></td>
      <td><img src="images/classic18_large_dataset_p24_reference.png" width="340" alt="Reference p24"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic19_single_column_list</b></td>
      <td><img src="images/classic19_single_column_list_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic19_single_column_list_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic20_all_empty_cells</b></td>
      <td><img src="images/classic20_all_empty_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic20_all_empty_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 1.0</td>
    </tr>
    <tr>
      <td valign="top"><b>classic21_header_only</b></td>
      <td><img src="images/classic21_header_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic21_header_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic22_long_sheet_name</b></td>
      <td><img src="images/classic22_long_sheet_name_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic22_long_sheet_name_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9994</td>
    </tr>
    <tr>
      <td valign="top"><b>classic23_unicode_text</b></td>
      <td><img src="images/classic23_unicode_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic23_unicode_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9178</td>
    </tr>
    <tr>
      <td valign="top"><b>classic24_red_text</b></td>
      <td><img src="images/classic24_red_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic24_red_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.997</td>
    </tr>
    <tr>
      <td valign="top"><b>classic25_multiple_colors</b></td>
      <td><img src="images/classic25_multiple_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic25_multiple_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9947</td>
    </tr>
    <tr>
      <td valign="top"><b>classic26_inline_strings</b></td>
      <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
    </tr>
    <tr>
      <td valign="top"><b>classic27_single_row</b></td>
      <td><img src="images/classic27_single_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic27_single_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9994</td>
    </tr>
    <tr>
      <td valign="top"><b>classic28_duplicate_values</b></td>
      <td><img src="images/classic28_duplicate_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic28_duplicate_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td valign="top"><b>classic29_formula_results</b></td>
      <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.997</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic30_mixed_empty_and_filled_sheets</b><br><small>p1</small></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9994</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic31_bold_header_row</b></td>
      <td><img src="images/classic31_bold_header_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic31_bold_header_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic32_right_aligned_numbers</b></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9982</td>
    </tr>
    <tr>
      <td valign="top"><b>classic33_centered_text</b></td>
      <td><img src="images/classic33_centered_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic33_centered_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
    </tr>
    <tr>
      <td valign="top"><b>classic34_explicit_column_widths</b></td>
      <td><img src="images/classic34_explicit_column_widths_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic34_explicit_column_widths_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9965</td>
    </tr>
    <tr>
      <td valign="top"><b>classic35_explicit_row_heights</b></td>
      <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9836</td>
    </tr>
    <tr>
      <td valign="top"><b>classic36_merged_cells</b></td>
      <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9816</td>
    </tr>
    <tr>
      <td valign="top"><b>classic37_freeze_panes</b></td>
      <td><img src="images/classic37_freeze_panes_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic37_freeze_panes_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic38_hyperlink_cell</b></td>
      <td><img src="images/classic38_hyperlink_cell_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic38_hyperlink_cell_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
    </tr>
    <tr>
      <td valign="top"><b>classic39_financial_table</b></td>
      <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9949</td>
    </tr>
    <tr>
      <td valign="top"><b>classic40_scientific_notation</b></td>
      <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9554</td>
    </tr>
    <tr>
      <td valign="top"><b>classic41_integer_vs_float</b></td>
      <td><img src="images/classic41_integer_vs_float_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic41_integer_vs_float_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9754</td>
    </tr>
    <tr>
      <td valign="top"><b>classic42_boolean_values</b></td>
      <td><img src="images/classic42_boolean_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic42_boolean_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9944</td>
    </tr>
    <tr>
      <td valign="top"><b>classic43_inventory_report</b></td>
      <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9903</td>
    </tr>
    <tr>
      <td valign="top"><b>classic44_employee_roster</b></td>
      <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9741</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic45_sales_by_region</b><br><small>p1</small></td>
      <td><img src="images/classic45_sales_by_region_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic45_sales_by_region_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9977</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic45_sales_by_region_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic45_sales_by_region_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic45_sales_by_region_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic45_sales_by_region_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic45_sales_by_region_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic45_sales_by_region_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic46_grade_book</b></td>
      <td><img src="images/classic46_grade_book_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic46_grade_book_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9936</td>
    </tr>
    <tr>
      <td valign="top"><b>classic47_time_series</b></td>
      <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9891</td>
    </tr>
    <tr>
      <td valign="top"><b>classic48_survey_results</b></td>
      <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9874</td>
    </tr>
    <tr>
      <td valign="top"><b>classic49_contact_list</b></td>
      <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9497</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic50_budget_vs_actuals</b><br><small>p1</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9906</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic50_budget_vs_actuals_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic50_budget_vs_actuals_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic51_product_catalog</b></td>
      <td><img src="images/classic51_product_catalog_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic51_product_catalog_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9807</td>
    </tr>
    <tr>
      <td valign="top"><b>classic52_pivot_summary</b></td>
      <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9896</td>
    </tr>
    <tr>
      <td valign="top"><b>classic53_invoice</b></td>
      <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9928</td>
    </tr>
    <tr>
      <td valign="top"><b>classic54_multi_level_header</b></td>
      <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9925</td>
    </tr>
    <tr>
      <td valign="top"><b>classic55_error_values</b></td>
      <td><img src="images/classic55_error_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic55_error_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9951</td>
    </tr>
    <tr>
      <td valign="top"><b>classic56_alternating_row_colors</b></td>
      <td><img src="images/classic56_alternating_row_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic56_alternating_row_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8856</td>
    </tr>
    <tr>
      <td valign="top"><b>classic57_cjk_only</b></td>
      <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8525</td>
    </tr>
    <tr>
      <td valign="top"><b>classic58_mixed_numeric_formats</b></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9573</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic59_multi_sheet_summary</b><br><small>p1</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9969</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic59_multi_sheet_summary_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic59_multi_sheet_summary_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic59_multi_sheet_summary_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic60_large_wide_table</b><br><small>p1</small></td>
      <td><img src="images/classic60_large_wide_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic60_large_wide_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9636</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic60_large_wide_table_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic60_large_wide_table_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic60_large_wide_table_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic60_large_wide_table_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic60_large_wide_table_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic60_large_wide_table_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic61_product_card_with_image</b></td>
      <td><img src="images/classic61_product_card_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic61_product_card_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.985</td>
    </tr>
    <tr>
      <td valign="top"><b>classic62_company_logo_header</b></td>
      <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.987</td>
    </tr>
    <tr>
      <td valign="top"><b>classic63_two_products_side_by_side</b></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9828</td>
    </tr>
    <tr>
      <td valign="top"><b>classic64_employee_directory_with_photo</b></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9813</td>
    </tr>
    <tr>
      <td valign="top"><b>classic65_inventory_with_product_photos</b></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9733</td>
    </tr>
    <tr>
      <td valign="top"><b>classic66_invoice_with_logo</b></td>
      <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9836</td>
    </tr>
    <tr>
      <td valign="top"><b>classic67_real_estate_listing</b></td>
      <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9803</td>
    </tr>
    <tr>
      <td valign="top"><b>classic68_restaurant_menu</b></td>
      <td><img src="images/classic68_restaurant_menu_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic68_restaurant_menu_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.966</td>
    </tr>
    <tr>
      <td valign="top"><b>classic69_image_only_sheet</b></td>
      <td><img src="images/classic69_image_only_sheet_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic69_image_only_sheet_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9728</td>
    </tr>
    <tr>
      <td valign="top"><b>classic70_product_catalog_with_images</b></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.968</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic71_multi_sheet_with_images</b><br><small>p1</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9914</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic72_bar_chart_image_with_data</b></td>
      <td><img src="images/classic72_bar_chart_image_with_data_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic72_bar_chart_image_with_data_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9743</td>
    </tr>
    <tr>
      <td valign="top"><b>classic73_event_flyer_with_banner</b></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9656</td>
    </tr>
    <tr>
      <td valign="top"><b>classic74_dashboard_with_kpi_image</b></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9564</td>
    </tr>
    <tr>
      <td valign="top"><b>classic75_certificate_with_seal</b></td>
      <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9874</td>
    </tr>
    <tr>
      <td valign="top"><b>classic76_product_image_grid</b></td>
      <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9786</td>
    </tr>
    <tr>
      <td valign="top"><b>classic77_news_article_with_hero_image</b></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9671</td>
    </tr>
    <tr>
      <td valign="top"><b>classic78_small_icon_per_row</b></td>
      <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9865</td>
    </tr>
    <tr>
      <td valign="top"><b>classic79_wide_panoramic_banner</b></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic80_portrait_tall_image</b></td>
      <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9858</td>
    </tr>
    <tr>
      <td valign="top"><b>classic81_step_by_step_with_images</b></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9773</td>
    </tr>
    <tr>
      <td valign="top"><b>classic82_before_after_images</b></td>
      <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9622</td>
    </tr>
    <tr>
      <td valign="top"><b>classic83_color_swatch_palette</b></td>
      <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9788</td>
    </tr>
    <tr>
      <td valign="top"><b>classic84_travel_destination_cards</b></td>
      <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9648</td>
    </tr>
    <tr>
      <td valign="top"><b>classic85_lab_results_with_image</b></td>
      <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9148</td>
    </tr>
    <tr>
      <td valign="top"><b>classic86_software_screenshot_features</b></td>
      <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9778</td>
    </tr>
    <tr>
      <td valign="top"><b>classic87_sports_results_with_logos</b></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9934</td>
    </tr>
    <tr>
      <td valign="top"><b>classic88_image_after_data</b></td>
      <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9874</td>
    </tr>
    <tr>
      <td valign="top"><b>classic89_nutrition_label_with_image</b></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9829</td>
    </tr>
    <tr>
      <td valign="top"><b>classic90_project_status_with_milestones</b></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9412</td>
    </tr>
  </tbody>
</table>

## Detailed Results

### classic01_basic_table_with_headers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9924
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1317 bytes, Reference=30311 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 0.9971
- **Visual Average:** 0.9959
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2295 bytes, Reference=36003 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic02_multiple_worksheets.pdf
+++ reference/classic02_multiple_worksheets.pdf
@@ -11,5 +11,5 @@
 ---PAGE---

 Metric Value

 Total Reve 1130

-Total Cost 3700

+Total Costs 3700

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
- **Visual Average:** 0.9997
- **Overall Score:** 0.9999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=623 bytes, Reference=19860 bytes

Text content: ✅ Identical

### classic05_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9868
- **Overall Score:** 0.9947
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=8588 bytes, Reference=62308 bytes

Text content: ✅ Identical

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9309
- **Overall Score:** 0.9724
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=39094 bytes, Reference=185703 bytes

Text content: ✅ Identical

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9972
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1136 bytes, Reference=24806 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9943
- **Overall Score:** 0.9977
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1162 bytes, Reference=27336 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.9319
- **Visual Average:** 0.0805
- **Overall Score:** 0.505
- **Pages:** MiniPdf=1, Reference=12
- **File Size:** MiniPdf=2807 bytes, Reference=29170 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,5 +1,26 @@
 Long Text Column

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

 Short

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
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
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=968 bytes, Reference=27644 bytes

Text content: ✅ Identical

### classic11_sparse_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.9988
- **Overall Score:** 0.9995
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1037 bytes, Reference=23538 bytes

Text content: ✅ Identical

### classic12_sparse_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=875 bytes, Reference=24923 bytes

Text content: ✅ Identical

### classic13_date_strings

- **Text Similarity:** 0.949
- **Visual Average:** 0.9901
- **Overall Score:** 0.9756
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1199 bytes, Reference=29104 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic13_date_strings.pdf
+++ reference/classic13_date_strings.pdf
@@ -1,6 +1,6 @@
 Date Event

-2025-01-15Launch

-2025-06-30Release

-2025-12-25Holiday

-2026-01-01New Year

-2026-02-23Today
+2025-01-1 Launch

+2025-06-3 Release

+2025-12-2 Holiday

+2026-01-0 New Year

+2026-02-2 Today
```
</details>

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9937
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1176 bytes, Reference=29057 bytes

Text content: ✅ Identical

### classic15_negative_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9946
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1265 bytes, Reference=28526 bytes

Text content: ✅ Identical

### classic16_percentage_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.993
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1177 bytes, Reference=29888 bytes

Text content: ✅ Identical

### classic17_currency_strings

- **Text Similarity:** 0.9662
- **Visual Average:** 0.9917
- **Overall Score:** 0.9832
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2292 bytes, Reference=29862 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic17_currency_strings.pdf
+++ reference/classic17_currency_strings.pdf
@@ -3,6 +3,5 @@
 Gadget $149.00

 Premium $1,299.99

 Budget $4.50

-Euro Item 49.99

-€

-Yen Item JPY5000
+Euro Item €49.99

+Yen Item ¥5000
```
</details>

### classic18_large_dataset

- **Text Similarity:** 1.0
- **Visual Average:** 0.8676
- **Overall Score:** 0.947
- **Pages:** MiniPdf=24, Reference=24
- **File Size:** MiniPdf=532792 bytes, Reference=2487195 bytes

Text content: ✅ Identical

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
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
- **Visual Average:** 0.9988
- **Overall Score:** 0.9995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=828 bytes, Reference=22034 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9984
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=865 bytes, Reference=23683 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.8017
- **Visual Average:** 0.9928
- **Overall Score:** 0.9178
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3070 bytes, Reference=67722 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic23_unicode_text.pdf
+++ reference/classic23_unicode_text.pdf
@@ -1,12 +1,9 @@
 Language Greeting Extra

 English Hello World

+你好 世界

 Chinese

-你好 世界

+こんにちは世界

 Japanese

-こんにちは世界

-Korean

-안녕하세요세계

-Arabic

-ملاعلاابحرم

-Emoji

-���� ✅❌
+Korean 안녕하세요세계

+Arabicمرحبا العالم

+Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9924
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1064 bytes, Reference=39031 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 0.9978
- **Visual Average:** 0.989
- **Overall Score:** 0.9947
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1771 bytes, Reference=43116 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic25_multiple_colors.pdf
+++ reference/classic25_multiple_colors.pdf
@@ -1,4 +1,4 @@
-Color NameSample Text

+Color NamSample Text

 Red This is red text

 Green This is green text

 Blue This is blue text

```
</details>

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9966
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1037 bytes, Reference=25018 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=923 bytes, Reference=23681 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.993
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1539 bytes, Reference=24729 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9925
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1451 bytes, Reference=27548 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9986
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1382 bytes, Reference=27418 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 0.996
- **Visual Average:** 0.9896
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1596 bytes, Reference=40714 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic31_bold_header_row.pdf
+++ reference/classic31_bold_header_row.pdf
@@ -1,5 +1,5 @@
 Product Category Price Stock

-Laptop Electronic 999.99 50

+Laptop Electronics 999.99 50

 Desk Furniture 349 20

 Pen Stationery 1.99 500

 Chair Furniture 199 30
```
</details>

### classic32_right_aligned_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9954
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=978 bytes, Reference=27582 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.998
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1301 bytes, Reference=26648 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 1.0
- **Visual Average:** 0.9913
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1200 bytes, Reference=28834 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Text Similarity:** 0.9647
- **Visual Average:** 0.9943
- **Overall Score:** 0.9836
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=881 bytes, Reference=25108 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
-Tall Heade Value

+Tall HeadeValue

 Extra Tall 42

-Normal Row10
+Normal Ro 10
```
</details>

### classic36_merged_cells

- **Text Similarity:** 0.963
- **Visual Average:** 0.9911
- **Overall Score:** 0.9816
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1102 bytes, Reference=27256 bytes

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
- **Visual Average:** 0.9824
- **Overall Score:** 0.993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4689 bytes, Reference=46420 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9961
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=907 bytes, Reference=26279 bytes

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
- **Visual Average:** 0.9872
- **Overall Score:** 0.9949
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2047 bytes, Reference=43383 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 0.8991
- **Visual Average:** 0.9893
- **Overall Score:** 0.9554
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1212 bytes, Reference=30852 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic40_scientific_notation.pdf
+++ reference/classic40_scientific_notation.pdf
@@ -1,6 +1,6 @@
 Label Value

-Avogadro 6.022E+23

+Avogadro 6.02E+23

 Planck 6.626E-34

-Speed of L 299800000

-Electron m 9.109E-31

-Pi approx 3.14159265358979
+Speed of L 3E+08

+Electron m9.109E-31

+Pi approx 3.141593
```
</details>

### classic41_integer_vs_float

- **Text Similarity:** 0.9453
- **Visual Average:** 0.9932
- **Overall Score:** 0.9754
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1453 bytes, Reference=29637 bytes

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

- **Text Similarity:** 0.9947
- **Visual Average:** 0.9913
- **Overall Score:** 0.9944
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1190 bytes, Reference=28631 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

 Dark Mode TRUE

-Notificati FALSE

+Notificatio FALSE

 Auto-save TRUE

 Analytics FALSE

 Beta Featu TRUE
```
</details>

### classic43_inventory_report

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9774
- **Overall Score:** 0.9903
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3045 bytes, Reference=49849 bytes

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

- **Text Similarity:** 0.9683
- **Visual Average:** 0.967
- **Overall Score:** 0.9741
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3476 bytes, Reference=43656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

-1001 Alice Smith Engineerin Senior Eng alice@example.com

-1002 Bob Jones Marketing Marketing bob@example.com

+1001 Alice Smith EngineerinSenior Engalice@example.com

+1002 Bob Jones MarketingMarketingbob@example.com

 1003 Carol Williams HR HR Specialcarol@example.com

-1004 David Brown Engineerin Junior Eng david@example.com

-1005 Eve Davis Finance Financial eve@example.com

-1006 Frank Miller Sales Sales Repr frank@example.com

-1007 Grace Wilson Engineerin Tech Lead grace@example.com

+1004 David Brown EngineerinJunior Engdavid@example.com

+1005 Eve Davis Finance Financial Aeve@example.com

+1006 Frank Miller Sales Sales Reprfrank@example.com

+1007 Grace Wilson EngineerinTech Lead grace@example.com

 1008 Henry Moore Support Support Sphenry@example.com
```
</details>

### classic45_sales_by_region

- **Text Similarity:** 1.0
- **Visual Average:** 0.9943
- **Overall Score:** 0.9977
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3174 bytes, Reference=37087 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.9841
- **Overall Score:** 0.9936
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3325 bytes, Reference=40993 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.9728
- **Overall Score:** 0.9891
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6941 bytes, Reference=55976 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9828
- **Visual Average:** 0.9858
- **Overall Score:** 0.9874
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2378 bytes, Reference=36069 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
-Question StrongAgreAgree Neutral Disagree StrongDisagree

+Question StrongAgr Agree Neutral Disagree StrongDisagree

 Easy to us 30 45 15 7 3

-Recommend25 40 20 10 5

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

-Good suppo35 40 15 7 3

+Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.9005
- **Visual Average:** 0.9738
- **Overall Score:** 0.9497
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2728 bytes, Reference=41523 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smit +1-555-010alice@examNew York USA

-Bob Jones +44-20-794bob@examplLondon UK

-Carol Wang+86-10-123carol@examBeijing China

-David Mull +49-30-123david@examBerlin Germany

-Eve Martin +33-1-23-4 eve@examplParis France

-Frank Tana+81-3-1234frank@examTokyo Japan

-Grace Kim +82-2-1234grace@exam Seoul Korea
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

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9788
- **Overall Score:** 0.9906
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=6550 bytes, Reference=54986 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic50_budget_vs_actuals.pdf
+++ reference/classic50_budget_vs_actuals.pdf
@@ -1,18 +1,18 @@
-DepartmentQ1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin 200000 200000 210000 220000 830000

 Marketing 80000 90000 85000 95000 350000

 Sales 120000 130000 140000 150000 540000

 HR 40000 40000 42000 43000 165000

 Finance 35000 35000 37000 38000 145000

 ---PAGE---

-DepartmentQ1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin 195000 205000 215000 225000 840000

 Marketing 82000 88000 91000 97000 358000

 Sales 118000 135000 142000 148000 543000

 HR 39000 41000 41500 44000 165500

 Finance 34000 36000 37500 39000 146500

 ---PAGE---

-DepartmentQ1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin -5000 5000 5000 5000 10000

 Marketing 2000 -2000 6000 2000 8000

 Sales -2000 5000 2000 -2000 3000

```
</details>

### classic51_product_catalog

- **Text Similarity:** 0.9771
- **Visual Average:** 0.9747
- **Overall Score:** 0.9807
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3468 bytes, Reference=44297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
 Part# Name Descriptio Weight(g) Price

-P-001 Basic WidgStandard w150 4.99

-P-002 Pro Widget Enhanced w180 12.99

-P-003 Mini GadgeCompact ga90 19.99

-P-004 Max GadgetFull-size 450 89.99

-P-005 Connector Type-A con80 7.49

-P-006 Connector Type-B con110 9.99

+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgeEnhanced 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size g 450 89.99

+P-005 ConnectorType-A con 80 7.49

+P-006 ConnectorType-B con 110 9.99

 P-007 Adapter X Universal 200 15.99

 P-008 Adapter Y Travel pow 120 11.99

-P-009 Mount BracWall mount600 24.99

-P-010 Carry CasePadded car350 34.99
+P-009 Mount BraWall moun 600 24.99

+P-010 Carry CasePadded ca 350 34.99
```
</details>

### classic52_pivot_summary

- **Text Similarity:** 0.9956
- **Visual Average:** 0.9784
- **Overall Score:** 0.9896
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2426 bytes, Reference=44493 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic52_pivot_summary.pdf
+++ reference/classic52_pivot_summary.pdf
@@ -1,4 +1,4 @@
-Region Electronic Furniture Clothing Food Total

+Region ElectronicsFurniture Clothing Food Total

 North 45000 12000 8000 22000 87000

 South 38000 15000 11000 25000 89000

 East 52000 9000 14000 18000 93000

```
</details>

### classic53_invoice

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9836
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2516 bytes, Reference=53425 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -6,7 +6,7 @@
 ACME Corporation

 123 Business Rd, Suite 400

 New York, NY 10001

-Item Qty Unit Price Total

+Item Qty Unit PriceTotal

 Consulting 10 150 1500

 Software L 5 99 495

 Hardware 2 249.99 499.98

```
</details>

### classic54_multi_level_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.9812
- **Overall Score:** 0.9925
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2150 bytes, Reference=38782 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9878
- **Overall Score:** 0.9951
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1650 bytes, Reference=34677 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.7141
- **Overall Score:** 0.8856
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2201 bytes, Reference=32363 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Text Similarity:** 0.7624
- **Visual Average:** 0.8689
- **Overall Score:** 0.8525
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3137 bytes, Reference=88207 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic57_cjk_only.pdf
+++ reference/classic57_cjk_only.pdf
@@ -1,11 +1,11 @@
-序号 产品名称 价格 库存

+序号 产品名称价格 库存

+笔记本电脑

 1 5999 100

-笔记本电脑

+智能手机

 2 2999 250

-智能手机

+平板电脑

 3 1999 150

-平板电脑

+蓝牙耳机

 4 299 500

-蓝牙耳机

-5 99 1000

-充电器
+充电器

+5 99 1000
```
</details>

### classic58_mixed_numeric_formats

- **Text Similarity:** 0.9028
- **Visual Average:** 0.9904
- **Overall Score:** 0.9573
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1602 bytes, Reference=32815 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic58_mixed_numeric_formats.pdf
+++ reference/classic58_mixed_numeric_formats.pdf
@@ -2,9 +2,9 @@
 Integer 1000000

 Float 2dp 3.14

 Float 5dp 3.14159

-Negative i -42

-Negative f -3.14

+Negative in -42

+Negative fl -3.14

 Very small 0.0001

-Very large 9999999.99

+Very large 10000000

 Zero 0

-Scientific 12300000000
+Scientific 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9923
- **Overall Score:** 0.9969
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=4336 bytes, Reference=44781 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9091
- **Overall Score:** 0.9636
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=55057 bytes, Reference=263350 bytes

Text content: ✅ Identical

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9624
- **Overall Score:** 0.985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2056 bytes, Reference=36974 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 0.996
- **Visual Average:** 0.9714
- **Overall Score:** 0.987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2750 bytes, Reference=42880 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic62_company_logo_header.pdf
+++ reference/classic62_company_logo_header.pdf
@@ -1,6 +1,6 @@
 ACME Corporation

 Annual Report 2025

-DepartmentQ1 Q2 Q3 Q4

+DepartmenQ1 Q2 Q3 Q4

 Sales 120 135 142 160

 Engineerin 85 90 95 100

 Marketing 60 65 70 75
```
</details>

### classic63_two_products_side_by_side

- **Text Similarity:** 1.0
- **Visual Average:** 0.9569
- **Overall Score:** 0.9828
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3067 bytes, Reference=28933 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.9835
- **Visual Average:** 0.9698
- **Overall Score:** 0.9813
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4403 bytes, Reference=43408 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title DepartmentEmail

-Alice Chen Engineer R&D alice@example.com

-Bob Smith Manager Sales bob@example.com

-Carol WangDesigner UX carol@example.com
+Photo Name Title DepartmeEmail

+Alice ChenEngineer R&D alice@example.com

+Bob SmithManager Sales bob@example.com

+Carol WanDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9779
- **Visual Average:** 0.9554
- **Overall Score:** 0.9733
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6598 bytes, Reference=48227 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic65_inventory_with_product_photos.pdf
+++ reference/classic65_inventory_with_product_photos.pdf
@@ -1,6 +1,6 @@
 Image SKU Name Qty Price

-SKU-001 Red Widget50 9.99

-SKU-002 Blue Gadge30 14.99

-SKU-003 Green Tool100 4.49

-SKU-004 Yellow Dev25 29.99

-SKU-005 Purple Gea75 7.99
+SKU-001 Red Widge 50 9.99

+SKU-002 Blue Gadge 30 14.99

+SKU-003 Green Too 100 4.49

+SKU-004 Yellow Dev 25 29.99

+SKU-005 Purple Gea 75 7.99
```
</details>

### classic66_invoice_with_logo

- **Text Similarity:** 0.9868
- **Visual Average:** 0.9721
- **Overall Score:** 0.9836
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2710 bytes, Reference=45034 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -1,8 +1,8 @@
 INVOICE

 Invoice #: INV-20250301

 Date: 2025-03-01

-Descriptio Qty Unit Price Total

+DescriptiQty Unit PriceTotal

 Consulting 8 150 1200

 Software L 1 299 299

-Support Pa1 99 99

+Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Text Similarity:** 1.0
- **Visual Average:** 0.9508
- **Overall Score:** 0.9803
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2739 bytes, Reference=44030 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Text Similarity:** 0.9881
- **Visual Average:** 0.927
- **Overall Score:** 0.966
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5250 bytes, Reference=47320 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,9 +1,9 @@
 Today's Menu

-Grilled Sa $18.99

+Grilled S $18.99

 Fresh Atlantic salmon with herbs

-Caesar Sal $12.99

+Caesar Sa $12.99

 Romaine lettuce, croutons, parmesan

-Beef Burge$14.99

+Beef Burg $14.99

 8oz Angus beef, brioche bun

-Pasta Prim $13.99

+Pasta Pri $13.99

 Seasonal vegetables, olive oil
```
</details>

### classic69_image_only_sheet

- **Text Similarity:** 1.0
- **Visual Average:** 0.9321
- **Overall Score:** 0.9728
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2459 bytes, Reference=8905 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Text Similarity:** 0.9793
- **Visual Average:** 0.9408
- **Overall Score:** 0.968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4410 bytes, Reference=44156 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

-Classic Pe $3.99

+Classic P $3.99

 A reliable ballpoint pen

-Leather No $12.99

+Leather $12.99

 Premium A5 notebook

-Desk Organ$24.99

+Desk Orga $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 0.9966
- **Visual Average:** 0.9819
- **Overall Score:** 0.9914
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=5021 bytes, Reference=37419 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic71_multi_sheet_with_images.pdf
+++ reference/classic71_multi_sheet_with_images.pdf
@@ -6,6 +6,6 @@
 Digital 50000

 Print 20000

 ---PAGE---

-DepartmentHeadcount

+DepartmenHeadcount

 Engineerin 45

 Sales 30
```
</details>

### classic72_bar_chart_image_with_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9358
- **Overall Score:** 0.9743
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3076 bytes, Reference=41342 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9939
- **Visual Average:** 0.92
- **Overall Score:** 0.9656
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3071 bytes, Reference=44512 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -3,7 +3,7 @@
 Venue: Convention Center Hall A

 Speakers: 20+ Industry Leaders

 Time Session Speaker

-09:00 Opening KeDr. Jane Kim

-10:30 AI in Prac Prof. Mark Liu

-13:00 Cloud Arch Eng. Sara Patel

+09:00 Opening KDr. Jane Kim

+10:30 AI in Pract Prof. Mark Liu

+13:00 Cloud ArchEng. Sara Patel

 15:00 Panel Disc All Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.947
- **Visual Average:** 0.9439
- **Overall Score:** 0.9564
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4121 bytes, Reference=48755 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -1,10 +1,6 @@
 Executive Dashboard Q1 2025

 KPI Target Actual Status

-Revenue 500000 523000 Above

-✓

-New Custom200 187 Below

-✗

-NPS Score 70 74 Above

-✓

-Churn Rate< 3% 2.8% Above

-✓
+Revenue 500000 523000 ✓ Above

+New Custo 200 187  Below ✗

+NPS Score 70 74 ✓ Above

+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Text Similarity:** 1.0
- **Visual Average:** 0.9686
- **Overall Score:** 0.9874
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1981 bytes, Reference=39135 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9464
- **Overall Score:** 0.9786
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5002 bytes, Reference=39017 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9178
- **Overall Score:** 0.9671
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2719 bytes, Reference=52664 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9863
- **Visual Average:** 0.9799
- **Overall Score:** 0.9865
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6064 bytes, Reference=41646 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login Alice Done

+Fix login b Alice Done

 Write unit Bob In Progress

 Deploy to Carol Pending

-Code revie Alice Done

-Update docDave In Progress
+Code revieAlice Done

+Update doDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9099
- **Overall Score:** 0.964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2889 bytes, Reference=43015 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9646
- **Overall Score:** 0.9858
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2325 bytes, Reference=39079 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9432
- **Overall Score:** 0.9773
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5110 bytes, Reference=47175 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9704
- **Visual Average:** 0.9352
- **Overall Score:** 0.9622
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4978 bytes, Reference=42486 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -1,6 +1,5 @@
 Before After

-Old design legacy UI New design modern UI

-– –

+Old design – legacy UI New design – modern UI

 Metric Before After Delta

 Load time 4.2s 1.1s -74%

 Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Text Similarity:** 0.9862
- **Visual Average:** 0.9608
- **Overall Score:** 0.9788
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6915 bytes, Reference=45933 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

-Primary Bl RGB(0, 82, 165)

+Primary BlRGB(0, 82, 165)

 Primary ReRGB(197, 27, 50)

-Accent GreRGB(0, 163, 108)

-Neutral Gr RGB(128, 128, 128)

-Warm YelloRGB(255, 193, 7)

+Accent Gr RGB(0, 163, 108)

+Neutral GrRGB(128, 128, 128)

+Warm YellRGB(255, 193, 7)

 Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Text Similarity:** 1.0
- **Visual Average:** 0.912
- **Overall Score:** 0.9648
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4377 bytes, Reference=42524 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.8277
- **Visual Average:** 0.9594
- **Overall Score:** 0.9148
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4708 bytes, Reference=47866 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,12 +1,7 @@
 Sample Analysis Report

-Parameter Value Unit Reference Flag

-pH 7.35 7.35 7.4 Normal

-–

-Glucose 5.2 mmol/L 3.9 5.5 Normal

-–

-Sodium 142 mEq/L 136 145 Normal

-–

-Potassium 5 mEq/L 3.5 5.0 Normal

-–

-Creatinine 1.4 mg/dL 0.6 1.2 High

-–
+ParameteValue Unit ReferenceFlag

+pH 7.35 7.35 – 7.45Normal

+Glucose 5.2 mmol/L 3.9 – 5.5 Normal

+Sodium 142 mEq/L 136 – 145 Normal

+Potassium 5 mEq/L 3.5 – 5.0 Normal

+Creatinine 1.4 mg/dL 0.6 – 1.2 High
```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.9932
- **Visual Average:** 0.9513
- **Overall Score:** 0.9778
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2837 bytes, Reference=41961 bytes

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

 Cloud SyncYes

-Offline Mo Yes

+Offline MoYes

 API AccessPro only

 Export to Yes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9834
- **Overall Score:** 0.9934
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5584 bytes, Reference=47076 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 0.997
- **Visual Average:** 0.9714
- **Overall Score:** 0.9874
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2880 bytes, Reference=43273 bytes

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

- **Text Similarity:** 0.9878
- **Visual Average:** 0.9695
- **Overall Score:** 0.9829
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3076 bytes, Reference=47194 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount per% Daily Value

+Nutrient Amount p% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

 Saturated 0.5g 3%

 Sodium 160mg 7%

-Total Carb 22g 8%

-Dietary Fi 3g 11%

+Total Carb22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.9067
- **Visual Average:** 0.9463
- **Overall Score:** 0.9412
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4215 bytes, Reference=47112 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,9 +1,8 @@
-Project Orion Status Report

-–

+Project Orion – Status Report

 Reporting Period: Q1 2025

-Milestone Due Date Owner Status

-RequiremenJan 15 PM Team Complete

-Architectu Feb 1 Tech Lead Complete

-Alpha Rele Feb 28 Dev Team In Progress

+MilestoneDue DateOwner Status

+RequiremeJan 15 PM Team Complete

+ArchitectuFeb 1 Tech Lead Complete

+Alpha ReleFeb 28 Dev Team In Progress

 Beta Testi Mar 31 QA Team Not Started

-Production Apr 15 DevOps Not Started
+ProductionApr 15 DevOps Not Started
```
</details>

## Improvement Suggestions

### ⚠ Low-Score Test Cases (below 0.8)

1. **classic09_long_text** (score: 0.505)

Review the text diffs and visual comparisons above to identify specific rendering issues.
