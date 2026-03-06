import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

targets = [
    'classic83_color_swatch_palette',
    'classic70_product_catalog_with_images',
    'classic64_employee_directory_with_photo',
    'classic78_small_icon_per_row',
    'classic86_software_screenshot_features',
    'classic49_contact_list',
    'classic51_product_catalog',
    'classic68_restaurant_menu',
]

for c in data:
    if c['name'] in targets:
        print('=== %s ===' % c['name'])
        print('overall=%.4f txt=%.4f vis=%.4f mp=%d ref=%d' % (
            c['overall_score'], c['text_similarity'], c['visual_avg'],
            c['minipdf_pages'], c['reference_pages']))
        d = c.get('text_diff', '')
        lines = d.split('\n')
        # Show only diff lines (starting with + or -)
        diffs = [l for l in lines if l.startswith('+') or l.startswith('-')]
        for l in diffs[:40]:
            print(l)
        print()
