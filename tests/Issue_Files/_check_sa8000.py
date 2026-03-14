import json
with open('tests/Issue_Files/reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
for item in data:
    name = item['name']
    if name != 'SA8000 ch sample':
        continue
    print(f"Name: {name}")
    print(f"Text sim: {item['text_similarity']}")
    print(f"Visual avg: {item['visual_avg']}")
    print(f"Overall: {item['overall_score']}")
    print(f"Pages: M={item['minipdf_pages']}, R={item['reference_pages']}")
    print(f"Size: M={item['minipdf_size']}, R={item['reference_size']}")
    for i, vs in enumerate(item['visual_scores']):
        print(f"  Page {i+1}: visual={vs}")
