import openpyxl
import sys

xlsx_dir = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output'
names = ['classic06_tall_table', 'classic24_red_text', 'classic44_employee_roster', 'classic57_cjk_only', 'classic23_unicode_text', 'classic36_merged_cells', 'classic38_hyperlink_cell', 'classic58_mixed_numeric_formats']

for fname in names:
    path = f'{xlsx_dir}\\{fname}.xlsx'
    try:
        wb = openpyxl.load_workbook(path)
        ws = wb.active
        print(f'{fname}:')
        for col_letter, cd in ws.column_dimensions.items():
            if cd.width:
                print(f'  col {col_letter}: width={cd.width}, customWidth={cd.customWidth}')
        sf = ws.sheet_format.defaultColWidth
        print(f'  sheet defaultColWidth={sf}')
        print()
    except Exception as e:
        print(f'{fname}: ERROR {e}')
