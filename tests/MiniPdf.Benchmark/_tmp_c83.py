import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

for x in data:
    if x['name'] == 'classic83_color_swatch_palette':
        print(f"=== {x['name']} overall={x['overall_score']:.4f} txt={x['text_similarity']:.4f} vis={x['visual_avg']:.4f} ===")
        print(f"  flat_txt={x.get('flat_text_similarity',0):.4f} word_txt={x.get('word_text_similarity',0):.4f}")
        print(f"  visual_scores={x.get('visual_scores', [])}")
        if x.get('text_diff'):
            diff = x['text_diff']
            for line in diff.split('\n')[:40]:
                print(line[:200])
