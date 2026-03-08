import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
names = ['classic04_single_cell','classic09_long_text','classic10_special_xml_characters',
         'classic11_sparse_rows','classic19_single_column_list','classic77_news_article_with_hero_image']
for d in data:
    if d['name'] in names:
        print(f"{d['name']}: overall={d['overall_score']:.4f} text={d['text_similarity']:.4f} vis={d['visual_avg']:.4f} pages={d['minipdf_pages']}/{d['reference_pages']}")
