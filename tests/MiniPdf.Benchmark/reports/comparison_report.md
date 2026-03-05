# MiniPdf vs Reference PDF Comparison Report

Generated: 2026-03-05T18:19:36.050619

## Summary

| # | Test Case | Text Sim | Visual Avg | Pages (M/R) | Overall |
|---|-----------|----------|------------|-------------|--------|
| 1 | 🟢 classic01_basic_table_with_headers | 1.0 | 0.9969 | 1/1 | **0.9988** |
| 2 | 🟢 classic02_multiple_worksheets | 0.9971 | 0.998 | 3/3 | **0.998** |
| 3 | 🟢 classic03_empty_workbook | 1.0 | 1.0 | 1/1 | **1.0** |
| 4 | 🟢 classic04_single_cell | 1.0 | 0.9998 | 1/1 | **0.9999** |
| 5 | 🟢 classic05_wide_table | 1.0 | 0.9938 | 3/3 | **0.9975** |
| 6 | 🟢 classic06_tall_table | 1.0 | 0.9447 | 5/5 | **0.9779** |
| 7 | 🟢 classic07_numbers_only | 1.0 | 0.999 | 1/1 | **0.9996** |
| 8 | 🟢 classic08_mixed_text_and_numbers | 1.0 | 0.9979 | 1/1 | **0.9992** |
| 9 | 🔴 classic09_long_text | 0.6785 | 0.577 | 7/12 | **0.6022** |
| 10 | 🟢 classic100_stacked_bar_chart | 0.9663 | 0.9075 | 1/1 | **0.9495** |
| 11 | 🟢 classic101_percent_stacked_bar | 0.9623 | 0.8783 | 1/1 | **0.9362** |
| 12 | 🟢 classic102_line_chart_with_markers | 0.8372 | 0.9884 | 2/2 | **0.9302** |
| 13 | 🟡 classic103_pie_chart_with_labels | 0.7368 | 0.9739 | 2/2 | **0.8843** |
| 14 | 🟡 classic104_combo_bar_line_chart | 0.7872 | 0.7538 | 2/2 | **0.8164** |
| 15 | 🟡 classic105_3d_bar_chart | 0.9032 | 0.7426 | 2/2 | **0.8583** |
| 16 | 🟢 classic106_3d_pie_chart | 0.8243 | 0.9675 | 2/2 | **0.9167** |
| 17 | 🟡 classic107_multi_series_line | 0.7375 | 0.7766 | 2/2 | **0.8056** |
| 18 | 🟢 classic108_stacked_area_chart | 0.9643 | 0.896 | 1/1 | **0.9441** |
| 19 | 🟢 classic109_scatter_with_trendline | 0.8226 | 0.985 | 2/2 | **0.923** |
| 20 | 🟢 classic10_special_xml_characters | 1.0 | 0.9967 | 1/1 | **0.9987** |
| 21 | 🟡 classic110_chart_with_legend | 0.8372 | 0.7792 | 2/2 | **0.8466** |
| 22 | 🟢 classic111_chart_with_axis_labels | 0.8267 | 0.9765 | 2/2 | **0.9213** |
| 23 | 🟡 classic112_multiple_charts | 0.875 | 0.7595 | 2/2 | **0.8538** |
| 24 | 🟡 classic113_chart_sheet | 0.9259 | 0.7339 | 2/2 | **0.8639** |
| 25 | 🟢 classic114_chart_large_dataset | 0.9015 | 0.8874 | 4/4 | **0.9156** |
| 26 | 🟢 classic115_chart_negative_values | 0.8316 | 0.9717 | 2/2 | **0.9213** |
| 27 | 🟢 classic116_percent_stacked_area | 0.9649 | 0.8802 | 1/1 | **0.938** |
| 28 | 🟡 classic117_stock_ohlc_chart | 0.7938 | 0.7282 | 2/2 | **0.8088** |
| 29 | 🟢 classic118_bar_chart_custom_colors | 0.9275 | 0.9601 | 2/2 | **0.955** |
| 30 | 🟢 classic119_dashboard_multi_charts | 0.8409 | 0.9341 | 2/2 | **0.91** |
| 31 | 🟢 classic11_sparse_rows | 1.0 | 0.9995 | 2/2 | **0.9998** |
| 32 | 🟡 classic120_chart_with_date_axis | 0.6195 | 0.7833 | 2/2 | **0.7611** |
| 33 | 🟢 classic121_thin_borders | 1.0 | 0.9934 | 1/1 | **0.9974** |
| 34 | 🟢 classic122_thick_outer_thin_inner | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 35 | 🟢 classic123_dashed_borders | 0.9802 | 0.9964 | 1/1 | **0.9906** |
| 36 | 🟢 classic124_colored_borders | 1.0 | 0.9929 | 1/1 | **0.9972** |
| 37 | 🟢 classic125_solid_fills | 0.9869 | 0.9927 | 1/1 | **0.9918** |
| 38 | 🟢 classic126_dark_header | 0.9953 | 0.9915 | 1/1 | **0.9947** |
| 39 | 🟢 classic127_font_styles | 0.9964 | 0.9924 | 1/1 | **0.9955** |
| 40 | 🟢 classic128_font_sizes | 0.982 | 0.9937 | 1/1 | **0.9903** |
| 41 | 🟢 classic129_alignment_combos | 1.0 | 0.996 | 1/1 | **0.9984** |
| 42 | 🟢 classic12_sparse_columns | 1.0 | 0.998 | 1/1 | **0.9992** |
| 43 | 🟢 classic130_wrap_and_indent | 1.0 | 0.9867 | 1/1 | **0.9947** |
| 44 | 🟢 classic131_number_formats | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 45 | 🟢 classic132_striped_table | 0.9984 | 0.9772 | 1/1 | **0.9902** |
| 46 | 🟢 classic133_gradient_rows | 1.0 | 0.9898 | 1/1 | **0.9959** |
| 47 | 🟢 classic134_heatmap | 1.0 | 0.9699 | 1/1 | **0.988** |
| 48 | 🟢 classic135_bottom_border_only | 1.0 | 0.9958 | 1/1 | **0.9983** |
| 49 | 🟢 classic136_financial_report_styled | 1.0 | 0.9824 | 1/1 | **0.993** |
| 50 | 🟢 classic137_checkerboard | 1.0 | 0.9593 | 1/1 | **0.9837** |
| 51 | 🟢 classic138_color_grid | 1.0 | 0.9853 | 1/1 | **0.9941** |
| 52 | 🟢 classic139_pattern_fills | 1.0 | 0.9812 | 1/1 | **0.9925** |
| 53 | 🟢 classic13_date_strings | 1.0 | 0.9967 | 1/1 | **0.9987** |
| 54 | 🟢 classic140_rotated_text | 0.9583 | 0.9942 | 1/1 | **0.981** |
| 55 | 🟢 classic141_mixed_edge_borders | 1.0 | 0.9934 | 1/1 | **0.9974** |
| 56 | 🟢 classic142_styled_invoice | 1.0 | 0.9571 | 1/1 | **0.9828** |
| 57 | 🟢 classic143_colored_tabs | 1.0 | 0.9989 | 4/4 | **0.9996** |
| 58 | 🟢 classic144_note_style_cells | 1.0 | 0.9891 | 1/1 | **0.9956** |
| 59 | 🟢 classic145_status_badges | 1.0 | 0.9762 | 1/1 | **0.9905** |
| 60 | 🟢 classic146_double_border_table | 1.0 | 0.9891 | 1/1 | **0.9956** |
| 61 | 🟢 classic147_multi_sheet_styled | 1.0 | 0.9927 | 3/3 | **0.9971** |
| 62 | 🟢 classic148_frozen_styled_grid | 1.0 | 0.9314 | 1/1 | **0.9726** |
| 63 | 🟢 classic149_merged_styled_sections | 1.0 | 0.9674 | 1/1 | **0.987** |
| 64 | 🟢 classic14_decimal_numbers | 1.0 | 0.9973 | 1/1 | **0.9989** |
| 65 | 🟢 classic150_kitchen_sink_styles | 0.9677 | 0.9686 | 1/1 | **0.9745** |
| 66 | 🟢 classic151_multilingual_greetings | 0.8485 | 0.9893 | 1/1 | **0.9351** |
| 67 | 🟢 classic152_emoji_sampler | 0.7869 | 0.9906 | 1/1 | **0.911** |
| 68 | 🟢 classic153_currency_symbols | 0.9723 | 0.9897 | 1/1 | **0.9848** |
| 69 | 🟢 classic154_math_symbols | 0.9012 | 0.9919 | 1/1 | **0.9572** |
| 70 | 🟢 classic155_diacritical_marks | 1.0 | 0.9937 | 1/1 | **0.9975** |
| 71 | 🟡 classic156_rtl_bidi_text | 0.6038 | 0.9965 | 1/1 | **0.8401** |
| 72 | 🟢 classic157_cjk_extended | 0.7957 | 0.9876 | 1/1 | **0.9133** |
| 73 | 🟢 classic158_emoji_skin_tones | 1.0 | 0.9817 | 1/1 | **0.9927** |
| 74 | 🟢 classic159_zwj_emoji | 0.7769 | 0.9893 | 1/1 | **0.9065** |
| 75 | 🟢 classic15_negative_numbers | 1.0 | 0.9976 | 1/1 | **0.999** |
| 76 | 🟢 classic160_punctuation_marks | 0.8655 | 0.995 | 1/1 | **0.9442** |
| 77 | 🟢 classic161_box_drawing | 0.8822 | 0.9929 | 1/1 | **0.95** |
| 78 | 🟡 classic162_cjk_emoji_styled | 0.6703 | 0.9913 | 1/1 | **0.8646** |
| 79 | 🟢 classic163_cyrillic_alphabets | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 80 | 🟡 classic164_indic_scripts | 0.6882 | 0.9958 | 1/1 | **0.8736** |
| 81 | 🟢 classic165_southeast_asian | 0.9127 | 0.987 | 1/1 | **0.9599** |
| 82 | 🟢 classic166_emoji_progress | 1.0 | 0.9659 | 1/1 | **0.9864** |
| 83 | 🟢 classic167_musical_symbols | 0.7551 | 0.9957 | 1/1 | **0.9003** |
| 84 | 🟢 classic168_mixed_ltr_rtl_styled | 0.8718 | 0.9908 | 1/1 | **0.945** |
| 85 | 🟡 classic169_korean_invoice | 0.7196 | 0.9907 | 1/1 | **0.8841** |
| 86 | 🟢 classic16_percentage_strings | 0.9877 | 0.9972 | 1/1 | **0.994** |
| 87 | 🟢 classic170_emoji_dashboard | 0.9216 | 0.9869 | 1/1 | **0.9634** |
| 88 | 🟢 classic171_ipa_phonetic | 0.9478 | 0.9929 | 1/1 | **0.9763** |
| 89 | 🟢 classic172_emoji_timeline | 0.8945 | 0.9884 | 1/1 | **0.9532** |
| 90 | 🟢 classic173_african_languages | 0.7804 | 0.9902 | 1/1 | **0.9082** |
| 91 | 🟢 classic174_technical_symbols | 0.8705 | 0.9889 | 1/1 | **0.9438** |
| 92 | 🟢 classic175_multiscript_catalog | 0.8296 | 0.9855 | 1/1 | **0.926** |
| 93 | 🟢 classic176_combining_characters | 0.8706 | 0.9932 | 1/1 | **0.9455** |
| 94 | 🟡 classic177_emoji_calendar | 0.72 | 0.9901 | 1/1 | **0.884** |
| 95 | 🟢 classic178_caucasus_ethiopic | 0.9313 | 0.9913 | 1/1 | **0.969** |
| 96 | 🟢 classic179_emoji_inventory | 0.7887 | 0.9854 | 1/1 | **0.9096** |
| 97 | 🟢 classic17_currency_strings | 1.0 | 0.9962 | 1/1 | **0.9985** |
| 98 | 🟢 classic180_polyglot_paragraph | 0.8468 | 0.9921 | 1/1 | **0.9356** |
| 99 | 🟢 classic18_large_dataset | 1.0 | 0.8931 | 24/24 | **0.9572** |
| 100 | 🟢 classic19_single_column_list | 1.0 | 0.9967 | 1/1 | **0.9987** |
| 101 | 🟢 classic20_all_empty_cells | 1.0 | 1.0 | 1/1 | **1.0** |
| 102 | 🟢 classic21_header_only | 1.0 | 0.9986 | 1/1 | **0.9994** |
| 103 | 🟢 classic22_long_sheet_name | 1.0 | 0.9985 | 1/1 | **0.9994** |
| 104 | 🟢 classic23_unicode_text | 0.7884 | 0.9951 | 1/1 | **0.9134** |
| 105 | 🟢 classic24_red_text | 1.0 | 0.9962 | 1/1 | **0.9985** |
| 106 | 🟢 classic25_multiple_colors | 0.9978 | 0.9962 | 1/1 | **0.9976** |
| 107 | 🟢 classic26_inline_strings | 1.0 | 0.9975 | 1/1 | **0.999** |
| 108 | 🟢 classic27_single_row | 1.0 | 0.9984 | 1/1 | **0.9994** |
| 109 | 🟢 classic28_duplicate_values | 1.0 | 0.9965 | 1/1 | **0.9986** |
| 110 | 🟢 classic29_formula_results | 1.0 | 0.9971 | 1/1 | **0.9988** |
| 111 | 🟢 classic30_mixed_empty_and_filled_sheets | 1.0 | 0.9987 | 2/2 | **0.9995** |
| 112 | 🟢 classic31_bold_header_row | 1.0 | 0.9965 | 1/1 | **0.9986** |
| 113 | 🟢 classic32_right_aligned_numbers | 1.0 | 0.9974 | 1/1 | **0.999** |
| 114 | 🟢 classic33_centered_text | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 115 | 🟢 classic34_explicit_column_widths | 1.0 | 0.9963 | 1/1 | **0.9985** |
| 116 | 🟢 classic35_explicit_row_heights | 0.9882 | 0.999 | 1/1 | **0.9949** |
| 117 | 🟢 classic36_merged_cells | 1.0 | 0.997 | 1/1 | **0.9988** |
| 118 | 🟢 classic37_freeze_panes | 1.0 | 0.9899 | 1/1 | **0.996** |
| 119 | 🟢 classic38_hyperlink_cell | 1.0 | 0.9969 | 1/1 | **0.9988** |
| 120 | 🟢 classic39_financial_table | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 121 | 🟢 classic40_scientific_notation | 1.0 | 0.9964 | 1/1 | **0.9986** |
| 122 | 🟢 classic41_integer_vs_float | 1.0 | 0.997 | 1/1 | **0.9988** |
| 123 | 🟢 classic42_boolean_values | 0.9948 | 0.9963 | 1/1 | **0.9964** |
| 124 | 🟢 classic43_inventory_report | 0.9984 | 0.9887 | 1/1 | **0.9948** |
| 125 | 🟢 classic44_employee_roster | 0.9652 | 0.9855 | 1/1 | **0.9803** |
| 126 | 🟢 classic45_sales_by_region | 1.0 | 0.9976 | 4/4 | **0.999** |
| 127 | 🟢 classic46_grade_book | 1.0 | 0.9908 | 1/1 | **0.9963** |
| 128 | 🟢 classic47_time_series | 1.0 | 0.9821 | 1/1 | **0.9928** |
| 129 | 🟢 classic48_survey_results | 0.9971 | 0.9935 | 1/1 | **0.9962** |
| 130 | 🟢 classic49_contact_list | 0.9737 | 0.9896 | 1/1 | **0.9853** |
| 131 | 🟢 classic50_budget_vs_actuals | 0.9978 | 0.9909 | 3/3 | **0.9955** |
| 132 | 🟢 classic51_product_catalog | 0.9747 | 0.9876 | 1/1 | **0.9849** |
| 133 | 🟢 classic52_pivot_summary | 0.9978 | 0.9916 | 1/1 | **0.9958** |
| 134 | 🟢 classic53_invoice | 0.9968 | 0.9912 | 1/1 | **0.9952** |
| 135 | 🟢 classic54_multi_level_header | 1.0 | 0.994 | 1/1 | **0.9976** |
| 136 | 🟢 classic55_error_values | 1.0 | 0.9945 | 1/1 | **0.9978** |
| 137 | 🟢 classic56_alternating_row_colors | 1.0 | 0.9903 | 1/1 | **0.9961** |
| 138 | 🟢 classic57_cjk_only | 0.7826 | 0.9953 | 1/1 | **0.9112** |
| 139 | 🟢 classic58_mixed_numeric_formats | 0.9905 | 0.9958 | 1/1 | **0.9945** |
| 140 | 🟢 classic59_multi_sheet_summary | 1.0 | 0.9962 | 4/4 | **0.9985** |
| 141 | 🟢 classic60_large_wide_table | 1.0 | 0.9346 | 4/4 | **0.9738** |
| 142 | 🟢 classic61_product_card_with_image | 1.0 | 0.9982 | 1/1 | **0.9993** |
| 143 | 🟢 classic62_company_logo_header | 0.996 | 0.9949 | 1/1 | **0.9964** |
| 144 | 🟢 classic63_two_products_side_by_side | 1.0 | 0.9939 | 1/1 | **0.9976** |
| 145 | 🟢 classic64_employee_directory_with_photo | 0.9835 | 0.9938 | 1/1 | **0.9909** |
| 146 | 🟢 classic65_inventory_with_product_photos | 0.9906 | 0.9949 | 1/1 | **0.9942** |
| 147 | 🟢 classic66_invoice_with_logo | 0.9836 | 0.9956 | 1/1 | **0.9917** |
| 148 | 🟢 classic67_real_estate_listing | 1.0 | 0.9944 | 1/1 | **0.9978** |
| 149 | 🟢 classic68_restaurant_menu | 0.9881 | 0.9776 | 1/1 | **0.9863** |
| 150 | 🟢 classic69_image_only_sheet | 1.0 | 1.0 | 1/1 | **1.0** |
| 151 | 🟢 classic70_product_catalog_with_images | 0.9862 | 0.9934 | 1/1 | **0.9918** |
| 152 | 🟢 classic71_multi_sheet_with_images | 0.9966 | 0.999 | 3/3 | **0.9982** |
| 153 | 🟢 classic72_bar_chart_image_with_data | 1.0 | 0.9856 | 1/1 | **0.9942** |
| 154 | 🟢 classic73_event_flyer_with_banner | 0.9939 | 0.9936 | 1/1 | **0.995** |
| 155 | 🟢 classic74_dashboard_with_kpi_image | 0.9595 | 0.9874 | 1/1 | **0.9788** |
| 156 | 🟢 classic75_certificate_with_seal | 1.0 | 0.9867 | 1/1 | **0.9947** |
| 157 | 🟢 classic76_product_image_grid | 1.0 | 0.9897 | 1/1 | **0.9959** |
| 158 | 🟢 classic77_news_article_with_hero_image | 1.0 | 0.9911 | 1/1 | **0.9964** |
| 159 | 🟢 classic78_small_icon_per_row | 0.9799 | 0.9953 | 1/1 | **0.9901** |
| 160 | 🟢 classic79_wide_panoramic_banner | 1.0 | 0.9946 | 1/1 | **0.9978** |
| 161 | 🟢 classic80_portrait_tall_image | 1.0 | 0.9948 | 1/1 | **0.9979** |
| 162 | 🟢 classic81_step_by_step_with_images | 1.0 | 0.9921 | 1/1 | **0.9968** |
| 163 | 🟢 classic82_before_after_images | 0.9926 | 0.9906 | 1/1 | **0.9933** |
| 164 | 🟢 classic83_color_swatch_palette | 0.9863 | 0.9931 | 1/1 | **0.9918** |
| 165 | 🟢 classic84_travel_destination_cards | 1.0 | 0.9897 | 1/1 | **0.9959** |
| 166 | 🟢 classic85_lab_results_with_image | 0.9911 | 0.9877 | 1/1 | **0.9915** |
| 167 | 🟢 classic86_software_screenshot_features | 0.973 | 0.9948 | 1/1 | **0.9871** |
| 168 | 🟢 classic87_sports_results_with_logos | 1.0 | 0.9948 | 1/1 | **0.9979** |
| 169 | 🟢 classic88_image_after_data | 0.997 | 0.9948 | 1/1 | **0.9967** |
| 170 | 🟢 classic89_nutrition_label_with_image | 0.9903 | 0.995 | 1/1 | **0.9941** |
| 171 | 🟢 classic90_project_status_with_milestones | 0.9572 | 0.9852 | 1/1 | **0.977** |
| 172 | 🟢 classic91_simple_bar_chart | 0.9493 | 0.9607 | 2/2 | **0.964** |
| 173 | 🟢 classic92_horizontal_bar_chart | 0.9563 | 0.9665 | 2/2 | **0.9691** |
| 174 | 🟢 classic93_line_chart | 0.8257 | 0.9861 | 2/2 | **0.9247** |
| 175 | 🟢 classic94_pie_chart | 0.878 | 0.9249 | 2/2 | **0.9212** |
| 176 | 🟡 classic95_area_chart | 0.6441 | 0.7651 | 2/2 | **0.7637** |
| 177 | 🟢 classic96_scatter_chart | 0.8714 | 0.9855 | 2/2 | **0.9428** |
| 178 | 🟢 classic97_doughnut_chart | 0.8571 | 0.9317 | 2/2 | **0.9155** |
| 179 | 🟢 classic98_radar_chart | 0.8876 | 0.9892 | 2/2 | **0.9507** |
| 180 | 🟢 classic99_bubble_chart | 0.8447 | 0.966 | 2/2 | **0.9243** |

**Average Overall Score: 0.9652**

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
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
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
      <td rowspan="5" valign="top"><span style="color:#3fb950">⬤</span> 0.9779</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9992</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9495</td>
    </tr>
    <tr>
      <td valign="top"><b>classic101_percent_stacked_bar</b></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic101_percent_stacked_bar_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9362</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic102_line_chart_with_markers</b><br><small>p1</small></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic102_line_chart_with_markers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9302</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8843</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8164</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8583</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9167</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8056</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9441</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic109_scatter_with_trendline</b><br><small>p1</small></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic109_scatter_with_trendline_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.923</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic110_chart_with_legend</b><br><small>p1</small></td>
      <td><img src="images/classic110_chart_with_legend_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic110_chart_with_legend_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8466</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9213</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8538</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8639</td>
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
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9156</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9213</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.938</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic117_stock_ohlc_chart</b><br><small>p1</small></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic117_stock_ohlc_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.8088</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.955</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.91</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7611</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9906</td>
    </tr>
    <tr>
      <td valign="top"><b>classic124_colored_borders</b></td>
      <td><img src="images/classic124_colored_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic124_colored_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9972</td>
    </tr>
    <tr>
      <td valign="top"><b>classic125_solid_fills</b></td>
      <td><img src="images/classic125_solid_fills_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic125_solid_fills_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9918</td>
    </tr>
    <tr>
      <td valign="top"><b>classic126_dark_header</b></td>
      <td><img src="images/classic126_dark_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic126_dark_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9947</td>
    </tr>
    <tr>
      <td valign="top"><b>classic127_font_styles</b></td>
      <td><img src="images/classic127_font_styles_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic127_font_styles_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9955</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9984</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic132_striped_table</b></td>
      <td><img src="images/classic132_striped_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic132_striped_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9902</td>
    </tr>
    <tr>
      <td valign="top"><b>classic133_gradient_rows</b></td>
      <td><img src="images/classic133_gradient_rows_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic133_gradient_rows_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9959</td>
    </tr>
    <tr>
      <td valign="top"><b>classic134_heatmap</b></td>
      <td><img src="images/classic134_heatmap_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic134_heatmap_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic135_bottom_border_only</b></td>
      <td><img src="images/classic135_bottom_border_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic135_bottom_border_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9983</td>
    </tr>
    <tr>
      <td valign="top"><b>classic136_financial_report_styled</b></td>
      <td><img src="images/classic136_financial_report_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic136_financial_report_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic137_checkerboard</b></td>
      <td><img src="images/classic137_checkerboard_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic137_checkerboard_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9837</td>
    </tr>
    <tr>
      <td valign="top"><b>classic138_color_grid</b></td>
      <td><img src="images/classic138_color_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic138_color_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9941</td>
    </tr>
    <tr>
      <td valign="top"><b>classic139_pattern_fills</b></td>
      <td><img src="images/classic139_pattern_fills_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic139_pattern_fills_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9925</td>
    </tr>
    <tr>
      <td valign="top"><b>classic13_date_strings</b></td>
      <td><img src="images/classic13_date_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic13_date_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
    </tr>
    <tr>
      <td valign="top"><b>classic140_rotated_text</b></td>
      <td><img src="images/classic140_rotated_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic140_rotated_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.981</td>
    </tr>
    <tr>
      <td valign="top"><b>classic141_mixed_edge_borders</b></td>
      <td><img src="images/classic141_mixed_edge_borders_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic141_mixed_edge_borders_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9974</td>
    </tr>
    <tr>
      <td valign="top"><b>classic142_styled_invoice</b></td>
      <td><img src="images/classic142_styled_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic142_styled_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9828</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic143_colored_tabs</b><br><small>p1</small></td>
      <td><img src="images/classic143_colored_tabs_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic143_colored_tabs_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9996</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td valign="top"><b>classic145_status_badges</b></td>
      <td><img src="images/classic145_status_badges_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic145_status_badges_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9905</td>
    </tr>
    <tr>
      <td valign="top"><b>classic146_double_border_table</b></td>
      <td><img src="images/classic146_double_border_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic146_double_border_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9956</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic147_multi_sheet_styled</b><br><small>p1</small></td>
      <td><img src="images/classic147_multi_sheet_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic147_multi_sheet_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9971</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9726</td>
    </tr>
    <tr>
      <td valign="top"><b>classic149_merged_styled_sections</b></td>
      <td><img src="images/classic149_merged_styled_sections_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic149_merged_styled_sections_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.987</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9745</td>
    </tr>
    <tr>
      <td valign="top"><b>classic151_multilingual_greetings</b></td>
      <td><img src="images/classic151_multilingual_greetings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic151_multilingual_greetings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9351</td>
    </tr>
    <tr>
      <td valign="top"><b>classic152_emoji_sampler</b></td>
      <td><img src="images/classic152_emoji_sampler_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic152_emoji_sampler_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.911</td>
    </tr>
    <tr>
      <td valign="top"><b>classic153_currency_symbols</b></td>
      <td><img src="images/classic153_currency_symbols_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic153_currency_symbols_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9848</td>
    </tr>
    <tr>
      <td valign="top"><b>classic154_math_symbols</b></td>
      <td><img src="images/classic154_math_symbols_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic154_math_symbols_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9572</td>
    </tr>
    <tr>
      <td valign="top"><b>classic155_diacritical_marks</b></td>
      <td><img src="images/classic155_diacritical_marks_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic155_diacritical_marks_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9975</td>
    </tr>
    <tr>
      <td valign="top"><b>classic156_rtl_bidi_text</b></td>
      <td><img src="images/classic156_rtl_bidi_text_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic156_rtl_bidi_text_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8401</td>
    </tr>
    <tr>
      <td valign="top"><b>classic157_cjk_extended</b></td>
      <td><img src="images/classic157_cjk_extended_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic157_cjk_extended_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9133</td>
    </tr>
    <tr>
      <td valign="top"><b>classic158_emoji_skin_tones</b></td>
      <td><img src="images/classic158_emoji_skin_tones_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic158_emoji_skin_tones_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9927</td>
    </tr>
    <tr>
      <td valign="top"><b>classic159_zwj_emoji</b></td>
      <td><img src="images/classic159_zwj_emoji_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic159_zwj_emoji_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9065</td>
    </tr>
    <tr>
      <td valign="top"><b>classic15_negative_numbers</b></td>
      <td><img src="images/classic15_negative_numbers_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic15_negative_numbers_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.999</td>
    </tr>
    <tr>
      <td valign="top"><b>classic160_punctuation_marks</b></td>
      <td><img src="images/classic160_punctuation_marks_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic160_punctuation_marks_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9442</td>
    </tr>
    <tr>
      <td valign="top"><b>classic161_box_drawing</b></td>
      <td><img src="images/classic161_box_drawing_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic161_box_drawing_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.95</td>
    </tr>
    <tr>
      <td valign="top"><b>classic162_cjk_emoji_styled</b></td>
      <td><img src="images/classic162_cjk_emoji_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic162_cjk_emoji_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8646</td>
    </tr>
    <tr>
      <td valign="top"><b>classic163_cyrillic_alphabets</b></td>
      <td><img src="images/classic163_cyrillic_alphabets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic163_cyrillic_alphabets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic164_indic_scripts</b></td>
      <td><img src="images/classic164_indic_scripts_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic164_indic_scripts_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8736</td>
    </tr>
    <tr>
      <td valign="top"><b>classic165_southeast_asian</b></td>
      <td><img src="images/classic165_southeast_asian_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic165_southeast_asian_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9599</td>
    </tr>
    <tr>
      <td valign="top"><b>classic166_emoji_progress</b></td>
      <td><img src="images/classic166_emoji_progress_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic166_emoji_progress_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9864</td>
    </tr>
    <tr>
      <td valign="top"><b>classic167_musical_symbols</b></td>
      <td><img src="images/classic167_musical_symbols_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic167_musical_symbols_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9003</td>
    </tr>
    <tr>
      <td valign="top"><b>classic168_mixed_ltr_rtl_styled</b></td>
      <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic168_mixed_ltr_rtl_styled_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.945</td>
    </tr>
    <tr>
      <td valign="top"><b>classic169_korean_invoice</b></td>
      <td><img src="images/classic169_korean_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic169_korean_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.8841</td>
    </tr>
    <tr>
      <td valign="top"><b>classic16_percentage_strings</b></td>
      <td><img src="images/classic16_percentage_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic16_percentage_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.994</td>
    </tr>
    <tr>
      <td valign="top"><b>classic170_emoji_dashboard</b></td>
      <td><img src="images/classic170_emoji_dashboard_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic170_emoji_dashboard_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9634</td>
    </tr>
    <tr>
      <td valign="top"><b>classic171_ipa_phonetic</b></td>
      <td><img src="images/classic171_ipa_phonetic_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic171_ipa_phonetic_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9763</td>
    </tr>
    <tr>
      <td valign="top"><b>classic172_emoji_timeline</b></td>
      <td><img src="images/classic172_emoji_timeline_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic172_emoji_timeline_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9532</td>
    </tr>
    <tr>
      <td valign="top"><b>classic173_african_languages</b></td>
      <td><img src="images/classic173_african_languages_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic173_african_languages_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9082</td>
    </tr>
    <tr>
      <td valign="top"><b>classic174_technical_symbols</b></td>
      <td><img src="images/classic174_technical_symbols_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic174_technical_symbols_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9438</td>
    </tr>
    <tr>
      <td valign="top"><b>classic175_multiscript_catalog</b></td>
      <td><img src="images/classic175_multiscript_catalog_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic175_multiscript_catalog_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.926</td>
    </tr>
    <tr>
      <td valign="top"><b>classic176_combining_characters</b></td>
      <td><img src="images/classic176_combining_characters_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic176_combining_characters_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9455</td>
    </tr>
    <tr>
      <td valign="top"><b>classic177_emoji_calendar</b></td>
      <td><img src="images/classic177_emoji_calendar_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic177_emoji_calendar_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#d29922">⬤</span> 0.884</td>
    </tr>
    <tr>
      <td valign="top"><b>classic178_caucasus_ethiopic</b></td>
      <td><img src="images/classic178_caucasus_ethiopic_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic178_caucasus_ethiopic_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.969</td>
    </tr>
    <tr>
      <td valign="top"><b>classic179_emoji_inventory</b></td>
      <td><img src="images/classic179_emoji_inventory_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic179_emoji_inventory_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9096</td>
    </tr>
    <tr>
      <td valign="top"><b>classic17_currency_strings</b></td>
      <td><img src="images/classic17_currency_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic17_currency_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
    </tr>
    <tr>
      <td valign="top"><b>classic180_polyglot_paragraph</b></td>
      <td><img src="images/classic180_polyglot_paragraph_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic180_polyglot_paragraph_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9356</td>
    </tr>
    <tr>
      <td rowspan="24" valign="top"><b>classic18_large_dataset</b><br><small>p1</small></td>
      <td><img src="images/classic18_large_dataset_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic18_large_dataset_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="24" valign="top"><span style="color:#3fb950">⬤</span> 0.9572</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9987</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9994</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9134</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic26_inline_strings</b></td>
      <td><img src="images/classic26_inline_strings_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic26_inline_strings_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.999</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
    </tr>
    <tr>
      <td valign="top"><b>classic29_formula_results</b></td>
      <td><img src="images/classic29_formula_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic29_formula_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic30_mixed_empty_and_filled_sheets</b><br><small>p1</small></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic30_mixed_empty_and_filled_sheets_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9995</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
    </tr>
    <tr>
      <td valign="top"><b>classic35_explicit_row_heights</b></td>
      <td><img src="images/classic35_explicit_row_heights_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic35_explicit_row_heights_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9949</td>
    </tr>
    <tr>
      <td valign="top"><b>classic36_merged_cells</b></td>
      <td><img src="images/classic36_merged_cells_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic36_merged_cells_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9988</td>
    </tr>
    <tr>
      <td valign="top"><b>classic39_financial_table</b></td>
      <td><img src="images/classic39_financial_table_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic39_financial_table_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic40_scientific_notation</b></td>
      <td><img src="images/classic40_scientific_notation_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic40_scientific_notation_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9986</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic43_inventory_report</b></td>
      <td><img src="images/classic43_inventory_report_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic43_inventory_report_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9948</td>
    </tr>
    <tr>
      <td valign="top"><b>classic44_employee_roster</b></td>
      <td><img src="images/classic44_employee_roster_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic44_employee_roster_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9803</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9963</td>
    </tr>
    <tr>
      <td valign="top"><b>classic47_time_series</b></td>
      <td><img src="images/classic47_time_series_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic47_time_series_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9928</td>
    </tr>
    <tr>
      <td valign="top"><b>classic48_survey_results</b></td>
      <td><img src="images/classic48_survey_results_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic48_survey_results_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9962</td>
    </tr>
    <tr>
      <td valign="top"><b>classic49_contact_list</b></td>
      <td><img src="images/classic49_contact_list_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic49_contact_list_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9853</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic50_budget_vs_actuals</b><br><small>p1</small></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic50_budget_vs_actuals_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9955</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9849</td>
    </tr>
    <tr>
      <td valign="top"><b>classic52_pivot_summary</b></td>
      <td><img src="images/classic52_pivot_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic52_pivot_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9958</td>
    </tr>
    <tr>
      <td valign="top"><b>classic53_invoice</b></td>
      <td><img src="images/classic53_invoice_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic53_invoice_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9952</td>
    </tr>
    <tr>
      <td valign="top"><b>classic54_multi_level_header</b></td>
      <td><img src="images/classic54_multi_level_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic54_multi_level_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9961</td>
    </tr>
    <tr>
      <td valign="top"><b>classic57_cjk_only</b></td>
      <td><img src="images/classic57_cjk_only_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic57_cjk_only_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9112</td>
    </tr>
    <tr>
      <td valign="top"><b>classic58_mixed_numeric_formats</b></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic58_mixed_numeric_formats_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9945</td>
    </tr>
    <tr>
      <td rowspan="4" valign="top"><b>classic59_multi_sheet_summary</b><br><small>p1</small></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic59_multi_sheet_summary_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9985</td>
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
      <td rowspan="4" valign="top"><span style="color:#3fb950">⬤</span> 0.9738</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9993</td>
    </tr>
    <tr>
      <td valign="top"><b>classic62_company_logo_header</b></td>
      <td><img src="images/classic62_company_logo_header_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic62_company_logo_header_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic63_two_products_side_by_side</b></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic63_two_products_side_by_side_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9976</td>
    </tr>
    <tr>
      <td valign="top"><b>classic64_employee_directory_with_photo</b></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic64_employee_directory_with_photo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9909</td>
    </tr>
    <tr>
      <td valign="top"><b>classic65_inventory_with_product_photos</b></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic65_inventory_with_product_photos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic66_invoice_with_logo</b></td>
      <td><img src="images/classic66_invoice_with_logo_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic66_invoice_with_logo_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9917</td>
    </tr>
    <tr>
      <td valign="top"><b>classic67_real_estate_listing</b></td>
      <td><img src="images/classic67_real_estate_listing_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic67_real_estate_listing_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 1.0</td>
    </tr>
    <tr>
      <td valign="top"><b>classic70_product_catalog_with_images</b></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic70_product_catalog_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9918</td>
    </tr>
    <tr>
      <td rowspan="3" valign="top"><b>classic71_multi_sheet_with_images</b><br><small>p1</small></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic71_multi_sheet_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="3" valign="top"><span style="color:#3fb950">⬤</span> 0.9982</td>
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
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9942</td>
    </tr>
    <tr>
      <td valign="top"><b>classic73_event_flyer_with_banner</b></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic73_event_flyer_with_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.995</td>
    </tr>
    <tr>
      <td valign="top"><b>classic74_dashboard_with_kpi_image</b></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic74_dashboard_with_kpi_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9788</td>
    </tr>
    <tr>
      <td valign="top"><b>classic75_certificate_with_seal</b></td>
      <td><img src="images/classic75_certificate_with_seal_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic75_certificate_with_seal_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9947</td>
    </tr>
    <tr>
      <td valign="top"><b>classic76_product_image_grid</b></td>
      <td><img src="images/classic76_product_image_grid_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic76_product_image_grid_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9959</td>
    </tr>
    <tr>
      <td valign="top"><b>classic77_news_article_with_hero_image</b></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic77_news_article_with_hero_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9964</td>
    </tr>
    <tr>
      <td valign="top"><b>classic78_small_icon_per_row</b></td>
      <td><img src="images/classic78_small_icon_per_row_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic78_small_icon_per_row_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9901</td>
    </tr>
    <tr>
      <td valign="top"><b>classic79_wide_panoramic_banner</b></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic79_wide_panoramic_banner_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9978</td>
    </tr>
    <tr>
      <td valign="top"><b>classic80_portrait_tall_image</b></td>
      <td><img src="images/classic80_portrait_tall_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic80_portrait_tall_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9979</td>
    </tr>
    <tr>
      <td valign="top"><b>classic81_step_by_step_with_images</b></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic81_step_by_step_with_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9968</td>
    </tr>
    <tr>
      <td valign="top"><b>classic82_before_after_images</b></td>
      <td><img src="images/classic82_before_after_images_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic82_before_after_images_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9933</td>
    </tr>
    <tr>
      <td valign="top"><b>classic83_color_swatch_palette</b></td>
      <td><img src="images/classic83_color_swatch_palette_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic83_color_swatch_palette_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9918</td>
    </tr>
    <tr>
      <td valign="top"><b>classic84_travel_destination_cards</b></td>
      <td><img src="images/classic84_travel_destination_cards_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic84_travel_destination_cards_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9959</td>
    </tr>
    <tr>
      <td valign="top"><b>classic85_lab_results_with_image</b></td>
      <td><img src="images/classic85_lab_results_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic85_lab_results_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9915</td>
    </tr>
    <tr>
      <td valign="top"><b>classic86_software_screenshot_features</b></td>
      <td><img src="images/classic86_software_screenshot_features_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic86_software_screenshot_features_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9871</td>
    </tr>
    <tr>
      <td valign="top"><b>classic87_sports_results_with_logos</b></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic87_sports_results_with_logos_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9979</td>
    </tr>
    <tr>
      <td valign="top"><b>classic88_image_after_data</b></td>
      <td><img src="images/classic88_image_after_data_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic88_image_after_data_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9967</td>
    </tr>
    <tr>
      <td valign="top"><b>classic89_nutrition_label_with_image</b></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic89_nutrition_label_with_image_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.9941</td>
    </tr>
    <tr>
      <td valign="top"><b>classic90_project_status_with_milestones</b></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic90_project_status_with_milestones_p1_reference.png" width="340" alt="Reference p1"></td>
      <td valign="top"><span style="color:#3fb950">⬤</span> 0.977</td>
    </tr>
    <tr>
      <td rowspan="2" valign="top"><b>classic91_simple_bar_chart</b><br><small>p1</small></td>
      <td><img src="images/classic91_simple_bar_chart_p1_minipdf.png" width="340" alt="MiniPdf p1"></td>
      <td><img src="images/classic91_simple_bar_chart_p1_reference.png" width="340" alt="Reference p1"></td>
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.964</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9691</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9212</td>
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
      <td rowspan="2" valign="top"><span style="color:#d29922">⬤</span> 0.7637</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9428</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9155</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9507</td>
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
      <td rowspan="2" valign="top"><span style="color:#3fb950">⬤</span> 0.9243</td>
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
- **Visual Average:** 0.9969
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1366 bytes, Reference=30311 bytes

Text content: ✅ Identical

### classic02_multiple_worksheets

- **Text Similarity:** 0.9971
- **Visual Average:** 0.998
- **Overall Score:** 0.998
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=2397 bytes, Reference=36003 bytes

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
- **Visual Average:** 0.9938
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=9056 bytes, Reference=62308 bytes

Text content: ✅ Identical

### classic06_tall_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9447
- **Overall Score:** 0.9779
- **Pages:** MiniPdf=5, Reference=5
- **File Size:** MiniPdf=40903 bytes, Reference=185703 bytes

Text content: ✅ Identical

### classic07_numbers_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.999
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1203 bytes, Reference=24806 bytes

Text content: ✅ Identical

### classic08_mixed_text_and_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9979
- **Overall Score:** 0.9992
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1199 bytes, Reference=27336 bytes

Text content: ✅ Identical

### classic09_long_text

- **Text Similarity:** 0.6785
- **Visual Average:** 0.577
- **Overall Score:** 0.6022
- **Pages:** MiniPdf=7, Reference=12
- **File Size:** MiniPdf=2432 bytes, Reference=29170 bytes

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
- **Visual Average:** 0.9075
- **Overall Score:** 0.9495
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4582 bytes, Reference=47565 bytes

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
- **Visual Average:** 0.8783
- **Overall Score:** 0.9362
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5285 bytes, Reference=49462 bytes

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
- **Visual Average:** 0.9884
- **Overall Score:** 0.9302
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

- **Text Similarity:** 0.7368
- **Visual Average:** 0.9739
- **Overall Score:** 0.8843
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403985 bytes, Reference=48488 bytes

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
- **Visual Average:** 0.7538
- **Overall Score:** 0.8164
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4479 bytes, Reference=54330 bytes

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
- **Visual Average:** 0.7426
- **Overall Score:** 0.8583
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3908 bytes, Reference=138437 bytes

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
- **Visual Average:** 0.9675
- **Overall Score:** 0.9167
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403768 bytes, Reference=76353 bytes

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
- **Visual Average:** 0.7766
- **Overall Score:** 0.8056
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=15253 bytes, Reference=82303 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic107_multi_series_line.pdf
+++ reference/classic107_multi_series_line.pdf
@@ -1,33 +1,41 @@
 Day AAPL GOOG MSFT

-Day 1 178.48 140.49 402.83 Stock Price Trend (20 Day

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

-Day 1Day 2Day 3Day 4Day 5Day 6Day 7Day 8Day 9Day 10Day 11Day 12D

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
- **Visual Average:** 0.896
- **Overall Score:** 0.9441
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=10940 bytes, Reference=51253 bytes

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

- **Text Similarity:** 0.8226
- **Visual Average:** 0.985
- **Overall Score:** 0.923
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5365 bytes, Reference=60738 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic109_scatter_with_trendline.pdf
+++ reference/classic109_scatter_with_trendline.pdf
@@ -1,18 +1,21 @@
-Study Hour Exam Score

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
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=989 bytes, Reference=27644 bytes

Text content: ✅ Identical

### classic110_chart_with_legend

- **Text Similarity:** 0.8372
- **Visual Average:** 0.7792
- **Overall Score:** 0.8466
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
- **Visual Average:** 0.9765
- **Overall Score:** 0.9213
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3418 bytes, Reference=51007 bytes

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
- **Visual Average:** 0.7595
- **Overall Score:** 0.8538
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6777 bytes, Reference=59342 bytes

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
- **Visual Average:** 0.7339
- **Overall Score:** 0.8639
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
- **Visual Average:** 0.8874
- **Overall Score:** 0.9156
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=30697 bytes, Reference=128765 bytes

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
- **Visual Average:** 0.9717
- **Overall Score:** 0.9213
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4194 bytes, Reference=51633 bytes

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
- **Visual Average:** 0.8802
- **Overall Score:** 0.938
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11188 bytes, Reference=50765 bytes

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

- **Text Similarity:** 0.7938
- **Visual Average:** 0.7282
- **Overall Score:** 0.8088
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=8339 bytes, Reference=62401 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic117_stock_ohlc_chart.pdf
+++ reference/classic117_stock_ohlc_chart.pdf
@@ -1,23 +1,27 @@
 Day Open High Low Close

-Day 1 148.96 149.78 146.96 147.41 Stock OHLC (1

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

- **Text Similarity:** 0.9275
- **Visual Average:** 0.9601
- **Overall Score:** 0.955
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3538 bytes, Reference=48780 bytes

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
- **Visual Average:** 0.9341
- **Overall Score:** 0.91
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=218257 bytes, Reference=65175 bytes

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
- **Visual Average:** 0.9995
- **Overall Score:** 0.9998
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1052 bytes, Reference=23538 bytes

Text content: ✅ Identical

### classic120_chart_with_date_axis

- **Text Similarity:** 0.6195
- **Visual Average:** 0.7833
- **Overall Score:** 0.7611
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5955 bytes, Reference=56955 bytes

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

-2025-01-012025-01-312025-03-022025-04-012025-05-012025-05-312025-06-302025-07-302025-08-292025-09

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
- **Visual Average:** 0.9934
- **Overall Score:** 0.9974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8257 bytes, Reference=39925 bytes

Text content: ✅ Identical

### classic122_thick_outer_thin_inner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8249 bytes, Reference=40404 bytes

Text content: ✅ Identical

### classic123_dashed_borders

- **Text Similarity:** 0.9802
- **Visual Average:** 0.9964
- **Overall Score:** 0.9906
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2626 bytes, Reference=35187 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic123_dashed_borders.pdf
+++ reference/classic123_dashed_borders.pdf
@@ -1,6 +1,6 @@
-Border Styl Sample

+Border StSample

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
- **Visual Average:** 0.9929
- **Overall Score:** 0.9972
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3392 bytes, Reference=38667 bytes

Text content: ✅ Identical

### classic125_solid_fills

- **Text Similarity:** 0.9869
- **Visual Average:** 0.9927
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2099 bytes, Reference=39001 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic125_solid_fills.pdf
+++ reference/classic125_solid_fills.pdf
@@ -1,9 +1,9 @@
-Fill Name Filled Cell

+Fill NameFilled Cell

 Light Blue Background

-Light Gree Background

-Light Yello Background

+Light GreeBackground

+Light YelloBackground

 Light Red Background

-Light Purpl Background

-Light Oran Background

+Light PurplBackground

+Light OranBackground

 Gray 25% Background

 Sky Blue Background
```
</details>

### classic126_dark_header

- **Text Similarity:** 0.9953
- **Visual Average:** 0.9915
- **Overall Score:** 0.9947
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2297 bytes, Reference=44287 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic126_dark_header.pdf
+++ reference/classic126_dark_header.pdf
@@ -1,5 +1,5 @@
-Employee Departmen Salary Start Date

-Alice Smith Engineerin 95000 2020-03-15

+EmployeeDepartmen Salary Start Date

+Alice SmithEngineerin 95000 2020-03-15

 Bob Jones Marketing 72000 2019-07-01

 Carol Lee Finance 88000 2021-01-10

 David Kim Engineerin 102000 2018-11-20

```
</details>

### classic127_font_styles

- **Text Similarity:** 0.9964
- **Visual Average:** 0.9924
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1739 bytes, Reference=72555 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic127_font_styles.pdf
+++ reference/classic127_font_styles.pdf
@@ -2,8 +2,8 @@
 Bold Sample Bold text

 Italic Sample Italic text

 Underline Sample Underline text

-Strikethrou Sample Strikethrough text

+Strikethro Sample Strikethrough text

 Bold Italic Sample Bold Italic text

-Bold Under Sample Bold Underline text

+Bold Unde Sample Bold Underline text

 Double Un Sample Double Underline text

 Bold + Red Sample Bold + Red text
```
</details>

### classic128_font_sizes

- **Text Similarity:** 0.982
- **Visual Average:** 0.9937
- **Overall Score:** 0.9903
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1818 bytes, Reference=48278 bytes

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
- **Visual Average:** 0.996
- **Overall Score:** 0.9984
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1434 bytes, Reference=35431 bytes

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
- **Visual Average:** 0.9867
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
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2416 bytes, Reference=43396 bytes

Text content: ✅ Identical

### classic132_striped_table

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9772
- **Overall Score:** 0.9902
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=16149 bytes, Reference=47692 bytes

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
- **Visual Average:** 0.9898
- **Overall Score:** 0.9959
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4232 bytes, Reference=54544 bytes

Text content: ✅ Identical

### classic134_heatmap

- **Text Similarity:** 1.0
- **Visual Average:** 0.9699
- **Overall Score:** 0.988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6134 bytes, Reference=44182 bytes

Text content: ✅ Identical

### classic135_bottom_border_only

- **Text Similarity:** 1.0
- **Visual Average:** 0.9958
- **Overall Score:** 0.9983
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1341 bytes, Reference=32996 bytes

Text content: ✅ Identical

### classic136_financial_report_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9824
- **Overall Score:** 0.993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=8727 bytes, Reference=46675 bytes

Text content: ✅ Identical

### classic137_checkerboard

- **Text Similarity:** 1.0
- **Visual Average:** 0.9593
- **Overall Score:** 0.9837
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7628 bytes, Reference=42995 bytes

Text content: ✅ Identical

### classic138_color_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9853
- **Overall Score:** 0.9941
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1924 bytes, Reference=26461 bytes

Text content: ✅ Identical

### classic139_pattern_fills

- **Text Similarity:** 1.0
- **Visual Average:** 0.9812
- **Overall Score:** 0.9925
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2664 bytes, Reference=43091 bytes

Text content: ✅ Identical

### classic13_date_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1270 bytes, Reference=29104 bytes

Text content: ✅ Identical

### classic140_rotated_text

- **Text Similarity:** 0.9583
- **Visual Average:** 0.9942
- **Overall Score:** 0.981
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1823 bytes, Reference=39253 bytes

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
- **Visual Average:** 0.9934
- **Overall Score:** 0.9974
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2261 bytes, Reference=36300 bytes

Text content: ✅ Identical

### classic142_styled_invoice

- **Text Similarity:** 1.0
- **Visual Average:** 0.9571
- **Overall Score:** 0.9828
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=10373 bytes, Reference=52625 bytes

Text content: ✅ Identical

### classic143_colored_tabs

- **Text Similarity:** 1.0
- **Visual Average:** 0.9989
- **Overall Score:** 0.9996
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=2543 bytes, Reference=43510 bytes

Text content: ✅ Identical

### classic144_note_style_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.9891
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2972 bytes, Reference=48027 bytes

Text content: ✅ Identical

### classic145_status_badges

- **Text Similarity:** 1.0
- **Visual Average:** 0.9762
- **Overall Score:** 0.9905
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11663 bytes, Reference=60432 bytes

Text content: ✅ Identical

### classic146_double_border_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9891
- **Overall Score:** 0.9956
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7192 bytes, Reference=41798 bytes

Text content: ✅ Identical

### classic147_multi_sheet_styled

- **Text Similarity:** 1.0
- **Visual Average:** 0.9927
- **Overall Score:** 0.9971
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=16027 bytes, Reference=54487 bytes

Text content: ✅ Identical

### classic148_frozen_styled_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9314
- **Overall Score:** 0.9726
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=45160 bytes, Reference=67067 bytes

Text content: ✅ Identical

### classic149_merged_styled_sections

- **Text Similarity:** 1.0
- **Visual Average:** 0.9674
- **Overall Score:** 0.987
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=11296 bytes, Reference=48481 bytes

Text content: ✅ Identical

### classic14_decimal_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9973
- **Overall Score:** 0.9989
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1218 bytes, Reference=29057 bytes

Text content: ✅ Identical

### classic150_kitchen_sink_styles

- **Text Similarity:** 0.9677
- **Visual Average:** 0.9686
- **Overall Score:** 0.9745
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

### classic151_multilingual_greetings

- **Text Similarity:** 0.8485
- **Visual Average:** 0.9893
- **Overall Score:** 0.9351
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4350454 bytes, Reference=103044 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic151_multilingual_greetings.pdf
+++ reference/classic151_multilingual_greetings.pdf
@@ -1,24 +1,16 @@
 Language Hello Thank you

 English Hello Thank you

+你好 谢谢

 Chinese

-你好 谢谢

+こんにちは ありがとう

 Japanese

-こんにちは ありがとう

-Korean

-안녕하세요 감사합니다

-Thai

-สวัสดี ขอบคุณ

+Korean 안녕하세요 감사합니다

+Thai สวัสดี ขอบคุณ

+नमस्ते धन्यवाद

 Hindi

-नमस्ते धन्यवाद

-Arabic

-ﺎﺒﺣﺮﻣ ﺍﺮﻜﺷ

-Hebrew

-םולש הדות

-Greek

-Γεια σου Ευχαριστώ

-Russian

-Привет Спасибо

-Vietnamese Xin chào

-C ả m ơ n

-Turkish Merhaba

-Teşekkürler
+Arabicمرحبا شكرا

+Hebrewשלום תודה

+Greek Γεια σου Ευχαριστώ

+Russian Привет Спасибо

+Vietnamese Xin chào Cảm ơn

+Turkish Merhaba Teşekkürler
```
</details>

### classic152_emoji_sampler

- **Text Similarity:** 0.7869
- **Visual Average:** 0.9906
- **Overall Score:** 0.911
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=438647 bytes, Reference=69423 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic152_emoji_sampler.pdf
+++ reference/classic152_emoji_sampler.pdf
@@ -1,17 +1,11 @@
 Category Emoji

-Faces

-😀😃😄😁😆

-Hearts

-❤ ️ 🧡💛💚💙

-Animals

-🐶🐱🐭🐹🐰

-Food

-🍎🍐🍊🍋🍌

-Travel

-✈ ️ 🚗🚌🚂🚀

-Sports

-⚽🏀🏈⚾🎾

-Symbols

-✅❌⚠ ️ 🔴🟢

-Hands

-👍👎👏🤝✌ ️
+Faces 😀😃😄😁😆

+Hearts ❤️

+Animals 🐶🐱🐭🐹🐰

+Food 🍎🍐🍊🍋🍌

+Travel 

+⚽ ⚾

+Sports 🏀🏈 🎾

+⚠️ 

+Symbols  🔴🟢

+Hands  ✌️ 
```
</details>

### classic153_currency_symbols

- **Text Similarity:** 0.9723
- **Visual Average:** 0.9897
- **Overall Score:** 0.9848
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4128579 bytes, Reference=41642 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic153_currency_symbols.pdf
+++ reference/classic153_currency_symbols.pdf
@@ -4,16 +4,10 @@
 British Pound £ £1,234.56

 Japanese Yen ¥ ¥123,456

 Chinese Yuan ¥ ¥1,234.56

-Korean Won

-₩ ₩1,234,560

-Indian Rupee

-₹ ₹ 1,23,456

-Thai Baht

-฿ ฿ 1,234.56

-Russian Ruble

-₽ ₽ 1 234,56

-Turkish Lira

-₺ ₺ 1.234,56

-Bitcoin

-₿ ₿ 0.05

+Korean Won ₩ ₩1,234,560

+Indian Rupee ₹ ₹1,23,456

+Thai Baht ฿ ฿1,234.56

+Russian Ruble ₽ ₽1 234,56

+Turkish Lira ₺ ₺1.234,56

+Bitcoin ₿ ₿0.05

 Swiss Franc CHF CHF 1'234.56
```
</details>

### classic154_math_symbols

- **Text Similarity:** 0.9012
- **Visual Average:** 0.9919
- **Overall Score:** 0.9572
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4620988 bytes, Reference=61270 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic154_math_symbols.pdf
+++ reference/classic154_math_symbols.pdf
@@ -1,19 +1,11 @@
 Category Symbols

-Operators

-± × ÷ ≠ ≤ ≥ ≈ ∝ ∓

-Greek Letters

-α β γ δ ε ζ η θ ι κ λ μ

-Set Theory

-∈ ∉ ⊂ ⊃ ∪ ∩ ∅ ℵ

-Logic

-∀ ∃ ¬ ∧ ∨ ⊕ ⟹ ⟺

-Calculus

-∫ ∬ ∮ ∂ ∇ ∑ ∏ √

-Arrows

-→ ← ↑ ↓ ↔ ⇒ ⇐ ⇔

-Misc

-∞ ℏ ℝ ℤ ℚ ℕ ℂ

-Superscripts

-x² y³ aⁿ e ⁱ

-Subscripts

-H ₂ O CO ₂ x ₙ a ᵢ
+Operators ± × ÷ ≠ ≤ ≥ ≈ ∝∓

+Greek Letters α β γ δ ε ζ η θ ι κ λ μ

+Set Theory ∈∉⊂⊃∪ ∩ ∅ℵ

+Logic ∀∃ ¬ ∧∨⊕ ⟹⟺

+Calculus ∫ ∬∮ ∂ ∇ ∑ ∏ √

+Arrows → ← ↑ ↓ ↔ ⇒⇐⇔

+Misc ∞ ℏℝℤℚℕℂ

+Superscripts x² y³ aⁿ eⁱ

+ₙ

+Subscripts H₂O CO₂ x  aᵢ
```
</details>

### classic155_diacritical_marks

- **Text Similarity:** 1.0
- **Visual Average:** 0.9937
- **Overall Score:** 0.9975
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4130970 bytes, Reference=37150 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic155_diacritical_marks.pdf
+++ reference/classic155_diacritical_marks.pdf
@@ -1,20 +1,11 @@
 Type Examples

-Acute

-á é í ó ú ý ś ź ć ń

-Grave

-à è ì ò ù ỳ

-Circumflex

-â ê î ô û ŵ ŷ ĉ

+Acute á é í ó ú ý ś ź ć ń

+Grave à è ì ò ù ỳ

+Circumflex â ê î ô û ŵ ŷ ĉ

 Umlaut ä ë ï ö ü ÿ

-Tilde

-ã ñ õ ũ ĩ

-Cedilla

-ç ş ţ ḑ ȩ

-Ring

-å ů

-Caron

-č š ž ř ň ě ď ť

-Stroke

-ø đ ħ ł ŧ

-Ligatures

-æ œ ß ĳ
+Tilde ã ñ õ ũ ĩ

+Cedilla ç ş ţ ḑ ȩ

+Ring å ů

+Caron č š ž ř ň ě ď ť

+Stroke ø đ ħ ł ŧ

+Ligatures æ œ ß ĳ
```
</details>

### classic156_rtl_bidi_text

- **Text Similarity:** 0.6038
- **Visual Average:** 0.9965
- **Overall Score:** 0.8401
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3855174 bytes, Reference=30355 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic156_rtl_bidi_text.pdf
+++ reference/classic156_rtl_bidi_text.pdf
@@ -1,9 +1,5 @@
 Script Text

-Arabic

-ﺎﺒﺣﺮﻣ ﻢﻟﺎﻌﻟﺎﺑ

-Hebrew

-םולש םלוע

-Persian

-ﻡﻼﺳ ﺍیﻥﺩ

-Urdu

-ﻮﻟیہ ﺍیﻥﺩ
+Arabicمرحبا بالعالم

+Hebrewשלום עולם

+Persianسالم دنیا

+Urduہیلو دنیا
```
</details>

### classic157_cjk_extended

- **Text Similarity:** 0.7957
- **Visual Average:** 0.9876
- **Overall Score:** 0.9133
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4173419 bytes, Reference=124659 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic157_cjk_extended.pdf
+++ reference/classic157_cjk_extended.pdf
@@ -1,15 +1,15 @@
 Variant Text Notes

+简体中文测试字符串

 Simplified CN Mainland China

-简体中文测试字符串

+繁體中文測試字串

 Traditional CN Taiwan / HK

-繁體中文測試字串

+漢字とひらがなとカタカナ

 Japanese mixed Kanji + Hiragana + Katakana

-漢字とひらがなとカタカナ

-Korean mixed Hangul + Hanja

-한글과 漢字 혼용 텍스트

+漢字

+Korean mixed 한글과 혼용텍스트 Hangul + Hanja

 Rare CJK CJK Ext-B (SMP)

 𠀀𠀁𠀂𠀃𠀄

+ＡＢＣＤ１２３４

 Full-width Full-width alphanumeric

-ＡＢＣＤ１２３４

-Half-width kana Half-width katakana

-ｱｲｳｴｵ ｶｷｸｹｺ
+ｱｲｳｴｵ ｶｷｸｹｺ

+Half-width kana Half-width katakana
```
</details>

### classic158_emoji_skin_tones

- **Text Similarity:** 1.0
- **Visual Average:** 0.9817
- **Overall Score:** 0.9927
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4098674 bytes, Reference=46353 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic158_emoji_skin_tones.pdf
+++ reference/classic158_emoji_skin_tones.pdf
@@ -1,12 +1,6 @@
-Gesture

-Skin Tones (light → dark)

-Thumbs up

-👍🏻👍🏼👍🏽👍🏾👍🏿

-Waving

-👋🏻👋🏼👋🏽👋🏾👋🏿

-Clapping

-👏🏻👏🏼👏🏽👏🏾👏🏿

-Raised fist

-✊🏻✊🏼✊🏽✊🏾✊🏿

-Person

-🧑🏻🧑🏼🧑🏽🧑🏾🧑🏿
+Gesture Skin Tones (light → dark)

+Thumbs up 👍🏻👍🏼👍🏽👍🏾👍🏿

+Waving 👋🏻👋🏼👋🏽👋🏾👋🏿

+Clapping 👏🏻👏🏼👏🏽👏🏾👏🏿

+Raised fist ✊🏻✊🏼✊🏽✊🏾✊🏿

+Person 🧑🏻🧑🏼🧑🏽🧑🏾🧑🏿
```
</details>

### classic159_zwj_emoji

- **Text Similarity:** 0.7769
- **Visual Average:** 0.9893
- **Overall Score:** 0.9065
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=605144 bytes, Reference=59557 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic159_zwj_emoji.pdf
+++ reference/classic159_zwj_emoji.pdf
@@ -1,19 +1,15 @@
 Description Emoji

-Family

-👨 ‍ 👩 ‍ 👧 ‍ 👦

-Couple with heart

-👩 ‍ ❤ ️ ‍ 👨

-Woman technologist

-👩 ‍ 💻

-Man cook

-👨 ‍ 🍳

+

+Family 👨‍👩‍👧‍👦  

+Couple with heart 👩‍❤️‍👨

+Woman technologist 👩‍💻

+Man cook 👨‍🍳

+

 Rainbow flag

-🏳 ️ ‍ 🌈

-Trans flag

-🏳 ️ ‍ ⚧ ️

-Firefighter

-🧑 ‍ 🚒

-Health worker

-🧑 ‍ ⚕ ️

-Service dog

-🐕 ‍ 🦺
+Trans flag 🏳️‍⚧️

+ #

+Firefighter 🧑‍🚒

+⚕

+Health worker 🧑‍ ️

+

+Service dog 🐕‍🦺
```
</details>

### classic15_negative_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9976
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1317 bytes, Reference=28526 bytes

Text content: ✅ Identical

### classic160_punctuation_marks

- **Text Similarity:** 0.8655
- **Visual Average:** 0.995
- **Overall Score:** 0.9442
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4259789 bytes, Reference=75536 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic160_punctuation_marks.pdf
+++ reference/classic160_punctuation_marks.pdf
@@ -1,14 +1,10 @@
 Type Characters

 Latin . , ; : ! ? … — – ' '  « »

+。、；：！？「」『』【】（）

 CJK

-。、；：！？「」『』【】（）

-Arabic

-، ؛٪ ؟ ﷽

+Arabic ،  ؛ ؟٪ ﷽

+।॥꣸꣹꣺

 Devanagari

-। ॥ ꣸ ꣹ ꣺

-Thai

-ฯ ๆ ๏ ๚ ๛

-Misc brackets

-⟨⟩ ⟪⟫ ⌈⌉ ⌊⌋ ‖

-Typographic

-† ‡ § ¶ © ® ™ ℠
+Thai ฯๆ๏๚๛

+Misc brackets ⟨⟩ ⟪⟫ ‖ ⌈⌉⌊⌋

+Typographic † ‡ § ¶ © ® ™ ℠
```
</details>

### classic161_box_drawing

- **Text Similarity:** 0.8822
- **Visual Average:** 0.9929
- **Overall Score:** 0.95
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4288530 bytes, Reference=55369 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic161_box_drawing.pdf
+++ reference/classic161_box_drawing.pdf
@@ -1,13 +1,9 @@
 Type Characters

-Light box

-┌──┬──┐│ │ │├──┼──┤└──┴──┘

-Heavy box

-┏━━┳━━┓┃ ┃ ┃┣━━╋━━┫┗━━┻━━┛

-Double box

-╔══╦══╗║ ║ ║╠══╬══╣╚══╩══╝

-Blocks

-▀ ▁▂▃▄▅▆▇█ ░ ▒ ▓

-Geometric

-■□▪▫▲△▼▽◆◇○●◎

-Braille

-⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊
+Light box ┌──┬──┐│  │  │├──┼──┤└──┴──┘

+Heavy box ┏━━┳━━┓┃  ┃  ┃┣━━╋━━┫┗━━┻━━┛

+Double box ╔══╦══╗║  ║  ║╠══╬══╣╚══╩══╝

+▁▂▃▅▆▇

+Blocks ▀ ▄ █ ░▒▓

+◆◇ ◎

+Geometric ■□▪▫▲ △▽ ▼ ○●

+Braille ⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊
```
</details>

### classic162_cjk_emoji_styled

- **Text Similarity:** 0.6703
- **Visual Average:** 0.9913
- **Overall Score:** 0.8646
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=430619 bytes, Reference=78504 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic162_cjk_emoji_styled.pdf
+++ reference/classic162_cjk_emoji_styled.pdf
@@ -1,9 +1,13 @@
 Icon Dish Price Rating

+寿司

+🍣 ★★★★★

 ¥1,200

-🍣 寿司 ★★★★★

+ラーメン

+🍜 ★★★★

 ¥850

-🍜 ラーメン ★★★★

-¥1,500

-🍱 弁当 ★★★★★

-¥400

-🍙 おにぎり ★★★
+弁当

+★★★★★

+🍱 ¥1,500

+おにぎり

+★★★

+🍙 ¥400
```
</details>

### classic163_cyrillic_alphabets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3953250 bytes, Reference=35137 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic163_cyrillic_alphabets.pdf
+++ reference/classic163_cyrillic_alphabets.pdf
@@ -1,11 +1,6 @@
 Language Sample Text

-Russian

-Съешь ещё этих мягких французских булок, да выпей чаю.

-Ukrainian

-Жебракують філософи при ґанку церкви в Гадячі.

-Serbian

-Ђурађ Бранковић је био владар Србије.

-Bulgarian

-Щъркел яде бялата жаба.

-Mongolian

-Би монгол хэл дээр бичиж байна.
+Russian Съешь ещё этих мягких французских булок, да выпей чаю.

+Ukrainian Жебракують філософи при ґанку церкви в Гадячі.

+Serbian Ђурађ Бранковић је био владар Србије.

+Bulgarian Щъркел яде бялата жаба.

+Mongolian Би монгол хэл дээр бичиж байна.
```
</details>

### classic164_indic_scripts

- **Text Similarity:** 0.6882
- **Visual Average:** 0.9958
- **Overall Score:** 0.8736
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=275521 bytes, Reference=38784 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic164_indic_scripts.pdf
+++ reference/classic164_indic_scripts.pdf
@@ -1,11 +1,11 @@
 Script Sample

+नमस्ते

 Devanagari

-नमस्ते

+வணக்கம்

 Tamil

-வணக்கம்

+নমস্কার

 Bengali

-নমস্কার

+నమస్కారం

 Telugu

-నమస్కారం

-Gujarati

-નમસ્તે
+નમસ્તે

+Gujarati
```
</details>

### classic165_southeast_asian

- **Text Similarity:** 0.9127
- **Visual Average:** 0.987
- **Overall Score:** 0.9599
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3948239 bytes, Reference=68897 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic165_southeast_asian.pdf
+++ reference/classic165_southeast_asian.pdf
@@ -1,11 +1,9 @@
 Script Sample

-Thai

-ภาษาไทยเป็นภาษาที่มีวรรณยุกต์

+Thai ภาษาไทยเป็นภาษาที่มีวรรณยุกต์

+ພາສາລາວເປັນພາສາທີ່ສວຍງາມ

 Lao

-ພາສາລາວເປັນພາສາທີ່ສວຍງາມ

-Myanmar

-မြန်မာဘာသာစကားသည် လှပသည်

+Myanmar မြန်မဘသစကသည်လှပသည်

+ភាសាខ្មែ រជាភាសាចំណាស់

 Khmer

-ភាសាខ្មែរជាភាសាចំណាស់

 Tibetan

 བོད་ཀྱི་སྐད་ཡིག་ནི་གལ་ཆེན་པོ་ཡིན།
```
</details>

### classic166_emoji_progress

- **Text Similarity:** 1.0
- **Visual Average:** 0.9659
- **Overall Score:** 0.9864
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4095991 bytes, Reference=40333 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic166_emoji_progress.pdf
+++ reference/classic166_emoji_progress.pdf
@@ -1,13 +1,7 @@
 Task Status Progress

-Design

-✅ 🟩🟩🟩🟩🟩🟩🟩🟩🟩🟩 100%

-Frontend

-🔄 🟩🟩🟩🟩🟩🟩🟩⬜⬜⬜ 70%

-Backend

-🔄 🟩🟩🟩🟩🟩⬜⬜⬜⬜⬜ 50%

-Testing

-⏳ 🟩🟩⬜⬜⬜⬜⬜⬜⬜⬜ 20%

-Deploy

-❌ ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%

-Docs

-🔄 🟩🟩🟩🟩🟩🟩🟩🟩⬜⬜ 80%
+Design ✅ 🟩🟩🟩🟩🟩🟩🟩🟩🟩🟩 100%

+Frontend 🔄 🟩🟩🟩🟩🟩🟩🟩⬜⬜⬜ 70%

+Backend 🔄 🟩🟩🟩🟩🟩⬜⬜⬜⬜⬜ 50%

+Testing ⏳ 🟩🟩⬜⬜⬜⬜⬜⬜⬜⬜ 20%

+Deploy ❌ ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%

+Docs 🔄 🟩🟩🟩🟩🟩🟩🟩🟩⬜⬜ 80%
```
</details>

### classic167_musical_symbols

- **Text Similarity:** 0.7551
- **Visual Average:** 0.9957
- **Overall Score:** 0.9003
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4163889 bytes, Reference=78122 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic167_musical_symbols.pdf
+++ reference/classic167_musical_symbols.pdf
@@ -1,11 +1,9 @@
 Category Symbols

-Music

-♩ ♪ ♫ ♬ 🎵 🎶

-Chess

-♔♕♖♗♘♙

-Zodiac

-♈♉♊♋♌♍♎♏♐♑♒♓

+♩♪♫♬

+Music 🎵🎶

+Chess ♔♕♖♗♘♙

+Zodiac ♈♉♊♋♌♍♎♏♐♑♒♓

+⚀⚁⚂⚃⚄⚅

 Dice

-⚀ ⚁ ⚂ ⚃ ⚄ ⚅

-Weather

-☀ ☁ ☂ ☃ ❄ ☔
+☀☁☂☃

+Weather ❄ ☔
```
</details>

### classic168_mixed_ltr_rtl_styled

- **Text Similarity:** 0.8718
- **Visual Average:** 0.9908
- **Overall Score:** 0.945
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4131663 bytes, Reference=43114 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic168_mixed_ltr_rtl_styled.pdf
+++ reference/classic168_mixed_ltr_rtl_styled.pdf
@@ -1,7 +1,5 @@
 Code Name Price

 EN-001 Programming Book $29.99

 FR-002 Livre de code €25.00

-AR-003 50 SAR

-ﺏﺎﺘﻛ ﺔﺠﻣﺮﺑ

-HE-004

-רפס דוק ₪120
+AR-003كتاب برمجة50 SAR

+HE-004ספר קוד₪120
```
</details>

### classic169_korean_invoice

- **Text Similarity:** 0.7196
- **Visual Average:** 0.9907
- **Overall Score:** 0.8841
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4171081 bytes, Reference=55693 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic169_korean_invoice.pdf
+++ reference/classic169_korean_invoice.pdf
@@ -1,13 +1,13 @@
-거래명세서 (Transaction Statement)

+거래명세서

+(Transaction Statement)

 번호 상품명 수량 단가 금액

-1 2

-노트북 컴퓨터 ₩1,200,000 ₩2,400,000

-2 5

-무선 마우스 ₩25,000 ₩125,000

-3 2

-모니터 27 인치 ₩350,000 ₩700,000

-4 3

-키보드 ( 기계식 ) ₩89,000 ₩267,000

-5 10

-USB 허브 ₩15,000 ₩150,000

-합계 ₩3,642,000
+1 노트북컴퓨터 2 ₩1,200,000 ₩2,400,000

+2 무선마우스 5 ₩25,000 ₩125,000

+모니터 인치

+3 27 2 ₩350,000 ₩700,000

+키보드 기계식

+4 ( ) 3 ₩89,000 ₩267,000

+허브

+5 USB 10 ₩15,000 ₩150,000

+합계

+₩3,642,000
```
</details>

### classic16_percentage_strings

- **Text Similarity:** 0.9877
- **Visual Average:** 0.9972
- **Overall Score:** 0.994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1221 bytes, Reference=29888 bytes

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

### classic170_emoji_dashboard

- **Text Similarity:** 0.9216
- **Visual Average:** 0.9869
- **Overall Score:** 0.9634
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=150016 bytes, Reference=50748 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic170_emoji_dashboard.pdf
+++ reference/classic170_emoji_dashboard.pdf
@@ -1,13 +1,7 @@
 Metric Value Target

-Revenue $1.2M $1.0M

-🟢

-Pipeline $800K $900K

-🟡

-Churn Rate 5.2% 3.0%

-🔴

-NPS Score 72 65

-🟢

-Response Time 2.1s 1.5s

-🟡

-Uptime 99.95% 99.9%

-🟢
+🟢 Revenue $1.2M $1.0M

+🟡 Pipeline $800K $900K

+🔴 Churn Rate 5.2% 3.0%

+🟢 NPS Score 72 65

+🟡 Response Time 2.1s 1.5s

+🟢 Uptime 99.95% 99.9%
```
</details>

### classic171_ipa_phonetic

- **Text Similarity:** 0.9478
- **Visual Average:** 0.9929
- **Overall Score:** 0.9763
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4140233 bytes, Reference=41195 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic171_ipa_phonetic.pdf
+++ reference/classic171_ipa_phonetic.pdf
@@ -1,15 +1,8 @@
 Category IPA Symbols

-Plosives

-p b t d ʈ ɖ c ɟ k ɡ q ɢ ʔ

-Nasals

-m ɱ n ɳ ɲ ŋ ɴ

-Fricatives

-ɸ β f v θ ð s z ʃ ʒ ʂ ʐ ç ʝ x ɣ

-Vowels

-i y ɨ ʉ ɯ u e ø ɘ ɵ ɤ o ɛ œ ɜ ɞ ʌ ɔ æ a ɶ ɑ ɒ

-Tones

-˥ ˦ ˧ ˨ ˩ ˥˩ ˩˥

-Diacritics

-ʰ ʷ ʲ ˠ ˤ ⁿ ˡ

-Example word

-/ ˌɪ nt əˈ næ ʃə n ə l/ (international)
+Plosives p b t d ʈ ɖ c ɟ k ɡ q ɢ ʔ

+Nasals m ɱ n ɳ ɲ ŋ ɴ

+Fricatives ɸ β f v θ ð s z ʃ ʒ ʂ ʐ ç ʝ x ɣ

+Vowels i y ɨ ʉ ɯ u e ø ɘ ɵ ɤ o ɛ œ ɜ ɞ ʌ ɔ æ a ɶ ɑ ɒ

+Tones ˥ ˦ ˧ ˨ ˩ ˥˩ ˩˥

+Diacritics ʰ ʷ ʲ ˠ ˤ ⁿ ˡ

+Example word /ˌɪntəˈnæʃənəl/ (international)
```
</details>

### classic172_emoji_timeline

- **Text Similarity:** 0.8945
- **Visual Average:** 0.9884
- **Overall Score:** 0.9532
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=152556 bytes, Reference=53459 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic172_emoji_timeline.pdf
+++ reference/classic172_emoji_timeline.pdf
@@ -1,17 +1,9 @@
 Date Icon Milestone Status

-2025-01-15 Idea conceived

-💡 ✅

-2025-02-01 Requirements gathered

-📋 ✅

-2025-03-10 Design completed

-🎨 ✅

-2025-04-20 Development started

-🔨 ✅

-2025-06-15 Testing phase

-🧪 🔄

-2025-07-01 Bug fixing

-🐛 🔄

-2025-08-01 Launch day

-🚀 ⏳

-2025-09-01 Post-launch review

-📊 ⏳
+2025-01-15 💡 Idea conceived ✅

+2025-02-01 📋 Requirements gathered ✅

+2025-03-10 🎨 Design completed ✅

+2025-04-20 🔨 Development started ✅

+2025-06-15 🧪 Testing phase 🔄

+2025-07-01 🐛 Bug fixing 🔄

+2025-08-01 🚀 Launch day ⏳

+2025-09-01 📊 Post-launch review ⏳
```
</details>

### classic173_african_languages

- **Text Similarity:** 0.7804
- **Visual Average:** 0.9902
- **Overall Score:** 0.9082
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4127821 bytes, Reference=41297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic173_african_languages.pdf
+++ reference/classic173_african_languages.pdf
@@ -1,12 +1,10 @@
 Language Greeting Region

 Swahili Habari! Karibu sana. East Africa

-Amharic Ethiopia

-ሰላም! እንኳን ደህና መጣህ.

-Yoruba Nigeria

-Ẹ kú àár ọ̀ ! Ẹ kú al ẹ́ !

+ሰላም እንኳንደህናመጣህ

+Amharic ! . Ethiopia

+Yoruba Ẹ kú àárọ̀! Ẹ kú alẹ́! Nigeria

 Zulu Sawubona! Unjani? South Africa

 Hausa Sannu! Barka da zuwa. West Africa

-Igbo Nigeria

-Nn ọọ ! Ked ụ ?

-Tigrinya Eritrea

-ሰላም! ከመይ ኣለኻ?
+Igbo Nnọọ! Kedụ? Nigeria

+ሰላም ከመይኣለኻ

+Tigrinya ! ? Eritrea
```
</details>

### classic174_technical_symbols

- **Text Similarity:** 0.8705
- **Visual Average:** 0.9889
- **Overall Score:** 0.9438
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4163547 bytes, Reference=45148 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic174_technical_symbols.pdf
+++ reference/classic174_technical_symbols.pdf
@@ -1,14 +1,10 @@
 Category Symbols / Examples

-SI Units kg * m * s * A * K * mol * cd

-Derived

-N · Pa · J · W · V · Ω · Hz

-Prefixes

-μ (micro) · m (milli) · k (kilo) · M (mega) · G (giga)

-Electrical

-Ω kΩ MΩ · μF nF pF · mH μH

-Temp 100 degC = 212 degF = 373.15 K

+SI Units kg · m · s · A · K · mol · cd

+Derived N · Pa · J · W · V · Ω · Hz

+Prefixes μ (micro) · m (milli) · k (kilo) · M (mega) · G (giga)

+Electrical Ω kΩ MΩ · μF nF pF · mH μH

+Temp 100°C = 212°F = 373.15 K

 Copyright © 2025 Company™ — All Rights Reserved®

-Fractions

-½ ⅓ ¼ ⅕ ⅙ ⅛ ⅔ ¾ ⅘

-Roman nums

-Ⅰ Ⅱ Ⅲ Ⅳ Ⅴ Ⅵ Ⅶ Ⅷ Ⅸ Ⅹ Ⅺ Ⅻ
+Fractions ½ ⅓ ¼ ⅕ ⅙ ⅛ ⅔ ¾ ⅘

+ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩⅪⅫ

+Roman nums
```
</details>

### classic175_multiscript_catalog

- **Text Similarity:** 0.8296
- **Visual Average:** 0.9855
- **Overall Score:** 0.926
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=819319 bytes, Reference=98125 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic175_multiscript_catalog.pdf
+++ reference/classic175_multiscript_catalog.pdf
@@ -1,17 +1,11 @@
 # Product (EN) Product (Local) Price Icon

-1 Green Tea ¥500

-緑茶 🍵

-2 Kimchi

-김치 ₩3,000 🥬

-3 Samosa

-समोसा ₹ 50 🥟

-4 Croissant Croissant €2.50

-🥐

-5 Taco Taco $3.99

-🌮

-6 Borscht

-Борщ ₽ 250 🍲

-7 Falafel

-ﻞﻓﻼﻓ ₪15 🧆

-8 Pad Thai

-ผัดไทย ฿ 80 🍜
+緑茶

+1 Green Tea ¥500 🍵

+2 Kimchi 김치 ₩3,000 🥬

+समोसा

+3 Samosa ₹50 🥟

+4 Croissant Croissant €2.50 🥐

+5 Taco Taco $3.99 🌮

+6 Borscht Борщ ₽250 🍲

+7 Falafelفالفل₪15 🧆

+8 Pad Thai ผัดไทย ฿80 🍜
```
</details>

### classic176_combining_characters

- **Text Similarity:** 0.8706
- **Visual Average:** 0.9932
- **Overall Score:** 0.9455
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4339243 bytes, Reference=43113 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic176_combining_characters.pdf
+++ reference/classic176_combining_characters.pdf
@@ -1,13 +1,8 @@
 Type Examples

-Single combining

-é = e + ́ ñ = n +

-Double combining

-ệ = e + ̣ +

-Vietnamese

-ắ ằ ẵ ẳ ặ ố ồ ỗ ổ ộ ứ ừ ữ ử ự

-Zalgo-like

-H e ̵̖̘ ̷̝̣ l l ̶̤ o ̴̥

-Precomposed vs decomposed

-ü (precomposed) vs u ̈ (decomposed)

-Hangul Jamo

-ㅎ ㅏ ㄴ ㄱ ㅡ ㄹ → 한글
+Single combining é = e + ́   ñ = n + ̃

+Double combining ệ = e + ̣ + ̂

+Vietnamese ắ ằ ẵ ẳ ặ ố ồ ỗ ổ ộ ứ ừ ữ ử ự

+Zalgo-like H̵̖̘e̷̝̣l̶̤l̴̥o̸̮

+Precomposed vs decomposed ü (precomposed) vs ü (decomposed)

+ㅎ ㅏ ㄴ ㄱ ㅡ ㄹ →

+Hangul Jamo 한글
```
</details>

### classic177_emoji_calendar

- **Text Similarity:** 0.72
- **Visual Average:** 0.9901
- **Overall Score:** 0.884
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=430189 bytes, Reference=68347 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic177_emoji_calendar.pdf
+++ reference/classic177_emoji_calendar.pdf
@@ -1,25 +1,15 @@
 Month Emoji Event

-January New Year

-🎆

-February Valentine's Day

-💝

-March Spring Equinox

-🌸

-April Easter

-🐣

-May Mother's Day

-👩

+January 🎆 New Year

+February 💝 Valentine's Day

+March 🌸 Spring Equinox

+April 🐣 Easter

+May 👩 Mother's Day

+☀️

 June Summer Solstice

-☀ ️

-July Independence Day

-🎆

-August Vacation Season

-🏖 ️

-September Back to School

-📚

-October Halloween

-🎃

-November Thanksgiving

-🦃

-December Christmas

-🎄
+July 🎆 Independence Day

+%

+August  🏖️ Vacation Season

+September 📚 Back to School

+October 🎃 Halloween

+November 🦃 Thanksgiving

+December 🎄 Christmas
```
</details>

### classic178_caucasus_ethiopic

- **Text Similarity:** 0.9313
- **Visual Average:** 0.9913
- **Overall Score:** 0.969
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4138152 bytes, Reference=41776 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic178_caucasus_ethiopic.pdf
+++ reference/classic178_caucasus_ethiopic.pdf
@@ -1,11 +1,7 @@
 Script Sample Text

-Georgian

-საქართველო არის ძველი ცივილიზაცია .

-Armenian

-Հայաստանը հին քաղաքակրթություն ունի .

+Georgian საქართველო არის ძველი ცივილიზაცია.

+Armenian Հայաստանը հին քաղաքակրթություն ունի.

+ኢትዮጵያየጥንታዊሥልጣኔምድርናት።

 Ethiopic

-ኢትዮጵያ የጥንታዊ ሥልጣኔ ምድር ናት።

-Georgian mkhedru

-ა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

-Armenian alphabe

-Ա Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
+Georgian mkhedrა ბ გ დ ე ვ ზ თ ი კ ლ მ ნ ო პ

+Armenian alphabԱ Բ Գ Դ Ե Զ Է Ը Թ Ժ Ի Լ Խ Ծ Կ
```
</details>

### classic179_emoji_inventory

- **Text Similarity:** 0.7887
- **Visual Average:** 0.9854
- **Overall Score:** 0.9096
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4101485 bytes, Reference=72495 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic179_emoji_inventory.pdf
+++ reference/classic179_emoji_inventory.pdf
@@ -1,21 +1,14 @@
 Icon Item Stock Min Status

-Smartphone 150 50

-📱 🟢 OK

-Laptop 42 30

-💻 🟡 Low

-Printer 8 10

-🖨 ️ 🔴 Reorder

-Headphones 200 40

-🎧 🟢 OK

-Keyboard 75 25

-⌨ ️ 🟢 OK

-Mouse 18 20

-🖱 ️ 🔴 Reorder

-Camera 12 10

-📷 🟡 Low

-Charger 300 100

-🔌 🟢 OK

-USB Drive 5 15

-💾 🔴 Reorder

-Monitor 35 20

-🖥 ️ 🟢 OK
+📱 Smartphone 150 50 🟢 OK

+💻 Laptop 42 30 🟡 Low

+

+ 🖨️ Printer 8 10 🔴 Reorder

+🎧 Headphones 200 40 🟢 OK

+⌨️ Keyboard 75 25 🟢 OK

+

+ 🖱️ Mouse 18 20 🔴 Reorder

+📷 Camera 12 10 🟡 Low

+🔌 Charger 300 100 🟢 OK

+💾 USB Drive 5 15 🔴 Reorder

+

+ 🖥️ Monitor 35 20 🟢 OK
```
</details>

### classic17_currency_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1334 bytes, Reference=29862 bytes

Text content: ✅ Identical

### classic180_polyglot_paragraph

- **Text Similarity:** 0.8468
- **Visual Average:** 0.9921
- **Overall Score:** 0.9356
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4310746 bytes, Reference=79238 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic180_polyglot_paragraph.pdf
+++ reference/classic180_polyglot_paragraph.pdf
@@ -1,16 +1,13 @@
 Language Greeting

 English The quick brown fox.

+速い茶色の狐。

 Japanese

-速い茶色の狐。

-Korean

-빠른 갈색 여우 .

-Russian

-Быстрая бурая лиса.

-Greek

-Η γρήγορη αλεπού.

-Thai

-สุนัขจิ้งจอกสีน้ำตาล

+빠른갈색여우

+Korean .

+Russian Быстрая бурая лиса.

+Greek Η γρήγορη αλεπού.

+Thai สุนัขจิ้งจอกสีน้ำตาล

+तेज़भूरीलोमड़ी

 Hindi

-तेज़ भूरी लोमड़ी

-Emoji

-🦊 ➡ ️ 🐕
+➡️

+Emoji 🐕
```
</details>

### classic18_large_dataset

- **Text Similarity:** 1.0
- **Visual Average:** 0.8931
- **Overall Score:** 0.9572
- **Pages:** MiniPdf=24, Reference=24
- **File Size:** MiniPdf=562822 bytes, Reference=2487195 bytes

Text content: ✅ Identical

### classic19_single_column_list

- **Text Similarity:** 1.0
- **Visual Average:** 0.9967
- **Overall Score:** 0.9987
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
- **Visual Average:** 0.9986
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=843 bytes, Reference=22034 bytes

Text content: ✅ Identical

### classic22_long_sheet_name

- **Text Similarity:** 1.0
- **Visual Average:** 0.9985
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=885 bytes, Reference=23683 bytes

Text content: ✅ Identical

### classic23_unicode_text

- **Text Similarity:** 0.7884
- **Visual Average:** 0.9951
- **Overall Score:** 0.9134
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=816192 bytes, Reference=67722 bytes

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

-ﺎﺒﺣﺮﻣ ﻢﻟﺎﻌﻟﺍ

-Emoji

-😀🎉 ✅❌
+Korean 안녕하세요세계

+Arabicمرحبا العالم

+Emoji 😀🎉 ✅❌
```
</details>

### classic24_red_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1088 bytes, Reference=39031 bytes

Text content: ✅ Identical

### classic25_multiple_colors

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9962
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1832 bytes, Reference=43116 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic25_multiple_colors.pdf
+++ reference/classic25_multiple_colors.pdf
@@ -1,4 +1,4 @@
-Color Nam Sample Text

+Color NamSample Text

 Red This is red text

 Green This is green text

 Blue This is blue text

```
</details>

### classic26_inline_strings

- **Text Similarity:** 1.0
- **Visual Average:** 0.9975
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1064 bytes, Reference=25018 bytes

Text content: ✅ Identical

### classic27_single_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9984
- **Overall Score:** 0.9994
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=949 bytes, Reference=23681 bytes

Text content: ✅ Identical

### classic28_duplicate_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9965
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1600 bytes, Reference=24729 bytes

Text content: ✅ Identical

### classic29_formula_results

- **Text Similarity:** 1.0
- **Visual Average:** 0.9971
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1529 bytes, Reference=27548 bytes

Text content: ✅ Identical

### classic30_mixed_empty_and_filled_sheets

- **Text Similarity:** 1.0
- **Visual Average:** 0.9987
- **Overall Score:** 0.9995
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=1426 bytes, Reference=27418 bytes

Text content: ✅ Identical

### classic31_bold_header_row

- **Text Similarity:** 1.0
- **Visual Average:** 0.9965
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1677 bytes, Reference=40714 bytes

Text content: ✅ Identical

### classic32_right_aligned_numbers

- **Text Similarity:** 1.0
- **Visual Average:** 0.9974
- **Overall Score:** 0.999
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1012 bytes, Reference=27582 bytes

Text content: ✅ Identical

### classic33_centered_text

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1357 bytes, Reference=26648 bytes

Text content: ✅ Identical

### classic34_explicit_column_widths

- **Text Similarity:** 1.0
- **Visual Average:** 0.9963
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1250 bytes, Reference=28834 bytes

Text content: ✅ Identical

### classic35_explicit_row_heights

- **Text Similarity:** 0.9882
- **Visual Average:** 0.999
- **Overall Score:** 0.9949
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=925 bytes, Reference=25108 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic35_explicit_row_heights.pdf
+++ reference/classic35_explicit_row_heights.pdf
@@ -1,3 +1,3 @@
-Tall Heade Value

+Tall HeadeValue

 Extra Tall 42

 Normal Ro 10
```
</details>

### classic36_merged_cells

- **Text Similarity:** 1.0
- **Visual Average:** 0.997
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1133 bytes, Reference=27256 bytes

Text content: ✅ Identical

### classic37_freeze_panes

- **Text Similarity:** 1.0
- **Visual Average:** 0.9899
- **Overall Score:** 0.996
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5238 bytes, Reference=46420 bytes

Text content: ✅ Identical

### classic38_hyperlink_cell

- **Text Similarity:** 1.0
- **Visual Average:** 0.9969
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=925 bytes, Reference=26279 bytes

Text content: ✅ Identical

### classic39_financial_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2133 bytes, Reference=43383 bytes

Text content: ✅ Identical

### classic40_scientific_notation

- **Text Similarity:** 1.0
- **Visual Average:** 0.9964
- **Overall Score:** 0.9986
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1254 bytes, Reference=30852 bytes

Text content: ✅ Identical

### classic41_integer_vs_float

- **Text Similarity:** 1.0
- **Visual Average:** 0.997
- **Overall Score:** 0.9988
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1514 bytes, Reference=29637 bytes

Text content: ✅ Identical

### classic42_boolean_values

- **Text Similarity:** 0.9948
- **Visual Average:** 0.9963
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1260 bytes, Reference=28631 bytes

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

- **Text Similarity:** 0.9984
- **Visual Average:** 0.9887
- **Overall Score:** 0.9948
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3200 bytes, Reference=49849 bytes

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

- **Text Similarity:** 0.9652
- **Visual Average:** 0.9855
- **Overall Score:** 0.9803
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3776 bytes, Reference=43656 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic44_employee_roster.pdf
+++ reference/classic44_employee_roster.pdf
@@ -1,9 +1,9 @@
 EmpID First Last Dept Title Email

-1001 Alice Smith Engineerin Senior Eng alice@example.com

-1002 Bob Jones Marketing Marketing bob@example.com

-1003 Carol Williams HR HR Special carol@example.com

-1004 David Brown Engineerin Junior Engi david@example.com

-1005 Eve Davis Finance Financial A eve@example.com

-1006 Frank Miller Sales Sales Repr frank@example.com

-1007 Grace Wilson Engineerin Tech Lead grace@example.com

-1008 Henry Moore Support Support Sp henry@example.com
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
- **Visual Average:** 0.9976
- **Overall Score:** 0.999
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=3294 bytes, Reference=37087 bytes

Text content: ✅ Identical

### classic46_grade_book

- **Text Similarity:** 1.0
- **Visual Average:** 0.9908
- **Overall Score:** 0.9963
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3519 bytes, Reference=40993 bytes

Text content: ✅ Identical

### classic47_time_series

- **Text Similarity:** 1.0
- **Visual Average:** 0.9821
- **Overall Score:** 0.9928
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7348 bytes, Reference=55976 bytes

Text content: ✅ Identical

### classic48_survey_results

- **Text Similarity:** 0.9971
- **Visual Average:** 0.9935
- **Overall Score:** 0.9962
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2524 bytes, Reference=36069 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic48_survey_results.pdf
+++ reference/classic48_survey_results.pdf
@@ -1,6 +1,6 @@
 Question StrongAgr Agree Neutral Disagree StrongDisagree

 Easy to us 30 45 15 7 3

-Recomme 25 40 20 10 5

+Recommen 25 40 20 10 5

 Fair price 20 35 25 15 5

 Good supp 35 40 15 7 3

 Satisfied 28 42 18 8 4
```
</details>

### classic49_contact_list

- **Text Similarity:** 0.9737
- **Visual Average:** 0.9896
- **Overall Score:** 0.9853
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2988 bytes, Reference=41523 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic49_contact_list.pdf
+++ reference/classic49_contact_list.pdf
@@ -1,8 +1,8 @@
 Name Phone Email City Country

-Alice Smith +1-555-01 alice@exa New York USA

-Bob Jones +44-20-79 bob@exa London UK

-Carol Wan +86-10-12 carol@exa Beijing China

-David Mull +49-30-12 david@ex Berlin Germany

-Eve Martin +33-1-23-4 eve@exa Paris France

-Frank Tan +81-3-123 frank@exa Tokyo Japan

-Grace Kim +82-2-123 grace@ex Seoul Korea
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
- **Visual Average:** 0.9909
- **Overall Score:** 0.9955
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=6951 bytes, Reference=54986 bytes

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

- **Text Similarity:** 0.9747
- **Visual Average:** 0.9876
- **Overall Score:** 0.9849
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3791 bytes, Reference=44297 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic51_product_catalog.pdf
+++ reference/classic51_product_catalog.pdf
@@ -1,11 +1,11 @@
-Part# Name Description Weight(g) Price

-P-001 Basic Widg Standard 150 4.99

-P-002 Pro Widget Enhanced 180 12.99

-P-003 Mini Gadg Compact g 90 19.99

-P-004 Max Gadg Full-size g 450 89.99

-P-005 Connector Type-A co 80 7.49

-P-006 Connector Type-B co 110 9.99

-P-007 Adapter X Universal p 200 15.99

+Part# Name Descriptio Weight(g) Price

+P-001 Basic WidgStandard w 150 4.99

+P-002 Pro WidgeEnhanced 180 12.99

+P-003 Mini GadgeCompact g 90 19.99

+P-004 Max GadgeFull-size g 450 89.99

+P-005 ConnectorType-A con 80 7.49

+P-006 ConnectorType-B con 110 9.99

+P-007 Adapter X Universal 200 15.99

 P-008 Adapter Y Travel pow 120 11.99

-P-009 Mount Bra Wall mount 600 24.99

-P-010 Carry Cas Padded ca 350 34.99
+P-009 Mount BraWall moun 600 24.99

+P-010 Carry CasePadded ca 350 34.99
```
</details>

### classic52_pivot_summary

- **Text Similarity:** 0.9978
- **Visual Average:** 0.9916
- **Overall Score:** 0.9958
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2540 bytes, Reference=44493 bytes

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
- **Visual Average:** 0.9912
- **Overall Score:** 0.9952
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2745 bytes, Reference=53425 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic53_invoice.pdf
+++ reference/classic53_invoice.pdf
@@ -6,9 +6,9 @@
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
- **Visual Average:** 0.994
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2285 bytes, Reference=38782 bytes

Text content: ✅ Identical

### classic55_error_values

- **Text Similarity:** 1.0
- **Visual Average:** 0.9945
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1713 bytes, Reference=34677 bytes

Text content: ✅ Identical

### classic56_alternating_row_colors

- **Text Similarity:** 1.0
- **Visual Average:** 0.9903
- **Overall Score:** 0.9961
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3220 bytes, Reference=32363 bytes

Text content: ✅ Identical

### classic57_cjk_only

- **Text Similarity:** 0.7826
- **Visual Average:** 0.9953
- **Overall Score:** 0.9112
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=291022 bytes, Reference=88207 bytes

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

- **Text Similarity:** 0.9905
- **Visual Average:** 0.9958
- **Overall Score:** 0.9945
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1707 bytes, Reference=32815 bytes

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

-Scientific a 1.23E+10
+Scientific 1.23E+10
```
</details>

### classic59_multi_sheet_summary

- **Text Similarity:** 1.0
- **Visual Average:** 0.9962
- **Overall Score:** 0.9985
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=4549 bytes, Reference=44781 bytes

Text content: ✅ Identical

### classic60_large_wide_table

- **Text Similarity:** 1.0
- **Visual Average:** 0.9346
- **Overall Score:** 0.9738
- **Pages:** MiniPdf=4, Reference=4
- **File Size:** MiniPdf=58117 bytes, Reference=263350 bytes

Text content: ✅ Identical

### classic61_product_card_with_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9982
- **Overall Score:** 0.9993
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2075 bytes, Reference=36974 bytes

Text content: ✅ Identical

### classic62_company_logo_header

- **Text Similarity:** 0.996
- **Visual Average:** 0.9949
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2841 bytes, Reference=42880 bytes

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
- **Visual Average:** 0.9939
- **Overall Score:** 0.9976
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3085 bytes, Reference=28933 bytes

Text content: ✅ Identical

### classic64_employee_directory_with_photo

- **Text Similarity:** 0.9835
- **Visual Average:** 0.9938
- **Overall Score:** 0.9909
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4763 bytes, Reference=43408 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic64_employee_directory_with_photo.pdf
+++ reference/classic64_employee_directory_with_photo.pdf
@@ -1,4 +1,4 @@
-Photo Name Title Departmen Email

-Alice Chen Engineer R&D alice@example.com

-Bob Smith Manager Sales bob@example.com

-Carol Wan Designer UX carol@example.com
+Photo Name Title DepartmeEmail

+Alice ChenEngineer R&D alice@example.com

+Bob SmithManager Sales bob@example.com

+Carol WanDesigner UX carol@example.com
```
</details>

### classic65_inventory_with_product_photos

- **Text Similarity:** 0.9906
- **Visual Average:** 0.9949
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6705 bytes, Reference=48227 bytes

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

- **Text Similarity:** 0.9836
- **Visual Average:** 0.9956
- **Overall Score:** 0.9917
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2819 bytes, Reference=45034 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic66_invoice_with_logo.pdf
+++ reference/classic66_invoice_with_logo.pdf
@@ -1,8 +1,8 @@
 INVOICE

 Invoice #: INV-20250301

 Date: 2025-03-01

-Description Qty Unit Price Total

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
- **Visual Average:** 0.9944
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2854 bytes, Reference=44030 bytes

Text content: ✅ Identical

### classic68_restaurant_menu

- **Text Similarity:** 0.9881
- **Visual Average:** 0.9776
- **Overall Score:** 0.9863
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5341 bytes, Reference=47320 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic68_restaurant_menu.pdf
+++ reference/classic68_restaurant_menu.pdf
@@ -1,9 +1,9 @@
 Today's Menu

-Grilled Sal $18.99

+Grilled S $18.99

 Fresh Atlantic salmon with herbs

-Caesar Sal $12.99

+Caesar Sa $12.99

 Romaine lettuce, croutons, parmesan

-Beef Burge $14.99

+Beef Burg $14.99

 8oz Angus beef, brioche bun

-Pasta Prim $13.99

+Pasta Pri $13.99

 Seasonal vegetables, olive oil
```
</details>

### classic69_image_only_sheet

- **Text Similarity:** 1.0
- **Visual Average:** 1.0
- **Overall Score:** 1.0
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2459 bytes, Reference=8905 bytes

Text content: ✅ Identical

### classic70_product_catalog_with_images

- **Text Similarity:** 0.9862
- **Visual Average:** 0.9934
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4463 bytes, Reference=44156 bytes

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

 Desk Orga $24.99

 Bamboo desk tidy set
```
</details>

### classic71_multi_sheet_with_images

- **Text Similarity:** 0.9966
- **Visual Average:** 0.999
- **Overall Score:** 0.9982
- **Pages:** MiniPdf=3, Reference=3
- **File Size:** MiniPdf=5095 bytes, Reference=37419 bytes

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
- **Visual Average:** 0.9856
- **Overall Score:** 0.9942
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3208 bytes, Reference=41342 bytes

Text content: ✅ Identical

### classic73_event_flyer_with_banner

- **Text Similarity:** 0.9939
- **Visual Average:** 0.9936
- **Overall Score:** 0.995
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3212 bytes, Reference=44512 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic73_event_flyer_with_banner.pdf
+++ reference/classic73_event_flyer_with_banner.pdf
@@ -3,7 +3,7 @@
 Venue: Convention Center Hall A

 Speakers: 20+ Industry Leaders

 Time Session Speaker

-09:00 Opening K Dr. Jane Kim

-10:30 AI in Practi Prof. Mark Liu

-13:00 Cloud Arch Eng. Sara Patel

+09:00 Opening KDr. Jane Kim

+10:30 AI in Pract Prof. Mark Liu

+13:00 Cloud ArchEng. Sara Patel

 15:00 Panel Disc All Speakers
```
</details>

### classic74_dashboard_with_kpi_image

- **Text Similarity:** 0.9595
- **Visual Average:** 0.9874
- **Overall Score:** 0.9788
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4096595 bytes, Reference=48755 bytes

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

-Churn Rat < 3% 2.8%

-✓ Above
+Revenue 500000 523000 ✓ Above

+New Custo 200 187  Below ✗

+NPS Score 70 74 ✓ Above

+Churn Rate< 3% 2.8% ✓ Above
```
</details>

### classic75_certificate_with_seal

- **Text Similarity:** 1.0
- **Visual Average:** 0.9867
- **Overall Score:** 0.9947
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=1996 bytes, Reference=39135 bytes

Text content: ✅ Identical

### classic76_product_image_grid

- **Text Similarity:** 1.0
- **Visual Average:** 0.9897
- **Overall Score:** 0.9959
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5056 bytes, Reference=39017 bytes

Text content: ✅ Identical

### classic77_news_article_with_hero_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9911
- **Overall Score:** 0.9964
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2737 bytes, Reference=52664 bytes

Text content: ✅ Identical

### classic78_small_icon_per_row

- **Text Similarity:** 0.9799
- **Visual Average:** 0.9953
- **Overall Score:** 0.9901
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=6164 bytes, Reference=41646 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic78_small_icon_per_row.pdf
+++ reference/classic78_small_icon_per_row.pdf
@@ -1,6 +1,6 @@
 Icon Task Assignee Status

 Fix login b Alice Done

-Write unit t Bob In Progress

-Deploy to s Carol Pending

-Code revie Alice Done

-Update do Dave In Progress
+Write unit Bob In Progress

+Deploy to Carol Pending

+Code revieAlice Done

+Update doDave In Progress
```
</details>

### classic79_wide_panoramic_banner

- **Text Similarity:** 1.0
- **Visual Average:** 0.9946
- **Overall Score:** 0.9978
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2979 bytes, Reference=43015 bytes

Text content: ✅ Identical

### classic80_portrait_tall_image

- **Text Similarity:** 1.0
- **Visual Average:** 0.9948
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2367 bytes, Reference=39079 bytes

Text content: ✅ Identical

### classic81_step_by_step_with_images

- **Text Similarity:** 1.0
- **Visual Average:** 0.9921
- **Overall Score:** 0.9968
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5152 bytes, Reference=47175 bytes

Text content: ✅ Identical

### classic82_before_after_images

- **Text Similarity:** 0.9926
- **Visual Average:** 0.9906
- **Overall Score:** 0.9933
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3899 bytes, Reference=42486 bytes

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

- **Text Similarity:** 0.9863
- **Visual Average:** 0.9931
- **Overall Score:** 0.9918
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=7040 bytes, Reference=45933 bytes

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
- **Visual Average:** 0.9897
- **Overall Score:** 0.9959
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=4407 bytes, Reference=42524 bytes

Text content: ✅ Identical

### classic85_lab_results_with_image

- **Text Similarity:** 0.9911
- **Visual Average:** 0.9877
- **Overall Score:** 0.9915
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3497 bytes, Reference=47866 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic85_lab_results_with_image.pdf
+++ reference/classic85_lab_results_with_image.pdf
@@ -1,6 +1,6 @@
 Sample Analysis Report

-Parameter Value Unit Reference Flag

-pH 7.35 7.35 – 7.45 Normal

+ParameteValue Unit ReferenceFlag

+pH 7.35 7.35 – 7.45Normal

 Glucose 5.2 mmol/L 3.9 – 5.5 Normal

 Sodium 142 mEq/L 136 – 145 Normal

 Potassium 5 mEq/L 3.5 – 5.0 Normal

```
</details>

### classic86_software_screenshot_features

- **Text Similarity:** 0.973
- **Visual Average:** 0.9948
- **Overall Score:** 0.9871
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2932 bytes, Reference=41961 bytes

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

-Offline Mo Yes

-API Acces Pro only

-Export to P Yes
+Cloud SyncYes

+Offline MoYes

+API AccessPro only

+Export to Yes
```
</details>

### classic87_sports_results_with_logos

- **Text Similarity:** 1.0
- **Visual Average:** 0.9948
- **Overall Score:** 0.9979
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=5750 bytes, Reference=47076 bytes

Text content: ✅ Identical

### classic88_image_after_data

- **Text Similarity:** 0.997
- **Visual Average:** 0.9948
- **Overall Score:** 0.9967
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=2962 bytes, Reference=43273 bytes

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

- **Text Similarity:** 0.9903
- **Visual Average:** 0.995
- **Overall Score:** 0.9941
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3264 bytes, Reference=47194 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic89_nutrition_label_with_image.pdf
+++ reference/classic89_nutrition_label_with_image.pdf
@@ -1,11 +1,11 @@
 Nutrition Facts

 Serving Size: 30g (approx. 1 cup)

-Nutrient Amount pe % Daily Value

+Nutrient Amount p% Daily Value

 Calories 120 kcal

 Total Fat 3g 4%

 Saturated 0.5g 3%

 Sodium 160mg 7%

-Total Carb 22g 8%

-Dietary Fib 3g 11%

+Total Carb22g 8%

+Dietary Fib3g 11%

 Sugars 4g

 Protein 3g
```
</details>

### classic90_project_status_with_milestones

- **Text Similarity:** 0.9572
- **Visual Average:** 0.9852
- **Overall Score:** 0.977
- **Pages:** MiniPdf=1, Reference=1
- **File Size:** MiniPdf=3392 bytes, Reference=47112 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic90_project_status_with_milestones.pdf
+++ reference/classic90_project_status_with_milestones.pdf
@@ -1,8 +1,8 @@
 Project Orion – Status Report

 Reporting Period: Q1 2025

-Milestone Due Date Owner Status

-Requireme Jan 15 PM Team Complete

-Architectur Feb 1 Tech Lead Complete

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

### classic91_simple_bar_chart

- **Text Similarity:** 0.9493
- **Visual Average:** 0.9607
- **Overall Score:** 0.964
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3084 bytes, Reference=46981 bytes

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

- **Text Similarity:** 0.9563
- **Visual Average:** 0.9665
- **Overall Score:** 0.9691
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=3800 bytes, Reference=49903 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic92_horizontal_bar_chart.pdf
+++ reference/classic92_horizontal_bar_chart.pdf
@@ -1,5 +1,6 @@
-Departmen Headcount

-Engineerin 45 Headcount by Department

+DepartmenHeadcount

+Engineerin 45

+Headcount by Department

 Sales 30

 Marketing 18

 HR 12

@@ -11,5 +12,7 @@
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

- **Text Similarity:** 0.8257
- **Visual Average:** 0.9861
- **Overall Score:** 0.9247
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=5274 bytes, Reference=58815 bytes

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
- **Visual Average:** 0.9249
- **Overall Score:** 0.9212
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=403625 bytes, Reference=47211 bytes

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
- **Visual Average:** 0.7651
- **Overall Score:** 0.7637
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=12752 bytes, Reference=60817 bytes

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

-00:001:002:003:004:005:006:007:008:009:0010:0011:0012:0013:0014:0015:0016:0017:0018:0019:0020:0021:0

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

- **Text Similarity:** 0.8714
- **Visual Average:** 0.9855
- **Overall Score:** 0.9428
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=6339 bytes, Reference=62711 bytes

<details><summary>Text Diff</summary>

```diff
--- minipdf/classic96_scatter_chart.pdf
+++ reference/classic96_scatter_chart.pdf
@@ -1,27 +1,34 @@
-Ad Spend ( Sales ($K)

-45 96 Ad Spend vs Sales

+Ad Spend (Sales ($K)

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
- **Visual Average:** 0.9317
- **Overall Score:** 0.9155
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=302234 bytes, Reference=47227 bytes

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
- **Visual Average:** 0.9892
- **Overall Score:** 0.9507
- **Pages:** MiniPdf=2, Reference=2
- **File Size:** MiniPdf=4070 bytes, Reference=47620 bytes

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
- **Visual Average:** 0.966
- **Overall Score:** 0.9243
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
1. **classic120_chart_with_date_axis** (score: 0.7611)
1. **classic95_area_chart** (score: 0.7637)

Review the text diffs and visual comparisons above to identify specific rendering issues.
