# Rust MiniPdf vs Microsoft 365 Excel Reference PDF Comparison Report

Generated: 2026-08-23T22:18:55.322602

## Summary

| # | Test Case | Valid | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|-------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | ✅ | 1.0 | 0.9951 | 1/1 | **0.998** |
| 2 | 🟢 classic02_multiple_worksheets | ✅ | 1.0 | 0.9968 | 3/3 | **0.9987** |
| 3 | 🟢 classic03_empty_workbook | ✅ | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | ✅ | 1.0 | 0.9996 | 1/1 | **0.9998** |
| 5 | 🟢 classic05_wide_table | ✅ | 0.9474 | 0.9911 | 3/3 | **0.9754** |
| 6 | 🟢 classic06_tall_table | ✅ | 0.9558 | 0.9163 | 5/5 | **0.9488** |
| 7 | 🟢 classic07_numbers_only | ✅ | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 8 | 🟢 classic08_mixed_text_and_numbers | ✅ | 0.9926 | 0.9961 | 1/1 | **0.9955** |
| 9 | 🟡 classic09_long_text | ✅ | 0.899 | 0.9731 | 1/12 | **0.8488** |
| 10 | 🟢 classic10_special_xml_characters | ✅ | 0.9909 | 0.9942 | 1/1 | **0.994** |
| 11 | 🟢 classic11_sparse_rows | ✅ | 1.0 | 0.9985 | 2/2 | **0.9994** |
| 12 | 🟡 classic12_sparse_columns | ✅ | 0.9091 | 0.9966 | 1/2 | **0.8623** |
| 13 | 🟢 classic13_date_strings | ✅ | 1.0 | 0.9938 | 1/1 | **0.9975** |
| 14 | 🟢 classic14_decimal_numbers | ✅ | 1.0 | 0.9947 | 1/1 | **0.9979** |
| 15 | 🟢 classic15_negative_numbers | ✅ | 0.8249 | 0.996 | 1/1 | **0.9284** |
| 16 | 🟢 classic16_percentage_strings | ✅ | 1.0 | 0.9945 | 1/1 | **0.9978** |
| 17 | 🟢 classic17_currency_strings | ✅ | 0.9808 | 0.9935 | 1/1 | **0.9897** |
| 18 | 🟡 classic18_large_dataset | ✅ | 0.9083 | 0.8775 | 23/42 | **0.8143** |
| 19 | 🟢 classic19_single_column_list | ✅ | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 20 | 🟢 classic20_all_empty_cells | ✅ | 1.0 | 1.0 | 1/1 | **1.0** |
| 21 | 🟢 classic21_header_only | ✅ | 1.0 | 0.9986 | 1/1 | **0.9994** |
| 22 | 🟢 classic22_long_sheet_name | ✅ | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 23 | 🟢 classic23_unicode_text | ✅ | 0.961 | 0.991 | 1/1 | **0.9808** |
| 24 | 🟢 classic24_red_text | ✅ | 1.0 | 0.9937 | 1/1 | **0.9975** |
| 25 | 🟢 classic25_multiple_colors | ✅ | 0.9955 | 0.9917 | 1/1 | **0.9949** |
| 26 | 🟢 classic26_inline_strings | ✅ | 1.0 | 0.9967 | 1/1 | **0.9987** |
| 27 | 🟢 classic27_single_row | ✅ | 1.0 | 0.9979 | 1/1 | **0.9992** |
| 28 | 🟢 classic28_duplicate_values | ✅ | 1.0 | 0.9957 | 1/1 | **0.9983** |
| 29 | 🟢 classic29_formula_results | ✅ | 1.0 | 0.994 | 1/1 | **0.9976** |
| 30 | 🔴 classic30_mixed_empty_and_filled_sheets | ✅ | 0.0 | 0.9946 | 4/2 | **0.4978** |
| 31 | 🟢 classic31_bold_header_row | ✅ | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 32 | 🟢 classic32_right_aligned_numbers | ✅ | 1.0 | 0.9961 | 1/1 | **0.9984** |
| 33 | 🟢 classic33_centered_text | ✅ | 1.0 | 0.9974 | 1/1 | **0.999** |
| 34 | 🟢 classic34_explicit_column_widths | ✅ | 1.0 | 0.9923 | 1/1 | **0.9969** |
| 35 | 🟢 classic35_explicit_row_heights | ✅ | 0.9462 | 0.9979 | 1/1 | **0.9776** |
| 36 | 🟢 classic36_merged_cells | ✅ | 0.9643 | 0.9942 | 1/1 | **0.9834** |
| 37 | 🟢 classic37_freeze_panes | ✅ | 1.0 | 0.9838 | 1/1 | **0.9935** |
| 38 | 🟢 classic38_hyperlink_cell | ✅ | 1.0 | 0.9963 | 1/1 | **0.9985** |
| 39 | 🟢 classic39_financial_table | ✅ | 0.99 | 0.9896 | 1/1 | **0.9918** |
| 40 | 🟡 classic40_scientific_notation | ✅ | 0.7203 | 0.9929 | 1/1 | **0.8853** |
| 41 | 🟢 classic41_integer_vs_float | ✅ | 0.936 | 0.9949 | 1/1 | **0.9724** |
| 42 | 🟢 classic42_boolean_values | ✅ | 0.8136 | 0.9928 | 1/1 | **0.9226** |
| 43 | 🟢 classic43_inventory_report | ✅ | 1.0 | 0.9809 | 1/1 | **0.9924** |
| 44 | 🟢 classic44_employee_roster | ✅ | 0.7759 | 0.9748 | 1/1 | **0.9003** |
| 45 | 🟢 classic45_sales_by_region | ✅ | 1.0 | 0.9962 | 4/4 | **0.9985** |
| 46 | 🟢 classic46_grade_book | ✅ | 1.0 | 0.9875 | 1/1 | **0.995** |
| 47 | 🟢 classic47_time_series | ✅ | 1.0 | 0.9708 | 1/1 | **0.9883** |
| 48 | 🟢 classic48_survey_results | ✅ | 0.9859 | 0.9896 | 1/1 | **0.9902** |
| 49 | 🟡 classic49_contact_list | ✅ | 0.6491 | 0.9829 | 1/1 | **0.8528** |
| 50 | 🟢 classic50_budget_vs_actuals | ✅ | 0.9956 | 0.9819 | 3/3 | **0.991** |
| 51 | 🟡 classic51_product_catalog | ✅ | 0.6242 | 0.977 | 1/1 | **0.8405** |
| 52 | 🟢 classic52_pivot_summary | ✅ | 1.0 | 0.9825 | 1/1 | **0.993** |
| 53 | 🟢 classic53_invoice | ✅ | 0.9346 | 0.985 | 1/1 | **0.9678** |
| 54 | 🟢 classic54_multi_level_header | ✅ | 1.0 | 0.9854 | 1/1 | **0.9942** |
| 55 | 🟢 classic55_error_values | ✅ | 0.9864 | 0.9912 | 1/1 | **0.991** |
| 56 | 🟢 classic56_alternating_row_colors | ✅ | 1.0 | 0.9734 | 1/1 | **0.9894** |
| 57 | 🟢 classic57_cjk_only | ✅ | 1.0 | 0.9891 | 1/1 | **0.9956** |
| 58 | 🟢 classic58_mixed_numeric_formats | ✅ | 0.881 | 0.9919 | 1/1 | **0.9492** |
| 59 | 🟢 classic59_multi_sheet_summary | ✅ | 1.0 | 0.9927 | 4/4 | **0.9971** |
| 60 | 🟡 classic60_large_wide_table | ✅ | 0.9083 | 0.8587 | 4/6 | **0.8068** |
| 61 | 🟢 classic61_product_card_with_image | ✅ | 0.9908 | 0.9906 | 1/1 | **0.9926** |
| 62 | 🟢 classic62_company_logo_header | ✅ | 0.992 | 0.9895 | 1/1 | **0.9926** |
| 63 | 🟢 classic63_two_products_side_by_side | ✅ | 1.0 | 0.9846 | 1/1 | **0.9938** |
| 64 | 🟢 classic64_employee_directory_with_photo | ✅ | 0.9902 | 0.9823 | 1/1 | **0.989** |
| 65 | 🟢 classic65_inventory_with_product_photos | ✅ | 0.9786 | 0.9844 | 1/1 | **0.9852** |
| 66 | 🟢 classic66_invoice_with_logo | ✅ | 0.9415 | 0.9857 | 1/1 | **0.9709** |
| 67 | 🟢 classic67_real_estate_listing | ✅ | 1.0 | 0.9837 | 1/1 | **0.9935** |
| 68 | 🟡 classic68_restaurant_menu | ✅ | 0.7901 | 0.7471 | 1/1 | **0.8149** |
| 69 | 🟢 classic69_image_only_sheet | ✅ | 1.0 | 0.9808 | 1/1 | **0.9923** |
| 70 | 🟢 classic70_product_catalog_with_images | ✅ | 0.9582 | 0.9683 | 1/1 | **0.9706** |
| 71 | 🟢 classic71_multi_sheet_with_images | ✅ | 1.0 | 0.9942 | 3/3 | **0.9977** |
| 72 | 🟡 classic72_bar_chart_image_with_data | ✅ | 1.0 | 0.6405 | 1/1 | **0.8562** |
| 73 | 🟢 classic73_event_flyer_with_banner | ✅ | 0.9087 | 0.9736 | 1/1 | **0.9529** |
| 74 | 🟢 classic74_dashboard_with_kpi_image | ✅ | 0.9846 | 0.78 | 1/1 | **0.9058** |
| 75 | 🟢 classic75_certificate_with_seal | ✅ | 1.0 | 0.9714 | 1/1 | **0.9886** |
| 76 | 🟢 classic76_product_image_grid | ✅ | 0.98 | 0.9729 | 1/1 | **0.9812** |
| 77 | 🟢 classic77_news_article_with_hero_image | ✅ | 1.0 | 0.9676 | 1/1 | **0.987** |
| 78 | 🟢 classic78_small_icon_per_row | ✅ | 0.9283 | 0.9864 | 1/1 | **0.9659** |
| 79 | 🟢 classic79_wide_panoramic_banner | ✅ | 0.9939 | 0.9695 | 1/1 | **0.9854** |
| 80 | 🟢 classic80_portrait_tall_image | ✅ | 1.0 | 0.9856 | 1/1 | **0.9942** |
| 81 | 🟢 classic81_step_by_step_with_images | ✅ | 1.0 | 0.971 | 1/1 | **0.9884** |
| 82 | 🟢 classic82_before_after_images | ✅ | 0.9926 | 0.9668 | 1/1 | **0.9838** |
| 83 | 🟢 classic83_color_swatch_palette | ✅ | 0.9734 | 0.9801 | 1/1 | **0.9814** |
| 84 | 🟢 classic84_travel_destination_cards | ✅ | 1.0 | 0.9663 | 1/1 | **0.9865** |
| 85 | 🟢 classic85_lab_results_with_image | ✅ | 0.9846 | 0.8941 | 1/1 | **0.9515** |
| 86 | 🟢 classic86_software_screenshot_features | ✅ | 0.9801 | 0.9848 | 1/1 | **0.986** |
| 87 | 🟢 classic87_sports_results_with_logos | ✅ | 1.0 | 0.9895 | 1/1 | **0.9958** |
| 88 | 🟢 classic88_image_after_data | ✅ | 1.0 | 0.9728 | 1/1 | **0.9891** |
| 89 | 🟢 classic89_nutrition_label_with_image | ✅ | 0.9452 | 0.9867 | 1/1 | **0.9728** |
| 90 | 🟡 classic90_project_status_with_milestones | ✅ | 0.7957 | 0.8925 | 1/1 | **0.8753** |
| 91 | 🔴 classic91_simple_bar_chart | ✅ | 0.8718 | 0.6001 | 1/2 | **0.6888** |
| 92 | 🔴 classic92_horizontal_bar_chart | ✅ | 0.8673 | 0.5889 | 1/2 | **0.6825** |
| 93 | 🟡 classic93_line_chart | ✅ | 0.918 | 0.7269 | 1/2 | **0.758** |
| 94 | 🔴 classic94_pie_chart | ✅ | 0.7291 | 0.4435 | 1/2 | **0.569** |
| 95 | 🟡 classic95_area_chart | ✅ | 0.9524 | 0.6462 | 1/2 | **0.7394** |
| 96 | 🟡 classic96_scatter_chart | ✅ | 0.8889 | 0.6805 | 1/2 | **0.7278** |
| 97 | 🔴 classic97_doughnut_chart | ✅ | 0.8021 | 0.4485 | 1/2 | **0.6002** |
| 98 | 🔴 classic98_radar_chart | ✅ | 0.7027 | 0.7005 | 1/2 | **0.6613** |
| 99 | 🟡 classic99_bubble_chart | ✅ | 0.902 | 0.6355 | 1/2 | **0.715** |
| 100 | 🟡 classic100_stacked_bar_chart | ✅ | 0.8621 | 0.6006 | 1/1 | **0.7851** |
| 101 | 🟡 classic101_percent_stacked_bar | ✅ | 0.8696 | 0.5995 | 1/1 | **0.7876** |
| 102 | 🟡 classic102_line_chart_with_markers | ✅ | 0.92 | 0.7667 | 1/2 | **0.7747** |
| 103 | 🔴 classic103_pie_chart_with_labels | ✅ | 0.4727 | 0.4832 | 1/2 | **0.4824** |
| 104 | 🟡 classic104_combo_bar_line_chart | ✅ | 0.9333 | 0.5971 | 1/2 | **0.7122** |
| 105 | 🔴 classic105_3d_bar_chart | ✅ | 0.8824 | 0.5396 | 1/2 | **0.6688** |
| 106 | 🔴 classic106_3d_pie_chart | ✅ | 0.7928 | 0.54 | 1/2 | **0.6331** |
| 107 | 🟡 classic107_multi_series_line | ✅ | 0.9858 | 0.947 | 1/2 | **0.8731** |
| 108 | 🔴 classic108_stacked_area_chart | ✅ | 0.8974 | 0.4355 | 1/2 | **0.6332** |
| 109 | 🔴 classic109_scatter_with_trendline | ✅ | 0.7901 | 0.6602 | 1/2 | **0.6801** |
| 110 | 🔴 classic110_chart_with_legend | ✅ | 0.7843 | 0.5891 | 1/2 | **0.6494** |
| 111 | 🔴 classic111_chart_with_axis_labels | ✅ | 0.7895 | 0.6346 | 1/2 | **0.6696** |
| 112 | 🟡 classic112_multiple_charts | ✅ | 0.9492 | 0.6165 | 1/2 | **0.7263** |
| 113 | 🔴 classic113_chart_sheet | ✅ | 0.9091 | 0.5355 | 1/2 | **0.6778** |
| 114 | 🟡 classic114_chart_large_dataset | ✅ | 0.8817 | 0.954 | 3/4 | **0.8343** |
| 115 | 🔴 classic115_chart_negative_values | ✅ | 0.7978 | 0.6198 | 1/2 | **0.667** |
| 116 | 🔴 classic116_percent_stacked_area | ✅ | 0.9091 | 0.3877 | 1/2 | **0.6187** |
| 117 | 🟡 classic117_stock_ohlc_chart | ✅ | 0.9864 | 0.7859 | 1/2 | **0.8089** |
| 118 | 🔴 classic118_bar_chart_custom_colors | ✅ | 0.8966 | 0.5774 | 1/2 | **0.6896** |
| 119 | 🔴 classic119_dashboard_multi_charts | ✅ | 0.8475 | 0.5026 | 1/2 | **0.64** |
| 120 | 🟡 classic120_chart_with_date_axis | ✅ | 0.9123 | 0.7928 | 1/2 | **0.782** |
| 121 | 🟢 classic121_thin_borders | ✅ | 1.0 | 0.9763 | 1/1 | **0.9905** |
| 122 | 🟢 classic122_thick_outer_thin_inner | ✅ | 1.0 | 0.9689 | 1/1 | **0.9876** |
| 123 | 🟢 classic123_dashed_borders | ✅ | 0.9653 | 0.99 | 1/1 | **0.9821** |
| 124 | 🟢 classic124_colored_borders | ✅ | 1.0 | 0.9827 | 1/1 | **0.9931** |
| 125 | 🟢 classic125_solid_fills | ✅ | 0.9897 | 0.982 | 1/1 | **0.9887** |
| 126 | 🟢 classic126_dark_header | ✅ | 0.993 | 0.9849 | 1/1 | **0.9912** |
| 127 | 🟢 classic127_font_styles | ✅ | 0.9195 | 0.9843 | 1/1 | **0.9615** |
| 128 | 🟢 classic128_font_sizes | ✅ | 1.0 | 0.9905 | 1/1 | **0.9962** |
| 129 | 🟢 classic129_alignment_combos | ✅ | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 130 | 🟢 classic130_wrap_and_indent | ✅ | 1.0 | 0.9796 | 1/1 | **0.9918** |
| 131 | 🟡 classic131_number_formats | ✅ | 0.5 | 0.9834 | 1/1 | **0.7934** |
| 132 | 🟢 classic132_striped_table | ✅ | 1.0 | 0.9553 | 1/1 | **0.9821** |
| 133 | 🟢 classic133_gradient_rows | ✅ | 1.0 | 0.9678 | 1/1 | **0.9871** |
| 134 | 🟢 classic134_heatmap | ✅ | 1.0 | 0.9399 | 1/1 | **0.976** |
| 135 | 🟢 classic135_bottom_border_only | ✅ | 1.0 | 0.9894 | 1/1 | **0.9958** |
| 136 | 🟡 classic136_financial_report_styled | ✅ | 0.5932 | 0.9511 | 1/1 | **0.8177** |
| 137 | 🟢 classic137_checkerboard | ✅ | 1.0 | 0.9565 | 1/1 | **0.9826** |
| 138 | 🟢 classic138_color_grid | ✅ | 0.9406 | 0.972 | 1/1 | **0.965** |
| 139 | 🟢 classic139_pattern_fills | ✅ | 1.0 | 0.8246 | 1/1 | **0.9298** |
| 140 | 🟢 classic140_rotated_text | ✅ | 0.9583 | 0.9903 | 1/1 | **0.9794** |
| 141 | 🟢 classic141_mixed_edge_borders | ✅ | 1.0 | 0.9844 | 1/1 | **0.9938** |
| 142 | 🟢 classic142_styled_invoice | ✅ | 0.8339 | 0.9313 | 1/1 | **0.9061** |
| 143 | 🟢 classic143_colored_tabs | ✅ | 1.0 | 0.9979 | 4/4 | **0.9992** |
| 144 | 🟢 classic144_note_style_cells | ✅ | 1.0 | 0.9684 | 1/1 | **0.9874** |
| 145 | 🟢 classic145_status_badges | ✅ | 1.0 | 0.9515 | 1/1 | **0.9806** |
| 146 | 🟢 classic146_double_border_table | ✅ | 1.0 | 0.9688 | 1/1 | **0.9875** |
| 147 | 🟢 classic147_multi_sheet_styled | ✅ | 0.9834 | 0.9844 | 3/3 | **0.9871** |
| 148 | 🟢 classic148_frozen_styled_grid | ✅ | 0.9928 | 0.8622 | 1/1 | **0.942** |
| 149 | 🟢 classic149_merged_styled_sections | ✅ | 0.9324 | 0.9353 | 1/1 | **0.9471** |
| 150 | 🟢 classic150_kitchen_sink_styles | ✅ | 0.9916 | 0.9268 | 1/1 | **0.9674** |
| 151 | 🟢 classic151_multilingual_greetings | ✅ | 0.9761 | 0.9833 | 1/1 | **0.9838** |
| 152 | 🟢 classic152_emoji_sampler | ✅ | 0.9677 | 0.9852 | 1/1 | **0.9812** |
| 153 | 🟢 classic153_currency_symbols | ✅ | 0.9967 | 0.9854 | 1/1 | **0.9928** |
| 154 | 🟢 classic154_math_symbols | ✅ | 1.0 | 0.989 | 1/1 | **0.9956** |
| 155 | 🟢 classic155_diacritical_marks | ✅ | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 156 | 🟡 classic156_rtl_bidi_text | ✅ | 0.6818 | 0.9945 | 1/1 | **0.8705** |
| 157 | 🟢 classic157_cjk_extended | ✅ | 0.9841 | 0.9769 | 1/1 | **0.9844** |
| 158 | 🟢 classic158_emoji_skin_tones | ✅ | 0.9673 | 0.9882 | 1/1 | **0.9822** |
| 159 | 🟢 classic159_zwj_emoji | ✅ | 0.9372 | 0.9899 | 1/1 | **0.9708** |
| 160 | 🟢 classic160_punctuation_marks | ✅ | 0.9683 | 0.9933 | 1/1 | **0.9846** |
| 161 | 🟢 classic161_box_drawing | ✅ | 0.9752 | 0.9844 | 1/1 | **0.9838** |
| 162 | 🟢 classic162_cjk_emoji_styled | ✅ | 1.0 | 0.9877 | 1/1 | **0.9951** |
| 163 | 🟢 classic163_cyrillic_alphabets | ✅ | 1.0 | 0.9844 | 1/1 | **0.9938** |
| 164 | 🟢 classic164_indic_scripts | ✅ | 0.9947 | 0.9933 | 1/1 | **0.9952** |
| 165 | 🟡 classic165_southeast_asian | ✅ | 0.663 | 0.9858 | 1/1 | **0.8595** |
| 166 | 🟢 classic166_emoji_progress | ✅ | 0.9881 | 0.9717 | 1/1 | **0.9839** |
| 167 | 🟢 classic167_musical_symbols | ✅ | 1.0 | 0.9843 | 1/1 | **0.9937** |
| 168 | 🟢 classic168_mixed_ltr_rtl_styled | ✅ | 0.9259 | 0.974 | 1/1 | **0.96** |
| 169 | 🟢 classic169_korean_invoice | ✅ | 0.993 | 0.9814 | 1/1 | **0.9898** |
| 170 | 🟢 classic170_emoji_dashboard | ✅ | 0.9871 | 0.9771 | 1/1 | **0.9857** |
| 171 | 🟢 classic171_ipa_phonetic | ✅ | 0.9981 | 0.9894 | 1/1 | **0.995** |
| 172 | 🟢 classic172_emoji_timeline | ✅ | 1.0 | 0.9798 | 1/1 | **0.9919** |
| 173 | 🟢 classic173_african_languages | ✅ | 0.8864 | 0.9847 | 1/1 | **0.9484** |
| 174 | 🟢 classic174_technical_symbols | ✅ | 0.9971 | 0.9829 | 1/1 | **0.992** |
| 175 | 🟢 classic175_multiscript_catalog | ✅ | 0.9864 | 0.9798 | 1/1 | **0.9865** |
| 176 | 🟢 classic176_combining_characters | ✅ | 0.9837 | 0.9868 | 1/1 | **0.9882** |
| 177 | 🟢 classic177_emoji_calendar | ✅ | 1.0 | 0.985 | 1/1 | **0.994** |
| 178 | 🟢 classic178_caucasus_ethiopic | ✅ | 0.8462 | 0.9852 | 1/1 | **0.9326** |
| 179 | 🟢 classic179_emoji_inventory | ✅ | 1.0 | 0.9799 | 1/1 | **0.992** |
| 180 | 🟢 classic180_polyglot_paragraph | ✅ | 0.9846 | 0.9881 | 1/1 | **0.9891** |
| 181 | 🟢 classic181_feedback_tracker_with_images | ✅ | 0.9939 | 0.9688 | 2/2 | **0.9851** |
| 182 | 🟢 classic182_dense_long_text_columns | ✅ | 0.9845 | 0.9729 | 2/2 | **0.983** |
| 183 | 🟢 classic183_mixed_content_grid | ✅ | 1.0 | 0.9616 | 1/1 | **0.9846** |
| 184 | 🟢 classic184_wide_narrow_columns | ✅ | 1.0 | 0.9472 | 1/1 | **0.9789** |
| 185 | 🟢 classic185_tall_rows_vertical_align | ✅ | 1.0 | 0.9866 | 1/1 | **0.9946** |
| 186 | 🟢 classic186_multi_sheet_image_report | ✅ | 1.0 | 0.9917 | 2/2 | **0.9967** |
| 187 | 🟢 classic187_bug_report_with_screenshots | ✅ | 1.0 | 0.9342 | 1/1 | **0.9737** |
| 188 | 🟢 classic188_merged_header_with_images | ✅ | 1.0 | 0.9691 | 1/1 | **0.9876** |
| 189 | 🟢 classic189_alternating_image_text_rows | ✅ | 0.8589 | 0.9224 | 1/1 | **0.9125** |
| 190 | 🟢 classic190_dashboard_kpi_images | ✅ | 0.9815 | 0.9677 | 1/1 | **0.9797** |
| 191 | 🟢 classic191_payroll_calculator | ✅ | 0.8842 | 0.9141 | 9/9 | **0.9193** |

**Average Overall Score: 0.9251**

## Labeled Side-by-Side Comparison

<table>
<tr><th>Case</th><th>Comparison</th></tr>
<tr>
  <td><b>classic01_basic_table_with_headers<br><small>format: xlsx | case: classic01_basic_table_with_headers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic01_basic_table_with_headers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic01_basic_table_with_headers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic02_multiple_worksheets<br><small>format: xlsx | case: classic02_multiple_worksheets | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic02_multiple_worksheets_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic02_multiple_worksheets page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic03_empty_workbook<br><small>format: xlsx | case: classic03_empty_workbook | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic03_empty_workbook_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic03_empty_workbook page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic04_single_cell<br><small>format: xlsx | case: classic04_single_cell | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic04_single_cell_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic04_single_cell page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic05_wide_table<br><small>format: xlsx | case: classic05_wide_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic05_wide_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic05_wide_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic06_tall_table<br><small>format: xlsx | case: classic06_tall_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic06_tall_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic06_tall_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic07_numbers_only<br><small>format: xlsx | case: classic07_numbers_only | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic07_numbers_only_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic07_numbers_only page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic08_mixed_text_and_numbers<br><small>format: xlsx | case: classic08_mixed_text_and_numbers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic08_mixed_text_and_numbers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic08_mixed_text_and_numbers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic09_long_text<br><small>format: xlsx | case: classic09_long_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic09_long_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic09_long_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic10_special_xml_characters<br><small>format: xlsx | case: classic10_special_xml_characters | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic10_special_xml_characters_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic10_special_xml_characters page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic11_sparse_rows<br><small>format: xlsx | case: classic11_sparse_rows | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic11_sparse_rows_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic11_sparse_rows page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic12_sparse_columns<br><small>format: xlsx | case: classic12_sparse_columns | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic12_sparse_columns_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic12_sparse_columns page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic13_date_strings<br><small>format: xlsx | case: classic13_date_strings | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic13_date_strings_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic13_date_strings page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic14_decimal_numbers<br><small>format: xlsx | case: classic14_decimal_numbers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic14_decimal_numbers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic14_decimal_numbers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic15_negative_numbers<br><small>format: xlsx | case: classic15_negative_numbers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic15_negative_numbers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic15_negative_numbers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic16_percentage_strings<br><small>format: xlsx | case: classic16_percentage_strings | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic16_percentage_strings_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic16_percentage_strings page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic17_currency_strings<br><small>format: xlsx | case: classic17_currency_strings | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic17_currency_strings_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic17_currency_strings page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic18_large_dataset<br><small>format: xlsx | case: classic18_large_dataset | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic18_large_dataset_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic18_large_dataset page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic19_single_column_list<br><small>format: xlsx | case: classic19_single_column_list | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic19_single_column_list_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic19_single_column_list page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic20_all_empty_cells<br><small>format: xlsx | case: classic20_all_empty_cells | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic20_all_empty_cells_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic20_all_empty_cells page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic21_header_only<br><small>format: xlsx | case: classic21_header_only | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic21_header_only_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic21_header_only page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic22_long_sheet_name<br><small>format: xlsx | case: classic22_long_sheet_name | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic22_long_sheet_name_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic22_long_sheet_name page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic23_unicode_text<br><small>format: xlsx | case: classic23_unicode_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic23_unicode_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic23_unicode_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic24_red_text<br><small>format: xlsx | case: classic24_red_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic24_red_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic24_red_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic25_multiple_colors<br><small>format: xlsx | case: classic25_multiple_colors | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic25_multiple_colors_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic25_multiple_colors page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic26_inline_strings<br><small>format: xlsx | case: classic26_inline_strings | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic26_inline_strings_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic26_inline_strings page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic27_single_row<br><small>format: xlsx | case: classic27_single_row | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic27_single_row_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic27_single_row page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic28_duplicate_values<br><small>format: xlsx | case: classic28_duplicate_values | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic28_duplicate_values_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic28_duplicate_values page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic29_formula_results<br><small>format: xlsx | case: classic29_formula_results | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic29_formula_results_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic29_formula_results page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic30_mixed_empty_and_filled_sheets<br><small>format: xlsx | case: classic30_mixed_empty_and_filled_sheets | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic30_mixed_empty_and_filled_sheets_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic30_mixed_empty_and_filled_sheets page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic31_bold_header_row<br><small>format: xlsx | case: classic31_bold_header_row | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic31_bold_header_row_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic31_bold_header_row page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic32_right_aligned_numbers<br><small>format: xlsx | case: classic32_right_aligned_numbers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic32_right_aligned_numbers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic32_right_aligned_numbers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic33_centered_text<br><small>format: xlsx | case: classic33_centered_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic33_centered_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic33_centered_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic34_explicit_column_widths<br><small>format: xlsx | case: classic34_explicit_column_widths | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic34_explicit_column_widths_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic34_explicit_column_widths page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic35_explicit_row_heights<br><small>format: xlsx | case: classic35_explicit_row_heights | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic35_explicit_row_heights_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic35_explicit_row_heights page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic36_merged_cells<br><small>format: xlsx | case: classic36_merged_cells | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic36_merged_cells_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic36_merged_cells page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic37_freeze_panes<br><small>format: xlsx | case: classic37_freeze_panes | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic37_freeze_panes_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic37_freeze_panes page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic38_hyperlink_cell<br><small>format: xlsx | case: classic38_hyperlink_cell | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic38_hyperlink_cell_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic38_hyperlink_cell page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic39_financial_table<br><small>format: xlsx | case: classic39_financial_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic39_financial_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic39_financial_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic40_scientific_notation<br><small>format: xlsx | case: classic40_scientific_notation | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic40_scientific_notation_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic40_scientific_notation page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic41_integer_vs_float<br><small>format: xlsx | case: classic41_integer_vs_float | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic41_integer_vs_float_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic41_integer_vs_float page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic42_boolean_values<br><small>format: xlsx | case: classic42_boolean_values | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic42_boolean_values_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic42_boolean_values page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic43_inventory_report<br><small>format: xlsx | case: classic43_inventory_report | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic43_inventory_report_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic43_inventory_report page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic44_employee_roster<br><small>format: xlsx | case: classic44_employee_roster | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic44_employee_roster_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic44_employee_roster page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic45_sales_by_region<br><small>format: xlsx | case: classic45_sales_by_region | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic45_sales_by_region_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic45_sales_by_region page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic46_grade_book<br><small>format: xlsx | case: classic46_grade_book | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic46_grade_book_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic46_grade_book page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic47_time_series<br><small>format: xlsx | case: classic47_time_series | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic47_time_series_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic47_time_series page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic48_survey_results<br><small>format: xlsx | case: classic48_survey_results | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic48_survey_results_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic48_survey_results page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic49_contact_list<br><small>format: xlsx | case: classic49_contact_list | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic49_contact_list_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic49_contact_list page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic50_budget_vs_actuals<br><small>format: xlsx | case: classic50_budget_vs_actuals | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic50_budget_vs_actuals_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic50_budget_vs_actuals page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic51_product_catalog<br><small>format: xlsx | case: classic51_product_catalog | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic51_product_catalog_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic51_product_catalog page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic52_pivot_summary<br><small>format: xlsx | case: classic52_pivot_summary | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic52_pivot_summary_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic52_pivot_summary page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic53_invoice<br><small>format: xlsx | case: classic53_invoice | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic53_invoice_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic53_invoice page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic54_multi_level_header<br><small>format: xlsx | case: classic54_multi_level_header | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic54_multi_level_header_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic54_multi_level_header page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic55_error_values<br><small>format: xlsx | case: classic55_error_values | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic55_error_values_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic55_error_values page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic56_alternating_row_colors<br><small>format: xlsx | case: classic56_alternating_row_colors | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic56_alternating_row_colors_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic56_alternating_row_colors page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic57_cjk_only<br><small>format: xlsx | case: classic57_cjk_only | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic57_cjk_only_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic57_cjk_only page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic58_mixed_numeric_formats<br><small>format: xlsx | case: classic58_mixed_numeric_formats | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic58_mixed_numeric_formats_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic58_mixed_numeric_formats page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic59_multi_sheet_summary<br><small>format: xlsx | case: classic59_multi_sheet_summary | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic59_multi_sheet_summary_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic59_multi_sheet_summary page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic60_large_wide_table<br><small>format: xlsx | case: classic60_large_wide_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic60_large_wide_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic60_large_wide_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic61_product_card_with_image<br><small>format: xlsx | case: classic61_product_card_with_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic61_product_card_with_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic61_product_card_with_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic62_company_logo_header<br><small>format: xlsx | case: classic62_company_logo_header | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic62_company_logo_header_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic62_company_logo_header page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic63_two_products_side_by_side<br><small>format: xlsx | case: classic63_two_products_side_by_side | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic63_two_products_side_by_side_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic63_two_products_side_by_side page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic64_employee_directory_with_photo<br><small>format: xlsx | case: classic64_employee_directory_with_photo | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic64_employee_directory_with_photo_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic64_employee_directory_with_photo page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic65_inventory_with_product_photos<br><small>format: xlsx | case: classic65_inventory_with_product_photos | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic65_inventory_with_product_photos_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic65_inventory_with_product_photos page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic66_invoice_with_logo<br><small>format: xlsx | case: classic66_invoice_with_logo | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic66_invoice_with_logo_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic66_invoice_with_logo page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic67_real_estate_listing<br><small>format: xlsx | case: classic67_real_estate_listing | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic67_real_estate_listing_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic67_real_estate_listing page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic68_restaurant_menu<br><small>format: xlsx | case: classic68_restaurant_menu | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic68_restaurant_menu_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic68_restaurant_menu page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic69_image_only_sheet<br><small>format: xlsx | case: classic69_image_only_sheet | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic69_image_only_sheet_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic69_image_only_sheet page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic70_product_catalog_with_images<br><small>format: xlsx | case: classic70_product_catalog_with_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic70_product_catalog_with_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic70_product_catalog_with_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic71_multi_sheet_with_images<br><small>format: xlsx | case: classic71_multi_sheet_with_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic71_multi_sheet_with_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic71_multi_sheet_with_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic72_bar_chart_image_with_data<br><small>format: xlsx | case: classic72_bar_chart_image_with_data | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic72_bar_chart_image_with_data_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic72_bar_chart_image_with_data page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic73_event_flyer_with_banner<br><small>format: xlsx | case: classic73_event_flyer_with_banner | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic73_event_flyer_with_banner_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic73_event_flyer_with_banner page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic74_dashboard_with_kpi_image<br><small>format: xlsx | case: classic74_dashboard_with_kpi_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic74_dashboard_with_kpi_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic74_dashboard_with_kpi_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic75_certificate_with_seal<br><small>format: xlsx | case: classic75_certificate_with_seal | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic75_certificate_with_seal_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic75_certificate_with_seal page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic76_product_image_grid<br><small>format: xlsx | case: classic76_product_image_grid | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic76_product_image_grid_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic76_product_image_grid page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic77_news_article_with_hero_image<br><small>format: xlsx | case: classic77_news_article_with_hero_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic77_news_article_with_hero_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic77_news_article_with_hero_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic78_small_icon_per_row<br><small>format: xlsx | case: classic78_small_icon_per_row | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic78_small_icon_per_row_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic78_small_icon_per_row page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic79_wide_panoramic_banner<br><small>format: xlsx | case: classic79_wide_panoramic_banner | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic79_wide_panoramic_banner_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic79_wide_panoramic_banner page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic80_portrait_tall_image<br><small>format: xlsx | case: classic80_portrait_tall_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic80_portrait_tall_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic80_portrait_tall_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic81_step_by_step_with_images<br><small>format: xlsx | case: classic81_step_by_step_with_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic81_step_by_step_with_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic81_step_by_step_with_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic82_before_after_images<br><small>format: xlsx | case: classic82_before_after_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic82_before_after_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic82_before_after_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic83_color_swatch_palette<br><small>format: xlsx | case: classic83_color_swatch_palette | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic83_color_swatch_palette_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic83_color_swatch_palette page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic84_travel_destination_cards<br><small>format: xlsx | case: classic84_travel_destination_cards | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic84_travel_destination_cards_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic84_travel_destination_cards page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic85_lab_results_with_image<br><small>format: xlsx | case: classic85_lab_results_with_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic85_lab_results_with_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic85_lab_results_with_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic86_software_screenshot_features<br><small>format: xlsx | case: classic86_software_screenshot_features | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic86_software_screenshot_features_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic86_software_screenshot_features page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic87_sports_results_with_logos<br><small>format: xlsx | case: classic87_sports_results_with_logos | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic87_sports_results_with_logos_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic87_sports_results_with_logos page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic88_image_after_data<br><small>format: xlsx | case: classic88_image_after_data | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic88_image_after_data_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic88_image_after_data page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic89_nutrition_label_with_image<br><small>format: xlsx | case: classic89_nutrition_label_with_image | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic89_nutrition_label_with_image_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic89_nutrition_label_with_image page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic90_project_status_with_milestones<br><small>format: xlsx | case: classic90_project_status_with_milestones | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic90_project_status_with_milestones_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic90_project_status_with_milestones page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic91_simple_bar_chart<br><small>format: xlsx | case: classic91_simple_bar_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic91_simple_bar_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic91_simple_bar_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic92_horizontal_bar_chart<br><small>format: xlsx | case: classic92_horizontal_bar_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic92_horizontal_bar_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic92_horizontal_bar_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic93_line_chart<br><small>format: xlsx | case: classic93_line_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic93_line_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic93_line_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic94_pie_chart<br><small>format: xlsx | case: classic94_pie_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic94_pie_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic94_pie_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic95_area_chart<br><small>format: xlsx | case: classic95_area_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic95_area_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic95_area_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic96_scatter_chart<br><small>format: xlsx | case: classic96_scatter_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic96_scatter_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic96_scatter_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic97_doughnut_chart<br><small>format: xlsx | case: classic97_doughnut_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic97_doughnut_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic97_doughnut_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic98_radar_chart<br><small>format: xlsx | case: classic98_radar_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic98_radar_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic98_radar_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic99_bubble_chart<br><small>format: xlsx | case: classic99_bubble_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic99_bubble_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic99_bubble_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic100_stacked_bar_chart<br><small>format: xlsx | case: classic100_stacked_bar_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic100_stacked_bar_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic100_stacked_bar_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic101_percent_stacked_bar<br><small>format: xlsx | case: classic101_percent_stacked_bar | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic101_percent_stacked_bar_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic101_percent_stacked_bar page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic102_line_chart_with_markers<br><small>format: xlsx | case: classic102_line_chart_with_markers | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic102_line_chart_with_markers_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic102_line_chart_with_markers page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic103_pie_chart_with_labels<br><small>format: xlsx | case: classic103_pie_chart_with_labels | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic103_pie_chart_with_labels_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic103_pie_chart_with_labels page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic104_combo_bar_line_chart<br><small>format: xlsx | case: classic104_combo_bar_line_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic104_combo_bar_line_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic104_combo_bar_line_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic105_3d_bar_chart<br><small>format: xlsx | case: classic105_3d_bar_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic105_3d_bar_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic105_3d_bar_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic106_3d_pie_chart<br><small>format: xlsx | case: classic106_3d_pie_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic106_3d_pie_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic106_3d_pie_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic107_multi_series_line<br><small>format: xlsx | case: classic107_multi_series_line | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic107_multi_series_line_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic107_multi_series_line page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic108_stacked_area_chart<br><small>format: xlsx | case: classic108_stacked_area_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic108_stacked_area_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic108_stacked_area_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic109_scatter_with_trendline<br><small>format: xlsx | case: classic109_scatter_with_trendline | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic109_scatter_with_trendline_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic109_scatter_with_trendline page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic110_chart_with_legend<br><small>format: xlsx | case: classic110_chart_with_legend | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic110_chart_with_legend_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic110_chart_with_legend page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic111_chart_with_axis_labels<br><small>format: xlsx | case: classic111_chart_with_axis_labels | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic111_chart_with_axis_labels_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic111_chart_with_axis_labels page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic112_multiple_charts<br><small>format: xlsx | case: classic112_multiple_charts | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic112_multiple_charts_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic112_multiple_charts page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic113_chart_sheet<br><small>format: xlsx | case: classic113_chart_sheet | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic113_chart_sheet_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic113_chart_sheet page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic114_chart_large_dataset<br><small>format: xlsx | case: classic114_chart_large_dataset | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic114_chart_large_dataset_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic114_chart_large_dataset page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic115_chart_negative_values<br><small>format: xlsx | case: classic115_chart_negative_values | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic115_chart_negative_values_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic115_chart_negative_values page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic116_percent_stacked_area<br><small>format: xlsx | case: classic116_percent_stacked_area | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic116_percent_stacked_area_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic116_percent_stacked_area page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic117_stock_ohlc_chart<br><small>format: xlsx | case: classic117_stock_ohlc_chart | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic117_stock_ohlc_chart_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic117_stock_ohlc_chart page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic118_bar_chart_custom_colors<br><small>format: xlsx | case: classic118_bar_chart_custom_colors | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic118_bar_chart_custom_colors_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic118_bar_chart_custom_colors page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic119_dashboard_multi_charts<br><small>format: xlsx | case: classic119_dashboard_multi_charts | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic119_dashboard_multi_charts_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic119_dashboard_multi_charts page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic120_chart_with_date_axis<br><small>format: xlsx | case: classic120_chart_with_date_axis | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic120_chart_with_date_axis_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic120_chart_with_date_axis page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic121_thin_borders<br><small>format: xlsx | case: classic121_thin_borders | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic121_thin_borders_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic121_thin_borders page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic122_thick_outer_thin_inner<br><small>format: xlsx | case: classic122_thick_outer_thin_inner | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic122_thick_outer_thin_inner_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic122_thick_outer_thin_inner page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic123_dashed_borders<br><small>format: xlsx | case: classic123_dashed_borders | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic123_dashed_borders_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic123_dashed_borders page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic124_colored_borders<br><small>format: xlsx | case: classic124_colored_borders | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic124_colored_borders_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic124_colored_borders page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic125_solid_fills<br><small>format: xlsx | case: classic125_solid_fills | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic125_solid_fills_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic125_solid_fills page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic126_dark_header<br><small>format: xlsx | case: classic126_dark_header | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic126_dark_header_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic126_dark_header page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic127_font_styles<br><small>format: xlsx | case: classic127_font_styles | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic127_font_styles_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic127_font_styles page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic128_font_sizes<br><small>format: xlsx | case: classic128_font_sizes | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic128_font_sizes_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic128_font_sizes page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic129_alignment_combos<br><small>format: xlsx | case: classic129_alignment_combos | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic129_alignment_combos_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic129_alignment_combos page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic130_wrap_and_indent<br><small>format: xlsx | case: classic130_wrap_and_indent | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic130_wrap_and_indent_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic130_wrap_and_indent page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic131_number_formats<br><small>format: xlsx | case: classic131_number_formats | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic131_number_formats_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic131_number_formats page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic132_striped_table<br><small>format: xlsx | case: classic132_striped_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic132_striped_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic132_striped_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic133_gradient_rows<br><small>format: xlsx | case: classic133_gradient_rows | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic133_gradient_rows_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic133_gradient_rows page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic134_heatmap<br><small>format: xlsx | case: classic134_heatmap | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic134_heatmap_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic134_heatmap page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic135_bottom_border_only<br><small>format: xlsx | case: classic135_bottom_border_only | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic135_bottom_border_only_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic135_bottom_border_only page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic136_financial_report_styled<br><small>format: xlsx | case: classic136_financial_report_styled | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic136_financial_report_styled_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic136_financial_report_styled page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic137_checkerboard<br><small>format: xlsx | case: classic137_checkerboard | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic137_checkerboard_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic137_checkerboard page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic138_color_grid<br><small>format: xlsx | case: classic138_color_grid | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic138_color_grid_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic138_color_grid page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic139_pattern_fills<br><small>format: xlsx | case: classic139_pattern_fills | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic139_pattern_fills_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic139_pattern_fills page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic140_rotated_text<br><small>format: xlsx | case: classic140_rotated_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic140_rotated_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic140_rotated_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic141_mixed_edge_borders<br><small>format: xlsx | case: classic141_mixed_edge_borders | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic141_mixed_edge_borders_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic141_mixed_edge_borders page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic142_styled_invoice<br><small>format: xlsx | case: classic142_styled_invoice | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic142_styled_invoice_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic142_styled_invoice page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic143_colored_tabs<br><small>format: xlsx | case: classic143_colored_tabs | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic143_colored_tabs_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic143_colored_tabs page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic144_note_style_cells<br><small>format: xlsx | case: classic144_note_style_cells | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic144_note_style_cells_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic144_note_style_cells page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic145_status_badges<br><small>format: xlsx | case: classic145_status_badges | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic145_status_badges_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic145_status_badges page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic146_double_border_table<br><small>format: xlsx | case: classic146_double_border_table | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic146_double_border_table_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic146_double_border_table page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic147_multi_sheet_styled<br><small>format: xlsx | case: classic147_multi_sheet_styled | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic147_multi_sheet_styled_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic147_multi_sheet_styled page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic148_frozen_styled_grid<br><small>format: xlsx | case: classic148_frozen_styled_grid | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic148_frozen_styled_grid_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic148_frozen_styled_grid page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic149_merged_styled_sections<br><small>format: xlsx | case: classic149_merged_styled_sections | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic149_merged_styled_sections_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic149_merged_styled_sections page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic150_kitchen_sink_styles<br><small>format: xlsx | case: classic150_kitchen_sink_styles | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic150_kitchen_sink_styles_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic150_kitchen_sink_styles page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic151_multilingual_greetings<br><small>format: xlsx | case: classic151_multilingual_greetings | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic151_multilingual_greetings_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic151_multilingual_greetings page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic152_emoji_sampler<br><small>format: xlsx | case: classic152_emoji_sampler | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic152_emoji_sampler_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic152_emoji_sampler page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic153_currency_symbols<br><small>format: xlsx | case: classic153_currency_symbols | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic153_currency_symbols_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic153_currency_symbols page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic154_math_symbols<br><small>format: xlsx | case: classic154_math_symbols | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic154_math_symbols_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic154_math_symbols page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic155_diacritical_marks<br><small>format: xlsx | case: classic155_diacritical_marks | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic155_diacritical_marks_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic155_diacritical_marks page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic156_rtl_bidi_text<br><small>format: xlsx | case: classic156_rtl_bidi_text | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic156_rtl_bidi_text_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic156_rtl_bidi_text page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic157_cjk_extended<br><small>format: xlsx | case: classic157_cjk_extended | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic157_cjk_extended_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic157_cjk_extended page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic158_emoji_skin_tones<br><small>format: xlsx | case: classic158_emoji_skin_tones | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic158_emoji_skin_tones_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic158_emoji_skin_tones page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic159_zwj_emoji<br><small>format: xlsx | case: classic159_zwj_emoji | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic159_zwj_emoji_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic159_zwj_emoji page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic160_punctuation_marks<br><small>format: xlsx | case: classic160_punctuation_marks | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic160_punctuation_marks_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic160_punctuation_marks page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic161_box_drawing<br><small>format: xlsx | case: classic161_box_drawing | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic161_box_drawing_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic161_box_drawing page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic162_cjk_emoji_styled<br><small>format: xlsx | case: classic162_cjk_emoji_styled | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic162_cjk_emoji_styled_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic162_cjk_emoji_styled page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic163_cyrillic_alphabets<br><small>format: xlsx | case: classic163_cyrillic_alphabets | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic163_cyrillic_alphabets_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic163_cyrillic_alphabets page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic164_indic_scripts<br><small>format: xlsx | case: classic164_indic_scripts | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic164_indic_scripts_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic164_indic_scripts page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic165_southeast_asian<br><small>format: xlsx | case: classic165_southeast_asian | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic165_southeast_asian_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic165_southeast_asian page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic166_emoji_progress<br><small>format: xlsx | case: classic166_emoji_progress | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic166_emoji_progress_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic166_emoji_progress page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic167_musical_symbols<br><small>format: xlsx | case: classic167_musical_symbols | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic167_musical_symbols_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic167_musical_symbols page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic168_mixed_ltr_rtl_styled<br><small>format: xlsx | case: classic168_mixed_ltr_rtl_styled | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic168_mixed_ltr_rtl_styled_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic168_mixed_ltr_rtl_styled page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic169_korean_invoice<br><small>format: xlsx | case: classic169_korean_invoice | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic169_korean_invoice_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic169_korean_invoice page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic170_emoji_dashboard<br><small>format: xlsx | case: classic170_emoji_dashboard | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic170_emoji_dashboard_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic170_emoji_dashboard page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic171_ipa_phonetic<br><small>format: xlsx | case: classic171_ipa_phonetic | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic171_ipa_phonetic_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic171_ipa_phonetic page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic172_emoji_timeline<br><small>format: xlsx | case: classic172_emoji_timeline | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic172_emoji_timeline_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic172_emoji_timeline page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic173_african_languages<br><small>format: xlsx | case: classic173_african_languages | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic173_african_languages_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic173_african_languages page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic174_technical_symbols<br><small>format: xlsx | case: classic174_technical_symbols | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic174_technical_symbols_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic174_technical_symbols page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic175_multiscript_catalog<br><small>format: xlsx | case: classic175_multiscript_catalog | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic175_multiscript_catalog_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic175_multiscript_catalog page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic176_combining_characters<br><small>format: xlsx | case: classic176_combining_characters | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic176_combining_characters_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic176_combining_characters page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic177_emoji_calendar<br><small>format: xlsx | case: classic177_emoji_calendar | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic177_emoji_calendar_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic177_emoji_calendar page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic178_caucasus_ethiopic<br><small>format: xlsx | case: classic178_caucasus_ethiopic | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic178_caucasus_ethiopic_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic178_caucasus_ethiopic page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic179_emoji_inventory<br><small>format: xlsx | case: classic179_emoji_inventory | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic179_emoji_inventory_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic179_emoji_inventory page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic180_polyglot_paragraph<br><small>format: xlsx | case: classic180_polyglot_paragraph | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic180_polyglot_paragraph_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic180_polyglot_paragraph page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic181_feedback_tracker_with_images<br><small>format: xlsx | case: classic181_feedback_tracker_with_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic181_feedback_tracker_with_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic181_feedback_tracker_with_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic182_dense_long_text_columns<br><small>format: xlsx | case: classic182_dense_long_text_columns | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic182_dense_long_text_columns_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic182_dense_long_text_columns page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic183_mixed_content_grid<br><small>format: xlsx | case: classic183_mixed_content_grid | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic183_mixed_content_grid_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic183_mixed_content_grid page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic184_wide_narrow_columns<br><small>format: xlsx | case: classic184_wide_narrow_columns | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic184_wide_narrow_columns_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic184_wide_narrow_columns page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic185_tall_rows_vertical_align<br><small>format: xlsx | case: classic185_tall_rows_vertical_align | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic185_tall_rows_vertical_align_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic185_tall_rows_vertical_align page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic186_multi_sheet_image_report<br><small>format: xlsx | case: classic186_multi_sheet_image_report | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic186_multi_sheet_image_report_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic186_multi_sheet_image_report page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic187_bug_report_with_screenshots<br><small>format: xlsx | case: classic187_bug_report_with_screenshots | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic187_bug_report_with_screenshots_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic187_bug_report_with_screenshots page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic188_merged_header_with_images<br><small>format: xlsx | case: classic188_merged_header_with_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic188_merged_header_with_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic188_merged_header_with_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic189_alternating_image_text_rows<br><small>format: xlsx | case: classic189_alternating_image_text_rows | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic189_alternating_image_text_rows_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic189_alternating_image_text_rows page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic190_dashboard_kpi_images<br><small>format: xlsx | case: classic190_dashboard_kpi_images | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic190_dashboard_kpi_images_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic190_dashboard_kpi_images page 1 comparison"></td>
</tr>
<tr>
  <td><b>classic191_payroll_calculator<br><small>format: xlsx | case: classic191_payroll_calculator | scope: rust-classic-xlsx-office</small></b><br>Page 1</td>
  <td><img src="side-by-side/classic191_payroll_calculator_p1_rust_minipdf_vs_microsoft_365_excel_reference.png" width="760" alt="classic191_payroll_calculator page 1 comparison"></td>
</tr>
</table>

## Difference Heatmaps

Blue areas are below the configured difference threshold; red areas have stronger pixel differences. The reference rendering is retained as faint context.

<table>
<tr><th>Case</th><th>Heatmap</th><th>Metrics</th></tr>
<tr>
  <td><b>classic01_basic_table_with_headers</b><br>Page 1</td>
  <td><img src="images/classic01_basic_table_with_headers_p1_heatmap.png" width="760" alt="classic01_basic_table_with_headers page 1 difference heatmap"></td>
  <td>changed: 9366 px (0.46%)<br>bbox: [111, 148, 416, 302]<br>mean abs RGB: 0.7088<br>RMSE RGB: 11.845<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic02_multiple_worksheets</b><br>Page 1</td>
  <td><img src="images/classic02_multiple_worksheets_p1_heatmap.png" width="760" alt="classic02_multiple_worksheets page 1 difference heatmap"></td>
  <td>changed: 5828 px (0.28%)<br>bbox: [112, 148, 318, 303]<br>mean abs RGB: 0.4492<br>RMSE RGB: 9.4585<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic03_empty_workbook</b><br>Page 1</td>
  <td><img src="images/classic03_empty_workbook_p1_heatmap.png" width="760" alt="classic03_empty_workbook page 1 difference heatmap"></td>
  <td>changed: 0 px (0.00%)<br>bbox: None<br>mean abs RGB: 0.0<br>RMSE RGB: 0.0<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic04_single_cell</b><br>Page 1</td>
  <td><img src="images/classic04_single_cell_p1_heatmap.png" width="760" alt="classic04_single_cell page 1 difference heatmap"></td>
  <td>changed: 670 px (0.03%)<br>bbox: [113, 147, 165, 177]<br>mean abs RGB: 0.0533<br>RMSE RGB: 3.2989<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic05_wide_table</b><br>Page 1</td>
  <td><img src="images/classic05_wide_table_p1_heatmap.png" width="760" alt="classic05_wide_table page 1 difference heatmap"></td>
  <td>changed: 17870 px (0.87%)<br>bbox: [111, 148, 995, 333]<br>mean abs RGB: 1.3662<br>RMSE RGB: 16.5157<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic06_tall_table</b><br>Page 1</td>
  <td><img src="images/classic06_tall_table_p1_heatmap.png" width="760" alt="classic06_tall_table page 1 difference heatmap"></td>
  <td>changed: 255989 px (12.50%)<br>bbox: [113, 147, 712, 1524]<br>mean abs RGB: 19.1036<br>RMSE RGB: 61.1766<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic07_numbers_only</b><br>Page 1</td>
  <td><img src="images/classic07_numbers_only_p1_heatmap.png" width="760" alt="classic07_numbers_only page 1 difference heatmap"></td>
  <td>changed: 3137 px (0.15%)<br>bbox: [175, 148, 423, 271]<br>mean abs RGB: 0.2423<br>RMSE RGB: 6.9472<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic08_mixed_text_and_numbers</b><br>Page 1</td>
  <td><img src="images/classic08_mixed_text_and_numbers_p1_heatmap.png" width="760" alt="classic08_mixed_text_and_numbers page 1 difference heatmap"></td>
  <td>changed: 7505 px (0.37%)<br>bbox: [111, 148, 318, 333]<br>mean abs RGB: 0.572<br>RMSE RGB: 10.6534<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic09_long_text</b><br>Page 1</td>
  <td><img src="images/classic09_long_text_p1_heatmap.png" width="760" alt="classic09_long_text page 1 difference heatmap"></td>
  <td>changed: 47324 px (2.31%)<br>bbox: [111, 147, 1241, 301]<br>mean abs RGB: 3.6479<br>RMSE RGB: 27.0923<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic10_special_xml_characters</b><br>Page 1</td>
  <td><img src="images/classic10_special_xml_characters_p1_heatmap.png" width="760" alt="classic10_special_xml_characters page 1 difference heatmap"></td>
  <td>changed: 11665 px (0.57%)<br>bbox: [111, 147, 442, 369]<br>mean abs RGB: 0.8796<br>RMSE RGB: 13.1519<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic11_sparse_rows</b><br>Page 1</td>
  <td><img src="images/classic11_sparse_rows_p1_heatmap.png" width="760" alt="classic11_sparse_rows page 1 difference heatmap"></td>
  <td>changed: 3201 px (0.16%)<br>bbox: [111, 148, 212, 771]<br>mean abs RGB: 0.2428<br>RMSE RGB: 6.9123<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic12_sparse_columns</b><br>Page 1</td>
  <td><img src="images/classic12_sparse_columns_p1_heatmap.png" width="760" alt="classic12_sparse_columns page 1 difference heatmap"></td>
  <td>changed: 4436 px (0.22%)<br>bbox: [113, 147, 1046, 243]<br>mean abs RGB: 0.3441<br>RMSE RGB: 8.307<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic13_date_strings</b><br>Page 1</td>
  <td><img src="images/classic13_date_strings_p1_heatmap.png" width="760" alt="classic13_date_strings page 1 difference heatmap"></td>
  <td>changed: 12045 px (0.59%)<br>bbox: [112, 148, 312, 336]<br>mean abs RGB: 0.9089<br>RMSE RGB: 13.4109<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic14_decimal_numbers</b><br>Page 1</td>
  <td><img src="images/classic14_decimal_numbers_p1_heatmap.png" width="760" alt="classic14_decimal_numbers page 1 difference heatmap"></td>
  <td>changed: 8965 px (0.44%)<br>bbox: [112, 147, 318, 336]<br>mean abs RGB: 0.6843<br>RMSE RGB: 11.6416<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic15_negative_numbers</b><br>Page 1</td>
  <td><img src="images/classic15_negative_numbers_p1_heatmap.png" width="760" alt="classic15_negative_numbers page 1 difference heatmap"></td>
  <td>changed: 9615 px (0.47%)<br>bbox: [91, 147, 318, 369]<br>mean abs RGB: 0.7286<br>RMSE RGB: 12.0072<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic16_percentage_strings</b><br>Page 1</td>
  <td><img src="images/classic16_percentage_strings_p1_heatmap.png" width="760" alt="classic16_percentage_strings page 1 difference heatmap"></td>
  <td>changed: 10180 px (0.50%)<br>bbox: [112, 148, 291, 333]<br>mean abs RGB: 0.7626<br>RMSE RGB: 12.2215<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic17_currency_strings</b><br>Page 1</td>
  <td><img src="images/classic17_currency_strings_p1_heatmap.png" width="760" alt="classic17_currency_strings page 1 difference heatmap"></td>
  <td>changed: 12728 px (0.62%)<br>bbox: [111, 148, 315, 365]<br>mean abs RGB: 0.9453<br>RMSE RGB: 13.5988<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic18_large_dataset</b><br>Page 1</td>
  <td><img src="images/classic18_large_dataset_p1_heatmap.png" width="760" alt="classic18_large_dataset page 1 difference heatmap"></td>
  <td>changed: 340909 px (16.65%)<br>bbox: [112, 147, 1034, 1521]<br>mean abs RGB: 25.9895<br>RMSE RGB: 71.8095<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic19_single_column_list</b><br>Page 1</td>
  <td><img src="images/classic19_single_column_list_p1_heatmap.png" width="760" alt="classic19_single_column_list page 1 difference heatmap"></td>
  <td>changed: 15686 px (0.77%)<br>bbox: [113, 148, 189, 802]<br>mean abs RGB: 1.1432<br>RMSE RGB: 14.8578<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic20_all_empty_cells</b><br>Page 1</td>
  <td><img src="images/classic20_all_empty_cells_p1_heatmap.png" width="760" alt="classic20_all_empty_cells page 1 difference heatmap"></td>
  <td>changed: 0 px (0.00%)<br>bbox: None<br>mean abs RGB: 0.0<br>RMSE RGB: 0.0<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic21_header_only</b><br>Page 1</td>
  <td><img src="images/classic21_header_only_p1_heatmap.png" width="760" alt="classic21_header_only page 1 difference heatmap"></td>
  <td>changed: 2826 px (0.14%)<br>bbox: [112, 147, 576, 177]<br>mean abs RGB: 0.2161<br>RMSE RGB: 6.5457<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic22_long_sheet_name</b><br>Page 1</td>
  <td><img src="images/classic22_long_sheet_name_p1_heatmap.png" width="760" alt="classic22_long_sheet_name page 1 difference heatmap"></td>
  <td>changed: 3899 px (0.19%)<br>bbox: [113, 147, 318, 240]<br>mean abs RGB: 0.3025<br>RMSE RGB: 7.7867<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic23_unicode_text</b><br>Page 1</td>
  <td><img src="images/classic23_unicode_text_p1_heatmap.png" width="760" alt="classic23_unicode_text page 1 difference heatmap"></td>
  <td>changed: 17916 px (0.88%)<br>bbox: [111, 148, 423, 369]<br>mean abs RGB: 1.3076<br>RMSE RGB: 15.8331<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic24_red_text</b><br>Page 1</td>
  <td><img src="images/classic24_red_text_p1_heatmap.png" width="760" alt="classic24_red_text page 1 difference heatmap"></td>
  <td>changed: 11806 px (0.58%)<br>bbox: [112, 148, 440, 275]<br>mean abs RGB: 0.687<br>RMSE RGB: 11.5348<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic25_multiple_colors</b><br>Page 1</td>
  <td><img src="images/classic25_multiple_colors_p1_heatmap.png" width="760" alt="classic25_multiple_colors page 1 difference heatmap"></td>
  <td>changed: 22652 px (1.11%)<br>bbox: [111, 147, 409, 430]<br>mean abs RGB: 1.0527<br>RMSE RGB: 14.0972<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic26_inline_strings</b><br>Page 1</td>
  <td><img src="images/classic26_inline_strings_p1_heatmap.png" width="760" alt="classic26_inline_strings page 1 difference heatmap"></td>
  <td>changed: 7355 px (0.36%)<br>bbox: [111, 147, 391, 240]<br>mean abs RGB: 0.555<br>RMSE RGB: 10.5018<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic27_single_row</b><br>Page 1</td>
  <td><img src="images/classic27_single_row_p1_heatmap.png" width="760" alt="classic27_single_row page 1 difference heatmap"></td>
  <td>changed: 3326 px (0.16%)<br>bbox: [113, 147, 778, 177]<br>mean abs RGB: 0.2589<br>RMSE RGB: 7.1788<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic28_duplicate_values</b><br>Page 1</td>
  <td><img src="images/classic28_duplicate_values_p1_heatmap.png" width="760" alt="classic28_duplicate_values page 1 difference heatmap"></td>
  <td>changed: 8030 px (0.39%)<br>bbox: [111, 148, 462, 302]<br>mean abs RGB: 0.6092<br>RMSE RGB: 11.0264<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic29_formula_results</b><br>Page 1</td>
  <td><img src="images/classic29_formula_results_p1_heatmap.png" width="760" alt="classic29_formula_results page 1 difference heatmap"></td>
  <td>changed: 8583 px (0.42%)<br>bbox: [111, 147, 527, 302]<br>mean abs RGB: 0.6662<br>RMSE RGB: 11.5602<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic30_mixed_empty_and_filled_sheets</b><br>Page 1</td>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_heatmap.png" width="760" alt="classic30_mixed_empty_and_filled_sheets page 1 difference heatmap"></td>
  <td>changed: 1679 px (0.08%)<br>bbox: [119, 147, 279, 223]<br>mean abs RGB: 0.1337<br>RMSE RGB: 5.2153<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic31_bold_header_row</b><br>Page 1</td>
  <td><img src="images/classic31_bold_header_row_p1_heatmap.png" width="760" alt="classic31_bold_header_row page 1 difference heatmap"></td>
  <td>changed: 15895 px (0.78%)<br>bbox: [112, 147, 527, 302]<br>mean abs RGB: 1.2619<br>RMSE RGB: 16.0324<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic32_right_aligned_numbers</b><br>Page 1</td>
  <td><img src="images/classic32_right_aligned_numbers_p1_heatmap.png" width="760" alt="classic32_right_aligned_numbers page 1 difference heatmap"></td>
  <td>changed: 7364 px (0.36%)<br>bbox: [113, 147, 318, 271]<br>mean abs RGB: 0.5685<br>RMSE RGB: 10.6434<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic33_centered_text</b><br>Page 1</td>
  <td><img src="images/classic33_centered_text_p1_heatmap.png" width="760" alt="classic33_centered_text page 1 difference heatmap"></td>
  <td>changed: 5150 px (0.25%)<br>bbox: [113, 147, 596, 240]<br>mean abs RGB: 0.3998<br>RMSE RGB: 8.94<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic34_explicit_column_widths</b><br>Page 1</td>
  <td><img src="images/classic34_explicit_column_widths_p1_heatmap.png" width="760" alt="classic34_explicit_column_widths page 1 difference heatmap"></td>
  <td>changed: 12311 px (0.60%)<br>bbox: [113, 147, 662, 275]<br>mean abs RGB: 0.9422<br>RMSE RGB: 13.7021<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic35_explicit_row_heights</b><br>Page 1</td>
  <td><img src="images/classic35_explicit_row_heights_p1_heatmap.png" width="760" alt="classic35_explicit_row_heights page 1 difference heatmap"></td>
  <td>changed: 5551 px (0.27%)<br>bbox: [111, 178, 318, 344]<br>mean abs RGB: 0.4245<br>RMSE RGB: 9.1722<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic36_merged_cells</b><br>Page 1</td>
  <td><img src="images/classic36_merged_cells_p1_heatmap.png" width="760" alt="classic36_merged_cells page 1 difference heatmap"></td>
  <td>changed: 11739 px (0.57%)<br>bbox: [112, 147, 483, 271]<br>mean abs RGB: 0.9028<br>RMSE RGB: 13.4257<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic37_freeze_panes</b><br>Page 1</td>
  <td><img src="images/classic37_freeze_panes_p1_heatmap.png" width="760" alt="classic37_freeze_panes page 1 difference heatmap"></td>
  <td>changed: 42610 px (2.08%)<br>bbox: [113, 147, 487, 802]<br>mean abs RGB: 3.2227<br>RMSE RGB: 25.2354<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic38_hyperlink_cell</b><br>Page 1</td>
  <td><img src="images/classic38_hyperlink_cell_p1_heatmap.png" width="760" alt="classic38_hyperlink_cell page 1 difference heatmap"></td>
  <td>changed: 8882 px (0.43%)<br>bbox: [112, 148, 478, 243]<br>mean abs RGB: 0.67<br>RMSE RGB: 11.5593<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic39_financial_table</b><br>Page 1</td>
  <td><img src="images/classic39_financial_table_p1_heatmap.png" width="760" alt="classic39_financial_table page 1 difference heatmap"></td>
  <td>changed: 19862 px (0.97%)<br>bbox: [111, 147, 527, 365]<br>mean abs RGB: 1.4598<br>RMSE RGB: 17.0075<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic40_scientific_notation</b><br>Page 1</td>
  <td><img src="images/classic40_scientific_notation_p1_heatmap.png" width="760" alt="classic40_scientific_notation page 1 difference heatmap"></td>
  <td>changed: 14016 px (0.68%)<br>bbox: [58, 147, 318, 337]<br>mean abs RGB: 1.08<br>RMSE RGB: 14.6673<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic41_integer_vs_float</b><br>Page 1</td>
  <td><img src="images/classic41_integer_vs_float_p1_heatmap.png" width="760" alt="classic41_integer_vs_float page 1 difference heatmap"></td>
  <td>changed: 11404 px (0.56%)<br>bbox: [111, 147, 318, 427]<br>mean abs RGB: 0.8566<br>RMSE RGB: 12.9788<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic42_boolean_values</b><br>Page 1</td>
  <td><img src="images/classic42_boolean_values_p1_heatmap.png" width="760" alt="classic42_boolean_values page 1 difference heatmap"></td>
  <td>changed: 10590 px (0.52%)<br>bbox: [111, 147, 298, 333]<br>mean abs RGB: 0.8041<br>RMSE RGB: 12.6042<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic43_inventory_report</b><br>Page 1</td>
  <td><img src="images/classic43_inventory_report_p1_heatmap.png" width="760" alt="classic43_inventory_report page 1 difference heatmap"></td>
  <td>changed: 39317 px (1.92%)<br>bbox: [112, 147, 748, 397]<br>mean abs RGB: 3.0401<br>RMSE RGB: 24.7011<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic44_employee_roster</b><br>Page 1</td>
  <td><img src="images/classic44_employee_roster_p1_heatmap.png" width="760" alt="classic44_employee_roster page 1 difference heatmap"></td>
  <td>changed: 56803 px (2.77%)<br>bbox: [113, 147, 839, 431]<br>mean abs RGB: 4.2635<br>RMSE RGB: 28.9301<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic45_sales_by_region</b><br>Page 1</td>
  <td><img src="images/classic45_sales_by_region_p1_heatmap.png" width="760" alt="classic45_sales_by_region page 1 difference heatmap"></td>
  <td>changed: 6720 px (0.33%)<br>bbox: [112, 147, 318, 303]<br>mean abs RGB: 0.5185<br>RMSE RGB: 10.1799<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic46_grade_book</b><br>Page 1</td>
  <td><img src="images/classic46_grade_book_p1_heatmap.png" width="760" alt="classic46_grade_book page 1 difference heatmap"></td>
  <td>changed: 25235 px (1.23%)<br>bbox: [111, 147, 801, 396]<br>mean abs RGB: 1.949<br>RMSE RGB: 19.7108<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic47_time_series</b><br>Page 1</td>
  <td><img src="images/classic47_time_series_p1_heatmap.png" width="760" alt="classic47_time_series page 1 difference heatmap"></td>
  <td>changed: 69137 px (3.38%)<br>bbox: [113, 147, 527, 1149]<br>mean abs RGB: 5.2773<br>RMSE RGB: 32.3033<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic48_survey_results</b><br>Page 1</td>
  <td><img src="images/classic48_survey_results_p1_heatmap.png" width="760" alt="classic48_survey_results page 1 difference heatmap"></td>
  <td>changed: 21036 px (1.03%)<br>bbox: [112, 147, 782, 333]<br>mean abs RGB: 1.6082<br>RMSE RGB: 17.854<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic49_contact_list</b><br>Page 1</td>
  <td><img src="images/classic49_contact_list_p1_heatmap.png" width="760" alt="classic49_contact_list page 1 difference heatmap"></td>
  <td>changed: 41668 px (2.03%)<br>bbox: [111, 147, 621, 400]<br>mean abs RGB: 3.1558<br>RMSE RGB: 24.9733<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic50_budget_vs_actuals</b><br>Page 1</td>
  <td><img src="images/classic50_budget_vs_actuals_p1_heatmap.png" width="760" alt="classic50_budget_vs_actuals page 1 difference heatmap"></td>
  <td>changed: 33615 px (1.64%)<br>bbox: [112, 147, 736, 333]<br>mean abs RGB: 2.6004<br>RMSE RGB: 22.8191<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic51_product_catalog</b><br>Page 1</td>
  <td><img src="images/classic51_product_catalog_p1_heatmap.png" width="760" alt="classic51_product_catalog page 1 difference heatmap"></td>
  <td>changed: 56576 px (2.76%)<br>bbox: [113, 147, 671, 494]<br>mean abs RGB: 4.317<br>RMSE RGB: 29.2605<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic52_pivot_summary</b><br>Page 1</td>
  <td><img src="images/classic52_pivot_summary_p1_heatmap.png" width="760" alt="classic52_pivot_summary page 1 difference heatmap"></td>
  <td>changed: 32892 px (1.61%)<br>bbox: [111, 147, 736, 333]<br>mean abs RGB: 2.6003<br>RMSE RGB: 22.9743<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic53_invoice</b><br>Page 1</td>
  <td><img src="images/classic53_invoice_p1_heatmap.png" width="760" alt="classic53_invoice page 1 difference heatmap"></td>
  <td>changed: 39995 px (1.95%)<br>bbox: [111, 152, 527, 771]<br>mean abs RGB: 3.1272<br>RMSE RGB: 25.1296<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic54_multi_level_header</b><br>Page 1</td>
  <td><img src="images/classic54_multi_level_header_p1_heatmap.png" width="760" alt="classic54_multi_level_header page 1 difference heatmap"></td>
  <td>changed: 25073 px (1.22%)<br>bbox: [112, 148, 841, 302]<br>mean abs RGB: 2.0188<br>RMSE RGB: 20.3726<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic55_error_values</b><br>Page 1</td>
  <td><img src="images/classic55_error_values_p1_heatmap.png" width="760" alt="classic55_error_values page 1 difference heatmap"></td>
  <td>changed: 17540 px (0.86%)<br>bbox: [111, 147, 436, 369]<br>mean abs RGB: 1.3273<br>RMSE RGB: 16.1954<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic56_alternating_row_colors</b><br>Page 1</td>
  <td><img src="images/classic56_alternating_row_colors_p1_heatmap.png" width="760" alt="classic56_alternating_row_colors page 1 difference heatmap"></td>
  <td>changed: 73275 px (3.58%)<br>bbox: [105, 147, 428, 491]<br>mean abs RGB: 1.9655<br>RMSE RGB: 16.4271<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic57_cjk_only</b><br>Page 1</td>
  <td><img src="images/classic57_cjk_only_p1_heatmap.png" width="760" alt="classic57_cjk_only page 1 difference heatmap"></td>
  <td>changed: 16717 px (0.82%)<br>bbox: [112, 144, 527, 334]<br>mean abs RGB: 1.1131<br>RMSE RGB: 13.9907<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic58_mixed_numeric_formats</b><br>Page 1</td>
  <td><img src="images/classic58_mixed_numeric_formats_p1_heatmap.png" width="760" alt="classic58_mixed_numeric_formats page 1 difference heatmap"></td>
  <td>changed: 18590 px (0.91%)<br>bbox: [111, 147, 318, 462]<br>mean abs RGB: 1.4048<br>RMSE RGB: 16.6326<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic59_multi_sheet_summary</b><br>Page 1</td>
  <td><img src="images/classic59_multi_sheet_summary_p1_heatmap.png" width="760" alt="classic59_multi_sheet_summary page 1 difference heatmap"></td>
  <td>changed: 12421 px (0.61%)<br>bbox: [113, 147, 422, 333]<br>mean abs RGB: 0.9527<br>RMSE RGB: 13.738<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic60_large_wide_table</b><br>Page 1</td>
  <td><img src="images/classic60_large_wide_table_p1_heatmap.png" width="760" alt="classic60_large_wide_table page 1 difference heatmap"></td>
  <td>changed: 423789 px (20.70%)<br>bbox: [112, 147, 1046, 1521]<br>mean abs RGB: 32.1581<br>RMSE RGB: 79.7419<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic61_product_card_with_image</b><br>Page 1</td>
  <td><img src="images/classic61_product_card_with_image_p1_heatmap.png" width="760" alt="classic61_product_card_with_image page 1 difference heatmap"></td>
  <td>changed: 13484 px (0.66%)<br>bbox: [105, 141, 588, 444]<br>mean abs RGB: 0.9628<br>RMSE RGB: 13.4412<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic62_company_logo_header</b><br>Page 1</td>
  <td><img src="images/classic62_company_logo_header_p1_heatmap.png" width="760" alt="classic62_company_logo_header page 1 difference heatmap"></td>
  <td>changed: 23193 px (1.13%)<br>bbox: [105, 141, 632, 431]<br>mean abs RGB: 1.8408<br>RMSE RGB: 19.3011<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic63_two_products_side_by_side</b><br>Page 1</td>
  <td><img src="images/classic63_two_products_side_by_side_p1_heatmap.png" width="760" alt="classic63_two_products_side_by_side page 1 difference heatmap"></td>
  <td>changed: 23769 px (1.16%)<br>bbox: [105, 141, 567, 381]<br>mean abs RGB: 1.731<br>RMSE RGB: 18.3611<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic64_employee_directory_with_photo</b><br>Page 1</td>
  <td><img src="images/classic64_employee_directory_with_photo_p1_heatmap.png" width="760" alt="classic64_employee_directory_with_photo page 1 difference heatmap"></td>
  <td>changed: 36631 px (1.79%)<br>bbox: [105, 142, 727, 406]<br>mean abs RGB: 2.594<br>RMSE RGB: 22.0068<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic65_inventory_with_product_photos</b><br>Page 1</td>
  <td><img src="images/classic65_inventory_with_product_photos_p1_heatmap.png" width="760" alt="classic65_inventory_with_product_photos page 1 difference heatmap"></td>
  <td>changed: 36414 px (1.78%)<br>bbox: [105, 148, 631, 556]<br>mean abs RGB: 2.5662<br>RMSE RGB: 21.9389<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic66_invoice_with_logo</b><br>Page 1</td>
  <td><img src="images/classic66_invoice_with_logo_p1_heatmap.png" width="760" alt="classic66_invoice_with_logo page 1 difference heatmap"></td>
  <td>changed: 27077 px (1.32%)<br>bbox: [105, 141, 660, 515]<br>mean abs RGB: 2.1967<br>RMSE RGB: 21.1487<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic67_real_estate_listing</b><br>Page 1</td>
  <td><img src="images/classic67_real_estate_listing_p1_heatmap.png" width="760" alt="classic67_real_estate_listing page 1 difference heatmap"></td>
  <td>changed: 26658 px (1.30%)<br>bbox: [105, 141, 640, 417]<br>mean abs RGB: 1.7143<br>RMSE RGB: 17.0747<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic68_restaurant_menu</b><br>Page 1</td>
  <td><img src="images/classic68_restaurant_menu_p1_heatmap.png" width="760" alt="classic68_restaurant_menu page 1 difference heatmap"></td>
  <td>changed: 85832 px (4.19%)<br>bbox: [111, 149, 656, 885]<br>mean abs RGB: 4.6404<br>RMSE RGB: 27.5185<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic69_image_only_sheet</b><br>Page 1</td>
  <td><img src="images/classic69_image_only_sheet_p1_heatmap.png" width="760" alt="classic69_image_only_sheet page 1 difference heatmap"></td>
  <td>changed: 22358 px (1.09%)<br>bbox: [105, 141, 582, 460]<br>mean abs RGB: 1.5403<br>RMSE RGB: 16.1038<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic70_product_catalog_with_images</b><br>Page 1</td>
  <td><img src="images/classic70_product_catalog_with_images_p1_heatmap.png" width="760" alt="classic70_product_catalog_with_images page 1 difference heatmap"></td>
  <td>changed: 55574 px (2.71%)<br>bbox: [105, 153, 542, 866]<br>mean abs RGB: 4.1183<br>RMSE RGB: 27.107<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic71_multi_sheet_with_images</b><br>Page 1</td>
  <td><img src="images/classic71_multi_sheet_with_images_p1_heatmap.png" width="760" alt="classic71_multi_sheet_with_images page 1 difference heatmap"></td>
  <td>changed: 8763 px (0.43%)<br>bbox: [105, 141, 318, 319]<br>mean abs RGB: 0.6616<br>RMSE RGB: 11.2412<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic72_bar_chart_image_with_data</b><br>Page 1</td>
  <td><img src="images/classic72_bar_chart_image_with_data_p1_heatmap.png" width="760" alt="classic72_bar_chart_image_with_data page 1 difference heatmap"></td>
  <td>changed: 19587 px (0.96%)<br>bbox: [111, 149, 423, 427]<br>mean abs RGB: 1.89<br>RMSE RGB: 17.7494<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic73_event_flyer_with_banner</b><br>Page 1</td>
  <td><img src="images/classic73_event_flyer_with_banner_p1_heatmap.png" width="760" alt="classic73_event_flyer_with_banner page 1 difference heatmap"></td>
  <td>changed: 44615 px (2.18%)<br>bbox: [105, 141, 582, 807]<br>mean abs RGB: 3.538<br>RMSE RGB: 26.387<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic74_dashboard_with_kpi_image</b><br>Page 1</td>
  <td><img src="images/classic74_dashboard_with_kpi_image_p1_heatmap.png" width="760" alt="classic74_dashboard_with_kpi_image page 1 difference heatmap"></td>
  <td>changed: 73499 px (3.59%)<br>bbox: [112, 149, 917, 365]<br>mean abs RGB: 2.07<br>RMSE RGB: 19.7436<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic75_certificate_with_seal</b><br>Page 1</td>
  <td><img src="images/classic75_certificate_with_seal_p1_heatmap.png" width="760" alt="classic75_certificate_with_seal page 1 difference heatmap"></td>
  <td>changed: 38899 px (1.90%)<br>bbox: [207, 178, 776, 372]<br>mean abs RGB: 2.9014<br>RMSE RGB: 24.1545<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic76_product_image_grid</b><br>Page 1</td>
  <td><img src="images/classic76_product_image_grid_p1_heatmap.png" width="760" alt="classic76_product_image_grid page 1 difference heatmap"></td>
  <td>changed: 48090 px (2.35%)<br>bbox: [105, 149, 683, 522]<br>mean abs RGB: 3.1222<br>RMSE RGB: 23.6526<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic77_news_article_with_hero_image</b><br>Page 1</td>
  <td><img src="images/classic77_news_article_with_hero_image_p1_heatmap.png" width="760" alt="classic77_news_article_with_hero_image page 1 difference heatmap"></td>
  <td>changed: 58241 px (2.84%)<br>bbox: [105, 141, 850, 873]<br>mean abs RGB: 4.1623<br>RMSE RGB: 27.5539<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic78_small_icon_per_row</b><br>Page 1</td>
  <td><img src="images/classic78_small_icon_per_row_p1_heatmap.png" width="760" alt="classic78_small_icon_per_row page 1 difference heatmap"></td>
  <td>changed: 25406 px (1.24%)<br>bbox: [105, 147, 534, 389]<br>mean abs RGB: 1.8643<br>RMSE RGB: 19.148<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic79_wide_panoramic_banner</b><br>Page 1</td>
  <td><img src="images/classic79_wide_panoramic_banner_p1_heatmap.png" width="760" alt="classic79_wide_panoramic_banner page 1 difference heatmap"></td>
  <td>changed: 42843 px (2.09%)<br>bbox: [105, 141, 863, 712]<br>mean abs RGB: 3.6126<br>RMSE RGB: 27.1226<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic80_portrait_tall_image</b><br>Page 1</td>
  <td><img src="images/classic80_portrait_tall_image_p1_heatmap.png" width="760" alt="classic80_portrait_tall_image page 1 difference heatmap"></td>
  <td>changed: 29175 px (1.42%)<br>bbox: [105, 141, 740, 429]<br>mean abs RGB: 1.7979<br>RMSE RGB: 18.1588<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic81_step_by_step_with_images</b><br>Page 1</td>
  <td><img src="images/classic81_step_by_step_with_images_p1_heatmap.png" width="760" alt="classic81_step_by_step_with_images page 1 difference heatmap"></td>
  <td>changed: 61980 px (3.03%)<br>bbox: [105, 149, 675, 1000]<br>mean abs RGB: 2.7227<br>RMSE RGB: 22.2239<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic82_before_after_images</b><br>Page 1</td>
  <td><img src="images/classic82_before_after_images_p1_heatmap.png" width="760" alt="classic82_before_after_images page 1 difference heatmap"></td>
  <td>changed: 45045 px (2.20%)<br>bbox: [105, 147, 660, 604]<br>mean abs RGB: 2.719<br>RMSE RGB: 20.4131<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic83_color_swatch_palette</b><br>Page 1</td>
  <td><img src="images/classic83_color_swatch_palette_p1_heatmap.png" width="760" alt="classic83_color_swatch_palette page 1 difference heatmap"></td>
  <td>changed: 51517 px (2.52%)<br>bbox: [105, 149, 611, 730]<br>mean abs RGB: 3.9584<br>RMSE RGB: 28.0205<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic84_travel_destination_cards</b><br>Page 1</td>
  <td><img src="images/classic84_travel_destination_cards_p1_heatmap.png" width="760" alt="classic84_travel_destination_cards page 1 difference heatmap"></td>
  <td>changed: 67644 px (3.30%)<br>bbox: [105, 149, 820, 918]<br>mean abs RGB: 3.9991<br>RMSE RGB: 25.2654<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic85_lab_results_with_image</b><br>Page 1</td>
  <td><img src="images/classic85_lab_results_with_image_p1_heatmap.png" width="760" alt="classic85_lab_results_with_image page 1 difference heatmap"></td>
  <td>changed: 53394 px (2.61%)<br>bbox: [112, 149, 718, 400]<br>mean abs RGB: 2.5453<br>RMSE RGB: 20.9919<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic86_software_screenshot_features</b><br>Page 1</td>
  <td><img src="images/classic86_software_screenshot_features_p1_heatmap.png" width="760" alt="classic86_software_screenshot_features page 1 difference heatmap"></td>
  <td>changed: 25135 px (1.23%)<br>bbox: [105, 141, 684, 399]<br>mean abs RGB: 1.165<br>RMSE RGB: 14.8185<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic87_sports_results_with_logos</b><br>Page 1</td>
  <td><img src="images/classic87_sports_results_with_logos_p1_heatmap.png" width="760" alt="classic87_sports_results_with_logos page 1 difference heatmap"></td>
  <td>changed: 23047 px (1.13%)<br>bbox: [105, 149, 736, 456]<br>mean abs RGB: 1.8296<br>RMSE RGB: 19.2338<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic88_image_after_data</b><br>Page 1</td>
  <td><img src="images/classic88_image_after_data_p1_heatmap.png" width="760" alt="classic88_image_after_data page 1 difference heatmap"></td>
  <td>changed: 43687 px (2.13%)<br>bbox: [105, 147, 678, 491]<br>mean abs RGB: 2.0383<br>RMSE RGB: 19.5621<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic89_nutrition_label_with_image</b><br>Page 1</td>
  <td><img src="images/classic89_nutrition_label_with_image_p1_heatmap.png" width="760" alt="classic89_nutrition_label_with_image page 1 difference heatmap"></td>
  <td>changed: 29314 px (1.43%)<br>bbox: [105, 141, 627, 525]<br>mean abs RGB: 1.9733<br>RMSE RGB: 19.2583<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic90_project_status_with_milestones</b><br>Page 1</td>
  <td><img src="images/classic90_project_status_with_milestones_p1_heatmap.png" width="760" alt="classic90_project_status_with_milestones page 1 difference heatmap"></td>
  <td>changed: 72804 px (3.56%)<br>bbox: [111, 149, 885, 430]<br>mean abs RGB: 3.6954<br>RMSE RGB: 24.8812<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic91_simple_bar_chart</b><br>Page 1</td>
  <td><img src="images/classic91_simple_bar_chart_p1_heatmap.png" width="760" alt="classic91_simple_bar_chart page 1 difference heatmap"></td>
  <td>changed: 99045 px (4.84%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 6.3242<br>RMSE RGB: 31.1594<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic92_horizontal_bar_chart</b><br>Page 1</td>
  <td><img src="images/classic92_horizontal_bar_chart_p1_heatmap.png" width="760" alt="classic92_horizontal_bar_chart page 1 difference heatmap"></td>
  <td>changed: 92488 px (4.52%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 6.0421<br>RMSE RGB: 31.0837<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic93_line_chart</b><br>Page 1</td>
  <td><img src="images/classic93_line_chart_p1_heatmap.png" width="760" alt="classic93_line_chart page 1 difference heatmap"></td>
  <td>changed: 33932 px (1.66%)<br>bbox: [111, 147, 1055, 726]<br>mean abs RGB: 2.3364<br>RMSE RGB: 20.5444<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic94_pie_chart</b><br>Page 1</td>
  <td><img src="images/classic94_pie_chart_p1_heatmap.png" width="760" alt="classic94_pie_chart page 1 difference heatmap"></td>
  <td>changed: 234669 px (11.46%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 14.3717<br>RMSE RGB: 45.7096<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic95_area_chart</b><br>Page 1</td>
  <td><img src="images/classic95_area_chart_p1_heatmap.png" width="760" alt="classic95_area_chart page 1 difference heatmap"></td>
  <td>changed: 134982 px (6.59%)<br>bbox: [112, 148, 1055, 927]<br>mean abs RGB: 8.6407<br>RMSE RGB: 36.9627<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic96_scatter_chart</b><br>Page 1</td>
  <td><img src="images/classic96_scatter_chart_p1_heatmap.png" width="760" alt="classic96_scatter_chart page 1 difference heatmap"></td>
  <td>changed: 61918 px (3.02%)<br>bbox: [111, 146, 1055, 802]<br>mean abs RGB: 4.129<br>RMSE RGB: 26.9724<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic97_doughnut_chart</b><br>Page 1</td>
  <td><img src="images/classic97_doughnut_chart_p1_heatmap.png" width="760" alt="classic97_doughnut_chart page 1 difference heatmap"></td>
  <td>changed: 233142 px (11.39%)<br>bbox: [112, 148, 1055, 726]<br>mean abs RGB: 14.1906<br>RMSE RGB: 45.0775<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic98_radar_chart</b><br>Page 1</td>
  <td><img src="images/classic98_radar_chart_p1_heatmap.png" width="760" alt="classic98_radar_chart page 1 difference heatmap"></td>
  <td>changed: 32404 px (1.58%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 2.1886<br>RMSE RGB: 19.8562<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic99_bubble_chart</b><br>Page 1</td>
  <td><img src="images/classic99_bubble_chart_p1_heatmap.png" width="760" alt="classic99_bubble_chart page 1 difference heatmap"></td>
  <td>changed: 65436 px (3.20%)<br>bbox: [113, 146, 1055, 726]<br>mean abs RGB: 4.4081<br>RMSE RGB: 27.4778<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic100_stacked_bar_chart</b><br>Page 1</td>
  <td><img src="images/classic100_stacked_bar_chart_p1_heatmap.png" width="760" alt="classic100_stacked_bar_chart page 1 difference heatmap"></td>
  <td>changed: 104071 px (5.08%)<br>bbox: [112, 148, 999, 897]<br>mean abs RGB: 6.4488<br>RMSE RGB: 31.7783<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic101_percent_stacked_bar</b><br>Page 1</td>
  <td><img src="images/classic101_percent_stacked_bar_p1_heatmap.png" width="760" alt="classic101_percent_stacked_bar page 1 difference heatmap"></td>
  <td>changed: 126091 px (6.16%)<br>bbox: [111, 147, 999, 925]<br>mean abs RGB: 7.8885<br>RMSE RGB: 35.2497<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic102_line_chart_with_markers</b><br>Page 1</td>
  <td><img src="images/classic102_line_chart_with_markers_p1_heatmap.png" width="760" alt="classic102_line_chart_with_markers page 1 difference heatmap"></td>
  <td>changed: 31806 px (1.55%)<br>bbox: [111, 147, 1055, 726]<br>mean abs RGB: 2.1538<br>RMSE RGB: 19.7951<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic103_pie_chart_with_labels</b><br>Page 1</td>
  <td><img src="images/classic103_pie_chart_with_labels_p1_heatmap.png" width="760" alt="classic103_pie_chart_with_labels page 1 difference heatmap"></td>
  <td>changed: 181129 px (8.85%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 11.2738<br>RMSE RGB: 40.9499<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic104_combo_bar_line_chart</b><br>Page 1</td>
  <td><img src="images/classic104_combo_bar_line_chart_p1_heatmap.png" width="760" alt="classic104_combo_bar_line_chart page 1 difference heatmap"></td>
  <td>changed: 89575 px (4.37%)<br>bbox: [111, 147, 1055, 726]<br>mean abs RGB: 5.5788<br>RMSE RGB: 29.2691<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic105_3d_bar_chart</b><br>Page 1</td>
  <td><img src="images/classic105_3d_bar_chart_p1_heatmap.png" width="760" alt="classic105_3d_bar_chart page 1 difference heatmap"></td>
  <td>changed: 130479 px (6.37%)<br>bbox: [111, 148, 1055, 726]<br>mean abs RGB: 8.6104<br>RMSE RGB: 37.1005<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic106_3d_pie_chart</b><br>Page 1</td>
  <td><img src="images/classic106_3d_pie_chart_p1_heatmap.png" width="760" alt="classic106_3d_pie_chart page 1 difference heatmap"></td>
  <td>changed: 144169 px (7.04%)<br>bbox: [111, 148, 1055, 726]<br>mean abs RGB: 11.0471<br>RMSE RGB: 43.7571<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic107_multi_series_line</b><br>Page 1</td>
  <td><img src="images/classic107_multi_series_line_p1_heatmap.png" width="760" alt="classic107_multi_series_line page 1 difference heatmap"></td>
  <td>changed: 83753 px (4.09%)<br>bbox: [113, 148, 1055, 805]<br>mean abs RGB: 6.1053<br>RMSE RGB: 34.1837<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic108_stacked_area_chart</b><br>Page 1</td>
  <td><img src="images/classic108_stacked_area_chart_p1_heatmap.png" width="760" alt="classic108_stacked_area_chart page 1 difference heatmap"></td>
  <td>changed: 377916 px (18.46%)<br>bbox: [111, 147, 1055, 954]<br>mean abs RGB: 22.8459<br>RMSE RGB: 57.0348<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic109_scatter_with_trendline</b><br>Page 1</td>
  <td><img src="images/classic109_scatter_with_trendline_p1_heatmap.png" width="760" alt="classic109_scatter_with_trendline page 1 difference heatmap"></td>
  <td>changed: 53818 px (2.63%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 3.5301<br>RMSE RGB: 24.8686<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic110_chart_with_legend</b><br>Page 1</td>
  <td><img src="images/classic110_chart_with_legend_p1_heatmap.png" width="760" alt="classic110_chart_with_legend page 1 difference heatmap"></td>
  <td>changed: 89555 px (4.37%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 5.8099<br>RMSE RGB: 30.4894<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic111_chart_with_axis_labels</b><br>Page 1</td>
  <td><img src="images/classic111_chart_with_axis_labels_p1_heatmap.png" width="760" alt="classic111_chart_with_axis_labels page 1 difference heatmap"></td>
  <td>changed: 71927 px (3.51%)<br>bbox: [111, 147, 1055, 726]<br>mean abs RGB: 4.8023<br>RMSE RGB: 28.1337<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic112_multiple_charts</b><br>Page 1</td>
  <td><img src="images/classic112_multiple_charts_p1_heatmap.png" width="760" alt="classic112_multiple_charts page 1 difference heatmap"></td>
  <td>changed: 85900 px (4.20%)<br>bbox: [111, 147, 1055, 1126]<br>mean abs RGB: 5.5468<br>RMSE RGB: 29.9142<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic113_chart_sheet</b><br>Page 1</td>
  <td><img src="images/classic113_chart_sheet_p1_heatmap.png" width="760" alt="classic113_chart_sheet page 1 difference heatmap"></td>
  <td>changed: 124370 px (6.07%)<br>bbox: [112, 148, 1055, 838]<br>mean abs RGB: 7.6893<br>RMSE RGB: 34.212<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic114_chart_large_dataset</b><br>Page 1</td>
  <td><img src="images/classic114_chart_large_dataset_p1_heatmap.png" width="760" alt="classic114_chart_large_dataset page 1 difference heatmap"></td>
  <td>changed: 73430 px (3.59%)<br>bbox: [102, 147, 1055, 1521]<br>mean abs RGB: 5.3103<br>RMSE RGB: 31.8713<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic115_chart_negative_values</b><br>Page 1</td>
  <td><img src="images/classic115_chart_negative_values_p1_heatmap.png" width="760" alt="classic115_chart_negative_values page 1 difference heatmap"></td>
  <td>changed: 60802 px (2.97%)<br>bbox: [111, 146, 1055, 726]<br>mean abs RGB: 3.9346<br>RMSE RGB: 25.3915<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic116_percent_stacked_area</b><br>Page 1</td>
  <td><img src="images/classic116_percent_stacked_area_p1_heatmap.png" width="760" alt="classic116_percent_stacked_area page 1 difference heatmap"></td>
  <td>changed: 485268 px (23.70%)<br>bbox: [111, 147, 1055, 954]<br>mean abs RGB: 30.0294<br>RMSE RGB: 65.6583<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic117_stock_ohlc_chart</b><br>Page 1</td>
  <td><img src="images/classic117_stock_ohlc_chart_p1_heatmap.png" width="760" alt="classic117_stock_ohlc_chart page 1 difference heatmap"></td>
  <td>changed: 105201 px (5.14%)<br>bbox: [113, 147, 1055, 726]<br>mean abs RGB: 6.9985<br>RMSE RGB: 34.2017<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic118_bar_chart_custom_colors</b><br>Page 1</td>
  <td><img src="images/classic118_bar_chart_custom_colors_p1_heatmap.png" width="760" alt="classic118_bar_chart_custom_colors page 1 difference heatmap"></td>
  <td>changed: 85940 px (4.20%)<br>bbox: [111, 148, 1055, 726]<br>mean abs RGB: 6.322<br>RMSE RGB: 34.9559<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic119_dashboard_multi_charts</b><br>Page 1</td>
  <td><img src="images/classic119_dashboard_multi_charts_p1_heatmap.png" width="760" alt="classic119_dashboard_multi_charts page 1 difference heatmap"></td>
  <td>changed: 229885 px (11.23%)<br>bbox: [112, 149, 1055, 1021]<br>mean abs RGB: 14.5968<br>RMSE RGB: 47.2174<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic120_chart_with_date_axis</b><br>Page 1</td>
  <td><img src="images/classic120_chart_with_date_axis_p1_heatmap.png" width="760" alt="classic120_chart_with_date_axis page 1 difference heatmap"></td>
  <td>changed: 51466 px (2.51%)<br>bbox: [112, 147, 1055, 726]<br>mean abs RGB: 3.5991<br>RMSE RGB: 25.8217<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic121_thin_borders</b><br>Page 1</td>
  <td><img src="images/classic121_thin_borders_p1_heatmap.png" width="760" alt="classic121_thin_borders page 1 difference heatmap"></td>
  <td>changed: 31362 px (1.53%)<br>bbox: [104, 140, 533, 337]<br>mean abs RGB: 2.3986<br>RMSE RGB: 21.8591<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic122_thick_outer_thin_inner</b><br>Page 1</td>
  <td><img src="images/classic122_thick_outer_thin_inner_p1_heatmap.png" width="760" alt="classic122_thick_outer_thin_inner page 1 difference heatmap"></td>
  <td>changed: 36528 px (1.78%)<br>bbox: [104, 139, 535, 335]<br>mean abs RGB: 3.005<br>RMSE RGB: 24.9519<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic123_dashed_borders</b><br>Page 1</td>
  <td><img src="images/classic123_dashed_borders_p1_heatmap.png" width="760" alt="classic123_dashed_borders page 1 difference heatmap"></td>
  <td>changed: 19294 px (0.94%)<br>bbox: [112, 147, 346, 335]<br>mean abs RGB: 1.4424<br>RMSE RGB: 16.8021<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic124_colored_borders</b><br>Page 1</td>
  <td><img src="images/classic124_colored_borders_p1_heatmap.png" width="760" alt="classic124_colored_borders page 1 difference heatmap"></td>
  <td>changed: 33398 px (1.63%)<br>bbox: [111, 147, 545, 368]<br>mean abs RGB: 2.4047<br>RMSE RGB: 21.693<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic125_solid_fills</b><br>Page 1</td>
  <td><img src="images/classic125_solid_fills_p1_heatmap.png" width="760" alt="classic125_solid_fills page 1 difference heatmap"></td>
  <td>changed: 37537 px (1.83%)<br>bbox: [112, 147, 332, 431]<br>mean abs RGB: 1.864<br>RMSE RGB: 18.1956<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic126_dark_header</b><br>Page 1</td>
  <td><img src="images/classic126_dark_header_p1_heatmap.png" width="760" alt="classic126_dark_header page 1 difference heatmap"></td>
  <td>changed: 32517 px (1.59%)<br>bbox: [105, 142, 539, 333]<br>mean abs RGB: 2.3668<br>RMSE RGB: 21.1254<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic127_font_styles</b><br>Page 1</td>
  <td><img src="images/classic127_font_styles_p1_heatmap.png" width="760" alt="classic127_font_styles page 1 difference heatmap"></td>
  <td>changed: 35184 px (1.72%)<br>bbox: [112, 147, 504, 430]<br>mean abs RGB: 2.6236<br>RMSE RGB: 22.9308<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic128_font_sizes</b><br>Page 1</td>
  <td><img src="images/classic128_font_sizes_p1_heatmap.png" width="760" alt="classic128_font_sizes page 1 difference heatmap"></td>
  <td>changed: 24835 px (1.21%)<br>bbox: [112, 147, 465, 535]<br>mean abs RGB: 2.0679<br>RMSE RGB: 20.8601<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic129_alignment_combos</b><br>Page 1</td>
  <td><img src="images/classic129_alignment_combos_p1_heatmap.png" width="760" alt="classic129_alignment_combos page 1 difference heatmap"></td>
  <td>changed: 18346 px (0.90%)<br>bbox: [111, 147, 849, 431]<br>mean abs RGB: 1.4584<br>RMSE RGB: 17.2261<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic130_wrap_and_indent</b><br>Page 1</td>
  <td><img src="images/classic130_wrap_and_indent_p1_heatmap.png" width="760" alt="classic130_wrap_and_indent page 1 difference heatmap"></td>
  <td>changed: 21640 px (1.06%)<br>bbox: [111, 147, 846, 438]<br>mean abs RGB: 1.6676<br>RMSE RGB: 18.2789<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic131_number_formats</b><br>Page 1</td>
  <td><img src="images/classic131_number_formats_p1_heatmap.png" width="760" alt="classic131_number_formats page 1 difference heatmap"></td>
  <td>changed: 34409 px (1.68%)<br>bbox: [111, 147, 838, 490]<br>mean abs RGB: 2.6294<br>RMSE RGB: 22.8385<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic132_striped_table</b><br>Page 1</td>
  <td><img src="images/classic132_striped_table_p1_heatmap.png" width="760" alt="classic132_striped_table page 1 difference heatmap"></td>
  <td>changed: 105890 px (5.17%)<br>bbox: [105, 141, 533, 492]<br>mean abs RGB: 3.3992<br>RMSE RGB: 22.243<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic133_gradient_rows</b><br>Page 1</td>
  <td><img src="images/classic133_gradient_rows_p1_heatmap.png" width="760" alt="classic133_gradient_rows page 1 difference heatmap"></td>
  <td>changed: 85332 px (4.17%)<br>bbox: [105, 147, 467, 491]<br>mean abs RGB: 3.4788<br>RMSE RGB: 26.806<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic134_heatmap</b><br>Page 1</td>
  <td><img src="images/classic134_heatmap_p1_heatmap.png" width="760" alt="classic134_heatmap page 1 difference heatmap"></td>
  <td>changed: 121381 px (5.93%)<br>bbox: [112, 147, 846, 397]<br>mean abs RGB: 5.9544<br>RMSE RGB: 31.8203<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic135_bottom_border_only</b><br>Page 1</td>
  <td><img src="images/classic135_bottom_border_only_p1_heatmap.png" width="760" alt="classic135_bottom_border_only page 1 difference heatmap"></td>
  <td>changed: 19777 px (0.97%)<br>bbox: [104, 146, 558, 335]<br>mean abs RGB: 1.5539<br>RMSE RGB: 17.744<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic136_financial_report_styled</b><br>Page 1</td>
  <td><img src="images/classic136_financial_report_styled_p1_heatmap.png" width="760" alt="classic136_financial_report_styled page 1 difference heatmap"></td>
  <td>changed: 75481 px (3.69%)<br>bbox: [104, 141, 761, 460]<br>mean abs RGB: 4.706<br>RMSE RGB: 28.8017<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic137_checkerboard</b><br>Page 1</td>
  <td><img src="images/classic137_checkerboard_p1_heatmap.png" width="760" alt="classic137_checkerboard page 1 difference heatmap"></td>
  <td>changed: 105198 px (5.14%)<br>bbox: [105, 143, 584, 564]<br>mean abs RGB: 7.9533<br>RMSE RGB: 37.982<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic138_color_grid</b><br>Page 1</td>
  <td><img src="images/classic138_color_grid_p1_heatmap.png" width="760" alt="classic138_color_grid page 1 difference heatmap"></td>
  <td>changed: 54582 px (2.67%)<br>bbox: [105, 143, 608, 397]<br>mean abs RGB: 2.0726<br>RMSE RGB: 16.6136<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic139_pattern_fills</b><br>Page 1</td>
  <td><img src="images/classic139_pattern_fills_p1_heatmap.png" width="760" alt="classic139_pattern_fills page 1 difference heatmap"></td>
  <td>changed: 71955 px (3.51%)<br>bbox: [112, 147, 584, 556]<br>mean abs RGB: 3.63<br>RMSE RGB: 24.6588<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic140_rotated_text</b><br>Page 1</td>
  <td><img src="images/classic140_rotated_text_p1_heatmap.png" width="760" alt="classic140_rotated_text page 1 difference heatmap"></td>
  <td>changed: 18656 px (0.91%)<br>bbox: [112, 148, 421, 1094]<br>mean abs RGB: 1.4526<br>RMSE RGB: 17.1075<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic141_mixed_edge_borders</b><br>Page 1</td>
  <td><img src="images/classic141_mixed_edge_borders_p1_heatmap.png" width="760" alt="classic141_mixed_edge_borders page 1 difference heatmap"></td>
  <td>changed: 32189 px (1.57%)<br>bbox: [111, 147, 705, 493]<br>mean abs RGB: 2.3031<br>RMSE RGB: 21.5882<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic142_styled_invoice</b><br>Page 1</td>
  <td><img src="images/classic142_styled_invoice_p1_heatmap.png" width="760" alt="classic142_styled_invoice page 1 difference heatmap"></td>
  <td>changed: 91155 px (4.45%)<br>bbox: [104, 143, 914, 585]<br>mean abs RGB: 4.5147<br>RMSE RGB: 27.2671<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic143_colored_tabs</b><br>Page 1</td>
  <td><img src="images/classic143_colored_tabs_p1_heatmap.png" width="760" alt="classic143_colored_tabs page 1 difference heatmap"></td>
  <td>changed: 5224 px (0.26%)<br>bbox: [112, 147, 318, 240]<br>mean abs RGB: 0.4216<br>RMSE RGB: 9.3013<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic144_note_style_cells</b><br>Page 1</td>
  <td><img src="images/classic144_note_style_cells_p1_heatmap.png" width="760" alt="classic144_note_style_cells page 1 difference heatmap"></td>
  <td>changed: 48102 px (2.35%)<br>bbox: [111, 147, 761, 335]<br>mean abs RGB: 1.6431<br>RMSE RGB: 15.4884<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic145_status_badges</b><br>Page 1</td>
  <td><img src="images/classic145_status_badges_p1_heatmap.png" width="760" alt="classic145_status_badges page 1 difference heatmap"></td>
  <td>changed: 90177 px (4.40%)<br>bbox: [105, 141, 855, 400]<br>mean abs RGB: 4.055<br>RMSE RGB: 24.833<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic146_double_border_table</b><br>Page 1</td>
  <td><img src="images/classic146_double_border_table_p1_heatmap.png" width="760" alt="classic146_double_border_table page 1 difference heatmap"></td>
  <td>changed: 42056 px (2.05%)<br>bbox: [104, 139, 681, 306]<br>mean abs RGB: 2.9355<br>RMSE RGB: 23.745<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic147_multi_sheet_styled</b><br>Page 1</td>
  <td><img src="images/classic147_multi_sheet_styled_p1_heatmap.png" width="760" alt="classic147_multi_sheet_styled page 1 difference heatmap"></td>
  <td>changed: 25682 px (1.25%)<br>bbox: [104, 141, 491, 306]<br>mean abs RGB: 1.4906<br>RMSE RGB: 15.3599<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic148_frozen_styled_grid</b><br>Page 1</td>
  <td><img src="images/classic148_frozen_styled_grid_p1_heatmap.png" width="760" alt="classic148_frozen_styled_grid page 1 difference heatmap"></td>
  <td>changed: 444737 px (21.72%)<br>bbox: [104, 141, 1102, 804]<br>mean abs RGB: 13.5561<br>RMSE RGB: 41.3612<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic149_merged_styled_sections</b><br>Page 1</td>
  <td><img src="images/classic149_merged_styled_sections_p1_heatmap.png" width="760" alt="classic149_merged_styled_sections page 1 difference heatmap"></td>
  <td>changed: 107607 px (5.26%)<br>bbox: [104, 142, 867, 555]<br>mean abs RGB: 6.611<br>RMSE RGB: 34.0673<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic150_kitchen_sink_styles</b><br>Page 1</td>
  <td><img src="images/classic150_kitchen_sink_styles_p1_heatmap.png" width="760" alt="classic150_kitchen_sink_styles page 1 difference heatmap"></td>
  <td>changed: 89056 px (4.35%)<br>bbox: [104, 142, 996, 619]<br>mean abs RGB: 5.7815<br>RMSE RGB: 32.5348<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic151_multilingual_greetings</b><br>Page 1</td>
  <td><img src="images/classic151_multilingual_greetings_p1_heatmap.png" width="760" alt="classic151_multilingual_greetings page 1 difference heatmap"></td>
  <td>changed: 36525 px (1.78%)<br>bbox: [111, 147, 979, 555]<br>mean abs RGB: 2.6805<br>RMSE RGB: 22.7382<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic152_emoji_sampler</b><br>Page 1</td>
  <td><img src="images/classic152_emoji_sampler_p1_heatmap.png" width="760" alt="classic152_emoji_sampler page 1 difference heatmap"></td>
  <td>changed: 29261 px (1.43%)<br>bbox: [111, 148, 484, 431]<br>mean abs RGB: 2.1364<br>RMSE RGB: 20.2875<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic153_currency_symbols</b><br>Page 1</td>
  <td><img src="images/classic153_currency_symbols_p1_heatmap.png" width="760" alt="classic153_currency_symbols page 1 difference heatmap"></td>
  <td>changed: 35023 px (1.71%)<br>bbox: [111, 147, 569, 552]<br>mean abs RGB: 2.6227<br>RMSE RGB: 22.7056<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic154_math_symbols</b><br>Page 1</td>
  <td><img src="images/classic154_math_symbols_p1_heatmap.png" width="760" alt="classic154_math_symbols page 1 difference heatmap"></td>
  <td>changed: 24534 px (1.20%)<br>bbox: [111, 147, 624, 462]<br>mean abs RGB: 1.7595<br>RMSE RGB: 18.2852<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic155_diacritical_marks</b><br>Page 1</td>
  <td><img src="images/classic155_diacritical_marks_p1_heatmap.png" width="760" alt="classic155_diacritical_marks page 1 difference heatmap"></td>
  <td>changed: 19169 px (0.94%)<br>bbox: [111, 147, 477, 494]<br>mean abs RGB: 1.4171<br>RMSE RGB: 16.5804<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic156_rtl_bidi_text</b><br>Page 1</td>
  <td><img src="images/classic156_rtl_bidi_text_p1_heatmap.png" width="760" alt="classic156_rtl_bidi_text page 1 difference heatmap"></td>
  <td>changed: 8214 px (0.40%)<br>bbox: [111, 148, 803, 306]<br>mean abs RGB: 0.6226<br>RMSE RGB: 11.0674<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic157_cjk_extended</b><br>Page 1</td>
  <td><img src="images/classic157_cjk_extended_p1_heatmap.png" width="760" alt="classic157_cjk_extended page 1 difference heatmap"></td>
  <td>changed: 42908 px (2.10%)<br>bbox: [111, 148, 975, 396]<br>mean abs RGB: 2.995<br>RMSE RGB: 23.5807<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic158_emoji_skin_tones</b><br>Page 1</td>
  <td><img src="images/classic158_emoji_skin_tones_p1_heatmap.png" width="760" alt="classic158_emoji_skin_tones page 1 difference heatmap"></td>
  <td>changed: 23901 px (1.17%)<br>bbox: [111, 147, 546, 337]<br>mean abs RGB: 1.8132<br>RMSE RGB: 18.911<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic159_zwj_emoji</b><br>Page 1</td>
  <td><img src="images/classic159_zwj_emoji_p1_heatmap.png" width="760" alt="classic159_zwj_emoji page 1 difference heatmap"></td>
  <td>changed: 25408 px (1.24%)<br>bbox: [111, 148, 477, 462]<br>mean abs RGB: 1.9815<br>RMSE RGB: 19.9507<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic160_punctuation_marks</b><br>Page 1</td>
  <td><img src="images/classic160_punctuation_marks_p1_heatmap.png" width="760" alt="classic160_punctuation_marks page 1 difference heatmap"></td>
  <td>changed: 16387 px (0.80%)<br>bbox: [111, 147, 844, 400]<br>mean abs RGB: 1.1952<br>RMSE RGB: 15.2018<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic161_box_drawing</b><br>Page 1</td>
  <td><img src="images/classic161_box_drawing_p1_heatmap.png" width="760" alt="classic161_box_drawing page 1 difference heatmap"></td>
  <td>changed: 28732 px (1.40%)<br>bbox: [111, 147, 860, 365]<br>mean abs RGB: 2.2859<br>RMSE RGB: 21.6718<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic162_cjk_emoji_styled</b><br>Page 1</td>
  <td><img src="images/classic162_cjk_emoji_styled_p1_heatmap.png" width="760" alt="classic162_cjk_emoji_styled page 1 difference heatmap"></td>
  <td>changed: 24175 px (1.18%)<br>bbox: [105, 142, 678, 305]<br>mean abs RGB: 1.7236<br>RMSE RGB: 18.4973<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic163_cyrillic_alphabets</b><br>Page 1</td>
  <td><img src="images/classic163_cyrillic_alphabets_p1_heatmap.png" width="760" alt="classic163_cyrillic_alphabets page 1 difference heatmap"></td>
  <td>changed: 33066 px (1.61%)<br>bbox: [112, 147, 878, 337]<br>mean abs RGB: 2.5004<br>RMSE RGB: 22.2145<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic164_indic_scripts</b><br>Page 1</td>
  <td><img src="images/classic164_indic_scripts_p1_heatmap.png" width="760" alt="classic164_indic_scripts page 1 difference heatmap"></td>
  <td>changed: 12694 px (0.62%)<br>bbox: [111, 147, 427, 336]<br>mean abs RGB: 0.9678<br>RMSE RGB: 13.8327<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic165_southeast_asian</b><br>Page 1</td>
  <td><img src="images/classic165_southeast_asian_p1_heatmap.png" width="760" alt="classic165_southeast_asian page 1 difference heatmap"></td>
  <td>changed: 23533 px (1.15%)<br>bbox: [111, 147, 678, 333]<br>mean abs RGB: 1.7394<br>RMSE RGB: 18.3387<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic166_emoji_progress</b><br>Page 1</td>
  <td><img src="images/classic166_emoji_progress_p1_heatmap.png" width="760" alt="classic166_emoji_progress page 1 difference heatmap"></td>
  <td>changed: 51841 px (2.53%)<br>bbox: [111, 147, 818, 369]<br>mean abs RGB: 3.4728<br>RMSE RGB: 24.8736<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic167_musical_symbols</b><br>Page 1</td>
  <td><img src="images/classic167_musical_symbols_p1_heatmap.png" width="760" alt="classic167_musical_symbols page 1 difference heatmap"></td>
  <td>changed: 19677 px (0.96%)<br>bbox: [112, 147, 752, 335]<br>mean abs RGB: 1.4701<br>RMSE RGB: 16.8985<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic168_mixed_ltr_rtl_styled</b><br>Page 1</td>
  <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_heatmap.png" width="760" alt="classic168_mixed_ltr_rtl_styled page 1 difference heatmap"></td>
  <td>changed: 40679 px (1.99%)<br>bbox: [104, 141, 797, 306]<br>mean abs RGB: 2.1902<br>RMSE RGB: 18.1711<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic169_korean_invoice</b><br>Page 1</td>
  <td><img src="images/classic169_korean_invoice_p1_heatmap.png" width="760" alt="classic169_korean_invoice page 1 difference heatmap"></td>
  <td>changed: 38305 px (1.87%)<br>bbox: [114, 144, 868, 461]<br>mean abs RGB: 3.0081<br>RMSE RGB: 24.5808<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic170_emoji_dashboard</b><br>Page 1</td>
  <td><img src="images/classic170_emoji_dashboard_p1_heatmap.png" width="760" alt="classic170_emoji_dashboard page 1 difference heatmap"></td>
  <td>changed: 43248 px (2.11%)<br>bbox: [114, 147, 671, 369]<br>mean abs RGB: 2.5969<br>RMSE RGB: 21.2394<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic171_ipa_phonetic</b><br>Page 1</td>
  <td><img src="images/classic171_ipa_phonetic_p1_heatmap.png" width="760" alt="classic171_ipa_phonetic page 1 difference heatmap"></td>
  <td>changed: 23927 px (1.17%)<br>bbox: [111, 147, 721, 401]<br>mean abs RGB: 1.7986<br>RMSE RGB: 18.7736<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic172_emoji_timeline</b><br>Page 1</td>
  <td><img src="images/classic172_emoji_timeline_p1_heatmap.png" width="760" alt="classic172_emoji_timeline page 1 difference heatmap"></td>
  <td>changed: 38546 px (1.88%)<br>bbox: [112, 147, 704, 430]<br>mean abs RGB: 3.0153<br>RMSE RGB: 24.6538<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic173_african_languages</b><br>Page 1</td>
  <td><img src="images/classic173_african_languages_p1_heatmap.png" width="760" alt="classic173_african_languages page 1 difference heatmap"></td>
  <td>changed: 30644 px (1.50%)<br>bbox: [111, 148, 830, 400]<br>mean abs RGB: 2.2968<br>RMSE RGB: 21.2345<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic174_technical_symbols</b><br>Page 1</td>
  <td><img src="images/classic174_technical_symbols_p1_heatmap.png" width="760" alt="classic174_technical_symbols page 1 difference heatmap"></td>
  <td>changed: 35365 px (1.73%)<br>bbox: [111, 146, 804, 427]<br>mean abs RGB: 2.6238<br>RMSE RGB: 22.6056<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic175_multiscript_catalog</b><br>Page 1</td>
  <td><img src="images/classic175_multiscript_catalog_p1_heatmap.png" width="760" alt="classic175_multiscript_catalog page 1 difference heatmap"></td>
  <td>changed: 43965 px (2.15%)<br>bbox: [105, 142, 1007, 431]<br>mean abs RGB: 2.9003<br>RMSE RGB: 23.0787<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic176_combining_characters</b><br>Page 1</td>
  <td><img src="images/classic176_combining_characters_p1_heatmap.png" width="760" alt="classic176_combining_characters page 1 difference heatmap"></td>
  <td>changed: 25252 px (1.23%)<br>bbox: [111, 147, 752, 369]<br>mean abs RGB: 1.8773<br>RMSE RGB: 19.0942<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic177_emoji_calendar</b><br>Page 1</td>
  <td><img src="images/classic177_emoji_calendar_p1_heatmap.png" width="760" alt="classic177_emoji_calendar page 1 difference heatmap"></td>
  <td>changed: 38638 px (1.89%)<br>bbox: [111, 147, 550, 556]<br>mean abs RGB: 2.9327<br>RMSE RGB: 24.1012<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic178_caucasus_ethiopic</b><br>Page 1</td>
  <td><img src="images/classic178_caucasus_ethiopic_p1_heatmap.png" width="760" alt="classic178_caucasus_ethiopic page 1 difference heatmap"></td>
  <td>changed: 29274 px (1.43%)<br>bbox: [111, 147, 718, 336]<br>mean abs RGB: 2.2142<br>RMSE RGB: 20.8948<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic179_emoji_inventory</b><br>Page 1</td>
  <td><img src="images/classic179_emoji_inventory_p1_heatmap.png" width="760" alt="classic179_emoji_inventory page 1 difference heatmap"></td>
  <td>changed: 49438 px (2.41%)<br>bbox: [105, 143, 843, 494]<br>mean abs RGB: 3.3313<br>RMSE RGB: 24.5135<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic180_polyglot_paragraph</b><br>Page 1</td>
  <td><img src="images/classic180_polyglot_paragraph_p1_heatmap.png" width="760" alt="classic180_polyglot_paragraph page 1 difference heatmap"></td>
  <td>changed: 24983 px (1.22%)<br>bbox: [111, 148, 667, 431]<br>mean abs RGB: 1.8091<br>RMSE RGB: 18.5847<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic181_feedback_tracker_with_images</b><br>Page 1</td>
  <td><img src="images/classic181_feedback_tracker_with_images_p1_heatmap.png" width="760" alt="classic181_feedback_tracker_with_images page 1 difference heatmap"></td>
  <td>changed: 73407 px (3.58%)<br>bbox: [105, 143, 972, 1381]<br>mean abs RGB: 5.6223<br>RMSE RGB: 33.024<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic182_dense_long_text_columns</b><br>Page 1</td>
  <td><img src="images/classic182_dense_long_text_columns_p1_heatmap.png" width="760" alt="classic182_dense_long_text_columns page 1 difference heatmap"></td>
  <td>changed: 65369 px (3.19%)<br>bbox: [112, 147, 920, 431]<br>mean abs RGB: 4.9586<br>RMSE RGB: 31.363<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic183_mixed_content_grid</b><br>Page 1</td>
  <td><img src="images/classic183_mixed_content_grid_p1_heatmap.png" width="760" alt="classic183_mixed_content_grid page 1 difference heatmap"></td>
  <td>changed: 77107 px (3.77%)<br>bbox: [112, 147, 990, 631]<br>mean abs RGB: 5.2362<br>RMSE RGB: 30.2713<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic184_wide_narrow_columns</b><br>Page 1</td>
  <td><img src="images/classic184_wide_narrow_columns_p1_heatmap.png" width="760" alt="classic184_wide_narrow_columns page 1 difference heatmap"></td>
  <td>changed: 143165 px (6.99%)<br>bbox: [105, 143, 937, 802]<br>mean abs RGB: 10.7565<br>RMSE RGB: 45.6956<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic185_tall_rows_vertical_align</b><br>Page 1</td>
  <td><img src="images/classic185_tall_rows_vertical_align_p1_heatmap.png" width="760" alt="classic185_tall_rows_vertical_align page 1 difference heatmap"></td>
  <td>changed: 36286 px (1.77%)<br>bbox: [111, 149, 1012, 617]<br>mean abs RGB: 2.8561<br>RMSE RGB: 23.9741<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic186_multi_sheet_image_report</b><br>Page 1</td>
  <td><img src="images/classic186_multi_sheet_image_report_p1_heatmap.png" width="760" alt="classic186_multi_sheet_image_report page 1 difference heatmap"></td>
  <td>changed: 16600 px (0.81%)<br>bbox: [112, 148, 620, 333]<br>mean abs RGB: 1.2878<br>RMSE RGB: 16.0869<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic187_bug_report_with_screenshots</b><br>Page 1</td>
  <td><img src="images/classic187_bug_report_with_screenshots_p1_heatmap.png" width="760" alt="classic187_bug_report_with_screenshots page 1 difference heatmap"></td>
  <td>changed: 149864 px (7.32%)<br>bbox: [105, 143, 1101, 744]<br>mean abs RGB: 10.1691<br>RMSE RGB: 42.8123<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic188_merged_header_with_images</b><br>Page 1</td>
  <td><img src="images/classic188_merged_header_with_images_p1_heatmap.png" width="760" alt="classic188_merged_header_with_images page 1 difference heatmap"></td>
  <td>changed: 65637 px (3.21%)<br>bbox: [111, 161, 891, 608]<br>mean abs RGB: 4.0059<br>RMSE RGB: 25.8629<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic189_alternating_image_text_rows</b><br>Page 1</td>
  <td><img src="images/classic189_alternating_image_text_rows_p1_heatmap.png" width="760" alt="classic189_alternating_image_text_rows page 1 difference heatmap"></td>
  <td>changed: 178381 px (8.71%)<br>bbox: [112, 147, 962, 1077]<br>mean abs RGB: 11.6418<br>RMSE RGB: 44.5364<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic190_dashboard_kpi_images</b><br>Page 1</td>
  <td><img src="images/classic190_dashboard_kpi_images_p1_heatmap.png" width="760" alt="classic190_dashboard_kpi_images page 1 difference heatmap"></td>
  <td>changed: 72141 px (3.52%)<br>bbox: [105, 178, 919, 640]<br>mean abs RGB: 5.496<br>RMSE RGB: 32.8698<br>threshold: 12, gain: 5.0</td>
</tr>
<tr>
  <td><b>classic191_payroll_calculator</b><br>Page 1</td>
  <td><img src="images/classic191_payroll_calculator_p1_heatmap.png" width="760" alt="classic191_payroll_calculator page 1 difference heatmap"></td>
  <td>changed: 205112 px (10.02%)<br>bbox: [104, 142, 1086, 509]<br>mean abs RGB: 5.4902<br>RMSE RGB: 26.6063<br>threshold: 12, gain: 5.0</td>
</tr>
</table>

## Visual Comparison

<table>
<tr><th>Rust MiniPdf</th><th>Microsoft 365 Excel Reference</th></tr>
<tr>
  <td><b>classic01_basic_table_with_headers<br><small>format: xlsx | case: classic01_basic_table_with_headers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic01_basic_table_with_headers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic01_basic_table_with_headers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic01_basic_table_with_headers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic02_multiple_worksheets<br><small>format: xlsx | case: classic02_multiple_worksheets | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic02_multiple_worksheets <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic02_multiple_worksheets_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic02_multiple_worksheets_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic03_empty_workbook<br><small>format: xlsx | case: classic03_empty_workbook | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic03_empty_workbook <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic03_empty_workbook_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic03_empty_workbook_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic04_single_cell<br><small>format: xlsx | case: classic04_single_cell | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic04_single_cell <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic04_single_cell_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic04_single_cell_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic05_wide_table<br><small>format: xlsx | case: classic05_wide_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic05_wide_table <span style="color:#3fb950">⬤</span> 97.5%</td>
</tr>
<tr>
  <td><img src="images/classic05_wide_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic05_wide_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic06_tall_table<br><small>format: xlsx | case: classic06_tall_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic06_tall_table <span style="color:#3fb950">⬤</span> 94.9%</td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic07_numbers_only<br><small>format: xlsx | case: classic07_numbers_only | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic07_numbers_only <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic07_numbers_only_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic07_numbers_only_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic08_mixed_text_and_numbers<br><small>format: xlsx | case: classic08_mixed_text_and_numbers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic08_mixed_text_and_numbers <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic08_mixed_text_and_numbers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic09_long_text<br><small>format: xlsx | case: classic09_long_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic09_long_text <span style="color:#d29922">⬤</span> 84.9%</td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic09_long_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic10_special_xml_characters<br><small>format: xlsx | case: classic10_special_xml_characters | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic10_special_xml_characters <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic10_special_xml_characters_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic10_special_xml_characters_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic11_sparse_rows<br><small>format: xlsx | case: classic11_sparse_rows | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic11_sparse_rows <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic11_sparse_rows_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic11_sparse_rows_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic12_sparse_columns<br><small>format: xlsx | case: classic12_sparse_columns | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic12_sparse_columns <span style="color:#d29922">⬤</span> 86.2%</td>
</tr>
<tr>
  <td><img src="images/classic12_sparse_columns_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic12_sparse_columns_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic13_date_strings<br><small>format: xlsx | case: classic13_date_strings | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic13_date_strings <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic14_decimal_numbers<br><small>format: xlsx | case: classic14_decimal_numbers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic14_decimal_numbers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic14_decimal_numbers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic14_decimal_numbers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic15_negative_numbers<br><small>format: xlsx | case: classic15_negative_numbers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic15_negative_numbers <span style="color:#3fb950">⬤</span> 92.8%</td>
</tr>
<tr>
  <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic16_percentage_strings<br><small>format: xlsx | case: classic16_percentage_strings | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic16_percentage_strings <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic17_currency_strings<br><small>format: xlsx | case: classic17_currency_strings | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic17_currency_strings <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic18_large_dataset<br><small>format: xlsx | case: classic18_large_dataset | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic18_large_dataset <span style="color:#d29922">⬤</span> 81.4%</td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic19_single_column_list<br><small>format: xlsx | case: classic19_single_column_list | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic19_single_column_list <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic19_single_column_list_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic19_single_column_list_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic20_all_empty_cells<br><small>format: xlsx | case: classic20_all_empty_cells | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic20_all_empty_cells <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic20_all_empty_cells_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic20_all_empty_cells_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic21_header_only<br><small>format: xlsx | case: classic21_header_only | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic21_header_only <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic21_header_only_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic21_header_only_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic22_long_sheet_name<br><small>format: xlsx | case: classic22_long_sheet_name | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic22_long_sheet_name <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic22_long_sheet_name_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic22_long_sheet_name_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic23_unicode_text<br><small>format: xlsx | case: classic23_unicode_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic23_unicode_text <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic23_unicode_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic23_unicode_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic24_red_text<br><small>format: xlsx | case: classic24_red_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic24_red_text <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic24_red_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic24_red_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic25_multiple_colors<br><small>format: xlsx | case: classic25_multiple_colors | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic25_multiple_colors <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic25_multiple_colors_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic25_multiple_colors_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic26_inline_strings<br><small>format: xlsx | case: classic26_inline_strings | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic26_inline_strings <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic27_single_row<br><small>format: xlsx | case: classic27_single_row | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic27_single_row <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic27_single_row_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic27_single_row_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic28_duplicate_values<br><small>format: xlsx | case: classic28_duplicate_values | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic28_duplicate_values <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic28_duplicate_values_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic28_duplicate_values_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic29_formula_results<br><small>format: xlsx | case: classic29_formula_results | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic29_formula_results <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic30_mixed_empty_and_filled_sheets<br><small>format: xlsx | case: classic30_mixed_empty_and_filled_sheets | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic30_mixed_empty_and_filled_sheets <span style="color:#f85149">⬤</span> 49.8%</td>
</tr>
<tr>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic31_bold_header_row<br><small>format: xlsx | case: classic31_bold_header_row | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic31_bold_header_row <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic31_bold_header_row_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic31_bold_header_row_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic32_right_aligned_numbers<br><small>format: xlsx | case: classic32_right_aligned_numbers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic32_right_aligned_numbers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic32_right_aligned_numbers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic32_right_aligned_numbers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic33_centered_text<br><small>format: xlsx | case: classic33_centered_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic33_centered_text <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic33_centered_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic33_centered_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic34_explicit_column_widths<br><small>format: xlsx | case: classic34_explicit_column_widths | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic34_explicit_column_widths <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic34_explicit_column_widths_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic34_explicit_column_widths_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic35_explicit_row_heights<br><small>format: xlsx | case: classic35_explicit_row_heights | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic35_explicit_row_heights <span style="color:#3fb950">⬤</span> 97.8%</td>
</tr>
<tr>
  <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic36_merged_cells<br><small>format: xlsx | case: classic36_merged_cells | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic36_merged_cells <span style="color:#3fb950">⬤</span> 98.3%</td>
</tr>
<tr>
  <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic37_freeze_panes<br><small>format: xlsx | case: classic37_freeze_panes | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic37_freeze_panes <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic37_freeze_panes_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic37_freeze_panes_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic38_hyperlink_cell<br><small>format: xlsx | case: classic38_hyperlink_cell | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic38_hyperlink_cell <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic38_hyperlink_cell_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic38_hyperlink_cell_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic39_financial_table<br><small>format: xlsx | case: classic39_financial_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic39_financial_table <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic40_scientific_notation<br><small>format: xlsx | case: classic40_scientific_notation | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic40_scientific_notation <span style="color:#d29922">⬤</span> 88.5%</td>
</tr>
<tr>
  <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic41_integer_vs_float<br><small>format: xlsx | case: classic41_integer_vs_float | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic41_integer_vs_float <span style="color:#3fb950">⬤</span> 97.2%</td>
</tr>
<tr>
  <td><img src="images/classic41_integer_vs_float_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic41_integer_vs_float_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic42_boolean_values<br><small>format: xlsx | case: classic42_boolean_values | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic42_boolean_values <span style="color:#3fb950">⬤</span> 92.3%</td>
</tr>
<tr>
  <td><img src="images/classic42_boolean_values_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic42_boolean_values_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic43_inventory_report<br><small>format: xlsx | case: classic43_inventory_report | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic43_inventory_report <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic44_employee_roster<br><small>format: xlsx | case: classic44_employee_roster | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic44_employee_roster <span style="color:#3fb950">⬤</span> 90.0%</td>
</tr>
<tr>
  <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic45_sales_by_region<br><small>format: xlsx | case: classic45_sales_by_region | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic45_sales_by_region <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic45_sales_by_region_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic45_sales_by_region_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic46_grade_book<br><small>format: xlsx | case: classic46_grade_book | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic46_grade_book <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic46_grade_book_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic46_grade_book_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic47_time_series<br><small>format: xlsx | case: classic47_time_series | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic47_time_series <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic48_survey_results<br><small>format: xlsx | case: classic48_survey_results | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic48_survey_results <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic49_contact_list<br><small>format: xlsx | case: classic49_contact_list | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic49_contact_list <span style="color:#d29922">⬤</span> 85.3%</td>
</tr>
<tr>
  <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic50_budget_vs_actuals<br><small>format: xlsx | case: classic50_budget_vs_actuals | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic50_budget_vs_actuals <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic51_product_catalog<br><small>format: xlsx | case: classic51_product_catalog | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic51_product_catalog <span style="color:#d29922">⬤</span> 84.0%</td>
</tr>
<tr>
  <td><img src="images/classic51_product_catalog_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic51_product_catalog_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic52_pivot_summary<br><small>format: xlsx | case: classic52_pivot_summary | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic52_pivot_summary <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic53_invoice<br><small>format: xlsx | case: classic53_invoice | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic53_invoice <span style="color:#3fb950">⬤</span> 96.8%</td>
</tr>
<tr>
  <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic54_multi_level_header<br><small>format: xlsx | case: classic54_multi_level_header | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic54_multi_level_header <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic55_error_values<br><small>format: xlsx | case: classic55_error_values | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic55_error_values <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic55_error_values_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic55_error_values_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic56_alternating_row_colors<br><small>format: xlsx | case: classic56_alternating_row_colors | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic56_alternating_row_colors <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic56_alternating_row_colors_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic56_alternating_row_colors_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic57_cjk_only<br><small>format: xlsx | case: classic57_cjk_only | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic57_cjk_only <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic58_mixed_numeric_formats<br><small>format: xlsx | case: classic58_mixed_numeric_formats | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic58_mixed_numeric_formats <span style="color:#3fb950">⬤</span> 94.9%</td>
</tr>
<tr>
  <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic59_multi_sheet_summary<br><small>format: xlsx | case: classic59_multi_sheet_summary | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic59_multi_sheet_summary <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic60_large_wide_table<br><small>format: xlsx | case: classic60_large_wide_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic60_large_wide_table <span style="color:#d29922">⬤</span> 80.7%</td>
</tr>
<tr>
  <td><img src="images/classic60_large_wide_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic60_large_wide_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic61_product_card_with_image<br><small>format: xlsx | case: classic61_product_card_with_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic61_product_card_with_image <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic61_product_card_with_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic61_product_card_with_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic62_company_logo_header<br><small>format: xlsx | case: classic62_company_logo_header | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic62_company_logo_header <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic63_two_products_side_by_side<br><small>format: xlsx | case: classic63_two_products_side_by_side | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic63_two_products_side_by_side <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic64_employee_directory_with_photo<br><small>format: xlsx | case: classic64_employee_directory_with_photo | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic64_employee_directory_with_photo <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic65_inventory_with_product_photos<br><small>format: xlsx | case: classic65_inventory_with_product_photos | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic65_inventory_with_product_photos <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic66_invoice_with_logo<br><small>format: xlsx | case: classic66_invoice_with_logo | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic66_invoice_with_logo <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic67_real_estate_listing<br><small>format: xlsx | case: classic67_real_estate_listing | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic67_real_estate_listing <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic68_restaurant_menu<br><small>format: xlsx | case: classic68_restaurant_menu | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic68_restaurant_menu <span style="color:#d29922">⬤</span> 81.5%</td>
</tr>
<tr>
  <td><img src="images/classic68_restaurant_menu_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic68_restaurant_menu_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic69_image_only_sheet<br><small>format: xlsx | case: classic69_image_only_sheet | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic69_image_only_sheet <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic69_image_only_sheet_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic69_image_only_sheet_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic70_product_catalog_with_images<br><small>format: xlsx | case: classic70_product_catalog_with_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic70_product_catalog_with_images <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic70_product_catalog_with_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic70_product_catalog_with_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic71_multi_sheet_with_images<br><small>format: xlsx | case: classic71_multi_sheet_with_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic71_multi_sheet_with_images <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic72_bar_chart_image_with_data<br><small>format: xlsx | case: classic72_bar_chart_image_with_data | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic72_bar_chart_image_with_data <span style="color:#d29922">⬤</span> 85.6%</td>
</tr>
<tr>
  <td><img src="images/classic72_bar_chart_image_with_data_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic72_bar_chart_image_with_data_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic73_event_flyer_with_banner<br><small>format: xlsx | case: classic73_event_flyer_with_banner | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic73_event_flyer_with_banner <span style="color:#3fb950">⬤</span> 95.3%</td>
</tr>
<tr>
  <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic74_dashboard_with_kpi_image<br><small>format: xlsx | case: classic74_dashboard_with_kpi_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic74_dashboard_with_kpi_image <span style="color:#3fb950">⬤</span> 90.6%</td>
</tr>
<tr>
  <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic75_certificate_with_seal<br><small>format: xlsx | case: classic75_certificate_with_seal | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic75_certificate_with_seal <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic76_product_image_grid<br><small>format: xlsx | case: classic76_product_image_grid | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic76_product_image_grid <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic77_news_article_with_hero_image<br><small>format: xlsx | case: classic77_news_article_with_hero_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic77_news_article_with_hero_image <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic78_small_icon_per_row<br><small>format: xlsx | case: classic78_small_icon_per_row | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic78_small_icon_per_row <span style="color:#3fb950">⬤</span> 96.6%</td>
</tr>
<tr>
  <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic79_wide_panoramic_banner<br><small>format: xlsx | case: classic79_wide_panoramic_banner | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic79_wide_panoramic_banner <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic80_portrait_tall_image<br><small>format: xlsx | case: classic80_portrait_tall_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic80_portrait_tall_image <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic81_step_by_step_with_images<br><small>format: xlsx | case: classic81_step_by_step_with_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic81_step_by_step_with_images <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic82_before_after_images<br><small>format: xlsx | case: classic82_before_after_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic82_before_after_images <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic83_color_swatch_palette<br><small>format: xlsx | case: classic83_color_swatch_palette | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic83_color_swatch_palette <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic84_travel_destination_cards<br><small>format: xlsx | case: classic84_travel_destination_cards | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic84_travel_destination_cards <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic85_lab_results_with_image<br><small>format: xlsx | case: classic85_lab_results_with_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic85_lab_results_with_image <span style="color:#3fb950">⬤</span> 95.2%</td>
</tr>
<tr>
  <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic86_software_screenshot_features<br><small>format: xlsx | case: classic86_software_screenshot_features | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic86_software_screenshot_features <span style="color:#3fb950">⬤</span> 98.6%</td>
</tr>
<tr>
  <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic87_sports_results_with_logos<br><small>format: xlsx | case: classic87_sports_results_with_logos | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic87_sports_results_with_logos <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic88_image_after_data<br><small>format: xlsx | case: classic88_image_after_data | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic88_image_after_data <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic89_nutrition_label_with_image<br><small>format: xlsx | case: classic89_nutrition_label_with_image | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic89_nutrition_label_with_image <span style="color:#3fb950">⬤</span> 97.3%</td>
</tr>
<tr>
  <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic90_project_status_with_milestones<br><small>format: xlsx | case: classic90_project_status_with_milestones | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic90_project_status_with_milestones <span style="color:#d29922">⬤</span> 87.5%</td>
</tr>
<tr>
  <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic91_simple_bar_chart<br><small>format: xlsx | case: classic91_simple_bar_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic91_simple_bar_chart <span style="color:#f85149">⬤</span> 68.9%</td>
</tr>
<tr>
  <td><img src="images/classic91_simple_bar_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic91_simple_bar_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic92_horizontal_bar_chart<br><small>format: xlsx | case: classic92_horizontal_bar_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic92_horizontal_bar_chart <span style="color:#f85149">⬤</span> 68.2%</td>
</tr>
<tr>
  <td><img src="images/classic92_horizontal_bar_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic92_horizontal_bar_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic93_line_chart<br><small>format: xlsx | case: classic93_line_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic93_line_chart <span style="color:#d29922">⬤</span> 75.8%</td>
</tr>
<tr>
  <td><img src="images/classic93_line_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic93_line_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic94_pie_chart<br><small>format: xlsx | case: classic94_pie_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic94_pie_chart <span style="color:#f85149">⬤</span> 56.9%</td>
</tr>
<tr>
  <td><img src="images/classic94_pie_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic94_pie_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic95_area_chart<br><small>format: xlsx | case: classic95_area_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic95_area_chart <span style="color:#d29922">⬤</span> 73.9%</td>
</tr>
<tr>
  <td><img src="images/classic95_area_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic95_area_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic96_scatter_chart<br><small>format: xlsx | case: classic96_scatter_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic96_scatter_chart <span style="color:#d29922">⬤</span> 72.8%</td>
</tr>
<tr>
  <td><img src="images/classic96_scatter_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic96_scatter_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic97_doughnut_chart<br><small>format: xlsx | case: classic97_doughnut_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic97_doughnut_chart <span style="color:#f85149">⬤</span> 60.0%</td>
</tr>
<tr>
  <td><img src="images/classic97_doughnut_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic97_doughnut_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic98_radar_chart<br><small>format: xlsx | case: classic98_radar_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic98_radar_chart <span style="color:#f85149">⬤</span> 66.1%</td>
</tr>
<tr>
  <td><img src="images/classic98_radar_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic98_radar_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic99_bubble_chart<br><small>format: xlsx | case: classic99_bubble_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic99_bubble_chart <span style="color:#d29922">⬤</span> 71.5%</td>
</tr>
<tr>
  <td><img src="images/classic99_bubble_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic99_bubble_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic100_stacked_bar_chart<br><small>format: xlsx | case: classic100_stacked_bar_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic100_stacked_bar_chart <span style="color:#d29922">⬤</span> 78.5%</td>
</tr>
<tr>
  <td><img src="images/classic100_stacked_bar_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic100_stacked_bar_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic101_percent_stacked_bar<br><small>format: xlsx | case: classic101_percent_stacked_bar | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic101_percent_stacked_bar <span style="color:#d29922">⬤</span> 78.8%</td>
</tr>
<tr>
  <td><img src="images/classic101_percent_stacked_bar_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic101_percent_stacked_bar_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic102_line_chart_with_markers<br><small>format: xlsx | case: classic102_line_chart_with_markers | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic102_line_chart_with_markers <span style="color:#d29922">⬤</span> 77.5%</td>
</tr>
<tr>
  <td><img src="images/classic102_line_chart_with_markers_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic102_line_chart_with_markers_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic103_pie_chart_with_labels<br><small>format: xlsx | case: classic103_pie_chart_with_labels | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic103_pie_chart_with_labels <span style="color:#f85149">⬤</span> 48.2%</td>
</tr>
<tr>
  <td><img src="images/classic103_pie_chart_with_labels_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic103_pie_chart_with_labels_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic104_combo_bar_line_chart<br><small>format: xlsx | case: classic104_combo_bar_line_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic104_combo_bar_line_chart <span style="color:#d29922">⬤</span> 71.2%</td>
</tr>
<tr>
  <td><img src="images/classic104_combo_bar_line_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic104_combo_bar_line_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic105_3d_bar_chart<br><small>format: xlsx | case: classic105_3d_bar_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic105_3d_bar_chart <span style="color:#f85149">⬤</span> 66.9%</td>
</tr>
<tr>
  <td><img src="images/classic105_3d_bar_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic105_3d_bar_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic106_3d_pie_chart<br><small>format: xlsx | case: classic106_3d_pie_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic106_3d_pie_chart <span style="color:#f85149">⬤</span> 63.3%</td>
</tr>
<tr>
  <td><img src="images/classic106_3d_pie_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic106_3d_pie_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic107_multi_series_line<br><small>format: xlsx | case: classic107_multi_series_line | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic107_multi_series_line <span style="color:#d29922">⬤</span> 87.3%</td>
</tr>
<tr>
  <td><img src="images/classic107_multi_series_line_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic107_multi_series_line_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic108_stacked_area_chart<br><small>format: xlsx | case: classic108_stacked_area_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic108_stacked_area_chart <span style="color:#f85149">⬤</span> 63.3%</td>
</tr>
<tr>
  <td><img src="images/classic108_stacked_area_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic108_stacked_area_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic109_scatter_with_trendline<br><small>format: xlsx | case: classic109_scatter_with_trendline | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic109_scatter_with_trendline <span style="color:#f85149">⬤</span> 68.0%</td>
</tr>
<tr>
  <td><img src="images/classic109_scatter_with_trendline_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic109_scatter_with_trendline_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic110_chart_with_legend<br><small>format: xlsx | case: classic110_chart_with_legend | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic110_chart_with_legend <span style="color:#f85149">⬤</span> 64.9%</td>
</tr>
<tr>
  <td><img src="images/classic110_chart_with_legend_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic110_chart_with_legend_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic111_chart_with_axis_labels<br><small>format: xlsx | case: classic111_chart_with_axis_labels | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic111_chart_with_axis_labels <span style="color:#f85149">⬤</span> 67.0%</td>
</tr>
<tr>
  <td><img src="images/classic111_chart_with_axis_labels_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic111_chart_with_axis_labels_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic112_multiple_charts<br><small>format: xlsx | case: classic112_multiple_charts | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic112_multiple_charts <span style="color:#d29922">⬤</span> 72.6%</td>
</tr>
<tr>
  <td><img src="images/classic112_multiple_charts_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic112_multiple_charts_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic113_chart_sheet<br><small>format: xlsx | case: classic113_chart_sheet | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic113_chart_sheet <span style="color:#f85149">⬤</span> 67.8%</td>
</tr>
<tr>
  <td><img src="images/classic113_chart_sheet_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic113_chart_sheet_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic114_chart_large_dataset<br><small>format: xlsx | case: classic114_chart_large_dataset | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic114_chart_large_dataset <span style="color:#d29922">⬤</span> 83.4%</td>
</tr>
<tr>
  <td><img src="images/classic114_chart_large_dataset_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic114_chart_large_dataset_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic115_chart_negative_values<br><small>format: xlsx | case: classic115_chart_negative_values | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic115_chart_negative_values <span style="color:#f85149">⬤</span> 66.7%</td>
</tr>
<tr>
  <td><img src="images/classic115_chart_negative_values_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic115_chart_negative_values_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic116_percent_stacked_area<br><small>format: xlsx | case: classic116_percent_stacked_area | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic116_percent_stacked_area <span style="color:#f85149">⬤</span> 61.9%</td>
</tr>
<tr>
  <td><img src="images/classic116_percent_stacked_area_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic116_percent_stacked_area_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic117_stock_ohlc_chart<br><small>format: xlsx | case: classic117_stock_ohlc_chart | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic117_stock_ohlc_chart <span style="color:#d29922">⬤</span> 80.9%</td>
</tr>
<tr>
  <td><img src="images/classic117_stock_ohlc_chart_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic117_stock_ohlc_chart_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic118_bar_chart_custom_colors<br><small>format: xlsx | case: classic118_bar_chart_custom_colors | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic118_bar_chart_custom_colors <span style="color:#f85149">⬤</span> 69.0%</td>
</tr>
<tr>
  <td><img src="images/classic118_bar_chart_custom_colors_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic118_bar_chart_custom_colors_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic119_dashboard_multi_charts<br><small>format: xlsx | case: classic119_dashboard_multi_charts | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic119_dashboard_multi_charts <span style="color:#f85149">⬤</span> 64.0%</td>
</tr>
<tr>
  <td><img src="images/classic119_dashboard_multi_charts_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic119_dashboard_multi_charts_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic120_chart_with_date_axis<br><small>format: xlsx | case: classic120_chart_with_date_axis | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic120_chart_with_date_axis <span style="color:#d29922">⬤</span> 78.2%</td>
</tr>
<tr>
  <td><img src="images/classic120_chart_with_date_axis_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic120_chart_with_date_axis_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic121_thin_borders<br><small>format: xlsx | case: classic121_thin_borders | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic121_thin_borders <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic121_thin_borders_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic121_thin_borders_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic122_thick_outer_thin_inner<br><small>format: xlsx | case: classic122_thick_outer_thin_inner | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic122_thick_outer_thin_inner <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic122_thick_outer_thin_inner_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic122_thick_outer_thin_inner_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic123_dashed_borders<br><small>format: xlsx | case: classic123_dashed_borders | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic123_dashed_borders <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic123_dashed_borders_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic123_dashed_borders_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic124_colored_borders<br><small>format: xlsx | case: classic124_colored_borders | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic124_colored_borders <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic124_colored_borders_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic124_colored_borders_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic125_solid_fills<br><small>format: xlsx | case: classic125_solid_fills | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic125_solid_fills <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic125_solid_fills_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic125_solid_fills_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic126_dark_header<br><small>format: xlsx | case: classic126_dark_header | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic126_dark_header <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic126_dark_header_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic126_dark_header_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic127_font_styles<br><small>format: xlsx | case: classic127_font_styles | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic127_font_styles <span style="color:#3fb950">⬤</span> 96.2%</td>
</tr>
<tr>
  <td><img src="images/classic127_font_styles_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic127_font_styles_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic128_font_sizes<br><small>format: xlsx | case: classic128_font_sizes | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic128_font_sizes <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic128_font_sizes_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic128_font_sizes_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic129_alignment_combos<br><small>format: xlsx | case: classic129_alignment_combos | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic129_alignment_combos <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic129_alignment_combos_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic129_alignment_combos_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic130_wrap_and_indent<br><small>format: xlsx | case: classic130_wrap_and_indent | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic130_wrap_and_indent <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic130_wrap_and_indent_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic130_wrap_and_indent_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic131_number_formats<br><small>format: xlsx | case: classic131_number_formats | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic131_number_formats <span style="color:#d29922">⬤</span> 79.3%</td>
</tr>
<tr>
  <td><img src="images/classic131_number_formats_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic131_number_formats_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic132_striped_table<br><small>format: xlsx | case: classic132_striped_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic132_striped_table <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic132_striped_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic132_striped_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic133_gradient_rows<br><small>format: xlsx | case: classic133_gradient_rows | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic133_gradient_rows <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic133_gradient_rows_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic133_gradient_rows_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic134_heatmap<br><small>format: xlsx | case: classic134_heatmap | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic134_heatmap <span style="color:#3fb950">⬤</span> 97.6%</td>
</tr>
<tr>
  <td><img src="images/classic134_heatmap_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic134_heatmap_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic135_bottom_border_only<br><small>format: xlsx | case: classic135_bottom_border_only | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic135_bottom_border_only <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic135_bottom_border_only_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic135_bottom_border_only_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic136_financial_report_styled<br><small>format: xlsx | case: classic136_financial_report_styled | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic136_financial_report_styled <span style="color:#d29922">⬤</span> 81.8%</td>
</tr>
<tr>
  <td><img src="images/classic136_financial_report_styled_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic136_financial_report_styled_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic137_checkerboard<br><small>format: xlsx | case: classic137_checkerboard | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic137_checkerboard <span style="color:#3fb950">⬤</span> 98.3%</td>
</tr>
<tr>
  <td><img src="images/classic137_checkerboard_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic137_checkerboard_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic138_color_grid<br><small>format: xlsx | case: classic138_color_grid | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic138_color_grid <span style="color:#3fb950">⬤</span> 96.5%</td>
</tr>
<tr>
  <td><img src="images/classic138_color_grid_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic138_color_grid_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic139_pattern_fills<br><small>format: xlsx | case: classic139_pattern_fills | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic139_pattern_fills <span style="color:#3fb950">⬤</span> 93.0%</td>
</tr>
<tr>
  <td><img src="images/classic139_pattern_fills_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic139_pattern_fills_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic140_rotated_text<br><small>format: xlsx | case: classic140_rotated_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic140_rotated_text <span style="color:#3fb950">⬤</span> 97.9%</td>
</tr>
<tr>
  <td><img src="images/classic140_rotated_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic140_rotated_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic141_mixed_edge_borders<br><small>format: xlsx | case: classic141_mixed_edge_borders | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic141_mixed_edge_borders <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic141_mixed_edge_borders_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic141_mixed_edge_borders_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic142_styled_invoice<br><small>format: xlsx | case: classic142_styled_invoice | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic142_styled_invoice <span style="color:#3fb950">⬤</span> 90.6%</td>
</tr>
<tr>
  <td><img src="images/classic142_styled_invoice_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic142_styled_invoice_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic143_colored_tabs<br><small>format: xlsx | case: classic143_colored_tabs | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic143_colored_tabs <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic143_colored_tabs_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic143_colored_tabs_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic144_note_style_cells<br><small>format: xlsx | case: classic144_note_style_cells | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic144_note_style_cells <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic144_note_style_cells_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic144_note_style_cells_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic145_status_badges<br><small>format: xlsx | case: classic145_status_badges | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic145_status_badges <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic145_status_badges_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic145_status_badges_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic146_double_border_table<br><small>format: xlsx | case: classic146_double_border_table | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic146_double_border_table <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic146_double_border_table_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic146_double_border_table_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic147_multi_sheet_styled<br><small>format: xlsx | case: classic147_multi_sheet_styled | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic147_multi_sheet_styled <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic147_multi_sheet_styled_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic147_multi_sheet_styled_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic148_frozen_styled_grid<br><small>format: xlsx | case: classic148_frozen_styled_grid | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic148_frozen_styled_grid <span style="color:#3fb950">⬤</span> 94.2%</td>
</tr>
<tr>
  <td><img src="images/classic148_frozen_styled_grid_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic148_frozen_styled_grid_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic149_merged_styled_sections<br><small>format: xlsx | case: classic149_merged_styled_sections | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic149_merged_styled_sections <span style="color:#3fb950">⬤</span> 94.7%</td>
</tr>
<tr>
  <td><img src="images/classic149_merged_styled_sections_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic149_merged_styled_sections_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic150_kitchen_sink_styles<br><small>format: xlsx | case: classic150_kitchen_sink_styles | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic150_kitchen_sink_styles <span style="color:#3fb950">⬤</span> 96.7%</td>
</tr>
<tr>
  <td><img src="images/classic150_kitchen_sink_styles_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic150_kitchen_sink_styles_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic151_multilingual_greetings<br><small>format: xlsx | case: classic151_multilingual_greetings | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic151_multilingual_greetings <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic151_multilingual_greetings_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic151_multilingual_greetings_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic152_emoji_sampler<br><small>format: xlsx | case: classic152_emoji_sampler | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic152_emoji_sampler <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic152_emoji_sampler_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic152_emoji_sampler_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic153_currency_symbols<br><small>format: xlsx | case: classic153_currency_symbols | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic153_currency_symbols <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic153_currency_symbols_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic153_currency_symbols_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic154_math_symbols<br><small>format: xlsx | case: classic154_math_symbols | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic154_math_symbols <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic154_math_symbols_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic154_math_symbols_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic155_diacritical_marks<br><small>format: xlsx | case: classic155_diacritical_marks | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic155_diacritical_marks <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic155_diacritical_marks_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic155_diacritical_marks_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic156_rtl_bidi_text<br><small>format: xlsx | case: classic156_rtl_bidi_text | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic156_rtl_bidi_text <span style="color:#d29922">⬤</span> 87.1%</td>
</tr>
<tr>
  <td><img src="images/classic156_rtl_bidi_text_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic156_rtl_bidi_text_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic157_cjk_extended<br><small>format: xlsx | case: classic157_cjk_extended | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic157_cjk_extended <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic157_cjk_extended_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic157_cjk_extended_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic158_emoji_skin_tones<br><small>format: xlsx | case: classic158_emoji_skin_tones | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic158_emoji_skin_tones <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic158_emoji_skin_tones_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic158_emoji_skin_tones_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic159_zwj_emoji<br><small>format: xlsx | case: classic159_zwj_emoji | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic159_zwj_emoji <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic159_zwj_emoji_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic159_zwj_emoji_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic160_punctuation_marks<br><small>format: xlsx | case: classic160_punctuation_marks | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic160_punctuation_marks <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic160_punctuation_marks_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic160_punctuation_marks_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic161_box_drawing<br><small>format: xlsx | case: classic161_box_drawing | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic161_box_drawing <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic161_box_drawing_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic161_box_drawing_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic162_cjk_emoji_styled<br><small>format: xlsx | case: classic162_cjk_emoji_styled | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic162_cjk_emoji_styled <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic162_cjk_emoji_styled_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic162_cjk_emoji_styled_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic163_cyrillic_alphabets<br><small>format: xlsx | case: classic163_cyrillic_alphabets | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic163_cyrillic_alphabets <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic163_cyrillic_alphabets_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic163_cyrillic_alphabets_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic164_indic_scripts<br><small>format: xlsx | case: classic164_indic_scripts | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic164_indic_scripts <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic164_indic_scripts_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic164_indic_scripts_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic165_southeast_asian<br><small>format: xlsx | case: classic165_southeast_asian | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic165_southeast_asian <span style="color:#d29922">⬤</span> 86.0%</td>
</tr>
<tr>
  <td><img src="images/classic165_southeast_asian_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic165_southeast_asian_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic166_emoji_progress<br><small>format: xlsx | case: classic166_emoji_progress | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic166_emoji_progress <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic166_emoji_progress_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic166_emoji_progress_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic167_musical_symbols<br><small>format: xlsx | case: classic167_musical_symbols | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic167_musical_symbols <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic167_musical_symbols_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic167_musical_symbols_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic168_mixed_ltr_rtl_styled<br><small>format: xlsx | case: classic168_mixed_ltr_rtl_styled | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic168_mixed_ltr_rtl_styled <span style="color:#3fb950">⬤</span> 96.0%</td>
</tr>
<tr>
  <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic169_korean_invoice<br><small>format: xlsx | case: classic169_korean_invoice | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic169_korean_invoice <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic169_korean_invoice_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic169_korean_invoice_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic170_emoji_dashboard<br><small>format: xlsx | case: classic170_emoji_dashboard | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic170_emoji_dashboard <span style="color:#3fb950">⬤</span> 98.6%</td>
</tr>
<tr>
  <td><img src="images/classic170_emoji_dashboard_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic170_emoji_dashboard_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic171_ipa_phonetic<br><small>format: xlsx | case: classic171_ipa_phonetic | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic171_ipa_phonetic <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic171_ipa_phonetic_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic171_ipa_phonetic_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic172_emoji_timeline<br><small>format: xlsx | case: classic172_emoji_timeline | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic172_emoji_timeline <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic172_emoji_timeline_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic172_emoji_timeline_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic173_african_languages<br><small>format: xlsx | case: classic173_african_languages | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic173_african_languages <span style="color:#3fb950">⬤</span> 94.8%</td>
</tr>
<tr>
  <td><img src="images/classic173_african_languages_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic173_african_languages_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic174_technical_symbols<br><small>format: xlsx | case: classic174_technical_symbols | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic174_technical_symbols <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic174_technical_symbols_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic174_technical_symbols_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic175_multiscript_catalog<br><small>format: xlsx | case: classic175_multiscript_catalog | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic175_multiscript_catalog <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic175_multiscript_catalog_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic175_multiscript_catalog_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic176_combining_characters<br><small>format: xlsx | case: classic176_combining_characters | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic176_combining_characters <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic176_combining_characters_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic176_combining_characters_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic177_emoji_calendar<br><small>format: xlsx | case: classic177_emoji_calendar | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic177_emoji_calendar <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic177_emoji_calendar_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic177_emoji_calendar_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic178_caucasus_ethiopic<br><small>format: xlsx | case: classic178_caucasus_ethiopic | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic178_caucasus_ethiopic <span style="color:#3fb950">⬤</span> 93.3%</td>
</tr>
<tr>
  <td><img src="images/classic178_caucasus_ethiopic_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic178_caucasus_ethiopic_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic179_emoji_inventory<br><small>format: xlsx | case: classic179_emoji_inventory | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic179_emoji_inventory <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic179_emoji_inventory_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic179_emoji_inventory_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic180_polyglot_paragraph<br><small>format: xlsx | case: classic180_polyglot_paragraph | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic180_polyglot_paragraph <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic180_polyglot_paragraph_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic180_polyglot_paragraph_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic181_feedback_tracker_with_images<br><small>format: xlsx | case: classic181_feedback_tracker_with_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic181_feedback_tracker_with_images <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic181_feedback_tracker_with_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic181_feedback_tracker_with_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic182_dense_long_text_columns<br><small>format: xlsx | case: classic182_dense_long_text_columns | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic182_dense_long_text_columns <span style="color:#3fb950">⬤</span> 98.3%</td>
</tr>
<tr>
  <td><img src="images/classic182_dense_long_text_columns_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic182_dense_long_text_columns_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic183_mixed_content_grid<br><small>format: xlsx | case: classic183_mixed_content_grid | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic183_mixed_content_grid <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic183_mixed_content_grid_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic183_mixed_content_grid_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic184_wide_narrow_columns<br><small>format: xlsx | case: classic184_wide_narrow_columns | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic184_wide_narrow_columns <span style="color:#3fb950">⬤</span> 97.9%</td>
</tr>
<tr>
  <td><img src="images/classic184_wide_narrow_columns_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic184_wide_narrow_columns_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic185_tall_rows_vertical_align<br><small>format: xlsx | case: classic185_tall_rows_vertical_align | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic185_tall_rows_vertical_align <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic185_tall_rows_vertical_align_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic185_tall_rows_vertical_align_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic186_multi_sheet_image_report<br><small>format: xlsx | case: classic186_multi_sheet_image_report | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic186_multi_sheet_image_report <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic186_multi_sheet_image_report_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic186_multi_sheet_image_report_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic187_bug_report_with_screenshots<br><small>format: xlsx | case: classic187_bug_report_with_screenshots | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic187_bug_report_with_screenshots <span style="color:#3fb950">⬤</span> 97.4%</td>
</tr>
<tr>
  <td><img src="images/classic187_bug_report_with_screenshots_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic187_bug_report_with_screenshots_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic188_merged_header_with_images<br><small>format: xlsx | case: classic188_merged_header_with_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic188_merged_header_with_images <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic188_merged_header_with_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic188_merged_header_with_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic189_alternating_image_text_rows<br><small>format: xlsx | case: classic189_alternating_image_text_rows | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic189_alternating_image_text_rows <span style="color:#3fb950">⬤</span> 91.2%</td>
</tr>
<tr>
  <td><img src="images/classic189_alternating_image_text_rows_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic189_alternating_image_text_rows_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic190_dashboard_kpi_images<br><small>format: xlsx | case: classic190_dashboard_kpi_images | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic190_dashboard_kpi_images <span style="color:#3fb950">⬤</span> 98.0%</td>
</tr>
<tr>
  <td><img src="images/classic190_dashboard_kpi_images_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic190_dashboard_kpi_images_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
<tr>
  <td><b>classic191_payroll_calculator<br><small>format: xlsx | case: classic191_payroll_calculator | scope: rust-classic-xlsx-office</small></b></td>
  <td colspan="1">classic191_payroll_calculator <span style="color:#3fb950">⬤</span> 91.9%</td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p1_minipdf.png" width="340" alt="Rust MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p1_reference.png" width="340" alt="Microsoft 365 Excel Reference"></td>
</tr>
</table>

## Detailed Results

### classic01_basic_table_with_headers

- **Case Metadata:** format: xlsx | case: classic01_basic_table_with_headers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic01_basic_table_with_headers.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9951
- **Overall Score:** 0.998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=43997 bytes, Reference=51282 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Case Metadata:** format: xlsx | case: classic02_multiple_worksheets | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic02_multiple_worksheets.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9968
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=51340 bytes, Reference=56518 bytes

Text content: ✅ Identical

### classic03_empty_workbook

- **Case Metadata:** format: xlsx | case: classic03_empty_workbook | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic03_empty_workbook.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=897 bytes, Reference=25793 bytes

Text content: ✅ Identical

### classic04_single_cell

- **Case Metadata:** format: xlsx | case: classic04_single_cell | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic04_single_cell.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9996
- **Overall Score:** 0.9998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=27862 bytes, Reference=27469 bytes

Text content: ✅ Identical

### classic05_wide_table

- **Case Metadata:** format: xlsx | case: classic05_wide_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic05_wide_table.xlsx
- **Text Similarity:** 0.9474
- **Visual Average:** 0.9911
- **Overall Score:** 0.9754
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=66485 bytes, Reference=60760 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic05_wide_table.pdf
+++ reference/classic05_wide_table.pdf
@@ -1,6 +1,6 @@
-A B C D E F G H I J

-A1 B1 C1 D1 E1 F1 G1 H1 I1 J1

-A2 B2 C2 D2 E2 F2 G2 H2 I2 J2

-A3 B3 C3 D3 E3 F3 G3 H3 I3 J3

-A4 B4 C4 D4 E4 F4 G4 H4 I4 J4

-A5 B5 C5 D5 E5 F5 G5 H5 I5 J5
+A B C D E F G H I

+A1 B1 C1 D1 E1 F1 G1 H1 I1

+A2 B2 C2 D2 E2 F2 G2 H2 I2

+A3 B3 C3 D3 E3 F3 G3 H3 I3

+A4 B4 C4 D4 E4 F4 G4 H4 I4

+A5 B5 C5 D5 E5 F5 G5 H5 I5
```
</details>

### classic06_tall_table

- **Case Metadata:** format: xlsx | case: classic06_tall_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic06_tall_table.xlsx
- **Text Similarity:** 0.9558
- **Visual Average:** 0.9163
- **Overall Score:** 0.9488
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=887040 bytes, Reference=116996 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic06_tall_table.pdf
+++ reference/classic06_tall_table.pdf
@@ -41,4 +41,8 @@
 Row40 Val40 This is the description for row number 40

 Row41 Val41 This is the description for row number 41

 Row42 Val42 This is the description for row number 42

-Row43 Val43 This is the description for row number 43
+Row43 Val43 This is the description for row number 43

+Row44 Val44 This is the description for row number 44

+Row45 Val45 This is the description for row number 45

+Row46 Val46 This is the description for row number 46

+Row47 Val47 This is the description for row number 47
```
</details>

### classic07_numbers_only

- **Case Metadata:** format: xlsx | case: classic07_numbers_only | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic07_numbers_only.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=32658 bytes, Reference=42110 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Case Metadata:** format: xlsx | case: classic08_mixed_text_and_numbers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic08_mixed_text_and_numbers.xlsx
- **Text Similarity:** 0.9926
- **Visual Average:** 0.9961
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=39361 bytes, Reference=44923 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic08_mixed_text_and_numbers.pdf
+++ reference/classic08_mixed_text_and_numbers.pdf
@@ -2,5 +2,5 @@
 Item 10.5

 Tax 0.08

 Total 10.58

-Discount - 1.5

+Discount -1.5

 Final 9.08
```
</details>

### classic09_long_text

- **Case Metadata:** format: xlsx | case: classic09_long_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic09_long_text.xlsx
- **Text Similarity:** 0.899
- **Visual Average:** 0.9731
- **Overall Score:** 0.8488
- **Pages:** MiniPdf=1, Reference=12
- **File Size:** MiniPdf=196279 bytes, Reference=44367 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,5 +1,5 @@
 Long Text Column

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

 Short

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
```
</details>

### classic10_special_xml_characters

- **Case Metadata:** format: xlsx | case: classic10_special_xml_characters | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic10_special_xml_characters.xlsx
- **Text Similarity:** 0.9909
- **Visual Average:** 0.9942
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45001 bytes, Reference=43022 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic10_special_xml_characters.pdf
+++ reference/classic10_special_xml_characters.pdf
@@ -1,7 +1,7 @@
 Special Characters

 A&B

-< tag>

-" quoted"

+<tag>

+"quoted"

 it's

 Tom & Jerry < Batman > Superman

 He said "hello" & she replied 'hi'
```
</details>

### classic11_sparse_rows

- **Case Metadata:** format: xlsx | case: classic11_sparse_rows | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic11_sparse_rows.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=33349 bytes, Reference=34964 bytes

Text content: ✅ Identical

### classic12_sparse_columns

- **Case Metadata:** format: xlsx | case: classic12_sparse_columns | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic12_sparse_columns.xlsx
- **Text Similarity:** 0.9091
- **Visual Average:** 0.9966
- **Overall Score:** 0.8623
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=35633 bytes, Reference=41546 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic12_sparse_columns.pdf
+++ reference/classic12_sparse_columns.pdf
@@ -1,3 +1,3 @@
 Left Right

 Data1 FarRight

-Row3 VeryFar
+Row3
```
</details>

### classic13_date_strings

- **Case Metadata:** format: xlsx | case: classic13_date_strings | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic13_date_strings.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45112 bytes, Reference=49430 bytes

Text content: ✅ Identical

### classic14_decimal_numbers

- **Case Metadata:** format: xlsx | case: classic14_decimal_numbers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic14_decimal_numbers.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9947
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=42865 bytes, Reference=52898 bytes

Text content: ✅ Identical

### classic15_negative_numbers

- **Case Metadata:** format: xlsx | case: classic15_negative_numbers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic15_negative_numbers.xlsx
- **Text Similarity:** 0.8249
- **Visual Average:** 0.996
- **Overall Score:** 0.9284
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=42019 bytes, Reference=42915 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic15_negative_numbers.pdf
+++ reference/classic15_negative_numbers.pdf
@@ -1,7 +1,7 @@
 Label Value

-Loss - 100

-Small Loss - 0.5

+Loss -100

+Small Loss -0.5

 Zero 0

 Gain 50

-- 99999.99000000001 Big Loss

-Tiny - 0.001
+Big Loss -100000

+Tiny -0.001
```
</details>

### classic16_percentage_strings

- **Case Metadata:** format: xlsx | case: classic16_percentage_strings | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic16_percentage_strings.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9945
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=44350 bytes, Reference=52564 bytes

Text content: ✅ Identical

### classic17_currency_strings

- **Case Metadata:** format: xlsx | case: classic17_currency_strings | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic17_currency_strings.xlsx
- **Text Similarity:** 0.9808
- **Visual Average:** 0.9935
- **Overall Score:** 0.9897
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=44455 bytes, Reference=52509 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic17_currency_strings.pdf
+++ reference/classic17_currency_strings.pdf
@@ -1,7 +1,7 @@
 Item Price

-Widget $ 19.99

-Gadget $ 149.00

-Premium $ 1,299.99

-Budget $ 4.50

+Widget $19.99

+Gadget $149.00

+Premium $1,299.99

+Budget $4.50

 Euro Item €49.99

 Yen Item ¥5000
```
</details>

### classic18_large_dataset

- **Case Metadata:** format: xlsx | case: classic18_large_dataset | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic18_large_dataset.xlsx
- **Text Similarity:** 0.9083
- **Visual Average:** 0.8775
- **Overall Score:** 0.8143
- **Pages:** MiniPdf=23, Reference=42
- **File Size:** MiniPdf=5134282 bytes, Reference=920992 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic18_large_dataset.pdf
+++ reference/classic18_large_dataset.pdf
@@ -1,44 +1,48 @@
-Col0 Col1 Col2 Col3 Col4 Col5 Col6 Col7 Col8 Col9

-R0C0 R0C1 R0C2 R0C3 R0C4 R0C5 R0C6 R0C7 R0C8 R0C9

-R1C0 R1C1 R1C2 R1C3 R1C4 R1C5 R1C6 R1C7 R1C8 R1C9

-R2C0 R2C1 R2C2 R2C3 R2C4 R2C5 R2C6 R2C7 R2C8 R2C9

-R3C0 R3C1 R3C2 R3C3 R3C4 R3C5 R3C6 R3C7 R3C8 R3C9

-R4C0 R4C1 R4C2 R4C3 R4C4 R4C5 R4C6 R4C7 R4C8 R4C9

-R5C0 R5C1 R5C2 R5C3 R5C4 R5C5 R5C6 R5C7 R5C8 R5C9

-R6C0 R6C1 R6C2 R6C3 R6C4 R6C5 R6C6 R6C7 R6C8 R6C9

-R7C0 R7C1 R7C2 R7C3 R7C4 R7C5 R7C6 R7C7 R7C8 R7C9

-R8C0 R8C1 R8C2 R8C3 R8C4 R8C5 R8C6 R8C7 R8C8 R8C9

-R9C0 R9C1 R9C2 R9C3 R9C4 R9C5 R9C6 R9C7 R9C8 R9C9

-R10C0 R10C1 R10C2 R10C3 R10C4 R10C5 R10C6 R10C7 R10C8 R10C9

-R11C0 R11C1 R11C2 R11C3 R11C4 R11C5 R11C6 R11C7 R11C8 R11C9

-R12C0 R12C1 R12C2 R12C3 R12C4 R12C5 R12C6 R12C7 R12C8 R12C9

-R13C0 R13C1 R13C2 R13C3 R13C4 R13C5 R13C6 R13C7 R13C8 R13C9

-R14C0 R14C1 R14C2 R14C3 R14C4 R14C5 R14C6 R14C7 R14C8 R14C9

-R15C0 R15C1 R15C2 R15C3 R15C4 R15C5 R15C6 R15C7 R15C8 R15C9

-R16C0 R16C1 R16C2 R16C3 R16C4 R16C5 R16C6 R16C7 R16C8 R16C9

-R17C0 R17C1 R17C2 R17C3 R17C4 R17C5 R17C6 R17C7 R17C8 R17C9

-R18C0 R18C1 R18C2 R18C3 R18C4 R18C5 R18C6 R18C7 R18C8 R18C9

-R19C0 R19C1 R19C2 R19C3 R19C4 R19C5 R19C6 R19C7 R19C8 R19C9

-R20C0 R20C1 R20C2 R20C3 R20C4 R20C5 R20C6 R20C7 R20C8 R20C9

-R21C0 R21C1 R21C2 R21C3 R21C4 R21C5 R21C6 R21C7 R21C8 R21C9

-R22C0 R22C1 R22C2 R22C3 R22C4 R22C5 R22C6 R22C7 R22C8 R22C9

-R23C0 R23C1 R23C2 R23C3 R23C4 R23C5 R23C6 R23C7 R23C8 R23C9

-R24C0 R24C1 R24C2 R24C3 R24C4 R24C5 R24C6 R24C7 R24C8 R24C9

-R25C0 R25C1 R25C2 R25C3 R25C4 R25C5 R25C6 R25C7 R25C8 R25C9

-R26C0 R26C1 R26C2 R26C3 R26C4 R26C5 R26C6 R26C7 R26C8 R26C9

-R27C0 R27C1 R27C2 R27C3 R27C4 R27C5 R27C6 R27C7 R27C8 R27C9

-R28C0 R28C1 R28C2 R28C3 R28C4 R28C5 R28C6 R28C7 R28C8 R28C9

-R29C0 R29C1 R29C2 R29C3 R29C4 R29C5 R29C6 R29C7 R29C8 R29C9

-R30C0 R30C1 R30C2 R30C3 R30C4 R30C5 R30C6 R30C7 R30C8 R30C9

-R31C0 R31C1 R31C2 R31C3 R31C4 R31C5 R31C6 R31C7 R31C8 R31C9

-R32C0 R32C1 R32C2 R32C3 R32C4 R32C5 R32C6 R32C7 R32C8 R32C9

-R33C0 R33C1 R33C2 R33C3 R33C4 R33C5 R33C6 R33C7 R33C8 R33C9

-R34C0 R34C1 R34C2 R34C3 R34C4 R34C5 R34C6 R34C7 R34C8 R34C9

-R35C0 R35C1 R35C2 R35C3 R35C4 R35C5 R35C6 R35C7 R35C8 R35C9

-R36C0 R36C1 R36C2 R36C3 R36C4 R36C5 R36C6 R36C7 R36C8 R36C9

-R37C0 R37C1 R37C2 R37C3 R37C4 R37C5 R37C6 R37C7 R37C8 R37C9

-R38C0 R38C1 R38C2 R38C3 R38C4 R38C5 R38C6 R38C7 R38C8 R38C9

-R39C0 R39C1 R39C2 R39C3 R39C4 R39C5 R39C6 R39C7 R39C8 R39C9

-R40C0 R40C1 R40C2 R40C3 R40C4 R40C5 R40C6 R40C7 R40C8 R40C9

-R41C0 R41C1 R41C2 R41C3 R41C4 R41C5 R41C6 R41C7 R41C8 R41C9

-R42C0 R42C1 R42C2 R42C3 R42C4 R42C5 R42C6 R42C7 R42C8 R42C9
+Col0 Col1 Col2 Col3 Col4 Col5 Col6 Col7 Col8

+R0C0 R0C1 R0C2 R0C3 R0C4 R0C5 R0C6 R0C7 R0C8

+R1C0 R1C1 R1C2 R1C3 R1C4 R1C5 R1C6 R1C7 R1C8

+R2C0 R2C1 R2C2 R2C3 R2C4 R2C5 R2C6 R2C7 R2C8

+R3C0 R3C1 R3C2 R3C3 R3C4 R3C5 R3C6 R3C7 R3C8

+R4C0 R4C1 R4C2 R4C3 R4C4 R4C5 R4C6 R4C7 R4C8

+
... (2304 more characters)

```
</details>

### classic19_single_column_list

- **Case Metadata:** format: xlsx | case: classic19_single_column_list | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic19_single_column_list.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=44301 bytes, Reference=47524 bytes

Text content: ✅ Identical

### classic20_all_empty_cells

- **Case Metadata:** format: xlsx | case: classic20_all_empty_cells | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic20_all_empty_cells.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=897 bytes, Reference=25793 bytes

Text content: ✅ Identical

### classic21_header_only

- **Case Metadata:** format: xlsx | case: classic21_header_only | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic21_header_only.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9986
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=31068 bytes, Reference=35519 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Case Metadata:** format: xlsx | case: classic22_long_sheet_name | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic22_long_sheet_name.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=32990 bytes, Reference=36175 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Case Metadata:** format: xlsx | case: classic23_unicode_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic23_unicode_text.xlsx
- **Text Similarity:** 0.961
- **Visual Average:** 0.991
- **Overall Score:** 0.9808
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=81672 bytes, Reference=121984 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic23_unicode_text.pdf
+++ reference/classic23_unicode_text.pdf
@@ -3,5 +3,5 @@
 Chinese 你好 世界

 Japanese こんにちは世界

 Korean 안녕하세요세계

-Arabicا م

+Arabicمرحبا العالم

 Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Case Metadata:** format: xlsx | case: classic24_red_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic24_red_text.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9937
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=43848 bytes, Reference=41978 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Case Metadata:** format: xlsx | case: classic25_multiple_colors | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic25_multiple_colors.xlsx
- **Text Similarity:** 0.9955
- **Visual Average:** 0.9917
- **Overall Score:** 0.9949
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=55010 bytes, Reference=44930 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic25_multiple_colors.pdf
+++ reference/classic25_multiple_colors.pdf
@@ -1,4 +1,4 @@
-Color NameSample Text

+Color Nam Sample Text

 Red This is red text

 Green This is green text

 Blue This is blue text

```
</details>

### classic26_inline_strings

- **Case Metadata:** format: xlsx | case: classic26_inline_strings | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic26_inline_strings.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=36905 bytes, Reference=39664 bytes

Text content: ✅ Identical

### classic27_single_row

- **Case Metadata:** format: xlsx | case: classic27_single_row | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic27_single_row.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=33416 bytes, Reference=33233 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Case Metadata:** format: xlsx | case: classic28_duplicate_values | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic28_duplicate_values.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9957
- **Overall Score:** 0.9983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=32762 bytes, Reference=30315 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Case Metadata:** format: xlsx | case: classic29_formula_results | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic29_formula_results.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=37695 bytes, Reference=42240 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Case Metadata:** format: xlsx | case: classic30_mixed_empty_and_filled_sheets | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic30_mixed_empty_and_filled_sheets.xlsx
- **Text Similarity:** 0.0
- **Visual Average:** 0.9946
- **Overall Score:** 0.4978
- **Pages:** MiniPdf=4, Reference=2
- **File Size:** MiniPdf=38603 bytes, Reference=40960 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic30_mixed_empty_and_filled_sheets.pdf
+++ reference/classic30_mixed_empty_and_filled_sheets.pdf
@@ -0,0 +1,3 @@
+Hello World

+Foo Bar

+Baz Qux
```
</details>

### classic31_bold_header_row

- **Case Metadata:** format: xlsx | case: classic31_bold_header_row | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic31_bold_header_row.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=72327 bytes, Reference=77301 bytes

Text content: ✅ Identical

### classic32_right_aligned_numbers

- **Case Metadata:** format: xlsx | case: classic32_right_aligned_numbers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic32_right_aligned_numbers.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9961
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=40773 bytes, Reference=50644 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Case Metadata:** format: xlsx | case: classic33_centered_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic33_centered_text.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9974
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=36262 bytes, Reference=41368 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Case Metadata:** format: xlsx | case: classic34_explicit_column_widths | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic34_explicit_column_widths.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9923
- **Overall Score:** 0.9969
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=44996 bytes, Reference=47831 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Case Metadata:** format: xlsx | case: classic35_explicit_row_heights | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic35_explicit_row_heights.xlsx
- **Text Similarity:** 0.9462
- **Visual Average:** 0.9979
- **Overall Score:** 0.9776
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=37786 bytes, Reference=40677 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
-Tall HeaderValue

-Extra Tall Row 42

-Normal Row 10
+Tall Heade Value

+Extra Tall R 42

+Normal Ro 10
```
</details>

### classic36_merged_cells

- **Case Metadata:** format: xlsx | case: classic36_merged_cells | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic36_merged_cells.xlsx
- **Text Similarity:** 0.9643
- **Visual Average:** 0.9942
- **Overall Score:** 0.9834
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=43221 bytes, Reference=43920 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic36_merged_cells.pdf
+++ reference/classic36_merged_cells.pdf
@@ -1,4 +1,4 @@
-Merged Header Spanning Three Columns

+Merged Header Spanning Three C

 Col1 Col2 Col3

 Row2A Row2B Row2C

 Row3A Row3B Row3C
```
</details>

### classic37_freeze_panes

- **Case Metadata:** format: xlsx | case: classic37_freeze_panes | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic37_freeze_panes.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9838
- **Overall Score:** 0.9935
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=63721 bytes, Reference=58159 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Case Metadata:** format: xlsx | case: classic38_hyperlink_cell | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic38_hyperlink_cell.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9963
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=40144 bytes, Reference=41405 bytes

Text content: ✅ Identical

### classic39_financial_table

- **Case Metadata:** format: xlsx | case: classic39_financial_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic39_financial_table.xlsx
- **Text Similarity:** 0.99
- **Visual Average:** 0.9896
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=48317 bytes, Reference=52570 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic39_financial_table.pdf
+++ reference/classic39_financial_table.pdf
@@ -1,7 +1,7 @@
 Month Budget Actual Variance

-Jan 10000 9500 - 500

+Jan 10000 9500 -500

 Feb 10000 10800 800

-Mar 10000 9900 - 100

+Mar 10000 9900 -100

 Apr 10000 11200 1200

-May 10000 9700 - 300

+May 10000 9700 -300

 Jun 10000 10050 50
```
</details>

### classic40_scientific_notation

- **Case Metadata:** format: xlsx | case: classic40_scientific_notation | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic40_scientific_notation.xlsx
- **Text Similarity:** 0.7203
- **Visual Average:** 0.9929
- **Overall Score:** 0.8853
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=51528 bytes, Reference=60659 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic40_scientific_notation.pdf
+++ reference/classic40_scientific_notation.pdf
@@ -1,6 +1,6 @@
 Label Value

-Avogadro6.022e+23

-Planck 6.626e-34

-Speed of Light 299800000

-9.108999999999999e-31 Electron mass

-Pi approx 3.14159265358979
+Avogadro 6.02E+23

+Planck 6.63E-34

+Speed of Li 3E+08

+Electron m 9.11E-31

+Pi approx 3.141593
```
</details>

### classic41_integer_vs_float

- **Case Metadata:** format: xlsx | case: classic41_integer_vs_float | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic41_integer_vs_float.xlsx
- **Text Similarity:** 0.936
- **Visual Average:** 0.9949
- **Overall Score:** 0.9724
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=43273 bytes, Reference=46475 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic41_integer_vs_float.pdf
+++ reference/classic41_integer_vs_float.pdf
@@ -1,9 +1,9 @@
 Type Value

 Integer 42

 Float 42

-NegInt - 7

-NegFloat - 7.5

+NegInt -7

+NegFloat -7.5

 Zero 0

 ZeroFloat 0

 Large 1000000

-Small 1e-06
+Small 0.000001
```
</details>

### classic42_boolean_values

- **Case Metadata:** format: xlsx | case: classic42_boolean_values | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic42_boolean_values.xlsx
- **Text Similarity:** 0.8136
- **Visual Average:** 0.9928
- **Overall Score:** 0.9226
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=41686 bytes, Reference=44451 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

-Dark Mode1

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

- **Case Metadata:** format: xlsx | case: classic43_inventory_report | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic43_inventory_report.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9809
- **Overall Score:** 0.9924
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=89244 bytes, Reference=89289 bytes

Text content: ✅ Identical

### classic44_employee_roster

- **Case Metadata:** format: xlsx | case: classic44_employee_roster | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic44_employee_roster.xlsx
- **Text Similarity:** 0.7759
- **Visual Average:** 0.9748
- **Overall Score:** 0.9003
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=84356 bytes, Reference=69090 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

-1001 Alice Smith EngineeringSenior Engineer alice@example.com

-1002 Bob Jones Marketing Marketing Manager bob@example.com

-1003 Carol Williams HR HR Specialist carol@example.com

-1004 David Brown EngineeringJunior Engineer david@example.com

-1005 Eve Davis Finance Financial Analyst eve@example.com

-1006 Frank Miller Sales Sales Representative frank@example.com

-1007 Grace Wilson EngineeringTech Lead grace@example.com

-1008 Henry Moore Support Support Specialist henry@example.com
+1001 Alice Smith Engineerin Senior Eng alice@example.com

+1002 Bob Jones Marketing Marketing bob@example.com

+1003 Carol Williams HR HR Specialicarol@example.com

+1004 David Brown Engineerin Junior Engidavid@example.com

+1005 Eve Davis Finance Financial A eve@example.com

+1006 Frank Miller Sales Sales Reprefrank@example.com

+1007 Grace Wilson Engineerin Tech Lead grace@example.com

+1008 Henry Moore Support Support Sphenry@example.com
```
</details>

### classic45_sales_by_region

- **Case Metadata:** format: xlsx | case: classic45_sales_by_region | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic45_sales_by_region.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=49347 bytes, Reference=52603 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Case Metadata:** format: xlsx | case: classic46_grade_book | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic46_grade_book.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9875
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=55393 bytes, Reference=60464 bytes

Text content: ✅ Identical

### classic47_time_series

- **Case Metadata:** format: xlsx | case: classic47_time_series | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic47_time_series.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9708
- **Overall Score:** 0.9883
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=83253 bytes, Reference=60034 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Case Metadata:** format: xlsx | case: classic48_survey_results | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic48_survey_results.xlsx
- **Text Similarity:** 0.9859
- **Visual Average:** 0.9896
- **Overall Score:** 0.9902
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=52926 bytes, Reference=57959 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
-Question StrongAgreeAgree Neutral Disagree StrongDisagree

+Question StrongAgreAgree Neutral Disagree StrongDisagree

 Easy to use 30 45 15 7 3

-Recommend 25 40 20 10 5

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

-Good support 35 40 15 7 3

+Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Case Metadata:** format: xlsx | case: classic49_contact_list | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic49_contact_list.xlsx
- **Text Similarity:** 0.6491
- **Visual Average:** 0.9829
- **Overall Score:** 0.8528
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=79636 bytes, Reference=72007 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smith + 1-555-0101 alice@example.com New York USA

-Bob Jones + 44-20-7946-0958 bob@example.co.uk London UK

-Carol Wang + 86-10-1234-5678 carol@example.cn Beijing China

-David Muller + 49-30-1234567 david@example.de Berlin Germany

-Eve Martin + 33-1-23-45-67-89 eve@example.fr Paris France

-Frank Tanaka + 81-3-1234-5678 frank@example.jp Tokyo Japan

-Grace Kim + 82-2-1234-5678 grace@example.kr Seoul Korea
+Alice Smith+1-555-010alice@examNew York USA

+Bob Jones +44-20-794bob@examLondon UK

+Carol Wang+86-10-123carol@exa Beijing China

+David Mull +49-30-123david@exaBerlin Germany

+Eve Martin+33-1-23-4eve@examParis France

+Frank Tana+81-3-1234frank@exaTokyo Japan

+Grace Kim +82-2-1234grace@exaSeoul Korea
```
</details>

### classic50_budget_vs_actuals

- **Case Metadata:** format: xlsx | case: classic50_budget_vs_actuals | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic50_budget_vs_actuals.xlsx
- **Text Similarity:** 0.9956
- **Visual Average:** 0.9819
- **Overall Score:** 0.991
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=88736 bytes, Reference=65935 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic50_budget_vs_actuals.pdf
+++ reference/classic50_budget_vs_actuals.pdf
@@ -1,5 +1,5 @@
-DepartmentQ1 Q2 Q3 Q4 Annual

-Engineering 200000 200000 210000 220000 830000

+DepartmenQ1 Q2 Q3 Q4 Annual

+Engineerin 200000 200000 210000 220000 830000

 Marketing 80000 90000 85000 95000 350000

 Sales 120000 130000 140000 150000 540000

 HR 40000 40000 42000 43000 165000

```
</details>

### classic51_product_catalog

- **Case Metadata:** format: xlsx | case: classic51_product_catalog | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic51_product_catalog.xlsx
- **Text Similarity:** 0.6242
- **Visual Average:** 0.977
- **Overall Score:** 0.8405
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=93659 bytes, Reference=72763 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
 Part# Name DescriptionWeight(g) Price

-P-001 Basic Widget Standard widget for everyday use 150 4.99

-P-002 Pro WidgetEnhanced widget with premium features 180 12.99

-P-003 Mini GadgetCompact gadget for mobile use 90 19.99

-P-004 Max GadgetFull-size gadget, industrial grade 89.98999999999999 450

-P-005 Connector AType-A connector cable, 1m 80 7.49

-P-006 Connector BType-B connector cable, 2m 110 9.99

-P-007 Adapter X Universal power adapter 200 15.99

-P-008 Adapter Y Travel power adapter 120 11.99

-P-009 Mount Bracket Wall mount bracket, steel 600 24.99

-P-010 Carry CasePadded carry case, waterproof 350 34.99
+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgetEnhanced w 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size ga 450 89.99

+P-005 Connector Type-A con 80 7.49

+P-006 Connector Type-B con 110 9.99

+P-007 Adapter X Universal p 200 15.99

+P-008 Adapter Y Travel pow 120 11.99

+P-009 Mount Bra Wall moun 600 24.99

+P-010 Carry Case Padded car 350 34.99
```
</details>

### classic52_pivot_summary

- **Case Metadata:** format: xlsx | case: classic52_pivot_summary | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic52_pivot_summary.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9825
- **Overall Score:** 0.993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=81017 bytes, Reference=88958 bytes

Text content: ✅ Identical

### classic53_invoice

- **Case Metadata:** format: xlsx | case: classic53_invoice | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic53_invoice.xlsx
- **Text Similarity:** 0.9346
- **Visual Average:** 0.985
- **Overall Score:** 0.9678
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=101190 bytes, Reference=105898 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -7,10 +7,10 @@
 123 Business Rd, Suite 400

 New York, NY 10001

 Item Qty Unit Price Total

-Consulting Services10 150 1500

-Software License 5 99 495

+Consulting 10 150 1500

+Software L 5 99 495

 Hardware 2 249.99 499.98

-Support Plan (annual) 1 1200 1200

+Support Pla 1 1200 1200

 Subtotal 3694.98

 Tax (8%) 295.6

 Total Due 3990.58
```
</details>

### classic54_multi_level_header

- **Case Metadata:** format: xlsx | case: classic54_multi_level_header | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic54_multi_level_header.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9854
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=68510 bytes, Reference=73000 bytes

Text content: ✅ Identical

### classic55_error_values

- **Case Metadata:** format: xlsx | case: classic55_error_values | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic55_error_values.xlsx
- **Text Similarity:** 0.9864
- **Visual Average:** 0.9912
- **Overall Score:** 0.991
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=53380 bytes, Reference=60359 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic55_error_values.pdf
+++ reference/classic55_error_values.pdf
@@ -1,7 +1,7 @@
 Metric Value Status

 Sales 12345 OK

-Revenue # N/A Missing

-Cost # REF! Broken ref

-Profit # DIV/0! Div by zero

-Units # VALUE! Wrong type

+Revenue #N/A Missing

+Cost #REF! Broken ref

+Profit #DIV/0! Div by zero

+Units #VALUE! Wrong type

 Target 15000 OK
```
</details>

### classic56_alternating_row_colors

- **Case Metadata:** format: xlsx | case: classic56_alternating_row_colors | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic56_alternating_row_colors.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9734
- **Overall Score:** 0.9894
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=47526 bytes, Reference=50296 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Case Metadata:** format: xlsx | case: classic57_cjk_only | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic57_cjk_only.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9891
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45698 bytes, Reference=54240 bytes

Text content: ✅ Identical

### classic58_mixed_numeric_formats

- **Case Metadata:** format: xlsx | case: classic58_mixed_numeric_formats | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic58_mixed_numeric_formats.xlsx
- **Text Similarity:** 0.881
- **Visual Average:** 0.9919
- **Overall Score:** 0.9492
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=52181 bytes, Reference=56245 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic58_mixed_numeric_formats.pdf
+++ reference/classic58_mixed_numeric_formats.pdf
@@ -2,9 +2,9 @@
 Integer 1000000

 Float 2dp 3.14

 Float 5dp 3.14159

-Negative int - 42

-Negative float - 3.14

+Negative in -42

+Negative fl -3.14

 Very small 0.0001

-Very large 9999999.99

+Very large 10000000

 Zero 0

-Scientific approx 12300000000
+Scientific a 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Case Metadata:** format: xlsx | case: classic59_multi_sheet_summary | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic59_multi_sheet_summary.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9927
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=65776 bytes, Reference=61161 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Case Metadata:** format: xlsx | case: classic60_large_wide_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic60_large_wide_table.xlsx
- **Text Similarity:** 0.9083
- **Visual Average:** 0.8587
- **Overall Score:** 0.8068
- **Pages:** MiniPdf=4, Reference=6
- **File Size:** MiniPdf=559725 bytes, Reference=130804 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic60_large_wide_table.pdf
+++ reference/classic60_large_wide_table.pdf
@@ -1,44 +1,48 @@
-Col01 Col02 Col03 Col04 Col05 Col06 Col07 Col08 Col09 Col10

-R01C01 R01C02 R01C03 R01C04 R01C05 R01C06 R01C07 R01C08 R01C09 R01C10

-R02C01 R02C02 R02C03 R02C04 R02C05 R02C06 R02C07 R02C08 R02C09 R02C10

-R03C01 R03C02 R03C03 R03C04 R03C05 R03C06 R03C07 R03C08 R03C09 R03C10

-R04C01 R04C02 R04C03 R04C04 R04C05 R04C06 R04C07 R04C08 R04C09 R04C10

-R05C01 R05C02 R05C03 R05C04 R05C05 R05C06 R05C07 R05C08 R05C09 R05C10

-R06C01 R06C02 R06C03 R06C04 R06C05 R06C06 R06C07 R06C08 R06C09 R06C10

-R07C01 R07C02 R07C03 R07C04 R07C05 R07C06 R07C07 R07C08 R07C09 R07C10

-R08C01 R08C02 R08C03 R08C04 R08C05 R08C06 R08C07 R08C08 R08C09 R08C10

-R09C01 R09C02 R09C03 R09C04 R09C05 R09C06 R09C07 R09C08 R09C09 R09C10

-R10C01 R10C02 R10C03 R10C04 R10C05 R10C06 R10C07 R10C08 R10C09 R10C10

-R11C01 R11C02 R11C03 R11C04 R11C05 R11C06 R11C07 R11C08 R11C09 R11C10

-R12C01 R12C02 R12C03 R12C04 R12C05 R12C06 R12C07 R12C08 R12C09 R12C10

-R13C01 R13C02 R13C03 R13C04 R13C05 R13C06 R13C07 R13C08 R13C09 R13C10

-R14C01 R14C02 R14C03 R14C04 R14C05 R14C06 R14C07 R14C08 R14C09 R14C10

-R15C01 R15C02 R15C03 R15C04 R15C05 R15C06 R15C07 R15C08 R15C09 R15C10

-R16C01 R16C02 R16C03 R16C04 R16C05 R16C06 R16C07 R16C08 R16C09 R16C10

-R17C01 R17C02 R17C03 R17C04 R17C05 R17C06 R17C07 R17C08 R17C09 R17C10

-R18C01 R18C02 R18C03 R18C04 R18C05 R18C06 R18C07 R18C08 R18C09 R18C10

-R19C01 R19C02 R19C03 R19C04 R19C05 R19C06 R19C07 R19C08 R19C09 R19C10

-R20C01 R20C02 R20C03 R20C04 R20C05 R20C06 R20C07 R20C08 R20C09 R20C10

-R21C01 R21C02 R21C03 R21C04 R21C05 R21C06 R21C07 R21C08 R21C09 R21C10

-R22C01 R22C02 R22C03 R22C04 R22C05 R22C06 R22C07 R22C08 R22C09 R22C10

-R23C01 R23C02 R23C03 R23C04 R23C05 R23C06 R23C07 R23C08 R23C09 R23C10

-R24C01 R24C02 R24C03 R24C04 R24C05 R24C06 R24C07 R24C08 R24C09 R24C10

-R25C01 R25C02 R25C03 R25C04 R25C05 R25C06 R25C07 R25C08 R25C09 R25C10

-R26C01 R26C02 R26C03 R26C04 R26C05 R26C06 R26C07 R26C08 R26C09 R26C10

-R27C01 R27C02 R27C03 R27C04 R27C05 R27C06 R27C07 R27C08 R27C09 R27C10

-R28C01 R28C02 R28C03 R28C04 R28C05 R28C06 R28C07 R28C08 R28C09 R28C10

-R29C01 R29C02 R29C03 R29C04 R29C05 R29C06 R29C07 R29C08 R29C09 R29C10

-R30C01 R30C02 R30C03 R30C04 R30C05 R30C06 R30C07 R30C08 R30C09 R30C10

-R31C01 R31C02 R31C03 R31C04 R31C05 R31C06 R31C07 R31C08 R31C09 R31C10

-R32C01 R32C02 R32C03 R32C04 R32C05 R32C06 R32C07 R32C08 R32C09 R32C10

-R33C01 R33C02 R33C03 R33C04 R33C05 R33C06 R33C07 R33C08 R33C09 R33C10

-R34C01 R34C02 R34C03 R34C04 R34C05 R34C06 R34C07 R34C08 R34C09 R34C10

-R35C01 R35C02 R35C03 R35C04 R35C05 R35C06 R35C07 R35C08 R35C09 R35C10

-R36C01 R36C02 R36C03 R36C04 R36C05 R36C06 R36C07 R36C08 R36C09 R36C10

-R37C01 R37C02 R37C03 R37C04 R37C05 R37C06 R37C07 R37C08 R37C09 R37C10

-R38C01 R38C02 R38C03 R38C04 R38C05 R38C06 R38C07 R38C08 R38C09 R38C10

-R39C01 R39C02 R39C03 R39C04 R39C05 R39C06 R39C07 R39C08 R39C09 R39C10

-R40C01 R40C02 R40C03 R4
... (3372 more characters)

```
</details>

### classic61_product_card_with_image

- **Case Metadata:** format: xlsx | case: classic61_product_card_with_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic61_product_card_with_image.xlsx
- **Text Similarity:** 0.9908
- **Visual Average:** 0.9906
- **Overall Score:** 0.9926
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=64113 bytes, Reference=71701 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic61_product_card_with_image.pdf
+++ reference/classic61_product_card_with_image.pdf
@@ -1,6 +1,6 @@
 Product Name

 Widget Pro 3000

 Price

-$ 29.99

+$29.99

 In Stock

 150
```
</details>

### classic62_company_logo_header

- **Case Metadata:** format: xlsx | case: classic62_company_logo_header | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic62_company_logo_header.xlsx
- **Text Similarity:** 0.992
- **Visual Average:** 0.9895
- **Overall Score:** 0.9926
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=73806 bytes, Reference=79788 bytes

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

-Engineering 85 90 95 100

+Engineerin 85 90 95 100

 Marketing 60 65 70 75
```
</details>

### classic63_two_products_side_by_side

- **Case Metadata:** format: xlsx | case: classic63_two_products_side_by_side | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic63_two_products_side_by_side.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9846
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=42602 bytes, Reference=46024 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Case Metadata:** format: xlsx | case: classic64_employee_directory_with_photo | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic64_employee_directory_with_photo.xlsx
- **Text Similarity:** 0.9902
- **Visual Average:** 0.9823
- **Overall Score:** 0.989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=78020 bytes, Reference=71738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title DepartmentEmail

-Alice ChenEngineer R&D alice@example.com

-Bob SmithManager Sales bob@example.com

+Photo Name Title DepartmenEmail

+Alice Chen Engineer R&D alice@example.com

+Bob Smith Manager Sales bob@example.com

 Carol WangDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Case Metadata:** format: xlsx | case: classic65_inventory_with_product_photos | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic65_inventory_with_product_photos.xlsx
- **Text Similarity:** 0.9786
- **Visual Average:** 0.9844
- **Overall Score:** 0.9852
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=80961 bytes, Reference=81216 bytes

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

- **Case Metadata:** format: xlsx | case: classic66_invoice_with_logo | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic66_invoice_with_logo.xlsx
- **Text Similarity:** 0.9415
- **Visual Average:** 0.9857
- **Overall Score:** 0.9709
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=81910 bytes, Reference=87535 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -2,7 +2,7 @@
 Invoice #: INV-20250301

 Date: 2025-03-01

 DescriptionQty Unit Price Total

-Consulting Services8 150 1200

-Software License 1 299 299

-Support Package 1 99 99

+Consulting 8 150 1200

+Software L 1 299 299

+Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Case Metadata:** format: xlsx | case: classic67_real_estate_listing | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic67_real_estate_listing.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9837
- **Overall Score:** 0.9935
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=80909 bytes, Reference=95016 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Case Metadata:** format: xlsx | case: classic68_restaurant_menu | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic68_restaurant_menu.xlsx
- **Text Similarity:** 0.7901
- **Visual Average:** 0.7471
- **Overall Score:** 0.8149
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=88954 bytes, Reference=89964 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,9 +1,9 @@
 Today's Menu

-Grilled Salmon $ 18.99

+Grilled Salm $18.99

 Fresh Atlantic salmon with herbs

-Caesar Salad $ 12.99

+Caesar Sala $12.99

 Romaine lettuce, croutons, parmesan

-Beef Burger $ 14.99

+Beef Burge $14.99

 8oz Angus beef, brioche bun

-Pasta Primavera $ 13.99

+Pasta Prim $13.99

 Seasonal vegetables, olive oil
```
</details>

### classic69_image_only_sheet

- **Case Metadata:** format: xlsx | case: classic69_image_only_sheet | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic69_image_only_sheet.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9808
- **Overall Score:** 0.9923
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2761 bytes, Reference=6125 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Case Metadata:** format: xlsx | case: classic70_product_catalog_with_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic70_product_catalog_with_images.xlsx
- **Text Similarity:** 0.9582
- **Visual Average:** 0.9683
- **Overall Score:** 0.9706
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=82026 bytes, Reference=85583 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

-Classic Pen $ 3.99

+Classic Pen $3.99

 A reliable ballpoint pen

-Leather Notebook $ 12.99

+Leather No $12.99

 Premium A5 notebook

-Desk Organizer $ 24.99

+Desk Organ $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Case Metadata:** format: xlsx | case: classic71_multi_sheet_with_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic71_multi_sheet_with_images.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9942
- **Overall Score:** 0.9977
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=52790 bytes, Reference=56101 bytes

Text content: ✅ Identical

### classic72_bar_chart_image_with_data

- **Case Metadata:** format: xlsx | case: classic72_bar_chart_image_with_data | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic72_bar_chart_image_with_data.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.6405
- **Overall Score:** 0.8562
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=72729 bytes, Reference=74026 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Case Metadata:** format: xlsx | case: classic73_event_flyer_with_banner | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic73_event_flyer_with_banner.xlsx
- **Text Similarity:** 0.9087
- **Visual Average:** 0.9736
- **Overall Score:** 0.9529
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=89129 bytes, Reference=87466 bytes

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
+09:00 Opening KeDr. Jane Kim

+10:30 AI in Practi Prof. Mark Liu

+13:00 Cloud ArchEng. Sara Patel

+15:00 Panel DiscuAll Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Case Metadata:** format: xlsx | case: classic74_dashboard_with_kpi_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic74_dashboard_with_kpi_image.xlsx
- **Text Similarity:** 0.9846
- **Visual Average:** 0.78
- **Overall Score:** 0.9058
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=97388 bytes, Reference=99265 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -1,6 +1,6 @@
 Executive Dashboard Q1 2025

 KPI Target Actual Status

 Revenue 500000 523000 ✓ Above

-New Customers 200 187 ✗ Below

+New Custo 200 187 ✗ Below

 NPS Score 70 74 ✓ Above

-Churn Rate < 3% 2.8% ✓ Above
+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Case Metadata:** format: xlsx | case: classic75_certificate_with_seal | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic75_certificate_with_seal.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9714
- **Overall Score:** 0.9886
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76111 bytes, Reference=77570 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Case Metadata:** format: xlsx | case: classic76_product_image_grid | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic76_product_image_grid.xlsx
- **Text Similarity:** 0.98
- **Visual Average:** 0.9729
- **Overall Score:** 0.9812
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=67945 bytes, Reference=67180 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic76_product_image_grid.pdf
+++ reference/classic76_product_image_grid.pdf
@@ -1,5 +1,5 @@
 Best Sellers

 Red Phone Case Blue Speakers

-$ 9.99 $ 49.99

+$9.99 $49.99

 Green Backpack Yellow Headset

-$ 34.99 $ 29.99
+$34.99 $29.99
```
</details>

### classic77_news_article_with_hero_image

- **Case Metadata:** format: xlsx | case: classic77_news_article_with_hero_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic77_news_article_with_hero_image.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9676
- **Overall Score:** 0.987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=121784 bytes, Reference=112553 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Case Metadata:** format: xlsx | case: classic78_small_icon_per_row | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic78_small_icon_per_row.xlsx
- **Text Similarity:** 0.9283
- **Visual Average:** 0.9864
- **Overall Score:** 0.9659
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=81340 bytes, Reference=76703 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login bugAlice Done

-Write unit tests Bob In Progress

-Deploy to staging Carol Pending

-Code review PR #42 Alice Done

-Update docsDave In Progress
+Fix login buAlice Done

+Write unit Bob In Progress

+Deploy to sCarol Pending

+Code revie Alice Done

+Update docDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Case Metadata:** format: xlsx | case: classic79_wide_panoramic_banner | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic79_wide_panoramic_banner.xlsx
- **Text Similarity:** 0.9939
- **Visual Average:** 0.9695
- **Overall Score:** 0.9854
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=79717 bytes, Reference=87926 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic79_wide_panoramic_banner.pdf
+++ reference/classic79_wide_panoramic_banner.pdf
@@ -2,5 +2,5 @@
 Introducing the next generation of innovation.

 Available starting April 1, 2025

 Model Storage RAM Price

-Pro 256GB 16GB $ 999

-Max 512GB 32GB $ 1499
+Pro 256GB 16GB $999

+Max 512GB 32GB $1499
```
</details>

### classic80_portrait_tall_image

- **Case Metadata:** format: xlsx | case: classic80_portrait_tall_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic80_portrait_tall_image.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9856
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76221 bytes, Reference=71550 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Case Metadata:** format: xlsx | case: classic81_step_by_step_with_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic81_step_by_step_with_images.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.971
- **Overall Score:** 0.9884
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=90598 bytes, Reference=93150 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Case Metadata:** format: xlsx | case: classic82_before_after_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic82_before_after_images.xlsx
- **Text Similarity:** 0.9926
- **Visual Average:** 0.9668
- **Overall Score:** 0.9838
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75783 bytes, Reference=79534 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -1,5 +1,5 @@
 Before After

 Old design – legacy UI New design – modern UI

 Metric Before After Delta

-Load time 4.2s 1.1s - 74%

-Conversion2.1% 4.8% + 129%
+Load time 4.2s 1.1s -74%

+Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Case Metadata:** format: xlsx | case: classic83_color_swatch_palette | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic83_color_swatch_palette.xlsx
- **Text Similarity:** 0.9734
- **Visual Average:** 0.9801
- **Overall Score:** 0.9814
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=84886 bytes, Reference=82749 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

-Primary Blue RGB(0, 82, 165)

-Primary RedRGB(197, 27, 50)

-Accent Green RGB(0, 163, 108)

-Neutral Grey RGB(128, 128, 128)

-Warm Yellow RGB(255, 193, 7)

+Primary BluRGB(0, 82, 165)

+Primary ReRGB(197, 27, 50)

+Accent GreRGB(0, 163, 108)

+Neutral GreRGB(128, 128, 128)

+Warm YelloRGB(255, 193, 7)

 Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Case Metadata:** format: xlsx | case: classic84_travel_destination_cards | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic84_travel_destination_cards.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9663
- **Overall Score:** 0.9865
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=84931 bytes, Reference=83209 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Case Metadata:** format: xlsx | case: classic85_lab_results_with_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic85_lab_results_with_image.xlsx
- **Text Similarity:** 0.9846
- **Visual Average:** 0.8941
- **Overall Score:** 0.9515
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=86995 bytes, Reference=91041 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,5 +1,5 @@
 Sample Analysis Report

-ParameterValue Unit Reference Range Flag

+Parameter Value Unit Reference Flag

 pH 7.35 7.35 – 7.45Normal

 Glucose 5.2 mmol/L 3.9 – 5.5 Normal

 Sodium 142 mEq/L 136 – 145 Normal

```
</details>

### classic86_software_screenshot_features

- **Case Metadata:** format: xlsx | case: classic86_software_screenshot_features | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic86_software_screenshot_features.xlsx
- **Text Similarity:** 0.9801
- **Visual Average:** 0.9848
- **Overall Score:** 0.986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=77497 bytes, Reference=75924 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic86_software_screenshot_features.pdf
+++ reference/classic86_software_screenshot_features.pdf
@@ -4,6 +4,6 @@
 Dark ModeYes

 Auto Save Yes

 Cloud SyncYes

-Offline Mode Yes

-API AccessPro only

-Export to PDF Yes
+Offline Mo Yes

+API Access Pro only

+Export to PYes
```
</details>

### classic87_sports_results_with_logos

- **Case Metadata:** format: xlsx | case: classic87_sports_results_with_logos | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic87_sports_results_with_logos.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9895
- **Overall Score:** 0.9958
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75897 bytes, Reference=84228 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Case Metadata:** format: xlsx | case: classic88_image_after_data | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic88_image_after_data.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9728
- **Overall Score:** 0.9891
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=77522 bytes, Reference=84797 bytes

Text content: ✅ Identical

### classic89_nutrition_label_with_image

- **Case Metadata:** format: xlsx | case: classic89_nutrition_label_with_image | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic89_nutrition_label_with_image.xlsx
- **Text Similarity:** 0.9452
- **Visual Average:** 0.9867
- **Overall Score:** 0.9728
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=87948 bytes, Reference=90810 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount per serving % Daily Value

+Nutrient Amount pe% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

-Saturated Fat 0.5g 3%

+Saturated F0.5g 3%

 Sodium 160mg 7%

-Total Carbohydrate 22g 8%

-Dietary Fiber 3g 11%

+Total Carbo22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Case Metadata:** format: xlsx | case: classic90_project_status_with_milestones | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic90_project_status_with_milestones.xlsx
- **Text Similarity:** 0.7957
- **Visual Average:** 0.8925
- **Overall Score:** 0.8753
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=94748 bytes, Reference=88752 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
 Project Orion – Status Report

 Reporting Period: Q1 2025

-MilestoneDue Date Owner Status

-Requirements Freeze Jan 15 PM Team Complete

-Architecture Review Feb 1 Tech Lead Complete

-Alpha Release Feb 28 Dev Team In Progress

-Beta TestingMar 31 QA Team Not Started

-Production Deploy Apr 15 DevOps Not Started
+Milestone Due Date Owner Status

+RequiremeJan 15 PM Team Complete

+ArchitecturFeb 1 Tech Lead Complete

+Alpha Rele Feb 28 Dev Team In Progress

+Beta TestinMar 31 QA Team Not Started

+ProductionApr 15 DevOps Not Started
```
</details>

### classic91_simple_bar_chart

- **Case Metadata:** format: xlsx | case: classic91_simple_bar_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic91_simple_bar_chart.xlsx
- **Text Similarity:** 0.8718
- **Visual Average:** 0.6001
- **Overall Score:** 0.6888
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=43023 bytes, Reference=76902 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic91_simple_bar_chart.pdf
+++ reference/classic91_simple_bar_chart.pdf
@@ -1,6 +1,9 @@
 Product Revenue

 Widget A 12000

+Product Revenue

 Widget B 18500

 Widget C 9200

 Widget D 22000

-Widget E 15600
+Widget E 15600

+Revenue ($)

+Product
```
</details>

### classic92_horizontal_bar_chart

- **Case Metadata:** format: xlsx | case: classic92_horizontal_bar_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic92_horizontal_bar_chart.xlsx
- **Text Similarity:** 0.8673
- **Visual Average:** 0.5889
- **Overall Score:** 0.6825
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=45355 bytes, Reference=78581 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic92_horizontal_bar_chart.pdf
+++ reference/classic92_horizontal_bar_chart.pdf
@@ -1,5 +1,6 @@
-DepartmentHeadcount

-Engineering 45

+DepartmenHeadcount

+Engineerin 45

+Headcount by Department

 Sales 30

 Marketing 18

 HR 12

```
</details>

### classic93_line_chart

- **Case Metadata:** format: xlsx | case: classic93_line_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic93_line_chart.xlsx
- **Text Similarity:** 0.918
- **Visual Average:** 0.7269
- **Overall Score:** 0.758
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=46505 bytes, Reference=85633 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic93_line_chart.pdf
+++ reference/classic93_line_chart.pdf
@@ -1,5 +1,6 @@
 Month Avg Temp (C)

 Jan 3

+Monthly Average Temperature

 Feb 5

 Mar 10

 Apr 15

@@ -10,4 +11,5 @@
 Sep 22

 Oct 15

 Nov 8

-Dec 4
+Dec 4

+Temperature (C)
```
</details>

### classic94_pie_chart

- **Case Metadata:** format: xlsx | case: classic94_pie_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic94_pie_chart.xlsx
- **Text Similarity:** 0.7291
- **Visual Average:** 0.4435
- **Overall Score:** 0.569
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=44644 bytes, Reference=78532 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic94_pie_chart.pdf
+++ reference/classic94_pie_chart.pdf
@@ -1,6 +1,12 @@
 Segment Share (%)

 Enterprise 35

+Market Share by Segment

 SMB 28

 Consumer 22

-Government 10

-Education 5
+Governme 10

+Education 5

+Enterp

+SMB

+Consu

+Gover

+Educa
```
</details>

### classic95_area_chart

- **Case Metadata:** format: xlsx | case: classic95_area_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic95_area_chart.xlsx
- **Text Similarity:** 0.9524
- **Visual Average:** 0.6462
- **Overall Score:** 0.7394
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=51582 bytes, Reference=80677 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic95_area_chart.pdf
+++ reference/classic95_area_chart.pdf
@@ -1,5 +1,6 @@
 Hour Users

 00:00 214

+Website Traffic by Hour

 01:00 216

 02:00 218

 03:00 221

@@ -9,6 +10,7 @@
 07:00 240

 08:00 250

 09:00 265

+Users

 10:00 288

 11:00 329

 12:00 408

```
</details>

### classic96_scatter_chart

- **Case Metadata:** format: xlsx | case: classic96_scatter_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic96_scatter_chart.xlsx
- **Text Similarity:** 0.8889
- **Visual Average:** 0.6805
- **Overall Score:** 0.7278
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=45164 bytes, Reference=82367 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic96_scatter_chart.pdf
+++ reference/classic96_scatter_chart.pdf
@@ -1,5 +1,6 @@
-Ad Spend ($K) Sales ($K)

+Ad Spend ( Sales ($K)

 45 96

+Ad Spend vs Sales

 6 11

 20 43

 13 22

@@ -10,6 +11,7 @@
 18 38

 37 94

 6 20

+Sales ($K)

 17 49

 49 119

 31 68

@@ -17,5 +19,6 @@
 22 40

 15 37

 26 57

+Ad Spend ($K)

 14 28

 26 52
```
</details>

### classic97_doughnut_chart

- **Case Metadata:** format: xlsx | case: classic97_doughnut_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic97_doughnut_chart.xlsx
- **Text Similarity:** 0.8021
- **Visual Average:** 0.4485
- **Overall Score:** 0.6002
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=43236 bytes, Reference=76024 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic97_doughnut_chart.pdf
+++ reference/classic97_doughnut_chart.pdf
@@ -1,6 +1,12 @@
 Category Amount

 Salaries 50000

+Budget Allocation

 Rent 12000

 Marketing 8000

 R&D 15000

-Other 5000
+Other 5000

+Sala

+Ren

+Ma

+R&D

+Oth
```
</details>

### classic98_radar_chart

- **Case Metadata:** format: xlsx | case: classic98_radar_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic98_radar_chart.xlsx
- **Text Similarity:** 0.7027
- **Visual Average:** 0.7005
- **Overall Score:** 0.6613
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=43615 bytes, Reference=75968 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic98_radar_chart.pdf
+++ reference/classic98_radar_chart.pdf
@@ -1,7 +1,14 @@
 Skill Score

 Python 9

+Developer Skill Radar

 SQL 8

-Communication 7

+Communic 7

 Leadership 6

 Design 5

-DevOps 7
+DevOps 7

+Python

+SQL

+Commun

+Leadersh

+Design

+DevOps
```
</details>

### classic99_bubble_chart

- **Case Metadata:** format: xlsx | case: classic99_bubble_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic99_bubble_chart.xlsx
- **Text Similarity:** 0.902
- **Visual Average:** 0.6355
- **Overall Score:** 0.715
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=44137 bytes, Reference=86738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic99_bubble_chart.pdf
+++ reference/classic99_bubble_chart.pdf
@@ -1,7 +1,10 @@
 Price ($) Rating Units Sold

 10 4.2 500

+Product Comparison

 25 4.5 300

 50 3.8 150

 15 4 420

 35 4.7 200

-8 3.5 600
+8 3.5 600

+Rating

+Price ($)
```
</details>

### classic100_stacked_bar_chart

- **Case Metadata:** format: xlsx | case: classic100_stacked_bar_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic100_stacked_bar_chart.xlsx
- **Text Similarity:** 0.8621
- **Visual Average:** 0.6006
- **Overall Score:** 0.7851
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=41521 bytes, Reference=75642 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic100_stacked_bar_chart.pdf
+++ reference/classic100_stacked_bar_chart.pdf
@@ -2,4 +2,9 @@
 North 30 40 35 50

 South 25 30 45 40

 East 40 35 30 45

-West 20 25 40 35
+West 20 25 40 35

+Quarterly Revenue by Region

+Q4

+Q3

+Q2

+Q1
```
</details>

### classic101_percent_stacked_bar

- **Case Metadata:** format: xlsx | case: classic101_percent_stacked_bar | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic101_percent_stacked_bar.xlsx
- **Text Similarity:** 0.8696
- **Visual Average:** 0.5995
- **Overall Score:** 0.7876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=43699 bytes, Reference=78650 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic101_percent_stacked_bar.pdf
+++ reference/classic101_percent_stacked_bar.pdf
@@ -3,4 +3,9 @@
 2022 38 30 18 14

 2023 35 32 20 13

 2024 33 35 18 14

-2025 30 38 17 15
+2025 30 38 17 15

+Traffic Source Mix by Year

+Direct

+Referral

+Paid

+Organic
```
</details>

### classic102_line_chart_with_markers

- **Case Metadata:** format: xlsx | case: classic102_line_chart_with_markers | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic102_line_chart_with_markers.xlsx
- **Text Similarity:** 0.92
- **Visual Average:** 0.7667
- **Overall Score:** 0.7747
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=42081 bytes, Reference=78986 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic102_line_chart_with_markers.pdf
+++ reference/classic102_line_chart_with_markers.pdf
@@ -1,7 +1,9 @@
 Year Users (K) Revenue (K)

 2020 10 50

+Company Grow

 2021 25 120

 2022 55 280

 2023 90 500

 2024 140 780

-2025 200 1100
+2025 200 1100

+Value (K)
```
</details>

### classic103_pie_chart_with_labels

- **Case Metadata:** format: xlsx | case: classic103_pie_chart_with_labels | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic103_pie_chart_with_labels.xlsx
- **Text Similarity:** 0.4727
- **Visual Average:** 0.4832
- **Overall Score:** 0.4824
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=41727 bytes, Reference=76626 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic103_pie_chart_with_labels.pdf
+++ reference/classic103_pie_chart_with_labels.pdf
@@ -1,6 +1,17 @@
 OS Share (%)

 Windows 42

-macOS 28

-Linux 15

+Share (%),

+Desktop OS Market Share

+macOS 28 Other, 5, 5%

+Share (%),

+Linux 15 ChromeOS, 10,

+10%

 ChromeOS 10

-Other 5
+Other 5

+Share (%),

+Share (%), Linux,

+Windows, 42,

+15, 15%

+42%

+Share (%),

+macOS, 28, 28%
```
</details>

### classic104_combo_bar_line_chart

- **Case Metadata:** format: xlsx | case: classic104_combo_bar_line_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic104_combo_bar_line_chart.xlsx
- **Text Similarity:** 0.9333
- **Visual Average:** 0.5971
- **Overall Score:** 0.7122
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=42429 bytes, Reference=76509 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic104_combo_bar_line_chart.pdf
+++ reference/classic104_combo_bar_line_chart.pdf
@@ -1,5 +1,6 @@
 Month Sales Target

 Jan 42 45

+Sales vs Targe

 Feb 48 47

 Mar 51 50

 Apr 45 50

```
</details>

### classic105_3d_bar_chart

- **Case Metadata:** format: xlsx | case: classic105_3d_bar_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic105_3d_bar_chart.xlsx
- **Text Similarity:** 0.8824
- **Visual Average:** 0.5396
- **Overall Score:** 0.6688
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=40890 bytes, Reference=103065 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic105_3d_bar_chart.pdf
+++ reference/classic105_3d_bar_chart.pdf
@@ -1,5 +1,6 @@
 Region 2024 2025

 APAC 120 145

+Revenue by Region (3

 EMEA 95 110

 Americas 150 175

 LATAM 40 55
```
</details>

### classic106_3d_pie_chart

- **Case Metadata:** format: xlsx | case: classic106_3d_pie_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic106_3d_pie_chart.xlsx
- **Text Similarity:** 0.7928
- **Visual Average:** 0.54
- **Overall Score:** 0.6331
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=45633 bytes, Reference=113696 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic106_3d_pie_chart.pdf
+++ reference/classic106_3d_pie_chart.pdf
@@ -1,7 +1,14 @@
 Category Amount

 Food 800

+Monthly Expense Breakdown (3D)

 Housing 1500

 Transport 400

-Entertainment 300

+Entertainm 300

 Savings 700

-Other 200
+Other 200

+F

+H

+T

+E

+S

+O
```
</details>

### classic107_multi_series_line

- **Case Metadata:** format: xlsx | case: classic107_multi_series_line | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic107_multi_series_line.xlsx
- **Text Similarity:** 0.9858
- **Visual Average:** 0.947
- **Overall Score:** 0.8731
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=76148 bytes, Reference=91236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic107_multi_series_line.pdf
+++ reference/classic107_multi_series_line.pdf
@@ -1,5 +1,6 @@
 Day AAPL GOOG MSFT

 Day 1 178.48 140.49 402.83

+S

 Day 2 179.43 140.38 401.69

 Day 3 177.25 143.38 403.21

 Day 4 175.75 143.94 404.47

@@ -10,6 +11,7 @@
 Day 9 173.1 137.59 403.53

 Day 10 172.64 139.72 401.94

 Day 11 173.32 139.12 400.69

+Price ($)

 Day 12 172.11 140.8 402.75

 Day 13 173.5 143.13 404.12

 Day 14 172.29 141.53 404.52

```
</details>

### classic108_stacked_area_chart

- **Case Metadata:** format: xlsx | case: classic108_stacked_area_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic108_stacked_area_chart.xlsx
- **Text Similarity:** 0.8974
- **Visual Average:** 0.4355
- **Overall Score:** 0.6332
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=48749 bytes, Reference=86751 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic108_stacked_area_chart.pdf
+++ reference/classic108_stacked_area_chart.pdf
@@ -4,4 +4,9 @@
 Mar 125 110 230 115

 Apr 140 120 250 120

 May 150 130 240 125

-Jun 160 140 260 130
+Jun 160 140 260 130

+Traffic by Channel (Stacked)

+Direct

+Search

+Social

+Email
```
</details>

### classic109_scatter_with_trendline

- **Case Metadata:** format: xlsx | case: classic109_scatter_with_trendline | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic109_scatter_with_trendline.xlsx
- **Text Similarity:** 0.7901
- **Visual Average:** 0.6602
- **Overall Score:** 0.6801
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=42891 bytes, Reference=86322 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic109_scatter_with_trendline.pdf
+++ reference/classic109_scatter_with_trendline.pdf
@@ -1,16 +1,21 @@
-Study HoursExam Score

+Study HourExam Score

 5 59

+Study Hours vs Exam Score

 8 90

 9 85

+y = 8.1272x + 20.8

 2 35

+R² = 0.9586

 9 99

 5 68

 2 35

 8 92

 5 65

 3 45

+Score

 9 100

 6 62

 9 89

 1 30

-10 98
+10 98

+Hours
```
</details>

### classic110_chart_with_legend

- **Case Metadata:** format: xlsx | case: classic110_chart_with_legend | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic110_chart_with_legend.xlsx
- **Text Similarity:** 0.7843
- **Visual Average:** 0.5891
- **Overall Score:** 0.6494
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=44867 bytes, Reference=88129 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic110_chart_with_legend.pdf
+++ reference/classic110_chart_with_legend.pdf
@@ -1,6 +1,9 @@
 Browser 2024 (%) 2025 (%)

 Chrome 65 62

+Browser Market Share Com

 Safari 18 20

 Firefox 8 7

 Edge 6 8

-Other 3 3
+Other 3 3

+Market Share (%)

+2024 (%) 2025 (
```
</details>

### classic111_chart_with_axis_labels

- **Case Metadata:** format: xlsx | case: classic111_chart_with_axis_labels | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic111_chart_with_axis_labels.xlsx
- **Text Similarity:** 0.7895
- **Visual Average:** 0.6346
- **Overall Score:** 0.6696
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=43489 bytes, Reference=79609 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic111_chart_with_axis_labels.pdf
+++ reference/classic111_chart_with_axis_labels.pdf
@@ -1,7 +1,10 @@
 Country CO2 (Mt)

 China 10500

+CO2 Emissions by Country

 USA 5000

 India 2700

 Russia 1700

 Japan 1100

-Germany 700
+Germany 700

+Country

+CO2 Emissions (Megatons)
```
</details>

### classic112_multiple_charts

- **Case Metadata:** format: xlsx | case: classic112_multiple_charts | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic112_multiple_charts.xlsx
- **Text Similarity:** 0.9492
- **Visual Average:** 0.6165
- **Overall Score:** 0.7263
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=44637 bytes, Reference=86399 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic112_multiple_charts.pdf
+++ reference/classic112_multiple_charts.pdf
@@ -1,7 +1,9 @@
 Month Revenue Costs Profit

 Jan 50 30 20

+Revenue

 Feb 55 32 23

 Mar 60 35 25

 Apr 52 28 24

 May 70 40 30

-Jun 75 42 33
+Jun 75 42 33

+Profit T
```
</details>

### classic113_chart_sheet

- **Case Metadata:** format: xlsx | case: classic113_chart_sheet | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic113_chart_sheet.xlsx
- **Text Similarity:** 0.9091
- **Visual Average:** 0.5355
- **Overall Score:** 0.6778
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=36040 bytes, Reference=68612 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic113_chart_sheet.pdf
+++ reference/classic113_chart_sheet.pdf
@@ -1,5 +1,6 @@
 Quarter Revenue

 Q1 250

+Quarterly Revenue

 Q2 310

 Q3 285

 Q4 400
```
</details>

### classic114_chart_large_dataset

- **Case Metadata:** format: xlsx | case: classic114_chart_large_dataset | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic114_chart_large_dataset.xlsx
- **Text Similarity:** 0.8817
- **Visual Average:** 0.954
- **Overall Score:** 0.8343
- **Pages:** MiniPdf=3, Reference=4
- **File Size:** MiniPdf=103939 bytes, Reference=97214 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic114_chart_large_dataset.pdf
+++ reference/classic114_chart_large_dataset.pdf
@@ -1,18 +1,19 @@
 Day Value

 1 97.7

+100-Day Value

 2 93.7

-96.09999999999999 3

+3 96.1

 4 93.7

-95.59999999999999 5

+5 95.6

 6 92.3

-98.09999999999999 7

+7 98.1

 8 100.5

 9 98.7

-94.40000000000001 10

-98.59999999999999 11

+10 94.4

+11 98.6

 12 103.5

 13 102.2

-98.40000000000001 14

+14 98.4

 15 104.2

 16 109

 17 109.1

@@ -41,4 +42,8 @@
 40 131

 41 131.7

 42 137.3

-43 137.6
+43 137.6

+44 133.5

+45 130

+46 128.3

+47 127
```
</details>

### classic115_chart_negative_values

- **Case Metadata:** format: xlsx | case: classic115_chart_negative_values | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic115_chart_negative_values.xlsx
- **Text Similarity:** 0.7978
- **Visual Average:** 0.6198
- **Overall Score:** 0.667
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=41566 bytes, Reference=85182 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic115_chart_negative_values.pdf
+++ reference/classic115_chart_negative_values.pdf
@@ -1,9 +1,11 @@
 Month Profit/Loss

 Jan 15

-Feb - 8

+Monthly Profit & Loss

+Feb -8

 Mar 22

-Apr - 3

+Apr -3

 May 30

-Jun - 12

+Jun -12

 Jul 18

-Aug 5
+Aug 5

+Amount ($K)
```
</details>

### classic116_percent_stacked_area

- **Case Metadata:** format: xlsx | case: classic116_percent_stacked_area | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic116_percent_stacked_area.xlsx
- **Text Similarity:** 0.9091
- **Visual Average:** 0.3877
- **Overall Score:** 0.6187
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=46028 bytes, Reference=80966 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic116_percent_stacked_area.pdf
+++ reference/classic116_percent_stacked_area.pdf
@@ -4,4 +4,9 @@
 2019 30 28 19 23

 2021 25 28 18 29

 2023 20 26 17 37

-2025 15 24 16 45
+2025 15 24 16 45

+Energy Mix Transition

+Renewable

+Nuclear

+Gas

+Coal
```
</details>

### classic117_stock_ohlc_chart

- **Case Metadata:** format: xlsx | case: classic117_stock_ohlc_chart | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic117_stock_ohlc_chart.xlsx
- **Text Similarity:** 0.9864
- **Visual Average:** 0.7859
- **Overall Score:** 0.8089
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=63772 bytes, Reference=91947 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic117_stock_ohlc_chart.pdf
+++ reference/classic117_stock_ohlc_chart.pdf
@@ -8,4 +8,5 @@
 Day 7 151.08 155.51 150.22 150.81

 Day 8 152.42 155.53 152.31 152.99

 Day 9 152.32 154.36 151.02 152.05

-Day 10 152.27 156.85 148.76 156.35
+Day 10 152.27 156.85 148.76 156.35

+Price ($)
```
</details>

### classic118_bar_chart_custom_colors

- **Case Metadata:** format: xlsx | case: classic118_bar_chart_custom_colors | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic118_bar_chart_custom_colors.xlsx
- **Text Similarity:** 0.8966
- **Visual Average:** 0.5774
- **Overall Score:** 0.6896
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=41366 bytes, Reference=78458 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic118_bar_chart_custom_colors.pdf
+++ reference/classic118_bar_chart_custom_colors.pdf
@@ -1,5 +1,6 @@
 Rating Count

 Excellent 45

+Customer Satisfaction Survey

 Good 30

 Average 15

 Poor 7

```
</details>

### classic119_dashboard_multi_charts

- **Case Metadata:** format: xlsx | case: classic119_dashboard_multi_charts | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic119_dashboard_multi_charts.xlsx
- **Text Similarity:** 0.8475
- **Visual Average:** 0.5026
- **Overall Score:** 0.64
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=78450 bytes, Reference=94742 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic119_dashboard_multi_charts.pdf
+++ reference/classic119_dashboard_multi_charts.pdf
@@ -1,4 +1,5 @@
 KPI Dashboard - Q4 2025

+Revenue vs Expenses

 Month Revenue Expenses

 Oct 85 60

 Nov 92 65

@@ -6,4 +7,8 @@
 Segment Share

 Enterprise 45

 SMB 30

-Consumer 25
+Consumer 25

+Revenue by Segment

+Enter

+SMB

+Cons
```
</details>

### classic120_chart_with_date_axis

- **Case Metadata:** format: xlsx | case: classic120_chart_with_date_axis | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic120_chart_with_date_axis.xlsx
- **Text Similarity:** 0.9123
- **Visual Average:** 0.7928
- **Overall Score:** 0.782
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=49055 bytes, Reference=82299 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic120_chart_with_date_axis.pdf
+++ reference/classic120_chart_with_date_axis.pdf
@@ -1,5 +1,6 @@
 Date Downloads

 2025-01-01 581

+Monthly Downloads (202

 2025-01-31 594

 2025-03-02 592

 2025-04-01 692

@@ -10,4 +11,6 @@
 2025-08-29 774

 2025-09-28 788

 2025-10-28 820

-2025-11-27 865
+Downloads

+2025-11-27 865

+Date
```
</details>

### classic121_thin_borders

- **Case Metadata:** format: xlsx | case: classic121_thin_borders | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic121_thin_borders.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9763
- **Overall Score:** 0.9905
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=73607 bytes, Reference=74465 bytes

Text content: ✅ Identical

### classic122_thick_outer_thin_inner

- **Case Metadata:** format: xlsx | case: classic122_thick_outer_thin_inner | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic122_thick_outer_thin_inner.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9689
- **Overall Score:** 0.9876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75333 bytes, Reference=78822 bytes

Text content: ✅ Identical

### classic123_dashed_borders

- **Case Metadata:** format: xlsx | case: classic123_dashed_borders | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic123_dashed_borders.xlsx
- **Text Similarity:** 0.9653
- **Visual Average:** 0.99
- **Overall Score:** 0.9821
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=68396 bytes, Reference=61720 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic123_dashed_borders.pdf
+++ reference/classic123_dashed_borders.pdf
@@ -1,6 +1,6 @@
-Border StyleSample

+Border Sty Sample

 dashed Bordered cell

 dotted Bordered cell

 dashDot Bordered cell

-dashDotDotBordered cell

-mediumDashed Bordered cell
+dashDotDoBordered cell

+mediumDaBordered cell
```
</details>

### classic124_colored_borders

- **Case Metadata:** format: xlsx | case: classic124_colored_borders | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic124_colored_borders.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9827
- **Overall Score:** 0.9931
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76766 bytes, Reference=63553 bytes

Text content: ✅ Identical

### classic125_solid_fills

- **Case Metadata:** format: xlsx | case: classic125_solid_fills | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic125_solid_fills.xlsx
- **Text Similarity:** 0.9897
- **Visual Average:** 0.982
- **Overall Score:** 0.9887
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75913 bytes, Reference=69321 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic125_solid_fills.pdf
+++ reference/classic125_solid_fills.pdf
@@ -3,7 +3,7 @@
 Light GreenBackground

 Light YellowBackground

 Light Red Background

-Light PurpleBackground

-Light Orange Background

+Light Purpl Background

+Light OrangBackground

 Gray 25% Background

 Sky Blue Background
```
</details>

### classic126_dark_header

- **Case Metadata:** format: xlsx | case: classic126_dark_header | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic126_dark_header.xlsx
- **Text Similarity:** 0.993
- **Visual Average:** 0.9849
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=83710 bytes, Reference=83023 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic126_dark_header.pdf
+++ reference/classic126_dark_header.pdf
@@ -1,6 +1,6 @@
-EmployeeDepartmentSalary Start Date

-Alice SmithEngineering 95000 2020-03-15

+EmployeeDepartment Salary Start Date

+Alice SmithEngineerin 95000 2020-03-15

 Bob Jones Marketing 72000 2019-07-01

 Carol Lee Finance 88000 2021-01-10

-David Kim Engineering 102000 2018-11-20

+David Kim Engineerin 102000 2018-11-20

 Eva Chen HR 68000 2022-05-03
```
</details>

### classic127_font_styles

- **Case Metadata:** format: xlsx | case: classic127_font_styles | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic127_font_styles.xlsx
- **Text Similarity:** 0.9195
- **Visual Average:** 0.9843
- **Overall Score:** 0.9615
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=133977 bytes, Reference=121281 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic127_font_styles.pdf
+++ reference/classic127_font_styles.pdf
@@ -2,8 +2,8 @@
 Bold Sample Bold text

 Italic Sample Italic text

 Underline Sample Underline text

-Strikethrough Sample Strikethrough text

+StrikethrouSample Strikethrough text

 Bold Italic Sample Bold Italic text

-Bold Underline Sample Bold Underline text

-Double Underline Sample Double Underline text

+Bold Under Sample Bold Underline text

+Double Un Sample Double Underline text

 Bold + Red Sample Bold + Red text
```
</details>

### classic128_font_sizes

- **Case Metadata:** format: xlsx | case: classic128_font_sizes | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic128_font_sizes.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9905
- **Overall Score:** 0.9962
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=68153 bytes, Reference=66894 bytes

Text content: ✅ Identical

### classic129_alignment_combos

- **Case Metadata:** format: xlsx | case: classic129_alignment_combos | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic129_alignment_combos.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=67927 bytes, Reference=65763 bytes

Text content: ✅ Identical

### classic130_wrap_and_indent

- **Case Metadata:** format: xlsx | case: classic130_wrap_and_indent | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic130_wrap_and_indent.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9796
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75082 bytes, Reference=70816 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic130_wrap_and_indent.pdf
+++ reference/classic130_wrap_and_indent.pdf
@@ -1,5 +1,7 @@
 Wrapped Text Indented Text

-This is a long text that should wrap within the cell when text wrapping is enabled.

+This is a long text that should wrap

+within the cell when text wrapping is

+enabled.

 Indent level 0

 Indent level 1

 Indent level 2

```
</details>

### classic131_number_formats

- **Case Metadata:** format: xlsx | case: classic131_number_formats | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic131_number_formats.xlsx
- **Text Similarity:** 0.5
- **Visual Average:** 0.9834
- **Overall Score:** 0.7934
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=78414 bytes, Reference=77127 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic131_number_formats.pdf
+++ reference/classic131_number_formats.pdf
@@ -1,11 +1,11 @@
 Format Value Display

-#,## 0 1234567 1234567

-#,## 0.00 1234567.891 1234567.891

-$#,## 0.00 9876.5 9876.5

-0.00% 0.8523 0.8523

-0.00E+00 123456789 123456789

-0000 42 42

-#,## 0;(#,##0) - 5000 - 5000

-yyyy-mm-dd 45658 45658

-dd/mm/yyyy 45658 45658

-hh:mm:ss 0.75 0.75
+#,##0 1,234,567 1234567

+#,##0.00 1,234,567.89 1234567.891

+$#,##0.00 $9,876.50 9876.5

+0.00% 85.23% 0.8523

+0.00E+00 1.23E+08 123456789

+0000 0042 42

+#,##0;(#,##0) (5,000) -5000

+yyyy-mm-dd 2025-01-01 45658

+dd/mm/yyyy 01/01/2025 45658

+hh:mm:ss 18:00:00 0.75
```
</details>

### classic132_striped_table

- **Case Metadata:** format: xlsx | case: classic132_striped_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic132_striped_table.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9553
- **Overall Score:** 0.9821
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=100495 bytes, Reference=84504 bytes

Text content: ✅ Identical

### classic133_gradient_rows

- **Case Metadata:** format: xlsx | case: classic133_gradient_rows | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic133_gradient_rows.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9678
- **Overall Score:** 0.9871
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76478 bytes, Reference=75810 bytes

Text content: ✅ Identical

### classic134_heatmap

- **Case Metadata:** format: xlsx | case: classic134_heatmap | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic134_heatmap.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9399
- **Overall Score:** 0.976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=71083 bytes, Reference=78105 bytes

Text content: ✅ Identical

### classic135_bottom_border_only

- **Case Metadata:** format: xlsx | case: classic135_bottom_border_only | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic135_bottom_border_only.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9894
- **Overall Score:** 0.9958
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=62730 bytes, Reference=58955 bytes

Text content: ✅ Identical

### classic136_financial_report_styled

- **Case Metadata:** format: xlsx | case: classic136_financial_report_styled | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic136_financial_report_styled.xlsx
- **Text Similarity:** 0.5932
- **Visual Average:** 0.9511
- **Overall Score:** 0.8177
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=89252 bytes, Reference=100226 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic136_financial_report_styled.pdf
+++ reference/classic136_financial_report_styled.pdf
@@ -1,8 +1,8 @@
 Category 2024 2025

-Revenue 450000 520000

-Cost of Goods - 180000 - 195000

-Gross Profit 270000 325000

-Operating Expenses - 120000 - 135000

-R&D - 45000 - 55000

-Marketing - 30000 - 38000

-Net Income 75000 97000
+Revenue $450,000.00 $520,000.00

+Cost of Goods -$180,000.00 -$195,000.00

+Gross Profit $270,000.00 $325,000.00

+Operating Expenses -$120,000.00 -$135,000.00

+R&D -$45,000.00 -$55,000.00

+Marketing -$30,000.00 -$38,000.00

+Net Income $75,000.00 $97,000.00
```
</details>

### classic137_checkerboard

- **Case Metadata:** format: xlsx | case: classic137_checkerboard | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic137_checkerboard.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9565
- **Overall Score:** 0.9826
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8292 bytes, Reference=31191 bytes

Text content: ✅ Identical

### classic138_color_grid

- **Case Metadata:** format: xlsx | case: classic138_color_grid | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic138_color_grid.xlsx
- **Text Similarity:** 0.9406
- **Visual Average:** 0.972
- **Overall Score:** 0.965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=39991 bytes, Reference=45006 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic138_color_grid.pdf
+++ reference/classic138_color_grid.pdf
@@ -1,4 +1,4 @@
-# FF6B6B # FFD93D # 6BCB77

-# 4D96FF # FF8E71 # C780FF

-# FFB4B4 # B5DEFF # E8FFC1

-# FFC0D0 # D5AAFF # A0E7E5
+#FF6B6B #FFD93D #6BCB77

+#4D96FF #FF8E71 #C780FF

+#FFB4B4 #B5DEFF #E8FFC1

+#FFC0D0 #D5AAFF #A0E7E5
```
</details>

### classic139_pattern_fills

- **Case Metadata:** format: xlsx | case: classic139_pattern_fills | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic139_pattern_fills.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.8246
- **Overall Score:** 0.9298
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=80584 bytes, Reference=80842 bytes

Text content: ✅ Identical

### classic140_rotated_text

- **Case Metadata:** format: xlsx | case: classic140_rotated_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic140_rotated_text.xlsx
- **Text Similarity:** 0.9583
- **Visual Average:** 0.9903
- **Overall Score:** 0.9794
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=66589 bytes, Reference=68994 bytes

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

- **Case Metadata:** format: xlsx | case: classic141_mixed_edge_borders | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic141_mixed_edge_borders.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9844
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=70589 bytes, Reference=66621 bytes

Text content: ✅ Identical

### classic142_styled_invoice

- **Case Metadata:** format: xlsx | case: classic142_styled_invoice | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic142_styled_invoice.xlsx
- **Text Similarity:** 0.8339
- **Visual Average:** 0.9313
- **Overall Score:** 0.9061
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=101175 bytes, Reference=105680 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic142_styled_invoice.pdf
+++ reference/classic142_styled_invoice.pdf
@@ -2,10 +2,10 @@
 Invoice #: INV-2025-0099

 Date: 2025-06-15

 Item Description Qty Price Total

-SVC-001 Web Development 40 125 5000

-SVC-002 UI/UX Design 20 100 2000

-SVC-003 Testing & QA 15 90 1350

-LIC-001 Annual License 1 2400 2400

-Subtotal: 10750

-Tax (10%): 1075

-Total: 11825
+SVC-001 Web Development 40 $125.00 $5,000.00

+SVC-002 UI/UX Design 20 $100.00 $2,000.00

+SVC-003 Testing & QA 15 $90.00 $1,350.00

+LIC-001 Annual License 1 $2,400.00 $2,400.00

+Subtotal: $10,750.00

+Tax (10%): $1,075.00

+Total: $11,825.00
```
</details>

### classic143_colored_tabs

- **Case Metadata:** format: xlsx | case: classic143_colored_tabs | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic143_colored_tabs.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=72774 bytes, Reference=74632 bytes

Text content: ✅ Identical

### classic144_note_style_cells

- **Case Metadata:** format: xlsx | case: classic144_note_style_cells | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic144_note_style_cells.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9684
- **Overall Score:** 0.9874
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=98960 bytes, Reference=94925 bytes

Text content: ✅ Identical

### classic145_status_badges

- **Case Metadata:** format: xlsx | case: classic145_status_badges | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic145_status_badges.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9515
- **Overall Score:** 0.9806
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=103243 bytes, Reference=89140 bytes

Text content: ✅ Identical

### classic146_double_border_table

- **Case Metadata:** format: xlsx | case: classic146_double_border_table | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic146_double_border_table.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9688
- **Overall Score:** 0.9875
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=77625 bytes, Reference=77024 bytes

Text content: ✅ Identical

### classic147_multi_sheet_styled

- **Case Metadata:** format: xlsx | case: classic147_multi_sheet_styled | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic147_multi_sheet_styled.xlsx
- **Text Similarity:** 0.9834
- **Visual Average:** 0.9844
- **Overall Score:** 0.9871
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=104794 bytes, Reference=97561 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic147_multi_sheet_styled.pdf
+++ reference/classic147_multi_sheet_styled.pdf
@@ -1,5 +1,5 @@
 Metric Value

-Total Revenue $ 1,200,000

-Total Costs $ 780,000

-Net Profit $ 420,000

+Total Revenue $1,200,000

+Total Costs $780,000

+Net Profit $420,000

 Margin 35%
```
</details>

### classic148_frozen_styled_grid

- **Case Metadata:** format: xlsx | case: classic148_frozen_styled_grid | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic148_frozen_styled_grid.xlsx
- **Text Similarity:** 0.9928
- **Visual Average:** 0.8622
- **Overall Score:** 0.942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=165010 bytes, Reference=90882 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic148_frozen_styled_grid.pdf
+++ reference/classic148_frozen_styled_grid.pdf
@@ -1,5 +1,5 @@
 ID Name Category Value Status Date

-1 Item-001 Beta 705.1799999999999 Active 2025-11-13

+1 Item-001 Beta 705.18 Active 2025-11-13

 2 Item-002 Beta 290.98 Active 2025-04-16

 3 Item-003 Gamma 86.63 Inactive 2025-09-22

 4 Item-004 Gamma 702.78 Inactive 2025-06-14

```
</details>

### classic149_merged_styled_sections

- **Case Metadata:** format: xlsx | case: classic149_merged_styled_sections | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic149_merged_styled_sections.xlsx
- **Text Similarity:** 0.9324
- **Visual Average:** 0.9353
- **Overall Score:** 0.9471
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=97216 bytes, Reference=93062 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic149_merged_styled_sections.pdf
+++ reference/classic149_merged_styled_sections.pdf
@@ -1,11 +1,11 @@
 Quarterly Performance Report

 Revenue Breakdown

 Source Q1 Q2 Total

-Online 120000 140000 260000

-Retail 90000 85000 175000

-Wholesale 60000 70000 130000

+Online 120,000 140,000 260,000

+Retail 90,000 85,000 175,000

+Wholesale 60,000 70,000 130,000

 Expense Summary

 Category Q1 Q2 Total

-Salaries 200000 210000 410000

-Marketing 30000 35000 65000

-Operations 50000 48000 98000
+Salaries 200,000 210,000 410,000

+Marketing 30,000 35,000 65,000

+Operations 50,000 48,000 98,000
```
</details>

### classic150_kitchen_sink_styles

- **Case Metadata:** format: xlsx | case: classic150_kitchen_sink_styles | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic150_kitchen_sink_styles.xlsx
- **Text Similarity:** 0.9916
- **Visual Average:** 0.9268
- **Overall Score:** 0.9674
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=132084 bytes, Reference=121318 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic150_kitchen_sink_styles.pdf
+++ reference/classic150_kitchen_sink_styles.pdf
@@ -5,8 +5,8 @@
 Strikethrough Deprecated Item Removed

 Dark Fill White on Dark Inverted

 Red Border Attention! Alert style

-Currency Format 12345.678 Formatted number

-Percentage 0.8756 Percent display

+Currency Format $12,345.68 Formatted number

+Percentage 87.56% Percent display

 This text wraps in the cell nicely

 Wrap + Center Multi-line

 Pattern Fill Gray pattern Hatched

```
</details>

### classic151_multilingual_greetings

- **Case Metadata:** format: xlsx | case: classic151_multilingual_greetings | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic151_multilingual_greetings.xlsx
- **Text Similarity:** 0.9761
- **Visual Average:** 0.9833
- **Overall Score:** 0.9838
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=114442 bytes, Reference=108265 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic151_multilingual_greetings.pdf
+++ reference/classic151_multilingual_greetings.pdf
@@ -5,8 +5,8 @@
 Korean 안녕하세요 감사합니다

 Thai สวัสดี ขอบคุณ

 Hindi नमस्ते धन्यवाद

-Arabicا ا

-Hebrewם ה

+Arabicمرحبا شكرا

+Hebrewשלום תודה

 Greek Γεια σου Ευχαριστώ

 Russian Привет Спасибо

 Vietnamese Xin chào Cảm ơn

```
</details>

### classic152_emoji_sampler

- **Case Metadata:** format: xlsx | case: classic152_emoji_sampler | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic152_emoji_sampler.xlsx
- **Text Similarity:** 0.9677
- **Visual Average:** 0.9852
- **Overall Score:** 0.9812
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=85627 bytes, Reference=105280 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic152_emoji_sampler.pdf
+++ reference/classic152_emoji_sampler.pdf
@@ -1,9 +1,9 @@
 Category Emoji

 Faces 😀😃😄😁😆

-Hearts ❤️ 🧡💛💚💙

+Hearts ❤️🧡💛💚💙

 Animals 🐶🐱🐭🐹🐰

 Food 🍎🍐🍊🍋🍌

-Travel ✈️ 🚗🚌🚂🚀

-Sports ⚽ 🏀🏈 ⚾ 🎾

-Symbols ✅❌ ⚠️ 🔴🟢

-Hands 👍👎👏🤝 ✌️
+Travel ✈️🚗🚌🚂🚀

+Sports ⚽🏀🏈⚾🎾

+Symbols ✅❌⚠️🔴🟢

+Hands 👍👎👏🤝✌️
```
</details>

### classic153_currency_symbols

- **Case Metadata:** format: xlsx | case: classic153_currency_symbols | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic153_currency_symbols.xlsx
- **Text Similarity:** 0.9967
- **Visual Average:** 0.9854
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=80552 bytes, Reference=66698 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic153_currency_symbols.pdf
+++ reference/classic153_currency_symbols.pdf
@@ -1,12 +1,12 @@
 Currency Symbol Example

-US Dollar $ $ 1,234.56

+US Dollar $ $1,234.56

 Euro € €1.234,56

 British Pound £ £1,234.56

 Japanese Yen ¥ ¥123,456

 Chinese Yuan ¥ ¥1,234.56

 Korean Won ₩ ₩1,234,560

 Indian Rupee ₹ ₹1,23,456

-Thai Baht ฿ ฿ 1,234.56

+Thai Baht ฿ ฿1,234.56

 Russian Ruble ₽ ₽1 234,56

 Turkish Lira ₺ ₺1.234,56

 Bitcoin ₿ ₿0.05

```
</details>

### classic154_math_symbols

- **Case Metadata:** format: xlsx | case: classic154_math_symbols | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic154_math_symbols.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.989
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=89747 bytes, Reference=85712 bytes

Text content: ✅ Identical

### classic155_diacritical_marks

- **Case Metadata:** format: xlsx | case: classic155_diacritical_marks | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic155_diacritical_marks.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=61919 bytes, Reference=63383 bytes

Text content: ✅ Identical

### classic156_rtl_bidi_text

- **Case Metadata:** format: xlsx | case: classic156_rtl_bidi_text | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic156_rtl_bidi_text.xlsx
- **Text Similarity:** 0.6818
- **Visual Average:** 0.9945
- **Overall Score:** 0.8705
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45454 bytes, Reference=47337 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic156_rtl_bidi_text.pdf
+++ reference/classic156_rtl_bidi_text.pdf
@@ -1,5 +1,5 @@
 Script Text

-Arabicب ابحرماملاعل

-Hebrewם

-Persianا

-Urduا
+Arabicمرحبا بالعالم

+Hebrewשלום עולם

+Persianسالم دنیا

+Urduہیلو دنیا
```
</details>

### classic157_cjk_extended

- **Case Metadata:** format: xlsx | case: classic157_cjk_extended | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic157_cjk_extended.xlsx
- **Text Similarity:** 0.9841
- **Visual Average:** 0.9769
- **Overall Score:** 0.9844
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=95059 bytes, Reference=118156 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic157_cjk_extended.pdf
+++ reference/classic157_cjk_extended.pdf
@@ -3,6 +3,6 @@
 Traditional CN 繁體中文測試字串 Taiwan / HK

 Japanese mixed 漢字とひらがなとカタカナ Kanji + Hiragana + Katakana

 Korean mixed 한글과 漢字 혼용 텍스트 Hangul + Hanja

-Rare CJK ????? CJK Ext-B (SMP)

+Rare CJK 𠀀𠀁𠀂𠀃𠀄 CJK Ext-B (SMP)

 Full-width ＡＢＣＤ１２３４ Full-width alphanumeric

 Half-width kana ｱｲｳｴｵ ｶｷｸｹｺ Half-width katakana
```
</details>

### classic158_emoji_skin_tones

- **Case Metadata:** format: xlsx | case: classic158_emoji_skin_tones | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic158_emoji_skin_tones.xlsx
- **Text Similarity:** 0.9673
- **Visual Average:** 0.9882
- **Overall Score:** 0.9822
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=73327 bytes, Reference=99585 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic158_emoji_skin_tones.pdf
+++ reference/classic158_emoji_skin_tones.pdf
@@ -2,5 +2,5 @@
 Thumbs up 👍🏻👍🏼👍🏽👍🏾👍🏿

 Waving 👋🏻👋🏼👋🏽👋🏾👋🏿

 Clapping 👏🏻👏🏼👏🏽👏🏾👏🏿

-Raised fist ✊ 🏻 ✊ 🏼 ✊ 🏽 ✊ 🏾 ✊ 🏿

+Raised fist ✊🏻✊🏼✊🏽✊🏾✊🏿

 Person 🧑🏻🧑🏼🧑🏽🧑🏾🧑🏿
```
</details>

### classic159_zwj_emoji

- **Case Metadata:** format: xlsx | case: classic159_zwj_emoji | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic159_zwj_emoji.xlsx
- **Text Similarity:** 0.9372
- **Visual Average:** 0.9899
- **Overall Score:** 0.9708
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=77822 bytes, Reference=106035 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic159_zwj_emoji.pdf
+++ reference/classic159_zwj_emoji.pdf
@@ -1,10 +1,10 @@
 Description Emoji

-Family 👨 ‍ 👩 ‍ 👧 ‍ 👦

-Couple with heart 👩 ‍ ❤️ ‍ 👨

-Woman technologist 👩 ‍ 💻

-Man cook 👨 ‍ 🍳

-Rainbow flag 🏳️ ‍ 🌈

-Trans flag 🏳️ ‍ ⚧️

-Firefighter 🧑 ‍ 🚒

-Health worker 🧑 ‍ ⚕️

-Service dog 🐕 ‍ 🦺
+Family 👨‍👩‍👧‍👦

+Couple with heart 👩‍❤️‍👨

+Woman technologist 👩‍💻

+Man cook 👨‍🍳

+Rainbow flag 🏳️‍🌈

+Trans flag 🏳️‍⚧️

+Firefighter 🧑‍🚒

+Health worker 🧑‍⚕️

+Service dog 🐕‍🦺
```
</details>

### classic160_punctuation_marks

- **Case Metadata:** format: xlsx | case: classic160_punctuation_marks | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic160_punctuation_marks.xlsx
- **Text Similarity:** 0.9683
- **Visual Average:** 0.9933
- **Overall Score:** 0.9846
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=108983 bytes, Reference=110515 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic160_punctuation_marks.pdf
+++ reference/classic160_punctuation_marks.pdf
@@ -1,7 +1,7 @@
 Type Characters

 Latin . , ; : ! ? … — – ' '  « »

-CJK 。、 ；：！？ 「」『』【】 （）

-Arabic ﷽

+CJK 。、；：！？「」『』【】（）

+Arabic ، ؛ ؟ ٪ ﷽

 Devanagari । ॥ ꣸ ꣹ ꣺

 Thai ฯ ๆ ๏ ๚ ๛

 Misc brackets ⟨⟩ ⟪⟫ ⌈⌉ ⌊⌋ ‖

```
</details>

### classic161_box_drawing

- **Case Metadata:** format: xlsx | case: classic161_box_drawing | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic161_box_drawing.xlsx
- **Text Similarity:** 0.9752
- **Visual Average:** 0.9844
- **Overall Score:** 0.9838
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=70201 bytes, Reference=94886 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic161_box_drawing.pdf
+++ reference/classic161_box_drawing.pdf
@@ -2,6 +2,6 @@
 Light box ┌──┬──┐│  │  │├──┼──┤└──┴──┘

 Heavy box ┏━━┳━━┓┃  ┃  ┃┣━━╋━━┫┗━━┻━━┛

 Double box ╔══╦══╗║  ║  ║╠══╬══╣╚══╩══╝

-Blocks ▀▁▂▃▄▅▆▇█ ░▒▓

-Geometric ■□▪▫▲△▼▽◆◇○●◎

+Blocks ▀ ▁ ▂▃ ▄ ▅▆▇ █ ░▒▓

+Geometric ■□▪▫▲ △ ▼ ▽◆◇ ○● ◎

 Braille ⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊
```
</details>

### classic162_cjk_emoji_styled

- **Case Metadata:** format: xlsx | case: classic162_cjk_emoji_styled | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic162_cjk_emoji_styled.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9877
- **Overall Score:** 0.9951
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=79061 bytes, Reference=133693 bytes

Text content: ✅ Identical

### classic163_cyrillic_alphabets

- **Case Metadata:** format: xlsx | case: classic163_cyrillic_alphabets | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic163_cyrillic_alphabets.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9844
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=67496 bytes, Reference=56100 bytes

Text content: ✅ Identical

### classic164_indic_scripts

- **Case Metadata:** format: xlsx | case: classic164_indic_scripts | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic164_indic_scripts.xlsx
- **Text Similarity:** 0.9947
- **Visual Average:** 0.9933
- **Overall Score:** 0.9952
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=59301 bytes, Reference=53352 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic164_indic_scripts.pdf
+++ reference/classic164_indic_scripts.pdf
@@ -1,6 +1,6 @@
 Script Sample

 Devanagari नमस्ते

-Tamil வணக்கம்்

+Tamil வணக்கம்

 Bengali নমস্কার

 Telugu నమస్కారం

 Gujarati નમસ્તે
```
</details>

### classic165_southeast_asian

- **Case Metadata:** format: xlsx | case: classic165_southeast_asian | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic165_southeast_asian.xlsx
- **Text Similarity:** 0.663
- **Visual Average:** 0.9858
- **Overall Score:** 0.8595
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=80510 bytes, Reference=94767 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic165_southeast_asian.pdf
+++ reference/classic165_southeast_asian.pdf
@@ -1,6 +1,6 @@
 Script Sample

-Thai ภาษาไทยเป็นภาษาที่ มีวรรณยุกต์

-Lao ພາສາລາວເປັນພາສາທີ່ສວ

-Myanmar ????????????????? ??????

+Thai ภาษาไทยเป็นภาษาที่มีวรรณยุกต์

+Lao ພາສາລາວເປັນພາສາທ ີ່ສວຍງາມ

+Myanmar မြန်ြာဘာသာစကာားသည် လှပသည်

 Khmer ភាសាខ្មែរជាភាសាចំណាស់

-Tibetan ?????????????????????????????????
+Tibetan བོད་ཀྱི་སྐད་ཡྱིག་ནྱི་གལ་ཆེན་པོ་ཡྱིན།
```
</details>

### classic166_emoji_progress

- **Case Metadata:** format: xlsx | case: classic166_emoji_progress | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic166_emoji_progress.xlsx
- **Text Similarity:** 0.9881
- **Visual Average:** 0.9717
- **Overall Score:** 0.9839
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=70214 bytes, Reference=101519 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic166_emoji_progress.pdf
+++ reference/classic166_emoji_progress.pdf
@@ -1,7 +1,7 @@
 Task Status Progress

 Design ✅ 🟩🟩🟩🟩🟩🟩🟩🟩🟩🟩 100%

-Frontend 🔄 🟩🟩🟩🟩🟩🟩🟩 ⬜⬜⬜ 70%

-Backend 🔄 🟩🟩🟩🟩🟩 ⬜⬜⬜⬜⬜ 50%

-Testing ⏳ 🟩🟩 ⬜⬜⬜⬜⬜⬜⬜⬜ 20%

+Frontend 🔄 🟩🟩🟩🟩🟩🟩🟩⬜⬜⬜ 70%

+Backend 🔄 🟩🟩🟩🟩🟩⬜⬜⬜⬜⬜ 50%

+Testing ⏳ 🟩🟩⬜⬜⬜⬜⬜⬜⬜⬜ 20%

 Deploy ❌ ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%

-Docs 🔄 🟩🟩🟩🟩🟩🟩🟩🟩 ⬜⬜ 80%
+Docs 🔄 🟩🟩🟩🟩🟩🟩🟩🟩⬜⬜ 80%
```
</details>

### classic167_musical_symbols

- **Case Metadata:** format: xlsx | case: classic167_musical_symbols | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic167_musical_symbols.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9843
- **Overall Score:** 0.9937
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=75957 bytes, Reference=107225 bytes

Text content: ✅ Identical

### classic168_mixed_ltr_rtl_styled

- **Case Metadata:** format: xlsx | case: classic168_mixed_ltr_rtl_styled | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic168_mixed_ltr_rtl_styled.xlsx
- **Text Similarity:** 0.9259
- **Visual Average:** 0.974
- **Overall Score:** 0.96
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=79805 bytes, Reference=83592 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic168_mixed_ltr_rtl_styled.pdf
+++ reference/classic168_mixed_ltr_rtl_styled.pdf
@@ -1,5 +1,5 @@
 Code Name Price

-EN-001 Programming Book $ 29.99

+EN-001 Programming Book $29.99

 FR-002 Livre de code €25.00

-AR-003ة50 SAR

-HE-004ד₪120
+AR-003كتاب برمجة50 SAR

+HE-004ספר קוד₪120
```
</details>

### classic169_korean_invoice

- **Case Metadata:** format: xlsx | case: classic169_korean_invoice | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic169_korean_invoice.xlsx
- **Text Similarity:** 0.993
- **Visual Average:** 0.9814
- **Overall Score:** 0.9898
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=94393 bytes, Reference=118888 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic169_korean_invoice.pdf
+++ reference/classic169_korean_invoice.pdf
@@ -1,8 +1,8 @@
-거래명세서 ( Transaction Statement)

+거래명세서 (Transaction Statement)

 번호 상품명 수량 단가 금액

 1 노트북 컴퓨터 2 ₩1,200,000 ₩2,400,000

 2 무선 마우스 5 ₩25,000 ₩125,000

 3 모니터 27 인치 2 ₩350,000 ₩700,000

-4 키보드 (기계식) 3 ₩89,000 ₩267,000

+4 키보드 ( 기계식 ) 3 ₩89,000 ₩267,000

 5 USB 허브 10 ₩15,000 ₩150,000

 합계 ₩3,642,000
```
</details>

### classic170_emoji_dashboard

- **Case Metadata:** format: xlsx | case: classic170_emoji_dashboard | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic170_emoji_dashboard.xlsx
- **Text Similarity:** 0.9871
- **Visual Average:** 0.9771
- **Overall Score:** 0.9857
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=85129 bytes, Reference=137376 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic170_emoji_dashboard.pdf
+++ reference/classic170_emoji_dashboard.pdf
@@ -1,6 +1,6 @@
 Metric Value Target

-🟢 Revenue $ 1.2M $ 1.0M

-🟡 Pipeline $ 800K $ 900K

+🟢 Revenue $1.2M $1.0M

+🟡 Pipeline $800K $900K

 🔴 Churn Rate 5.2% 3.0%

 🟢 NPS Score 72 65

 🟡 Response Time 2.1s 1.5s

```
</details>

### classic171_ipa_phonetic

- **Case Metadata:** format: xlsx | case: classic171_ipa_phonetic | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic171_ipa_phonetic.xlsx
- **Text Similarity:** 0.9981
- **Visual Average:** 0.9894
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76250 bytes, Reference=76615 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic171_ipa_phonetic.pdf
+++ reference/classic171_ipa_phonetic.pdf
@@ -5,4 +5,4 @@
 Vowels i y ɨ ʉ ɯ u e ø ɘ ɵ ɤ o ɛ œ ɜ ɞ ʌ ɔ æ a ɶ ɑ ɒ

 Tones ˥ ˦ ˧ ˨ ˩ ˥˩ ˩˥

 Diacritics ʰ ʷ ʲ ˠ ˤ ⁿ ˡ

-Example word / ˌɪntəˈnæʃənəl/ (international)
+Example word /ˌɪntəˈnæʃənəl/ (international)
```
</details>

### classic172_emoji_timeline

- **Case Metadata:** format: xlsx | case: classic172_emoji_timeline | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic172_emoji_timeline.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9798
- **Overall Score:** 0.9919
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=86098 bytes, Reference=117160 bytes

Text content: ✅ Identical

### classic173_african_languages

- **Case Metadata:** format: xlsx | case: classic173_african_languages | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic173_african_languages.xlsx
- **Text Similarity:** 0.8864
- **Visual Average:** 0.9847
- **Overall Score:** 0.9484
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=60304 bytes, Reference=64361 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic173_african_languages.pdf
+++ reference/classic173_african_languages.pdf
@@ -1,8 +1,8 @@
 Language Greeting Region

 Swahili Habari! Karibu sana. East Africa

-Amharic ???! ???? ??? ???. Ethiopia

+Amharic ሰላም ! እንኳን ደህና መጣህ . Ethiopia

 Yoruba Ẹ kú àárọ̀! Ẹ kú alẹ́! Nigeria

 Zulu Sawubona! Unjani? South Africa

 Hausa Sannu! Barka da zuwa. West Africa

 Igbo Nnọọ! Kedụ? Nigeria

-Tigrinya ???! ??? ???? Eritrea
+Tigrinya ሰላም ! ከመይ ኣለኻ ? Eritrea
```
</details>

### classic174_technical_symbols

- **Case Metadata:** format: xlsx | case: classic174_technical_symbols | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic174_technical_symbols.xlsx
- **Text Similarity:** 0.9971
- **Visual Average:** 0.9829
- **Overall Score:** 0.992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=87550 bytes, Reference=81967 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic174_technical_symbols.pdf
+++ reference/classic174_technical_symbols.pdf
@@ -6,4 +6,4 @@
 Temp 100°C = 212°F = 373.15 K

 Copyright © 2025 Company™ — All Rights Reserved®

 Fractions ½ ⅓ ¼ ⅕ ⅙ ⅛ ⅔ ¾ ⅘

-Roman nums Ⅰ Ⅱ Ⅲ Ⅳ Ⅴ Ⅵ Ⅶ Ⅷ Ⅸ Ⅹ Ⅺ Ⅻ
+Roman nums Ⅰ Ⅱ Ⅲ Ⅳ Ⅴ Ⅵ Ⅶ Ⅷ Ⅸ Ⅹ Ⅺ Ⅺ
```
</details>

### classic175_multiscript_catalog

- **Case Metadata:** format: xlsx | case: classic175_multiscript_catalog | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic175_multiscript_catalog.xlsx
- **Text Similarity:** 0.9864
- **Visual Average:** 0.9798
- **Overall Score:** 0.9865
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=137309 bytes, Reference=191190 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic175_multiscript_catalog.pdf
+++ reference/classic175_multiscript_catalog.pdf
@@ -3,7 +3,7 @@
 2 Kimchi 김치 ₩3,000 🥬

 3 Samosa समोसा ₹50 🥟

 4 Croissant Croissant €2.50 🥐

-5 Taco Taco $ 3.99 🌮

+5 Taco Taco $3.99 🌮

 6 Borscht Борщ ₽250 🍲

-7 Falafelل₪15 🧆

-8 Pad Thai ผัดไทย ฿ 80 🍜
+7 Falafelفالفل₪15 🧆

+8 Pad Thai ผัดไทย ฿80 🍜
```
</details>

### classic176_combining_characters

- **Case Metadata:** format: xlsx | case: classic176_combining_characters | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic176_combining_characters.xlsx
- **Text Similarity:** 0.9837
- **Visual Average:** 0.9868
- **Overall Score:** 0.9882
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=74614 bytes, Reference=68236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic176_combining_characters.pdf
+++ reference/classic176_combining_characters.pdf
@@ -2,6 +2,6 @@
 Single combining é = e + ́   ñ = n + ̃

 Double combining ệ = e + ̣ + ̂

 Vietnamese ắ ằ ẵ ẳ ặ ố ồ ỗ ổ ộ ứ ừ ữ ử ự

-Zalgo-like H̵̖̘e̷̝̣l̶̤l̴̥o̸̮

-Precomposed vs decomposed ü (precomposed) vs ü (decomposed )

+Zalgo-like H ̵̖̘e ̣l ̶̤l ̴̥o ̸̮

+Precomposed vs decomposed ü (precomposed) vs ü (decomposed)

 Hangul Jamo ㅎ ㅏ ㄴ ㄱ ㅡ ㄹ → 한글
```
</details>

### classic177_emoji_calendar

- **Case Metadata:** format: xlsx | case: classic177_emoji_calendar | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic177_emoji_calendar.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.985
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=82163 bytes, Reference=107156 bytes

Text content: ✅ Identical

### classic178_caucasus_ethiopic

- **Case Metadata:** format: xlsx | case: classic178_caucasus_ethiopic | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic178_caucasus_ethiopic.xlsx
- **Text Similarity:** 0.8462
- **Visual Average:** 0.9852
- **Overall Score:** 0.9326
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=63246 bytes, Reference=58144 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic178_caucasus_ethiopic.pdf
+++ reference/classic178_caucasus_ethiopic.pdf
@@ -1,6 +1,6 @@
 Script Sample Text

 Georgian საქართველო არის ძველი ცივილიზაცია.

 Armenian Հայաստանը հին քաղաքակրթություն ունի.

-Ethiopic ????? ????? ???? ??? ???

-Georgian mkhedruli ა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

-Armenian alphabetԱ Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
+Ethiopic ኢትዮጵያ የጥንታዊ ሥልጣኔ ምድር ናት።

+Georgian mkhedrა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

+Armenian alphab Ա Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
```
</details>

### classic179_emoji_inventory

- **Case Metadata:** format: xlsx | case: classic179_emoji_inventory | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic179_emoji_inventory.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9799
- **Overall Score:** 0.992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=109708 bytes, Reference=138581 bytes

Text content: ✅ Identical

### classic180_polyglot_paragraph

- **Case Metadata:** format: xlsx | case: classic180_polyglot_paragraph | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic180_polyglot_paragraph.xlsx
- **Text Similarity:** 0.9846
- **Visual Average:** 0.9881
- **Overall Score:** 0.9891
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=110811 bytes, Reference=153951 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic180_polyglot_paragraph.pdf
+++ reference/classic180_polyglot_paragraph.pdf
@@ -1,9 +1,9 @@
 Language Greeting

 English The quick brown fox.

 Japanese 速い茶色の狐。

-Korean 빠른 갈색 여우.

+Korean 빠른 갈색 여우 .

 Russian Быстрая бурая лиса.

 Greek Η γρήγορη αλεπού.

-Thai สุนัขจิ้งจอกสีน้ำตาล

-Hindi तेज़ भूरी लोमड़ीी

+Thai สุนัขจิ้งจอกสีน ้ำตำล

+Hindi तेज़ भूरी लोमडी

 Emoji 🦊 ➡️ 🐕
```
</details>

### classic181_feedback_tracker_with_images

- **Case Metadata:** format: xlsx | case: classic181_feedback_tracker_with_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic181_feedback_tracker_with_images.xlsx
- **Text Similarity:** 0.9939
- **Visual Average:** 0.9688
- **Overall Score:** 0.9851
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=117663 bytes, Reference=93919 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic181_feedback_tracker_with_images.pdf
+++ reference/classic181_feedback_tracker_with_images.pdf
@@ -6,4 +6,4 @@
 2026-03-03 Eve Wrong checklist used for application

 2026-03-03 Frank Name and contact details missing on cover letter

 2026-03-04 Grace Unable to scroll and read the privacy policy

-2026-03-04 Hank Applicant has three children, only one birth cert uploaded
+2026-03-04 Hank Applicant has three children, only one birth cert up
```
</details>

### classic182_dense_long_text_columns

- **Case Metadata:** format: xlsx | case: classic182_dense_long_text_columns | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic182_dense_long_text_columns.xlsx
- **Text Similarity:** 0.9845
- **Visual Average:** 0.9729
- **Overall Score:** 0.983
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=171359 bytes, Reference=105199 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic182_dense_long_text_columns.pdf
+++ reference/classic182_dense_long_text_columns.pdf
@@ -1,9 +1,9 @@
 ID First Name Last Name Department Position Title

 1001 Alexander Papadopoulos Engineering Senior Software Engineer

-1002 Magdalena Kowalczyk Human ResourcesHR Business Partner Lead

+1002 Magdalena Kowalczyk Human Resource HR Business Partner Lead

 1003 Christopher O'Sullivan Finance Chief Financial Analyst

-1004 Priyanka Ramasubramanian Marketing Digital Marketing Strategist

-1005 Jean-Pierre Beaumont Sales Regional Sales Director (EMEA)

+1004 Priyanka Ramasubrama Marketing Digital Marketing Strategist

+1005 Jean-Pierre Beaumont Sales Regional Sales Director (EM

 1006 Anastasia Volkov Engineering Principal Data Scientist

-1007 Mohammed Al-Rashidi Operations Supply Chain Optimization Manager

+1007 Mohammed Al-Rashidi Operations Supply Chain Optimization

 1008 Guadalupe Hernandez Legal Senior Corporate Counsel
```
</details>

### classic183_mixed_content_grid

- **Case Metadata:** format: xlsx | case: classic183_mixed_content_grid | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic183_mixed_content_grid.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9616
- **Overall Score:** 0.9846
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=88307 bytes, Reference=79725 bytes

Text content: ✅ Identical

### classic184_wide_narrow_columns

- **Case Metadata:** format: xlsx | case: classic184_wide_narrow_columns | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic184_wide_narrow_columns.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9472
- **Overall Score:** 0.9789
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=149860 bytes, Reference=102871 bytes

Text content: ✅ Identical

### classic185_tall_rows_vertical_align

- **Case Metadata:** format: xlsx | case: classic185_tall_rows_vertical_align | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic185_tall_rows_vertical_align.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9866
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=87438 bytes, Reference=72408 bytes

Text content: ✅ Identical

### classic186_multi_sheet_image_report

- **Case Metadata:** format: xlsx | case: classic186_multi_sheet_image_report | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic186_multi_sheet_image_report.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9917
- **Overall Score:** 0.9967
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=96223 bytes, Reference=92322 bytes

Text content: ✅ Identical

### classic187_bug_report_with_screenshots

- **Case Metadata:** format: xlsx | case: classic187_bug_report_with_screenshots | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic187_bug_report_with_screenshots.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9342
- **Overall Score:** 0.9737
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=100164 bytes, Reference=94413 bytes

Text content: ✅ Identical

### classic188_merged_header_with_images

- **Case Metadata:** format: xlsx | case: classic188_merged_header_with_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic188_merged_header_with_images.xlsx
- **Text Similarity:** 1.0
- **Visual Average:** 0.9691
- **Overall Score:** 0.9876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=76280 bytes, Reference=81139 bytes

Text content: ✅ Identical

### classic189_alternating_image_text_rows

- **Case Metadata:** format: xlsx | case: classic189_alternating_image_text_rows | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic189_alternating_image_text_rows.xlsx
- **Text Similarity:** 0.8589
- **Visual Average:** 0.9224
- **Overall Score:** 0.9125
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=116205 bytes, Reference=93481 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic189_alternating_image_text_rows.pdf
+++ reference/classic189_alternating_image_text_rows.pdf
@@ -1,7 +1,7 @@
 Step Action Expected Result Evidence

-Step 1 Open login page Login form is displayed with email and password fields See below

-Step 2 Enter valid credentials Dashboard loads within 3 seconds See below

-Step 3 Click export button CSV file downloads with all visible data See below

-Step 4 Apply date filter Table updates to show only matching records See below

-Step 5 Resize browser window Layout remains responsive at 768px width See below

-Step 6 Toggle dark mode All components switch to dark theme colors See below
+Step 1 Open login page Login form is displayed with e See below

+Step 2 Enter valid credentials Dashboard loads within 3 secoSee below

+Step 3 Click export button CSV file downloads with all visSee below

+Step 4 Apply date filter Table updates to show only mSee below

+Step 5 Resize browser window Layout remains responsive at See below

+Step 6 Toggle dark mode All components switch to darkSee below
```
</details>

### classic190_dashboard_kpi_images

- **Case Metadata:** format: xlsx | case: classic190_dashboard_kpi_images | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic190_dashboard_kpi_images.xlsx
- **Text Similarity:** 0.9815
- **Visual Average:** 0.9677
- **Overall Score:** 0.9797
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=86175 bytes, Reference=96601 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic190_dashboard_kpi_images.pdf
+++ reference/classic190_dashboard_kpi_images.pdf
@@ -1,8 +1,8 @@
 Q1 2026 KPI Dashboard

 Revenue Users NPS Churn

-$ 2.4M 12,450 72 3.2%

+$2.4M 12,450 72 3.2%

 Metric Target Actual Variance

-Revenue $ 2.0M $ 2.4M + 20%

-Users 10,000 12,450 + 24.5%

-NPS 65 72 + 10.8%

-Churn 4.0% 3.2% - 20%
+Revenue $2.0M $2.4M +20%

+Users 10,000 12,450 +24.5%

+NPS 65 72 +10.8%

+Churn 4.0% 3.2% -20%
```
</details>

### classic191_payroll_calculator

- **Case Metadata:** format: xlsx | case: classic191_payroll_calculator | scope: rust-classic-xlsx-office
- **Source:** tests/MiniPdf.Scripts/output/classic191_payroll_calculator.xlsx
- **Text Similarity:** 0.8842
- **Visual Average:** 0.9141
- **Overall Score:** 0.9193
- **Pages:** MiniPdf=9, Reference=9
- **File Size:** MiniPdf=349482 bytes, Reference=189742 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic191_payroll_calculator.pdf
+++ reference/classic191_payroll_calculator.pdf
@@ -1,8 +1,8 @@
 Employee Register

 Information contained in this employee register is highly confidential

-ID Employee's Name M/F Hire Date Occupation

-1 Adam Jones M 2013-02-15 Senior Accountant

-2 Nichola Brown F 2011-09-28 CR Manager

-3 Benny Erwin M 2011-05-11 Applications PM

-4 Rachel Kim F 2016-08-03 HR Specialist

-5 Carlos Ruiz M 2019-11-20 Sales Rep
+ID Employee's Name M/F Hire Date

+1 Adam Jones M 2013-02-15

+2 Nichola Brown F 2011-09-28

+3 Benny Erwin M 2011-05-11

+4 Rachel Kim F 2016-08-03

+5 Carlos Ruiz M 2019-11-20
```
</details>

## Improvement Suggestions

### ⚠ Low-Score Test Cases (below 0.8)

1. **classic103_pie_chart_with_labels** (score: 0.4824)
1. **classic30_mixed_empty_and_filled_sheets** (score: 0.4978)
1. **classic94_pie_chart** (score: 0.569)
1. **classic97_doughnut_chart** (score: 0.6002)
1. **classic116_percent_stacked_area** (score: 0.6187)
1. **classic106_3d_pie_chart** (score: 0.6331)
1. **classic108_stacked_area_chart** (score: 0.6332)
1. **classic119_dashboard_multi_charts** (score: 0.64)
1. **classic110_chart_with_legend** (score: 0.6494)
1. **classic98_radar_chart** (score: 0.6613)
1. **classic115_chart_negative_values** (score: 0.667)
1. **classic105_3d_bar_chart** (score: 0.6688)
1. **classic111_chart_with_axis_labels** (score: 0.6696)
1. **classic113_chart_sheet** (score: 0.6778)
1. **classic109_scatter_with_trendline** (score: 0.6801)
1. **classic92_horizontal_bar_chart** (score: 0.6825)
1. **classic91_simple_bar_chart** (score: 0.6888)
1. **classic118_bar_chart_custom_colors** (score: 0.6896)
1. **classic104_combo_bar_line_chart** (score: 0.7122)
1. **classic99_bubble_chart** (score: 0.715)
1. **classic112_multiple_charts** (score: 0.7263)
1. **classic96_scatter_chart** (score: 0.7278)
1. **classic95_area_chart** (score: 0.7394)
1. **classic93_line_chart** (score: 0.758)
1. **classic102_line_chart_with_markers** (score: 0.7747)
1. **classic120_chart_with_date_axis** (score: 0.782)
1. **classic100_stacked_bar_chart** (score: 0.7851)
1. **classic101_percent_stacked_bar** (score: 0.7876)
1. **classic131_number_formats** (score: 0.7934)

Review the text diffs and visual comparisons above to identify specific rendering issues.
