# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-04T16:56:18.049864

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9938 | 1/1 | **0.9975** |
| 2 | 🟢 classic02_multiple_worksheets | 0.9942 | 0.9957 | 3/3 | **0.996** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9997 | 1/1 | **0.9999** |
| 5 | 🟢 classic05_wide_table | 1.0 | 0.9886 | 3/3 | **0.9954** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.9292 | 5/5 | **0.9717** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.9969 | 1/1 | **0.9988** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9956 | 1/1 | **0.9982** |
| 9 | 🔴 classic09_long_text | 0.6809 | 0.6604 | 8/12 | **0.6365** |
| 10 | 🟢 classic100_stacked_bar_chart | 0.9348 | 0.9123 | 1/1 | **0.9388** |
| 11 | 🟢 classic101_percent_stacked_bar | 0.9273 | 0.8771 | 1/1 | **0.9218** |
| 12 | 🟢 classic102_line_chart_with_markers | 0.8531 | 0.988 | 2/2 | **0.9364** |
| 13 | 🟡 classic103_pie_chart_with_labels | 0.68 | 0.9742 | 2/2 | **0.8617** |
| 14 | 🟡 classic104_combo_bar_line_chart | 0.7708 | 0.7542 | 2/2 | **0.81** |
| 15 | 🟡 classic105_3d_bar_chart | 0.8832 | 0.7434 | 2/2 | **0.8506** |
| 16 | 🟢 classic106_3d_pie_chart | 0.9558 | 0.968 | 2/2 | **0.9695** |
| 17 | 🟡 classic107_multi_series_line | 0.7566 | 0.7733 | 2/2 | **0.812** |
| 18 | 🟢 classic108_stacked_area_chart | 0.931 | 0.8935 | 1/1 | **0.9298** |
| 19 | 🟢 classic109_scatter_with_trendline | 0.784 | 0.9861 | 2/2 | **0.908** |
| 20 | 🟢 classic10_special_xml_characters | 1.0 | 0.994 | 1/1 | **0.9976** |
| 21 | 🟡 classic110_chart_with_legend | 0.8 | 0.7795 | 2/2 | **0.8318** |
| 22 | 🟢 classic111_chart_with_axis_labels | 0.8267 | 0.977 | 2/2 | **0.9215** |
| 23 | 🟡 classic112_multiple_charts | 0.8769 | 0.7583 | 2/2 | **0.8541** |
| 24 | 🟡 classic113_chart_sheet | 0.9259 | 0.7331 | 2/2 | **0.8636** |
| 25 | 🟢 classic114_chart_large_dataset | 0.9322 | 0.8858 | 4/4 | **0.9272** |
| 26 | 🟢 classic115_chart_negative_values | 0.8789 | 0.9721 | 2/2 | **0.9404** |
| 27 | 🟢 classic116_percent_stacked_area | 0.9322 | 0.8786 | 1/1 | **0.9243** |
| 28 | 🟡 classic117_stock_ohlc_chart | 0.7817 | 0.7223 | 2/2 | **0.8016** |
| 29 | 🟢 classic118_bar_chart_custom_colors | 0.9565 | 0.9609 | 2/2 | **0.967** |
| 30 | 🟢 classic119_dashboard_multi_charts | 0.9149 | 0.9342 | 2/2 | **0.9396** |
| 31 | 🟢 classic11_sparse_rows | 1.0 | 0.9991 | 2/2 | **0.9996** |
| 32 | 🟡 classic120_chart_with_date_axis | 0.5574 | 0.7839 | 2/2 | **0.7365** |
| 33 | 🟢 classic12_sparse_columns | 1.0 | 0.9974 | 1/1 | **0.999** |
| 34 | 🟢 classic13_date_strings | 0.9738 | 0.9927 | 1/1 | **0.9866** |
| 35 | 🟢 classic14_decimal_numbers | 1.0 | 0.9946 | 1/1 | **0.9978** |
| 36 | 🟢 classic15_negative_numbers | 0.908 | 0.9957 | 1/1 | **0.9615** |
| 37 | 🟢 classic16_percentage_strings | 0.9877 | 0.9936 | 1/1 | **0.9925** |
| 38 | 🟢 classic17_currency_strings | 1.0 | 0.9924 | 1/1 | **0.997** |
| 39 | 🟢 classic18_large_dataset | 1.0 | 0.8692 | 24/24 | **0.9477** |
| 40 | 🟢 classic19_single_column_list | 1.0 | 0.9944 | 1/1 | **0.9978** |
| 41 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 42 | 🟢 classic21_header_only | 1.0 | 0.9983 | 1/1 | **0.9993** |
| 43 | 🟢 classic22_long_sheet_name | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 44 | 🟢 classic23_unicode_text | 0.8033 | 0.9935 | 1/1 | **0.9187** |
| 45 | 🟢 classic24_red_text | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 46 | 🟢 classic25_multiple_colors | 0.9955 | 0.9911 | 1/1 | **0.9946** |
| 47 | 🟢 classic26_inline_strings | 1.0 | 0.9967 | 1/1 | **0.9987** |
| 48 | 🟢 classic27_single_row | 1.0 | 0.9979 | 1/1 | **0.9992** |
| 49 | 🟢 classic28_duplicate_values | 1.0 | 0.994 | 1/1 | **0.9976** |
| 50 | 🟢 classic29_formula_results | 1.0 | 0.9931 | 1/1 | **0.9972** |
| 51 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.9982 | 2/2 | **0.9993** |
| 52 | 🟢 classic31_bold_header_row | 0.996 | 0.9912 | 1/1 | **0.9949** |
| 53 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.9949 | 1/1 | **0.998** |
| 54 | 🟢 classic33_centered_text | 1.0 | 0.9978 | 1/1 | **0.9991** |
| 55 | 🟢 classic34_explicit_column_widths | 1.0 | 0.9897 | 1/1 | **0.9959** |
| 56 | 🟢 classic35_explicit_row_heights | 0.9639 | 0.9964 | 1/1 | **0.9841** |
| 57 | 🟢 classic36_merged_cells | 0.9936 | 0.9914 | 1/1 | **0.994** |
| 58 | 🟢 classic37_freeze_panes | 1.0 | 0.9848 | 1/1 | **0.9939** |
| 59 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9963 | 1/1 | **0.9985** |
| 60 | 🟢 classic39_financial_table | 1.0 | 0.9892 | 1/1 | **0.9957** |
| 61 | 🟢 classic40_scientific_notation | 0.9388 | 0.9934 | 1/1 | **0.9729** |
| 62 | 🟢 classic41_integer_vs_float | 1.0 | 0.9945 | 1/1 | **0.9978** |
| 63 | 🟢 classic42_boolean_values | 0.9894 | 0.9927 | 1/1 | **0.9928** |
| 64 | 🟢 classic43_inventory_report | 0.9984 | 0.9807 | 1/1 | **0.9916** |
| 65 | 🟢 classic44_employee_roster | 0.9581 | 0.9722 | 1/1 | **0.9721** |
| 66 | 🟢 classic45_sales_by_region | 1.0 | 0.9951 | 4/4 | **0.998** |
| 67 | 🟢 classic46_grade_book | 1.0 | 0.9861 | 1/1 | **0.9944** |
| 68 | 🟢 classic47_time_series | 1.0 | 0.978 | 1/1 | **0.9912** |
| 69 | 🟢 classic48_survey_results | 0.9913 | 0.9879 | 1/1 | **0.9917** |
| 70 | 🟢 classic49_contact_list | 0.8712 | 0.9809 | 1/1 | **0.9408** |
| 71 | 🟢 classic50_budget_vs_actuals | 0.9933 | 0.9812 | 3/3 | **0.9898** |
| 72 | 🟢 classic51_product_catalog | 0.9573 | 0.9799 | 1/1 | **0.9749** |
| 73 | 🟢 classic52_pivot_summary | 0.9956 | 0.9815 | 1/1 | **0.9908** |
| 74 | 🟢 classic53_invoice | 0.9918 | 0.9868 | 1/1 | **0.9914** |
| 75 | 🟢 classic54_multi_level_header | 1.0 | 0.9854 | 1/1 | **0.9942** |
| 76 | 🟢 classic55_error_values | 1.0 | 0.9901 | 1/1 | **0.996** |
| 77 | 🟢 classic56_alternating_row_colors | 0.997 | 0.9859 | 1/1 | **0.9932** |
| 78 | 🟡 classic57_cjk_only | 0.7826 | 0.9095 | 1/1 | **0.8768** |
| 79 | 🟢 classic58_mixed_numeric_formats | 0.9579 | 0.9918 | 1/1 | **0.9799** |
| 80 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9932 | 4/4 | **0.9973** |
| 81 | 🟢 classic60_large_wide_table | 1.0 | 0.9129 | 4/4 | **0.9652** |
| 82 | 🟢 classic61_product_card_with_image | 1.0 | 0.9941 | 1/1 | **0.9976** |
| 83 | 🟢 classic62_company_logo_header | 0.9879 | 0.9908 | 1/1 | **0.9915** |
| 84 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9927 | 1/1 | **0.9971** |
| 85 | 🟢 classic64_employee_directory_with_photo | 0.9833 | 0.9884 | 1/1 | **0.9887** |
| 86 | 🟢 classic65_inventory_with_product_photos | 0.9809 | 0.987 | 1/1 | **0.9872** |
| 87 | 🟢 classic66_invoice_with_logo | 0.9766 | 0.992 | 1/1 | **0.9874** |
| 88 | 🟢 classic67_real_estate_listing | 0.9966 | 0.9932 | 1/1 | **0.9959** |
| 89 | 🟢 classic68_restaurant_menu | 0.9904 | 0.9769 | 1/1 | **0.9869** |
| 90 | 🟢 classic69_image_only_sheet | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 91 | 🟢 classic70_product_catalog_with_images | 0.9895 | 0.9864 | 1/1 | **0.9904** |
| 92 | 🟢 classic71_multi_sheet_with_images | 0.9896 | 0.995 | 3/3 | **0.9938** |
| 93 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.9826 | 1/1 | **0.993** |
| 94 | 🟢 classic73_event_flyer_with_banner | 0.9361 | 0.9912 | 1/1 | **0.9709** |
| 95 | 🟢 classic74_dashboard_with_kpi_image | 0.956 | 0.9815 | 1/1 | **0.975** |
| 96 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9855 | 1/1 | **0.9942** |
| 97 | 🟢 classic76_product_image_grid | 1.0 | 0.9834 | 1/1 | **0.9934** |
| 98 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.9855 | 1/1 | **0.9942** |
| 99 | 🟢 classic78_small_icon_per_row | 0.9897 | 0.9909 | 1/1 | **0.9922** |
| 100 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9928 | 1/1 | **0.9971** |
| 101 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9874 | 1/1 | **0.995** |
| 102 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.984 | 1/1 | **0.9936** |
| 103 | 🟢 classic82_before_after_images | 0.9926 | 0.9875 | 1/1 | **0.992** |
| 104 | 🟢 classic83_color_swatch_palette | 0.9916 | 0.9871 | 1/1 | **0.9915** |
| 105 | 🟢 classic84_travel_destination_cards | 1.0 | 0.9841 | 1/1 | **0.9936** |
| 106 | 🟢 classic85_lab_results_with_image | 0.991 | 0.9871 | 1/1 | **0.9912** |
| 107 | 🟢 classic86_software_screenshot_features | 0.9827 | 0.9937 | 1/1 | **0.9906** |
| 108 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.9922 | 1/1 | **0.9969** |
| 109 | 🟢 classic88_image_after_data | 0.997 | 0.9877 | 1/1 | **0.9939** |
| 110 | 🟢 classic89_nutrition_label_with_image | 0.9878 | 0.9908 | 1/1 | **0.9914** |
| 111 | 🟢 classic90_project_status_with_milestones | 0.9492 | 0.9782 | 1/1 | **0.971** |
| 112 | 🟢 classic91_simple_bar_chart | 0.9585 | 0.9617 | 2/2 | **0.9681** |
| 113 | 🟢 classic92_horizontal_bar_chart | 0.9535 | 0.9673 | 2/2 | **0.9683** |
| 114 | 🟢 classic93_line_chart | 0.8624 | 0.9863 | 2/2 | **0.9395** |
| 115 | 🟢 classic94_pie_chart | 0.9967 | 0.9312 | 2/2 | **0.9712** |
| 116 | 🟡 classic95_area_chart | 0.678 | 0.7651 | 2/2 | **0.7772** |
| 117 | 🟢 classic96_scatter_chart | 0.7943 | 0.9861 | 2/2 | **0.9122** |
| 118 | 🟢 classic97_doughnut_chart | 1.0 | 0.938 | 2/2 | **0.9752** |
| 119 | 🟢 classic98_radar_chart | 0.8869 | 0.99 | 2/2 | **0.9508** |
| 120 | 🟢 classic99_bubble_chart | 0.8245 | 0.9706 | 2/2 | **0.918** |

**Average Overall Score: 0.9639**

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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic02_multiple_worksheets</b><br><small>p1</small></td>
      <td><img src="images/classic02_multiple_worksheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic02_multiple_worksheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.996</td>
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
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9954</td>
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
      <td rowspan="5" valign="top"><span style="color:#3fb950">⬤</span> 0.9717</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic08_mixed_text_and_numbers</b></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9982</td>
    </tr>
    <tr>
      <td rowspan="12" valign="top"><b>classic09_long_text</b><br><small>p1</small></td>
      <td><img src="images/classic09_long_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic09_long_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="12" valign="top"><span style="color:#f85149">⬤</span> 0.6365</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic09_long_text_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic09_long_text_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic09_long_text_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic09_long_text_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic09_long_text_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic09_long_text_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td align="center"><small>p5</small></td>
      <td><img src="images/classic09_long_text_p5_minipdf.png" width="340" alt="MiniPdf p5"></td>
      <td><img src="images/classic09_long_text_p5_reference.png" width="340" alt="Reference p5"></td>
    </tr>
    <tr>
      <td align="center"><small>p6</small></td>
      <td><img src="images/classic09_long_text_p6_minipdf.png" width="340" alt="MiniPdf p6"></td>
      <td><img src="images/classic09_long_text_p6_reference.png" width="340" alt="Reference p6"></td>
    </tr>
    <tr>
      <td align="center"><small>p7</small></td>
      <td><img src="images/classic09_long_text_p7_minipdf.png" width="340" alt="MiniPdf p7"></td>
      <td><img src="images/classic09_long_text_p7_reference.png" width="340" alt="Reference p7"></td>
    </tr>
    <tr>
      <td align="center"><small>p8</small></td>
      <td><img src="images/classic09_long_text_p8_minipdf.png" width="340" alt="MiniPdf p8"></td>
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
      <td valign="top"><b>classic100_stacked_bar_chart</b></td>
      <td><img src="images/classic100_stacked_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic100_stacked_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9388</td>
    </tr>
    <tr>
      <td valign="top"><b>classic101_percent_stacked_bar</b></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9218</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic102_line_chart_with_markers</b><br><small>p1</small></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9364</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic102_line_chart_with_markers_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic102_line_chart_with_markers_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic103_pie_chart_with_labels</b><br><small>p1</small></td>
      <td><img src="images/classic103_pie_chart_with_labels_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic103_pie_chart_with_labels_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8617</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic103_pie_chart_with_labels_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic103_pie_chart_with_labels_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic104_combo_bar_line_chart</b><br><small>p1</small></td>
      <td><img src="images/classic104_combo_bar_line_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic104_combo_bar_line_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.81</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic104_combo_bar_line_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic104_combo_bar_line_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic105_3d_bar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic105_3d_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic105_3d_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8506</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic105_3d_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic105_3d_bar_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic106_3d_pie_chart</b><br><small>p1</small></td>
      <td><img src="images/classic106_3d_pie_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic106_3d_pie_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9695</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic106_3d_pie_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic106_3d_pie_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic107_multi_series_line</b><br><small>p1</small></td>
      <td><img src="images/classic107_multi_series_line_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic107_multi_series_line_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.812</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic107_multi_series_line_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic107_multi_series_line_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic108_stacked_area_chart</b></td>
      <td><img src="images/classic108_stacked_area_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic108_stacked_area_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9298</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic109_scatter_with_trendline</b><br><small>p1</small></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.908</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic109_scatter_with_trendline_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic109_scatter_with_trendline_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic10_special_xml_characters</b></td>
      <td><img src="images/classic10_special_xml_characters_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic10_special_xml_characters_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic110_chart_with_legend</b><br><small>p1</small></td>
      <td><img src="images/classic110_chart_with_legend_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic110_chart_with_legend_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8318</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic110_chart_with_legend_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic110_chart_with_legend_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic111_chart_with_axis_labels</b><br><small>p1</small></td>
      <td><img src="images/classic111_chart_with_axis_labels_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic111_chart_with_axis_labels_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9215</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic111_chart_with_axis_labels_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic111_chart_with_axis_labels_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic112_multiple_charts</b><br><small>p1</small></td>
      <td><img src="images/classic112_multiple_charts_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic112_multiple_charts_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8541</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic112_multiple_charts_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic112_multiple_charts_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic113_chart_sheet</b><br><small>p1</small></td>
      <td><img src="images/classic113_chart_sheet_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic113_chart_sheet_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8636</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic113_chart_sheet_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic113_chart_sheet_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic114_chart_large_dataset</b><br><small>p1</small></td>
      <td><img src="images/classic114_chart_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic114_chart_large_dataset_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9272</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic114_chart_large_dataset_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic114_chart_large_dataset_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic114_chart_large_dataset_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic114_chart_large_dataset_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic114_chart_large_dataset_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic114_chart_large_dataset_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic115_chart_negative_values</b><br><small>p1</small></td>
      <td><img src="images/classic115_chart_negative_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic115_chart_negative_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9404</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic115_chart_negative_values_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic115_chart_negative_values_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic116_percent_stacked_area</b></td>
      <td><img src="images/classic116_percent_stacked_area_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic116_percent_stacked_area_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9243</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic117_stock_ohlc_chart</b><br><small>p1</small></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8016</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic117_stock_ohlc_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic117_stock_ohlc_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic118_bar_chart_custom_colors</b><br><small>p1</small></td>
      <td><img src="images/classic118_bar_chart_custom_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic118_bar_chart_custom_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.967</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic118_bar_chart_custom_colors_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic118_bar_chart_custom_colors_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic119_dashboard_multi_charts</b><br><small>p1</small></td>
      <td><img src="images/classic119_dashboard_multi_charts_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic119_dashboard_multi_charts_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9396</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic119_dashboard_multi_charts_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic119_dashboard_multi_charts_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic11_sparse_rows</b><br><small>p1</small></td>
      <td><img src="images/classic11_sparse_rows_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic11_sparse_rows_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9996</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic11_sparse_rows_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic11_sparse_rows_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic120_chart_with_date_axis</b><br><small>p1</small></td>
      <td><img src="images/classic120_chart_with_date_axis_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic120_chart_with_date_axis_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7365</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic120_chart_with_date_axis_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic120_chart_with_date_axis_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic12_sparse_columns</b></td>
      <td><img src="images/classic12_sparse_columns_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic12_sparse_columns_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.999</td>
    </tr>
    <tr>
      <td valign="top"><b>classic13_date_strings</b></td>
      <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9866</td>
    </tr>
    <tr>
      <td valign="top"><b>classic14_decimal_numbers</b></td>
      <td><img src="images/classic14_decimal_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic14_decimal_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic15_negative_numbers</b></td>
      <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9615</td>
    </tr>
    <tr>
      <td valign="top"><b>classic16_percentage_strings</b></td>
      <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9925</td>
    </tr>
    <tr>
      <td valign="top"><b>classic17_currency_strings</b></td>
      <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.997</td>
    </tr>
    <tr>
      <td rowspan="24" valign="top"><b>classic18_large_dataset</b><br><small>p1</small></td>
      <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="24" valign="top"><span style="color:#3fb950">⬤</span> 0.9477</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic22_long_sheet_name</b></td>
      <td><img src="images/classic22_long_sheet_name_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic22_long_sheet_name_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic23_unicode_text</b></td>
      <td><img src="images/classic23_unicode_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic23_unicode_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9187</td>
    </tr>
    <tr>
      <td valign="top"><b>classic24_red_text</b></td>
      <td><img src="images/classic24_red_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic24_red_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9965</td>
    </tr>
    <tr>
      <td valign="top"><b>classic25_multiple_colors</b></td>
      <td><img src="images/classic25_multiple_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic25_multiple_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9946</td>
    </tr>
    <tr>
      <td valign="top"><b>classic26_inline_strings</b></td>
      <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
    </tr>
    <tr>
      <td valign="top"><b>classic27_single_row</b></td>
      <td><img src="images/classic27_single_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic27_single_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
    </tr>
    <tr>
      <td valign="top"><b>classic28_duplicate_values</b></td>
      <td><img src="images/classic28_duplicate_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic28_duplicate_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic29_formula_results</b></td>
      <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic30_mixed_empty_and_filled_sheets</b><br><small>p1</small></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9993</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9949</td>
    </tr>
    <tr>
      <td valign="top"><b>classic32_right_aligned_numbers</b></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.998</td>
    </tr>
    <tr>
      <td valign="top"><b>classic33_centered_text</b></td>
      <td><img src="images/classic33_centered_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic33_centered_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9991</td>
    </tr>
    <tr>
      <td valign="top"><b>classic34_explicit_column_widths</b></td>
      <td><img src="images/classic34_explicit_column_widths_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic34_explicit_column_widths_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9959</td>
    </tr>
    <tr>
      <td valign="top"><b>classic35_explicit_row_heights</b></td>
      <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9841</td>
    </tr>
    <tr>
      <td valign="top"><b>classic36_merged_cells</b></td>
      <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.994</td>
    </tr>
    <tr>
      <td valign="top"><b>classic37_freeze_panes</b></td>
      <td><img src="images/classic37_freeze_panes_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic37_freeze_panes_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9939</td>
    </tr>
    <tr>
      <td valign="top"><b>classic38_hyperlink_cell</b></td>
      <td><img src="images/classic38_hyperlink_cell_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic38_hyperlink_cell_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
    </tr>
    <tr>
      <td valign="top"><b>classic39_financial_table</b></td>
      <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9957</td>
    </tr>
    <tr>
      <td valign="top"><b>classic40_scientific_notation</b></td>
      <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9729</td>
    </tr>
    <tr>
      <td valign="top"><b>classic41_integer_vs_float</b></td>
      <td><img src="images/classic41_integer_vs_float_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic41_integer_vs_float_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic42_boolean_values</b></td>
      <td><img src="images/classic42_boolean_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic42_boolean_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9928</td>
    </tr>
    <tr>
      <td valign="top"><b>classic43_inventory_report</b></td>
      <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9916</td>
    </tr>
    <tr>
      <td valign="top"><b>classic44_employee_roster</b></td>
      <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9721</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic45_sales_by_region</b><br><small>p1</small></td>
      <td><img src="images/classic45_sales_by_region_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic45_sales_by_region_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.998</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9944</td>
    </tr>
    <tr>
      <td valign="top"><b>classic47_time_series</b></td>
      <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9912</td>
    </tr>
    <tr>
      <td valign="top"><b>classic48_survey_results</b></td>
      <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9917</td>
    </tr>
    <tr>
      <td valign="top"><b>classic49_contact_list</b></td>
      <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9408</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic50_budget_vs_actuals</b><br><small>p1</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9898</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9749</td>
    </tr>
    <tr>
      <td valign="top"><b>classic52_pivot_summary</b></td>
      <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9908</td>
    </tr>
    <tr>
      <td valign="top"><b>classic53_invoice</b></td>
      <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9914</td>
    </tr>
    <tr>
      <td valign="top"><b>classic54_multi_level_header</b></td>
      <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic55_error_values</b></td>
      <td><img src="images/classic55_error_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic55_error_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.996</td>
    </tr>
    <tr>
      <td valign="top"><b>classic56_alternating_row_colors</b></td>
      <td><img src="images/classic56_alternating_row_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic56_alternating_row_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9932</td>
    </tr>
    <tr>
      <td valign="top"><b>classic57_cjk_only</b></td>
      <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8768</td>
    </tr>
    <tr>
      <td valign="top"><b>classic58_mixed_numeric_formats</b></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9799</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic59_multi_sheet_summary</b><br><small>p1</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9973</td>
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
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9652</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic62_company_logo_header</b></td>
      <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9915</td>
    </tr>
    <tr>
      <td valign="top"><b>classic63_two_products_side_by_side</b></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9971</td>
    </tr>
    <tr>
      <td valign="top"><b>classic64_employee_directory_with_photo</b></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9887</td>
    </tr>
    <tr>
      <td valign="top"><b>classic65_inventory_with_product_photos</b></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9872</td>
    </tr>
    <tr>
      <td valign="top"><b>classic66_invoice_with_logo</b></td>
      <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9874</td>
    </tr>
    <tr>
      <td valign="top"><b>classic67_real_estate_listing</b></td>
      <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9959</td>
    </tr>
    <tr>
      <td valign="top"><b>classic68_restaurant_menu</b></td>
      <td><img src="images/classic68_restaurant_menu_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic68_restaurant_menu_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9869</td>
    </tr>
    <tr>
      <td valign="top"><b>classic69_image_only_sheet</b></td>
      <td><img src="images/classic69_image_only_sheet_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic69_image_only_sheet_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9989</td>
    </tr>
    <tr>
      <td valign="top"><b>classic70_product_catalog_with_images</b></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9904</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic71_multi_sheet_with_images</b><br><small>p1</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9938</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic73_event_flyer_with_banner</b></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9709</td>
    </tr>
    <tr>
      <td valign="top"><b>classic74_dashboard_with_kpi_image</b></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic75_certificate_with_seal</b></td>
      <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic76_product_image_grid</b></td>
      <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9934</td>
    </tr>
    <tr>
      <td valign="top"><b>classic77_news_article_with_hero_image</b></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic78_small_icon_per_row</b></td>
      <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9922</td>
    </tr>
    <tr>
      <td valign="top"><b>classic79_wide_panoramic_banner</b></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9971</td>
    </tr>
    <tr>
      <td valign="top"><b>classic80_portrait_tall_image</b></td>
      <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic81_step_by_step_with_images</b></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9936</td>
    </tr>
    <tr>
      <td valign="top"><b>classic82_before_after_images</b></td>
      <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.992</td>
    </tr>
    <tr>
      <td valign="top"><b>classic83_color_swatch_palette</b></td>
      <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9915</td>
    </tr>
    <tr>
      <td valign="top"><b>classic84_travel_destination_cards</b></td>
      <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9936</td>
    </tr>
    <tr>
      <td valign="top"><b>classic85_lab_results_with_image</b></td>
      <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9912</td>
    </tr>
    <tr>
      <td valign="top"><b>classic86_software_screenshot_features</b></td>
      <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9906</td>
    </tr>
    <tr>
      <td valign="top"><b>classic87_sports_results_with_logos</b></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9969</td>
    </tr>
    <tr>
      <td valign="top"><b>classic88_image_after_data</b></td>
      <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9939</td>
    </tr>
    <tr>
      <td valign="top"><b>classic89_nutrition_label_with_image</b></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9914</td>
    </tr>
    <tr>
      <td valign="top"><b>classic90_project_status_with_milestones</b></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.971</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic91_simple_bar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic91_simple_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic91_simple_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9681</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic91_simple_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic91_simple_bar_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic92_horizontal_bar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic92_horizontal_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic92_horizontal_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9683</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic92_horizontal_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic92_horizontal_bar_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic93_line_chart</b><br><small>p1</small></td>
      <td><img src="images/classic93_line_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic93_line_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9395</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic93_line_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic93_line_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic94_pie_chart</b><br><small>p1</small></td>
      <td><img src="images/classic94_pie_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic94_pie_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9712</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic94_pie_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic94_pie_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic95_area_chart</b><br><small>p1</small></td>
      <td><img src="images/classic95_area_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic95_area_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7772</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic95_area_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic95_area_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic96_scatter_chart</b><br><small>p1</small></td>
      <td><img src="images/classic96_scatter_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic96_scatter_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9122</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic96_scatter_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic96_scatter_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic97_doughnut_chart</b><br><small>p1</small></td>
      <td><img src="images/classic97_doughnut_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic97_doughnut_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9752</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic97_doughnut_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic97_doughnut_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic98_radar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic98_radar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic98_radar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9508</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic98_radar_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic98_radar_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic99_bubble_chart</b><br><small>p1</small></td>
      <td><img src="images/classic99_bubble_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic99_bubble_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.918</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic99_bubble_chart_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic99_bubble_chart_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
  </tbody>
</table>

## Detailed Results

### classic01_basic_table_with_headers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1321 bytes, Reference=30311 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 0.9942
- **Visual Average:** 0.9957
- **Overall Score:** 0.996
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2302 bytes, Reference=36003 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic02_multiple_worksheets.pdf
+++ reference/classic02_multiple_worksheets.pdf
@@ -10,6 +10,6 @@
 Utilities 200

 ---PAGE---

 Metric Value

-Total Rev 1130

-Total Cost 3700

+Total Reve 1130

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
- **Visual Average:** 0.9886
- **Overall Score:** 0.9954
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=8588 bytes, Reference=62308 bytes

Text content: ✅ Identical

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9292
- **Overall Score:** 0.9717
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=39106 bytes, Reference=185703 bytes

Text content: ✅ Identical

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9969
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1166 bytes, Reference=24806 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9956
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1168 bytes, Reference=27336 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.6809
- **Visual Average:** 0.6604
- **Overall Score:** 0.6365
- **Pages:** MiniPdf=8, Reference=12
- **File Size:** MiniPdf=2623 bytes, Reference=29170 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,12 +1,22 @@
 Long Text Column

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

----PAGE---

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+Short

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

 

 ---PAGE---

-Short

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

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

 ---PAGE---

 

 ---PAGE---

```
</details>

### classic100_stacked_bar_chart

- **Text Similarity:** 0.9348
- **Visual Average:** 0.9123
- **Overall Score:** 0.9388
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4698 bytes, Reference=47565 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic100_stacked_bar_chart.pdf
+++ reference/classic100_stacked_bar_chart.pdf
@@ -4,12 +4,14 @@
 East 40 35 30 45

 West 20 25 40 35

 Quarterly Revenue by Region

-Q4 Q3 Q2 Q1

 180

 160

 140

-120

+120 Q4

+Q3

 100

+Q2

+Q1

 80

 60

 40

```
</details>

### classic101_percent_stacked_bar

- **Text Similarity:** 0.9273
- **Visual Average:** 0.8771
- **Overall Score:** 0.9218
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5402 bytes, Reference=49462 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic101_percent_stacked_bar.pdf
+++ reference/classic101_percent_stacked_bar.pdf
@@ -5,13 +5,15 @@
 2024 33 35 18 14

 2025 30 38 17 15

 Traffic Source Mix by Year

-Direct Referral Paid Organic

 100%

 90%

 80%

 70%

-60%

+Direct

+60% Referral

+Paid

 50%

+Organic

 40%

 30%

 20%

```
</details>

### classic102_line_chart_with_markers

- **Text Similarity:** 0.8531
- **Visual Average:** 0.988
- **Overall Score:** 0.9364
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4855 bytes, Reference=52236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic102_line_chart_with_markers.pdf
+++ reference/classic102_line_chart_with_markers.pdf
@@ -3,17 +3,20 @@
 Company Growth

 2021 25 120

 2022 55 280

-Users (K) Revenue (K)

 1200

 2023 90 500

 2024 140 780

 2025 200 1100

 1000

 800

+Value (K)

 600

-Value (K)

 400

 200

 0

-2020 2021 2022 2023 20

----PAGE---
+2020 2021 2022 2023 202

+---PAGE---

+h

+Users (K)

+Revenue (K)

+24 2025
```
</details>

### classic103_pie_chart_with_labels

- **Text Similarity:** 0.68
- **Visual Average:** 0.9742
- **Overall Score:** 0.8617
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=404235 bytes, Reference=48488 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic103_pie_chart_with_labels.pdf
+++ reference/classic103_pie_chart_with_labels.pdf
@@ -3,16 +3,26 @@
 Desktop OS Market Share

 macOS 28

 Linux 15

-ChromeO 10

-Other; Share (%); 5; 5%

+ChromeOS 10

+Other; Share

 Other 5

-ChromeOS; Share (%); 10; 10%

-Windows; Share (%); 42; 42%

-Linux; Share (%); 15; 15%

-macOS; Share (%); 28; 28%

-Windows

-macOS

-Linux

-ChromeOS

-Other

----PAGE---
+(%); 5; 5%

+ChromeOS;

+Share (%);

+10; 10%

+Wind

+mac

+Linux; Share

+Linu

+(%); 15; 15% Windows;

+Share (%); 42; Chro

+42%

+Othe

+macOS; Share

+(%); 28; 28%

+---PAGE---

+dows

+OS

+x

+omeOS

+er
```
</details>

### classic104_combo_bar_line_chart

- **Text Similarity:** 0.7708
- **Visual Average:** 0.7542
- **Overall Score:** 0.81
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4525 bytes, Reference=54330 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic104_combo_bar_line_chart.pdf
+++ reference/classic104_combo_bar_line_chart.pdf
@@ -3,17 +3,19 @@
 Sales vs Target

 Feb 48 47

 Mar 51 50

-Sales Target

-70

+70 70

 Apr 45 50

 May 56 54

-60

+60 60

 Jun 62 60

-50

-40

-30

-20

-10

-0

-Jan Feb Mar Apr May

----PAGE---
+50 50

+40 40

+30 30

+20 20

+10 10

+0 0

+Jan Jan Feb Feb Mar Mar Apr Apr M M

+---PAGE---

+Sales

+Target

+May May Jun Jun
```
</details>

### classic105_3d_bar_chart

- **Text Similarity:** 0.8832
- **Visual Average:** 0.7434
- **Overall Score:** 0.8506
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3969 bytes, Reference=138437 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic105_3d_bar_chart.pdf
+++ reference/classic105_3d_bar_chart.pdf
@@ -1,10 +1,8 @@
 Region 2024 2025

 APAC 120 145

-Revenue by Region (3D)

+Revenue by Region (3

 EMEA 95 110

 Americas 150 175

-2024 2025

-200

 LATAM 40 55

 180

 160

@@ -16,5 +14,9 @@
 40

 20

 0

-APAC EMEA Americas LATAM

----PAGE---
+APAC EMEA Americas

+---PAGE---

+D)

+2024

+2025

+LATAM
```
</details>

### classic106_3d_pie_chart

- **Text Similarity:** 0.9558
- **Visual Average:** 0.968
- **Overall Score:** 0.9695
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=404079 bytes, Reference=76353 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic106_3d_pie_chart.pdf
+++ reference/classic106_3d_pie_chart.pdf
@@ -3,13 +3,15 @@
 Monthly Expense Breakdown (3D)

 Housing 1500

 Transport 400

-Entertain 300

+Entertainm 300

 Savings 700

 Other 200

 Food

 Housing

-Transport

-Entertainme…

+Transpo

+Entertai

 Savings

 Other

----PAGE---
+---PAGE---

+rt

+nment
```
</details>

### classic107_multi_series_line

- **Text Similarity:** 0.7566
- **Visual Average:** 0.7733
- **Overall Score:** 0.812
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=15164 bytes, Reference=82303 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic107_multi_series_line.pdf
+++ reference/classic107_multi_series_line.pdf
@@ -1,9 +1,8 @@
 Day AAPL GOOG MSFT

 Day 1 178.48 140.49 402.83

-Stock Price Trend (20 Da

+Stock Price

 Day 2 179.43 140.38 401.69

 Day 3 177.25 143.38 403.21

-AAPL GOOG MSFT

 450

 Day 4 175.75 143.94 404.47

 Day 5 178.19 142.62 403.35

@@ -14,20 +13,29 @@
 Day 8 175.18 138.97 405.07

 300

 Day 9 173.1 137.59 403.53

-Day 10 172.64 139.72 401.94 250

+250

+Day 10 172.64 139.72 401.94

+Price ($)

 Day 11 173.32 139.12 400.69

 200

-Day 12 172.11 140.8 402.75 Price ($)

-Day 13 173.5 143.13 404.12 150

+Day 12 172.11 140.8 402.75

+150

+Day 13 173.5 143.13 404.12

 Day 14 172.29 141.53 404.52

 100

 Day 15 172.95 143.24 406.95

+50

 Day 16 174.74 146.1 408

-50

 Day 17 175.83 147.89 407.98

 0

-Day 18 177.62 150.15 408.05

-Day 1Day 2Day 3Day 4Day 5Day 6Day 7Day 8Day 9Day 10Day 11Day 1

+Day 18 177.62 150.15 408.05 Day Day Day Day Day Day Day Day Day Da

+1 2 3 4 5 6 7 8 9 1

 Day 19 176.68 149.43 408.73

 Day 20 177.07 149.4 408.07

----PAGE---
+---PAGE---

+Trend (20 Days)

+AAPL

+GOOG

+MSFT

+ay Day Day Day Day Day Day Day Day Day Day

+0 11 12 13 14 15 16 17 18 19 20
```
</details>

### classic108_stacked_area_chart

- **Text Similarity:** 0.931
- **Visual Average:** 0.8935
- **Overall Score:** 0.9298
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11034 bytes, Reference=51253 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic108_stacked_area_chart.pdf
+++ reference/classic108_stacked_area_chart.pdf
@@ -6,12 +6,15 @@
 May 150 130 240 125

 Jun 160 140 260 130

 Traffic by Channel (Stacked)

-Direct Search Social Email

 800

 700

 600

+Direct

 500

+Search

+Social

 400

+Email

 300

 200

 100

```
</details>

### classic109_scatter_with_trendline

- **Text Similarity:** 0.784
- **Visual Average:** 0.9861
- **Overall Score:** 0.908
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5316 bytes, Reference=60738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic109_scatter_with_trendline.pdf
+++ reference/classic109_scatter_with_trendline.pdf
@@ -1,27 +1,30 @@
-Study Ho Exam Score

+Study HouExam Score

 5 59

 Study Hours vs Exam Score

 8 90

 9 85

-Students

 120

 2 35

 9 99

+100

 5 68

-100

-2 35

-8 92

-80

+f(x) = 8.12719751809721 x + 20.8283350568769

+2 35 R² = 0.958630685218316

+8 92 80

 5 65

-3 45

-60

+Stud

+3 45 Score 60

+Line

 9 100

-6 62 Score

-9 89 40

+6 62

+40

+9 89

 1 30

+20

 10 98

-20

 0

 0 2 4 6 8 10 12

 Hours

----PAGE---
+---PAGE---

+dents

+ear (Students)
```
</details>

### classic10_special_xml_characters

- **Text Similarity:** 1.0
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=968 bytes, Reference=27644 bytes

Text content: ✅ Identical

### classic110_chart_with_legend

- **Text Similarity:** 0.8
- **Visual Average:** 0.7795
- **Overall Score:** 0.8318
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4007 bytes, Reference=52253 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic110_chart_with_legend.pdf
+++ reference/classic110_chart_with_legend.pdf
@@ -1,19 +1,21 @@
 Browser 2024 (%) 2025 (%)

 Chrome 65 62

-Browser Market Share Comparison

+Browser Market Share Com

 Safari 18 20

 Firefox 8 7

-2024 (%) 2025 (%)

 70

 Edge 6 8

 Other 3 3

 60

 50

+Market Share (%)

 40

 30

-Market Share (%)

 20

 10

 0

-Chrome Safari Firefox Edge Oth

----PAGE---
+Chrome Safari Firefox

+2024 (%) 2025 (%)

+---PAGE---

+mparison

+Edge Other
```
</details>

### classic111_chart_with_axis_labels

- **Text Similarity:** 0.8267
- **Visual Average:** 0.977
- **Overall Score:** 0.9215
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3376 bytes, Reference=51007 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic111_chart_with_axis_labels.pdf
+++ reference/classic111_chart_with_axis_labels.pdf
@@ -9,9 +9,12 @@
 Germany 700

 Japan

 Russia

-CO2 Emissions (Megatons) India

+Country

+India

 USA

 China

 0 2,000 4,000 6,000 8,000 10,000

-Country

----PAGE---
+CO2 Emissions (Megatons)

+---PAGE---

+CO2 (Mt)

+0 12,000
```
</details>

### classic112_multiple_charts

- **Text Similarity:** 0.8769
- **Visual Average:** 0.7583
- **Overall Score:** 0.8541
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6801 bytes, Reference=59342 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic112_multiple_charts.pdf
+++ reference/classic112_multiple_charts.pdf
@@ -1,9 +1,8 @@
 Month Revenue Costs Profit

 Jan 50 30 20

-Revenue & Costs

+Revenue & Co

 Feb 55 32 23

 Mar 60 35 25

-Revenue Costs

 80

 Apr 52 28 24

 May 70 40 30

@@ -16,7 +15,7 @@
 20

 10

 0

-Jan Feb Mar Apr May

+Jan Feb Mar Apr

 Profit Trend

 35

 30

@@ -26,5 +25,12 @@
 10

 5

 0

-Jan Feb Mar Apr M

----PAGE---
+Jan Feb Mar Apr

+---PAGE---

+osts

+Revenue

+Costs

+May Jun

+d

+Profit

+May Jun
```
</details>

### classic113_chart_sheet

- **Text Similarity:** 0.9259
- **Visual Average:** 0.7331
- **Overall Score:** 0.8636
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3130 bytes, Reference=43602 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic113_chart_sheet.pdf
+++ reference/classic113_chart_sheet.pdf
@@ -3,8 +3,7 @@
 Quarterly Revenue

 Q2 310

 Q3 285

-450

-Q4 400

+Q4 400 450

 400

 350

 300

@@ -14,5 +13,7 @@
 100

 50

 0

-Q1 Q2 Q3 Q4

----PAGE---
+Q1 Q2 Q3

+---PAGE---

+Revenue

+Q4
```
</details>

### classic114_chart_large_dataset

- **Text Similarity:** 0.9322
- **Visual Average:** 0.8858
- **Overall Score:** 0.9272
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=30127 bytes, Reference=128765 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic114_chart_large_dataset.pdf
+++ reference/classic114_chart_large_dataset.pdf
@@ -1,6 +1,6 @@
 Day Value

 1 97.7

-100-Day Value Trend

+100-Day Value Tr

 2 93.7

 3 96.1

 160

@@ -11,7 +11,8 @@
 7 98.1

 120

 8 100.5

-9 98.7 100

+9 98.7

+100

 10 94.4

 80

 11 98.6

@@ -21,11 +22,12 @@
 14 98.4

 40

 15 104.2

-16 109 20

+16 109

+20

 17 109.1

 0

 18 105.3

-1234567891011213141516171819202122324252627282930313233435363738394041424344546474849505152535455657585960616263646566768697071727374757677879808182838485868

+1 5 9 13 17 21 25 29 33 37 41 45 49 53 57 61 65

 19 108.6

 20 114.2

 21 112.6

@@ -110,4 +112,7 @@
 98 133.6

 99 138

 100 142.1

----PAGE---
+---PAGE---

+rend

+Value

+69 73 77 81 85 89 93 97
```
</details>

### classic115_chart_negative_values

- **Text Similarity:** 0.8789
- **Visual Average:** 0.9721
- **Overall Score:** 0.9404
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4140 bytes, Reference=51633 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic115_chart_negative_values.pdf
+++ reference/classic115_chart_negative_values.pdf
@@ -10,14 +10,17 @@
 Jun -12

 25

 Jul 18

-Aug 5 20

+Aug 5

+20

 15

+Amount ($K)

 10

-Amount ($K)

 5

 0

+Jan Feb Mar Apr May Jun Jul Au

 -5

 -10

 -15

-Jan Feb Mar Apr May Jun Jul Aug

----PAGE---
+---PAGE---

+Profit/Loss

+ug
```
</details>

### classic116_percent_stacked_area

- **Text Similarity:** 0.9322
- **Visual Average:** 0.8786
- **Overall Score:** 0.9243
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11280 bytes, Reference=50765 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic116_percent_stacked_area.pdf
+++ reference/classic116_percent_stacked_area.pdf
@@ -6,13 +6,15 @@
 2023 20 26 17 37

 2025 15 24 16 45

 Energy Mix Transition

-Renewable Nuclear Gas Coal

 100%

 90%

 80%

 70%

-60%

+Renewable

+60% Nuclear

+Gas

 50%

+Coal

 40%

 30%

 20%

```
</details>

### classic117_stock_ohlc_chart

- **Text Similarity:** 0.7817
- **Visual Average:** 0.7223
- **Overall Score:** 0.8016
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=8372 bytes, Reference=62401 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic117_stock_ohlc_chart.pdf
+++ reference/classic117_stock_ohlc_chart.pdf
@@ -1,25 +1,27 @@
 Day Open High Low Close

 Day 1 148.96 149.78 146.96 147.41

-Stock OHLC

+St

 Day 2 147.04 147.63 144.4 146.23

 Day 3 145.63 149.68 145.47 149.58

-Open High Low Close

-180

+160

 Day 4 149.32 150.14 147.39 148.55

 Day 5 146.58 150.1 143.38 147.36

-160

 Day 6 147.91 152.44 145.49 149.32

-140

+155

 Day 7 151.08 155.51 150.22 150.81

 Day 8 152.42 155.53 152.31 152.99

-120

 Day 9 152.32 154.36 151.02 152.05

-Day 10 152.27 156.85 148.76 156.35 100

-80

+150

+Day 10 152.27 156.85 148.76 156.35

 Price ($)

-60

-40

-20

-0

-Day 1 Day 2 Day 3 Day 4 Day 5

----PAGE---
+145

+140

+135

+Day 1 Day 2 Day 3 D

+---PAGE---

+tock OHLC (10 Days)

+Open

+High

+Low

+Close

+Day 4 Day 5 Day 6 Day 7 Day 8 Day 9 Day 10
```
</details>

### classic118_bar_chart_custom_colors

- **Text Similarity:** 0.9565
- **Visual Average:** 0.9609
- **Overall Score:** 0.967
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3494 bytes, Reference=48780 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic118_bar_chart_custom_colors.pdf
+++ reference/classic118_bar_chart_custom_colors.pdf
@@ -16,5 +16,7 @@
 10

 5

 0

-Excellent Good Average Poor Very Poor

----PAGE---
+Excellent Good Average Poor Very

+---PAGE---

+Count

+y Poor
```
</details>

### classic119_dashboard_multi_charts

- **Text Similarity:** 0.9149
- **Visual Average:** 0.9342
- **Overall Score:** 0.9396
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=218459 bytes, Reference=65175 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic119_dashboard_multi_charts.pdf
+++ reference/classic119_dashboard_multi_charts.pdf
@@ -2,14 +2,12 @@
 Revenue vs Expenses

 Month Revenue Expenses

 Oct 85 60

-Revenue Expenses

 120

 Nov 92 65

 Dec 110 70

 100

 80

-60

-Segment Share

+Segment Share 60

 Enterprise 45

 40

 SMB 30

@@ -21,5 +19,6 @@
 Enterprise

 SMB

 Consumer

-Slice4

----PAGE---
+---PAGE---

+Revenue

+Expenses
```
</details>

### classic11_sparse_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.9991
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1037 bytes, Reference=23538 bytes

Text content: ✅ Identical

### classic120_chart_with_date_axis

- **Text Similarity:** 0.5574
- **Visual Average:** 0.7839
- **Overall Score:** 0.7365
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5778 bytes, Reference=56955 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic120_chart_with_date_axis.pdf
+++ reference/classic120_chart_with_date_axis.pdf
@@ -1,26 +1,32 @@
 Date Downloads

 2025-01-0 581

-Monthly Downloads (2025)

+Monthly Downloads (20

 2025-01-3 594

 2025-03-0 592

-900

+1000

 2025-04-0 692

-2025-05-0 760

+2025-05-0 760 900

+2025-05-3 733

 800

-2025-05-3 733

+2025-06-3 763

 700

-2025-06-3 763

 2025-07-3 767

 600

 2025-08-2 774

-2025-09-2 788 500

+Downloads

+500

+2025-09-2 788

+400

 2025-10-2 820

-400

-2025-11-2 865 Downloads

-300

+2025-11-2 865 300

 200

 100

 0

-2025-01-01 2025-01-31 2025-03-02 2025-04-01 2025-05-01 2025-05-31 2025-06-30 2025-07-30 2025-08-29 2025-0

+2025- 2025- 2025- 2025- 2025- 2025- 2025- 2025- 2025- 2

+01-01 01-31 03-02 04-01 05-01 05-31 06-30 07-30 08-29 0

 Date

----PAGE---
+---PAGE---

+025)

+Downloads

+2025- 2025- 2025-

+09-28 10-28 11-27
```
</details>

### classic12_sparse_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.9974
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=875 bytes, Reference=24923 bytes

Text content: ✅ Identical

### classic13_date_strings

- **Text Similarity:** 0.9738
- **Visual Average:** 0.9927
- **Overall Score:** 0.9866
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1194 bytes, Reference=29104 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic13_date_strings.pdf
+++ reference/classic13_date_strings.pdf
@@ -1,6 +1,6 @@
 Date Event

-2025-01-1Launch

-2025-06-3Release

-2025-12-2Holiday

-2026-01-0New Year

-2026-02-2Today
+2025-01-1 Launch

+2025-06-3 Release

+2025-12-2 Holiday

+2026-01-0 New Year

+2026-02-2 Today
```
</details>

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9946
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1181 bytes, Reference=29057 bytes

Text content: ✅ Identical

### classic15_negative_numbers

- **Text Similarity:** 0.908
- **Visual Average:** 0.9957
- **Overall Score:** 0.9615
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1264 bytes, Reference=28526 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic15_negative_numbers.pdf
+++ reference/classic15_negative_numbers.pdf
@@ -1,7 +1,7 @@
 Label Value

 Loss -100

-Small Los -0.5

+Small Loss -0.5

 Zero 0

 Gain 50

-Big Loss -100000

+Big Loss -99999.99

 Tiny -0.001
```
</details>

### classic16_percentage_strings

- **Text Similarity:** 0.9877
- **Visual Average:** 0.9936
- **Overall Score:** 0.9925
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1176 bytes, Reference=29888 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic16_percentage_strings.pdf
+++ reference/classic16_percentage_strings.pdf
@@ -1,5 +1,5 @@
 Metric Rate

-Conversio 12.5%

+Conversion12.5%

 Bounce 45.3%

 Retention 88.7%

 Churn 3.2%

```
</details>

### classic17_currency_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9924
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1284 bytes, Reference=29862 bytes

Text content: ✅ Identical

### classic18_large_dataset

- **Text Similarity:** 1.0
- **Visual Average:** 0.8692
- **Overall Score:** 0.9477
- **Pages:** MiniPdf=24, Reference=24
- **File Size:** MiniPdf=533022 bytes, Reference=2487195 bytes

Text content: ✅ Identical

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9944
- **Overall Score:** 0.9978
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
- **Visual Average:** 0.9983
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=828 bytes, Reference=22034 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=867 bytes, Reference=23683 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.8033
- **Visual Average:** 0.9935
- **Overall Score:** 0.9187
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3034 bytes, Reference=67722 bytes

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

-こんにち世界

-Korean

-안녕하세세계

-Arabic

-ملاعلاابحرم

-Emoji

-����✅❌
+Korean 안녕하세요세계

+Arabicمرحبا العالم

+Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1064 bytes, Reference=39031 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 0.9955
- **Visual Average:** 0.9911
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1769 bytes, Reference=43116 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic25_multiple_colors.pdf
+++ reference/classic25_multiple_colors.pdf
@@ -1,4 +1,4 @@
-Color Na Sample Text

+Color NamSample Text

 Red This is red text

 Green This is green text

 Blue This is blue text

```
</details>

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1037 bytes, Reference=25018 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=923 bytes, Reference=23681 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1539 bytes, Reference=24729 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9931
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1475 bytes, Reference=27548 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1390 bytes, Reference=27418 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 0.996
- **Visual Average:** 0.9912
- **Overall Score:** 0.9949
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1600 bytes, Reference=40714 bytes

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
- **Visual Average:** 0.9949
- **Overall Score:** 0.998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=980 bytes, Reference=27582 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9978
- **Overall Score:** 0.9991
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1312 bytes, Reference=26648 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 1.0
- **Visual Average:** 0.9897
- **Overall Score:** 0.9959
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1206 bytes, Reference=28834 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Text Similarity:** 0.9639
- **Visual Average:** 0.9964
- **Overall Score:** 0.9841
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=881 bytes, Reference=25108 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
-Tall Head Value

+Tall HeadeValue

 Extra Tall 42

-Normal R 10
+Normal Ro 10
```
</details>

### classic36_merged_cells

- **Text Similarity:** 0.9936
- **Visual Average:** 0.9914
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1093 bytes, Reference=27256 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic36_merged_cells.pdf
+++ reference/classic36_merged_cells.pdf
@@ -1,4 +1,4 @@
-Merged Header Spanning Thre

+Merged Header Spanning Three

 Col1 Col2 Col3

 Row2A Row2B Row2C

 Row3A Row3B Row3C
```
</details>

### classic37_freeze_panes

- **Text Similarity:** 1.0
- **Visual Average:** 0.9848
- **Overall Score:** 0.9939
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4826 bytes, Reference=46420 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9963
- **Overall Score:** 0.9985
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
- **Visual Average:** 0.9892
- **Overall Score:** 0.9957
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2049 bytes, Reference=43383 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 0.9388
- **Visual Average:** 0.9934
- **Overall Score:** 0.9729
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1197 bytes, Reference=30852 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic40_scientific_notation.pdf
+++ reference/classic40_scientific_notation.pdf
@@ -1,6 +1,6 @@
 Label Value

-Avogadro 6E+23

-Planck 6.63E-34

-Speed of 3E+08

-Electron 9.11E-31

+Avogadro 6.02E+23

+Planck 6.626E-34

+Speed of L 3E+08

+Electron m9.109E-31

 Pi approx 3.141593
```
</details>

### classic41_integer_vs_float

- **Text Similarity:** 1.0
- **Visual Average:** 0.9945
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1460 bytes, Reference=29637 bytes

Text content: ✅ Identical

### classic42_boolean_values

- **Text Similarity:** 0.9894
- **Visual Average:** 0.9927
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1189 bytes, Reference=28631 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

-Dark Mod TRUE

+Dark Mode TRUE

 Notificatio FALSE

 Auto-save TRUE

 Analytics FALSE

-Beta Feat TRUE
+Beta Featu TRUE
```
</details>

### classic43_inventory_report

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9807
- **Overall Score:** 0.9916
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3048 bytes, Reference=49849 bytes

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

- **Text Similarity:** 0.9581
- **Visual Average:** 0.9722
- **Overall Score:** 0.9721
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3505 bytes, Reference=43656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

-1001 Alice Smith Engineeri Senior En alice@example.com

-1002 Bob Jones Marketing Marketing bob@example.com

-1003 Carol Williams HR HR Speci carol@example.com

-1004 David Brown Engineeri Junior En david@example.com

-1005 Eve Davis Finance Financial eve@example.com

-1006 Frank Miller Sales Sales Repfrank@example.com

-1007 Grace Wilson Engineeri Tech Lea grace@example.com

-1008 Henry Moore Support Support S henry@example.com
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
- **Visual Average:** 0.9951
- **Overall Score:** 0.998
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3174 bytes, Reference=37087 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.9861
- **Overall Score:** 0.9944
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3339 bytes, Reference=40993 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.978
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7020 bytes, Reference=55976 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9913
- **Visual Average:** 0.9879
- **Overall Score:** 0.9917
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2382 bytes, Reference=36069 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
 Question StrongAgr Agree Neutral Disagree StrongDisagree

-Easy to u 30 45 15 7 3

-Recomme 25 40 20 10 5

+Easy to us 30 45 15 7 3

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

-Good sup 35 40 15 7 3

+Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.8712
- **Visual Average:** 0.9809
- **Overall Score:** 0.9408
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2698 bytes, Reference=41523 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smit +1-555-01alice@ex New York USA

-Bob Jone +44-20-79bob@exa London UK

-Carol Wa +86-10-12carol@ex Beijing China

-David Mul +49-30-12david@ex Berlin Germany

-Eve Marti +33-1-23- eve@exa Paris France

-Frank Tan+81-3-123frank@ex Tokyo Japan

-Grace Ki +82-2-123grace@exSeoul Korea
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

- **Text Similarity:** 0.9933
- **Visual Average:** 0.9812
- **Overall Score:** 0.9898
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=6549 bytes, Reference=54986 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic50_budget_vs_actuals.pdf
+++ reference/classic50_budget_vs_actuals.pdf
@@ -1,19 +1,19 @@
-Departme Q1 Q2 Q3 Q4 Annual

-Engineeri 200000 200000 210000 220000 830000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin 200000 200000 210000 220000 830000

 Marketing 80000 90000 85000 95000 350000

 Sales 120000 130000 140000 150000 540000

 HR 40000 40000 42000 43000 165000

 Finance 35000 35000 37000 38000 145000

 ---PAGE---

-Departme Q1 Q2 Q3 Q4 Annual

-Engineeri 195000 205000 215000 225000 840000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin 195000 205000 215000 225000 840000

 Marketing 82000 88000 91000 97000 358000

 Sales 118000 135000 142000 148000 543000

 HR 39000 41000 41500 44000 165500

 Finance 34000 36000 37500 39000 146500

 ---PAGE---

-Departme Q1 Q2 Q3 Q4 Annual

-Engineeri -5000 5000 5000 5000 10000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin -5000 5000 5000 5000 10000

 Marketing 2000 -2000 6000 2000 8000

 Sales -2000 5000 2000 -2000 3000

 HR -1000 1000 -500 1000 500

```
</details>

### classic51_product_catalog

- **Text Similarity:** 0.9573
- **Visual Average:** 0.9799
- **Overall Score:** 0.9749
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3437 bytes, Reference=44297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
 Part# Name Descriptio Weight(g) Price

-P-001 Basic Wid Standard 150 4.99

-P-002 Pro Widg Enhanced 180 12.99

-P-003 Mini GadgCompact 90 19.99

-P-004 Max GadgFull-size g 450 89.99

-P-005 ConnectorType-A co 80 7.49

-P-006 ConnectorType-B co 110 9.99

+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgeEnhanced 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size g 450 89.99

+P-005 ConnectorType-A con 80 7.49

+P-006 ConnectorType-B con 110 9.99

 P-007 Adapter X Universal 200 15.99

-P-008 Adapter Y Travel po 120 11.99

-P-009 Mount BraWall mou 600 24.99

-P-010 Carry Cas Padded c 350 34.99
+P-008 Adapter Y Travel pow 120 11.99

+P-009 Mount BraWall moun 600 24.99

+P-010 Carry CasePadded ca 350 34.99
```
</details>

### classic52_pivot_summary

- **Text Similarity:** 0.9956
- **Visual Average:** 0.9815
- **Overall Score:** 0.9908
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2423 bytes, Reference=44493 bytes

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

- **Text Similarity:** 0.9918
- **Visual Average:** 0.9868
- **Overall Score:** 0.9914
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2519 bytes, Reference=53425 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -6,11 +6,11 @@
 ACME Corporation

 123 Business Rd, Suite 400

 New York, NY 10001

-Item Qty Unit Price Total

-Consultin 10 150 1500

-Software 5 99 495

+Item Qty Unit PriceTotal

+Consulting 10 150 1500

+Software L 5 99 495

 Hardware 2 249.99 499.98

-Support P 1 1200 1200

+Support Pl 1 1200 1200

 Subtotal 3694.98

 Tax (8%) 295.6

 Total Due 3990.58
```
</details>

### classic54_multi_level_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.9854
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2168 bytes, Reference=38782 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9901
- **Overall Score:** 0.996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1650 bytes, Reference=34677 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 0.997
- **Visual Average:** 0.9859
- **Overall Score:** 0.9932
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3112 bytes, Reference=32363 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic56_alternating_row_colors.pdf
+++ reference/classic56_alternating_row_colors.pdf
@@ -8,4 +8,4 @@
 7 Product 7 70

 8 Product 8 80

 9 Product 9 90

-10 Product 1 100
+10 Product 10 100
```
</details>

### classic57_cjk_only

- **Text Similarity:** 0.7826
- **Visual Average:** 0.9095
- **Overall Score:** 0.8768
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3173 bytes, Reference=88207 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic57_cjk_only.pdf
+++ reference/classic57_cjk_only.pdf
@@ -1,11 +1,11 @@
 序号 产品名称价格 库存

+笔记本电脑

 1 5999 100

-笔记本电

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

- **Text Similarity:** 0.9579
- **Visual Average:** 0.9918
- **Overall Score:** 0.9799
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1601 bytes, Reference=32815 bytes

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

-Very smal 0.0001

-Very large 1E+07

+Negative in -42

+Negative fl -3.14

+Very small 0.0001

+Very large 10000000

 Zero 0

-Scientific 1.2E+10
+Scientific 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9932
- **Overall Score:** 0.9973
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=4363 bytes, Reference=44781 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9129
- **Overall Score:** 0.9652
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=55077 bytes, Reference=263350 bytes

Text content: ✅ Identical

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9941
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2057 bytes, Reference=36974 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 0.9879
- **Visual Average:** 0.9908
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2749 bytes, Reference=42880 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic62_company_logo_header.pdf
+++ reference/classic62_company_logo_header.pdf
@@ -1,6 +1,6 @@
 ACME Corporation

 Annual Report 2025

-Departme Q1 Q2 Q3 Q4

+DepartmenQ1 Q2 Q3 Q4

 Sales 120 135 142 160

-Engineeri 85 90 95 100

+Engineerin 85 90 95 100

 Marketing 60 65 70 75
```
</details>

### classic63_two_products_side_by_side

- **Text Similarity:** 1.0
- **Visual Average:** 0.9927
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3067 bytes, Reference=28933 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.9833
- **Visual Average:** 0.9884
- **Overall Score:** 0.9887
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4677 bytes, Reference=43408 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title Departme Email

-Alice Che Engineer R&D alice@example.com

+Photo Name Title DepartmeEmail

+Alice ChenEngineer R&D alice@example.com

 Bob SmithManager Sales bob@example.com

-Carol Wa Designer UX carol@example.com
+Carol WanDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9809
- **Visual Average:** 0.987
- **Overall Score:** 0.9872
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6585 bytes, Reference=48227 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic65_inventory_with_product_photos.pdf
+++ reference/classic65_inventory_with_product_photos.pdf
@@ -1,6 +1,6 @@
 Image SKU Name Qty Price

-SKU-001 Red Widg 50 9.99

-SKU-002 Blue Gad 30 14.99

-SKU-003 Green To 100 4.49

-SKU-004 Yellow De 25 29.99

-SKU-005 Purple Ge 75 7.99
+SKU-001 Red Widge 50 9.99

+SKU-002 Blue Gadge 30 14.99

+SKU-003 Green Too 100 4.49

+SKU-004 Yellow Dev 25 29.99

+SKU-005 Purple Gea 75 7.99
```
</details>

### classic66_invoice_with_logo

- **Text Similarity:** 0.9766
- **Visual Average:** 0.992
- **Overall Score:** 0.9874
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

-Consultin 8 150 1200

-Software 1 299 299

-Support P 1 99 99

+DescriptiQty Unit PriceTotal

+Consulting 8 150 1200

+Software L 1 299 299

+Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Text Similarity:** 0.9966
- **Visual Average:** 0.9932
- **Overall Score:** 0.9959
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2739 bytes, Reference=44030 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic67_real_estate_listing.pdf
+++ reference/classic67_real_estate_listing.pdf
@@ -3,7 +3,7 @@
 List Price: $485,000

 Feature Detail

 Bedrooms 4

-Bathroom 2.5

+Bathrooms 2.5

 Sq Ft 2100

 Lot Size 0.25 acres

 Year Built 1998
```
</details>

### classic68_restaurant_menu

- **Text Similarity:** 0.9904
- **Visual Average:** 0.9769
- **Overall Score:** 0.9869
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5247 bytes, Reference=47320 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,7 +1,7 @@
 Today's Menu

-Grilled Sal$18.99

+Grilled S $18.99

 Fresh Atlantic salmon with herbs

-Caesar S $12.99

+Caesar Sa $12.99

 Romaine lettuce, croutons, parmesan

 Beef Burg $14.99

 8oz Angus beef, brioche bun

```
</details>

### classic69_image_only_sheet

- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2459 bytes, Reference=8905 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Text Similarity:** 0.9895
- **Visual Average:** 0.9864
- **Overall Score:** 0.9904
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4406 bytes, Reference=44156 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

 Classic P $3.99

 A reliable ballpoint pen

-Leather N $12.99

+Leather $12.99

 Premium A5 notebook

-Desk Org $24.99

+Desk Orga $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 0.9896
- **Visual Average:** 0.995
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=5023 bytes, Reference=37419 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic71_multi_sheet_with_images.pdf
+++ reference/classic71_multi_sheet_with_images.pdf
@@ -6,6 +6,6 @@
 Digital 50000

 Print 20000

 ---PAGE---

-Departme Headcount

-Engineeri 45

+DepartmenHeadcount

+Engineerin 45

 Sales 30
```
</details>

### classic72_bar_chart_image_with_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9826
- **Overall Score:** 0.993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3076 bytes, Reference=41342 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9361
- **Visual Average:** 0.9912
- **Overall Score:** 0.9709
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3068 bytes, Reference=44512 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -3,7 +3,7 @@
 Venue: Convention Center Hall A

 Speakers: 20+ Industry Leaders

 Time Session Speaker

-09:00 Opening Dr. Jane Kim

-10:30 AI in PractProf. Mark Liu

-13:00 Cloud Arc Eng. Sara Patel

-15:00 Panel Dis All Speakers
+09:00 Opening KDr. Jane Kim

+10:30 AI in Pract Prof. Mark Liu

+13:00 Cloud ArchEng. Sara Patel

+15:00 Panel Disc All Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.956
- **Visual Average:** 0.9815
- **Overall Score:** 0.975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4133 bytes, Reference=48755 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -1,10 +1,6 @@
 Executive Dashboard Q1 2025

 KPI Target Actual Status

-Revenue 500000 523000

-✓ Above

-New Cust 200 187

-✗ Below

-NPS Scor 70 74

-✓ Above

-Churn Rat< 3% 2.8%

-✓ Above
+Revenue 500000 523000 ✓ Above

+New Custo 200 187  Below ✗

+NPS Score 70 74 ✓ Above

+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Text Similarity:** 1.0
- **Visual Average:** 0.9855
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1981 bytes, Reference=39135 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9834
- **Overall Score:** 0.9934
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5002 bytes, Reference=39017 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9855
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2719 bytes, Reference=52664 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9897
- **Visual Average:** 0.9909
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6064 bytes, Reference=41646 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login bAlice Done

+Fix login b Alice Done

 Write unit Bob In Progress

 Deploy to Carol Pending

-Code revi Alice Done

+Code revieAlice Done

 Update doDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9928
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2889 bytes, Reference=43015 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9874
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2325 bytes, Reference=39079 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.984
- **Overall Score:** 0.9936
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5110 bytes, Reference=47175 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9926
- **Visual Average:** 0.9875
- **Overall Score:** 0.992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3834 bytes, Reference=42486 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -2,4 +2,4 @@
 Old design – legacy UI New design – modern UI

 Metric Before After Delta

 Load time 4.2s 1.1s -74%

-Conversio 2.1% 4.8% +129%
+Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Text Similarity:** 0.9916
- **Visual Average:** 0.9871
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6911 bytes, Reference=45933 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

 Primary BlRGB(0, 82, 165)

-Primary R RGB(197, 27, 50)

+Primary ReRGB(197, 27, 50)

 Accent Gr RGB(0, 163, 108)

 Neutral GrRGB(128, 128, 128)

 Warm YellRGB(255, 193, 7)

-Dark Nav RGB(10, 30, 70)
+Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Text Similarity:** 1.0
- **Visual Average:** 0.9841
- **Overall Score:** 0.9936
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4377 bytes, Reference=42524 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.991
- **Visual Average:** 0.9871
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3267 bytes, Reference=47866 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,7 +1,7 @@
 Sample Analysis Report

-Paramete Value Unit ReferenceFlag

-pH 7.35 7.35 – 7.4 Normal

+ParameteValue Unit ReferenceFlag

+pH 7.35 7.35 – 7.45Normal

 Glucose 5.2 mmol/L 3.9 – 5.5 Normal

 Sodium 142 mEq/L 136 – 145 Normal

-Potassiu 5 mEq/L 3.5 – 5.0 Normal

+Potassium 5 mEq/L 3.5 – 5.0 Normal

 Creatinine 1.4 mg/dL 0.6 – 1.2 High
```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.9827
- **Visual Average:** 0.9937
- **Overall Score:** 0.9906
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2834 bytes, Reference=41961 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic86_software_screenshot_features.pdf
+++ reference/classic86_software_screenshot_features.pdf
@@ -1,9 +1,9 @@
 MiniApp v2.0

 The fastest lightweight app

 Feature Available

-Dark Mod Yes

-Auto SaveYes

-Cloud SynYes

+Dark ModeYes

+Auto Save Yes

+Cloud SyncYes

 Offline MoYes

-API AccesPro only

+API AccessPro only

 Export to Yes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9922
- **Overall Score:** 0.9969
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5588 bytes, Reference=47076 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 0.997
- **Visual Average:** 0.9877
- **Overall Score:** 0.9939
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2888 bytes, Reference=43273 bytes

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
- **Visual Average:** 0.9908
- **Overall Score:** 0.9914
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3073 bytes, Reference=47194 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount p % Daily Value

+Nutrient Amount p% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

 Saturated 0.5g 3%

 Sodium 160mg 7%

-Total Car 22g 8%

-Dietary Fi 3g 11%

+Total Carb22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.9492
- **Visual Average:** 0.9782
- **Overall Score:** 0.971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3167 bytes, Reference=47112 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
 Project Orion – Status Report

 Reporting Period: Q1 2025

-Milestone Due Date Owner Status

-Requirem Jan 15 PM Team Complete

-Architectu Feb 1 Tech Lea Complete

-Alpha Rel Feb 28 Dev TeamIn Progress

+MilestoneDue DateOwner Status

+RequiremeJan 15 PM Team Complete

+ArchitectuFeb 1 Tech Lead Complete

+Alpha ReleFeb 28 Dev Team In Progress

 Beta Testi Mar 31 QA Team Not Started

-Productio Apr 15 DevOps Not Started
+ProductionApr 15 DevOps Not Started
```
</details>

### classic91_simple_bar_chart

- **Text Similarity:** 0.9585
- **Visual Average:** 0.9617
- **Overall Score:** 0.9681
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3048 bytes, Reference=46981 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic91_simple_bar_chart.pdf
+++ reference/classic91_simple_bar_chart.pdf
@@ -12,6 +12,8 @@
 10000

 5000

 0

-Widget A Widget B Widget C Widget D Widget E

+Widget A Widget B Widget C Widget D Widg

 Product

----PAGE---
+---PAGE---

+Revenue

+get E
```
</details>

### classic92_horizontal_bar_chart

- **Text Similarity:** 0.9535
- **Visual Average:** 0.9673
- **Overall Score:** 0.9683
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3723 bytes, Reference=49903 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic92_horizontal_bar_chart.pdf
+++ reference/classic92_horizontal_bar_chart.pdf
@@ -1,16 +1,18 @@
-Departme Headcount

-Engineeri 45

+DepartmenHeadcount

+Engineerin 45

 Headcount by Department

 Sales 30

 Marketing 18

 HR 12

 Operations

 Finance 15

-Operation 25

+Operations 25

 Finance

 HR

 Marketing

 Sales

 Engineering

-0 5 10 15 20 25 30 35 40 45 5

----PAGE---
+0 5 10 15 20 25 30 35 40 45

+---PAGE---

+Headcount

+50
```
</details>

### classic93_line_chart

- **Text Similarity:** 0.8624
- **Visual Average:** 0.9863
- **Overall Score:** 0.9395
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5196 bytes, Reference=58815 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic93_line_chart.pdf
+++ reference/classic93_line_chart.pdf
@@ -1,6 +1,6 @@
 Month Avg Temp (C)

 Jan 3

-Monthly Average Temperature

+Monthly Average Temperatur

 Feb 5

 Mar 10

 30

@@ -12,12 +12,15 @@
 Aug 27

 20

 Sep 22

+Temperature (C)

 Oct 15

-15

-Nov 8

-Dec 4 Temperature (C)

+Nov 8 15

+Dec 4

 10

 5

 0

-Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov

----PAGE---
+Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov D

+---PAGE---

+re

+Avg Temp (C)

+Dec
```
</details>

### classic94_pie_chart

- **Text Similarity:** 0.9967
- **Visual Average:** 0.9312
- **Overall Score:** 0.9712
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403856 bytes, Reference=47211 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic94_pie_chart.pdf
+++ reference/classic94_pie_chart.pdf
@@ -3,7 +3,7 @@
 Market Share by Segment

 SMB 28

 Consumer 22

-Governm 10

+Governme 10

 Education 5

 Enterprise

 SMB

```
</details>

### classic95_area_chart

- **Text Similarity:** 0.678
- **Visual Average:** 0.7651
- **Overall Score:** 0.7772
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=12602 bytes, Reference=60817 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic95_area_chart.pdf
+++ reference/classic95_area_chart.pdf
@@ -14,21 +14,26 @@
 08:00 250

 09:00 265

 600

+Users

 10:00 288

-11:00 329 Users

-12:00 408 400

+11:00 329

+400

+12:00 408

 13:00 600

 14:00 1000

 200

 15:00 600

 16:00 408

 0

-17:00 329

-00:001:002:003:004:005:006:007:008:009:0010:0011:0012:0013:0014:0015:0016:0017:0018:0019:0020:0021:0

+17:00 329 00: 01: 02: 03: 04: 05: 06: 07: 08: 09: 10: 11: 12: 13: 14: 15: 16: 17: 18: 1

+00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0

 18:00 288

 19:00 265

 20:00 250

 21:00 240

 22:00 233

 23:00 228

----PAGE---
+---PAGE---

+Users

+19: 20: 21: 22: 23:

+00 00 00 00 00
```
</details>

### classic96_scatter_chart

- **Text Similarity:** 0.7943
- **Visual Average:** 0.9861
- **Overall Score:** 0.9122
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6262 bytes, Reference=62711 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic96_scatter_chart.pdf
+++ reference/classic96_scatter_chart.pdf
@@ -1,9 +1,8 @@
-Ad Spend Sales ($K)

+Ad Spend (Sales ($K)

 45 96

 Ad Spend vs Sales

 6 11

 20 43

-Data Points

 140

 13 22

 48 117

@@ -14,20 +13,22 @@
 6 5

 18 38

 80

+Sales ($K)

 37 94

+60

 6 20

-60

-17 49 Sales ($K)

+17 49

+40

 49 119

-40

 31 68

+20

 33 83

-20

 22 40

+0

+0 10 20 30 40 50 60

 15 37

-0

-26 57

-0 10 20 30 40 50 60

-14 28 Ad Spend ($K)

+26 57 Ad Spend ($K)

+14 28

 26 52

----PAGE---
+---PAGE---

+Data Points
```
</details>

### classic97_doughnut_chart

- **Text Similarity:** 1.0
- **Visual Average:** 0.938
- **Overall Score:** 0.9752
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=302479 bytes, Reference=47227 bytes

Text content: ✅ Identical

### classic98_radar_chart

- **Text Similarity:** 0.8869
- **Visual Average:** 0.99
- **Overall Score:** 0.9508
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4010 bytes, Reference=47620 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic98_radar_chart.pdf
+++ reference/classic98_radar_chart.pdf
@@ -2,21 +2,15 @@
 Python 9

 Developer Skill Radar

 SQL 8

-Communi 7

+Communic 7

+Leadership 6

 Python

-Leadershi 6

-Design 5 10

-9

+Design 5

+10

 DevOps 7

-8

-7

 DevOps SQL

-6

 5

-4

-3

-2

-1

+Score

 0

 Design Communication

 Leadership

```
</details>

### classic99_bubble_chart

- **Text Similarity:** 0.8245
- **Visual Average:** 0.9706
- **Overall Score:** 0.918
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4289 bytes, Reference=57405 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic99_bubble_chart.pdf
+++ reference/classic99_bubble_chart.pdf
@@ -3,18 +3,23 @@
 Product Comparison

 25 4.5 300

 50 3.8 150

-Products

-6

+5

 15 4 420

 35 4.7 200

+4.5

 8 3.5 600

-5

 4

+3.5

 3

 Rating

+2.5

 2

+1.5

 1

+0.5

 0

-0 10 20 30 40 50

+5 10 15 20 25 30 35 40 45

 Price ($)

----PAGE---
+---PAGE---

+Products

+50 55
```
</details>

## Improvement Suggestions

### ⚠ Low-Score Test Cases (below 0.8)

1. **classic09_long_text** (score: 0.6365)
1. **classic120_chart_with_date_axis** (score: 0.7365)
1. **classic95_area_chart** (score: 0.7772)

Review the text diffs and visual comparisons above to identify specific rendering issues.
