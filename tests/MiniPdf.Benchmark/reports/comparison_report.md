# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-22T23:50:48.928123

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9947 | 1/1 | **0.9979** |
| 2 | 🟢 classic02_multiple_worksheets | 1.0 | 0.9966 | 3/3 | **0.9986** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9996 | 1/1 | **0.9998** |
| 5 | 🟢 classic05_wide_table | 0.8846 | 0.9908 | 3/3 | **0.9502** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.8625 | 5/5 | **0.945** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.9984 | 1/1 | **0.9994** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9958 | 1/1 | **0.9983** |
| 9 | 🟡 classic09_long_text | 0.2984 | 0.9921 | 12/12 | **0.7162** |
| 10 | 🟢 classic10_special_xml_characters | 1.0 | 0.993 | 1/1 | **0.9972** |
| 11 | 🟢 classic11_sparse_rows | 1.0 | 0.9989 | 2/2 | **0.9996** |
| 12 | 🔴 classic12_sparse_columns | 1.0 | 0.4988 | 1/2 | **0.6995** |
| 13 | 🟢 classic13_date_strings | 0.949 | 0.9932 | 1/1 | **0.9769** |
| 14 | 🟢 classic14_decimal_numbers | 1.0 | 0.995 | 1/1 | **0.998** |
| 15 | 🟢 classic15_negative_numbers | 0.9375 | 0.9949 | 1/1 | **0.973** |
| 16 | 🟢 classic16_percentage_strings | 0.9939 | 0.9945 | 1/1 | **0.9954** |
| 17 | 🟢 classic17_currency_strings | 1.0 | 0.9925 | 1/1 | **0.997** |
| 18 | 🔴 classic18_large_dataset | 0.9001 | 0.4434 | 24/42 | **0.6374** |
| 19 | 🟢 classic19_single_column_list | 1.0 | 0.9908 | 1/1 | **0.9963** |
| 20 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 21 | 🟢 classic21_header_only | 1.0 | 0.9984 | 1/1 | **0.9994** |
| 22 | 🟢 classic22_long_sheet_name | 1.0 | 0.9979 | 1/1 | **0.9992** |
| 23 | 🟢 classic23_unicode_text | 0.9046 | 0.99 | 1/1 | **0.9578** |
| 24 | 🟢 classic24_red_text | 1.0 | 0.9944 | 1/1 | **0.9978** |
| 25 | 🟢 classic25_multiple_colors | 1.0 | 0.9907 | 1/1 | **0.9963** |
| 26 | 🟢 classic26_inline_strings | 1.0 | 0.996 | 1/1 | **0.9984** |
| 27 | 🟢 classic27_single_row | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 28 | 🟢 classic28_duplicate_values | 1.0 | 0.9954 | 1/1 | **0.9982** |
| 29 | 🟢 classic29_formula_results | 1.0 | 0.9953 | 1/1 | **0.9981** |
| 30 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.998 | 2/2 | **0.9992** |
| 31 | 🟢 classic31_bold_header_row | 1.0 | 0.9912 | 1/1 | **0.9965** |
| 32 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.996 | 1/1 | **0.9984** |
| 33 | 🟢 classic33_centered_text | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 34 | 🟢 classic34_explicit_column_widths | 1.0 | 0.9935 | 1/1 | **0.9974** |
| 35 | 🟢 classic35_explicit_row_heights | 0.9773 | 0.9973 | 1/1 | **0.9898** |
| 36 | 🟢 classic36_merged_cells | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 37 | 🟢 classic37_freeze_panes | 1.0 | 0.9754 | 1/1 | **0.9902** |
| 38 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9951 | 1/1 | **0.998** |
| 39 | 🟢 classic39_financial_table | 1.0 | 0.9892 | 1/1 | **0.9957** |
| 40 | 🟢 classic40_scientific_notation | 0.9515 | 0.9931 | 1/1 | **0.9778** |
| 41 | 🟢 classic41_integer_vs_float | 1.0 | 0.9931 | 1/1 | **0.9972** |
| 42 | 🟢 classic42_boolean_values | 0.9948 | 0.9933 | 1/1 | **0.9952** |
| 43 | 🟢 classic43_inventory_report | 1.0 | 0.9779 | 1/1 | **0.9912** |
| 44 | 🟢 classic44_employee_roster | 0.9935 | 0.9681 | 1/1 | **0.9846** |
| 45 | 🟢 classic45_sales_by_region | 1.0 | 0.9961 | 4/4 | **0.9984** |
| 46 | 🟢 classic46_grade_book | 1.0 | 0.9858 | 1/1 | **0.9943** |
| 47 | 🟢 classic47_time_series | 1.0 | 0.9611 | 1/1 | **0.9844** |
| 48 | 🟢 classic48_survey_results | 0.9914 | 0.9891 | 1/1 | **0.9922** |
| 49 | 🟢 classic49_contact_list | 0.8971 | 0.9778 | 1/1 | **0.95** |
| 50 | 🟢 classic50_budget_vs_actuals | 0.9978 | 0.9834 | 3/3 | **0.9925** |
| 51 | 🟢 classic51_product_catalog | 0.9826 | 0.9735 | 1/1 | **0.9824** |
| 52 | 🟢 classic52_pivot_summary | 0.9978 | 0.9828 | 1/1 | **0.9922** |
| 53 | 🟢 classic53_invoice | 0.9968 | 0.9775 | 1/1 | **0.9897** |
| 54 | 🟢 classic54_multi_level_header | 1.0 | 0.9864 | 1/1 | **0.9946** |
| 55 | 🟢 classic55_error_values | 1.0 | 0.9899 | 1/1 | **0.996** |
| 56 | 🟢 classic56_alternating_row_colors | 1.0 | 0.9544 | 1/1 | **0.9818** |
| 57 | 🟢 classic57_cjk_only | 1.0 | 0.9915 | 1/1 | **0.9966** |
| 58 | 🟢 classic58_mixed_numeric_formats | 0.9969 | 0.9897 | 1/1 | **0.9946** |
| 59 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9937 | 4/4 | **0.9975** |
| 60 | 🔴 classic60_large_wide_table | 0.852 | 0.5776 | 4/6 | **0.6718** |
| 61 | 🟢 classic61_product_card_with_image | 1.0 | 0.9737 | 1/1 | **0.9895** |
| 62 | 🟢 classic62_company_logo_header | 0.996 | 0.9725 | 1/1 | **0.9874** |
| 63 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9682 | 1/1 | **0.9873** |
| 64 | 🟢 classic64_employee_directory_with_photo | 0.9902 | 0.9654 | 1/1 | **0.9822** |
| 65 | 🟢 classic65_inventory_with_product_photos | 0.9938 | 0.9638 | 1/1 | **0.983** |
| 66 | 🟢 classic66_invoice_with_logo | 0.9935 | 0.9734 | 1/1 | **0.9868** |
| 67 | 🟢 classic67_real_estate_listing | 1.0 | 0.9471 | 1/1 | **0.9788** |
| 68 | 🟢 classic68_restaurant_menu | 0.9953 | 0.9329 | 1/1 | **0.9713** |
| 69 | 🟢 classic69_image_only_sheet | 1.0 | 0.9113 | 1/1 | **0.9645** |
| 70 | 🟢 classic70_product_catalog_with_images | 0.9932 | 0.9364 | 1/1 | **0.9718** |
| 71 | 🟢 classic71_multi_sheet_with_images | 0.9966 | 0.983 | 3/3 | **0.9918** |
| 72 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.9379 | 1/1 | **0.9752** |
| 73 | 🟢 classic73_event_flyer_with_banner | 0.9412 | 0.9378 | 1/1 | **0.9516** |
| 74 | 🟢 classic74_dashboard_with_kpi_image | 0.9938 | 0.9615 | 1/1 | **0.9821** |
| 75 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9694 | 1/1 | **0.9878** |
| 76 | 🟢 classic76_product_image_grid | 1.0 | 0.9408 | 1/1 | **0.9763** |
| 77 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.9146 | 1/1 | **0.9658** |
| 78 | 🟢 classic78_small_icon_per_row | 0.9767 | 0.983 | 1/1 | **0.9839** |
| 79 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9477 | 1/1 | **0.9791** |
| 80 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9553 | 1/1 | **0.9821** |
| 81 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.9511 | 1/1 | **0.9804** |
| 82 | 🟢 classic82_before_after_images | 0.9963 | 0.9313 | 1/1 | **0.971** |
| 83 | 🟢 classic83_color_swatch_palette | 0.9783 | 0.9593 | 1/1 | **0.975** |
| 84 | 🟢 classic84_travel_destination_cards | 1.0 | 0.9139 | 1/1 | **0.9656** |
| 85 | 🟢 classic85_lab_results_with_image | 0.9978 | 0.9634 | 1/1 | **0.9845** |
| 86 | 🟢 classic86_software_screenshot_features | 0.9833 | 0.9433 | 1/1 | **0.9706** |
| 87 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.9803 | 1/1 | **0.9921** |
| 88 | 🟢 classic88_image_after_data | 1.0 | 0.9674 | 1/1 | **0.987** |
| 89 | 🟢 classic89_nutrition_label_with_image | 0.988 | 0.9637 | 1/1 | **0.9807** |
| 90 | 🟢 classic90_project_status_with_milestones | 0.9926 | 0.9515 | 1/1 | **0.9776** |
| 91 | 🟢 classic91_simple_bar_chart | 0.8732 | 0.9387 | 2/2 | **0.9248** |
| 92 | 🟡 classic92_horizontal_bar_chart | 0.6833 | 0.9481 | 2/2 | **0.8526** |
| 93 | 🟢 classic93_line_chart | 0.866 | 0.9829 | 2/2 | **0.9396** |
| 94 | 🟢 classic94_pie_chart | 0.8734 | 0.9298 | 2/2 | **0.9213** |
| 95 | 🟢 classic95_area_chart | 0.9 | 0.9368 | 2/2 | **0.9347** |
| 96 | 🟢 classic96_scatter_chart | 0.7826 | 0.9764 | 2/2 | **0.9036** |
| 97 | 🟢 classic97_doughnut_chart | 0.872 | 0.9326 | 2/2 | **0.9218** |
| 98 | 🟡 classic98_radar_chart | 0.7416 | 0.9861 | 2/2 | **0.8911** |
| 99 | 🟢 classic99_bubble_chart | 0.8509 | 0.9759 | 2/2 | **0.9307** |
| 100 | 🟡 classic100_stacked_bar_chart | 0.7867 | 0.875 | 1/1 | **0.8647** |
| 101 | 🟡 classic101_percent_stacked_bar | 0.7778 | 0.847 | 1/1 | **0.8499** |
| 102 | 🟢 classic102_line_chart_with_markers | 0.7723 | 0.9834 | 2/2 | **0.9023** |
| 103 | 🟡 classic103_pie_chart_with_labels | 0.5474 | 0.9408 | 2/2 | **0.7953** |
| 104 | 🟡 classic104_combo_bar_line_chart | 0.7761 | 0.9315 | 2/2 | **0.883** |
| 105 | 🟡 classic105_3d_bar_chart | 0.7208 | 0.9261 | 2/2 | **0.8588** |
| 106 | 🟢 classic106_3d_pie_chart | 0.9366 | 0.946 | 2/2 | **0.953** |
| 107 | 🟢 classic107_multi_series_line | 0.848 | 0.9635 | 2/2 | **0.9246** |
| 108 | 🔴 classic108_stacked_area_chart | 0.7974 | 0.3854 | 1/2 | **0.5731** |
| 109 | 🟡 classic109_scatter_with_trendline | 0.7037 | 0.9793 | 2/2 | **0.8732** |
| 110 | 🟡 classic110_chart_with_legend | 0.7778 | 0.9574 | 2/2 | **0.8941** |
| 111 | 🟡 classic111_chart_with_axis_labels | 0.735 | 0.9603 | 2/2 | **0.8781** |
| 112 | 🟡 classic112_multiple_charts | 0.6948 | 0.9372 | 2/2 | **0.8528** |
| 113 | 🟡 classic113_chart_sheet | 0.7442 | 0.9128 | 2/2 | **0.8628** |
| 114 | 🟢 classic114_chart_large_dataset | 0.9379 | 0.9774 | 4/4 | **0.9661** |
| 115 | 🟢 classic115_chart_negative_values | 0.8421 | 0.9627 | 2/2 | **0.9219** |
| 116 | 🔴 classic116_percent_stacked_area | 0.7755 | 0.3547 | 1/2 | **0.5521** |
| 117 | 🟡 classic117_stock_ohlc_chart | 0.8072 | 0.9028 | 2/2 | **0.884** |
| 118 | 🟢 classic118_bar_chart_custom_colors | 0.8344 | 0.9543 | 2/2 | **0.9155** |
| 119 | 🟡 classic119_dashboard_multi_charts | 0.8253 | 0.9084 | 2/2 | **0.8935** |
| 120 | 🟡 classic120_chart_with_date_axis | 0.4138 | 0.9748 | 2/2 | **0.7554** |
| 121 | 🟢 classic121_thin_borders | 1.0 | 0.9848 | 1/1 | **0.9939** |
| 122 | 🟢 classic122_thick_outer_thin_inner | 1.0 | 0.9808 | 1/1 | **0.9923** |
| 123 | 🟢 classic123_dashed_borders | 0.9882 | 0.9899 | 1/1 | **0.9912** |
| 124 | 🟢 classic124_colored_borders | 1.0 | 0.981 | 1/1 | **0.9924** |
| 125 | 🟢 classic125_solid_fills | 0.9845 | 0.9713 | 1/1 | **0.9823** |
| 126 | 🟢 classic126_dark_header | 0.993 | 0.9781 | 1/1 | **0.9884** |
| 127 | 🟢 classic127_font_styles | 0.9982 | 0.9788 | 1/1 | **0.9908** |
| 128 | 🟢 classic128_font_sizes | 1.0 | 0.985 | 1/1 | **0.994** |
| 129 | 🟢 classic129_alignment_combos | 1.0 | 0.9906 | 1/1 | **0.9962** |
| 130 | 🟢 classic130_wrap_and_indent | 1.0 | 0.9887 | 1/1 | **0.9955** |
| 131 | 🟢 classic131_number_formats | 1.0 | 0.98 | 1/1 | **0.992** |
| 132 | 🟢 classic132_striped_table | 0.9984 | 0.9267 | 1/1 | **0.97** |
| 133 | 🟢 classic133_gradient_rows | 1.0 | 0.927 | 1/1 | **0.9708** |
| 134 | 🟢 classic134_heatmap | 1.0 | 0.91 | 1/1 | **0.964** |
| 135 | 🟢 classic135_bottom_border_only | 1.0 | 0.9891 | 1/1 | **0.9956** |
| 136 | 🟢 classic136_financial_report_styled | 1.0 | 0.9532 | 1/1 | **0.9813** |
| 137 | 🟢 classic137_checkerboard | 1.0 | 0.8608 | 1/1 | **0.9443** |
| 138 | 🟢 classic138_color_grid | 1.0 | 0.9296 | 1/1 | **0.9718** |
| 139 | 🟢 classic139_pattern_fills | 1.0 | 0.9311 | 1/1 | **0.9724** |
| 140 | 🟢 classic140_rotated_text | 0.9583 | 0.9902 | 1/1 | **0.9794** |
| 141 | 🟢 classic141_mixed_edge_borders | 1.0 | 0.9846 | 1/1 | **0.9938** |
| 142 | 🟢 classic142_styled_invoice | 1.0 | 0.9125 | 1/1 | **0.965** |
| 143 | 🟢 classic143_colored_tabs | 0.9972 | 0.9971 | 4/4 | **0.9977** |
| 144 | 🟢 classic144_note_style_cells | 1.0 | 0.9717 | 1/1 | **0.9887** |
| 145 | 🟢 classic145_status_badges | 1.0 | 0.9374 | 1/1 | **0.975** |
| 146 | 🟢 classic146_double_border_table | 1.0 | 0.9731 | 1/1 | **0.9892** |
| 147 | 🟢 classic147_multi_sheet_styled | 1.0 | 0.9766 | 3/3 | **0.9906** |
| 148 | 🟡 classic148_frozen_styled_grid | 1.0 | 0.7318 | 1/1 | **0.8927** |
| 149 | 🟢 classic149_merged_styled_sections | 1.0 | 0.9201 | 1/1 | **0.968** |
| 150 | 🟢 classic150_kitchen_sink_styles | 0.9839 | 0.9354 | 1/1 | **0.9677** |
| 151 | 🟢 classic151_multilingual_greetings | 0.9291 | 0.9808 | 1/1 | **0.964** |
| 152 | 🟢 classic152_emoji_sampler | 1.0 | 0.9818 | 1/1 | **0.9927** |
| 153 | 🟢 classic153_currency_symbols | 0.9918 | 0.9796 | 1/1 | **0.9886** |
| 154 | 🟢 classic154_math_symbols | 0.9881 | 0.986 | 1/1 | **0.9896** |
| 155 | 🟢 classic155_diacritical_marks | 1.0 | 0.9888 | 1/1 | **0.9955** |
| 156 | 🟡 classic156_rtl_bidi_text | 0.5912 | 0.9953 | 1/1 | **0.8346** |
| 157 | 🟢 classic157_cjk_extended | 1.0 | 0.9761 | 1/1 | **0.9904** |
| 158 | 🟢 classic158_emoji_skin_tones | 1.0 | 0.9819 | 1/1 | **0.9928** |
| 159 | 🟢 classic159_zwj_emoji | 0.9372 | 0.985 | 1/1 | **0.9689** |
| 160 | 🟢 classic160_punctuation_marks | 0.9915 | 0.9914 | 1/1 | **0.9932** |
| 161 | 🟢 classic161_box_drawing | 0.9976 | 0.9854 | 1/1 | **0.9932** |
| 162 | 🟢 classic162_cjk_emoji_styled | 1.0 | 0.9804 | 1/1 | **0.9922** |
| 163 | 🟢 classic163_cyrillic_alphabets | 0.9398 | 0.9832 | 1/1 | **0.9692** |
| 164 | 🟢 classic164_indic_scripts | 1.0 | 0.9945 | 1/1 | **0.9978** |
| 165 | 🟢 classic165_southeast_asian | 0.9672 | 0.9923 | 1/1 | **0.9838** |
| 166 | 🟢 classic166_emoji_progress | 1.0 | 0.9691 | 1/1 | **0.9876** |
| 167 | 🟢 classic167_musical_symbols | 1.0 | 0.9886 | 1/1 | **0.9954** |
| 168 | 🟢 classic168_mixed_ltr_rtl_styled | 0.8696 | 0.9732 | 1/1 | **0.9371** |
| 169 | 🟢 classic169_korean_invoice | 1.0 | 0.9788 | 1/1 | **0.9915** |
| 170 | 🟢 classic170_emoji_dashboard | 1.0 | 0.9654 | 1/1 | **0.9862** |
| 171 | 🟢 classic171_ipa_phonetic | 0.9703 | 0.9867 | 1/1 | **0.9828** |
| 172 | 🟢 classic172_emoji_timeline | 1.0 | 0.9769 | 1/1 | **0.9908** |
| 173 | 🟢 classic173_african_languages | 0.9783 | 0.983 | 1/1 | **0.9845** |
| 174 | 🟢 classic174_technical_symbols | 0.9971 | 0.9797 | 1/1 | **0.9907** |
| 175 | 🟢 classic175_multiscript_catalog | 0.9686 | 0.9728 | 1/1 | **0.9766** |
| 176 | 🟢 classic176_combining_characters | 0.9469 | 0.9861 | 1/1 | **0.9732** |
| 177 | 🟢 classic177_emoji_calendar | 1.0 | 0.9771 | 1/1 | **0.9908** |
| 178 | 🟢 classic178_caucasus_ethiopic | 0.9894 | 0.9849 | 1/1 | **0.9897** |
| 179 | 🟢 classic179_emoji_inventory | 1.0 | 0.9655 | 1/1 | **0.9862** |
| 180 | 🟢 classic180_polyglot_paragraph | 0.9772 | 0.9868 | 1/1 | **0.9856** |
| 181 | 🟢 classic181_feedback_tracker_with_images | 0.9865 | 0.8748 | 2/2 | **0.9445** |
| 182 | 🟢 classic182_dense_long_text_columns | 0.9291 | 0.9603 | 2/2 | **0.9558** |
| 183 | 🟢 classic183_mixed_content_grid | 1.0 | 0.9357 | 1/1 | **0.9743** |
| 184 | 🟢 classic184_wide_narrow_columns | 1.0 | 0.9142 | 1/1 | **0.9657** |
| 185 | 🟢 classic185_tall_rows_vertical_align | 1.0 | 0.9805 | 1/1 | **0.9922** |
| 186 | 🟢 classic186_multi_sheet_image_report | 1.0 | 0.9353 | 2/2 | **0.9741** |
| 187 | 🟢 classic187_bug_report_with_screenshots | 1.0 | 0.8547 | 1/1 | **0.9419** |
| 188 | 🟢 classic188_merged_header_with_images | 1.0 | 0.9447 | 1/1 | **0.9779** |
| 189 | 🟢 classic189_alternating_image_text_rows | 0.8974 | 0.8632 | 1/1 | **0.9042** |
| 190 | 🟢 classic190_dashboard_kpi_images | 1.0 | 0.9437 | 1/1 | **0.9775** |
| 191 | 🟡 classic191_payroll_calculator | 0.7135 | 0.895 | 9/9 | **0.8434** |
| 192 | 🟡 nthu_article | 0.9324 | 0.8205 | 21/20 | **0.8012** |
| 193 | ⚪ nthu_article_new | N/A | N/A | ?/? | **N/A** |
| 194 | ⚪ nthu_article_old | N/A | N/A | ?/? | **N/A** |

**Average Overall Score: 0.9470**

## Visual Comparison

<table>
<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>
<tr>
  <td><b>classic01_basic_table_with_headers</b></td>
  <td colspan="1">classic01_basic_table_with_headers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic01_basic_table_with_headers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic01_basic_table_with_headers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic02_multiple_worksheets</b></td>
  <td colspan="1">classic02_multiple_worksheets <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic02_multiple_worksheets_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic02_multiple_worksheets_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic02_multiple_worksheets_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic02_multiple_worksheets_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic02_multiple_worksheets_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic02_multiple_worksheets_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic03_empty_workbook</b></td>
  <td colspan="1">classic03_empty_workbook <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic03_empty_workbook_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic03_empty_workbook_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic04_single_cell</b></td>
  <td colspan="1">classic04_single_cell <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic04_single_cell_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic04_single_cell_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic05_wide_table</b></td>
  <td colspan="1">classic05_wide_table <span style="color:#3fb950">⬤</span> 95.0%</td>
</tr>
<tr>
  <td><img src="images/classic05_wide_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic05_wide_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic05_wide_table_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic05_wide_table_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic05_wide_table_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic05_wide_table_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic06_tall_table</b></td>
  <td colspan="1">classic06_tall_table <span style="color:#3fb950">⬤</span> 94.5%</td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic06_tall_table_p5_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic06_tall_table_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic07_numbers_only</b></td>
  <td colspan="1">classic07_numbers_only <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic07_numbers_only_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic07_numbers_only_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic08_mixed_text_and_numbers</b></td>
  <td colspan="1">classic08_mixed_text_and_numbers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic08_mixed_text_and_numbers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic08_mixed_text_and_numbers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic09_long_text</b></td>
  <td colspan="1">classic09_long_text <span style="color:#d29922">⬤</span> 71.6%</td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p5_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p6_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p6_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p7_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p7_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p8_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p8_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p9_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p9_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p10_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p10_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p11_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p11_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic09_long_text_p12_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic09_long_text_p12_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic10_special_xml_characters</b></td>
  <td colspan="1">classic10_special_xml_characters <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic10_special_xml_characters_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic10_special_xml_characters_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic11_sparse_rows</b></td>
  <td colspan="1">classic11_sparse_rows <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic11_sparse_rows_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic11_sparse_rows_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic11_sparse_rows_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic11_sparse_rows_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic12_sparse_columns</b></td>
  <td colspan="1">classic12_sparse_columns <span style="color:#f85149">⬤</span> 70.0%</td>
</tr>
<tr>
  <td><img src="images/classic12_sparse_columns_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic12_sparse_columns_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic12_sparse_columns_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic13_date_strings</b></td>
  <td colspan="1">classic13_date_strings <span style="color:#3fb950">⬤</span> 97.7%</td>
</tr>
<tr>
  <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic14_decimal_numbers</b></td>
  <td colspan="1">classic14_decimal_numbers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic14_decimal_numbers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic14_decimal_numbers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic15_negative_numbers</b></td>
  <td colspan="1">classic15_negative_numbers <span style="color:#3fb950">⬤</span> 97.3%</td>
</tr>
<tr>
  <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic16_percentage_strings</b></td>
  <td colspan="1">classic16_percentage_strings <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic17_currency_strings</b></td>
  <td colspan="1">classic17_currency_strings <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic18_large_dataset</b></td>
  <td colspan="1">classic18_large_dataset <span style="color:#f85149">⬤</span> 63.7%</td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p5_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p6_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p6_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p7_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p7_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p8_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p8_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p9_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p9_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p10_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p10_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p11_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p11_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p12_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p12_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p13_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p13_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p14_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p14_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p15_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p15_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p16_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p16_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p17_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p17_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p18_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p18_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p19_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p19_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p20_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p20_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p21_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p21_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p22_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p22_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p23_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p23_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic18_large_dataset_p24_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic18_large_dataset_p24_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p25_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p26_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p27_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p28_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p29_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p30_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p31_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p32_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p33_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p34_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p35_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p36_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p37_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p38_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p39_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p40_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p41_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic18_large_dataset_p42_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic19_single_column_list</b></td>
  <td colspan="1">classic19_single_column_list <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic19_single_column_list_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic19_single_column_list_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic20_all_empty_cells</b></td>
  <td colspan="1">classic20_all_empty_cells <span style="color:#3fb950">⬤</span> 100.0%</td>
</tr>
<tr>
  <td><img src="images/classic20_all_empty_cells_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic20_all_empty_cells_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic21_header_only</b></td>
  <td colspan="1">classic21_header_only <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic21_header_only_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic21_header_only_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic22_long_sheet_name</b></td>
  <td colspan="1">classic22_long_sheet_name <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic22_long_sheet_name_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic22_long_sheet_name_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic23_unicode_text</b></td>
  <td colspan="1">classic23_unicode_text <span style="color:#3fb950">⬤</span> 95.8%</td>
</tr>
<tr>
  <td><img src="images/classic23_unicode_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic23_unicode_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic24_red_text</b></td>
  <td colspan="1">classic24_red_text <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic24_red_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic24_red_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic25_multiple_colors</b></td>
  <td colspan="1">classic25_multiple_colors <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic25_multiple_colors_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic25_multiple_colors_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic26_inline_strings</b></td>
  <td colspan="1">classic26_inline_strings <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic27_single_row</b></td>
  <td colspan="1">classic27_single_row <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic27_single_row_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic27_single_row_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic28_duplicate_values</b></td>
  <td colspan="1">classic28_duplicate_values <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic28_duplicate_values_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic28_duplicate_values_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic29_formula_results</b></td>
  <td colspan="1">classic29_formula_results <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic30_mixed_empty_and_filled_sheets</b></td>
  <td colspan="1">classic30_mixed_empty_and_filled_sheets <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic30_mixed_empty_and_filled_sheets_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic31_bold_header_row</b></td>
  <td colspan="1">classic31_bold_header_row <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic31_bold_header_row_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic31_bold_header_row_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic32_right_aligned_numbers</b></td>
  <td colspan="1">classic32_right_aligned_numbers <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic32_right_aligned_numbers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic32_right_aligned_numbers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic33_centered_text</b></td>
  <td colspan="1">classic33_centered_text <span style="color:#3fb950">⬤</span> 99.9%</td>
</tr>
<tr>
  <td><img src="images/classic33_centered_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic33_centered_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic34_explicit_column_widths</b></td>
  <td colspan="1">classic34_explicit_column_widths <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic34_explicit_column_widths_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic34_explicit_column_widths_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic35_explicit_row_heights</b></td>
  <td colspan="1">classic35_explicit_row_heights <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic36_merged_cells</b></td>
  <td colspan="1">classic36_merged_cells <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic37_freeze_panes</b></td>
  <td colspan="1">classic37_freeze_panes <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic37_freeze_panes_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic37_freeze_panes_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic38_hyperlink_cell</b></td>
  <td colspan="1">classic38_hyperlink_cell <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic38_hyperlink_cell_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic38_hyperlink_cell_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic39_financial_table</b></td>
  <td colspan="1">classic39_financial_table <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic40_scientific_notation</b></td>
  <td colspan="1">classic40_scientific_notation <span style="color:#3fb950">⬤</span> 97.8%</td>
</tr>
<tr>
  <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic41_integer_vs_float</b></td>
  <td colspan="1">classic41_integer_vs_float <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic41_integer_vs_float_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic41_integer_vs_float_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic42_boolean_values</b></td>
  <td colspan="1">classic42_boolean_values <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic42_boolean_values_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic42_boolean_values_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic43_inventory_report</b></td>
  <td colspan="1">classic43_inventory_report <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic44_employee_roster</b></td>
  <td colspan="1">classic44_employee_roster <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic45_sales_by_region</b></td>
  <td colspan="1">classic45_sales_by_region <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic45_sales_by_region_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic45_sales_by_region_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic45_sales_by_region_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic45_sales_by_region_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic45_sales_by_region_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic45_sales_by_region_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic45_sales_by_region_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic45_sales_by_region_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic46_grade_book</b></td>
  <td colspan="1">classic46_grade_book <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic46_grade_book_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic46_grade_book_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic47_time_series</b></td>
  <td colspan="1">classic47_time_series <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic48_survey_results</b></td>
  <td colspan="1">classic48_survey_results <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic49_contact_list</b></td>
  <td colspan="1">classic49_contact_list <span style="color:#3fb950">⬤</span> 95.0%</td>
</tr>
<tr>
  <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic50_budget_vs_actuals</b></td>
  <td colspan="1">classic50_budget_vs_actuals <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic50_budget_vs_actuals_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic50_budget_vs_actuals_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic50_budget_vs_actuals_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic50_budget_vs_actuals_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic51_product_catalog</b></td>
  <td colspan="1">classic51_product_catalog <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic51_product_catalog_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic51_product_catalog_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic52_pivot_summary</b></td>
  <td colspan="1">classic52_pivot_summary <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic53_invoice</b></td>
  <td colspan="1">classic53_invoice <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic54_multi_level_header</b></td>
  <td colspan="1">classic54_multi_level_header <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic55_error_values</b></td>
  <td colspan="1">classic55_error_values <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic55_error_values_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic55_error_values_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic56_alternating_row_colors</b></td>
  <td colspan="1">classic56_alternating_row_colors <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic56_alternating_row_colors_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic56_alternating_row_colors_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic57_cjk_only</b></td>
  <td colspan="1">classic57_cjk_only <span style="color:#3fb950">⬤</span> 99.7%</td>
</tr>
<tr>
  <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic58_mixed_numeric_formats</b></td>
  <td colspan="1">classic58_mixed_numeric_formats <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic59_multi_sheet_summary</b></td>
  <td colspan="1">classic59_multi_sheet_summary <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic59_multi_sheet_summary_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic59_multi_sheet_summary_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic59_multi_sheet_summary_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic59_multi_sheet_summary_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic59_multi_sheet_summary_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic59_multi_sheet_summary_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic60_large_wide_table</b></td>
  <td colspan="1">classic60_large_wide_table <span style="color:#f85149">⬤</span> 67.2%</td>
</tr>
<tr>
  <td><img src="images/classic60_large_wide_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic60_large_wide_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic60_large_wide_table_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic60_large_wide_table_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic60_large_wide_table_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic60_large_wide_table_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic60_large_wide_table_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic60_large_wide_table_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic60_large_wide_table_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic60_large_wide_table_p6_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic61_product_card_with_image</b></td>
  <td colspan="1">classic61_product_card_with_image <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic61_product_card_with_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic61_product_card_with_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic62_company_logo_header</b></td>
  <td colspan="1">classic62_company_logo_header <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic63_two_products_side_by_side</b></td>
  <td colspan="1">classic63_two_products_side_by_side <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic64_employee_directory_with_photo</b></td>
  <td colspan="1">classic64_employee_directory_with_photo <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic65_inventory_with_product_photos</b></td>
  <td colspan="1">classic65_inventory_with_product_photos <span style="color:#3fb950">⬤</span> 98.3%</td>
</tr>
<tr>
  <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic66_invoice_with_logo</b></td>
  <td colspan="1">classic66_invoice_with_logo <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic67_real_estate_listing</b></td>
  <td colspan="1">classic67_real_estate_listing <span style="color:#3fb950">⬤</span> 97.9%</td>
</tr>
<tr>
  <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic68_restaurant_menu</b></td>
  <td colspan="1">classic68_restaurant_menu <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic68_restaurant_menu_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic68_restaurant_menu_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic69_image_only_sheet</b></td>
  <td colspan="1">classic69_image_only_sheet <span style="color:#3fb950">⬤</span> 96.5%</td>
</tr>
<tr>
  <td><img src="images/classic69_image_only_sheet_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic69_image_only_sheet_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic70_product_catalog_with_images</b></td>
  <td colspan="1">classic70_product_catalog_with_images <span style="color:#3fb950">⬤</span> 97.2%</td>
</tr>
<tr>
  <td><img src="images/classic70_product_catalog_with_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic70_product_catalog_with_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic71_multi_sheet_with_images</b></td>
  <td colspan="1">classic71_multi_sheet_with_images <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic71_multi_sheet_with_images_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic71_multi_sheet_with_images_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic71_multi_sheet_with_images_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic71_multi_sheet_with_images_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic72_bar_chart_image_with_data</b></td>
  <td colspan="1">classic72_bar_chart_image_with_data <span style="color:#3fb950">⬤</span> 97.5%</td>
</tr>
<tr>
  <td><img src="images/classic72_bar_chart_image_with_data_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic72_bar_chart_image_with_data_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic73_event_flyer_with_banner</b></td>
  <td colspan="1">classic73_event_flyer_with_banner <span style="color:#3fb950">⬤</span> 95.2%</td>
</tr>
<tr>
  <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic74_dashboard_with_kpi_image</b></td>
  <td colspan="1">classic74_dashboard_with_kpi_image <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic75_certificate_with_seal</b></td>
  <td colspan="1">classic75_certificate_with_seal <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic76_product_image_grid</b></td>
  <td colspan="1">classic76_product_image_grid <span style="color:#3fb950">⬤</span> 97.6%</td>
</tr>
<tr>
  <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic77_news_article_with_hero_image</b></td>
  <td colspan="1">classic77_news_article_with_hero_image <span style="color:#3fb950">⬤</span> 96.6%</td>
</tr>
<tr>
  <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic78_small_icon_per_row</b></td>
  <td colspan="1">classic78_small_icon_per_row <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic79_wide_panoramic_banner</b></td>
  <td colspan="1">classic79_wide_panoramic_banner <span style="color:#3fb950">⬤</span> 97.9%</td>
</tr>
<tr>
  <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic80_portrait_tall_image</b></td>
  <td colspan="1">classic80_portrait_tall_image <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic81_step_by_step_with_images</b></td>
  <td colspan="1">classic81_step_by_step_with_images <span style="color:#3fb950">⬤</span> 98.0%</td>
</tr>
<tr>
  <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic82_before_after_images</b></td>
  <td colspan="1">classic82_before_after_images <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic83_color_swatch_palette</b></td>
  <td colspan="1">classic83_color_swatch_palette <span style="color:#3fb950">⬤</span> 97.5%</td>
</tr>
<tr>
  <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic84_travel_destination_cards</b></td>
  <td colspan="1">classic84_travel_destination_cards <span style="color:#3fb950">⬤</span> 96.6%</td>
</tr>
<tr>
  <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic85_lab_results_with_image</b></td>
  <td colspan="1">classic85_lab_results_with_image <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic86_software_screenshot_features</b></td>
  <td colspan="1">classic86_software_screenshot_features <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic87_sports_results_with_logos</b></td>
  <td colspan="1">classic87_sports_results_with_logos <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic88_image_after_data</b></td>
  <td colspan="1">classic88_image_after_data <span style="color:#3fb950">⬤</span> 98.7%</td>
</tr>
<tr>
  <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic89_nutrition_label_with_image</b></td>
  <td colspan="1">classic89_nutrition_label_with_image <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic90_project_status_with_milestones</b></td>
  <td colspan="1">classic90_project_status_with_milestones <span style="color:#3fb950">⬤</span> 97.8%</td>
</tr>
<tr>
  <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic91_simple_bar_chart</b></td>
  <td colspan="1">classic91_simple_bar_chart <span style="color:#3fb950">⬤</span> 92.5%</td>
</tr>
<tr>
  <td><img src="images/classic91_simple_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic91_simple_bar_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic91_simple_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic91_simple_bar_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic92_horizontal_bar_chart</b></td>
  <td colspan="1">classic92_horizontal_bar_chart <span style="color:#d29922">⬤</span> 85.3%</td>
</tr>
<tr>
  <td><img src="images/classic92_horizontal_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic92_horizontal_bar_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic92_horizontal_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic92_horizontal_bar_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic93_line_chart</b></td>
  <td colspan="1">classic93_line_chart <span style="color:#3fb950">⬤</span> 94.0%</td>
</tr>
<tr>
  <td><img src="images/classic93_line_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic93_line_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic93_line_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic93_line_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic94_pie_chart</b></td>
  <td colspan="1">classic94_pie_chart <span style="color:#3fb950">⬤</span> 92.1%</td>
</tr>
<tr>
  <td><img src="images/classic94_pie_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic94_pie_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic94_pie_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic94_pie_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic95_area_chart</b></td>
  <td colspan="1">classic95_area_chart <span style="color:#3fb950">⬤</span> 93.5%</td>
</tr>
<tr>
  <td><img src="images/classic95_area_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic95_area_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic95_area_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic95_area_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic96_scatter_chart</b></td>
  <td colspan="1">classic96_scatter_chart <span style="color:#3fb950">⬤</span> 90.4%</td>
</tr>
<tr>
  <td><img src="images/classic96_scatter_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic96_scatter_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic96_scatter_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic96_scatter_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic97_doughnut_chart</b></td>
  <td colspan="1">classic97_doughnut_chart <span style="color:#3fb950">⬤</span> 92.2%</td>
</tr>
<tr>
  <td><img src="images/classic97_doughnut_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic97_doughnut_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic97_doughnut_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic97_doughnut_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic98_radar_chart</b></td>
  <td colspan="1">classic98_radar_chart <span style="color:#d29922">⬤</span> 89.1%</td>
</tr>
<tr>
  <td><img src="images/classic98_radar_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic98_radar_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic98_radar_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic98_radar_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic99_bubble_chart</b></td>
  <td colspan="1">classic99_bubble_chart <span style="color:#3fb950">⬤</span> 93.1%</td>
</tr>
<tr>
  <td><img src="images/classic99_bubble_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic99_bubble_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic99_bubble_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic99_bubble_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic100_stacked_bar_chart</b></td>
  <td colspan="1">classic100_stacked_bar_chart <span style="color:#d29922">⬤</span> 86.5%</td>
</tr>
<tr>
  <td><img src="images/classic100_stacked_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic100_stacked_bar_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic101_percent_stacked_bar</b></td>
  <td colspan="1">classic101_percent_stacked_bar <span style="color:#d29922">⬤</span> 85.0%</td>
</tr>
<tr>
  <td><img src="images/classic101_percent_stacked_bar_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic101_percent_stacked_bar_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic102_line_chart_with_markers</b></td>
  <td colspan="1">classic102_line_chart_with_markers <span style="color:#3fb950">⬤</span> 90.2%</td>
</tr>
<tr>
  <td><img src="images/classic102_line_chart_with_markers_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic102_line_chart_with_markers_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic102_line_chart_with_markers_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic102_line_chart_with_markers_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic103_pie_chart_with_labels</b></td>
  <td colspan="1">classic103_pie_chart_with_labels <span style="color:#d29922">⬤</span> 79.5%</td>
</tr>
<tr>
  <td><img src="images/classic103_pie_chart_with_labels_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic103_pie_chart_with_labels_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic103_pie_chart_with_labels_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic103_pie_chart_with_labels_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic104_combo_bar_line_chart</b></td>
  <td colspan="1">classic104_combo_bar_line_chart <span style="color:#d29922">⬤</span> 88.3%</td>
</tr>
<tr>
  <td><img src="images/classic104_combo_bar_line_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic104_combo_bar_line_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic104_combo_bar_line_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic104_combo_bar_line_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic105_3d_bar_chart</b></td>
  <td colspan="1">classic105_3d_bar_chart <span style="color:#d29922">⬤</span> 85.9%</td>
</tr>
<tr>
  <td><img src="images/classic105_3d_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic105_3d_bar_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic105_3d_bar_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic105_3d_bar_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic106_3d_pie_chart</b></td>
  <td colspan="1">classic106_3d_pie_chart <span style="color:#3fb950">⬤</span> 95.3%</td>
</tr>
<tr>
  <td><img src="images/classic106_3d_pie_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic106_3d_pie_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic106_3d_pie_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic106_3d_pie_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic107_multi_series_line</b></td>
  <td colspan="1">classic107_multi_series_line <span style="color:#3fb950">⬤</span> 92.5%</td>
</tr>
<tr>
  <td><img src="images/classic107_multi_series_line_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic107_multi_series_line_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic107_multi_series_line_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic107_multi_series_line_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic108_stacked_area_chart</b></td>
  <td colspan="1">classic108_stacked_area_chart <span style="color:#f85149">⬤</span> 57.3%</td>
</tr>
<tr>
  <td><img src="images/classic108_stacked_area_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic108_stacked_area_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic108_stacked_area_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic109_scatter_with_trendline</b></td>
  <td colspan="1">classic109_scatter_with_trendline <span style="color:#d29922">⬤</span> 87.3%</td>
</tr>
<tr>
  <td><img src="images/classic109_scatter_with_trendline_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic109_scatter_with_trendline_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic109_scatter_with_trendline_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic109_scatter_with_trendline_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic110_chart_with_legend</b></td>
  <td colspan="1">classic110_chart_with_legend <span style="color:#d29922">⬤</span> 89.4%</td>
</tr>
<tr>
  <td><img src="images/classic110_chart_with_legend_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic110_chart_with_legend_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic110_chart_with_legend_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic110_chart_with_legend_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic111_chart_with_axis_labels</b></td>
  <td colspan="1">classic111_chart_with_axis_labels <span style="color:#d29922">⬤</span> 87.8%</td>
</tr>
<tr>
  <td><img src="images/classic111_chart_with_axis_labels_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic111_chart_with_axis_labels_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic111_chart_with_axis_labels_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic111_chart_with_axis_labels_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic112_multiple_charts</b></td>
  <td colspan="1">classic112_multiple_charts <span style="color:#d29922">⬤</span> 85.3%</td>
</tr>
<tr>
  <td><img src="images/classic112_multiple_charts_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic112_multiple_charts_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic112_multiple_charts_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic112_multiple_charts_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic113_chart_sheet</b></td>
  <td colspan="1">classic113_chart_sheet <span style="color:#d29922">⬤</span> 86.3%</td>
</tr>
<tr>
  <td><img src="images/classic113_chart_sheet_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic113_chart_sheet_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic113_chart_sheet_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic113_chart_sheet_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic114_chart_large_dataset</b></td>
  <td colspan="1">classic114_chart_large_dataset <span style="color:#3fb950">⬤</span> 96.6%</td>
</tr>
<tr>
  <td><img src="images/classic114_chart_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic114_chart_large_dataset_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic114_chart_large_dataset_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic114_chart_large_dataset_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic114_chart_large_dataset_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic114_chart_large_dataset_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic114_chart_large_dataset_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic114_chart_large_dataset_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic115_chart_negative_values</b></td>
  <td colspan="1">classic115_chart_negative_values <span style="color:#3fb950">⬤</span> 92.2%</td>
</tr>
<tr>
  <td><img src="images/classic115_chart_negative_values_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic115_chart_negative_values_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic115_chart_negative_values_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic115_chart_negative_values_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic116_percent_stacked_area</b></td>
  <td colspan="1">classic116_percent_stacked_area <span style="color:#f85149">⬤</span> 55.2%</td>
</tr>
<tr>
  <td><img src="images/classic116_percent_stacked_area_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic116_percent_stacked_area_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><i>missing</i></td>
  <td><img src="images/classic116_percent_stacked_area_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic117_stock_ohlc_chart</b></td>
  <td colspan="1">classic117_stock_ohlc_chart <span style="color:#d29922">⬤</span> 88.4%</td>
</tr>
<tr>
  <td><img src="images/classic117_stock_ohlc_chart_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic117_stock_ohlc_chart_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic117_stock_ohlc_chart_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic117_stock_ohlc_chart_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic118_bar_chart_custom_colors</b></td>
  <td colspan="1">classic118_bar_chart_custom_colors <span style="color:#3fb950">⬤</span> 91.5%</td>
</tr>
<tr>
  <td><img src="images/classic118_bar_chart_custom_colors_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic118_bar_chart_custom_colors_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic118_bar_chart_custom_colors_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic118_bar_chart_custom_colors_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic119_dashboard_multi_charts</b></td>
  <td colspan="1">classic119_dashboard_multi_charts <span style="color:#d29922">⬤</span> 89.3%</td>
</tr>
<tr>
  <td><img src="images/classic119_dashboard_multi_charts_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic119_dashboard_multi_charts_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic119_dashboard_multi_charts_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic119_dashboard_multi_charts_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic120_chart_with_date_axis</b></td>
  <td colspan="1">classic120_chart_with_date_axis <span style="color:#d29922">⬤</span> 75.5%</td>
</tr>
<tr>
  <td><img src="images/classic120_chart_with_date_axis_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic120_chart_with_date_axis_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic120_chart_with_date_axis_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic120_chart_with_date_axis_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic121_thin_borders</b></td>
  <td colspan="1">classic121_thin_borders <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic121_thin_borders_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic121_thin_borders_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic122_thick_outer_thin_inner</b></td>
  <td colspan="1">classic122_thick_outer_thin_inner <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic122_thick_outer_thin_inner_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic122_thick_outer_thin_inner_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic123_dashed_borders</b></td>
  <td colspan="1">classic123_dashed_borders <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic123_dashed_borders_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic123_dashed_borders_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic124_colored_borders</b></td>
  <td colspan="1">classic124_colored_borders <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic124_colored_borders_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic124_colored_borders_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic125_solid_fills</b></td>
  <td colspan="1">classic125_solid_fills <span style="color:#3fb950">⬤</span> 98.2%</td>
</tr>
<tr>
  <td><img src="images/classic125_solid_fills_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic125_solid_fills_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic126_dark_header</b></td>
  <td colspan="1">classic126_dark_header <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic126_dark_header_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic126_dark_header_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic127_font_styles</b></td>
  <td colspan="1">classic127_font_styles <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic127_font_styles_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic127_font_styles_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic128_font_sizes</b></td>
  <td colspan="1">classic128_font_sizes <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic128_font_sizes_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic128_font_sizes_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic129_alignment_combos</b></td>
  <td colspan="1">classic129_alignment_combos <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic129_alignment_combos_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic129_alignment_combos_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic130_wrap_and_indent</b></td>
  <td colspan="1">classic130_wrap_and_indent <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic130_wrap_and_indent_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic130_wrap_and_indent_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic131_number_formats</b></td>
  <td colspan="1">classic131_number_formats <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic131_number_formats_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic131_number_formats_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic132_striped_table</b></td>
  <td colspan="1">classic132_striped_table <span style="color:#3fb950">⬤</span> 97.0%</td>
</tr>
<tr>
  <td><img src="images/classic132_striped_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic132_striped_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic133_gradient_rows</b></td>
  <td colspan="1">classic133_gradient_rows <span style="color:#3fb950">⬤</span> 97.1%</td>
</tr>
<tr>
  <td><img src="images/classic133_gradient_rows_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic133_gradient_rows_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic134_heatmap</b></td>
  <td colspan="1">classic134_heatmap <span style="color:#3fb950">⬤</span> 96.4%</td>
</tr>
<tr>
  <td><img src="images/classic134_heatmap_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic134_heatmap_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic135_bottom_border_only</b></td>
  <td colspan="1">classic135_bottom_border_only <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic135_bottom_border_only_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic135_bottom_border_only_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic136_financial_report_styled</b></td>
  <td colspan="1">classic136_financial_report_styled <span style="color:#3fb950">⬤</span> 98.1%</td>
</tr>
<tr>
  <td><img src="images/classic136_financial_report_styled_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic136_financial_report_styled_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic137_checkerboard</b></td>
  <td colspan="1">classic137_checkerboard <span style="color:#3fb950">⬤</span> 94.4%</td>
</tr>
<tr>
  <td><img src="images/classic137_checkerboard_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic137_checkerboard_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic138_color_grid</b></td>
  <td colspan="1">classic138_color_grid <span style="color:#3fb950">⬤</span> 97.2%</td>
</tr>
<tr>
  <td><img src="images/classic138_color_grid_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic138_color_grid_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic139_pattern_fills</b></td>
  <td colspan="1">classic139_pattern_fills <span style="color:#3fb950">⬤</span> 97.2%</td>
</tr>
<tr>
  <td><img src="images/classic139_pattern_fills_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic139_pattern_fills_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic140_rotated_text</b></td>
  <td colspan="1">classic140_rotated_text <span style="color:#3fb950">⬤</span> 97.9%</td>
</tr>
<tr>
  <td><img src="images/classic140_rotated_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic140_rotated_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic141_mixed_edge_borders</b></td>
  <td colspan="1">classic141_mixed_edge_borders <span style="color:#3fb950">⬤</span> 99.4%</td>
</tr>
<tr>
  <td><img src="images/classic141_mixed_edge_borders_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic141_mixed_edge_borders_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic142_styled_invoice</b></td>
  <td colspan="1">classic142_styled_invoice <span style="color:#3fb950">⬤</span> 96.5%</td>
</tr>
<tr>
  <td><img src="images/classic142_styled_invoice_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic142_styled_invoice_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic143_colored_tabs</b></td>
  <td colspan="1">classic143_colored_tabs <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic143_colored_tabs_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic143_colored_tabs_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic143_colored_tabs_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic143_colored_tabs_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic143_colored_tabs_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic143_colored_tabs_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic143_colored_tabs_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic143_colored_tabs_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic144_note_style_cells</b></td>
  <td colspan="1">classic144_note_style_cells <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic144_note_style_cells_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic144_note_style_cells_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic145_status_badges</b></td>
  <td colspan="1">classic145_status_badges <span style="color:#3fb950">⬤</span> 97.5%</td>
</tr>
<tr>
  <td><img src="images/classic145_status_badges_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic145_status_badges_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic146_double_border_table</b></td>
  <td colspan="1">classic146_double_border_table <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic146_double_border_table_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic146_double_border_table_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic147_multi_sheet_styled</b></td>
  <td colspan="1">classic147_multi_sheet_styled <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic147_multi_sheet_styled_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic147_multi_sheet_styled_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic147_multi_sheet_styled_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic147_multi_sheet_styled_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic147_multi_sheet_styled_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic147_multi_sheet_styled_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic148_frozen_styled_grid</b></td>
  <td colspan="1">classic148_frozen_styled_grid <span style="color:#d29922">⬤</span> 89.3%</td>
</tr>
<tr>
  <td><img src="images/classic148_frozen_styled_grid_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic148_frozen_styled_grid_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic149_merged_styled_sections</b></td>
  <td colspan="1">classic149_merged_styled_sections <span style="color:#3fb950">⬤</span> 96.8%</td>
</tr>
<tr>
  <td><img src="images/classic149_merged_styled_sections_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic149_merged_styled_sections_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic150_kitchen_sink_styles</b></td>
  <td colspan="1">classic150_kitchen_sink_styles <span style="color:#3fb950">⬤</span> 96.8%</td>
</tr>
<tr>
  <td><img src="images/classic150_kitchen_sink_styles_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic150_kitchen_sink_styles_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic151_multilingual_greetings</b></td>
  <td colspan="1">classic151_multilingual_greetings <span style="color:#3fb950">⬤</span> 96.4%</td>
</tr>
<tr>
  <td><img src="images/classic151_multilingual_greetings_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic151_multilingual_greetings_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic152_emoji_sampler</b></td>
  <td colspan="1">classic152_emoji_sampler <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic152_emoji_sampler_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic152_emoji_sampler_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic153_currency_symbols</b></td>
  <td colspan="1">classic153_currency_symbols <span style="color:#3fb950">⬤</span> 98.9%</td>
</tr>
<tr>
  <td><img src="images/classic153_currency_symbols_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic153_currency_symbols_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic154_math_symbols</b></td>
  <td colspan="1">classic154_math_symbols <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic154_math_symbols_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic154_math_symbols_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic155_diacritical_marks</b></td>
  <td colspan="1">classic155_diacritical_marks <span style="color:#3fb950">⬤</span> 99.6%</td>
</tr>
<tr>
  <td><img src="images/classic155_diacritical_marks_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic155_diacritical_marks_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic156_rtl_bidi_text</b></td>
  <td colspan="1">classic156_rtl_bidi_text <span style="color:#d29922">⬤</span> 83.5%</td>
</tr>
<tr>
  <td><img src="images/classic156_rtl_bidi_text_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic156_rtl_bidi_text_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic157_cjk_extended</b></td>
  <td colspan="1">classic157_cjk_extended <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic157_cjk_extended_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic157_cjk_extended_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic158_emoji_skin_tones</b></td>
  <td colspan="1">classic158_emoji_skin_tones <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic158_emoji_skin_tones_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic158_emoji_skin_tones_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic159_zwj_emoji</b></td>
  <td colspan="1">classic159_zwj_emoji <span style="color:#3fb950">⬤</span> 96.9%</td>
</tr>
<tr>
  <td><img src="images/classic159_zwj_emoji_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic159_zwj_emoji_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic160_punctuation_marks</b></td>
  <td colspan="1">classic160_punctuation_marks <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic160_punctuation_marks_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic160_punctuation_marks_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic161_box_drawing</b></td>
  <td colspan="1">classic161_box_drawing <span style="color:#3fb950">⬤</span> 99.3%</td>
</tr>
<tr>
  <td><img src="images/classic161_box_drawing_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic161_box_drawing_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic162_cjk_emoji_styled</b></td>
  <td colspan="1">classic162_cjk_emoji_styled <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic162_cjk_emoji_styled_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic162_cjk_emoji_styled_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic163_cyrillic_alphabets</b></td>
  <td colspan="1">classic163_cyrillic_alphabets <span style="color:#3fb950">⬤</span> 96.9%</td>
</tr>
<tr>
  <td><img src="images/classic163_cyrillic_alphabets_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic163_cyrillic_alphabets_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic164_indic_scripts</b></td>
  <td colspan="1">classic164_indic_scripts <span style="color:#3fb950">⬤</span> 99.8%</td>
</tr>
<tr>
  <td><img src="images/classic164_indic_scripts_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic164_indic_scripts_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic165_southeast_asian</b></td>
  <td colspan="1">classic165_southeast_asian <span style="color:#3fb950">⬤</span> 98.4%</td>
</tr>
<tr>
  <td><img src="images/classic165_southeast_asian_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic165_southeast_asian_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic166_emoji_progress</b></td>
  <td colspan="1">classic166_emoji_progress <span style="color:#3fb950">⬤</span> 98.8%</td>
</tr>
<tr>
  <td><img src="images/classic166_emoji_progress_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic166_emoji_progress_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic167_musical_symbols</b></td>
  <td colspan="1">classic167_musical_symbols <span style="color:#3fb950">⬤</span> 99.5%</td>
</tr>
<tr>
  <td><img src="images/classic167_musical_symbols_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic167_musical_symbols_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic168_mixed_ltr_rtl_styled</b></td>
  <td colspan="1">classic168_mixed_ltr_rtl_styled <span style="color:#3fb950">⬤</span> 93.7%</td>
</tr>
<tr>
  <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic169_korean_invoice</b></td>
  <td colspan="1">classic169_korean_invoice <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic169_korean_invoice_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic169_korean_invoice_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic170_emoji_dashboard</b></td>
  <td colspan="1">classic170_emoji_dashboard <span style="color:#3fb950">⬤</span> 98.6%</td>
</tr>
<tr>
  <td><img src="images/classic170_emoji_dashboard_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic170_emoji_dashboard_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic171_ipa_phonetic</b></td>
  <td colspan="1">classic171_ipa_phonetic <span style="color:#3fb950">⬤</span> 98.3%</td>
</tr>
<tr>
  <td><img src="images/classic171_ipa_phonetic_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic171_ipa_phonetic_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic172_emoji_timeline</b></td>
  <td colspan="1">classic172_emoji_timeline <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic172_emoji_timeline_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic172_emoji_timeline_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic173_african_languages</b></td>
  <td colspan="1">classic173_african_languages <span style="color:#3fb950">⬤</span> 98.5%</td>
</tr>
<tr>
  <td><img src="images/classic173_african_languages_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic173_african_languages_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic174_technical_symbols</b></td>
  <td colspan="1">classic174_technical_symbols <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic174_technical_symbols_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic174_technical_symbols_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic175_multiscript_catalog</b></td>
  <td colspan="1">classic175_multiscript_catalog <span style="color:#3fb950">⬤</span> 97.7%</td>
</tr>
<tr>
  <td><img src="images/classic175_multiscript_catalog_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic175_multiscript_catalog_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic176_combining_characters</b></td>
  <td colspan="1">classic176_combining_characters <span style="color:#3fb950">⬤</span> 97.3%</td>
</tr>
<tr>
  <td><img src="images/classic176_combining_characters_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic176_combining_characters_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic177_emoji_calendar</b></td>
  <td colspan="1">classic177_emoji_calendar <span style="color:#3fb950">⬤</span> 99.1%</td>
</tr>
<tr>
  <td><img src="images/classic177_emoji_calendar_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic177_emoji_calendar_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic178_caucasus_ethiopic</b></td>
  <td colspan="1">classic178_caucasus_ethiopic <span style="color:#3fb950">⬤</span> 99.0%</td>
</tr>
<tr>
  <td><img src="images/classic178_caucasus_ethiopic_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic178_caucasus_ethiopic_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic179_emoji_inventory</b></td>
  <td colspan="1">classic179_emoji_inventory <span style="color:#3fb950">⬤</span> 98.6%</td>
</tr>
<tr>
  <td><img src="images/classic179_emoji_inventory_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic179_emoji_inventory_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic180_polyglot_paragraph</b></td>
  <td colspan="1">classic180_polyglot_paragraph <span style="color:#3fb950">⬤</span> 98.6%</td>
</tr>
<tr>
  <td><img src="images/classic180_polyglot_paragraph_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic180_polyglot_paragraph_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic181_feedback_tracker_with_images</b></td>
  <td colspan="1">classic181_feedback_tracker_with_images <span style="color:#3fb950">⬤</span> 94.5%</td>
</tr>
<tr>
  <td><img src="images/classic181_feedback_tracker_with_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic181_feedback_tracker_with_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic181_feedback_tracker_with_images_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic181_feedback_tracker_with_images_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic182_dense_long_text_columns</b></td>
  <td colspan="1">classic182_dense_long_text_columns <span style="color:#3fb950">⬤</span> 95.6%</td>
</tr>
<tr>
  <td><img src="images/classic182_dense_long_text_columns_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic182_dense_long_text_columns_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic182_dense_long_text_columns_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic182_dense_long_text_columns_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic183_mixed_content_grid</b></td>
  <td colspan="1">classic183_mixed_content_grid <span style="color:#3fb950">⬤</span> 97.4%</td>
</tr>
<tr>
  <td><img src="images/classic183_mixed_content_grid_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic183_mixed_content_grid_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic184_wide_narrow_columns</b></td>
  <td colspan="1">classic184_wide_narrow_columns <span style="color:#3fb950">⬤</span> 96.6%</td>
</tr>
<tr>
  <td><img src="images/classic184_wide_narrow_columns_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic184_wide_narrow_columns_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic185_tall_rows_vertical_align</b></td>
  <td colspan="1">classic185_tall_rows_vertical_align <span style="color:#3fb950">⬤</span> 99.2%</td>
</tr>
<tr>
  <td><img src="images/classic185_tall_rows_vertical_align_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic185_tall_rows_vertical_align_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic186_multi_sheet_image_report</b></td>
  <td colspan="1">classic186_multi_sheet_image_report <span style="color:#3fb950">⬤</span> 97.4%</td>
</tr>
<tr>
  <td><img src="images/classic186_multi_sheet_image_report_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic186_multi_sheet_image_report_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic186_multi_sheet_image_report_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic186_multi_sheet_image_report_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic187_bug_report_with_screenshots</b></td>
  <td colspan="1">classic187_bug_report_with_screenshots <span style="color:#3fb950">⬤</span> 94.2%</td>
</tr>
<tr>
  <td><img src="images/classic187_bug_report_with_screenshots_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic187_bug_report_with_screenshots_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic188_merged_header_with_images</b></td>
  <td colspan="1">classic188_merged_header_with_images <span style="color:#3fb950">⬤</span> 97.8%</td>
</tr>
<tr>
  <td><img src="images/classic188_merged_header_with_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic188_merged_header_with_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic189_alternating_image_text_rows</b></td>
  <td colspan="1">classic189_alternating_image_text_rows <span style="color:#3fb950">⬤</span> 90.4%</td>
</tr>
<tr>
  <td><img src="images/classic189_alternating_image_text_rows_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic189_alternating_image_text_rows_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic190_dashboard_kpi_images</b></td>
  <td colspan="1">classic190_dashboard_kpi_images <span style="color:#3fb950">⬤</span> 97.8%</td>
</tr>
<tr>
  <td><img src="images/classic190_dashboard_kpi_images_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic190_dashboard_kpi_images_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>classic191_payroll_calculator</b></td>
  <td colspan="1">classic191_payroll_calculator <span style="color:#d29922">⬤</span> 84.3%</td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p5_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p6_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p6_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p7_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p7_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p8_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p8_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/classic191_payroll_calculator_p9_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/classic191_payroll_calculator_p9_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><b>nthu_article</b></td>
  <td colspan="1">nthu_article <span style="color:#d29922">⬤</span> 80.1%</td>
</tr>
<tr>
  <td><img src="images/nthu_article_p1_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p1_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p2_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p2_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p3_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p3_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p4_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p4_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p5_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p5_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p6_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p6_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p7_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p7_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p8_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p8_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p9_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p9_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p10_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p10_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p11_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p11_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p12_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p12_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p13_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p13_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p14_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p14_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p15_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p15_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p16_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p16_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p17_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p17_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p18_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p18_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p19_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p19_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p20_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><img src="images/nthu_article_p20_reference.png" width="340" alt="Reference"></td>
</tr>
<tr>
  <td><img src="images/nthu_article_p21_minipdf.png" width="340" alt="MiniPdf"></td>
  <td><i>missing</i></td>
</tr>
<tr>
  <td><b>nthu_article_new</b></td>
  <td colspan="1">nthu_article_new N/A</td>
</tr>
<tr>
  <td colspan="2"><i>No images</i></td>
</tr>
<tr>
  <td><b>nthu_article_old</b></td>
  <td colspan="1">nthu_article_old N/A</td>
</tr>
<tr>
  <td colspan="2"><i>No images</i></td>
</tr>
</table>

## Detailed Results

### classic01_basic_table_with_headers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9947
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1619 bytes, Reference=51282 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9966
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2763 bytes, Reference=56518 bytes

Text content: ✅ Identical

### classic03_empty_workbook

- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=709 bytes, Reference=7283 bytes

Text content: ✅ Identical

### classic04_single_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9996
- **Overall Score:** 0.9998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=767 bytes, Reference=27469 bytes

Text content: ✅ Identical

### classic05_wide_table

- **Text Similarity:** 0.8846
- **Visual Average:** 0.9908
- **Overall Score:** 0.9502
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=10552 bytes, Reference=60760 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic05_wide_table.pdf
+++ reference/classic05_wide_table.pdf
@@ -1,20 +1,20 @@
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

 ---PAGE---

-K L M N O P Q R S T

-K1 L1 M1 N1 O1 P1 Q1 R1 S1 T1

-K2 L2 M2 N2 O2 P2 Q2 R2 S2 T2

-K3 L3 M3 N3 O3 P3 Q3 R3 S3 T3

-K4 L4 M4 N4 O4 P4 Q4 R4 S4 T4

-K5 L5 M5 N5 O5 P5 Q5 R5 S5 T5

+J K L M N O P Q R

+J1 K1 L1 M1 N1 O1 P1 Q1 R1

+J2 K2 L2 M2 N2 O2 P2 Q2 R2

+J3 K3 L3 M3 N3 O3 P3 Q3 R3

+J4 K4 L4 M4 N4 O4 P4 Q4 R4

+J5 K5 L5 M5 N5 O5 P5 Q5 R5

 ---PAGE---

-U V W X Y Z

-U1 V1 W1 X1 Y1 Z1

-U2 V2 W2 X2 Y2 Z2

-U3 V3 W3 X3 Y3 Z3

-U4 V4 W4 X4 Y4 Z4

-U5 V5 W5 X5 Y5 Z5
+S T U V W X Y Z

+S1 T1 U1 V1 W1 X1 Y1 Z1

+S2 T2 U2 V2 W2 X2 Y2 Z2

+S3 T3 U3 V3 W3 X3 Y3 Z3

+S4 T4 U4 V4 W4 X4 Y4 Z4

+S5 T5 U5 V5 W5 X5 Y5 Z5
```
</details>

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.8625
- **Overall Score:** 0.945
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=45906 bytes, Reference=116996 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic06_tall_table.pdf
+++ reference/classic06_tall_table.pdf
@@ -41,12 +41,12 @@
 Row40 Val40 This is the description for row number 40

 Row41 Val41 This is the description for row number 41

 Row42 Val42 This is the description for row number 42

----PAGE---

 Row43 Val43 This is the description for row number 43

 Row44 Val44 This is the description for row number 44

 Row45 Val45 This is the description for row number 45

 Row46 Val46 This is the description for row number 46

 Row47 Val47 This is the description for row number 47

+---PAGE---

 Row48 Val48 This is the description for row number 48

 Row49 Val49 This is the description for row number 49

 Row50 Val50 This is the description for row number 50

@@ -85,7 +85,6 @@
 Row83 Val83 This is the description for row number 83

 Row84 Val84 This is the description for row number 84

 Row85 Val85 This is the description for row number 85

----PAGE---

 Row86 Val86 This is the description for row number 86

 Row87 Val87 This is the description for row number 87

 Row88 Val88 This is the description for row number 88

@@ -96,6 +95,7 @@
 Row93 Val93 This is the description for row number 93

 Row94 Val94 This is the description for row number 94

 Row95 Val95 This is the description for row number 95

+---PAGE---

 Row96 Val96 This is the description for row number 96

 Row97 Val97 This is the description for row number 97

 Row98 Val98 This is the description for row number 98

@@ -129,7 +129,6 @@
 Row126 Val126 This is the description for row number 126

 Row127 Val127 This is the description for row number 127

 Row128 Val128 This is the description for row number 128

----PAGE---

 Row129 Val129 This is the description for row number 129

 Row130 Val130 This is the description for row number 130

 Row131 Val131 This is the description for row number 131

@@ -145,6 +144,7 @@
 Row141 Val141 This is the description for row number 141

 Row142 Val142 This is the description for row number 142

 Row143 Val143 This is the description for row number 143

+---PAGE---

 Row144 Val144 This is the description for row number 144

 Row145 Val145 This is the description for row number 145

 Row146 Val146 This is the description for row number 146

@@ -173,7 +173,6 @@
 Row169 Val169 This is the description for row number 169

 Row170 Val170 This is the description for row number 170

 Row171 Val171 This is the description for row number 171

----PAGE---

 Row172 Val172 This is the description for row number 172

 Row173 Val173 This is the description for row number 173

 Row174 Val174 This is the description for row number 174

@@ -194,6 +193,7 @@
 Row189 Val189 This is the description for row number 189

 Row190 Val190 This is the description for row number 190

 Row191 Val191 This is the description for row number 191

+---PAGE---

 Row192 Val192 This is the description for row number 192

 Row193 Val193 This is the description for row number 193

 Row194 Val194 This is the description 
... (19 more characters)

```
</details>

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9984
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1432 bytes, Reference=42110 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9958
- **Overall Score:** 0.9983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1428 bytes, Reference=44923 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.2984
- **Visual Average:** 0.9921
- **Overall Score:** 0.7162
- **Pages:** MiniPdf=12, Reference=12
- **File Size:** MiniPdf=3853 bytes, Reference=44367 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic09_long_text.pdf
+++ reference/classic09_long_text.pdf
@@ -1,26 +1,38 @@
 Long Text Column

-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

 Short

-YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+AAAAAAAAAAAAAAAAA BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

+BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

 ---PAGE---

-

----PAGE---
+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

+---PAGE---

+YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
```
</details>

### classic10_special_xml_characters

- **Text Similarity:** 1.0
- **Visual Average:** 0.993
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1178 bytes, Reference=43022 bytes

Text content: ✅ Identical

### classic11_sparse_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.9989
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1236 bytes, Reference=34964 bytes

Text content: ✅ Identical

### classic12_sparse_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.4988
- **Overall Score:** 0.6995
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=1077 bytes, Reference=41546 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic12_sparse_columns.pdf
+++ reference/classic12_sparse_columns.pdf
@@ -1,3 +1,5 @@
 Left Right

 Data1 FarRight

-Row3 VeryFar
+Row3

+---PAGE---

+VeryFar
```
</details>

### classic13_date_strings

- **Text Similarity:** 0.949
- **Visual Average:** 0.9932
- **Overall Score:** 0.9769
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1499 bytes, Reference=49430 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic13_date_strings.pdf
+++ reference/classic13_date_strings.pdf
@@ -1,6 +1,6 @@
 Date Event

-2025-01-1 Launch

-2025-06-3 Release

-2025-12-2 Holiday

-2026-01-0 New Year

-2026-02-2 Today
+2025-01-15Launch

+2025-06-30Release

+2025-12-25Holiday

+2026-01-01New Year

+2026-02-23Today
```
</details>

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.995
- **Overall Score:** 0.998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1447 bytes, Reference=52898 bytes

Text content: ✅ Identical

### classic15_negative_numbers

- **Text Similarity:** 0.9375
- **Visual Average:** 0.9949
- **Overall Score:** 0.973
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1562 bytes, Reference=42915 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic15_negative_numbers.pdf
+++ reference/classic15_negative_numbers.pdf
@@ -3,5 +3,5 @@
 Small Loss -0.5

 Zero 0

 Gain 50

-Big Loss -99999.99

+Big Loss -100000

 Tiny -0.001
```
</details>

### classic16_percentage_strings

- **Text Similarity:** 0.9939
- **Visual Average:** 0.9945
- **Overall Score:** 0.9954
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1451 bytes, Reference=52564 bytes

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

- **Text Similarity:** 1.0
- **Visual Average:** 0.9925
- **Overall Score:** 0.997
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1579 bytes, Reference=52509 bytes

Text content: ✅ Identical

### classic18_large_dataset

- **Text Similarity:** 0.9001
- **Visual Average:** 0.4434
- **Overall Score:** 0.6374
- **Pages:** MiniPdf=24, Reference=42
- **File Size:** MiniPdf=649296 bytes, Reference=920992 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic18_large_dataset.pdf
+++ reference/classic18_large_dataset.pdf
@@ -1,1024 +1,2043 @@
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

----PAGE---

-R42C0 R42C1 R42C2 R42C3 R42C4 R42C5 R42C6 R42C7 R42C8 R42C9

-R43C0 R43C1 R43C2 R43C3 R43C4 R43C5 R43C6 R43C7 R43C8 R43C9

-R44C0 R44C1 R44C2 R44C3 R44C4 R44C5 R44C6 R44C7 R44C8 R44C9

-R45C0 R45C1 R45C2 R45C3 R45C4 R45C5 R45C6 R45C7 R45C8 R45C9

-R46C0 R46C1 R46C2 R46C3 R46C4 R46C5 R46C6 R46C7 R46C8 R46C9

-R47C0 R47C1 R47C
... (141839 more characters)

```
</details>

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9908
- **Overall Score:** 0.9963
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1941 bytes, Reference=47524 bytes

Text content: ✅ Identical

### classic20_all_empty_cells

- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=709 bytes, Reference=7283 bytes

Text content: ✅ Identical

### classic21_header_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9984
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1016 bytes, Reference=35519 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1066 bytes, Reference=36175 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.9046
- **Visual Average:** 0.99
- **Overall Score:** 0.9578
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=686494 bytes, Reference=121984 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic23_unicode_text.pdf
+++ reference/classic23_unicode_text.pdf
@@ -3,5 +3,5 @@
 Chinese 你好 世界

 Japanese こんにちは世界

 Korean 안녕하세요세계

-Arabic ﺎﺒﺣﺮﻣ ﻢﻟﺎﻌﻟﺍ

+Arabicمرحبا العالم

 Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9944
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1286 bytes, Reference=41978 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.9907
- **Overall Score:** 0.9963
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2109 bytes, Reference=44930 bytes

Text content: ✅ Identical

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.996
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1270 bytes, Reference=39664 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1138 bytes, Reference=33233 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9954
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1893 bytes, Reference=30315 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9953
- **Overall Score:** 0.9981
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1807 bytes, Reference=42240 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.998
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1666 bytes, Reference=40960 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9912
- **Overall Score:** 0.9965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1982 bytes, Reference=77301 bytes

Text content: ✅ Identical

### classic32_right_aligned_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.996
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=310877 bytes, Reference=50644 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=311021 bytes, Reference=41368 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 1.0
- **Visual Average:** 0.9935
- **Overall Score:** 0.9974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1507 bytes, Reference=47831 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Text Similarity:** 0.9773
- **Visual Average:** 0.9973
- **Overall Score:** 0.9898
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1106 bytes, Reference=40677 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
 Tall Heade Value

-Extra Tall 42

+Extra Tall R 42

 Normal Ro 10
```
</details>

### classic36_merged_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1348 bytes, Reference=43920 bytes

Text content: ✅ Identical

### classic37_freeze_panes

- **Text Similarity:** 1.0
- **Visual Average:** 0.9754
- **Overall Score:** 0.9902
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6043 bytes, Reference=58159 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9951
- **Overall Score:** 0.998
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=309729 bytes, Reference=41405 bytes

Text content: ✅ Identical

### classic39_financial_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9892
- **Overall Score:** 0.9957
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2490 bytes, Reference=52570 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 0.9515
- **Visual Average:** 0.9931
- **Overall Score:** 0.9778
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1485 bytes, Reference=60659 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic40_scientific_notation.pdf
+++ reference/classic40_scientific_notation.pdf
@@ -1,6 +1,6 @@
 Label Value

 Avogadro 6.02E+23

-Planck 6.626E-34

+Planck 6.63E-34

 Speed of Li 3E+08

-Electron m9.109E-31

-Pi approx 3.1415927
+Electron m 9.11E-31

+Pi approx 3.141593
```
</details>

### classic41_integer_vs_float

- **Text Similarity:** 1.0
- **Visual Average:** 0.9931
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1792 bytes, Reference=46475 bytes

Text content: ✅ Identical

### classic42_boolean_values

- **Text Similarity:** 0.9948
- **Visual Average:** 0.9933
- **Overall Score:** 0.9952
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1489 bytes, Reference=44451 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic42_boolean_values.pdf
+++ reference/classic42_boolean_values.pdf
@@ -1,6 +1,6 @@
 Feature Enabled

 Dark Mode TRUE

-Notification FALSE

+Notificatio FALSE

 Auto-save TRUE

 Analytics FALSE

 Beta Featu TRUE
```
</details>

### classic43_inventory_report

- **Text Similarity:** 1.0
- **Visual Average:** 0.9779
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3731 bytes, Reference=89289 bytes

Text content: ✅ Identical

### classic44_employee_roster

- **Text Similarity:** 0.9935
- **Visual Average:** 0.9681
- **Overall Score:** 0.9846
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4341 bytes, Reference=69090 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

 1001 Alice Smith Engineerin Senior Eng alice@example.com

 1002 Bob Jones Marketing Marketing bob@example.com

-1003 Carol Williams HR HR Special carol@example.com

-1004 David Brown Engineerin Junior Engi david@example.com

+1003 Carol Williams HR HR Specialicarol@example.com

+1004 David Brown Engineerin Junior Engidavid@example.com

 1005 Eve Davis Finance Financial A eve@example.com

-1006 Frank Miller Sales Sales Repr frank@example.com

+1006 Frank Miller Sales Sales Reprefrank@example.com

 1007 Grace Wilson Engineerin Tech Lead grace@example.com

-1008 Henry Moore Support Support Sp henry@example.com
+1008 Henry Moore Support Support Sphenry@example.com
```
</details>

### classic45_sales_by_region

- **Text Similarity:** 1.0
- **Visual Average:** 0.9961
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3782 bytes, Reference=52603 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.9858
- **Overall Score:** 0.9943
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4100 bytes, Reference=60464 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.9611
- **Overall Score:** 0.9844
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8505 bytes, Reference=60034 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9914
- **Visual Average:** 0.9891
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2946 bytes, Reference=57959 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
-Question StrongAgre Agree Neutral Disagree StrongDisagree

-Easy to us 30 45 15 7 3

-Recomme 25 40 20 10 5

+Question StrongAgreAgree Neutral Disagree StrongDisagree

+Easy to use 30 45 15 7 3

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

 Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.8971
- **Visual Average:** 0.9778
- **Overall Score:** 0.95
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3442 bytes, Reference=72007 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smith +1-555-01 alice@exa New York USA

-Bob Jones +44-20-79 bob@exa London UK

-Carol Wan +86-10-12 carol@exa Beijing China

-David Mull +49-30-12 david@exa Berlin Germany

-Eve Martin +33-1-23-4 eve@exa Paris France

-Frank Tan +81-3-123 frank@exa Tokyo Japan

-Grace Kim +82-2-123 grace@ex Seoul Korea
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

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9834
- **Overall Score:** 0.9925
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=7972 bytes, Reference=65935 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic50_budget_vs_actuals.pdf
+++ reference/classic50_budget_vs_actuals.pdf
@@ -1,18 +1,18 @@
-Departmen Q1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin 200000 200000 210000 220000 830000

 Marketing 80000 90000 85000 95000 350000

 Sales 120000 130000 140000 150000 540000

 HR 40000 40000 42000 43000 165000

 Finance 35000 35000 37000 38000 145000

 ---PAGE---

-Departmen Q1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin 195000 205000 215000 225000 840000

 Marketing 82000 88000 91000 97000 358000

 Sales 118000 135000 142000 148000 543000

 HR 39000 41000 41500 44000 165500

 Finance 34000 36000 37500 39000 146500

 ---PAGE---

-Departmen Q1 Q2 Q3 Q4 Annual

+DepartmenQ1 Q2 Q3 Q4 Annual

 Engineerin -5000 5000 5000 5000 10000

 Marketing 2000 -2000 6000 2000 8000

 Sales -2000 5000 2000 -2000 3000

```
</details>

### classic51_product_catalog

- **Text Similarity:** 0.9826
- **Visual Average:** 0.9735
- **Overall Score:** 0.9824
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4366 bytes, Reference=72763 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
-Part# Name Description Weight(g) Price

-P-001 Basic Widg Standard w 150 4.99

-P-002 Pro Widget Enhanced 180 12.99

-P-003 Mini Gadg Compact g 90 19.99

-P-004 Max Gadg Full-size g 450 89.99

-P-005 Connector Type-A co 80 7.49

-P-006 Connector Type-B co 110 9.99

+Part# Name DescriptionWeight(g) Price

+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgetEnhanced w 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size ga 450 89.99

+P-005 Connector Type-A con 80 7.49

+P-006 Connector Type-B con 110 9.99

 P-007 Adapter X Universal p 200 15.99

 P-008 Adapter Y Travel pow 120 11.99

-P-009 Mount Bra Wall mount 600 24.99

-P-010 Carry Case Padded ca 350 34.99
+P-009 Mount Bra Wall moun 600 24.99

+P-010 Carry Case Padded car 350 34.99
```
</details>

### classic52_pivot_summary

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9828
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2967 bytes, Reference=88958 bytes

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

- **Text Similarity:** 0.9968
- **Visual Average:** 0.9775
- **Overall Score:** 0.9897
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3182 bytes, Reference=105898 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -8,9 +8,9 @@
 New York, NY 10001

 Item Qty Unit Price Total

 Consulting 10 150 1500

-Software Li 5 99 495

+Software L 5 99 495

 Hardware 2 249.99 499.98

-Support Pl 1 1200 1200

+Support Pla 1 1200 1200

 Subtotal 3694.98

 Tax (8%) 295.6

 Total Due 3990.58
```
</details>

### classic54_multi_level_header

- **Text Similarity:** 1.0
- **Visual Average:** 0.9864
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2676 bytes, Reference=73000 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9899
- **Overall Score:** 0.996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2014 bytes, Reference=60359 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.9544
- **Overall Score:** 0.9818
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=316890 bytes, Reference=50296 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9915
- **Overall Score:** 0.9966
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=156346 bytes, Reference=54240 bytes

Text content: ✅ Identical

### classic58_mixed_numeric_formats

- **Text Similarity:** 0.9969
- **Visual Average:** 0.9897
- **Overall Score:** 0.9946
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2000 bytes, Reference=56245 bytes

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

 Scientific a 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9937
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=5216 bytes, Reference=61161 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 0.852
- **Visual Average:** 0.5776
- **Overall Score:** 0.6718
- **Pages:** MiniPdf=4, Reference=6
- **File Size:** MiniPdf=66649 bytes, Reference=130804 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic60_large_wide_table.pdf
+++ reference/classic60_large_wide_table.pdf
@@ -1,105 +1,158 @@
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

-R40C01 R40C02 R40C03 
... (11920 more characters)

```
</details>

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9737
- **Overall Score:** 0.9895
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2258 bytes, Reference=71701 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 0.996
- **Visual Average:** 0.9725
- **Overall Score:** 0.9874
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3151 bytes, Reference=79788 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic62_company_logo_header.pdf
+++ reference/classic62_company_logo_header.pdf
@@ -1,6 +1,6 @@
 ACME Corporation

 Annual Report 2025

-Departmen Q1 Q2 Q3 Q4

+DepartmenQ1 Q2 Q3 Q4

 Sales 120 135 142 160

 Engineerin 85 90 95 100

 Marketing 60 65 70 75
```
</details>

### classic63_two_products_side_by_side

- **Text Similarity:** 1.0
- **Visual Average:** 0.9682
- **Overall Score:** 0.9873
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3266 bytes, Reference=46024 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.9902
- **Visual Average:** 0.9654
- **Overall Score:** 0.9822
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4816 bytes, Reference=71738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title Departmen Email

+Photo Name Title DepartmenEmail

 Alice Chen Engineer R&D alice@example.com

 Bob Smith Manager Sales bob@example.com

-Carol Wan Designer UX carol@example.com
+Carol WangDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9938
- **Visual Average:** 0.9638
- **Overall Score:** 0.983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7042 bytes, Reference=81216 bytes

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

 SKU-005 Purple Gea 75 7.99
```
</details>

### classic66_invoice_with_logo

- **Text Similarity:** 0.9935
- **Visual Average:** 0.9734
- **Overall Score:** 0.9868
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3128 bytes, Reference=87535 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -1,8 +1,8 @@
 INVOICE

 Invoice #: INV-20250301

 Date: 2025-03-01

-Description Qty Unit Price Total

+DescriptionQty Unit Price Total

 Consulting 8 150 1200

-Software Li 1 299 299

+Software L 1 299 299

 Support Pa 1 99 99

 Total 1598
```
</details>

### classic67_real_estate_listing

- **Text Similarity:** 1.0
- **Visual Average:** 0.9471
- **Overall Score:** 0.9788
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3062 bytes, Reference=95016 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Text Similarity:** 0.9953
- **Visual Average:** 0.9329
- **Overall Score:** 0.9713
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5567 bytes, Reference=89964 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,7 +1,7 @@
 Today's Menu

-Grilled Sal $18.99

+Grilled Salm $18.99

 Fresh Atlantic salmon with herbs

-Caesar Sal $12.99

+Caesar Sala $12.99

 Romaine lettuce, croutons, parmesan

 Beef Burge $14.99

 8oz Angus beef, brioche bun

```
</details>

### classic69_image_only_sheet

- **Text Similarity:** 1.0
- **Visual Average:** 0.9113
- **Overall Score:** 0.9645
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2592 bytes, Reference=6125 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Text Similarity:** 0.9932
- **Visual Average:** 0.9364
- **Overall Score:** 0.9718
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4682 bytes, Reference=85583 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic70_product_catalog_with_images.pdf
+++ reference/classic70_product_catalog_with_images.pdf
@@ -1,7 +1,7 @@
 Product Catalog - Spring 2025

-Classic Pe $3.99

+Classic Pen $3.99

 A reliable ballpoint pen

 Leather No $12.99

 Premium A5 notebook

-Desk Orga $24.99

+Desk Organ $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 0.9966
- **Visual Average:** 0.983
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=5396 bytes, Reference=56101 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic71_multi_sheet_with_images.pdf
+++ reference/classic71_multi_sheet_with_images.pdf
@@ -6,6 +6,6 @@
 Digital 50000

 Print 20000

 ---PAGE---

-Departmen Headcount

+DepartmenHeadcount

 Engineerin 45

 Sales 30
```
</details>

### classic72_bar_chart_image_with_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9379
- **Overall Score:** 0.9752
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3518 bytes, Reference=74026 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9412
- **Visual Average:** 0.9378
- **Overall Score:** 0.9516
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3498 bytes, Reference=87466 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -3,7 +3,7 @@
 Venue: Convention Center Hall A

 Speakers: 20+ Industry Leaders

 Time Session Speaker

-09:00 Opening K Dr. Jane Kim

+09:00 Opening KeDr. Jane Kim

 10:30 AI in Practi Prof. Mark Liu

-13:00 Cloud Arch Eng. Sara Patel

-15:00 Panel Disc All Speakers
+13:00 Cloud ArchEng. Sara Patel

+15:00 Panel DiscuAll Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.9938
- **Visual Average:** 0.9615
- **Overall Score:** 0.9821
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=295449 bytes, Reference=99265 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic74_dashboard_with_kpi_image.pdf
+++ reference/classic74_dashboard_with_kpi_image.pdf
@@ -3,4 +3,4 @@
 Revenue 500000 523000 ✓ Above

 New Custo 200 187 ✗ Below

 NPS Score 70 74 ✓ Above

-Churn Rat < 3% 2.8% ✓ Above
+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Text Similarity:** 1.0
- **Visual Average:** 0.9694
- **Overall Score:** 0.9878
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2174 bytes, Reference=77570 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9408
- **Overall Score:** 0.9763
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5265 bytes, Reference=67180 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9146
- **Overall Score:** 0.9658
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2919 bytes, Reference=112553 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9767
- **Visual Average:** 0.983
- **Overall Score:** 0.9839
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6463 bytes, Reference=76703 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

-Fix login b Alice Done

-Write unit t Bob In Progress

-Deploy to s Carol Pending

+Fix login buAlice Done

+Write unit Bob In Progress

+Deploy to sCarol Pending

 Code revie Alice Done

-Update do Dave In Progress
+Update docDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9477
- **Overall Score:** 0.9791
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3234 bytes, Reference=87926 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9553
- **Overall Score:** 0.9821
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2554 bytes, Reference=71550 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9511
- **Overall Score:** 0.9804
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5361 bytes, Reference=93150 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9963
- **Visual Average:** 0.9313
- **Overall Score:** 0.971
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4163 bytes, Reference=79534 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic82_before_after_images.pdf
+++ reference/classic82_before_after_images.pdf
@@ -2,4 +2,4 @@
 Old design – legacy UI New design – modern UI

 Metric Before After Delta

 Load time 4.2s 1.1s -74%

-Conversion 2.1% 4.8% +129%
+Conversion2.1% 4.8% +129%
```
</details>

### classic83_color_swatch_palette

- **Text Similarity:** 0.9783
- **Visual Average:** 0.9593
- **Overall Score:** 0.975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7280 bytes, Reference=82749 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic83_color_swatch_palette.pdf
+++ reference/classic83_color_swatch_palette.pdf
@@ -1,7 +1,7 @@
 Brand Color Palette

-Primary Bl RGB(0, 82, 165)

-Primary Re RGB(197, 27, 50)

-Accent Gre RGB(0, 163, 108)

-Neutral Gr RGB(128, 128, 128)

-Warm Yell RGB(255, 193, 7)

+Primary BluRGB(0, 82, 165)

+Primary ReRGB(197, 27, 50)

+Accent GreRGB(0, 163, 108)

+Neutral GreRGB(128, 128, 128)

+Warm YelloRGB(255, 193, 7)

 Dark Navy RGB(10, 30, 70)
```
</details>

### classic84_travel_destination_cards

- **Text Similarity:** 1.0
- **Visual Average:** 0.9139
- **Overall Score:** 0.9656
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4599 bytes, Reference=83209 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9634
- **Overall Score:** 0.9845
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3876 bytes, Reference=91041 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,6 +1,6 @@
 Sample Analysis Report

 Parameter Value Unit Reference Flag

-pH 7.35 7.35 – 7.45 Normal

+pH 7.35 7.35 – 7.45Normal

 Glucose 5.2 mmol/L 3.9 – 5.5 Normal

 Sodium 142 mEq/L 136 – 145 Normal

 Potassium 5 mEq/L 3.5 – 5.0 Normal

```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.9833
- **Visual Average:** 0.9433
- **Overall Score:** 0.9706
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3193 bytes, Reference=75924 bytes

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

-Cloud Syn Yes

+Cloud SyncYes

 Offline Mo Yes

-API Acces Pro only

-Export to P Yes
+API Access Pro only

+Export to PYes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9803
- **Overall Score:** 0.9921
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6108 bytes, Reference=84228 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 1.0
- **Visual Average:** 0.9674
- **Overall Score:** 0.987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3275 bytes, Reference=84797 bytes

Text content: ✅ Identical

### classic89_nutrition_label_with_image

- **Text Similarity:** 0.988
- **Visual Average:** 0.9637
- **Overall Score:** 0.9807
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3606 bytes, Reference=90810 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount pe % Daily Value

+Nutrient Amount pe% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

-Saturated 0.5g 3%

+Saturated F0.5g 3%

 Sodium 160mg 7%

-Total Carb 22g 8%

-Dietary Fib 3g 11%

+Total Carbo22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.9926
- **Visual Average:** 0.9515
- **Overall Score:** 0.9776
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3753 bytes, Reference=88752 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
 Project Orion – Status Report

 Reporting Period: Q1 2025

 Milestone Due Date Owner Status

-Requireme Jan 15 PM Team Complete

-Architectur Feb 1 Tech Lead Complete

+RequiremeJan 15 PM Team Complete

+ArchitecturFeb 1 Tech Lead Complete

 Alpha Rele Feb 28 Dev Team In Progress

-Beta Testin Mar 31 QA Team Not Started

-Production Apr 15 DevOps Not Started
+Beta TestinMar 31 QA Team Not Started

+ProductionApr 15 DevOps Not Started
```
</details>

### classic91_simple_bar_chart

- **Text Similarity:** 0.8732
- **Visual Average:** 0.9387
- **Overall Score:** 0.9248
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3436 bytes, Reference=76902 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic91_simple_bar_chart.pdf
+++ reference/classic91_simple_bar_chart.pdf
@@ -2,16 +2,14 @@
 Widget A 12000

 Product Revenue

 Widget B 18500

-25000

 Widget C 9200

 Widget D 22000

 Widget E 15600

-20000

-15000

 Revenue ($)

-10000

-5000

-0

-Widget A Widget B Widget C Widget D Widget E

 Product

----PAGE---
+---PAGE---

+Widget A

+Widget B

+Widget C

+Widget D

+Widget E
```
</details>

### classic92_horizontal_bar_chart

- **Text Similarity:** 0.6833
- **Visual Average:** 0.9481
- **Overall Score:** 0.8526
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4200 bytes, Reference=78581 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic92_horizontal_bar_chart.pdf
+++ reference/classic92_horizontal_bar_chart.pdf
@@ -1,15 +1,15 @@
-Departmen Headcount

+DepartmenHeadcount

 Engineerin 45

 Headcount by Department

 Sales 30

 Marketing 18

-HR 12 Operations

+HR 12

 Finance 15

 Operations 25

+---PAGE---

+Engineering

+Sales

+Marketing

+HR

 Finance

-HR

-Marketing

-Sales

-Engineering

-0 5 10 15 20 25 30 35 40 45 5

----PAGE---
+Operations
```
</details>

### classic93_line_chart

- **Text Similarity:** 0.866
- **Visual Average:** 0.9829
- **Overall Score:** 0.9396
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5794 bytes, Reference=85633 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic93_line_chart.pdf
+++ reference/classic93_line_chart.pdf
@@ -2,22 +2,27 @@
 Jan 3

 Monthly Average Temperature

 Feb 5

-30

 Mar 10

 Apr 15

 May 20

-25

 Jun 25

 Jul 28

-20

 Aug 27

 Sep 22

-15

 Oct 15

-Nov 8 Temperature (C)

+Nov 8

 Dec 4

-10

-5

-0

-Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov

----PAGE---
+Temperature (C)

+---PAGE---

+Jan

+Feb

+Mar

+Apr

+May

+Jun

+Jul

+Aug

+Sep

+Oct

+Nov

+Dec
```
</details>

### classic94_pie_chart

- **Text Similarity:** 0.8734
- **Visual Average:** 0.9298
- **Overall Score:** 0.9213
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=404218 bytes, Reference=78532 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic94_pie_chart.pdf
+++ reference/classic94_pie_chart.pdf
@@ -5,9 +5,13 @@
 Consumer 22

 Governme 10

 Education 5

-Enterprise

+Enterp

 SMB

-Consumer

-Government

-Education

----PAGE---
+Consu

+Gover

+Educa

+---PAGE---

+prise

+umer

+rnment

+ation
```
</details>

### classic95_area_chart

- **Text Similarity:** 0.9
- **Visual Average:** 0.9368
- **Overall Score:** 0.9347
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=13560 bytes, Reference=80677 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic95_area_chart.pdf
+++ reference/classic95_area_chart.pdf
@@ -2,29 +2,22 @@
 00:00 214

 Website Traffic by Hour

 01:00 216

-1200

 02:00 218

 03:00 221

 04:00 224

-1000

 05:00 228

 06:00 233

-800

 07:00 240

 08:00 250

-600

 09:00 265

-10:00 288 Users

+Users

+10:00 288

 11:00 329

-400

 12:00 408

 13:00 600

-200

 14:00 1000

 15:00 600

-0

 16:00 408

-00:001:002:003:004:005:006:007:008:009:0010:0011:0012:0013:0014:0015:0016:0017:0018:0019:0020:0021:0

 17:00 329

 18:00 288

 19:00 265

@@ -32,4 +25,5 @@
 21:00 240

 22:00 233

 23:00 228

----PAGE---
+---PAGE---

+Users
```
</details>

### classic96_scatter_chart

- **Text Similarity:** 0.7826
- **Visual Average:** 0.9764
- **Overall Score:** 0.9036
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6963 bytes, Reference=82367 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic96_scatter_chart.pdf
+++ reference/classic96_scatter_chart.pdf
@@ -2,31 +2,38 @@
 45 96

 Ad Spend vs Sales

 6 11

-140

 20 43

 13 22

-120

 48 117

 10 31

-100

 32 64

 6 5

-80

 18 38

 37 94

-60

-6 20 Sales ($K)

+6 20

+Sales ($K)

 17 49

-40

 49 119

 31 68

-20

 33 83

 22 40

-0

 15 37

-0 10 20 30 40 50 60

-26 57 Ad Spend ($K)

+26 57

+Ad Spend ($K)

 14 28

 26 52

----PAGE---
+---PAGE---

+45

+6

+20

+13

+48

+10

+32

+6

+18

+37

+6

+17

+49

+31
```
</details>

### classic97_doughnut_chart

- **Text Similarity:** 0.872
- **Visual Average:** 0.9326
- **Overall Score:** 0.9218
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=302816 bytes, Reference=76024 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic97_doughnut_chart.pdf
+++ reference/classic97_doughnut_chart.pdf
@@ -5,9 +5,14 @@
 Marketing 8000

 R&D 15000

 Other 5000

-Salaries

-Rent

-Marketing

+Sala

+Ren

+Ma

 R&D

-Other

----PAGE---
+Oth

+---PAGE---

+aries

+nt

+rketing

+D

+her
```
</details>

### classic98_radar_chart

- **Text Similarity:** 0.7416
- **Visual Average:** 0.9861
- **Overall Score:** 0.8911
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4470 bytes, Reference=75968 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic98_radar_chart.pdf
+++ reference/classic98_radar_chart.pdf
@@ -2,21 +2,16 @@
 Python 9

 Developer Skill Radar

 SQL 8

+Communic 7

+Leadership 6

+Design 5

+DevOps 7

 Python

-Communic 7

-Leadership 6 10

-9

-Design 5

-8

-7

-DevOps 7 DevOps SQL

-6

-5

-4

-3

-2

-1

-0

-Design Communication

-Leadership

----PAGE---
+SQL

+Commun

+Leadersh

+Design

+DevOps

+---PAGE---

+nication

+hip
```
</details>

### classic99_bubble_chart

- **Text Similarity:** 0.8509
- **Visual Average:** 0.9759
- **Overall Score:** 0.9307
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4743 bytes, Reference=86738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic99_bubble_chart.pdf
+++ reference/classic99_bubble_chart.pdf
@@ -2,18 +2,16 @@
 10 4.2 500

 Product Comparison

 25 4.5 300

-6

 50 3.8 150

 15 4 420

 35 4.7 200

-5

 8 3.5 600

-4

-3

 Rating

-2

-1

-0

-0 10 20 30 40 50

 Price ($)

----PAGE---
+---PAGE---

+10

+25

+50

+15

+35

+8
```
</details>

### classic100_stacked_bar_chart

- **Text Similarity:** 0.7867
- **Visual Average:** 0.875
- **Overall Score:** 0.8647
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5035 bytes, Reference=75642 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic100_stacked_bar_chart.pdf
+++ reference/classic100_stacked_bar_chart.pdf
@@ -4,14 +4,7 @@
 East 40 35 30 45

 West 20 25 40 35

 Quarterly Revenue by Region

-180

-160

-140

-120

-100

-80

-60

-40

-20

-0

-North South East West
+Q4

+Q3

+Q2

+Q1
```
</details>

### classic101_percent_stacked_bar

- **Text Similarity:** 0.7778
- **Visual Average:** 0.847
- **Overall Score:** 0.8499
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5794 bytes, Reference=78650 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic101_percent_stacked_bar.pdf
+++ reference/classic101_percent_stacked_bar.pdf
@@ -5,15 +5,7 @@
 2024 33 35 18 14

 2025 30 38 17 15

 Traffic Source Mix by Year

-100%

-90%

-80%

-70%

-60%

-50%

-40%

-30%

-20%

-10%

-0%

-2021 2022 2023 2024 2025
+Direct

+Referral

+Paid

+Organic
```
</details>

### classic102_line_chart_with_markers

- **Text Similarity:** 0.7723
- **Visual Average:** 0.9834
- **Overall Score:** 0.9023
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5228 bytes, Reference=78986 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic102_line_chart_with_markers.pdf
+++ reference/classic102_line_chart_with_markers.pdf
@@ -1,18 +1,13 @@
 Year Users (K) Revenue (K)

 2020 10 50

-Company Growth

+Company Grow

 2021 25 120

-1200

 2022 55 280

 2023 90 500

 2024 140 780

-1000

 2025 200 1100

-800

-600

 Value (K)

-400

-200

-0

-2020 2021 2022 2023 20

----PAGE---
+---PAGE---

+wth

+Users (K)

+Revenue (K)
```
</details>

### classic103_pie_chart_with_labels

- **Text Similarity:** 0.5474
- **Visual Average:** 0.9408
- **Overall Score:** 0.7953
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=404613 bytes, Reference=76626 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic103_pie_chart_with_labels.pdf
+++ reference/classic103_pie_chart_with_labels.pdf
@@ -1,18 +1,23 @@
 OS Share (%)

 Windows 42

+Share (%),

 Desktop OS Market Share

-macOS 28

-Linux 15

-Other; Share (%); 5; 5%

+macOS 28 Other, 5, 5%

+Share (%),

+Linux 15 ChromeOS, 10,

+10%

 ChromeOS 10

-ChromeOS; Share (%); 10; 10%

 Other 5

-Windows; Share (%); 42; 42%

-Linux; Share (%); 15; 15%

-macOS; Share (%); 28; 28%

+Share (%),

+Share (%), Linux,

+Windows, 42,

+15, 15%

+42%

+Share (%),

+macOS, 28, 28%

+---PAGE---

 Windows

 macOS

 Linux

 ChromeOS

-Other

----PAGE---
+Other
```
</details>

### classic104_combo_bar_line_chart

- **Text Similarity:** 0.7761
- **Visual Average:** 0.9315
- **Overall Score:** 0.883
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4911 bytes, Reference=76509 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic104_combo_bar_line_chart.pdf
+++ reference/classic104_combo_bar_line_chart.pdf
@@ -1,18 +1,12 @@
 Month Sales Target

 Jan 42 45

-Sales vs Target

+Sales vs Targe

 Feb 48 47

-70

 Mar 51 50

 Apr 45 50

-60

 May 56 54

 Jun 62 60

-50

-40

-30

-20

-10

-0

-Jan Feb Mar Apr May

----PAGE---
+---PAGE---

+et

+Sales

+Target
```
</details>

### classic105_3d_bar_chart

- **Text Similarity:** 0.7208
- **Visual Average:** 0.9261
- **Overall Score:** 0.8588
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4300 bytes, Reference=103089 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic105_3d_bar_chart.pdf
+++ reference/classic105_3d_bar_chart.pdf
@@ -1,19 +1,10 @@
 Region 2024 2025

 APAC 120 145

-Revenue by Region (3D)

+Revenue by Region (3

 EMEA 95 110

-200

 Americas 150 175

 LATAM 40 55

-180

-160

-140

-120

-100

-80

-60

-40

-20

-0

-APAC EMEA Americas LATAM

----PAGE---
+---PAGE---

+3D)

+2024

+2025
```
</details>

### classic106_3d_pie_chart

- **Text Similarity:** 0.9366
- **Visual Average:** 0.946
- **Overall Score:** 0.953
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=404449 bytes, Reference=113656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic106_3d_pie_chart.pdf
+++ reference/classic106_3d_pie_chart.pdf
@@ -6,10 +6,16 @@
 Entertainm 300

 Savings 700

 Other 200

+F

+H

+T

+E

+S

+O

+---PAGE---

 Food

 Housing

 Transport

 Entertainment

 Savings

-Other

----PAGE---
+Other
```
</details>

### classic107_multi_series_line

- **Text Similarity:** 0.848
- **Visual Average:** 0.9635
- **Overall Score:** 0.9246
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=16325 bytes, Reference=91236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic107_multi_series_line.pdf
+++ reference/classic107_multi_series_line.pdf
@@ -1,32 +1,28 @@
 Day AAPL GOOG MSFT

 Day 1 178.48 140.49 402.83

-Stock Price Trend (20 Day

+S

 Day 2 179.43 140.38 401.69

-450

 Day 3 177.25 143.38 403.21

 Day 4 175.75 143.94 404.47

-400

 Day 5 178.19 142.62 403.35

-Day 6 176.32 143.16 405.88 350

+Day 6 176.32 143.16 405.88

 Day 7 177.72 141 405.11

-300

 Day 8 175.18 138.97 405.07

-Day 9 173.1 137.59 403.53 250

+Day 9 173.1 137.59 403.53

 Day 10 172.64 139.72 401.94

-200

-Day 11 173.32 139.12 400.69 Price ($)

+Day 11 173.32 139.12 400.69

+Price ($)

 Day 12 172.11 140.8 402.75

-150

 Day 13 173.5 143.13 404.12

-100

 Day 14 172.29 141.53 404.52

 Day 15 172.95 143.24 406.95

-50

 Day 16 174.74 146.1 408

-0

 Day 17 175.83 147.89 407.98

-Day 1Day 2Day 3Day 4Day 5Day 6Day 7Day 8Day 9Day 10Day 11Day 12D

 Day 18 177.62 150.15 408.05

 Day 19 176.68 149.43 408.73

 Day 20 177.07 149.4 408.07

----PAGE---
+---PAGE---

+Stock Price Trend (20 Days)

+AAPL

+GOOG

+MSFT
```
</details>

### classic108_stacked_area_chart

- **Text Similarity:** 0.7974
- **Visual Average:** 0.3854
- **Overall Score:** 0.5731
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=11481 bytes, Reference=86751 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic108_stacked_area_chart.pdf
+++ reference/classic108_stacked_area_chart.pdf
@@ -6,13 +6,8 @@
 May 150 130 240 125

 Jun 160 140 260 130

 Traffic by Channel (Stacked)

-800

-700

-600

-500

-400

-300

-200

-100

-0

-Jan Feb Mar Apr May Jun
+Direct

+Search

+Social

+Email

+---PAGE---
```
</details>

### classic109_scatter_with_trendline

- **Text Similarity:** 0.7037
- **Visual Average:** 0.9793
- **Overall Score:** 0.8732
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5901 bytes, Reference=86322 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic109_scatter_with_trendline.pdf
+++ reference/classic109_scatter_with_trendline.pdf
@@ -1,27 +1,25 @@
-Study Hour Exam Score

+Study HourExam Score

 5 59

 Study Hours vs Exam Score

 8 90

-120

 9 85

+y = 8.1272x + 20.8

 2 35

+R² = 0.9586

 9 99

-100

 5 68

 2 35

-80

 8 92

 5 65

-60

 3 45

-9 100 Score

+Score

+9 100

 6 62

-40

 9 89

 1 30

-20

 10 98

-0

-0 2 4 6 8 10 12

 Hours

----PAGE---
+---PAGE---

+828

+Students

+Linear (Students)
```
</details>

### classic110_chart_with_legend

- **Text Similarity:** 0.7778
- **Visual Average:** 0.9574
- **Overall Score:** 0.8941
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4351 bytes, Reference=88129 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic110_chart_with_legend.pdf
+++ reference/classic110_chart_with_legend.pdf
@@ -1,18 +1,12 @@
 Browser 2024 (%) 2025 (%)

 Chrome 65 62

-Browser Market Share Comparison

+Browser Market Share Com

 Safari 18 20

-70

 Firefox 8 7

 Edge 6 8

-60

 Other 3 3

-50

-40

-30

 Market Share (%)

-20

-10

-0

-Chrome Safari Firefox Edge Oth

----PAGE---
+2024 (%) 2025 (

+---PAGE---

+mparison

+(%)
```
</details>

### classic111_chart_with_axis_labels

- **Text Similarity:** 0.735
- **Visual Average:** 0.9603
- **Overall Score:** 0.8781
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3802 bytes, Reference=79609 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic111_chart_with_axis_labels.pdf
+++ reference/classic111_chart_with_axis_labels.pdf
@@ -3,14 +3,15 @@
 CO2 Emissions by Country

 USA 5000

 India 2700

-Russia 1700 Germany

+Russia 1700

 Japan 1100

 Germany 700

+Country

+CO2 Emissions (Megatons)

+---PAGE---

+China

+USA

+India

+Russia

 Japan

-Russia

-CO2 Emissions (Megatons) India

-USA

-China

-0 2,000 4,000 6,000 8,000 10,000

-Country

----PAGE---
+Germany
```
</details>

### classic112_multiple_charts

- **Text Similarity:** 0.6948
- **Visual Average:** 0.9372
- **Overall Score:** 0.8528
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=7393 bytes, Reference=86399 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic112_multiple_charts.pdf
+++ reference/classic112_multiple_charts.pdf
@@ -1,29 +1,20 @@
 Month Revenue Costs Profit

 Jan 50 30 20

-Revenue & Costs

+Revenue

 Feb 55 32 23

-80

 Mar 60 35 25

 Apr 52 28 24

-70

 May 70 40 30

-60

 Jun 75 42 33

-50

-40

-30

-20

-10

-0

-Jan Feb Mar Apr May

-Profit Trend

-35

-30

-25

-20

-15

-10

-5

-0

-Jan Feb Mar Apr M

----PAGE---
+Profit T

+---PAGE---

+& Costs

+Revenue

+Costs

+Trend

+Jan

+Feb

+Mar

+Apr

+May

+Jun
```
</details>

### classic113_chart_sheet

- **Text Similarity:** 0.7442
- **Visual Average:** 0.9128
- **Overall Score:** 0.8628
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3503 bytes, Reference=68612 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic113_chart_sheet.pdf
+++ reference/classic113_chart_sheet.pdf
@@ -2,17 +2,10 @@
 Q1 250

 Quarterly Revenue

 Q2 310

-450

 Q3 285

 Q4 400

-400

-350

-300

-250

-200

-150

-100

-50

-0

-Q1 Q2 Q3 Q4

----PAGE---
+---PAGE---

+Q1

+Q2

+Q3

+Q4
```
</details>

### classic114_chart_large_dataset

- **Text Similarity:** 0.9379
- **Visual Average:** 0.9774
- **Overall Score:** 0.9661
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=33361 bytes, Reference=97214 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic114_chart_large_dataset.pdf
+++ reference/classic114_chart_large_dataset.pdf
@@ -1,30 +1,22 @@
 Day Value

 1 97.7

-100-Day Value Trend

+100-Day Value

 2 93.7

-160

 3 96.1

 4 93.7

-140

 5 95.6

 6 92.3

-120

 7 98.1

-8 100.5 100

+8 100.5

 9 98.7

-80

 10 94.4

 11 98.6

-60

 12 103.5

 13 102.2

-40

 14 98.4

-15 104.2 20

+15 104.2

 16 109

-0

 17 109.1

-1234567891011213141516171819202122324252627282930313233435363738394041424344546474849505152535455657585960616263646566768697071727374757677879808182838485868788

 18 105.3

 19 108.6

 20 114.2

@@ -50,12 +42,12 @@
 40 131

 41 131.7

 42 137.3

----PAGE---

 43 137.6

 44 133.5

 45 130

 46 128.3

 47 127

+---PAGE---

 48 124.3

 49 120.2

 50 118.9

@@ -94,7 +86,6 @@
 83 111.2

 84 107

 85 112.6

----PAGE---

 86 114.8

 87 118

 88 118.9

@@ -105,9 +96,25 @@
 93 129.2

 94 126.2

 95 131.7

+---PAGE---

 96 133.1

 97 129.3

 98 133.6

 99 138

 100 142.1

----PAGE---
+---PAGE---

+Trend

+1

+2

+3

+4

+5

+6

+7

+8

+9

+10

+11

+12

+13

+14
```
</details>

### classic115_chart_negative_values

- **Text Similarity:** 0.8421
- **Visual Average:** 0.9627
- **Overall Score:** 0.9219
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4650 bytes, Reference=85182 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic115_chart_negative_values.pdf
+++ reference/classic115_chart_negative_values.pdf
@@ -2,22 +2,19 @@
 Jan 15

 Monthly Profit & Loss

 Feb -8

-35

 Mar 22

 Apr -3

-30

 May 30

-25

 Jun -12

-Jul 18 20

+Jul 18

 Aug 5

-15

-10

 Amount ($K)

-5

-0

--5

--10

--15

-Jan Feb Mar Apr May Jun Jul Aug

----PAGE---
+---PAGE---

+Jan

+Feb

+Mar

+Apr

+May

+Jun

+Jul

+Aug
```
</details>

### classic116_percent_stacked_area

- **Text Similarity:** 0.7755
- **Visual Average:** 0.3547
- **Overall Score:** 0.5521
- **Pages:** MiniPdf=1, Reference=2
- **File Size:** MiniPdf=11745 bytes, Reference=80966 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic116_percent_stacked_area.pdf
+++ reference/classic116_percent_stacked_area.pdf
@@ -6,15 +6,8 @@
 2023 20 26 17 37

 2025 15 24 16 45

 Energy Mix Transition

-100%

-90%

-80%

-70%

-60%

-50%

-40%

-30%

-20%

-10%

-0%

-2015 2017 2019 2021 2023 2025
+Renewable

+Nuclear

+Gas

+Coal

+---PAGE---
```
</details>

### classic117_stock_ohlc_chart

- **Text Similarity:** 0.8072
- **Visual Average:** 0.9028
- **Overall Score:** 0.884
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=9099 bytes, Reference=91947 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic117_stock_ohlc_chart.pdf
+++ reference/classic117_stock_ohlc_chart.pdf
@@ -1,23 +1,18 @@
 Day Open High Low Close

 Day 1 148.96 149.78 146.96 147.41

-Stock OHLC (1

 Day 2 147.04 147.63 144.4 146.23

-180

 Day 3 145.63 149.68 145.47 149.58

 Day 4 149.32 150.14 147.39 148.55

-160

 Day 5 146.58 150.1 143.38 147.36

-Day 6 147.91 152.44 145.49 149.32 140

+Day 6 147.91 152.44 145.49 149.32

 Day 7 151.08 155.51 150.22 150.81

-120

 Day 8 152.42 155.53 152.31 152.99

-Day 9 152.32 154.36 151.02 152.05 100

+Day 9 152.32 154.36 151.02 152.05

 Day 10 152.27 156.85 148.76 156.35

-80

 Price ($)

-60

-40

-20

-0

-Day 1 Day 2 Day 3 Day 4 Day 5

----PAGE---
+---PAGE---

+Stock OHLC (10 Days)

+Open

+High

+Low

+Close
```
</details>

### classic118_bar_chart_custom_colors

- **Text Similarity:** 0.8344
- **Visual Average:** 0.9543
- **Overall Score:** 0.9155
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3914 bytes, Reference=78458 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic118_bar_chart_custom_colors.pdf
+++ reference/classic118_bar_chart_custom_colors.pdf
@@ -2,19 +2,12 @@
 Excellent 45

 Customer Satisfaction Survey

 Good 30

-50

 Average 15

 Poor 7

-45

 Very Poor 3

-40

-35

-30

-25

-20

-15

-10

-5

-0

-Excellent Good Average Poor Very Poor

----PAGE---
+---PAGE---

+Excellent

+Good

+Average

+Poor

+Very Poor
```
</details>

### classic119_dashboard_multi_charts

- **Text Similarity:** 0.8253
- **Visual Average:** 0.9084
- **Overall Score:** 0.8935
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=218941 bytes, Reference=94742 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic119_dashboard_multi_charts.pdf
+++ reference/classic119_dashboard_multi_charts.pdf
@@ -1,24 +1,19 @@
 KPI Dashboard - Q4 2025

 Revenue vs Expenses

 Month Revenue Expenses

-120

 Oct 85 60

 Nov 92 65

-100

 Dec 110 70

-80

-60

 Segment Share

-40

 Enterprise 45

 SMB 30

-20

 Consumer 25

-0

-Oct Nov Dec

 Revenue by Segment

-Enterprise

+Enter

 SMB

-Consumer

-Slice4

----PAGE---
+Cons

+---PAGE---

+Revenue

+Expenses

+rprise

+sumer
```
</details>

### classic120_chart_with_date_axis

- **Text Similarity:** 0.4138
- **Visual Average:** 0.9748
- **Overall Score:** 0.7554
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6507 bytes, Reference=82299 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic120_chart_with_date_axis.pdf
+++ reference/classic120_chart_with_date_axis.pdf
@@ -1,25 +1,30 @@
 Date Downloads

-2025-01-0 581

-Monthly Downloads (2025)

-2025-01-3 594

-900

-2025-03-0 592

-2025-04-0 692

-800

-2025-05-0 760

-2025-05-3 733 700

-2025-06-3 763

-600

-2025-07-3 767

-2025-08-2 774 500

-2025-09-2 788

-400

-2025-10-2 820 Downloads

-2025-11-2 865

-300

-200

-100

-0

-2025-01-012025-01-312025-03-022025-04-012025-05-012025-05-312025-06-302025-07-302025-08-292025-09

+2025-01-01 581

+Monthly Downloads (202

+2025-01-31 594

+2025-03-02 592

+2025-04-01 692

+2025-05-01 760

+2025-05-31 733

+2025-06-30 763

+2025-07-30 767

+2025-08-29 774

+2025-09-28 788

+2025-10-28 820

+Downloads

+2025-11-27 865

 Date

----PAGE---
+---PAGE---

+25)

+2025-01-01

+2025-01-31

+2025-03-02

+2025-04-01

+2025-05-01

+2025-05-31

+2025-06-30

+2025-07-30

+2025-08-29

+2025-09-28

+2025-10-28

+2025-11-27
```
</details>

### classic121_thin_borders

- **Text Similarity:** 1.0
- **Visual Average:** 0.9848
- **Overall Score:** 0.9939
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=323546 bytes, Reference=74465 bytes

Text content: ✅ Identical

### classic122_thick_outer_thin_inner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9808
- **Overall Score:** 0.9923
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=324335 bytes, Reference=78822 bytes

Text content: ✅ Identical

### classic123_dashed_borders

- **Text Similarity:** 0.9882
- **Visual Average:** 0.9899
- **Overall Score:** 0.9912
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=311939 bytes, Reference=61720 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic123_dashed_borders.pdf
+++ reference/classic123_dashed_borders.pdf
@@ -1,6 +1,6 @@
-Border Styl Sample

+Border Sty Sample

 dashed Bordered cell

 dotted Bordered cell

 dashDot Bordered cell

-dashDotDo Bordered cell

-mediumDa Bordered cell
+dashDotDoBordered cell

+mediumDaBordered cell
```
</details>

### classic124_colored_borders

- **Text Similarity:** 1.0
- **Visual Average:** 0.981
- **Overall Score:** 0.9924
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=312708 bytes, Reference=63553 bytes

Text content: ✅ Identical

### classic125_solid_fills

- **Text Similarity:** 0.9845
- **Visual Average:** 0.9713
- **Overall Score:** 0.9823
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=312982 bytes, Reference=69321 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic125_solid_fills.pdf
+++ reference/classic125_solid_fills.pdf
@@ -1,9 +1,9 @@
 Fill Name Filled Cell

 Light Blue Background

-Light Gree Background

-Light Yello Background

+Light GreenBackground

+Light YellowBackground

 Light Red Background

 Light Purpl Background

-Light Oran Background

+Light OrangBackground

 Gray 25% Background

 Sky Blue Background
```
</details>

### classic126_dark_header

- **Text Similarity:** 0.993
- **Visual Average:** 0.9781
- **Overall Score:** 0.9884
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2456 bytes, Reference=83023 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic126_dark_header.pdf
+++ reference/classic126_dark_header.pdf
@@ -1,5 +1,5 @@
-Employee Departmen Salary Start Date

-Alice Smith Engineerin 95000 2020-03-15

+EmployeeDepartment Salary Start Date

+Alice SmithEngineerin 95000 2020-03-15

 Bob Jones Marketing 72000 2019-07-01

 Carol Lee Finance 88000 2021-01-10

 David Kim Engineerin 102000 2018-11-20

```
</details>

### classic127_font_styles

- **Text Similarity:** 0.9982
- **Visual Average:** 0.9788
- **Overall Score:** 0.9908
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2223 bytes, Reference=121281 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic127_font_styles.pdf
+++ reference/classic127_font_styles.pdf
@@ -2,7 +2,7 @@
 Bold Sample Bold text

 Italic Sample Italic text

 Underline Sample Underline text

-Strikethrou Sample Strikethrough text

+StrikethrouSample Strikethrough text

 Bold Italic Sample Bold Italic text

 Bold Under Sample Bold Underline text

 Double Un Sample Double Underline text

```
</details>

### classic128_font_sizes

- **Text Similarity:** 1.0
- **Visual Average:** 0.985
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2129 bytes, Reference=66894 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic128_font_sizes.pdf
+++ reference/classic128_font_sizes.pdf
@@ -8,5 +8,4 @@
 16 Font size 16

 18 Font size 18

 20 Font size 20

-24

-Font size 24
+24 Font size 24
```
</details>

### classic129_alignment_combos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9906
- **Overall Score:** 0.9962
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=313155 bytes, Reference=65763 bytes

Text content: ✅ Identical

### classic130_wrap_and_indent

- **Text Similarity:** 1.0
- **Visual Average:** 0.9887
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=317273 bytes, Reference=70816 bytes

Text content: ✅ Identical

### classic131_number_formats

- **Text Similarity:** 1.0
- **Visual Average:** 0.98
- **Overall Score:** 0.992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=316622 bytes, Reference=77127 bytes

Text content: ✅ Identical

### classic132_striped_table

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9267
- **Overall Score:** 0.97
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=334054 bytes, Reference=84504 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic132_striped_table.pdf
+++ reference/classic132_striped_table.pdf
@@ -8,4 +8,4 @@
 Product 7 Sports 399.94 4.5

 Product 8 Sports 281.79 2.5

 Product 9 Sports 445.84 1.8

-Product 10 Electronics 276.34 3.4
+Product 10Electronics 276.34 3.4
```
</details>

### classic133_gradient_rows

- **Text Similarity:** 1.0
- **Visual Average:** 0.927
- **Overall Score:** 0.9708
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=317578 bytes, Reference=75810 bytes

Text content: ✅ Identical

### classic134_heatmap

- **Text Similarity:** 1.0
- **Visual Average:** 0.91
- **Overall Score:** 0.964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=317157 bytes, Reference=78105 bytes

Text content: ✅ Identical

### classic135_bottom_border_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9891
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=313103 bytes, Reference=58955 bytes

Text content: ✅ Identical

### classic136_financial_report_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9532
- **Overall Score:** 0.9813
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=327352 bytes, Reference=100226 bytes

Text content: ✅ Identical

### classic137_checkerboard

- **Text Similarity:** 1.0
- **Visual Average:** 0.8608
- **Overall Score:** 0.9443
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=314970 bytes, Reference=31191 bytes

Text content: ✅ Identical

### classic138_color_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9296
- **Overall Score:** 0.9718
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=315561 bytes, Reference=45006 bytes

Text content: ✅ Identical

### classic139_pattern_fills

- **Text Similarity:** 1.0
- **Visual Average:** 0.9311
- **Overall Score:** 0.9724
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=311870 bytes, Reference=80842 bytes

Text content: ✅ Identical

### classic140_rotated_text

- **Text Similarity:** 0.9583
- **Visual Average:** 0.9902
- **Overall Score:** 0.9794
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=311201 bytes, Reference=68994 bytes

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

- **Text Similarity:** 1.0
- **Visual Average:** 0.9846
- **Overall Score:** 0.9938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=311161 bytes, Reference=66621 bytes

Text content: ✅ Identical

### classic142_styled_invoice

- **Text Similarity:** 1.0
- **Visual Average:** 0.9125
- **Overall Score:** 0.965
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=329316 bytes, Reference=105680 bytes

Text content: ✅ Identical

### classic143_colored_tabs

- **Text Similarity:** 0.9972
- **Visual Average:** 0.9971
- **Overall Score:** 0.9977
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=2911 bytes, Reference=74632 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic143_colored_tabs.pdf
+++ reference/classic143_colored_tabs.pdf
@@ -12,4 +12,4 @@
 ---PAGE---

 Label Value

 Employees 50

-Open Role 5
+Open Roles 5
```
</details>

### classic144_note_style_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.9717
- **Overall Score:** 0.9887
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3203 bytes, Reference=94925 bytes

Text content: ✅ Identical

### classic145_status_badges

- **Text Similarity:** 1.0
- **Visual Average:** 0.9374
- **Overall Score:** 0.975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=330495 bytes, Reference=89140 bytes

Text content: ✅ Identical

### classic146_double_border_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9731
- **Overall Score:** 0.9892
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=323950 bytes, Reference=77024 bytes

Text content: ✅ Identical

### classic147_multi_sheet_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9766
- **Overall Score:** 0.9906
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=336481 bytes, Reference=97561 bytes

Text content: ✅ Identical

### classic148_frozen_styled_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.7318
- **Overall Score:** 0.8927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=364660 bytes, Reference=90882 bytes

Text content: ✅ Identical

### classic149_merged_styled_sections

- **Text Similarity:** 1.0
- **Visual Average:** 0.9201
- **Overall Score:** 0.968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=328457 bytes, Reference=93062 bytes

Text content: ✅ Identical

### classic150_kitchen_sink_styles

- **Text Similarity:** 0.9839
- **Visual Average:** 0.9354
- **Overall Score:** 0.9677
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=320247 bytes, Reference=121318 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic150_kitchen_sink_styles.pdf
+++ reference/classic150_kitchen_sink_styles.pdf
@@ -10,5 +10,4 @@
 This text wraps in the cell nicely

 Wrap + Center Multi-line

 Pattern Fill Gray pattern Hatched

-Large Font Size 24

-BIG
+Large Font BIG Size 24
```
</details>

### classic151_multilingual_greetings

- **Text Similarity:** 0.9291
- **Visual Average:** 0.9808
- **Overall Score:** 0.964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4541849 bytes, Reference=108265 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic151_multilingual_greetings.pdf
+++ reference/classic151_multilingual_greetings.pdf
@@ -5,9 +5,9 @@
 Korean 안녕하세요 감사합니다

 Thai สวัสดี ขอบคุณ

 Hindi नमस्ते धन्यवाद

-Arabic ﺎﺒﺣﺮﻣ ﺍﺮﻜﺷ

-Hebrew םולש הדות

-Greek Γεια σου Ευχαριστ ώ

+Arabicمرحبا شكرا

+Hebrewשלום תודה

+Greek Γεια σου Ευχαριστώ

 Russian Привет Спасибо

-Vietnamese Xin chào C ả m ơ n

-Turkish Merhaba Te ş ekkürler
+Vietnamese Xin chào Cảm ơn

+Turkish Merhaba Teşekkürler
```
</details>

### classic152_emoji_sampler

- **Text Similarity:** 1.0
- **Visual Average:** 0.9818
- **Overall Score:** 0.9927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=165117 bytes, Reference=105280 bytes

Text content: ✅ Identical

### classic153_currency_symbols

- **Text Similarity:** 0.9918
- **Visual Average:** 0.9796
- **Overall Score:** 0.9886
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=327744 bytes, Reference=66698 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic153_currency_symbols.pdf
+++ reference/classic153_currency_symbols.pdf
@@ -5,9 +5,9 @@
 Japanese Yen ¥ ¥123,456

 Chinese Yuan ¥ ¥1,234.56

 Korean Won ₩ ₩1,234,560

-Indian Rupee ₹ ₹ 1,23,456

-Thai Baht ฿ ฿ 1,234.56

-Russian Ruble ₽ ₽ 1 234,56

-Turkish Lira ₺ ₺ 1.234,56

-Bitcoin ₿ ₿ 0.05

+Indian Rupee ₹ ₹1,23,456

+Thai Baht ฿ ฿1,234.56

+Russian Ruble ₽ ₽1 234,56

+Turkish Lira ₺ ₺1.234,56

+Bitcoin ₿ ₿0.05

 Swiss Franc CHF CHF 1'234.56
```
</details>

### classic154_math_symbols

- **Text Similarity:** 0.9881
- **Visual Average:** 0.986
- **Overall Score:** 0.9896
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1090941 bytes, Reference=85712 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic154_math_symbols.pdf
+++ reference/classic154_math_symbols.pdf
@@ -6,5 +6,5 @@
 Calculus ∫ ∬ ∮ ∂ ∇ ∑ ∏ √

 Arrows → ← ↑ ↓ ↔ ⇒ ⇐ ⇔

 Misc ∞ ℏ ℝ ℤ ℚ ℕ ℂ

-Superscripts x² y³ a ⁿ e ⁱ

-Subscripts H ₂ O CO ₂ x ₙ a ᵢ
+Superscripts x² y³ aⁿ eⁱ

+Subscripts H₂O CO₂ x ₙ aᵢ
```
</details>

### classic155_diacritical_marks

- **Text Similarity:** 1.0
- **Visual Average:** 0.9888
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4325225 bytes, Reference=63383 bytes

Text content: ✅ Identical

### classic156_rtl_bidi_text

- **Text Similarity:** 0.5912
- **Visual Average:** 0.9953
- **Overall Score:** 0.8346
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=328793 bytes, Reference=47337 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic156_rtl_bidi_text.pdf
+++ reference/classic156_rtl_bidi_text.pdf
@@ -1,5 +1,5 @@
 Script Text

-Arabic ﺎﺒﺣﺮﻣ ﻢﻟﺎﻌﻟﺎﺑ

-Hebrew םולש םלוע

-Persian ﻡﻼﺳ ﺍیﻥﺩ

-Urdu ﻮﻟیہ ﺍیﻥﺩ
+Arabicمرحبا بالعالم

+Hebrewשלום עולם

+Persianسالم دنیا

+Urduہیلو دنیا
```
</details>

### classic157_cjk_extended

- **Text Similarity:** 1.0
- **Visual Average:** 0.9761
- **Overall Score:** 0.9904
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=368113 bytes, Reference=118155 bytes

Text content: ✅ Identical

### classic158_emoji_skin_tones

- **Text Similarity:** 1.0
- **Visual Average:** 0.9819
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=297658 bytes, Reference=99585 bytes

Text content: ✅ Identical

### classic159_zwj_emoji

- **Text Similarity:** 0.9372
- **Visual Average:** 0.985
- **Overall Score:** 0.9689
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=361522 bytes, Reference=106035 bytes

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

- **Text Similarity:** 0.9915
- **Visual Average:** 0.9914
- **Overall Score:** 0.9932
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=457612 bytes, Reference=110515 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic160_punctuation_marks.pdf
+++ reference/classic160_punctuation_marks.pdf
@@ -1,7 +1,7 @@
 Type Characters

 Latin . , ; : ! ? … — – ' '  « »

 CJK 。、；：！？「」『』【】（）

-Arabic ، ؛٪ ؟ ﷽

+Arabic ، ؛ ؟ ٪ ﷽

 Devanagari । ॥ ꣸ ꣹ ꣺

 Thai ฯ ๆ ๏ ๚ ๛

 Misc brackets ⟨⟩ ⟪⟫ ⌈⌉ ⌊⌋ ‖

```
</details>

### classic161_box_drawing

- **Text Similarity:** 0.9976
- **Visual Average:** 0.9854
- **Overall Score:** 0.9932
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=329615 bytes, Reference=94886 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic161_box_drawing.pdf
+++ reference/classic161_box_drawing.pdf
@@ -2,6 +2,6 @@
 Light box ┌──┬──┐│  │  │├──┼──┤└──┴──┘

 Heavy box ┏━━┳━━┓┃  ┃  ┃┣━━╋━━┫┗━━┻━━┛

 Double box ╔══╦══╗║  ║  ║╠══╬══╣╚══╩══╝

-Blocks ▀ ▁▂▃ ▄ ▅▆▇ █ ░▒▓

+Blocks ▀ ▁ ▂▃ ▄ ▅▆▇ █ ░▒▓

 Geometric ■□▪▫▲ △ ▼ ▽◆◇ ○● ◎

 Braille ⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊
```
</details>

### classic162_cjk_emoji_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9804
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=299254 bytes, Reference=133693 bytes

Text content: ✅ Identical

### classic163_cyrillic_alphabets

- **Text Similarity:** 0.9398
- **Visual Average:** 0.9832
- **Overall Score:** 0.9692
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3911299 bytes, Reference=56100 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic163_cyrillic_alphabets.pdf
+++ reference/classic163_cyrillic_alphabets.pdf
@@ -1,6 +1,6 @@
 Language Sample Text

-Russian Съешь ещё этих мягких французских булок , да вып

-Ukrainian Жебракують ф і лософи при ґ анку церкви в Гадяч і .

-Serbian Ђ ура ђ Бранкови ћ ј е био владар Срби ј е .

-Bulgarian Щъркел яде бялата жаба .

-Mongolian Би монгол хэл дээр бичиж байна .
+Russian Съешь ещё этих мягких французских булок, да выпей чаю.

+Ukrainian Жебракують філософи при ґанку церкви в Гадячі.

+Serbian Ђурађ Бранковић је био владар Србије.

+Bulgarian Щъркел яде бялата жаба.

+Mongolian Би монгол хэл дээр бичиж байна.
```
</details>

### classic164_indic_scripts

- **Text Similarity:** 1.0
- **Visual Average:** 0.9945
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=145966 bytes, Reference=53352 bytes

Text content: ✅ Identical

### classic165_southeast_asian

- **Text Similarity:** 0.9672
- **Visual Average:** 0.9923
- **Overall Score:** 0.9838
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=147035 bytes, Reference=94767 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic165_southeast_asian.pdf
+++ reference/classic165_southeast_asian.pdf
@@ -1,6 +1,7 @@
 Script Sample

 Thai ภาษาไทยเป็นภาษาที่มีวรรณยุกต์

-Lao ພາສາລາວເປັນພາສາທີ່ສວຍງາມ

-Myanmar မြန်မာဘာသာစကားသည် လှပသည်

+ີ່

+Lao ພາສາລາວເປັນພາສາທ ສວຍງາມ

+Myanmar မြန်ြာဘာသာစကာားသည် လှပသည်

 Khmer ភាសាខ្មែរជាភាសាចំណាស់

-Tibetan བོད་ཀྱི་སྐད་ཡིག་ནི་གལ་ཆེན་པོ་ཡིན།
+Tibetan བོད་ཀྱི་སྐད་ཡྱིག་ནྱི་གལ་ཆེན་པོ་ཡྱིན།
```
</details>

### classic166_emoji_progress

- **Text Similarity:** 1.0
- **Visual Average:** 0.9691
- **Overall Score:** 0.9876
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=295066 bytes, Reference=101519 bytes

Text content: ✅ Identical

### classic167_musical_symbols

- **Text Similarity:** 1.0
- **Visual Average:** 0.9886
- **Overall Score:** 0.9954
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=638288 bytes, Reference=107225 bytes

Text content: ✅ Identical

### classic168_mixed_ltr_rtl_styled

- **Text Similarity:** 0.8696
- **Visual Average:** 0.9732
- **Overall Score:** 0.9371
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=326691 bytes, Reference=83592 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic168_mixed_ltr_rtl_styled.pdf
+++ reference/classic168_mixed_ltr_rtl_styled.pdf
@@ -1,5 +1,5 @@
 Code Name Price

 EN-001 Programming Book $29.99

 FR-002 Livre de code €25.00

-AR-003ﺔﺠﻣﺮﺑ ﺏﺎﺘﻛ50 SAR

-HE-004דוק רפס₪120
+AR-003كتاب برمجة50 SAR

+HE-004ספר קוד₪120
```
</details>

### classic169_korean_invoice

- **Text Similarity:** 1.0
- **Visual Average:** 0.9788
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=369437 bytes, Reference=118888 bytes

Text content: ✅ Identical

### classic170_emoji_dashboard

- **Text Similarity:** 1.0
- **Visual Average:** 0.9654
- **Overall Score:** 0.9862
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=150391 bytes, Reference=137376 bytes

Text content: ✅ Identical

### classic171_ipa_phonetic

- **Text Similarity:** 0.9703
- **Visual Average:** 0.9867
- **Overall Score:** 0.9828
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4094817 bytes, Reference=76615 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic171_ipa_phonetic.pdf
+++ reference/classic171_ipa_phonetic.pdf
@@ -5,4 +5,4 @@
 Vowels i y ɨ ʉ ɯ u e ø ɘ ɵ ɤ o ɛ œ ɜ ɞ ʌ ɔ æ a ɶ ɑ ɒ

 Tones ˥ ˦ ˧ ˨ ˩ ˥˩ ˩˥

 Diacritics ʰ ʷ ʲ ˠ ˤ ⁿ ˡ

-Example word / ˌ ɪ nt əˈ næ ʃ ə n ə l/ (international)
+Example word /ˌɪntəˈnæʃənəl/ (international)
```
</details>

### classic172_emoji_timeline

- **Text Similarity:** 1.0
- **Visual Average:** 0.9769
- **Overall Score:** 0.9908
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=153155 bytes, Reference=117160 bytes

Text content: ✅ Identical

### classic173_african_languages

- **Text Similarity:** 0.9783
- **Visual Average:** 0.983
- **Overall Score:** 0.9845
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=327713 bytes, Reference=64361 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic173_african_languages.pdf
+++ reference/classic173_african_languages.pdf
@@ -1,8 +1,8 @@
 Language Greeting Region

 Swahili Habari! Karibu sana. East Africa

-Amharic ሰላም! እንኳን ደህና መጣህ. Ethiopia

-Yoruba Ẹ kú àár ọ̀ ! Ẹ kú al ẹ́ ! Nigeria

+Amharic ሰላም ! እንኳን ደህና መጣህ . Ethiopia

+Yoruba Ẹ kú àárọ̀! Ẹ kú alẹ́! Nigeria

 Zulu Sawubona! Unjani? South Africa

 Hausa Sannu! Barka da zuwa. West Africa

-Igbo Nn ọọ ! Ked ụ ? Nigeria

-Tigrinya ሰላም! ከመይ ኣለኻ? Eritrea
+Igbo Nnọọ! Kedụ? Nigeria

+Tigrinya ሰላም ! ከመይ ኣለኻ ? Eritrea
```
</details>

### classic174_technical_symbols

- **Text Similarity:** 0.9971
- **Visual Average:** 0.9797
- **Overall Score:** 0.9907
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=869778 bytes, Reference=81967 bytes

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

- **Text Similarity:** 0.9686
- **Visual Average:** 0.9728
- **Overall Score:** 0.9766
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4174562 bytes, Reference=191190 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic175_multiscript_catalog.pdf
+++ reference/classic175_multiscript_catalog.pdf
@@ -1,9 +1,9 @@
 # Product (EN) Product (Local) Price Icon

 1 Green Tea 緑茶 ¥500 🍵

 2 Kimchi 김치 ₩3,000 🥬

-3 Samosa समोसा ₹ 50 🥟

+3 Samosa समोसा ₹50 🥟

 4 Croissant Croissant €2.50 🥐

 5 Taco Taco $3.99 🌮

-6 Borscht Борщ ₽ 250 🍲

-7 Falafel ﻞﻓﻼﻓ ₪15 🧆

-8 Pad Thai ผัดไทย ฿ 80 🍜
+6 Borscht Борщ ₽250 🍲

+7 Falafelفالفل₪15 🧆

+8 Pad Thai ผัดไทย ฿80 🍜
```
</details>

### classic176_combining_characters

- **Text Similarity:** 0.9469
- **Visual Average:** 0.9861
- **Overall Score:** 0.9732
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=541356 bytes, Reference=68236 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic176_combining_characters.pdf
+++ reference/classic176_combining_characters.pdf
@@ -1,7 +1,7 @@
 Type Examples

-Single combining é = e + ́ ñ = n +

-Double combining ệ = e + ̣ +

+Single combining é = e + ́   ñ = n + ̃

+Double combining ệ = e + ̣ + ̂

 Vietnamese ắ ằ ẵ ẳ ặ ố ồ ỗ ổ ộ ứ ừ ữ ử ự

-Zalgo-like H e ̵̖̘ ̷̝̣ l l ̶̤ o ̴̥

-Precomposed vs decomposed ü (precomposed) vs u (decomposed) ̈

+Zalgo-like H ̵̖̘e ̣l ̶̤l ̴̥o ̸̮

+Precomposed vs decomposed ü (precomposed) vs ü (decomposed)

 Hangul Jamo ㅎ ㅏ ㄴ ㄱ ㅡ ㄹ → 한글
```
</details>

### classic177_emoji_calendar

- **Text Similarity:** 1.0
- **Visual Average:** 0.9771
- **Overall Score:** 0.9908
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=156896 bytes, Reference=107156 bytes

Text content: ✅ Identical

### classic178_caucasus_ethiopic

- **Text Similarity:** 0.9894
- **Visual Average:** 0.9849
- **Overall Score:** 0.9897
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=336838 bytes, Reference=58144 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic178_caucasus_ethiopic.pdf
+++ reference/classic178_caucasus_ethiopic.pdf
@@ -1,6 +1,6 @@
 Script Sample Text

-Georgian საქართველო არის ძველი ცივილიზაცია .

-Armenian Հայաստանը հին քաղաքակրթություն ունի .

+Georgian საქართველო არის ძველი ცივილიზაცია.

+Armenian Հայաստանը հին քաղաքակրթություն ունի.

 Ethiopic ኢትዮጵያ የጥንታዊ ሥልጣኔ ምድር ናት።

-Georgian mkhedru ა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

-Armenian alphabe Ա Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
+Georgian mkhedrა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

+Armenian alphab Ա Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
```
</details>

### classic179_emoji_inventory

- **Text Similarity:** 1.0
- **Visual Average:** 0.9655
- **Overall Score:** 0.9862
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=300476 bytes, Reference=138581 bytes

Text content: ✅ Identical

### classic180_polyglot_paragraph

- **Text Similarity:** 0.9772
- **Visual Average:** 0.9868
- **Overall Score:** 0.9856
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4229073 bytes, Reference=153951 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic180_polyglot_paragraph.pdf
+++ reference/classic180_polyglot_paragraph.pdf
@@ -2,8 +2,8 @@
 English The quick brown fox.

 Japanese 速い茶色の狐。

 Korean 빠른 갈색 여우 .

-Russian Быстрая бурая лиса .

-Greek Η γρ ή γορη αλεπο ύ .

-Thai สุนัขจิ้งจอกสีน้ำตาล

-Hindi तेज़ भूरी लोमड़ी

+Russian Быстрая бурая лиса.

+Greek Η γρήγορη αλεπού.

+Thai สุนัขจิ้งจอกสีน ้ำตำล

+Hindi तेज़ भूरी लोमडी

 Emoji 🦊 ➡️ 🐕
```
</details>

### classic181_feedback_tracker_with_images

- **Text Similarity:** 0.9865
- **Visual Average:** 0.8748
- **Overall Score:** 0.9445
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=12105 bytes, Reference=93919 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic181_feedback_tracker_with_images.pdf
+++ reference/classic181_feedback_tracker_with_images.pdf
@@ -6,6 +6,7 @@
 2026-03-03 Eve Wrong checklist used for application

 2026-03-03 Frank Name and contact details missing on cover letter

 2026-03-04 Grace Unable to scroll and read the privacy policy

-2026-03-04 Hank Applicant has three children, only one birth cert uploaded

+2026-03-04 Hank Applicant has three children, only one birth cert up

 ---PAGE---

-Screenshot
+Screenshot

+ploaded
```
</details>

### classic182_dense_long_text_columns

- **Text Similarity:** 0.9291
- **Visual Average:** 0.9603
- **Overall Score:** 0.9558
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6293 bytes, Reference=105199 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic182_dense_long_text_columns.pdf
+++ reference/classic182_dense_long_text_columns.pdf
@@ -1,19 +1,19 @@
 ID First Name Last Name Department Position Title

 1001 Alexander Papadopoulos Engineering Senior Software Engineer

-1002 Magdalena Kowalczyk Human Resources HR Business Partner Lead

+1002 Magdalena Kowalczyk Human Resource HR Business Partner Lead

 1003 Christopher O'Sullivan Finance Chief Financial Analyst

-1004 Priyanka Ramasubraman Marketing Digital Marketing Strategist

-1005 Jean-Pierre Beaumont Sales Regional Sales Director (EMEA)

+1004 Priyanka Ramasubrama Marketing Digital Marketing Strategist

+1005 Jean-Pierre Beaumont Sales Regional Sales Director (EM

 1006 Anastasia Volkov Engineering Principal Data Scientist

-1007 Mohammed Al-Rashidi Operations Supply Chain Optimization Manager

+1007 Mohammed Al-Rashidi Operations Supply Chain Optimization

 1008 Guadalupe Hernandez Legal Senior Corporate Counsel

 ---PAGE---

 Email Address Phone Notes

-alexander.papadopoulos@example.c +1-555-0101 Transferred from Athens office in Q2

-magdalena.kowalczyk@example.co +1-555-0102 Fluent in Polish, German, and English

-christopher.osullivan@example.com +1-555-0103 CPA certified, MBA from Wharton

+alexander.papadopoulos@example+1-555-0101 Transferred from Athens office in Q2

+magdalena.kowalczyk@example.co+1-555-0102 Fluent in Polish, German, and English

+christopher.osullivan@example.co +1-555-0103 CPA certified, MBA from Wharton

 priyanka.r@example.com +1-555-0104 Led rebranding campaign for APAC region

-jean-pierre.beaumont@example.com +1-555-0105 15+ years experience in B2B SaaS

+jean-pierre.beaumont@example.co+1-555-0105 15+ years experience in B2B SaaS

 anastasia.volkov@example.com +1-555-0106 PhD in Machine Learning, Stanford

-mohammed.alrashidi@example.com +1-555-0107 Six Sigma Black Belt certified

-guadalupe.hernandez@example.co +1-555-0108 Bar admitted in CA, NY, TX
+mohammed.alrashidi@example.co+1-555-0107 Six Sigma Black Belt certified

+guadalupe.hernandez@example.co+1-555-0108 Bar admitted in CA, NY, TX
```
</details>

### classic183_mixed_content_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9357
- **Overall Score:** 0.9743
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5585 bytes, Reference=79725 bytes

Text content: ✅ Identical

### classic184_wide_narrow_columns

- **Text Similarity:** 1.0
- **Visual Average:** 0.9142
- **Overall Score:** 0.9657
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=14288 bytes, Reference=102871 bytes

Text content: ✅ Identical

### classic185_tall_rows_vertical_align

- **Text Similarity:** 1.0
- **Visual Average:** 0.9805
- **Overall Score:** 0.9922
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=317713 bytes, Reference=72408 bytes

Text content: ✅ Identical

### classic186_multi_sheet_image_report

- **Text Similarity:** 1.0
- **Visual Average:** 0.9353
- **Overall Score:** 0.9741
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=7554 bytes, Reference=92322 bytes

Text content: ✅ Identical

### classic187_bug_report_with_screenshots

- **Text Similarity:** 1.0
- **Visual Average:** 0.8547
- **Overall Score:** 0.9419
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7902 bytes, Reference=94413 bytes

Text content: ✅ Identical

### classic188_merged_header_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9447
- **Overall Score:** 0.9779
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7637 bytes, Reference=81139 bytes

Text content: ✅ Identical

### classic189_alternating_image_text_rows

- **Text Similarity:** 0.8974
- **Visual Average:** 0.8632
- **Overall Score:** 0.9042
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=9498 bytes, Reference=93481 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic189_alternating_image_text_rows.pdf
+++ reference/classic189_alternating_image_text_rows.pdf
@@ -1,7 +1,7 @@
 Step Action Expected Result Evidence

-Step 1 Open login page Login form is displayed with em See below

-Step 2 Enter valid credentials Dashboard loads within 3 secon See below

-Step 3 Click export button CSV file downloads with all visi See below

-Step 4 Apply date filter Table updates to show only mat See below

-Step 5 Resize browser window Layout remains responsive at 7 See below

-Step 6 Toggle dark mode All components switch to dark t See below
+Step 1 Open login page Login form is displayed with e See below

+Step 2 Enter valid credentials Dashboard loads within 3 secoSee below

+Step 3 Click export button CSV file downloads with all visSee below

+Step 4 Apply date filter Table updates to show only mSee below

+Step 5 Resize browser window Layout remains responsive at See below

+Step 6 Toggle dark mode All components switch to darkSee below
```
</details>

### classic190_dashboard_kpi_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9437
- **Overall Score:** 0.9775
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6622 bytes, Reference=96601 bytes

Text content: ✅ Identical

### classic191_payroll_calculator

- **Text Similarity:** 0.7135
- **Visual Average:** 0.895
- **Overall Score:** 0.8434
- **Pages:** MiniPdf=9, Reference=9
- **File Size:** MiniPdf=103959 bytes, Reference=189742 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic191_payroll_calculator.pdf
+++ reference/classic191_payroll_calculator.pdf
@@ -1,64 +1,66 @@
 Employee Register

 Information contained in this employee register is highly confidential

-ID Employee's Name M/F Hire Date Occupation

-1Adam Jones M 2013-02-15 Senior Accountant

-2Nichola Brown F 2011-09-28 CR Manager

-3Benny Erwin M 2011-05-11 Applications PM

-4Rachel Kim F 2016-08-03 HR Specialist

-5Carlos Ruiz M 2019-11-20 Sales Rep

+ID Employee's Name M/F Hire Date

+1 Adam Jones M 2013-02-15

+2 Nichola Brown F 2011-09-28

+3 Benny Erwin M 2011-05-11

+4 Rachel Kim F 2016-08-03

+5 Carlos Ruiz M 2019-11-20

 ---PAGE---

-Regular Hourly Rate Overtime Hourly Exempt from

-Annual Salary ($) Pay Frequency

-($) Rate ($) Overtime

-42,000.00 20.19 30.29 Yes Monthly

-54,481.00 26.19 39.29 Yes Monthly

-48,785.00 23.45 35.18 No Monthly

-39,500.00 19.00 28.50 Yes Bi-Weekly

-51,200.00 24.62 36.92 No Monthly

+Regular Hourly Overtime Hourly Exempt from

+Occupation Annual Salary ($)

+Rate ($) Rate ($) Overtime

+Senior Accountant 42,000.00 20.19 30.29 Yes

+CR Manager 54,481.00 26.19 39.29 Yes

+Applications PM 48,785.00 23.45 35.18 No

+HR Specialist 39,500.00 19.00 28.50 Yes

+Sales Rep 51,200.00 24.62 36.92 No

 ---PAGE---

-Additional Withhold

-W-4 Form Filing Status Federal Allowance

-($)

-W-4 (2020+) Exempt 2 50.00

-W-4 (2020+) Married 4 0.00

-W-4 (pre-2020) Married 4 0.00

-W-4 (2020+) Single 1 25.00

-W-4 (2020+) Single 0 0.00

+Additional

+Pay Frequency W-4 Form Filing Status Federal Allowance

+Withhold ($)

+Monthly W-4 (2020+) Exempt 2 50.00

+Monthly W-4 (2020+) Married 4 0.00

+Monthly W-4 (pre-2020) Married 4 0.00

+Bi-Weekly W-4 (2020+) Single 1 25.00

+Monthly W-4 (2020+) Single 0 0.00

 ---PAGE---

 Payroll Calculator

-Pay Period Hours

-ID Employee Name From To Regular Hours Holiday Hours

-1Adam Jones 2020-12-01 2020-12-31 173.33 0.00

-2Nichola Brown 2020-12-01 2020-12-31 173.33 0.00

-3Benny Erwin 2020-12-01 2020-12-31 173.33 0.00

-4Rachel Kim 2020-12-01 2020-12-31 80.00 8.00

-5Carlos Ruiz 2020-12-01 2020-12-31 173.33 0.00

+Pay Period

+ID Employee Name From To Regular Hours

+1 Adam Jones 2020-12-01 2020-12-31 173.33

+2 Nichola Brown 2020-12-01 2020-12-31 173.33

+3 Benny Erwin 2020-12-01 2020-12-31 173.33

+4 Rachel Kim 2020-12-01 2020-12-31 80.00

+5 Carlos Ruiz 2020-12-01 2020-12-31 173.33

 ---PAGE---

-Taxable Compensat Post-Tax Reimburse

-Vacation Hours Sick Hours Overtime Hours Pre-Tax Deductions

-ion ments

-0.00 0.00 0.00 500.00 0.00 500.00

+Hours

+Taxable Pre-Tax

+Holiday Hours Vacation Hours Sick Hours Overtime Hours

+Compensation Deductions

+0.00 0.00 0.00 0.00 500.00 0.00

 0.00 0.00 0.00 0.00 0.00 0.00

-0.00 0.00 8.00 0.00 0.00 0.00

-0.00 0.00 0.00 0.00 0.00 0.00

-16.00 0.00 4.00 200.00 0.00 200.00

+0.00 0.00 0.00 8.00 0.00 0.00

+8.00 0.00 0.00 0.00 0.00 0.00

+0.00 16.00 0.00 4.00 200.00 0.00

 ---PAGE---

-Pre-Tax Adjustments Withholdings

-Tax Deferral Plan

-Gross 
... (806 more characters)

```
</details>

### nthu_article

- **Text Similarity:** 0.9324
- **Visual Average:** 0.8205
- **Overall Score:** 0.8012
- **Pages:** MiniPdf=21, Reference=20
- **File Size:** MiniPdf=4691713 bytes, Reference=798510 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/nthu_article.pdf
+++ reference/nthu_article.pdf
@@ -1,7 +1,7 @@
 國立清華大學中國文學系碩博士學位論文格式規範

-經111年11月09日111學年度第一學期第3次系務會議通過

-一、論文編印項目次序與頁碼編排

-（一）紙本論文次序

+經 111 年 11 月 09 日 111 學年度第一學期第 3 次系務會議通過

+一、 論文編印項目次序與頁碼編排

+（一） 紙本論文次序

 封面 Front Cover

 書名頁 Title Page（同封面）

 論文指導教授推薦書(影本)

@@ -20,147 +20,145 @@
 名詞附錄 Glossary of Terms (可免)

 索引 Index (可免)

 封底 Back Cover、書背 Spine

-（二）電子檔論文次序

+（二） 電子檔論文次序

 去掉前項 藍字 項目。

-（三）頁碼編排

-1，頁碼從「誌謝辭」開始編排，用小寫羅馬數字標示頁碼（ i, ii, iii… ）。

-2，論文本文開始至最末，用阿拉伯數字標示頁碼。

-3，頁碼標於頁面底端，置中。

-（四）版面配置

-包括封面，全書版面配置邊界設定為下上2 .54 公分，左右 2.9 公分

-二、

+（三） 頁碼編排

+1 ，頁碼從「誌謝辭」開始編排，用小寫羅馬數字標示頁碼（ i, ii, iii… ）。

+2 ，論文本文開始至最末，用阿拉伯數字標示頁碼。

+3 ，頁碼標於頁面底端，置中。

+（四） 版面配置

+包括封面，全書版面配置邊界設定為下上 2.54 公分，左右 2.9 公分

 1

 ---PAGE---

-封面（與書名頁）、書背格式

-封面顏色：碩士論文 土黃色 ，上光、膠裝；博士論文 淺藍色 ，上光、膠裝（

-https://registra.site.nthu.edu.tw/var/file/211/1211/img/75/203558730.pdf ）。 論

-文封面建議採用亮面雲彩紙。如下範例 A 、範例 B （可直接下載套用。此為教

-務處公布之格式）。

+二、封面（與書名頁）、書背格式

+封面顏色：碩士論文 土黃色 ，上光、膠裝；博士論文 淺藍色 ，上光、膠裝

+（ https://registra.site.nthu.edu.tw/var/file/211/1211/img/75/203558730.pdf ）。論

+文封面建議採用亮面雲彩紙。如下範例 A 、範例 B （可直接下載套用。此為

+教務處公布之格式）。

 三、論文指導教授推薦書、學位考試委員審定書、授權書

-推薦書、審定書自中文系網頁下載，相關規定請洽詢系辦。授權書包括：「

-國立清華大學博碩士論文全文電子檔著作權授權書」、「國立清華大學博碩

-士紙本論文著作權授權書」、「國家圖書館博碩士論文電子檔案上網授權

-書」、「華藝博碩士論文全文電子學位論文授權書」，相關規定可參 附件一

-，或國立清華大學圖書館博碩士論文全系統，常見問題第 7 項。

+推薦書、審定書自中文系網頁下載，相關規定請洽詢系辦。授權書包括：

+「國立清華大學博碩士論文全文電子檔著作權授權書」、「國立清華大學博

+碩士紙本論文著作權授權書」、「國家圖書館博碩士論文電子檔案上網授權

+書」、「華藝博碩士論文全文電子學位論文授權書」，相關規定可參 附件一 ，

+或國立清華大學圖書館博碩士論文全系統，常見問題第 7 項。

 四、中英文摘要

 摘要應裝訂於論文內。內容應說明研究目的、論述重點、研究方法或程序、

 研究結果與結論及關鍵詞（ 5-7 個）等，以 500 字為原則，至多不超過二頁，

-橫式書寫，行距建議設定為 1.2 行（ word 中選「多行」，設定 1.2 ），段距 0.5

-行。標題「摘要」二字或「 Abstract 」1 6 級字，置中，可加黑。「關鍵詞：」

-或「 Key words: 」12級，可加黑；與摘要主文間，空一行。英文與數字用

-Times New Roman 。如範例 C （中文）與範例 D （英文）。

-五、目次

+橫式書寫，行距建議設定為 1.2 行 （ word 中選「多行」，設定 1.2 ），段距

+0.5 行 。標題「摘要」二字或「 Abstract 」 16 級字，置中，可加黑。「關鍵

+詞：」或「 Key words: 」 12 級，可加黑；與摘要主文間，空一行。英文與數

+字用 Times New Roman 。如範例 C （中文）與範例 D （英文）。

+五、 目次

 包括各章、各節、節下小標之標題、參考文獻、附錄及其所在之頁數。「目

-次」二字標題置中， 16 級標楷體。各目次 12 級字。若有表次、圖次，則列於

-其後，均使用新頁開始表列，「表目次」（或「圖目次」）二字 16 級標楷

+次」二字標題置中， 16 級標楷體。各目次 12 級字。若有表次、圖次，則列

+於其後，均使用新頁開始表列，「表目次」（或「圖目次」）二字 16 級標楷

 體，其下各表（或各圖）用 12 級字。英文與數字用 Times New Roman 。目次

 格式如範例 E ，表次格式如範例 F ，圖次格式如範例 G 。

 六、論文正文

-（一）章節標題

+（一） 章節標題

 章次與章名間空一格，如「第一章　緒論」。 18 級，標楷體，置中。

 各章新頁起。節次與節名間空一格，如「第一節　研究動機」。 16 級，

 標楷體，置中。章節以下小節所使用符號，依「一、」、「（一）」、

-「1、」、「（1）」 …… 等順序表示， 14 級（或 12 級），左齊。

-（二）內文

-1，請用新式標號，惟書名號改用《》，篇名號改用〈〉。在行文

-中，書名和篇名連用時，省略篇名號，如《莊子 ‧ 天下》。若為英

-文，書名請用斜體，篇名請用 “ ” 。日文翻譯成中文，行文時亦請

+「 1 、」、「（ 1 ）」……等順序表示， 14 級（或 12 級），左齊。

+（二） 內文

+1 ，請用新式標號，惟書名號改用《》，篇名號改用〈〉。在行文中，

+書名和篇名連用時，省略篇名號，如《莊子‧天下》。若為英文，

+書名請用斜體，篇名請用“ ”。日文翻譯成中文，行文時亦請一

 2

 ---PAGE---

-一併改用中文新式標號。

-2，行距建議設定為 1.2 行（ word 中選「多行」，設定 1.2 ），各段「左

-右對齊」。

-（三）引文

+併改用中文新式標號。

+2 ，行距建議設定為 1.2 行 （ word 中選「多行」，設定 1.2 ），各段

+「左右對齊」。

+（三） 引文

 所有引文均須核對無誤。引文左縮排 3 字元，標楷體 12 級。若需特別

 引用之外文，也依上述格式處理，字體建議用 Times New Roman 。外

 文引文，亦可翻譯成流暢達意之中文，於註腳中附上所引篇章之外文

 原名，並得視需要將所徵引之原文置於註腳中。

... (17267 more characters)

```
</details>

### nthu_article_new

**Error:** Reference PDF not found

### nthu_article_old

**Error:** Reference PDF not found

## Improvement Suggestions

### ⚠ Low-Score Test Cases (below 0.8)

1. **classic116_percent_stacked_area** (score: 0.5521)
1. **classic108_stacked_area_chart** (score: 0.5731)
1. **classic18_large_dataset** (score: 0.6374)
1. **classic60_large_wide_table** (score: 0.6718)
1. **classic12_sparse_columns** (score: 0.6995)
1. **classic09_long_text** (score: 0.7162)
1. **classic120_chart_with_date_axis** (score: 0.7554)
1. **classic103_pie_chart_with_labels** (score: 0.7953)

Review the text diffs and visual comparisons above to identify specific rendering issues.
