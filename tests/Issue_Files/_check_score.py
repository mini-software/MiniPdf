import json
with open('reports_docx/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)
for item in data:
    if 'SA8000' in item['name']:
        print('Score: %.4f' % item['overall_score'])
        print('text_sim: %.4f' % item['text_similarity'])
        print('visual_avg: %.4f' % item['visual_avg'])
        for i, v in enumerate(item['visual_scores']):
            print('  Page %d: visual=%.4f' % (i+1, v))
