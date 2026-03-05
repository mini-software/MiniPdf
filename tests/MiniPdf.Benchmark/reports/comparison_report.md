# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-05T09:47:01.348906

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9971 | 1/1 | **0.9988** |
| 2 | 🟢 classic02_multiple_worksheets | 0.9971 | 0.9978 | 3/3 | **0.998** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9998 | 1/1 | **0.9999** |
| 5 | 🟢 classic05_wide_table | 1.0 | 0.994 | 3/3 | **0.9976** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.9469 | 5/5 | **0.9788** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.9991 | 1/1 | **0.9996** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9978 | 1/1 | **0.9991** |
| 9 | 🔴 classic09_long_text | 0.6785 | 0.5771 | 7/12 | **0.6022** |
| 10 | 🟢 classic100_stacked_bar_chart | 0.9663 | 0.9158 | 1/1 | **0.9528** |
| 11 | 🟢 classic101_percent_stacked_bar | 0.9623 | 0.8815 | 1/1 | **0.9375** |
| 12 | 🟢 classic102_line_chart_with_markers | 0.8372 | 0.9881 | 2/2 | **0.9301** |
| 13 | 🟡 classic103_pie_chart_with_labels | 0.7368 | 0.975 | 2/2 | **0.8847** |
| 14 | 🟡 classic104_combo_bar_line_chart | 0.7872 | 0.7521 | 2/2 | **0.8157** |
| 15 | 🟡 classic105_3d_bar_chart | 0.9032 | 0.7461 | 2/2 | **0.8597** |
| 16 | 🟢 classic106_3d_pie_chart | 0.8243 | 0.9684 | 2/2 | **0.9171** |
| 17 | 🟡 classic107_multi_series_line | 0.7375 | 0.7781 | 2/2 | **0.8062** |
| 18 | 🟢 classic108_stacked_area_chart | 0.9643 | 0.8977 | 1/1 | **0.9448** |
| 19 | 🟢 classic109_scatter_with_trendline | 0.8293 | 0.9853 | 2/2 | **0.9258** |
| 20 | 🟢 classic10_special_xml_characters | 1.0 | 0.9966 | 1/1 | **0.9986** |
| 21 | 🟡 classic110_chart_with_legend | 0.8605 | 0.7813 | 2/2 | **0.8567** |
| 22 | 🟢 classic111_chart_with_axis_labels | 0.8267 | 0.9758 | 2/2 | **0.921** |
| 23 | 🟡 classic112_multiple_charts | 0.875 | 0.7566 | 2/2 | **0.8526** |
| 24 | 🟡 classic113_chart_sheet | 0.9259 | 0.7375 | 2/2 | **0.8654** |
| 25 | 🟢 classic114_chart_large_dataset | 0.9015 | 0.8867 | 4/4 | **0.9153** |
| 26 | 🟢 classic115_chart_negative_values | 0.8316 | 0.9721 | 2/2 | **0.9215** |
| 27 | 🟢 classic116_percent_stacked_area | 0.9649 | 0.8828 | 1/1 | **0.9391** |
| 28 | 🟡 classic117_stock_ohlc_chart | 0.7897 | 0.7303 | 2/2 | **0.808** |
| 29 | 🟢 classic118_bar_chart_custom_colors | 0.9275 | 0.9627 | 2/2 | **0.9561** |
| 30 | 🟢 classic119_dashboard_multi_charts | 0.8409 | 0.9349 | 2/2 | **0.9103** |
| 31 | 🟢 classic11_sparse_rows | 1.0 | 0.9994 | 2/2 | **0.9998** |
| 32 | 🟡 classic120_chart_with_date_axis | 0.5738 | 0.783 | 2/2 | **0.7427** |
| 33 | 🟢 classic121_thin_borders | 1.0 | 0.9936 | 1/1 | **0.9974** |
| 34 | 🟢 classic122_thick_outer_thin_inner | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 35 | 🟢 classic123_dashed_borders | 0.992 | 0.995 | 1/1 | **0.9948** |
| 36 | 🟢 classic124_colored_borders | 1.0 | 0.9938 | 1/1 | **0.9975** |
| 37 | 🟢 classic125_solid_fills | 0.992 | 0.9934 | 1/1 | **0.9942** |
| 38 | 🟢 classic126_dark_header | 0.9929 | 0.9905 | 1/1 | **0.9934** |
| 39 | 🟢 classic127_font_styles | 0.9909 | 0.9923 | 1/1 | **0.9933** |
| 40 | 🟢 classic128_font_sizes | 0.982 | 0.9937 | 1/1 | **0.9903** |
| 41 | 🟢 classic129_alignment_combos | 1.0 | 0.9964 | 1/1 | **0.9986** |
| 42 | 🟢 classic12_sparse_columns | 1.0 | 0.998 | 1/1 | **0.9992** |
| 43 | 🟢 classic130_wrap_and_indent | 1.0 | 0.9868 | 1/1 | **0.9947** |
| 44 | 🟢 classic131_number_formats | 1.0 | 0.9917 | 1/1 | **0.9967** |
| 45 | 🟢 classic132_striped_table | 1.0 | 0.9776 | 1/1 | **0.991** |
| 46 | 🟢 classic133_gradient_rows | 1.0 | 0.9924 | 1/1 | **0.997** |
| 47 | 🟢 classic134_heatmap | 1.0 | 0.9718 | 1/1 | **0.9887** |
| 48 | 🟢 classic135_bottom_border_only | 1.0 | 0.9954 | 1/1 | **0.9982** |
| 49 | 🟢 classic136_financial_report_styled | 1.0 | 0.9821 | 1/1 | **0.9928** |
| 50 | 🟢 classic137_checkerboard | 1.0 | 0.9618 | 1/1 | **0.9847** |
| 51 | 🟢 classic138_color_grid | 1.0 | 0.9861 | 1/1 | **0.9944** |
| 52 | 🟢 classic139_pattern_fills | 1.0 | 0.9833 | 1/1 | **0.9933** |
| 53 | 🟢 classic13_date_strings | 0.9738 | 0.9962 | 1/1 | **0.988** |
| 54 | 🟢 classic140_rotated_text | 0.9583 | 0.994 | 1/1 | **0.9809** |
| 55 | 🟢 classic141_mixed_edge_borders | 1.0 | 0.9938 | 1/1 | **0.9975** |
| 56 | 🟢 classic142_styled_invoice | 1.0 | 0.9589 | 1/1 | **0.9836** |
| 57 | 🟢 classic143_colored_tabs | 1.0 | 0.9986 | 4/4 | **0.9994** |
| 58 | 🟢 classic144_note_style_cells | 1.0 | 0.9896 | 1/1 | **0.9958** |
| 59 | 🟢 classic145_status_badges | 1.0 | 0.9795 | 1/1 | **0.9918** |
| 60 | 🟢 classic146_double_border_table | 1.0 | 0.99 | 1/1 | **0.996** |
| 61 | 🟢 classic147_multi_sheet_styled | 1.0 | 0.9931 | 3/3 | **0.9972** |
| 62 | 🟢 classic148_frozen_styled_grid | 1.0 | 0.9359 | 1/1 | **0.9744** |
| 63 | 🟢 classic149_merged_styled_sections | 1.0 | 0.969 | 1/1 | **0.9876** |
| 64 | 🟢 classic14_decimal_numbers | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 65 | 🟢 classic150_kitchen_sink_styles | 0.9677 | 0.9696 | 1/1 | **0.9749** |
| 66 | 🟢 classic15_negative_numbers | 1.0 | 0.9971 | 1/1 | **0.9988** |
| 67 | 🟢 classic16_percentage_strings | 0.9938 | 0.997 | 1/1 | **0.9963** |
| 68 | 🟢 classic17_currency_strings | 1.0 | 0.9962 | 1/1 | **0.9985** |
| 69 | 🟢 classic18_large_dataset | 1.0 | 0.8969 | 24/24 | **0.9588** |
| 70 | 🟢 classic19_single_column_list | 1.0 | 0.9959 | 1/1 | **0.9984** |
| 71 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 72 | 🟢 classic21_header_only | 1.0 | 0.9987 | 1/1 | **0.9995** |
| 73 | 🟢 classic22_long_sheet_name | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 74 | 🟢 classic23_unicode_text | 0.7917 | 0.9948 | 1/1 | **0.9146** |
| 75 | 🟢 classic24_red_text | 1.0 | 0.9963 | 1/1 | **0.9985** |
| 76 | 🟢 classic25_multiple_colors | 1.0 | 0.9951 | 1/1 | **0.998** |
| 77 | 🟢 classic26_inline_strings | 1.0 | 0.997 | 1/1 | **0.9988** |
| 78 | 🟢 classic27_single_row | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 79 | 🟢 classic28_duplicate_values | 1.0 | 0.9968 | 1/1 | **0.9987** |
| 80 | 🟢 classic29_formula_results | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 81 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.9986 | 2/2 | **0.9994** |
| 82 | 🟢 classic31_bold_header_row | 1.0 | 0.9961 | 1/1 | **0.9984** |
| 83 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.9974 | 1/1 | **0.999** |
| 84 | 🟢 classic33_centered_text | 1.0 | 0.9983 | 1/1 | **0.9993** |
| 85 | 🟢 classic34_explicit_column_widths | 1.0 | 0.996 | 1/1 | **0.9984** |
| 86 | 🟢 classic35_explicit_row_heights | 1.0 | 0.9987 | 1/1 | **0.9995** |
| 87 | 🟢 classic36_merged_cells | 1.0 | 0.9958 | 1/1 | **0.9983** |
| 88 | 🟢 classic37_freeze_panes | 1.0 | 0.9899 | 1/1 | **0.996** |
| 89 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9968 | 1/1 | **0.9987** |
| 90 | 🟢 classic39_financial_table | 1.0 | 0.9945 | 1/1 | **0.9978** |
| 91 | 🟢 classic40_scientific_notation | 1.0 | 0.996 | 1/1 | **0.9984** |
| 92 | 🟢 classic41_integer_vs_float | 1.0 | 0.9969 | 1/1 | **0.9988** |
| 93 | 🟢 classic42_boolean_values | 0.9895 | 0.9954 | 1/1 | **0.994** |
| 94 | 🟢 classic43_inventory_report | 0.9984 | 0.9905 | 1/1 | **0.9956** |
| 95 | 🟢 classic44_employee_roster | 0.9967 | 0.9847 | 1/1 | **0.9926** |
| 96 | 🟢 classic45_sales_by_region | 1.0 | 0.9976 | 4/4 | **0.999** |
| 97 | 🟢 classic46_grade_book | 1.0 | 0.9921 | 1/1 | **0.9968** |
| 98 | 🟢 classic47_time_series | 1.0 | 0.9847 | 1/1 | **0.9939** |
| 99 | 🟢 classic48_survey_results | 0.9942 | 0.9932 | 1/1 | **0.995** |
| 100 | 🟢 classic49_contact_list | 0.9459 | 0.9892 | 1/1 | **0.974** |
| 101 | 🟢 classic50_budget_vs_actuals | 1.0 | 0.9907 | 3/3 | **0.9963** |
| 102 | 🟢 classic51_product_catalog | 0.9782 | 0.987 | 1/1 | **0.9861** |
| 103 | 🟢 classic52_pivot_summary | 1.0 | 0.9906 | 1/1 | **0.9962** |
| 104 | 🟢 classic53_invoice | 0.9951 | 0.9914 | 1/1 | **0.9946** |
| 105 | 🟢 classic54_multi_level_header | 1.0 | 0.9947 | 1/1 | **0.9979** |
| 106 | 🟢 classic55_error_values | 1.0 | 0.9944 | 1/1 | **0.9978** |
| 107 | 🟢 classic56_alternating_row_colors | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 108 | 🟡 classic57_cjk_only | 0.7826 | 0.9144 | 1/1 | **0.8788** |
| 109 | 🟢 classic58_mixed_numeric_formats | 0.9937 | 0.9953 | 1/1 | **0.9956** |
| 110 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9964 | 4/4 | **0.9986** |
| 111 | 🟢 classic60_large_wide_table | 1.0 | 0.9368 | 4/4 | **0.9747** |
| 112 | 🟢 classic61_product_card_with_image | 1.0 | 0.9971 | 1/1 | **0.9988** |
| 113 | 🟢 classic62_company_logo_header | 1.0 | 0.9938 | 1/1 | **0.9975** |
| 114 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9956 | 1/1 | **0.9982** |
| 115 | 🟢 classic64_employee_directory_with_photo | 0.9967 | 0.9908 | 1/1 | **0.995** |
| 116 | 🟢 classic65_inventory_with_product_photos | 0.9906 | 0.9911 | 1/1 | **0.9927** |
| 117 | 🟢 classic66_invoice_with_logo | 0.9868 | 0.9951 | 1/1 | **0.9928** |
| 118 | 🟢 classic67_real_estate_listing | 1.0 | 0.9928 | 1/1 | **0.9971** |
| 119 | 🟢 classic68_restaurant_menu | 0.9784 | 0.9873 | 1/1 | **0.9863** |
| 120 | 🟢 classic69_image_only_sheet | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 121 | 🟢 classic70_product_catalog_with_images | 0.9826 | 0.989 | 1/1 | **0.9886** |
| 122 | 🟢 classic71_multi_sheet_with_images | 1.0 | 0.9981 | 3/3 | **0.9992** |
| 123 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.9913 | 1/1 | **0.9965** |
| 124 | 🟢 classic73_event_flyer_with_banner | 0.9446 | 0.9919 | 1/1 | **0.9746** |
| 125 | 🟢 classic74_dashboard_with_kpi_image | 0.9625 | 0.9903 | 1/1 | **0.9811** |
| 126 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9891 | 1/1 | **0.9956** |
| 127 | 🟢 classic76_product_image_grid | 1.0 | 0.9919 | 1/1 | **0.9968** |
| 128 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.989 | 1/1 | **0.9956** |
| 129 | 🟢 classic78_small_icon_per_row | 0.9898 | 0.9941 | 1/1 | **0.9936** |
| 130 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9934 | 1/1 | **0.9974** |
| 131 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9921 | 1/1 | **0.9968** |
| 132 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.9889 | 1/1 | **0.9956** |
| 133 | 🟢 classic82_before_after_images | 0.9925 | 0.9929 | 1/1 | **0.9942** |
| 134 | 🟢 classic83_color_swatch_palette | 0.9916 | 0.9901 | 1/1 | **0.9927** |
| 135 | 🟢 classic84_travel_destination_cards | 1.0 | 0.9864 | 1/1 | **0.9946** |
| 136 | 🟢 classic85_lab_results_with_image | 0.9933 | 0.9913 | 1/1 | **0.9938** |
| 137 | 🟢 classic86_software_screenshot_features | 0.9862 | 0.993 | 1/1 | **0.9917** |
| 138 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.9928 | 1/1 | **0.9971** |
| 139 | 🟢 classic88_image_after_data | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 140 | 🟢 classic89_nutrition_label_with_image | 0.9976 | 0.9936 | 1/1 | **0.9965** |
| 141 | 🟢 classic90_project_status_with_milestones | 0.9585 | 0.9889 | 1/1 | **0.979** |
| 142 | 🟢 classic91_simple_bar_chart | 0.9493 | 0.9626 | 2/2 | **0.9648** |
| 143 | 🟢 classic92_horizontal_bar_chart | 0.9706 | 0.9665 | 2/2 | **0.9748** |
| 144 | 🟢 classic93_line_chart | 0.8257 | 0.9861 | 2/2 | **0.9247** |
| 145 | 🟢 classic94_pie_chart | 0.878 | 0.926 | 2/2 | **0.9216** |
| 146 | 🟡 classic95_area_chart | 0.6441 | 0.765 | 2/2 | **0.7636** |
| 147 | 🟢 classic96_scatter_chart | 0.8921 | 0.9855 | 2/2 | **0.951** |
| 148 | 🟢 classic97_doughnut_chart | 0.8571 | 0.9319 | 2/2 | **0.9156** |
| 149 | 🟢 classic98_radar_chart | 0.8876 | 0.9894 | 2/2 | **0.9508** |
| 150 | 🟢 classic99_bubble_chart | 0.8447 | 0.9682 | 2/2 | **0.9252** |

**Average Overall Score: 0.9712**

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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic02_multiple_worksheets</b><br><small>p1</small></td>
      <td><img src="images/classic02_multiple_worksheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic02_multiple_worksheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.998</td>
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
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
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
      <td rowspan="5" valign="top"><span style="color:#3fb950">⬤</span> 0.9788</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9996</td>
    </tr>
    <tr>
      <td valign="top"><b>classic08_mixed_text_and_numbers</b></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic08_mixed_text_and_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9991</td>
    </tr>
    <tr>
      <td rowspan="12" valign="top"><b>classic09_long_text</b><br><small>p1</small></td>
      <td><img src="images/classic09_long_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic09_long_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="12" valign="top"><span style="color:#f85149">⬤</span> 0.6022</td>
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
      <td valign="top"><b>classic100_stacked_bar_chart</b></td>
      <td><img src="images/classic100_stacked_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic100_stacked_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9528</td>
    </tr>
    <tr>
      <td valign="top"><b>classic101_percent_stacked_bar</b></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9375</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic102_line_chart_with_markers</b><br><small>p1</small></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9301</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8847</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8157</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8597</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9171</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8062</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9448</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic109_scatter_with_trendline</b><br><small>p1</small></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9258</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic110_chart_with_legend</b><br><small>p1</small></td>
      <td><img src="images/classic110_chart_with_legend_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic110_chart_with_legend_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8567</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.921</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8526</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8654</td>
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
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9153</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9215</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9391</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic117_stock_ohlc_chart</b><br><small>p1</small></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.808</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9561</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9103</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9998</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7427</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic120_chart_with_date_axis_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic120_chart_with_date_axis_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic121_thin_borders</b></td>
      <td><img src="images/classic121_thin_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic121_thin_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9974</td>
    </tr>
    <tr>
      <td valign="top"><b>classic122_thick_outer_thin_inner</b></td>
      <td><img src="images/classic122_thick_outer_thin_inner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic122_thick_outer_thin_inner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic123_dashed_borders</b></td>
      <td><img src="images/classic123_dashed_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic123_dashed_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9948</td>
    </tr>
    <tr>
      <td valign="top"><b>classic124_colored_borders</b></td>
      <td><img src="images/classic124_colored_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic124_colored_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic125_solid_fills</b></td>
      <td><img src="images/classic125_solid_fills_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic125_solid_fills_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic126_dark_header</b></td>
      <td><img src="images/classic126_dark_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic126_dark_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9934</td>
    </tr>
    <tr>
      <td valign="top"><b>classic127_font_styles</b></td>
      <td><img src="images/classic127_font_styles_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic127_font_styles_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9933</td>
    </tr>
    <tr>
      <td valign="top"><b>classic128_font_sizes</b></td>
      <td><img src="images/classic128_font_sizes_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic128_font_sizes_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9903</td>
    </tr>
    <tr>
      <td valign="top"><b>classic129_alignment_combos</b></td>
      <td><img src="images/classic129_alignment_combos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic129_alignment_combos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
    </tr>
    <tr>
      <td valign="top"><b>classic12_sparse_columns</b></td>
      <td><img src="images/classic12_sparse_columns_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic12_sparse_columns_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
    </tr>
    <tr>
      <td valign="top"><b>classic130_wrap_and_indent</b></td>
      <td><img src="images/classic130_wrap_and_indent_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic130_wrap_and_indent_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9947</td>
    </tr>
    <tr>
      <td valign="top"><b>classic131_number_formats</b></td>
      <td><img src="images/classic131_number_formats_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic131_number_formats_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9967</td>
    </tr>
    <tr>
      <td valign="top"><b>classic132_striped_table</b></td>
      <td><img src="images/classic132_striped_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic132_striped_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.991</td>
    </tr>
    <tr>
      <td valign="top"><b>classic133_gradient_rows</b></td>
      <td><img src="images/classic133_gradient_rows_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic133_gradient_rows_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.997</td>
    </tr>
    <tr>
      <td valign="top"><b>classic134_heatmap</b></td>
      <td><img src="images/classic134_heatmap_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic134_heatmap_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9887</td>
    </tr>
    <tr>
      <td valign="top"><b>classic135_bottom_border_only</b></td>
      <td><img src="images/classic135_bottom_border_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic135_bottom_border_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9982</td>
    </tr>
    <tr>
      <td valign="top"><b>classic136_financial_report_styled</b></td>
      <td><img src="images/classic136_financial_report_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic136_financial_report_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9928</td>
    </tr>
    <tr>
      <td valign="top"><b>classic137_checkerboard</b></td>
      <td><img src="images/classic137_checkerboard_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic137_checkerboard_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9847</td>
    </tr>
    <tr>
      <td valign="top"><b>classic138_color_grid</b></td>
      <td><img src="images/classic138_color_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic138_color_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9944</td>
    </tr>
    <tr>
      <td valign="top"><b>classic139_pattern_fills</b></td>
      <td><img src="images/classic139_pattern_fills_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic139_pattern_fills_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9933</td>
    </tr>
    <tr>
      <td valign="top"><b>classic13_date_strings</b></td>
      <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic140_rotated_text</b></td>
      <td><img src="images/classic140_rotated_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic140_rotated_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9809</td>
    </tr>
    <tr>
      <td valign="top"><b>classic141_mixed_edge_borders</b></td>
      <td><img src="images/classic141_mixed_edge_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic141_mixed_edge_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic142_styled_invoice</b></td>
      <td><img src="images/classic142_styled_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic142_styled_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9836</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic143_colored_tabs</b><br><small>p1</small></td>
      <td><img src="images/classic143_colored_tabs_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic143_colored_tabs_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9994</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic143_colored_tabs_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic143_colored_tabs_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic143_colored_tabs_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic143_colored_tabs_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td align="center"><small>p4</small></td>
      <td><img src="images/classic143_colored_tabs_p4_minipdf.png" width="340" alt="MiniPdf p4"></td>
      <td><img src="images/classic143_colored_tabs_p4_reference.png" width="340" alt="Reference p4"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic144_note_style_cells</b></td>
      <td><img src="images/classic144_note_style_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic144_note_style_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9958</td>
    </tr>
    <tr>
      <td valign="top"><b>classic145_status_badges</b></td>
      <td><img src="images/classic145_status_badges_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic145_status_badges_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9918</td>
    </tr>
    <tr>
      <td valign="top"><b>classic146_double_border_table</b></td>
      <td><img src="images/classic146_double_border_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic146_double_border_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.996</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic147_multi_sheet_styled</b><br><small>p1</small></td>
      <td><img src="images/classic147_multi_sheet_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic147_multi_sheet_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td align="center"><small>p2</small></td>
      <td><img src="images/classic147_multi_sheet_styled_p2_minipdf.png" width="340" alt="MiniPdf p2"></td>
      <td><img src="images/classic147_multi_sheet_styled_p2_reference.png" width="340" alt="Reference p2"></td>
    </tr>
    <tr>
      <td align="center"><small>p3</small></td>
      <td><img src="images/classic147_multi_sheet_styled_p3_minipdf.png" width="340" alt="MiniPdf p3"></td>
      <td><img src="images/classic147_multi_sheet_styled_p3_reference.png" width="340" alt="Reference p3"></td>
    </tr>
    <tr>
      <td valign="top"><b>classic148_frozen_styled_grid</b></td>
      <td><img src="images/classic148_frozen_styled_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic148_frozen_styled_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9744</td>
    </tr>
    <tr>
      <td valign="top"><b>classic149_merged_styled_sections</b></td>
      <td><img src="images/classic149_merged_styled_sections_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic149_merged_styled_sections_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9876</td>
    </tr>
    <tr>
      <td valign="top"><b>classic14_decimal_numbers</b></td>
      <td><img src="images/classic14_decimal_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic14_decimal_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9989</td>
    </tr>
    <tr>
      <td valign="top"><b>classic150_kitchen_sink_styles</b></td>
      <td><img src="images/classic150_kitchen_sink_styles_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic150_kitchen_sink_styles_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9749</td>
    </tr>
    <tr>
      <td valign="top"><b>classic15_negative_numbers</b></td>
      <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic16_percentage_strings</b></td>
      <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9963</td>
    </tr>
    <tr>
      <td valign="top"><b>classic17_currency_strings</b></td>
      <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
    </tr>
    <tr>
      <td rowspan="24" valign="top"><b>classic18_large_dataset</b><br><small>p1</small></td>
      <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="24" valign="top"><span style="color:#3fb950">⬤</span> 0.9588</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9146</td>
    </tr>
    <tr>
      <td valign="top"><b>classic24_red_text</b></td>
      <td><img src="images/classic24_red_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic24_red_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
    </tr>
    <tr>
      <td valign="top"><b>classic25_multiple_colors</b></td>
      <td><img src="images/classic25_multiple_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic25_multiple_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.998</td>
    </tr>
    <tr>
      <td valign="top"><b>classic26_inline_strings</b></td>
      <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
    </tr>
    <tr>
      <td valign="top"><b>classic29_formula_results</b></td>
      <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9989</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
    </tr>
    <tr>
      <td valign="top"><b>classic32_right_aligned_numbers</b></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic32_right_aligned_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.999</td>
    </tr>
    <tr>
      <td valign="top"><b>classic33_centered_text</b></td>
      <td><img src="images/classic33_centered_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic33_centered_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic34_explicit_column_widths</b></td>
      <td><img src="images/classic34_explicit_column_widths_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic34_explicit_column_widths_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
    </tr>
    <tr>
      <td valign="top"><b>classic35_explicit_row_heights</b></td>
      <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic36_merged_cells</b></td>
      <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9983</td>
    </tr>
    <tr>
      <td valign="top"><b>classic37_freeze_panes</b></td>
      <td><img src="images/classic37_freeze_panes_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic37_freeze_panes_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.996</td>
    </tr>
    <tr>
      <td valign="top"><b>classic38_hyperlink_cell</b></td>
      <td><img src="images/classic38_hyperlink_cell_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic38_hyperlink_cell_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
    </tr>
    <tr>
      <td valign="top"><b>classic39_financial_table</b></td>
      <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic40_scientific_notation</b></td>
      <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
    </tr>
    <tr>
      <td valign="top"><b>classic41_integer_vs_float</b></td>
      <td><img src="images/classic41_integer_vs_float_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic41_integer_vs_float_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic42_boolean_values</b></td>
      <td><img src="images/classic42_boolean_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic42_boolean_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.994</td>
    </tr>
    <tr>
      <td valign="top"><b>classic43_inventory_report</b></td>
      <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td valign="top"><b>classic44_employee_roster</b></td>
      <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9926</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic45_sales_by_region</b><br><small>p1</small></td>
      <td><img src="images/classic45_sales_by_region_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic45_sales_by_region_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.999</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9968</td>
    </tr>
    <tr>
      <td valign="top"><b>classic47_time_series</b></td>
      <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9939</td>
    </tr>
    <tr>
      <td valign="top"><b>classic48_survey_results</b></td>
      <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic49_contact_list</b></td>
      <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.974</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic50_budget_vs_actuals</b><br><small>p1</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9963</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9861</td>
    </tr>
    <tr>
      <td valign="top"><b>classic52_pivot_summary</b></td>
      <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9962</td>
    </tr>
    <tr>
      <td valign="top"><b>classic53_invoice</b></td>
      <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9946</td>
    </tr>
    <tr>
      <td valign="top"><b>classic54_multi_level_header</b></td>
      <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9979</td>
    </tr>
    <tr>
      <td valign="top"><b>classic55_error_values</b></td>
      <td><img src="images/classic55_error_values_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic55_error_values_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic56_alternating_row_colors</b></td>
      <td><img src="images/classic56_alternating_row_colors_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic56_alternating_row_colors_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9965</td>
    </tr>
    <tr>
      <td valign="top"><b>classic57_cjk_only</b></td>
      <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8788</td>
    </tr>
    <tr>
      <td valign="top"><b>classic58_mixed_numeric_formats</b></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic59_multi_sheet_summary</b><br><small>p1</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
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
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9747</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic62_company_logo_header</b></td>
      <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic63_two_products_side_by_side</b></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9982</td>
    </tr>
    <tr>
      <td valign="top"><b>classic64_employee_directory_with_photo</b></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic65_inventory_with_product_photos</b></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9927</td>
    </tr>
    <tr>
      <td valign="top"><b>classic66_invoice_with_logo</b></td>
      <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9928</td>
    </tr>
    <tr>
      <td valign="top"><b>classic67_real_estate_listing</b></td>
      <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9971</td>
    </tr>
    <tr>
      <td valign="top"><b>classic68_restaurant_menu</b></td>
      <td><img src="images/classic68_restaurant_menu_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic68_restaurant_menu_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9863</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9886</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic71_multi_sheet_with_images</b><br><small>p1</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9965</td>
    </tr>
    <tr>
      <td valign="top"><b>classic73_event_flyer_with_banner</b></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9746</td>
    </tr>
    <tr>
      <td valign="top"><b>classic74_dashboard_with_kpi_image</b></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9811</td>
    </tr>
    <tr>
      <td valign="top"><b>classic75_certificate_with_seal</b></td>
      <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td valign="top"><b>classic76_product_image_grid</b></td>
      <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9968</td>
    </tr>
    <tr>
      <td valign="top"><b>classic77_news_article_with_hero_image</b></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td valign="top"><b>classic78_small_icon_per_row</b></td>
      <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9936</td>
    </tr>
    <tr>
      <td valign="top"><b>classic79_wide_panoramic_banner</b></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9974</td>
    </tr>
    <tr>
      <td valign="top"><b>classic80_portrait_tall_image</b></td>
      <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9968</td>
    </tr>
    <tr>
      <td valign="top"><b>classic81_step_by_step_with_images</b></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td valign="top"><b>classic82_before_after_images</b></td>
      <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic83_color_swatch_palette</b></td>
      <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9927</td>
    </tr>
    <tr>
      <td valign="top"><b>classic84_travel_destination_cards</b></td>
      <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9946</td>
    </tr>
    <tr>
      <td valign="top"><b>classic85_lab_results_with_image</b></td>
      <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9938</td>
    </tr>
    <tr>
      <td valign="top"><b>classic86_software_screenshot_features</b></td>
      <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9917</td>
    </tr>
    <tr>
      <td valign="top"><b>classic87_sports_results_with_logos</b></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9971</td>
    </tr>
    <tr>
      <td valign="top"><b>classic88_image_after_data</b></td>
      <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic89_nutrition_label_with_image</b></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9965</td>
    </tr>
    <tr>
      <td valign="top"><b>classic90_project_status_with_milestones</b></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.979</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic91_simple_bar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic91_simple_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic91_simple_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9648</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9748</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9247</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9216</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7636</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.951</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9156</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9252</td>
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
- **Visual Average:** 0.9971
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1365 bytes, Reference=30311 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 0.9971
- **Visual Average:** 0.9978
- **Overall Score:** 0.998
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2378 bytes, Reference=36003 bytes

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
- **Visual Average:** 0.9998
- **Overall Score:** 0.9999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=626 bytes, Reference=19860 bytes

Text content: ✅ Identical

### classic05_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=9038 bytes, Reference=62308 bytes

Text content: ✅ Identical

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9469
- **Overall Score:** 0.9788
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=40702 bytes, Reference=185703 bytes

Text content: ✅ Identical

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9991
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1203 bytes, Reference=24806 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9978
- **Overall Score:** 0.9991
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1204 bytes, Reference=27336 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.6785
- **Visual Average:** 0.5771
- **Overall Score:** 0.6022
- **Pages:** MiniPdf=7, Reference=12
- **File Size:** MiniPdf=2435 bytes, Reference=29170 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,12 +1,24 @@
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

+---PAGE---

+

 ---PAGE---

 

 ---PAGE---

```
</details>

### classic100_stacked_bar_chart

- **Text Similarity:** 0.9663
- **Visual Average:** 0.9158
- **Overall Score:** 0.9528
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4568 bytes, Reference=47565 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic100_stacked_bar_chart.pdf
+++ reference/classic100_stacked_bar_chart.pdf
@@ -7,8 +7,11 @@
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

- **Text Similarity:** 0.9623
- **Visual Average:** 0.8815
- **Overall Score:** 0.9375
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5270 bytes, Reference=49462 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic101_percent_stacked_bar.pdf
+++ reference/classic101_percent_stacked_bar.pdf
@@ -9,8 +9,11 @@
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

- **Text Similarity:** 0.8372
- **Visual Average:** 0.9881
- **Overall Score:** 0.9301
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4796 bytes, Reference=52236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic102_line_chart_with_markers.pdf
+++ reference/classic102_line_chart_with_markers.pdf
@@ -1,16 +1,22 @@
 Year Users (K) Revenue (K)

-2020 10 50 Company Growth

+2020 10 50

+Company Growth

 2021 25 120

-2022 55 280 1200

+2022 55 280

+1200

 2023 90 500

 2024 140 780

+2025 200 1100

 1000

-2025 200 1100

 800

+Value (K)

 600

-Value (K)

 400

 200

 0

-2020 2021 2022 2023 2024

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

- **Text Similarity:** 0.7368
- **Visual Average:** 0.975
- **Overall Score:** 0.8847
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403976 bytes, Reference=48488 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic103_pie_chart_with_labels.pdf
+++ reference/classic103_pie_chart_with_labels.pdf
@@ -1,10 +1,28 @@
 OS Share (%)

-Windows 42 Desktop OS Market Share

+Windows 42

+Desktop OS Market Share

 macOS 28

 Linux 15

-ChromeOS 10 Other; Share (%); 5; 5%

-Other 5 ChromeOS; Share (%); 10; 10%

-Windows; Share (%); 42; 42%

-Linux; Share (%); 15; 15%

-macOS; Share (%); 28; 28%

----PAGE---
+ChromeOS 10

+Other; Share

+Other 5

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

- **Text Similarity:** 0.7872
- **Visual Average:** 0.7521
- **Overall Score:** 0.8157
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4478 bytes, Reference=54330 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic104_combo_bar_line_chart.pdf
+++ reference/classic104_combo_bar_line_chart.pdf
@@ -1,15 +1,21 @@
 Month Sales Target

-Jan 42 45 Sales vs Target

+Jan 42 45

+Sales vs Target

 Feb 48 47

-Mar 51 50 70

+Mar 51 50

+70 70

 Apr 45 50

-May 56 54 60

+May 56 54

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

- **Text Similarity:** 0.9032
- **Visual Average:** 0.7461
- **Overall Score:** 0.8597
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3907 bytes, Reference=138437 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic105_3d_bar_chart.pdf
+++ reference/classic105_3d_bar_chart.pdf
@@ -1,7 +1,8 @@
 Region 2024 2025

-APAC 120 145 Revenue by Region (3D)

+APAC 120 145

+Revenue by Region (3

 EMEA 95 110

-Americas 150 175 200

+Americas 150 175

 LATAM 40 55

 180

 160

@@ -13,5 +14,9 @@
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

- **Text Similarity:** 0.8243
- **Visual Average:** 0.9684
- **Overall Score:** 0.9171
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403759 bytes, Reference=76353 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic106_3d_pie_chart.pdf
+++ reference/classic106_3d_pie_chart.pdf
@@ -1,8 +1,17 @@
 Category Amount

-Food 800 Monthly Expense Breakdown (3D)

+Food 800

+Monthly Expense Breakdown (3D)

 Housing 1500

 Transport 400

 Entertainm 300

 Savings 700

 Other 200

----PAGE---
+Food

+Housing

+Transpo

+Entertai

+Savings

+Other

+---PAGE---

+rt

+nment
```
</details>

### classic107_multi_series_line

- **Text Similarity:** 0.7375
- **Visual Average:** 0.7781
- **Overall Score:** 0.8062
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=15256 bytes, Reference=82303 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic107_multi_series_line.pdf
+++ reference/classic107_multi_series_line.pdf
@@ -1,33 +1,41 @@
 Day AAPL GOOG MSFT

-Day 1 178.48 140.49 402.83 Stock Price Trend (20 Days

+Day 1 178.48 140.49 402.83

+Stock Price

 Day 2 179.43 140.38 401.69

-Day 3 177.25 143.38 403.21 450

+Day 3 177.25 143.38 403.21

+450

 Day 4 175.75 143.94 404.47

+Day 5 178.19 142.62 403.35

 400

-Day 5 178.19 142.62 403.35

 Day 6 176.32 143.16 405.88

 350

 Day 7 177.72 141 405.11

+Day 8 175.18 138.97 405.07

 300

-Day 8 175.18 138.97 405.07

 Day 9 173.1 137.59 403.53

 250

 Day 10 172.64 139.72 401.94

+Price ($)

+Day 11 173.32 139.12 400.69

 200

-Day 11 173.32 139.12 400.69

-Price ($)

 Day 12 172.11 140.8 402.75

 150

 Day 13 173.5 143.13 404.12

+Day 14 172.29 141.53 404.52

 100

-Day 14 172.29 141.53 404.52

 Day 15 172.95 143.24 406.95

 50

 Day 16 174.74 146.1 408

+Day 17 175.83 147.89 407.98

 0

-Day 17 175.83 147.89 407.98

-Day 1Day 2Day 3Day 4Day 5Day 6Day 7Day 8Day 9Day 10Day 11Day 12Da

-Day 18 177.62 150.15 408.05

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

- **Text Similarity:** 0.9643
- **Visual Average:** 0.8977
- **Overall Score:** 0.9448
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=10919 bytes, Reference=51253 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic108_stacked_area_chart.pdf
+++ reference/classic108_stacked_area_chart.pdf
@@ -9,8 +9,12 @@
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

- **Text Similarity:** 0.8293
- **Visual Average:** 0.9853
- **Overall Score:** 0.9258
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5356 bytes, Reference=60738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic109_scatter_with_trendline.pdf
+++ reference/classic109_scatter_with_trendline.pdf
@@ -1,18 +1,21 @@
-Study HourExam Score

-5 59 Study Hours vs Exam Score

+Study HouExam Score

+5 59

+Study Hours vs Exam Score

 8 90

-9 85 120

+9 85

+120

 2 35

 9 99

 100

 5 68

-2 35

-80

-8 92

+f(x) = 8.12719751809721 x + 20.8283350568769

+2 35 R² = 0.958630685218316

+8 92 80

 5 65

-3 45 60

+Stud

+3 45 Score 60

+Line

 9 100

-Score

 6 62

 40

 9 89

@@ -22,4 +25,6 @@
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
- **Visual Average:** 0.9966
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=989 bytes, Reference=27644 bytes

Text content: ✅ Identical

### classic110_chart_with_legend

- **Text Similarity:** 0.8605
- **Visual Average:** 0.7813
- **Overall Score:** 0.8567
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3943 bytes, Reference=52253 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic110_chart_with_legend.pdf
+++ reference/classic110_chart_with_legend.pdf
@@ -1,15 +1,21 @@
 Browser 2024 (%) 2025 (%)

-Chrome 65 62 Browser Market Share Comparison

+Chrome 65 62

+Browser Market Share Com

 Safari 18 20

-Firefox 8 7 70

+Firefox 8 7

+70

 Edge 6 8

-Other 3 3 60

+Other 3 3

+60

 50

+Market Share (%)

 40

 30

-Market Share (%)

 20

 10

 0

-Chrome Safari Firefox Edge Other

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
- **Visual Average:** 0.9758
- **Overall Score:** 0.921
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3417 bytes, Reference=51007 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic111_chart_with_axis_labels.pdf
+++ reference/classic111_chart_with_axis_labels.pdf
@@ -1,5 +1,6 @@
 Country CO2 (Mt)

-China 10500 CO2 Emissions by Country

+China 10500

+CO2 Emissions by Country

 USA 5000

 India 2700

 Russia 1700

@@ -8,9 +9,12 @@
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

- **Text Similarity:** 0.875
- **Visual Average:** 0.7566
- **Overall Score:** 0.8526
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6776 bytes, Reference=59342 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic112_multiple_charts.pdf
+++ reference/classic112_multiple_charts.pdf
@@ -1,18 +1,21 @@
 Month Revenue Costs Profit

-Jan 50 30 20 Revenue & Costs

+Jan 50 30 20

+Revenue & Co

 Feb 55 32 23

-Mar 60 35 25 80

+Mar 60 35 25

+80

 Apr 52 28 24

+May 70 40 30

 70

-May 70 40 30

-Jun 75 42 33 60

+Jun 75 42 33

+60

 50

 40

 30

 20

 10

 0

-Jan Feb Mar Apr May

+Jan Feb Mar Apr

 Profit Trend

 35

 30

@@ -22,5 +25,12 @@
 10

 5

 0

-Jan Feb Mar Apr Ma

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
- **Visual Average:** 0.7375
- **Overall Score:** 0.8654
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3159 bytes, Reference=43602 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic113_chart_sheet.pdf
+++ reference/classic113_chart_sheet.pdf
@@ -1,8 +1,9 @@
 Quarter Revenue

-Q1 250 Quarterly Revenue

+Q1 250

+Quarterly Revenue

 Q2 310

-Q3 285 450

-Q4 400

+Q3 285

+Q4 400 450

 400

 350

 300

@@ -12,5 +13,7 @@
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

- **Text Similarity:** 0.9015
- **Visual Average:** 0.8867
- **Overall Score:** 0.9153
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=30728 bytes, Reference=128765 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic114_chart_large_dataset.pdf
+++ reference/classic114_chart_large_dataset.pdf
@@ -1,30 +1,33 @@
 Day Value

-1 97.7 100-Day Value Trend

+1 97.7

+100-Day Value Tr

 2 93.7

-3 96.1 160

+3 96.1

+160

 4 93.7

+5 95.6

 140

-5 95.6

 6 92.3

+7 98.1

 120

-7 98.1

 8 100.5

+9 98.7

 100

-9 98.7

-10 94.4 80

+10 94.4

+80

 11 98.6

+12 103.5

 60

-12 103.5

 13 102.2

+14 98.4

 40

-14 98.4

 15 104.2

+16 109

 20

-16 109

+17 109.1

 0

-17 109.1

-1234567891011213141516171819202122324252627282930313233435363738394041424344546474849505152535455657585960616263646566768697071727374757677879808182838485868788

 18 105.3

+1 5 9 13 17 21 25 29 33 37 41 45 49 53 57 61 65

 19 108.6

 20 114.2

 21 112.6

@@ -109,4 +112,7 @@
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

- **Text Similarity:** 0.8316
- **Visual Average:** 0.9721
- **Overall Score:** 0.9215
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4193 bytes, Reference=51633 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic115_chart_negative_values.pdf
+++ reference/classic115_chart_negative_values.pdf
@@ -1,22 +1,26 @@
 Month Profit/Loss

-Jan 15 Monthly Profit & Loss

+Jan 15

+Monthly Profit & Loss

 Feb -8

-Mar 22 35

+Mar 22

+35

 Apr -3

+May 30

 30

-May 30

+Jun -12

 25

-Jun -12

 Jul 18

+Aug 5

 20

-Aug 5

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

- **Text Similarity:** 0.9649
- **Visual Average:** 0.8828
- **Overall Score:** 0.9391
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11165 bytes, Reference=50765 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic116_percent_stacked_area.pdf
+++ reference/classic116_percent_stacked_area.pdf
@@ -10,8 +10,11 @@
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

- **Text Similarity:** 0.7897
- **Visual Average:** 0.7303
- **Overall Score:** 0.808
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=8324 bytes, Reference=62401 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic117_stock_ohlc_chart.pdf
+++ reference/classic117_stock_ohlc_chart.pdf
@@ -1,23 +1,27 @@
 Day Open High Low Close

-Day 1 148.96 149.78 146.96 147.41 Stock OHLC (10

+Day 1 148.96 149.78 146.96 147.41

+St

 Day 2 147.04 147.63 144.4 146.23

-Day 3 145.63 149.68 145.47 149.58 180

+Day 3 145.63 149.68 145.47 149.58

+160

 Day 4 149.32 150.14 147.39 148.55

-160

 Day 5 146.58 150.1 143.38 147.36

 Day 6 147.91 152.44 145.49 149.32

-140

+155

 Day 7 151.08 155.51 150.22 150.81

-120

 Day 8 152.42 155.53 152.31 152.99

 Day 9 152.32 154.36 151.02 152.05

-100

+150

 Day 10 152.27 156.85 148.76 156.35

-80

 Price ($)

-60

-40

-20

-0

-Day 1 Day 2 Day 3 Day 4 Day 5 D

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

- **Text Similarity:** 0.9275
- **Visual Average:** 0.9627
- **Overall Score:** 0.9561
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3529 bytes, Reference=48780 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic118_bar_chart_custom_colors.pdf
+++ reference/classic118_bar_chart_custom_colors.pdf
@@ -1,10 +1,12 @@
 Rating Count

-Excellent 45 Customer Satisfaction Survey

+Excellent 45

+Customer Satisfaction Survey

 Good 30

-Average 15 50

+Average 15

+50

 Poor 7

+Very Poor 3

 45

-Very Poor 3

 40

 35

 30

@@ -14,5 +16,7 @@
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

- **Text Similarity:** 0.8409
- **Visual Average:** 0.9349
- **Overall Score:** 0.9103
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=218239 bytes, Reference=65175 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic119_dashboard_multi_charts.pdf
+++ reference/classic119_dashboard_multi_charts.pdf
@@ -1,19 +1,24 @@
 KPI Dashboard - Q4 2025

 Revenue vs Expenses

 Month Revenue Expenses

-Oct 85 60 120

+Oct 85 60

+120

 Nov 92 65

+Dec 110 70

 100

-Dec 110 70

 80

-60

-Segment Share

+Segment Share 60

+Enterprise 45

 40

-Enterprise 45

 SMB 30

+Consumer 25

 20

-Consumer 25

 0

 Oct Nov Dec

 Revenue by Segment

----PAGE---
+Enterprise

+SMB

+Consumer

+---PAGE---

+Revenue

+Expenses
```
</details>

### classic11_sparse_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.9994
- **Overall Score:** 0.9998
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1052 bytes, Reference=23538 bytes

Text content: ✅ Identical

### classic120_chart_with_date_axis

- **Text Similarity:** 0.5738
- **Visual Average:** 0.783
- **Overall Score:** 0.7427
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5855 bytes, Reference=56955 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic120_chart_with_date_axis.pdf
+++ reference/classic120_chart_with_date_axis.pdf
@@ -1,26 +1,32 @@
 Date Downloads

-2025-01-0 581 Monthly Downloads (2025)

+2025-01-0 581

+Monthly Downloads (20

 2025-01-3 594

-2025-03-0 592 900

+2025-03-0 592

+1000

 2025-04-0 692

+2025-05-0 760 900

+2025-05-3 733

 800

-2025-05-0 760

-2025-05-3 733

+2025-06-3 763

 700

-2025-06-3 763

+2025-07-3 767

 600

-2025-07-3 767

 2025-08-2 774

+Downloads

 500

 2025-09-2 788

 400

 2025-10-2 820

-Downloads

-2025-11-2 865

-300

+2025-11-2 865 300

 200

 100

 0

-2025-01-01 2025-01-31 2025-03-02 2025-04-01 2025-05-01 2025-05-31 2025-06-30 2025-07-30 2025-08-29 2025-09

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

### classic121_thin_borders

- **Text Similarity:** 1.0
- **Visual Average:** 0.9936
- **Overall Score:** 0.9974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8208 bytes, Reference=39925 bytes

Text content: ✅ Identical

### classic122_thick_outer_thin_inner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8200 bytes, Reference=40404 bytes

Text content: ✅ Identical

### classic123_dashed_borders

- **Text Similarity:** 0.992
- **Visual Average:** 0.995
- **Overall Score:** 0.9948
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2576 bytes, Reference=35187 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic123_dashed_borders.pdf
+++ reference/classic123_dashed_borders.pdf
@@ -1,4 +1,4 @@
-Border StylSample

+Border StSample

 dashed Bordered cell

 dotted Bordered cell

 dashDot Bordered cell

```
</details>

### classic124_colored_borders

- **Text Similarity:** 1.0
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3361 bytes, Reference=38667 bytes

Text content: ✅ Identical

### classic125_solid_fills

- **Text Similarity:** 0.992
- **Visual Average:** 0.9934
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2034 bytes, Reference=39001 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic125_solid_fills.pdf
+++ reference/classic125_solid_fills.pdf
@@ -1,9 +1,9 @@
-Fill Name Filled Cell

-Light BlueBackground

+Fill NameFilled Cell

+Light Blue Background

 Light GreeBackground

 Light YelloBackground

 Light Red Background

 Light PurplBackground

 Light OranBackground

-Gray 25%Background

+Gray 25% Background

 Sky Blue Background
```
</details>

### classic126_dark_header

- **Text Similarity:** 0.9929
- **Visual Average:** 0.9905
- **Overall Score:** 0.9934
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2210 bytes, Reference=44287 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic126_dark_header.pdf
+++ reference/classic126_dark_header.pdf
@@ -1,6 +1,6 @@
 EmployeeDepartmen Salary Start Date

 Alice SmithEngineerin 95000 2020-03-15

-Bob JonesMarketing 72000 2019-07-01

+Bob Jones Marketing 72000 2019-07-01

 Carol Lee Finance 88000 2021-01-10

-David KimEngineerin 102000 2018-11-20

-Eva ChenHR 68000 2022-05-03
+David Kim Engineerin 102000 2018-11-20

+Eva Chen HR 68000 2022-05-03
```
</details>

### classic127_font_styles

- **Text Similarity:** 0.9909
- **Visual Average:** 0.9923
- **Overall Score:** 0.9933
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1690 bytes, Reference=72555 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic127_font_styles.pdf
+++ reference/classic127_font_styles.pdf
@@ -2,8 +2,8 @@
 Bold Sample Bold text

 Italic Sample Italic text

 Underline Sample Underline text

-StrikethrouSample Strikethrough text

+Strikethro Sample Strikethrough text

 Bold Italic Sample Bold Italic text

-Bold UnderSample Bold Underline text

-Double UnSample Double Underline text

+Bold Unde Sample Bold Underline text

+Double Un Sample Double Underline text

 Bold + Red Sample Bold + Red text
```
</details>

### classic128_font_sizes

- **Text Similarity:** 0.982
- **Visual Average:** 0.9937
- **Overall Score:** 0.9903
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1807 bytes, Reference=48278 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic128_font_sizes.pdf
+++ reference/classic128_font_sizes.pdf
@@ -5,8 +5,7 @@
 Font size 9

 10 Font size 10

 11 Font size 11

-Font size 12

-12

+12 Font size 12

 Font size 14

 14

 Font size 16

```
</details>

### classic129_alignment_combos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9964
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1430 bytes, Reference=35431 bytes

Text content: ✅ Identical

### classic12_sparse_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.998
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=893 bytes, Reference=24923 bytes

Text content: ✅ Identical

### classic130_wrap_and_indent

- **Text Similarity:** 1.0
- **Visual Average:** 0.9868
- **Overall Score:** 0.9947
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1150 bytes, Reference=36937 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic130_wrap_and_indent.pdf
+++ reference/classic130_wrap_and_indent.pdf
@@ -1,5 +1,7 @@
 Wrapped Text Indented Text

-This is a long text that should wrap within the cell when text wrapping is enabled.

+This is a long text that should wrap

+within the cell when text wrapping

+is enabled.

 Indent level 0

 Indent level 1

 Indent level 2

```
</details>

### classic131_number_formats

- **Text Similarity:** 1.0
- **Visual Average:** 0.9917
- **Overall Score:** 0.9967
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2418 bytes, Reference=43396 bytes

Text content: ✅ Identical

### classic132_striped_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9776
- **Overall Score:** 0.991
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=16029 bytes, Reference=47692 bytes

Text content: ✅ Identical

### classic133_gradient_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.9924
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4221 bytes, Reference=54544 bytes

Text content: ✅ Identical

### classic134_heatmap

- **Text Similarity:** 1.0
- **Visual Average:** 0.9718
- **Overall Score:** 0.9887
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6089 bytes, Reference=44182 bytes

Text content: ✅ Identical

### classic135_bottom_border_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9954
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1341 bytes, Reference=32996 bytes

Text content: ✅ Identical

### classic136_financial_report_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9821
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8727 bytes, Reference=46675 bytes

Text content: ✅ Identical

### classic137_checkerboard

- **Text Similarity:** 1.0
- **Visual Average:** 0.9618
- **Overall Score:** 0.9847
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7604 bytes, Reference=42995 bytes

Text content: ✅ Identical

### classic138_color_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9861
- **Overall Score:** 0.9944
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1924 bytes, Reference=26461 bytes

Text content: ✅ Identical

### classic139_pattern_fills

- **Text Similarity:** 1.0
- **Visual Average:** 0.9833
- **Overall Score:** 0.9933
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2664 bytes, Reference=43091 bytes

Text content: ✅ Identical

### classic13_date_strings

- **Text Similarity:** 0.9738
- **Visual Average:** 0.9962
- **Overall Score:** 0.988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1224 bytes, Reference=29104 bytes

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

### classic140_rotated_text

- **Text Similarity:** 0.9583
- **Visual Average:** 0.994
- **Overall Score:** 0.9809
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1811 bytes, Reference=39253 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic140_rotated_text.pdf
+++ reference/classic140_rotated_text.pdf
@@ -1,12 +1,22 @@
 Rotation Text

 0 Rotated

-15 Rotated

-30 Rotated

-45 Rotated

-60 Rotated

-90 Rotated

-105 Rotated

-120 Rotated

-135 Rotated

-150 Rotated

-180 Rotated
+Rotated

+15

+Rotated

+30

+Rotated

+45

+Rotated

+60

+Rotated

+90

+Rotated

+105

+Rotated

+120

+Rotated

+135

+Rotated

+150

+Rotated

+180
```
</details>

### classic141_mixed_edge_borders

- **Text Similarity:** 1.0
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2261 bytes, Reference=36300 bytes

Text content: ✅ Identical

### classic142_styled_invoice

- **Text Similarity:** 1.0
- **Visual Average:** 0.9589
- **Overall Score:** 0.9836
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=10373 bytes, Reference=52625 bytes

Text content: ✅ Identical

### classic143_colored_tabs

- **Text Similarity:** 1.0
- **Visual Average:** 0.9986
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=2507 bytes, Reference=43510 bytes

Text content: ✅ Identical

### classic144_note_style_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.9896
- **Overall Score:** 0.9958
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2972 bytes, Reference=48027 bytes

Text content: ✅ Identical

### classic145_status_badges

- **Text Similarity:** 1.0
- **Visual Average:** 0.9795
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11663 bytes, Reference=60432 bytes

Text content: ✅ Identical

### classic146_double_border_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.99
- **Overall Score:** 0.996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7192 bytes, Reference=41798 bytes

Text content: ✅ Identical

### classic147_multi_sheet_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9931
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=16027 bytes, Reference=54487 bytes

Text content: ✅ Identical

### classic148_frozen_styled_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9359
- **Overall Score:** 0.9744
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45160 bytes, Reference=67067 bytes

Text content: ✅ Identical

### classic149_merged_styled_sections

- **Text Similarity:** 1.0
- **Visual Average:** 0.969
- **Overall Score:** 0.9876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11296 bytes, Reference=48481 bytes

Text content: ✅ Identical

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1217 bytes, Reference=29057 bytes

Text content: ✅ Identical

### classic150_kitchen_sink_styles

- **Text Similarity:** 0.9677
- **Visual Average:** 0.9696
- **Overall Score:** 0.9749
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3296 bytes, Reference=74184 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic150_kitchen_sink_styles.pdf
+++ reference/classic150_kitchen_sink_styles.pdf
@@ -1,7 +1,6 @@
 Style Showcase

 Feature Example Notes

-Styled Text

-Bold Italic Red Font combo

+Bold Italic Red Styled Text Font combo

 Double Underline Important Value Emphasis

 Strikethrough Deprecated Item Removed

 Dark Fill White on Dark Inverted

```
</details>

### classic15_negative_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9971
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1307 bytes, Reference=28526 bytes

Text content: ✅ Identical

### classic16_percentage_strings

- **Text Similarity:** 0.9938
- **Visual Average:** 0.997
- **Overall Score:** 0.9963
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1207 bytes, Reference=29888 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic16_percentage_strings.pdf
+++ reference/classic16_percentage_strings.pdf
@@ -1,5 +1,5 @@
 Metric Rate

-Conversio12.5%

+Conversion12.5%

 Bounce 45.3%

 Retention 88.7%

 Churn 3.2%

```
</details>

### classic17_currency_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1319 bytes, Reference=29862 bytes

Text content: ✅ Identical

### classic18_large_dataset

- **Text Similarity:** 1.0
- **Visual Average:** 0.8969
- **Overall Score:** 0.9588
- **Pages:** MiniPdf=24, Reference=24
- **File Size:** MiniPdf=561821 bytes, Reference=2487195 bytes

Text content: ✅ Identical

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9959
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1640 bytes, Reference=29688 bytes

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
- **Visual Average:** 0.9987
- **Overall Score:** 0.9995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=842 bytes, Reference=22034 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=884 bytes, Reference=23683 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.7917
- **Visual Average:** 0.9948
- **Overall Score:** 0.9146
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3126 bytes, Reference=67722 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic23_unicode_text.pdf
+++ reference/classic23_unicode_text.pdf
@@ -1,12 +1,9 @@
-LanguageGreeting Extra

+Language Greeting Extra

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

-����✅❌
+Korean 안녕하세요세계

+Arabicمرحبا العالم

+Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9963
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1084 bytes, Reference=39031 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.9951
- **Overall Score:** 0.998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1815 bytes, Reference=43116 bytes

Text content: ✅ Identical

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.997
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1061 bytes, Reference=25018 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=943 bytes, Reference=23681 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9968
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1595 bytes, Reference=24729 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1528 bytes, Reference=27548 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9986
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1422 bytes, Reference=27418 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9961
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1656 bytes, Reference=40714 bytes

Text content: ✅ Identical

### classic32_right_aligned_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9974
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1003 bytes, Reference=27582 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9983
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1356 bytes, Reference=26648 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 1.0
- **Visual Average:** 0.996
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1242 bytes, Reference=28834 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Text Similarity:** 1.0
- **Visual Average:** 0.9987
- **Overall Score:** 0.9995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=900 bytes, Reference=25108 bytes

Text content: ✅ Identical

### classic36_merged_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.9958
- **Overall Score:** 0.9983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1122 bytes, Reference=27256 bytes

Text content: ✅ Identical

### classic37_freeze_panes

- **Text Similarity:** 1.0
- **Visual Average:** 0.9899
- **Overall Score:** 0.996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5057 bytes, Reference=46420 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9968
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=922 bytes, Reference=26279 bytes

Text content: ✅ Identical

### classic39_financial_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9945
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2132 bytes, Reference=43383 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 1.0
- **Visual Average:** 0.996
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1234 bytes, Reference=30852 bytes

Text content: ✅ Identical

### classic41_integer_vs_float

- **Text Similarity:** 1.0
- **Visual Average:** 0.9969
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1513 bytes, Reference=29637 bytes

Text content: ✅ Identical

### classic42_boolean_values

- **Text Similarity:** 0.9895
- **Visual Average:** 0.9954
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1227 bytes, Reference=28631 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

 Dark Mode TRUE

-NotificationFALSE

+Notificatio FALSE

 Auto-save TRUE

 Analytics FALSE

 Beta Featu TRUE
```
</details>

### classic43_inventory_report

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9905
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3184 bytes, Reference=49849 bytes

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

- **Text Similarity:** 0.9967
- **Visual Average:** 0.9847
- **Overall Score:** 0.9926
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3671 bytes, Reference=43656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

 1001 Alice Smith EngineerinSenior Engalice@example.com

-1002 Bob Jones MarketingMarketing bob@example.com

+1002 Bob Jones MarketingMarketingbob@example.com

 1003 Carol Williams HR HR Specialcarol@example.com

-1004 David Brown EngineerinJunior Engidavid@example.com

+1004 David Brown EngineerinJunior Engdavid@example.com

 1005 Eve Davis Finance Financial Aeve@example.com

 1006 Frank Miller Sales Sales Reprfrank@example.com

-1007 Grace Wilson EngineerinTech Leadgrace@example.com

+1007 Grace Wilson EngineerinTech Lead grace@example.com

 1008 Henry Moore Support Support Sphenry@example.com
```
</details>

### classic45_sales_by_region

- **Text Similarity:** 1.0
- **Visual Average:** 0.9976
- **Overall Score:** 0.999
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3290 bytes, Reference=37087 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.9921
- **Overall Score:** 0.9968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3506 bytes, Reference=40993 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.9847
- **Overall Score:** 0.9939
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7403 bytes, Reference=55976 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9942
- **Visual Average:** 0.9932
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2491 bytes, Reference=36069 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
-Question StrongAgrAgree Neutral Disagree StrongDisagree

+Question StrongAgr Agree Neutral Disagree StrongDisagree

 Easy to us 30 45 15 7 3

-Recomme 25 40 20 10 5

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

 Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.9459
- **Visual Average:** 0.9892
- **Overall Score:** 0.974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2820 bytes, Reference=41523 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smith+1-555-01alice@exaNew York USA

-Bob Jones+44-20-79bob@exa London UK

-Carol Wan+86-10-12carol@exaBeijing China

-David Mull+49-30-12david@exBerlin Germany

-Eve Martin+33-1-23-4eve@exa Paris France

-Frank Tan+81-3-123frank@exaTokyo Japan

-Grace Kim+82-2-123grace@exSeoul Korea
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

- **Text Similarity:** 1.0
- **Visual Average:** 0.9907
- **Overall Score:** 0.9963
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=6876 bytes, Reference=54986 bytes

Text content: ✅ Identical

### classic51_product_catalog

- **Text Similarity:** 0.9782
- **Visual Average:** 0.987
- **Overall Score:** 0.9861
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3604 bytes, Reference=44297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
-Part# Name DescriptionWeight(g) Price

-P-001 Basic WidgStandard 150 4.99

-P-002 Pro WidgetEnhanced 180 12.99

-P-003 Mini GadgCompact g 90 19.99

-P-004 Max GadgFull-size g 450 89.99

-P-005 Connector Type-A co 80 7.49

-P-006 Connector Type-B co 110 9.99

-P-007 Adapter XUniversal p 200 15.99

-P-008 Adapter YTravel pow 120 11.99

-P-009 Mount BraWall mount 600 24.99

-P-010 Carry CasPadded ca 350 34.99
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

- **Text Similarity:** 1.0
- **Visual Average:** 0.9906
- **Overall Score:** 0.9962
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2531 bytes, Reference=44493 bytes

Text content: ✅ Identical

### classic53_invoice

- **Text Similarity:** 0.9951
- **Visual Average:** 0.9914
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2685 bytes, Reference=53425 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -1,14 +1,14 @@
 INVOICE

 Invoice #: INV-2025-0042

 Date: 2025-03-01

-Due Date:2025-03-31

+Due Date: 2025-03-31

 Bill To:

 ACME Corporation

 123 Business Rd, Suite 400

 New York, NY 10001

-Item Qty Unit Price Total

+Item Qty Unit PriceTotal

 Consulting 10 150 1500

-Software Li 5 99 495

+Software L 5 99 495

 Hardware 2 249.99 499.98

 Support Pl 1 1200 1200

 Subtotal 3694.98

```
</details>

### classic54_multi_level_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.9947
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2259 bytes, Reference=38782 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9944
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1708 bytes, Reference=34677 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3196 bytes, Reference=32363 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Text Similarity:** 0.7826
- **Visual Average:** 0.9144
- **Overall Score:** 0.8788
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3243 bytes, Reference=88207 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic57_cjk_only.pdf
+++ reference/classic57_cjk_only.pdf
@@ -1,11 +1,11 @@
 序号 产品名称价格 库存

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

- **Text Similarity:** 0.9937
- **Visual Average:** 0.9953
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1666 bytes, Reference=32815 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic58_mixed_numeric_formats.pdf
+++ reference/classic58_mixed_numeric_formats.pdf
@@ -5,6 +5,6 @@
 Negative in -42

 Negative fl -3.14

 Very small 0.0001

-Very large10000000

+Very large 10000000

 Zero 0

-Scientific a1.23E+10
+Scientific 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9964
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=4545 bytes, Reference=44781 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9368
- **Overall Score:** 0.9747
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=58015 bytes, Reference=263350 bytes

Text content: ✅ Identical

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9971
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2075 bytes, Reference=36974 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2816 bytes, Reference=42880 bytes

Text content: ✅ Identical

### classic63_two_products_side_by_side

- **Text Similarity:** 1.0
- **Visual Average:** 0.9956
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3085 bytes, Reference=28933 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.9967
- **Visual Average:** 0.9908
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4726 bytes, Reference=43408 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title DepartmenEmail

+Photo Name Title DepartmeEmail

 Alice ChenEngineer R&D alice@example.com

 Bob SmithManager Sales bob@example.com

 Carol WanDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9906
- **Visual Average:** 0.9911
- **Overall Score:** 0.9927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6659 bytes, Reference=48227 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic65_inventory_with_product_photos.pdf
+++ reference/classic65_inventory_with_product_photos.pdf
@@ -1,6 +1,6 @@
 Image SKU Name Qty Price

 SKU-001 Red Widge 50 9.99

-SKU-002 Blue Gadg 30 14.99

-SKU-003 Green Tool 100 4.49

+SKU-002 Blue Gadge 30 14.99

+SKU-003 Green Too 100 4.49

 SKU-004 Yellow Dev 25 29.99

-SKU-005 Purple Ge 75 7.99
+SKU-005 Purple Gea 75 7.99
```
</details>

### classic66_invoice_with_logo

- **Text Similarity:** 0.9868
- **Visual Average:** 0.9951
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2778 bytes, Reference=45034 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -1,8 +1,8 @@
 INVOICE

 Invoice #: INV-20250301

 Date: 2025-03-01

-DescriptionQty Unit Price Total

+DescriptiQty Unit PriceTotal

 Consulting 8 150 1200

-Software Li 1 299 299

+Software L 1 299 299

 Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Text Similarity:** 1.0
- **Visual Average:** 0.9928
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2828 bytes, Reference=44030 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Text Similarity:** 0.9784
- **Visual Average:** 0.9873
- **Overall Score:** 0.9863
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5305 bytes, Reference=47320 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,9 +1,9 @@
 Today's Menu

-Grilled Sal$18.99

+Grilled S $18.99

 Fresh Atlantic salmon with herbs

-Caesar Sal$12.99

+Caesar Sa $12.99

 Romaine lettuce, croutons, parmesan

-Beef Burge$14.99

+Beef Burg $14.99

 8oz Angus beef, brioche bun

-Pasta Prim$13.99

+Pasta Pri $13.99

 Seasonal vegetables, olive oil
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

- **Text Similarity:** 0.9826
- **Visual Average:** 0.989
- **Overall Score:** 0.9886
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4439 bytes, Reference=44156 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

-Classic Pe$3.99

+Classic P $3.99

 A reliable ballpoint pen

-Leather No$12.99

+Leather $12.99

 Premium A5 notebook

-Desk Orga$24.99

+Desk Orga $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9981
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=5076 bytes, Reference=37419 bytes

Text content: ✅ Identical

### classic72_bar_chart_image_with_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9913
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3207 bytes, Reference=41342 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9446
- **Visual Average:** 0.9919
- **Overall Score:** 0.9746
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3175 bytes, Reference=44512 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -4,6 +4,6 @@
 Speakers: 20+ Industry Leaders

 Time Session Speaker

 09:00 Opening KDr. Jane Kim

-10:30 AI in PractiProf. Mark Liu

+10:30 AI in Pract Prof. Mark Liu

 13:00 Cloud ArchEng. Sara Patel

-15:00 Panel DiscAll Speakers
+15:00 Panel Disc All Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.9625
- **Visual Average:** 0.9903
- **Overall Score:** 0.9811
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4259 bytes, Reference=48755 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -1,10 +1,6 @@
 Executive Dashboard Q1 2025

 KPI Target Actual Status

-Revenue 500000 523000

-✓ Above

-New Custo 200 187

-✗ Below

-NPS Score 70 74

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
- **Visual Average:** 0.9891
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1991 bytes, Reference=39135 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9919
- **Overall Score:** 0.9968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5052 bytes, Reference=39017 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.989
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2737 bytes, Reference=52664 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9898
- **Visual Average:** 0.9941
- **Overall Score:** 0.9936
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6118 bytes, Reference=41646 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login bAlice Done

-Write unit tBob In Progress

-Deploy to sCarol Pending

+Fix login b Alice Done

+Write unit Bob In Progress

+Deploy to Carol Pending

 Code revieAlice Done

 Update doDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9934
- **Overall Score:** 0.9974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2976 bytes, Reference=43015 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9921
- **Overall Score:** 0.9968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2367 bytes, Reference=39079 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9889
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5152 bytes, Reference=47175 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9925
- **Visual Average:** 0.9929
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3879 bytes, Reference=42486 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -1,5 +1,5 @@
 Before After

 Old design – legacy UI New design – modern UI

 Metric Before After Delta

-Load time4.2s 1.1s -74%

-Conversio2.1% 4.8% +129%
+Load time 4.2s 1.1s -74%

+Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Text Similarity:** 0.9916
- **Visual Average:** 0.9901
- **Overall Score:** 0.9927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6992 bytes, Reference=45933 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

 Primary BlRGB(0, 82, 165)

 Primary ReRGB(197, 27, 50)

-Accent GreRGB(0, 163, 108)

+Accent Gr RGB(0, 163, 108)

 Neutral GrRGB(128, 128, 128)

 Warm YellRGB(255, 193, 7)

-Dark NavyRGB(10, 30, 70)
+Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Text Similarity:** 1.0
- **Visual Average:** 0.9864
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4407 bytes, Reference=42524 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.9933
- **Visual Average:** 0.9913
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3450 bytes, Reference=47866 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,7 +1,7 @@
 Sample Analysis Report

-ParameterValue Unit Reference Flag

+ParameteValue Unit ReferenceFlag

 pH 7.35 7.35 – 7.45Normal

 Glucose 5.2 mmol/L 3.9 – 5.5 Normal

-Sodium 142 mEq/L 136 – 145Normal

+Sodium 142 mEq/L 136 – 145 Normal

 Potassium 5 mEq/L 3.5 – 5.0 Normal

 Creatinine 1.4 mg/dL 0.6 – 1.2 High
```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.9862
- **Visual Average:** 0.993
- **Overall Score:** 0.9917
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2877 bytes, Reference=41961 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic86_software_screenshot_features.pdf
+++ reference/classic86_software_screenshot_features.pdf
@@ -2,8 +2,8 @@
 The fastest lightweight app

 Feature Available

 Dark ModeYes

-Auto SaveYes

-Cloud SynYes

+Auto Save Yes

+Cloud SyncYes

 Offline MoYes

-API AccesPro only

-Export to PYes
+API AccessPro only

+Export to Yes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9928
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5745 bytes, Reference=47076 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2953 bytes, Reference=43273 bytes

Text content: ✅ Identical

### classic89_nutrition_label_with_image

- **Text Similarity:** 0.9976
- **Visual Average:** 0.9936
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3223 bytes, Reference=47194 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,6 +1,6 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount pe% Daily Value

+Nutrient Amount p% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

 Saturated 0.5g 3%

```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.9585
- **Visual Average:** 0.9889
- **Overall Score:** 0.979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3322 bytes, Reference=47112 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
 Project Orion – Status Report

 Reporting Period: Q1 2025

-Milestone Due Date Owner Status

+MilestoneDue DateOwner Status

 RequiremeJan 15 PM Team Complete

-ArchitecturFeb 1 Tech LeadComplete

-Alpha ReleFeb 28 Dev TeamIn Progress

-Beta TestiMar 31 QA Team Not Started

+ArchitectuFeb 1 Tech Lead Complete

+Alpha ReleFeb 28 Dev Team In Progress

+Beta Testi Mar 31 QA Team Not Started

 ProductionApr 15 DevOps Not Started
```
</details>

### classic91_simple_bar_chart

- **Text Similarity:** 0.9493
- **Visual Average:** 0.9626
- **Overall Score:** 0.9648
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3083 bytes, Reference=46981 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic91_simple_bar_chart.pdf
+++ reference/classic91_simple_bar_chart.pdf
@@ -1,7 +1,9 @@
 Product Revenue

-Widget A 12000 Product Revenue

+Widget A 12000

+Product Revenue

 Widget B 18500

-Widget C 9200 25000

+Widget C 9200

+25000

 Widget D 22000

 Widget E 15600

 20000

@@ -10,6 +12,8 @@
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

- **Text Similarity:** 0.9706
- **Visual Average:** 0.9665
- **Overall Score:** 0.9748
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3767 bytes, Reference=49903 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic92_horizontal_bar_chart.pdf
+++ reference/classic92_horizontal_bar_chart.pdf
@@ -1,5 +1,6 @@
 DepartmenHeadcount

-Engineerin 45 Headcount by Department

+Engineerin 45

+Headcount by Department

 Sales 30

 Marketing 18

 HR 12

@@ -11,5 +12,7 @@
 Marketing

 Sales

 Engineering

-0 5 10 15 20 25 30 35 40 45 50

----PAGE---
+0 5 10 15 20 25 30 35 40 45

+---PAGE---

+Headcount

+50
```
</details>

### classic93_line_chart

- **Text Similarity:** 0.8257
- **Visual Average:** 0.9861
- **Overall Score:** 0.9247
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5273 bytes, Reference=58815 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic93_line_chart.pdf
+++ reference/classic93_line_chart.pdf
@@ -1,21 +1,26 @@
 Month Avg Temp (C)

-Jan 3 Monthly Average Temperature

+Jan 3

+Monthly Average Temperatur

 Feb 5

-Mar 10 30

+Mar 10

+30

 Apr 15

 May 20

+Jun 25

 25

-Jun 25

 Jul 28

+Aug 27

 20

-Aug 27

 Sep 22

-Oct 15 15

-Nov 8

 Temperature (C)

+Oct 15

+Nov 8 15

 Dec 4

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

- **Text Similarity:** 0.878
- **Visual Average:** 0.926
- **Overall Score:** 0.9216
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403592 bytes, Reference=47211 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic94_pie_chart.pdf
+++ reference/classic94_pie_chart.pdf
@@ -1,7 +1,13 @@
 Segment Share (%)

-Enterprise 35 Market Share by Segment

+Enterprise 35

+Market Share by Segment

 SMB 28

 Consumer 22

 Governme 10

 Education 5

+Enterprise

+SMB

+Consumer

+Government

+Education

 ---PAGE---
```
</details>

### classic95_area_chart

- **Text Similarity:** 0.6441
- **Visual Average:** 0.765
- **Overall Score:** 0.7636
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=12751 bytes, Reference=60817 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic95_area_chart.pdf
+++ reference/classic95_area_chart.pdf
@@ -1,33 +1,39 @@
 Hour Users

-00:00 214 Website Traffic by Hour

+00:00 214

+Website Traffic by Hour

 01:00 216

-02:00 218 1200

+02:00 218

+1200

 03:00 221

 04:00 224

+05:00 228

 1000

-05:00 228

 06:00 233

+07:00 240

 800

-07:00 240

 08:00 250

-09:00 265 600

+09:00 265

+600

+Users

 10:00 288

-Users

 11:00 329

 400

 12:00 408

 13:00 600

+14:00 1000

 200

-14:00 1000

 15:00 600

+16:00 408

 0

-16:00 408

-00:001:002:003:004:005:006:007:008:009:0010:0011:0012:0013:0014:0015:0016:0017:0018:0019:0020:0021:002

-17:00 329

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

- **Text Similarity:** 0.8921
- **Visual Average:** 0.9855
- **Overall Score:** 0.951
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6330 bytes, Reference=62711 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic96_scatter_chart.pdf
+++ reference/classic96_scatter_chart.pdf
@@ -1,27 +1,34 @@
 Ad Spend (Sales ($K)

-45 96 Ad Spend vs Sales

+45 96

+Ad Spend vs Sales

 6 11

-20 43 140

+20 43

+140

 13 22

-48 117 120

+48 117

+120

 10 31

-32 64 100

+32 64

+100

 6 5

-18 38 80

+18 38

+80

+Sales ($K)

 37 94

-6 20 60

-Sales ($K)

+60

+6 20

 17 49

-49 119 40

+40

+49 119

 31 68

 20

 33 83

 22 40

 0

+0 10 20 30 40 50 60

 15 37

-0 10 20 30 40 50 60

-26 57

-Ad Spend ($K)

+26 57 Ad Spend ($K)

 14 28

 26 52

----PAGE---
+---PAGE---

+Data Points
```
</details>

### classic97_doughnut_chart

- **Text Similarity:** 0.8571
- **Visual Average:** 0.9319
- **Overall Score:** 0.9156
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=302225 bytes, Reference=47227 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic97_doughnut_chart.pdf
+++ reference/classic97_doughnut_chart.pdf
@@ -1,7 +1,13 @@
 Category Amount

-Salaries 50000 Budget Allocation

+Salaries 50000

+Budget Allocation

 Rent 12000

 Marketing 8000

 R&D 15000

 Other 5000

+Salaries

+Rent

+Marketing

+R&D

+Other

 ---PAGE---
```
</details>

### classic98_radar_chart

- **Text Similarity:** 0.8876
- **Visual Average:** 0.9894
- **Overall Score:** 0.9508
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4053 bytes, Reference=47620 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic98_radar_chart.pdf
+++ reference/classic98_radar_chart.pdf
@@ -1,22 +1,16 @@
 Skill Score

-Python 9 Developer Skill Radar

+Python 9

+Developer Skill Radar

 SQL 8

-Python

 Communic 7

 Leadership 6

+Python

+Design 5

 10

-9

-Design 5

-8

-7

 DevOps 7

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

- **Text Similarity:** 0.8447
- **Visual Average:** 0.9682
- **Overall Score:** 0.9252
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4295 bytes, Reference=57405 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic99_bubble_chart.pdf
+++ reference/classic99_bubble_chart.pdf
@@ -1,17 +1,25 @@
 Price ($) Rating Units Sold

-10 4.2 500 Product Comparison

+10 4.2 500

+Product Comparison

 25 4.5 300

-50 3.8 150 6

+50 3.8 150

+5

 15 4 420

 35 4.7 200

-5

+4.5

 8 3.5 600

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

1. **classic09_long_text** (score: 0.6022)
1. **classic120_chart_with_date_axis** (score: 0.7427)
1. **classic95_area_chart** (score: 0.7636)

Review the text diffs and visual comparisons above to identify specific rendering issues.
