import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

for x in data:
    if x['name'] in ['classic23_unicode_text', 'classic90_project_status_with_milestones', 
                      'classic68_restaurant_menu']:
        print(f"\n=== {x['name']} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f} pages: mini={x['minipdf_pages']} ref={x['reference_pages']} ===")
        if x.get('text_diff'):
            diff = x['text_diff']
            lines = diff.split('\n')
            shown = 0
            for line in lines:
                if line.startswith('+') or line.startswith('-'):
                    if not line.startswith('+++') and not line.startswith('---'):
                        print(line[:200])
                        shown += 1
                        if shown >= 25:
                            break
